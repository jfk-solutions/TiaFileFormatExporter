using TiaFileFormat.Database.StorageTypes;
using TiaFileFormatExporter.Classes;
using TiaFileFormatExporter.Exporters.Base;

namespace TiaFileFormatExporter.Exporters
{
    public class ExportTextListAsCsv : BaseExporter<TiaFileFormat.Wrappers.TextLists.TextList>
    {
        public override async Task Export(StorageBusinessObject sb, TiaFileFormat.Wrappers.TextLists.TextList textList, string dir)
        {
            var text = "Parent;From;To;Text [de-DE];Text [en-GB]\r\n";

            foreach (var txt in textList.Ranges.OrderBy(x => x.From))
            {
                var de = txt.Text?.Texts?.TryGetValue(1031, out var v) == true ? v : "<No value>";
                if (de.Contains('"'))
                    de = "\"" + de.Replace("\"", "\"\"") + "\"";

                var en = txt.Text?.Texts?.TryGetValue(2057, out v) == true ? v : "<No value>";
                if (en.Contains('"'))
                    en = "\"" + en.Replace("\"", "\"\"") + "\"";

                text += textList.Name + ";" + txt.From + ";" + txt.To + ";" + de + ";" + en + "\r\n";
            }
            var file1 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + "_" + textList.RangeType + ".csv"));
            File.WriteAllText(file1, text);
        }
    }
}
