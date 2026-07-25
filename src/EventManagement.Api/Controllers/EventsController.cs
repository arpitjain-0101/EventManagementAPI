 using EventManagement.Api.Contracts;
 using EventManagement.Api.Services;
 using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly IEventService _service;
    public EventsController(IEventService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EventResponse>>> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EventResponse>> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<EventResponse>> Create([FromBody] CreateEventRequest request)
    {
        if (request.MaxCapacity <= 0) return BadRequest("MaxCapacity must be > 0.");
        var created = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EventResponse>> Update(Guid id, [FromBody] UpdateEventRequest request)
    {
        if (request.MaxCapacity <= 0) return BadRequest("MaxCapacity must be > 0.");
        var updated = await _service.UpdateAsync(id, request);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _service.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }
}
