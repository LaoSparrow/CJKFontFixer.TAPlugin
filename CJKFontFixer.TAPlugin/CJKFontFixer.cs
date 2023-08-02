using System.Numerics;
using System.Reflection;
using ImGuiNET;
using TerraAngel;
using TerraAngel.Assets;
using TerraAngel.Plugin;

namespace CJKFontFixer.TAPlugin;

// ReSharper disable once InconsistentNaming
// ReSharper disable once UnusedType.Global
public class CJKFontFixer : Plugin
{
    public override string Name => "CJK Font Fixer";

    #region Font File Names

    public static string ChineseFontName => $"{ClientLoader.AssetPath}/SourceHanSansSC-VF.ttf";
    // public static string JapaneseFontName => $"{ClientLoader.AssetPath}/SourceHanSans-VF.ttf";
    // public static string KoreanFontName => $"{ClientLoader.AssetPath}/SourceHanSansK-VF.ttf";

    #endregion

    #region Extracted ClientAssets Fields

    private static Dictionary<float, ImFontPtr>? TerrariaFonts;
    private static Dictionary<float, ImFontPtr>? MonospaceFonts;

    #endregion

    #region Constructor / Load / Unload

    public CJKFontFixer(string path) : base(path)
    {

    }

    public override void Load()
    {
        Utils.ExtractFontFiles();
    }

    private int counter;
    public override void Update()
    {
        if (counter <= 60)
            counter++;
        if (counter == 60)
        {
            TerrariaFonts = (Dictionary<float, ImFontPtr>)typeof(ClientAssets)
                .GetField("TerrariaFonts", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
            MonospaceFonts = (Dictionary<float, ImFontPtr>)typeof(ClientAssets)
                .GetField("MonospaceFonts", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
            ClientLoader.MainRenderer?.EnqueuePreDrawAction(() =>
            {
                TerrariaFonts.Clear();
                MonospaceFonts.Clear();
            
                ImGui.GetIO().Fonts.AddFontDefault();
                ReloadFonts();
                ClientLoader.MainRenderer?.RebuildFontAtlas();
            });
        }
    }

    #endregion

    #region Font Reloading

    public static void ReloadFonts()
    {
        foreach (var size in ClientAssets.DefaultSizes)
        {
            ReloadMonospaceFont(size);
            ReloadTerrariaFont(size);
        }
    }

    public static void ReloadTerrariaFont(float size, bool withoutSymbols = false)
    {
        TerrariaFonts!.Add(size, ClientAssets.LoadFont(ClientAssets.TerrariaFontName, size, 0x0020, 0x007F));
        // ClientAssets.LoadFont(ClientAssets.FallbackFontName, size, true, Vector2.Zero, Vector2.Zero, 0f, float.MaxValue, 2f, 0x0080, 0x00FF, 0x0400, 0x04FF, 0x2320, 0x2330, 0x2000, 0x2020);
        Utils.LoadFont(ChineseFontName, size, true, Vector2.Zero, Vector2.Zero, 0f, float.MaxValue, 2f, Utils.GlyphRanges.ChineseFull);
        // It seems that Simplified Chinese Font contains Japanese?
        // If you found any issue on displaying Japanese, just uncomment this line as well as line 96
        // Utils.LoadFont(JapaneseFontName, size, true, Vector2.Zero, Vector2.Zero, 0f, float.MaxValue, 2f, Utils.GlyphRanges.Japanese);
        // Utils.LoadFont(KoreanFontName, size, true, Vector2.Zero, Vector2.Zero, 0f, float.MaxValue, 2f, Utils.GlyphRanges.Korean);

        if (!withoutSymbols)
        {
            ClientAssets.LoadFont(ClientAssets.IconFontName, size, true, new Vector2(0f, 4f), Icon.IconMin, Icon.IconMax);
        }
    }
    
    public static void ReloadMonospaceFont(float size, bool withoutSymbols = false)
    {
        MonospaceFonts!.Add(size, ClientAssets.LoadFont(ClientAssets.MonoFontName, size, 0x0020, 0x00FF, 0x0400, 0x04FF, 0x2020, 0x22FF));
        // ClientAssets.LoadFont(ClientAssets.FallbackFontName, size, true, Vector2.Zero, Vector2.Zero, 0f, float.MaxValue, 2f, 0x2320, 0x2330, 0x2000, 0x2020);
        Utils.LoadFont(ChineseFontName, size, true, Vector2.Zero, Vector2.Zero, 0f, float.MaxValue, 2f, Utils.GlyphRanges.ChineseFull);
        // Utils.LoadFont(JapaneseFontName, size, true, Vector2.Zero, Vector2.Zero, 0f, float.MaxValue, 2f, Utils.GlyphRanges.Japanese);
        // Utils.LoadFont(KoreanFontName, size, true, Vector2.Zero, Vector2.Zero, 0f, float.MaxValue, 2f, Utils.GlyphRanges.Korean);

        if (!withoutSymbols)
        {
            ClientAssets.LoadFont(ClientAssets.IconFontName, size, true, new Vector2(0f, 4f), Icon.IconMin, Icon.IconMax);
        }
    }
    
    #endregion
}