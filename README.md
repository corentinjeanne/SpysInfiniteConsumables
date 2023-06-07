## Make every consumable item infinite

Collect enough a consumable item to make it infinite!
Depending of the item, this requirement may vary.
There are a wide variety of groups for all sorts of consumables (usables, placeables, materials and more) each with their own sub-categories.


## No more item duplication

**WORK IN PROGRESS, WILL BE REWORKED IN THE FUTURE**

Prevents the duplication of consumables in various vays
 - Infinite critters will turn to somke when caught
 - Infinite placeable above the requireement will be consumed
 - Infinite bucket won't fill or empty


## Alter the behaviour of the mod

Edit the `Consumable Group Settings` config to modify the behaviour of the mod!
Toggle on/off certain groups and blacklist consumables if necessary.


## Easely see the infinities of an item

The infinities of every item can be displayed in various ways: 
 - In its tooltip via colored lines. This can also be used to display the categories and requirements of item.
 - On the item sprite via glow. Not that it can sometimes be hard to see.
 - In the item slot via colored dots.

Edit the `Infinities Display` config to change which one is active and display aditional informations or change the color of the infinities.


## Automaticaly detect the category of almost every item

If an item's category cannot be determined with the items's stats, it will be determined when the item is used!
This should work for almost every summoner, player booster and more.
Edit the `Category Detection` config to modify disable this or clear all the detected categories.

**Please report a bug if the category of an item is not detected or is incorrect.**



## Consumable groups

Below you can find all vanilla consumable groups and their respective categories.
Note than a consumable can be in includes in multiple groups.

### Usables
Items consumed uppon use.
 - `Weapon`: Deals damage.
 - `Recovery`: Heals life or mana.
 - `Buff`: Provides a buff (swiftness, archery...).
 - `Player booster`: Give a permanent boost to the player (live, mana...).
 - `World booster`: Permanently modifies the world.
 - `Summoner`: Summons a boss or an event.
 - `Critter`: Summong a critter or is a bait.
 - `Explosive`: Destroys tiles or creates liquid.
 - `Tool`: Has a miscellaneous use (move the player, light an area...).
 - `Unknown`: Is one of the above but not yet determined.

### Ammunitions
Items used as ammunition and consumed by a weapon.
 - `Basic`: Arrows, bullets, rockets and darts.
 - `Explosive`: Explodes on impact.
 - `Special`: All other ammunitions.

### Placeables
Items placing a tile.
 - `Block`: Places a block.
 - `Wall`: Places a wall.
 - `Wiring`: Wires.
 - `Torch`: Places a torch.
 - `Ore`: Raw minerals.
 - `Gem`: Not used for now.
 - `LightSource`: Emits light.
 - `Container`: Can store items.
 - `Functional`: Can be interacted with.
 - `CraftingStation`: Use to craf items.
 - `Decoration`: A basic furniture.
 - `MusicBox`: Plays music.
 - `Mechanical`: Can be triggered with a wire.
 - `Liquid`: Places or remove liquid.
 - `Seed`: Grows into a plant.
 - `Paint`: Color walls and tiles.

### Grab bags
Items giving various items when openned.
 - `Crate`: Can be openned.
 - `TreasureBag`: Dropped form a boss in expert mode or above.

### Materials
Items used for crafting.
Infinity is 1/2 of the requirement.
 - `Basic`: Blocks, common items.
 - `Ore`: Turn into bars.
 - `Furniture`: All other placeables
 - `Miscellaneous`: Only a material.
 - `NonStackable`: Weapons, armors, etc...

### Currencies
Items used to buy from NPCs.
Applies to a group of items and not individual items.
 - `Coin`: Currencies with more than one items (infinity is 1/50 of the requirement).
 - `SingleCoin`: Currencies with a single item (infinity is 1/5 of the requirement).

### Journey Sacrifices
Items that can be researched.


## Configs

