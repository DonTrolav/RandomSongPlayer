using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage;
using System.Linq;
using System.Reflection;
using UnityEngine.UI;
using BeatSaberMarkupLanguage.Components;
using UnityEngine;
using RandomSongPlayer.Configuration;
using System;

namespace RandomSongPlayer.UI
{
    internal class QuickButtonUI : NotifiableSingleton<QuickButtonUI>
    {
        [UIComponent("quick-button")]
        internal Button button;

        internal void Setup()
        {
            var levelFiltering = Resources.FindObjectsOfTypeAll<LevelCollectionViewController>().First();
            if (levelFiltering == null)
            {
                Plugin.Log.Error("Could not find level list.");
                return;
            }
            BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "RandomSongPlayer.UI.QuickButton.bsml"), levelFiltering.gameObject, this);
        }

        [UIAction("button-click")]
        async internal void RandomLevelButtonClick()
        {
            button.interactable = false;
            await MapSelector.SelectRandomSongAsync();
            button.interactable = true;
        }

        public void Show()
        {
            button.gameObject.SetActive(true);
        }

        public void Hide()
        {
            button.gameObject.SetActive(false);
        }
    }
}
