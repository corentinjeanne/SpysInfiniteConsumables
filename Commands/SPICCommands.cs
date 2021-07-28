using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ID;
namespace SPIC {

    public struct CommandValues {
        public string Identifier;
        public string Parameters;
        public string Usage => Identifier + " " + Parameters;
        public string Description;
        public CommandValues(string id, string parameters = "", string desc = ""){
            Identifier = id;
            Parameters = parameters;
            Description = desc;
        }
    }
    class SPICCommands : ModCommand {

        public override CommandType Type => CommandType.Chat;
        public override string Command => "spic";

        public override string Usage { 
            get {
                string text = "";
                foreach (CommandValues c in commands) text += c.Usage + '\n';
                return text;
            }
        }
        public override string Description { 
            get {
                string text = "";
                for (int i = 0; i < CommandCount; i++){
                    if(i != 0) text += '\n' + Command + "   ";
                    text += commands[i].Identifier + "   " + commands[i].Description;
                } 
                return text;
            }
        }

        public const int CommandCount = 3;
        public CommandValues[] commands = new CommandValues [CommandCount]{
            new CommandValues("category", "[name|type]", "Gives the category of an item (held if not specified)"),
            new CommandValues("set", "<category> [name|type] [req(c=12)]", "Sets the category of an item (held if not specified)"),
            new CommandValues("values", "", "Shows the possible categories")

        };

        public override bool Autoload(ref string name)
            => ModContent.GetInstance<ConsumableConfig>().commands;
        
        public override void Action(CommandCaller caller, string input, string[] args) {

            if(args.Length == 0){ // No commands called
                caller.Reply(Usage);
                return;
            }
            if(args[0] == commands[0].Identifier){
                CategoryCommand(caller, input, args);
                return;
            }
             if(args[0].ToLower() == commands[1].Identifier){
                 SetCommand(caller, input, args);
                 return;
            }
            if(args[0].ToLower() == commands[2].Identifier){
                ValueCommand(caller, input, args);
                return;
            }
            caller.Reply("Unknown command");
        }

        private bool ValueCommand(CommandCaller caller, string input, string[] args){
            string text = "Consumable Categories:";
            for (int i = 0; i < (int)ConsumableCategory.Custom+1; i++){
                text += '\n'+i.ToString() + $": {(ConsumableCategory)i}";
            }
            caller.Reply(text);
            return true;
        }

        private bool CategoryCommand(CommandCaller caller, string input, string[] args){
            int type = 0;
            if(args.Length == 1 || args[1] == "#") {
                type = caller.Player.HeldItem.type;
            } else {
                if(!int.TryParse(args[1], out type))
                    type = Utilities.NameToType(args[1]);
            }
            ConsumableCategory category = Utilities.GetCategory(type);
            caller.Reply($"Category of [i:{type}]: {(int)category}({category})");
            return true;
        }
        public static bool NeedSave = false;
        private bool SetCommand(CommandCaller caller, string input, string[] args){

            if(args.Length < 2){
                caller.Reply("Missing parameters");
                return false;
            }
            ConsumableCategory customCategory;
            if(int.TryParse(args[1], out int category))
                customCategory = (ConsumableCategory)category;
            else
                customCategory = Utilities.CategoryFromName(args[1]);

            int offset = 0;
            int requirement = 0;
            
            if(customCategory == ConsumableCategory.Custom && args.Length == 3 && int.TryParse(args[2], out int t))
                offset = 1;

            int type = 0;
            string itemName = "Unknown item";
            if(args.Length < 3+offset){
                type = caller.Player.HeldItem.type;
                itemName = caller.Player.HeldItem.Name;
            }else {
                itemName = args[2];
                if(!int.TryParse(args[2], out type)) // 3 args
                    type = Utilities.NameToType(args[2]);
            }
            if(customCategory == ConsumableCategory.Custom){
                if(args.Length >= 4-offset)
                    requirement = int.Parse(args[3-offset]);
                else {
                    caller.Reply("Missing parameters");
                    return false;
                }
            }
            ConsumableConfig config = ModContent.GetInstance<ConsumableConfig>();
            config.SetCustomConsumable(type, customCategory, requirement);
            NeedSave = true;
            caller.Reply($"Category of [i:{type}] set to {customCategory}");
            return true;
        }
    }

    public class AutoSave : ModWorld {

        // Save the config if a modification has benn made with those commands
        public override TagCompound Save(){
            if(!SPICCommands.NeedSave) return null;
            ModContent.GetInstance<ConsumableConfig>().Save();
            SPICCommands.NeedSave = false;
            return null;
        }
    }
}