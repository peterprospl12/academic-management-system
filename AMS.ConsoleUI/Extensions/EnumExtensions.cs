using System.ComponentModel;
using System.Reflection;

namespace AMS.ConsoleUI.Extensions;

public static class EnumExtensions
{
    public static string ToDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<DescriptionAttribute>();

        return attribute?.Description ?? value.ToString();
    }
}