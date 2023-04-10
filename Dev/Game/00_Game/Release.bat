CALL C:\Factory\SetEnv.bat
CALL Clean.bat
cx **

C:\apps\MakeResourceCluster\MakeResourceCluster.exe Resource out\Resource.dat

COPY C:\Dat\DxLibDotNet\DxLibDotNet3_24b\DxLib.dll out

acp Silvia20200001\Silvia20200001\bin\Release\Silvia20200001.exe out\*P.exe
xcp doc out

C:\Factory\SubTools\zip.exe /PE- /O out *P
