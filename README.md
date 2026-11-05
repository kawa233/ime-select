# IME Select

WindowsのIMEをON／OFFに切り替え、現在フォーカスされているウィンドウのIME状態を取得するコマンドラインツールです。  
(本家ime-selectはcで記述されていたため、Windows標準環境でもビルドできるようにc#で実装しています)

## ファイル構成

| ファイル | 内容 |
| --- | --- |
| `imeSelect.cs` | 引数の判定、IME状態の取得、ON／OFFの処理 |
| `build-ime-select.bat` | `imeSelect.cs` をコンパイルするバッチ |
| `out\ime-select.exe` | ビルドで生成される実行ファイル |

## 必要な環境

- Windows
- 次の場所にC#コンパイラがあること（バッチで使用）

```text
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe
```

## ビルド方法

`imeSelect.cs` と `build-ime-select.bat` を同じフォルダーに配置し、PowerShellから次のコマンドを実行します。

```powershell
.\build-ime-select.bat
```

バッチファイルがあるフォルダー直下の `out` に、`ime-select.exe` を出力します。`out` がなければ自動で作成します。別のカレントディレクトリからバッチを実行しても、出力先は変わりません。既存の `ime-select.exe` は再ビルド時に上書きされます。

コンパイラが見つからない場合、フォルダー作成またはコンパイルに失敗した場合は、終了コード `1` で終了します。

## 使い方

以下は、バッチファイルがあるフォルダーからPowerShellで実行する例です。

```powershell
# 現在のIME状態を表示
.\out\ime-select.exe

# IMEをOFFにする
.\out\ime-select.exe 0

# IMEをONにする
.\out\ime-select.exe 1
```

| 引数 | 動作 | 標準出力 |
| --- | --- | --- |
| なし | フォーカスされているウィンドウのIME状態を取得 | OFFなら `0`、ONなら `1` |
| `0` | IMEをOFFにするキー入力を送信 | なし |
| `1` | IMEをONにするキー入力を送信 | なし |
| その他、または複数の引数 | 使用方法を標準エラー出力に表示 | なし |

状態取得時の出力値と、プログラムの終了コードは別です。状態取得に成功した場合、ON／OFFのどちらでも終了コードは `0` です。状態取得の失敗や不正な引数の場合は `1` です。

PowerShellでは、実行直後に次のコマンドで終了コードを確認できます。

```powershell
$LASTEXITCODE
```

## vscodevimの設定方法

settings.jsonに実行ファイルへのパスを記載してください。
```
  "vim.autoSwitchInputMethod.enable": true,
  "vim.autoSwitchInputMethod.defaultIM": "0",
  "vim.autoSwitchInputMethod.obtainIMCmd": "ime-select.exe",
  "vim.autoSwitchInputMethod.switchIMCmd": "ime-select.exe {im}",
```

## クラス構成

各処理は `imeSelect.cs` の `ImeSelectApp` 名前空間内で、別々のクラスに分かれています。

| クラス | 役割 |
| --- | --- |
| `ImeSelect` | 引数に応じて各クラスの `Execute()` を呼び出す |
| `ImeStatus` | フォーカス先のIME状態を取得して表示する |
| `ImeOff` | IME OFFキー（`0x1A`）の押下と解放を送信する |
| `ImeOn` | IME ONキー（`0x16`）の押下と解放を送信する |

状態取得は `GetGUIThreadInfo` と `ImmGetDefaultIMEWnd` で対象を特定し、`SendMessageTimeoutW` でIMEに問い合わせます。問い合わせのタイムアウトは200ミリ秒です。ON／OFFには `keybd_event` を使用します。

## 状態取得に失敗した場合

状態取得に失敗すると、次のいずれかのメッセージを標準エラー出力に表示し、終了コード `1` で終了します。

| メッセージ | 内容 |
| --- | --- |
| `No focused window was found.` | フォーカス先のウィンドウを取得できない |
| `No IME window was found.` | 対象のIMEウィンドウを取得できない |
| `Could not send an IME command to the focused window.` | IMEへの問い合わせに失敗した |

操作対象は実行時にフォーカスされているウィンドウです。IMEに問い合わせできない環境では状態取得が失敗します。ON／OFFの処理はキー入力を送信するもので、送信後の状態を確認する処理はありません。

