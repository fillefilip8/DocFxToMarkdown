namespace DocFxToMarkdown;

public class DocFxClass : DocFxFile, IHasFields, IHasProperties, IHasMethods, IHasInheritance, IHasInheritedMembers, IHasConstructors, IExportable
{
    public List<DocFxMember> Fields { get; set; }
    public List<DocFxMember> Properties { get; set; }
    public List<DocFxMember> Methods { get; set; }
    public List<DocFxMember> Constructors { get; set; }
    public List<string> Inheritance { get; set; }
    public List<string> InheritedMembers { get; set; }
    public string OutputFileName { get; set; }
}