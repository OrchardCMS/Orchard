using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Orchard.Azure.MediaServices.ViewModels.Tasks {
	public class EncodeViewModel {

		public IEnumerable<string> EncodingPresets {
			get;
			set;
		}

		[Required]
		public string SelectedEncodingPreset {
			get;
			set;
		}
	}
}