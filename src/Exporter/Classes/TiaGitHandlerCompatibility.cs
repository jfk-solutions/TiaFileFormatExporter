using System.Globalization;
using System.Text;
using System.Xml;
using TiaFileFormat.Database.StorageTypes;
using TiaFileFormat.Wrappers;
using TiaFileFormat.Wrappers.Controller.Tags;

namespace TiaFileFormatExporter.Classes;

internal static class TiaGitHandlerCompatibility
{
    private const string ControllerTargetType =
        "Siemens.Simatic.HwConfiguration.Model.S7ControllerTargetData";
    private const string ForceTableType =
        "Siemens.Simatic.Lang.Model.Debugging.VarTableForceData";

    private static readonly Dictionary<string, string> FolderAliases = new(StringComparer.Ordinal)
    {
        ["ProgramBlocksFolder"] = "software",
        ["ControllerDataTypeFolder"] = "datatypes",
        ["ControllerTagsFolder"] = "variables",
        ["WatchtablesFolder"] = "watchesandforces",
        ["TextLists"] = "plcalarmtextlistgroup",
    };

    private static readonly HashSet<string> SkippedSubtrees = new(StringComparer.Ordinal)
    {
        "SystemBlocksFolder",
        "SystemDataTypeFolder",
    };

    public static bool IsControllerTarget(StorageBusinessObject storageObject) =>
        storageObject.TiaTypeName == ControllerTargetType;

    public static bool IsForceTable(StorageBusinessObject storageObject) =>
        storageObject.TiaTypeName == ForceTableType;

    public static bool ShouldSkipSubtree(StorageBusinessObject storageObject) =>
        SkippedSubtrees.Contains(storageObject.Name ?? storageObject.ProcessedName);

    public static string GetControllerPath(StorageBusinessObject controller) =>
        Path.Combine("Main", NormalizeFolderName(controller.Name));

    public static string GetChildPath(StorageBusinessObject storageObject, string parentPath)
    {
        var name = storageObject.Name ?? storageObject.ProcessedName ?? string.Empty;
        if (FolderAliases.TryGetValue(name, out var alias))
            return Path.Combine(parentPath, alias);

        return storageObject.ProjectTreeChildren.Any()
            ? Path.Combine(parentPath, NormalizeFolderName(name))
            : parentPath;
    }

    public static string NormalizeFolderName(string? name) =>
        (name ?? string.Empty).Replace("-", string.Empty)
            .Replace(".", string.Empty)
            .Replace(" ", string.Empty);

    public static void WriteForceTable(StorageBusinessObject storageObject, string directory)
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "Force table.force");
        using var writer = CreateWriter(path);
        writer.WriteStartDocument();
        writer.WriteStartElement("Document");
        writer.WriteStartElement("Engineering");
        writer.WriteAttributeString("version", "V19");
        writer.WriteEndElement();
        writer.WriteStartElement("SW.WatchAndForceTables.PlcForceTable");
        writer.WriteAttributeString("ID", "0");
        writer.WriteStartElement("AttributeList");
        writer.WriteElementString("Name", "Force table");
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndDocument();
    }

    public static void WritePlcTagTable(PlcTagTable table, string path)
    {
        using var writer = CreateWriter(path);
        var nextId = 0;

        writer.WriteStartDocument();
        writer.WriteStartElement("Document");
        writer.WriteStartElement("Engineering");
        writer.WriteAttributeString("version", "V19");
        writer.WriteEndElement();
        writer.WriteStartElement("SW.Tags.PlcTagTable");
        writer.WriteAttributeString("ID", FormatId(nextId++));
        writer.WriteStartElement("AttributeList");
        writer.WriteElementString("Name", table.Name);
        writer.WriteEndElement();

        var isDefaultSystemTable = table.StorageBusinessObject.CoreAttributes?.Subtype == "Default";
        if (!isDefaultSystemTable && (table.Tags.Count > 0 || table.UserConstants.Count > 0))
        {
            writer.WriteStartElement("ObjectList");
            foreach (var tag in table.Tags)
                WriteTag(writer, tag, table, ref nextId);
            foreach (var constant in table.UserConstants)
                WriteConstant(writer, constant, table, ref nextId);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndDocument();
    }

    private static void WriteTag(XmlWriter writer, PlcTag tag, PlcTagTable table, ref int nextId)
    {
        writer.WriteStartElement("SW.Tags.PlcTag");
        writer.WriteAttributeString("ID", FormatId(nextId++));
        writer.WriteAttributeString("CompositionName", "Tags");
        writer.WriteStartElement("AttributeList");
        writer.WriteElementString("DataTypeName", table.TagsWithUserDataType.Contains(tag)
            ? $"\"{tag.DataType}\""
            : tag.DataType);
        writer.WriteElementString("ExternalAccessible", tag.AccesibleFromHmi.ToString().ToLowerInvariant());
        writer.WriteElementString("LogicalAddress", tag.Address);
        writer.WriteElementString("Name", tag.Name);
        writer.WriteEndElement();
        WriteComment(writer, tag.GetMultiLanguageComment(), table, ref nextId);
        writer.WriteEndElement();
    }

    private static void WriteConstant(XmlWriter writer, PlcUserConstant constant, PlcTagTable table,
        ref int nextId)
    {
        writer.WriteStartElement("SW.Tags.PlcUserConstant");
        writer.WriteAttributeString("ID", FormatId(nextId++));
        writer.WriteAttributeString("CompositionName", "UserConstants");
        writer.WriteStartElement("AttributeList");
        writer.WriteElementString("DataTypeName", constant.DataType);
        writer.WriteElementString("Name", constant.Name);
        writer.WriteElementString("Value", constant.Value);
        writer.WriteEndElement();
        WriteComment(writer, constant.GetMultiLanguageComment(), table, ref nextId);
        writer.WriteEndElement();
    }

    private static void WriteComment(XmlWriter writer, MultiLanguageText? comment, PlcTagTable table,
        ref int nextId)
    {
        writer.WriteStartElement("ObjectList");
        writer.WriteStartElement("MultilingualText");
        writer.WriteAttributeString("ID", FormatId(nextId++));
        writer.WriteAttributeString("CompositionName", "Comment");
        writer.WriteStartElement("ObjectList");

        var cultures = table.StorageBusinessObject.Database.ProjectCultures
            .Select(culture => culture.LCID)
            .Distinct()
            .ToArray();
        foreach (var lcid in cultures)
        {
            writer.WriteStartElement("MultilingualTextItem");
            writer.WriteAttributeString("ID", FormatId(nextId++));
            writer.WriteAttributeString("CompositionName", "Items");
            writer.WriteStartElement("AttributeList");
            writer.WriteElementString("Culture", CultureInfo.GetCultureInfo(lcid).Name);
            string? text = null;
            comment?.Texts?.TryGetValue(lcid, out text);
            writer.WriteElementString("Text", text ?? string.Empty);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static XmlWriter CreateWriter(string path) => XmlWriter.Create(path, new XmlWriterSettings
    {
        Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true),
        Indent = true,
        IndentChars = "  ",
        NewLineChars = "\r\n",
        NewLineHandling = NewLineHandling.Replace,
    });

    private static string FormatId(int id) => id.ToString("X", CultureInfo.InvariantCulture);
}
