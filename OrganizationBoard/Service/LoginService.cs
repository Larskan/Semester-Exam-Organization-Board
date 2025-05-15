using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFramework.Data;
using EFrameWork.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Model;
using OrganizationBoard.DTO;
using OrganizationBoard.IService;

namespace OrganizationBoard.Service
{
    public class LoginService : ILoginService
    {
        private readonly OBDbContext _db;
        private readonly IBCryptService _bCryptService;

        public LoginService(OBDbContext db, IBCryptService bCryptService)
        {
            _db = db;
            _bCryptService = bCryptService;
        }
        public async Task<User> UserCheck(LoginDto dto)
        {
            try
            {
                var user = await _db.UserTables!
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null)
                {
                    throw new UnauthorizedAccessException();
                }

                // TODO: Decrypt Password

                bool valid = _bCryptService.VerifyPassword(dto.Password, user.Password);

                if (!valid)
                {
                    throw new UnauthorizedAccessException();
                }

                return user;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw new ApplicationException("Something went wrong while logging in.");
            }
        }

        public async System.Threading.Tasks.Task CreateAccountAndOrg(AccountAndOrgDto dto)
        {
            try
            {
                var adminRole = await _db.RoleTables!.FirstOrDefaultAsync(r => r.RoleName == "Admin");
                if (adminRole == null)
                {
                    throw new Exception("Admin role not found in the database.");
                }

                var Organization = new Organization
                {
                    OrganizationName = dto.OrgName
                };

                await _db.OrganizationTables!.AddAsync(Organization);
                await _db.SaveChangesAsync();

                var Password = _bCryptService.HashPassword(dto.Password);
                var user = new User
                {
                    Email = dto.Email,
                    Password = Password,
                    RoleID = adminRole.RoleID,
                    OrganizationID = Organization.OrganizationID
                };

                await _db.UserTables!.AddAsync(user);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw new ApplicationException("Something went wrong while logging in.");
            }
        }
    }
}