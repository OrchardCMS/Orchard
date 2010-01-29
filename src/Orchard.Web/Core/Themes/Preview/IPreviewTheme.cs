namespace Orchard.Core.Themes.Preview {
    public interface IPreviewTheme : IDependency {
        string GetPreviewTheme();
        void SetPreviewTheme(string themeName);
    }
}