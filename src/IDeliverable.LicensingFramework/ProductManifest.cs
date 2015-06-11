namespace IDeliverable.Licensing
{
    public class ProductManifest
    {
        public ProductManifest(int productId, string extensionName)
        {
            ProductId = productId;
            ExtensionName = extensionName;
        }

        /// <summary>
        /// The SendOwl product ID.
        /// </summary>
        public int ProductId { get; private set; }

        /// <summary>
        /// The tehchnical name of the module or theme.
        /// </summary>
        public string ExtensionName { get; private set; }
    }
}