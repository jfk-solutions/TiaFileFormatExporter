using TiaFileFormat.Database.StorageTypes;
using TiaFileFormat.Wrappers.Hmi.Connections;
using TiaFileFormatExporter.Exporters.Base;

namespace TiaFileFormatExporter.Exporters
{
    public class ExportHmiConnection : BaseExporter<HmiConnection>
    {
        public override async Task Export(StorageBusinessObject sb, HmiConnection hmiConnection, string dir)
        {
        }
    }
}
