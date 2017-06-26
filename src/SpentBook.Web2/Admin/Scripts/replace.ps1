param ([Parameter(Mandatory=$true)][string]$find,[Parameter(Mandatory=$true)][string]$projectName)
gci -r -include "*.cs","*.sln","*.csproj","*.json","*.cshtml" | 
foreach-object {
  $fileName = $_.fullname;
  $file = Get-Content $fileName
  $containsWord = $file | %{$_ -match $find};

  If($containsWord -contains $true) {
    echo $fileName;
    ($file)  |
    foreach-object {
      $_ -replace $find,$projectName 
    } | set-content $fileName
  }
}