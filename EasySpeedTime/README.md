﻿Easy Speed Time Mod
========

**Easy Speed Time Mod** is a [Stardew Valley](http://stardewvalley.net/) mod
which lets you controls times and move speed in the game in various ways.
You can freeze time by pressing a key or in indoor or mine, or by idle time.
You can adjust run/walk speed and do jump over fence.
Your move speed is affected by terrain, weather, etc.
It will open/close fence gates.
In-game wall clock is updated frequently.

Compatible with Stardew Valley 1.3+ and SMAPI 2.10+ on Windows, Linux, MacOS.

## Install
1. [Install the latest version of SMAPI](https://smapi.io).
2. Install [this mod from Nexus mods site](http://www.nexusmods.com/stardewvalley/mods/???).
3. Run the game using SMAPI.

## Use

 * Press `O` key to freeze time. It toggles; pauses and resumes time.
 * Time will freeze by location, like indoor, in mine, or while swimming.
 * After some short idle time, time is paused automatically.
 * After long AFK idle time, game is paused and a message shows up.
 * You can adjust movement speed, speed up run or walk by up to 150%.
 * Move speed is affected when moving on paved roads like a stone or boardwalk, or on hoe dirt.
 * Moving outdoor on stormy weather, or good weather and stamina affects speed.
 * Move speed slows at late night or when inventory is full.
 * Press `Space` key to jump over things or fence and obstacle.
 * Jumping also dives into water and swim, or gets out of water.
 * It automatically opens and closes the gate as player get near the fence gate.
 * In-game daytime clock is updated more frequently, on every 2 minutes, instead of 10 mins.

 * Edit `config.json` to change these time, speed and keys.

## Configure
Once you run the game, `config.json` text file will be created in the mod's folder.
You can edit it and change setting to configure the mod.

Available settings:

Setting                     | Purpose
----------------------------|------------------------------------------------
`MoveSpeedUp`               | Speed up player move (run/walk). 0 to 8. 0 for normal speed, 5 for 100% speed up.
`HorseSpeedUp`              | Speed up player's horse riding. 0 for normal horse speed.
`RoadBuff`                  | Speed up on moving on each type of roads. See Note.
`JumpKey`                   | Key to jump over things. Default is `"Space"` key.
`DisableAutoGate`           | Disable automatic fence gate open/close.
`DisableRunningClock`       | Disable update in-game daytime clock frequently, every 2 min instead of 10 mins.
`DisableTimeFreeze`         | Disable time freeze listed below.
`PauseKey`                  | Key to toggle time pause/resume. Default is `"O"` key.
`IdleTime`                  | Idle time in seconds before time freeze, 0 for no idle time freeze. Default 10 secs.
`PauseTime`                 | Idle time in seconds before game is paused, 0 for no game pause. Default 200 secs.
`FreezeInDoor`              | Freeze time while in-door like farm house or store.
`FreezeInMineCave`          | Freeze time in the Underground mine or Skull Cavern mine.
`FreezeOnSwimming`          | Freeze time when swimming.

For all key names, see [key bindings](https://stardewvalleywiki.com/Modding:Key_bindings)

## Note
* `RoadBuff` is comma-separated list of 11 integers. Default values: [ -1, 0, 1, -1, -1,    -1, 0, 1, 2, 2, -1 ];
* [ hoe_dirt = 0, wood, stone, ghost, iceTile, straw,  gravel, boardwalk = 7,
	colored_cobblestone, cobblestone, steppingStone = 10 ]
* New translation can be added in `Translation.json` file. Encoding is UTF-8.

## Credits
Source code: [Github repository](https://github.com/qqkookie/StardewEasyMod/tree/master/EasySpeedTime)

## Versions
#### 1.0
* Initial release.
