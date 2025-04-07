using ClosedXML.Excel;
using CommandLine;
using ImageMagick;
using Siemens.Simatic.Hmi.Utah.Globalization;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Text.Json;
using TiaFileFormat;
using TiaFileFormat.Database;
using TiaFileFormat.Database.StorageTypes;
using TiaFileFormat.Wrappers;
using TiaFileFormat.Wrappers.CodeBlock;
using TiaFileFormat.Wrappers.CodeBlock.Converter;
using TiaFileFormat.Wrappers.Controller.Tags;
using TiaFileFormat.Wrappers.Controller.WatchTable;
using TiaFileFormat.Wrappers.Converters.AutomationXml;
using TiaFileFormat.Wrappers.Hmi.Tags;
using TiaFileFormat.Wrappers.Hmi.WinCCAdvanced;
using TiaFileFormat.Wrappers.Hmi.WinCCUnified;
using TiaFileFormat.Wrappers.Images;
using TiaFileFormatExporter;
using TiaFileFormatExporter.Classes;
using TiaFileFormatExporter.Classes.Converters;
using TiaFileFormatExporter.Classes.Helper;

public class Program
{
    static HighLevelObjectConverterWrapper highLevelObjectConverterWrapper;
    static ConvertOptions convertOptions;
    static List<Task> exportTasks;
    static int runningTasks;
    static int exceptionCount;
    static int exportedCount;
    static int skippedCount;
    static string outDir;
    static string currentFile;
    static Options parsedOptions;
    static SemaphoreSlim maxBrowserTasks;
    static ConcurrentDictionary<string, DateTime> fileModifiedTimeStamps = new ConcurrentDictionary<string, DateTime>();
    static string fileNameModifiedTimeStamps;
    static Dictionary<string, string> pathReplacements;

    private async static Task Main(string[] args)
    {
        maxBrowserTasks = new SemaphoreSlim(10); //A maximum of 10 chrome instances could be started, to generate screenshots of a web page

        var parsedArgs = Parser.Default.ParseArguments<Options>(args);
        parsedOptions = parsedArgs.Value;

        highLevelObjectConverterWrapper = new HighLevelObjectConverterWrapper(new ImageToFileUriProvider(), new ImagesIncludingFromRtfConverter());
        convertOptions = new ConvertOptions();

        exportTasks = new List<Task>();

        var files = parsedArgs.Value.FileNames;
        outDir = parsedArgs.Value.OutDir;


        fileNameModifiedTimeStamps = Path.Combine(outDir, "fileModifiedTimeStamps.json");
        if (File.Exists(fileNameModifiedTimeStamps))
        {
            fileModifiedTimeStamps = JsonSerializer.Deserialize<ConcurrentDictionary<string, DateTime>>(File.ReadAllText(fileNameModifiedTimeStamps));
        }

        foreach (var f in files)
        {
            runningTasks = 0;
            exportedCount = 0;
            skippedCount = 0;

            var sw = new Stopwatch();
            sw.Start();

            var file = f;
            if (OperatingSystem.IsWindows() && file.StartsWith("/"))
                file = file.Substring(1);
            currentFile = file;

            var tfp = TiaFileProvider.CreateFromSingleFile(file);
            var database = TiaDatabaseFile.Load(tfp);

            //database.ParseAllObjects();

            var prjNm = Path.GetFileNameWithoutExtension(file) + "/";
            if (parsedOptions.NoProjectName)
                prjNm = "";

            if (parsedOptions.ReplacePath != null)
                pathReplacements = parsedOptions.ReplacePath.Select(x => x.Replace("\\", "/").Split("|")).ToDictionary(x => x[0], x => x[1]);

            if (parsedOptions.Image)
            {
                var imgs = database.FindStorageBusinessObjectsWithChildType<HmiInternalImageAttributes>();
                var duplicateNames = new HashSet<string>();

                var imageTasks = new List<Task>();
                foreach (var i in imgs)
                {
                    //TODO: maybe handle duplicated image names in some way!
                    if (!duplicateNames.Contains(i.ProcessedName))
                    {
                        duplicateNames.Add(i.ProcessedName);
                        imageTasks.Add(ExportObject(i, prjNm + "Images")!);
                    }
                }
                if (parsedOptions.Snapshot)
                {
                    await Task.WhenAll(imageTasks);
                }
            }

            if (database.RootObject.StoreObjectIds.TryGetValue("Project", out var prj))
                WalkProject((StorageBusinessObject)prj.StorageObject, prjNm + "Project");
            if (database.RootObject.StoreObjectIds.TryGetValue("Library", out var lb))
                WalkProject((StorageBusinessObject)lb.StorageObject, prjNm + "Library");

            await Task.WhenAll(exportTasks);

            sw.Stop();
            Console.WriteLine();
            Console.WriteLine("Export took: " + sw.ToString());

            File.WriteAllText(fileNameModifiedTimeStamps, JsonSerializer.Serialize(fileModifiedTimeStamps, new JsonSerializerOptions() { WriteIndented = true }));
        }
    }

