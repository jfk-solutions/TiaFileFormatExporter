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
                var de = txt.Text?.Texts?.TryGetValue(1031, out var v) == true ? EscapeLegacyField(v) : "<No value>";
                var en = txt.Text?.Texts?.TryGetValue(2057, out v) == true ? EscapeLegacyField(v) : "<No value>";

                text += textList.Name + ";" + txt.From + ";" + txt.To + ";" + de + ";" + en + "\r\n";
            }
            var file1 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + "_" + textList.RangeType + ".csv"));
            File.WriteAllText(file1, text);
        }

        private static string EscapeLegacyField(string value)
        {
            value = value.Replace("\r\n", "\n");
            var requiresQuotes = value.Contains(',') || value.Contains('"');
            var escaped = value.Replace("\"", "\"\"");
            return requiresQuotes ? "\"" + escaped + "\"" : escaped;
        }
    }
}
