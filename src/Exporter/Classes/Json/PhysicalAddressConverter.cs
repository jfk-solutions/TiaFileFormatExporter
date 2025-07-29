using System.Net.NetworkInformation;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TiaFileFormatExporter.Classes.Json
{
    public class PhysicalAddressConverter : JsonConverter<PhysicalAddress>
    {
        public override PhysicalAddress? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, PhysicalAddress physicalAddress, JsonSerializerOptions options) => writer.WriteStringValue(physicalAddress.ToString());
    }
}
