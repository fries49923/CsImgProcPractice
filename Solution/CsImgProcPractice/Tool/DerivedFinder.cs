using System.Reflection;

namespace CsImgProcPractice
{
    public class DerivedFinder
    {
        private readonly Type baseType;

        public DerivedFinder(Type type)
        {
            baseType = type
                ?? throw new ArgumentNullException(nameof(type));
        }

        /// <summary>
        /// Find type in assembly collection that implement and inherit a specific interface or abstract type.
        /// </summary>
        /// <param name="assemblies">The assembly collection</param>
        /// <returns>Array of eligible types.</returns>
        public Type[] FindType(IEnumerable<Assembly> assemblies)
        {
            var matchTypes = new List<Type>();
            foreach (var assembly in assemblies)
            {
                var types = FindType(assembly);
                matchTypes.AddRange(types);
            }

            return matchTypes.ToArray();
        }

        /// <summary>
        /// Find type in assembly that implement and inherit a specific interface or abstract type.
        /// </summary>
        /// <param name="assembly">The assembly</param>
        /// <returns>Array of eligible types.</returns>
        public Type[] FindType(Assembly assembly)
        {
            var matchTypes = new List<Type>();
            if (string.IsNullOrEmpty(baseType.FullName) is true
                || assembly is null)
            {
                return matchTypes.ToArray();
            }

            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                if (type.IsInterface
                    || type.IsAbstract)
                {
                    // No concrete
                    continue;
                }
                else if (baseType.IsAbstract
                    && GetAbstractClass(type, baseType.FullName) is not null)
                {
                    matchTypes.Add(type);
                }
                else if (baseType.IsInterface
                    && type.GetInterface(baseType.FullName) is not null)
                {
                    matchTypes.Add(type);
                }
            }

            return matchTypes.ToArray();
        }

        /// <summary>
        /// Searches for the abstract class with the specified name.
        /// </summary>
        /// <returns>
        /// An object representing the abstract class with the specified name, implemented or inherited by the current Type, if found; otherwise, null.
        /// </returns>
        public Type? GetAbstractClass(Type type, string name)
        {
            Type? baseType = type.BaseType;
            if (baseType is null)
            {
                return null;
            }

            if (baseType.IsAbstract
                && (baseType.Name == name || baseType.FullName == name))
            {
                return baseType;
            }
            else
            {
                return GetAbstractClass(baseType, name);
            }
        }
    }
}
