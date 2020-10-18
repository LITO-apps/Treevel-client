﻿using System;
using System.Collections;
using System.Linq;
using Treevel.Common.Entities;
using Treevel.Common.Utils;
using UnityEngine;

namespace Treevel.Modules.MenuSelectScene.LevelSelect
{
    public class RoadController : LineController
    {
        private LevelTreeController _endObjectController;

        protected override void Awake()
        {
            base.Awake();
            _endObjectController = endObject.GetComponent<LevelTreeController>();
        }

        protected override void Start()
        {
            base.Awake();
            lineRenderer.material.SetTextureScale("_MainTex", new Vector2(8 / lineLength, 1f));
        }

        protected override void SetSaveKey()
        {
            saveKey = $"{startObject.GetComponent<LevelTreeController>().treeId}{Constants.PlayerPrefsKeys.KEY_CONNECT_CHAR}{endObject.GetComponent<LevelTreeController>().treeId}";
        }

        public void Reset()
        {
            PlayerPrefs.DeleteKey(saveKey);
        }

        /// <summary>
        /// 道の状態の更新
        /// </summary>
        public override void UpdateState()
        {
            released = PlayerPrefs.GetInt(saveKey, Default.ROAD_RELEASED) == 1;

            if (!released) {
                if (constraintObjects.Length == 0) {
                    // 初期状態で解放されている道
                    released = true;
                    // 終点の木の状態の更新
                    _endObjectController.state = ETreeState.Released;
                    _endObjectController.ReflectTreeState();
                } else {
                    released = constraintObjects.All(tree => tree.GetComponent<LevelTreeController>().state >= ETreeState.Cleared);
                    if (released) {
                        // 道が非解放状態から解放状態に変わった時
                        StartCoroutine(ReleaseEndObject());
                    }
                }
            }

            if (!released) {
                // 非解放時
                lineRenderer.startColor = new Color(0.2f, 0.2f, 0.7f);
                lineRenderer.endColor = new Color(0.2f, 0.2f, 0.7f);
            }
        }

        /// <summary>
        /// 道が非解放状態から解放状態に変わった時のアニメーション(100フレームで色を変化させる)
        /// </summary>
        /// <returns></returns>
        private IEnumerator ReleaseEndObject()
        {
            // 終点の木の状態の更新
            _endObjectController.state = ETreeState.Released;

            // 道の更新アニメーション
            for (var i = 0; i < 100; i++) {
                lineRenderer.startColor = new Color((float)i / 100, (float)i / 100, (float)i / 100);
                lineRenderer.endColor = new Color((float)i / 100, (float)i / 100, (float)i / 100);
                yield return null;
            }

            // 終点の木の状態の更新アニメーション
            _endObjectController.ReflectTreeState();
        }

        /// <summary>
        /// 道の状態の保存
        /// </summary>
        public override void SaveState()
        {
            PlayerPrefs.SetInt(saveKey, Convert.ToInt32(released));
        }

        /// <summary>
        /// 線の幅の変更
        /// </summary>
        /// <param name="scale"> 拡大率 </param>
        public void ScaleWidth(float scale)
        {
            lineRenderer.startWidth = lineRenderer.endWidth = (float)Screen.width * width * scale;
        }
    }
}