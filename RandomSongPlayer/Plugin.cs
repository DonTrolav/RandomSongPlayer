using BeatSaberMarkupLanguage;
using BeatSaverSharp;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using RandomSongPlayer.Configuration;
using RandomSongPlayer.UI;
using SongCore;
using SongCore.Data;
using SongDetailsCache;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using UnityEngine;
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
        internal static SeperateSongFolder RandomSongsFolder { get; private set; }
        internal static SongDetails SongDetails { get; private set; }
        #endregion

        [Init]
        public Plugin(IPALogger logger)
        {
            Instance = this;
            Log = logger;
            Log?.Debug("Logger initialized.");

            //Settings.Update();
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
            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            Log.Debug("Config loaded");
        }

        [OnEnable]
        public void OnEnable()
        {
            Filter.FilterHelper.Enable();
            BeatSaberMarkupLanguage.GameplaySetup.GameplaySetup.instance.AddTab("RSP", "RandomSongPlayer.UI.FilterSettings.bsml", UI.FilterSettingsUI.instance);
            if (RandomSongsFolder == null)
            {
                Sprite rspLogo = SongCore.Utilities.Utils.LoadSpriteFromResources("RandomSongPlayer.Assets.rst-logo.png");
                RandomSongsFolder = Collections.AddSeperateSongFolder("Random Songs", PluginConfig.Instance.SongFolderPath, FolderLevelPack.NewPack, rspLogo);
            }
            BS_Utils.Utilities.BSEvents.lateMenuSceneLoadedFresh += BSEvents_lateMenuSceneLoadedFresh;
            BS_Utils.Utilities.BSEvents.OnLoad();
        }

        [OnDisable]
        public void OnDisable()
        {
            BeatSaberMarkupLanguage.GameplaySetup.GameplaySetup.instance.RemoveTab("RSP");
            Filter.FilterHelper.Disable();
        }

        private void BSEvents_lateMenuSceneLoadedFresh(ScenesTransitionSetupDataSO sO)
        {
            BS_Utils.Utilities.BSEvents.lateMenuSceneLoadedFresh -= BSEvents_lateMenuSceneLoadedFresh;
            var levelFiltering = Resources.FindObjectsOfTypeAll<LevelFilteringNavigationController>().First();
            QuickButtonUI.instance.Setup();
            levelFiltering.didSelectAnnotatedBeatmapLevelCollectionEvent -= OnMapPackChange;
            levelFiltering.didSelectAnnotatedBeatmapLevelCollectionEvent += OnMapPackChange;
        }

        private void OnMapPackChange(LevelFilteringNavigationController levelFilteringNavigationController, IAnnotatedBeatmapLevelCollection iAnnotatedBeatmapLevelCollection, GameObject gameObject, BeatmapCharacteristicSO beatmapCharacteristicSO)
        {
            if (QuickButtonUI.instance == null)
                return;
            if (!PluginConfig.Instance.ShowQuickButton)
            {
                QuickButtonUI.instance.Hide();
                return;
            }

            IBeatmapLevelPack levelPack = iAnnotatedBeatmapLevelCollection as IBeatmapLevelPack;
            if (levelPack?.packName == "Random Songs")
                QuickButtonUI.instance.Show();
            else
                QuickButtonUI.instance.Hide();
        }
    }
}
