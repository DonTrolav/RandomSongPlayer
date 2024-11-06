using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Util;
using RandomSongPlayer.Configuration;
using System.Xml.Serialization.Configuration;

namespace RandomSongPlayer.UI
{
    internal class QuickButtonUI : NotifiableSingleton<QuickButtonUI>
    {
        [UIComponent("quick-button")]
        internal Button button;

        internal void Setup()
        {
            var levelFiltering = Resources.FindObjectsOfTypeAll<LevelFilteringNavigationController>().First();
            //var levelFiltering = Resources.FindObjectsOfTypeAll<LevelCollectionNavigationController>().First();
            if (levelFiltering == null)
            {
                Plugin.Log.Error("Could not find level list.");
                return;
            }
            BSMLParser.Instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "RandomSongPlayer.UI.QuickButton.bsml"), levelFiltering.gameObject, this);
        }

        [UIAction("button-click")]
        async internal void RandomLevelButtonClick()
        {
            button.interactable = false;
            await MapSelector.SelectRandomSongAsync();
            button.interactable = true;
        }

        [UIValue("buttonX")]
        private float ButtonX { get { return PluginConfig.Instance.QuickButton.PositionX; } }

        [UIValue("buttonY")]
        private float ButtonY { get { return PluginConfig.Instance.QuickButton.PositionY; } }

        [UIValue("buttonWidth")]
        private float ButtonWidth { get { return PluginConfig.Instance.QuickButton.Width; } }

        [UIValue("buttonHeight")]
        private float ButtonHeight { get { return PluginConfig.Instance.QuickButton.Height; } }

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
