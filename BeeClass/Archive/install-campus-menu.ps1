$ErrorActionPreference = 'Stop'
$scenePath = 'Assets/Scenes/Level0-UI.unity'
if (Test-Path Library/CampusQuestMenu.done) { exit }
New-Item -ItemType Directory -Force Archive/CampusQuestMenu | Out-Null
Copy-Item -LiteralPath $scenePath -Destination Archive/CampusQuestMenu/Level0-before-menu.unity
$source = [IO.File]::ReadAllText((Join-Path (Get-Location) $scenePath))
$blocks = [regex]::Split($source, '(?m)(?=^--- !u!)')
$map = @{}
foreach ($block in $blocks) { if ($block -match '^--- !u!\d+ &(\d+)') { $map[$Matches[1]] = $block } }
$canvas = $map['639344293']
$oldChildren = [regex]::Match($canvas, '(?s)m_Children:.*?m_Father:').Value
foreach ($match in [regex]::Matches($oldChildren, 'fileID: (\d+)')) {
    $rt = $map[$match.Groups[1].Value]
    if ($rt -match 'm_GameObject: \{fileID: (\d+)\}') {
        $go = $Matches[1]
        if ($go -ne '2080239947' -and $go -ne '2106000000') { $map[$go] = $map[$go] -replace 'm_IsActive: 1','m_IsActive: 0' }
    }
}
$canvas = $canvas -replace '  - \{fileID: 2080239948\}\r?\n','' -replace '  - \{fileID: 2106000001\}\r?\n',''
$canvas = $canvas -replace '  m_Father:',"  - {fileID: 2118000001}`n  m_Father:"
$map['639344293'] = $canvas
$map['3366'] = ''
foreach ($id in @('2080239948','2106000001')) {
    $rt = $map[$id] -replace 'm_Father: \{fileID: 639344293\}','m_Father: {fileID: 2118000001}'
    if ($id -eq '2080239948') { $min='0.0341796875, y: 0.519514'; $max='0.283203125, y: 0.637467' }
    else { $min='0.04638671875, y: 0.215091'; $max='0.2587890625, y: 0.30876' }
    $rt = $rt -replace 'm_AnchorMin: .*',("m_AnchorMin: {x: " + $min + '}') -replace 'm_AnchorMax: .*',("m_AnchorMax: {x: " + $max + '}')
    $rt = $rt -replace 'm_AnchoredPosition: .*','m_AnchoredPosition: {x: 0, y: 0}' -replace 'm_SizeDelta: .*','m_SizeDelta: {x: 0, y: 0}'
    $map[$id] = $rt
}
foreach ($id in @('2080239950','2106000003')) { $map[$id] = $map[$id] -replace 'm_Color: .*','m_Color: {r: 1, g: 1, b: 1, a: 0}' }
foreach ($id in @('2080239949','2106000002')) { $map[$id] = $map[$id] -replace 'm_Transition: 1','m_Transition: 0' }
foreach ($id in @('481862742','2106000011')) {
    if ($map[$id] -match 'm_GameObject: \{fileID: (\d+)\}') { $go=$Matches[1]; $map[$go]=$map[$go] -replace 'm_IsActive: 1','m_IsActive: 0' }
}
$guid = [guid]::NewGuid().ToString('N')
if (Test-Path Assets/Art/Menu/CampusQuestMenu.png.meta) { $guid = ([regex]::Match([IO.File]::ReadAllText((Join-Path (Get-Location) 'Assets/Art/Menu/CampusQuestMenu.png.meta')), 'guid: (\w+)')).Groups[1].Value }
else { [IO.File]::WriteAllText((Join-Path (Get-Location) 'Assets/Art/Menu/CampusQuestMenu.png.meta'), "fileFormatVersion: 2`nguid: $guid`n") }
$newGo = $map['2080239947'] -replace '2080239947','2118000000' -replace '2080239948','2118000001' -replace '2080239949','2118000004' -replace '2080239950','2118000002' -replace '2080239951','2118000003' -replace 'm_Name: Start Button','m_Name: Campus Quest Image'
$newRect = $map['2080239948'] -replace '2080239948','2118000001' -replace '2080239947','2118000000' -replace '(?s)  m_Children:.*?  m_Father:','  m_Children: [ ]\n  m_Father:'
$newRect = $newRect.Replace('[ ]\n', "`n  - {fileID: 2080239948}`n  - {fileID: 2106000001}`n") -replace 'm_Father: \{fileID: 2118000001\}','m_Father: {fileID: 639344293}' -replace 'm_AnchorMin: .*','m_AnchorMin: {x: 0.5, y: 0.5}' -replace 'm_AnchorMax: .*','m_AnchorMax: {x: 0.5, y: 0.5}'
$raw = $map['2080239950'] -replace '2080239950','2118000002' -replace '2080239947','2118000000' -replace 'fe87c0e1cc204ed48ad3b37840f39efc','1344c3c82d62a2a41a3576d8abb8e3ea' -replace 'UnityEngine.UI.Image','UnityEngine.UI.RawImage' -replace 'm_Color: .*','m_Color: {r: 1, g: 1, b: 1, a: 1}' -replace 'm_RaycastTarget: 1','m_RaycastTarget: 0'
$raw = [regex]::Replace($raw, '(?s)  m_Sprite:.*', "  m_Texture: {fileID: 2800000, guid: $guid, type: 3}`n  m_UVRect: {serializedVersion: 2, x: 0, y: 0, width: 1, height: 1}`n")
$renderer = $map['2080239951'] -replace '2080239951','2118000003' -replace '2080239947','2118000000'
$aspect = @'
--- !u!114 &2118000004
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 2118000000}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 86710e43de46f6f4bac7c8e50813a599, type: 3}
  m_Name:
  m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.AspectRatioFitter
  m_AspectMode: 3
  m_AspectRatio: 1.7769177

'@
$result = ''
foreach ($block in $blocks) {
    if ($block -match '^--- !u!\d+ &(\d+)') { $result += $map[$Matches[1]] } else { $result += $block }
}
$result = $result -replace 'gameScene: Assets/Scenes/level1-Campus.unity','gameScene: Assets/Scenes/level1-Final.unity'
$result += $newGo + $newRect + $raw + $renderer + $aspect
[IO.File]::WriteAllText((Join-Path (Get-Location) $scenePath), $result)
$buildPath = Join-Path (Get-Location) 'ProjectSettings/EditorBuildSettings.asset'
$build = [IO.File]::ReadAllText($buildPath)
if ($build -notmatch 'path: Assets/Scenes/Level0-UI.unity') {
    $build = $build -replace '  m_configObjects:', "  - enabled: 1`n    path: Assets/Scenes/Level0-UI.unity`n    guid: 22dfccb650856104886f5ce99f17c8cf`n  m_configObjects:"
    [IO.File]::WriteAllText($buildPath,$build)
}
[IO.File]::WriteAllText((Join-Path (Get-Location) 'Library/CampusQuestMenu.done'), 'Menu scene installed on disk; Start -> level1-Final')
[IO.File]::WriteAllText((Join-Path (Get-Location) 'Library/Level0MenuSync.v3.done'), 'Campus Quest menu')
