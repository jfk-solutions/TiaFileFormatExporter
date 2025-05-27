using CommandLine;
using Siemens.Simatic.Hmi.Utah.Globalization;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using TiaFileFormat;
using TiaFileFormat.Database;
using TiaFileFormat.Database.StorageTypes;
using TiaFileFormat.Wrappers;
using TiaFileFormat.Wrappers.CodeBlocks;
using TiaFileFormat.Wrappers.Converters.Code;
using TiaFileFormatExporter;
using TiaFileFormatExporter.Classes;
using TiaFileFormatExporter.Classes.Converters;
using TiaFileFormatExporter.Exporters.Base;

public class Program
{
    static HighLevelObjectConverterWrapper highLevelObjectConverterWrapper;
    static List<Task> exportTasks;
    static int runningTasks;
    static int exceptionCount;
    static int exportedCount;
    static int skippedCount;
    static string outDir;
    static string currentFile;
    public static SemaphoreSlim maxBrowserTasks;
    static ConcurrentDictionary<string, DateTime> fileModifiedTimeStamps = new ConcurrentDictionary<string, DateTime>();
    static string fileNameModifiedTimeStamps;
    static Dictionary<string, string> pathReplacements;
    public static Options parsedOptions;
    public static ConvertOptions convertOptions;
    public static CodeBlockToSourceBlockConverter.ConvertOptions codeBlockConvertOptions = new CodeBlockToSourceBlockConverter.ConvertOptions() { Mnemonik = Mnemonic.German };
    public static Encoding encoding = new UTF8Encoding(true);
    private static Dictionary<Type, IExporter> exporters;

    private async static Task Main(string[] args)
    {
        maxBrowserTasks = new SemaphoreSlim(10); //A maximum of 10 chrome instances could be started, to generate screenshots of a web page

        exporters = typeof(Program).Assembly.GetTypes()
                                                .Where(x => x.BaseType?.IsGenericType == true && x.BaseType?.GetGenericTypeDefinition() == typeof(BaseExporter<>))
                                                .ToDictionary(x => x.BaseType!.GetGenericArguments()[0], x => (IExporter)Activator.CreateInstance(x)!);

        using var parser = new Parser(settings =>
        {
            settings.AllowMultiInstance = true;
            settings.HelpWriter = Console.Error;
        });
        var parsedArgs = parser.ParseArguments<Options>(args);
        parsedOptions = parsedArgs.Value;

        highLevelObjectConverterWrapper = new HighLevelObjectConverterWrapper(new ImageToFileUriProvider(), new ImagesIncludingFromRtfConverter());
        convertOptions = new ConvertOptions();

        exportTasks = new List<Task>();

        if (parsedArgs.Tag == ParserResultType.NotParsed)
        {
            Console.WriteLine(parsedArgs.ToString());
            Environment.Exit(1);
        }

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
                        if (exporters.TryGetValue(highLevelObject.GetType(), out var exporter))
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

                            exporter.Export(sb, highLevelObject, dir);
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

    public static string FixPath(string path)
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
