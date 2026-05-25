using LostAndFoundApi.Data;
using LostAndFoundBack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LostAndFoundBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConversationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ConversationsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("start/{adId}")]
        public async Task<IActionResult> StartConversation(int adId)
        {
            var currentUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var ad = await _context.Ads
                .FirstOrDefaultAsync(a => a.AdID == adId);

            if (ad == null)
                return NotFound("Skelbimas nerastas");

            var adAuthorId = ad.UserID;

            if (currentUserId == adAuthorId)
                return BadRequest("Negali rašyti pats sau");

            var existingConversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    c.AdID == adId &&
                    (
                        (c.User1ID == currentUserId && c.User2ID == adAuthorId) ||
                        (c.User1ID == adAuthorId && c.User2ID == currentUserId)
                    ));

            if (existingConversation != null)
            {
                return Ok(new
                {
                    conversationID = existingConversation.ConversationID
                });
            }

            var conversation = new Conversation
            {
                AdID = adId,
                User1ID = currentUserId,
                User2ID = adAuthorId
            };

            _context.Conversations.Add(conversation);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                conversationID = conversation.ConversationID
            });
        }
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var currentUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var unreadCount = await _context.Messages
                .Where(m =>
                    m.SenderID != currentUserId &&
                    !m.IsRead &&
                    (
                        m.Conversation.User1ID == currentUserId ||
                        m.Conversation.User2ID == currentUserId
                    ))
                .CountAsync();

            return Ok(new
            {
                unreadCount
            });
        }
        [HttpGet("my")]
        public async Task<IActionResult> GetMyConversations()
        {
            var currentUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var conversations = await _context.Conversations
                .Include(c => c.Ad)
                .Include(c => c.Messages)
                .Where(c =>
                    c.User1ID == currentUserId ||
                    c.User2ID == currentUserId)
                .Select(c => new
                {
                    conversationID = c.ConversationID,

                    adID = c.AdID,

                    adTitle = c.Ad != null
                        ? c.Ad.Title
                        : "",

                    adImage = _context.AdImages
                        .Where(i => i.AdID == c.AdID)
                        .Select(i => i.ImageID)
                        .FirstOrDefault(),

                    hasUnread = c.Messages.Any(m =>
                        !m.IsRead &&
                        m.SenderID != currentUserId),

                    lastMessage = c.Messages
                        .OrderByDescending(m => m.SentAt)
                        .Select(m => m.Text)
                        .FirstOrDefault(),

                    lastMessageTime = c.Messages
                        .OrderByDescending(m => m.SentAt)
                        .Select(m => m.SentAt)
                        .FirstOrDefault()
                })
                .OrderByDescending(c => c.lastMessageTime)
                .ToListAsync();

            return Ok(conversations);
        }
    }
}