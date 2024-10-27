using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using FaceAiSharp;
using WebApplication16.models;

namespace WebApplication16.Services
{
    public interface IFaceRecognitionService
    {
         Task<(bool, Dictionary<string, long>)> CheckIfSamePerson(string imageBase64_1, string imageBase64_2);
        Task<bool> IsBase64StringAsync(string base64);
        //Task<(bool, long)> CheckIfSamePerson(string imageBase64_1, string imageBase64_2); // עדכון החתימה להחזיר גם זמן
        bool IsBase64String(string base64);
        Task<RegistrationResult> registerUser(User user);

        Task<bool> registerUserWithMultipullImages(UserMultiply user);
        Task<(bool, User,double, Dictionary<string, long>)> CheckIfPersonInDbAsync(string imageBase64);
    }
}
//dotnet ef migrations add InitialCreate
