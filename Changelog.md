
# SPIC Changelog

## v4.0.0.1
- Updated Spiky's lib dependency to v1.3
- Fixed Infinite Materials not working with Quality od Terraria
- Fixed group materials count
- Fixed tooltip line opacity been ignored
- Fixed Glow and Dots scaling
- Fixed infinities been displayed in places they shouldn't be

## v4.0
- Reworked tile duplication
- Reworked the way the mod works internally
- Updated icon
- Added settings to the Display config
- Added loads of tooltips
- Simplified notification
- Added Version config
- Moved Definition and CustomDictionaryElement to Spiky's Lib
- Added automatic localization generation
- Updated to Spiky's lib v1.2
- Fixed bugs with preset

## v3.1.1.3
- Fixed material groups consuming when infinite

## v3.1.1.2
- Fixed Spiky's Lib reference to use the published build and not the dev build

## v3.1.1.1
- Adjusted default colors
- Fixed loading error with MagicStorage

## v3.1.1
- Fixed Rubblemaker 
- Allowed requirements to copy each other
- Merged Enabled and Settings fields in configs
- Added Spiky's Lib mod dependency
- Simplified Infinity overrides
- Replaced RegisterTooltipLine and RegisterCountToStr with interfaces
- Removed "static" variants of classes
- Replaced AddConfig calls with a static field check
- Remade the update notification
- Fixed default values been saved in json

## v3.1.0.1
- Fixed dot display for recipe materials

## v3.1
- Automatic ModConfig port.
- Magic storage v0.6 support.
- Added group specific presets.
- Added a hard cap to the displays cache refresh rate.
- Added a Simple glow option.
- Added Cannon ammo detection
- Added display ModType.
- Reduced cache size and added 'spiccache' command to display it.
- Made CountToString and GetTooltipLine optional and modular
- Removed the need for Bucket detection
- Renamed 'Shop' to 'Currency' and added toggles for various shops.
- Renamed 'Liquid' to 'Bucket'
- Moved in-game changelog into the config and made welcome message less intrusive.
- Improved Extra localization
- Fixed configs not loading properly in Multiplayer

## v3.0
- 1.4.4 port.
- Added Extractinator GrabBag category.
- Corrected incorrect TooltipLine names and values.
- Added Ammo slots, Coin slots, Journey research and Recipe Material exclusive areas.
- Simplified Category detection for Explosives and Containers.
- Colors Infinities configs.
- Reworked configs UI to include a sub-config per Group.
- Change Glow infinity display to look like Treasure Bags glow.
- Merged Category detection and Customs.
- Localized all the mod.
- Added Exclusive display to only display Infinities in a specific context.
- Generalized Categories into Extras.
- Renamed Consumable Groups to Infinity.
- Added new ModType Groups to group Infinities by their consumable type.
- Reworked Presets and Infinities into ModType.
- Reworked cache.
- Fixed incorrect tooltip sprite for weapons.
- Fixed Hair style change always consuming coins.
- Fixed Customs causing lag.

## v2.2.1
- Added Welcome message.
- Added custom requirements.
- Added logs on group or preset register.
- Improved the ItemCountWrapper display.
- Added temporary Item duplication prevention for Placeables.
- Added UIElement for custom EntityDefinitions.
- Completed localization for groups.
- Fixed DisableAboveRequirement requirement display.
- Fixed items in Void Vault been ignored.
- Fixed infinity not updating when buying items.
- Fixed Disabled categories always been infinite.

## v2.2.0.1
- Renamed RequirementSettings to GroupSettings.
- Fixed a bug causing any non detectable category to never be used.
- Fixed a multiplayer crash caused by the config not properly loading.

## v2.2
- Added Journey Sacrifices group.
- Added Mixed group for enabled but unused groups.
- Disabled tile duplication as it is full of bugs.
- Reworked and abstracted Consumable Group API to be easier to use.
- Reworked category, infinity and requirement
- Lots of API changes and reworks
- Added new optional interfaces for groups.
- Created many ConfigElements.
- Overhauled the visual of the configs.
- Added new configs items and presets.
- Replaced customs with a blacklist
- Configs will now adapt to the registered groups.
- Renamed 'Consumables' to 'Usables' for clarity.
- Renamed and corrected typos.
- Performances improvement (I think ?).
- Fixed a lot of incorrect category detection
- Fixed many bugs

## v2.1
- Ported to next tml stable.
- Magic storage integration.
- Infinities are now displayed in item sprite, glow and tooltip.
- Added Infinity display config.
- Improved the way infinites are stored.
- Improved localization.
- Fixed colored tooltip not been correctly displayed.
- Fixed mouseItem not been counted or been counted twice.
- Fixed grab bags and opened by leftclick not been detected.
- Fixed potions used by right click not been detected.
- Fixed detected categories been applied one tick
- Fixed a bug in stack detection causing the mod to crash on load.
- Fixed other bugs.

## v2.0
- rewrote and cleaned the entire codebase.
- new category and infinity system.
- added currencies and grab bags categories.
- merged placeable and wand ammo.
- added information to the item tooltip.
- added sub categories for most categories.
- reworked infinite materials.
- can now detect usable and explosive items.
- Reworked tile duplication to use a chunk system and be included in .twld file.
- more flexible configs.
- added many new config items.
- removed commands.
- reworked stack detection mechanism to be automatic.
- performance improvement.
- updated desc and README
- en-US localization
- added mod icon
- fixed a ton of bug.

## v1.3.1
- Finally released on the Mod Browser.
- 'Liquid' buckets can now be infinite.
- Works with wires and actuators.

## v1.3
- Can now works with mods increasing max stacks and added related config items.
- Furniture will now have the right category.

## v1.2
- Prevents item duplication for infinite usables.
- Added new categories.

## v1.1
- Custom categories and values introduced.
- Added 'set', 'category' and 'values' commands.

## v1.0
- Consumables can now be infinite.
