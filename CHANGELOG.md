# Road map

- [ ] ...

# Change log

These are the changes to each version that has been released
on the official Visual Studio extension gallery.

## 2.2.0
- [x] Removed useless files
- [x] Refactoring / code optimizing
- [x] Added unit tests
- [x] (Fixed) Problem with types defined in the external libs
- [x] Renamed Options.GlobalScope into Options.DeclareModule (because it is more clear and transparent name)
- [x] Removed Options.Export (useless, because not needed for modules and required without them)
- [x] (Fixed) When Options.DeclareModule is false no namespaces were added
- [x] Dynamic build module name from server dto's namespace
- [x] Added an Options.EOLType (to choose the EOL type Windows/Unix)
- [x] Added the Options.IndentTab and Options.IndentTabSize (to convert the tab indentations into spaces)
- [x] Added support while converting for "Byte -> number" and "GUID -> string"

## 2.1.0

- [x] Make it build locally (Visual Studio 2017 Professional)
- [x] Reconfigured the Build Server
- [x] Added Options.Export, to export classes/interfaces/enums instead of cover them by Module

## 2.0.0

- [x] Discard support for VB
- [x] Strengthen support for enum members
- [x] Set default namespace to Server.Dtos
- [x] Set generated file name to *.generated.d.ts
- [x] Change some default settings for generated d.ts code style

## 1.1

- [x] Option for using producing &lt;filename&gt;.cs.d.ts

## 1.0

- [x] Initial release
- [x] Single File Generator
- [x] Command to toggle custom tool
- [x] Update readme file with screenshots etc.
- [x] Website project support
