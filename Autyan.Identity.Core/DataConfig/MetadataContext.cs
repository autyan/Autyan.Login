using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autyan.Identity.Core.Component;
using Autyan.Identity.Core.Data;
using Humanizer;

namespace Autyan.Identity.Core.DataConfig
{
    public class MetadataContext
    {
        private MetadataContext()
        {

        }

        private static readonly IDictionary<Type, DatabaseModelMetadata> MetadataMapping = new Dictionary<Type, DatabaseModelMetadata>();

        private static readonly Type[] DatabaseTypes = {
            typeof (int), typeof (long), typeof (byte), typeof (bool), typeof (short), typeof (string),typeof(decimal),
            typeof (int?), typeof (long?), typeof (byte?), typeof (bool?), typeof (short?),typeof(decimal?),
            typeof (DateTime),
            typeof (DateTime?)
        };

        public static MetadataContext Instance { get; } = new MetadataContext();

        public DatabaseModelMetadata this[Type type] => MetadataMapping.ContainsKey(type) ? MetadataMapping[type] : null;

        public void Initilize(Assembly[] assemblies)
        {
            var finder = TypeFinder.SetScope(assemblies);
            foreach (var type in finder.Where(t => !t.IsAbstract && t.IsClass && t.BaseType == typeof(BaseEntity)))
            {
                MetadataMapping[type] = new DatabaseModelMetadata
                {
                    TableName = type.Name.Pluralize(),
                    Columns = type.GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)
                        .Where(p => DatabaseTypes.Contains(p.PropertyType)).Select(p => p.Name).ToList()
                };
            }
        }
    }
}
