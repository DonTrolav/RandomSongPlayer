# RandomSongPlayer
A plugin for Beat Saber that downloads a random song from BeatSaver and plays it.

## How to install
Install all required mods from BSManager:
- BSIPA
- BS Utils
- SongCore
- BeatSaberMarkupLanguage
- BeatSaverSharp
- SongDetailsCache

Download the release for your game version [here](https://github.com/DonTrolav/RandomSongPlayer/releases) and move it into the "Plugins" folder of your Beat Saber directory. 

## How to use
Navigate to the "Solo" game mode or join a multiplayer lobby. You will find the "RSP" settings under the "Mods" tab on the left-hand side.

Use the "Generate Random Song" button in the settings or the "RSP" button on the RandomSongs level pack to download and select a random map.

You can also define filter to narrow down the random maps that can be generated.

## Filters
A filter will check the map or difficulty for certain aspects:
- **"Basic"** will filter for general beatmap information (key, rating, duration) 
- **"Objects"** will filter for a number of objects (notes, bombs, walls) in a difficulty
- **"Mode"** will search in a specific characteristic/difficulty or mod requirements
- **"Extra"** will filter for additional settings.

Any filter setting will not be considered if the field is left empty.

For Scoresaber and Beatleader star ratings, set the minimum required stars to 0 if you want to filter for any ranked map.

The server preset allows for special filters using a filtering server. You can change the server address in the plugin configuration.

## Addendum
Thanks to DarkGrisen for the original plugin.

This plugin was inspired by the Random Song Tournament.
