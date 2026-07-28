using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Dtos;
using UserService.Exceptions;
using UserService.Extensions;
using UserService.Security;
using UserService.Services;

namespace UserService.Controllers
{
    /// <summary>
    /// Приложение №1, User Service section:
    ///   GET /api/users/{id}     — Admin, Owner
    ///   GET /api/users/batch    — internal only (Analytics Service)
    /// </summary>
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserProfileService _profileService;

        public UsersController(IUserProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet("{id:int}")]
        [Authorize] // any authenticated role; ownership is enforced in the service
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserProfileDto>> GetProfile(int id, CancellationToken ct)
        {
            var requestingUserId = User.GetUserId();
            var requestingRole = User.IsInRole("Admin") ? "Admin" : "Other";

            try
            {
                var profile = await _profileService.GetProfileAsync(id, requestingUserId, requestingRole, ct);
                return Ok(profile);
            }
            catch (ForbiddenProfileAccessException)
            {
                return Forbid();
            }
            catch (UserNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Batch name lookup — Analytics Service's only way to turn StudentId /
        /// InstructorId values into readable names (e.g. for
        /// GET /api/analytics/course/{id}/grades). Never called by the frontend.
        /// </summary>
        [HttpGet("batch")]
        [AllowAnonymous]
        [InternalOnly]
        [ProducesResponseType(typeof(List<UserBatchItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<UserBatchItemDto>>> GetBatch(
            [FromQuery] string ids, CancellationToken ct)
        {
            var parsedIds = (ids ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(int.Parse);

            var users = await _profileService.GetBatchAsync(parsedIds, ct);
            return Ok(users);
        }
    }
}
