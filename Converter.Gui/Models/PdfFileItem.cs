namespace Converter.Gui.Models;

public sealed record PdfFileItem(string Path)
{
    public string FileName => System.IO.Path.GetFileName(Path);
    public string Directory => System.IO.Path.GetDirectoryName(Path) ?? string.Empty;
}
