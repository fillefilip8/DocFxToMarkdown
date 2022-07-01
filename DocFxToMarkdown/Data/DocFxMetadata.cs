using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace DocFxToMarkdown;

public class DocFxItemSyntaxParameter
{
    public string Id { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
}

public class DocFxItemSyntaxReturn
{
    public string Type { get; set; }
    public string Description { get; set; }
}

public class DocFxItemSyntaxTypeParameter
{
    public string Id { get; set; }
    public string Description { get; set; }
}

public class DocFxItemSyntax
{
    public string Content { get; set; }

    public List<DocFxItemSyntaxParameter> Parameters { get; set; } = new List<DocFxItemSyntaxParameter>();
    
    public DocFxItemSyntaxReturn? Return { get; set; }

    public List<DocFxItemSyntaxTypeParameter> TypeParameters { get; set; } = new List<DocFxItemSyntaxTypeParameter>();
}

public class DocFxItem
{
    [YamlMember(Alias = "uid")] public string UId { get; set; }

    public string Id { get; set; }
    
    public string Parent { get; set; }

    public string Name { get; set; }

    public string NameWithType { get; set; }

    public string Type { get; set; }

    public string FullName { get; set; }

    public string? Namespace { get; set; }

    public string Summary { get; set; }

    public DocFxItemSyntax Syntax { get; set; }

    public List<string> Implements { get; set; }

    public List<string> InheritedMembers { get; set; } = new List<string>();

    public List<string> Inheritance { get; set; } = new List<string>();
}

public class DocFxReference : IEquatable<DocFxReference>
{
    [YamlMember(Alias = "uid"), JsonPropertyName("uid")] public string Id { get; set; }
    
    public string Name { get; set; }
    public string? Parent { get; set; }

    public string? CommentId { get; set; }

    public string FullName { get; set; }


    public string Href { get; set; }
    
    public bool IsExternal { get; set; }

    [JsonIgnore]
    public bool IsType => CommentId?.StartsWith("T:") ?? false;
    [JsonIgnore]
    public bool IsField => CommentId?.StartsWith("F:") ?? false;
    public bool IsProperty => CommentId?.StartsWith("P:") ?? false;
    public bool IsNamespace => CommentId?.StartsWith("N:") ?? false;

    public bool Equals(DocFxReference? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((DocFxReference)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id);
    }
}

public class DocFxMetadataFile
{
    public List<DocFxItem> Items { get; set; }
    
    public List<DocFxReference> References { get; set; } = new List<DocFxReference>();
}