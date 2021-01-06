using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using VrmLib;
using VrmLib.Bvh;

namespace VrmLibTests
{
    public class BvhTests
    {
        DirectoryInfo RootPath
        {
            get
            {
                return new FileInfo(GetType().Assembly.Location).Directory.Parent.Parent;
            }
        }

        [Test]
        [TestCase("Assets/StreamingAssets/VRM.Samples/Motions/test.txt")]
        public void BvhTest(string filename)
        {
            var path = Path.Combine(RootPath.FullName, filename);
            var text = File.ReadAllText(path, Encoding.UTF8);
            var bvh = BvhParser.Parse(text);
            Assert.AreEqual(4007, bvh.FrameCount);

            var model = ModelExtensionsForBvh.Load(Path.GetFileName(path), bvh);
        }

        [Test]
        [TestCase("Assets/StreamingAssets/VRM.Samples/Motions/test.txt")]
        public void SkeletonEstimatorTest(string filename)
        {
            var path = Path.Combine(RootPath.FullName, filename);
            var text = File.ReadAllText(path, Encoding.UTF8);
            var bvh = BvhParser.Parse(text);

            var model = ModelExtensionsForBvh.CreateFromBvh(bvh.Root);

            var estimated = SkeletonEstimator.Detect(model.Root);
            Assert.AreEqual(estimated[HumanoidBones.hips].Name, "Hips");
            Assert.AreEqual(estimated[HumanoidBones.spine].Name, "Spine");
            Assert.AreEqual(estimated[HumanoidBones.chest].Name, "Spine1");
            Assert.AreEqual(estimated[HumanoidBones.neck].Name, "Neck");
            Assert.AreEqual(estimated[HumanoidBones.head].Name, "Head");
            Assert.AreEqual(estimated[HumanoidBones.leftShoulder].Name, "LeftShoulder");
            Assert.AreEqual(estimated[HumanoidBones.leftUpperArm].Name, "LeftArm");
            Assert.AreEqual(estimated[HumanoidBones.leftLowerArm].Name, "LeftForeArm");
            Assert.AreEqual(estimated[HumanoidBones.leftHand].Name, "LeftHand");
            Assert.AreEqual(estimated[HumanoidBones.rightShoulder].Name, "RightShoulder");
            Assert.AreEqual(estimated[HumanoidBones.rightUpperArm].Name, "RightArm");
            Assert.AreEqual(estimated[HumanoidBones.rightLowerArm].Name, "RightForeArm");
            Assert.AreEqual(estimated[HumanoidBones.rightHand].Name, "RightHand");
            Assert.AreEqual(estimated[HumanoidBones.leftUpperLeg].Name, "LeftUpLeg");
            Assert.AreEqual(estimated[HumanoidBones.leftLowerLeg].Name, "LeftLeg");
            Assert.AreEqual(estimated[HumanoidBones.leftFoot].Name, "LeftFoot");
            Assert.AreEqual(estimated[HumanoidBones.leftToes].Name, "LeftToeBase");
            Assert.AreEqual(estimated[HumanoidBones.rightUpperLeg].Name, "RightUpLeg");
            Assert.AreEqual(estimated[HumanoidBones.rightLowerLeg].Name, "RightLeg");
            Assert.AreEqual(estimated[HumanoidBones.rightFoot].Name, "RightFoot");
            Assert.AreEqual(estimated[HumanoidBones.rightToes].Name, "RightToeBase");
        }

        DirectoryInfo TestModelPath
        {
            get
            {
                var env = Environment.GetEnvironmentVariable("VRM_TEST_MODELS");
                return new DirectoryInfo(env);
            }
        }

