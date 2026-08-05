using TiaFileFormat.Database.StorageTypes;
using BaseHmiTypes.TextGraphicLists;
using TiaFileFormatExporter.Exporters.Base;

namespace TiaFileFormatExporter.Exporters
{
    public class ExportGraphicList : BaseExporter<HmiGraphicList>
    {
        public override async Task Export(StorageBusinessObject sb, HmiGraphicList graphicList, string dir)
        {
        }
    }
}
