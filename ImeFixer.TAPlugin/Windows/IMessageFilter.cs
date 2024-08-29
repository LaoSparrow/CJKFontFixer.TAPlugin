namespace ImeFixer.TAPlugin.Windows;

public interface IMessageFilter
{
    bool PreFilterMessage(ref Message m);
}