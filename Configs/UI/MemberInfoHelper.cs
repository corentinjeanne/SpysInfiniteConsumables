
using System;
using System.Reflection;
using Terraria.ModLoader.Config.UI;

namespace SPIC.Configs.UI {

    public static class ConfigReflectionHelper {

        public static readonly Assembly tModLoader = Assembly.Load("tModLoader");
        public static readonly Type ObjectElement = tModLoader.GetType("Terraria.ModLoader.Config.UI.ObjectElement");
        public static readonly Type UIModConfig = tModLoader.GetType("Terraria.ModLoader.Config.UI.UIModConfig");
        public static readonly PropertyInfo UIModConfig_Tooltip = UIModConfig.GetProperty("Tooltip", BindingFlags.Static | BindingFlags.Public);
        public static readonly PropertyInfo ConfigElement_TextDisplayFunction = typeof(ConfigElement).GetProperty("TextDisplayFunction", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly PropertyInfo ConfigElement_DrawLabel = typeof(ConfigElement).GetProperty("DrawLabel", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly FieldInfo ConfigElement_backgroundColor = typeof(ConfigElement).GetField("backgroundColor", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly FieldInfo ObjectElement_pendindChanges = ObjectElement.GetField("pendingChanges", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly FieldInfo ObjectElement_expanded = ObjectElement.GetField("expanded", BindingFlags.Instance | BindingFlags.NonPublic);

    }
}