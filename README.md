# BumpVersions
Small tool to bump the version numbers of your project

License and source: https://github.com/jrnker/BumpVersions

### Description
It will search the submitted path for AssemblyInfo.* files, parse these and increase version numbers as specified.

It's meant to be implemented into your Visual Studio projects build flow to organize your executables and such with proper version numbers.

Built on .Net 4.5.2

### Usage
  bumpversions.exe [pathtosearch] [-major|-minor|-build|-revision] [-reset]
  
  Note: if submitting a path with quotation marks, e.g. a path with spaces, then place space between the end of the path and the second quotation mark.

Example with normal path:
```sh
  bumpversions.exe C:\repo\BumpVersions\ -build -reset
```
Example with path with spaces:
```sh
  bumpversions.exe "C:\my repo\BumpVersions\ " -build -reset
```

### Pre compiled version
..can be found in https://github.com/jrnker/BumpVersions/tree/master/bin/Release

### Implement in your project (C#)
* Open your project settings and go to Build Events
* In *Pre-build event command line:*, insert the below code, with adjusted paths to bumpversions.exe

Pre-build event command line:
```sh
if "$(ConfigurationName)"=="Debug" Goto Debug
if "$(ConfigurationName)"=="Release" Goto Release
Goto End

:Debug
C:\repo\Distribution\tools\bumpversions.exe "$(ProjectDir) " -revision -reset
Goto End

:Release
C:\repo\Distribution\tools\bumpversions.exe "$(ProjectDir) " -build -reset
Goto End

:End
```

With this configuration, your project will increase like this:

For debug mode, v1.0.0.0, v1.0.0.1, v1.0.0.2

For release mode, v1.0.0.0, v1.0.1.0, v1.0.2.0
