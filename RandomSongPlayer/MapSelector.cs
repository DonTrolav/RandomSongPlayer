using BeatSaverSharp.Models;
using HMUI;
using IPA.Utilities;
using RandomSongPlayer.UI;
using SongCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RandomSongPlayer
{
    internal static class MapSelector
    {
        private static BeatmapCharacteristicSO selectThisCharacteristic = null;
        private static BeatmapDifficulty selectThisDifficulty = BeatmapDifficulty.Easy;
        private static IPreviewBeatmapLevel beatmapCheck = null;

        private delegate void RSPDownloadedCallback(CustomPreviewBeatmapLevel chosenSong, string chosenCharDiff);

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
                    CustomPreviewBeatmapLevel installedMap = Plugin.RandomSongsFolder.Levels[path];
                    callback?.Invoke(installedMap, charDiff);
                }

                Loader.OnLevelPacksRefreshed += OnLevelPacksRefreshed;
                Loader.Instance.RefreshSongs(false);
            }
            else
            {
                CustomPreviewBeatmapLevel installedMap = Plugin.RandomSongsFolder.Levels[path];
                callback?.Invoke(installedMap, charDiff);
            }
        }

        internal static async Task SelectRandomSongAsync()
        {
            await DownloadRandomSongAsync(SelectBeatmap);
        }

        private static void SelectBeatmap(CustomPreviewBeatmapLevel installedMap, string charDiff)
        {
            int installedLevelIndex = Plugin.RandomSongsFolder.LevelPack.beatmapLevelCollection.beatmapLevels.Select((item, index) => new { item, index }).FirstOrDefault(x => x.item.levelID == installedMap.levelID).index;
            Plugin.Log.Debug($"Installed Level Index: {installedLevelIndex}");

            // Dismiss PracticeViewController if it's active
            LevelSelectionFlowCoordinator levelFlowCoordinator = Resources.FindObjectsOfTypeAll<LevelSelectionFlowCoordinator>().FirstOrDefault(); ;
            PracticeViewController practivePreview = Resources.FindObjectsOfTypeAll<PracticeViewController>().FirstOrDefault();
            if (practivePreview != null && practivePreview.isActiveAndEnabled && levelFlowCoordinator != null)
                levelFlowCoordinator.DismissViewController(practivePreview, ViewController.AnimationDirection.Horizontal, null, true);

            LevelFilteringNavigationController levelNavigation = Resources.FindObjectsOfTypeAll<LevelFilteringNavigationController>().First();
            SelectLevelCategoryViewController levelSelector = levelNavigation._selectLevelCategoryViewController;
            StandardLevelDetailViewController levelDetails = Resources.FindObjectsOfTypeAll<StandardLevelDetailViewController>()?.First();
            levelSelector.Setup(SelectLevelCategoryViewController.LevelCategory.CustomSongs, levelNavigation._enabledLevelCategories);
            levelNavigation._levelPackIdToBeSelectedAfterPresent = Plugin.RandomSongsFolder.LevelPack.packID;
            levelNavigation.UpdateSecondChildControllerContent(levelSelector.selectedLevelCategory);

            PreviewDifficultyBeatmapSet previewDifficultyBeatmapSet = installedMap.previewDifficultyBeatmapSets.Where(x => x.beatmapDifficulties.Any(y => x.beatmapCharacteristic.serializedName.ToLower() + y.ToString().ToLower() == charDiff)).FirstOrDefault();
            levelDetails.didChangeContentEvent -= SelectCharacteristicAndDifficulty;
            if (previewDifficultyBeatmapSet != null)
            {
                selectThisCharacteristic = previewDifficultyBeatmapSet.beatmapCharacteristic;
                selectThisDifficulty = previewDifficultyBeatmapSet.beatmapDifficulties.Where(x => charDiff.EndsWith(x.ToString().ToLower())).First();
                beatmapCheck = installedMap;
                levelDetails.didChangeContentEvent += SelectCharacteristicAndDifficulty;
            }
            else
            {
                selectThisCharacteristic = null;
                selectThisDifficulty = BeatmapDifficulty.Easy;
                beatmapCheck = null;
            }

            LevelCollectionTableView levelCollectionTable = Resources.FindObjectsOfTypeAll<LevelCollectionTableView>().First();
            levelCollectionTable.SelectLevel(installedMap);
        }

        private static void SelectCharacteristicAndDifficulty(StandardLevelDetailViewController sLDVC, StandardLevelDetailViewController.ContentType cT)
        {
            if (sLDVC?.beatmapLevel is null)
                return;
            if (sLDVC.beatmapLevel.levelID != beatmapCheck?.levelID || cT != StandardLevelDetailViewController.ContentType.OwnedAndReady)
                return;
            sLDVC.didChangeContentEvent -= SelectCharacteristicAndDifficulty;
            StandardLevelDetailView sLDV = sLDVC._standardLevelDetailView;
            if (selectThisCharacteristic is null)
                return;

            Plugin.Log.Debug(selectThisCharacteristic.serializedName);
            Plugin.Log.Debug(selectThisDifficulty.ToString());

            BeatmapCharacteristicSegmentedControlController bCSCC = sLDV._beatmapCharacteristicSegmentedControlController;
            IconSegmentedControl iSCC = bCSCC._segmentedControl;
            int characteristicIndex = bCSCC._beatmapCharacteristics.IndexOf(selectThisCharacteristic);
            if (characteristicIndex != -1)
            {
                iSCC.SelectCellWithNumber(characteristicIndex);
                bCSCC.HandleDifficultySegmentedControlDidSelectCell(iSCC, characteristicIndex);
                BeatmapDifficultySegmentedControlController bDSCC = sLDV._beatmapDifficultySegmentedControlController;
                List<BeatmapDifficulty> difficulties = bDSCC._difficulties;
                int difficultyIndex = difficulties.IndexOf(selectThisDifficulty);
                if (difficultyIndex != -1)
                {
                    bDSCC._difficultySegmentedControl.SelectCellWithNumber(difficultyIndex);
                    bDSCC.HandleDifficultySegmentedControlDidSelectCell(iSCC, difficultyIndex);
                }
            }
        }
    }
}
