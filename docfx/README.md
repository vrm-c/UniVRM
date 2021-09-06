# docfx 作業メモ

## 初期化

Project ROOT から

```
$ docfx init -o docfx -q
```

## csproj をコピー

Unity の csproj(vscode向け) を `docfx/src/UniVRM` にコピーする python スクリプト作った。

```
$ pip install invoke
$ cd docfx
docfx$ invoke copy-csproj
```

コピーする際にパスを調整している。
`docfx/src/UniVRM` からの相対パスに改変。

`Assets` => `..\\..\\..\\Assets`
