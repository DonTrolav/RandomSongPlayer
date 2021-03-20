using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Attributes;
namespace RandomSongPlayer.UI
{
    public class ModifierUI : NotifiableSingleton<ModifierUI>
    {
        [UIValue("introSkipToggle")]
        public bool introSkipToggle = true;
        [UIAction("setIntroSkipToggle")]
        void SetIntro(bool value)
        {
            introSkipToggle = value;
        }
        [UIValue("outroSkipToggle")]
        public bool outroSkipToggle = true;
        [UIAction("setOutroSkipToggle")]
        void SetOutro(bool value)
        {
            outroSkipToggle = value;
        }


    }
}