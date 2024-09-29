using System.IO;
using SPIC.Default.Globals;
using SpikysLib;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SPIC.Packets;

public sealed class SetInfinite : ModPacketHandler {
    public static SetInfinite Instance = null!;

    public static ModPacket GetPacket(int x, int y, TileFlags tile, WireFlags wire) {
        ModPacket packet = Instance.GetPacket();
        packet.Write(x);
        packet.Write(y);
        packet.Write((byte)tile);
        packet.Write((byte)wire);
        return packet;
    }

    public override void Handle(BinaryReader reader, int fromWho) {
        int x = reader.ReadInt32();
        int y = reader.ReadInt32();
        TileFlags tile = (TileFlags)reader.ReadByte();
        WireFlags wire = (WireFlags)reader.ReadByte();

        if (tile != 0) InfiniteWorld.Instance.SetInfinite(x, y, tile);
        if (wire != 0) InfiniteWorld.Instance.SetInfinite(x, y, wire);
        if (Main.netMode == NetmodeID.Server) GetPacket(x, y, tile, wire).Send(-1, fromWho);
    }
}