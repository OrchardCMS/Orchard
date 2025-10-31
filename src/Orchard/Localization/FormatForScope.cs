namespace Orchard.Localization
{
    public class FormatForScope
    {
        public FormatForScope(string format, string scope)
        {
            Scope = scope;
            Format = format;
        }
        public string Scope { get; set; }
        public string Format { get; set; }
    }
}
