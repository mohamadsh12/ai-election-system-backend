using FaceAiSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Text.RegularExpressions;
using FaceAiSharp.Extensions;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using VotingApp.Data;
using Microsoft.Extensions.Caching.Memory;

using WebApplication16.models;
using Microsoft.EntityFrameworkCore;
using MathNet.Numerics.RootFinding;

namespace WebApplication16.Services;

public class FaceRecognitionService : IFaceRecognitionService
{
    private readonly VotingContext _context;
    private readonly IFaceDetectorWithLandmarks _faceDetector;
    private readonly IFaceEmbeddingsGenerator _faceEmbeddingsGenerator;
    private readonly IMemoryCache _cache;
    public async Task<List<User>> GetUsersAsync()
    {
        const string cacheKey = "usersList";

        // בדוק אם רשימת המשתמשים נמצאת כבר במטמון
        if (_cache.TryGetValue(cacheKey, out List<User> users))
        {
            return users;
        }

        // אם לא, טען אותם מהמסד נתונים
        users = await _context.Users.ToListAsync();

        // שמור את רשימת המשתמשים במטמון למשך זמן מסוים
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10000));

        _cache.Set(cacheKey, users, cacheEntryOptions);

        return users;
    }
    public FaceRecognitionService(VotingContext context, IMemoryCache cache)
    {
        _cache = cache;

        _context = context;
        _faceDetector = FaceAiSharpBundleFactory.CreateFaceDetectorWithLandmarks();
        _faceEmbeddingsGenerator = FaceAiSharpBundleFactory.CreateFaceEmbeddingsGenerator();
    }
    // פונקציה לניקוי המטמון עבור מפתח מסוים
    public async Task ClearUsersCacheAsync()
    {
        const string cacheKey = "usersList";
         _cache.Remove(cacheKey);
    }
    //זיהוי פנים אמור להיות רק של התמונה שהתקבלה לא לאלה שקיימות ...
    //לשמור בדאטא בייס אמבדידינג של תמונה ככה זה יחסוך מלא זמן

    //אני שומר אותם כמו שהן ולא צריך לחתוך רק להכניס לבדיקה


    //פונקציה שמשווה בין 2 תמונות ומחזירה האם זה אותו אדם או לא
    //לא בשימוש אבל עובד
    public async Task<(bool, Dictionary<string, long>)> CheckIfSamePerson(string imageBase64_1, string imageBase64_2)
    {
        var stopwatch = new Stopwatch();
        var timings = new Dictionary<string, long>();

        stopwatch.Start();
        byte[] imageBytes1 = Convert.FromBase64String(imageBase64_1);
        byte[] imageBytes2 = Convert.FromBase64String(imageBase64_2);
        stopwatch.Stop();
        timings["ConvertBase64"] = stopwatch.ElapsedMilliseconds;

        stopwatch.Restart();
        using var ms1 = new MemoryStream(imageBytes1);
        using var ms2 = new MemoryStream(imageBytes2);
        stopwatch.Stop();
        timings["MemoryStream"] = stopwatch.ElapsedMilliseconds;

        stopwatch.Restart();
          var imgLoadTasks = new[]
        {
            Image.LoadAsync<Rgb24>(ms1),
            Image.LoadAsync<Rgb24>(ms2)
        };
        var imgResults = await Task.WhenAll(imgLoadTasks);
        var img1 = imgResults[0];
        var img2 = imgResults[1];
        stopwatch.Stop();
        timings["LoadImage"] = stopwatch.ElapsedMilliseconds;



      




        stopwatch.Restart();
        var faces1Task = Task.Run(() => _faceDetector.DetectFaces(img1));
        var faces2Task = Task.Run(() => _faceDetector.DetectFaces(img2));
        var faces = await Task.WhenAll(faces1Task, faces2Task);
        stopwatch.Stop();
        timings["DetectFaces"] = stopwatch.ElapsedMilliseconds;

        var faces1 = faces[0];
        var faces2 = faces[1];

        if (faces1.Count == 0 || faces2.Count == 0)
        {
            timings["Total"] = stopwatch.ElapsedMilliseconds;
            return (false, timings);
        }

        var face1 = faces1.First();
        var face2 = faces2.First();

        stopwatch.Restart();
        var faceImage1Task = CropFaceAsync(img1, face1);
        var faceImage2Task = CropFaceAsync(img2, face2);
        var faceImages = await Task.WhenAll(faceImage1Task, faceImage2Task);
        stopwatch.Stop();
        timings["CropFace"] = stopwatch.ElapsedMilliseconds;

        stopwatch.Restart();
        var embedding1Task = Task.Run(() => _faceEmbeddingsGenerator.GenerateEmbedding(faceImages[0]));
        var embedding2Task = Task.Run(() => _faceEmbeddingsGenerator.GenerateEmbedding(faceImages[1]));
        var embeddings = await Task.WhenAll(embedding1Task, embedding2Task);
        stopwatch.Stop();
        timings["GenerateEmbedding"] = stopwatch.ElapsedMilliseconds;

        var embedding1 = embeddings[0];
        var embedding2 = embeddings[1];

        stopwatch.Restart();
        var dot = embedding1.Dot(embedding2);
        stopwatch.Stop();
        timings["DotProduct"] = stopwatch.ElapsedMilliseconds;

        var threshold = 0.6; // תוכל לשנות את הסף בהתאם לצורך

        timings["Total"] = stopwatch.ElapsedMilliseconds;
        return (dot >= threshold, timings);
    }


 


    //להפוך את זה ליותר מהיר כמו הפונקציה שיש לי שמשווה בין 2 תמונות 
    //בודק האם הבן אדם נמצא בתוך הדאטא בייס
    public async Task<(bool, User, double,Dictionary<string,long>)> CheckIfPersonInDbAsync(string imageBase64)
    {
        var stopwatch = new Stopwatch();
        var timings = new Dictionary<string, long>();

        stopwatch.Start();
        byte[] imageBytes1 = Convert.FromBase64String(imageBase64);
        stopwatch.Stop();
        timings["ConvertBase64"] = stopwatch.ElapsedMilliseconds;

        stopwatch.Restart();
        using var ms1 = new MemoryStream(imageBytes1);
        stopwatch.Stop();
        timings["MemoryStream"] = stopwatch.ElapsedMilliseconds;

        stopwatch.Restart();
        var imgLoadTask = Image.LoadAsync<Rgb24>(ms1);
        var usersLoadTask = GetUsersAsync();
        await Task.WhenAll(imgLoadTask, usersLoadTask);
        var img1 = imgLoadTask.Result;
        var users = usersLoadTask.Result;
        stopwatch.Stop();
        timings["LoadImageAndUsers"] = stopwatch.ElapsedMilliseconds;





        stopwatch.Restart();
        var faces1 = await Task.Run(() => _faceDetector.DetectFaces(img1));
        stopwatch.Stop();
        timings["DetectFaces"] = stopwatch.ElapsedMilliseconds;

        if (faces1.Count == 0)
        {
            timings["Total"] = stopwatch.ElapsedMilliseconds;
            return (false, null, 0,timings);
        }

        var face1 = faces1.First();

        stopwatch.Restart();
        var faceImage1Task = CropFaceAsync(img1, face1);
        var embedding1Task = faceImage1Task.ContinueWith(t => _faceEmbeddingsGenerator.GenerateEmbedding(t.Result));
        await Task.WhenAll(faceImage1Task, embedding1Task);
        var faceImage1 = faceImage1Task.Result;
        var embedding1 = embedding1Task.Result;
        stopwatch.Stop();
        timings["CropFaceAndGenerateEmbedding"] = stopwatch.ElapsedMilliseconds;

        var result= await CompareEmbeddingsAsync(embedding1Task.Result, users);
       
        stopwatch.Stop();
        timings["Total"] = stopwatch.ElapsedMilliseconds;
        return (result!=null, result, 0, timings);
    }
    public async Task<RegistrationResult> registerUser(User user)
    {


        var users = await GetUsersAsync();


        bool IsUserInDbById = users.FirstOrDefault((u) => u.UserId == user.UserId) != null;

        if (IsUserInDbById)
        {
            return new RegistrationResult
            {
                Success = false,
                Message = "המשתמש כבר קיים במערכת."
            };
        }




        byte[] imageBytes1 = Convert.FromBase64String(user.ImageBase64);
       
        using var ms1 = new MemoryStream(imageBytes1);
     
        var img1 = await Image.LoadAsync<Rgb24>(ms1);
      
       
        var faces1 = await Task.Run(() => _faceDetector.DetectFaces(img1));

        if (faces1.Count == 0)
        {
            return new RegistrationResult
            {
                Success = false,
                Message ="לא זוהו פנים בתמונה"
            };
        }

        var face1 = faces1.First();





        var faceImage1 = await CropFaceAsync(img1, face1);
      
        var embedding1 = await Task.Run(() => _faceEmbeddingsGenerator.GenerateEmbedding(faceImage1));

        User user2 = new();
        user2.Age = user.Age;
        user2.FaceEmbedding = embedding1;
        user2.ImageBase64 = user.ImageBase64;
        user2.AnnualVoteLimit= user.AnnualVoteLimit;
        user2.Name= user.Name;
        // יצירת GUID ייחודי
        //Guid uniqueId = Guid.NewGuid();

        // המרה למחרוזת
        //string uniqueIdString = uniqueId.ToString();
        user2.UserId= user.UserId;
        await _context.Users.AddAsync(user2);
       int rows=   await _context.SaveChangesAsync();
        await ClearUsersCacheAsync();
        await GetUsersAsync();
        return new RegistrationResult
        {
            Success = rows > 0,
            Message = rows > 0 ? "המשתמש נרשם בהצלחה" : "נכשלה השמירה של המשתמש"
        };
    }

   public async Task<bool> registerUserWithMultipullImages(UserMultiply user)
    {
        // רשימה שתשמור את כל ה-Embeddings של התמונות
        List<float[]> embeddingsList = new List<float[]>();

        foreach (var imageBase64 in user.ImagesBase64)
        {
            byte[] imageBytes = Convert.FromBase64String(imageBase64);

            using var ms = new MemoryStream(imageBytes);

            var img = await Image.LoadAsync<Rgb24>(ms);

            var faces = await Task.Run(() => _faceDetector.DetectFaces(img));

            if (faces.Count == 0)
            {
                continue; // אם אין פנים בתמונה, דלג על התמונה הזו
            }

            var face = faces.First();
            var faceImage = await CropFaceAsync(img, face);

            var embedding = await Task.Run(() => _faceEmbeddingsGenerator.GenerateEmbedding(faceImage));

            embeddingsList.Add(embedding); // הוסף את ה-Embedding לרשימה
        }

        if (embeddingsList.Count == 0)
        {
            // אם לא נמצא אף פנים בתמונות, החזר false
            return false;
        }

        // חישוב ממוצע ה-Embeddings
        var averageEmbedding = CalculateAverageEmbedding(embeddingsList);

        User user2 = new User
        {
            Age = user.Age,
            FaceEmbedding = averageEmbedding, // שמירת הממוצע
            ImageBase64 = user!=null? user.ImagesBase64.FirstOrDefault():"", // ניתן לשמור את התמונה הראשונה או אחרת לפי הצורך
            AnnualVoteLimit = user.AnnualVoteLimit,
            Name = user.Name,
            UserId = Guid.NewGuid().ToString() // יצירת GUID ייחודי
        };

        await _context.Users.AddAsync(user2);
        int rows = await _context.SaveChangesAsync();
        await ClearUsersCacheAsync();
        await GetUsersAsync();
        return rows > 0;
    }

    // פונקציה לחישוב ממוצע ה-Embeddings
    private float[] CalculateAverageEmbedding(List<float[]> embeddingsList)
    {
        int embeddingLength = embeddingsList[0].Length;
        float[] averageEmbedding = new float[embeddingLength];

        foreach (var embedding in embeddingsList)
        {
            for (int i = 0; i < embeddingLength; i++)
            {
                averageEmbedding[i] += embedding[i];
            }
        }

        for (int i = 0; i < embeddingLength; i++)
        {
            averageEmbedding[i] /= embeddingsList.Count;
        }

        return averageEmbedding;
    }



    public async Task<Image<Rgb24>> CropFaceAsync(Image<Rgb24> image, FaceDetectorResult face)
    {
        var boundingBox = face.Box;
        await Task.Run(() =>
        
        //image.Mutate(ctx => ctx.Crop(new Rectangle((int)boundingBox.X, (int)boundingBox.Y, (int)boundingBox.Width, (int)boundingBox.Height)))
        
        image.Mutate(ctx =>
        {
            ctx.Crop(new Rectangle((int)boundingBox.X, (int)boundingBox.Y, (int)boundingBox.Width, (int)boundingBox.Height));
            ctx.AutoOrient();
            ctx.Contrast(1.2f); // שיפור ניגודיות
            ctx.GaussianSharpen(0.5f); // חידוד תמונה
        })
        
        
        );
        return image;
    }
    private async Task<User > CompareEmbeddingsAsync(float[] embedding1, List<User> users)
    {
        var tasks = users.Select(async user =>
        {
            if (user.FaceEmbedding != null)
            {
                var dot = await Task.Run(() => embedding1.Dot(user.FaceEmbedding));
                if (dot >= 0.5) // סף ההשוואה
                {
                    user.dot= dot;
                    return ( true,  user, dot);
                }
                else if (dot > 0.3)
                {
                    Console.WriteLine( "this is bigger");
                }
                else
                {
                    Console.WriteLine("this is not valid");
                }
            }
            return (false, user,  0.0);
        });

        var results = await Task.WhenAll(tasks);
        var match = results.FirstOrDefault(result => result.Item1);

        return match.user;
    }

    public async Task<bool> IsBase64StringAsync(string base64)
    {
        return await Task.FromResult(IsBase64String(base64));
    }

    public bool IsBase64String(string base64)
    {
        if (string.IsNullOrEmpty(base64))
            return false;

        // Check for valid base64 string format
        base64 = base64.Trim();
        return (base64.Length % 4 == 0) && Regex.IsMatch(base64, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
    }
}

//dotnet tool install --global dotnet-ef


//dotnet ef migrations add InitialCreate
//dotnet ef database update
