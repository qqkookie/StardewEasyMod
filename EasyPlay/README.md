Easy Play Mod
========

**Easy Play Mod** is a [Stardew Valley](http://stardewvalley.net/) mod
which enhance you mining experience.
It adds mine elevator in Skull Cavern Mine like normal mine.
It shows stones in mine with hidden mine ladder.

**Don't Ask To Eat Mod** is mod
which removes frequent confirm dialogue asking to eat
when clicking action button, holding edible item in hand.
It blocks asking to eat/drink for most edible items, except allowed by player.

Compatible with Stardew Valley 1.3+ and SMAPI 2.10+ on Windows, Linux, MacOS.

## Install
1. [Install the latest version of SMAPI](https://smapi.io).
2. Install [this mod from Nexus mods site](http://www.nexusmods.com/stardewvalley/mods/???).
3. Run the game using SMAPI.

## Use

 * You explore normal Mine in northern mountain first to level 5 (6th floor) to use mine elevator. Then you will be able to use elevator in Skull Cavern Mine in the Calico Desert area, too.
 * In mine, select **pickaxe** tool will mark stones over hidden mine ladder with coral color square.

## Configure
Once you run the game, `config.json` text file will be created in the mod's folder.
You can edit it and change setting to configure the mod.

Available settings:

Setting                     | Purpose
----------------------------|------------------------------------------------
`ForceLadder`               | Find Ladder: Force breaking next stone will generate ladder.
`DisableEasyHorse`          | Easy Horse: Disable horse whistle and slim horse.
`HorseWhistleKey`           | Horse Whistle: Key to summon horse to player. Default is `'V'` key.
`DisableWhistleSound`       | Horse Whistle: Disable whistle sound.
`DisableDontAskToEat`       | Don't Ask to Eat: Restore eating/drinking action and dialogue.
`EatPrefixKey`              | Don't Ask to Eat: Key to allow eating something not in the food list.
`Foods`                     | Don't Ask to Eat: Eat/drink only these in the foods list. Don't ask to eat other edibles.

 * Pressing Left Control + horse whistle key near the horse will remove horse hat and put it into your inventory.
 * The `Foods` list is name list of permitted foods. Name can be either localized display name or English name,
upper/lower case is ignored. For edible items not listed here, confirm dialogue will be blocked.
In square bracket list, each item is comma-separated and enclosed in double quotation marks.
 * Example =>  `"Foods" : [ "Potato", "large egg", "gArLiC", "설탕", "조자 콜라" ]`
 * To eat edible item not registered in the `Foods` list, choose the item in hand
and press `EatPrefixKey` key. Default is `"F8"` key.
Then you choose `Yes` on confirm dialogue to eat it,
the edible item will be automatically added to the allowed `Foods` list.
Afterward, you don't have to press prefix key to eat same kind of edible item.

## Credits
Source code: [Github repository](https://github.com/qqkookie/StardewEasyMod/tree/master/EasyPlay)

## Versions
#### 1.0
* Initial release.
