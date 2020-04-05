﻿namespace Project.Scripts.Utils.Definitions
{
    /// <summary>
    /// シーンの名前
    /// </summary>
    public static class SceneName
    {
        public const string MENU_SELECT_SCENE = "MenuSelectScene";
        public const string LEVEL_SELECT_SCENE = "LevelSelectScene";
        public const string SPRING_STAGE_SELECT_SCENE = "SpringStageSelectScene";
        public const string SUMMER_STAGE_SELECT_SCENE = "SummerStageSelectScene";
        public const string AUTOMN_STAGE_SELECT_SCENE = "AutomnStageSelectScene";
        public const string WINTER_STAGE_SELECT_SCENE = "WinterStageSelectScene";
        public const string GAME_PLAY_SCENE = "GamePlayScene";
        public const string GAME_PAUSE_SCENE = "GamePauseScene";
        public const string RECORD_SCENE = "RecordScene";
        public const string TUTORIAL_SCENE = "TutorialScene";
        public const string CONFIG_SCENE = "ConfigScene";
    }

    /// <summary>
    /// Sorting Layer の名前
    /// </summary>
    public static class SortingLayerName
    {
        public const string TILE = "Tile";
        public const string HOLE = "Hole";
        public const string PANEL = "Panel";
        public const string BULLET = "Bullet";
        public const string BULLET_WARNING = "BulletWarning";
    }

    /// <summary>
    /// タグの名前
    /// </summary>
    public static class TagName
    {
        public const string TILE = "Tile";
        public const string NUMBER_PANEL = "NumberPanel";
        public const string DUMMY_PANEL = "DummyPanel";
        public const string BULLET = "Bullet";
        public const string BULLET_WARNING = "BulletWarning";
        public const string GRAPH_UI = "GraphUi";
    }

    /// <summary>
    /// Addressable Asset System で使うアドレス、
    /// Addressables Groups Windowsの「Addressable Name」と一致する必要がある
    /// </summary>
    public static class Address
    {
        // パネル関連
        public const string NUMBER_PANEL_PREFAB = "NumberPanelPrefab";
        public const string LIFE_NUMBER_PANEL_PREFAB = "LifeNumberPanelPrefab";
        public const string DYNAMIC_DUMMY_PANEL_PREFAB  = "DynamicDummyPanelPrefab";
        public const string STATIC_DUMMY_PANEL_PREFAB = "StaticDummyPanelPrefab";
        public const string DYNAMIC_DUMMY_PANEL_SPRITE = "dynamicDummyPanel";
        public const string STATIC_DUMMY_PANEL_SPRITE = "staticDummyPanel";
        public const string NUMBER_PANEL_SPRITE_PREFIX = "numberPanel";
        public const string LIFE_NUMBER_PANEL_SPRITE_PREFIX = "lifeNumberPanel";

        // タイル関連
        public const string NORMAL_TILE_PREFAB = "normalTilePrefab";
        public const string WARP_TILE_PREFAB = "warpTilePrefab";
        public const string NUMBER_TILE_SPRITE_PREFIX = "numberTile";

        // 銃弾関連
        public const string NORMAL_CARTRIDGE_GENERATOR_PREFAB = "NormalCartridgeGeneratorPrefab";
        public const string TURN_CARTRIDGE_GENERATOR_PREFAB = "TurnCartridgeGeneratorPrefab";
        public const string NORMAL_HOLE_GENERATOR_PREFAB = "NormalHoleGeneratorPrefab";
        public const string AIMING_HOLE_GENERATOR_PREFAB = "AimingHoleGeneratorPrefab";
    }
}
