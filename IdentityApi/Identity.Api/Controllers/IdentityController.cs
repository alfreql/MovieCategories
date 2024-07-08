using Identity.Api.Dto;
using Identity.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

/*
 * No using default ASP.NET Core Identity, in order to not generate stuff using EF.
 */

[ApiController]
public class IdentityController : ControllerBase
{
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IApplicationUserService _userService;

    public IdentityController(ITokenGenerator tokenGenerator, IApplicationUserService userService)
    {
        _tokenGenerator = tokenGenerator ?? throw new ArgumentNullException(nameof(tokenGenerator));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    [HttpPost("Users")]
    public async Task<ActionResult<int>> CreateUser(CreateUserRequest userRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        return await _userService.CreateAsync(userRequest.Email, userRequest.Password);
    }

    [HttpPost("token")]
    public async Task<ActionResult> CreateToken(CreateTokenRequest createTokenRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var token = await _tokenGenerator.GenerateTokenAsync(createTokenRequest.Email, createTokenRequest.Password);
        return Ok(new
        {
            Token = token.token,
            ExpireTime = token.expire
        });
    }
}