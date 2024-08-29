using System.Reflection;
using TerraAngel.Plugin;

namespace AAAssemblyResolver.TAPlugin;

// ReSharper disable once InconsistentNaming
// ReSharper disable once UnusedType.Global
public class AAAssemblyResolver : Plugin
{
    public override string Name => "AAAssembly Resolver";
    
    public AAAssemblyResolver(string path) : base(path)
    {
    }

    public override void Load()
    {
        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
    }

    public override void Unload()
    {
        AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
    }
    
    private Assembly? OnAssemblyResolve(object? sender, ResolveEventArgs args)
    {
        var fileName = args.Name.Split(',')[0] + ".dll";
        var path = Path.Combine(Path.GetDirectoryName(PluginPath)!, fileName);
        if (File.Exists(path))
            return Assembly.LoadFrom(path);
        return null;
    }
    
    
}