using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrganizationBoard.IService;
using EFrameWork.Model;

namespace OrganizationBoard.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpPost("createUser")]
        public async Task<IActionResult> CreateUser([FromBody] User user, int adminID)
        {
            var response = await _adminService.CreateUser(user, adminID);
            if (response.IsSuccess)
                return StatusCode(response.StatusCode, response.Message);
            return Ok(response);
        }

    }
}