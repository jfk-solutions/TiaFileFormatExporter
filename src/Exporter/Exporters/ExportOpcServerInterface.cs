using TiaFileFormat.Database.StorageTypes;
using TiaFileFormatExporter.Classes;
using TiaFileFormatExporter.Exporters.Base;

namespace TiaFileFormatExporter.Exporters
{
    public class ExportOpcServerInterface : BaseExporter<TiaFileFormat.Wrappers.Controller.Opc.OpcServerInterface>
    {
        public override async Task Export(StorageBusinessObject sb, TiaFileFormat.Wrappers.Controller.Opc.OpcServerInterface opcServerInterface, string dir)
        {
            var file1 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".xml"));
            File.WriteAllText(file1, opcServerInterface.ServerInterfaceFile);
        }
    }
}
