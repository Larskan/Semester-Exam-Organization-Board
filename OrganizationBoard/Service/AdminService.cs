using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrganizationBoard.DTO;
using Microsoft.EntityFrameworkCore;
using EFramework.Data;
using EFrameWork.Model;
using OrganizationBoard.IService;

namespace OrganizationBoard.Service
{
    public class AdminService : IAdminService
    {
        private readonly OBDbContext _context;
        public AdminService(OBDbContext context)
        {
            _context = context;
        }
        
        // V01 - TA01 - Elevation: Member has access to Lead/Admin perms
        private async Task<bool> IsUserAdmin(int userId){
            var user = await _context.UserTables.FirstOrDefaultAsync(u => u.UserID == userId);
            return user != null && user.RoleID == 1;
        }

        #region User Management
        // 1 Decsision = 2 Tests
        // Test: Admin as valid user, set to False = 403.
        // Test: Admin as valid user, creating new user
        public async Task<OperationResponse<UserDto>> CreateUser(UserDto user, int requestingAdminId)
        {
            if (!await IsUserAdmin(requestingAdminId))
                return new OperationResponse<UserDto>("Access Denied", false, 403);

            try
            {

                var newUser = new User
                {
                    Email = user.Email,
                    Password = user.Password,
                    RoleID = user.RoleID,
                    OrganizationID = user.OrganizationID,
                    TeamID = user.TeamID
                };
                _context.UserTables!.Add(newUser);
                await _context.SaveChangesAsync();

                user.UserID = newUser.UserID;
                return new OperationResponse<UserDto>(user, "User created successfully");
            }
            catch (Exception ex)
            {
                return new OperationResponse<UserDto>(ex.Message, false, 500);
            }
        }


        // 3 Decisions = 4 Tests
        // Test: Admin as valid user, set to False = 403.
        // Test: existingUser as null = 404
        // Test: existingUser as valid user
        // Test: failing to update user = 500
        public async Task<OperationResponse<UserDto>> UpdateUser(UserDto user, int requestingAdminId)
        {
            if (!await IsUserAdmin(requestingAdminId))
                return new OperationResponse<UserDto>("Access Denied", false, 403);

            try
            {
                var existingUser = await _context.UserTables!.FindAsync(user.UserID);
                if (existingUser == null)
                    return new OperationResponse<UserDto>("User not found", false, 404);

                existingUser.Email = user.Email;
                existingUser.Password = user.Password;
                existingUser.RoleID = user.RoleID;
                existingUser.OrganizationID = user.OrganizationID;
                existingUser.TeamID = user.TeamID;

                _context.UserTables.Update(existingUser);
                await _context.SaveChangesAsync();

                return new OperationResponse<UserDto>(user, "User updated successfully");
            }
            catch (Exception ex)
            {
                return new OperationResponse<UserDto>(ex.Message, false, 500);
            }
        }

        // 3 Decisions = 4 Tests
        // Test: Admin as valid user, set to False = 403.
        // Test: existingUser as null = 404
        // Test: existingUser as valid user, deleting user. 
        // Test: failing to update user = 500
        public async Task<OperationResponse<bool>> DeleteUser(int userId, int requestingAdminId)
        {
            if (!await IsUserAdmin(requestingAdminId))
                return new OperationResponse<bool>("Access Denied", false, 403);

            try
            {
                var user = await _context.UserTables!.FindAsync(userId);
                if (user == null)
                    return new OperationResponse<bool>("User not found", false, 404);

                _context.UserTables.Remove(user);
                await _context.SaveChangesAsync();

                return new OperationResponse<bool>(true, "User deleted successfully");
            }
            catch (Exception ex)
            {
                return new OperationResponse<bool>(ex.Message, false, 500);
            }
        }

        // 3 Decisions = 4 Tests
        // Test: Admin as valid user, set to False = 403.
        // Test: Admin as valid user, getting user
        // Test: user as null = 404
        // Test: failing to get user = 500
        public async Task<OperationResponse<UserDto>> GetUser(int userId, int requestingAdminId)
        {
            try
            {
                var user = await _context.UserTables!.FindAsync(userId);
                if (user == null)
                    return new OperationResponse<UserDto>("User not found", false, 404);

                if (!await IsUserAdmin(requestingAdminId))
                    return new OperationResponse<UserDto>("Access Denied", false, 403);

                var result = new UserDto
                {
                    UserID = user.UserID,
                    Email = user.Email,
                    Password = user.Password,
                    RoleID = user.RoleID,
                    OrganizationID = user.OrganizationID,
                    TeamID = user.TeamID
                };

                return new OperationResponse<UserDto>(result);
            }
            catch (System.Exception ex)
            {
                return new OperationResponse<UserDto>(ex.Message, false, 500);
            }
        }

        // 1 Decision = 2 Tests
        // Test: Admin as valid user, set to False = 403.
        // Test: Admin as valid user, getting all users
        public async Task<OperationResponse<List<UserDto>>> GetAllUsers(int requestingAdminId)
        {
            if (!await IsUserAdmin(requestingAdminId))
                return new OperationResponse<List<UserDto>>("Access Denied", false, 403);

            var users = await _context.UserTables!.Select(u => new UserDto
            {
                UserID = u.UserID,
                Email = u.Email,
                Password = u.Password,
                RoleID = u.RoleID,
                OrganizationID = u.OrganizationID,
                TeamID = u.TeamID
            }).ToListAsync();

            return new OperationResponse<List<UserDto>>(users);
        }
        #endregion User Management

        #region Organization Management
        // 3 Decisions = 4 Tests
        // Test: Admin as valid user, set to False = 403.
        // Test: existingOrg as null = 404
        // Test: existingOrg as valid org
        // Test: failing to update org = 500
        public async Task<OperationResponse<Organization>> UpdateOrganization(Organization organization, int requestingAdminId)
        {
            if (!await IsUserAdmin(requestingAdminId))
                return new OperationResponse<Organization>("Access Denied", false, 403);

            try
            {

                var existingOrg = await _context.OrganizationTables!.FindAsync(organization.OrganizationID);
                if (existingOrg == null)
                    return new OperationResponse<Organization>("Organization not found", false, 404);

                existingOrg.OrganizationName = organization.OrganizationName;

                _context.OrganizationTables.Update(existingOrg);
                await _context.SaveChangesAsync();

                return new OperationResponse<Organization>(organization, "Organization updated successfully");
            }
            catch (Exception ex)
            {
                return new OperationResponse<Organization>(ex.Message, false, 500);
            }
        }
    #endregion Organization Management

        
    }
}