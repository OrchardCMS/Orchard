namespace Orchard.DisplayManagement {
    public interface IShapeMetadata {
        string Type { get; set; }
        string Position { get; set; }
        bool WasExecuted { get; set; }
    }
}
