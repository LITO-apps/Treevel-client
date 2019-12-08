﻿using System;
using System.Collections;
using System.IO;
using System.Linq;
using Project.Scripts.Utils.Definitions;
using Project.Scripts.GamePlayScene.Panel;
using Project.Scripts.UIComponents;
using Project.Scripts.Utils.PlayerPrefsUtils;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Project.Scripts.GamePlayScene
{
    public class GamePlayDirector : MonoBehaviour
    {
        private const string RESULT_WINDOW_NAME = "ResultPopup";
        private const string STAGE_NUMBER_TEXT_NAME = "StageNumberText";
        private const string PAUSE_WINDOW_NAME = "PauseWindow";
        private const string PAUSE_BUTTON_NAME = "PauseButton";

        public delegate void ChangeAction();

        /// <summary>
        /// 成功時のイベント
        /// </summary>
        public static event ChangeAction OnSucceed;

        /// <summary>
        /// 失敗時のイベント
        /// </summary>
        public static event ChangeAction OnFail;

        /// <summary>
        /// ゲームの状態一覧
        /// </summary>
        public enum EGameState {
            Opening,
            Playing,
            Success,
            Failure,
            Pausing
        }

        /// <summary>
        /// 木のレベル
        /// </summary>
        public static ELevelName levelName;

        /// <summary>
        /// 木のId
        /// </summary>
        public static ETreeName treeId;

        /// <summary>
        /// ステージ id
        /// </summary>
        public static int stageId;

        /// <summary>
        /// ゲームの現状態
        /// </summary>
        public EGameState state = EGameState.Opening;

        /// <summary>
        /// ゲーム画面以外を埋める背景
        /// </summary>
        [SerializeField] private GameObject _backgroundPrefab;

        /// <summary>
        /// 結果ウィンドウ
        /// </summary>
        private GameObject _resultWindow;

        /// <summary>
        /// 一時停止ウィンドウ
        /// </summary>
        private GameObject _pauseWindow;

        /// <summary>
        /// 一時停止ボタン
        /// </summary>
        private GameObject _pauseButton;

        /// <summary>
        /// ステージ id 表示用のテキスト
        /// </summary>
        private GameObject _stageNumberText;

        /// <summary>
        /// プレイ中の BGM
        /// </summary>
        private AudioSource _playingAudioSource;

        /// <summary>
        /// 成功時の音
        /// </summary>
        private AudioSource _successAudioSource;

        /// <summary>
        /// 失敗時の音
        /// </summary>
        private AudioSource _failureAudioSource;

        private void Awake()
        {
            _resultWindow = GameObject.Find(RESULT_WINDOW_NAME);

            _stageNumberText = GameObject.Find(STAGE_NUMBER_TEXT_NAME);

            _pauseWindow = GameObject.Find(PAUSE_WINDOW_NAME);
            _pauseButton = GameObject.Find(PAUSE_BUTTON_NAME);

            StartCoroutine(UnifyDisplay());

            SetAudioSources();
        }

        private void Start()
        {
            GameOpening();
        }

        /// <summary>
        /// アプリがバックグラウンドに移動する、または
        /// バックグラウンドから戻ってくる時に呼ばれる
        /// </summary>
        /// <param name="pauseStatus"></param>
        private void OnApplicationPause(bool pauseStatus)
        {
            // アプリがバックグラウンドに移動した時
            if (pauseStatus) {
                // 一時停止扱いとする
                Dispatch(EGameState.Pausing);
            }
        }

        /// <summary>
        /// アプリが終了した時に呼ばれる
        /// </summary>
        private void OnApplicationQuit()
        {
            // ゲーム失敗扱いとする
            Dispatch(EGameState.Failure);
        }

        /// <summary>
        /// ゲームがクリアしているかをチェックする
        /// </summary>
        public void CheckClear()
        {
            GameObject[] panels = GameObject.FindGameObjectsWithTag(TagName.NUMBER_PANEL);
            if (panels.Any(panel => panel.GetComponent<NumberPanelController>().Adapted == false)) return;
            // 全ての数字パネルが最終位置にいたら，成功状態に遷移
            Dispatch(EGameState.Success);
        }

        /// <summary>
        /// ゲームの状態を変更する
        /// </summary>
        /// <param name="nextState"> 変更したい状態 </param>
        /// <returns> 変更に成功したかどうか </returns>
        public bool Dispatch(EGameState nextState)
        {
            switch (nextState) {
                case EGameState.Opening:
                    // `Success`と`Failure`からの遷移のみを許す
                    if (state == EGameState.Success || state == EGameState.Failure) {
                        state = nextState;
                        GameOpening();
                        return true;
                    }

                    break;
                case EGameState.Playing:
                    // `Opening`と`Pausing`からの遷移のみを許す
                    if (state == EGameState.Opening || state == EGameState.Pausing) {
                        state = nextState;
                        GamePlaying();
                        return true;
                    }

                    break;
                case EGameState.Success:
                    // `Playing`からの遷移のみ許す
                    if (state == EGameState.Playing) {
                        state = nextState;
                        GameSucceed();
                        return true;
                    }

                    break;
                case EGameState.Failure:
                    // `Playing`と`Pausing`からの遷移のみ許す
                    if (state == EGameState.Playing || state == EGameState.Pausing) {
                        state = nextState;
                        GameFail();
                        return true;
                    }

                    break;
                case EGameState.Pausing:
                    // `Playing`からの遷移のみ許す
                    if (state == EGameState.Playing) {
                        state = nextState;
                        GamePausing();
                        return true;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(nextState), nextState, null);
            }

            return false;
        }

        /// <summary>
        /// 起動状態の処理
        /// </summary>
        private void GameOpening()
        {
            CleanObject();

            // StageStatusのデバッグ用
            var stageStatus = StageStatus.Get(stageId);
            var tmp = stageStatus.passed ? "クリア済み" : "未クリア";
            print("-------------------------------");
            print("ステージ" + stageId + "番は" + tmp + "です");
            print("ステージ" + stageId + "番の挑戦回数は" + stageStatus.challengeNum + "回です");
            print("ステージ" + stageId + "番の成功回数は" + stageStatus.successNum + "回です");
            print("ステージ" + stageId + "番の失敗回数は" + stageStatus.failureNum + "回です");
            print("-------------------------------");

            // 現在のステージ番号を格納
            _stageNumberText.GetComponent<Text>().text = levelName.ToString() + "_" + treeId.ToString() + "_" + stageId.ToString();

            // 結果ウィンドウを非表示
            _resultWindow.SetActive(false);

            // 一時停止ウィンドウを非表示
            _pauseWindow.SetActive(false);
            // 一時停止ボタンを有効にする
            _pauseButton.SetActive(true);

            // 番号に合わせたステージの作成
            StageGenerator.CreateStages(stageId);

            // 状態を変更する
            Dispatch(EGameState.Playing);
        }

        /// <summary>
        /// 開始状態の処理
        /// </summary>
        private void GamePlaying()
        {
            _playingAudioSource.Play();
        }

        /// <summary>
        /// 成功状態の処理
        /// </summary>
        private void GameSucceed()
        {
            EndProcess();
            _successAudioSource.Play();
            var ss = StageStatus.Get(stageId);
            // クリア済みにする
            ss.ClearStage(stageId);
            // 成功回数をインクリメント
            ss.IncSuccessNum(stageId);
            OnSucceed?.Invoke();
        }

        /// <summary>
        /// 失敗状態の処理
        /// </summary>
        private void GameFail()
        {
            EndProcess();
            _failureAudioSource.Play();
            // 失敗回数をインクリメント
            var ss = StageStatus.Get(stageId);
            ss.IncFailureNum(stageId);
            OnFail?.Invoke();
        }

        /// <summary>
        /// 一時停止状態の処理
        /// </summary>
        private void GamePausing()
        {
            // ゲーム内の時間を一時停止する
            Time.timeScale = 0.0f;
            // 一時停止ボタンを無効にする
            _pauseButton.SetActive(false);
            // 一時停止ウィンドウを表示する
            _pauseWindow.SetActive(true);
        }

        /// <summary>
        /// ゲーム終了時の共通処理
        /// </summary>
        private void EndProcess()
        {
            _playingAudioSource.Stop();
            _resultWindow.SetActive(true);
            // 一時停止ボタンを無効にする
            _pauseButton.SetActive(false);
        }

        /// <summary>
        /// 一時停止ボタン押下時の処理
        /// </summary>
        public void PauseButtonDown()
        {
            Dispatch(EGameState.Pausing);
        }

        /// <summary>
        /// タイル・パネル・銃弾オブジェクトの削除
        /// </summary>
        private static void CleanObject()
        {
            GameObject[] tiles = GameObject.FindGameObjectsWithTag(TagName.TILE);
            foreach (var tile in tiles) {
                // タイルの削除 (に伴いパネルも削除される)
                DestroyImmediate(tile);
            }

            GameObject[] bullets = GameObject.FindGameObjectsWithTag(TagName.BULLET);
            foreach (var bullet in bullets) {
                // 銃弾の削除
                DestroyImmediate(bullet);
            }
        }

        /// <summary>
        /// ゲーム画面のアスペクト比を統一する
        /// </summary>
        /// <param name="resultWindow"> 結果ウィンドウ </param>
        /// Bug: ゲーム画面遷移時にカメラ範囲が狭くなることがある
        private IEnumerator UnifyDisplay()
        {
            // 想定するデバイスのアスペクト比
            const float targetRatio = WindowSize.WIDTH / WindowSize.HEIGHT;
            // 実際のデバイスのアスペクト比
            var currentRatio = (float) Screen.width / Screen.height;
            // 許容するアスペクト比の誤差
            const float aspectRatioError = 0.001f;
            if (currentRatio < targetRatio - aspectRatioError || targetRatio + aspectRatioError < currentRatio) {
                // ゲーム盤面以外を埋める背景画像を表示する
                var background = Instantiate(_backgroundPrefab);
                background.transform.position = new Vector2(0f, 0f);
                var originalWidth = background.GetComponent<SpriteRenderer>().size.x;
                var originalHeight = background.GetComponent<SpriteRenderer>().size.y;
                var ratio = 0f;
                if (currentRatio > targetRatio + aspectRatioError) {
                    // 横長のデバイスの場合
                    ratio = targetRatio / currentRatio;
                    var rectX = (1 - ratio) / 2f;
                    background.transform.localScale = new Vector2(WindowSize.WIDTH / originalWidth / ratio, WindowSize.HEIGHT / originalHeight);
                    // 背景を描画するために1フレーム待つ
                    yield return null;
                    Destroy(background);
                    Camera.main.rect = new Rect(rectX, 0f, ratio, 1f);
                    // カメラの描画範囲を縮小させ、縮小させた範囲の背景を取り除くために1フレーム待つ
                    yield return null;
                } else if (currentRatio < targetRatio - aspectRatioError) {
                    // 縦長のデバイスの場合
                    ratio = currentRatio / targetRatio;
                    var rectY = (1 - ratio) / 2f;
                    background.transform.localScale = new Vector2(WindowSize.WIDTH / originalWidth / ratio, WindowSize.HEIGHT / originalHeight / ratio);
                    yield return null;
                    Destroy(background);
                    Camera.main.rect = new Rect(0f, rectY, 1f, ratio);
                    yield return null;
                }
                // 結果ウィンドウの大きさを変える
                _resultWindow.transform.localScale = new Vector2(ratio, ratio);
                // 一時停止ウィンドウの大きさを変える
                _pauseWindow.transform.localScale = new Vector2(ratio, ratio);
            }
        }

        /// <summary>
        /// 音源のセットアップ
        /// </summary>
        private void SetAudioSources()
        {
            // 各音源の設定
            // Playing
            gameObject.AddComponent<AudioSource>();
            _playingAudioSource = gameObject.GetComponents<AudioSource>()[0];
            SetAudioSource(clipName: AudioClipName.PLAYING, audioSource: _playingAudioSource, time: 2.0f, loop: true,
                volumeRate: 0.25f);
            // Success
            gameObject.AddComponent<AudioSource>();
            _successAudioSource = gameObject.GetComponents<AudioSource>()[1];
            SetAudioSource(clipName: AudioClipName.SUCCESS, audioSource: _successAudioSource);
            // Failure
            gameObject.AddComponent<AudioSource>();
            _failureAudioSource = gameObject.GetComponents<AudioSource>()[2];
            SetAudioSource(clipName: AudioClipName.FAILURE, audioSource: _failureAudioSource);
        }

        /// <summary>
        /// 個々の音源のセットアップ
        /// </summary>
        /// <param name="clipName"> 音源名 </param>
        /// <param name="audioSource"> 音源 </param>
        /// <param name="time"> 開始時間 </param>
        /// <param name="loop"> 繰り返しの有無 </param>
        /// <param name="volumeRate"> 設定音量に対する比率 </param>
        private static void SetAudioSource(string clipName, AudioSource audioSource, float time = 0.0f,
            bool loop = false, float volumeRate = 1.0f)
        {
            var clip = Resources.Load<AudioClip>("Clips/GamePlayScene/" + clipName);
            audioSource.clip = clip;
            audioSource.time = time;
            audioSource.loop = loop;
            audioSource.volume = PlayerPrefs.GetFloat(PlayerPrefsKeys.VOLUME, Audio.DEFAULT_VOLUME) * volumeRate;
        }
    }
}
