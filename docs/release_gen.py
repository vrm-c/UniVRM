#
# Release 時のドキュメントを作成するスクリプト
#
# required
#   pip install GitPython pyperclip
#
# 1. github release page の markdown => clipboard
# 2. changelog => docs/release/079/v0.XX.Y.md
# 3. download button => docs/index.html
#
import pathlib
import re
import subprocess
import git.repo
import re
import pathlib
import io
from functools import cmp_to_key

HERE = pathlib.Path(__file__).absolute().parent
UNIVRM_VERSION = HERE.parent / 'Assets/VRM/Runtime/Format/VRMVersion.cs'
MERGE_PATTERN = re.compile(r'Merge pull request #(\d+)')


def gen(version: str, hash: str):
    version_hash = f'{version}_{hash[0:4]}'
    return f'''
# Download

* for `Unity-2019.4.LTS` or later
* [UniVRM-{version_hash}.unitypackage](https://github.com/vrm-c/UniVRM/releases/download/v{version}/UniVRM-{version_hash}.unitypackage)

> `v0.87.0` から UniGLTF_VRMShaders と UniVRM が合体してひとつになりました。
> From `v0.87.0`, UniGLTF_VRMShaders and UniVRM have been merged into one.

ReleaseNote
* [日本語](https://vrm-c.github.io/UniVRM/ja/release/079/v{version}.html)
* [English](https://vrm-c.github.io/UniVRM/en/release/079/v{version}.html)

## other unitypackage
### UniVRM API sample
* [UniVRM_Samples-{version_hash}.unitypackage](https://github.com/vrm-c/UniVRM/releases/download/v{version}/UniVRM_Samples-{version_hash}.unitypackage)
### VRM-1.0Beta
* [VRM-{version_hash}.unitypackage](https://github.com/vrm-c/UniVRM/releases/download/v{version}/VRM-{version_hash}.unitypackage)
### VRM-1.0Beta API sample
* [VRM_Samples-{version_hash}.unitypackage](https://github.com/vrm-c/UniVRM/releases/download/v{version}/VRM_Samples-{version_hash}.unitypackage)

|package|folder|
|-|-|
|UniVRM|Assets/VRMShaders, Assets/UniGLTF, Assets/VRM|
|UniVRM_Samples|Assets/VRM_Samples|
|VRM|Assets/VRMShaders, Assets/UniGLTF, Assets/VRM10|
|VRM_Samples|Assets/VRM10_Samples|

# UPM

| UPM package         | rename           | UPM url                                                                |
|---------------------|------------------|------------------------------------------------------------------------|
| com.vrmc.vrmshaders |                  | https://github.com/vrm-c/UniVRM.git?path=/Assets/VRMShaders#v{version} |
| com.vrmc.gltf       | com.vrmc.unigltf | https://github.com/vrm-c/UniVRM.git?path=/Assets/UniGLTF#v{version}    |
| com.vrmc.univrm     |                  | https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM#v{version}        |
| com.vrmc.vrm        | com.vrmc.univrm1 | https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM10#v{version}      |

```json
// manifest.json
{{
  "dependencies": {{
    ///
    "com.vrmc.vrmshaders": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRMShaders#v{version}",
    "com.vrmc.gltf": "https://github.com/vrm-c/UniVRM.git?path=/Assets/UniGLTF#v{version}",
    "com.vrmc.univrm": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM#v{version}",
    "com.vrmc.vrm": "https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM10#v{version}",
    ///
  }}
}}
```

'''


def get_version() -> str:
    m = re.search(r'public const string VERSION = "(\d.\d+.\d)";',
                  UNIVRM_VERSION.read_text(encoding='utf-8'))
    if m:
        return m[1]
    raise Exception("no version")


def get_hash(repo, tag_name) -> str:
    # res = subprocess.check_output("git rev-parse HEAD")
    # return res.decode('utf-8')
    for tag in repo.tags:
        if tag.name == tag_name:
            return tag.commit.hexsha
    raise Exception()


def copy_release_md(version: str, hash: str):
    text = gen(version, hash)
    import pyperclip
    pyperclip.copy(text)
    print('copy to clipboard')


#
#
#
def change_log(repo: git.repo.Repo, version: str):
    major, minor, patch = [int(x) for x in version.split('.')]
    rev = f'v{major}.{minor-1}.0..v{major}.{minor}.0'

    w = io.StringIO()
    w.write(f'# v{version}: 1.0準備\n')
    w.write('\n')
    for item in repo.iter_commits(rev=rev):
        m = MERGE_PATTERN.match(item.message)
        if m:
            # merge commit
            pr = m[1]
            lines = item.message.splitlines()

            w.write(
                f'* [[\\#{pr}](https://github.com/vrm-c/UniVRM/pull/{pr})] {lines[2]}\n'
            )
    return w.getvalue()


def get_tags(repo):
    P = re.compile(r'v(\w+)\.(\w+)\.(\w+)')
    for tag in repo.tags:
        m = P.match(tag.name)
        if m:
            yield int(m[1]), int(m[2]), int(m[3])


if __name__ == '__main__':
    repo = git.repo.Repo(str(HERE.parent))

    def cmp_tag(l, r):
        if l[0] != r[0]:
            return l[0] - r[0]
        if l[1] != r[1]:
            return l[1] - r[1]
        return l[2] - r[2]

    tags = [tag for tag in get_tags(repo)]
    tags = sorted(tags, key=cmp_to_key(cmp_tag))
    x, y, z = tags[-1]
    version = f'{x}.{y}.{z}'
    hash = get_hash(repo, f'v{version}')
    # 1.
    copy_release_md(f'{version}', hash)
    # 2.
    release = HERE / f'release/079/v{version}.md'
    if not release.exists():
        text = change_log(repo, f'{version}')
        release.write_text(text, encoding='utf-8')
    # 3.
    (HERE / 'index.html').write_text(f'''<html>
<body>

    <head>
        <style type="text/css">
            html,
            body {{
                color: black;
                background-color: white;
                width: 100%;
                height: 100%;

                display: flex;
                flex-direction: column;
            }}

            main {{
                display: flex;
                justify-content: center;
                align-items: center;
                flex-grow: 1;
            }}

            .btn {{
                color: white;
                background-color: green;
                padding: 0.5em;
                border-radius: 0.3em;
                text-decoration: none;
            }}

            .btn h1 {{
                text-align: center;
            }}

            .btn h2 {{
                text-align: center;
            }}
        </style>
    </head>
    <header>

    </header>
    <main>
        <a href="https://github.com/vrm-c/UniVRM/releases/download/v{version}/UniVRM-{version}_{hash[0:4]}.unitypackage" class="btn">
            <div class="btn">
                <h1>Download</h1>
                <h2>UniVRM-{version}</h2>
            </div>
        </a>
    </main>
    <nav>
        API Document
        <ul>
            <li><a href="./ja/">日本語</a></li>
            <li><a href="./en/">English</a></li>
        </ul>
    </nav>
</body>
</html>
''',
                                     encoding='utf-8')
