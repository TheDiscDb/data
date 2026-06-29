#requires -Version 7
<#
Backfills missing `Slug` in disc*.json files under data/data/.

Algorithm (per release directory):
  1. Read every disc*.json in the dir.
  2. Collect existing Slugs (so we don't collide).
  3. For each disc whose Slug is missing/empty:
        candidate = format.ToLower() with "uhd" -> "4k"
        if format is empty: candidate = "disc"
        if candidate collides (case-insensitive) with an existing slug
        in the release: candidate = "{candidate}-{index}"
  4. Insert `"Slug": "<candidate>",` immediately before the `"Name":`
     line in the file (preserves ordering of all other fields and the
     2-space indent).

Use -WhatIf to preview without writing.
#>

[CmdletBinding(SupportsShouldProcess)]
param(
    [string]$DataRoot = "B:\code\thediscdb\data\data"
)

$ErrorActionPreference = "Stop"

function Get-CandidateSlug {
    param([string]$Format, [int]$Index)
    if ([string]::IsNullOrWhiteSpace($Format)) { return "disc" }
    $c = $Format.Trim().ToLowerInvariant()
    if ($c -eq "uhd") { return "4k" }
    return $c
}

$files = Get-ChildItem -Recurse -Path $DataRoot -Filter "disc*.json"
Write-Host "Found $($files.Count) disc*.json files."

# Group by release directory
$byRelease = @{}
foreach ($f in $files) {
    $dir = $f.Directory.FullName
    if (-not $byRelease.ContainsKey($dir)) { $byRelease[$dir] = @() }
    $byRelease[$dir] += $f
}

$totalUpdated = 0
$totalReleases = 0

foreach ($releaseDir in $byRelease.Keys | Sort-Object) {
    $releaseFiles = $byRelease[$releaseDir]
    # First pass: parse current state
    $discs = @()
    foreach ($f in $releaseFiles) {
        $j = Get-Content $f.FullName -Raw | ConvertFrom-Json
        $discs += [pscustomobject]@{
            File   = $f
            Index  = [int]$j.Index
            Slug   = $j.Slug
            Name   = $j.Name
            Format = $j.Format
        }
    }

    # Existing slugs (case-insensitive) to avoid collision
    $taken = New-Object System.Collections.Generic.HashSet[string] ([StringComparer]::OrdinalIgnoreCase)
    foreach ($d in $discs) {
        if (-not [string]::IsNullOrWhiteSpace($d.Slug)) { [void]$taken.Add($d.Slug) }
    }

    $changedInRelease = $false
    foreach ($d in $discs | Where-Object { [string]::IsNullOrWhiteSpace($_.Slug) }) {
        $candidate = Get-CandidateSlug -Format $d.Format -Index $d.Index
        if ($taken.Contains($candidate)) {
            $candidate = "$candidate-$($d.Index)"
        }
        # Extra paranoia in case "{base}-{index}" still collides
        $i = 2
        while ($taken.Contains($candidate)) {
            $candidate = "$(Get-CandidateSlug -Format $d.Format -Index $d.Index)-$i"
            $i++
        }
        [void]$taken.Add($candidate)

        # Insert `"Slug": "<candidate>",` before the `"Name":` line.
        $lines = Get-Content $d.File.FullName
        $newLines = New-Object System.Collections.Generic.List[string]
        $inserted = $false
        foreach ($line in $lines) {
            if (-not $inserted -and $line -match '^(\s*)"Name"\s*:') {
                $indent = $matches[1]
                $newLines.Add("$indent`"Slug`": `"$candidate`",")
                $inserted = $true
            }
            $newLines.Add($line)
        }
        if (-not $inserted) {
            Write-Warning "Could not find Name line in $($d.File.FullName); skipping."
            continue
        }

        if ($PSCmdlet.ShouldProcess($d.File.FullName, "Insert Slug=`"$candidate`"")) {
            # Preserve original line endings: Get-Content -> Set-Content uses default
            # which is CRLF on Windows. Match repo style by detecting input.
            $originalRaw = [IO.File]::ReadAllText($d.File.FullName)
            $eol = if ($originalRaw -match "`r`n") { "`r`n" } else { "`n" }
            [IO.File]::WriteAllText($d.File.FullName, ($newLines -join $eol) + $eol)
        }
        $totalUpdated++
        $changedInRelease = $true
    }
    if ($changedInRelease) { $totalReleases++ }
}

Write-Host ""
Write-Host "=================="
Write-Host "Updated $totalUpdated disc files across $totalReleases releases."
if ($WhatIfPreference) { Write-Host "(WhatIf mode — no files were written.)" }
