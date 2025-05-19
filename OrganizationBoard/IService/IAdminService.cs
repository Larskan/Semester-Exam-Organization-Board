using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrganizationBoard.DTO;
using EFrameWork.Model;

namespace OrganizationBoard.IService
{
    public interface IAdminService
    {
        // User Management
        Task<OperationResponse<User>> CreateUser(User user, int requestingAdminId);
        Task<OperationResponse<User>> GetUser(int userId, int requestingAdminId);
        Task<OperationResponse<User>> UpdateUser(User user, int requestingAdminId);
        Task<OperationResponse<bool>> DeleteUser(int userId, int requestingAdminId);
        Task<OperationResponse<List<User>>> GetAllUsers(int requestingAdminId);

        //Organization Management
        Task<OperationResponse<Organization>> UpdateOrganization(Organization organization, int requestingAdminId);
    }
}