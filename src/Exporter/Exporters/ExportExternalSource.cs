using TiaFileFormat.Database.StorageTypes;
using TiaFileFormat.Wrappers.Controller.ExternalSources;
using TiaFileFormatExporter.Exporters.Base;

namespace TiaFileFormatExporter.Exporters
{
    public class ExportExternalSource : BaseExporter<ExternalSource>
    {
        public override async Task Export(StorageBusinessObject sb, ExternalSource externalSource, string dir)
        {
            var file1 = FixPath(Path.Combine(dir, "ExternalSource", externalSource.Name));
            File.WriteAllBytes(file1, externalSource.Content);
        }
    }
}
