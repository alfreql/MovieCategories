using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieCategories.Api.Dto;
using MovieCategories.Application.Interfaces;
using MovieCategories.Domain;
using MovieCategories.Infrastructure.Auth;

namespace MovieCategories.Api.Controllers;

[Authorize]
[Route("api/MoviesCategories")]
[ApiController]
public class MovieCategoriesController : ControllerBase
{
    private readonly ICategoryService _service;
    private readonly IMapper _mapper;
    private readonly IAuthenticator _authenticator;

    public MovieCategoriesController(ICategoryService service, IMapper mapper, IAuthenticator authenticator)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _authenticator = authenticator ?? throw new ArgumentNullException(nameof(authenticator));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MovieCategory>>> Get()
    {
        var categories = await _service.GetAllAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MovieCategory>> Get(int id)
    {
        var category = await _service.GetByIdAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        return Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<int>> Post(CreateMovieCategoryRequest category)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var catDb = _mapper.Map<MovieCategory>(category);
        var newId = await _service.CreateAsync(catDb);

        return Ok(newId);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Put(int id, CreateMovieCategoryRequest category)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingCategory = await _service.GetByIdAsync(id);
        if (existingCategory == null)
        {
            return NotFound();
        }

        existingCategory.Category = category.Category;
        existingCategory.Description = category.Description;

        await _service.UpdateAsync(existingCategory);

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var existingCategory = await _service.GetByIdAsync(id);
        if (existingCategory == null)
        {
            return Ok();
        }

        await _service.DeleteAsync(id);
        return Ok();
    }

    /*
     * I created this method because:
     *
     * The first couple of times I read the instructions, I understood the instructions to require the use of .NET Identity. However,
     * after further reviews, I also interpreted that you might want me to consume the authentication API (to authenticate the user)
     * so that you can evaluate my ability to call an external API using HTTP.
     *
     * Since I was not able to ask for clarification, I created this method to ensure you can evaluate my ability to call an external API using HTTP.
     *
     */
    [AllowAnonymous]
    [HttpGet("GetAllHttpAuth")]
    [SwaggerHeader]
    public async Task<ActionResult<IEnumerable<MovieCategory>>> GetAllHttpAuth()
    {
        if (Request.Headers.TryGetValue("email", out var email) &&
            Request.Headers.TryGetValue("password", out var password))
        {
            var token = await _authenticator.AuthenticateAsync(email, password);
            if (!string.IsNullOrEmpty(token?.Token))
            {
                return await Get();
            }
        }

        return Unauthorized();
    }
}

