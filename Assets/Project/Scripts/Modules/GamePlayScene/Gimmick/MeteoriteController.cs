using System.Collections;
using System.Linq;
using Treevel.Common.Entities;
using Treevel.Common.Entities.GameDatas;
using Treevel.Common.Managers;
using Treevel.Common.Utils;
using Treevel.Modules.GamePlayScene.Bottle;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Treevel.Modules.GamePlayScene.Gimmick
{
    public class MeteoriteController : GimmickControllerBase
    {
        /// <summary>
        /// 目標座標
        /// </summary>
        private Vector2 _targetPos;

        /// <summary>
        /// 目標座標からどれだけ離れた位置に出現させるか
        /// </summary>
        private Vector2 _startPosMargin;

        /// <summary>
        /// 攻撃するために移動する距離
        /// </summary>
        private float _attackMoveDistance;

        /// <summary>
        /// 隕石部分
        /// </summary>
        [SerializeField] private GameObject _meteorite;

        /// <summary>
        /// 隕石のSpriteRenderer
        /// </summary>
        private SpriteRenderer _meteoriteRenderer;

        /// <summary>
        /// 隕石のAnimator
        /// </summary>
        private Animator _meteoriteAnimator;

        /// <summary>
        /// 隕石の穴部分のSpriteRenderer
        /// </summary>
        [SerializeField] private SpriteRenderer _meteoriteHoleRenderer;

        /// <summary>
        /// 隕石の影
        /// </summary>
        private GameObject _shadow;

        private const string _ANIMATOR_PARAM_TRIGGER_WARNING = "Warning";
        private const string _ATTACK_ANIMATION_CLIP_NAME = "Meteorite@attack";
        private static readonly int _ATTACK_STATE_NAME_HASH = Animator.StringToHash("Meteorite@attack");
        private static readonly int _REMAINING_STATE_NAME_HASH = Animator.StringToHash("Meteorite@remaining");

        private void Awake()
        {
            base.Awake();
            _startPosMargin = new Vector2(2.5f * GameWindowController.Instance.GetTileWidth(), 1.5f * GameWindowController.Instance.GetTileWidth());

            _meteoriteRenderer = _meteorite.GetComponent<SpriteRenderer>();
            _meteoriteAnimator = GetComponent<Animator>();

            this.OnTriggerEnter2DAsObservable()
                .Select(other => other.GetComponent<BottleControllerBase>())
                .Where(bottle => bottle != null)
                .Where(bottle => bottle.IsAttackable)
                .Where(bottle => !bottle.IsInvincible)
                .Subscribe(bottle => HandleCollision(bottle.gameObject))
                .AddTo(this);

            GamePlayDirector.Instance.GameEnd.Subscribe(_ => {
                SoundManager.Instance.StopSE(ESEKey.Gimmick_Meteorite_Drop);
                SoundManager.Instance.StopSE(ESEKey.Gimmick_Meteorite_Collide);

            }).AddTo(this);
            GamePlayDirector.Instance.GameSucceeded.Subscribe(_ => {
                if (_shadow != null) Destroy(_shadow);
                Destroy(gameObject);
            }).AddTo(this);
            GamePlayDirector.Instance.GameFailed.Subscribe(_ => {
                if (_shadow != null) _shadow.GetComponent<Animator>().speed = 0;
                _meteoriteAnimator.speed = 0f;
            }).AddTo(this);
        }

        public override void Initialize(GimmickData gimmickData)
        {
            base.Initialize(gimmickData);
            // 描画順序の設定
            _meteoriteRenderer.sortingOrder += 2;
            _meteoriteHoleRenderer.sortingOrder += 1;

            switch (gimmickData.type) {
                case EGimmickType.Meteorite:
                    if (gimmickData.isRandom) {
                        var row = GimmickLibrary.SamplingArrayIndex(gimmickData.randomRow.ToArray());
                        var column = GimmickLibrary.SamplingArrayIndex(gimmickData.randomColumn.ToArray());
                        _targetPos = BoardManager.Instance.GetTilePos(column, row);
                    } else {
                        _targetPos =
                            BoardManager.Instance.GetTilePos((int)gimmickData.targetColumn,
                                                             (int)gimmickData.targetRow);
                    }

                    break;
                case EGimmickType.AimingMeteorite:
                    if (gimmickData.isRandom) {
                        // 乱数インデックスを重みに基づいて取得
                        var randomIndex =
                            GimmickLibrary.SamplingArrayIndex(gimmickData.randomAttackableBottles.ToArray());
                        // 乱数インデックスをボトルIDに変換
                        var targetId = CalcBottleIdByRandomArrayIndex(randomIndex);
                        _targetPos = BoardManager.Instance.GetBottlePosById(targetId);
                    } else {
                        _targetPos = BoardManager.Instance.GetBottlePosById(gimmickData.targetBottle);
                    }

                    break;
                default:
                    throw new System.NotImplementedException("不正なギミックタイプです");
            }

            var position = _targetPos + _startPosMargin;
            // 画面外に発生しないように位置の調整
            var positionX = Mathf.Min(position.x, (GameWindowController.Instance.GetGameCoreSpaceWidth() - _meteoriteRenderer.bounds.size.x) / 2f);
            var positionY = Mathf.Min(position.y, (GameWindowController.Instance.GetGameCoreSpaceHeight() - _meteoriteRenderer.bounds.size.y) / 2f);
            position = new Vector2(positionX, positionY);
            _attackMoveDistance = (position - _targetPos).magnitude;
            transform.position = position;
        }

        public override IEnumerator Trigger()
        {
            var audioIndex = SoundManager.Instance.PlaySE(ESEKey.Gimmick_Meteorite_Drop);
            // 影の発生
            AsyncOperationHandle<GameObject> shadowOp;
            yield return shadowOp = AddressableAssetManager.Instantiate(Constants.Address.METEORITE_SHADOW_PREFAB);
            _shadow = shadowOp.Result;
            _shadow.transform.position = _targetPos;

            // 動き出しのアニメーション
            _meteoriteAnimator.SetTrigger(_ANIMATOR_PARAM_TRIGGER_WARNING);
            _shadow.GetComponent<Animator>().SetTrigger(_ANIMATOR_PARAM_TRIGGER_WARNING);
            // Attackまで待つ
            yield return new WaitUntil(() => _meteoriteAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash == _ATTACK_STATE_NAME_HASH);

            // 落下アニメーション
            yield return MoveToTargetPosition();

            SoundManager.Instance.StopSE(ESEKey.Gimmick_Meteorite_Drop, audioIndex);
            audioIndex = SoundManager.Instance.PlaySE(ESEKey.Gimmick_Meteorite_Collide);
            // 落下後、Bottleの奥に描画する
            _meteoriteRenderer.sortingLayerName = Constants.SortingLayerName.METEORITE;
            // 隕石の跡が消えるまで待つ
            yield return new WaitUntil(() => _meteoriteAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash != _REMAINING_STATE_NAME_HASH);
            SoundManager.Instance.StopSE(ESEKey.Gimmick_Meteorite_Collide, audioIndex);
            Destroy(_shadow);
            Destroy(gameObject);
        }

        private IEnumerator MoveToTargetPosition()
        {
            // 攻撃のクリップの長さ、スタート位置、終了位置からスピードを算出
            var attackAnimClip = _meteoriteAnimator.runtimeAnimatorController.animationClips.Single(c => c.name == _ATTACK_ANIMATION_CLIP_NAME);
            var attackAnimationTime = attackAnimClip.length;
            var speed = _attackMoveDistance / attackAnimationTime;

            var direction = (_targetPos - (Vector2)transform.position);
            direction.Normalize();
            var animationTime = 0f;
            var startPosition = transform.position;
            while (animationTime <= attackAnimationTime) {
                transform.position = Vector2.Lerp(startPosition, _targetPos, animationTime / attackAnimationTime);
                animationTime += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            transform.position =  _targetPos;
        }

        /// <summary>
        /// 乱数配列のインデックスをボトルのIdに変換する
        /// </summary>
        /// <param name="index">_randomAttackableBottlesから取ったインデックス</param>
        /// <returns>ボトルのID</returns>
        private static int CalcBottleIdByRandomArrayIndex(int index)
        {
            var bottles = BottleLibrary.OrderedAttackableBottles;
            var bottleAtIndex = bottles.ElementAt(index);
            return bottleAtIndex.Id;
        }
    }
}
