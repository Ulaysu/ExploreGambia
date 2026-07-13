namespace ExploreGambia.API.Tests.Authorization
{
    internal static class TestUsers
    {
        public const string UserRole = "User";
        public const string GuideRole = "Guide";
        public const string AdminRole = "Admin";

        public const string UserId = "authorization-user-id";
        public const string GuideId = "authorization-guide-id";
        public const string AdminId = "authorization-admin-id";

        public const string UserEmail = "traveller@example.test";
        public const string GuideEmail = "guide@example.test";
        public const string AdminEmail = "admin@example.test";

        public static string UserIdFor(string role)
        {
            return role switch
            {
                UserRole => UserId,
                GuideRole => GuideId,
                AdminRole => AdminId,
                _ => "authorization-unknown-id"
            };
        }

        public static string EmailFor(string role)
        {
            return role switch
            {
                UserRole => UserEmail,
                GuideRole => GuideEmail,
                AdminRole => AdminEmail,
                _ => "unknown@example.test"
            };
        }
    }
}
