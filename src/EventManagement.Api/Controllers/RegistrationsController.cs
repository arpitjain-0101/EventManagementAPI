 using EventManagement.Api.Contracts;
 using EventManagement.Api.Models;
 using EventManagement.Api.Services;
 using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Api.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}/registrations")]
public class RegistrationsController : ControllerBase
{
    private readonly IRegistrationService _service;
    public RegistrationsController(IRegistrationService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RegistrationUser>>> GetUsers(Guid eventId)
    {
        var users = await _service.GetUsersAsync(eventId);
        return users is null ? NotFound() : Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> Register(Guid eventId, [FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId)) return BadRequest("UserId is required.");
        var result = await _service.RegisterAsync(eventId, request.UserId.Trim(), request.Name, request.Email, DateTimeOffset.UtcNow);
        return result.Success ? Ok() : BadRequest(result.Error);
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> Unregister(Guid eventId, string userId)
    {
        var result = await _service.UnregisterAsync(eventId, userId);
        return result.Success ? NoContent() : BadRequest(result.Error);
    }
}
