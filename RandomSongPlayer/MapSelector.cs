using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using HMUI;
using SongCore;
using RandomSongPlayer.UI;
using BeatSaverSharp.Models;

namespace RandomSongPlayer
{
    internal static class MapSelector
    {
        private static BeatmapCharacteristicSO selectThisCharacteristic = null;
        private static BeatmapDifficulty selectThisDifficulty = BeatmapDifficulty.Easy;
        private static BeatmapLevel beatmapCheck = null;

        private delegate void RSPDownloadedCallback(BeatmapLevel chosenSong, string chosenCharDiff);

        private static async Task DownloadRandomSongAsync(RSPDownloadedCallback callback)
        {
            (Beatmap mapData, string charDiff) = await RandomSongGenerator.GenerateRandomMap();
            if (mapData is null)
                return;

            (bool freshDownload, string path) = await MapInstaller.InstallMap(mapData);
            if (path is null)
            {
                Plugin.Log.Info("Could not download the map. Check beatsaver servers.");
                FilterSettingsUI.instance.ChangeWarning("Could not download the map. Check beatsaver servers.", FilterSettingsUI.COLOR_ORANGE);
                return;
            }

            Plugin.Log.Info("Chosen Random Song: " + path);
            Plugin.Log.Info("-> Char / Diff: " + charDiff);

            path = Path.GetFullPath(path);

            if (freshDownload)
            {
                void OnLevelPacksRefreshed()
                {
                    Loader.OnLevelPacksRefreshed -= OnLevelPacksRefreshed;
                    BeatmapLevel installedMap = Plugin.RandomSongsFolder.Levels[path];
                    callback?.Invoke(installedMap, charDiff);
                }

                Loader.OnLevelPacksRefreshed += OnLevelPacksRefreshed;
                Loader.Instance.RefreshSongs(false);
            }
            else
            {
                BeatmapLevel installedMap = Plugin.RandomSongsFolder.Levels[path];
                callback?.Invoke(installedMap, charDiff);
            }
        }

        internal static async Task SelectRandomSongAsync()
        {
            await DownloadRandomSongAsync(SelectBeatmap);
        }

        private static void SelectBeatmap(BeatmapLevel installedMap, string charDiff)
        {
            int installedLevelIndex = Plugin.RandomSongsFolder.LevelPack.beatmapLevels.Select((item, index) => new { item, index }).FirstOrDefault(x => x.item.levelID == installedMap.levelID).index;
            Plugin.Log.Debug($"Installed Level Index: {installedLevelIndex}");

            DismissPracticeView();

            LevelFilteringNavigationController levelNavigation = Resources.FindObjectsOfTypeAll<LevelFilteringNavigationController>().First();
            SelectLevelCategoryViewController levelSelector = levelNavigation._selectLevelCategoryViewController;
            StandardLevelDetailViewController levelDetails = Resources.FindObjectsOfTypeAll<StandardLevelDetailViewController>()?.First();
            levelSelector.Setup(SelectLevelCategoryViewController.LevelCategory.CustomSongs, levelNavigation._enabledLevelCategories);
            levelNavigation._levelPackIdToBeSelectedAfterPresent = Plugin.RandomSongsFolder.LevelPack.packID;
            levelNavigation.UpdateSecondChildControllerContent(levelSelector.selectedLevelCategory);

            (selectThisCharacteristic, selectThisDifficulty) = SearchDiffInInstalledMap(installedMap, charDiff);
            levelDetails.didChangeContentEvent -= SelectCharacteristicAndDifficulty;
            if (selectThisCharacteristic is null)
            {
                beatmapCheck = null;
            }
            else
            {
                beatmapCheck = installedMap;
                levelDetails.didChangeContentEvent += SelectCharacteristicAndDifficulty;
            }

            LevelCollectionTableView levelCollectionTable = Resources.FindObjectsOfTypeAll<LevelCollectionTableView>().First();
            levelCollectionTable.SelectLevel(installedMap);
        }

        private static void DismissPracticeView()
        {
            // Dismiss PracticeViewController if it's active
            LevelSelectionFlowCoordinator levelFlowCoordinator = Resources.FindObjectsOfTypeAll<LevelSelectionFlowCoordinator>().FirstOrDefault();
            PracticeViewController practivePreview = Resources.FindObjectsOfTypeAll<PracticeViewController>().FirstOrDefault();
            if (practivePreview != null && practivePreview.isActiveAndEnabled && levelFlowCoordinator != null)
                levelFlowCoordinator.DismissViewController(practivePreview, ViewController.AnimationDirection.Horizontal, null, true);
        }

        private static (BeatmapCharacteristicSO, BeatmapDifficulty) SearchDiffInInstalledMap(BeatmapLevel installedMap, string charDiff)
        {
            foreach (var characteristic in installedMap.GetCharacteristics())
            {
                foreach (var difficulty in installedMap.GetDifficulties(characteristic))
                {
                    if (characteristic.serializedName.ToLower() + difficulty.ToString().ToLower() == charDiff)
                        return (characteristic, difficulty);
                }
            }
            return (null, BeatmapDifficulty.Easy);
        }

        private static void SelectCharacteristicAndDifficulty(StandardLevelDetailViewController levelDetailView, StandardLevelDetailViewController.ContentType contentType)
        {
            if (levelDetailView?.beatmapLevel is null)
                return;
            if (levelDetailView.beatmapLevel.levelID != beatmapCheck?.levelID || contentType != StandardLevelDetailViewController.ContentType.OwnedAndReady)
                return;
            levelDetailView.didChangeContentEvent -= SelectCharacteristicAndDifficulty;
            StandardLevelDetailView standardLevelDetailView = levelDetailView._standardLevelDetailView;
            if (selectThisCharacteristic is null)
                return;

            Plugin.Log.Debug(selectThisCharacteristic.serializedName);
            Plugin.Log.Debug(selectThisDifficulty.ToString());

            BeatmapCharacteristicSegmentedControlController characteristControl = standardLevelDetailView._beatmapCharacteristicSegmentedControlController;
            IconSegmentedControl iconControl = characteristControl._segmentedControl;
            int characteristicIndex = characteristControl._beatmapCharacteristics.IndexOf(selectThisCharacteristic);
            if (characteristicIndex != -1)
            {
                iconControl.SelectCellWithNumber(characteristicIndex);
                characteristControl.HandleBeatmapCharacteristicSegmentedControlDidSelectCell(iconControl, characteristicIndex);
                BeatmapDifficultySegmentedControlController difficultyControl = standardLevelDetailView._beatmapDifficultySegmentedControlController;
                List<BeatmapDifficulty> difficulties = difficultyControl._difficulties;
                int difficultyIndex = difficulties.IndexOf(selectThisDifficulty);
                if (difficultyIndex != -1)
                {
                    difficultyControl._difficultySegmentedControl.SelectCellWithNumber(difficultyIndex);
                    difficultyControl.HandleDifficultySegmentedControlDidSelectCell(iconControl, difficultyIndex);
                }
            }
        }
    }
}
