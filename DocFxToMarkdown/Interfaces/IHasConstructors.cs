namespace DocFxToMarkdown;

public interface IHasConstructors
{
    List<DocFxMember> Constructors { get; set; }
}