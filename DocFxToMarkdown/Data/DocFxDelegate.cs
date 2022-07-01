namespace DocFxToMarkdown;

public class DocFxDelegate : DocFxFile, IExportable
{
    public string OutputFileName { get; set; }
}