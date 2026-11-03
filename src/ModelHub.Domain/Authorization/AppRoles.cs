namespace ModelHub.Domain.Authorization;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Analyst = "Analyst";
    public const string Reviewer = "Reviewer";

    public static bool IsValid(string? role)
    {
        return role is Admin or Analyst or Reviewer;
    }
}
