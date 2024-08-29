using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using ImeFixer.TAPlugin.Windows;
using MonoMod.RuntimeDetour;
using ReLogic.Localization.IME;
using ReLogic.OS;
using ReLogic.OS.FNA;
using SDL2;
using TerraAngel;
using TerraAngel.Plugin;
using Terraria;

namespace ImeFixer.TAPlugin;

// ReSharper disable once InconsistentNaming
// ReSharper disable once UnusedType.Global
public class ImeFixer : Plugin
{
    public override string Name => "Ime Fixer";

    public WindowsMessageHook? _wndProcHook;
    public Hook? DrawWindowsIMEPanelHook;

    public ImeFixer(string path) : base(path)
    {
    }

    public override void Load()
    {
        if (!Platform.IsWindows)
            return;

        DrawWindowsIMEPanelHook ??= new Hook(
            () => Main.instance.DrawWindowsIMEPanel(default, default),
            () => OnDrawWindowsIMEPanel(default!, default, default));
        
        SDL.SDL_SysWMinfo info = default(SDL.SDL_SysWMinfo);
        SDL.SDL_VERSION(out info.version);
        SDL.SDL_GetWindowWMInfo(Main.instance.Window.Handle, ref info);
        var windowHandle = info.info.win.window;
        
        _wndProcHook ??= new WindowsMessageHook(windowHandle);

        List<Action<char>> keyPressCallbacks = (List<Action<char>>)typeof(PlatformIme)
            .GetField("_keyPressCallbacks", BindingFlags.Instance | BindingFlags.NonPublic)
            !.GetValue(Platform.Get<IImeService>())!;
        
        typeof(Platform).GetMethod("RegisterService", BindingFlags.Instance | BindingFlags.NonPublic)
            !.MakeGenericMethod(typeof(IImeService)).Invoke(Platform.Current, new object?[] { new WinImm32Ime(_wndProcHook, windowHandle) });

        foreach (var c in keyPressCallbacks)
        {
            Platform.Get<IImeService>().AddKeyListener(c);
        }
        
        var imeService = Platform.Get<IImeService>();
        if (!imeService.IsEnabled)
            imeService.Enable();
        
        ClientLoader.MainRenderer?.AddWindow(new ImeWindow());
    }

    public override void Unload()
    {
        if (!Platform.IsWindows)
            return;
        
        var window = ClientLoader.MainRenderer?.ClientWindows.Find(x => x is ImeWindow);
        if (window != null)
            ClientLoader.MainRenderer?.RemoveWindow(window);
        
        List<Action<char>> keyPressCallbacks = (List<Action<char>>)typeof(PlatformIme)
            .GetField("_keyPressCallbacks", BindingFlags.Instance | BindingFlags.NonPublic)
            !.GetValue(Platform.Get<IImeService>())!;
        
        typeof(Platform).GetMethod("RegisterService", BindingFlags.Instance | BindingFlags.NonPublic)
            !.MakeGenericMethod(typeof(IImeService)).Invoke(Platform.Current, new object?[] { new FNAIme() });
        
        foreach (var c in keyPressCallbacks)
        {
            Platform.Get<IImeService>().AddKeyListener(c);
        }

        if (_wndProcHook != null)
        {
            _wndProcHook.Dispose();
            _wndProcHook = null;
        }

        if (DrawWindowsIMEPanelHook != null)
        {
            DrawWindowsIMEPanelHook.Undo();
            DrawWindowsIMEPanelHook.Dispose();
            DrawWindowsIMEPanelHook = null;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void OnDrawWindowsIMEPanel(Main self, Vector2 position, float xAnchor = 0f)
    {
        
    }
}
