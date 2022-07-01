using System.Security;

namespace DocFxToMarkdown;

public static class StringUtil
{
    public static string FixGenericString(string s)
    {
        return SecurityElement.Escape(s);
    }
}