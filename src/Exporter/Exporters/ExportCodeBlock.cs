using TiaFileFormat.Database.StorageTypes;
using TiaFileFormat.Wrappers.CodeBlocks;
using TiaFileFormatExporter.Classes;
using TiaFileFormat.Wrappers.Converters.AutomationXml;
using TiaFileFormat.Wrappers.Converters.Code;
using TiaFileFormatExporter.Exporters.Base;

namespace TiaFileFormatExporter.Exporters
{
    public class ExportImage : BaseExporter<CodeBlock>
    {
        public override async Task Export(StorageBusinessObject sb, CodeBlock codeBlock, string dir)
        {
            var file1 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".xml"));
            var xml = codeBlock.ToAutomationXml();
            File.WriteAllText(file1, xml);
            if (codeBlock.BlockLang == BlockLang.SCL)
            {
                var file2 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".scl"));
                File.WriteAllText(file2, codeBlock.ToSourceBlock(codeBlockConvertOptions), encoding);
            }
            else if (codeBlock.BlockLang == BlockLang.STL)
            {
                var file2 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".awl"));
                File.WriteAllText(file2, codeBlock.ToSourceBlock(codeBlockConvertOptions), encoding);
            }
        }
    }
}
