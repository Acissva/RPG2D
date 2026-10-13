$ErrorActionPreference = 'Stop'
$engineProjectDir = 'E:\Tuanjie\Projects\2D\RPG2D'
$packageDir = Join-Path $engineProjectDir 'Library/PackageCache/com.h8man.2d.navmeshplus@8fb73cc139'
$sourceFile = Join-Path $packageDir 'NavMeshComponents/Scripts/NavMeshBuilder2d.cs'
$original = [IO.File]::ReadAllText($sourceFile)
$oldBlock = @'
#if UNITY_2023_1_OR_NEWER
            if (collider.compositeOperation != Collider2D.CompositeOperation.None)
#else
            if (collider.usedByComposite)
#endif
            {
                collider = collider.GetComponent<CompositeCollider2D>();
            }
'@
$newBlock = @'
            if (collider == null)
            {
                return;
            }

            Collider2D originalCollider = collider;
#if UNITY_2023_1_OR_NEWER || TUANJIE_1_9_OR_NEWER
            if (collider.compositeOperation != Collider2D.CompositeOperation.None)
            {
                collider = collider.composite;
            }
#else
            if (collider.usedByComposite)
            {
                collider = collider.GetComponent<CompositeCollider2D>();
            }
#endif
            if (collider == null)
            {
                Debug.LogWarning($"NavMeshPlus: {originalCollider.name} is merged but has no CompositeCollider2D; skipping source.", originalCollider);
                return;
            }
'@
$normalized = $original.Replace("`r`n", "`n")
$oldBlock = $oldBlock.Replace("`r`n", "`n")
$newBlock = $newBlock.Replace("`r`n", "`n")
if ([regex]::Matches($normalized, [regex]::Escape($oldBlock)).Count -ne 1) { throw '源文件与已核查版本不一致，停止生成补丁。' }
$patched = $normalized.Replace($oldBlock, $newBlock).Replace("`n", "`r`n")
[IO.File]::WriteAllText((Join-Path $PSScriptRoot 'NavMeshBuilder2d.cs'), $patched, [Text.UTF8Encoding]::new($true))
$checkDir = Join-Path $PSScriptRoot '核查'
[void](New-Item -ItemType Directory -Path $checkDir -Force)
[xml]$projectXml = Get-Content -LiteralPath (Join-Path $engineProjectDir 'RPG2D.csproj') -Raw
$namespaceUri = 'http://schemas.microsoft.com/developer/msbuild/2003'
foreach ($item in @($projectXml.SelectNodes('//*[@Include]'))) {
    if ($item.LocalName -in @('Compile', 'ProjectReference', 'Analyzer', 'AdditionalFiles', 'None') -or ($item.LocalName -eq 'Reference' -and $item.Include -eq 'NavMeshPlus')) {
        [void]$item.ParentNode.RemoveChild($item)
    }
}
foreach ($hintPath in $projectXml.SelectNodes('//*[local-name()="HintPath"]')) {
    if (![IO.Path]::IsPathRooted($hintPath.InnerText)) { $hintPath.InnerText = Join-Path $engineProjectDir $hintPath.InnerText }
}
foreach ($assemblyName in $projectXml.SelectNodes('//*[local-name()="AssemblyName"]')) { $assemblyName.InnerText = 'NavMeshPlus' }
foreach ($outputPath in $projectXml.SelectNodes('//*[local-name()="OutputPath"]')) { $outputPath.InnerText = Join-Path $checkDir 'bin/' }
foreach ($define in $projectXml.SelectNodes('//*[local-name()="DefineConstants"]')) { $define.InnerText += ';IS_TERRAIN_USED' }
foreach ($propertyName in @('BaseIntermediateOutputPath', 'IntermediateOutputPath')) {
    $property = $projectXml.CreateElement($propertyName, $namespaceUri)
    $property.InnerText = Join-Path $checkDir 'obj/'
    [void]$projectXml.Project.PropertyGroup[0].AppendChild($property)
}
$sources = & rg --files (Join-Path $packageDir 'NavMeshComponents/Scripts') -g '*.cs'
$sourceGroup = $projectXml.CreateElement('ItemGroup', $namespaceUri)
foreach ($source in $sources) {
    $compile = $projectXml.CreateElement('Compile', $namespaceUri)
    $compile.SetAttribute('Include', $(if ([IO.Path]::GetFileName($source) -eq 'NavMeshBuilder2d.cs') { Join-Path $PSScriptRoot 'NavMeshBuilder2d.cs' } else { $source }))
    [void]$sourceGroup.AppendChild($compile)
}
[void]$projectXml.Project.AppendChild($sourceGroup)
$projectXml.Save((Join-Path $checkDir 'NavMeshPlus.csproj'))
Write-Output "已在设计目录生成单处兼容补丁与 $($sources.Count) 个运行时源文件的编译核查项目。开发工程未修改。"
