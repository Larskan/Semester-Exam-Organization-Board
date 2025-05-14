using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrganizationBoard.DTO;
using Microsoft.EntityFrameworkCore;
using EFramework.Data;
using OrganizationBoard.IService;
using EFrameWork.Model;

namespace OrganizationBoard.Service
{
    public class BoardService //: IBoardService
    {
        private readonly OBDbContext _context;
        public BoardService(OBDbContext context)
        {
            _context = context;
        }

        private async Task<bool> IsUserAdmin(int userId){
            var user = await _context.UserTables.FirstOrDefaultAsync(u => u.UserID == userId);
            return user != null && user.RoleID == 1;
        }
        private async Task<bool> IsUserTeamLeader(int userId){
            var user = await _context.UserTables.FirstOrDefaultAsync(u => u.UserID == userId);
            return user != null && user.RoleID == 2;
        }
        private async Task<bool> IsUserTeamMember(int userId){
            var user = await _context.UserTables.FirstOrDefaultAsync(u => u.UserID == userId);
            return user != null && user.RoleID == 3;
        }

        public async Task<OperationResponse<Board>> CreateBoard(Board board, int requestingUserId)
        {
            // Only Team Leaders can create boards
            if (!await IsUserTeamLeader(requestingUserId))
                return new OperationResponse<Board>("Access Denied", false, 403);
            
            var newBoard = new Board
            {
                BoardName = board.BoardName,
                TeamID = board.TeamID
            };
            _context.BoardTables!.Add(newBoard);
            await _context.SaveChangesAsync();

            board.BoardID = newBoard.BoardID;
            return new OperationResponse<Board>(board, "Board created successfully");
        }
    }
}