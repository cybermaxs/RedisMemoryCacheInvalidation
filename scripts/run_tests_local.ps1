Remove-Item ../build/coverage -rec -force

..\packages\OpenCover.4.6.166\tools\OpenCover.Console.exe -register:user -target:..\packages\xunit.runner.console.2.0.0\tools\xunit.console.x86.exe -targetargs:"""..\tests\RedisMemoryCacheInvalidation.Tests\bin\Release\RedisMemoryCacheInvalidation.Tests.dll"" -noshadow -appveyor -notrait ""category=Integration""" -filter:"+[RedisMemoryCacheInvalidation]*" -output:..\build\opencoverCoverage.xml

..\packages\ReportGenerator.2.2.0.0\tools\ReportGenerator.exe -reports:..\build\opencoverCoverage.xml -targetdir:../build/coverage 