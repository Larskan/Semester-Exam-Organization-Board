using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        #region Helper Methods
        private int GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        #endregion Helper Methods

        #region User Management
        [HttpGet("GetUser/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUser(int userId)
        {
            var adminId = GetUserIdFromClaims();
            if (adminId <= 0)
            {
                return BadRequest("Invalid user ID.");
            }
            var response = await _adminService.GetUser(userId, adminId);
            if (response.IsSuccess)
                return Ok(response.Data);
            return NotFound(response.Message);
        }

        [HttpGet("GetAllUsers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers(int adminId)
        {
            var response = await _adminService.GetAllUsers(adminId);
            if (response.IsSuccess)
                return Ok(response.Data);
            return NotFound(response.Message);
        }

        [HttpPost("CreateUser")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDto user, int adminId)
        {
            var response = await _adminService.CreateUser(user, adminId);
            if (response.IsSuccess)
                return StatusCode(response.StatusCode, response.Message);
            return Ok(response);
        }

        [HttpPut("UpdateUser")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser([FromBody] UserCreateDto user, int adminId)
        {
            var response = await _adminService.UpdateUser(user, adminId);
            if (response.IsSuccess)
                return Ok(response.Data);
            return NotFound(response.Message);
        }

        [HttpDelete("DeleteUser/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int userId, int adminId)
        {
            var response = await _adminService.DeleteUser(userId, adminId);
            if (response.IsSuccess)
                return Ok(response.Message);
            return NotFound(response.Message);
        }
        #endregion User Management

        #region Organization Management
        [HttpPut("UpdateOrganization")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrganization([FromBody] Organization organization, int adminId)
        {
            var response = await _adminService.UpdateOrganization(organization, adminId);
            if (response.IsSuccess)
                return Ok(response.Data);
            return NotFound(response.Message);
        }
        #endregion Organization Management

    }
}