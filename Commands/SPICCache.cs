
using Terraria.ModLoader;

namespace SPIC.Commands;

public class SPICChache : ModCommand {
    public override string Command => "spiccache";
    public override string Description => "Displays SPIC cache usage";
    public override string Usage => "/spiccache";
    public override CommandType Type => CommandType.Chat;

    public override void Action(CommandCaller caller, string input, string[] args) {
        if (args.Length != 0) return;
        string stats = InfinityManager.CacheStats();
        caller.Reply(stats);
    }
}