# VRアプリケーションの FirstPerson 設定による Renderer の可視制御

* `isSelf==true` 自分のアバター
* `isSelf==false` 自分以外のアバター
* FP Camera: HMDと連動するカメラ
* TP Camera: それ以外のすべてのカメラ

| isSelf | FirstPerson設定 | FP Camera | TP Camera | レイヤーによる可視制御例                      |
|--------|-----------------|-----------|-----------|-----------------------------------------------|
| true   | FirstPersonOnly | ✅         | ❌         | FIRSTPERSON                                   |
| true   | ThirdPersonOnly | ❌         | ✅         | THIRDPERSON                                   |
| true   | Both            | ✅         | ✅         | default                                       |
| true   | Auto            | -         | -         | Both と ThirdPersonOnly に分割する |
| false  | FirstPersonOnly | ❌         | ❌         | 完全に描画されない。Rendererの描画を止める    |
| false  | ThirdPersonOnly | ✅         | ✅         | default                                       |
| false  | Both            | ✅         | ✅         | default                                       |
| false  | Auto            | ✅         | ✅         | default。メッシュ分割など特別な処理は不要     |
