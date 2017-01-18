# Copyright 2012-2017 Lawo AG (http://www.lawo.com).
# Distributed under the Boost Software License, Version 1.0.
# (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)

$ErrorActionPreference = "Stop"
"Build and Publish Ember+ Sharp"
$packageName = "EmberPlusSharp.zip"

# Read Credentials
$password = Read-Host 'cimaster Password'
$gitHubApiKey = Read-Host 'GitHub API Key'

# Delete existing package
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
[void](Invoke-WebRequest "http://cimaster.lawo.de:8080/job/EmberPlusSharp/build?token=B497D548-5260-49C0-8932-5A40187E7866")

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
$newPackagePath = [IO.Path]::Combine($tempDirectory, $newPackageName);
Rename-Item $packagePath $newPackageName

"Cloning gh-pages..."
$ghpagesDirectory = [IO.Path]::Combine($tempDirectory, [IO.Path]::GetRandomFileName())
git clone -q --branch gh-pages "https://github.com/Lawo/ember-plus-sharp.git" "$ghpagesDirectory"
cd $ghpagesDirectory

"Removing current documentation..."
Remove-Item * -Recurse -Exclude .git

"Adding new documentation..."
$documentationPattern = [IO.Path]::Combine($packageDirectory, "Documentation", "*.*")
xcopy $documentationPattern . /e /q
git add -A

"Committing documentation changes..."
$tag = "v" + $version
$message = "Publish " + $tag + " documentation"
git commit -m $message

"Setting tag on master branch..."
git checkout -q master
git tag $tag

"Pushing everything..."
git push -q origin refs/heads/* refs/tags/*

"Creating GitHub Release..."
$auth = 'Basic ' + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes($gitHubApiKey + ":x-oauth-basic"))
$releaseData = @{ tag_name = $tag; name = $tag; draft = $true; prerelease = $false; }

$releaseParams = @{
    Uri = "https://api.github.com/repos/Lawo/ember-plus-sharp/releases";
    Method = 'POST';
    Headers = @{ Authorization = $auth; }
    ContentType = 'application/json';
    Body = (ConvertTo-Json $releaseData -Compress)
}

$result = Invoke-RestMethod @releaseParams

"Uploading Zip..."
$uploadUri = $result | Select -ExpandProperty upload_url
$uploadUri = $uploadUri -replace '\{\?name,label\}', "?name=$newPackageName"

$uploadParams = @{
    Uri = $uploadUri;
    Method = 'POST';
    Headers = @{ Authorization = $auth }
    ContentType = 'application/zip';
    InFile = $newPackagePath
}

Invoke-RestMethod @uploadParams
