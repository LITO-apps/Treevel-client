using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using PlayFab;
using Treevel.Common.Components.UIs;
using Treevel.Common.Managers;
using Treevel.Common.Networks;
using Treevel.Common.Networks.Requests;
using Treevel.Common.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Treevel.Modules.StartUpScene
{
    public class StartUpDirector : MonoBehaviour
    {
        [SerializeField] private GameObject _startButton;

        private async void Start()
        {
            SoundManager.Instance.PlayBGM(EBGMKey.StartUp);

            // Don't destroy EventSystem
            var eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem != null) DontDestroyOnLoad(eventSystem.gameObject);
            var tapObjectCanvas = FindObjectOfType<TapObjectGenerator>();
            if (tapObjectCanvas != null) DontDestroyOnLoad(tapObjectCanvas.GetComponent<Canvas>());

            // UIManager Initialize
            await UIManager.Instance.InitializeAsync();

            #if ENV_DEV
            Debug.Log("DEV");
            #else
            Debug.Log("PROD");
            #endif

            // AASを初期化
            await AddressableAssetManager.Initialize();

            // リモートサーバへ接続（ログイン）
            if (await NetworkService.Execute(new LoginRequest())) {
                Debug.Log($"PlayFabID[{PlayFabSettings.staticPlayer.PlayFabId}] Login Success");
            }

            // GameDataManager初期化
            await GameDataManager.InitializeAsync();

            await StageRecordService.Instance.PreloadAllStageRecordsAsync();
            await UserRecordService.Instance.PreloadUserRecordAsync()
                .ContinueWith(UserRecordService.Instance.UpdateStartupDaysAsync);

            // MenuSelectSceneを読み込み
            await AddressableAssetManager
                .LoadScene(Constants.SceneName.MENU_SELECT_SCENE, LoadSceneMode.Additive).ToUniTask();

            // Google Mobile Ad 初期化
            MobileAds.Initialize(initStatus => { });

            // 全部完了したら開始ボタンを表示
            _startButton.SetActive(true);
        }

        public void OnStartButtonClicked()
        {
            SoundManager.Instance.PlaySE(ESEKey.UI_Button_Click_Start_App);

            // MenuSelectSceneのBGMを流す
            SoundManager.Instance.PlayBGM(EBGMKey.MenuSelect);
            SoundManager.Instance.PlaySE(ESEKey.LevelSelect_River);

            // Unload Startup Scene
            SceneManager.UnloadSceneAsync(Constants.SceneName.START_UP_SCENE);
        }
    }
}
