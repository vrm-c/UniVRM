using UnityEditor;
using UnityEngine;

namespace VRMShaders.VRM10.MToon10.Editor
{
    internal static class EditorMenu
    {
        [MenuItem("Assets/VRM10/MToon -> MToon10 Migration", isValidateFunction: true, priority: 910)]
        private static bool MigrateMToonMaterialValidation()
        {
            if (Selection.activeObject is Material material)
            {
                return material.shader != null && material.shader.name == "VRM/MToon";
            }
            return false;
        }

        [MenuItem("Assets/VRM10/MToon -> MToon10 Migration", isValidateFunction: false, priority: 910)]
        private static void MigrateMToonMaterial()
        {
            if (Selection.activeObject is Material material)
            {
                Undo.RecordObject(material, nameof(MigrateMToonMaterial));
                var migrator = new MToonMaterialMigrator();
                if (migrator.TryMigrate(material))
                {
                    Debug.Log($"Migrated {material.name} to MToon10");
                }
                else
                {
                    Debug.LogWarning($"Failed to migrate {material.name} to MToon10");
                }
            }
        }
    }
}