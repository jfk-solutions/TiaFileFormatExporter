using TiaFileFormat.Database.StorageTypes;
using TiaFileFormat.Wrappers.CodeBlocks;
using TiaFileFormatExporter.Classes;
using TiaFileFormat.Wrappers.Converters.AutomationXml;
using TiaFileFormat.Wrappers.Converters.Code;
using TiaFileFormatExporter.Exporters.Base;
using System.Text;

namespace TiaFileFormatExporter.Exporters
{
    public class ExportCodeBlock : BaseExporter<BaseBlock>
    {
        public static AutomationXmlConverter.ConvertOptions codeBlockConvertOptionsXml = 
            new AutomationXmlConverter.ConvertOptions() 
            { 
                AutomationXmlWithoutNetworksOnSclAndStlBlocks = true, 
                WithDefaultsInInterface = true,
                WriteCommentAndTitleAlthoughWhenEmpty = true,
            };

        private static Encoding utf8WithBom = new UTF8Encoding(true);

        public override async Task Export(StorageBusinessObject sb, BaseBlock baseBlock, string dir)
        {
            var file1 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".xml"));
            var xml = baseBlock.ToAutomationXml(codeBlockConvertOptionsXml);
            File.WriteAllText(file1, xml, utf8WithBom);
            if (baseBlock.BlockLang == BlockLang.SCL)
            {
                var file2 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".scl"));
                File.WriteAllText(file2, baseBlock.ToSourceBlock(codeBlockConvertOptions), encoding);
            }
            else if (baseBlock.BlockLang == BlockLang.STL)
            {
                var file2 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".awl"));
                File.WriteAllText(file2, baseBlock.ToSourceBlock(codeBlockConvertOptions), encoding);
            }
            else if (baseBlock is UserDataType)
            {
                var file2 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".udt"));
                File.WriteAllText(file2, baseBlock.ToSourceBlock(codeBlockConvertOptions), encoding);
            }
            else if (baseBlock is DataBlock)
            {
                var file2 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + ".db"));
                File.WriteAllText(file2, baseBlock.ToSourceBlock(codeBlockConvertOptions), encoding);
            }
        }
    }
}
