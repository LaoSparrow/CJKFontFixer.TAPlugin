using System.Numerics;
using System.Reflection;
using ImGuiNET;
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

    internal static class GlyphRanges
    {
        public static IntPtr ChineseFull => ImGui.GetIO().Fonts.GetGlyphRangesChineseFull();
        public static IntPtr Japanese => ImGui.GetIO().Fonts.GetGlyphRangesJapanese();
        public static IntPtr Korean => ImGui.GetIO().Fonts.GetGlyphRangesKorean();
    }
    
    internal static unsafe ImFontPtr LoadFont(string path, float size, bool merge, Vector2 glyphOffset, Vector2 glyphExtraSpacing, float minAdvanceX, float maxAdvanceX, float rasterizerMultiply, IntPtr glyphRanges)
    {
        ImFontConfigPtr config = ImGuiNative.ImFontConfig_ImFontConfig();
        ImFontPtr font = LoadFont(path, size, merge, glyphOffset, glyphExtraSpacing, minAdvanceX, maxAdvanceX, config.OversampleH, config.OversampleV, config.PixelSnapH, rasterizerMultiply, glyphRanges);
        config.Destroy();
        return font;
    }
    
    internal static unsafe ImFontPtr LoadFont(string path, float size, bool merge, Vector2 glyphOffset, Vector2 glyphExtraSpacing, float minAdvanceX, float maxAdvanceX, int overSampleH, int overSampleV, bool pixelSnapH, float rasterizerMultiply, IntPtr glyphRanges)
    {
        ImGuiIOPtr io = ImGui.GetIO();
        ImFontConfigPtr config = ImGuiNative.ImFontConfig_ImFontConfig();

        config.MergeMode = merge;
        config.GlyphOffset = glyphOffset;
        config.GlyphExtraSpacing = glyphExtraSpacing;
        config.GlyphMinAdvanceX = minAdvanceX;
        config.GlyphMaxAdvanceX = maxAdvanceX;
        config.OversampleH = overSampleH;
        config.OversampleV = overSampleV;
        config.PixelSnapH = pixelSnapH;
        config.RasterizerMultiply = rasterizerMultiply;
        
        ImFontPtr font = io.Fonts.AddFontFromFileTTF(path, size, config, glyphRanges);
        config.Destroy();
        return font;
    }
}