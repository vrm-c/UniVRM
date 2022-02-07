# glbフォーマット概説

JSON記述部と、画像や頂点配列を記録するバイナリ部の２つの部分からなります。

gltf形式では、URLやパスで参照する方法で外部のバイナリデータにアクセスします。
glb形式ではJSON部とバイナリ部をひとつのファイルにまとめていて、バイト列のオフセットでバイナリデータにアクセスします。
プログラムから扱うには外部ファイルへのアクセスが無いglb形式の方が簡単[^VRM_glb]です。

[^VRM_glb]: VRMではglbを採用しています。

## glb形式

``ヘッダ部 + チャンク部繰り返し``という構造になっています。
実質的には、
``ヘッダ部 + JSON CHUNk + BINARY CHUNK``となります。

ヘッダ部

| 長さ | 内容           | 型    | 値     |
|:-----|:---------------|:------|:-------|
| 4    |                | ascii | "glTF" |
| 4    | gltfバージョン | int32 | 2      |
| 4    | file size      | int32 |        |

チャンク部

| 長さ       | 内容       | 型       | 値                  |
|:-----------|:-----------|:---------|:--------------------|
| 4          | chunk size | int32    |                     |
| 4          | chunk type | ascii    | "JSON" or "BIN\x00" |
| chunk size | chunk body | バイト列 |                     |

### python3によるパース例

```python
import struct
import json

class Reader:
    def __init__(self, data: bytes)->None:
        self.data = data
        self.pos = 0

    def read_str(self, size):
        result = self.data[self.pos: self.pos + size]
        self.pos += size
        return result.strip()

    def read(self, size):
        result = self.data[self.pos: self.pos + size]
        self.pos += size
        return result

    def read_uint(self):
        result = struct.unpack('I', self.data[self.pos:self.pos + 4])[0]
        self.pos += 4
        return result


def parse_glb(data: bytes):
    reader = Reader(data)
    magic = reader.read_str(4)
    if  magic != b'glTF':
        raise Exception(f'magic not found: #{magic}')

    version = reader.read_uint()
    if version != 2:
        raise Exception(f'version:#{version} is not 2')

    size = reader.read_uint()
    size -= 12

    json_str = None
    body = None
    while size > 0:
        #print(size)

        chunk_size = reader.read_uint()
        size -= 4

        chunk_type = reader.read_str(4)
        size -= 4

        chunk_data = reader.read(chunk_size)
        size -= chunk_size

        if chunk_type == b'BIN\x00':
            body = chunk_data
        elif chunk_type == b'JSON':
            json_str = chunk_data
        else:
            raise Exception(f'unknown chunk_type: {chunk_type}')

    return json.loads(json_str), body


with open('AliciaSolid.vrm', 'rb') as f:
    parsed, body = parse_glb(f.read())
```
