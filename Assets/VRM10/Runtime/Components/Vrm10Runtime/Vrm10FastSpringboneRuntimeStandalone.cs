using UniGLTF;
using UnityEngine;
using UniGLTF.SpringBoneJobs.InputPorts;
using UniGLTF.SpringBoneJobs;
using System.Threading.Tasks;
using UniGLTF.SpringBoneJobs.Blittables;

namespace UniVRM10
{
    /// <summary>
    /// FastSpringbone(job) で動作します。
    /// FastSpringBoneService(Singleton)を経由せずに直接実行します。
    /// 
    /// シーンに２体以上の vrm-1.0 モデルがある場合は FastSpringBoneService でバッチングする方が効率的です。
    /// </summary>
    public class Vrm10FastSpringboneRuntimeStandalone : IVrm10SpringBoneRuntime
    {
        private Vrm10Instance m_instance;
        private FastSpringBoneBuffer m_fastSpringBoneBuffer;
        public FastSpringBoneBufferCombiner m_bufferCombiner = new();
        private FastSpringBoneScheduler m_fastSpringBoneScheduler;

        public void SetJointLevel(Transform joint, BlittableJointMutable jointSettings)
        {
            if (m_bufferCombiner.Combined is FastSpringBoneCombinedBuffer combined)
            {
                combined.SetJointLevel(joint, jointSettings);
            }
        }

        public void SetModelLevel(Transform modelRoot, BlittableModelLevel modelSettings)
        {
            if (m_bufferCombiner.Combined is FastSpringBoneCombinedBuffer combined)
            {
                combined.SetModelLevel(modelRoot, modelSettings);
            }
        }

        public Vrm10FastSpringboneRuntimeStandalone()
        {
            m_fastSpringBoneScheduler = new(m_bufferCombiner);
        }

        public async Task InitializeAsync(Vrm10Instance instance, IAwaitCaller awaitCaller)
        {
            m_instance = instance;

            // NOTE: FastSpringBoneService は UnitTest などでは動作しない
            if (Application.isPlaying)
            {
                await ConstructSpringBoneAsync(awaitCaller);
            }
        }

        public void Dispose()
        {
            m_bufferCombiner.Unregister(m_fastSpringBoneBuffer);
            m_fastSpringBoneBuffer.Dispose();

            m_fastSpringBoneScheduler.Dispose();
            m_bufferCombiner.Dispose();
        }

        /// <summary>
        /// このVRMに紐づくSpringBone関連のバッファを再構築する
        /// ランタイム実行時にSpringBoneに対して変更を行いたいときは、このメソッドを明示的に呼ぶ必要がある
        /// </summary>
        public bool ReconstructSpringBone()
        {
            // new ImmediateCaller() により即時実行して結果を得る。
            // スパイクは許容する。
            var task = ConstructSpringBoneAsync(new ImmediateCaller());
            return task.Result;
        }

        /// <summary>
        /// 多重実行防止。
        /// m_building は ConstructSpringBoneAsync 専用。他で使う場合は注意。
        /// </summary>
        private bool m_building = false;

        /// <returns>ConstructSpringBoneAsync がすでに実行中の場合は中止して false で戻る</returns>
        private async Task<bool> ConstructSpringBoneAsync(IAwaitCaller awaitCaller)
        {
            if (m_building)
            {
                return false;
            }
            m_building = true;

            // 登録削除
            if (m_fastSpringBoneBuffer != null)
            {
                m_bufferCombiner.Unregister(m_fastSpringBoneBuffer);
            }

            m_fastSpringBoneBuffer = await FastSpringBoneBufferFactory.ConstructSpringBoneAsync(new ImmediateCaller(), m_instance, m_fastSpringBoneBuffer);

            // 登録
            m_bufferCombiner.Register(m_fastSpringBoneBuffer);

            m_building = false;
            return true;
        }

        public void RestoreInitialTransform()
        {
            // Spring の joint に対応する transform の回転を初期状態
            var instance = m_instance.GetComponent<RuntimeGltfInstance>();
            foreach (var logic in m_fastSpringBoneBuffer.Logics)
            {
                var transform = m_fastSpringBoneBuffer.Transforms[logic.headTransformIndex];
                transform.localRotation = instance.InitialTransformStates[transform].LocalRotation;
            }

            // jobs のバッファにも反映する必要あり
            m_bufferCombiner.InitializeJointsLocalRotation(m_fastSpringBoneBuffer);
        }

        public void Process()
        {
            m_fastSpringBoneScheduler.Schedule(Time.deltaTime).Complete();
        }
    }
}