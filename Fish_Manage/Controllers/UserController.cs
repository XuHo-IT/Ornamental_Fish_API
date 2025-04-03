using AutoMapper;
using Fish_Manage.Models;
using Fish_Manage.Models.DTO;
using Fish_Manage.Models.DTO.FaceBook;
using Fish_Manage.Models.DTO.Password;
using Fish_Manage.Models.DTO.User;
using Fish_Manage.Repository;
using Fish_Manage.Repository.DTO;
using Fish_Manage.Repository.IRepository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace Fish_Manage.Controllers
{

    [ApiController]
    public class UserController : Controller
    {
        private readonly FishManageContext _context;
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;
        private readonly JwtService _jwtService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly EmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly APIResponse _response = new();
        private readonly IHttpClientFactory _httpClientFactory;

        public UserController(FishManageContext context, IUserRepository userRepo, IMapper mapper, JwtService jwtService, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, EmailSender emailSender, IConfiguration configuration, APIResponse response, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _userRepo = userRepo;
            _mapper = mapper;
            _jwtService = jwtService;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _configuration = configuration;
            _response = response;
            _httpClientFactory = httpClientFactory;
        }
        [HttpGet("api/User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIResponse>> GetUsers()
        {
            var userList = await _userRepo.GetAllAsync();

            var userDTOList = new List<UserDTO>();

            foreach (var user in userList)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDTOList.Add(new UserDTO
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Name = user.Name,
                    Email = user.Email,
                    Role = roles.FirstOrDefault() ?? "customer",
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    ImageUrl = user.ImageUrl,
                });
            }

            _response.Result = userDTOList;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }

        [HttpPost("api/User/login")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            var user = await _userRepo.GetUserByUsername(model.UserName);
            if (user == null || !await _userRepo.ValidatePassword(user, model.Password))
            {
                return Unauthorized(new APIResponse
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    IsSuccess = false,
                    ErrorMessages = new List<string> { "Invalid username or password" }
                });
            }

            var token = await _jwtService.GenerateToken(user);
            var loginResponse = new LoginResponseDTO
            {
                Token = token,
                User = new UserDTO
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Name = user.Name,
                    Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "customer",
                    PhoneNumber = user.PhoneNumber,
                    Gender = user.Gender,
                    DateOfBirth = user.DateOfBirth,
                    Address = user.Address,
                }
            };


            return Ok(new APIResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = loginResponse
            });
        }
        [HttpPost("api/User/loginWithOkta")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LoginWithOkta([FromBody] OktaLoginDTO model)
        {
            var handler = new JwtSecurityTokenHandler();
            SecurityToken validatedToken;

            try
            {
                var authority = _configuration["Okta:Authority"];
                var clientId = _configuration["Okta:ClientId"];
                var clientSecret = _configuration["Okta:ClientSecret"];
                var tokenEndpoint = $"{authority}/v1/token";

                // Exchange Authorization Code for Access and ID Token
                using (var client = _httpClientFactory.CreateClient())
                {
                    var tokenRequest = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "code", model.AuthorizationCode },
                { "redirect_uri", model.RedirectUri }
            };

                    var response = await client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(tokenRequest));
                    if (!response.IsSuccessStatusCode)
                    {
                        return Unauthorized(new APIResponse
                        {
                            StatusCode = HttpStatusCode.Unauthorized,
                            IsSuccess = false,
                            ErrorMessages = new List<string> { "Failed to exchange authorization code for tokens." }
                        });
                    }

                    var tokenResult = await response.Content.ReadFromJsonAsync<OktaTokenResponse>();
                    model.Code = tokenResult.IdToken;
                }

                var validationParameters = new TokenValidationParameters
                {
                    ValidIssuer = authority,
                    ValidAudience = clientId,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    IssuerSigningKeys = await GetSigningKeysFromOkta(authority)
                };

                var principal = handler.ValidateToken(model.Code, validationParameters, out validatedToken);
                if (principal == null)
                {
                    return Unauthorized(new APIResponse
                    {
                        StatusCode = HttpStatusCode.Unauthorized,
                        IsSuccess = false,
                        ErrorMessages = new List<string> { "Invalid Okta token" }
                    });
                }

                var userEmail = principal.FindFirst(ClaimTypes.Email)?.Value;
                var userName = principal.FindFirst(ClaimTypes.Name)?.Value;
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Check if user exists in the database
                var user = await _userRepo.GetUserByEmail(userEmail);
                if (user == null)
                {
                    // Register new user in the backend
                    user = new ApplicationUser
                    {
                        UserName = userName,
                        Email = userEmail,
                        Name = userName,
                        PhoneNumber = "",
                        Gender = false,
                        DateOfBirth = new DateTime(2000, 1, 1),
                        Address = "Unknown",
                        ImageUrl = ""
                    };

                    var result = await _userManager.CreateAsync(user);

                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "customer");
                    }
                    else
                    {
                        return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
                    }
                    if (!result.Succeeded)
                    {
                        return BadRequest(new APIResponse
                        {
                            StatusCode = HttpStatusCode.BadRequest,
                            IsSuccess = false,
                            ErrorMessages = result.Errors.Select(e => e.Description).ToList()
                        });
                    }

                    // Assign default role
                    await _userManager.AddToRoleAsync(user, "customer");
                }

                // Generate JWT token for the user
                var token = await _jwtService.GenerateToken(user);
                var loginResponse = new LoginResponseDTO
                {
                    Token = token,
                    User = new UserDTO
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        Name = user.Name,
                        Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "customer",
                        PhoneNumber = user.PhoneNumber,
                        Gender = user.Gender,
                        DateOfBirth = user.DateOfBirth,
                        Address = user.Address,
                    }
                };

                return Ok(new APIResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = true,
                    Result = loginResponse
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new APIResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    ErrorMessages = new List<string> { ex.Message }
                });
            }
        }

        private static async Task<IEnumerable<SecurityKey>> GetSigningKeysFromOkta(string authority)
        {
            using (var httpClient = new HttpClient())
            {
                var jwksUrl = $"{authority}/v1/keys";
                var jwksResponse = await httpClient.GetStringAsync(jwksUrl);
                var jwks = new JsonWebKeySet(jwksResponse);
                return jwks.Keys;
            }
        }

        [HttpPost("api/User/register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register(
         [FromForm] RegisterationRequestDTO model,
         IFormFile? imageFile,
         [FromServices] CloudinaryService cloudinaryService)
        {
            if (!_userRepo.IsUniqueUser(model.UserName))
            {
                return BadRequest(new APIResponse
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = new List<string> { "Username already exists" }
                });
            }

            var user = _mapper.Map<ApplicationUser>(model);

            if (imageFile != null && imageFile.Length > 0)
            {
                user.ImageUrl = await cloudinaryService.UploadImageAsync(imageFile);
            }
            else
            {
                user.ImageUrl = "https://default-image-url.com/default.png";
            }

            var registeredUser = await _userRepo.Register(model, user.ImageUrl);

            if (registeredUser == null)
            {
                return BadRequest(new APIResponse
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = new List<string> { "Registration failed" }
                });
            }


            // ✅ Generate JWT token
            var token = await _jwtService.GenerateToken(registeredUser);

            return Ok(new APIResponse
            {
                StatusCode = HttpStatusCode.Created,
                IsSuccess = true,
                Result = new LoginResponseDTO
                {
                    Token = token,
                    User = new UserDTO
                    {
                        Id = registeredUser.Id,
                        UserName = registeredUser.UserName,
                        Name = registeredUser.Name,
                        Role = model.Role,
                        Gender = registeredUser.Gender,
                        DateOfBirth = registeredUser.DateOfBirth,
                        Address = registeredUser.Address,
                        ImageUrl = user.ImageUrl,
                        Email = registeredUser.Email,
                        PhoneNumber = registeredUser.PhoneNumber
                    }
                }
            });
        }

        [HttpPut("api/User/UserRole/{id}")]
        public async Task<IActionResult> UpdateUserRole(string id, [FromBody] UpdateUserRoleDto model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Remove old roles
            var existingRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, existingRoles);

            // Add new role
            var result = await _userManager.AddToRoleAsync(user, model.Role);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Failed to update role", errors = result.Errors });
            }

            return Ok(new { message = "Role updated successfully" });
        }

        // DTO for role update request
        public class UpdateUserRoleDto
        {
            public string Role { get; set; }
        }
        [HttpPost("api/User/ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Failed to change password", errors = result.Errors });
            }

            return Ok(new { message = "Password changed successfully" });
        }

        [HttpPost("api/User/login-facebook")]
        public async Task<IActionResult> FacebookLogin([FromBody] FacebookLoginRequestDTO request)
        {
            if (string.IsNullOrEmpty(request.AccessToken))
                return BadRequest(new { message = "Invalid Facebook token" });

            using var httpClient = new HttpClient();
            var fbValidationUrl = $"https://graph.facebook.com/me?access_token={request.AccessToken}&fields=id,name,email";
            var response = await httpClient.GetAsync(fbValidationUrl);

            if (!response.IsSuccessStatusCode)
                return Unauthorized(new { message = "Invalid Facebook token" });

            var fbUserData = JsonConvert.DeserializeObject<FacebookUserResponseDTO>(await response.Content.ReadAsStringAsync());
            if (fbUserData == null || string.IsNullOrEmpty(fbUserData.Id))
                return Unauthorized(new { message = "Facebook authentication failed" });

            var existingUser = await _userRepo.GetUserByEmail(fbUserData.Email) ?? new ApplicationUser
            {
                UserName = fbUserData.Email,
                Email = fbUserData.Email,
                Name = fbUserData.Name,
                NormalizedEmail = fbUserData.Email.ToUpper(),
                EmailConfirmed = true,
            };
            var passwordHasher = new PasswordHasher<ApplicationUser>();
            existingUser.PasswordHash = passwordHasher.HashPassword(existingUser, fbUserData.Email);

            if (existingUser.Id == null)
            {
                var result = await _userRepo.CreateUserAsync(existingUser);
                if (!result.Succeeded) return BadRequest(new { message = "User creation failed" });

                await _userRepo.AddToRoleAsync(existingUser.Id, "customer");
            }

            var token = await _jwtService.GenerateToken(existingUser);
            return Ok(new { token, userId = existingUser.Id, isAuthenticated = true, isAdmin = false });
        }
        [Authorize(Roles = "admin")]
        [HttpDelete("api/User/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();

            var user = await _userRepo.GetAsync(u => u.Id == id);
            if (user == null) return NotFound();

            await _userRepo.RemoveAsync(user);
            return NoContent();
        }
        [HttpPut("api/User/{id}")]
        public async Task<ActionResult<APIResponse>> UpdateUser(
    string id,
    [FromForm] UserUpdateDTO updateDTO,
    IFormFile imageFile,
    [FromServices] CloudinaryService cloudinaryService)
        {
            if (updateDTO == null || id != updateDTO.Id)
            {
                return BadRequest(new APIResponse
                {
                    IsSuccess = false,
                    ErrorMessages = new List<string> { "Invalid user data" }
                });
            }

            var user = await _userRepo.GetAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound(new APIResponse
                {
                    IsSuccess = false,
                    ErrorMessages = new List<string> { "User not found" }
                });
            }

            user.UserName = updateDTO.UserName;
            user.Name = updateDTO.Name;
            user.Email = updateDTO.Email;
            user.PhoneNumber = updateDTO.PhoneNumber;
            user.Gender = updateDTO.Gender;
            user.Address = updateDTO.Address;
            user.DateOfBirth = updateDTO.DateOfBirth;

            if (!string.IsNullOrEmpty(updateDTO.NewPassword))
            {
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, updateDTO.NewPassword);

                if (!resetResult.Succeeded)
                {
                    return BadRequest(new APIResponse
                    {
                        IsSuccess = false,
                        ErrorMessages = new List<string> { "Password reset failed" }
                    });
                }
            }
            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadedImageUrl = await cloudinaryService.UploadImageAsync(imageFile);
                if (!string.IsNullOrEmpty(uploadedImageUrl))
                {
                    user.ImageUrl = uploadedImageUrl;
                }
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(updateDTO.Role))
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, updateDTO.Role);
            }


            await _userRepo.UpdateAsync(user);
            await _userRepo.SaveAsync();

            return Ok(new APIResponse
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.NoContent,
                ErrorMessages = new List<string> { "User updated successfully" }
            });
        }

        [HttpPost("api/User/forgotPass")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest(new APIResponse
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = new List<string> { "Email not found" }
                });
            }

            // Generate Password Reset Token
            string token = await _userManager.GeneratePasswordResetTokenAsync(user);
            string resetLink = $"https://localhost:5173/Login/ResetPass.html?email={user.Email}&token={WebUtility.UrlEncode(token)}";

            string subject = "Password Reset Request";
            string message = $"Click the link below to reset your password:<br/><a href='{resetLink}'>Reset Password</a>";

            await _emailSender.SendEmailAsync(user.Email, subject, message);

            return Ok(new APIResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = "Password reset email sent successfully."
            });
        }

        [HttpGet("api/User/{id}", Name = "GetUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> GetUser(string id)
        {
            try
            {
                if (id == "")
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var user = await _userRepo.GetAsync(u => u.Id == id);
                if (user == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                var currentRoles = await _userManager.GetRolesAsync(user);
                var userDTO = _mapper.Map<UserDTO>(user);
                userDTO.Role = currentRoles.FirstOrDefault();

                _response.Result = userDTO;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
            }
            return _response;
        }
        [HttpPost("api/User/resetPass")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest(new APIResponse
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = new List<string> { "Invalid email" }
                });
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(new APIResponse
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = result.Errors.Select(e => e.Description).ToList()
                });
            }

            return Ok(new APIResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = "Password has been successfully reset."
            });
        }


        [HttpGet("api/User/loginGoogle")]
        public ActionResult LoginWithGoogle()
        {
            var properties = new AuthenticationProperties()
            {
                RedirectUri = Url.Action("GoogleResponse", "User", null, Request.Scheme, Request.Host.Value),
                AllowRefresh = true,
            };
            return Challenge(properties, "Google");
        }


        [HttpGet("signin-google")]
        public async Task<IActionResult> GoogleResponse()
        {
            var queryString = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Console.WriteLine($"Google Response Query: {queryString}"); // Debugging log
                                                                        // Extract user information from Google login
            var email = queryString.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = queryString.Principal.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { message = "Email not found from Google" });
            }

            // Check if user exists
            var existingUser = await _userRepo.GetUserByEmail(email);
            if (existingUser == null)
            {
                existingUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    Name = name,
                    NormalizedEmail = email.ToUpper(),
                    EmailConfirmed = true
                };

                var passwordHasher = new PasswordHasher<ApplicationUser>();
                existingUser.PasswordHash = passwordHasher.HashPassword(existingUser, email);

                var resultCreate = await _userRepo.CreateUserAsync(existingUser);
                if (!resultCreate.Succeeded)
                    return BadRequest(new { message = "User creation failed" });

                await _userRepo.AddToRoleAsync(existingUser.Id, "customer");
            }
            var token = await _jwtService.GenerateToken(existingUser);

            var frontendUrl = $"https://localhost:5173/?token={token}";
            return Redirect(frontendUrl);
        }

        [HttpPost("api/User/logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { Message = "Logged out successfully" });
        }
    }

}
