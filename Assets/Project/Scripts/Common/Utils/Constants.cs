using UnityEngine;

namespace Treevel.Common.Utils
{
    public class Constants
    {
        /// <summary>
        /// FPS
        /// </summary>
        public static readonly int FRAME_RATE = (int)Mathf.Round(1.0f / Time.fixedDeltaTime);

        /// <summary>
        /// Google AdMob 関連定数
        /// </summary>
        public static class MobileAds
        {
            #if UNITY_IOS
            public const string BANNER_UNIT_ID_GAME_PAUSED = "ca-app-pub-3388557544498929/9549692311";
            public const string BANNER_UNIT_ID_STAGE_SELECT = "ca-app-pub-3388557544498929/7381913729";
            #elif UNITY_ANDROID
            public const string BANNER_UNIT_ID_GAME_PAUSED = "ca-app-pub-3388557544498929/4716453440";
            public const string BANNER_UNIT_ID_STAGE_SELECT = "ca-app-pub-3388557544498929/3390697828";
            #else
            public const string BANNER_UNIT_ID_GAME_PAUSED = "";
            public const string BANNER_UNIT_ID_STAGE_SELECT = "";
            #endif
        }

        /// <summary>
        /// PlayerPrefs で使うキー群
        /// </summary>
        public static class PlayerPrefsKeys
        {
            public const string TREE = "TREE";
            public const string BRANCH_STATE = "BranchState";
            public const string ROAD_ANIMATION_STATE = "RoadAnimationState";
            public const string TREE_ANIMATION_STATE = "TreeAnimationState";
            public const string BGM_VOLUME = "BGM_VOLUME";
            public const string SE_VOLUME = "SE_VOLUME";
            public const string LANGUAGE = "LANGUAGE";
            public const string STAGE_DETAILS = "STAGE_DETAILS";
            public const string LEVEL_SELECT_CANVAS_SCALE = "LEVEL_SELECT_CANVAS_SCALE";
            public const string LEVEL_SELECT_SCROLL_POSITION = "LEVEL_SELECT_SCROLL_POSITION";
            public const char KEY_CONNECT_CHAR = '-';
            public const string USER_RECORD = PlayFabKeys.USER_RECORD;
            public const string DATABASE_LOGIN_ID = "DATABASE_LOGIN_ID";
        }

        public static class PlayFabKeys
        {
            public const string USER_RECORD = "user_record";
        }

        /// <summary>
        /// アニメーションクリップの名前
        /// </summary>
        public static class AnimationClipName
        {
            public const string BOTTLE_GET_ATTACKED = "BottleGetAttacked";
            public const string BOTTLE_DEAD = "BottleDead";
        }

        /// <summary>
        /// タイルの名前
        /// </summary>
        public static class TileName
        {
            public const string NORMAL_TILE = "NormalTile";
            public const string GOAL_TILE = "GoalTile";
            public const string WARP_TILE = "WarpTile";
            public const string HOLY_TILE = "HolyTile";
            public const string SPIDERWEB_TILE = "SpiderwebTile";
            public const string ICE_TILE = "IceTile";
        }

        /// <summary>
        /// ボトルの名前
        /// </summary>
        public static class BottleName
        {
            public const string DYNAMIC_DUMMY_BOTTLE = "DynamicDummyBottle";
            public const string STATIC_DUMMY_BOTTLE = "StaticDummyBottle";
            public const string NORMAL_BOTTLE = "NormalBottle";
            public const string ATTACKABLE_DUMMY_BOTTLE = "AttackableDummyBottle";
            public const string ERASABLE_BOTTLE = "ErasableBottle";
        }

        /// <summary>
        /// ステージ情報
        /// </summary>
        public static class StageSize
        {
            /// <summary>
            /// ステージのタイル行数
            /// </summary>
            public const int ROW = 5;

            /// <summary>
            /// ステージのタイル列数
            /// </summary>
            public const int COLUMN = 3;

            /// <summary>
            /// タイルの合計数
            /// </summary>
            public const int TILE_NUM = ROW * COLUMN;
        }

