using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatSaverSharp;
using BeatSaverSharp.Models;
using RandomSongPlayer.Configuration;

namespace RandomSongPlayer
{
    internal static class MapInstaller
    {
        internal static async Task<(bool,string)> InstallMap(Beatmap mapData)
        {
            foreach (var level in Plugin.RandomSongsFolder.Levels)
            {
                if (mapData.LatestVersion.Hash.ToLower() == SongCore.Collections.hashForLevelID(level.Value.levelID).ToLower())
                {
                    Plugin.Log.Debug("Skipping download of map " + mapData.LatestVersion.Key + " since we already have it");
                    return (false, level.Key);
                }
            }


            byte[] zipData = await DownloadMap(mapData);
            if (!(zipData is null))
            {
                string mapPath = GetAndCreateMapDirectory(mapData);
                if (await ExtractZip(zipData, mapPath))
                {
                    return (true, mapPath);
                }
            }
            return (false, null);
        }

        private static async Task<byte[]> DownloadMap(Beatmap mapData)
        {
            try
            {
                byte[] zipData = await mapData.LatestVersion.DownloadZIP();
                return zipData;
            }
            catch (Exception ex)
            {
                Plugin.Log.Critical("Unable to download map zip: " + ex.ToString());
                return null;
            }

        }

        private static string GetAndCreateMapDirectory(Beatmap mapData)
        {
            string basePath = mapData.ID + " (" + mapData.Metadata.SongName + " - " + mapData.Metadata.LevelAuthorName + ")";
            basePath = string.Join("", basePath.Split((Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).ToArray())));
            string path = Path.Combine(PluginConfig.Instance.SongFolderPath, basePath);
            if (Directory.Exists(path))
            {
                int pathNum = 1;
                while (Directory.Exists(path + $" ({pathNum})")) ++pathNum;
                path += $" ({pathNum})";
            }
            Directory.CreateDirectory(path);
            return path;
        }

        private static async Task<bool> ExtractZip(byte[] zipData, string mapPath)
        {
            Stream zipStream = new MemoryStream(zipData);
            try
            {
                ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

                await Task.Run(() =>
                {
                    foreach (var entry in archive.Entries)
                    {
                        var fullName = entry.FullName;
                        bool invalidFileName = false;
                        foreach (char c in Path.GetInvalidFileNameChars())
                            if (fullName.Contains(c))
                                invalidFileName = true;
                        if (invalidFileName) 
                            continue;

                        var entryPath = Path.Combine(mapPath, entry.Name);
                        
                        if (!File.Exists(entryPath))
                            entry.ExtractToFile(entryPath);
                    }
                }).ConfigureAwait(false);
                archive.Dispose();
                zipStream.Close();
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log.Critical("Unable to extract map zip: " + ex.ToString());
                zipStream.Close();
                return false;
            }
        }
    }
}
