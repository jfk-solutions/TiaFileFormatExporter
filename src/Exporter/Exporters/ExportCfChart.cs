using System.Text.Json;
using TiaFileFormat.Database.StorageTypes;
using TiaFileFormatExporter.Classes;
using TiaFileFormatExporter.Exporters.Base;

namespace TiaFileFormatExporter.Exporters
{
    public class ExportCfChart : BaseExporter<TiaFileFormat.Wrappers.CfCharts.CfChart>
    {
        public override async Task Export(StorageBusinessObject sb, TiaFileFormat.Wrappers.CfCharts.CfChart cfChart, string dir)
        {
            var file1 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".json"));
            File.WriteAllText(file1, JsonSerializer.Serialize(cfChart, new JsonSerializerOptions() { WriteIndented = true }));
        }
    }
}
