using TiaFileFormat.Database.StorageTypes;
using BaseHmiTypes.Screens.Base;
using BaseHmiTypes.Scripts;
using TiaFileFormatExporter.Classes;
using TiaFileFormatExporter.Exporters.Base;
using TiaFileFormat.Wrappers.Converters.AutomationXml;

namespace TiaFileFormatExporter.Exporters
{
    //TODO: This is a WIP, will completely be changed
    public class ExportWinCCScript : BaseExporter<HmiScript>
    {
        public async override Task Export(StorageBusinessObject sb, HmiScript winCCScript, string dir)
        {
            var scriptName = (winCCScript.Name ?? sb.ProcessedName).FixFileName();
            var file1 = FixPath(Path.Combine(dir, scriptName + (winCCScript.Language switch
            {
                HmiScriptLanguage.VBScript => ".vb",
                HmiScriptLanguage.JavaScript => ".js",
                HmiScriptLanguage.C => ".c",
                _ => ".txt",
            })));
            File.WriteAllText(file1, winCCScript.SourceCode);

            var file2 = FixPath(Path.Combine(dir, scriptName + ".xml"));
            var xml = winCCScript.ToAutomationXml();
            File.WriteAllText(file2, xml);
        }
    }
}
