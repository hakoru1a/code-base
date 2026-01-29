using Contracts.Identity;
using Microsoft.IdentityModel.Tokens;
using Shared.Configurations;
using Shared.DTOs.Identity;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Identity
{
    public class TokenService : ITokenService<TokenResponse, TokenRequest>
    {
        private readonly JwtSettings _jwtSettings;

        public TokenService(JwtSettings jwtSettings)
        {
            this._jwtSettings = jwtSettings;
        }

        public TokenResponse GetToken(TokenRequest request)
        {
            string token = GenerateJwt();
            return new TokenResponse(token);
        }
        private string GenerateEncryptedToken(SigningCredentials signingCredentials)
        {
            var claims = new[]
            {
                new Claim("Role", "Admin")
            };
            var token = new JwtSecurityToken(
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: signingCredentials,
                claims: claims);

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }
        private string GenerateJwt() => GenerateEncryptedToken(GetSigningCredential());

        private SigningCredentials GetSigningCredential()
        {
            byte[] secret = Encoding.UTF8.GetBytes(_jwtSettings.Key ?? throw new InvalidOperationException("JWT Key is not configured"));
            return new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256);
        }

    }
}
