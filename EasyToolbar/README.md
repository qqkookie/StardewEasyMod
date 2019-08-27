Easy Toolbar Mod
========

**Easy Toolbar Mod** is a [Stardew Valley](http://stardewvalley.net/) mod
that lets you rotate up/down the top inventory row of the toolbar by pressing `Tab` key
and let mouse wheel scroll into empty toolbar slot.
When you press left control key, it selects appropriate tool for you smartly.
When you press '~' key, currently selected tool is deselected and empty-handed.

Compatible with Stardew Valley 1.3+ and SMAPI 2.10+ on Linux, Mac, and Windows.

## Install
1. [Install the latest version of SMAPI](https://smapi.io/).
2. [Install this mod from Nexus mods](http://www.nexusmods.com/stardewvalley/mods/1100).
3. Run the game using SMAPI.

## Configure
Once you run the game, `config.json` text file will be created in the mod's folder.
You can edit it and change setting to configure the mod.

Available settings:

Setting                     | Purpose
----------------------------|----------------------------
`ShiftDownKey`              | Key to shift down toolbar row. Default is `"Tab"` key.
`ShiftUpKey`                | Key to shift up toolbar row.
`ResetOnShift`              | Reset to first slot after shifting toolbar row.
`DeselectOnShift`           | Deselect current tool after shifting toolbar row.
`ScrollToEmpty`             | When mouse wheel scrolls toolbar slots, scroll into empty slot.
`AutoToolKey`               | Key to select appropriate tool for the current tool hit location.<BR> Default is `"LeftControl"` (Left Control key)
`WeedsTool`                 | Tool to select to cut "Weeds". One of `"Axe`, `"Scythe"`, or `"Hoe"`.
`DefaultTool`               | Default tool to select when nothing is appropriate.
`DeselectToolKey`           | Key to deselect current tool. (empty hand) Default is `"OemTilde"`(~) key.

* For all key names, see [key bindings](https://stardewvalleywiki.com/Modding:Key_bindings)
* `DefaultTool` can be tool name like "Scythe" or tool type like "Weapon" or numbered slot ("1"-"12")
or "none" to select nothing (empty hand) or "keep" for keeping current selected tool.
For Underground Mine or Skull Cavern, default tool is "Weapon".
* For `AutoToolKey`, You can hit the key or press and hold it to select tool.
* Pressing `"Escape"` key (to open Game menu) will also deselect tool, same as `DeselectToolKey`.




## Credits
Source code: [Github repository](https://github.com/qqkookie/StardewEasyMod/tree/master/EasyToolbar)

This is based on code of [Rotate Toolbar mod](https://www.nexusmods.com/stardewvalley/mods/1100) by Pathoschild@Nexus.

## Versions

### 1.0
* Initial release.
