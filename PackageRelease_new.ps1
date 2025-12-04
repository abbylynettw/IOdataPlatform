# ====================================
# 椤圭洰鎵撳寘鑴氭湰
# 鍔熻兘锛氬皢鏁版嵁搴撹剼鏈€佹枃妗ｅ拰鍙戝竷绋嬪簭绉诲姩鍒版闈㈠苟鍘嬬缉
# ====================================

# 璁剧疆缂栫爜涓?UTF-8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

# 閰嶇疆鍙傛暟
$WorkspaceRoot = $PSScriptRoot
$DatabaseScriptsSourceDir = Join-Path $WorkspaceRoot "DatabaseScripts"
$DocsSourceDir = Join-Path $WorkspaceRoot "docs"
$PublishSourceDir = "E:\Client_Publish"

# 妗岄潰璺緞
$DesktopPath = [Environment]::GetFolderPath("Desktop")
$Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$TempPackageDir = Join-Path $DesktopPath "Release_Package_$Timestamp"

$MaxSizeMB = 50
$MaxSizeBytes = $MaxSizeMB * 1024 * 1024

# 棰滆壊杈撳嚭鍑芥暟
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

# 鏍煎紡鍖栨枃浠跺ぇ灏?
function Format-FileSize {
    param([long]$Size)
    
    if ($Size -ge 1GB) {
        return "{0:N2} GB" -f ($Size / 1GB)
    } elseif ($Size -ge 1MB) {
        return "{0:N2} MB" -f ($Size / 1MB)
    } elseif ($Size -ge 1KB) {
        return "{0:N2} KB" -f ($Size / 1KB)
    } else {
        return "{0} bytes" -f $Size
    }
}

