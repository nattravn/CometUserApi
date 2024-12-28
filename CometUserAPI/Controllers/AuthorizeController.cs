using ClosedXML;
using CometUserAPI.Container;
using CometUserAPI.Entities;
using CometUserAPI.Model;
using CometUserAPI.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CometUserAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizeController : ControllerBase
    {
        private readonly CometUserDBContext _dbContext;
        private readonly JwtSettings _jwtSettings;
        private readonly IRefreshHandler _refreshHandler;

        public AuthorizeController(CometUserDBContext dbContext, IOptions<JwtSettings> options, IRefreshHandler refreshHandler)
        {
            this._dbContext = dbContext;
            this._jwtSettings = options.Value;
            this._refreshHandler = refreshHandler;
        }
        [HttpPost("GenerateToken")]
        public async Task<IActionResult> GenerateToken([FromBody] UserCred userCred)
        {
            var user = await this._dbContext.TblUsers.FirstOrDefaultAsync(item => item.Username == userCred.username && item.Password == userCred.password);
            if (user != null)
            {
                //generate token
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenKey = Encoding.UTF8.GetBytes(this._jwtSettings.securitykey);
                var tokendesc = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, user.Role)
                    }),
                    Expires = DateTime.Now.AddSeconds(3000),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256)
                };
                var token = tokenHandler.CreateToken(tokendesc);
                var finalToken = tokenHandler.WriteToken(token);
                return Ok(new TokenResponse() { Token = finalToken, RefreshToken = await this._refreshHandler.GenerateToken(userCred.username) });
            }
            else
            {
                return Unauthorized();
            }

        }

        [HttpPost("GenerateRefreshToken")]
        public async Task<IActionResult> GenerateToken([FromBody] TokenResponse token)
        {
            var _refreshToken = await this._dbContext.TblRefreshtokens.FirstOrDefaultAsync(item => item.Refreshtoken == token.RefreshToken);
            if (_refreshToken != null)
            {
                //generate token
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenKey = Encoding.UTF8.GetBytes(this._jwtSettings.securitykey);
                SecurityToken securityToken;
                var principle = tokenHandler.ValidateToken(token.Token, new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(tokenKey),
                    ValidateIssuer = false,
                    ValidateAudience = false
                }, out securityToken);

                var _token = securityToken as JwtSecurityToken;
                if (_token != null && _token.Header.Alg.Equals(SecurityAlgorithms.HmacSha256))
                {
                    string username = principle.Identity?.Name;
                    var _existingdata = await this._dbContext.TblRefreshtokens.FirstOrDefaultAsync(item => item.Userid == username 
                    && item.Refreshtoken == token.RefreshToken);
                    if(_existingdata != null)
                    {
                        var _newToken = new JwtSecurityToken(
                            claims: principle.Claims.ToArray(),
                            expires: DateTime.Now.AddSeconds(30),
                            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._jwtSettings.securitykey)),
                            SecurityAlgorithms.HmacSha256)
                            );
                        var _finalToken = tokenHandler.WriteToken(_newToken);
                        return Ok(new TokenResponse() { Token = _finalToken, RefreshToken = await this._refreshHandler.GenerateToken(username) });
                    } else
                    {
                        return Unauthorized();
                    }
                }else
                {
                    return Unauthorized();
                }
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}
