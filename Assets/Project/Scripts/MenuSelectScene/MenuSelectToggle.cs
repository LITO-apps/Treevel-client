﻿using Project.Scripts.Utils.Definitions;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Project.Scripts.MenuSelectScene
{
    public class MenuSelectToggle : Toggle
    {
        protected override void Awake()
        {
            base.Awake();
            onValueChanged.AddListener(delegate {
                ToggleValueChanged(gameObject);
            });
        }

        /// <summary>
        /// トグルの状態が変更した場合の処理
        /// </summary>
        /// <param name="toggle"> 変更したトグル </param>
        protected virtual void ToggleValueChanged(GameObject toggle)
        {
            #if UNITY_EDITOR
            if (!EditorApplication.isPlaying) return;
            #endif

            // ONになった場合のみ処理
            if (isOn) {
                // 現在チェックされている Toggle を取得
                // LevelSelectSceneの時はStageSelect Toggleを選択する
                var nowScene = MenuSelectDirector.Instance.NowScene;
                if (nowScene == SceneName.LEVEL_SELECT_SCENE) nowScene = SceneName.STAGE_SELECT_SCENE;
                var checkedToggle = GameObject.Find(nowScene.Replace("Scene", ""));

                if (checkedToggle != null) {
                    checkedToggle.GetComponent<MenuSelectToggle>().isOn = false;
                    // 今のシーンをアンロード
                    SceneManager.UnloadSceneAsync(checkedToggle.GetComponent<MenuSelectToggle>().GetSceneName());
                    // 新しいシーンをロード
                    StartCoroutine(MenuSelectDirector.Instance.ChangeScene(GetSceneName()));
                }
            }
        }

        /// <summary>
        /// ToggleGroup の AllowSwitchOff=false を再現
        /// <see cref="Toggle.OnPointerClick"/>.
        /// </summary>
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            if (!IsActive() || !IsInteractable())
                return;

            // ON 状態のものを OFF にすることは許さない
            if (!isOn) isOn = true;
        }

        /// <summary>
        /// Toggleに紐づいているscene nameを返す
        /// </summary>
        /// <returns></returns>
        protected virtual string GetSceneName() {
            return name + "Scene";
        }
    }
}
