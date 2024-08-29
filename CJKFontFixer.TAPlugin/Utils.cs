using System.Reflection;
using TerraAngel;

namespace CJKFontFixer.TAPlugin;

internal static class Utils
{
    internal static void ExtractFontFiles()
    {
        var asm = Assembly.GetExecutingAssembly();
        var fontFiles = asm
            .GetManifestResourceNames()
            .Where(x => x.EndsWith(".ttf"))
            .Select(x => new
            {
                FileName = x,
                Stream = asm.GetManifestResourceStream(x)
            });
        foreach (var x in fontFiles)
        {
            var fileName = x.FileName.Substring("CJKFontFixer.TAPlugin.Resources.".Length,
                x.FileName.Length - "CJKFontFixer.TAPlugin.Resources.".Length);
            var path = $"{ClientLoader.AssetPath}/{fileName}";
            if (File.Exists(path))
                continue;
            
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            x.Stream!.CopyTo(fs);
        }
    }
    
}