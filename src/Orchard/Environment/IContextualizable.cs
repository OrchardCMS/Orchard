using System;

namespace Orchard.Environment {
    public interface IContextualizable {
        void Hook(params Action[] contextualizers);
    }
}