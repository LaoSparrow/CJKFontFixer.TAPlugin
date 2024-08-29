using System.Runtime.InteropServices;
using ImeFixer.TAPlugin.WinImm32;

namespace ImeFixer.TAPlugin.Windows;

public class WindowsMessageHook : IDisposable
{
    private delegate nint WndProcCallback(nint hWnd, int msg, nint wParam, nint lParam);

    private const int GWL_WNDPROC = -4;

    private nint _windowHandle = IntPtr.Zero;

    private nint _previousWndProc = IntPtr.Zero;

    private WndProcCallback _wndProc;

    private List<IMessageFilter> _filters = new List<IMessageFilter>();

    private bool disposedValue;

    public WindowsMessageHook(nint windowHandle)
    {
        _windowHandle = windowHandle;
        _wndProc = WndProc;
        _previousWndProc = NativeMethods.SetWindowLongPtr(_windowHandle, -4, Marshal.GetFunctionPointerForDelegate((Delegate)_wndProc));
    }

    public void AddMessageFilter(IMessageFilter filter)
    {
        _filters.Add(filter);
    }

    public void RemoveMessageFilter(IMessageFilter filter)
    {
        _filters.Remove(filter);
    }

    private nint WndProc(nint hWnd, int msg, nint wParam, nint lParam)
    {
        Message message = Message.Create(hWnd, msg, wParam, lParam);
        if (InternalWndProc(ref message))
        {
            return message.Result;
        }
        return NativeMethods.CallWindowProc(_previousWndProc, message.HWnd, message.Msg, message.WParam, message.LParam);
    }

    private bool InternalWndProc(ref Message message)
    {
        foreach (IMessageFilter filter in _filters)
        {
            if (filter.PreFilterMessage(ref message))
            {
                return true;
            }
        }
        return false;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            NativeMethods.SetWindowLongPtr(_windowHandle, -4, _previousWndProc);
            disposedValue = true;
        }
    }

    ~WindowsMessageHook()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}