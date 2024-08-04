using System;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomSongPlayer.Filter;
using BeatSaverSharp.Models;
using SongDetailsCache.Structs;
using Newtonsoft.Json;
using SimpleJSON;
using System.Diagnostics;
using RandomSongPlayer.Configuration;

namespace RandomSongPlayer
{
    internal static class RandomSongGenerator
    {
        internal static bool AllMapsInCache { get; private set; } = false;
        internal static FilterResponse InitialCache { get; private set; } = null;
        internal static List<FilteredMap> CurrentCache { get; private set; } = null;

        private static string lastRequest = string.Empty;
        private static readonly Random rnjesus = new Random();

        internal static async Task<(Beatmap, string)> GenerateRandomMap()
        {
            Plugin.Log.Info("Searching for random beatmap");
            string newRequest = FilterHelper.GetActiveFilteringString();

            await FillCache(newRequest);
            lastRequest = newRequest;

            return await FindMap();
        }


        private static bool FilterServerIsNeeded(JSONNode currentFilterSet)
        {
            foreach(var x in currentFilterSet)
            {
                if (!FilterHelper.NATIVE_FILTERS.Contains(x.Key))
                {
                    return true;
                }
            }
            return false;
        }

        #region FillCache
        private static async Task FillCache(string newRequest)
        {
            Plugin.Log.Debug("Old Filter: " + lastRequest);
            Plugin.Log.Debug("New Filter: " + newRequest);
            if (newRequest != lastRequest || !AllMapsInCache)
            {
                InitialCache = null;
                CurrentCache = null;
                if (FilterServerIsNeeded(FilterHelper.CurrentFilterSet))
                {
                    // If the server is needed, fill cache from Server
                    InitialCache = await FillCacheFromServer(newRequest);
                }

                if (InitialCache is null)
                {
                    // If the server is not needed or doesn't respond, use local filtering
                    InitialCache = FillCacheFromLocal(FilterHelper.CurrentFilterSet);
                }

                AllMapsInCache = (InitialCache.count == InitialCache.maps.Count);
            }

            if (CurrentCache is null || CurrentCache.Count == 0)
            {
                CurrentCache = InitialCache.maps.ToList();
            }
        }

        private static FilterResponse FillCacheFromLocal(JSONNode currentFilterSet)
        {
            FilterResponse response = new FilterResponse
            {
                count = 0,
                maps = new List<FilteredMap>()
            };

            var sw = Stopwatch.StartNew();
            IEnumerable<Song> songsToSearch = Plugin.SongDetails.songs;
            Plugin.Log.Info("Original song count: " + songsToSearch.Count().ToString());

            LocalMapFilter songFilter = new LocalMapFilter(currentFilterSet);
            IEnumerable<SongDifficulty> difficultiesToSearch = songsToSearch.Where(x => songFilter.CheckFilter(x)).SelectMany(x => x.difficulties);
           
            if (difficultiesToSearch.Count() == 0)
                return response;

            Plugin.Log.Debug("Original diff count: " + difficultiesToSearch.Count().ToString());
            LocalDiffFilter difficultyFilter = new LocalDiffFilter(currentFilterSet);

            IEnumerable<IGrouping<Song, SongDifficulty>> foundSongs = difficultiesToSearch.Where(x => difficultyFilter.CheckFilter(x)).GroupBy(x => x.song);
            Plugin.Log.Info($"Found songs: {foundSongs.Count()} (in {sw.ElapsedMilliseconds}ms)");

            response.count = difficultiesToSearch.Count();
            foreach (var group in foundSongs)
            {
                response.maps.Add(new FilteredMap
                {
                    hash = group.Key.hash,
                    key = group.Key.key,
                    diffs = group.Select(x => CharacteristicToLowerString(x.characteristic) + x.difficulty.ToString().ToLower()).ToList()
                });
            }
            return response;
        }

        private static async Task<FilterResponse> FillCacheFromServer(string filterString)
        {
            Plugin.Log.Info("Trying to get a random filtered key");

            try
            {
                var content = new StringContent(filterString, Encoding.UTF8, "application/json");
                var response = await Plugin.HttpClient.PostAsync(PluginConfig.Instance.FilterServerAddress, content);
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    Plugin.Log.Warn("Current RSP filter set is invalid!");
                    return null;
                } 
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    Plugin.Log.Info("Error occured while contacting the filter server.");
                    return null;
                }

                string responseString = await response.Content.ReadAsStringAsync();

                var filteredResponse = JsonConvert.DeserializeObject<FilterResponse>(responseString);
                return filteredResponse;
            }
            catch (TaskCanceledException)
            {
                Plugin.Log.Info("Request to filtering server timed out.");
                return null;
            }
            catch (HttpRequestException)
            {
                Plugin.Log.Info("Could not reach the filter server.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
        #endregion

        #region FindMap
        private static async Task<(Beatmap, string)> FindMap()
        {
            if (InitialCache.count == 0)
            {
                Plugin.Log.Info("Could not find a map with these filters!");
                return (null, null);
            }

            while (CurrentCache.Count > 0)
            {
                int index = rnjesus.Next(CurrentCache.Count);
                FilteredMap chosenMap = CurrentCache[index];
                string randomKey = chosenMap.key;
                string charDiff = string.Empty;

                List<string> diffsToChooseFrom = chosenMap.diffs;
                int diffCount = diffsToChooseFrom.Count();
                if (diffCount > 0)
                    charDiff = diffsToChooseFrom[rnjesus.Next(diffCount)];

                CurrentCache.RemoveAt(index);

                Beatmap mapData = await UpdateMapData(randomKey);
                if (!(mapData is null) && charDiff.Length > 0)
                    return (mapData, charDiff);
            }

            Plugin.Log.Info("Could not find a valid map!");
            return (null, null);
        }

        private static async Task<Beatmap> UpdateMapData(string randomKey)
        {
            try
            {
                Beatmap mapData = await Plugin.BeatsaverClient.Beatmap(randomKey);
                if (!(mapData is null))
                    Plugin.Log.Info("Found map " + randomKey + ": " + mapData.Metadata.SongAuthorName + " - " + mapData.Metadata.SongName + " by " + mapData.Metadata.LevelAuthorName);
                return mapData;
            }
            catch (HttpRequestException ex)
            {
                Plugin.Log.Info("Failed to download map with key '" + randomKey + "'. Map was most likely deleted: " + ex.Message);
            }
            catch (Exception ex)
            {
                Plugin.Log.Error("Error loading MapData: " + ex.Message);
            }

            return null;
        }
        #endregion


        private static string CharacteristicToLowerString(MapCharacteristic characteristic)
        {
            switch (characteristic)
            {
                case MapCharacteristic.Standard:
                    return "standard";
                case MapCharacteristic.OneSaber:
                    return "onesaber";
                case MapCharacteristic.NoArrows:
                    return "noarrows";
                case MapCharacteristic.NinetyDegree:
                    return "90degree";
                case MapCharacteristic.ThreeSixtyDegree:
                    return "360degree";
                case MapCharacteristic.Lightshow:
                    return "lightshow";
                case MapCharacteristic.Lawless:
                    return "lawless";
                case MapCharacteristic.Custom:
                default:
                    return "custom";
            }
        }
    }
}
