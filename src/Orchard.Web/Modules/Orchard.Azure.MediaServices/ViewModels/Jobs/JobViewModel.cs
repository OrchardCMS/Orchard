namespace Orchard.Azure.MediaServices.ViewModels.Jobs {
    public class JobViewModel {
        public string Name { get; set; }
        public string Description { get; set; }
        public string OutputAssetName { get; set; }
        public string OutputAssetDescription { get; set; }
        public int SelectedInputAssetId { get; set; }
        public dynamic TaskEditorShape { get; set; }
    }
}