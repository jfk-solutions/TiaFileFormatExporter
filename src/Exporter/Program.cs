using CommandLine;
using ImageMagick;
using Siemens.Simatic.Hmi.Utah.Globalization;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using TiaFileFormat;
using TiaFileFormat.Database;
using TiaFileFormat.Database.StorageTypes;
using TiaFileFormat.Wrappers;
using TiaFileFormat.Wrappers.CodeBlock;
using TiaFileFormat.Wrappers.CodeBlock.Converter;
using TiaFileFormat.Wrappers.Controller.Tags;
using TiaFileFormat.Wrappers.Controller.WatchTable;
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

            var prjNm = Path.GetFileNameWithoutExtension(file);

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
                        imageTasks.Add(ExportObject(i, prjNm + "/Images")!);
                    }
                }
                if (parsedOptions.Snapshot)
                {
                    await Task.WhenAll(imageTasks);
                }
            }

            if (database.RootObject.StoreObjectIds.TryGetValue("Project", out var prj))
                WalkProject((StorageBusinessObject)prj.StorageObject, prjNm + "/Project");
            if (database.RootObject.StoreObjectIds.TryGetValue("Library", out var lb))
                WalkProject((StorageBusinessObject)lb.StorageObject, prjNm + "/Library");

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
                       
                        if (highLevelObject.ModifiedTime != null)
                        {
                            if (fileModifiedTimeStamps.TryGetValue(nm, out var dt) && dt == highLevelObject.ModifiedTime)
                            {
                                skippedCount++;
                                return;
                            }
                            fileModifiedTimeStamps[nm] = highLevelObject.ModifiedTime.Value;
                        }

                        exportedCount++;

                        switch (highLevelObject)
                        {
                            case Image image:
                                {
                                    Directory.CreateDirectory(dir);
                                    var file = Path.Combine(dir, image.Name.FixFileName() + "." + image.ImageType.ToString().ToLowerInvariant());
                                    File.WriteAllBytes(file, image.Data);
                                    if (parsedOptions.ConvertMetafilesToSvg && (image.ImageType == ImageType.EMF || image.ImageType == ImageType.WMF))
                                    {
                                        using var ms = new MemoryStream();
                                        using var magickImage = new MagickImage(image.Data);
                                        magickImage.Write(ms, MagickFormat.Svg);
                                        ms.Position = 0;
                                        using var sr = new StreamReader(ms);
                                        var text = sr.ReadToEnd();
                                        var file2 = Path.Combine(dir, image.Name.FixFileName() + ".svg");
                                        File.WriteAllText(file2, text);
                                    }
                                    break;
                                }
                            case PlcTagTable plcTagTable:
                                {
                                    Directory.CreateDirectory(dir);
                                    var file1 = Path.Combine(dir, plcTagTable.Name.FixFileName() + "_Tags.csv");
                                    var csv1 = CsvSerializer.ToCsv(plcTagTable.Tags);
                                    File.WriteAllText(file1, csv1);
                                    var file2 = Path.Combine(dir, plcTagTable.Name.FixFileName() + "_Constants.csv");
                                    var csv2 = CsvSerializer.ToCsv(plcTagTable.UserConstants);
                                    File.WriteAllText(file2, csv2);
                                    break;
                                }
                            case HmiTagTable hmiTagTable:
                                {
                                    Directory.CreateDirectory(dir);
                                    var file = Path.Combine(dir, hmiTagTable.Name.FixFileName() + ".csv");
                                    var csv = CsvSerializer.ToCsv(hmiTagTable.Tags);
                                    File.WriteAllText(file, csv);
                                    break;
                                }
                            case WatchTable watchTable:
                                {
                                    Directory.CreateDirectory(dir);
                                    var file = Path.Combine(dir, watchTable.Name.FixFileName() + ".csv");
                                    var csv = CsvSerializer.ToCsv(watchTable.Items);
                                    File.WriteAllText(file, csv);
                                    break;
                                }
                            case WinCCUnifiedScreen winCCUnifiedScreen:
                                {
                                    Directory.CreateDirectory(dir);
                                    var file1 = Path.Combine(dir, sb.Name.FixFileName() + ".html");
                                    File.WriteAllText(file1, winCCUnifiedScreen.Html);
                                    var file2 = Path.Combine(dir, sb.Name.FixFileName() + ".js");
                                    File.WriteAllText(file2, winCCUnifiedScreen.GetScriptString());
                                    if (parsedOptions.Snapshot)
                                    {
                                        var file3 = Path.Combine(dir, sb.Name.FixFileName() + ".png");
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
                                    Directory.CreateDirectory(dir);
                                    var file1 = Path.Combine(dir, sb.Name.FixFileName() + ".html");
                                    File.WriteAllText(file1, winCCScreen.Html);
                                    var file2 = Path.Combine(dir, sb.Name.FixFileName() + ".vb");
                                    File.WriteAllText(file2, winCCScreen.GetScriptString());
                                    if (parsedOptions.Snapshot)
                                    {
                                        var file3 = Path.Combine(dir, sb.Name.FixFileName() + ".png");
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
                                    break;
                                }
                            case WinCCScript winCCScript:
                                {
                                    Directory.CreateDirectory(dir);
                                    var file1 = Path.Combine(dir, sb.Name.FixFileName() + (winCCScript.ScriptType switch
                                    {
                                        TiaFileFormat.Wrappers.Hmi.ScriptType.VB => ".vb",
                                        TiaFileFormat.Wrappers.Hmi.ScriptType.Javascript => ".js",
                                        TiaFileFormat.Wrappers.Hmi.ScriptType.C => ".c",
                                        TiaFileFormat.Wrappers.Hmi.ScriptType.C_Header => ".h",
                                    }));
                                    File.WriteAllText(file1, winCCScript.Script);
                                    break;
                                }
                            case CodeBlock codeBlock:
                                {
                                    Directory.CreateDirectory(dir);
                                    var file1 = Path.Combine(dir, sb.Name.FixFileName() + ".xml");
                                    File.WriteAllText(file1, codeBlock.ToAutomationXml());
                                    if (codeBlock.BlockLang== BlockLang.SCL)
                                    {
                                        var file2 = Path.Combine(dir, sb.Name.FixFileName() + ".scl");
                                        File.WriteAllText(file2, string.Join("", codeBlock.ToCode()));
                                    }
                                    else if (codeBlock.BlockLang == BlockLang.STL)
                                    {
                                        var file2 = Path.Combine(dir, sb.Name.FixFileName() + ".awl");
                                        var networsWithCode = codeBlock.Networks.Zip(codeBlock.ToCode(Mnemonic.German));
                                        File.WriteAllText(file2, string.Join("", networsWithCode.Select(x => "#### Network - " + x.First.Title + "\n\n" + x.Second + "\n\n")));
                                    }
                                    break;
                                }
                            case TiaFileFormat.Wrappers.TextLists.TextList textList:
                                {
                                    Directory.CreateDirectory(dir);
                                    var file1 = Path.Combine(dir, sb.Name.FixFileName() + ".json");
                                    File.WriteAllText(file1, JsonSerializer.Serialize(textList, new JsonSerializerOptions() { WriteIndented = true }));
                                    break;
                                }
                            case TiaFileFormat.Wrappers.Controller.Alarms.AlarmList alarmList:
                                {
                                    Directory.CreateDirectory(dir);
                                    var file1 = Path.Combine(dir, sb.Name.FixFileName() + ".json");
                                    File.WriteAllText(file1, JsonSerializer.Serialize(alarmList, new JsonSerializerOptions() { WriteIndented = true }));
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
}
