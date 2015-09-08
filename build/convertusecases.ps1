﻿Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
#------------------------------

Import-Module "$BuildToolsRoot\modules\nuget.psm1" -DisableNameChecking
Import-Module "$BuildToolsRoot\modules\msbuild.psm1" -DisableNameChecking
Import-Module "$BuildToolsRoot\modules\transform.psm1" -DisableNameChecking
Import-Module "$BuildToolsRoot\modules\entrypoint.psm1" -DisableNameChecking

Task Build-ConvertUseCasesService -Precondition { $Metadata['ConvertUseCasesService'] -ne $null } {
	$packageInfo = Get-PackageInfo '2GIS.NuClear.AdvancedSearch.Tools.ConvertTrackedUseCases'
	$toolsDir = $packageInfo.VersionedDir

	$commonMetadata = $Metadata.Common
	$tempDir = Join-Path $commonMetadata.Dir.Temp 'ConvertUseCasesService'
	Copy-Item $toolsDir $tempDir -Force -Recurse

	$configFileName = '2GIS.NuClear.AdvancedSearch.Tools.ConvertTrackedUseCases.exe.config'
	$configFilePath = Join-Path $toolsDir $configFileName
	$transformedConfig = Get-TransformedConfig $configFilePath
	$tempConfigFilePath = Join-Path $tempDir $configFileName
	$transformedConfig.Save($tempConfigFilePath)

	Publish-Artifacts $tempDir 'ConvertUseCasesService'
}

Task Deploy-ConvertUseCasesService -Depends Build-ConvertUseCasesService -Precondition { $Metadata['ConvertUseCasesService'] -ne $null } {
	
	Load-WinServiceModule 'ConvertUseCasesService'
	Take-WinServiceOffline 'ConvertUseCasesService'
	Deploy-WinService 'ConvertUseCasesService'
}