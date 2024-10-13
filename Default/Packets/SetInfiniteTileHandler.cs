using System.IO;
using SPIC.Default.Globals;
using SpikysLib;
using Terraria.ModLoader;

namespace SPIC.Default.Packets;

public sealed class SetInfiniteTileHandler : ModPacketHandler {
    public static SetInfiniteTileHandler Instance = null!;

    public static ModPacket GetPacket(int x, int y, TileFlags tile) => GetPacket(x, y, true, (byte)tile);
    public static ModPacket GetPacket(int x, int y, WireFlags wire) => GetPacket(x, y, false, (byte)wire);
    private static ModPacket GetPacket(int x, int y, bool tile, byte flags) {
        ModPacket packet = Instance.GetPacket();
        packet.Write(x);
        packet.Write(y);
        packet.Write((byte)(tile ? flags | 128 : flags));
        return packet;
    }

    public override void Handle(BinaryReader reader, int fromWho) {
        int x = reader.ReadInt32();
        int y = reader.ReadInt32();
        byte flags = reader.ReadByte();
        if ((flags & 128) != 0) InfiniteWorld.Instance.SetInfinite(x, y, (TileFlags)flags & TileFlags.All);
        else InfiniteWorld.Instance.SetInfinite(x, y, (WireFlags)flags);
    }
}