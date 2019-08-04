Easy Save Mod
======

**Easy Save Mod** is a [Stardew Valley](http://stardewvalley.net/) mod
which lets you save your game anytime, anywhere by pressing a key
and automatically backs up your save file saved anytime including new day
and before you start playing game.
And Intro screen is skipped and game can be loaded.

Compatible with Stardew Valley 1.3+ and SMAPI 2.10+ on Linux, Mac, and Windows.

## Install
1. [Install the latest version of SMAPI](https://smapi.io).
2. Install [this mod from Nexus mods site](http://www.nexusmods.com/stardewvalley/mods/???).
3. Run the game using SMAPI.

## Use
 * Press `F9` to save anywhere. Edit `config.json` to change the key.
 * Your game save files will be backed up automatically on each in-game new day.
 * Also on each calendar day before you start playing the game, it will be backed up daily.
 * When game is launched, intro screen is skipped and goes straight to the title or load screen, or coop screen
It also skips the screen transitions, so starting the game is much faster.
 * Or you can load last loaded game automatically on launching, skipping Load game menu. 
 * Screen resolution and position of the windowed title screen can be set.
 * In-game options can be shared among save files. Option change is also shared.
 * Game will not pause while loading even if it loses input focus.

 You can find the back up files in **`%APPDATA%/StardewValley/BackUps`** folder.

## Configure
Once you run the game, `config.json` text file will be created in the mod's folder.
You can edit it and change setting to configure the mod.

Available settings:
Setting                     | Purpose
----------------------------|-----------
`IntroSkipTo`               | When skipping intro, which screen to go?
Can be one of `"Title"`, `"Load"`, `"Host"`, `"Join"`, `"AutoLoad"` and `"AutoHost"`.
`LastLoadedSave`            | Last loaded game for `AutoLoad/AutoHost`. Updated automatically.
`ForgetLastOnTitle`         | When returned to title screen, foret last loaded game.
`TitleWindow`               | Preferred "Windowed" screen resolution and position of title screen. 
List of 4 numbers of width, hieght and optional X, Y position. Default {1920, 1080, -1, -1}
`ShareOptions`              | Share common game options among saves. It saves/loads option changes.
`BackupCount`               | Total number of save backups to keep. Last 50 backups by default.
`DisableBackupOnSave`       | Don't backup on in-game nightly (new day) game save.
`DisableSaveAnyTime`        | Disable saving anytime on key press.
`DisableBackupSaveAnyTime`  | Disable backup of save file by saving anytime.
`SaveAnytimeKey`            | The key which initiates a save at anytime. Default is `"F9"` key.

For all key names, see [key bindings](https://stardewvalleywiki.com/Modding:Key_bindings)

`IntroSkipto` options `Host` and `Join` mean Co-Op play hosting game screen and joining as client.

## Credits
Source code: [Github repository](https://github.com/qqkookie/StardewEasyMod/tree/master/EasySave)

This mod is based on code of [Save Anywhere mod](http://www.nexusmods.com/stardewvalley/mods/444)
version 2.11.0 and [Advanced Save Backup mod](http://www.nexusmods.com/stardewvalley/mods/435)
version 1.7, both by Alpha_Omegasis@Nexus or janavarro95@Github

Intro skip is based on code of [Skip Intro mod](https://www.nexusmods.com/stardewvalley/mods/533)
version 1.8.2 by Pathoschild@Nexus or Pathoschild@GitHub

## Versions
#### 1.0
* Initial release. Merged two mod's by Alpha_Omegasis into one
