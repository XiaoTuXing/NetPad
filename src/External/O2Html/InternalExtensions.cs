using System;
using System.Collections.Generic;
using System.Linq;

namespace O2Html;

internal static class InternalExtensions
{
    public static bool In<T>(this T item, params T[] collection)
    {
        return collection.Contains(item);
    }

    public static string ReplaceIfExists(this string source, string strToReplace, string replaceWith)
    {
        if (source.Contains(strToReplace))
            return source.Replace(strToReplace, replaceWith);

        return source;
    }

    public static string GetReadableName(this Type type, bool withNamespace = false, bool forHtml = false)
    {
        string name = type.FullName ?? type.Name;

        if (type.GenericTypeArguments.Length > 0)
        {
            name = name.Split('`')[0];
            name += "<";

            foreach (var tArg in type.GenericTypeArguments)
            {
                name += GetReadableName(tArg, withNamespace, forHtml) + ", ";
            }

            name = name.TrimEnd(' ', ',') + ">";
        }

        if (!withNamespace)
        {
            string typeNamespace = type.Namespace ?? string.Empty;

            if (typeNamespace.Length > 1 && name.StartsWith(typeNamespace))
                name = name.Substring(typeNamespace.Length + 1); // +1 to trim the '.' after the namespace
        }

        if (forHtml)
        {
            name = name
                .ReplaceIfExists("<", "&lt;")
                .ReplaceIfExists(">", "&gt;");
        }

        if (type.FullName?.StartsWith("System.Nullable`") == true)
        {
            int iStart = withNamespace ? "System.Nullable<".Length : "Nullable<".Length;
            name = name.Substring(iStart, name.Length - iStart) + "?";
        }

        return name;
    }

    internal static Type? GetCollectionElementType(this Type collectionType)
    {
        Type? elementType;

        if (collectionType.IsArray)
        {
            elementType = collectionType.GetElementType();
        }
        else
        {
            Type? iEnumerable = FindIEnumerable(collectionType);

            if (iEnumerable == null)
            {
                return typeof(object);
            }

            elementType = iEnumerable.GetGenericArguments()[0];
        }

        return elementType;
    }

    private static Type? FindIEnumerable(Type collectionType)
    {
        if (collectionType == typeof(string))
        {
            return null;
        }

        if (collectionType.IsGenericType)
        {
            foreach (Type arg in collectionType.GetGenericArguments())
            {
                Type iEnumerable = typeof(IEnumerable<>).MakeGenericType(arg);
                if (iEnumerable.IsAssignableFrom(collectionType))
                {
                    return iEnumerable;
                }
            }
        }

        Type[] interfaces = collectionType.GetInterfaces();

        foreach (Type iFace in interfaces)
        {
            Type? iEnumerable = FindIEnumerable(iFace);
            if (iEnumerable != null) return iEnumerable;
        }

        if (collectionType.BaseType != null && collectionType.BaseType != typeof(object))
        {
            return FindIEnumerable(collectionType.BaseType);
        }

        return null;
    }
}
