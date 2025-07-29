using System.Text.Json;
using TiaFileFormat.Database.StorageTypes;
using TiaFileFormatExporter.Classes;
using TiaFileFormatExporter.Exporters.Base;

namespace TiaFileFormatExporter.Exporters
{
    public class ExportOpcClientInterface : BaseExporter<TiaFileFormat.Wrappers.Controller.Opc.OpcClientInterface>
    {
        public override async Task Export(StorageBusinessObject sb, TiaFileFormat.Wrappers.Controller.Opc.OpcClientInterface opcClientInterface, string dir)
        {
            var file1 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".json"));
            File.WriteAllText(file1, JsonSerializer.Serialize(opcClientInterface, new JsonSerializerOptions() { WriteIndented = true }));
        }
    }
}