# 鑾峰彇鐩綍澶у皬
function Get-DirectorySize {
    param([string]$Path)
    
    if (-not (Test-Path $Path)) {
        return 0
    }
    
    $size = (Get-ChildItem -Path $Path -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
    if ($null -eq $size) { return 0 }
    return $size
}

# 澶嶅埗鐩綍鍒版闈?
function Copy-ToDesktop {
    param(
        [string]$SourcePath,
        [string]$DestinationName,
        [string]$Description
    )
    
    if (-not (Test-Path $SourcePath)) {
        Write-ColorOutput "璺宠繃 $Description - 婧愮洰褰曚笉瀛樺湪: $SourcePath" "Red"
        return $null
    }
    
    $destPath = Join-Path $TempPackageDir $DestinationName
    
    Write-ColorOutput "`n姝ｅ湪澶嶅埗: $Description" "Cyan"
    Write-ColorOutput "浠? $SourcePath" "Gray"
    Write-ColorOutput "鍒? $destPath" "Gray"
    
    try {
        Copy-Item -Path $SourcePath -Destination $destPath -Recurse -Force -ErrorAction Stop
        $size = Get-DirectorySize -Path $destPath
        Write-ColorOutput "澶嶅埗瀹屾垚 ($(Format-FileSize $size))" "Green"
        return $destPath
    } catch {
        Write-ColorOutput "澶嶅埗澶辫触: $_" "Red"
        return $null
    }
}

# 鍒涘缓鏁版嵁搴撹剼鏈洰褰曪紙濡傛灉涓嶅瓨鍦級
function Initialize-DatabaseScriptsDirectory {
    if (-not (Test-Path $DatabaseScriptsSourceDir)) {
        New-Item -ItemType Directory -Path $DatabaseScriptsSourceDir -Force | Out-Null
        Write-ColorOutput "鍒涘缓鏁版嵁搴撹剼鏈洰褰? $DatabaseScriptsSourceDir" "Green"
        
        # 澶嶅埗鐜版湁鐨凷QL鑴氭湰
        $existingSqlDir = Join-Path $WorkspaceRoot "IODataPlatform\Assets\SQL"
        if (Test-Path $existingSqlDir) {
            $sqlFiles = Get-ChildItem -Path $existingSqlDir -Filter "*.sql"
            if ($sqlFiles.Count -gt 0) {
                Write-ColorOutput "鍙戠幇鐜版湁SQL鑴氭湰锛屾鍦ㄥ鍒?.." "Yellow"
                foreach ($file in $sqlFiles) {
                    $destPath = Join-Path $DatabaseScriptsSourceDir $file.Name
                    if (-not (Test-Path $destPath)) {
                        Copy-Item -Path $file.FullName -Destination $destPath
                        Write-ColorOutput "  - 澶嶅埗: $($file.Name)" "Cyan"
                    }
                }
            }
        }
    }
}

# ====================================
# 涓绘祦绋?
# ====================================

Write-ColorOutput "`n========================================" "Magenta"
Write-ColorOutput "  椤圭洰鎵撳寘鑴氭湰" "Magenta"
Write-ColorOutput "  鎵撳寘鏃堕棿: $Timestamp" "Magenta"
Write-ColorOutput "========================================`n" "Magenta"

# 1. 鍒濆鍖栨暟鎹簱鑴氭湰鐩綍
Initialize-DatabaseScriptsDirectory

# 2. 鍒涘缓妗岄潰涓存椂鐩綍
Write-ColorOutput "鍒涘缓妗岄潰涓存椂鐩綍..." "Cyan"
if (Test-Path $TempPackageDir) {
    Remove-Item -Path $TempPackageDir -Recurse -Force
}
New-Item -ItemType Directory -Path $TempPackageDir -Force | Out-Null
Write-ColorOutput "涓存椂鐩綍: $TempPackageDir`n" "Green"

# 3. 澶嶅埗鍚勪釜鐩綍鍒版闈?
$dbDestPath = Copy-ToDesktop -SourcePath $DatabaseScriptsSourceDir -DestinationName "DatabaseScripts" -Description "鏁版嵁搴撹剼鏈?
$docsDestPath = Copy-ToDesktop -SourcePath $DocsSourceDir -DestinationName "Docs" -Description "鏂囨。"
$publishDestPath = Copy-ToDesktop -SourcePath $PublishSourceDir -DestinationName "Client_Publish" -Description "鍙戝竷绋嬪簭"

Write-ColorOutput "`n========================================" "Magenta"
Write-ColorOutput "  寮€濮嬪帇缂? "Magenta"
Write-ColorOutput "========================================" "Magenta"

# 4. 鍘嬬缉鏁翠釜鐩綍锛堝寘鍚墍鏈夊唴瀹癸級
$zipFile = Join-Path $DesktopPath "Release_Package_$Timestamp.zip"

Write-ColorOutput "`n姝ｅ湪鍘嬬缉鏁翠釜鍙戝竷鍖?.." "Cyan"
$totalSize = Get-DirectorySize -Path $TempPackageDir
$totalSizeFormatted = Format-FileSize -Size $totalSize
Write-ColorOutput "鎬诲ぇ灏? $totalSizeFormatted" "Gray"

# 妫€鏌ユ槸鍚﹂渶瑕佸垎鍗?
if ($totalSize -gt $MaxSizeBytes) {
    Write-ColorOutput "鍖呭ぇ灏忚秴杩?${MaxSizeMB}MB锛屽皢鍒涘缓鍒嗗嵎鍘嬬缉鍖? "Yellow"
    
    # 浣跨敤 7-Zip 杩涜鍒嗗嵎鍘嬬缉
    $7zipPath = "C:\Program Files\7-Zip\7z.exe"
    if (Test-Path $7zipPath) {
        $volumeSize = "${MaxSizeMB}m"
        $7zFile = $zipFile -replace '\.zip$', '.7z'
        
        Write-ColorOutput "浣跨敤 7-Zip 鍒嗗嵎鍘嬬缉..." "Yellow"
        & $7zipPath a -v$volumeSize $7zFile "$TempPackageDir\*"
        
        if ($LASTEXITCODE -eq 0) {
            $archives = Get-ChildItem -Path $DesktopPath -Filter "$(Split-Path $7zFile -Leaf)*"
            Write-ColorOutput "鍒嗗嵎鍘嬬缉瀹屾垚锛屽叡 $($archives.Count) 涓枃浠? "Green"
            foreach ($archive in $archives) {
                Write-ColorOutput "  - $($archive.Name) ($(Format-FileSize $archive.Length))" "Gray"
            }
        } else {
            Write-ColorOutput "7-Zip鍘嬬缉澶辫触" "Red"
        }
    } else {
        Write-ColorOutput "鏈畨瑁?-Zip (C:\Program Files\7-Zip\7z.exe)" "Yellow"
        Write-ColorOutput "灏嗗垱寤哄崟涓帇缂╁寘锛堝彲鑳借秴杩?{MaxSizeMB}MB锛? "Yellow"
        
        Compress-Archive -Path "$TempPackageDir\*" -DestinationPath $zipFile -Force
        $zipSize = (Get-Item $zipFile).Length
        Write-ColorOutput "鍘嬬缉瀹屾垚: $(Split-Path $zipFile -Leaf) ($(Format-FileSize $zipSize))" "Green"
    }
} else {
    # 鍗曚釜鍘嬬缉鍖?
    Compress-Archive -Path "$TempPackageDir\*" -DestinationPath $zipFile -Force
    $zipSize = (Get-Item $zipFile).Length
    Write-ColorOutput "鍘嬬缉瀹屾垚: $(Split-Path $zipFile -Leaf) ($(Format-FileSize $zipSize))" "Green"
}

# 5. 娓呯悊涓存椂鐩綍
Write-ColorOutput "`n娓呯悊涓存椂鐩綍..." "Yellow"
try {
    Remove-Item -Path $TempPackageDir -Recurse -Force -ErrorAction Stop
    Write-ColorOutput "涓存椂鐩綍宸叉竻鐞? "Green"
} catch {
    Write-ColorOutput "鏃犳硶鍒犻櫎涓存椂鐩綍: $TempPackageDir" "Yellow"
    Write-ColorOutput "璇锋墜鍔ㄥ垹闄? "Yellow"
}

# 6. 瀹屾垚
Write-ColorOutput "`n========================================" "Magenta"
Write-ColorOutput "  鎵撳寘瀹屾垚锛? "Magenta"
Write-ColorOutput "  鎵€鏈夊帇缂╁寘宸蹭繚瀛樺埌妗岄潰" "Magenta"
Write-ColorOutput "========================================`n" "Magenta"

# 鎵撳紑妗岄潰鏂囦欢澶?
Write-ColorOutput "姝ｅ湪鎵撳紑妗岄潰..." "Cyan"
Start-Process $DesktopPath
