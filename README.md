# DepthFirstScheduler(深さ優先スケジューラー)
Asynchronous task scheduler for Unity-5.6 or later

これは、Unity5.6でTaskが無いことを補完するためのライブラリです。
木構造にタスクを組み立てて深さ優先で消化します。

* タスクの実行スケジューラー(Unityメインスレッドやスレッドプール)を指定できる

# 使い方

```cs
var schedulable = new Schedulable<Unit>();

schedulable
    .AddTask(Scheduler.ThreadPool, () => // 子供のタスクを追加する
    {
        return glTF_VRM_Material.Parse(ctx.Json);
    })
    .ContinueWith(Scheduler.MainThread, gltfMaterials => // 兄弟のタスクを追加する
    {
        ctx.MaterialImporter = new VRMMaterialImporter(ctx, gltfMaterials);
    })
    .Subscribe(Scheduler.MainThread, onLoaded, onError);
    ;
```

# Schedulable<T>
T型の結果を返すタスク。

## AddTask(IScheduler scheduler, Func<T> firstTask) 

子供のタスクを追加する。

ToDo: 一つ目の子供に引数を渡す手段が無い

## ContinueWith

## ContinueWithCoroutine

## OnExecute

動的にタスクを追加するためのHook。

中で、

```
parent.AddTask
```

することで実行時に子タスクを追加できる。

## Subscribe
タスクの実行を開始する。
実行結果を得る。


# Scheduler
## StepScheduler
Unity
## CurrentThreadScheduler
即時
## ThreadPoolScheduler
スレッド
## ThreadScheduler
スレッド

