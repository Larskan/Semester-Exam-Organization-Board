using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrganizationBoard.IService;

namespace OrganizationBoard.Controller
{
    // [Authorize]
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

        #region Board Management

        [HttpGet("GetBoard/{boardId}")]
        public async Task<IActionResult> GetBoard(int boardId, int userId)
        {
            var result = await _boardService.GetBoard(boardId, userId);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }
            return NotFound(result.Message);
        }

        [HttpGet("GetTeamBoards/{teamId}")]
        public async Task<IActionResult> GetTeamBoards(int teamId, int userId)
        {
            var result = await _boardService.GetTeamBoards(teamId, userId);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }
            return BadRequest(result.Message);
        }

        // [Authorize(Roles = "Team Leader")]
        [HttpPost("CreateBoard")]
        public async Task<IActionResult> CreateBoard([FromBody] EFrameWork.Model.Board board, int userId)
        {
            var result = await _boardService.CreateBoard(board, userId);
            if (result.IsSuccess)
            {
                return CreatedAtAction(nameof(GetBoard), new { boardId = result.Data.BoardID }, result.Data);
            }
            return BadRequest(result.Message);
        }

        // [Authorize(Roles = "Team Leader")]
        [HttpPut("UpdateBoard")]
        public async Task<IActionResult> UpdateBoard([FromBody] EFrameWork.Model.Board board, int userId)
        {
            var result = await _boardService.UpdateBoard(board, userId);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }
            return BadRequest(result.Message);
        }

        // [Authorize(Roles = "Team Leader")]
        [HttpDelete("DeleteBoard/{boardId}")]
        public async Task<IActionResult> DeleteBoard(int boardId, int userId)
        {
            var result = await _boardService.DeleteBoard(boardId, userId);
            if (result.IsSuccess)
            {
                return NoContent();
            }
            return BadRequest(result.Message);
        }

        #endregion
    }
}