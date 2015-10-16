using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PI.Authentication.Models
{

    public class PIAuthenticationSettingsPart : ContentPart
    {
        public string LDAPString
        {
            get { return this.Retrieve(x => x.LDAPString); }
            set { this.Store(x => x.LDAPString, value); }
        }


        public string Domain
        {
            get { return this.Retrieve(x => x.Domain); }
            set { this.Store(x => x.Domain, value); }
        }

        public string LoginPage
        {
            get { return this.Retrieve(x => x.LoginPage); }
            set { this.Store(x => x.LoginPage, value); }
        }


    }

}
