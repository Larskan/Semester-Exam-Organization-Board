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
        public async Task<OperationResponse<User>> CreateUser(User user, int requestingAdminId)
        {
            if (!await IsUserAdmin(requestingAdminId))
                return new OperationResponse<User>("Access Denied", false, 403);

            var newUser = new User
            {
                Email = user.Email,
                Password = user.Password, //Hash it //FIXME
                RoleID = user.RoleID,
                OrganizationID = user.OrganizationID,
                TeamID = user.TeamID
            };
            _context.UserTables!.Add(newUser);
            await _context.SaveChangesAsync();

            user.UserID = newUser.UserID;
            return new OperationResponse<User>(user, "User created successfully");
        }

        public async Task<OperationResponse<User>> GetUser(int userId, int requestingAdminId)
        {
            if (!await IsUserAdmin(requestingAdminId))
                return new OperationResponse<User>("Access Denied", false, 403);

            var user = await _context.UserTables!.FindAsync(userId);
            if (user == null)
                return new OperationResponse<User>("User not found", false, 404);

            var result = new User
            {
                UserID = user.UserID,
                Email = user.Email,
                Password = user.Password,
                RoleID = user.RoleID,
                OrganizationID = user.OrganizationID,
                TeamID = user.TeamID
            };

            return new OperationResponse<User>(result);
        }

        public async Task<OperationResponse<User>> UpdateUser(User user, int requestingAdminId)
        {
            if (!await IsUserAdmin(requestingAdminId))
                return new OperationResponse<User>("Access Denied", false, 403);

            var existingUser = await _context.UserTables!.FindAsync(user.UserID);
            if (existingUser == null)
                return new OperationResponse<User>("User not found", false, 404);

            existingUser.Email = user.Email;
            existingUser.Password = user.Password; //Hash it
            existingUser.RoleID = user.RoleID;
            existingUser.OrganizationID = user.OrganizationID;
            existingUser.TeamID = user.TeamID;

            _context.UserTables.Update(existingUser);
            await _context.SaveChangesAsync();

            return new OperationResponse<User>(user, "User updated successfully");
        }

        public async Task<OperationResponse<bool>> DeleteUser(int userId, int requestingAdminId)
        {
            if (!await IsUserAdmin(requestingAdminId))
                return new OperationResponse<bool>("Access Denied", false, 403);

            var user = await _context.UserTables!.FindAsync(userId);
            if (user == null)
                return new OperationResponse<bool>("User not found", false, 404);

            _context.UserTables.Remove(user);
            await _context.SaveChangesAsync();

            return new OperationResponse<bool>(true, "User deleted successfully");
        }

        public async Task<OperationResponse<List<User>>> GetAllUsers(int requestingAdminId)
        {
            if (!await IsUserAdmin(requestingAdminId))
                return new OperationResponse<List<User>>("Access Denied", false, 403);

            var users = await _context.UserTables!.Select(u => new User
            {
                UserID = u.UserID,
                Email = u.Email,
                Password = u.Password, //Hash it
                RoleID = u.RoleID,
                OrganizationID = u.OrganizationID,
                TeamID = u.TeamID
            }).ToListAsync();

            return new OperationResponse<List<User>>(users);
        }
    #endregion User Management

        
    }
}