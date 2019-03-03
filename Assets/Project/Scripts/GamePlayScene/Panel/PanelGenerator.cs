﻿using System;
using UnityEngine;

namespace Project.Scripts.GamePlayScene.Panel
{
	public class PanelGenerator : MonoBehaviour
	{
		public GameObject numberPanel1Prefab;
		public GameObject numberPanel2Prefab;
		public GameObject numberPanel3Prefab;
		public GameObject numberPanel4Prefab;
		public GameObject numberPanel5Prefab;
		public GameObject numberPanel6Prefab;
		public GameObject numberPanel7Prefab;
		public GameObject numberPanel8Prefab;
		public GameObject staticDummyPanelPrefab;
		public GameObject dynamicDummyPanelPrefab;

		// 現段階では8枚のパネル群
		public void CreatePanels(int stageId)
		{
			switch (stageId)
			{
				case 1:
					CreateDynamicDummyPanel(initialTileNum: "3", panelPrefab: dynamicDummyPanelPrefab);
					CreateNumberPanel(initialTileNum: "4", finalTileNum: "4", panelPrefab: numberPanel1Prefab);
					CreateNumberPanel(initialTileNum: "5", finalTileNum: "5", panelPrefab: numberPanel2Prefab);
					CreateNumberPanel(initialTileNum: "6", finalTileNum: "6", panelPrefab: numberPanel3Prefab);
					CreateNumberPanel(initialTileNum: "7", finalTileNum: "7", panelPrefab: numberPanel4Prefab);
					CreateNumberPanel(initialTileNum: "8", finalTileNum: "8", panelPrefab: numberPanel5Prefab);
					CreateNumberPanel(initialTileNum: "9", finalTileNum: "9", panelPrefab: numberPanel6Prefab);
					CreateNumberPanel(initialTileNum: "10", finalTileNum: "10", panelPrefab: numberPanel7Prefab);
					CreateNumberPanel(initialTileNum: "14", finalTileNum: "11", panelPrefab: numberPanel8Prefab);
					CreateStaticDummyPanel(initialTileNum: "15", panelPrefab: staticDummyPanelPrefab);
					break;
				case 2:
					CreateNumberPanel(initialTileNum: "1", finalTileNum: "4", panelPrefab: numberPanel1Prefab);
					CreateNumberPanel(initialTileNum: "3", finalTileNum: "5", panelPrefab: numberPanel2Prefab);
					CreateNumberPanel(initialTileNum: "5", finalTileNum: "6", panelPrefab: numberPanel3Prefab);
					CreateNumberPanel(initialTileNum: "6", finalTileNum: "7", panelPrefab: numberPanel4Prefab);
					CreateNumberPanel(initialTileNum: "8", finalTileNum: "8", panelPrefab: numberPanel5Prefab);
					CreateNumberPanel(initialTileNum: "11", finalTileNum: "9", panelPrefab: numberPanel6Prefab);
					CreateNumberPanel(initialTileNum: "13", finalTileNum: "10", panelPrefab: numberPanel7Prefab);
					CreateNumberPanel(initialTileNum: "15", finalTileNum: "11", panelPrefab: numberPanel8Prefab);
					break;
				default:
					throw new NotImplementedException();
			}
		}

		private static void CreateNumberPanel(string initialTileNum, string finalTileNum, GameObject panelPrefab)
		{
			var panel = Instantiate(panelPrefab);
			panel.GetComponent<NumberPanelController>().Initialize(initialTileNum, finalTileNum);
		}

		private static void CreateStaticDummyPanel(string initialTileNum, GameObject panelPrefab)
		{
			var panel = Instantiate(panelPrefab);
			panel.GetComponent<StaticPanelController>().Initialize(initialTileNum);
		}

		private static void CreateDynamicDummyPanel(string initialTileNum, GameObject panelPrefab)
		{
			var panel = Instantiate(panelPrefab);
			panel.GetComponent<DynamicPanelController>().Initialize(initialTileNum);
		}
	}
}
