﻿using System.Collections.Generic;
using Project.Scripts.GameDatas;
using Project.Scripts.GamePlayScene.Tile;
using Project.Scripts.Utils.Definitions;
using Project.Scripts.Utils.Patterns;
using UnityEngine;
using static Project.Scripts.GameDatas.StageData;

namespace Project.Scripts.GamePlayScene.Panel
{
    public class PanelGenerator : SingletonObject<PanelGenerator>
    {
        [SerializeField] private GameObject _numberPanelPrefab;
        [SerializeField] private GameObject _lifeNumberPanelPrefab;

        [SerializeField] private GameObject _staticDummyPanelPrefab;
        [SerializeField] private GameObject _dynamicDummyPanelPrefab;

        private TileGenerator _tileGenerator;

        private void Awake()
        {
            _tileGenerator = FindObjectOfType<TileGenerator>();
        }

        public void CreatePanels(ICollection<PanelData> panelDatas)
        {
            foreach (PanelData panelData in panelDatas) {
                switch (panelData.type) {
                    case EPanelType.Number:
                        var numberPanel = Instantiate(_numberPanelPrefab);
                        var numberPanelSprite = Resources.Load<Sprite>("Textures/Panel/numberPanel" + panelData.number);
                        if (numberPanelSprite != null) numberPanel.GetComponent<SpriteRenderer>().sprite = numberPanelSprite;
                        numberPanel.GetComponent<NumberPanelController>().Initialize(panelData.number, panelData.initPos, panelData.targetPos);
                        break;
                    case EPanelType.Dynamic:
                        CreateDynamicDummyPanel(panelData.initPos);
                        break;
                    case EPanelType.Static:
                        CreateStaticDummyPanel(panelData.initPos);
                        break;
                    case EPanelType.LifeNumber:
                        var lifeNumberPanel = Instantiate(_lifeNumberPanelPrefab);
                        var lifeNumberPanelSprite = Resources.Load<Sprite>("Textures/Panel/lifeNumberPanel" + panelData.number);
                        if (lifeNumberPanelSprite != null) lifeNumberPanel.GetComponent<SpriteRenderer>().sprite = lifeNumberPanelSprite;
                        lifeNumberPanel.GetComponent<LifeNumberPanelController>().Initialize(panelData.number, panelData.initPos, panelData.targetPos, panelData.life);
                        break;
                }
            }
        }

        /// <summary>
        /// 動かないダミーパネルを作成する
        /// </summary>
        /// <param name="initialTileNum"> 配置するタイルの番号 </param>
        public void CreateStaticDummyPanel(int initialTileNum)
        {
            var panel = Instantiate(_staticDummyPanelPrefab);
            panel.GetComponent<StaticPanelController>().Initialize(initialTileNum);
        }

        /// <summary>
        /// 動くダミーパネルを作成する
        /// </summary>
        /// <param name="initialTileNum"> 最初に配置するタイルの番号 </param>
        public void CreateDynamicDummyPanel(int initialTileNum)
        {
            var panel = Instantiate(_dynamicDummyPanelPrefab);
            panel.GetComponent<DynamicPanelController>().Initialize(initialTileNum);
        }
    }
}
