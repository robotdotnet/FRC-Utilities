# Thanks to Andrew Lock for help with this script
# http://andrewlock.net/publishing-your-first-nuget-package-with-appveyor-and-myget/

param (
  [switch]$release = $false,
  [switch]$build = $false,
  [switch]$test = $false,
  [switch]$skipNtCore = $false,
  [switch]$updatexml = $false,
  [switch]$pack = $false,
  [switch]$debug = $false
)

<#  
.SYNOPSIS
    You can add this to you build script to ensure that psbuild is available before calling
    Invoke-MSBuild. If psbuild is not available locally it will be downloaded automatically.
#>
function EnsurePsbuildInstalled{  
    [cmdletbinding()]
    param(
        [string]$psbuildInstallUri = 'https://raw.githubusercontent.com/ligershark/psbuild/master/src/GetPSBuild.ps1'
    )
    process{
        if(-not (Get-Command "Invoke-MsBuild" -errorAction SilentlyContinue)){
            'Installing psbuild from [{0}]' -f $psbuildInstallUri | Write-Verbose
            (new-object Net.WebClient).DownloadString($psbuildInstallUri) | iex
        }
        else{
            'psbuild already loaded, skipping download' | Write-Verbose
        }

        # make sure it's loaded and throw if not
        if(-not (Get-Command "Invoke-MsBuild" -errorAction SilentlyContinue)){
            throw ('Unable to install/load psbuild from [{0}]' -f $psbuildInstallUri)
        }
    }
}

# Taken from psake https://github.com/psake/psake

<#  
.SYNOPSIS
  This is a helper function that runs a scriptblock and checks the PS variable $lastexitcode
  to see if an error occcured. If an error is detected then an exception is thrown.
  This function allows you to run command-line programs without having to
  explicitly check the $lastexitcode variable.
.EXAMPLE
  exec { svn info $repository_trunk } "Error executing SVN. Please verify SVN command-line client is installed"
#>
function Exec  
{
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][scriptblock]$cmd,
        [Parameter(Position=1,Mandatory=0)][string]$errorMessage = ($msgs.error_bad_command -f $cmd)
    )
    & $cmd
    if ($lastexitcode -ne 0) {
        throw ("Exec: " + $errorMessage)
    }
}

If (Test-Path Env:APPVEYOR_REPO_TAG_NAME) {
  if (($env:APPVEYOR_REPO_TAG_NAME).Contains("-") -eq $false) {
     $release = $true
     echo "Tagged Release"
    }
    echo "Tag but not release"
}
echo "Not Release"


If ($release) {
 $revision =  ""
} Else {
 $revision = @{ $true = $env:APPVEYOR_BUILD_NUMBER; $false = 1 }[$env:APPVEYOR_BUILD_NUMBER -ne $NULL];
 $revision = "--version-suffix={0:D4}" -f [convert]::ToInt32($revision, 10)
}

If ($debug) {
 $configuration = "-c=Debug"
} ElseIf ($env:APPVEYOR) {
 $configuration = "-c=CI"
} Else {
$configuration = "-c=Release"
}

If ($debug) {
     $libLoc = "bin\Debug"
    } ElseIf ($env:APPVEYOR) {
     $libLoc = "bin\CI"
    } Else {
    $libLoc = "bin\Release"
    }
    
    echo $libLoc

function UploadAppVeyorTestResults {
 # upload results to AppVeyor
  $wc = New-Object 'System.Net.WebClient'
  $wc.UploadFile("https://ci.appveyor.com/api/testresults/nunit3/$($env:APPVEYOR_JOB_ID)", (Resolve-Path .\TestResult.xml))
}

function Build {
  exec { & dotnet restore }
  echo $configuration
  
  exec { & dotnet build src\NativeLibraryUtilities $configuration $revision }
}

function Pack { 
  if (Test-Path .\artifacts) { Remove-Item .\artifacts -Force -Recurse }

  exec { & dotnet pack src\NativeLibraryUtilities $configuration $revision --no-build -o .\artifacts }
  

  if ($env:APPVEYOR) {
    Get-ChildItem .\artifacts\*.nupkg | % { Push-AppveyorArtifact $_.FullName -FileName $_.Name }
  }
}

function UpdateXml {    
    # Copy built files
    if ((Test-Path .\buildTemp) -eq $false) {
  md .\buildTemp
 }
 
  .\NuGet.exe install EWSoftware.SHFB -Version 2016.4.9 -o buildTemp

   .\NuGet.exe install EWSoftware.SHFB.NETFramework -Version 4.6 -o buildTemp
   
   Copy-Item src\NativeLibraryUtilities\$libLoc\net451\NativeLibraryUtilities.dll buildTemp\NativeLibraryUtilities.dll
   Copy-Item src\NativeLibraryUtilities\$libLoc\net451\NativeLibraryUtilities.xml buildTemp\NativeLibraryUtilities.xml
   
   EnsurePsbuildInstalled
   
   if ($debug) { 
   $sandConfig = "Debug"
   } else {
    $sandConfig = "Release"
   }
   
   & 'C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe' Sandcastle\NetworkTables.NetCore.shfbproj /property:Configuration=Release /v:m

   Copy-Item Sandcastle\Help\NativeLibraryUtilities.xml src\NativeLibraryUtilities\$libLoc\net451\NativeLibraryUtilities.xml
   Copy-Item Sandcastle\Help\NativeLibraryUtilities.xml src\NativeLibraryUtilities\$libLoc\netstandard1.3\NativeLibraryUtilities.xml
}

if ($release) {
 if ((Test-Path .\buildTemp) -eq $false) {
  md .\buildTemp
 }

 # Remove beta defintion from project.json files
  Copy-Item src\NativeLibraryUtilities\project.json buildTemp\NativeLibraryUtilities.projectjson
  
  $netTablesJson = Get-Content 'src\NativeLibraryUtilities\project.json' -raw | ConvertFrom-Json
  $netTablesJson.version = $netTablesJson.version.Substring(0, $netTablesJson.version.IndexOf("-"))
  $netTablesJson | ConvertTo-Json -Depth 5 | Set-Content 'src\NativeLibraryUtilities\project.json'
 
}

if ($build) {
 Build
}

if ($test) {
 #Test
}

if ($updatexml) {
 #UpdateXml
}

if ($pack) {
 Pack
}

if ($release) {
 # Add beta definition back into project.json
 Copy-Item buildTemp\NativeLibraryUtilities.projectjson src\NativeLibraryUtilities\project.json
 
 Remove-Item buildTemp\NativeLibraryUtilities.projectjson
}