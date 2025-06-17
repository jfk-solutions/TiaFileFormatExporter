using TiaFileFormat.Database.StorageTypes;
using TiaFileFormat.Wrappers.Controller.NamedValues;
using TiaFileFormatExporter.Exporters.Base;

namespace TiaFileFormatExporter.Exporters
{
    public class ExportNamedValueType : BaseExporter<NamedValue>
    {
        public override async Task Export(StorageBusinessObject sb, NamedValue namedValue, string dir)
        {
            var file1 = FixPath(Path.Combine(dir, namedValue.Name));
            File.WriteAllBytes(file1, namedValue.Content);
        }
    }
}
