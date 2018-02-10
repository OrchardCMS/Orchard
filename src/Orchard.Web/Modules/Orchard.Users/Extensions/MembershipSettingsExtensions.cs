namespace Orchard.Security {
    public static class MembershipSettingsExtensions {
        public static int GetMinimumPasswordLength(this IMembershipSettings membershipSettings) {
            return membershipSettings.EnableCustomPasswordPolicy ? membershipSettings.MinimumPasswordLength : 7;
        }
    }
}