using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganizationBoard.IService
{
    public interface IBCryptService
    {
        public string HashPassword(string password);
        public bool VerifyPassword(string enteredPassword, string storedHashedPassword);
    }
}