using System.Reflection;
using Terraria.ModLoader.Config;

namespace SPIC.ConsumableGroup;

public static class CategoryHelper {

    public static string Label(this System.Enum category) {
        MemberInfo enumFieldMemberInfo = category.GetType().GetMember(category.ToString())[0];
        LabelAttribute? labelAttribute = (LabelAttribute?)System.Attribute.GetCustomAttribute(enumFieldMemberInfo, typeof(LabelAttribute));
        return labelAttribute?.Label ?? category.ToString();
    }
    public static string Label<TCategory>(TCategory category) where TCategory : System.Enum {
        MemberInfo enumFieldMemberInfo = typeof(TCategory).GetMember(category.ToString())[0];
        LabelAttribute? labelAttribute = (LabelAttribute?)System.Attribute.GetCustomAttribute(enumFieldMemberInfo, typeof(LabelAttribute));
        return labelAttribute?.Label ?? category.ToString();
    }

    public const byte None = 0;
    public const byte NotSupported = 254;
    public const byte Unknown = 255;

}
