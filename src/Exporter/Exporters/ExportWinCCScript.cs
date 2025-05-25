using TiaFileFormat.Database.StorageTypes;
using TiaFileFormat.Wrappers.Hmi.WinCCAdvanced;
using TiaFileFormatExporter.Classes;
using TiaFileFormatExporter.Exporters.Base;
using TiaFileFormat.Wrappers.Converters.AutomationXml;

namespace TiaFileFormatExporter.Exporters
{
    public class ExportWinCCScript : BaseExporter<WinCCScript>
    {
        public async override Task Export(StorageBusinessObject sb, WinCCScript winCCScript, string dir)
        {
            var file1 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + (winCCScript.ScriptLang switch
            {
                TiaFileFormat.Wrappers.Hmi.ScriptLang.VB => ".vb",
                TiaFileFormat.Wrappers.Hmi.ScriptLang.Javascript => ".js",
                TiaFileFormat.Wrappers.Hmi.ScriptLang.C => ".c",
                TiaFileFormat.Wrappers.Hmi.ScriptLang.C_Header => ".h",
            })));
            File.WriteAllText(file1, winCCScript.Script);

            var file2 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".xml"));
            var xml = winCCScript.ToAutomationXml();
            File.WriteAllText(file2, xml);
        }
    }
}
