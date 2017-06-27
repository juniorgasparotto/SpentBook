@echo off
setlocal
:PROMPT
SET /P AREYOUSURE="Are you sure (y/[n])? "
IF /I "%AREYOUSURE%" NEQ "Y" GOTO END

dotnet ef database drop -f -c ApplicationContext
rd /s /q "Migrations"
timeout 10
:END
endlocal