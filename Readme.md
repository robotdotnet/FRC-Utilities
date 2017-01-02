# NativeLibraryUtilities

**Build Status**

| Windows                 | NuGet                 |
| ------------------------|-----------------------|
| [![Build status][1]][2] | [![NuGet][7]][8]      |

[1]: https://ci.appveyor.com/api/projects/status/q0ecwijf3rb3k98y?svg=true
[2]: https://ci.appveyor.com/project/robotdotnet/nativelibraryutilities
[7]: https://img.shields.io/nuget/vpre/NativeLibraryUtilities.svg
[8]: https://www.nuget.org/packages/NativeLibraryUtilities

NativeLibraryUtilities is a set of utilities designed for easy extraction and loading of native libraries. It is designed as a much more flexible and usable replacement for P/Invoke

Additional features it has includes:
* Boot time check for all native symbols
* Support for extracting native libraries from embedded resource
* Support for detecting OS and Bitness of system at runtime, and selecting the proper library to loading

The only missing feature over P/Invoke is function overloading is not directly supported. Instead, 2 functions with different names must be used instead.

## Installation
Download from Nuget. Supported platforms equals anything that supports .NET 4.5 or .NET Standard 1.5. In addition, OS detection is currently supported for Windows, Mac and Linux, with custom operating systems being fairly easy to implement.

## Usage
Coming Soon!

## License
See [LICENSE.txt](LICENSE.txt)

## Contributors

Thad House (@thadhouse)