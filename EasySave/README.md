Easy Save Mod
======

**Easy Save Mod** is a [Stardew Valley](http://stardewvalley.net/) mod
which lets you save your game anytime, anywhere by pressing a key
and automatically backs up your save file saved anytime including new day
and before you start playing game.

Compatible with Stardew Valley 1.3+ and SMAPI 2.10+ on Linux, Mac, and Windows.

## Install
1. [Install the latest version of SMAPI](https://smapi.io).
2. Install [this mod from Nexus mods site](http://www.nexusmods.com/stardewvalley/mods/???).
3. Run the game using SMAPI.

## Use
Press `F9` to save anywhere. Edit `config.json` to change the key.
Your game save files will be backed up automatically on each in-game new day.
Also on each calendar day before you start playing the game, it will be backed up daily.

You can find the back up files in **`%APPDATA%/StardewValley/BackUps`** folder.

## Configure
Once you run the game, `config.json` text file will be created in the mod's folder.
You can edit it and change setting to configure the mod.

Available settings:
Setting                     | Purpose
----------------------------|-----------
`BackupCount`               | Total number of save backups to keep. Last 50 backups by default.
`DisableBackupOnSave`       | Don't backup on in-game nightly (new day) game save.
`DisableSaveAnyTime`        | Disable saving anytime on key press.
`DisableBackupSaveAnyTime`  | Disable backup of save file by saving anytime.
`SaveAnytimeKey`            | The key which initiates a save at anytime. Default is `"F9"` key.

For all key names, see [key bindings](https://stardewvalleywiki.com/Modding:Key_bindings)

## Credits
Source code: [Github repository](https://github.com/qqkookie/StardewEasyMod/tree/master/EasySave)

This mod is based on code of [Save Anywhere mod](http://www.nexusmods.com/stardewvalley/mods/444)
version 2.11.0 and [Advanced Save Backup mod](http://www.nexusmods.com/stardewvalley/mods/435)
version 1.7, both by Alpha_Omegasis@Nexus or janavarro95@Github

## Versions
#### 1.0
* Initial release. Merged two mod's by Alpha_Omegasis into one
