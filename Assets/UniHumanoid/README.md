# UniHumanoid

Unity humanoid utility with bvh importer.

# License

* [MIT](./LICENSE.md)

# BVH runtime loader

```cs
var context = new BvhImporterContext();
context.Parse(path);
context.Load(); // create Skeleton hierarchy and mesh for visualize
GameObject root = context.Root;
```

## RuntimeLoader
* Scenes/RuntimeBvhLoader.unity

## RuntimeLoader and PoseTransfer
Load BVH and transfer pose to any model with humanoid avatar.

* Scenes/PoseTransfer.unity

![humanpose transfer target](doc/humanpose_transfer_inspector.png)

![humanpose transfer](doc/humanpose_transfer.png)

# Load bvh and create prefab with AnimationClip

Drop bvh file to Assets folder.
Then, AssetPostprocessor import bvh file.

* create a hierarchy prefab 
* create a humanoid Avatar
* create a legacy mode AnimationClip
* create a skinned mesh for preview

![bvh prefab](doc/assets.png)

Instanciate prefab to scene.

![bvh gameobject](doc/mesh.png)

That object can play. 

# BoneMapping

This script help create human avatar from exist GameObject hierarchy.
First, attach this script to root GameObject that has Animator.

Next, setup below.

* model position is origin
* model look at +z orientation
* model root node rotation is Quatenion.identity
* Set hips bone.

press Guess bone mapping.
If fail to guess bone mapping, you can set bones manually.

Optional, press Ensure T-Pose.
Create avatar.

![bvh bone mapping](doc/bvh_bonemapping.png)

These humanoids imported by [UniGLTF](https://github.com/ousttrue/UniGLTF) and created human avatar by BoneMapping. 

![humanoid](doc/humanoid.gif)

# Download BVH files

* https://sites.google.com/a/cgspeed.com/cgspeed/motion-capture
* http://mocapdata.com/
* http://www.thetrailerspark.com/download/Mocap/Packed/EYES-JAPAN/BVH/

