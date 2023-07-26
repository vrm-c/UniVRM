using UnityEditor;
using UnityEngine;

namespace VRM10.Settings
{
    [FilePath("ProjectSettings/Vrm10ProjectEditorSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal class Vrm10ProjectEditorSettings : ScriptableSingleton<Vrm10ProjectEditorSettings>
    {
        [SerializeField] private MaterialDescriptorGeneratorFactory materialDescriptorGeneratorFactory; 
        
        public MaterialDescriptorGeneratorFactory MaterialDescriptorGeneratorFactory => materialDescriptorGeneratorFactory;
        public void Save()
        {
            Save(true);
        }
    }
}