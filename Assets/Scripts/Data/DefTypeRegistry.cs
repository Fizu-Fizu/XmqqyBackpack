using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XmqqyBackpack
{
    public static class DefTypeRegistry
    {
        private static Dictionary<string, Type> _types;
        private static bool _initialized;

        public static void Initialize()
        {
            if (_initialized) return;

            _types = new Dictionary<string, Type>();

            var defTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(ThingDef)) && !t.IsAbstract);

            foreach (var type in defTypes)
            {
                _types[type.Name] = type;
            }

            _initialized = true;
        }

        public static Type GetType(string typeName)
        {
            if (!_initialized) Initialize();

            _types.TryGetValue(typeName, out var type);
            return type;
        }

        public static IEnumerable<string> GetAllTypeNames()
        {
            if (!_initialized) Initialize();
            return _types.Keys;
        }
    }
}
