namespace Four2n.Orchard.MiniProfiler
{
    using System.Diagnostics;

    using global::Orchard.Environment;
    using global::Orchard.Environment.Extensions.Models;
    using global::Orchard.Environment.State;

    public class Eventer : IFeatureEventHandler, IShellStateManagerEventHandler
    {
        public void Installing(Feature feature)
        {
            Debug.WriteLine("Installing");
        }

        public void Installed(Feature feature)
        {
            Debug.WriteLine("Installed");
        }

        public void Enabling(Feature feature)
        {
            Debug.WriteLine("Enabling");
        }

        public void Enabled(Feature feature)
        {
            Debug.WriteLine("Enabled");
        }

        public void Disabling(Feature feature)
        {
            Debug.WriteLine("Disabling");
        }

        public void Disabled(Feature feature)
        {
            Debug.WriteLine("Disabled");
        }

        public void Uninstalling(Feature feature)
        {
            Debug.WriteLine("Uninstalling");
        }

        public void Uninstalled(Feature feature)
        {
            Debug.WriteLine("uninstalled");
        }

        public void ApplyChanges()
        {
            Debug.WriteLine("ApplyChanges");
        }
    }
}