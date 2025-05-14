using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrganizationBoard.DTO;
using EFrameWork.Model;

namespace OrganizationBoard.IService
{
    public interface ITeamService
    {
        // Team Management
        Task<OperationResponse<Team>> CreateTeam(Team team, int userId); //Team leader
        Task<OperationResponse<Team>> UpdateTeam(Team team, int userId); //Team leader
        Task<OperationResponse<bool>> DeleteTeam(int teamId, int userId);  //Team leader
        Task<OperationResponse<List<User>>> GetTeamMembers(int teamId, int userId); //All in team can see
        Task<OperationResponse<bool>> AssignUserToTeam(int teamId, int userIdToAssign, int requestingLeaderId); // Team Leader
        Task<OperationResponse<bool>> RemoveUserFromTeam(int teamId, int userIdToRemove, int requestingLeaderId); // Team Leader

    }
}