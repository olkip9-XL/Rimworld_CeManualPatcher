using CeManualPatcher.Saveable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Extension
{
    internal static class List_ToolCESaveableExtension
    {
        internal static ToolCESaveable GetById(this List<ToolCESaveable> tools, string id)
        {
            ToolCESaveable tool = tools.FirstOrDefault(t => t.id == id);

            if(tool == null)
            {
                Log.Error($"Tool with id {id} not found");
            }

            return tool;
        }
    }
}
