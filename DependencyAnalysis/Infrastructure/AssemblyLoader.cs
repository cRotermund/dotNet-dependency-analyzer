using System.Reflection;
using System.Runtime.InteropServices;

namespace DependencyAnalysis.Infrastructure
{
    public class AssemblyLoader : IDisposable 
    {
        private readonly string _binPath;
        private readonly MetadataLoadContext _context;
        private readonly Dictionary<string, Assembly?> _assemblyCache;

        //This constructor will use the current directory of the running application as the default resolver
        public AssemblyLoader() : this(Directory.GetCurrentDirectory()) {}
        
        public AssemblyLoader(string binPath)
        {
            _binPath = binPath;
            _assemblyCache = new ();
            
            //This is kind of a hack, but we need to try and avoid collisions.  Leverage assemblies on the bin path 
            //and don't load runtime assemblies into the resolver if they share the same name...  I think there is 
            //probably a better way to do this.  Use a last-in wins algorithm, handling the target bin path second
            Dictionary<string, string> pathByName = new();
            
            List<string> allPaths = new();
            allPaths.AddRange(Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll"));
            allPaths.AddRange(Directory.GetFiles(_binPath, "*.dll"));
            
            foreach (var assemblyPath in allPaths)
            {
                var simpleName = Path.GetFileNameWithoutExtension(assemblyPath);
                pathByName[simpleName] = assemblyPath;
            }
            
            var resolver = new PathAssemblyResolver(pathByName.Values);
            _context = new MetadataLoadContext(resolver);
        }

        public Assembly? Load(AssemblyName assemblyName)
        {
            Assembly? _loaded = null;

            if (_assemblyCache.TryGetValue(assemblyName.Name, out _loaded))
            {
                return _loaded;
            }
            
            try
            {
                _loaded = _context.LoadFromAssemblyName(assemblyName);
            }
            catch (Exception)
            {
                _loaded = null;
            }

            _assemblyCache.Add(assemblyName.Name, _loaded);
            return _loaded;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}