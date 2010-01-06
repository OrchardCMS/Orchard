using System;
using Orchard.Mvc.ViewModels;

namespace Orchard.Users.ViewModels {
    public class LogOnViewModel : BaseViewModel {
        public string Title { get; set; }

        public string ReturnUrl { get; set; }
    }
}
