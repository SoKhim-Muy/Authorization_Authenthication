using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MyCodeFirstProject.Models;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class AppUserApiController : ControllerBase
{
    private readonly IAppUserRepository _userRepo;
    public AppUserApiController(IAppUserRepository userRepo) => _userRepo = userRepo;

    // GET: api/AppUserApi
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppUser>>> GetAll() 
        => Ok(await _userRepo.GetAllAsync());

    // GET: api/AppUserApi/5
    [HttpGet("{id}")]
    public async Task<ActionResult<AppUser>> GetById(int id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        return user == null ? NotFound() : Ok(user);
    }

    // POST: api/AppUserApi
    [HttpPost]
    [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> Create([FromBody] AppUser user)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _userRepo.InsertAsync(user);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    // PUT: api/AppUserApi/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Update(int id, [FromBody] AppUser user)
    {
        if (id != user.Id || !ModelState.IsValid) return BadRequest();
        await _userRepo.UpdateAsync(user);
        return NoContent();
    }

    // DELETE: api/AppUserApi/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Delete(int id)
    {
        if (await _userRepo.GetByIdAsync(id) == null) return NotFound();
        await _userRepo.DeleteAsync(id);
        return NoContent();
    }
}