        /// <summary>
        /// シーンの名前
        /// </summary>
        public static class SceneName
        {
            public const string MENU_SELECT_SCENE = "MenuSelectScene";
            public const string SPRING_STAGE_SELECT_SCENE = "SpringStageSelectScene";
            public const string SUMMER_STAGE_SELECT_SCENE = "SummerStageSelectScene";
            public const string AUTUMN_STAGE_SELECT_SCENE = "AutumnStageSelectScene";
            public const string WINTER_STAGE_SELECT_SCENE = "WinterStageSelectScene";
            public const string GAME_PLAY_SCENE = "GamePlayScene";
            public const string START_UP_SCENE = "StartUpScene";
        }

        /// <summary>
        /// Sorting Layer の名前
        /// </summary>
        public static class SortingLayerName
        {
            public const string TILE = "Tile";
            public const string METEORITE = "Meteorite";
            public const string BOTTLE = "Bottle";
            public const string GIMMICK = "Gimmick";
            public const string GIMMICK_WARNING = "GimmickWarning";
            public const string ROAD = "Road";
        }

        /// <summary>
        /// タグの名前
        /// </summary>
        public static class TagName
        {
            public const string TILE = "Tile";
            public const string NORMAL_BOTTLE = "NormalBottle";
            public const string DUMMY_BOTTLE = "DummyBottle";
            public const string GIMMICK = "Gimmick";
            public const string GIMMICK_WARNING = "GimmickWarning";
            public const string GRAPH_UI = "GraphUi";
            public const string TREE = "Tree";
            public const string ROAD = "Road";
            public const string STAGE = "Stage";
            public const string BRANCH = "Branch";
        }

        /// <summary>
        /// Addressable Asset System で使うアドレス、
        /// Addressables Groups Windowsの「Addressable Name」と一致する必要がある
        /// </summary>
        public static class Address
        {
            // ボトル関連
            public const string GOAL_BOTTLE_PREFAB = "GoalBottlePrefab";
            public const string DYNAMIC_DUMMY_BOTTLE_PREFAB = "DynamicDummyBottlePrefab";
            public const string STATIC_DUMMY_BOTTLE_PREFAB = "StaticDummyBottlePrefab";
            public const string ERASABLE_BOTTLE_PREFAB = "ErasableBottlePrefab";
            public const string ATTACKABLE_DUMMY_BOTTLE_PREFAB = "AttackableDummyBottlePrefab";
            public const string DYNAMIC_DUMMY_BOTTLE_SPRITE = "dynamicDummyBottle";
            public const string STATIC_DUMMY_BOTTLE_SPRITE = "staticDummyBottle";
            public const string GOAL_BOTTLE_SPRITE_PREFIX = "goalBottle_";

            // ボトル関連のエフェクト
            public const string SELFISH_ATTRIBUTE_PREFAB = "SelfishAttributePrefab";
            public const string LIFE_ATTRIBUTE_PREFAB = "LifeAttributePrefab";
            public const string LIFE_VALUE_SPRITE_PREFIX = "life_";
            public const string LIFE_CRACK_SPRITE_INFIX = "_life_";
            public const string DARK_ATTRIBUTE_PREFAB = "DarkAttributePrefab";
            public const string REVERSE_ATTRIBUTE_PREFAB = "ReverseAttributePrefab";

