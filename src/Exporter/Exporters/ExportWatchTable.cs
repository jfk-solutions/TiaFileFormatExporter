using TiaFileFormat.Database.StorageTypes;
using TiaFileFormat.Wrappers.Controller.WatchTable;
using TiaFileFormatExporter.Classes;
using TiaFileFormatExporter.Classes.Helper;
using TiaFileFormatExporter.Exporters.Base;

namespace TiaFileFormatExporter.Exporters
{
    public class ExportWatchTable : BaseExporter<WatchTable>
    {
        public override async Task Export(StorageBusinessObject sb, WatchTable watchTable, string dir)
        {
            var file = FixPath(Path.Combine(dir, watchTable.Name.FixFileName() + ".csv"));
            var csv = CsvSerializer.ToCsv(watchTable.Items);
            File.WriteAllText(file, csv);
        }
    }
}
