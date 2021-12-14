using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System.ComponentModel.DataAnnotations;

namespace Orchard.Users.ViewModels
{
    [OrchardFeature("Orchard.Users.ROUser")]
    public class UserProfileViewModel {
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public IContent User { get; set; }
        public String SSN { get; set; }
        public String Address { get; set }
    }
}