﻿using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Treevel.Common.Entities;
using Treevel.Common.Managers;
using Treevel.Modules.GamePlayScene.Bottle;
using Treevel.Modules.GamePlayScene.Gimmick;
using Treevel.Modules.GamePlayScene.Tile;
using UnityEngine;

namespace Treevel.Modules.GamePlayScene
{
    public static class StageGenerator
    {
        /// <summary>
        /// ステージ作成が完了したかどうかのフラグ
        /// </summary>
        public static bool CreatedFinished { get; private set; }

        /// <summary>
        /// ステージを作成する
        /// </summary>
        /// <param name="treeId"> 木のID </param>
        /// <param name="stageNumber"> ステージ番号 </param>
        /// <exception cref="NotImplementedException"> 実装されていないステージ id を指定した場合 </exception>
        public static async void CreateStages(ETreeId treeId, int stageNumber)
        {
            CreatedFinished = false;
            var tasks = new List<UniTask>();

            // ステージデータ読み込む
            var stageData = GameDataManager.GetStage(treeId, stageNumber);
            if (stageData != null) {
                await UniTask.WhenAll(
                    TileGenerator.Instance.CreateTiles(stageData.TileDatas),
                    GimmickGenerator.Instance.Initialize(stageData.GimmickDatas)
                );
                await BottleGenerator.CreateBottles(stageData.BottleDatas);
            } else {
                // 存在しないステージ
                Debug.LogError("Unable to create a stage whose stageId is " + stageNumber + ".");
            }

            CreatedFinished = true;
            GamePlayDirector.Instance.Dispatch(GamePlayDirector.EGameState.Playing);
        }
    }
}
