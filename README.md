# Spy's Infinite Consumables
![Spy's Infinite Consumables](icon_workshop.png)

## Make every consumable item infinite

Collect enough a consumable item to make it infinite!
Depending of the item, this requirement may vary.
There are a wide variety of [Infinities](#infinities) for all sorts of consumables (usable, placeable, materials and more) each with their own categories.

## Highly Customizable

Edit the [Infinity Settings](#infinity-settings) config to modify the behavior of the mod.
Toggle on/off certain Infinities, change the requirement of item or even set custom ones.

## No item duplication

Tiles, NPCs or projectiles created with an infinite consumable wont drop their items on death.

## Easily see the Infinities of an item

The Infinities of every item can be displayed in various ways:

- With colored tooltip lines. This can also be used to display extra information about the item such as its requirement or category.
- With a glow around the item. Note that it can sometimes be hard to see.
- With colored dot in the inventory slot.

Edit the [Infinities Display](#infinity-display) config to change which one is active, display additional information or change the color of the Infinities.

## Automatically detect the category of almost every item

If an item's category cannot be determined with the items's stats, it will be determined when the item is used!
This should work for almost every summoner, player booster and more.
If this fails, you can always set it via a custom requirement.

## Infinities

Infinities are grouped into Groups depending of the type of consumable they affect.
You can find below all vanilla Infinities their respective `Categories`.
Note than a consumable can be in includes in multiple Groups and Infinities.


### Item Infinities

Consumable items will have a requirement in one or more of the infinities bellow depending of its use.

#### Usable (Items consumed upon use)
- `Weapon`: Deals damage.
- `Potion`: Heals life, mana or provides a buff (swiftness, archery...).
- `Booster`: Permanently change to the player (live, mana...) or the world
- `Summoner`: Summons a boss or an event.
- `Critter`: Summons a critter or is a bait.
- `Tool`: Has a miscellaneous use (move the player, light an area, ...).
- `Tool?`: Is one of the above but not yet determined.

#### Ammunition (Items used as ammunitionw)
- `Basic`: Arrows, bullets, rockets and darts.
- `Special`: All other ammunition.

#### Placeable (Items placing a tile)
- `Tile`: Places a block or a wall.
- `Torch`: Places a torch.
- `Ore`: Raw minerals.
- `Furniture`: Places a background tile
- `Wiring`: Wires.
- `Mechanical`: Can be triggered with a wire.
- `Bucket`: Places or remove liquid.
- `Seed`: Grows into a plant.
- `Paint`: Color walls and tiles.

#### Grab bag (Items giving various items when opened)
- `Container`: Can be opened.
- `Extractinator`: Can be extractinated
- `Treasure Bag`: Dropped form a boss in expert mode or above.

#### Materials (Items used for crafting)
- `Basic`: Blocks, common items.
- `Ore`: Turn into bars.
- `Furniture`: All other placeables
- `Miscellaneous`: Only a material.
- `Non Stackable`: Weapons, armors, etc...

#### Journey Sacrifices (Items that can be researched)

### Currency Infinities

Items used as currency are classified as one of the following:
- `Coins`: Currencies with more than one items.
- `Single coin`: Currencies with a single item.

They will also have requirements in the following infinities: 
- **Shop**
- **Nurse**
- **Reforging**
- **Other Purchases**

## Configs

### Infinity Settings

This is the main config of the mod (server side).

- `Features` Controls specific aspects of the mod
- `Infinity Configs`: Contains the config of every type of consumable and its infinities

### Infinity Display

This config contains various ways to display the infinities of items and their display config.

- `General`: Determines what is displayed.
- `Displays`: Controls enabled infinity displays and their individual config.
- `Infinity Configs`: The display config of each Infinity.
- `Performances`: Options to improve the performance of the mod.

## Changelog
See [Changelog](Changelog.md)