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
using Polly;

namespace OrganizationBoard.Service
{
    public class LoginService : ILoginService
    {
        private readonly OBDbContext _db;
        private readonly IBCryptService _bCryptService;
        private readonly IRsaService _rsaService;

        public LoginService(OBDbContext db, IBCryptService bCryptService, IRsaService rsaService)
        {
            _bCryptService = bCryptService;
            _rsaService = rsaService;
            _db = db;
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

                var decryptPassword = _rsaService.Decrypt(dto.Password);
                bool valid = _bCryptService.VerifyPassword(decryptPassword, user.Password);

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
                var email = dto.Email?.Trim();
                var orgName = dto.OrgName?.Trim();

                // LINQ and EFramework stops SQL injections
                bool emailExists = await _db.UserTables!.AnyAsync(u => u.Email == email);

                if (emailExists)
                {
                    throw new ApplicationException("A user with this email already exists.");
                }

                var adminRole = await _db.RoleTables!.FirstOrDefaultAsync(r => r.RoleName == "Admin");
                if (adminRole == null)
                {
                    throw new Exception("Admin role not found in the database.");
                }

                var Organization = new Organization
                {
                    OrganizationName = orgName
                };

                await _db.OrganizationTables!.AddAsync(Organization);
                await _db.SaveChangesAsync();

                var Password = _bCryptService.HashPassword(dto.Password);
                var user = new User
                {
                    Email = email,
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