using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Treevel.Common.Entities;
using Treevel.Common.Patterns.Singleton;
using UniRx;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Treevel.Common.Managers
{
    public enum EBGMKey
    {
        StartUp,
        MenuSelect,
        StageSelect_Spring,
        StageSelect_Summer,
        StageSelect_Autumn,
        StageSelect_Winter,
        GamePlay_Tutorial,
        GamePlay_Spring,
        GamePlay_Summer,
        GamePlay_Autumn,
        GamePlay_Winter,
        GamePlay_Difficult,
    }

    public enum ESEKey
    {
        UI_Dropdown_Close,
        UI_Button_Click_Start_App,
        UI_Button_Click_Start_Game,
        UI_Button_Click_General,
        UI_Button_Invalid,
        UI_SnapScrollView,
        LevelSelect_River,
        GamePlay_Failed_1,
        GamePlay_Failed_2,
        GamePlay_Success,
        GamePlay_Success_Cracker,
        Bottle_Move,
        Bottle_Break,
        Bottle_Destroy,
        ErasableBottle_In,
        ErasableBottle_Out,
        Tile_Warp,
        Gimmick_Powder,
        Gimmick_Gust,
        Gimmick_Thunder_1,
        Gimmick_Thunder_2,
        Gimmick_Meteorite_Collide,
        Gimmick_Meteorite_Drop,
    }

    public class SoundManager : SingletonObjectBase<SoundManager>
    {
        /// <summary>
        /// 同時再生できるSEの数
        /// </summary>
        private const int _MAX_SE_NUM = 16;

        /// <summary>
        /// 複数SE同時再生を考慮し、SE用のAudioSourceを複数用意する
        /// </summary>
        private AudioSource[] _sePlayers;

        /// <summary>
        /// BGM再生用のAudioSource
        /// </summary>
        private AudioSource _bgmPlayer;

        /// <summary>
        /// BGMリスト
        /// </summary>
        [SerializeField] private List<SoundData> _bgmClipList;

        /// <summary>
        /// SEリスト
        /// </summary>
        [SerializeField] private List<SoundData> _seClipList;

        /// <summary>
        /// 初期BGMボリューム
        /// </summary>
        [SerializeField] [Range(0, 1)] private float _INITIAL_BGM_VOLUME = 0.25f;

        /// <summary>
        /// 初期SEボリューム
        /// </summary>
        [SerializeField] [Range(0, 1)] private float _INITIAL_SE_VOLUME = 1.0f;

        /// <summary>
        /// オーディオミキサー
        /// </summary>
        [SerializeField] private AudioMixerGroup _bgmMixer;
        [SerializeField] private AudioMixerGroup _seMixer;

        /// <summary>
        /// BGMループでフェイドインフェイドアウトする時間（秒）
        /// </summary>
        private const float _BGM_LOOP_FADE_TIME = 2.0f;

        /// <summary>
        /// ループの音量を制御するタイマー用
        /// </summary>
        private IDisposable _loopVolumeController;

        /// <summary>
        /// Fade中のタスクをキャンセル用トークン
        /// </summary>
        private CancellationTokenSource _fadeCancellationToken;

        private void Awake()
        {
            if (!gameObject) {
                UIManager.Instance.ShowErrorMessageAsync(EErrorCode.UnknownError).Forget();
                return;
            }

            // BGM再生用のAudioSourceをアタッチする
            _bgmPlayer = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
            _bgmPlayer.loop = true;
            _bgmPlayer.playOnAwake = false;
            _bgmPlayer.outputAudioMixerGroup = _bgmMixer;

            // SE再生用のAudioSourceをアタッチする
            _sePlayers = new AudioSource[_MAX_SE_NUM];
            for (var i = 0; i < _MAX_SE_NUM; i++) {
                var player = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
                player.loop = false;
                player.playOnAwake = false;
                player.outputAudioMixerGroup = _seMixer;

                _sePlayers[i] = player;
            }

            _fadeCancellationToken = new CancellationTokenSource();
            _fadeCancellationToken.RegisterRaiseCancelOnDestroy(this);

            ResetVolume();
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// SEを再生する
        /// </summary>
        /// <param name="key"> 再生するSEのキー </param>
        /// <returns> 再生するAudioSourceのインデックス </returns>
        public int PlaySE(ESEKey key)
        {
            var clip = GetSEClip(key);
            if (clip == null) return -1;

            if (_sePlayers.Any(src => !src.isPlaying)) {
                // 再生していないAudioSourceを探す
                var playerIndex = Enumerable.Range(0, _MAX_SE_NUM)
                    .First(i => !_sePlayers.ElementAt(i).isPlaying);
                var player = _sePlayers.ElementAt(playerIndex);

                player.clip = clip;
                player.PlayOneShot(clip);
                return playerIndex;
            } else {
                Debug.LogWarning($"Failed to Play SE: {key} because audio sources are fully assigned");
                return -1;
            }
        }

        /// <summary>
        /// 与えられたSEのリストからランダムで一つを再生
        /// </summary>
        /// <param name="keys">SEリスト</param>
        public void PlaySERandom(ESEKey[] keys)
        {
            var playIndex = Random.Range(0, keys.Length);
            PlaySE(keys[playIndex]);
        }

        /// <summary>
        /// SEを停止する
        /// </summary>
        /// <param name="key"> 停止するSEのキー </param>
        /// <param name="index"> 停止するAudioSourceのインデックス </param>
        public void StopSE(ESEKey key, int index = -1)
        {
            var clip = GetSEClip(key);
            if (clip == null) return;

            if (0 <= index && index <= _MAX_SE_NUM) {
                // AudioSourceのインデックスを指定する場合
                var player = _sePlayers.ElementAt(index);
                if (!player.clip.name.Equals(key.ToString())) return;

                player.Stop();
                player.clip = null;
                return;
            }

            var players = _sePlayers
                .Where(src => src.clip != null && (src.clip.name == clip.name));
            foreach (var player in players) {
                player.Stop();
                player.clip = null;
            }
        }

        /// <summary>
        /// 複数のSEを停止する
        /// </summary>
        /// <param name="keys">停止したいSEのリスト</param>
        public void StopSE(IEnumerable<ESEKey> keys)
        {
            foreach (var key in keys) {
                StopSE(key);
            }
        }

        /// <summary>
        /// SE再生中かどうか
        /// </summary>
        /// <param name="key"> 確認したいSEのキー </param>
        /// <returns>再生中であれば`true`、そうでなければ`false`</returns>
        public bool IsPlayingSE(ESEKey key)
        {
            var clip = GetSEClip(key);
            if (clip == null) return false;

            return _sePlayers
                .Where(src => src.clip != null)
                .Any(src => (src.clip.name == clip.name) && src.isPlaying);
        }

        /// <summary>
        /// BGMを再生する
        /// </summary>
        /// <param name="key"> 再生するBGMのキー </param>
        /// <param name="playback"></param>
        public void PlayBGM(EBGMKey key, float playback = 0f, float fadeInTime = 2.0f)
        {
            var clip = GetBGMClip(key);
            if (clip == null) return;

            // 実行中のタイマーを取り消し
            _loopVolumeController?.Dispose();

            // Fade out タスクを止める
            _fadeCancellationToken.Cancel();

            // BGM再生中であれば停止しておく
            if (_bgmPlayer.isPlaying) _bgmPlayer.Stop();

            // フェイド中などで音量が変わった場合があるので一度リセットする
            ResetBGMVolume();

            _bgmPlayer.clip = clip;
            _bgmPlayer.time = playback;
            _bgmPlayer.Play();
            FadeBGMVolumeAsync(0, _bgmPlayer.volume, fadeInTime).Forget();

            if (_bgmPlayer.loop) {
                // ループ終わるところでフェイドアウトフェイドインするようにタイマーを設置する
                var clipLength = clip.length;
                _loopVolumeController = Observable.Timer(TimeSpan.FromSeconds(clipLength - _BGM_LOOP_FADE_TIME), TimeSpan.FromSeconds(clipLength)).Subscribe(_ => {
                    Debug.Log("Start FadeIn/FadeOut");
                    var initVolume = _bgmPlayer.volume;
                    FadeBGMVolumeAsync(_bgmPlayer.volume, 0, _BGM_LOOP_FADE_TIME).ContinueWith(() => FadeBGMVolumeAsync(0, initVolume, _BGM_LOOP_FADE_TIME));
                }).AddTo(this);
            }
        }

        /// <summary>
        /// BGMの音量をフェイドさせる
        /// </summary>
        /// <param name="from">初期値(0.0 to 1.0)</param>
        /// <param name="to">終値(0.0 to 1.0)</param>
        /// <param name="time">変化する時間(>0)</param>
        private async UniTask FadeBGMVolumeAsync(float from, float to, float time)
        {
            Debug.Assert(time > 0);

            var elapsed = 0f;
            while (elapsed < time) {
                if (_fadeCancellationToken.Token.IsCancellationRequested)
                    return;

                _bgmPlayer.volume = Mathf.Lerp(from, to, elapsed / time);
                await UniTask.WaitForEndOfFrame();
                elapsed += Time.deltaTime;
            }

            _bgmPlayer.volume = to;
        }

        /// <summary>
        /// BGM の音量を変更する
        /// </summary>
        /// <param name="volume"></param>
        public void SetMasterBGMVolume(float volume)
        {
            // 倍率からdBに変換する
            var dB = 10 * Mathf.Log10(volume);
            _bgmMixer.audioMixer.SetFloat("BGMVolume", dB);
        }

        /// <summary>
        /// 再生中のBGMを停止する
        /// </summary>
        public void StopBGMAsync(float fadeOutTime = 2.0f)
        {
            FadeBGMVolumeAsync(_bgmPlayer.volume, 0, fadeOutTime).ContinueWith(() => {
                _bgmPlayer.Stop();
                ResetBGMVolume();
            });
        }

        private AudioClip GetSEClip(ESEKey key)
        {
            try {
                var soundData = _seClipList.Single(data => data.key.Equals(key.ToString()));
                return soundData.clip;
            } catch (InvalidOperationException) {
                Debug.LogError($"Cannot Find SE clip with key: {key}");
                return null;
            } catch (ArgumentNullException) {
                Debug.LogError("SE Clip list is empty");
                return null;
            }
        }

        private AudioClip GetBGMClip(EBGMKey key)
        {
            try {
                var soundData = _bgmClipList.Single(data => data.key.Equals(key.ToString()));
                return soundData.clip;
            } catch (InvalidOperationException) {
                Debug.LogError($"Cannot Find BGM clip with key: {key}");
                return null;
            } catch (ArgumentNullException) {
                Debug.LogError("BGM Clip list is empty");
                return null;
            }
        }

        private void ResetBGMVolume()
        {
            _bgmPlayer.volume = _INITIAL_BGM_VOLUME * UserSettings.Instance.BGMVolume.Value;
        }

        private void ResetSEVolume()
        {
            foreach (var sePlayer in _sePlayers) {
                sePlayer.volume = _INITIAL_SE_VOLUME * UserSettings.Instance.SEVolume.Value;
            }
        }

        public void ResetVolume()
        {
            ResetBGMVolume();
            ResetSEVolume();
        }

        /// <summary>
        /// プレハブにクリップリストを保存できるようにデータ化するためのクラス
        /// </summary>
        [Serializable]
        private class SoundData
        {
            [SerializeField] public string key;
            [SerializeField] public AudioClip clip;
        }

        #if UNITY_EDITOR
        /// <summary>
        /// 簡単にゲーム中使用するSE、BGMを登録できるためエディタを改造する
        /// </summary>
        [CustomEditor(typeof(SoundManager))]
        public class SoundManagerEditor : Editor
        {
            private SerializedProperty _bgmListProp;
            private SerializedProperty _seListProp;

            public void OnEnable()
            {
                _bgmListProp = serializedObject.FindProperty("_bgmClipList");
                _seListProp = serializedObject.FindProperty("_seClipList");

                var src = target as SoundManager;

                if (!src) {
                    Debug.LogWarning("Failed to cast target to SoundManager");
                    return;
                }

                // 新規追加のBGMキーがある時の対応
                src._bgmClipList = Enum.GetNames(typeof(EBGMKey))
                    .ToDictionary(key => key, key => src._bgmClipList.Find(data => data.key.Equals(key)))
                    .Select(pair => pair.Value)
                    .ToList();

                // 新規追加のSEキーがある時の対応
                src._seClipList = Enum.GetNames(typeof(ESEKey))
                    .ToDictionary(key => key, key => src._seClipList.Find(data => data.key.Equals(key)))
                    .Select(pair => pair.Value)
                    .ToList();
            }

            public override void OnInspectorGUI()
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(serializedObject.FindProperty("_bgmMixer"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_seMixer"));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("_INITIAL_BGM_VOLUME"),
                                              new GUIContent("Initial BGM Volume"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_INITIAL_SE_VOLUME"),
                                              new GUIContent("Initial SE Volume"));

                EditorGUILayout.LabelField("BGM List", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                foreach (var eBgm in Enum.GetValues(typeof(EBGMKey)) as EBGMKey[]) {
                    var soundDataProp = _bgmListProp.GetArrayElementAtIndex((int)eBgm);
                    // BGMのキーを保存
                    var keyProp = soundDataProp.FindPropertyRelative("key");
                    keyProp.stringValue = eBgm.ToString();

                    EditorGUILayout.PropertyField(soundDataProp.FindPropertyRelative("clip"),
                                                  new GUIContent(eBgm.ToString()));
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.LabelField("SE List", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                foreach (var eSe in Enum.GetValues(typeof(ESEKey)) as ESEKey[]) {
                    var soundDataProp = _seListProp.GetArrayElementAtIndex((int)eSe);
                    var keyProp = soundDataProp.FindPropertyRelative("key");
                    keyProp.stringValue = eSe.ToString();

                    EditorGUILayout.PropertyField(soundDataProp.FindPropertyRelative("clip"),
                                                  new GUIContent(eSe.ToString()));
                }

                EditorGUILayout.EndVertical();

                if (!EditorGUI.EndChangeCheck()) return;

                // Set object dirty, this will make it be saved after saving the project.
                EditorUtility.SetDirty(serializedObject.targetObject);

                serializedObject.ApplyModifiedProperties();
            }
        }
        #endif
    }
}
