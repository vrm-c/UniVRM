# ClothWarp(仮)

これは UniVRM の cloth の開発版です。

## 概要

縦糸(Warp)を横に連結して四角格子(Grid)を作ります。

> 縦糸は従来の `SpringBone` とだいたい同じものです。

各四角格子にはバネによる横方向の拘束(フックの法則)と当たり判定(２枚の三角形) を持たせます。

これにより縦糸の間を `Collider`(球/カプセル) がすりぬけることを防止することができます。

## Components

設定置き場。布を構成する縦糸(Warp)と、縦糸を横に連結した四角格子(Grid)の２段階とする

### UniVRM10.ClothWarp.Components.ClothWarpRoot

各 particle にはVRM-0.X のように子孫を自動登録し Base 設定を適用する。
Base 設定を変更する場合は変える方を列挙設定できる。
Custom と Disable を選択できる。

- ゆれものの根元にアタッチする
- [x] 子孫に HumanoidBone がある場合にアタッチ不可
- [ ] 枝分かれ
- [ ] WarpRoot らからデフォルト以外の Warp を選び出す
- [ ] Center
- [ ] Scale

```
         ひとつめの joint にアタッチする
         | 子孫の Transform は自動的に登録される(デフォルト設定を適用)
         | | 個別に無効化・カスタム設定をできるようにする
WarpRoot O=o=o=o=o
              \
               o 枝分かれも自動的に登録
```

|                 | MonoBehaviour | move                      | rotate     |
| --------------- | ------------- | ------------------------- | ---------- |
| root            | attach        | 動かない                  | 回転する   |
| particles[x]    |               | verlet 積分で慣性移動する | 回転する   |
| particles[末端] |               | verlet 積分で慣性移動する | 回転しない |

#### Settings

- [warp]verlet_center: 速度を算出する座標の原点(curernt-pvev は
- [warp]stiffness(剛性): 初期姿勢に戻る力
- [warp]force(Vec3): 下向きにすれば gravity
- [warp]dragforce(抗力): 減速率(1-dragforce を velocity に乗算する)
 (current-current_center)-(prev-prev_center) に変わる)
- [particle]mode: base, custom, disabled
- [particle]stiffness(剛性): 初期姿勢に戻る力
- [particle]force(Vec3): 下向きにすれば gravity
- [particle]dragforce(抗力): 減速率(1-dragforce を velocity に乗算する)

#### 枝分かれの例外処理

- 枝分かれした particle は一番兄の particle の local 移動を複製する(独自の速度、衝突はしない)

### UniVRM10.ClothWarp.Components.ClothGrid

- 隣り合う Warp に横方向の拘束が追加される
- particle sphere の衝突のかわりに四角格子の三角形が衝突する

- [ ] デフォルト(WarpRoot から children[0] を選択する)以外の縦糸選択
- [ ] 三角形の厚み(法線方向の offset)

```
WarpRoot0 o=o=o=o
          | | |
WarpRoot1 o=o=o <- 短い方の長さに合わせる
          | | |
WarpRoot2 o=o=o
          |
          v
          CloseLoop=true ならば WarpRoot0 と連結して輪を閉じる
```

#### Settings

- CloseLoop: 最後の Warp と 最初の Warp を接続して輪を作る。Warp が３本以上必要。

## Colliders

- UniVRM10.VRM10SpringBoneCollider
- UniVRM10.VRM10SpringBoneCollider.Group

を使う。

## Logic

### input phase

- current_position, prev_position が持ち越される
- {FromTransform}{位置を修正}{回転を得る} Update root position. each rotation.

### dynamics phase

- (if cloth){add force} weft constarint(横方向 ばね拘束)
- {位置を求める} 速度、力を解決
- {位置を修正} parent constraint(親子間の距離を一定に保つ。再帰)

### collision phase

- (if not cloth){位置を修正} particle(Sphere) x collider(Sphere or Capsule)
- (if cloth){位置を修正} cloth(Triangle) x collider(SPhere or Capsule)

### output phase

- {回転を修正} calc rotation from position(再帰) 👈 ここまで回転は出てこないことに注意
- {ToTransform} apply result rotation 👈 回転しかしない(伸びない)

## TODO

### Debug

- [ ] Debug 用の詳細な Gizmo

### 工夫

- [ ] Cloth の片面衝突
- [ ] 衝突時の velocity 下げ

### Optimize

- [x] 衝突グループ
