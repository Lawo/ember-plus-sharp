rmdir TestResults /s /q
rmdir CoverageResults /s /q

"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" Lawo.EmberPlusTest\bin\Release\Lawo.EmberPlusTest.dll LawoTest\bin\Release\LawoTest.dll /InIsolation /TestCaseFilter:"TestCategory=Unattended" /EnableCodeCoverage /Settings:CodeCoverage.runsettings /Logger:trx

mkdir CoverageResults
move TestResults\*.trx CoverageResults\TestResults.trx
for /r TestResults %%f in (*.coverage) do move "%%f" CoverageResults\CodeCoverage.coverage
"C:\Program Files (x86)\VisualCoverage\VisualCoverage.exe" -i CoverageResults\CodeCoverage.coverage --clover CoverageResults\CodeCoverage.xml