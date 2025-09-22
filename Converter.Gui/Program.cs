using System;
using System.Windows.Forms;

namespace Converter.Gui;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        try
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur au démarrage: {ex.Message}\n\nStackTrace:\n{ex.StackTrace}", 
                          "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
