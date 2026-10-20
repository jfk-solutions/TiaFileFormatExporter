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

    [TestMethod]
    public void AllIncludesMetadataByDefault()
    {
        var options = Parse("--all");
        Assert.IsTrue(options.ShouldExportAml);
        Assert.IsTrue(options.ShouldExportCrossReferences);
    }

    [TestMethod]
    [DataRow("--no-aml", false, true)]
    [DataRow("--no-xref", true, false)]
    public void AllCanExcludeEachMetadataExportIndependently(string exclusion, bool aml, bool xref)
    {
        var options = Parse("--all", exclusion);
        Assert.AreEqual(aml, options.ShouldExportAml);
        Assert.AreEqual(xref, options.ShouldExportCrossReferences);
        Assert.IsFalse(options.OmitEmptyInheritedTypeChildren);
    }

    [TestMethod]
    public void AllCanExcludeBothMetadataExportsWithoutChangingXmlDefaults()
    {
        var options = Parse("--all", "--no-aml", "--no-xref");
        Assert.IsFalse(options.ShouldExportAml);
        Assert.IsFalse(options.ShouldExportCrossReferences);
        Assert.IsFalse(options.OmitEmptyInheritedTypeChildren);
        Assert.IsTrue(options.StreamAutomationXml);
    }

    [TestMethod]
    public void ExclusionsOverrideExplicitMetadataSelections()
    {
        var options = Parse("--aml", "--xref", "--no-aml", "--no-xref");
        Assert.IsFalse(options.ShouldExportAml);
        Assert.IsFalse(options.ShouldExportCrossReferences);
    }

    [TestMethod]
    [DataRow("--aml", true, false)]
    [DataRow("--xref", false, true)]
    public void IndividualMetadataSelectionsRemainIndependent(string selection, bool aml, bool xref)
    {
        var options = Parse(selection);
        Assert.AreEqual(aml, options.ShouldExportAml);
        Assert.AreEqual(xref, options.ShouldExportCrossReferences);
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
