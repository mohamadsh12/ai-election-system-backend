using FaceAiSharp;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Diagnostics;
using WebApplication16.models;
using WebApplication16.Services;

namespace WebApplication16.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FaceRecognitionController : ControllerBase
    {
        private readonly IFaceRecognitionService faceRecognitionService;

        public FaceRecognitionController(IFaceRecognitionService _faceRecognitionService)
        {
            faceRecognitionService = _faceRecognitionService;
        }
        //משווה בין 2 תמונות לבדוק 
        //לא בשימוש 
        //
        [HttpPost]
        [Route("compare")]
        public async Task<IActionResult> CompareImages([FromBody] CompareImagesRequest request)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var base64ValidationTasks = new[]
            {
                faceRecognitionService.IsBase64StringAsync(request.ImageBase64_1),
                faceRecognitionService.IsBase64StringAsync(request.ImageBase64_2)
            };

            var base64ValidationStartTime = stopwatch.ElapsedMilliseconds;
            var base64ValidationResults = await Task.WhenAll(base64ValidationTasks);
            var base64ValidationTime = stopwatch.ElapsedMilliseconds - base64ValidationStartTime;

            if (!base64ValidationResults[0] || !base64ValidationResults[1])
            {
                stopwatch.Stop();
                var totalElapsedTimeInvalid = stopwatch.ElapsedMilliseconds;
                return BadRequest(new { Message = "Invalid Base64 string.", Base64ValidationTime = base64ValidationTime, TotalElapsedTime = totalElapsedTimeInvalid });
            }

            var checkIfSamePersonStartTime = stopwatch.ElapsedMilliseconds;
            var (isSamePerson, elapsedTime) = await faceRecognitionService.CheckIfSamePerson(request.ImageBase64_1, request.ImageBase64_2);
            var checkIfSamePersonTime = stopwatch.ElapsedMilliseconds - checkIfSamePersonStartTime;

            stopwatch.Stop();
            var totalElapsedTimeValid = stopwatch.ElapsedMilliseconds;

            return Ok(new { IsSamePerson = isSamePerson, ElapsedTime = elapsedTime, Base64ValidationTime = base64ValidationTime, CheckIfSamePersonTime = checkIfSamePersonTime, TotalElapsedTime = totalElapsedTimeValid });
        }

















        //בודק האם המשתמש קיים בנתונים שלנו אם כן הוא מחזיר את המשתמש עצמו 
        [HttpPost]
        [Route("checkInDb")]
        public async Task<IActionResult> CheckIfPersonInDb([FromBody] string imageBase64)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var (isSamePerson, user, dot,timing) = await faceRecognitionService.CheckIfPersonInDbAsync(imageBase64);

            stopwatch.Stop();
            var totalElapsedTime = stopwatch.ElapsedMilliseconds;

            if (isSamePerson)
            {
                return Ok(new { Dot = dot, User = user, TotalElapsedTime = totalElapsedTime ,Timing = timing });
            }

            return NotFound(new { Message = "Person not found in database.", TotalElapsedTime = totalElapsedTime });
        }
        //הרשמה של משתמש
        [HttpPost]
        [Route("registerUser")]
        public async Task<IActionResult> RegisterUser([FromBody] User user)
        {
            var result = await faceRecognitionService.registerUser(user);
            if (result.Success)
            {
                return Ok("המשתמש נרשם בהצלחה.");
            }
            return BadRequest("אירעה שגיאה בלתי צפויה." + result.Message);
        }
        //בודק האם הסטרינג זה base 64
        [HttpPost]
        [Route("isBase64String")]
        public async Task<IActionResult> IsBase64String([FromBody] string base64)
        {
            var isValid = await faceRecognitionService.IsBase64StringAsync(base64);
            return Ok(isValid);
        }

      
    }


    public class CropFaceRequest
    {
        public Image<Rgb24> Image { get; set; }
        public FaceDetectorResult Face { get; set; }
    }
}
//dotnet tool install --global dotnet-ef
