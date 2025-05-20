using Microsoft.AspNetCore.Mvc;
using OrganizationBoard.IService;
using EFrameWork.Model;
using Microsoft.AspNetCore.Authorization;
using OrganizationBoard.DTO;

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

        #region Team Management
        [HttpGet("GetTeamMembers/{teamId}")]
        [Authorize(Roles = "Team Leader, Team Member, Admin")]
        public async Task<IActionResult> GetTeamMembers(int teamId, int userId)
        {
            var response = await _teamService.GetTeamMembers(teamId, userId);
            if (response.IsSuccess)
                return Ok(response.Data);
            return NotFound(response.Message);
        }

        [HttpPost("CreateTeam")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTeam([FromBody] TeamDto team, int userId)
        {
            var response = await _teamService.CreateTeam(team, userId);
            if (response.IsSuccess)
                return CreatedAtAction(nameof(GetTeamMembers), new { teamId = response.Data.TeamID }, response.Data);
            return BadRequest(response.Message);
        }

        [HttpPut("UpdateTeam")]
        [Authorize(Roles = "Team Leader")]
        public async Task<IActionResult> UpdateTeamName(TeamDto team, int userId)
        {
            var response = await _teamService.UpdateTeamName(team, userId);
            if (response.IsSuccess)
                return Ok(response.Data);
            return NotFound(response.Message);
        }

        [HttpDelete("DeleteTeam/{teamId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTeam(int teamId, int userId)
        {
            var response = await _teamService.DeleteTeam(teamId, userId);
            if (response.IsSuccess)
                return Ok(response.Message);
            return NotFound(response.Message);
        }

        [HttpPost("AssignUserToTeam")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignUserToTeam(int teamId, int userIdToAssign, int requestingLeaderId)
        {
            var response = await _teamService.AssignUserToTeam(teamId, userIdToAssign, requestingLeaderId);
            if (response.IsSuccess)
                return Ok(response.Message);
            return NotFound(response.Message);
        }

        [HttpDelete("RemoveUserFromTeam")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveUserFromTeam(int teamId, int userIdToRemove, int requestingLeaderId)
        {
            var response = await _teamService.RemoveUserFromTeam(teamId, userIdToRemove, requestingLeaderId);
            if (response.IsSuccess)
                return Ok(response.Message);
            return NotFound(response.Message);
        }
        #endregion Team Management
    }
}