﻿using Project.Scripts.GameDatas;
using Project.Scripts.GamePlayScene.Tile;
using Project.Scripts.Utils;
using Project.Scripts.Utils.Definitions;
using SpriteGlow;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Project.Scripts.GamePlayScene.Bottle
{
    [RequireComponent(typeof(PostProcessVolume))]
    [RequireComponent(typeof(SpriteGlowEffect))]
    public class NormalBottleController : DynamicBottleController
    {
        /// <summary>
        /// 目標位置
        /// </summary>
        private int _targetPos;

        /// <summary>
        /// 光らせるエフェクト
        /// </summary>
        private SpriteGlowEffect _spriteGlowEffect;

        protected override void Awake()
        {
            base.Awake();
            OnEnterTile += HandleOnEnterTile;
            OnExitTile += HandleOnExitTile;
        }

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="bottleData">ボトルデータ</param>
        public override async void Initialize(BottleData bottleData)
        {
            _spriteGlowEffect = GetComponent<SpriteGlowEffect>();
            _spriteGlowEffect.enabled = false;

            // parse data
            var finalPos = bottleData.targetPos;
            _targetPos = finalPos;
            var targetTileSprite = AddressableAssetManager.GetAsset<Sprite>(bottleData.targetTileSprite);

            base.Initialize(bottleData);

            // set handler
            var lifeEffect = await AddressableAssetManager.Instantiate(Address.LIFE_EFFECT_PREFAB).Task;
            lifeEffect.GetComponent<LifeEffectController>().Initialize(this, bottleData.life);

            #if UNITY_EDITOR
            name = BottleName.NORMAL_BOTTLE + Id.ToString();
            #endif

            // 目標とするタイルのスプライトを設定
            var finalTile = BoardManager.Instance.GetTile(finalPos);
            finalTile.GetComponent<NormalTileController>().SetSprite(targetTileSprite);
        }

        private void HandleOnEnterTile(GameObject targetTile)
        {
            if (IsSuccess()) {
                // 最終タイルにいるかどうかで，光らせるかを決める
                _spriteGlowEffect.enabled = true;

                DoWhenSuccess();
            }
        }

        private void HandleOnExitTile(GameObject targetTile)
        {
            _spriteGlowEffect.enabled = false;
        }

        /// <summary>
        /// 目標タイルにいる時の処理
        /// </summary>
        private void DoWhenSuccess()
        {
            // ステージの成功判定
            GameObject.FindObjectOfType<GamePlayDirector>().CheckClear();
        }

        /// <summary>
        /// 目標タイルにいるかどうか
        /// </summary>
        /// <returns></returns>
        public override bool IsSuccess()
        {
            return BoardManager.Instance.GetBottlePos(this) == _targetPos;
        }
    }
}
