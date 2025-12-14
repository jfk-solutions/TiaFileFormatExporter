using System.Text.Json;
using TiaFileFormat.Database.StorageTypes;
using TiaFileFormat.Wrappers.Controller.Network;
using TiaFileFormatExporter.Classes;
using TiaFileFormatExporter.Classes.Json;
using TiaFileFormatExporter.Exporters.Base;

namespace TiaFileFormatExporter.Exporters
{
    public class ExportNetworkInformation : BaseExporter<NetworkInformation>
    {
        private static JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions() { WriteIndented = true };

        static ExportNetworkInformation()
        {
            jsonSerializerOptions.Converters.Add(new PhysicalAddressConverter());
        }

        public override async Task Export(StorageBusinessObject sb, NetworkInformation networkInformation, string dir)
        {
            var file = FixPath(Path.Combine(dir, "Network", networkInformation.Name.FixFileName() + ".json"));
            Directory.CreateDirectory(FixPath(Path.Combine(dir, "Network")));
            if (networkInformation is EthernetNetworkInformation eth)
                File.WriteAllText(file, JsonSerializer.Serialize(eth, jsonSerializerOptions));
            else if (networkInformation is ProfibusNetworkInformation pb)
                File.WriteAllText(file, JsonSerializer.Serialize(pb, jsonSerializerOptions));
        }
    }
}
