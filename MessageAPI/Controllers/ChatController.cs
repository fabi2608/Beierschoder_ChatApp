using MessageAPI.Models;
using MessageAPI.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;

        public ChatController(ChatService chatService)
        {
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
        }

        [HttpGet]
        public async Task<ActionResult<List<Chat>>> GetChats()
        {
            try
            {
                var chats = await _chatService.GetAsync();
                return Ok(chats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("User/{username}")]
        public async Task<ActionResult<List<Chat>>> GetChatsForUser(string username)
        {
            try
            {
                var chats = await _chatService.GetChatsForUser(username);
                return Ok(chats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("History/{username1}/{username2}")]
        public async Task<ActionResult<List<Message>>> GetChatHistory(string username1, string username2)
        {
            try
            {
                var chatHistory = await _chatService.GetChatHistoryAsync(username1, username2);
                return Ok(chatHistory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("Participants/{userId1}/{userId2}")]
        public async Task<ActionResult<Chat>> GetChatWithParticipants(string userId1, string userId2)
        {
            try
            {
                var chat = await _chatService.GetChatWithParticipantsAsync(userId1, userId2);
                if (chat == null)
                {
                    return NotFound();
                }
                return Ok(chat);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateChat(Chat chat)
        {
            try
            {
                await _chatService.CreateAsync(chat);
                return CreatedAtRoute("GetChat", new { id = chat.Id }, chat);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> UpdateChat(string id, Chat chatIn)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return BadRequest("Invalid chat ID format");
            }

            var chat = await _chatService.GetAsync(id);
            if (chat == null)
            {
                return NotFound();
            }

            chatIn.Id = id;

            try
            {
                await _chatService.UpdateChatAsync(id, chatIn);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{chatId:length(24)}/Messages/{messageId:length(24)}")]
        public async Task<IActionResult> DeleteMessage(string chatId, string messageId)
        {
            try
            {
                var result = await _chatService.DeleteMessageAsync(chatId, messageId);
                if (result)
                {
                    return NoContent();
                }
                return NotFound("Message or Chat not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
