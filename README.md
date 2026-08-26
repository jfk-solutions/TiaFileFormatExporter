# TiaFileFormatExporter
Sample Exporter wich uses the TiaFileFormat DLL

# Info 

This Project is mostly a Demo on how to use the Library. But it also can be used to export many parts of a TIA Project to different Files.
But you can also use the C# objects directly.

The tool should work on every OS where dotnet is supported.

# How to use

dotnet TiaFileFormatExporter.dll "TiaProject.ap20" --plcblock --out "d:\export"

The TIA Project can be in a Folder, or you can also directly export a compressed project.

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
- `--stream-automation-xml` writes PLC XML directly to the destination stream instead of first
  constructing a complete in-memory string.

The Options to export are:

 - --all - export everything the program supports
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

# License 

This demo project is MIT licensed. The TiaFileFormat DLL has a proprietary license and needs to be obtained separately.
