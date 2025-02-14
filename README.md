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
 - --plcblock - PLC Blocks (DB, FB, FC, ...)
 - --plctagtable - PLC tag tables as csv
 - --images - images
 - --hmitagtable - HMI tag tables as csv
 - --plcwatchtable - PLC watchtables as csv
 - --winccscript - WinCC Scripts (vb, js, c)
 - --wincctagtable - WinCC Tag Tables as CSV
 - --screens - export WinCC/WinCCUnified screens as HTML
 - --snapshot - generate a Image of the Screen HTML via Chrome

# License 

This demo project is MIT licensed. The TiaFileFormat DLL has a proprietary license and needs to be obtained separately.