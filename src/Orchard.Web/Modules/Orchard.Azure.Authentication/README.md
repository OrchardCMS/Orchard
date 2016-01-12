# Orchard Azure Authentication Module

Orchard Azure Authentication is a module that enables Azure Authentication to Orchard using Azure Active Directory and OpenID connect. The module overrides the default login and integrates into the standard pipeline for Orchard.

## How to use

1. If you don't have an Azure Active Directory, get more info [here](https://azure.microsoft.com/en-us/documentation/articles/active-directory-whatis/). Create an application in Azure that will correspond to your Orchard instance. The settings you will need to configure Orchard Azure Active Directory Authentication will be on the configure tab of the created application.
2. Before enabling the module, your must add an admin user to Orchard via the Orchard admin. Ensure that the user name matches the Azure Active Directory user name for the user you wish to have admin privileges.
3. After enabling the module, before navigating away from the admin, go to site settings under the **Azure Authentication** group. Fill in the information from your Azure Active Directory. Tenant, AppName, and ClientId must be set. Other fields can be left as defaults.
4. You will have to restart your Orchard site for the new settings to take effect.
5. Navigate to the Orchard home screen. Clear site cookies, and watch Azure Authentication take over!

## License

This module is licensed under the Apache 2.0 license, a copy is included in the repository.
