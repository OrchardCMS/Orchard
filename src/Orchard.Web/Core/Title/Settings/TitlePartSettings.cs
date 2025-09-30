using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Orchard.Core.Title.Settings {

    
    public class TitlePartSettings {
        // Whenever this constant is changed a new migration step must be created to update the length of the field on the DB
        public const int MaxTitleLength = 1024;
        [Range(0, MaxTitleLength)]
        [DisplayName("Maximum Length")]
        public int MaxLength {get; set;}

    }
}