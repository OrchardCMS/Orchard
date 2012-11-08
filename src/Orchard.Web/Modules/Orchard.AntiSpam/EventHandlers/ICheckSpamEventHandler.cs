using Orchard.Events;

namespace Orchard.AntiSpam.EventHandlers {
    public interface ICheckSpamEventHandler : IEventHandler {
        /// <param name="context">
        /// Dynamic object representing the parameters for the call
        /// - Content (in IContent): the IContent that should trigger events when checked
        /// - Text (in string): the text which is submitted for spam analysis
        /// - Checked (out bool): will be assigned to true if the spam could be checked
        /// - IsPam (out bool): True if the text has been reported as spam
        /// </param>
        void CheckSpam(dynamic context);
    }
}