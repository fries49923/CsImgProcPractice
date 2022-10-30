using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace CsImgProcPractice
{
    public class AssemblyLoader
    {
        private string folder;

        public AssemblyLoader(string folder)
        {
            this.folder = folder ?? throw new ArgumentNullException("folder is null");
        }

        /// <summary>
        /// Load .net Assembly
        /// </summary>
        /// <returns>Assembly array</returns>
        public Assembly[] Load()
        {
            List<Assembly> assemblies = new List<Assembly>();

            if (!Directory.Exists(this.folder))
            {
                return assemblies.ToArray();
            }

            string[] files = Directory.GetFiles(folder, "*.dll");

            // Load Assembly from dll file
            foreach (string file in files)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFile(file);
                    assemblies.Add(assembly);
                }
                catch (Exception)
                { }
            }

            return assemblies.ToArray();
        }
    }
}
