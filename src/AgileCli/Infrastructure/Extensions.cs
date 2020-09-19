using System;

namespace AgileCli.Infrastructure
{
    public static class Extensions
    {
        public static T ToEnum<T>(this string value) where T : Enum => (T)Enum.Parse(typeof(T), value, true);
    }
}
