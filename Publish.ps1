$ErrorActionPreference = "Stop"
"Build and Publish Ember+ Sharp"

# Delete existing package
$packageName = "EmberPlusSharp_Windows_AnyCpu_Release.zip"
$password = Read-Host 'cimaster Password'
echo open cimaster.lawo.de >temp.ftp
echo cimaster >>temp.ftp
echo $password >>temp.ftp
echo cd /var/www/ember-plus-sharp/trunk/bin >>temp.ftp
echo del $packageName >>temp.ftp
echo quit >>temp.ftp
ftp -s:temp.ftp
del temp.ftp

$packageDirectory = [IO.Path]::Combine([IO.Path]::GetTempPath(), [IO.Path]::GetRandomFileName())
[void][IO.Directory]::CreateDirectory($packageDirectory)
$packagePath = [IO.Path]::Combine($packageDirectory, $packageName)
$requestPath = "http://cimaster.lawo.de/ember-plus-sharp/trunk/bin/" + $packageName

# Make sure package is no longer present
try
{
    Invoke-WebRequest $requestPath -OutFile $packagePath
    "FATAL: Package deletion failed."
    exit 
}
catch
{
}

# Start Build
[void](Invoke-WebRequest "http://cimaster.lawo.de:8080/job/EmberPlusSharp_Windows_AnyCpu_Release/build?token=B497D548-5260-49C0-8932-5A40187E7866")

# Download package as soon as it's available
$downloadSucceeded = $false

while (!$downloadSucceeded)
{
    try
    {
        Invoke-WebRequest $requestPath -OutFile $packagePath
        $downloadSucceeded = $true
    }
    catch
    {
        Start-Sleep -Seconds 10
    }
}

7z x "$packagePath" -o"$packageDirectory"
