namespace IDeliverable.Licensing
{
    public interface ILicense
    {
        int ProductId { get; }
        string Key { get; }
    }
}