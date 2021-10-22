md .\TestResults

dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=TestResults\coverage.cobertura.xml
reportgenerator "-reports:TestResults/coverage.cobertura.xml" "-targetdir:TestResults/CoverageReport" -reporttypes:Html
call .\TestResults\CoverageReport\index.html

rem pause
