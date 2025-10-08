namespace TransitHub.Models.DTOs
{
    public class UserRolesResponse
    {
        public string Email { get; set; } = string.Empty;
        public List<string> AvailableRoles { get; set; } = new();
        public string DefaultRole { get; set; } = "User";
        public bool RequiresRoleSelection { get; set; }
    }
}