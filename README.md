# Mini3DCad
## 三面図からつくる三次元ＣＡＤ

平面図や側面図で作成した図形から3D図形を作成する三次元CAD  
当初、サーフェースのグラフィックライブラリを自作して作っていたが描画速度や機能的に限界があったので OpenGL を使って作成。  
使い方などは[説明書](Document/Mini3DCad_Manual.pdf)を参照。  
実行方法は[Mini3DCad.zip](Mini3DCad.zip)をダウンロードし適当なフォルダーに展開して Mini3DCad.exe を実行する。
<img src="Image/download.png" width="80%">


### 画面
2D表示    
<img src="Image/MainWindow6pSw2D.png" width="80%">  

3D表示  
<img src="Image/MainWindow6pSw3D.png" width="80%">  

2Dデータから3Dデータの作成(押出)  
<img src="Image/2Dデータの3Dに変換.png" width="80%">  

スピーカ端子(2D)　　
<img src="Image/SP端子2D.png" width="80%">  
スピーカ端子(3D)　　
<img src="Image/SP端子3D.png" width="80%">  


ティーカップ(2D)　　
<img src="Image/ティーカップ2D.png" width="80%">  

ティーカップ(3D化)　　
<img src="Image/ティーカップ2D→3D.png" width="80%">  

ティーカップ(3D)　　
<img src="Image/ティーカップ3D.png" width="80%">  

スピーカ(正面図、側面図)  
<img src="Image/FLAT6正面2D.png" width="50%"><img src="Image/FLAT6側面2D.png" width="50%">  

スピーカ(3D)
<img src="Image/FLAT6_3D.png" width="80%">  

バックロードホーン(2D)  
<img src="Image/バックロードホーン2D.png" width="80%">  
バックロードホーン(3D)  
<img src="Image/バックロードホーン3D.png" width="80%">  

電解コンデンサ(3D)  
<img src="Image/電解コンデンサ.png" width="80%">  

電源トランス(3D)  
<img src="Image/電源トランス.png" width="80%">  

### 履歴
2024/12/10  穴ありポリゴンの押出ができるように修正(ポリゴンの分割が不要となった)  
2024/09/17  2D交点計算見直し、グループ選択ピック追加  
2024/09/09  メモ機能追加(モードレスダイヤログと計算機能)  
2024/07/25  グループ属性の追加、グループピックをサポート  
2024/07/11  属性一括設定のレイヤー設定に追加を追加  
2024/07/10  ブレントコマンドのポリラインを追加  
2024/06/30  反転(ミラー)を線分から平面に対する反転に変更  
2024/06/27  2D背景色設定追加  
2024/06/27  ワイヤーフレーム表示を追加  
2024/06/26  3D要素でアウトライン表示を追加して3Dデータの編集を追加  
2024/06/22  属性変更にレイヤー追加を追加  
2024/06/20  属性変更に端面表示を追加  
2024/06/20  ブレンドコマンド追加  
2024/06/14  円弧のストレッチをサポート  
2024/06/02  ポリラインとポリゴンにR面取り(フィレット)機能追加  
2024/05/19  ポリラインとポリゴンに円弧座標追加  
2024/03/30  2Dデータのストレッチコマンドを追加  
2024/03/27  接続コマンドで複数ピック(3要素以上)に対応  
2024/03/26  CadAppから要素コピー(クリップボード経由)データの貼付けに対応  
2024/03/23  コピーの移動、回転で複数位置への対応  
2024/03/22  グリッド表示速度若干改善  
2024/03/21  図面コメント追加、バックアップ処理をシステム設定ダイヤログに集約  
2024/03/17 イメージデータトリミングダイヤログ追加  
2024/03/13 データバックアップ機能追加 
2024/03/10 画面コピー/保存機能追加  
2024/03/08 要素コピー/ペースト(クリップボード経由)  
2024/03/04 計測機能追加  
2024/03/01 レイヤ機能をサポート  
2024/02/28 トリムコマンド追加  
2024/02/24 ミラー(反転)コマンド追加  
2024/02/20 オートロケイト機能など操作機能追加  
2024/02/08 3D表示をOpenGL(OpenTK)に変更  
2023/12/25 プロトタイプの原型(自作グラフィックライブラリ)  

### ■実行環境
[Mini3DCad.zip](Mini3DCad.zip)をダウンロードして適当なフォルダに展開し、フォルダ内の Mini3DCad.exe をダブルクリックして実行します。  
動作環境によって「.NET 7.0 Runtime」が必要になる場合もあります。  
https://dotnet.microsoft.com/ja-jp/download


### ■開発環境  
開発ソフト : Microsoft Visual Studio 2022  
開発言語　 : C# 10.0 Windows アプリケーション  
フレームワーク　 :  .NET 7.0  
NuGetライブラリ : なし  
自作ライブラリ  : CoreLib (三次元の幾何計算も含む)  
