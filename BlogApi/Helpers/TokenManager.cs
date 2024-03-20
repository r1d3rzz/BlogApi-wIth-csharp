using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace BlogApi.Helpers
{
    public class TokenManager
    {
        public static string CreateToken(IConfiguration configuration)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var Sectoken = new JwtSecurityToken(
                  configuration["Jwt:Issuer"],
                  configuration["Jwt:Issuer"],
                  null,
                  expires: DateTime.Now.AddSeconds(20),
                  signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(Sectoken);
        }
    }
}
