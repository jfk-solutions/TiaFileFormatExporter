using TiaFileFormat.Database.StorageTypes;
using TiaFileFormat.Wrappers.Hmi.WinCCAdvanced;
using TiaFileFormatExporter.Classes;
using TiaFileFormatExporter.Exporters.Base;

namespace TiaFileFormatExporter.Exporters
{
    //TODO: This is a WIP, will completely be changed
    public class ExportWinCCScreen : BaseExporter<WinCCScreen>
    {
        public async override Task Export(StorageBusinessObject sb, WinCCScreen winCCScreen, string dir)
        {
            var file1 = FixPath(Path.Combine(dir, winCCScreen.Name.FixFileName() + ".html"));
            File.WriteAllText(file1, winCCScreen.Html);
            var file2 = FixPath(Path.Combine(dir, winCCScreen.Name.FixFileName() + ".vb"));
            File.WriteAllText(file2, winCCScreen.GetScriptString());
            if (parsedOptions.Snapshot)
            {
                var file3 = FixPath(Path.Combine(dir, winCCScreen.Name.FixFileName() + ".png"));
                await maxBrowserTasks.WaitAsync();
                try
                {
                    await HtmlToPngRenderer.RenderHtmlToPngAsync("file:///" + file1.Replace("\\", "/").Replace(" ", "%20"), file3, winCCScreen.Width, winCCScreen.Height);
                }
                finally
                {
                    maxBrowserTasks.Release();
                }
            }

            //var file4 = Path.Combine(dir, sb.Name.FixFileName() + ".xml");
            //var xml = winCCScreen.ToAutomationXml();
            //File.WriteAllText(file4, xml);
        }
    }
}
