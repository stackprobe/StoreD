CALL C:\Factory\SetEnv.bat
CALL Clean.bat
cx **

acp Claes20200001\Claes20200001\bin\Release\Claes20200001.exe out\*P.exe
xcp doc out

C:\Factory\SubTools\zip.exe /PE- /O out *P
