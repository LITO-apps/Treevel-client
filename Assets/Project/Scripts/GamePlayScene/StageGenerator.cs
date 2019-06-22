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
		public const string BULLET_GROUP_GENERATOR_NAME = "BulletGroupGenerator";

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
					coroutines.Add(bulletGroupGenerator.CreateBulletGroup(
						appearanceTime: 1.0f,
						interval: 5.0f,
						loop: false,
						bulletGenerators: new List<GameObject>()
						{
							bulletGroupGenerator.CreateNormalCartridgeGenerator(ratio: 10,
									cartridgeDirection: CartridgeDirection.Random, row: Row.Random),
							bulletGroupGenerator.CreateTurnCartridgeGenerator(ratio: 10,
									cartridgeDirection: CartridgeDirection.Random, row: Row.Random),
							bulletGroupGenerator.CreateNormalHoleGenerator(ratio: 10, row: Row.Random,
									column: Column.Random),
							bulletGroupGenerator.CreateAimingHoleGenerator(ratio: 10)
						}));
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
					coroutines.Add(bulletGroupGenerator.CreateBulletGroup(
						appearanceTime: 1.0f,
						interval: 5.0f,
						loop: true,
						bulletGenerators: new List<GameObject>()
						{
							bulletGroupGenerator.CreateNormalCartridgeGenerator(ratio: 10,
									cartridgeDirection: CartridgeDirection.ToLeft, row: Row.First)
						}));
					coroutines.Add(bulletGroupGenerator.CreateBulletGroup(
						appearanceTime: 3.0f,
						interval: 5.0f,
						loop: true,
						bulletGenerators: new List<GameObject>()
						{
							bulletGroupGenerator.CreateNormalHoleGenerator(ratio: 10, row: Row.First,
								column: Column.Random)
						}));
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
					coroutines.Add(bulletGroupGenerator.CreateBulletGroup(
						appearanceTime: 3.0f,
						interval: 5.0f,
						loop: true,
						bulletGenerators: new List<GameObject>()
						{
							bulletGroupGenerator.CreateAimingHoleGenerator(ratio: 10)
						}));
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
				// ランダムに銃弾を生成するテストステージ
				case 4:
					// 銃弾実体生成
					coroutines.Add(bulletGroupGenerator.CreateBulletGroup(
						appearanceTime: 1.0f,
						interval: 5.0f,
						loop: false,
						bulletGenerators: new List<GameObject>()
						{
							{
								bulletGroupGenerator.CreateNormalCartridgeGenerator(ratio: 10,
									cartridgeDirection: CartridgeDirection.Random, row: Row.Random)
							},
							{
								bulletGroupGenerator.CreateTurnCartridgeGenerator(ratio: 10,
									cartridgeDirection: CartridgeDirection.Random, row: Row.Random)
							},
							{
								bulletGroupGenerator.CreateNormalHoleGenerator(ratio: 10, row: Row.Random,
									column: Column.Random)
							},
							{bulletGroupGenerator.CreateAimingHoleGenerator(ratio: 10)}
						}));
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
				// 記録画面テスト用 (`case 1`と全く同じ)
				case 1001:
					// 銃弾実体生成
					coroutines.Add(bulletGroupGenerator.CreateBulletGroup(
						appearanceTime: 1.0f,
						interval: 5.0f,
						loop: false,
						bulletGenerators: new List<GameObject>()
						{
							bulletGroupGenerator.CreateNormalCartridgeGenerator(ratio: 10,
								cartridgeDirection: CartridgeDirection.Random, row: Row.Random),
							bulletGroupGenerator.CreateTurnCartridgeGenerator(ratio: 10,
								cartridgeDirection: CartridgeDirection.Random, row: Row.Random),
							bulletGroupGenerator.CreateNormalHoleGenerator(ratio: 10, row: Row.Random,
								column: Column.Random),
							bulletGroupGenerator.CreateAimingHoleGenerator(ratio: 10)
						}));
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
			}

			// 銃弾の一括作成
			bulletGroupGenerator.CreateBulletGroups(coroutines);
		}
	}
}
