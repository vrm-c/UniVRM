using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UniVRM10.VRM10Viewer
{
    [Serializable]
    class UIFields
    {
        [SerializeField]
        Toggle ToggleMotionTPose = default;

        [SerializeField]
        Toggle ToggleMotionBVH = default;

        [SerializeField]
        ToggleGroup ToggleMotion = default;

        public void Reset(ObjectMap map)
        {
            ToggleMotionTPose = map.Get<Toggle>("TPose");
            ToggleMotionBVH = map.Get<Toggle>("BVH");
            ToggleMotion = map.Get<ToggleGroup>("_Motion_");
        }

        public bool IsTPose
        {
            get => ToggleMotion.ActiveToggles().FirstOrDefault() == ToggleMotionTPose;
            set
            {
                ToggleMotionTPose.isOn = value;
                ToggleMotionBVH.isOn = !value;
            }
        }
    }
}