using Microsoft.AspNetCore.Mvc;

using WebApplication16.models;
using WebApplication16.Services;

namespace VotingApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IFaceRecognitionService _faceRecognitionService;

        public UsersController(IUserService userService, IFaceRecognitionService faceRecognitionService)
        {
            _userService = userService;
            _faceRecognitionService = faceRecognitionService;
        }
        //מקבל את המשתמשים מהדאטה בייס
        [HttpGet]
        public async Task<IEnumerable<UserAdmin>> GetUsers()
        {
            return  await _userService.GetAllUsers();

        }
        //מקבל משתמש לפי האיידי שלו
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _userService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }

            return user;
        }
        //יוצר משתמש חדש מתוך הממשק אדמין
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            var registrationResult = await _faceRecognitionService.registerUser(user);

            if (registrationResult.Success&& registrationResult!=null)
            {
                return Ok(new { Message = registrationResult.Message });
            }
            else
            {
                return BadRequest(new { Message = registrationResult.Message });
            }
        }
        //פה יוצר משתמש שיש לו כמה תמונות כי זו לוגיקה שונה
        [HttpPost("multiImages")]
        public async Task<ActionResult<User>> CreateUserWithMultiImages(UserMultiply user)
        {
            try
            {
                var createdUser = await _faceRecognitionService.registerUserWithMultipullImages(user);
                return Ok();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest();
            }
        }
        //מעדכן את התוכן של המשתמש
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, User user)
        {
            if (id != user.UserId)
            {
                return BadRequest();
            }

            var updatedUser = await _userService.UpdateUser(user);
            return Ok(updatedUser);
        }
        //מוחק משתמש
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUser(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
