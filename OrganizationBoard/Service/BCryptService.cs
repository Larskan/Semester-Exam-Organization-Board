using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;
using OrganizationBoard.IService;


namespace OrganizationBoard.Service
{
    public class BCryptService : IBCryptService
    {
        public string HashPassword(string password)
        {
            // The GenerateSalt method is optional; you can just call HashPassword directly
            string salt = BCrypt.Net.BCrypt.GenerateSalt();
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);
            return hashedPassword; // Store this in your database
        }
        public bool VerifyPassword(string enteredPassword, string storedHashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHashedPassword);
        }
    }
}