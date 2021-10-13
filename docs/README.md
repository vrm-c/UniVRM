```{orphan}
```

# Sphinx 作業

## 初期化

```
$ pip install sphinx
$ mkdir docs
$ cd docs
docs$ sphinx-quickstart
```

`docs/_build/ja` に日本語ドキュメントを出力する
```
doc$ sphinx-build . _build/ja
```

## MySt 導入

markdown で記事を記述する

```
$ pip install --upgrade myst-parser
```

conf.py
```py
extensions = ['myst_parser']
```

## gettext 導入

```
$ pip install sphinx-intl
```

conf.py
```py
locale_dirs = ['locale/']
gettext_compact = False
```

## gettext 更新

potファイル作成
```
doc$ sphinx-build -M gettext . _pot
# => _pot/gettext
```

poファイルを作成
```
doc$ sphinx-intl update -p _pot/gettext -l en
# => locale/en
```

ロケールを使ってサイトビルド
```
doc$ sphinx-build . _build/en -D language=en
```
