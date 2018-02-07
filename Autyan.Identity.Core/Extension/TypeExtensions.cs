﻿using System;

namespace Autyan.Identity.Core.Extension
{
    public static class TypeExtensions
    {
        public static bool IsQueryTypes(this Type type)
        {
            if (type.IsPrimitive || type == typeof(string) || type.IsArray || type == typeof(DateTime)
                || type == typeof(DateTimeOffset)  || type == typeof(Enum))
            {
                return true;
            }

            var underlyingType = Nullable.GetUnderlyingType(type);
            return underlyingType?.IsPrimitive == true;
        }
    }
}
