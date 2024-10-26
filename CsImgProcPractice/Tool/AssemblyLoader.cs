using System.IO;
using System.Reflection;

namespace CsImgProcPractice
{
    public class AssemblyLoader
    {
        private readonly string folder;

        public AssemblyLoader(string folder)
        {
            this.folder = folder
                ?? throw new ArgumentNullException(nameof(folder));
        }

        /// <summary>
        /// Load .net Assembly
        /// </summary>
        /// <returns>Assembly array</returns>
        public Assembly[] Load()
        {
            var assemblies = new List<Assembly>();

            if (Directory.Exists(folder) is false)
            {
                return assemblies.ToArray();
            }

            var files = Directory.GetFiles(folder, "*.dll");

            // Load Assembly from dll file
            foreach (var file in files)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(file);
                    assemblies.Add(assembly);
                }
                catch (Exception)
                { }
            }

            return assemblies.ToArray();
        }
    }
}
