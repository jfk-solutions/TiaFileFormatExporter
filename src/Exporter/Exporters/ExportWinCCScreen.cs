using TiaFileFormat.Database.StorageTypes;
using BaseHmiTypes.Converters.Html;
using BaseHmiTypes.Screens.Base;
using TiaFileFormatExporter.Classes;
using TiaFileFormatExporter.Exporters.Base;

namespace TiaFileFormatExporter.Exporters
{
    //TODO: This is a WIP, will completely be changed
    public class ExportWinCCScreen : BaseExporter<HmiScreenBase>
    {
        public async override Task Export(StorageBusinessObject sb, HmiScreenBase winCCScreen, string dir)
        {
            var screenName = (winCCScreen.Name ?? sb.ProcessedName).FixFileName();
            var file1 = FixPath(Path.Combine(dir, screenName + ".html"));
            var html = await new HmiScreenToHtmlConverter().ConvertAsync(winCCScreen, Program.hmiProject);
            File.WriteAllText(file1, html);
            if (parsedOptions.Snapshot)
            {
                var file3 = FixPath(Path.Combine(dir, screenName + ".png"));
                await maxBrowserTasks.WaitAsync();
                try
                {
                    var width = winCCScreen.Width.StaticValue > 0 ? (int)Math.Ceiling(winCCScreen.Width.StaticValue) : 1024;
                    var height = winCCScreen.Height.StaticValue > 0 ? (int)Math.Ceiling(winCCScreen.Height.StaticValue) : 768;
                    await HtmlToPngRenderer.RenderHtmlToPngAsync("file:///" + file1.Replace("\\", "/").Replace(" ", "%20"), file3, width, height);
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
