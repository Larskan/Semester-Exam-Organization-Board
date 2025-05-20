using Microsoft.AspNetCore.Mvc;
using OrganizationBoard.IService;
using EFrameWork.Model;
using Microsoft.AspNetCore.Authorization;
using OrganizationBoard.DTO;
using System.Security.Claims;

namespace OrganizationBoard.Controller
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _teamService;
        public TeamController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        #region Helper Methods
        private int GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        private int GetTeamIdFromClaims()
        {
            var teamIdClaim = User.FindFirst("TeamID");
            return teamIdClaim != null ? int.Parse(teamIdClaim.Value) : 0;
        }

        #endregion Helper Methods

        #region Team Management
        [HttpGet("GetTeamMembers/{teamId}")]
        [Authorize(Roles = "Team Leader, Team Member, Admin")]
        public async Task<IActionResult> GetTeamMembers(int teamId)
        {
            var validId = GetUserIdFromClaims();
            if (validId <= 0)
                return BadRequest("Invalid user ID.");
            
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var isTeamMember = GetTeamIdFromClaims();
            
            // You can only view your own team members if you are a team member or leader. Admin can view all.
            if (userRole != "Admin" && isTeamMember != teamId)
                return Forbid("You are not a member of this team.");

            var response = await _teamService.GetTeamMembers(teamId, validId);
            if (response.IsSuccess)
                return Ok(response.Data);
            return NotFound(response.Message);
        }

        [HttpPost("CreateTeam")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTeam([FromBody] TeamDto team)
        {
            var adminId = GetUserIdFromClaims();
            if (adminId <= 0)
                return BadRequest("Invalid user ID.");

            var response = await _teamService.CreateTeam(team, adminId);
            if (response.IsSuccess)
                return CreatedAtAction(nameof(GetTeamMembers), new { teamId = response.Data.TeamID }, response.Data);
            return BadRequest(response.Message);
        }

        [HttpPut("UpdateTeam")]
        [Authorize(Roles = "Team Leader")]
        public async Task<IActionResult> UpdateTeamName(TeamDto team)
        {
            var leaderId = GetUserIdFromClaims();
            if (leaderId <= 0)
                return BadRequest("Invalid user ID.");

            var response = await _teamService.UpdateTeamName(team, leaderId);
            if (response.IsSuccess)
                return Ok(response.Data);
            return NotFound(response.Message);
        }

        [HttpDelete("DeleteTeam/{teamId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTeam(int teamId)
        {
            var adminId = GetUserIdFromClaims();
            if (adminId <= 0)
                return BadRequest("Invalid user ID.");

            var response = await _teamService.DeleteTeam(teamId, adminId);
            if (response.IsSuccess)
                return Ok(response.Message);
            return NotFound(response.Message);
        }

        [HttpPost("AssignUserToTeam")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignUserToTeam(int teamId, int userIdToAssign)
        {
            var adminId = GetUserIdFromClaims();
            if (adminId <= 0)
                return BadRequest("Invalid user ID.");

            var response = await _teamService.AssignUserToTeam(teamId, userIdToAssign, adminId);
            if (response.IsSuccess)
                return Ok(response.Message);
            return NotFound(response.Message);
        }

        [HttpDelete("RemoveUserFromTeam")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveUserFromTeam(int teamId, int userIdToRemove)
        {
            var adminId = GetUserIdFromClaims();
            if (adminId <= 0)
                return BadRequest("Invalid user ID.");

            var response = await _teamService.RemoveUserFromTeam(teamId, userIdToRemove, adminId);
            if (response.IsSuccess)
                return Ok(response.Message);
            return NotFound(response.Message);
        }
        #endregion Team Management
    }
}