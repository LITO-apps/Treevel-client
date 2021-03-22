﻿using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Treevel.Common.Entities;
using Treevel.Common.Managers;
using Treevel.Common.Networks;
using Treevel.Common.Networks.Requests;
using Treevel.Common.Utils;
using Treevel.Modules.StageSelectScene;
using UnityEngine;
using UnityEngine.UI;

namespace Treevel.Modules.MenuSelectScene.LevelSelect
{
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Button))]
    public class LevelTreeController : TreeControllerBase
    {
        /// <summary>
        /// 木のレベル
        /// </summary>
        [SerializeField] private ESeasonId _seasonId;

        [SerializeField] private Material _material;

        [SerializeField] private Button _button;

        public override async void UpdateState()
        {
            // 現在状態をPlayerPrefsから得る
            state = (ETreeState)Enum.ToObject(typeof(ETreeState),
                                              PlayerPrefs.GetInt(Constants.PlayerPrefsKeys.TREE + treeId.ToString(),
                                                                 Default.TREE_STATE));
            // 状態の更新
            switch (state) {
                case ETreeState.Unreleased:
                    break;
                case ETreeState.Released:
                    // Implementorに任せる
                    state = clearHandler.GetTreeState();
                    break;
                case ETreeState.Cleared:
                    // 全クリアかどうかをチェックする
                    var stageNum = treeId.GetStageNum();
                    var tasks = Enumerable.Range(1, stageNum)
                        // FIXME: 呼ばれるたびに ステージ数 分リクエストしてしまうので、リクエストを減らす工夫をする
                        .Select(s => NetworkService.Execute(new GetStageStatusRequest(treeId, s)));
                    var stageStatuses = await UniTask.WhenAll(tasks);
                    var clearStageNum = stageStatuses.Count(status => status.state == EStageState.Cleared);
                    state = clearStageNum == stageNum ? ETreeState.AllCleared : state;
                    break;
                case ETreeState.AllCleared:
                    break;
                default:
                    throw new NotImplementedException();
            }

            // 状態の反映
            ReflectTreeState();
        }

        public void Reset()
        {
            PlayerPrefs.DeleteKey(Constants.PlayerPrefsKeys.TREE + treeId.ToString());
        }

        protected override void ReflectUnreleasedState()
        {
            // グレースケール
            GetComponent<Image>().material = _material;
            _button.enabled = false;
        }

        protected override void ReflectReleasedState()
        {
            GetComponent<Image>().material = null;
            _button.enabled = true;
        }

        protected override void ReflectClearedState()
        {
            GetComponent<Image>().material = null;
            _button.enabled = true;
            // TODO: アニメーション
            Debug.Log($"{treeId} is cleared.");
        }

        protected override void ReflectAllClearedState()
        {
            GetComponent<Image>().material = null;
            _button.enabled = true;
            // TODO: アニメーション
            Debug.Log($"{treeId} is all cleared.");
        }

        /// <summary>
        /// 木が押されたとき
        /// </summary>
        public void TreeButtonDown()
        {
            StageSelectDirector.seasonId = _seasonId;
            StageSelectDirector.treeId = treeId;
            AddressableAssetManager.LoadScene(_seasonId.GetSceneName());
        }

        /// <summary>
        /// 木の状態の保存
        /// </summary>
        public void SaveState()
        {
            PlayerPrefs.SetInt(Constants.PlayerPrefsKeys.TREE + treeId, Convert.ToInt32(state));
        }
    }
}
