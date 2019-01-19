using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using M11.Common.Attributes;

namespace M11.Common.Extentions
{
    public static class EnumExtentions
    {
        public static string GetDescription(this Enum value)
        {
            var enumMember = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            var descriptionAttribute =
                enumMember == null
                    ? default(DescriptionAttribute)
                    : enumMember.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute;
            return
                descriptionAttribute == null
                    ? value.ToString()
                    : descriptionAttribute.Description;
        }

        public static string GetFullDescription(this Enum value)
        {
            var enumMember = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            var descriptionAttribute =
                enumMember == null
                    ? default(FullDescriptionAttribute)
                    : enumMember.GetCustomAttribute(typeof(FullDescriptionAttribute)) as FullDescriptionAttribute;
            return
                descriptionAttribute == null
                    ? value.ToString()
                    : descriptionAttribute.FullDescription;
        }
    }
}
