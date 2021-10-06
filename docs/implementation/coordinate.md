# 座標系の変換

UniVRMは、インポート・エクスポート時に座標変換を実行しています。

|       | 右 | 上 | 前 | 掌性 | UV原点 |                                                        |
|-------|----|----|----|------|--------|--------------------------------------------------------|
| Unity | +X | +Y | +Z | 左手 | 左下   |                                                        |
| VRM-0 | +X | +Y | -Z | 右手 | 左上   |                                                        |
| VRM-1 | -X | +Y | +Z | 右手 | 左上   |                                                        |
| glTF  |    | +Y |    | 右手 | 左上   | デフォルトはVRM-0方式。オプションでVRM-1方式を選べます |

## UV

以下のように変換します。

```{math}
V^{\prime} = 1 - V
```

## Unity <-> VRM0

Z軸を反転します。

### Vector3: Position, Normalなど

```csharp
public static Vector3 ReverseZ(this Vector3 v)
{
    return new Vector3(v.x, v.y, -v.z);
}
```

### Quaternion: Rotation

```csharp
public static Quaternion ReverseZ(this Quaternion q)
{
    float angle;
    Vector3 axis;
    q.ToAngleAxis(out angle, out axis);
    return Quaternion.AngleAxis(-angle, ReverseZ(axis));
}
```

### Matrix: BindMatrices

スケール値が入っているとうまくいきません

```csharp
public static Matrix4x4 ReverseZ(this Matrix4x4 m)
{
#if UNITY_2017_1_OR_NEWER
    m.SetTRS(m.GetColumn(3).ReverseZ(), m.rotation.ReverseZ(), Vector3.one);
#else
    m.SetTRS(m.ExtractPosition().ReverseZ(), m.ExtractRotation().ReverseZ(), Vector3.one);
#endif
    return m;
}
```

## Unity <-> VRM1

X軸を反転します。
