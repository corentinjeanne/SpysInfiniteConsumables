using SPIC.Config;
using Terraria;
using Terraria.ModLoader;

namespace SPIC.Commands {

	public class ItemID : ModCommand {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "id";
        public override string Usage => "/id [name|type]";
        public override string Description => "Gives the ID and the name of an item.";

        public override bool IsLoadingEnabled(Mod mod) {
            return ModContent.GetInstance<ConsumableConfig>().Commands;
        }
        public override void Action(CommandCaller caller, string input, string[] args) {
            int type;
            string name;
            if (args.Length == 0 || args[0] == "#") { // no args
                type = caller.Player.HeldItem.type;
                name = caller.Player.HeldItem.Name;
            }
            else if (int.TryParse(args[0], out type)) { // args is a string
                Item item = new Item(type);
                name = item.Name;
            }
            else {
                name = args[0];
                type = Utility.NameToType(args[0]);
            }
            caller.Reply($"ItemID {type}: [i:{type}] {name}");            
        }
    }
}