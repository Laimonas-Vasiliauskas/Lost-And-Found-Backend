using LostAndFoundApi.Data;
using LostAndFoundBack.Dtos;
using LostAndFoundBack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
                .Select(a => new
                {
                    adID = a.AdID,
                    userID = a.UserID,
                    title = a.Title,
                    description = a.Description,
                    location = a.Location,
                    type = a.Type,
                    createdAt = a.CreatedAt,
                    images = _context.AdImages
                        .Where(i => i.AdID == a.AdID)
                        .Select(i => i.ImageID)
                        .ToList()
                })
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
                adID = ad.AdID
            });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAd(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim);

            var ad = _context.Ads.FirstOrDefault(a => a.AdID == id && a.UserID == userId);

            if (ad == null)
            {
                return NotFound(new { message = "Skelbimas nerastas arba nepriklauso vartotojui" });
            }

            var images = _context.AdImages.Where(i => i.AdID == id).ToList();

            _context.AdImages.RemoveRange(images);
            _context.Ads.Remove(ad);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Skelbimas ištrintas sėkmingai" });
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

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var image = new AdImage
            {
                AdID = adId,
                ImageData = memoryStream.ToArray(),
                ContentType = file.ContentType
            };

            _context.AdImages.Add(image);
            await _context.SaveChangesAsync();

            return Ok(new { imageId = image.ImageID });
        }

        [HttpGet("image/{imageId}")]
        public IActionResult GetImage(int imageId)
        {
            var image = _context.AdImages.FirstOrDefault(i => i.ImageID == imageId);

            if (image == null)
                return NotFound();

            return File(image.ImageData, image.ContentType);
        }
        [HttpGet("{id}")]
        public IActionResult GetAdById(int id)
        {
            var ad = _context.Ads
                .Where(a => a.AdID == id)
                .Select(a => new
                {
                    adID = a.AdID,
                    userID = a.UserID,
                    title = a.Title,
                    description = a.Description,
                    location = a.Location,
                    type = a.Type,
                    createdAt = a.CreatedAt,
                    images = _context.AdImages
                        .Where(i => i.AdID == a.AdID)
                        .Select(i => i.ImageID)
                        .ToList()
                })
                .FirstOrDefault();

            if (ad == null)
            {
                return NotFound(new { message = "Skelbimas nerastas" });
            }

            return Ok(ad);
        }
        [HttpGet("category/{categoryId}")]
        public IActionResult GetAdsByCategory(int categoryId)
        {
            var ads = _context.Ads
                .Where(a => a.CategoryID == categoryId)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new
                {
                    adID = a.AdID,
                    userID = a.UserID,
                    categoryID = a.CategoryID,
                    title = a.Title,
                    description = a.Description,
                    location = a.Location,
                    type = a.Type,
                    createdAt = a.CreatedAt,
                    images = _context.AdImages
                        .Where(i => i.AdID == a.AdID)
                        .Select(i => i.ImageID)
                        .ToList()
                })
                .ToList();

            return Ok(ads);
        }
        [HttpGet]
        public IActionResult GetAllAds()
        {
            var ads = _context.Ads
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new
                {
                    adID = a.AdID,
                    userID = a.UserID,
                    title = a.Title,
                    description = a.Description,
                    location = a.Location,
                    type = a.Type,
                    createdAt = a.CreatedAt,
                    images = _context.AdImages
                        .Where(i => i.AdID == a.AdID)
                        .Select(i => i.ImageID)
                        .ToList()
                })
                .ToList();

            return Ok(ads);
        }
    }
}