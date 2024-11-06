using IPA.Logging;
using RandomSongPlayer.Configuration;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RandomSongPlayer.Filter
{
    public static class FilterHelper
    {
        #region Constants
        private const string DEFAULT_FILTER = "{\n  \n}";
        private const string FILTER_EXTENSION = ".rspf";
        private const string DEFAULT_FILE = "default";
        #endregion

        #region Statics
        public static readonly string[] NATIVE_FILTERS =
        {
            "mapMinKey", "mapMaxKey",
            "mapMinRating", "mapMaxRating",
            "mapMinBPM", "mapMaxBPM",
            "mapMinDuration", "mapMaxDuration",
            "mapMinVotes", "mapMaxVotes",
            "mapMinUpvotes", "mapMaxUpvotes",
            "mapMinDownvotes", "mapMaxDownvotes",
            "mapMinDownloads", "mapMaxDownloads",
            "mapMinUDP", "mapMaxUDP",
            "diffIsCharacteristic",
            "diffIsDifficulty",
            "diffHasMods",
            "diffMinNotes", "diffMaxNotes",
            "diffMinNPS", "diffMaxNPS",
            "diffMinBombs", "diffMaxBombs",
            "diffMinBPS", "diffMaxBPS",
            "diffMinObstacles", "diffMaxObstacles",
            "diffMinOPS", "diffMaxOPS",
            "diffMinNJS", "diffMaxNJS",
            "diffMinStarsScoreSaber", "diffMaxStarsScoreSaber",
            "diffMinStarsBeatLeader", "diffMaxStarsBeatLeader"
        };
        private static Dictionary<string, JSONNode> filterSets;
        private static string currentFilterName;
        private static bool ownSave = false; // prevent reload after we have saved ourselves
        public static JSONNode CurrentFilterSet { get { return filterSets[currentFilterName]; } }
        public static string CurrentFilterSetName { get { return currentFilterName; } }
        private static FileSystemWatcher FileWatcher { get; set; }
        #endregion

        public static bool TrySetCurrentFilter(string filterName)
        {
            if (filterSets.ContainsKey(filterName))
            {
                Plugin.Log.Debug($"Changed filter set to {filterName}");
                currentFilterName = filterName;
                OnFilterSetsAltered?.Invoke();
                return true;
            }
            else
            {
                Plugin.Log.Warn($"Could not change filter set to {filterName}!");
                return false;
            }
        }

        

        #region Events
        public static event Action OnFilterSetsAltered;

        internal static List<object> GetFilterSetNames()
        {
            if (filterSets is null)
            {
                Plugin.Log.Debug("Filter Set List does not exist. This should not happen.");
                return new List<object>();
            }
            else
            {
                return new List<object>(filterSets.Keys.OrderBy(x => x));
            }
        }
        #endregion

        #region FileSaveLoad
        public static void SaveCurrent()
        {
            Save(currentFilterName);
        }

        public static void Save(string filterName)
        {
            string filterPath = Path.Combine(PluginConfig.Instance.FiltersPath, filterName + FILTER_EXTENSION);
            Directory.CreateDirectory(PluginConfig.Instance.FiltersPath);
            if (filterSets.TryGetValue(filterName, out JSONNode node))
            {
                ownSave = true;
                File.WriteAllText(filterPath, node.ToString(2));
            }
        }

        public static void Load(string filterPath)
        {
            if (!File.Exists(filterPath))
                return;

            string filterSet = Path.GetFileNameWithoutExtension(filterPath);
            string filterContent = File.ReadAllText(filterPath);

            filterSets.Remove(filterSet);
            try
            {
                JSONNode filter = JSON.Parse(filterContent);
                if (filter != null)
                {
                    filterSets[filterSet] = filter;
                }
                else
                {
                    Plugin.Log.Warn($"Invalid filter: {filterSet}");
                }
            }
            catch (Exception e)
            {
                Plugin.Log.Warn($"Could not parse filter: {filterSet}");
                Plugin.Log.Warn(e.Message);
            }
        }

        public static void LoadSetup()
        {
            filterSets = new Dictionary<string, JSONNode>();
            string[] filterFiles = Directory.GetFiles(PluginConfig.Instance.FiltersPath, "*" + FILTER_EXTENSION);
            foreach (string filePath in filterFiles)
            {
                Load(filePath);
            }

            if (filterSets.Count == 0) CreateDefaultIfEmpty();
            else currentFilterName = filterSets.Keys.First();
        }

        public static void NewOrSelect(string filterName)
        {
            if (filterSets.ContainsKey(filterName))
            {
                currentFilterName = filterName;
            }
            else
            {
                string filterPath = Path.Combine(PluginConfig.Instance.FiltersPath, filterName + FILTER_EXTENSION);
                File.WriteAllText(filterPath, DEFAULT_FILTER);
                currentFilterName = filterName;
            }
        }

        internal static void DeleteCurrent()
        {
            Delete(currentFilterName);
        }

        internal static void Delete(string filterName)
        {
            Plugin.Log.Debug($"Trying to delete filter {filterName}");

            string filterPath = Path.Combine(PluginConfig.Instance.FiltersPath, filterName + FILTER_EXTENSION);
            Plugin.Log.Debug($"Path to filter {filterPath}");

            if (File.Exists(filterPath))
                File.Delete(filterPath);
            else
                filterSets.Remove(filterName);
        }
        #endregion

        #region Setup
        public static void Enable()
        {
            Directory.CreateDirectory(PluginConfig.Instance.FiltersPath);
            LoadSetup();
            FileWatcher = new FileSystemWatcher
            {
                Path = PluginConfig.Instance.FiltersPath,
                Filter = "*" + FILTER_EXTENSION,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                IncludeSubdirectories = false,
                EnableRaisingEvents = true
            };
            FileWatcher.Changed += OnChanged;
            FileWatcher.Created += OnCreated;
            FileWatcher.Deleted += OnDeleted;
            FileWatcher.Renamed += OnRenamed;
            FileWatcher.Error += OnError;
        }

        public static void Disable()
        {
            FileWatcher.Dispose();
            FileWatcher = null;
            filterSets = null;
            currentFilterName = null;
        }

        private static void CreateDefaultIfEmpty()
        {
            if (filterSets.Count == 0)
            {
                filterSets[DEFAULT_FILE] = JSON.Parse(DEFAULT_FILTER);
                Save(DEFAULT_FILE);
                currentFilterName = DEFAULT_FILE;
            }
            else
            {
                if (!filterSets.ContainsKey(currentFilterName))
                {
                    currentFilterName = filterSets.Keys.First();
                }
            }
        }
        #endregion

        #region FilterStrings
        public static string GetActiveFilteringString()
        {
            return GetFilteringString(currentFilterName);
        }

        public static string GetFilteringString(string filterSet)
        {
            if (filterSets.TryGetValue(filterSet, out JSONNode node))
            {
                return node.ToString();
            }
            else
            {
                return string.Empty;
            }    
        }
        #endregion

        #region FilterEvents
        private static void OnError(object sender, ErrorEventArgs e)
        {
            Plugin.Log.Error("RSP FileSystemWatcher Error: " + e.ToString());
        }

        private static void OnRenamed(object sender, RenamedEventArgs e)
        {
            Plugin.Log.Debug("Old file path: " + e.OldFullPath);
            Plugin.Log.Debug("New file path: " + e.FullPath);
            string oldFileName = Path.GetFileNameWithoutExtension(e.OldFullPath);
            string oldFileExt = Path.GetExtension(e.OldFullPath);
            string newFileName = Path.GetFileNameWithoutExtension(e.FullPath);
            string newFileExt = Path.GetExtension(e.FullPath);

            if (oldFileExt == FILTER_EXTENSION && newFileExt == FILTER_EXTENSION)
            {
                if (filterSets.TryGetValue(oldFileName, out JSONNode node))
                {
                    filterSets.Remove(oldFileName);
                    filterSets[newFileName] = node;
                }
                else Load(e.FullPath);
            }
            else if (oldFileExt == FILTER_EXTENSION) filterSets.Remove(oldFileName);
            else if (newFileExt == FILTER_EXTENSION) Load(e.FullPath);
            OnFilterSetsAltered?.Invoke();
        }

        private static void OnDeleted(object sender, FileSystemEventArgs e)
        {
            Plugin.Log.Debug("Deleted file: " + e.FullPath);
            string delFileName = Path.GetFileNameWithoutExtension(e.FullPath);
            string delFileExt = Path.GetExtension(e.FullPath);

            if (delFileExt == FILTER_EXTENSION)
            {
                filterSets.Remove(delFileName);
            }
            CreateDefaultIfEmpty();
            OnFilterSetsAltered?.Invoke();
        }

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            Plugin.Log.Debug("Created file: " + e.FullPath);
            string creFileExt = Path.GetExtension(e.FullPath);

            if (creFileExt == FILTER_EXTENSION) Load(e.FullPath);
            OnFilterSetsAltered?.Invoke();
        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            Plugin.Log.Debug("Changed file: " + e.FullPath);
            string chaFileExt = Path.GetExtension(e.FullPath);

            if (chaFileExt == FILTER_EXTENSION)
            {
                if (!ownSave)
                    Load(e.FullPath);
                ownSave = false;
            }
            CreateDefaultIfEmpty();
            OnFilterSetsAltered?.Invoke();
        }
        #endregion
    }
}
