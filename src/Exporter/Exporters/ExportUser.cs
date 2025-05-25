using System.Text.Json;
using TiaFileFormat.Database.StorageTypes;
using TiaFileFormatExporter.Classes;
using TiaFileFormatExporter.Exporters.Base;

namespace TiaFileFormatExporter.Exporters
{
    public class ExportUser : BaseExporter<TiaFileFormat.Wrappers.UserManagement.User>
    {
        public override async Task Export(StorageBusinessObject sb, TiaFileFormat.Wrappers.UserManagement.User user, string dir)
        {
            var file1 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".json"));
            File.WriteAllText(file1, JsonSerializer.Serialize(user, new JsonSerializerOptions() { WriteIndented = true }));
        }
    }
}
