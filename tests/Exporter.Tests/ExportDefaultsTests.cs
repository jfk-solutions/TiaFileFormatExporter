using CommandLine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TiaFileFormatExporter;

namespace Exporter.Tests;

[TestClass]
public class ExportDefaultsTests
{
    [TestMethod]
    public void AllUsesBoundedStreamingExportWithoutRetainingConvertedObjects()
    {
        var options = Parse("--all");
        Assert.AreEqual(Math.Min(2, Environment.ProcessorCount), options.MaxParallelism);
        Assert.IsTrue(options.StreamAutomationXml);
        Assert.IsTrue(options.DisableConverterCache);
        Assert.IsFalse(options.OmitEmptyInheritedTypeChildren);
    }

    [TestMethod]
    [DataRow("0", 0)]
    [DataRow("6", 6)]
    public void AllPreservesExplicitWorkerLimits(string argument, int expected)
    {
        Assert.AreEqual(expected, Parse("--all", "--max-parallelism", argument).MaxParallelism);
    }

    [TestMethod]
    public void AllOnlyOmitsInheritedDeclarationsWhenExplicitlyRequested()
    {
        var options = Parse("--all", "--omit-empty-inherited-type-children");
        Assert.IsTrue(options.OmitEmptyInheritedTypeChildren);
        Assert.AreEqual(Math.Min(2, Environment.ProcessorCount), options.MaxParallelism);
        Assert.IsTrue(options.StreamAutomationXml);
        Assert.IsTrue(options.DisableConverterCache);
    }

    [TestMethod]
    public void IndividualExportsKeepTheirExistingDefaults()
    {
        var options = Parse("--plcblock");
        Assert.AreEqual(0, options.MaxParallelism);
        Assert.IsFalse(options.StreamAutomationXml);
        Assert.IsFalse(options.DisableConverterCache);
        Assert.IsFalse(options.OmitEmptyInheritedTypeChildren);
    }

    private static Options Parse(params string[] arguments)
    {
        using var parser = new Parser(settings => settings.HelpWriter = null);
        var result = parser.ParseArguments<Options>(new[] { "input.zap18", "--out", "output" }.Concat(arguments));
        Assert.AreEqual(ParserResultType.Parsed, result.Tag);
        var options = result.Value;
        options.ApplyExportDefaults();
        return options;
    }
}
