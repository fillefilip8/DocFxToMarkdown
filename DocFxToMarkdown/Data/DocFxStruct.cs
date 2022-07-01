using DocFxToMarkdown;

public class DocFxStruct : DocFxFile, IHasFields, IHasProperties, IHasMethods, IHasConstructors, IExportable
{
    public List<DocFxMember> Fields { get; set; }
    public List<DocFxMember> Properties { get; set; }
    public List<DocFxMember> Methods { get; set; }
    public List<DocFxMember> Constructors { get; set; }
    public string OutputFileName { get; set; }
}