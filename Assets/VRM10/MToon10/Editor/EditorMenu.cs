using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VRM10.MToon10.Editor
{
    internal static class EditorMenu
    {
        [MenuItem("Assets/VRM10/MToon -> MToon10 Material Migration", isValidateFunction: true, priority: 1100)]
        private static bool MigrateMToonMaterialValidation()
        {
            var objects = Selection.objects;
            return objects.All(x => x is Material);
        }

        [MenuItem("Assets/VRM10/MToon -> MToon10 Material Migration", isValidateFunction: false, priority: 1100)]
        private static void MigrateMToonMaterial()
        {
            var objects = Selection.objects;
            var migrator = new MToonMaterialMigrator();
            Undo.RecordObjects(objects, nameof(MigrateMToonMaterial));
            foreach (var obj in Selection.objects)
            {
                if (obj == null) continue;

                if (obj is Material material && migrator.TryMigrate(material, validateShaderName: true))
                {
                    Debug.Log($"Migrated {material.name} to MToon10");
                }
                else
                {
                    Debug.LogWarning($"Failed to migrate {obj.name} to MToon10");
                }
            }
        }
    }
}