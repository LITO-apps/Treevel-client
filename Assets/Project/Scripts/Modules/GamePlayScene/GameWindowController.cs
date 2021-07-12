using Treevel.Common.Patterns.Singleton;
using Treevel.Common.Utils;
using UnityEngine;

namespace Treevel.Modules.GamePlayScene
{
    public class GameWindowController : SingletonObjectBase<GameWindowController>
    {
        /// <summary>
        /// ゲーム画面以外を埋める背景
        /// </summary>
        [SerializeField] private GameObject _background;

        /// <summary>
        /// ゲーム画面と同じ大きさの背景用マスク
        /// </summary>
        [SerializeField] private GameObject _backgroundMask;

        /// <summary>
        /// ゲーム空間の横幅
        /// </summary>
        private float _gameSpaceWidth = Constants.WindowSize.WIDTH;

        /// <summary>
        /// ゲーム空間のうち、任意のデバイスで描画が保証される領域の横幅
        /// </summary>
        private float _gameCoreSpaceWidth = Constants.WindowSize.WIDTH;

        private void Awake()
        {
            UnifyDisplay();
        }

        /// <summary>
        /// ゲーム画面のアスペクト比を統一する
        /// </summary>
        /// Bug: ゲーム画面遷移時にカメラ範囲が狭くなることがある
        private void UnifyDisplay()
        {
            // 想定するデバイスのアスペクト比
            const float targetRatio = Constants.WindowSize.WIDTH / Constants.WindowSize.HEIGHT;
            // 実際のデバイスのアスペクト比
            var currentRatio = (float)Screen.width / Screen.height;
            // 許容するアスペクト比の誤差
            const float aspectRatioError = 0.001f;

            if (Mathf.Abs(currentRatio - targetRatio) <= aspectRatioError) return;

            // ゲーム盤面以外を埋める背景画像を表示する
            var backgroundSize = _background.GetComponent<SpriteRenderer>().bounds.size;
            var originalWidth = backgroundSize.x;
            var originalHeight = backgroundSize.y;
            var camera = Camera.main;
            if (currentRatio > targetRatio + aspectRatioError) {
                // 横長のデバイスの場合
                var ratio = targetRatio / currentRatio;
                var rectX = (1 - ratio) / 2f;
                _backgroundMask.transform.localScale = new Vector2(Constants.WindowSize.WIDTH / originalWidth,
                                                                   Constants.WindowSize.HEIGHT / originalHeight);
                _background.transform.localScale = new Vector2(Constants.WindowSize.WIDTH / originalWidth / ratio,
                                                               Constants.WindowSize.HEIGHT / originalHeight);
                _gameSpaceWidth /= ratio;
            } else if (currentRatio < targetRatio - aspectRatioError) {
                // 縦長のデバイスの場合
                var ratio = currentRatio / targetRatio;
                var rectY = (1 - ratio) / 2f;
                _backgroundMask.transform.localScale = new Vector2(Constants.WindowSize.WIDTH / originalWidth * ratio,
                                                                   Constants.WindowSize.HEIGHT / originalHeight *
                                                                   ratio);
                _background.transform.localScale = new Vector2(Constants.WindowSize.WIDTH / originalWidth * ratio,
                                                               Constants.WindowSize.HEIGHT / originalHeight);
                // ゲーム画面の横幅を更新
                _gameSpaceWidth *= ratio;
                _gameCoreSpaceWidth *= ratio;
            }

            #if ENV_DEV
            // ゲーム画面を9:16に区切るダミー背景を描画する
            _backgroundMask.GetComponent<SpriteMask>().enabled = true;
            _background.GetComponent<SpriteRenderer>().enabled = true;
            #endif
        }

        /// <summary>
        /// デバイスの横幅を取得する
        /// </summary>
        /// <returns></returns>
        public float GetGameSpaceWidth()
        {
            return _gameSpaceWidth;
        }

        /// <summary>
        /// ゲーム空間のうち、任意のデバイスで描画が保証される領域の横幅を取得する
        /// </summary>
        /// <returns></returns>
        public float GetGameCoreSpaceWidth()
        {
            return _gameCoreSpaceWidth;
        }

        /// <summary>
        /// ゲーム空間のうち、任意のデバイスで描画が保証される領域の縦幅を取得する
        /// </summary>
        /// <returns></returns>
        public float GetGameCoreSpaceHeight()
        {
            return _gameCoreSpaceWidth * Constants.WindowSize.HEIGHT / Constants.WindowSize.WIDTH;
        }

        /// <summary>
        /// タイルの横幅を取得する
        /// </summary>
        /// <returns></returns>
        public float GetTileWidth()
        {
            return _gameCoreSpaceWidth * Constants.TileRatioToWindowWidth.WIDTH_RATIO;
        }

        /// <summary>
        /// タイルの縦幅を取得する
        /// </summary>
        /// <returns></returns>
        public float GetTileHeight()
        {
            return _gameCoreSpaceWidth * Constants.TileRatioToWindowWidth.HEIGHT_RATIO;
        }
    }
}
