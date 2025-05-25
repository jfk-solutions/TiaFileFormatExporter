using TiaFileFormat.Database.StorageTypes;
using TiaFileFormat.Wrappers.Controller.Tags;
using TiaFileFormatExporter.Classes;
using TiaFileFormatExporter.Classes.Helper;
using TiaFileFormatExporter.Exporters.Base;

namespace TiaFileFormatExporter.Exporters
{
    public class ExportPlcTagTable : BaseExporter<PlcTagTable>
    {
        public override async Task Export(StorageBusinessObject sb, PlcTagTable plcTagTable, string dir)
        {
            var file1 = FixPath(Path.Combine(dir, plcTagTable.Name.FixFileName() + "_Tags.csv"));
            var csv1 = CsvSerializer.ToCsv(plcTagTable.Tags);
            File.WriteAllText(file1, csv1);
            var file2 = FixPath(Path.Combine(dir, plcTagTable.Name.FixFileName() + "_Constants.csv"));
            var csv2 = CsvSerializer.ToCsv(plcTagTable.UserConstants);
            File.WriteAllText(file2, csv2);
        }
    }
}
