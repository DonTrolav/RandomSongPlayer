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

namespace RandomSongPlayer
{
    internal static class MapInstaller
    {
        internal static async Task<(bool,string)> InstallMap(Beatmap mapData)
        {
            // Don't download if we already have the map in our playlist
            foreach (var level in Plugin.randomSongsFolder.Levels)
            {
                if (mapData.Hash.ToLower() == SongCore.Collections.hashForLevelID(level.Value.levelID).ToLower())
                {
                    Logger.log.Info("Skipping download of map " + mapData.Key + " since we already have it");
                    return (false, level.Key);
                }
            }


            byte[] zipData = await DownloadMap(mapData);
            if (!(zipData is null))
            {
                string mapPath = GetAndCreateMapDirectory(mapData);
                if (await ExtractZip(mapData, zipData, mapPath))
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
                byte[] zipData = await mapData.DownloadZip();
                return zipData;
            }
            catch (Exception ex)
            {
                Logger.log.Critical("Unable to download map zip: " + ex.ToString());
                return null;
            }
            
        }

        private static string GetAndCreateMapDirectory(Beatmap mapData)
        {
            // consistent with beatsaver downloader so I guess think twice before changing anything here
            string basePath = mapData.Key + " (" + mapData.Metadata.SongName + " - " + mapData.Metadata.LevelAuthorName + ")";
            basePath = string.Join("", basePath.Split((Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).ToArray())));
            string path = Setup.RandomSongsFolder + "/" + basePath;
            if (Directory.Exists(path))
            {
                int pathNum = 1;
                while (Directory.Exists(path + $" ({pathNum})")) ++pathNum;
                path += $" ({pathNum})";
            }
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        private static void UnzipFile(string fileName)
        {
            ZipFile.ExtractToDirectory(fileName + ".zip", fileName);
            File.Delete(fileName + ".zip");
        }

        private static async Task<bool> ExtractZip(Beatmap beatmap, byte[] zipData, string mapPath)
        {
            Stream zipStream = new MemoryStream(zipData);
            try
            {
                ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

                await Task.Run(() =>
                {
                    foreach (var entry in archive.Entries)
                    {
                        // ignore anything in subfolder or otherwise invalid
                        var fullName = entry.FullName;
                        bool invalidFileName = false;
                        foreach (char c in Path.GetInvalidFileNameChars())
                            if (fullName.Contains(c))
                                invalidFileName = true;
                        if (invalidFileName) 
                            continue;

                        var entryPath = Path.Combine(mapPath, entry.Name); // Name instead of FullName for better security and because song zips don't have nested directories anyway
                        
                        if (!File.Exists(entryPath)) // Either we're overwriting or there's no existing file
                            entry.ExtractToFile(entryPath);
                    }
                }).ConfigureAwait(false);
                archive.Dispose();
                zipStream.Close();
                return true;
            }
            catch (Exception ex)
            {
                Logger.log.Critical("Unable to extract map zip: " + ex.ToString());
                zipStream.Close();
                return false;
            }
        }
    }
}
