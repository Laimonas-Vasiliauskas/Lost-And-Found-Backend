using LostAndFoundApi.Data;
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
                Type = dto.Type,
                Location = dto.Location,
                Title = dto.Title,
                Description = dto.Description,
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
    }
}