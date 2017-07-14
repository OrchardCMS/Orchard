using System;

namespace Orchard.Glimpse.Models {
    public interface IDurationMessage {
        TimeSpan Duration { get; set; }
    }
}