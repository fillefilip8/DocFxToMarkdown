using YamlDotNet.Serialization;

namespace DocFxToMarkdown;

public abstract class DocFxFile
{
    [YamlMember(Alias = "uid")] public string UId { get; set; }
    public string Id { get; set; }

    public DocFxItem Raw { get; set; }
    public DocFxMetadataFile RawFile { get; set; }

    public string FileName { get; set; }

    public string Parent { get; set; }

    public string Name { get; set; }

    public string NameWithType { get; set; }
    
    //public string Type { get; set; }

    public string FullName { get; set; }

    public string Namespace { get; set; }

    public string Summary { get; set; }
    
    public DocFxItemSyntax Syntax { get; set; }
}