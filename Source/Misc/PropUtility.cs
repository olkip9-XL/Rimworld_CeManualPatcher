using RimWorld;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Verse;

namespace CeManualPatcher.Misc
{
    public static class PropUtility
    {

        public static readonly BindingFlags all = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty;

        public static object GetPropValue(object instance, string propName)
        {
            //var field = instance.GetType().GetField(propName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            FieldInfo field = Field(instance.GetType(), propName);
            if (field != null)
            {
                return field.GetValue(instance);
            }
            return null;
        }
        public static bool SetPropValue(object instance, string propName, object value)
        {
            FieldInfo field = Field(instance.GetType(), propName);
            if (field != null)
            {
                field.SetValue(instance, value);
                return true;
            }
            return false;
        }

        public static string GetPropValueString(object instance, string propName)
        {
            FieldInfo field = Field(instance.GetType(), propName);
            if (field != null)
            {
                object value = field.GetValue(instance);
                if (value != null)
                {
                    if (value is Def def)
                    {
                        return def.defName;
                    }
                    else
                    {
                        return value.ToString();
                    }
                }
            }
            return null;
        }

        public static bool SetPropValueString(object instance, string propName, string valueString)
        {
            FieldInfo field = Field(instance.GetType(), propName);



            if (field != null)
            {
                if (string.IsNullOrEmpty(valueString) || valueString == "null")
                {
                    field.SetValue(instance, null);
                    return true;
                }

                Type fieldType = field.FieldType;

                if (typeof(Def).IsAssignableFrom(fieldType))
                {
                    Type dbType = typeof(DefDatabase<>).MakeGenericType(fieldType);
                    MethodInfo mi = dbType.GetMethod("GetNamed", new[] { typeof(string), typeof(bool) });
                    Def def = (Def)mi.Invoke(null, new object[] { valueString, false });
                    if (def != null)
                    {
                        field.SetValue(instance, def);
                        return true;
                    }
                }
                else if (fieldType.IsEnum ||
                    (fieldType.IsGenericType
                      && fieldType.GetGenericTypeDefinition() == typeof(Nullable<>)
                      && fieldType.GetGenericArguments()[0].IsEnum))
                {
                    try
                    {
                        object enumValue = Enum.Parse(fieldType.IsGenericType ? fieldType.GetGenericArguments()[0] : fieldType, valueString);
                        field.SetValue(instance, enumValue);
                        return true;
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Can't Set enum {valueString} to {fieldType.ToString()} in {propName}: {e}");
                        return false;
                    }
                }
                else if (fieldType.IsValueType)
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
                    Log.Error($"Unsupported type {fieldType.ToString()} : {propName}");
                    return false;
                }
            }
            return false;
        }

        public static void CopyPropValue<T>(T source, T target)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "Source object cannot be null");
            }
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target), "Target object cannot be null");
            }

            //Check type
            Type type = typeof(T);
            if (type.IsValueType)
            {
                throw new ArgumentException("Source and target must be reference types", nameof(type));
            }

            if (!type.IsInstanceOfType(target))
            {
                throw new ArgumentException($"Target must be of type {type.ToString()}", nameof(target));
            }

            if (!type.IsInstanceOfType(source))
            {
                throw new ArgumentException($"Source must be of type {type.ToString()}", nameof(source));
            }

            // Copy properties
            while (type != null && type != typeof(object))
            {
                foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    object fieldValue = field.GetValue(source);
                    field.SetValue(target, fieldValue);
                }
                type = type.BaseType;
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


        private static FieldInfo Field(Type type, string fieldName)
        {
            if ((object)type == null || string.IsNullOrEmpty(fieldName))
            {
                return null;
            }

            FieldInfo fieldInfo = FindIncludingBaseTypes(type, (Type t) => t.GetField(fieldName, all));
            if ((object)fieldInfo == null)
            {
                Log.Error($"Can't find {fieldName} in type {type}");
            }

            return fieldInfo;
        }

        private static T FindIncludingBaseTypes<T>(Type type, Func<Type, T> func) where T : class
        {
            do
            {
                T val = func(type);
                if (val != null)
                {
                    return val;
                }

                type = type.BaseType;
            }
            while ((object)type != null);
            return null;
        }


    }
}
