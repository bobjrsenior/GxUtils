using System;
using System.Linq;
using System.ComponentModel;
using System.Reflection;
using System.Collections.Generic;

namespace LibGxFormat
{
    public static class EnumUtils
    {
        /// <summary>
        /// Get a description attribute of a member of an enumeration, or if it doesn't exist, the name of the enumeration member.
        /// </summary>
        /// <param name="value">A member from an enumeration.</param>
        /// <returns>The description attribute of the member of the enumeration, or otherwise, the name of the enumeration member.</returns>
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        public static string GetEnumFlagsString(Enum value)
        {
            IEnumerable<Enum> allFlags = Enum.GetValues(value.GetType()).Cast<Enum>();
            IEnumerable<Enum> flagsSet = allFlags.Where(f => value.HasFlag(f)).ToArray();
            return flagsSet.Any() ? string.Join(",", flagsSet.Select(f => f.ToString())) : "No flags";
        }
    }
}
