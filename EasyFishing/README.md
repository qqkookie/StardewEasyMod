﻿Easy Fishing Mod
========

**Easy Fishing Mod** is a [Stardew Valley](http://stardewvalley.net/) game mod
that shows fishing-related infos and customize the fishing experience for easier fishing.

Compatible with Stardew Valley 1.3+ and SMAPI 2.10+ on Linux, Mac, and Windows.

## Install
1. [Install the latest version of SMAPI](https://smapi.io).
2. Install [this mod from Nexus mods site](http://www.nexusmods.com/stardewvalley/mods/???).
3. Run the game using SMAPI.

## Use
When you go fishing, this mod ...
* shows fish probabilities of the fishing spot;
* shows info on angling fish and treasure;
* adds gold/silver quality star on picture of fish just caught;
* adds extra fishing info in hovering description at fish collections menu page:
weather, active time, fishing locations and season of the fish;
* shows fish size in metric unit (cm), not inch, for English/Korean language;
* prevents casting fishing rod uselessly unless you are in front of water;
* let you to practice fishing any fish, anytime, anywhere;
* relaxes casting timing tolerance for max casting power;
* reduces time until fish bites;
* pulls the fishing rod out automatically on fish hit;
* instant fish catch;
* always perfect catch;
* adjusts fish difficulty;
* adjusts reel up/down speed;
* adjusts progress fill rate;
* always finds treasure;
* catches treasure without fail;
* fishing bait and tackle lasts longer times.

## Configure
Once you run the game, `config.json` text file will be created in the mod's folder.
You can edit it and change setting to configure the mod.
Configuration is refreshed when game save is loaded.

Available settings:

Setting                     | Purpose
----------------------------|-----------
`DisableFishingSpotInfo`    | Don't show fish probabilities info of current fishing spot.
`DisableCatchInfo`          | Don't show info on angling fish and treasure.
`DisableExtraInfo`          | Don't show extra info of fish collections in game menu.
`UnknownFishCheat`          | Show cheat info of fish you don't know yet.
`MetricSize`                | Display fish size in cm, not inch in English/Korean language.
`FishingPracticeKey`        | Key to practice fishing for training. Default is `"F11"` key.
`PracticeList`              | Fish name list for practice, chosen randomly out of the list.
`DisableFishingAdjust`      | Disable features below for easier fishing experience.
`RelaxCasting`              | Relaxed timing tolerance for Max casting power. 0 is normal, 100 is always max.
`QuickBite`                 | Shorter time until fish bites. 0 is normal, 100 is instant bite.
`AutoHitRod`                | Pull out the fishing rod automatically when fish bites.
`InstantFish`               | Catch fish instantly.
`AlwaysPerfect`             | Force every fish catch to be perfectly executed, even if it wasn't.
`EasyFish`                  | Reduce difficulty of fish. 0 is normal, 90 is the easiest.
`SlowReel`                  | Slow down movement of angling reel: reduction by percent. 0 is normal, 90 is the slowest.
`ExpandBar`                 | Expand size of green moving bar in percent. 0 is normal.
`ProgressStart`             | Set initial catch progress by percent.
`ProgressBoost`             | Boost catch progress rate by percent. 
`AlwaysTreasure`            | Always find treasure.
`CatchTreasure`             | On successful fish catch, catch treasure also without fail.
`LastingBait`               | Fishing bait lasts longer by percent. 50 means 150% the life.
`LastingTackle`             | Fishing tackle lasts longer by percent. 100 means double the life.

For all key names, see [key bindings](https://stardewvalleywiki.com/Modding:Key_bindings)

## Note on fishing practice
* For `ProgressBoost`, 0 is normal. Below 100, slow progress drain. At 100, no decay. 
Above 100, faster progress.
* Fish caught on practice does not count. No new fish in your inventory,
no fishing experience or level gain, no fish count or size record or collection,
no bait/tackle consumption and no stamina drain. Just finger training for you, gamer.
* `PracticeList` is comma separated list enclosed in brackets,
like [ "anchovy", "참치", "AnChOvY" ]. Names are in case-less English like
"salmon" or native word like "연어". Empty list ([]) means all fishes in the game.
Duplicate for frequent choice.
* For list of all fish names, refer [official wiki](https://stardewvalleywiki.com/Fish) or "FishList.txt" file included FYI in mod folder.
* If `LastingBait` or `LastingTackle` is not set, default is 20% longer life per fishing skill level.

## Credits
Source code: [Github repository](https://github.com/qqkookie/StardewEasyMod/tree/master/EasyFishing)

This mod is based on code of [Joys Of Efficiency mod](https://www.nexusmods.com/stardewvalley/mods/2162)
version 1.2.11 by yakminepunyo@Nexus or pomepome@Github

and [Fishing mod](https://github.com/Zoryn4163/SMAPI-Mods/tree/master/FishingMod)
version 2.7.1 by Zoryn4163@GitHub

## Versions

### 1.0
* Initial release. Merged two mod's into one.

