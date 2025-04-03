using Fish_Manage.Models;
using Fish_Manage.Models.DTO.User;
using Microsoft.AspNetCore.Identity;

namespace Fish_Manage.Repository.IRepository
{
    public interface IUserRepository : IRepository<ApplicationUser>
    {
        bool IsUniqueUser(string username);
        Task<bool> AddToRoleAsync(string userId, string roleName);
        Task<ApplicationUser> GetUserByEmail(string email);
        Task<ApplicationUser> GetUserByUsername(string userName);
        Task<IdentityResult> CreateUserAsync(ApplicationUser user);
        Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO);
        Task<ApplicationUser> Register(RegisterationRequestDTO registerationRequestDTO, string img);
        Task<bool> ValidatePassword(ApplicationUser user, string password);
        Task<ApplicationUser> UpdateAsync(ApplicationUser user);
    }
}
