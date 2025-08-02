using CeManualPatcher.Misc;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace CeManualPatcher.Patch
{
    public abstract class PatchBase<T> : IExposable where T : Def
    {
        protected string targetDefString = "null";
        private T targetDefInt;
        public T targetDef
        {
            get
            {
                if (targetDefInt == null)
                {
                    targetDefInt = DefDatabase<T>.GetNamed(targetDefString, false);
                }
                return targetDefInt;
            }
            set
            {
                targetDefString = value?.defName ?? "Null";
                targetDefInt = value;
            }


        }

        public abstract string PatchName { get; }
        
        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref targetDefString, "def", "Null");
        }
        public abstract void Reset();
        public abstract void PostLoadInit();
        public virtual void ExportPatch(string dirPath)
        {
            string folderPath = Path.Combine(dirPath, targetDef.modContentPack.PackageId);
            folderPath = Path.Combine(folderPath, PatchName);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, targetDef.defName + ".xml");

            XmlElement root = null;
            XmlDocument xmlDoc = XmlUtility.CreateBasePatchDoc(ref root, targetDef.modContentPack.Name);

            MakePatch(xmlDoc, root);

            xmlDoc.Save(filePath);
        }

        protected abstract void MakePatch(XmlDocument xmlDoc, XmlElement root);
    }
}
