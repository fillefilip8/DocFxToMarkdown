namespace DocFxToMarkdown;

public interface IHasProperties
{
    List<DocFxMember> Properties { get; set; }
}