using Terraria;
using Terraria.ModLoader;

namespace SPIC {

    public class ItemIDCommand : ModCommand {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "id";
        public override string Usage => "/id [name|type]";
        public override string Description => "Gives the ID and the name of an item";

        public override bool Autoload(ref string name)
            => ModContent.GetInstance<ConsumableConfig>().commands ? base.Autoload(ref name) : false;
        
        public override void Action(CommandCaller caller, string input, string[] args) {
            if(args.Length == 0 || args[0] == "#"){ // no args
                caller.Reply($"ID {caller.Player.HeldItem.type}: {caller.Player.HeldItem.Name}");
                return;
            }
            if(!int.TryParse(args[0], out int type)){ // args is a string
                string itemName = args[0].Replace('_', ' ');
                
                for (int i = 1; i < args.Length; i++) // fuse all the  args (if name has spaces)
                    itemName += ' ' + args[i];

                caller.Reply($"ID {Utilities.NameToType(itemName)}: {itemName}");
                return;
            } // args is a digit
            caller.Reply($"ID [i:{type}]: {type}");
        }
    }
}