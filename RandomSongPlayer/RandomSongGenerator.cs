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
        private const string SERVER_CONFIG = "/UserData/RandomSongFilterServer.txt";
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
            Page<PagedRequestOptions> latestMaps = await Plugin.beatsaverClient.Latest();
            string latestKey = latestMaps.Docs[0].Key;
            int keyAsDecimal = int.Parse(latestKey, System.Globalization.NumberStyles.HexNumber);

            // Randomize the key and download the map
            while (tries < maxTries && mapData == null)
            {
                string randomKey = await GetFilteredRandomKey();
                if (randomKey == null)
                {
                    int randomNumber = Plugin.rnd.Next(0, keyAsDecimal + 1);
                    randomKey = randomNumber.ToString("x");
                }

                await UpdateMapData(randomKey);
                tries++;
            }
            return mapData;
        }

        private static async Task<String> GetFilteredRandomKey()
        {
            // The filtering server is not public yet, so disable the filtering as long as the server address isn't provided
            string serverconfigfolder = Environment.CurrentDirectory + SERVER_CONFIG;
            if (!File.Exists(serverconfigfolder))
                return null;


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
                char[] serverconfigtrim = { '\n', ' ', '\t' };
                string api_accesspoint = File.ReadAllText(serverconfigfolder).Trim(serverconfigtrim);
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
                if (!(mapData is null)) { Logger.log.Info("Found map " + randomKey + ": " + mapData.Metadata.SongAuthorName + " - " + mapData.Metadata.SongName + " by " + mapData.Metadata.LevelAuthorName); }
                return mapData;
            }
            catch (System.Net.Http.HttpRequestException ex) { Logger.log.Info("Failed to download map with key '" + randomKey + "'. Map was most likely deleted: " + ex.Message); }
            catch (Exception ex) { Logger.log.Error("Error loading MapData: " + ex.Message); }
          
            return null;
        }
    }
}
