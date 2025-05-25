using ClosedXML.Excel;
using System.Globalization;
using System.Text.Json;
using TiaFileFormat.Database.StorageTypes;
using TiaFileFormat.Wrappers.Controller.Alarms;
using TiaFileFormatExporter.Classes;
using TiaFileFormatExporter.Exporters.Base;

namespace TiaFileFormatExporter.Exporters
{
    public class ExportAlarmList : BaseExporter<AlarmList>
    {
        public override async Task Export(StorageBusinessObject sb, AlarmList alarmList, string dir)
        {
            var file1 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".json"));
            File.WriteAllText(file1, JsonSerializer.Serialize(alarmList, new JsonSerializerOptions() { WriteIndented = true }));

            var file2 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".xlsx"));
            using (var workbook = new XLWorkbook())
            {
                workbook.CustomProperties.Add("TIA_Version", "2.1");
                workbook.CustomProperties.Add("FileContent", "Alarm types");

                var worksheet = workbook.Worksheets.Add("Type alarms");
                worksheet.Cell(1, 1).Value = "Location";
                worksheet.Cell(1, 2).Value = "Alarm name";
                worksheet.Cell(1, 3).Value = "Message class";
                worksheet.Cell(1, 4).Value = "Priority";
                worksheet.Cell(1, 5).Value = "Only information";
                worksheet.Cell(1, 6).Value = "Display class";
                worksheet.Cell(1, 7).Value = "Group id";
                worksheet.Cell(1, 8).Value = "Logging";

                var alarmColumn = new Dictionary<CultureInfo, int>();
                var infoColumn = new Dictionary<CultureInfo, int>();
                var addiColumn = new Dictionary<CultureInfo, int>();

                var langs = alarmList.Alarms.SelectMany(x => x.AlarmText.Texts.Keys).Where(x => x > 0).Distinct().Select(x => new CultureInfo(x)).ToList();
                var i = 8;
                foreach (var l in langs)
                {
                    alarmColumn[l] = i;
                    worksheet.Cell(1, i++).Value = "\"Alarm text\"" + " - " + l.DisplayName + " / [" + l.Name + "] / Event text";
                }
                foreach (var l in langs)
                {
                    infoColumn[l] = i;
                    worksheet.Cell(1, i++).Value = "\"Info text\"" + " - " + l.DisplayName + " / [" + l.Name + "] / Info text";
                }
                for (int j = 1; j <= 9; j++)
                {
                    foreach (var l in langs)
                    {
                        if (j == 1)
                            addiColumn[l] = i;
                        worksheet.Cell(1, i++).Value = "\"Additional text " + j + "\"" + " - " + l.DisplayName + " / [" + l.Name + "] / Additional text " + j;
                    }
                }

                int row = 1;
                foreach (var a in alarmList.Alarms)
                {
                    row++;
                    worksheet.Cell(row, 1).Value = a.Location;
                    worksheet.Cell(row, 2).Value = a.Name;
                    worksheet.Cell(row, 3).Value = a.AlarmClass?.Name;
                    worksheet.Cell(row, 4).Value = a.Priority;

                    foreach (var l in langs)
                    {
                        if (a.AlarmText != null && a.AlarmText.Texts.TryGetValue(l.LCID, out var alarmText))
                            worksheet.Cell(row, alarmColumn[l]).Value = alarmText;
                        if (a.InfoText != null && a.InfoText.Texts.TryGetValue(l.LCID, out var infoText))
                            worksheet.Cell(row, infoColumn[l]).Value = infoText;
                        if (a.AdditionalText1 != null && a.AdditionalText1.Texts.TryGetValue(l.LCID, out var add1))
                            worksheet.Cell(row, addiColumn[l] + 0).Value = add1;
                        if (a.AdditionalText2 != null && a.AdditionalText2.Texts.TryGetValue(l.LCID, out var add2))
                            worksheet.Cell(row, addiColumn[l] + 1).Value = add2;
                        if (a.AdditionalText3 != null && a.AdditionalText3.Texts.TryGetValue(l.LCID, out var add3))
                            worksheet.Cell(row, addiColumn[l] + 2).Value = add3;
                        if (a.AdditionalText4 != null && a.AdditionalText4.Texts.TryGetValue(l.LCID, out var add4))
                            worksheet.Cell(row, addiColumn[l] + 3).Value = add4;
                        if (a.AdditionalText5 != null && a.AdditionalText5.Texts.TryGetValue(l.LCID, out var add5))
                            worksheet.Cell(row, addiColumn[l] + 4).Value = add5;
                        if (a.AdditionalText6 != null && a.AdditionalText6.Texts.TryGetValue(l.LCID, out var add6))
                            worksheet.Cell(row, addiColumn[l] + 5).Value = add6;
                        if (a.AdditionalText7 != null && a.AdditionalText7.Texts.TryGetValue(l.LCID, out var add7))
                            worksheet.Cell(row, addiColumn[l] + 6).Value = add7;
                        if (a.AdditionalText8 != null && a.AdditionalText8.Texts.TryGetValue(l.LCID, out var add8))
                            worksheet.Cell(row, addiColumn[l] + 7).Value = add8;
                        if (a.AdditionalText9 != null && a.AdditionalText9.Texts.TryGetValue(l.LCID, out var add9))
                            worksheet.Cell(row, addiColumn[l] + 8).Value = add9;
                    }
                }
                workbook.SaveAs(file2);
            }
        }
    }
}
