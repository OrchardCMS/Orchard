using System;

namespace Orchard.FileSystems.Media {
    public class InvalidNameCharacterException : ArgumentException {
        public InvalidNameCharacterException(string message) : base(message) { }
    }
}
