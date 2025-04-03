using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CeManualPatcher.Extension
{
    public static class stringExtension
    {
        public static bool ContainsIgnoreCase(this string source, string target)
        {
            return source.IndexOf(target, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
