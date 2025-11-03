using Microsoft.AspNetCore.Mvc;
using DrMajokoProjects.API.Services.Interfaces;
using DrMajokoProjects.API.Models.Entities;
using System.Net.Http;
using System.Text.Json;

namespace DrMajokoProjects.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IFirebaseService _firebaseService;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(
            IAuthService authService,
            IFirebaseService firebaseService,
            ILogger<AuthController> logger,
            IConfiguration configuration)
        {
            _authService = authService;
            _firebaseService = firebaseService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var existingUser = await _authService.GetUserByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { error = "User with this email already exists" });
                }

                var user = await _authService.RegisterUserAsync(
                    request.Email,
                    request.Password,
                    request.Name,
                    request.Role,
                    request.PhoneNumber
                );

                return Ok(new
                {
                    message = "Registration successful. Your account is pending approval.",
                    user = new
                    {
                        user.Id,
                        user.Email,
                        user.Name,
                        user.Role,
                        user.Status
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new { error = "Registration failed", details = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                // First verify with Firebase Auth REST API
                var firebaseApiKey = _configuration["Firebase:ApiKey"];
                var loginResult = await VerifyFirebasePassword(request.Email, request.Password, firebaseApiKey);

                if (!loginResult.Success)
                {
                    return Unauthorized(new { error = "Invalid email or password" });
                }

                // Get user from Firestore
                var user = await _authService.LoginUserAsync(request.Email, request.Password);

                return Ok(new
                {
                    message = "Login successful",
                    user = new
                    {
                        user.Id,
                        user.FirebaseUid,
                        user.Email,
                        user.Name,
                        user.Role,
                        user.Status,
                        user.PhoneNumber,
                        user.Rating
                    },
                    idToken = loginResult.IdToken,
                    refreshToken = loginResult.RefreshToken
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { error = "Login failed", details = ex.Message });
            }
        }

        [HttpPost("logout")]
        public ActionResult Logout()
        {
            // Firebase handles token invalidation on client side
            return Ok(new { message = "Logout successful" });
        }

        [HttpGet("users")]
        public async Task<ActionResult<List<User>>> GetAllUsers()
        {
            try
            {
                var users = await _firebaseService.GetCollectionAsync<User>("users");
                return Ok(users.OrderByDescending(u => u.CreatedAt).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, new { error = "Failed to retrieve users" });
            }
        }

        [HttpGet("user/{id}")]
        public async Task<ActionResult<User>> GetUser(string id)
        {
            try
            {
                var user = await _firebaseService.GetDocumentAsync<User>("users", id);
                if (user == null)
                    return NotFound(new { error = "User not found" });

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving user {id}");
                return StatusCode(500, new { error = "Failed to retrieve user" });
            }
        }

        [HttpPut("user/{id}/approve")]
        public async Task<ActionResult> ApproveUser(string id)
        {
            try
            {
                var user = await _firebaseService.GetDocumentAsync<User>("users", id);
                if (user == null)
                    return NotFound(new { error = "User not found" });

                user.Status = "Approved";
                user.UpdatedAt = DateTime.UtcNow;
                await _firebaseService.UpdateDocumentAsync("users", id, user);

                return Ok(new { message = "User approved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error approving user {id}");
                return StatusCode(500, new { error = "Failed to approve user" });
            }
        }

        [HttpPut("user/{id}/deny")]
        public async Task<ActionResult> DenyUser(string id)
        {
            try
            {
                var user = await _firebaseService.GetDocumentAsync<User>("users", id);
                if (user == null)
                    return NotFound(new { error = "User not found" });

                user.Status = "Denied";
                user.UpdatedAt = DateTime.UtcNow;
                await _firebaseService.UpdateDocumentAsync("users", id, user);

                return Ok(new { message = "User denied successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error denying user {id}");
                return StatusCode(500, new { error = "Failed to deny user" });
            }
        }

        [HttpPut("user/{id}")]
        public async Task<ActionResult> UpdateUser(string id, [FromBody] User user)
        {
            try
            {
                user.Id = id;
                user.UpdatedAt = DateTime.UtcNow;
                await _firebaseService.UpdateDocumentAsync("users", id, user);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user {id}");
                return StatusCode(500, new { error = "Failed to update user" });
            }
        }

        // Helper method to verify password with Firebase
        private async Task<FirebaseAuthResult> VerifyFirebasePassword(string email, string password, string apiKey)
        {
            try
            {
                using var httpClient = new HttpClient();
                var requestBody = new
                {
                    email = email,
                    password = password,
                    returnSecureToken = true
                };

                var response = await httpClient.PostAsJsonAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}",
                    requestBody
                );

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<FirebaseAuthResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return new FirebaseAuthResult
                    {
                        Success = true,
                        IdToken = result.IdToken,
                        RefreshToken = result.RefreshToken
                    };
                }

                return new FirebaseAuthResult { Success = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying Firebase password");
                return new FirebaseAuthResult { Success = false };
            }
        }
    }

    // Request/Response models
    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class FirebaseAuthResponse
    {
        public string IdToken { get; set; }
        public string RefreshToken { get; set; }
        public string LocalId { get; set; }
    }

    public class FirebaseAuthResult
    {
        public bool Success { get; set; }
        public string IdToken { get; set; }
        public string RefreshToken { get; set; }
    }
}