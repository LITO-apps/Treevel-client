﻿using System;
using System.Collections;
using Project.Scripts.GamePlayScene;
using Project.Scripts.Utils;
using Project.Scripts.Utils.Definitions;
using Project.Scripts.Utils.PlayerPrefsUtils;
using SnapScroll;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Project.Scripts.StageSelectScene
{
    public class StageSelectDirector : MonoBehaviour
    {
        /// <summary>
        /// ステージボタンの Prefab
        /// </summary>
        [SerializeField] protected GameObject stageButtonPrefab;

        /// <summary>
        /// 概要を表示するポップアップ
        /// </summary>
        [SerializeField] private OverviewPopup _overviewPopup;

        private SnapScrollView _snapScrollView;

        private const string _LOADING_BACKGROUND = "LoadingBackground";
        private const string _LOADING = "Loading";
        private const string _LEFTBUTTON = "LeftButton";
        private const string _RIGHTBUTTON = "RightButton";
        private const string _BACKBUTTON = "BackButton";
        private const string _TREENAME = "TreeName";

        /// <summary>
        /// ロード中の背景
        /// </summary>
        private GameObject _loadingBackground;

        /// <summary>
        /// ロード中のアニメーション
        /// </summary>
        private GameObject _loading;

        /// <summary>
        /// 木のレベル
        /// </summary>
        public static ELevelName levelName;

        /// <summary>
        /// 木のId
        /// </summary>
        public static ETreeName treeId;

        /// <summary>
        /// 1つidの小さい木を表示するボタン
        /// </summary>
        private GameObject _leftButton;

        /// <summary>
        /// 1つidの大きい木を表示するボタン
        /// </summary>
        private GameObject _rightButton;

        /// <summary>
        /// LevelSelectSceneに戻るボタン
        /// </summary>
        private GameObject _backButton;

        /// <summary>
        /// 表示している木の名前
        /// </summary>
        private GameObject _treeName;

        // TODO: SnapScrollが働いたときの更新処理
        //       隣の木に移動するButtonが押せるかどうか
        //       木の名前を更新する

        private void Awake()
        {
            // 取得
            _snapScrollView = FindObjectOfType<SnapScrollView>();
            // ページの最大値を設定
            _snapScrollView.MaxPage = LevelInfo.TREE_NUM[levelName] - 1;
            // ページの横幅の設定
            _snapScrollView.PageSize = Screen.width;
            // ロード中背景を非表示にする
            _loadingBackground = GameObject.Find(_LOADING_BACKGROUND);
            _loadingBackground.SetActive(false);
            // ロードアニメーションを非表示にする
            _loading = GameObject.Find(_LOADING);
            _loading.SetActive(false);
            _overviewPopup = _overviewPopup ?? FindObjectOfType<OverviewPopup>();

            // UIの設定
            _leftButton = GameObject.Find(_LEFTBUTTON);
            _leftButton.GetComponent<Button>().onClick.AddListener(() => LeftButtonDown());
            _rightButton = GameObject.Find(_RIGHTBUTTON);
            _rightButton.GetComponent<Button>().onClick.AddListener(() => RightButtonDown());
            _backButton = GameObject.Find(_BACKBUTTON);
            _backButton.GetComponent<Button>().onClick.AddListener(() => BackButtonDown());
            _treeName = GameObject.Find(_TREENAME);

            // TODO: 非同期で呼び出す
            // 各ステージの選択ボタンなどを描画する
            Draw();

            // TODO: 表示している木の名前を描画する
            DrawTreeName();
        }

        /// <summary>
        /// 全難易度の画面を描画する
        /// </summary>
        private void Draw()
        {
            MakeButtons();
        }

        /// <summary>
        /// 表示している木の名前を描画する
        /// </summary>
        private void DrawTreeName()
        {
            // TODO: 現在表示している木の名前に更新する
            _treeName.GetComponentInChildren<Text>().text = treeId.ToString();
        }

        /// <summary>
        /// ボタンを配置する
        /// </summary>
        private void MakeButtons()
        {
            // 指定したレベルの全ての木の描画
            for (var i =0; i < LevelInfo.TREE_NUM[levelName]; i++) {
                // TODO: 選択した木を開くようにScrollViewを変更する
                for (var j = 0; j < TreeInfo.NUM[treeId]; j++) {
                    // ButtonのGameObjectを取得する
                    var button = GameObject.Find("Canvas/Trees/SnapScrollView/Viewport/Content/" + "Tree" + (i+1) + "/Stage" + (j+1));
                    // クリック時のリスナー
                    button.GetComponent<Button>().onClick.AddListener(() => StageButtonDown(button));
                    // TODO: ステージを選択できるか、ステージをクリアしたかどうかでButtonの表示を変更する
                }
                // TODO: ButtonとButtonの間に線を描画する
            }
        }

        /// <summary>
        /// タッチされたステージの概要を表示
        /// </summary>
        /// <param name="clickedButton"> タッチされたボタン </param>
        private void StageButtonDown(GameObject clickedButton)
        {
            // タップされたステージidを取得（暫定的にButtonのテキスト）
            Debug.Log(clickedButton);
            var stageId = int.Parse(clickedButton.GetComponentInChildren<Text>().text);

            if (PlayerPrefs.GetInt(PlayerPrefsKeys.DO_NOT_SHOW, 0) == 0) {
                // ポップアップを初期化する
                _overviewPopup.GetComponent<OverviewPopup>().SetStageId(stageId);
                // ポップアップを表示する
                _overviewPopup.gameObject.SetActive(true);
            } else {
                GoToGame(stageId);
            }
        }

        /// <summary>
        /// ステージ選択画面からゲーム選択画面へ移動する
        /// </summary>
        /// <param name="stageId"> ステージID </param>
        public void GoToGame(int stageId)
        {
            // 挑戦回数をインクリメント
            var ss = StageStatus.Get(stageId);
            ss.IncChallengeNum(stageId);
            // ステージ情報を渡す
            GamePlayDirector.levelName = levelName;
            GamePlayDirector.treeId = treeId;
            GamePlayDirector.stageId = stageId;
            // ロード中の背景を表示する
            _loadingBackground.SetActive(true);
            // ロード中のアニメーションを開始する
            _loading.SetActive(true);
            // シーン遷移
            AddressableAssetManager.Instance.LoadScene(SceneName.GAME_PLAY_SCENE);
        }

        /// <summary>
        /// 1つidの小さい木を表示する
        /// </summary>
        private void LeftButtonDown()
        {
            // TODO: 実装
        }

        /// <summary>
        /// 1つidの大きい木を表示する
        /// </summary>
        private void RightButtonDown()
        {
            // TODO: 実装
        }

        /// <summary>
        /// LevelSelectSceneに戻る
        /// </summary>
        private void BackButtonDown()
        {
            AddressableAssetManager.Instance.LoadScene(SceneName.MENU_SELECT_SCENE);
        }
    }
}
