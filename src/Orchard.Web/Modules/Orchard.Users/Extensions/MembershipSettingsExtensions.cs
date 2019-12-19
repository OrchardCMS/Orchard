namespace Orchard.Security {
    public static class MembershipSettingsExtensions {
        public static int GetMinimumPasswordLength(this IMembershipSettings membershipSettings) {
            return membershipSettings.EnableCustomPasswordPolicy ? membershipSettings.MinimumPasswordLength : 7;
        }
        public static bool GetPasswordLowercaseRequirement(this IMembershipSettings membershipSettings) {
            return membershipSettings.EnableCustomPasswordPolicy ? membershipSettings.EnablePasswordLowercaseRequirement : false;
        }
        public static bool GetPasswordUppercaseRequirement(this IMembershipSettings membershipSettings) {
            return membershipSettings.EnableCustomPasswordPolicy ? membershipSettings.EnablePasswordUppercaseRequirement : false;
        }
        public static bool GetPasswordNumberRequirement(this IMembershipSettings membershipSettings) {
            return membershipSettings.EnableCustomPasswordPolicy ? membershipSettings.EnablePasswordNumberRequirement : false;
        }
        public static bool GetPasswordSpecialRequirement(this IMembershipSettings membershipSettings) {
            return membershipSettings.EnableCustomPasswordPolicy ? membershipSettings.EnablePasswordSpecialRequirement : false;
        }
    }
}