            // タイル関連
            public const string NORMAL_TILE_PREFAB = "NormalTilePrefab";
            public const string NORMAL_TILE_SPRITE_PREFIX = "normalTile_";
            public const string GOAL_TILE_PREFAB = "GoalTilePrefab";
            public const string WARP_TILE_PREFAB = "WarpTilePrefab";
            public const string HOLY_TILE_PREFAB = "HolyTilePrefab";
            public const string SPIDERWEB_TILE_PREFAB = "SpiderwebTilePrefab";
            public const string SPIDERWEB_TILE_SPRITE = "SpiderwebTile";
            public const string SPIDERWEB_TILE_BOTTOM_LEFT_SPRITE = SPIDERWEB_TILE_SPRITE + "_bottom_left";
            public const string SPIDERWEB_TILE_BOTTOM_RIGHT_SPRITE = SPIDERWEB_TILE_SPRITE +"_bottom_right";
            public const string SPIDERWEB_TILE_TOP_LEFT_SPRITE = SPIDERWEB_TILE_SPRITE + "_top_left";
            public const string SPIDERWEB_TILE_TOP_RIGHT_SPRITE = SPIDERWEB_TILE_SPRITE + "_top_right";
            public const string SPIDERWEB_TILE_ON_BOTTLE_PREFIX = "_on_bottle";
            public const string ICE_TILE_PREFAB = "IceTilePrefab";
            public const string ICE_LAYER_MATERIAL = "IceLayerMaterial";
            public const string GOAL_TILE_SPRITE_PREFIX = "goalTile_";

            // 銃弾関連
            public const string NORMAL_HOLE_GENERATOR_PREFAB = "NormalHoleGeneratorPrefab";
            public const string AIMING_HOLE_GENERATOR_PREFAB = "AimingHoleGeneratorPrefab";

            // ギミック
            public const string TORNADO_PREFAB = "TornadoPrefab";
            public const string TORNADO_WARNING_SPRITE = "TornadoWarning";
            public const string TURN_WARNING_LEFT_SPRITE = "turnLeft";
            public const string TURN_WARNING_RIGHT_SPRITE = "turnRight";
            public const string TURN_WARNING_UP_SPRITE = "turnUp";
            public const string TURN_WARNING_BOTTOM_SPRITE = "turnBottom";
            public const string METEORITE_PREFAB = "MeteoritePrefab";
            public const string METEORITE_SHADOW_PREFAB = "MeteoriteShadowPrefab";
            public const string AIMING_METEORITE_PREFAB = "AimingMeteoritePrefab";
            public const string THUNDER_PREFAB = "ThunderPrefab";
            public const string SOLAR_BEAM_PREFAB = "SolarBeamPrefab";
            public const string GUST_WIND_PREFAB = "GustWindPrefab";
            public const string FOG_PREFAB = "FogPrefab";
            public const string POWDER_PREFAB = "PowderPrefab";
            public const string SAND_PILED_UP_POWDER_PREFAB = "SandPiledUpPowderPrefab";
            public const string SAND_POWDER_BACKGROUND_SPRITE = "SandBackground";
            public const string SAND_POWDER_PARTICLE_MATERIAL = "SandParticle";
            public const string ERASABLE_PREFAB = "ErasablePrefab";
        }

        /// <summary>
        /// 理想的なデバイス(iPhone X/XS)のサイズ
        /// </summary>
        public static class DeviceSize
        {
            public const int WIDTH = 1125;
            public const int HEIGHT = 2236;
        }

        /// <summary>
        /// ゲーム画面のウィンドウサイズ
        /// </summary>
        public static class WindowSize
        {
            public const float WIDTH = 900;
            public const float HEIGHT = 1600;
        }

        /// <summary>
        /// タイルの大きさ
        /// </summary>
        public static class TileRatioToWindowWidth
        {
            public const float WIDTH_RATIO = 0.22f;
            public const float HEIGHT_RATIO = WIDTH_RATIO;
        }

        /// <summary>
        /// ボトルの大きさ
        /// </summary>
        public static class BottleRatioToWindowWidth
        {
            public const float WIDTH_RATIO = TileRatioToWindowWidth.WIDTH_RATIO * 0.95f;
            public const float HEIGHT_RATIO = WIDTH_RATIO;
        }

        /// <summary>
        /// 一季節が許容できる木の数
        /// </summary>
        public const int MAX_TREE_NUM_IN_SEASON = 1000;

        public static class TimelineReferenceKey
        {
            public const string ROAD_TO_RELEASE = "RoadToRelease";
            public const string TREE_TO_RELEASE = "TreeToRelease";
        }
    }
}
