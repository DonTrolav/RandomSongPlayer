using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomSongPlayer.Filter;
using Newtonsoft.Json;
using BeatSaverSharp;

namespace RandomSongPlayer
{
    internal static class RandomSongGenerator
    {
        private const string FILTER_CONFIG = "/UserData/RandomSongFilter.json";
        private const string DEFAULT_FILTER =
@"{
    
}";

        internal static async Task<Beatmap> GenerateRandomKey()
        {
            int tries = 0;
            int maxTries = 20;
            Beatmap mapData = null;

            Logger.log.Info("Searching for random beatmap");

            // Look for the latest key on the Beatsaver API
            Page latestMaps = await Plugin.beatsaverClient.Latest();
            string latestKey = latestMaps.Docs[0].Key;
            int keyAsDecimal = int.Parse(latestKey, System.Globalization.NumberStyles.HexNumber);

            // Randomize the key and download the map
            bool nofilterresults = false;
            while (tries < maxTries && mapData == null)
            {
                string randomKey = null;
                if(!nofilterresults)
                    randomKey = await GetFilteredRandomKey();
                if (randomKey == null)
                {
                    // make sure we don't ask the server again if we weren't able to get a random key right now anyways
                    nofilterresults = true;
                    int randomNumber = Plugin.rnd.Next(0, keyAsDecimal + 1);
                    randomKey = randomNumber.ToString("x");
                }

                mapData = await UpdateMapData(randomKey);
                tries++;
            }
            return mapData;
        }

        private static async Task<String> GetFilteredRandomKey()
        {
            Logger.log.Info("Trying to get a random filtered key");

            // Load filter, do this each time so changes to the filter file will be used immediatly
            string filterpath = Environment.CurrentDirectory + FILTER_CONFIG;
            string filter;
            if (!File.Exists(filterpath))
            {
                File.WriteAllText(filterpath, DEFAULT_FILTER);
                filter = DEFAULT_FILTER;
            }
            else filter = File.ReadAllText(filterpath);

            try
            {
                //send the filter (yes, the whole file) to the server
                var content = new StringContent(filter, Encoding.UTF8, "application/json");
                string api_accesspoint = "https://rsp.bs.qwasyx3000.com/random_map";
                var response = await Plugin.client.PostAsync(api_accesspoint, content);
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    Logger.log.Warn("Random Song filters invalid; falling back to purely random songs");
                    return null;
                } 
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    Logger.log.Info("Some other error occured while contacting the filter server; falling back to purely random songs");
                    return null;
                }

                string responseString = await response.Content.ReadAsStringAsync();

                var filteredResponse = JsonConvert.DeserializeObject<FilterServerResponse>(responseString);
                if (filteredResponse.count == 0)
                {
                    Logger.log.Info("No songs with the provided filters found; falling back to purely random songs");
                    return null;
                }
                return filteredResponse.key;
            }
            catch (TaskCanceledException)
            {
                Logger.log.Info("Request to filtering server timed out; falling back to purely random songs");
                return null;
            }
            catch (HttpRequestException)
            {
                Logger.log.Info("Most likely could not reach the filter server; falling back to purely random songs");
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
                Beatmap mapData = await Plugin.beatsaverClient.Key(randomKey);
                if (!(mapData is null))
                {
                    Logger.log.Info("Found map " + randomKey + ": " + mapData.Metadata.SongAuthorName + " - " + mapData.Metadata.LevelAuthorName + " by " + mapData.Metadata.LevelAuthorName);
                }
                return mapData;
            }
            catch (HttpRequestException)
            {
                Logger.log.Info("Failed to download map with key '" + randomKey + "'. Map was most likely deleted.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }
    }
}
