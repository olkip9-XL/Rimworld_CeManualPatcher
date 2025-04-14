using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CeManualPatcher.Misc
{
    public static class PropUtility
    {
        public static object GetPropValue(object instance, string propName)
        {
            var field = instance.GetType().GetField(propName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                return field.GetValue(instance);
            }
            else
            {
                Log.Error($"Can't find {propName} in type {instance.GetType()}");
            }
            return null;
        }
        public static bool SetPropValue(object instance, string propName, object value)
        {
            var field = instance.GetType().GetField(propName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(instance, value);
                return true;
            }
            else
            {
                Log.Error($"Can't find {propName} in type {instance.GetType()}");
            }
            return false;
        }
        
        public static bool SetPropValueString(object instance, string propName, string valueString)
        {
            var field = instance.GetType().GetField(propName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                Type fieldType = field.FieldType;

                if(fieldType.IsValueType)
                {
                    object value;
                    try
                    {
                        value = Convert.ChangeType(valueString, fieldType);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Can't Set {valueString} to {fieldType.ToString()} in {propName}: {e}");
                        return false;
                    }

                    field.SetValue(instance, value);
                    return true;
                }
                else
                {
                    Log.Warning($"Can't Set {valueString} to {fieldType.ToString()} in {propName}");
                    return false;
                }
            }
            else
            {
                Log.Error($"Can't find {propName} in type {instance.GetType()}");
            }
            return false;
        }
        
        public static void CopyPropValue<T>(T source, T target)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "Source object cannot be null");
            }
            if(target == null)
            {
                throw new ArgumentNullException(nameof(target), "Target object cannot be null");
            }

            Type type = source.GetType();
            if (type != target.GetType())
            {
                throw new ArgumentException($"Source ({type.ToString()}) and target ({target.GetType().ToString()}) must be of the same type");
            }

            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object fieldValue = field.GetValue(source);
                field.SetValue(target, fieldValue);
            }
        }

        public static TChild ConvertToChild<TParent, TChild>(TParent parent) where TChild : TParent, new()
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent), "Parent object cannot be null");
            }

            TChild child = new TChild();

            Type parentType = typeof(TParent);

            foreach (PropertyInfo property in parentType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (property.CanRead && property.CanWrite)
                {
                    object value = property.GetValue(parent);
                    property.SetValue(child, value);
                }
            }

            foreach (FieldInfo field in parentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object value = field.GetValue(parent);
                field.SetValue(child, value);
            }

            return child;
        }

    }
}
