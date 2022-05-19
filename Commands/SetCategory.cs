// using Terraria.ModLoader;

// namespace SPIC.Commands {

//     class SetCategory : ModCommand {

//         public override CommandType Type => CommandType.Chat;
//         public override string Command => "spic";

//         public override string Usage => "/spic <consu> <category|req> [name|type]";
//         public override string Description => Terraria.Localization.Language.GetTextValue("Mods.SPIC.Commands.SetDesc");

//         public override bool IsLoadingEnabled(Mod mod) => Configs.ConsumableConfig.Instance.Commands;
//         public override void Action(CommandCaller caller, string input, string[] args) {
//             if (args.Length < 2) {
//                 caller.Reply(Usage);
//                 return;
//             }

//             int type;
//             if (args.Length == 2) {
//                 type = caller.Player.HeldItem.type;
//             }
//             else if (!int.TryParse(args[1], out type)) {
//                 type = Utility.NameToType(args[1]);
//             }

//             switch (args[0].ToLower()) {
//             case "c":
//                 AddCustom<Categories.Consumable>(type, args[1]);
//                 break;
//             case "a":
//                 AddCustom<Categories.Ammo>(type, args[1]);
//                 break;
//             case "b":
//                 AddCustom<Categories.GrabBag>(type, args[1]);
//                 break;
//             case "w":
//                 AddCustom<Categories.WandAmmo>(type, args[1]);
//                 break;
//             default:
//                 caller.Reply(Usage);
//                 return;
//             }
//         }

//         private static void AddCustom<T>(int type, string value) where T : struct, System.Enum {
//             Configs.ConsumableConfig config = Configs.ConsumableConfig.Instance;

//             Configs.CustomInfinity<T> custom = new();

//             if (int.TryParse(value, out int infinity)) custom.Infinity = infinity;
//             else {
//                 if (value[0] == '~' && int.TryParse(value[1..], out int category)) 
//                     custom.Category = (T)(object)category;
//                 else {
//                     custom.Category = System.Enum.Parse<T>(value, true);
//                 }
//             }
//             config.InGameSetCustom(type, custom);
//         }
//     }
// }