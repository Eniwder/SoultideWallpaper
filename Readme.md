# ソウルタイドの非公式壁紙アプリ

### DL先
[「 Assets >  SoultideWallpaper.zip 」からダウンロード](https://github.com/Eniwder/SoultideWallpaper/releases)

zipを展開後、中にある「WallpaperApp.exe」を実行すると動作する。

### 概要
![sample](https://github.com/user-attachments/assets/6ae8314c-8938-4c2f-8507-4427084ca074)

Windowsの壁紙を、ソウルタイド風のマップとSDキャラが動く壁紙に上書きする非公式アプリ。

擬似的に背景にしているだけなので、アプリを終了すると元の壁紙に戻ります。
 - 終了方法：タスクバーの端にある通知領域(非表示の場合は「∧」ボタンから表示)のタイルアイコンを右クリックして「Exit」を選択

常にこの背景にしておきたい場合はスタートアップに登録するとかで実現できます。

### 動作環境
- Windows 64bit
- 常時メモリ200MBくらい消費
- CPUほんの少し、GPU少し

### カスタマイズ
- 壁紙の変更
  - 「SoultideWallpaper/bin/SoultideWallpaper_Data/StreamingAssets」にある「background.png」を変更することでお好みの背景に設定できます。
- 詳細設定
  - 「SoultideWallpaper/bin/SoultideWallpaper_Data/StreamingAssets」にある「config.json」を変更することで細かい部分を設定できます。
    - 編集に失敗した場合は「[こちら](https://github.com/Eniwder/SoultideWallpaper/blob/main/Assets/StreamingAssets/config.json)」からコピペし直せば直ると思います。
- 壁紙ではなく単体のアプリとして開きたい場合は、「SoultideWallpaper/bin/SoultideWallpaper.exe」から起動します。
  - アプリは「Alt+Enter」で全画面とウィンドウを切り替えることができます。
  - ウィンドウサイズは自由に変更できますが、描画処理の関係上、サイズを変更した後に手動でアプリの再起動を推奨します。
  - アプリとして起動した場合の終了方法は、ウィンドウ状態でバツボタンを押すか「Alt+F4」で強制終了するかのどちらかです。
  - 壁紙モードを想定して作っているので、ウィンドウ状態の作り込みは甘いです。
  
### 探索スコア
アプリの右下に表示される数値は探索スコアです。環境次第では表示されない問題があるようです。

探索スコアは新しいタイルを見つけたり、キャラのアクション次第で増えるお遊び要素です。

詳細設定から非表示にできます。

### お問い合わせ
- https://x.com/Eniel120

> このソフトウェアは『ソウルタイド』の非公式ファン制作であり、著作権は原作の権利者に帰属します。
