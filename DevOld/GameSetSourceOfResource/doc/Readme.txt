=======================
GameSetSourceOfResource
=======================

----
説明

ゲームのリソースの提供者情報をマニュアルに反映する。


----
コマンド

GameSetSourceOfResource.exe 入力リソースDIR 入力マニュアルファイル 出力ファイル

	出力ファイル ... 入力マニュアルファイルを上書きする場合は * を指定すること。


----
実行例

GameSetSourceOfResource.exe C:\Dev\Game\00_Game\Resource C:\Dev\Game\00_Game\doc\Readme.txt C:\temp\Readme.txt

GameSetSourceOfResource.exe .\Resource .\out\Readme.txt *

