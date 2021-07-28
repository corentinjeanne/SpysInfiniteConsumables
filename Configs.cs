using System.Collections.Generic;
using System.ComponentModel;

using System.IO;
using Newtonsoft.Json;

using Terraria;

using Terraria.ModLoader;

using Terraria.ModLoader.Config;


namespace SPIC {

    public class CustomInfinity {

        public ItemDefinition Item;
        [JsonProperty]
        private ConsumableCategory _category;
        public ConsumableCategory Category {get => _category; set{
                if(value == ConsumableCategory.Custom) _category = ConsumableCategory.Other;
                else _category = value;
            }
        }

        public CustomInfinity(){}
        public CustomInfinity(ItemDefinition item, ConsumableCategory category){
            Item = item; Category = category;
        }
        public override string ToString() => $"{Item}: {Category}";
    }

    public class VeryCustomInfinity {

        public ItemDefinition Item;
        public int Requirement;
        public VeryCustomInfinity(){}
        public VeryCustomInfinity(ItemDefinition item, int requirement){
            Item = item; Requirement = requirement;
        }
        public override string ToString() => $"{Item}: {(Requirement > 0 ? $"{Requirement} stacks" : $"{-Requirement} items")}";
    }

    public class WierdStack {
        [JsonProperty]
        private ItemDefinition itemDefinition;
        [JsonProperty]
        private int stack;
        public WierdStack(){}
        public WierdStack(ItemDefinition item, int maxStack){
            itemDefinition = item; stack = maxStack;
        }
        public ItemDefinition Item() => itemDefinition;
        public int Stack() => stack;
        public override string ToString() => $"{itemDefinition}: {stack}/stack";
    }
    
    public class Button {}

    public class ConsumableConfig : ModConfig {

        public override ConfigScope Mode => ConfigScope.ClientSide;
        public static string CommandCustomPath {get; private set;}

        public override void OnLoaded() {
            CommandCustomPath = ConfigManager.ModConfigPath + @"\spic_ConsumableConfig.json";
        }
        public void Save(){
            using(StreamWriter sw = new StreamWriter(CommandCustomPath)){
                string serialisedConfig = JsonConvert.SerializeObject(this, ConfigManager.serializerSettings);
                sw.Write(serialisedConfig);
                // mod.Logger.Debug("Saved Config");
            }
        }


        [Header("General")]
        [DefaultValue(true), Label("[i:3104] Infinite Consumables")]
        public bool InfiniteConsumables;
        [Label("[i:3061] Infinite Tiles")]
        public bool InfiniteTiles;
        [DefaultValue(true), Label("[i:1293] Prevent item duplication"), Tooltip(
@"Tiles, walls and renewables projectiles won't drop their item if they are infinite
Furniture and critters above the required amount will be used
Buckets won't create empty or full buckets when used")]
        public bool PreventItemDupication;
        [DefaultValue(true), ReloadRequired, Label("[i:3617] Commands")]
        public bool commands;

        [Header("Consumables")]
        [Range(-999, 50), DefaultValue(1), Label("[i:279] Thrown weapons")]
        public int thrown;
        [DefaultValue(2), Label("[i:188] Recovery potions")]
        public int healingManaPotions;
        [Range(-999, 50), DefaultValue(-30), Label("[i:2347] Buff potions")]
        public int buffPotions;
        [Range(-999, 50), DefaultValue( 4), Label("[i:40] Basic ammuntions"), Tooltip("Arrows, bullets, darts & rockets")]
        public int ammunitions;
        [Range(-999, 50), DefaultValue(1), Label("[i:75] Special ammunitions")]
        public int specialAmmunitions;
        [Range(-999, 50), DefaultValue(-5), Label("[i:43] Boss & Event summoners")]
        public int summoning;
        [Range(-999, 50), DefaultValue(-10), Label("[i:2019] Criters & Baits ")]
        public int critters;
        [Range(-999, 50), DefaultValue(1), Label("[i:282] Other consumables")]
        public int others;


        [Header("Tiles")]
        [Range(-999, 50), DefaultValue(1), Label("[i:3] Blocks")]
        public int blocks;
        [Range(-999, 50), DefaultValue(-10), Label("[i:333] Furnitures")]
        public int furnitures;
        [Range(-999, 50), DefaultValue(1), Label("[i:132] Walls")]
        public int walls;
        [Range(-999, 50), DefaultValue(-10), Label("[i:206] Liquids")]
        public int liquids;


        [Header("Custom Categories & values")]
        [Label("[i:1913] Custom categories")]
        public List<CustomInfinity> customValues = new List<CustomInfinity>();
        [Label("[i:509] Custom Requirement"), Tooltip("Category 'Custom'")]
        public List<VeryCustomInfinity> customCustoms = new List<VeryCustomInfinity>();


        [Header("Mod Compatibility")]
        [Label("[i:306] Special Max stacks"), Tooltip(
@"Required if you use mods affecting max stacks
The mod will make assumptions about the stack size but they may be wrong
Generate it using the button bellow")]
        public List<WierdStack> wierdStacks = new List<WierdStack>();
        [Label("[i:538] Generate Stack list"), Tooltip("DISABLE any mod/config affecting max stacks before you generate the list")]
        public Button AutoDectectStack {
            get => null; set {if(value != null) AutoStack();}
        }

        private void AutoStack(){
            List<WierdStack> tempstacks = new List<WierdStack>();

            Item item = new Item();
            for (int i = 1; i < ItemLoader.ItemCount; i++){
                item.SetDefaults(i);
                if(item.maxStack > ConsumableStack.Max) return;
                ConsumableCategory category = Utilities.GetCategory(item, true);
                if(category != ConsumableCategory.Blacklist && item.maxStack != ConsumableStack.Assumption(category)) {
                    tempstacks.Add(new WierdStack(new ItemDefinition(item.type), item.maxStack));
                }
            }
            wierdStacks = tempstacks;

        }
        public void SetCustomConsumable (int itemType, ConsumableCategory category, int requirement = 0){
            SetCustomConsumable(new ItemDefinition(itemType), category, requirement);
        }
        public void SetCustomConsumable (ItemDefinition item, ConsumableCategory category, int requirement = 0){
            // mod.Logger.Debug($"Adding {item} to config");

            for (int i = customCustoms.Count - 1; i >= 0 ; i--){
                if(customCustoms[i].Item.Type == item.Type) customCustoms.RemoveAt(i);
            }
            for (int i = customValues.Count - 1; i >= 0 ; i--){
                if(customValues[i].Item.Type == item.Type) customValues.RemoveAt(i);
            }

            if(category == ConsumableCategory.Custom) customCustoms.Add(new VeryCustomInfinity(item, requirement));
            else customValues.Add(new CustomInfinity(item, category));
        }

    }
}