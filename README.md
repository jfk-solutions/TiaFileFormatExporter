# TiaFileFormatExporter
Sample Exporter wich uses the TiaFileFormat DLL

# Info 

This Project is mostly a Demo on how to use the Library. But it also can be used to export many parts of a TIA Project to different Files.
But you can also use the C# objects directly.

The tool should work on every OS where dotnet is supported.

# How to use

dotnet TiaFileFormatExporter.dll "TiaProject.ap20" --plcblock --out "d:\export"

The TIA Project can be in a Folder, or you can also directly export a compressed project.

Unpacked global libraries (`.alxx`) and archived global libraries (`.zalxx`) are
also accepted for every TIA version supported by `TiaFileFormat`. A standalone
library is exported automatically; select the desired object types as usual:

```powershell
dotnet TiaFileFormatExporter.dll "GlobalLibrary.zal20" --plcblock --out "D:\export"
dotnet TiaFileFormatExporter.dll "GlobalLibrary.al15_1" --all --out "D:\export"
```

Output is written below `<input name>/Library`. For an unpacked library, keep
the `.alxx` file with its accompanying `System` directory. `--lib` includes the
library embedded in a project, and `--all` includes both project and library
objects. The archive loader has no version-specific AL/ZAL extension whitelist.

## TiaGitHandler-compatible export

Compatibility is opt-in, so existing invocations and output defaults remain unchanged. The
following command exports PLC blocks, data types, tag tables, text lists, and force tables with
the same directory structure and normalization used by TiaGitHandler:

```powershell
TiaFileFormatExporter.exe "D:\path\Project.ap19" `
  --out "D:\path\export" `
  --tia-git-handler-compatible
```

For lower memory use and the best measured throughput on large projects, add:

```text
--indexed --prioritize-large-objects --max-parallelism 6 --no-cache --stream-automation-xml
```

The equivalent migration from the command-line TiaGitHandler defaults is:

```powershell
# Existing command; output is written below the current working directory.
TiaGitHandler.exe "D:\path\Project.ap19" false true

# Replacement; the output directory is explicit.
TiaFileFormatExporter.exe "D:\path\Project.ap19" `
  --out "D:\path\export" `
  --tia-git-handler-compatible `
  --indexed --prioritize-large-objects --max-parallelism 6 `
  --no-cache --stream-automation-xml
```

## Loading and performance options

- `--indexed` uses a supported TIA database index for lazy object loading. Missing and legacy
  indexes fall back to the sequential reader automatically, while malformed indexes still fail.
  The indexed reader can also be disabled simply by omitting this flag, including for unpacked
  projects.
- `--prioritize-large-objects` starts larger project objects earlier to reduce the parallel tail.
- `--max-parallelism <n>` limits concurrent object exports. `0` preserves the existing unbounded
  behavior.
- `--no-cache` disables the converted-object cache to reduce retained memory.
- `--no-segment-cache` additionally reduces retained blob buffers when using `--indexed`
  and a compatible library version. Segment data is reread on demand, so this trades export
  speed for a smaller memory footprint. Sequential loading retains its existing behavior.
- `--stream-automation-xml` writes PLC XML directly to the destination stream instead of first
  constructing a complete in-memory string.

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
