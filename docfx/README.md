# UniVRM Programming Document

`UniVRM/docfx` フォルダから `gh-pages` ブランチを作成する。

# dependencies

* [docfx](https://dotnet.github.io/docfx/)(2系)
* python3
  * pip install invoke
  * pip install watchdog

# 記事の更新

* index.md
* articles フォルダを更新して `articles/toc.yml` にそれを反映する

# docfx 作業記録
## 初期化

Project ROOT から

```
$ docfx init -o docfx -q
```

## csproj をコピー

* Unity の Editor を vscode に設定
* `Open C# project` により `sln` と `csproj` が生成される
* Unity の csproj(vscode向け) を `docfx/src/UniVRM` にコピーする python スクリプト作った

```
$ pip install invoke
$ cd docfx
docfx$ invoke copy-csproj
```

コピーする際にパスを調整している。
`docfx/src/UniVRM` からの相対パスに改変。

`Assets` => `..\\..\\..\\Assets`

## csproj から meta情報を生成する

```
docfx$ docfx metadata
```

[filter設定](./filterConfig.yml)

## preview

```
docfx$ docfx --serve
```

<http://localhost:8080/>

記事更新の反映

```
docfx$ docfx build
```

## github actions

`.github/workflows/docfx.yml`

```yml
name: DocFX

on:
  push:
    branches:
      - master

jobs:
  build:
    runs-on: windows-2019
    
    steps:
      - name: Checkout
        uses: actions/checkout@v2
        with:
          fetch-depth: 1

      # build docfx site to docfx/_site
      - name: DocFX
        shell: cmd
        run: |
          choco install docfx -y
          docfx docfx\docfx.json

      # push docfx/_site to gh-pages 
      - name: Publish Documentation on GitHub Pages
        uses: peaceiris/actions-gh-pages@v3
        with:
          github_token: ${{ secrets.GITHUB_TOKEN }}
          publish_dir: docfx/_site
```
