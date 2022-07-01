namespace DocFxToMarkdown;

public class DocFxInterface : DocFxFile, IHasFields, IHasProperties, IHasMethods, IExportable
{
    public List<DocFxMember> Fields { get; set; }
    public List<DocFxMember> Properties { get; set; }
    public List<DocFxMember> Methods { get; set; }
    public string OutputFileName { get; set; }
}