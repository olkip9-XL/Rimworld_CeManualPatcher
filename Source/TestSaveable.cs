using CombatExtended;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

using System.Reflection;

namespace CeManualPatcher
{
    public class TestSaveable : IExposable
    {
        public ThingDef thingDef
        {
            get=> DefDatabase<ThingDef>.GetNamed(defString);
            set=> defString = value.defName;
        }
        private string defString;

        public List<FieldInfoWarpper> verbWarppers = new List<FieldInfoWarpper>();
        private List<string> verbPropNames = new List<string>()
        {
            "recoilPattern",
            "ammoConsumedPerShotCount",
            "ejectsCasings"
        };

        public TestSaveable() { }

        public TestSaveable( ThingDef thingDef)
        {
            this.thingDef = thingDef;

            //verb  
            foreach (var propName in verbPropNames)
            {
                FieldInfoWarpper warpper = new FieldInfoWarpper(thingDef.Verbs[0] as VerbPropertiesCE, propName);
                verbWarppers.Add(warpper);

                PostLoadInit();
            }
        }


        public void ExposeData()
        {
            Scribe_Collections.Look(ref verbWarppers, "warpperList", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.LoadingVars && verbWarppers == null)
            {
                verbWarppers = new List<FieldInfoWarpper>();
            }
            Scribe_Values.Look(ref defString, "defString"); 
        }

        public void PostLoadInit()
        {
            foreach (var warpper in verbWarppers)
            {
                warpper.PostLoadInit(thingDef.Verbs[0] as VerbPropertiesCE);

                switch (warpper.fieldName)
                {
                    case "ammoConsumedPerShotCount":
                        warpper.LabelGetter = () => "MP_ammoConsumedPerShotCount".Translate();
                        break;
                    case "ejectsCasings":
                        warpper.LabelGetter = () => "MP_ejectsCasings".Translate();
                        break;
                    case "recoilPattern":
                        warpper.LabelGetter = () => "MP_recoilPattern".Translate();
                        warpper.ValueSetter = (instance) =>
                        {
                            List<RecoilPattern> list = Enum.GetValues(typeof(RecoilPattern)).Cast<RecoilPattern>().ToList();

                            FloatMenuUtility.MakeMenu(list,
                                (RecoilPattern x) => x.ToString(),
                                (RecoilPattern x) => delegate ()
                                {
                                    typeof(VerbPropertiesCE).GetField("recoilPattern", BindingFlags.Public | BindingFlags.Instance).SetValue(instance, x);
                                });
                        };
                        warpper.ValueLabelGetter = (obj) =>
                        {
                            RecoilPattern recoilPattern = (RecoilPattern)obj;
                            return recoilPattern.ToString() + "test";
                        };

                        break;
                }
            }

        }

    }
}
