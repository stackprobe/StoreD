CALL C:\Factory\SetEnv.bat
CALL Clean.bat
cx **

C:\apps\MakeResourceCluster\MakeResourceCluster.exe Resource out\Resource.dat

COPY /B C:\Dat\DxLibDotNet\DxLibDotNet3_24b\DxLib.dll out
COPY /B C:\Dat\DxLibDotNet\DxLibDotNet3_24b\DxLibDotNet.dll out

acp Silvia20200001\Silvia20200001\bin\Release\Silvia20200001.exe out\Game.exe
xcp doc out

C:\apps\GameSetSourceOfResource\GameSetSourceOfResource.exe .\Resource .\out\Readme.txt *

C:\Factory\SubTools\zip.exe /PE- /O out *P
