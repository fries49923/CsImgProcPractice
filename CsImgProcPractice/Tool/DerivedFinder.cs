using System;
using System.Collections.Generic;
using System.Reflection;

namespace CsImgProcPractice
{
    public class DerivedFinder
    {
        private Type baseType;

        public DerivedFinder(Type type)
        {
            this.baseType = type ?? throw new ArgumentNullException("type is null");
        }

        /// <summary>
        /// Find type in assembly collection that implement and inherit a specific interface or abstract type.
        /// </summary>
        /// <param name="assemblies">The assembly collection</param>
        /// <returns>Array of eligible types.</returns>
        public Type[] FindType(IEnumerable<Assembly> assemblies)
        {
            List<Type> matchTypes = new List<Type>();

            foreach (Assembly assembly in assemblies)
            {
                Type[] types = this.FindType(assembly);
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
            List<Type> matchTypes = new List<Type>();
            if (assembly == null)
            {
                matchTypes.ToArray();
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
                else if (this.baseType.IsAbstract
                    && GetAbstractClass(type, this.baseType.FullName) != null)
                {
                    matchTypes.Add(type);
                }
                else if (this.baseType.IsInterface
                    && type.GetInterface(this.baseType.FullName) != null)
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
        public Type GetAbstractClass(Type type, string name)
        {
            Type baseType = type.BaseType;

            if (baseType == null)
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
                return this.GetAbstractClass(baseType, name);
            }
        }
    }
}
