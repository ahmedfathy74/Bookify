namespace Bookify.Application.Services
{
    public interface IAuthService
    {
        Task<IEnumerable<ApplicationUser>> GetUsersAsync();
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
        Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
        Task<ManageUserResponseDto> AddUserAsync(CreateUserDto dto, string createdById);
        Task<ManageUserResponseDto> UpdateUserAsync(ApplicationUser user, IEnumerable<string> selectedRoles, string updatedById);
        Task<ManageUserResponseDto> ResetPasswordAsync(ApplicationUser user, string password ,string updatedById);
        Task<ApplicationUser?> ToggleUserStatusAsync(string userId, string updatedById);
        Task<ApplicationUser?> UnlockUserAsync(string id);
        Task<bool> AllowUserNameAsync(string? id, string userName);
        Task<bool> AllowEmailAsync(string? id, string email);

        Task<IEnumerable<IdentityRole>> GetRolesAsync();
    }
}
