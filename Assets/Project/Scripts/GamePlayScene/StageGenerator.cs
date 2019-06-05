﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Project.Scripts.GamePlayScene.Bullet;
using Project.Scripts.GamePlayScene.Panel;
using Project.Scripts.GamePlayScene.Tile;
using Project.Scripts.Utils.Definitions;

namespace Project.Scripts.GamePlayScene
{
	public class StageGenerator : MonoBehaviour
	{
		private const string TILE_GENERATOR_NAME = "TileGenerator";
		private const string PANEL_GENERATOR_NAME = "PanelGenerator";
		private const string BULLET_GROUP_GENERATOR_NAME = "BulletGroupGenerator";

		private TileGenerator tileGenerator;

		private PanelGenerator panelGenerator;

		private BulletGroupGenerator bulletGroupGenerator;

		private void Awake()
		{
			tileGenerator = GameObject.Find(TILE_GENERATOR_NAME).GetComponent<TileGenerator>();
			panelGenerator = GameObject.Find(PANEL_GENERATOR_NAME).GetComponent<PanelGenerator>();
			bulletGroupGenerator = GameObject.Find(BULLET_GROUP_GENERATOR_NAME).GetComponent<BulletGroupGenerator>();
		}

		public void CreateStages(int stageId)
		{
			List<IEnumerator> coroutines = new List<IEnumerator>();

			switch (stageId)
			{
				case 1:
					// 銃弾実体生成
					coroutines.Add(bulletGroupGenerator.CreateCartridge(
						cartridgeType: CartridgeType.Turn,
						appearanceTime: 1.0f,
						interval: 1.0f,
						direction: CartridgeDirection.ToLeft,
						row: Row.Second,
						loop: true,
						bulletInfo: BulletInfo.SetTurnCartridgeInfo(
							turnDirection: new[] {(int) CartridgeDirection.ToUp, (int) CartridgeDirection.ToLeft},
							turnLine: new[] {(int) Column.Right, (int) Row.First})));
					coroutines.Add(bulletGroupGenerator.CreateCartridge(
						cartridgeType: CartridgeType.Normal,
						direction: CartridgeDirection.ToUp,
						column: Column.Right,
						appearanceTime: 2.0f,
						interval: 4.0f));

					/* 特殊タイル -> 数字パネル -> 特殊パネル */
					// 数字パネル作成
					panelGenerator.PrepareTilesAndCreateNumberPanels(
						new List<Dictionary<string, int>>()
						{
							PanelGenerator.ComvartToDictionary(panelNum: 1, initialTileNum: 4, finalTileNum: 4),
							PanelGenerator.ComvartToDictionary(panelNum: 2, initialTileNum: 5, finalTileNum: 5),
							PanelGenerator.ComvartToDictionary(panelNum: 3, initialTileNum: 6, finalTileNum: 6),
							PanelGenerator.ComvartToDictionary(panelNum: 4, initialTileNum: 7, finalTileNum: 7),
							PanelGenerator.ComvartToDictionary(panelNum: 5, initialTileNum: 8, finalTileNum: 8),
							PanelGenerator.ComvartToDictionary(panelNum: 6, initialTileNum: 9, finalTileNum: 9),
							PanelGenerator.ComvartToDictionary(panelNum: 7, initialTileNum: 10, finalTileNum: 10),
							PanelGenerator.ComvartToDictionary(panelNum: 8, initialTileNum: 14, finalTileNum: 11)
						}
					);
					// 特殊パネル作成
					panelGenerator.CreateDynamicDummyPanel(initialTileNum: 3);
					panelGenerator.CreateStaticDummyPanel(initialTileNum: 15);
					break;
				case 2:
					// 銃弾実体生成
					coroutines.Add(bulletGroupGenerator.CreateCartridge(
						cartridgeType: CartridgeType.Normal,
						appearanceTime: 2.0f,
						interval: 0.5f,
						direction: CartridgeDirection.ToRight,
						row: Row.Fifth,
						loop: false));
					coroutines.Add(bulletGroupGenerator.CreateHole(
						holeType: HoleType.Normal,
						appearanceTime: 1.0f,
						interval: 2.0f));
					/* 特殊タイル -> 数字パネル -> 特殊パネル */
					// 特殊タイル作成
					tileGenerator.CreateWarpTiles(firstTileNum: 2, secondTileNum: 14);
					// 数字パネル作成
					panelGenerator.PrepareTilesAndCreateNumberPanels(
						new List<Dictionary<string, int>>()
						{
							PanelGenerator.ComvartToDictionary(panelNum: 1, initialTileNum: 1, finalTileNum: 4),
							PanelGenerator.ComvartToDictionary(panelNum: 2, initialTileNum: 3, finalTileNum: 5),
							PanelGenerator.ComvartToDictionary(panelNum: 3, initialTileNum: 5, finalTileNum: 6),
							PanelGenerator.ComvartToDictionary(panelNum: 4, initialTileNum: 6, finalTileNum: 7),
							PanelGenerator.ComvartToDictionary(panelNum: 5, initialTileNum: 8, finalTileNum: 8),
							PanelGenerator.ComvartToDictionary(panelNum: 6, initialTileNum: 11, finalTileNum: 9),
							PanelGenerator.ComvartToDictionary(panelNum: 7, initialTileNum: 13, finalTileNum: 10),
							PanelGenerator.ComvartToDictionary(panelNum: 8, initialTileNum: 15, finalTileNum: 11)
						}
					);
					break;
				// 必ずパネルを撃ち抜く銃弾のテストステージ
				case 3:
					// 銃弾実体生成
					coroutines.Add(bulletGroupGenerator.CreateHole(
						holeType: HoleType.Aiming,
						appearanceTime: 2.0f,
						interval: 2.0f,
						bulletInfo: BulletInfo.SetAimingHoleInfo(
							aimingPanel: new[] {1, 2, 3, 4, 5, 6, 7, 8})));

					/* 特殊タイル -> 数字パネル -> 特殊パネル */
					// 特殊タイル作成
					tileGenerator.CreateWarpTiles(firstTileNum: 2, secondTileNum: 14);
					// 数字パネル作成
					panelGenerator.PrepareTilesAndCreateNumberPanels(
						new List<Dictionary<string, int>>()
						{
							PanelGenerator.ComvartToDictionary(panelNum: 1, initialTileNum: 1, finalTileNum: 4),
							PanelGenerator.ComvartToDictionary(panelNum: 2, initialTileNum: 3, finalTileNum: 5),
							PanelGenerator.ComvartToDictionary(panelNum: 3, initialTileNum: 5, finalTileNum: 6),
							PanelGenerator.ComvartToDictionary(panelNum: 4, initialTileNum: 6, finalTileNum: 7),
							PanelGenerator.ComvartToDictionary(panelNum: 5, initialTileNum: 8, finalTileNum: 8),
							PanelGenerator.ComvartToDictionary(panelNum: 6, initialTileNum: 11, finalTileNum: 9),
							PanelGenerator.ComvartToDictionary(panelNum: 7, initialTileNum: 13, finalTileNum: 10),
							PanelGenerator.ComvartToDictionary(panelNum: 8, initialTileNum: 15, finalTileNum: 11)
						}
					);
					break;
				// 記録画面テスト用 (`case 1`と全く同じ)
				case 1001:
					// 銃弾実体生成
					coroutines.Add(bulletGenerator.CreateCartridge(
						cartridgeType: CartridgeType.Turn,
						appearanceTime: 1.0f,
						interval: 1.0f,
						direction: CartridgeDirection.ToLeft,
						line: (int) Row.Second,
						loop: true,
						additionalInfo: BulletGenerator.SetTurnCartridgeInfo(
							turnDirection: new[] {(int) CartridgeDirection.ToUp, (int) CartridgeDirection.ToLeft},
							turnLine: new[] {(int) Column.Right, (int) Row.First})));
					coroutines.Add(bulletGenerator.CreateCartridge(
						cartridgeType: CartridgeType.Normal,
						direction: CartridgeDirection.ToUp,
						line: (int) Column.Right,
						appearanceTime: 2.0f,
						interval: 4.0f));

					/* 特殊タイル -> 数字パネル -> 特殊パネル */
					// 数字パネル作成
					panelGenerator.PrepareTilesAndCreateNumberPanels(
						new List<Dictionary<string, int>>()
						{
							PanelGenerator.ComvartToDictionary(panelNum: 1, initialTileNum: 4, finalTileNum: 4),
							PanelGenerator.ComvartToDictionary(panelNum: 2, initialTileNum: 5, finalTileNum: 5),
							PanelGenerator.ComvartToDictionary(panelNum: 3, initialTileNum: 6, finalTileNum: 6),
							PanelGenerator.ComvartToDictionary(panelNum: 4, initialTileNum: 7, finalTileNum: 7),
							PanelGenerator.ComvartToDictionary(panelNum: 5, initialTileNum: 8, finalTileNum: 8),
							PanelGenerator.ComvartToDictionary(panelNum: 6, initialTileNum: 9, finalTileNum: 9),
							PanelGenerator.ComvartToDictionary(panelNum: 7, initialTileNum: 10, finalTileNum: 10),
							PanelGenerator.ComvartToDictionary(panelNum: 8, initialTileNum: 14, finalTileNum: 11)
						}
					);
					// 特殊パネル作成
					panelGenerator.CreateDynamicDummyPanel(initialTileNum: 3);
					panelGenerator.CreateStaticDummyPanel(initialTileNum: 15);
					break;
				default:
					throw new NotImplementedException();
				// ランダムに銃弾を生成するテストステージ
				case 4:
					// 銃弾実体生成
					coroutines.Add(bulletGroupGenerator.CreateRandomBullet(
						appearanceTime: 2.0f,
						interval: 18.0f,
						bulletInfo: BulletInfo.SetBulletInfo(
							randomBulletType: new int[] {10, 90},
							randomCartridgeType: new int[] {100, 0},
							randomHoleType: new int[] {50, 50})));
					coroutines.Add(bulletGroupGenerator.CreateCartridge(
						cartridgeType: CartridgeType.Random,
						appearanceTime: 8.0f,
						interval: 18.0f,
						direction: CartridgeDirection.ToBottom,
						column: Column.Center,
						bulletInfo: BulletInfo.SetCartridgeInfo()));
					coroutines.Add(bulletGroupGenerator.CreateHole(
						holeType: HoleType.Random,
						appearanceTime: 14.0f,
						interval: 18.0f,
						bulletInfo: BulletInfo.SetHoleInfo()));
					// タイル作成
					tileGenerator.CreateNormalTiles();
					// パネル作成
					panelGenerator.CreateNumberPanel(panelNum: 1, initialTileNum: 1, finalTileNum: 4);
					panelGenerator.CreateNumberPanel(panelNum: 2, initialTileNum: 3, finalTileNum: 5);
					panelGenerator.CreateNumberPanel(panelNum: 3, initialTileNum: 5, finalTileNum: 6);
					panelGenerator.CreateNumberPanel(panelNum: 4, initialTileNum: 6, finalTileNum: 7);
					panelGenerator.CreateNumberPanel(panelNum: 5, initialTileNum: 8, finalTileNum: 8);
					panelGenerator.CreateNumberPanel(panelNum: 6, initialTileNum: 11, finalTileNum: 9);
					panelGenerator.CreateNumberPanel(panelNum: 7, initialTileNum: 13, finalTileNum: 10);
					panelGenerator.CreateNumberPanel(panelNum: 8, initialTileNum: 15, finalTileNum: 11);
					break;
			}

			// 銃弾の一括作成
			bulletGroupGenerator.CreateBullets(coroutines);
		}
	}
}
