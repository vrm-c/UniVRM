using UnityEngine;


namespace VRM
{
    public class VRMHumanoidDescription : MonoBehaviour
    {
        [SerializeField]
        public Avatar Avatar;

        [SerializeField]
        public UniHumanoid.AvatarDescription Description;

        public UniHumanoid.AvatarDescription GetDescription(out bool isCreated)
        {
            isCreated = false;
            if (Description != null)
            {
                return Description;
            }

#if UNITY_EDITOR
            if (Avatar != null)
            {
                isCreated = true;
                return UniHumanoid.AvatarDescription.CreateFrom(Avatar);
            }
#endif

            return null;
        }

        private void OnValidate()
        {
            if (Avatar != null && (!Avatar.isValid || !Avatar.isHuman))
            {
                Avatar = null;
            }
        }

        void Reset()
        {
            if (TryGetComponent<Animator>(out var animator))
            {
                Avatar = animator.avatar;
                if (Avatar == null)
                {
                    return;
                }
            }
        }
    }
}
