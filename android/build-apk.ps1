$ErrorActionPreference = 'Stop'
$androidRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $androidRoot
$sdk = Join-Path $env:LOCALAPPDATA 'Android\Sdk'
$buildTools = Join-Path $sdk 'build-tools\36.0.0'
$platform = Join-Path $sdk 'platforms\android-37.0'
$jbr = 'C:\Program Files\Android\Android Studio\jbr'
foreach ($path in @($sdk, $buildTools, $platform, $jbr)) {
    if (-not (Test-Path $path)) { throw "Required Android build tool path not found: $path" }
}
$env:JAVA_HOME = $jbr
$env:Path = "$(Join-Path $jbr 'bin');$buildTools;$env:Path"
$work = Join-Path $androidRoot 'build-manual'
$output = Join-Path $androidRoot 'app\build\outputs\apk\debug'
$resources = Join-Path $work 'resources.zip'
$unsigned = Join-Path $work 'unsigned.apk'
$aligned = Join-Path $work 'aligned.apk'
$gen = Join-Path $work 'gen'
$classes = Join-Path $work 'classes'
$dex = Join-Path $work 'dex'
New-Item -ItemType Directory -Force -Path $work,$output,$gen,$classes,$dex | Out-Null
function Invoke-Tool($file, [string[]]$arguments) {
    & $file @arguments
    if ($LASTEXITCODE -ne 0) { throw "Build command failed: $file" }
}
Invoke-Tool (Join-Path $buildTools 'aapt2.exe') @('compile','--dir',(Join-Path $androidRoot 'app\src\main\res'),'-o',$resources)
Invoke-Tool (Join-Path $buildTools 'aapt2.exe') @('link','-I',(Join-Path $platform 'android.jar'),'--manifest',(Join-Path $androidRoot 'app\src\main\AndroidManifest.xml'),'-A',(Join-Path $androidRoot 'app\src\main\assets'),'-R',$resources,'--java',$gen,'--min-sdk-version','26','--target-sdk-version','34','--version-code','1','--version-name','1.0','--debug-mode','--auto-add-overlay','-o',$unsigned)
Invoke-Tool (Join-Path $jbr 'bin\javac.exe') @('-source','8','-target','8','-classpath',(Join-Path $platform 'android.jar'),'-d',$classes,(Join-Path $androidRoot 'app\src\main\java\com\musiko\player\MainActivity.java'))
$classFiles = @(Get-ChildItem -Path $classes -Recurse -Filter '*.class' | ForEach-Object { $_.FullName })
if ($classFiles.Count -eq 0) { throw 'No compiled Java classes found.' }
Invoke-Tool (Join-Path $buildTools 'd8.bat') (@('--min-api','26','--lib',(Join-Path $platform 'android.jar'),'--output',$dex) + $classFiles)
Invoke-Tool (Join-Path $jbr 'bin\jar.exe') @('uf',$unsigned,'-C',$dex,'classes.dex')
Invoke-Tool (Join-Path $buildTools 'zipalign.exe') @('-f','-p','4',$unsigned,$aligned)
$key = Join-Path $work 'musiko-debug.keystore'
if (-not (Test-Path $key)) {
    Invoke-Tool (Join-Path $jbr 'bin\keytool.exe') @('-genkeypair','-keystore',$key,'-storepass','android','-keypass','android','-alias','androiddebugkey','-keyalg','RSA','-keysize','2048','-validity','10000','-dname','CN=Musiko Debug,O=Musiko,C=US')
}
$apk = Join-Path $output 'Musiko-debug.apk'
Invoke-Tool (Join-Path $buildTools 'apksigner.bat') @('sign','--ks',$key,'--ks-pass','pass:android','--key-pass','pass:android','--ks-key-alias','androiddebugkey','--out',$apk,$aligned)
Write-Output "APK created: $apk"
