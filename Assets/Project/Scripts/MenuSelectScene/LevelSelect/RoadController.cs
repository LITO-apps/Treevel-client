﻿using Project.Scripts.Utils.Definitions;
using Project.Scripts.Utils.PlayerPrefsUtils;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.MenuSelectScene.LevelSelect
{
    public class RoadController : LineController
    {
        private TreeController _endObjectController;

        protected override void Awake()
        {
            base.Awake();
            _endObjectController = endObject.GetComponent<TreeController>();
        }

        protected override void SetSaveKey()
        {
            saveKey = $"{startObject.GetComponent<TreeController>().treeId}{PlayerPrefsKeys.KEY_CONNECT_CHAR}{endObject.GetComponent<TreeController>().treeId}";
        }

        public override void Reset()
        {
            PlayerPrefs.DeleteKey(saveKey);
        }

        /// <summary>
        /// 道の状態の更新
        /// </summary>
        public override void UpdateReleased()
        {
            released = PlayerPrefs.GetInt(saveKey, Default.ROAD_RELEASED) == 1;

            if (!released) {
                if (constraintObjects.Length == 0) {
                    // 初期状態で解放されている道
                    released = true;
                } else {
                    released = constraintObjects.All(tree => tree.GetComponent<TreeController>().state >= ETreeState.Cleared);
                }

                if (released) {
                    // 終点の木の状態の更新
                    _endObjectController.state = ETreeState.Released;
                    _endObjectController.ReflectTreeState();
                }
            }

            if (!released) {
                // 非解放時
                _renderer.startColor = new Color(0.2f, 0.2f, 0.7f);
                _renderer.endColor = new Color(0.2f, 0.2f, 0.7f);
            }
        }

        /// <summary>
        /// 道の状態の保存
        /// </summary>
        public override void SaveReleased()
        {
            PlayerPrefs.SetInt(saveKey, Convert.ToInt32(released));
        }
    }
}
