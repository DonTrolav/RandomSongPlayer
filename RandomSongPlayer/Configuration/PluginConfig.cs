using System.IO;
using System;
using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace RandomSongPlayer.Configuration
{
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; }
        public virtual string SongFolderPath { get; set; } = Path.Combine(Environment.CurrentDirectory, "Beat Saber_Data", "Random Songs");
        public virtual string FiltersPath { get; set; } = Path.Combine(Environment.CurrentDirectory, "UserData", "RandomSongFilters");
        public virtual string FilterServerAddress { get; set; } = "https://rsp.bs.qwasyx3000.com/random_maps";
        public virtual QuickButtonConfig QuickButton { get; set; } = new QuickButtonConfig();


        /// <summary>
        /// This is called whenever BSIPA reads the config from disk (including when file changes are detected).
        /// </summary>
        public virtual void OnReload()
        {

        }

        /// <summary>
        /// Call this to force BSIPA to update the config file. This is also called by BSIPA if it detects the file was modified.
        /// </summary>
        public virtual void Changed()
        {
            // Do stuff when the config is changed.
        }

        /// <summary>
        /// Call this to have BSIPA copy the values from <paramref name="other"/> into this config.
        /// </summary>
        public virtual void CopyFrom(PluginConfig other)
        {
            // This instance's members populated from other
        }
    }
}