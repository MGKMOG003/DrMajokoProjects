using FirebaseAdmin.Auth;
using DrMajokoProjects.API.Models.Entities;
using DrMajokoProjects.API.Services.Interfaces;

namespace DrMajokoProjects.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IFirebaseService _firebaseService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IFirebaseService firebaseService, ILogger<AuthService> logger)
        {
            _firebaseService = firebaseService;
            _logger = logger;
        }

        public async Task<User> RegisterUserAsync(string email, string password, string name, string role, string phoneNumber)
        {
            try
            {
                // Create Firebase Auth user
                var userRecordArgs = new UserRecordArgs
                {
                    Email = email,
                    Password = password,
                    DisplayName = name,
                    EmailVerified = false
                };

                var userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(userRecordArgs);

                // Create user in Firestore
                var user = new User
                {
                    FirebaseUid = userRecord.Uid,
                    Email = email,
                    Name = name,
                    PhoneNumber = phoneNumber,
                    Role = role,
                    Status = role == "Admin" ? "Approved" : "Pending",  // Auto-approve admins
                    Rating = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var userId = await _firebaseService.AddDocumentAsync("users", user);
                user.Id = userId;

                // Set custom claims for role-based access
                var claims = new Dictionary<string, object>
                {
                    { "role", role },
                    { "userId", userId }
                };
                await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(userRecord.Uid, claims);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error registering user {email}");
                throw;
            }
        }

        public async Task<User> LoginUserAsync(string email, string password)
        {
            try
            {
                // Get user by email from Firestore
                var users = await _firebaseService.QueryCollectionAsync<User>("users", "Email", email);
                var user = users.FirstOrDefault();

                if (user == null)
                {
                    throw new Exception("User not found");
                }

                if (user.Status != "Approved")
                {
                    throw new Exception("Account is pending approval or has been denied");
                }

                // Update last login
                await UpdateLastLoginAsync(user.Id);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error logging in user {email}");
                throw;
            }
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            try
            {
                var users = await _firebaseService.QueryCollectionAsync<User>("users", "Email", email);
                return users.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user by email {email}");
                throw;
            }
        }

        public async Task<User> GetUserByFirebaseUidAsync(string firebaseUid)
        {
            try
            {
                var users = await _firebaseService.QueryCollectionAsync<User>("users", "FirebaseUid", firebaseUid);
                return users.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user by Firebase UID {firebaseUid}");
                throw;
            }
        }

        public async Task<bool> VerifyPasswordAsync(string email, string password)
        {
            // Note: Firebase Admin SDK doesn't support password verification
            // This is typically done on the client side using Firebase Auth JS SDK
            // For API-only verification, you'd need to use Firebase REST API
            return true; // Placeholder
        }

        public async Task UpdateLastLoginAsync(string userId)
        {
            try
            {
                var user = await _firebaseService.GetDocumentAsync<User>("users", userId);
                if (user != null)
                {
                    user.LastLoginAt = DateTime.UtcNow;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _firebaseService.UpdateDocumentAsync("users", userId, user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating last login for user {userId}");
            }
        }
    }
}