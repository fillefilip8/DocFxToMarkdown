namespace DocFxToMarkdown;

public interface IHasFields
{
    List<DocFxMember> Fields { get; set; }
}