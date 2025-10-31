using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Orchard.Users.ViewModels
{
    [OrchardFeature("Orchard.Users.EditPasswordByAdmin")]
    public class UserEditPasswordViewModel
    {
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        public IContent User { get; set; }
    }
}