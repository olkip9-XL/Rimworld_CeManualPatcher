using UnityEngine;
using Verse;

namespace CeManualPatcher.Manager
{
    public interface IManager : IExposable
    {
        void PostLoadInit();
        void ExportAll();
        void DoWindowContents(Rect rect);
    }
}
