using System.CommandLine;
using System.Text;
using System.Text.RegularExpressions;
using DocFxToMarkdown;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

#region Write Methods

void WriteFields(StringBuilder sb, List<DocFxMember>? members)
{
    if (members != null && members.Count > 0)
    {
        sb.AppendLine();
        sb.AppendLine("### Fields");
        sb.AppendLine();

        foreach (var field in members)
        {
            sb.AppendLine($"#### {StringUtil.FixGenericString(field.Name)}");

            if (!string.IsNullOrEmpty(field.Description))
            {
                var summary = Regex.Replace(field.Description, @"<[^>]*>", string.Empty);
                sb.AppendLine(summary);
            }

            sb.AppendLine();
            sb.AppendLine("##### Declaration");
            sb.AppendLine();

            sb.AppendLine("```cs");
            sb.AppendLine(field.Syntax.Content);
            sb.AppendLine("```");
        }
    }
}

void WriteProperties(StringBuilder sb, List<DocFxMember>? members)
{
    if (members != null && members.Count > 0)
    {
        sb.AppendLine();
        sb.AppendLine("### Properties");
        sb.AppendLine();

        foreach (var property in members)
        {
            sb.AppendLine($"#### {StringUtil.FixGenericString(property.Name)}");

            if (!string.IsNullOrEmpty(property.Description))
            {
                var summary = Regex.Replace(property.Description, @"<[^>]*>", string.Empty);
                sb.AppendLine(summary);
            }

            sb.AppendLine();
            sb.AppendLine("##### Declaration");
            sb.AppendLine();

            sb.AppendLine("```cs");
            sb.AppendLine(property.Syntax.Content);
            sb.AppendLine("```");
        }
    }
}

async Task WriteMethods(StringBuilder sb, List<DocFxMember>? members)
{
    if (members != null && members.Count > 0)
    {
        sb.AppendLine("### Methods");

        foreach (var method in members)
        {
            sb.AppendLine($"#### {StringUtil.FixGenericString(method.Name)}");

            sb.AppendLine();
            if (!string.IsNullOrEmpty(method.Description))
            {
                var summary = Regex.Replace(method.Description, @"<[^>]*>", string.Empty);

                sb.AppendLine(summary);
            }

            sb.AppendLine();

            sb.AppendLine();
            sb.AppendLine("##### Declaration");
            sb.AppendLine();

            sb.AppendLine("```cs");
            sb.AppendLine(method.Syntax.Content);
            sb.AppendLine("```");

            if (method.Syntax.Parameters.Count > 0)
            {
                sb.AppendLine("##### Parameters");

                sb.AppendLine("| Type | Name | Description |");
                sb.AppendLine("| ---- | ---- | ---- |");

                foreach (var parameter in method.Syntax.Parameters)
                {
                    sb.AppendLine(
                        $"| {await TryGenerateLink(parameter.Type, true)} | {parameter.Id} | {parameter.Description} |");
                }
            }

            sb.AppendLine();

            if (method.Syntax.Return != null)
            {
                sb.AppendLine("##### Returns");

                sb.AppendLine("| Type | Description |");
                sb.AppendLine("| ---- | ---- |");

                sb.AppendLine(
                    $"| {await TryGenerateLink(method.Syntax.Return.Type, true)} | {method.Syntax.Return.Description} |");
            }

            sb.AppendLine();
        }
    }
}

async Task WriteConstructors(StringBuilder sb, List<DocFxMember>? members)
{
    if (members != null && members.Count > 0)
    {
        sb.AppendLine("### Constructors");

        foreach (var member in members)
        {
            sb.AppendLine();

            sb.AppendLine($"#### {StringUtil.FixGenericString(member.Name)}");

            sb.AppendLine();
            if (!string.IsNullOrEmpty(member.Description))
            {
                var summary = Regex.Replace(member.Description, @"<[^>]*>", string.Empty);

                sb.AppendLine(summary);
            }

            sb.AppendLine();

            sb.AppendLine();
            sb.AppendLine("##### Declaration");
            sb.AppendLine();

            sb.AppendLine("```cs");
            sb.AppendLine(member.Syntax.Content);
            sb.AppendLine("```");

            if (member.Syntax.Parameters.Count > 0)
            {
                sb.AppendLine("##### Parameters");

                sb.AppendLine("| Type | Name | Description |");
                sb.AppendLine("| ---- | ---- | ---- |");

                foreach (var parameter in member.Syntax.Parameters)
                {
                    sb.AppendLine(
                        $"| {await TryGenerateLink(parameter.Type, true)} | {parameter.Id} | {parameter.Description} |");
                }
            }
        }
    }
}

