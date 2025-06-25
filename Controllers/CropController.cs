using System.Net.Http.Headers;
using AgriSmartAPI.Data;
using AgriSmartAPI.DTO;
using AgriSmartAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using Newtonsoft.Json;
using PestDiagnosis = AgriSmartAPI.DTO.PestDiagnosis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgriSmartAPI.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public class CropController : ControllerBase
{
    private readonly AgriSmartContext _context;
    private readonly IHttpClientFactory _httpClientFactory;

    public CropController(AgriSmartContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("crop/calender")]
    public async Task<IActionResult> GenerateCropCalendar([FromBody] CropCalendarRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized("User ID not found in token.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Unauthorized("Authenticated user not found in database.");

            var cropData = GetCropData(request.CropName);
            var climateData = await GetClimateDataAsync(request.Location);

            var crop = new Crop
            {
                Name = request.CropName,
                Location = request.Location,
                PlantingDate = request.PlantingDate,
                CareSchedule = GenerateCareSchedule(request.CropName, request.PlantingDate, climateData),
                HarvestSchedule = GenerateHarvestSchedule(request.CropName, request.PlantingDate, cropData),
                CreatedAt = DateTime.UtcNow,
                User = user 
            };

            _context.Crops.Add(crop);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Crop calendar generated successfully",
                cropId = crop.Id,
                location = crop.Location,
                cropName = crop.Name,
                plantingDate = crop.PlantingDate,
                careSchedule = crop.CareSchedule,
                harvestSchedule = crop.HarvestSchedule,
                status = 1
            });
        }
        catch (DbUpdateException ex)
        {
            var innerException = ex.InnerException?.Message ?? ex.Message;
            return StatusCode(500, new
            {
                statusCode = 500,
                message = "Failed to save crop data.",
                detail = innerException
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                statusCode = 500,
                message = "An unexpected error occurred.",
                detail = ex.Message
            });
        }
    }
    

    [HttpPost("crop/pests/detector")]
    public async Task<IActionResult> UploadPestImage(IFormFile image)
        {
            try
            {
                var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(nameIdentifier) || !int.TryParse(nameIdentifier, out int farmerId))
                    return Unauthorized(new { message = "User ID not found in token" });

                if (image == null || image.Length == 0)
                    return BadRequest(new { message = "No image provided" });

                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("http://127.0.0.1:5000/");

                using var content = new MultipartFormDataContent();
                await using var stream = image.OpenReadStream();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);
                content.Add(fileContent, "image", image.FileName);
                var response = await client.PostAsync("api/v1/detect", content);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                var flaskResponse = JsonConvert.DeserializeObject<FlaskApiResponse>(responseContent);
                if (flaskResponse?.Status != "success" || flaskResponse.Result.Detections.Count == 0)
                    return BadRequest(new { message = "No pest or disease detected by the external API" });
                var topDetection = flaskResponse.Result.Detections
                    .OrderByDescending(d => d.Confidence)
                    .First();
                var pestDiagnosis = new PestDiagnosis
                {
                    FarmerId = farmerId,
                    ImageUrl = image.FileName, 
                    Diagnosis = topDetection.Disease,
                    TreatmentRecommendation = topDetection.Advice,
                    CreatedAt = DateTime.UtcNow
                };
                _context.PestDiagnoses.Add(pestDiagnosis);
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    message = "Pest diagnosis completed",
                    diagnosisId = pestDiagnosis.Id,
                    imageUrl = pestDiagnosis.ImageUrl,
                    diagnosis = pestDiagnosis.Diagnosis,
                    treatmentRecommendation = pestDiagnosis.TreatmentRecommendation,
                    confidence = topDetection.Confidence 
                });
            }
            catch (DbUpdateException ex)
            {
                var innerException = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { statusCode = 500, message = "Failed to save pest diagnosis.", detail = innerException });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { statusCode = 500, message = "Failed to contact detection API.", detail = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = "An unexpected error occurred.", detail = ex.Message });
            }
        }

    [HttpPost("soil/type/predict")]
    public async Task<IActionResult> PredictSoilType(IFormFile image)
    {
        if (image == null || image.Length == 0)
        {
            return BadRequest("No image provided.");
        }

        // Prepare the multipart form data to send to Flask API
        using var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri("http://localhost:5000/"); // Flask API URL

        using var content = new MultipartFormDataContent();
        using var stream = image.OpenReadStream();
        using var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(image.ContentType);
        content.Add(fileContent, "image", image.FileName);

        // Call Flask API
        var response = await client.PostAsync("predict", content);
        response.EnsureSuccessStatusCode(); // Throws if not 200 OK

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var predictionResult = System.Text.Json.JsonSerializer.Deserialize<SoilPredictionResult>(jsonResponse);

        if (predictionResult.Status != "success")
        {
            return StatusCode(500, "Flask API did not return a successful prediction.");
        }

        // Store in database
        var soilPrediction = new SoilPrediction
        {
            SoilType = predictionResult.SoilType,
            Description = predictionResult.Description,
            RecommendedCrops = string.Join(",", predictionResult.RecommendedCrops), // Store as comma-separated string
            PredictionDate = DateTime.UtcNow
        };

        _context.SoilPredictions.Add(soilPrediction);
        await _context.SaveChangesAsync();

        return Ok(soilPrediction);
    }

    [HttpGet("crops/user")]
    public async Task<IActionResult> GetCropsByUserId()
    {
        try
        {
            // Extract UserId from the authenticated user's claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { message = "User ID not found in token." });

            // Verify user exists in the database
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Unauthorized(new { message = "Authenticated user not found in database." });

            // Query crops associated with the user
            var crops = await _context.Crops
                .Where(c => c.User.Id == userId)
                .Select(c => new
                {
                    cropId = c.Id,
                    cropName = c.Name,
                    location = c.Location,
                    plantingDate = c.PlantingDate,
                    careSchedule = c.CareSchedule,
                    harvestSchedule = c.HarvestSchedule,
                    createdAt = c.CreatedAt
                })
                .ToListAsync();

            // Check if any crops were found
            if (!crops.Any())
                return NotFound(new { message = "No crops found for this user." });

            return Ok(new
            {
                message = "Crops retrieved successfully",
                crops = crops,
                status = 1
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                statusCode = 500,
                message = "An unexpected error occurred.",
                detail = ex.Message
            });
        }
    }

    private string GenerateCareSchedule(string cropName, DateTime plantingDate, JObject climateData)
    {
        var cropData = GetCropData(cropName);
        var currentTemp = climateData["current_observation"]?["condition"]?["temperature"]?.Value<double>() ?? 77.0;
        var humidity = climateData["current_observation"]?["atmosphere"]?["humidity"]?.Value<double>() ?? 50.0;
        var forecasts = climateData["forecasts"] as JArray;
        var nextDayPrecipitationText = forecasts?.FirstOrDefault(f => f["day"]?.ToString() == "Fri")?["text"]?.ToString() ?? "Clear";

        string waterFreq;
        if (humidity > 70 || nextDayPrecipitationText.Contains("Rain", StringComparison.OrdinalIgnoreCase))
            waterFreq = "every 3 days (high humidity or expected rain)";
        else if (humidity < 40)
            waterFreq = "every day (low humidity)";
        else
            waterFreq = "every 2 days";

        var fertilizeDays = currentTemp > 80 ? 10 : 14;
        var fertilizeDate = plantingDate.AddDays(fertilizeDays);

        var waterNeeds = cropData.WaterNeeds switch
        {
            "High" => "Ensure consistent moisture",
            "Moderate" => "Maintain moderate moisture",
            "Low" => "Avoid overwatering",
            _ => "Follow standard watering"
        };

        return $"Water {waterFreq}, fertilize on {fertilizeDate:yyyy-MM-dd}, {waterNeeds}";
    }

    private string GenerateHarvestSchedule(string cropName, DateTime plantingDate, CropData cropData)
    {
        int growthDays = cropData?.GrowthDays ?? 90;
        var harvestDate = plantingDate.AddDays(growthDays);
        return $"Harvest expected on {harvestDate:yyyy-MM-dd}";
    }

    private string GenerateSoilRecommendations(string color, string texture)
    {
        string recommendation = "Maintain current practices";
        if (color.ToLower() == "dark brown" && texture.ToLower() == "clay")
            recommendation = "Add organic matter to improve drainage";
        else if (color.ToLower() == "light yellow" && texture.ToLower() == "sandy")
            recommendation = "Add compost and mulch to retain moisture";
        else if (texture.ToLower() == "loamy")
            recommendation = "Rotate with legumes to maintain nitrogen levels";
        return recommendation;
    }

    private async Task<JObject> GetClimateDataAsync(string disrtrict)
    {
        var client = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://yahoo-weather5.p.rapidapi.com/weather?location={disrtrict}&format=json&u=f"),
            Headers =
            {
                { "User-Agent", "insomnia/10.3.1" },
                { "x-rapidapi-key", "ddc5fa1920msh709756e0f77b131p13b6dejsn68dbe74c4653" },
                { "x-rapidapi-host", "yahoo-weather5.p.rapidapi.com" },
            },
        };
        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return JObject.Parse(body);
        }
    }

    private CropData GetCropData(string cropName)
    {
        var cropData = new Dictionary<string, CropData>
        {
            { "Tomato", new CropData { GrowthDays = 90, WaterNeeds = "Moderate" } },
            { "Wheat", new CropData { GrowthDays = 120, WaterNeeds = "Low" } }
        };
        return cropData.TryGetValue(cropName, out var data) ? data : new CropData { GrowthDays = 90 };
    }
}
