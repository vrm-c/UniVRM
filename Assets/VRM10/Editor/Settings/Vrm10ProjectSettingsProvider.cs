using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRM10.Settings
{
    internal sealed class Vrm10ProjectSettingsProvider : SettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateProvider() => new Vrm10ProjectSettingsProvider();

        private Vrm10ProjectSettingsProvider() : base("Project/VRM10", SettingsScope.Project)
        {
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            var asset = Vrm10ProjectEditorSettings.instance;
            asset.hideFlags &= ~HideFlags.NotEditable;
            var assetObject = new SerializedObject(asset);

            var contentElement = new VisualElement
            {
                style =
                {
                    paddingLeft = 8,
                    paddingRight = 2,
                    paddingTop = 2,
                    paddingBottom = 2
                }
            };
            rootElement.Add(contentElement);
            var title = new Label
            {
                text = "VRM10",
                style =
                {
                    fontSize = 19,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };
            contentElement.Add(title);
            var propertyField = new PropertyField(assetObject.FindProperty("materialDescriptorGeneratorFactory"));
            propertyField.RegisterValueChangeCallback(_ => asset.Save());
            contentElement.Add(propertyField);

            contentElement.Bind(assetObject);
        }
    }
}