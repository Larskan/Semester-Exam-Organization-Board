using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrganizationBoard.DTO;
using Microsoft.EntityFrameworkCore;
using EFramework.Data;
using EFrameWork.Model;
using OrganizationBoard.IService;
using Polly;

namespace OrganizationBoard.Service
{
    public class AdminService : IAdminService
    {
        private readonly OBDbContext _context;
        private readonly IBCryptService _bCryptService;
        private readonly IAsyncPolicy _retryPolicy;
        public AdminService(OBDbContext context, IBCryptService bCryptService, IAsyncPolicy retryPolicy)
        {
            _context = context;
            _bCryptService = bCryptService;
            _retryPolicy = retryPolicy;
        }

        private async Task<bool> IsUserAdmin(int userId){
            var user = await _context.UserTables.FirstOrDefaultAsync(u => u.UserID == userId);
            return user != null && user.RoleID == 1;
        }

        #region User Management
        public async Task<OperationResponse<UserCreateDto>> CreateUser(UserCreateDto user, int requestingAdminId)
        {
            if (!await IsUserAdmin(requestingAdminId))
                return new OperationResponse<UserCreateDto>("Access Denied", false, 403);


            try
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    var hashedPassword = _bCryptService.HashPassword(user.Password);
                    bool emailExists = await _context.UserTables!.AnyAsync(u => u.Email == user.Email);

                    if (emailExists)
                        return new OperationResponse<UserCreateDto>("Email already exists", false, 400);

                    var newUser = new User
                    {
                        Email = user.Email,
                        Password = hashedPassword,
                        RoleID = user.RoleID,
                        OrganizationID = user.OrganizationID,
                        TeamID = user.TeamID
                    };
                    _context.UserTables!.Add(newUser);
                    await _context.SaveChangesAsync();

                    return new OperationResponse<UserCreateDto>(user, "User created successfully");
                });
            }
            catch (Exception ex)
            {
                return new OperationResponse<UserCreateDto>(ex.Message, false, 500);
            }
        }

        public async Task<OperationResponse<UserCreateDto>> UpdateUser(UserCreateDto user, int requestingAdminId)
        {
            if (!await IsUserAdmin(requestingAdminId))
                return new OperationResponse<UserCreateDto>("Access Denied", false, 403);

            var hashedPassword = _bCryptService.HashPassword(user.Password);
            try
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    bool emailExists = await _context.UserTables!.AnyAsync(u => u.Email == user.Email);



                    var existingUser = await _context.UserTables!.FindAsync(user.UserID);
                    if (existingUser == null)
                        return new OperationResponse<UserCreateDto>("User not found", false, 404);

                    // Cheks if any user has the email already && current email user has isnt different than the one you are trying to set it to
                    if (emailExists && existingUser.Email == user.Email)
                        return new OperationResponse<UserCreateDto>("Email already exists", false, 400);

                    existingUser.Email = user.Email;
                    existingUser.Password = hashedPassword;
                    existingUser.RoleID = user.RoleID;
                    existingUser.TeamID = user.TeamID;

                    _context.UserTables.Update(existingUser);
                    await _context.SaveChangesAsync();

                    return new OperationResponse<UserCreateDto>(user, "User updated successfully");
                });
            }
            catch (Exception ex)
            {
                return new OperationResponse<UserCreateDto>(ex.Message, false, 500);
            }
        }

        public async Task<OperationResponse<bool>> DeleteUser(int userId, int requestingAdminId)
        {
            if (!await IsUserAdmin(requestingAdminId))
                return new OperationResponse<bool>("Access Denied", false, 403);

            try
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    var user = await _context.UserTables!.FindAsync(userId);
                    if (user == null)
                        return new OperationResponse<bool>("User not found", false, 404);

                    _context.UserTables.Remove(user);
                    await _context.SaveChangesAsync();

                    return new OperationResponse<bool>(true, "User deleted successfully");
                });
            }
            catch (Exception ex)
            {
                return new OperationResponse<bool>(ex.Message, false, 500);
            }
        }

        public async Task<OperationResponse<UserDto>> GetUser(int userId, int requestingAdminId)
        {
            try
            {
                return await _retryPolicy.ExecuteAsync(async () =>
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
                        RoleID = user.RoleID,
                        OrganizationID = user.OrganizationID,
                        TeamID = user.TeamID
                    };

                    return new OperationResponse<UserDto>(result);
                });
            }
            catch (Exception ex)
            {
                return new OperationResponse<UserDto>(ex.Message, false, 500);
            }
        }

        public async Task<OperationResponse<List<UserDto>>> GetAllUsers(int requestingAdminId)
        {
            if (!await IsUserAdmin(requestingAdminId))
                return new OperationResponse<List<UserDto>>("Access Denied", false, 403);

            try
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    var users = await _context.UserTables!.Select(u => new UserDto
                    {
                        UserID = u.UserID,
                        Email = u.Email,
                        RoleID = u.RoleID,
                        OrganizationID = u.OrganizationID,
                        TeamID = u.TeamID
                    }).ToListAsync();

                    return new OperationResponse<List<UserDto>>(users);
                });
            }
            catch (Exception ex)
            {
                return new OperationResponse<List<UserDto>>(ex.Message, false, 500);
            }
        }
        #endregion User Management

        #region Organization Management
        public async Task<OperationResponse<Organization>> UpdateOrganization(Organization organization, int requestingAdminId)
        {
            if (!await IsUserAdmin(requestingAdminId))
                return new OperationResponse<Organization>("Access Denied", false, 403);

            try
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    var existingOrg = await _context.OrganizationTables!.FindAsync(organization.OrganizationID);
                    if (existingOrg == null)
                        return new OperationResponse<Organization>("Organization not found", false, 404);

                    existingOrg.OrganizationName = organization.OrganizationName;

                    _context.OrganizationTables.Update(existingOrg);
                    await _context.SaveChangesAsync();

                    return new OperationResponse<Organization>(organization, "Organization updated successfully");
                });
            }
            catch (Exception ex)
            {
                return new OperationResponse<Organization>(ex.Message, false, 500);
            }
        }
    #endregion Organization Management

        
    }
}