        [Test]
        [TestCase("Motions/bvh/liveanimation/la_bvh_sample00.bvh")]
        [TestCase("Motions/bvh/liveanimation/la_bvh_sample01.bvh")]
        [TestCase("Motions/bvh/liveanimation/la_bvh_sample02.bvh")]
        [TestCase("Motions/bvh/liveanimation/la_bvh_sample03.bvh")]
        [TestCase("Motions/bvh/liveanimation/la_bvh_sample04.bvh")]
        [TestCase("Motions/bvh/liveanimation/la_bvh_sample05.bvh")]
        [TestCase("Motions/bvh/liveanimation/la_bvh_sample06.bvh")]
        [TestCase("Motions/bvh/liveanimation/la_bvh_sample07.bvh")]
        [TestCase("Motions/bvh/liveanimation/la_bvh_sample08.bvh")]
        [TestCase("Motions/bvh/liveanimation/la_bvh_sample09.bvh")]
        [TestCase("Motions/bvh/liveanimation/la_bvh_sample10.bvh")]
        [TestCase("Motions/bvh/liveanimation/la_bvh_sample11.bvh")]
        [TestCase("Motions/bvh/liveanimation/la_bvh_sample12.bvh")]
        public void SkeletonEstimatorTestLA(string filename)
        {
            var path = Path.Combine(TestModelPath.FullName, filename);
            var text = File.ReadAllText(path, Encoding.UTF8);
            var bvh = BvhParser.Parse(text);
            var model = ModelExtensionsForBvh.CreateFromBvh(bvh.Root);
            var estimated = SkeletonEstimator.Detect(model.Root);
            Assert.AreEqual(estimated[HumanoidBones.hips].Name, "Hips");
            Assert.AreEqual(estimated[HumanoidBones.spine].Name, "Chest");
            Assert.AreEqual(estimated[HumanoidBones.chest].Name, "Chest2");
            Assert.AreEqual(estimated[HumanoidBones.neck].Name, "Neck");
            Assert.AreEqual(estimated[HumanoidBones.head].Name, "Head");
            Assert.AreEqual(estimated[HumanoidBones.leftShoulder].Name, "LeftCollar");
            Assert.AreEqual(estimated[HumanoidBones.leftUpperArm].Name, "LeftShoulder");
            Assert.AreEqual(estimated[HumanoidBones.leftLowerArm].Name, "LeftElbow");
            Assert.AreEqual(estimated[HumanoidBones.leftHand].Name, "LeftWrist");
            Assert.AreEqual(estimated[HumanoidBones.rightShoulder].Name, "RightCollar");
            Assert.AreEqual(estimated[HumanoidBones.rightUpperArm].Name, "RightShoulder");
            Assert.AreEqual(estimated[HumanoidBones.rightLowerArm].Name, "RightElbow");
            Assert.AreEqual(estimated[HumanoidBones.rightHand].Name, "RightWrist");
            Assert.AreEqual(estimated[HumanoidBones.leftUpperLeg].Name, "LeftHip");
            Assert.AreEqual(estimated[HumanoidBones.leftLowerLeg].Name, "LeftKnee");
            Assert.AreEqual(estimated[HumanoidBones.leftFoot].Name, "LeftAnkle");
            Assert.AreEqual(estimated[HumanoidBones.rightUpperLeg].Name, "RightHip");
            Assert.AreEqual(estimated[HumanoidBones.rightLowerLeg].Name, "RightKnee");
            Assert.AreEqual(estimated[HumanoidBones.rightFoot].Name, "RightAnkle");
        }

        [Test]
        [TestCase("Motions/bvh/accad/eric1.bvh")]
        [TestCase("Motions/bvh/accad/ericdog.bvh")]
        [TestCase("Motions/bvh/accad/ericrun.bvh")]
        [TestCase("Motions/bvh/accad/flip.bvh")]
        [TestCase("Motions/bvh/accad/swagger.bvh")]
        public void SkeletonEstimatorTestAccad(string filename)
        {
            var path = Path.Combine(TestModelPath.FullName, filename);
            var text = File.ReadAllText(path, Encoding.UTF8);
            var bvh = BvhParser.Parse(text);
            var bones = bvh.Root.Traverse().ToArray();
            foreach (var bone in bones)
            {
                Console.WriteLine(bone.Name);
            }
            var model = ModelExtensionsForBvh.CreateFromBvh(bvh.Root);
            var estimated = SkeletonEstimator.Detect(model.Root);

            Assert.AreEqual(estimated[HumanoidBones.hips].Name, "root");
            Assert.AreEqual(estimated[HumanoidBones.spine].Name, "lowerback");
            Assert.AreEqual(estimated[HumanoidBones.chest].Name, "upperback");
            Assert.AreEqual(estimated[HumanoidBones.upperChest].Name, "thorax");
            Assert.AreEqual(estimated[HumanoidBones.neck].Name, "neck");
            Assert.AreEqual(estimated[HumanoidBones.head].Name, "head");
            Assert.AreEqual(estimated[HumanoidBones.leftShoulder].Name, "lshoulderjoint");
            Assert.AreEqual(estimated[HumanoidBones.leftUpperArm].Name, "lhumerus");
            Assert.AreEqual(estimated[HumanoidBones.leftLowerArm].Name, "lradius");
            Assert.AreEqual(estimated[HumanoidBones.leftHand].Name, "lhand");
            Assert.AreEqual(estimated[HumanoidBones.rightShoulder].Name, "rshoulderjoint");
            Assert.AreEqual(estimated[HumanoidBones.rightUpperArm].Name, "rhumerus");
            Assert.AreEqual(estimated[HumanoidBones.rightLowerArm].Name, "rradius");
            Assert.AreEqual(estimated[HumanoidBones.rightHand].Name, "rhand");
            Assert.AreEqual(estimated[HumanoidBones.rightUpperLeg].Name, "rfemur");
            Assert.AreEqual(estimated[HumanoidBones.rightLowerLeg].Name, "rtibia");
            Assert.AreEqual(estimated[HumanoidBones.rightFoot].Name, "rfoot");
            Assert.AreEqual(estimated[HumanoidBones.rightToes].Name, "rtoes");
            Assert.AreEqual(estimated[HumanoidBones.leftUpperLeg].Name, "lfemur");
            Assert.AreEqual(estimated[HumanoidBones.leftLowerLeg].Name, "ltibia");
            Assert.AreEqual(estimated[HumanoidBones.leftFoot].Name, "lfoot");
            Assert.AreEqual(estimated[HumanoidBones.leftToes].Name, "ltoes");
        }
    }
}
