using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMGSupportSystem.Services.DTO;
using PMGSupportSystem.Services;

namespace PMGSuppor.ThangTQ.Microservices.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IServicesProvider _servicesProvider;
        public UserController(IServicesProvider servicesProvider)
        {
            _servicesProvider = servicesProvider;
        }

        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<User>>> GetAllUsersAsync()
        //{
        //    var users = await _servicesProvider.UserService.GetUsersAsync();
        //    return Ok(users);
        //}

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] string idToken)
        {
            var jwt = await _servicesProvider.UserService.LoginAsync(idToken);
            return Ok(jwt);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update-user/{userId}")]
        public async Task<IActionResult> ToggleUserStatus([FromRoute] Guid userId)
        {
            var user = await _servicesProvider.UserService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            user.Status = !(user.Status ?? false);
            await _servicesProvider.UserService.UpdateUserAsync(user);
            return Ok(new { Message = "Update status successfully", UserAfterUpdate = user });
        }

            [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            return Ok(new { Message = "Logout successful", Token = token, RevokedAt = DateTime.Now });
        }

        [Authorize(Roles = "Admin, Examiner")]
        [HttpPost("import-users")]
        public async Task<IActionResult> ImportUsersAsync([FromForm] FileDTO newUsersDTO)
        {
            if (newUsersDTO.DTOFile == null || newUsersDTO.DTOFile.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            using (var stream = newUsersDTO.DTOFile.OpenReadStream())
            {
                var importedUsers = await _servicesProvider.UserService.ImportUsersFromExcelAsync(stream);
                return Ok(new { Message = "Users imported successfully!", Count = importedUsers.Count() });
            }
        }
    }
}
