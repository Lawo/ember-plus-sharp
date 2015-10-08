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

$tempDirectory = [IO.Path]::Combine([IO.Path]::GetTempPath(), [IO.Path]::GetRandomFileName())
$packageDirectory = [IO.Path]::Combine($tempDirectory, [IO.Path]::GetRandomFileName())
[void][IO.Directory]::CreateDirectory($packageDirectory)
$packagePath = [IO.Path]::Combine($tempDirectory, $packageName)
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
    "The current package has been deleted successfully."
}

# Start Build
"Starting build..."
[void](Invoke-WebRequest "http://cimaster.lawo.de:8080/job/EmberPlusSharp_Windows_AnyCpu_Release/build?token=B497D548-5260-49C0-8932-5A40187E7866")

# Download package as soon as it's available
$downloadSucceeded = $false

while (!$downloadSucceeded)
{
    try
    {
        Invoke-WebRequest $requestPath -OutFile $packagePath
        $downloadSucceeded = $true
        "The new package has been downloaded successfully."
    }
    catch
    {
        "Waiting for the build to finish..."
        Start-Sleep -Seconds 10
    }
}

"Extracting package..."
7z x "$packagePath" -o"$packageDirectory"
$version = [Reflection.Assembly]::Loadfile([IO.Path]::Combine($packageDirectory, "GlowAnalyzerProxy.exe")).GetName().Version.ToString()
"Package version is " + $version + "."
$extension = [IO.Path]::GetExtension($packageName)
$newPackageName = $packageName.Replace($extension, "-" + $version + $extension)
Rename-Item $packagePath $newPackageName

"Cloning gh-pages..."
$ghpagesDirectory = [IO.Path]::Combine($tempDirectory, [IO.Path]::GetRandomFileName())
git clone -q --branch gh-pages "https://github.com/Lawo/ember-plus-sharp.git" "$ghpagesDirectory"
cd $ghpagesDirectory

"Removing current documentation..."
Remove-Item * -Recurse -Exclude .git

"Adding new documentation..."
$documentationPattern = [IO.Path]::Combine($packageDirectory, "Help", "*.*")
xcopy $documentationPattern . /e /q
git add -A

"Committing documentation changes..."
$tag = "v" + $version
$message = "Publish " + $tag + " documentation."
git commit -m $message

"Setting tag on master branch..."
git checkout -q master
git tag $tag

"Pushing everything..."
# git push --all origin

cd ..