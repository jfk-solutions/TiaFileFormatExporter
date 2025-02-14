using CommandLine;

namespace TiaFileFormatExporter
{
    public class Options
    {
        [Option("all", HelpText = "Enable All export.")]
        public bool All { get; set; }

        [Option("plcblock", HelpText = "Enable PlcBlock export.")]
        public bool PlcBlock { get; set; }

        [Option("plctagtable", HelpText = "Enable PlcTagTable export.")]
        public bool PlcTagTable { get; set; }

        [Option("images", HelpText = "Enable Images export.")]
        public bool Image { get; set; }

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

        [Option("snapshot", HelpText = "Enable Screen Snapshot generation.")]
        public bool Snapshot { get; set; }

        [Option('o', "out", HelpText = "OutDir", Required = true)]
        public string? OutDir { get; set; }

        [Value(0, MetaName = "input file", HelpText = "Input file to be processed.", Required = true)]
        public string? FileName { get; set; }
    }
}
