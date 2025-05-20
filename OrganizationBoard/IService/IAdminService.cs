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
        Task<OperationResponse<UserCreateDto>> CreateUser(UserCreateDto user, int requestingAdminId);
        Task<OperationResponse<UserDto>> GetUser(int userId, int requestingAdminId);
        Task<OperationResponse<UserCreateDto>> UpdateUser(UserCreateDto user, int requestingAdminId);
        Task<OperationResponse<bool>> DeleteUser(int userId, int requestingAdminId);
        Task<OperationResponse<List<UserDto>>> GetAllUsers(int requestingAdminId);

        //Organization Management
        Task<OperationResponse<Organization>> UpdateOrganization(Organization organization, int requestingAdminId);
    }
}