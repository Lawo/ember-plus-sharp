powershell -Command "Invoke-WebRequest https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile '%~dp0nuget.exe'"
"%~dp0nuget.exe" restore "%~dp0Lawo.EmberPlusSharp.sln"
"%ProgramFiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe" Lawo.EmberPlusSharp.sln /t:Clean /p:Configuration=Release
"%ProgramFiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe" Lawo.EmberPlusSharp.sln /t:Build /p:Configuration=Release
