using TiaFileFormat.Database.StorageTypes;
using TiaFileFormat.Wrappers.Hmi.GraphicLists;
using TiaFileFormatExporter.Exporters.Base;

namespace TiaFileFormatExporter.Exporters
{
    public class ExportGraphicList : BaseExporter<GraphicList>
    {
        public override async Task Export(StorageBusinessObject sb, GraphicList graphicList, string dir)
        {
        }
    }
}
