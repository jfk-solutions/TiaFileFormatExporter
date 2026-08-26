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

        [Option("base64images", HelpText = "Embed images in screen HTML as base64 data URIs.")]
        public bool Base64Images { get; set; }

        [Option("network", HelpText = "Enable Network Information export.")]
        public bool NetworkInformation { get; set; }

        [Option("omit-scl-stl-networks", HelpText = "Omit SCL/STL network code from PLC block Automation XML (Openness/TiaGitHandler-compatible).")]
        public bool OmitSclStlNetworksFromAutomationXml { get; set; }

        [Option("reset-setpoints", HelpText = "Normalize PLC interface SetPoint attribute values to false. Disabled by default.")]
        public bool ResetSetPoints { get; set; }

        [Option("remove-leading-multilingual-text-blank", HelpText = "Remove one leading blank from multilingual XML text (TiaGitHandler compatibility). Disabled by default.")]
        public bool RemoveLeadingMultilingualTextBlank { get; set; }

        [Option("omit-informative-ob-members", HelpText = "Omit informative OB interface members (TiaGitHandler compatibility). Disabled by default.")]
        public bool OmitInformativeOrganizationBlockMembers { get; set; }

        [Option("omit-empty-inherited-instance-db-members", HelpText = "Omit inherited instance-DB members without explicit values or Retain (TiaGitHandler compatibility). Disabled by default.")]
        public bool OmitEmptyInheritedInstanceDbMembers { get; set; }

        [Option("omit-empty-inherited-type-children", HelpText = "Do not expand inherited UDT/FB children without instance values or comments (TiaGitHandler compatibility). Disabled by default.")]
        public bool OmitEmptyInheritedTypeChildren { get; set; }

        [Option("omit-readonly-attributes", HelpText = "Omit informative/read-only Automation XML attributes (TiaGitHandler compatibility). Disabled by default.")]
        public bool OmitReadOnlyInterfaceAttributes { get; set; }

        [Option("german-mnemonics", HelpText = "Export STL/AWL using German mnemonics. The standalone default is International; TIA Portal uses its General/Application/Mnemonic user setting.")]
        public bool GermanMnemonics { get; set; }

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
