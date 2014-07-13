using System.ComponentModel.DataAnnotations;

namespace Orchard.Azure.MediaServices.ViewModels.Tasks {
	public class CreateThumbnailViewModel {

		public CreateThumbnailViewModel() {
			FileName = "{OriginalFilename}_{Size}_{ThumbnailTime}_{ThumbnailIndex}.{DefaultExtension}";
			Width = "*";
			Height = "80";
			StartTime = "0:0:0";
			Type = "Jpeg";
		}

		[Required]
		public string Width {
			get;
			set;
		}

		[Required]
		public string Height {
			get;
			set;
		}

		[Required]
		public string Type {
			get;
			set;
		}

		[Required]
		public string FileName {
			get;
			set;
		}

		[Required]
		public string StartTime {
			get;
			set;
		}

		public string StopTime {
			get;
			set;
		}

		public string Step {
			get;
			set;
		}
	}
}