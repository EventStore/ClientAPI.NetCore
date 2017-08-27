$testResults = Join-Path $env:APPVEYOR_BUILD_FOLDER testResults.xml
dotnet test test\EventStore.ClientAPI.NetCore.Tests\EventStore.ClientAPI.NetCore.Tests.csproj --no-build --logger="trx;LogFileName=$testResults"
$wc = New-Object System.Net.WebClient
$wc.UploadFile("https://ci.appveyor.com/api/testresults/mstest/$env:APPVEYOR_JOB_ID", $testResults)