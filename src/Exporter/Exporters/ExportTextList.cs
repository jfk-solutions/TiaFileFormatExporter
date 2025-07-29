using System.Text.Json;
using TiaFileFormat.Database.StorageTypes;
using TiaFileFormatExporter.Classes;
using TiaFileFormatExporter.Exporters.Base;

namespace TiaFileFormatExporter.Exporters
{
    public class ExportTextList : BaseExporter<TiaFileFormat.Wrappers.TextLists.TextList>
    {
        private static JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions() { WriteIndented = true };

        public override async Task Export(StorageBusinessObject sb, TiaFileFormat.Wrappers.TextLists.TextList textList, string dir)
        {
            var file1 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".json"));
            File.WriteAllText(file1, JsonSerializer.Serialize(textList, jsonSerializerOptions));
        }
    }
}
