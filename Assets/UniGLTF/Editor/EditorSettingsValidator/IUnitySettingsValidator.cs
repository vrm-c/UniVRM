namespace UniGLTF.EditorSettingsValidator
{
    public interface IUnitySettingsValidator
    {
        bool IsValid { get; }
        string HeaderDescription { get; }
        string CurrentValueDescription { get; }
        string RecommendedValueDescription { get; }
        void Validate();
    }
}