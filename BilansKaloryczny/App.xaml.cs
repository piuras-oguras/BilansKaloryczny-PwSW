using System;
using System.Windows;
using System.Windows.Threading;

namespace BilansKaloryczny;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += (s, ex) =>
        {
            MessageBox.Show(ex.Exception.ToString(), "Unhandled UI exception");
            ex.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
        {
            MessageBox.Show(ex.ExceptionObject?.ToString() ?? "Unknown", "Unhandled domain exception");
        };

        try
        {
            base.OnStartup(e);

            var win = new MainWindow();
            win.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString(), "Startup crash");
            Shutdown(-1);
        }
    }
}