namespace NovayaGlava_Desktop_Backend.Identities
{
    public class IdentityData
    {
        // Название claim, по которому и будет определяться роль. В данном случае - true (admin), false (default user)
        public const string AdminUserClaimName = "admin";

        // Название Policy
        public const string AdminUserPolicyName = "Admin";
    }
}
