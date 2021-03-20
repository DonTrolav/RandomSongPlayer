using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RandomSongPlayer.UI
{
    public class FilterSettingsUI : NotifiableSingleton<FilterSettingsUI>
    {
        private static readonly string EMPTY_FIELDS = "any";
        private static readonly string EMPTY_FIELDS_ALT = "none";

        [UIParams]
        private BSMLParserParams parserParams;

        private bool GetCharHelper(string name)
        {
            if (Filter.FilterHelper.filters["hasCharacteristic"] != null)
            {
                foreach (var characteristic in Filter.FilterHelper.filters["hasCharacteristic"].AsArray.Children)
                {
                    if (characteristic.Value == name)
                        return true;
                }
            }
            else
                return true;
            return false;
        }

        private void SetCharHelper(string name, bool value)
        {
            if (!value)
            {
                if (Filter.FilterHelper.filters["hasCharacteristic"] == null)
                {
                    Filter.FilterHelper.filters["hasCharacteristic"][0] = "Standard";
                    Filter.FilterHelper.filters["hasCharacteristic"][1] = "Lawless";
                    Filter.FilterHelper.filters["hasCharacteristic"][2] = "Lightshow";
                    Filter.FilterHelper.filters["hasCharacteristic"][3] = "OneSaber";
                    Filter.FilterHelper.filters["hasCharacteristic"][4] = "NoArrows";
                }

                foreach (var characteristic in Filter.FilterHelper.filters["hasCharacteristic"].AsArray.Children)
                {
                    if (characteristic.Value == name)
                    {
                        Filter.FilterHelper.filters["hasCharacteristic"].Remove(characteristic);
                        break;
                    }
                }
            }
            else
            {
                Filter.FilterHelper.filters["hasCharacteristic"].Add(name);

                if (GetCharHelper("Standard") && GetCharHelper("Lawless") && GetCharHelper("Lightshow") && GetCharHelper("OneSaber") && GetCharHelper("NoArrows"))
                    Filter.FilterHelper.filters.Remove("hasCharacteristic");
            }

            Filter.FilterHelper.Save();
            NotifyFilterReload();
        }

        private bool GetDiffHelper(string name)
        {
            if (Filter.FilterHelper.filters["hasDifficulty"] != null)
            {
                foreach (var characteristic in Filter.FilterHelper.filters["hasDifficulty"].AsArray.Children)
                {
                    if (characteristic.Value == name)
                        return true;
                }
            }
            else
                return true;
            return false;
        }

        private void SetDiffHelper(string name, bool value)
        {
            if (!value)
            {
                if (Filter.FilterHelper.filters["hasDifficulty"] == null)
                {
                    Filter.FilterHelper.filters["hasDifficulty"][0] = "easy";
                    Filter.FilterHelper.filters["hasDifficulty"][1] = "normal";
                    Filter.FilterHelper.filters["hasDifficulty"][2] = "hard";
                    Filter.FilterHelper.filters["hasDifficulty"][3] = "expert";
                    Filter.FilterHelper.filters["hasDifficulty"][4] = "expertplus";
                }

                foreach (var diff in Filter.FilterHelper.filters["hasDifficulty"].AsArray.Children)
                {
                    if (diff.Value == name)
                    {
                        Filter.FilterHelper.filters["hasDifficulty"].Remove(diff);
                        break;
                    }
                }
            }
            else
            {
                Filter.FilterHelper.filters["hasDifficulty"].Add(name);

                if (GetDiffHelper("easy") && GetDiffHelper("normal") && GetDiffHelper("hard") && GetDiffHelper("expert") && GetDiffHelper("expertplus"))
                    Filter.FilterHelper.filters.Remove("hasDifficulty");
            }

            Filter.FilterHelper.Save();
            NotifyFilterReload();
        }

        private string GetStringHelper(string name, bool alt_default)
        {
            if (Filter.FilterHelper.filters[name] != null)
                return Filter.FilterHelper.filters[name].Value;
            else if (alt_default)
                return EMPTY_FIELDS_ALT;
            else
                return EMPTY_FIELDS;
        }

        private void SetStringHelper(string name, string value)
        {
            // In case EMPTY_FIELDS is just some kind of spaces we could use this again, but for now this might fuck up presets
            //value = value.Replace(EMPTY_FIELDS, "");
            if (value != "")
                Filter.FilterHelper.filters[name] = value;
            else
                Filter.FilterHelper.filters.Remove(name);

            Filter.FilterHelper.Save();
            NotifyFilterReload();
        }

        private string GetDoubleHelper(string name, Func<double, double> transform = null)
        {
            if (Filter.FilterHelper.filters[name] != null)
            {
                double value = Filter.FilterHelper.filters[name].AsDouble;
                if (transform != null)
                    value = transform(value);
                return value.ToString("0.###");
            }
            else
                return EMPTY_FIELDS;
        }

        private void SetDoubleHelper(string name, string value, Func<double, double> transform = null)
        {
            value = value.Replace(EMPTY_FIELDS, "");
            double val;
            if (value != "" && Double.TryParse(value, out val))
            {
                if(transform != null)
                    val = transform(val);
                Filter.FilterHelper.filters[name].AsDouble = val;
            }
            else
                Filter.FilterHelper.filters.Remove(name);

            Filter.FilterHelper.Save();
            NotifyFilterReload();
        }
        private string GetIntegerHelper(string name, Func<int, int> transform = null)
        {
            if (Filter.FilterHelper.filters[name] != null)
            {
                int value = Filter.FilterHelper.filters[name].AsInt;
                if (transform != null)
                    value = transform(value);
                return value.ToString();
            }
            else
                return EMPTY_FIELDS;
        }

        private void SetIntegerHelper(string name, string value, Func<int, int> transform = null)
        {
            value = value.Replace(EMPTY_FIELDS, "");
            int val;
            if (value != "" && Int32.TryParse(value, out val))
            {
                if (transform != null)
                    val = transform(val);
                Filter.FilterHelper.filters[name].AsInt = val;
            }
            else
                Filter.FilterHelper.filters.Remove(name);

            Filter.FilterHelper.Save();
            NotifyFilterReload();
        }

        private void NotifyFilterReload()
        {
            parserParams.EmitEvent("reload");
        }

        private string DeleteNonHex(string value)
        {
            string allowed = "0123456789abcdefABCDEF";
            string res = "";
            foreach (var c in value)
                if (allowed.Contains(c))
                    res += c;
            return res;
        }

        [UIAction("#post-parse")]
        public void Setup()
        {
            Filter.FilterHelper.OnFiltersReload += NotifyFilterReload;
        }

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

        [UIValue("MinKey")]
        public string MinKey
        {
            get { return GetStringHelper("minKey", false); }
            set { SetStringHelper("minKey", DeleteNonHex(value)); }
        }

        [UIValue("MaxKey")]
        public string MaxKey
        {
            get { return GetStringHelper("maxKey", false); }
            set { SetStringHelper("maxKey", DeleteNonHex(value)); }
        }

        [UIValue("MinRating")]
        public string MinRating
        {
            get { return GetDoubleHelper("minRating", x => x * 100); }
            set { SetDoubleHelper("minRating", value, x => x / 100); }
        }

        [UIValue("MaxRating")]
        public string MaxRating
        {
            get { return GetDoubleHelper("maxRating", x => x * 100); }
            set { SetDoubleHelper("maxRating", value, x => x / 100); }
        }

        [UIValue("MinBPM")]
        public string MinBPM
        {
            get { return GetDoubleHelper("minBPM"); }
            set { SetDoubleHelper("minBPM", value); }
        }

        [UIValue("MaxBPM")]
        public string MaxBPM
        {
            get { return GetDoubleHelper("maxBPM"); }
            set { SetDoubleHelper("maxBPM", value); }
        }

        [UIValue("MinNPS")]
        public string MinNPS
        {
            get { return GetDoubleHelper("minNPS"); }
            set { SetDoubleHelper("minNPS", value); }
        }

        [UIValue("MaxNPS")]
        public string MaxNPS
        {
            get { return GetDoubleHelper("maxNPS"); }
            set { SetDoubleHelper("maxNPS", value); }
        }

        [UIValue("MinNJS")]
        public string MinNJS
        {
            get { return GetDoubleHelper("minNJS"); }
            set { SetDoubleHelper("minNJS", value); }
        }

        [UIValue("MaxNJS")]
        public string MaxNJS
        {
            get { return GetDoubleHelper("maxNJS"); }
            set { SetDoubleHelper("maxNJS", value); }
        }

        [UIValue("MinDuration")]
        public string MinDuration
        {
            get { return GetIntegerHelper("minDuration"); }
            set { SetIntegerHelper("minDuration", value); }
        }

        [UIValue("MaxDuration")]
        public string MaxDuration
        {
            get { return GetIntegerHelper("maxDuration"); }
            set { SetIntegerHelper("maxDuration", value); }
        }

        [UIValue("MinNotes")]
        public string MinNotes
        {
            get { return GetIntegerHelper("minNotes"); }
            set { SetIntegerHelper("minNotes", value); }
        }

        [UIValue("MaxNotes")]
        public string MaxNotes
        {
            get { return GetIntegerHelper("maxNotes"); }
            set { SetIntegerHelper("maxNotes", value); }
        }


        [UIValue("MinBombs")]
        public string MinBombs
        {
            get { return GetIntegerHelper("minBombs"); }
            set { SetIntegerHelper("minBombs", value); }
        }

        [UIValue("MaxBombs")]
        public string MaxBombs
        {
            get { return GetIntegerHelper("maxBombs"); }
            set { SetIntegerHelper("maxBombs", value); }
        }

        [UIValue("MinBPS")]
        public string MinBPS
        {
            get { return GetDoubleHelper("minBPS"); }
            set { SetDoubleHelper("minBPS", value); }
        }

        [UIValue("MaxBPS")]
        public string MaxBPS
        {
            get { return GetDoubleHelper("maxBPS"); }
            set { SetDoubleHelper("maxBPS", value); }
        }

        [UIValue("MinObstacles")]
        public string MinObstacles
        {
            get { return GetIntegerHelper("minObstacles"); }
            set { SetIntegerHelper("minObstacles", value); }
        }

        [UIValue("MaxObstacles")]
        public string MaxObstacles
        {
            get { return GetIntegerHelper("maxObstacles"); }
            set { SetIntegerHelper("maxObstacles", value); }
        }

        [UIValue("MinOPS")]
        public string MinOPS
        {
            get { return GetDoubleHelper("minOPS"); }
            set { SetDoubleHelper("minOPS", value); }
        }

        [UIValue("MaxOPS")]
        public string MaxOPS
        {
            get { return GetDoubleHelper("maxOPS"); }
            set { SetDoubleHelper("maxOPS", value); }
        }

        [UIValue("Preset")]
        public string Preset
        {
            get { return GetStringHelper("preset", true); }
            set { SetStringHelper("preset", value); }
        }
    }
}
