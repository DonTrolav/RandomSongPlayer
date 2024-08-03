using SimpleJSON;
using SongDetailsCache.Structs;
using System.Collections.Generic;
using System.Linq;

namespace RandomSongPlayer.Filter
{
    internal class LocalDiffFilter
    {
        private readonly bool searchInCharacteristicEnabled;
        private readonly IEnumerable<MapCharacteristic> searchInCharacteristic;

        private readonly bool searchInDifficultyEnabled;
        private readonly IEnumerable<MapDifficulty> searchInDifficulty;

        private readonly bool hasModsEnabled;
        private readonly IEnumerable<MapMods> hasMods;

        private readonly bool minNotesEnabled;
        private readonly uint minNotes;
        private readonly bool maxNotesEnabled;
        private readonly uint maxNotes;

        private readonly bool minNPSEnabled;
        private readonly float minNPS;
        private readonly bool maxNPSEnabled;
        private readonly float maxNPS;

        private readonly bool minBombsEnabled;
        private readonly uint minBombs;
        private readonly bool maxBombsEnabled;
        private readonly uint maxBombs;

        private readonly bool minBPSEnabled;
        private readonly float minBPS;
        private readonly bool maxBPSEnabled;
        private readonly float maxBPS;

        private readonly bool minObstaclesEnabled;
        private readonly uint minObstacles;
        private readonly bool maxObstaclesEnabled;
        private readonly uint maxObstacles;

        private readonly bool minOPSEnabled;
        private readonly float minOPS;
        private readonly bool maxOPSEnabled;
        private readonly float maxOPS;

        private readonly bool minNJSEnabled;
        private readonly float minNJS;
        private readonly bool maxNJSEnabled;
        private readonly float maxNJS;

        private readonly bool minStarsScoreSaberEnabled;
        private readonly float minStarsScoreSaber;
        private readonly bool maxStarsScoreSaberEnabled;
        private readonly float maxStarsScoreSaber;

        private readonly bool minStarsBeatLeaderEnabled;
        private readonly float minStarsBeatLeader;
        private readonly bool maxStarsBeatLeaderEnabled;
        private readonly float maxStarsBeatLeader;

        public LocalDiffFilter(JSONNode filterSet)
        {
            searchInCharacteristicEnabled = filterSet["diffIsCharacteristic"] != null;
            if (searchInCharacteristicEnabled) searchInCharacteristic = filterSet["diffIsCharacteristic"].AsArray.Children.Select(x => ParseCharacteristic(x.Value));
            searchInDifficultyEnabled = filterSet["diffIsDifficulty"] != null;
            if (searchInDifficultyEnabled) searchInDifficulty = filterSet["diffIsDifficulty"].AsArray.Children.Select(x => ParseDifficulty(x.Value));
            hasModsEnabled = filterSet["diffHasMods"] != null;
            if (hasModsEnabled) hasMods = filterSet["diffHasMods"].AsArray.Children.Select(x => ParseMod(x.Value));
            minNotesEnabled = filterSet["diffMinNotes"] != null && uint.TryParse(filterSet["diffMinNotes"], out minNotes);
            maxNotesEnabled = filterSet["diffMaxNotes"] != null && uint.TryParse(filterSet["diffMaxNotes"], out maxNotes);
            minNPSEnabled = filterSet["minNdiffMinNPSPS"] != null && float.TryParse(filterSet["diffMinNPS"], out minNPS);
            maxNPSEnabled = filterSet["diffMaxNPS"] != null && float.TryParse(filterSet["diffMaxNPS"], out maxNPS);
            minBombsEnabled = filterSet["diffMinBombs"] != null && uint.TryParse(filterSet["diffMinBombs"], out minBombs);
            maxBombsEnabled = filterSet["diffMaxBombs"] != null && uint.TryParse(filterSet["diffMaxBombs"], out maxBombs);
            minBPSEnabled = filterSet["diffMinBPS"] != null && float.TryParse(filterSet["diffMinBPS"], out minBPS);
            maxBPSEnabled = filterSet["diffMaxBPS"] != null && float.TryParse(filterSet["diffMaxBPS"], out maxBPS);
            minObstaclesEnabled = filterSet["diffMinObstacles"] != null && uint.TryParse(filterSet["diffMinObstacles"], out minObstacles);
            maxObstaclesEnabled = filterSet["diffMaxObstacles"] != null && uint.TryParse(filterSet["diffMaxObstacles"], out maxObstacles);
            minOPSEnabled = filterSet["diffMinOPS"] != null && float.TryParse(filterSet["diffMinOPS"], out minOPS);
            maxOPSEnabled = filterSet["diffMaxOPS"] != null && float.TryParse(filterSet["diffMaxOPS"], out maxOPS);
            minNJSEnabled = filterSet["diffMinNJS"] != null && float.TryParse(filterSet["diffMinNJS"], out minNJS);
            maxNJSEnabled = filterSet["diffMaxNJS"] != null && float.TryParse(filterSet["diffMaxNJS"], out maxNJS);
            minStarsScoreSaberEnabled = filterSet["diffMinStarsScoreSaber"] != null && float.TryParse(filterSet["diffMinStarsScoreSaber"], out minStarsScoreSaber);
            maxStarsScoreSaberEnabled = filterSet["diffMaxStarsScoreSaber"] != null && float.TryParse(filterSet["diffMaxStarsScoreSaber"], out maxStarsScoreSaber);
            minStarsBeatLeaderEnabled = filterSet["diffMinStarsBeatLeader"] != null && float.TryParse(filterSet["diffMinStarsBeatLeader"], out minStarsBeatLeader);
            maxStarsBeatLeaderEnabled = filterSet["diffMaxStarsBeatLeader"] != null && float.TryParse(filterSet["diffMaxStarsBeatLeader"], out maxStarsBeatLeader);
        }

