using Newtonsoft.Json;
using SimpleJSON;
using System;
using System.IO;

namespace RandomSongPlayer.Filter
{
    class FilterHelper
    {
        private const string FILTER_CONFIG = "/UserData/RandomSongFilter.json";
        private const string DEFAULT_FILTER =
@"{
    
}";

        public static JSONNode filters;
        private static bool ownSave = false; // prevent reload after we have saved ourselves

        public static void Save()
        {
            string filterpath = Environment.CurrentDirectory + FILTER_CONFIG;
            ownSave = true;
            File.WriteAllText(filterpath, filters.ToString(2));
        }

        public static event Action OnFiltersReload;

        public static void Load()
        {
            Logger.log.Info("reloading RSP filters");
            string filterpath = Environment.CurrentDirectory + FILTER_CONFIG;
            string filter;
            if (!File.Exists(filterpath))
            {
                File.WriteAllText(filterpath, DEFAULT_FILTER);
                filter = DEFAULT_FILTER;
            }
            else filter = File.ReadAllText(filterpath);
            filters = JSON.Parse(filter);

            OnFiltersReload?.Invoke();
        }

        private static void FileChanged(object source, FileSystemEventArgs e) {
            if (!ownSave)
                Load();
            ownSave = false;
        }

        public static void Setup()
        {
            Load();

            string filterpath = Environment.CurrentDirectory + FILTER_CONFIG;
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = Path.GetDirectoryName(filterpath);
            watcher.Filter = Path.GetFileName(filterpath);
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Changed += FileChanged;
            watcher.EnableRaisingEvents = true;
        }

        public static string GetFilteringString()
        {
            return filters.ToString();
        }
    }
}