    private static void WalkProject(StorageBusinessObject sb, string path)
    {
        ExportObject(sb, path);
        foreach (var o in sb.ProjectTreeChildren)
            WalkProject(o, path + "/" + sb.Name);
    }

    private static Task? ExportObject(StorageBusinessObject sb, string path)
    {
        if (highLevelObjectConverterWrapper.CouldConvert(sb))
        {
            var highLevelObjectType = highLevelObjectConverterWrapper.GetHighLevelObjectType(sb);

            if ((highLevelObjectType == HighLevelObjectType.PlcBlock && !parsedOptions.PlcBlock && !parsedOptions.All) ||
                (highLevelObjectType == HighLevelObjectType.PlcTagTable && !parsedOptions.PlcTagTable && !parsedOptions.All) ||
                (highLevelObjectType == HighLevelObjectType.PlcWatchTable && !parsedOptions.PlcWatchTable && !parsedOptions.All) ||
                (highLevelObjectType == HighLevelObjectType.WinCCScript && !parsedOptions.WinCCScript && !parsedOptions.All) ||
                (highLevelObjectType == HighLevelObjectType.WinCCTagTable && !parsedOptions.WinCCTagTable && !parsedOptions.All) ||
                (highLevelObjectType == HighLevelObjectType.WinCCScreen && !parsedOptions.Screens && !parsedOptions.All) ||
                (highLevelObjectType == HighLevelObjectType.WinCCUnifiedScreen && !parsedOptions.Screens && !parsedOptions.All) ||
                (highLevelObjectType == HighLevelObjectType.TextList && !parsedOptions.TextList && !parsedOptions.All) ||
                (highLevelObjectType == HighLevelObjectType.AlarmList && !parsedOptions.AlarmList && !parsedOptions.All) ||
                (highLevelObjectType == HighLevelObjectType.HmiAlarmList && !parsedOptions.HmiAlarmList && !parsedOptions.All) ||
                (highLevelObjectType == HighLevelObjectType.User && !parsedOptions.User && !parsedOptions.All) ||
                (highLevelObjectType == HighLevelObjectType.CfChart && !parsedOptions.Chart && !parsedOptions.All) ||
                (highLevelObjectType == HighLevelObjectType.Image && !parsedOptions.Image && !parsedOptions.All))
                return Task.CompletedTask;

            Interlocked.Increment(ref runningTasks);
            var task = Task.Run(async () =>
            {
                try
                {
                    var highLevelObject = highLevelObjectConverterWrapper.Convert(sb, convertOptions);
                    if (highLevelObject != null)
                    {
                        var dir = Path.Combine(outDir, path).FixPath();

                        var nm = Path.Combine(dir, highLevelObject.Name);
                       
                        if (highLevelObject.LastModified != null)
                        {
                            if (fileModifiedTimeStamps.TryGetValue(nm, out var dt) && dt == highLevelObject.LastModified)
                            {
                                skippedCount++;
                                return;
                            }
                            fileModifiedTimeStamps[nm] = highLevelObject.LastModified.Value;
                        }

                        exportedCount++;

                        Directory.CreateDirectory(FixPath(dir));

                        switch (highLevelObject)
                        {
                            case Image image:
                                {
                                    var file = FixPath(Path.Combine(dir, image.Name.FixFileName() + "." + image.ImageType.ToString().ToLowerInvariant()));
                                    File.WriteAllBytes(file, image.Data);
                                    if (parsedOptions.ConvertMetafilesToSvg && (image.ImageType == ImageType.EMF || image.ImageType == ImageType.WMF))
                                    {
                                        using var ms = new MemoryStream();
                                        using var magickImage = new MagickImage(image.Data);
                                        magickImage.Write(ms, MagickFormat.Svg);
                                        ms.Position = 0;
                                        using var sr = new StreamReader(ms);
                                        var text = sr.ReadToEnd();
                                        var file2 = FixPath(Path.Combine(dir, image.Name.FixFileName() + ".svg"));
                                        File.WriteAllText(file2, text);
                                    }
                                    break;
                                }
                            case PlcTagTable plcTagTable:
                                {
                                    var file1 = FixPath(Path.Combine(dir, plcTagTable.Name.FixFileName() + "_Tags.csv"));
                                    var csv1 = CsvSerializer.ToCsv(plcTagTable.Tags);
                                    File.WriteAllText(file1, csv1);
                                    var file2 = FixPath(Path.Combine(dir, plcTagTable.Name.FixFileName() + "_Constants.csv"));
                                    var csv2 = CsvSerializer.ToCsv(plcTagTable.UserConstants);
                                    File.WriteAllText(file2, csv2);
                                    break;
                                }
                            case HmiTagTable hmiTagTable:
                                {
                                    var file = FixPath(Path.Combine(dir, hmiTagTable.Name.FixFileName() + ".csv"));
                                    var csv = CsvSerializer.ToCsv(hmiTagTable.Tags);
                                    File.WriteAllText(file, csv);
                                    break;
                                }
                            case WatchTable watchTable:
                                {
                                    var file = FixPath(Path.Combine(dir, watchTable.Name.FixFileName() + ".csv"));
                                    var csv = CsvSerializer.ToCsv(watchTable.Items);
                                    File.WriteAllText(file, csv);
                                    break;
                                }
                            case WinCCUnifiedScreen winCCUnifiedScreen:
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
                                    break;
                                }
                            case WinCCScreen winCCScreen:
                                {
                                    var file1 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".html"));
                                    File.WriteAllText(file1, winCCScreen.Html);
                                    var file2 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".vb"));
                                    File.WriteAllText(file2, winCCScreen.GetScriptString());
                                    if (parsedOptions.Snapshot)
                                    {
                                        var file3 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".png"));
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
                                    break;
                                }
                            case WinCCScript winCCScript:
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
                                    break;
                                }
                            case CodeBlock codeBlock:
                                {
                                    var file1 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".xml"));
                                    var xml = codeBlock.ToAutomationXml();
                                    File.WriteAllText(file1, xml);
                                    if (codeBlock.BlockLang== BlockLang.SCL)
                                    {
                                        var file2 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".scl"));
                                        File.WriteAllText(file2, string.Join("", codeBlock.ToSourceBlock()));
                                    }
                                    else if (codeBlock.BlockLang == BlockLang.STL)
                                    {
                                        var file2 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".awl"));
                                        File.WriteAllText(file2, string.Join("", codeBlock.ToSourceBlock()));
                                    }
                                    break;
                                }
                            case TiaFileFormat.Wrappers.TextLists.TextList textList:
                                {
                                    var file1 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".json"));
                                    File.WriteAllText(file1, JsonSerializer.Serialize(textList, new JsonSerializerOptions() { WriteIndented = true }));
                                    break;
                                }
                            case TiaFileFormat.Wrappers.Hmi.GraphicLists.GraphicList graphicList:
                                {
                                    break;
                                }
                            case TiaFileFormat.Wrappers.Controller.Alarms.AlarmList alarmList:
                                {
                                    var file1 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".json"));
                                    File.WriteAllText(file1, JsonSerializer.Serialize(alarmList, new JsonSerializerOptions() { WriteIndented = true }));

                                    var file2 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".xlsx"));
                                    using (var workbook = new XLWorkbook())
                                    {
                                        workbook.CustomProperties.Add("TIA_Version", "2.1");
                                        workbook.CustomProperties.Add("FileContent", "Alarm types");

                                        var worksheet = workbook.Worksheets.Add("Type alarms");
                                        worksheet.Cell(1,1).Value = "Location";
                                        worksheet.Cell(1,2).Value = "Alarm name";
                                        worksheet.Cell(1,3).Value = "Message class";
                                        worksheet.Cell(1,4).Value = "Priority";
                                        worksheet.Cell(1,5).Value = "Only information";
                                        worksheet.Cell(1,6).Value = "Display class";
                                        worksheet.Cell(1,7).Value = "Group id";
                                        worksheet.Cell(1,8).Value = "Logging";

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
                                                if (a.AlarmText != null &&  a.AlarmText.Texts.TryGetValue(l.LCID, out var alarmText))
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
                                    break;
                                }
                            case TiaFileFormat.Wrappers.Hmi.Alarms.AlarmList alarmList:
                                {
                                    var file1 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".json"));
                                    File.WriteAllText(file1, JsonSerializer.Serialize(alarmList, new JsonSerializerOptions() { WriteIndented = true }));
                                    break;
                                }
                            case TiaFileFormat.Wrappers.Hmi.Connections.HmiConnection hmiConnection:
                                {
                                    break;
                                }
                            case TiaFileFormat.Wrappers.CfCharts.CfChart cfChart:
                                {
                                    var file1 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".json"));
                                    File.WriteAllText(file1, JsonSerializer.Serialize(cfChart, new JsonSerializerOptions() { WriteIndented = true }));
                                    break;
                                }
                            case TiaFileFormat.Wrappers.UserManagement.User user:
                                {
                                    var file1 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".json"));
                                    File.WriteAllText(file1, JsonSerializer.Serialize(user, new JsonSerializerOptions() { WriteIndented = true }));
                                    break;
                                }
                        }
                    }
                }
                catch (Exception)
                {
                    exceptionCount++;
                }

                Interlocked.Decrement(ref runningTasks);
                lock (exportTasks)
                {
                    Console.SetCursorPosition(2, 2);
                    Console.Write("file: " + currentFile + "           ");
                    Console.SetCursorPosition(2, 3);
                    Console.Write("export tasks: " + runningTasks + " todo from " + exportTasks.Count + "            ");
                    Console.SetCursorPosition(5, 4);
                    Console.Write("exported: " + exportedCount + "           ");
                    Console.SetCursorPosition(5, 5);
                    Console.Write("skipped : " + skippedCount + "           ");
                    Console.SetCursorPosition(5, 6);
                    Console.Write("exceptions: " + exceptionCount + "           ");
                }
            });
            lock (exportTasks)
            {
                exportTasks.Add(task);
            }
            return task;
        }
        return null;
    }

    private static string FixPath(string path)
    {
        if (pathReplacements == null)
            return path;
        var d = path;
        d = d.Replace("\\", "/");
        foreach (var p in pathReplacements)
        {
            d = d.Replace(p.Key, p.Value);
        }
        return d;
    }
}
