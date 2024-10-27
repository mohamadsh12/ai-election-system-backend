using Microsoft.EntityFrameworkCore;
using System;
using VotingApp.Data;
using WebApplication16.models;

namespace WebApplication16.Services
{
    public class VoteService: IVoteService
    {
        private readonly VotingContext _context;

        public VoteService(VotingContext context)
        {
            _context = context;
        }


        public async Task<List<Vote>> getAllVotes()
        {



            return await _context.Votes.ToListAsync();
        }

        public async Task<Vote> AddVoteAsync(Vote vote)
        {
            //Vote newVote = new Vote();
            //newVote.Party = vote.Party;
            // newVote.User = vote.User;  

            //לבדוק שהיוזר הוא יכול לבחור
            var numberOfVotes=await _context.Votes.Where(v=>v.myUserId==vote.myUserId).ToListAsync();
            var currentUser=await _context.Users.FirstOrDefaultAsync(u=>u.UserId==vote.myUserId);
            if (currentUser != null&&currentUser.AnnualVoteLimit<=numberOfVotes.Count())
            {
                return null;
            }


            await _context.Votes.AddAsync(vote);
            await _context.SaveChangesAsync();
            return vote;
        }

      

        public async Task<bool> DeleteVoteAsync(int id)
        {
            var vote = await _context.Votes.FindAsync(id);
            if (vote == null)
            {
                return false;
            }

            _context.Votes.Remove(vote);
            await _context.SaveChangesAsync();
            return true;
        }

   
    }
}
