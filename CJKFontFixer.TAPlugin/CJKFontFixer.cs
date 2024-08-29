using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using ImGuiNET;
using MonoMod.RuntimeDetour;
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

    #endregion

    #region Extracted ClientAssets Fields

    private static Dictionary<float, ImFontPtr>? TerrariaFonts =>
        (Dictionary<float, ImFontPtr>)typeof(ClientAssets)
        .GetField("TerrariaFonts", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
    private static Dictionary<float, ImFontPtr>? MonospaceFonts =>
        (Dictionary<float, ImFontPtr>)typeof(ClientAssets)
        .GetField("MonospaceFonts", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;

    #endregion

    #region Detour

    private static Hook? LoadTerrariaFontHook;
    private static Hook? LoadMonospaceFontHook;

    #endregion

    #region Constructor / Load / Unload

    public CJKFontFixer(string path) : base(path)
    {

    }

    public override void Load()
    {
        Utils.ExtractFontFiles();
        LoadTerrariaFontHook = new Hook(
            () => ClientAssets.LoadTerrariaFont(default, default),
            () => LoadTerrariaFont(default, default));
        LoadMonospaceFontHook = new Hook(
            () => ClientAssets.LoadMonospaceFont(default, default),
            () => LoadMonospaceFont(default, default));
        
    }

    private int counter = 0;
    
    public override void Update()
    {
        if (counter <= 60)
            counter++;
        if (counter == 60)
        {
            ClientLoader.MainRenderer?.EnqueuePreDrawAction(() =>
            {
                TerrariaFonts!.Clear();
                MonospaceFonts!.Clear();
            
                ImGui.GetIO().Fonts.Clear();
                ImGui.GetIO().Fonts.AddFontDefault();
                ClientAssets.LoadFonts(ImGui.GetIO());
                ClientLoader.MainRenderer?.RebuildFontAtlas();
            });
        }
    }

    public override void Unload()
    {
        LoadTerrariaFontHook?.Undo();
        LoadTerrariaFontHook?.Dispose();
        LoadTerrariaFontHook = null;
        
        LoadMonospaceFontHook?.Undo();
        LoadMonospaceFontHook?.Dispose();
        LoadMonospaceFontHook = null;

    }

    #endregion


    #region MyRegion

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void LoadTerrariaFont(float size, bool withoutSymbols = false)
    {
        TerrariaFonts!.Add(size, ClientAssets.LoadFont(ClientAssets.TerrariaFontName, size, 0x0020, 0x007F));

        ClientAssets.LoadFont(ChineseFontName, size, true, Vector2.Zero, Vector2.Zero, 0f, float.MaxValue, 2f,
            0x0020, 0x00FF, // Basic Latin + Latin Supplement
            0x2000, 0x206F, // General Punctuation
            0x3000, 0x30FF, // CJK Symbols and Punctuations, Hiragana, Katakana
            0x31F0, 0x31FF, // Katakana Phonetic Extensions
            0xFF00, 0xFFEF, // Half-width characters
            0xFFFD, 0xFFFD, // Invalid
            0x4e00, 0x9FAF ); // CJK Ideograms

        if (!withoutSymbols)
        {
            ClientAssets.LoadFont(ClientAssets.IconFontName, size, true, new Vector2(0f, 4f), Icon.IconMin, Icon.IconMax);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void LoadMonospaceFont(float size, bool withoutSymbols = false)
    {
        MonospaceFonts!.Add(size, ClientAssets.LoadFont(ClientAssets.MonoFontName, size, 0x0020, 0x00FF, 0x0400, 0x04FF, 0x2020, 0x22FF));
        ClientAssets.LoadFont(ChineseFontName, size, true, Vector2.Zero, Vector2.Zero, 0f, float.MaxValue, 2f,
            0x0020, 0x00FF, // Basic Latin + Latin Supplement
            0x2000, 0x206F, // General Punctuation
            0x3000, 0x30FF, // CJK Symbols and Punctuations, Hiragana, Katakana
            0x31F0, 0x31FF, // Katakana Phonetic Extensions
            0xFF00, 0xFFEF, // Half-width characters
            0xFFFD, 0xFFFD, // Invalid
            0x4e00, 0x9FAF ); // CJK Ideograms

        if (!withoutSymbols)
        {
            ClientAssets.LoadFont(ClientAssets.IconFontName, size, true, new Vector2(0f, 4f), Icon.IconMin, Icon.IconMax);
        }
    }

    #endregion
}