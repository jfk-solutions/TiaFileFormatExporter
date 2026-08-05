using TiaFileFormat.Database.StorageTypes;
using BaseHmiTypes.Tags;
using TiaFileFormatExporter.Classes;
using TiaFileFormatExporter.Classes.Helper;
using TiaFileFormatExporter.Exporters.Base;

namespace TiaFileFormatExporter.Exporters
{
    public class ExportHmiTagTable : BaseExporter<HmiTagTable>
    {
        public override async Task Export(StorageBusinessObject sb, HmiTagTable hmiTagTable, string dir)
        {
            var file = FixPath(Path.Combine(dir, (hmiTagTable.Name ?? sb.ProcessedName).FixFileName() + ".csv"));
            var csv = CsvSerializer.ToCsv(hmiTagTable.Tags);
            File.WriteAllText(file, csv);
        }
    }
}
