﻿using System;
using System.Linq;
using Project.Scripts.Utils.Definitions;
using Project.Scripts.GamePlayScene.Bullet;
using Project.Scripts.GamePlayScene.Panel;
using Project.Scripts.GamePlayScene.Tile;
using Project.Scripts.Utils.PlayerPrefsUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Project.Scripts.GamePlayScene
{
	public class GamePlayDirector : MonoBehaviour
	{
		public delegate void ChangeAction();

		public static event ChangeAction OnFail;
		public static event ChangeAction OnSucceed;

		public enum GameState
		{
			Opening,
			Playing,
			Success,
			Failure
		}

		public static int stageId;

		public GameState state = GameState.Opening;

		private GameObject tileGenerator;

		private GameObject panelGenerator;

		private GameObject bulletGenerator;

		private GameObject resultWindow;

		private GameObject resultText;

		private GameObject warningText;

		private GameObject stageNumberText;

		private AudioSource playingAudioSource;

		private AudioSource successAudioSource;

		private AudioSource failureAudioSource;

		private void Awake()
		{
			UnifyDisplay();

			tileGenerator = GameObject.Find("TileGenerator");
			panelGenerator = GameObject.Find("PanelGenerator");
			bulletGenerator = GameObject.Find("BulletGenerator");

			resultWindow = GameObject.Find("ResultWindow");

			resultText = resultWindow.transform.Find("Result").gameObject;
			warningText = resultWindow.transform.Find("Warning").gameObject;
			stageNumberText = GameObject.Find("StageNumberText");

			SetAudioSource();

			GameOpening();
		}

		private void OnApplicationPause(bool pauseStatus)
		{
			if (pauseStatus) // アプリがバックグラウンドに移動した時
			{
				if (Dispatch(GameState.Failure))
				{
					warningText.GetComponent<Text>().text = "アプリが\nバックグラウンドに\n移動しました";
				}
			}
		}

		public void CheckClear()
		{
			GameObject[] panels = GameObject.FindGameObjectsWithTag("NumberPanel");
			if (panels.Any(panel => panel.GetComponent<NumberPanelController>().adapted == false)) return;
			// 全ての数字パネルが最終位置にいたら，成功状態に遷移
			Dispatch(GameState.Success);
		}

		// 状態による振り分け処理
		public bool Dispatch(GameState nextState)
		{
			switch (nextState)
			{
				case GameState.Opening:
					// `Success`と`Failure`からの遷移のみを許す
					if (state == GameState.Success || state == GameState.Failure)
					{
						state = nextState;
						GameOpening();
						return true;
					}

					break;
				case GameState.Playing:
					// `Opening`からの遷移のみを許す
					if (state == GameState.Opening)
					{
						state = nextState;
						GamePlaying();
						return true;
					}

					break;
				case GameState.Success:
					// `Playing`からの遷移のみ許す
					if (state == GameState.Playing)
					{
						state = nextState;
						GameSucceed();
						return true;
					}

					break;
				case GameState.Failure:
					// `Playing`からの遷移のみ許す
					if (state == GameState.Playing)
					{
						state = nextState;
						GameFail();
						return true;
					}

					break;
				default:
					throw new ArgumentOutOfRangeException("nextState", nextState, null);
			}

			return false;
		}

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
			stageNumberText.GetComponent<Text>().text = stageId.ToString();

			// 結果ウィンドウを非表示
			resultWindow.SetActive(false);

			// 銃弾ジェネレータをアクティブ化
			bulletGenerator.SetActive(true);

			// タイル作成スクリプトを起動
			tileGenerator.GetComponent<TileGenerator>().CreateTiles(stageId);
			// パネル作成スクリプトを起動
			panelGenerator.GetComponent<PanelGenerator>().CreatePanels(stageId);
			// 銃弾作成スクリプトを起動
			bulletGenerator.GetComponent<BulletGenerator>().CreateBullets(stageId);

			// 状態を変更する
			Dispatch(GameState.Playing);
		}

		private void GamePlaying()
		{
			playingAudioSource.Play();
		}

		private void GameSucceed()
		{
			if (OnSucceed != null) OnSucceed();
			EndProcess();
			successAudioSource.Play();
			resultText.GetComponent<Text>().text = "成功!";
			var ss = StageStatus.Get(stageId);
			// クリア済みにする
			ss.ClearStage(stageId);
			// 成功回数をインクリメント
			ss.IncSuccessNum(stageId);
		}

		private void GameFail()
		{
			if (OnFail != null) OnFail();
			EndProcess();
			failureAudioSource.Play();
			resultText.GetComponent<Text>().text = "失敗!";
			// 失敗回数をインクリメント
			var ss = StageStatus.Get(stageId);
			ss.IncFailureNum(stageId);
		}

		private void EndProcess()
		{
			playingAudioSource.Stop();
			resultWindow.SetActive(true);
			bulletGenerator.SetActive(false);
		}

		public void RetryButtonDown()
		{
			// 挑戦回数をインクリメント
			var ss = StageStatus.Get(stageId);
			ss.IncChallengeNum(stageId);
			Dispatch(GameState.Opening);
		}

		public void BackButtonDown()
		{
			// StageSelectSceneに戻る
			SceneManager.LoadScene("MenuBarScene");
		}

		private static void CleanObject()
		{
			GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");
			foreach (var tile in tiles)
			{
				// タイルの削除 (に伴いパネルも削除される)
				Destroy(tile);
			}

			GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
			foreach (var bullet in bullets)
			{
				// 銃弾の削除
				Destroy(bullet);
			}
		}

		private static void UnifyDisplay()
		{
			// 想定するデバイスのアスペクト比
			const float targetRatio = WindowSize.WIDTH / WindowSize.HEIGHT;
			// 実際のデバイスのアスペクト比
			var currentRatio = (float) Screen.width / Screen.height;
			// 許容するアスペクト比の誤差
			const float aspectRatioError = 0.001f;
			if (currentRatio > targetRatio + aspectRatioError)
			{
				// 横長のデバイスの場合
				var ratio = targetRatio / currentRatio;
				var rectX = (1 - ratio) / 2f;
				Camera.main.rect = new Rect(rectX, 0f, ratio, 1f);
			}
			else if (currentRatio < targetRatio - aspectRatioError)
			{
				// 縦長のデバイスの場合
				var ratio = currentRatio / targetRatio;
				var rectY = (1 - ratio) / 2f;
				Camera.main.rect = new Rect(0f, rectY, 1f, ratio);
			}
		}

		private void SetAudioSource()
		{
			// 各音源の設定
			// Playing
			gameObject.AddComponent<AudioSource>();
			playingAudioSource = gameObject.GetComponents<AudioSource>()[0];
			SetAudioSource(clipName: "Playing", audioSource: playingAudioSource, time: 2.0f, volume: 0.10f, loop: true);
			// Success
			gameObject.AddComponent<AudioSource>();
			successAudioSource = gameObject.GetComponents<AudioSource>()[1];
			SetAudioSource(clipName: "Success", audioSource: successAudioSource, volume: 0.40f);
			// Failure
			gameObject.AddComponent<AudioSource>();
			failureAudioSource = gameObject.GetComponents<AudioSource>()[2];
			SetAudioSource(clipName: "Failure", audioSource: failureAudioSource, volume: 0.40f);
		}

		// AudioSourceの変数(音源名、開始時間、音量、繰り返しの有無)の設定
		private static void SetAudioSource(string clipName, AudioSource audioSource, float time = 0.0f,
			float volume = 1.0f, bool loop = false)
		{
			var clip = Resources.Load<AudioClip>("Clips/GamePlayScene/" + clipName);
			audioSource.clip = clip;
			audioSource.time = time;
			audioSource.volume = volume;
			audioSource.loop = loop;
		}
	}
}
