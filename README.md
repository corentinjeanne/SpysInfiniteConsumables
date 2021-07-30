# SpysInfiniteConsumables
This mod simply aims to make all consumables and tiles infinite once you have enought of them in your inventory.
It can also prevent duplication for most items.

### Categories
Consumables are split in different categories, with each one having a different customisable requirement.
You can correct / change te category of items in the config / via commands (i.g. a boss summer been categorised as 'other').
0. Blacklist: Non consumables items
1. Thrownm: Items of the throwing class - requires 1 stack to be infinite
1. Recovery: Healing and mana potions - requires 2 stacks
3. Buff: Potions providing a buff (swiftness, regenertion...) - requires 30 items
4. Ammo: Arrows, bullets, rockets and darts - - requires 4 stacks
5. SpecialAmmo: All other ammos - requires 1 stack
6. Summoner: Boss and event summoning items (does not work properly with modded items, you may want to set them up manualy) - requires 3 items
7. Critter and baits: Self expanatory - requires 10 items
8. Block -  - requires 1 stack
9. Furniture - requires 10 items
10. Wall - requires 1 stack
11. Liquid: Empty or full buckets (items with `bucket` in their name) - requires 10 items
12. Other: Consumables that dont fit in any category - requires 1 stack
13. Custom: You need to manualy set items and requirements for this category in the config or via the commands

### Mod Compatibility
Every modded Items from any mod shoud work fine with the mod, however their category could be wrong (mostly for boss summoner).
Mods increasing mas stack also work but they require a few steps to setup the mod correctly:
1. Disable any mod / config item increasing max stack. LEAVE CONTENT MODS ENABLE.
2. Initiale 'Generate Stack list' in the config', shouldn't take more than a few seconds. (doesn't do any this if 1 is not done)
3. Revert step 1 (enable mods/config items)

### Commands
They are a few in-game commads to help changing categories
- `/ID [name|type]`: Returns the Name and Type (id) of an item.
- `/SPIC category [name|type]`:  Return the category of an item.
- `/SPIC set <category> [name|type] [req(c=12)]`: Sets the category of an item. (with req as the requirement for custom categories).
- /SPIC values: return a list off all possible categories index and name.
`[name|type]`: the name or type of an item. Type `^` or skip it to use the item you hold
`[req(c=12)]`: the value of the requirement for custom categories.
`<category>`: a consumable category, can be its index or name.

### Config
###### General
Controls key aspects of the mods
###### Consumables and Tiles
Using a negative value for for a category requirement (`-X`) for items and a positive one stacks (-5 => 5 items, 2 => 2 stacks).
###### Custom Categories & values
Add items to the lists to change their cateogory.
The `Custom` category is not accesible from `Custom categories`, as all items in `Custom Requirement` will be in this category.
###### Mod Compatibility
See the `Mod Compatibility` section above to set the list.