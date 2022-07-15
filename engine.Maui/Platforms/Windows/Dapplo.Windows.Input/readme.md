This is package [Dapplo.Windows.Input](https://www.nuget.org/packages/Dapplo.Windows.Input).
The nuget reference is is not functional due to how the netstandard package is setup which is not compatible with iOS, Andriod, and MacCatalyst.  As a result, this is a direct linked dependency.
[Project site](https://www.dapplo.net/blocks/Dapplo.Windows)

The following error occurs on build:
```
Severity	Code	Description	Project	File	Line	Suppression State
Error	NETSDK1136	The target platform must be set to Windows (usually by including '-windows' in the TargetFramework property) when using Windows Forms or WPF, or referencing projects or packages that do so.	MauiApp3	C:\Program Files\dotnet\sdk\6.0.301\Sdks\Microsoft.NET.Sdk\targets\Microsoft.NET.Sdk.DefaultItems.Shared.targets	250	
```
