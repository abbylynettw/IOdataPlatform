# 从10月29日反编译版本恢复源代码
# 只恢复CS文件，XAML文件保留当前版本

$decompileRoot = "F:\Code\io-management-platform---wpfui\temp_decompile\oct29_release"
$projectRoot = "F:\Code\io-management-platform---wpfui\IODataPlatform"
$backupRoot = "F:\Code\io-management-platform---wpfui\backup_before_restore"

# 创建备份目录
if (!(Test-Path $backupRoot)) {
    New-Item -ItemType Directory -Path $backupRoot -Force | Out-Null
}

Write-Host "开始恢复10月29日版本..." -ForegroundColor Green

# 1. 备份当前工作区
Write-Host "`n1. 备份当前工作区..." -ForegroundColor Yellow
Copy-Item -Path $projectRoot -Destination $backupRoot -Recurse -Force
Write-Host "备份完成: $backupRoot" -ForegroundColor Green

# 2. 复制反编译的CS文件
Write-Host "`n2. 恢复CS文件..." -ForegroundColor Yellow

$csFiles = Get-ChildItem -Path "$decompileRoot\IODataPlatform" -Filter "*.cs" -Recurse

$copiedCount = 0
$skippedCount = 0

foreach ($file in $csFiles) {
    $relativePath = $file.FullName.Replace("$decompileRoot\IODataPlatform\", "")
    $targetPath = Join-Path $projectRoot $relativePath
    
    # 确保目标目录存在
    $targetDir = Split-Path $targetPath -Parent
    if (!(Test-Path $targetDir)) {
        New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
    }
    
    # 跳过自动生成的文件
    if ($relativePath -like "*\obj\*" -or $relativePath -like "*\bin\*") {
        $skippedCount++
        continue
    }
    
    # 复制文件
    Copy-Item -Path $file.FullName -Destination $targetPath -Force
    $copiedCount++
    
    if ($copiedCount % 10 -eq 0) {
        Write-Host "已恢复 $copiedCount 个文件..." -ForegroundColor Gray
    }
}

Write-Host "恢复完成！" -ForegroundColor Green
Write-Host "  - 恢复文件数: $copiedCount" -ForegroundColor Cyan
Write-Host "  - 跳过文件数: $skippedCount" -ForegroundColor Gray
Write-Host "`n备份位置: $backupRoot" -ForegroundColor Yellow
Write-Host "`n注意: XAML文件未被修改，保留了当前版本" -ForegroundColor Yellow
