using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace RandomSongPlayer.UI
{
    public class FilterSettingsUI : NotifiableSingleton<FilterSettingsUI>
    {
        internal static readonly string EMPTY_FIELD = "";
        internal static readonly Color COLOR_RED = new Color(1, 0, 0, 1);
        internal static readonly Color COLOR_ORANGE = new Color(1, (float)0.625, 0, 1);
        internal static readonly Color COLOR_YELLOW = new Color(1, 1, 0, 1);
        internal static readonly Color COLOR_GREEN = new Color(0, 1, 0, 1);
        internal static readonly Color COLOR_WHITE = new Color(1, 1, 1, 1);

        [UIParams]
        private BSMLParserParams parserParams;

        public static void Init()
        {
            GameplaySetup.Instance.AddTab("RSP", "RandomSongPlayer.UI.FilterSettings.bsml", instance);
        }

        #region HelperFunctions
        private bool GetCharHelper(string name)
        {
            if (Filter.FilterHelper.CurrentFilterSet["diffIsCharacteristic"] == null)
                return true;

            foreach (var characteristic in Filter.FilterHelper.CurrentFilterSet["diffIsCharacteristic"].AsArray.Children)
            {
                if (characteristic.Value == name)
                    return true;
            }

            return false;
        }

        private void SetCharHelper(string name, bool value)
        {
            if (value)
            {
                Filter.FilterHelper.CurrentFilterSet["diffIsCharacteristic"].Add(name);

                if (GetCharHelper("Standard") && GetCharHelper("Lawless") && GetCharHelper("Lightshow") && GetCharHelper("OneSaber") && GetCharHelper("NoArrows") && GetCharHelper("90Degree") && GetCharHelper("360Degree"))
                    Filter.FilterHelper.CurrentFilterSet.Remove("diffIsCharacteristic");
            }
            else
            {
                if (Filter.FilterHelper.CurrentFilterSet["diffIsCharacteristic"] == null)
                {
                    Filter.FilterHelper.CurrentFilterSet["diffIsCharacteristic"][0] = "Standard";
                    Filter.FilterHelper.CurrentFilterSet["diffIsCharacteristic"][1] = "Lawless";
                    Filter.FilterHelper.CurrentFilterSet["diffIsCharacteristic"][2] = "Lightshow";
                    Filter.FilterHelper.CurrentFilterSet["diffIsCharacteristic"][3] = "OneSaber";
                    Filter.FilterHelper.CurrentFilterSet["diffIsCharacteristic"][4] = "NoArrows";
                    Filter.FilterHelper.CurrentFilterSet["diffIsCharacteristic"][5] = "90Degree";
                    Filter.FilterHelper.CurrentFilterSet["diffIsCharacteristic"][6] = "360Degree";
                }

                foreach (var characteristic in Filter.FilterHelper.CurrentFilterSet["diffIsCharacteristic"].AsArray.Children)
                {
                    if (characteristic.Value == name)
                    {
                        Filter.FilterHelper.CurrentFilterSet["diffIsCharacteristic"].Remove(characteristic);
                        break;
                    }
                }
            }

            Filter.FilterHelper.SaveCurrent();
            NotifyFilterSetsAltered();
        }

        private bool GetDiffHelper(string name)
        {
            if (Filter.FilterHelper.CurrentFilterSet["diffIsDifficulty"] == null)
                return true;

            foreach (var characteristic in Filter.FilterHelper.CurrentFilterSet["diffIsDifficulty"].AsArray.Children)
            {
                if (characteristic.Value == name)
                    return true;
            }

            return false;
        }

        private void SetDiffHelper(string name, bool value)
        {
            if (value)
            {
                Filter.FilterHelper.CurrentFilterSet["diffIsDifficulty"].Add(name);

                if (GetDiffHelper("easy") && GetDiffHelper("normal") && GetDiffHelper("hard") && GetDiffHelper("expert") && GetDiffHelper("expertplus"))
                    Filter.FilterHelper.CurrentFilterSet.Remove("diffIsDifficulty");
            }
            else
            {
                if (Filter.FilterHelper.CurrentFilterSet["diffIsDifficulty"] == null)
                {
                    Filter.FilterHelper.CurrentFilterSet["diffIsDifficulty"][0] = "easy";
                    Filter.FilterHelper.CurrentFilterSet["diffIsDifficulty"][1] = "normal";
                    Filter.FilterHelper.CurrentFilterSet["diffIsDifficulty"][2] = "hard";
                    Filter.FilterHelper.CurrentFilterSet["diffIsDifficulty"][3] = "expert";
                    Filter.FilterHelper.CurrentFilterSet["diffIsDifficulty"][4] = "expertplus";
                }

                foreach (var diff in Filter.FilterHelper.CurrentFilterSet["diffIsDifficulty"].AsArray.Children)
                {
                    if (diff.Value == name)
                    {
                        Filter.FilterHelper.CurrentFilterSet["diffIsDifficulty"].Remove(diff);
                        break;
                    }
                }
            }

            Filter.FilterHelper.SaveCurrent();
            NotifyFilterSetsAltered();
        }

        private bool GetModHelper(string name)
        {
            if (Filter.FilterHelper.CurrentFilterSet["diffHasMods"] == null)
                return false;

            foreach (var characteristic in Filter.FilterHelper.CurrentFilterSet["diffHasMods"].AsArray.Children)
            {
                if (characteristic.Value == name)
                    return true;
            }

            return false;
        }

        private void SetModHelper(string name, bool value)
        {
            Plugin.Log.Info(name + " -> " + value.ToString());
            if (value)
            {
                Filter.FilterHelper.CurrentFilterSet["diffHasMods"].Add(name);
            }
            else
            {
                foreach (var diff in Filter.FilterHelper.CurrentFilterSet["diffHasMods"].AsArray.Children)
                {
                    if (diff.Value == name)
                    {
                        Filter.FilterHelper.CurrentFilterSet["diffHasMods"].Remove(diff);
                        break;
                    }
                }

                if (!GetModHelper("noodleextensions") && !GetModHelper("mappingextensions") && !GetModHelper("chroma") && !GetModHelper("cinema"))
                    Filter.FilterHelper.CurrentFilterSet.Remove("diffHasMods");
            }

            Filter.FilterHelper.SaveCurrent();
            NotifyFilterSetsAltered();
        }

        private string GetStringHelper(string name)
        {
            if (Filter.FilterHelper.CurrentFilterSet[name] != null)
                return Filter.FilterHelper.CurrentFilterSet[name].Value;
            else
                return EMPTY_FIELD;
        }

        private void SetStringHelper(string name, string value)
        {
            if (value != "")
                Filter.FilterHelper.CurrentFilterSet[name] = value;
            else
                Filter.FilterHelper.CurrentFilterSet.Remove(name);

            Filter.FilterHelper.SaveCurrent();
            NotifyFilterSetsAltered();
        }

        private string GetDoubleHelper(string name, Func<double, double> transform = null)
        {
            if (Filter.FilterHelper.CurrentFilterSet[name] != null)
            {
                double value = Filter.FilterHelper.CurrentFilterSet[name].AsDouble;
                if (transform != null)
                    value = transform(value);
                return value.ToString("0.###");
            }
            else
                return EMPTY_FIELD;
        }

        private void SetDoubleHelper(string name, string value, Func<double, double> transform = null)
        {
            //value = value.Replace(EMPTY_FIELDS, "");
            if (value != "" && Double.TryParse(value, out double val))
            {
                if(transform != null)
                    val = transform(val);
                Filter.FilterHelper.CurrentFilterSet[name].AsDouble = val;
            }
            else
                Filter.FilterHelper.CurrentFilterSet.Remove(name);

            Filter.FilterHelper.SaveCurrent();
            NotifyFilterSetsAltered();
        }
        private string GetIntegerHelper(string name, Func<int, int> transform = null)
        {
            if (Filter.FilterHelper.CurrentFilterSet[name] != null)
            {
                int value = Filter.FilterHelper.CurrentFilterSet[name].AsInt;
                if (transform != null)
                    value = transform(value);
                return value.ToString();
            }
            else
                return EMPTY_FIELD;
        }

        private void SetIntegerHelper(string name, string value, Func<int, int> transform = null)
        {
            //value = value.Replace(EMPTY_FIELDS, "");
            if (value != "" && Int32.TryParse(value, out int val))
            {
                if (transform != null)
                    val = transform(val);
                Filter.FilterHelper.CurrentFilterSet[name].AsInt = val;
            }
            else
                Filter.FilterHelper.CurrentFilterSet.Remove(name);

            Filter.FilterHelper.SaveCurrent();
            NotifyFilterSetsAltered();
        }

        private string DeleteNonHex(string value)
        {
            string check = value.ToLower();
            string allowed = "0123456789abcdef";
            string res = "";
            foreach (var c in check)
                if (allowed.Contains(c))
                    res += c;
            res.TrimStart('0');
            return res;
        }
        #endregion

        #region uhhyes
        private void NotifyFilterSetsAltered()
        {
            UpdateFilterSets();
            parserParams.EmitEvent("reload");
            findMapButton.interactable = true;
            ChangeWarning("", COLOR_WHITE);
        }

        [UIAction("#post-parse")]
        public void Setup()
        {
            NotifyFilterSetsAltered();
            Filter.FilterHelper.OnFilterSetsAltered += NotifyFilterSetsAltered;
        }

        private void UpdateFilterSets()
        {
            filterSetNames.Clear();
            filterSetNames.AddRange(Filter.FilterHelper.GetFilterSetNames());
            currentSelected = Filter.FilterHelper.CurrentFilterSetName;
            filterSetDropDown.UpdateChoices();
        }
        #endregion

        #region TabGeneral
        [UIValue("FilterSetList")]
        public List<object> filterSetNames = new List<object>() { "default" };

        [UIValue("CurrentFilterSet")]
        public string currentSelected = "default";

        [UIComponent("random-button")]
        internal UnityEngine.UI.Button findMapButton;

        [UIComponent("filterset-list")]
        internal BeatSaberMarkupLanguage.Components.Settings.DropDownListSetting filterSetDropDown;

        [UIComponent("warning-text")]
        internal TextMeshProUGUI warningText;

        [UIAction("switch-filter")]
        private void SwitchFilterSet(string choice)
        {
            Filter.FilterHelper.TrySetCurrentFilter(choice);
        }

        [UIAction("new-filter")]
        private void NewFilterSet(string name)
        {
            string filterName = string.Join("_", name.Split(Path.GetInvalidFileNameChars()));
            filterName = filterName.Trim();
            if (filterName != string.Empty)
            {
                if (invalidFilenames.Contains(filterName)) filterName += "_";
                Filter.FilterHelper.NewOrSelect(filterName);
            }
        }

        private readonly string[] invalidFilenames =
        {
            "AUX", "CON", "NUL", "PRN", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8",
            "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };

        [UIAction("del-filter")]
        private void DeleteFilterSet()
        {
            Plugin.Log.Debug("Deleting current filterset");
            Filter.FilterHelper.DeleteCurrent();
        }

        [UIAction("find-map")]
        internal async void FindMap()
        {
            await GenerateMap();
        }

        internal async Task GenerateMap()
        {
            findMapButton.interactable = false;
            QuickButtonUI.instance.button.interactable = false;

            await MapSelector.SelectRandomSongAsync();
            if (Plugin.RandomSongsFolder?.LevelPack is null)
            {
                ChangeWarning("Wait for SongCore to finish loading level packs!", FilterSettingsUI.COLOR_ORANGE);
            }
            else if (RandomSongGenerator.InitialCache is null)
            {
                ChangeWarning("Something went wrong!", COLOR_RED);
            }
            else if (RandomSongGenerator.InitialCache.count == 0)
            {
                ChangeWarning("Could not find a map using these filters!", COLOR_ORANGE);
            }
            else if (RandomSongGenerator.AllMapsInCache && RandomSongGenerator.CurrentCache.Count == 0)
            {
                ChangeWarning("All maps with this filter have been used.", COLOR_YELLOW);
            }
            else
            {
                ChangeWarning("", COLOR_WHITE);
            }

            findMapButton.interactable = true;
            QuickButtonUI.instance.button.interactable = true;
        }

        internal void ChangeWarning(string text, Color color)
        {
            warningText.text = text;
            warningText.color = color;
        }
        #endregion

        #region TabBasic
        [UIValue("MinKey")]
        public string MinKey
        {
            get { return GetStringHelper("mapMinKey"); }
            set { SetStringHelper("mapMinKey", DeleteNonHex(value)); }
        }

        [UIValue("MaxKey")]
        public string MaxKey
        {
            get { return GetStringHelper("mapMaxKey"); }
            set { SetStringHelper("mapMaxKey", DeleteNonHex(value)); }
        }

        [UIValue("MinRating")]
        public string MinRating
        {
            get { return GetDoubleHelper("mapMinRating", x => x * 100); }
            set { SetDoubleHelper("mapMinRating", value, x => x / 100); }
        }

        [UIValue("MaxRating")]
        public string MaxRating
        {
            get { return GetDoubleHelper("mapMaxRating", x => x * 100); }
            set { SetDoubleHelper("mapMaxRating", value, x => x / 100); }
        }

        [UIValue("MinBPM")]
        public string MinBPM
        {
            get { return GetDoubleHelper("mapMinBPM"); }
            set { SetDoubleHelper("mapMinBPM", value); }
        }

        [UIValue("MaxBPM")]
        public string MaxBPM
        {
            get { return GetDoubleHelper("mapMaxBPM"); }
            set { SetDoubleHelper("mapMaxBPM", value); }
        }

        [UIValue("MinDuration")]
        public string MinDuration
        {
            get { return GetIntegerHelper("mapMinDuration"); }
            set { SetIntegerHelper("mapMinDuration", value); }
        }

        [UIValue("MaxDuration")]
        public string MaxDuration
        {
            get { return GetIntegerHelper("mapMaxDuration"); }
            set { SetIntegerHelper("mapMaxDuration", value); }
        }

        [UIValue("MinNJS")]
        public string MinNJS
        {
            get { return GetDoubleHelper("diffMinNJS"); }
            set { SetDoubleHelper("diffMinNJS", value); }
        }

        [UIValue("MaxNJS")]
        public string MaxNJS
        {
            get { return GetDoubleHelper("diffMaxNJS"); }
            set { SetDoubleHelper("diffMaxNJS", value); }
        }
        #endregion

        #region TabMode        
        [UIValue("CharStandardToggle")]
        public bool CharStandardToggle
        {
            get { return GetCharHelper("Standard"); }
            set { SetCharHelper("Standard", value); }
        }

        [UIValue("CharLawlessToggle")]
        public bool CharLawlessToggle
        {
            get { return GetCharHelper("Lawless"); }
            set { SetCharHelper("Lawless", value); }
        }

        [UIValue("CharSingleSaberToggle")]
        public bool CharSingleSaberToggle
        {
            get { return GetCharHelper("OneSaber"); }
            set { SetCharHelper("OneSaber", value); }
        }

        [UIValue("CharLightshowToggle")]
        public bool CharLightshowToggle
        {
            get { return GetCharHelper("Lightshow"); }
            set { SetCharHelper("Lightshow", value); }
        }

        [UIValue("CharNoArrowsToggle")]
        public bool CharNoArrowsToggle
        {
            get { return GetCharHelper("NoArrows"); }
            set { SetCharHelper("NoArrows", value); }
        }

        [UIValue("Char90DegreeToggle")]
        public bool Char90DegreeToggle
        {
            get { return GetCharHelper("90Degree"); }
            set { SetCharHelper("90Degree", value); }
        }

        [UIValue("Char360DegreeToggle")]
        public bool Char360DegreeToggle
        {
            get { return GetCharHelper("360Degree"); }
            set { SetCharHelper("360Degree", value); }
        }

        [UIValue("DiffEasyToggle")]
        public bool DiffEasyToggle
        {
            get { return GetDiffHelper("easy"); }
            set { SetDiffHelper("easy", value); }
        }

        [UIValue("DiffNormalToggle")]
        public bool DiffNormalToggle
        {
            get { return GetDiffHelper("normal"); }
            set { SetDiffHelper("normal", value); }
        }

        [UIValue("DiffHardToggle")]
        public bool DiffHardToggle
        {
            get { return GetDiffHelper("hard"); }
            set { SetDiffHelper("hard", value); }
        }

        [UIValue("DiffExpertToggle")]
        public bool DiffExpertToggle
        {
            get { return GetDiffHelper("expert"); }
            set { SetDiffHelper("expert", value); }
        }

        [UIValue("DiffExpertplusToggle")]
        public bool DiffExpertplusToggle
        {
            get { return GetDiffHelper("expertplus"); }
            set { SetDiffHelper("expertplus", value); }
        }

        [UIValue("ModNoodleextensionsToggle")]
        public bool ModNoodleextensionsToggle
        {
            get { return GetModHelper("noodleextensions"); }
            set { SetModHelper("noodleextensions", value); }
        }

        [UIValue("ModMappingextensionsToggle")]
        public bool ModMappingextensionsToggle
        {
            get { return GetModHelper("mappingextensions"); }
            set { SetModHelper("mappingextensions", value); }
        }

        [UIValue("ModChromaToggle")]
        public bool ModChromaToggle
        {
            get { return GetModHelper("chroma"); }
            set { SetModHelper("chroma", value); }
        }

        [UIValue("ModCinemaToggle")]
        public bool ModCinemaToggle
        {
            get { return GetModHelper("cinema"); }
            set { SetModHelper("cinema", value); }
        }
        #endregion

        #region TabObjects
        [UIValue("MinNotes")]
        public string MinNotes
        {
            get { return GetIntegerHelper("diffMinNotes"); }
            set { SetIntegerHelper("diffMinNotes", value); }
        }

        [UIValue("MaxNotes")]
        public string MaxNotes
        {
            get { return GetIntegerHelper("diffMaxNotes"); }
            set { SetIntegerHelper("diffMaxNotes", value); }
        }

        [UIValue("MinNPS")]
        public string MinNPS
        {
            get { return GetDoubleHelper("diffMinNPS"); }
            set { SetDoubleHelper("diffMinNPS", value); }
        }

        [UIValue("MaxNPS")]
        public string MaxNPS
        {
            get { return GetDoubleHelper("diffMaxNPS"); }
            set { SetDoubleHelper("diffMaxNPS", value); }
        }

        [UIValue("MinBombs")]
        public string MinBombs
        {
            get { return GetIntegerHelper("diffMinBombs"); }
            set { SetIntegerHelper("diffMinBombs", value); }
        }

        [UIValue("MaxBombs")]
        public string MaxBombs
        {
            get { return GetIntegerHelper("diffMaxBombs"); }
            set { SetIntegerHelper("diffMaxBombs", value); }
        }

        [UIValue("MinBPS")]
        public string MinBPS
        {
            get { return GetDoubleHelper("diffMinBPS"); }
            set { SetDoubleHelper("diffMinBPS", value); }
        }

        [UIValue("MaxBPS")]
        public string MaxBPS
        {
            get { return GetDoubleHelper("diffMaxBPS"); }
            set { SetDoubleHelper("diffMaxBPS", value); }
        }

        [UIValue("MinObstacles")]
        public string MinObstacles
        {
            get { return GetIntegerHelper("diffMinObstacles"); }
            set { SetIntegerHelper("diffMinObstacles", value); }
        }

        [UIValue("MaxObstacles")]
        public string MaxObstacles
        {
            get { return GetIntegerHelper("diffMaxObstacles"); }
            set { SetIntegerHelper("diffMaxObstacles", value); }
        }

        [UIValue("MinOPS")]
        public string MinOPS
        {
            get { return GetDoubleHelper("diffMinOPS"); }
            set { SetDoubleHelper("diffMinOPS", value); }
        }

        [UIValue("MaxOPS")]
        public string MaxOPS
        {
            get { return GetDoubleHelper("diffMaxOPS"); }
            set { SetDoubleHelper("diffMaxOPS", value); }
        }
        #endregion

        #region TabExtra
        [UIValue("MinStarsScoreSaber")]
        public string MinStarsScoreSaber
        {
            get { return GetDoubleHelper("diffMinStarsScoreSaber"); }
            set { SetDoubleHelper("diffMinStarsScoreSaber", value); }
        }

        [UIValue("MaxStarsScoreSaber")]
        public string MaxStarsScoreSaber
        {
            get { return GetDoubleHelper("diffMaxStarsScoreSaber"); }
            set { SetDoubleHelper("diffMaxStarsScoreSaber", value); }
        }

        [UIValue("MinStarsBeatLeader")]
        public string MinStarsBeatLeader
        {
            get { return GetDoubleHelper("diffMinStarsBeatLeader"); }
            set { SetDoubleHelper("diffMinStarsBeatLeader", value); }
        }

        [UIValue("MaxStarsBeatLeader")]
        public string MaxStarsBeatLeader
        {
            get { return GetDoubleHelper("diffMaxStarsBeatLeader"); }
            set { SetDoubleHelper("diffMaxStarsBeatLeader", value); }
        }

        [UIValue("Preset")]
        public string Preset
        {
            get { return GetStringHelper("preset"); }
            set { SetStringHelper("preset", value); }
        }
        #endregion
    }
}