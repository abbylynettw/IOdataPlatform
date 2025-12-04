# 项目打包脚本
$WorkspaceRoot = $PSScriptRoot
$DatabaseScriptsDir = Join-Path $WorkspaceRoot "DatabaseScripts"
$DocsDir = Join-Path $WorkspaceRoot "docs"
$PublishDir = "E:\Client_Publish"
$DesktopPath = [Environment]::GetFolderPath("Desktop")
$Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$TempDir = Join-Path $DesktopPath "Release_Package_$Timestamp"

Write-Host "`n========================================" -ForegroundColor Magenta
Write-Host "  项目打包脚本 - $Timestamp" -ForegroundColor Magenta
Write-Host "========================================`n" -ForegroundColor Magenta

# 创建临时目录
New-Item -ItemType Directory -Path $TempDir -Force | Out-Null
Write-Host "创建临时目录: $TempDir`n" -ForegroundColor Green

# 复制数据库脚本
if (Test-Path $DatabaseScriptsDir) {
    Write-Host "复制数据库脚本..." -ForegroundColor Cyan
    Copy-Item -Path $DatabaseScriptsDir -Destination (Join-Path $TempDir "DatabaseScripts") -Recurse -Force
} else {
    Write-Host "跳过: 数据库脚本目录不存在" -ForegroundColor Yellow
}

# 复制文档
if (Test-Path $DocsDir) {
    Write-Host "复制文档..." -ForegroundColor Cyan
    Copy-Item -Path $DocsDir -Destination (Join-Path $TempDir "Docs") -Recurse -Force
} else {
    Write-Host "跳过: 文档目录不存在" -ForegroundColor Yellow
}

# 复制发布程序
if (Test-Path $PublishDir) {
    Write-Host "复制发布程序..." -ForegroundColor Cyan
    Copy-Item -Path $PublishDir -Destination (Join-Path $TempDir "Client_Publish") -Recurse -Force
} else {
    Write-Host "跳过: 发布目录不存在" -ForegroundColor Yellow
}

# 压缩
Write-Host "`n开始压缩..." -ForegroundColor Cyan
$ZipFile = Join-Path $DesktopPath "Release_Package_$Timestamp.zip"
Compress-Archive -Path "$TempDir\*" -DestinationPath $ZipFile -Force

# 清理临时目录
Remove-Item -Path $TempDir -Recurse -Force

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "  打包完成！" -ForegroundColor Green
Write-Host "  文件: $ZipFile" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Green

Start-Process $DesktopPath
