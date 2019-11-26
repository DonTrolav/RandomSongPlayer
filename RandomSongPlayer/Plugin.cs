﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using IPA;
using IPA.Config;
using IPA.Utilities;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;
using CustomUI.MenuButton;
using UnityEngine;
using System.Linq;
using SongCore;
using SongCore.Data;

namespace RandomSongPlayer
{
    public class Plugin : IBeatSaberPlugin
    {
        internal static System.Random rnd = new System.Random();
        internal static HttpClient client = new HttpClient();
        internal static Ref<PluginConfig> config;
        internal static IConfigProvider configProvider;
        internal static SeperateSongFolder randomSongsFolder;

        public void Init(IPALogger logger, [Config.Prefer("json")] IConfigProvider cfgProvider)
        {
            Logger.log = logger;
            configProvider = cfgProvider;

            Texture2D texture = LoadPNG("RandomSongAssets/random-song-tourney-icon.png");
            Sprite coverImage = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            
            randomSongsFolder = Collections.AddSeperateSongFolder("Random Songs", BeatSaber.InstallPath + "/" + Setup.RandomSongsFolder, FolderLevelPack.NewPack, coverImage);

            config = cfgProvider.MakeLink<PluginConfig>((p, v) =>
            {
                if (v.Value == null || v.Value.RegenerateConfig)
                    p.Store(v.Value = new PluginConfig() { RegenerateConfig = false });
                config = v;
            });
        }

        public static Texture2D LoadPNG(string filePath)
        {
            Texture2D tex = null;
            byte[] fileData;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2);
                
                tex.LoadImage(fileData);
            }
            return tex;
        }

        public void OnApplicationStart()
        {
            Logger.log.Debug("OnApplicationStart");
            Setup.InstantiateData();
        }

        public void OnApplicationQuit()
        {
            Logger.log.Debug("OnApplicationQuit");
        }

        public void OnFixedUpdate()
        {

        }

        public void OnUpdate()
        {

        }

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {/*
            var flowCoordinators = Resources.FindObjectsOfTypeAll<FlowCoordinator>();
            
            FileLogHelper.Log(prevScene.name);
            foreach (var flowCoordinator in flowCoordinators)
            {
                FileLogHelper.Log(flowCoordinator.name);
            }*/
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            try
            {
                if (scene.name == "MenuCore")
                {
                    MenuButtonUI.AddButton("Random Song Player", "Download a random song from Beat Saver and play it", () => { PlayRandomSongAsync(); });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error:");
            }
        }

        private void OnPlayRandomSongClicked()
        {
            var mainFlowCoordinator = Resources.FindObjectsOfTypeAll<MainFlowCoordinator>().First();
            RandomSongMenu randomSongMenu = mainFlowCoordinator.gameObject.AddComponent<RandomSongMenu>();
        }

        // this is just temporary ok!
        string path;
        public async void PlayRandomSongAsync()
        {
            await RandomSongGenerator.GenerateRandomKey(null);
            MapInstaller.InstallMap(RandomSongGenerator.mapData, out path);

            Loader.OnLevelPacksRefreshed += OnLevelPacksRefreshed;

            path = Path.GetFullPath(path);
            Logger.log.Info("Path: " + path);

            Loader.Instance.RefreshSongs(true);
        }

        private void OnLevelPacksRefreshed()
        {
            CustomPreviewBeatmapLevel installedMap = randomSongsFolder.Levels[path];
            var difficulty = (BeatmapDifficulty)Enum.Parse(typeof(BeatmapDifficulty), installedMap.standardLevelInfoSaveData.difficultyBeatmapSets.First().difficultyBeatmaps.Last().difficulty);

            LevelHelper.PlayLevel(installedMap, difficulty);

            Loader.OnLevelPacksRefreshed -= OnLevelPacksRefreshed;
        }

        public void OnSceneUnloaded(Scene scene)
        {

        }
    }
}