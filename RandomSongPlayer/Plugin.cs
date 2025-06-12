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
using IPALogger = IPA.Logging.Logger;
using BeatSaberMarkupLanguage.Util;

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
            BeatSaverConfig = new BeatSaverOptions(applicationName: "RandomSongPlayer", version: new Version(2, 0, 3));
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

        [OnStart]
        public void OnApplicationStart()
        {
            MainMenuAwaiter.MainMenuInitializing += delegate {
                FilterSettingsUI.Init();
                QuickButtonUI.Init();
                if (RandomSongsFolder == null)
                {
                    Sprite rspLogo = SongCore.Utilities.Utils.LoadSpriteFromResources("RandomSongPlayer.Assets.rst-logo.png");
                    RandomSongsFolder = Collections.AddSeparateSongFolder("Random Songs", PluginConfig.Instance.SongFolderPath, FolderLevelPack.NewPack, rspLogo);
                }
            };
        }

        [OnEnable]
        public void OnEnable()
        {
            Filter.FilterHelper.Enable();
        }

        [OnDisable]
        public void OnDisable()
        {
            Filter.FilterHelper.Disable();
        }
    }
}