        private MapCharacteristic ParseCharacteristic(string characteristic)
        {
            switch (characteristic)
            {
                case "Standard": return MapCharacteristic.Standard;
                case "Lawless": return MapCharacteristic.Lawless;
                case "OneSaber": return MapCharacteristic.OneSaber;
                case "Lightshow": return MapCharacteristic.Lightshow;
                case "NoArrows": return MapCharacteristic.NoArrows;
                case "90Degree": return MapCharacteristic.NinetyDegree;
                case "360Degree": return MapCharacteristic.ThreeSixtyDegree;
                default:
                    Plugin.Log.Warn("Could not parse characteristic: " + characteristic);
                    return MapCharacteristic.Standard; // should not happen
            }
        }

        private MapDifficulty ParseDifficulty(string difficulty)
        {
            switch (difficulty)
            {
                case "easy": return MapDifficulty.Easy;
                case "normal": return MapDifficulty.Normal;
                case "hard": return MapDifficulty.Hard;
                case "expert": return MapDifficulty.Expert;
                case "expertplus": return MapDifficulty.ExpertPlus;
                default:
                    Plugin.Log.Warn("Could not parse difficulty: " + difficulty);
                    return MapDifficulty.ExpertPlus; // should not happen
            }
        }

        private MapMods ParseMod(string mod)
        {
            switch (mod)
            {
                case "noodleextensions": return MapMods.NoodleExtensions;
                case "mappingextensions": return MapMods.MappingExtensions;
                case "chroma": return MapMods.Chroma;
                case "cinema": return MapMods.Cinema;
                default:
                    Plugin.Log.Warn("Could not parse mod: " + mod);
                    return MapMods.MappingExtensions; // should not happen
            }
        }

        internal bool CheckFilter(SongDifficulty difficulty)
        {
            if (searchInCharacteristicEnabled && !searchInCharacteristic.Contains(difficulty.characteristic)) return false;
            if (searchInDifficultyEnabled && !searchInDifficulty.Contains(difficulty.difficulty)) return false;
            if (hasModsEnabled && hasMods.Any(x => !difficulty.mods.HasFlag(x))) return false;
            if (minNotesEnabled && difficulty.notes < minNotes) return false;
            if (maxNotesEnabled && difficulty.notes > maxNotes) return false;
            if (minNPSEnabled && (difficulty.song.songDurationSeconds == 0 || difficulty.notes < minNPS * difficulty.song.songDurationSeconds)) return false;
            if (maxNPSEnabled && (difficulty.song.songDurationSeconds == 0 || difficulty.notes > maxNPS * difficulty.song.songDurationSeconds)) return false;
            if (minBombsEnabled && difficulty.bombs < minBombs) return false;
            if (maxBombsEnabled && difficulty.bombs > maxBombs) return false;
            if (minBPSEnabled && (difficulty.song.songDurationSeconds == 0 || difficulty.bombs < minBPS * difficulty.song.songDurationSeconds)) return false;
            if (maxBPSEnabled && (difficulty.song.songDurationSeconds == 0 || difficulty.bombs > maxBPS * difficulty.song.songDurationSeconds)) return false;
            if (minObstaclesEnabled && difficulty.obstacles < minObstacles) return false;
            if (maxObstaclesEnabled && difficulty.obstacles > maxObstacles) return false;
            if (minOPSEnabled && (difficulty.song.songDurationSeconds == 0 || difficulty.obstacles < minOPS * difficulty.song.songDurationSeconds)) return false;
            if (maxOPSEnabled && (difficulty.song.songDurationSeconds == 0 || difficulty.obstacles > maxOPS * difficulty.song.songDurationSeconds)) return false;
            if (minNJSEnabled && difficulty.njs < minNJS) return false;
            if (maxNJSEnabled && difficulty.njs > maxNJS) return false;
            if (minStarsScoreSaberEnabled && (!difficulty.song.rankedStates.HasFlag(RankedStates.ScoresaberRanked) || difficulty.stars < minStarsScoreSaber)) return false;
            if (maxStarsScoreSaberEnabled && (!difficulty.song.rankedStates.HasFlag(RankedStates.ScoresaberRanked) || difficulty.stars > maxStarsScoreSaber)) return false;
            if (minStarsBeatLeaderEnabled && (!difficulty.song.rankedStates.HasFlag(RankedStates.BeatleaderRanked) || difficulty.starsBeatleader < minStarsBeatLeader)) return false;
            if (maxStarsBeatLeaderEnabled && (!difficulty.song.rankedStates.HasFlag(RankedStates.BeatleaderRanked) || difficulty.starsBeatleader > maxStarsBeatLeader)) return false;
            return true;
        }
    }
}
