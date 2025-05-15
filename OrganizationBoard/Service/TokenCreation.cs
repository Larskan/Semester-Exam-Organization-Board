using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EFrameWork.Model;
using Microsoft.IdentityModel.Tokens;
using OrganizationBoard.IService;

namespace OrganizationBoard.Service
{
    public class TokenCreation : ITokenCreation
    {
        private readonly IConfiguration _config;

        public TokenCreation(IConfiguration config)
        {
            _config = config;
        }
        public JwtSecurityToken CreateToken(User user)
        {
            // Create JWT claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "Team Member")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return token;
        }
    }
}