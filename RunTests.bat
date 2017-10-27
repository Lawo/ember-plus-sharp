rem Copyright 2012-2017 Lawo AG (http://www.lawo.com).
rem Distributed under the Boost Software License, Version 1.0.
rem (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)

rmdir TestResults /s /q
rmdir CoverageResults /s /q

"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" Lawo.EmberPlusSharpTest\bin\Release\Lawo.EmberPlusSharpTest.dll LawoTest\bin\Release\LawoTest.dll /InIsolation /TestCaseFilter:"TestCategory!=Manual" /EnableCodeCoverage /Settings:CodeCoverage.runsettings /Logger:trx

mkdir CoverageResults
move TestResults\*.trx CoverageResults\TestResults.trx

rem Coverage is only supported in VS Enterprise, which we no longer have access to
rem for /r TestResults %%f in (*.coverage) do move "%%f" CoverageResults\CodeCoverage.coverage
rem "C:\Program Files (x86)\VisualCoverage\VisualCoverage.exe" -i CoverageResults\CodeCoverage.coverage --clover CoverageResults\CodeCoverage.xml