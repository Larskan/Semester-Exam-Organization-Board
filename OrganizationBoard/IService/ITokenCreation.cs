using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using EFrameWork.Model;

namespace OrganizationBoard.IService
{
    public interface ITokenCreation
    {
        JwtSecurityToken CreateToken(User user);
    }
}