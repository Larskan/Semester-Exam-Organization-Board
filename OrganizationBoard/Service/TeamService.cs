using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrganizationBoard.DTO;
using Microsoft.EntityFrameworkCore;
using EFramework.Data;
using EFrameWork.Model;
using OrganizationBoard.IService;

namespace OrganizationBoard.Service
{
    public class TeamService : ITeamService
    {
        private readonly OBDbContext _context;
        public TeamService(OBDbContext context)
        {
            _context = context;
        }

        #region Private Methods
        private async Task<bool> IsUserAdmin(int userId){
            var user = await _context.UserTables.FirstOrDefaultAsync(u => u.UserID == userId);
            return user != null && user.RoleID == 1;
        }
        private async Task<bool> IsUserTeamLeader(int userId)
        {
            var user = await _context.UserTables.FirstOrDefaultAsync(u => u.UserID == userId);
            return user != null && user.RoleID == 2;
        }
        private async Task<bool> IsUserTeamMember(int userId, int teamId)
        {
            var team = await _context.TeamTables.FirstOrDefaultAsync(t => t.TeamID == teamId);
            var user = await _context.UserTables.FirstOrDefaultAsync(u => u.UserID == userId);
            return user != null && user.TeamID == team.TeamID && team != null;
        }
        #endregion Private Methods

        public async Task<OperationResponse<TeamDto>> CreateTeam(TeamDto team, int requestingUserId)
        {
            if (!await IsUserAdmin(requestingUserId))
                return new OperationResponse<TeamDto>("Only admins can create teams.", false, 403);

            var user = await _context.UserTables!.FirstOrDefaultAsync(u => u.UserID == requestingUserId);

            try
            {
                var newTeam = new Team
                {
                    TeamName = team.TeamName
                };
                _context.TeamTables.Add(newTeam);
                await _context.SaveChangesAsync();

                team.TeamID = newTeam.TeamID;

                return new OperationResponse<TeamDto>(team, "Team created successfully.");
            }
            catch (Exception ex)
            {
                return new OperationResponse<TeamDto>(ex.Message, false, 500);
            }
        }

        public async Task<OperationResponse<TeamDto>> UpdateTeamName(TeamDto team, int requestingUserId)
        {
            if (!await IsUserTeamLeader(requestingUserId))
                    return new OperationResponse<TeamDto>("Access Denied.", false, 403);
            try
            {
                var existingTeam = _context.TeamTables!.FirstOrDefault(t => t.TeamID == team.TeamID);
                if (existingTeam == null)
                    return new OperationResponse<TeamDto>("Team not found.", false, 404);

                existingTeam.TeamName = team.TeamName;
                await _context.SaveChangesAsync();
                return new OperationResponse<TeamDto>(team, "Team updated successfully.");
            }
            catch (Exception ex)
            {
                return new OperationResponse<TeamDto>(ex.Message, false, 500);
            }
        }

        public async Task<OperationResponse<bool>> DeleteTeam(int teamId, int requestingUserId) 
        {
            if (!await IsUserAdmin(requestingUserId))
                return new OperationResponse<bool>("Only admins can delete teams.", false, 403);

            try
            {
                var team = _context.TeamTables!.FirstOrDefault(t => t.TeamID == teamId);
                if (team == null)
                    return new OperationResponse<bool>("Team not found.", false, 404);

                _context.TeamTables!.Remove(team);
                await _context.SaveChangesAsync();

                return new OperationResponse<bool>(true, "Team deleted successfully.");

            }
            catch (Exception ex)
            {
                return new OperationResponse<bool>(ex.Message, false, 500);
            }
        }

        public async Task<OperationResponse<List<UserDto>>> GetTeamMembers(int teamId, int requestingUserId)
        {
            // Team Leader and their members share teamID, so calling the IsUserTeamMember works for both of them. 
            if (!await IsUserTeamMember(requestingUserId, teamId) && !await IsUserAdmin(requestingUserId))
                return new OperationResponse<List<UserDto>>("Access Denied.", false, 403);

            try
            {
                // This query will never return a null, if empty, its returns empty list.
                var members = _context.UserTables!.Where(u => u.TeamID == teamId).ToList();
                if (members.Count == 0)
                    return new OperationResponse<List<UserDto>>("No members found in this team.", false, 404);

                var memberDtos = members.Select(u => new UserDto
                {
                    Email = u.Email,
                    RoleID = u.RoleID,
                    TeamID = u.TeamID
                }).ToList();

                return new OperationResponse<List<UserDto>>(memberDtos, "Members retrieved successfully.");
            }
            catch (Exception ex)
            {
                return new OperationResponse<List<UserDto>>(ex.Message, false, 500);
            }
        }

        public async Task<OperationResponse<bool>> AssignUserToTeam(int teamId, int userIdToAssign, int requestingAdminId)
        {
            if (!await IsUserAdmin(requestingAdminId))
                return new OperationResponse<bool>("Access Denied.", false, 403);

            try
            {
                var team = await _context.TeamTables.Include(t => t.Users).FirstOrDefaultAsync(t => t.TeamID == teamId);
                if (team == null)
                    return new OperationResponse<bool>("No such team.", false, 404);

                var userToAssign = await _context.UserTables.FindAsync(userIdToAssign);
                if (userToAssign == null)
                    return new OperationResponse<bool>("No user found.", false, 404);

                if (userToAssign.TeamID != null)
                    return new OperationResponse<bool>("User is already assigned to a team.", false, 400);

                team.Users.Add(userToAssign);
                await _context.SaveChangesAsync();
                return new OperationResponse<bool>(true, "User assigned to team successfully.");

            }
            catch (Exception ex)
            {
                return new OperationResponse<bool>(ex.Message, false, 500);
            }
        }

        public async Task<OperationResponse<bool>> RemoveUserFromTeam(int teamId, int userIdToRemove, int requestingAdminId)
        {
            if (!await IsUserAdmin(requestingAdminId))
                return new OperationResponse<bool>("Access Denied.", false, 403);

            try
            {
                var team = await _context.TeamTables.Include(t => t.Users).FirstOrDefaultAsync(t => t.TeamID == teamId);
                if (team == null)
                    return new OperationResponse<bool>("No such team.", false, 404);

                var userToAssign = await _context.UserTables.FindAsync(userIdToRemove);
                if (userToAssign == null)
                    return new OperationResponse<bool>("No user found.", false, 404);

                team.Users.Remove(userToAssign);
                await _context.SaveChangesAsync();
                return new OperationResponse<bool>(true, "User removed from team successfully.");

            }
            catch (Exception ex)
            {
                return new OperationResponse<bool>(ex.Message, false, 500);
            }
        }
    }
}