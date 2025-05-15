using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFrameWork.Model;
using OrganizationBoard.DTO;

namespace OrganizationBoard.IService
{
    public interface ILoginService
    {
        System.Threading.Tasks.Task<User> UserCheck(LoginDto dto);
        System.Threading.Tasks.Task CreateAccountAndOrg(AccountAndOrgDto dto);
    }
}