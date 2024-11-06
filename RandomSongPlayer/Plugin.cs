using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using UnityEngine;
using IPA;
using IPA.Config.Stores;
using RandomSongPlayer.Configuration;
using RandomSongPlayer.UI;
using SongCore;
using SongCore.Data;
using BeatSaverSharp;
using SongDetailsCache;
using BS_Utils.Utilities;
using IPALogger = IPA.Logging.Logger;

namespace RandomSongPlayer
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        #region Properties
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }
        internal static HttpClient HttpClient { get; private set; }
        internal static BeatSaver BeatsaverClient { get; private set; }
        private static BeatSaverOptions BeatSaverConfig { get; set; }
        internal static SeparateSongFolder RandomSongsFolder { get; private set; }
        internal static SongDetails SongDetails { get; private set; }
        #endregion

        [Init]
        public Plugin(IPALogger logger)
        {
            Instance = this;
            Log = logger;
            Log?.Debug("Logger initialized.");

            HttpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(2) };
            BeatSaverConfig = new BeatSaverOptions(applicationName: "RandomSongPlayer", version: new Version(2, 0, 0));
            BeatsaverClient = new BeatSaver(BeatSaverConfig);

            Stopwatch sw = Stopwatch.StartNew();
            SongDetails = SongDetails.Init().GetAwaiter().GetResult();
            logger.Debug($"Song Details loaded in {sw.ElapsedMilliseconds}ms");
        }

        [Init]
        public void InitWithConfig(IPA.Config.Config conf)
        {
            PluginConfig.Instance = conf.Generated<PluginConfig>();
            Log.Debug("Config loaded");
        }

        [OnEnable]
        public void OnEnable()
        {
            Filter.FilterHelper.Enable();
            BSEvents.lateMenuSceneLoadedFresh += BSEvents_lateMenuSceneLoadedFresh;
            BSEvents.OnLoad();
        }

        [OnDisable]
        public void OnDisable()
        {
            BeatSaberMarkupLanguage.GameplaySetup.GameplaySetup.Instance.RemoveTab("RSP");
            Filter.FilterHelper.Disable();
        }

        private void BSEvents_lateMenuSceneLoadedFresh(ScenesTransitionSetupDataSO sO)
        {
            BeatSaberMarkupLanguage.GameplaySetup.GameplaySetup.Instance.AddTab("RSP", "RandomSongPlayer.UI.FilterSettings.bsml", UI.FilterSettingsUI.instance);
            if (RandomSongsFolder == null)
            {
                Sprite rspLogo = SongCore.Utilities.Utils.LoadSpriteFromResources("RandomSongPlayer.Assets.rst-logo.png");
                RandomSongsFolder = Collections.AddSeparateSongFolder("Random Songs", PluginConfig.Instance.SongFolderPath, FolderLevelPack.NewPack, rspLogo);
            }

            BSEvents.lateMenuSceneLoadedFresh -= BSEvents_lateMenuSceneLoadedFresh;
            var levelFiltering = Resources.FindObjectsOfTypeAll<LevelFilteringNavigationController>().First();
            QuickButtonUI.instance.Setup();
            levelFiltering.didSelectBeatmapLevelPackEvent -= OnMapPackChange;
            levelFiltering.didSelectBeatmapLevelPackEvent += OnMapPackChange;
        }

        private void OnMapPackChange(LevelFilteringNavigationController levelFilter, BeatmapLevelPack levelPack, GameObject gameObject, LevelSelectionOptions options)
        {
            if (QuickButtonUI.instance == null)
                return;

            switch (PluginConfig.Instance.QuickButton.ShowMode)
            {
                case ShowMode.Never:
                    QuickButtonUI.instance.Hide();
                    break;
                case ShowMode.OnRandomPack:
                    if (levelPack?.packName == "Random Songs")
                        QuickButtonUI.instance.Show();
                    else
                        QuickButtonUI.instance.Hide();
                    break;
                case ShowMode.Always:
                    QuickButtonUI.instance.Show();
                    break;
            }
        }
    }
}
