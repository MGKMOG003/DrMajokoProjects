using DrMajokoProjects.API.Models.Entities;

namespace DrMajokoProjects.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<User> RegisterUserAsync(string email, string password, string name, string role, string phoneNumber);
        Task<User> LoginUserAsync(string email, string password);
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByFirebaseUidAsync(string firebaseUid);
        Task<bool> VerifyPasswordAsync(string email, string password);
        Task UpdateLastLoginAsync(string userId);
    }
}