using TiaFileFormat.Database.StorageTypes;
using BaseHmiTypes.Connections;
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
