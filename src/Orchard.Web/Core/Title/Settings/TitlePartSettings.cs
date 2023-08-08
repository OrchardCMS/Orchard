using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Orchard.Core.Title.Settings {
    public class TitlePartSettings {
        [Range(0, 1024)]
        [DisplayName("Maximum Length")]
        public int MaxLength {get; set;}

    }
}