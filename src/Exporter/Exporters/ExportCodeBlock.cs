using System.Text;
using TiaFileFormat.Database.StorageTypes;
using TiaFileFormat.Helper;
using TiaFileFormat.Wrappers.CodeBlocks;
using TiaFileFormat.Wrappers.Converters.AutomationXml;
using TiaFileFormat.Wrappers.Converters.Code;
using TiaFileFormatExporter.Classes;
using TiaFileFormatExporter.Exporters.Base;

namespace TiaFileFormatExporter.Exporters
{
    public class ExportCodeBlock : BaseExporter<BaseBlock>
    {
        public static AutomationXmlConverter.ConvertOptions codeBlockConvertOptionsXml = 
            new AutomationXmlConverter.ConvertOptions() 
            { 
                AutomationXmlWithoutNetworksOnSclAndStlBlocks = false, 
                WithDefaultsInInterface = true,
                WriteCommentAndTitleAlthoughWhenEmpty = true,
            };

        public static CodeBlockToSourceBlockConverter.ConvertOptions codeBlockConvertOptions =
          new CodeBlockToSourceBlockConverter.ConvertOptions()
          {
              StripHeader = false,
              Mnemonik = Mnemonic.German
          };

        private static SclToSclSourceCodeConverter sclConverter = new SclToSclSourceCodeConverter();
        private static StlToStlSourceCodeConverter stlConverter = new StlToStlSourceCodeConverter();

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
            else if (baseBlock.BlockLang == BlockLang.LAD_CLASSIC ||
                     baseBlock.BlockLang == BlockLang.FBD_CLASSIC ||
                     baseBlock.BlockLang == BlockLang.F_LAD ||
                     baseBlock.BlockLang == BlockLang.F_LAD)
            {
                var cb = (CodeBlock)baseBlock;
                var nr = 0;
                foreach(var nw in cb.Networks)
                {
                    nr++;
                    if (nw.BlockLang == BlockLang.SCL)
                    {
                        var file2 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + "_Network_" + nr + ".scl"));
                        var builder = new IndentedStringBuilder(3);
                        sclConverter.Convert(builder, nw, cb, codeBlockConvertOptions);
                        File.WriteAllText(file2, builder.ToString(), encoding);
                    }
                    else if (nw.BlockLang == BlockLang.STL)
                    {
                        var file2 = FixPath(Path.Combine(dir, sb.Name.FixFileName() + "_Network_" + nr + ".awl"));
                        var builder = new IndentedStringBuilder(3);
                        stlConverter.Convert(builder, nw, cb, codeBlockConvertOptions);
                        File.WriteAllText(file2, builder.ToString(), encoding);
                    }
                }
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
