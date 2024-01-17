echo "build: Build started"
$myLocation = Get-Location
Push-Location $myLocation

if(Test-Path ./artifacts) {
	echo "build: Cleaning .\artifacts"
	Remove-Item .\artifacts -Force -Recurse
}

& dotnet restore --no-cache
if($LASTEXITCODE -ne 0) { throw "Build failed with exit code $LASTEXITCODE" }    

& dotnet publish -c Release ./AzureSecretCheck.csproj
if($LASTEXITCODE -ne 0) { throw "Build failed with exit code $LASTEXITCODE" }  

& dotnet pack -c Release ./AzureSecretCheck.csproj -o ./artifacts --no-build
if($LASTEXITCODE -ne 0) { throw "Build failed with exit code $LASTEXITCODE" }  