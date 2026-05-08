using Microsoft.IdentityModel.Tokens;
using pro_exam.ViewModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace pro_exam.Authorization;

public class Authentication : IAuthentication<UserViewModel>
{
    private readonly IConfiguration _Configuration;//to read the key from appsettings.json
    public Authentication(IConfiguration configuration)
    {
        _Configuration = configuration;
    }
    public string GetJsonWebToken(UserViewModel entity)
    {

        var SecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_Configuration["Jwt:Key"]));//مفتاح التشفير
        var Credentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);//header

        var claims = new[]//payload
        {
                new Claim(JwtRegisteredClaimNames.NameId,entity.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.GivenName,entity.Name),
                new Claim(JwtRegisteredClaimNames.Email,entity.Email),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())//to make the token unique
            };

        var Token = new JwtSecurityToken(//to create the token
            issuer: _Configuration["Jwt:Issuer"],//جهة إصدار التوكن
            audience: _Configuration["Jwt:Audience"],//جهة استقبال التوكن
            claims: claims,
            expires: Convert.ToDateTime(DateTime.Now.AddDays(1)),
            signingCredentials: Credentials
            );

        return new JwtSecurityTokenHandler().WriteToken(Token);
    }

}

public class JwtAuthResponse
{
    public string? nameid { get; set; }
    public string? given_name { get; set; }
    public string? email { get; set; }
    public string? jti { get; set; }
    public string? exp { get; set; }
    public string? iss { get; set; }
    public string? aud { get; set; }

    //public string? Email { get; set; }
    //public string? UserType { get; set; }

}