using TiaFileFormat.Database.StorageTypes;
using TiaFileFormat.Wrappers.Hmi.WinCCUnified;
using TiaFileFormatExporter.Classes;
using TiaFileFormatExporter.Exporters.Base;

namespace TiaFileFormatExporter.Exporters
{
    //TODO: This is a WIP, will completely be changed
    public class ExportWinCCUnifiedScreen : BaseExporter<WinCCUnifiedScreen>
    {
        public async override Task Export(StorageBusinessObject sb, WinCCUnifiedScreen winCCUnifiedScreen, string dir)
        {
            var file1 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".html"));
            File.WriteAllText(file1, winCCUnifiedScreen.Html);
            var file2 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".js"));
            File.WriteAllText(file2, winCCUnifiedScreen.GetScriptString());
            if (parsedOptions.Snapshot)
            {
                var file3 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".png"));
                await maxBrowserTasks.WaitAsync();
                try
                {
                    await HtmlToPngRenderer.RenderHtmlToPngAsync("file:///" + file1.Replace("\\", "/").Replace(" ", "%20"), file3, 1024, 768);
                }
                finally
                {
                    maxBrowserTasks.Release();
                }
            }
        }
    }
}
