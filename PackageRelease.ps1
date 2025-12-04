# 项目打包脚本 - 移动到桌面
$WorkspaceRoot = $PSScriptRoot
$DatabaseScriptsDir = Join-Path $WorkspaceRoot "DatabaseScripts"
$DocsDir = Join-Path $WorkspaceRoot "docs"
$PublishDir = "E:\Client_Publish"
$DesktopPath = [Environment]::GetFolderPath("Desktop")
$Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$DestDir = Join-Path $DesktopPath "Release_Package_$Timestamp"

Write-Host "`n========================================" -ForegroundColor Magenta
Write-Host "  项目打包脚本 - $Timestamp" -ForegroundColor Magenta
Write-Host "========================================`n" -ForegroundColor Magenta

# 创建目标目录
New-Item -ItemType Directory -Path $DestDir -Force | Out-Null
Write-Host "创建目录: $DestDir`n" -ForegroundColor Green

# 复制数据库脚本
if (Test-Path $DatabaseScriptsDir) {
    Write-Host "复制数据库脚本..." -ForegroundColor Cyan
    Copy-Item -Path $DatabaseScriptsDir -Destination (Join-Path $DestDir "DatabaseScripts") -Recurse -Force
    Write-Host "  完成" -ForegroundColor Green
} else {
    Write-Host "跳过: 数据库脚本目录不存在" -ForegroundColor Yellow
}

# 复制文档
if (Test-Path $DocsDir) {
    Write-Host "复制文档..." -ForegroundColor Cyan
    Copy-Item -Path $DocsDir -Destination (Join-Path $DestDir "Docs") -Recurse -Force
    Write-Host "  完成" -ForegroundColor Green
} else {
    Write-Host "跳过: 文档目录不存在" -ForegroundColor Yellow
}

# 复制发布程序
if (Test-Path $PublishDir) {
    Write-Host "复制发布程序..." -ForegroundColor Cyan
    Copy-Item -Path $PublishDir -Destination (Join-Path $DestDir "Client_Publish") -Recurse -Force
    Write-Host "  完成" -ForegroundColor Green
} else {
    Write-Host "跳过: 发布目录不存在" -ForegroundColor Yellow
}

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "  完成！文件已复制到桌面" -ForegroundColor Green
Write-Host "  位置: $DestDir" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Green

Start-Process $DestDir
