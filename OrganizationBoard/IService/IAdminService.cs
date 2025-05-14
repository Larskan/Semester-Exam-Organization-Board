using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrganizationBoard.DTO;
using OrganizationBoard.Model;

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

        // Role Management
        Task<OperationResponse<Role>> CreateRole(Role role, int requestingAdminId);
        Task<OperationResponse<Role>> GetRole(int roleId, int requestingAdminId);
        Task<OperationResponse<Role>> UpdateRole(Role role, int requestingAdminId);
        Task<OperationResponse<bool>> DeleteRole(int roleId, int requestingAdminId);
        Task<OperationResponse<List<Role>>> GetAllRoles(int requestingAdminId);
        Task<OperationResponse<bool>> AssignRoleToUser(int userId, int roleId, int requestingAdminId);

        //Organization Management
        //Task<OperationResponse<Organization>> CreateOrganization(Organization organization, int requestingAdminId);
        Task<OperationResponse<Organization>> GetOrganization(int organizationId, int requestingAdminId);
        Task<OperationResponse<Organization>> UpdateOrganization(Organization organization, int requestingAdminId);
        Task<OperationResponse<bool>> DeleteOrganization(int organizationId, int requestingAdminId); //Discuss
        //Task<OperationResponse<List<Organization>>> GetAllOrganizations(int requestingAdminId);
    }
}