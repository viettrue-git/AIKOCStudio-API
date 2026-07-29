namespace AiKocStudio.Domain.Constants;

/// <summary>
/// String mirror of <see cref="Enums.UserRole"/> for use with
/// [Authorize(Roles = ...)] attributes, which require string constants.
/// </summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Member = "Member";
}
