using MessageAPI.Models;
using MessageAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace MessageAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly MessageService _messagesService;

    public MessageController(MessageService messagesService) =>
        _messagesService = messagesService;

    [HttpGet]
    public async Task<List<Message>> Get() =>
        await _messagesService.GetAsync();

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<Message>> Get(string id)
    {
        var message = await _messagesService.GetAsync(id);

        if (message is null)
        {
            return NotFound();
        }

        return message;
    }

    [HttpPost]
    public async Task<IActionResult> Post(Message message)
    {
        try
        {
            message.TimeStamp = DateTime.Now;
            await _messagesService.CreateAsync(message);
            return CreatedAtAction(nameof(Get), new { id = message.Id }, message);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error creating message: {ex.Message}");
        }
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, Message updatedMessage)
    {
        var message = await _messagesService.GetAsync(id);

        if (message is null)
        {
            return NotFound();
        }

        updatedMessage.Id = message.Id;

        await _messagesService.UpdateAsync(id, updatedMessage);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var message = await _messagesService.GetAsync(id);

        if (message is null)
        {
            return NotFound();
        }

        await _messagesService.RemoveAsync(id);

        return NoContent();
    }
}