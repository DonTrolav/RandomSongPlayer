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
        private static string lastRequest = string.Empty;
        internal static bool haveFullList = false;
        internal static FilterResponse initialCache = null;
        internal static List<FilteredMap> cacheList = null;
        private static readonly Random rnjesus = new Random();

        internal static async Task<(Beatmap, string)> GenerateRandomMap()
        {
            Plugin.Log.Info("Searching for random beatmap");

            // Fill Cache
            #region FillCache
            string newRequest = FilterHelper.GetActiveFilteringString();
            Plugin.Log.Debug("Filter: " + newRequest);
            if (!HasElementsLeft(cacheList) && lastRequest == newRequest && haveFullList)
            {
                cacheList = initialCache.maps.ToList();
            }
            else if (newRequest != lastRequest || !haveFullList)
            {
                FilterResponse tempResponse = null;
                if (DoINeedTheServer(FilterHelper.CurrentFilterSet))
                {
                    tempResponse = await FillCacheFromServer(newRequest);
                }
                if (tempResponse is null)
                {
                    haveFullList = true;
                    initialCache = FillCacheFromLocal(FilterHelper.CurrentFilterSet);
                }
                else
                {
                    initialCache = tempResponse;
                    haveFullList = (initialCache.count == initialCache.maps.Count);
                }
                cacheList = initialCache.maps.ToList();
            }
            #endregion
            lastRequest = newRequest;

            // Find map
            #region FindMap
            Beatmap mapData = null;
            string charDiff = null;
            if (initialCache.count == 0)
            {
                Plugin.Log.Info("Could not find a map with these filters!");
            }
            else
            {
                while (mapData == null && cacheList.Count > 0)
                {
                    int index = rnjesus.Next(cacheList.Count);
                    FilteredMap chosenMap = cacheList[index];
                    string randomKey = chosenMap.key;

                    List<string> diffsToChooseFrom = chosenMap.diffs;
                    int diffCount = diffsToChooseFrom.Count();
                    if (diffCount > 0) 
                        charDiff = diffsToChooseFrom[rnjesus.Next(diffCount)];

                    cacheList.RemoveAt(index);

                    mapData = await UpdateMapData(randomKey);
                }
                if (cacheList.Count == 0 && mapData is null)
                {
                    Plugin.Log.Info("Could not find a valid map!");
                }
            }
            #endregion

            return (mapData, charDiff);
        }

        private static bool DoINeedTheServer(JSONNode currentFilterSet)
        {
            foreach (var x in currentFilterSet)
            {
                if (!FilterHelper.NATIVE_FILTERS.Contains(x.Key))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool HasElementsLeft(List<FilteredMap> cache)
        {
            if (cache is null)
                return false;

            if (cache.Count == 0) 
                return false;
                
            return true;
        }

        private static FilterResponse FillCacheFromLocal(JSONNode currentFilterSet)
        {
            FilterResponse response;
            var sw = Stopwatch.StartNew();
            IEnumerable<Song> songsToSearch = Plugin.SongDetails.songs;
            Plugin.Log.Info("Original song count: " + songsToSearch.Count().ToString());

            LocalMapFilter songFilter = new LocalMapFilter(currentFilterSet);
            IEnumerable<SongDifficulty> difficultiesToSearch = songsToSearch.Where(x => songFilter.CheckFilter(x)).SelectMany(x => x.difficulties);
           
            if (difficultiesToSearch.Count() == 0)
            {
                 response = new FilterResponse
                 {
                     count = 0,
                     maps = new List<FilteredMap>()
                 };
            }
            else
            {
                Plugin.Log.Debug("Original diff count: " + difficultiesToSearch.Count().ToString());
                LocalDiffFilter difficultyFilter = new LocalDiffFilter(currentFilterSet);

                IEnumerable<IGrouping<Song, SongDifficulty>> foundSongs = difficultiesToSearch.Where(x => difficultyFilter.CheckFilter(x)).GroupBy(x => x.song);
                Plugin.Log.Info($"Found songs: {foundSongs.Count()} (in {sw.ElapsedMilliseconds}ms)");

                response = new FilterResponse
                {
                    count = difficultiesToSearch.Count(),
                    maps = new List<FilteredMap>()
                };

                foreach (var group in foundSongs)
                {
                    var tempMap = new FilteredMap
                    {
                        hash = group.Key.hash,
                        key = group.Key.key,
                        diffs = group.Select(x => CharacteristicToLowerString(x.characteristic) + x.difficulty.ToString().ToLower()).ToList()
                    };
                    response.maps.Add(tempMap);
                }
            }
            return response;
        }

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

        private static async Task<FilterResponse> FillCacheFromServer(string filterString)
        {
            Plugin.Log.Info("Trying to get a random filtered key");

            try
            {
                //send the filter (yes, the whole file) to the server
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

        private static async Task<Beatmap> UpdateMapData(string randomKey)
        {
            try
            {
                Beatmap mapData = await Plugin.BeatsaverClient.Beatmap(randomKey);
                if (!(mapData is null)) { Plugin.Log.Info("Found map " + randomKey + ": " + mapData.Metadata.SongAuthorName + " - " + mapData.Metadata.SongName + " by " + mapData.Metadata.LevelAuthorName); }
                return mapData;
            }
            catch (HttpRequestException ex) { Plugin.Log.Info("Failed to download map with key '" + randomKey + "'. Map was most likely deleted: " + ex.Message); }
            catch (Exception ex) { Plugin.Log.Error("Error loading MapData: " + ex.Message); }
          
            return null;
        }
    }
}
