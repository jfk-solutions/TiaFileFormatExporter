using CommandLine;

namespace TiaFileFormatExporter
{
    public class Options
    {
        [Option("lib", HelpText = "Enable export of Library objects.")]
        public bool ExportLib { get; set; }

        [Option("all", HelpText = "Enable All export.")]
        public bool All { get; set; }

        [Option("plcblock", HelpText = "Enable PlcBlock export.")]
        public bool PlcBlock { get; set; }

        [Option("plctagtable", HelpText = "Enable PlcTagTable export.")]
        public bool PlcTagTable { get; set; }

        [Option("images", HelpText = "Enable Images export.")]
        public bool Image { get; set; }

        [Option("network", HelpText = "Enable Network Information export.")]
        public bool NetworkInformation { get; set; }

        [Option("opc", HelpText = "Enable Opc export.")]
        public bool Opc { get; set; }

        [Option("convertmetafiles", HelpText = "Convert Metafiles (*.wmf, *.emf) to SVG.")]
        public bool ConvertMetafilesToSvg { get; set; }

        [Option("hmitagtable", HelpText = "Enable HmiTagTable export.")]
        public bool HmiTagTable { get; set; }

        [Option("plcwatchtable", HelpText = "Enable PlcWatchTable export.")]
        public bool PlcWatchTable { get; set; }

        [Option("winccscript", HelpText = "Enable WinCCScript export.")]
        public bool WinCCScript { get; set; }

        [Option("wincctagtable", HelpText = "Enable WinCCTagTable export.")]
        public bool WinCCTagTable { get; set; }

        [Option("screens", HelpText = "Enable Screens export.")]
        public bool Screens { get; set; }

        [Option("textlist", HelpText = "Enable TextList export.")]
        public bool TextList { get; set; }

        [Option("alarmlist", HelpText = "Enable AlarmList export.")]
        public bool AlarmList { get; set; }

        [Option("hmialarmlist", HelpText = "Enable HmiAlarmList export.")]
        public bool HmiAlarmList { get; set; }

        [Option("user", HelpText = "Enable User export.")]
        public bool User { get; set; }

        [Option("chart", HelpText = "Enable CFC Chart export.")]
        public bool Chart { get; set; }

        [Option("snapshot", HelpText = "Enable Screen Snapshot generation.")]
        public bool Snapshot { get; set; }

        [Option('o', "out", HelpText = "OutDir", Required = true)]
        public string? OutDir { get; set; }

        [Value(0, MetaName = "input file", HelpText = "Input file to be processed.", Required = true, Min = 1)]
        public IEnumerable<string>? FileNames { get; set; }

        [Option("noprojectname", HelpText = "Do not add Project Name to Path.")]
        public bool NoProjectName { get; set; }

        [Option("replacepath", HelpText = "Replacements for path, seperated via |")]
        public IEnumerable<string> ReplacePath { get; set; }
    }
}
