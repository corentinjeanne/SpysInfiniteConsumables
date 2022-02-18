using Terraria.ModLoader;

using SPIC.Config;

namespace SPIC.Commands {

	class SetCategory : ModCommand {

		public override CommandType Type => CommandType.Chat;
		public override string Command => "spic";

		public override string Usage => "/spic <category|req> [name|type]";
		public override string Description => "Set the category of an item.";

		public override bool IsLoadingEnabled(Mod mod) {
			return ModContent.GetInstance<ConsumableConfig>().Commands;
		}
		public override void Action(CommandCaller caller, string input, string[] args) {
			if(args.Length == 0) {
				caller.Reply(Usage);
				caller.Reply("Categories: ");
				for (int i = 0; i < ConsumableConfig.CategoryCount; i++) {
					caller.Reply($"c{i}: {(ConsumableConfig.Category)i}");
				}
				return;
			}
			ConsumableConfig config = ModContent.GetInstance<ConsumableConfig>();

			ConsumableConfig.Category? category = null;
			if (int.TryParse(args[0], out int req)) {
				if (req == 0) category = ConsumableConfig.Category.Blacklist;
			}
			else {
				if (args[0][0] == 'c' && int.TryParse(args[0][1..], out int c)) {
					category = (ConsumableConfig.Category)c;
				}
				else {
					category = ConsumableConfig.CategoryFromName(args[1]);
				}
			}

			int type;
			if (args.Length < 2) {
				type = caller.Player.HeldItem.type;
			}
			else {
				if (!int.TryParse(args[1], out type)) {
					type = Utility.NameToType(args[1]);
				}
			}

			if (!category.HasValue) {
				if (req < 0) req = Globals.SpicItem.MaxStack(type) * -req;
				config.InGameSetCustomCategory(type, req);
				caller.Reply($"Requirement of [i:{type}] set to {req} items");
			}
			else {
				config.InGameSetCustomCategory(type, category.Value/*, requirement*/);
				caller.Reply($"Category of [i:{type}] set to {category}");
			}
		}
	}
}