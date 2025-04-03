using Fish_Manage.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class JwtService
{
    private readonly string _key;
    private readonly UserManager<ApplicationUser> _userManager;

    public JwtService(IConfiguration config, UserManager<ApplicationUser> userManager)
    {
        _key = config["Jwt:Key"];
        _userManager = userManager;
    }
    public async Task<string> GenerateToken(ApplicationUser user)
    {
        var userRoles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
        };
        claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiration = DateTime.UtcNow.AddDays(7); // Token valid for 7 days

        var token = new JwtSecurityToken(
            issuer: "https://localhost:7229",
            audience: "https://localhost:7229",
            claims: claims,
            expires: expiration,
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
