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
    public class MessagesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MessagesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{conversationId}")]
        public async Task<IActionResult> GetMessages(int conversationId)
        {
            var currentUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    c.ConversationID == conversationId &&
                    (
                        c.User1ID == currentUserId ||
                        c.User2ID == currentUserId
                    ));

            if (conversation == null)
                return Forbid();

            var unreadMessages = await _context.Messages
                .Where(m =>
                    m.ConversationID == conversationId &&
                    m.SenderID != currentUserId &&
                    !m.IsRead)
                .ToListAsync();

            foreach (var msg in unreadMessages)
            {
                msg.IsRead = true;
            }

            await _context.SaveChangesAsync();

            var messages = await _context.Messages
                .Where(m => m.ConversationID == conversationId)
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    m.MessageID,
                    m.ConversationID,
                    m.SenderID,
                    m.Text,
                    m.SentAt
                })
                .ToListAsync();

            return Ok(messages);
        }

        [HttpPost("{conversationId}")]
        public async Task<IActionResult> SendMessage(
            int conversationId,
            [FromBody] SendMessageDto dto)
        {
            var currentUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    c.ConversationID == conversationId &&
                    (
                        c.User1ID == currentUserId ||
                        c.User2ID == currentUserId
                    ));

            if (conversation == null)
                return Forbid();

            if (string.IsNullOrWhiteSpace(dto.Text))
                return BadRequest("Žinutė tuščia");

            var message = new Message
            {
                ConversationID = conversationId,
                SenderID = currentUserId,
                Text = dto.Text
            };

            _context.Messages.Add(message);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                messageID = message.MessageID,
                conversationID = message.ConversationID,
                senderID = message.SenderID,
                text = message.Text,
                sentAt = message.SentAt
            });
        }
    }

    public class SendMessageDto
    {
        public string Text { get; set; } = string.Empty;
    }
}