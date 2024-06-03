using MessageAPI.Models;
using MessageAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MessageAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<User> GetUserAsync(string username, string password)
        {
            return await _userService.GetUserAsync(username, password);
        }

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<User>> GetUserById(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return user;
        }

        
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            try
            {
                var existingUser = await _userService.GetUserAsync(user.Username, user.Password);
                if (existingUser != null)
                {
                    return Conflict("UserAlreadyExists");
                }

                await _userService.CreateUserAsync(user);

                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating user: {ex.Message}");
            }
        }


        [HttpPost("Login")]
        public async Task<ActionResult<User>> Login(User user)
        {
            var userFromDb = await _userService.GetUserAsync(user.Username, user.Password);
            if (userFromDb == null)
            {
                return NotFound("UserNotFound");
            }
            else
            {
                return Ok(userFromDb);
            }
        }

        [HttpGet("Search")]
        public async Task<ActionResult> SearchUsersAsync(string query)
        {
            try
            {   
                var searchResults = await _userService.SearchUsersAsync(query);
                return Ok(searchResults);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{username}")]
        public async Task<ActionResult> DeleteUser(string username)
        {
            try
            {
                var deleted = await _userService.DeleteUserAsync(username);
                if (deleted)
                {
                    return Ok(new { message="UserDeleted"} );
                }
                else
                {
                    return NotFound("UserNotFound");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}