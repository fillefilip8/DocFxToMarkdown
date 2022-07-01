namespace DocFxToMarkdown;

public class DocFxEnum : DocFxFile, IHasFields, IExportable
{
    public List<DocFxMember> Fields { get; set; }
    public string OutputFileName { get; set; }
}