#endregion

var deserializer = new DeserializerBuilder()
    .WithNamingConvention(CamelCaseNamingConvention.Instance)
    .IgnoreUnmatchedProperties()
    .Build();

var types = new List<DocFxFile>();

var references = new List<DocFxReference>();

var typeMap = new Dictionary<string, Type>
{
    { "Class", typeof(DocFxClass) },
    { "Struct", typeof(DocFxStruct) },
    { "Interface", typeof(DocFxInterface) },
    { "Enum", typeof(DocFxEnum) },
    { "Delegate", typeof(DocFxDelegate) },
    { "Namespace", typeof(DocFxNamespace) }
};

var typeMapReversed = typeMap.ToDictionary(pair => pair.Value, pair => pair.Key);


async Task<string> TryGenerateLink(string name, bool isHtml = false, bool cleanup = false)
{
    var reference = references.FirstOrDefault(r => r.Id == name);

    if (reference != null)
    {
        string displayName = string.Empty;

        if (reference.Parent != null && reference.Parent.StartsWith("System"))
        {
            displayName = reference.FullName;
        }
        else if (reference.IsExternal)
        {
            displayName = reference.FullName;
        }
        else
        {
            displayName = reference.FullName;
        }

        if (!string.IsNullOrEmpty(displayName))
        {
            displayName = StringUtil.FixGenericString(displayName);
            Console.WriteLine(displayName);
            return displayName;
        }
    }
    else
    {
        Console.WriteLine("Could not transform url for " + name);
    }

    return cleanup ? string.Empty : name;
}

