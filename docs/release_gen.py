#
# github Release の markdown を作るスクリプト
#
import pathlib
import re
import subprocess

HERE = pathlib.Path(__file__).absolute().parent
UNIVRM_VERSION = HERE.parent / 'Assets/VRM/Runtime/Format/VRMVersion.cs'


def gen(version: str, hash: str):
    version_hash = f'{version}_{hash[0:4]}'
    print(f'''
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

''')


def get_version() -> str:
    m = re.search(r'public const string VERSION = "(\d.\d+.\d)";',
                  UNIVRM_VERSION.read_text(encoding='utf-8'))
    if m:
        return m[1]
    raise Exception("no version")


def get_hash() -> str:
    res = subprocess.check_output("git rev-parse HEAD")
    return res.decode('utf-8')


if __name__ == '__main__':
    version = get_version(UNIVRM_VERSION)
    hash = get_hash()
    gen(version, hash)
