using TiaFileFormat.Database.StorageTypes;
using TiaFileFormat.Wrappers.Hmi.Udts;
using TiaFileFormatExporter.Classes;
using TiaFileFormatExporter.Classes.Helper;
using TiaFileFormatExporter.Exporters.Base;

namespace TiaFileFormatExporter.Exporters
{
    public class ExportHmiUdt : BaseExporter<HmiUdt>
    {
        public override async Task Export(StorageBusinessObject sb, HmiUdt hmiUdt, string dir)
        {
            var file = FixPath(Path.Combine(dir, hmiUdt.Name.FixFileName() + ".csv"));
            var csv = CsvSerializer.ToCsv(hmiUdt.Children);
            File.WriteAllText(file, csv);
        }
    }
}
