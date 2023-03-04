using System;
using System.Collections.Generic;
using System.Text;

namespace System.Linq
{
    public static class LinqExpansion
    {
        public static string Aggregate(this IEnumerable<string> source, string separate)
        {
            if (source == null || source.Count() == 0) return "";
            return source.Aggregate((a, b) => a + separate + b);
        }
    }
}
