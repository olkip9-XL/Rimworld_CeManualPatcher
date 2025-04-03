using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher
{
    public class FieldInfoWarpper : IExposable
    {
        public string fieldName;
        public float fieldWidth = 100f;
        public object newValue = null;
        private object originalValue = null;
        public object instance { get; private set; }

        public Func<string> LabelGetter;
        public Action<object> ValueSetter;
        public Func<object,string> ValueLabelGetter;

        private Type instantType;
        private FieldInfo fieldInfo;
        public string Label
        {
            get
            {
                if (LabelGetter != null)
                {
                    return LabelGetter();
                }
                else
                {
                    return fieldName;
                }
            }
        }
        public Type FieldType => instantType.GetField(fieldName).FieldType;

        public object Value => instantType.GetField(fieldName).GetValue(instance);

        public FieldInfoWarpper() { }
        public FieldInfoWarpper(object instance, string fieldName)
        {
            this.fieldName = fieldName;
            InitData(instance);
        }
        public void ExposeData()
        {
            if(Scribe.mode == LoadSaveMode.Saving)
            {
                newValue = fieldInfo.GetValue(instance);
            }

            Scribe_Values.Look(ref fieldName, "fieldName");
            Scribe_Values.Look(ref fieldWidth, "fieldWidth");
            Scribe_Values.Look(ref newValue, "newValue");
        }
        public void PostLoadInit(object instance)
        {
           InitData(instance);
        }

        public void Apply()
        {
            if (newValue != null)
            {
                fieldInfo.SetValue(instance, newValue);
            }
        }

        public void Reset()
        {
            if (originalValue != null)
            {
                fieldInfo.SetValue(instance, originalValue);
            }
        }

        private void InitData(object instance)
        {
            this.instance = instance;
            this.instantType = instance.GetType();

            this.fieldInfo = instantType.GetField(fieldName);

            if (fieldInfo == null)
            {
                throw new ArgumentException($"Field '{fieldName}' not found in type '{instantType.Name}'.");
            }

            if (ValueSetter == null)
            {
                ValueSetter = (value) =>
                {
                    if (instance == value) return;
                    fieldInfo.SetValue(instance, value);
                };
            }

            if (LabelGetter == null)
            {
                LabelGetter = () =>
                {
                    return fieldInfo.Name;
                };
            }

            if(ValueLabelGetter == null)
            {
                ValueLabelGetter = (value) =>
                {
                    return Value.ToString();
                };
            }

            this.originalValue = fieldInfo.GetValue(instance);
        }
    }
}
