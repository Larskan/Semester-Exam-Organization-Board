using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrganizationBoard.IService;
using OrganizationBoard.DTO;

namespace OrganizationBoard.Controller
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BoardController : ControllerBase
    {
        // Assuming you have a service to handle board operations
        private readonly IBoardService _boardService;
        public BoardController(IBoardService boardService)
        {
            _boardService = boardService;
        }

        #region Helper Methods
        private int GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        private int GetTeamFromClaims()
        {
            var teamIdClaim = User.FindFirst("TeamID");
            return teamIdClaim != null ? int.Parse(teamIdClaim.Value) : 0;
        }
        #endregion

        #region Board Management

        [HttpGet("GetBoard/{boardId}")]
        [Authorize(Roles = "Team Leader, Team Member")]
        public async Task<IActionResult> GetBoard(int boardId)
        {
            var userId = GetUserIdFromClaims();
            if (userId <= 0)
            {
                return BadRequest("Invalid user ID.");
            }

            var result = await _boardService.GetBoard(boardId, userId);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }
            return NotFound(result.Message);
        }

        [HttpGet("GetTeamBoards/{teamId}")]
        [Authorize(Roles = "Team Leader, Team Member")]
        public async Task<IActionResult> GetTeamBoards()
        {
            var userId = GetUserIdFromClaims();
            var teamId = GetTeamFromClaims();
            if (userId == 0 || teamId == 0)
            {
                return BadRequest("Invalid user ID or team ID.");
            }

            var result = await _boardService.GetTeamBoards(teamId, userId);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }
            return BadRequest(result.Message);
        }

        // [Authorize(Roles = "Team Leader")]
        [HttpPost("CreateBoard")]
        [Authorize(Roles = "Team Leader")]
        public async Task<IActionResult> CreateBoard([FromBody] BoardDto board)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0)
            {
                return BadRequest("User ID not found");
            }

            var result = await _boardService.CreateBoard(board, userId);
            if (result.IsSuccess)
            {
                return CreatedAtAction(nameof(GetBoard), new { boardId = result.Data.BoardID }, result.Data);
            }
            return BadRequest(result.Message);
        }

        // [Authorize(Roles = "Team Leader")]
        [HttpPut("UpdateBoard")]
        [Authorize(Roles = "Team Leader")]
        public async Task<IActionResult> UpdateBoard([FromBody] BoardReadDto board)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0)
            {
                return BadRequest("User ID not found");
            }

            // Assuming the board object contains the BoardID
            if (board.BoardID <= 0 || board == null)
            {
                return BadRequest("Invalid Board ID or Board object.");
            }

            var result = await _boardService.UpdateBoard(board, userId);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }
            return BadRequest(result.Message);
        }

        // [Authorize(Roles = "Team Leader")]
        [HttpDelete("DeleteBoard/{boardId}")]
        [Authorize(Roles = "Team Leader")]
        public async Task<IActionResult> DeleteBoard(int boardId)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0)
            {
                return BadRequest("User ID not found");
            }

            var result = await _boardService.DeleteBoard(boardId, userId);
            if (result.IsSuccess)
            {
                return NoContent();
            }
            return BadRequest(result.Message);
        }

        [HttpGet("GetBoardTasks/{boardId}")]
        [Authorize(Roles = "Team Leader, Team Member")]
        public async Task<IActionResult> GetBoardTasks(int boardId)
        {
            var userId = GetUserIdFromClaims();
            if (userId <= 0)
            {
                return BadRequest("Invalid user ID.");
            }

            var result = await _boardService.GetBoardTasks(boardId, userId);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }
            return NotFound(result.Message);
        }

        #endregion

        #region Task Management

        // [Authorize(Roles = "Team Leader")]
        [HttpPost("CreateTask")]
        [Authorize(Roles = "Team Leader")]
        public async Task<IActionResult> CreateTask([FromBody] TaskDto task, int boardId)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0)
            {
                return BadRequest("User ID not found");
            }

            // Assuming the task object contains the BoardID
            task.BoardID = boardId;
            var result = await _boardService.CreateTask(task, boardId, userId);
            if (result.IsSuccess)
            {
                return CreatedAtAction(nameof(GetTask), new { taskId = result.Data.TaskID }, result.Data);
            }
            return BadRequest(result.Message);
        }

        [HttpGet("GetTask/{taskId}")]
        [Authorize(Roles = "Team Leader, Team Member")]
        public async Task<IActionResult> GetTask(int taskId)
        {
            var userId = GetUserIdFromClaims();
            if (userId <= 0)
            {
                return BadRequest("Invalid user ID.");
            }

            var result = await _boardService.GetTask(taskId, userId);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }
            return NotFound(result.Message);
        }

        // [Authorize(Roles = "Team Leader")]
        [HttpPut("UpdateTask")]
        [Authorize(Roles = "Team Leader")]
        public async Task<IActionResult> UpdateTask([FromBody] TaskReadDto task)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0)
            {
                return BadRequest("User ID not found");
            }

            var result = await _boardService.UpdateTask(task, userId);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }
            return BadRequest(result.Message);
        }

        // [Authorize(Roles = "Team Leader")]
        [HttpDelete("DeleteTask/{taskId}")]
        [Authorize(Roles = "Team Leader")]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0)
            {
                return BadRequest("User ID not found");
            }

            var result = await _boardService.DeleteTask(taskId, userId);
            if (result.IsSuccess)
            {
                return NoContent();
            }
            return BadRequest(result.Message);
        }

        [HttpPost("AssignTask/{taskId}/{assignedToUserId}")]
        [Authorize(Roles = "Team Leader")]
        public async Task<IActionResult> AssignTask(int taskId, int assignedToUserId)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0)
            {
                return BadRequest("User ID not found");
            }

            var result = await _boardService.AssignTask(taskId, userId, assignedToUserId);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }
            return BadRequest(result.Message);
        }

        [HttpPost("MarkTaskAsComplete/{taskId}")]
        [Authorize(Roles = "Team Leader, Team Member")]
        public async Task<IActionResult> MarkTaskAsComplete(int taskId)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0)
            {
                return BadRequest("User ID not found");
            }

            var result = await _boardService.MarkTaskAsComplete(taskId, userId);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }
            return BadRequest(result.Message);
        }
        [HttpPost("ConfirmTaskCompletion/{taskId}")]
        [Authorize(Roles = "Team Leader")]
        public async Task<IActionResult> ConfirmTaskCompletion(int taskId)
        {
            var userId = GetUserIdFromClaims();
            if (userId == 0)
            {
                return BadRequest("User ID not found");
            }

            var result = await _boardService.ConfirmTaskCompletion(taskId, userId);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }
            return BadRequest(result.Message);
        }

        #endregion
    }
}