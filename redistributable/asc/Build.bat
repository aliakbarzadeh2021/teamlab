@echo off

set Configuration=Debug
set BuildTargets=Build
set Deploy=0

@echo on

%SystemRoot%\Microsoft.NET\Framework\v3.5\MSBuild.exe ..\..\_ci\projects\build.proj /fl1 /flp1:LogFile=Build.log;Verbosity=Normal
pause