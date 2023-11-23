Write-Host "This script nukes all build outputs, make sure you want this!!!"
Write-Host "Press any key to continue or CTRL+C to stop this script"
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
Remove-Item -Path "./app/src/bin/" -Recurse -Force
Remove-Item -Path "./app/src/obj/" -Recurse -Force