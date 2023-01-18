$subCommand = $args[0]
$filettlUrl = "https://filettl.store"
$filettlUrl = "https://localhost:7000"
Add-Type -AssemblyName System.Net.Http
Add-Type -AssemblyName System.Web
$ErrorActionPreference = "Stop"

function Help {
    param ()
    Write-Host "NAME"
    Write-Host "`tfilettl.ps1"
    Write-Host "`r`n"

    Write-Host "Upload a file:"
    Write-Host "`tfilettl.ps1 upload `$file"
    Write-Host "`r`n"

    Write-Host "Download a file:"
    Write-Host "`tfilettl.ps1 download `$hash"
    Write-Host "`r`n"
}

function Upload {
    param (
        $filePath
    )

    if (!(Test-Path -path $filePath)) {
        Write-Warning "File not exists."
        return
    }

    $uri = $filettlUrl + "/api/files"
    $fileName = Split-Path -leaf $filePath
    $boundary = [System.Guid]::NewGuid().ToString()
    $content = [System.Text.Encoding]::GetEncoding('iso-8859-1').GetString([System.IO.File]::ReadAllBytes($filePath))
    $contentType = [System.Web.MimeMapping]::GetMimeMapping($fileName)
    $lf = "`r`n"
    $bodyLines = (
        "--$boundary",
        "Content-Disposition: form-data; name=`"file`"; filename=`"$fileName`"",
        "Content-Type: $contentType$LF",
        $content,
        "--$boundary--$lf"
    ) -join $LF
    $response = Invoke-RestMethod $uri -Method POST -ContentType "multipart/form-data; boundary=`"$boundary`"" -Body $bodyLines
    Write-Host "$response"
}

function Download {
    param($hash)
    if (!$hash) {
        Write-Warning "The file hash must not be empty."
        return
    }
    $uri = $filettlUrl + "/api/files?hash=$hash"

    Write-Host "Downloading $uri"
    $response = Invoke-WebRequest $uri
    $contentDisposition = $response.Headers.'Content-Disposition'
    $fileName = $contentDisposition.Split("=")[1].Split(";")[0].Replace("`"", "")

    $path = Join-Path "./" $fileName

    $file = [System.IO.FileStream]::new($path, [System.IO.FileMode]::Create)
    $response.RawContentStream.CopyTo($file)
    $file.close()

    Write-Host "Download Success"
}


switch ($subCommand) {
    ({ $subCommand -in @($null, '-h', '--help', '/?') }) {
        Help
    }
    ({ $subCommand -in @('u', 'upload') }) {
        Upload $args[1]
    }
    ({ $subCommand -in @('d', 'download') }) {
        Download $args[1]
    }
    Default {
        Write-Warning "filettl: '$subCommand' isn't a filettl command. See 'filettl help'."
    }
}