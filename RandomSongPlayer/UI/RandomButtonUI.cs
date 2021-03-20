using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static BeatSaberMarkupLanguage.Components.CustomListTableData;

namespace RandomSongPlayer.UI
{
    public class RandomButtonUI : NotifiableSingleton<RandomButtonUI>
    {
        // Plugin is RandomSongPlayer, need this value to download the map
        private Plugin plugin;

        [UIComponent("random-button")]
        internal UnityEngine.UI.Button button;

        internal void Setup(Plugin parentPlugin)
        {
            plugin = parentPlugin;
            try
            {
                var _levelListViewController = Resources.FindObjectsOfTypeAll<SelectLevelCategoryViewController>().Last();
                //var iconSegmentedControl = _levelListViewController.GetField<IconSegmentedControl, SelectLevelCategoryViewController>("_levelFilterCategoryIconSegmentedControl");
                //((RectTransform)iconSegmentedControl.transform).anchoredPosition = new Vector2(0, 4.5f);

                BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "RandomSongPlayer.UI.RandomButton.bsml"), _levelListViewController.gameObject, this);

                //_requestButton.ToggleWordWrapping(false);
                //_requestButton.SetButtonTextSize(5f);
                //UIHelper.AddHintText(_requestButton.transform as RectTransform, "Manage the current request queue");
                Logger.log.Info("Created RSG button!");
            }
            catch
            {
                Logger.log.Warn("Unable to create RSG button");
            }
        }

        public void Show()
        {
            button.gameObject.SetActive(true);
        }

        public void Hide()
        {
            button.gameObject.SetActive(false);
        }

        [UIAction("button-click")]
        async internal void RandomLevelButtonClick()
        {
            button.interactable = false;
            await plugin.SelectRandomSongAsync();
            button.interactable = true;
        }
    }
}