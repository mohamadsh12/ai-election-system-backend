using Microsoft.AspNetCore.Mvc;
using WebApplication16.Services;
using WebApplication16.models;
//8
//0.1
namespace WebApplication16.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class VotesController : ControllerBase
    {
        private readonly IVoteService _voteService;

        public VotesController(IVoteService voteService)
        {
            _voteService = voteService;
        }
        //מקבלת את כל הקולות
        [HttpGet]
        public async Task<ActionResult<Vote>> getAllVotes()
        {
            try
            {

                var votes = await _voteService.getAllVotes();
                return Ok(votes);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest();
            }
        }

        //מוסיפה הצבעה חדשה
        [HttpPost]
        public async Task<ActionResult<Vote>> PostVote(Vote vote)
        {
            try
            {

            var createdVote = await _voteService.AddVoteAsync(vote);
                if (createdVote == null)
                {
                    return BadRequest("Vote limit reached.");
                }
                return Ok();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest();
            }
        }

    
        //מחיקת הצבעה גם לא בשימוש
        // DELETE: api/Votes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVote(int id)
        {
            var result = await _voteService.DeleteVoteAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
