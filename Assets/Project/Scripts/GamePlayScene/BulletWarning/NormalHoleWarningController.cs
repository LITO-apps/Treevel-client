﻿using UnityEngine;
using Project.Scripts.Utils.Definitions;

namespace Project.Scripts.GamePlayScene.BulletWarning
{
    public class NormalHoleWarningController : BulletWarningController
    {
        protected override void Awake()
        {
            base.Awake();
            transform.localScale =
                new Vector2(HoleWarningSize.WIDTH / originalWidth, HoleWarningSize.HEIGHT / originalHeight);
        }

        /// <summary>
        /// 座標を設定する
        /// </summary>
        /// <param name="row"> 出現する行</param>
        /// <param name="column"> 出現する列 </param>
        public void Initialize(int row, int column)
        {
            const float topTilePositionY = WindowSize.HEIGHT * 0.5f - (TileSize.MARGIN_TOP + TileSize.HEIGHT * 0.5f);
            transform.position =
                new Vector2(TileSize.WIDTH * (column - 2), topTilePositionY - TileSize.HEIGHT * (row - 1));
        }
    }
}
