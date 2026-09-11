# TiaFileFormatExporter
Sample Exporter wich uses the TiaFileFormat DLL

# Info 

This Project is mostly a Demo on how to use the Library. But it also can be used to export many parts of a TIA Project to different Files.
But you can also use the C# objects directly.

The tool should work on every OS where dotnet is supported.

# How to use

dotnet TiaFileFormatExporter.dll "TiaProject.ap20" --plcblock --out "d:\export"

The TIA Project can be in a Folder, or you can also directly export a compressed project.

The Options to export are:

 - --all - export everything the program supports
 - --aml - project devices and networks as AutomationML
 - --xref - project cross-references as JSON
 - --plcblock - PLC Blocks (DB, FB, FC, ...)
 - --plctagtable - PLC tag tables as csv
 - --images - images
 - --base64images - embed images in screen HTML as base64 data URIs
 - --hmitagtable - HMI tag tables as csv
 - --plcwatchtable - PLC watchtables as csv
 - --winccscript - WinCC Scripts (vb, js, c)
 - --wincctagtable - WinCC Tag Tables as CSV
 - --screens - export WinCC/WinCCUnified screens as HTML
 - --snapshot - generate a Image of the Screen HTML via Chrome

For a self-contained screen HTML export, use `--screens --base64images`. The existing
`--images` option exports project images as separate files instead.

For device AML and cross-reference exports, use:

```sh
dotnet TiaFileFormatExporter.dll "TiaProject.ap20" --aml --xref --out "d:\export"
```

These exports write `Project/Devices.aml`, `Project/Devices.diagnostics.json`, and
`Project/CrossReferences.json` beneath the project output folder. AML contains all
convertible project devices and their networks; its diagnostics file lists skipped
devices and conversion messages. Cross-reference JSON contains objects, relations,
locations, addresses, and completeness diagnostics, with catalog IDs linking records.
It uses the project's XRef database when available and otherwise scans project data;
the JSON records the source and any fallback diagnostics. Both exports run with
`--all`, honor `--noprojectname` and `--replacepath`, and are regenerated on each run.

# License 

This demo project is MIT licensed. The TiaFileFormat DLL has a proprietary license and needs to be obtained separately.
