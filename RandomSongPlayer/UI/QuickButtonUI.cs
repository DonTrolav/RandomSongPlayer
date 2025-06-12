using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Util;
using RandomSongPlayer.Configuration;

namespace RandomSongPlayer.UI
{
    internal class QuickButtonUI : NotifiableSingleton<QuickButtonUI>
    {
        [UIComponent("quick-button")]
        internal Button button;

        public static void Init()
        {
            var levelFiltering = Resources.FindObjectsOfTypeAll<LevelFilteringNavigationController>().First();
            if (levelFiltering == null)
            {
                Plugin.Log.Error("Could not find level list.");
                return;
            }
            BSMLParser.Instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "RandomSongPlayer.UI.QuickButton.bsml"), levelFiltering.gameObject, instance);
            levelFiltering.didSelectBeatmapLevelPackEvent -= OnMapPackChange;
            levelFiltering.didSelectBeatmapLevelPackEvent += OnMapPackChange;
        }

        [UIAction("button-click")]
        async internal void RandomLevelButtonClick()
        {
            await FilterSettingsUI.instance.GenerateMap();
        }

        [UIValue("buttonX")]
        private float ButtonX { get { return PluginConfig.Instance.QuickButton.PositionX; } }

        [UIValue("buttonY")]
        private float ButtonY { get { return PluginConfig.Instance.QuickButton.PositionY; } }

        [UIValue("buttonWidth")]
        private float ButtonWidth { get { return PluginConfig.Instance.QuickButton.Width; } }

        [UIValue("buttonHeight")]
        private float ButtonHeight { get { return PluginConfig.Instance.QuickButton.Height; } }

        private static void OnMapPackChange(LevelFilteringNavigationController levelFilter, BeatmapLevelPack levelPack, GameObject gameObject, LevelSelectionOptions options)
        {
            if (instance == null)
                return;

            switch (PluginConfig.Instance.QuickButton.ShowMode)
            {
                case ShowMode.Never:
                    instance.Hide();
                    break;
                case ShowMode.OnRandomPack:
                    if (levelPack?.packName == "Random Songs")
                        instance.Show();
                    else
                        instance.Hide();
                    break;
                case ShowMode.Always:
                    instance.Show();
                    break;
            }
        }

        public void Show()
        {
            button.gameObject.SetActive(true);
            //button.transform.position = new Vector2(ButtonX, ButtonY);
        }

        public void Hide()
        {
            button.gameObject.SetActive(false);
        }
    }
}
