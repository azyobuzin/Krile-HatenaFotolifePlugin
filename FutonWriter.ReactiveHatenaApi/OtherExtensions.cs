using System;
using System.Collections.Generic;
using System.Linq;

namespace Azyobuzi.FutonWriter.ReactiveHatenaApi
{
    static class OtherExtensions
    {
        /// <summary>Escape RFC3986 String</summary>
        public static string UrlEncode(this string stringToEscape)
        {
            return Uri.EscapeDataString(stringToEscape)
                .Replace("!", "%21")
                .Replace("*", "%2A")
                .Replace("'", "%27")
                .Replace("(", "%28")
                .Replace(")", "%29");
        }

        public static bool IfRange<T>(this T source, params T[] targets)
        {
            return targets.Any(_ => EqualityComparer<T>.Default.Equals(source, _));
        }

        public static TResult Null<TIn, TResult>(this TIn source, Func<TIn, TResult> action, TResult @default = default(TResult))
        {
            return source != null
                ? action(source)
                : @default;
        }

        public static void Null<T>(this T source, Action<T> action, Action @default = null)
        {
            if (source != null)
                action(source);
            else if (@default != null)
                @default();
        }
    }
}