### Consumable Group Settings
This is the main config of the mod (server side)
 - `Enabled Groups`: Controls key aspects of the mods, such as active Consumable Groups.
 - `Group Settings`: Lists the settings of each Group. Click on a requirement to change between items and stacks.
 - `Customs`: Custom requirements for any consumable.

### Category detection
Controls automatic category detection (client side).
 - `Detected Categories`: All the detected categories. Reset the config or clear individual items to remove them.

### Infinity Display
Various ways to display the infinities of items.
 - `Item Tooltip`: In the item tooltip.
 - `Item glow`: In the colore of the item.
 - `Colored dots`: With colored dots around the item. Start end end are the positions of the 1st and last dot able to be displayed. In they are more infinities than dots, the will cycle every few seconds.


## Changelog

#### v2.2.1
 - Added Welcome message
 - Added custom requirements
 - Added logs on group or preset register
 - Improved the ItemCountWrapper display
 - Added temporary Item duplication prevention for Placeables.
 - Added UIElement for custom EntityDefinitions
 - Completed localization for groups.
 - Fixed DisableAboveRequirement requirement display.
 - Fixed items in Void Vault been ignored.
 - Fixed infinity not updating when buying items.
 - Fixed Disabled categories always been infinite.

#### v2.2.0.1
 - Renamed RequirementSetings to GroupSettings.
 - Fixed a bug causing any non detectable category to never be used.
 - Fixed a multiplayer crash caused by the config not properly loading.

#### v2.2
 - Added Journey Scrificices group.
 - Added Mixed group for enabled but unused groups.
 - Disabled tile dupplication as it is full of bugs.
 - Reworked and abstracted Consumable Group API to be easier to use.
 - Reworked category, infinity and requirement
 - Lots of API changes and reworks
 - Added new optional interfaces for groups.
 - Created many ConfigElements.
 - Overhalled the visual of the configs.
 - Added new configs items and presets.
 - Replaced customs with a blacklist
 - Configs will now adapt to the registered groups.
 - Renamed 'Consumables' to 'Usables' for clarity.
 - Remaned and corrected typos.
 - Performances imporovement (I think ?).
 - Fixed a lot of incorrect category detection
 - Fixed many bugs

#### v2.1
 - Ported to next tml stable.
 - Magic storage integration.
 - Infinities are now displayed in item sprite, glow and tooltip.
 - Added Infinity display config.
 - Imporved the way infinites are stored.
 - Improved locatization.
 - Fixed colored tooltip not been correctly displayed .
 - Fixed mouseItem not been counted or been counted twice.
 - Fixed grabbags and openned by leftclick not been detected.
 - Fixed potions used by right clik not been detected.
 - Fixed detected categories been applied one tick
 - Fixed a bug in stack detection causing the mod to crash on load.
 - Fixed other bugs.

#### v2.0
 - rewrote and cleaned the entire codebase.
 - new category and infinity system.
 - added currencies and grabbags categories.
 - merged placeable and wand ammo.
 - added informations to the item tooltip.
 - added sub categories for most categories.
 - reworked infite materials.
 - can now detect usable and explosive items.
 - Reworked tile duplication to use a chunk system and be induded in .twld file.
 - more flexible configs.
 - added many new config items.
 - removed commands.
 - reworked stack detection mecanism to be automatic.
 - performance improvement.
 - updated desc and README
 - en-US localization
 - added mod icon
 - fixed a ton of bug.

#### v1.3.1
 - Finaly released on the Mod Browser.
 - `Liquid` buckets can now be infinite.
 - Works with wires and actuators.

#### v1.3
 - Can now works with mods increasing max stacks and added related config items.
 - Furniture will now have the right category.

#### v1.2
 - Prevents item duppliation for infinite usables.
 - Added new categories.

#### v1.1
 - Custom categories and values introduced.
 - Added `set`, `category` and `values` commands.

#### v1.0
 - Consumables can now be infinite.