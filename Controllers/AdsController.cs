using Azure.Core;
using LostAndFoundApi.Data;
using LostAndFoundBack.Dtos;
using LostAndFoundBack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace LostAndFoundBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdsController(AppDbContext context)
        {
            _context = context;
        }
        [Authorize]
        [HttpGet("my")]
        public IActionResult GetMyAds()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim);

            var ads = _context.Ads
                .Where(a => a.UserID == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToList();

            return Ok(ads);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateAd([FromBody] CreateAdDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
            {
                return Unauthorized("UserID not found in token");
            }

            int userId = int.Parse(userIdClaim);

            var ad = new Ad
            {
                UserID = userId,
                CategoryID = dto.CategoryID,
                Title = dto.Title,
                Description = dto.Description,
                Type = dto.Type,
                Location = dto.Location,
                CreatedAt = DateTime.UtcNow
            };


            _context.Ads.Add(ad);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Skelbimas sukurtas sėkmingai",
                ad.AdID
            });
        }
        [Authorize]
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage([FromForm] UploadImageDto dto)
        {
            var file = dto.File;
            var adId = dto.AdId;

            if (file == null || file.Length == 0)
                return BadRequest("Failas nepasirinktas");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim);

            var ad = _context.Ads.FirstOrDefault(a => a.AdID == adId && a.UserID == userId);
            if (ad == null)
                return BadRequest("Skelbimas nerastas arba nepriklauso vartotojui");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var imageUrl = $"images/{fileName}";

            var image = new AdImage
            {
                AdID = adId,
                ImageUrl = imageUrl
            };

            _context.AdImages.Add(image);
            await _context.SaveChangesAsync();

            return Ok(new { imageUrl });
        }
        [HttpGet]
        public IActionResult GetAllAds()
        {
            var ads = _context.Ads
                .Select(a => new
                {
                    a.AdID,
                    a.Title,
                    a.Description,
                    a.Location,
                    a.Type,
                    a.CreatedAt,
                    Images = _context.AdImages
                        .Where(img => img.AdID == a.AdID)
                        .Select(img => img.ImageUrl)
                        .ToList()
                })
                .OrderByDescending(a => a.CreatedAt)
                .ToList();

            return Ok(ads);
        }
    }

}