using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Orchard.Core.Title.Settings {

    
    public class TitlePartSettings {
        public const int MAX_TITLE_LENGTH = 1024;
        [Range(0, MAX_TITLE_LENGTH)]
        [DisplayName("Maximum Length")]
        public int MaxLength {get; set;}

    }
}