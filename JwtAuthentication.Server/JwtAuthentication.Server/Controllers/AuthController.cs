using System.IdentityModel.Tokens.Jwt;
using JwtAuthentication.Server.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using JwtAuthentication.Server.Db;
using JwtAuthentication.Server.Services;

namespace JwtAuthentication.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserContext _userContext;
    private readonly ITokenService _tokenService;

    public AuthController(UserContext userContext, ITokenService tokenService)
    {
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    }

    [HttpPost, Route("login")]
    public IActionResult Login([FromBody] LoginModel? loginModel)
    {
        if (loginModel is null) return BadRequest("Invalid client request");

        var user = _userContext.LoginModels?.FirstOrDefault(u => 
            (u.UserName == loginModel.UserName) && (u.Password == loginModel.Password));
        
        if (user is null) return Unauthorized();

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, loginModel.UserName),
            new(ClaimTypes.Role, "Manager")
        };
        
        var accessToken = _tokenService.GenerateAccessToken(claims);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

        _userContext.SaveChanges();

        return Ok(new AuthenticatedResponse
        {
            Token = accessToken,
            RefreshToken = refreshToken
        });
    }
}