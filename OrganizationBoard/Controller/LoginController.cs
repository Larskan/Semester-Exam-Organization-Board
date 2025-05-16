using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EFramework.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OrganizationBoard.DTO;
using OrganizationBoard.IService;
using Polly;
using OrganizationBoard.Service;

namespace OrganizationBoard.Controller;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private readonly ILoginService _loginService;
    private readonly IAsyncPolicy _retryPolicy;
    private readonly ITokenCreation _tokenCreation;
    private readonly IRsaService _rsaService;

    public LoginController(ILoginService loginService, IAsyncPolicy retryPolicy, ITokenCreation tokenCreation, IRsaService rsaService)
    {
        _loginService = loginService;
        _retryPolicy = retryPolicy;
        _tokenCreation = tokenCreation;
        _rsaService = rsaService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginAsync([FromBody] LoginDto dto)
    {
        try
        {
            var user = await _retryPolicy.ExecuteAsync(() => _loginService.UserCheck(dto));
            if (user == null)
                return Unauthorized("Invalid credentials");

            var token = _tokenCreation.CreateToken(user);

            if (token == null)
                return Unauthorized("Invalid token credentials");

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized("Invalid email or password.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Login failed: {ex.Message}");
        }
    }

    [HttpPost("AccountAndOrgCreation")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateAccountAndOrg([FromBody] AccountAndOrgDto dto)
    {
        try
        {
            await _retryPolicy.ExecuteAsync(() => _loginService.CreateAccountAndOrg(dto));

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Login failed: {ex.Message}");
        }
    }

    [HttpPost]
    [Authorize(Roles = "Team Member,Admin")]
    public IActionResult CreateTask()
    {
        return Ok("Task created by Team Member!");
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult GetAdminData()
    {
        return Ok("Only admins see this.");
    }

    [HttpGet("public-key")]
    [AllowAnonymous]
    public IActionResult GetPublicKey()
    {
        var publicKey = _rsaService.GetPublicKey();
        return Ok(new { publicKey });
    }
    
    [HttpGet("EncryptPasswordBummyForWebsideResponsabilety")]
    [AllowAnonymous]
    public IActionResult EncryptPasswordBummyForWebsideResponsabilety(string password, string publicKeyPem)
    {
        var Encrypted = _rsaService.EncryptOutside(password,publicKeyPem);
        return Ok(new { Encrypted });
    }
}