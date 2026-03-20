<#
.SYNOPSIS
    CI 오류를 가져와 Cursor에서 수정할 수 있도록 준비하는 원클릭 스크립트

.DESCRIPTION
    1. GitHub Actions에서 실패한 CI 결과를 조회
    2. 오류 리포트를 .cursor/errors/errors.json에 저장
    3. 오류가 있는 파일을 Cursor에서 열기

.EXAMPLE
    .\scripts\fix-ci-errors.ps1
    .\scripts\fix-ci-errors.ps1 -Branch kth
    .\scripts\fix-ci-errors.ps1 -Limit 3
#>

param(
    [string]$Branch = "",
    [int]$Limit = 10
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$ErrorsFile = Join-Path $RepoRoot ".cursor\errors\errors.json"

function Write-Header {
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host "  CI Error Fetcher & Cursor Integration" -ForegroundColor Cyan
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host ""
}

function Test-Prerequisites {
    $ghInstalled = Get-Command gh -ErrorAction SilentlyContinue
    if (-not $ghInstalled) {
        Write-Host "[ERROR] GitHub CLI(gh)가 설치되어 있지 않습니다." -ForegroundColor Red
        Write-Host "  설치: winget install GitHub.cli" -ForegroundColor Yellow
        Write-Host "  인증: gh auth login" -ForegroundColor Yellow
        exit 1
    }

    $pythonInstalled = Get-Command python -ErrorAction SilentlyContinue
    if (-not $pythonInstalled) {
        Write-Host "[ERROR] Python이 설치되어 있지 않습니다." -ForegroundColor Red
        exit 1
    }

    try {
        $authStatus = gh auth status 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Host "[ERROR] GitHub CLI 인증이 필요합니다." -ForegroundColor Red
            Write-Host "  실행: gh auth login" -ForegroundColor Yellow
            exit 1
        }
    }
    catch {
        Write-Host "[ERROR] GitHub CLI 인증 확인 실패" -ForegroundColor Red
        exit 1
    }

    Write-Host "[OK] Prerequisites check passed" -ForegroundColor Green
}

function Invoke-FetchErrors {
    param([string]$BranchArg, [int]$LimitArg)

    $fetchScript = Join-Path $RepoRoot "scripts\fetch-errors.py"
    $args = @($fetchScript, "--limit", $LimitArg)
    if ($BranchArg) {
        $args += @("--branch", $BranchArg)
    }

    Write-Host "`nFetching CI errors..." -ForegroundColor Yellow
    python @args

    return $LASTEXITCODE
}

function Open-ErrorFilesInCursor {
    if (-not (Test-Path $ErrorsFile)) {
        return
    }

    $report = Get-Content $ErrorsFile -Raw | ConvertFrom-Json
    if (-not $report.errors -or $report.errors.Count -eq 0) {
        return
    }

    $files = $report.errors |
        Where-Object { $_.file -and $_.file -ne "" } |
        ForEach-Object { $_.file } |
        Sort-Object -Unique

    if ($files.Count -eq 0) {
        return
    }

    Write-Host "`nOpening error files in Cursor..." -ForegroundColor Yellow

    $cursorInstalled = Get-Command cursor -ErrorAction SilentlyContinue
    if (-not $cursorInstalled) {
        Write-Host "[INFO] Cursor CLI not found. Files with errors:" -ForegroundColor Yellow
        foreach ($f in $files) {
            $fullPath = Join-Path $RepoRoot $f
            if (Test-Path $fullPath) {
                Write-Host "  - $f" -ForegroundColor White
            }
        }
        return
    }

    foreach ($f in $files) {
        $fullPath = Join-Path $RepoRoot $f
        if (Test-Path $fullPath) {
            cursor $fullPath
        }
    }

    Write-Host "[OK] Opened $($files.Count) file(s) in Cursor" -ForegroundColor Green
}

function Show-NextSteps {
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host "  Next Steps" -ForegroundColor Cyan
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  1. Cursor에서 'CI 오류 수정해줘' 입력" -ForegroundColor White
    Write-Host "  2. AI가 .cursor/errors/errors.json을 읽고 자동 수정" -ForegroundColor White
    Write-Host "  3. 수정 확인 후 커밋 & 푸시" -ForegroundColor White
    Write-Host ""
}

# --- Main ---
Write-Header
Test-Prerequisites
$exitCode = Invoke-FetchErrors -BranchArg $Branch -LimitArg $Limit

if ($exitCode -eq 0) {
    Write-Host "`nAll branches are clean!" -ForegroundColor Green
}
else {
    Open-ErrorFilesInCursor
    Show-NextSteps
}
