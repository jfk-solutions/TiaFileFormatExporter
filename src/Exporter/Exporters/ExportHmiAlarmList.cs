using System.Text.Json;
using TiaFileFormat.Database.StorageTypes;
using TiaFileFormat.Wrappers.Hmi.Alarms;
using TiaFileFormatExporter.Classes;
using TiaFileFormatExporter.Exporters.Base;

namespace TiaFileFormatExporter.Exporters
{
    public class ExportHmiAlarmList : BaseExporter<AlarmList>
    {
        public override async Task Export(StorageBusinessObject sb, AlarmList alarmList, string dir)
        {
            var file1 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".json"));
            File.WriteAllText(file1, JsonSerializer.Serialize(alarmList, new JsonSerializerOptions() { WriteIndented = true }));
        }
    }
}
