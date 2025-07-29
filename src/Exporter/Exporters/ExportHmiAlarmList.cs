using System.Text.Json;
using TiaFileFormat.Database.StorageTypes;
using TiaFileFormat.Wrappers.Hmi.Alarms;
using TiaFileFormatExporter.Classes;
using TiaFileFormatExporter.Exporters.Base;

namespace TiaFileFormatExporter.Exporters
{
    public class ExportHmiAlarmList : BaseExporter<HmiAlarmList>
    {
        private static JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions() { WriteIndented = true };

        public override async Task Export(StorageBusinessObject sb, HmiAlarmList alarmList, string dir)
        {
            var file1 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".json"));
            File.WriteAllText(file1, JsonSerializer.Serialize(alarmList, jsonSerializerOptions));
        }
    }
}
