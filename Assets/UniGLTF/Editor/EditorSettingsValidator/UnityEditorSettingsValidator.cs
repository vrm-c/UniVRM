using System.Collections.Generic;
using System.Linq;

namespace UniGLTF.EditorSettingsValidator
{
    public sealed class UnityEditorSettingsValidator
    {
        public IEnumerable<IUnitySettingsValidator> Validators { get; } = new List<IUnitySettingsValidator>
        {
            new UnityColorSpaceSettingsValidator(),
        };
        
        public bool IsValid()
        {
            return Validators.All(validator => validator.IsValid);
        }
    }
}