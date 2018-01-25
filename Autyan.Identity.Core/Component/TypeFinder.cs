using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Autyan.Identity.Core.Component
{
    public class TypeFinder
    {
        private TypeFinder() { }
        private IList<Assembly> _assemblies;

        public static TypeFinder SetScope(params Assembly[] assemblies)
        {
            return new TypeFinder {_assemblies = assemblies};
        }

        public IEnumerable<Type> Where(Func<Type, bool> selector)
        {
            var type = new List<Type>(0);
            foreach (var assemlby in _assemblies)
            {
                try
                {
                    type.AddRange(assemlby.GetTypes().Where(selector));
                }
                catch (ReflectionTypeLoadException e)
                {
                    type.AddRange(e.Types.Where(t => t != null && selector(t)));
                }

            }
            return type;
        }
    }
}