async Task WriteNamespaceIndex(DocFxNamespace fxNamespace, DirectoryInfo output)
{
    List<DocFxFile> classes = new List<DocFxFile>();
    List<DocFxFile> structs = new List<DocFxFile>();
    List<DocFxFile> interfaces = new List<DocFxFile>();
    List<DocFxFile> enums = new List<DocFxFile>();
    List<DocFxFile> delegates = new List<DocFxFile>();

    for (int i = 0; i < fxNamespace.References.Count; i++)
    {
        int index = i;
        var type = types.Where(x => x.UId == fxNamespace.References[index].Id).FirstOrDefault();
        if (type == null)
        {
            continue;
        }

        switch (type.Raw.Type)
        {
            case "Class":
                classes.Add(type);
                break;
            case "Struct":
                structs.Add(type);
                break;
            case "Interface":
                interfaces.Add(type);
                break;
            case "Enum":
                enums.Add(type);
                break;
            case "Delegate":
                delegates.Add(type);
                break;
            default:
                continue;
        }
    }
    
    classes.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
    structs.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
    interfaces.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
    enums.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
    delegates.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
    
    var sb = new StringBuilder();

    sb.AppendLine("---");
    sb.AppendLine($"id: {fxNamespace.FullName}");
    sb.AppendLine($"title: {fxNamespace.FullName}");
    sb.AppendLine("---");
    sb.AppendLine();
    sb.AppendLine($"# {fxNamespace.FullName}");
    sb.AppendLine();

    await WriteType("Classes", sb, classes);
    await WriteType("Structs", sb, structs);
    await WriteType("Interfaces", sb, interfaces);
    await WriteType("Enums", sb, enums);
    await WriteType("Delegates", sb, delegates);

    if (!Directory.Exists(Path.Combine(output.FullName, fxNamespace.FullName)))
    {
        Directory.CreateDirectory(Path.Combine(output.FullName, fxNamespace.FullName));
    }

    await File.WriteAllTextAsync(Path.Combine(output.FullName, fxNamespace.FullName, "index.md"), sb.ToString());

    async Task WriteType(string type, StringBuilder builder, List<DocFxFile> files)
    {
        if (files.Count > 0)
        {
            builder.AppendLine($"## {type}");
            builder.AppendLine();
        
            for (int i = 0; i < files.Count; i++)
            {
                builder.AppendLine($"#### [{StringUtil.FixGenericString(files[i].Name)}](./{files[i].Id.Replace("`", "-")})");
                if (!string.IsNullOrWhiteSpace(files[i].Summary))
                {
                    var summary = Regex.Replace(files[i].Summary, @"<[^>]*>", string.Empty);

                    builder.AppendLine($"> {summary}");
                }
            }
        }
    }
}


var rootCommand = new RootCommand("DocFxToMarkdown");

var generateCommand = new Command("generate");

var inputDirOption = new Option<DirectoryInfo>(
    name: "--input",
    description: "The input folder containing the docfx files.");

var outputDirOption = new Option<DirectoryInfo>(
    name: "--output",
    description: "The output folder where the markdown files will be written.");

generateCommand.AddOption(inputDirOption);
generateCommand.AddOption(outputDirOption);

generateCommand.SetHandler(async (input, output) =>
    {
        if (!Directory.Exists(output.FullName))
        {
            Directory.CreateDirectory(output.FullName);
        }
    
        Console.WriteLine("Input Folder: " + input);
        Console.WriteLine("Output Folder: " + output);

        foreach (var dir in Directory.GetDirectories(output.FullName))
        {
            Directory.Delete(dir, true);
        }

        foreach (var file in Directory.GetFiles(input.FullName, "*.yml"))
        {
            if (file.EndsWith("toc.yml"))
            {
                continue;
            }

            var yaml = File.ReadAllText(file);
            var d = deserializer.Deserialize<DocFxMetadataFile>(yaml);

            var p = d.Items.FirstOrDefault(i =>
                i.Type == "Class" || i.Type == "Struct" || i.Type == "Enum" || i.Type == "Interface" ||
                i.Type == "Delegate" ||
                i.Type == "Namespace");

            if (p == null)
            {
                Console.WriteLine($"Skipping {file} as it doesn't contain any types.");
                continue;
            }

            if (!typeMap.TryGetValue(p.Type, out var t))
            {
                Console.WriteLine($"Unknown type: {p.Type}");
                continue;
            }

            var type = (DocFxFile)Activator.CreateInstance(t)!;

            Console.WriteLine($"Found {p.UId} ({t.Name})");

            type.Id = p.Id;
            type.Raw = p;
            type.RawFile = d;
            type.UId = p.UId;
            type.Name = p.Name;
            type.Summary = p.Summary;
            type.Syntax = p.Syntax;
            type.FullName = p.FullName;
            type.Namespace = p.Namespace!;
            type.NameWithType = p.NameWithType;
            type.Parent = p.Parent;
            type.FileName = Path.GetFileNameWithoutExtension(file);

            var children = d.Items.Where(i => i.Parent == p.UId).ToList();

            if (type is IHasConstructors constructors)
            {
                constructors.Constructors = children.Where(c => c.Type == "Constructor").Select(f => new DocFxMember
                {
                    Name = f.Name,
                    Description = f.Summary,
                    Syntax = f.Syntax
                }).ToList();
            }

            if (type is IHasFields fields)
            {
                fields.Fields = children.Where(c => c.Type == "Field").Select(f => new DocFxMember
                {
                    Name = f.Name,
                    Description = f.Summary,
                    Syntax = f.Syntax
                }).ToList();
            }

            if (type is IHasProperties properties)
            {
                properties.Properties = children.Where(c => c.Type == "Property").Select(f => new DocFxMember
                {
                    Name = f.Name,
                    Description = f.Summary,
                    Syntax = f.Syntax
                }).ToList();
            }

            if (type is IHasMethods methods)
            {
                methods.Methods = children.Where(c => c.Type == "Method").Select(f => new DocFxMember
                {
                    Name = f.Name,
                    Description = f.Summary,
                    Syntax = f.Syntax
                }).ToList();
            }

            if (type is IHasInheritance inheritance)
            {
                inheritance.Inheritance = p.Inheritance;
            }

            if (type is IHasInheritedMembers inheritedMembers)
            {
                inheritedMembers.InheritedMembers = p.InheritedMembers;
            }

            if (type is IExportable exportable)
            {
                exportable.OutputFileName =
                    Path.Combine(output.FullName, p.Namespace!, type.Id.Replace("`", "-") + ".md");
            }

            if (type is DocFxNamespace docFxNamespace)
            {
                docFxNamespace.References = d.References;
            }

            types.Add(type);
        }

        foreach (var n in types.Where(t => t is DocFxNamespace).Cast<DocFxNamespace>())
        {
            references.AddRange(n.References);

            foreach (var type in types.Where(t => t.Namespace == n.Name))
            {
                foreach (var reference in type.RawFile.References)
                {
                    if (!references.Exists(r => r.Id == reference.Id))
                    {
                        if (!reference.IsExternal)
                        {
                            references.Add(reference);
                        }
                    }
                }
            }
        }

        foreach (var type in types)
        {
            if (type is DocFxNamespace fxNamespace)
            {
                await WriteNamespaceIndex(fxNamespace, output);
                continue;
            }
            
            if (type is not IExportable exportable)
            {
                Console.WriteLine("Skipping " + type.Name);
                continue;
            }

            var sb = new StringBuilder();

            if (!Directory.Exists(Path.Combine(output.FullName, type.Namespace)))
            {
                Directory.CreateDirectory(Path.Combine(output.FullName, type.Namespace));
            }

            sb.AppendLine("---");
            sb.AppendLine($"id: {Path.GetFileNameWithoutExtension(exportable.OutputFileName)}");
            sb.AppendLine($"title: {type.Name}");
            sb.AppendLine("---");

            sb.AppendLine();

            var typeName = typeMapReversed[type.GetType()];

            sb.AppendLine($"# {typeName} {StringUtil.FixGenericString(type.Name)}");

            sb.AppendLine();

            if (!string.IsNullOrEmpty(type.Summary))
            {
                var summary = Regex.Replace(type.Summary, @"<[^>]*>", string.Empty);

                sb.AppendLine(summary);
            }

            sb.AppendLine();
            sb.AppendLine();

            if (type is IHasInheritance inheritance && inheritance.Inheritance.Count > 0)
            {
                sb.AppendLine("<div class=\"inheritance\">");

                sb.AppendLine();
                sb.AppendLine("##### Inheritance");
                sb.AppendLine();

                for (var index = 0; index < inheritance.Inheritance.Count; index++)
                {
                    var member = inheritance.Inheritance[index];

                    var style = $"\"--data-index\": {index}";

                    sb.AppendLine($"<div class=\"level\" style={{{{{style}}}}}>");

                    sb.AppendLine(await TryGenerateLink(member, true));

                    sb.AppendLine("</div>");
                }

                sb.AppendLine("</div>");
            }

            if (type is IHasInheritedMembers inheritedMembers && inheritedMembers.InheritedMembers.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("##### Inherited Members");
                sb.AppendLine();

                sb.AppendLine("<details>");

                sb.AppendLine("<summary>Show</summary>");

                var filtered = inheritedMembers.InheritedMembers.ToList();
                for (var index = 0; index < filtered.Count; index++)
                {
                    var member = filtered[index];

                    var link = await TryGenerateLink(member, false, true);

                    if (!string.IsNullOrEmpty(link))
                    {
                        sb.AppendLine();
                        sb.AppendLine(link);
                        sb.AppendLine();
                    }
                }

                sb.AppendLine("</details>");
            }

            sb.AppendLine();
            sb.AppendLine("##### Syntax");
            sb.AppendLine();

            sb.AppendLine("```cs");
            sb.AppendLine(type.Syntax.Content);
            sb.AppendLine("```");


            if (type.Syntax.TypeParameters.Count > 0)
            {
                sb.AppendLine();

                sb.AppendLine("##### Type Parameters");

                sb.AppendLine("| Name | Description |");
                sb.AppendLine("| ---- | ---- |");

                foreach (var parameter in type.Syntax.TypeParameters)
                {
                    sb.AppendLine($"| {parameter.Id} | {parameter.Description} |");
                }
            }

            sb.AppendLine();

            if (type is IHasConstructors constructors)
            {
                await WriteConstructors(sb, constructors.Constructors);
            }

            if (type is IHasFields fields)
            {
                WriteFields(sb, fields.Fields);
            }

            if (type is IHasProperties properties)
            {
                WriteProperties(sb, properties.Properties);
            }

            if (type is IHasMethods methods)
            {
                await WriteMethods(sb, methods.Methods);
            }

            File.WriteAllText(exportable.OutputFileName, sb.ToString());
        }

        var classes = types.FindAll(t => t is DocFxClass);
        var structs = types.FindAll(t => t is DocFxStruct);
        var enums = types.FindAll(t => t is DocFxEnum);
        var interfaces = types.FindAll(t => t is DocFxInterface);
        var delegates = types.FindAll(t => t is DocFxDelegate);
        var namespaces = types.FindAll(t => t is DocFxNamespace);

        Console.WriteLine("### Statistics ###");
        Console.WriteLine($"{types.Count} types");
        Console.WriteLine($"{classes.Count} classes");
        Console.WriteLine($"{structs.Count} structs");
        Console.WriteLine($"{enums.Count} enums");
        Console.WriteLine($"{interfaces.Count} interfaces");
        Console.WriteLine($"{delegates.Count} delegates");
        Console.WriteLine($"{namespaces.Count} namespaces");

        Console.WriteLine($"{references.Count} references");
    },
    inputDirOption, outputDirOption);

rootCommand.AddCommand(generateCommand);

return await rootCommand.InvokeAsync(args);