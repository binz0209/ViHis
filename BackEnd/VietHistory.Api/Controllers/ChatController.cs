using System;
using Microsoft.AspNetCore.Mvc;
using VietHistory.Domain.Entities;
using VietHistory.Infrastructure.Mongo;
using MongoDB.Driver;

namespace VietHistory.Api.Controllers;

[ApiController]
[Route("api/v1/chat")]
public class ChatController : ControllerBase
{
    private readonly IMongoContext _context;

    public ChatController(IMongoContext context)
    {
        _context = context;
    }

    /// <summary>Lấy danh sách chat boxes của user hoặc machine</summary>
    [HttpGet("boxes")]
    public async Task<ActionResult> GetChatBoxes([FromQuery] string? userId, [FromQuery] string? machineId)
    {
        try
        {
            var filter = Builders<ChatHistory>.Filter.Empty;
            
            if (!string.IsNullOrEmpty(userId))
            {
                filter = Builders<ChatHistory>.Filter.Eq(h => h.UserId, userId);
            }
            else if (!string.IsNullOrEmpty(machineId))
            {
                filter = Builders<ChatHistory>.Filter.Eq(h => h.MachineId, machineId);
            }
            else
            {
                return BadRequest(new { error = "userId hoặc machineId là bắt buộc" });
            }

            var histories = await _context.ChatHistories
                .Find(filter)
                .SortByDescending(h => h.LastMessageAt)
                .ToListAsync();

            var result = histories.Select(h => new
            {
                id = h.Id,
                name = h.Name,
                lastMessageAt = h.LastMessageAt,
                messageCount = h.MessageIds.Count
            }).ToList();

            return Ok(new { boxes = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>Lưu lịch sử chat của một box</summary>
    [HttpPost("history")]
    public async Task<ActionResult> SaveHistory([FromBody] SaveChatHistoryRequest request)
    {
        try
        {
            // Xác định box ID (nếu không có thì tạo mới)
            ChatHistory? chatHistory;
            
            if (string.IsNullOrEmpty(request.BoxId))
            {
                // Tạo mới
                chatHistory = new ChatHistory
                {
                    MachineId = request.MachineId,
                    UserId = request.UserId,
                    Name = request.BoxName ?? "Chat",
                    LastMessageAt = DateTime.UtcNow,
                    MessageIds = new List<string>()
                };
                await _context.ChatHistories.InsertOneAsync(chatHistory);
            }
            else
            {
                // Update existing
                chatHistory = await _context.ChatHistories
                    .Find(h => h.Id == request.BoxId)
                    .FirstOrDefaultAsync();

                if (chatHistory == null)
                {
                    return NotFound(new { error = "Chat box not found" });
                }

                // Xóa messages cũ
                await _context.ChatMessages.DeleteManyAsync(m => m.ChatId == chatHistory.Id);
                chatHistory.MessageIds.Clear();
            }

            // Thêm messages mới
            var messageIds = new List<string>();
            foreach (var msg in request.Messages)
            {
                var timestamp = DateTime.TryParse(msg.Timestamp, out var dt) ? dt : DateTime.UtcNow;
                
                var chatMessage = new ChatMessage
                {
                    Text = msg.Text,
                    Sender = msg.Sender,
                    ChatId = chatHistory.Id,
                    CreatedAt = timestamp,
                    UpdatedAt = timestamp
                };

                await _context.ChatMessages.InsertOneAsync(chatMessage);
                messageIds.Add(chatMessage.Id);
            }

            chatHistory.MessageIds = messageIds;
            chatHistory.LastMessageAt = DateTime.UtcNow;
            chatHistory.UpdatedAt = DateTime.UtcNow;

            await _context.ChatHistories.ReplaceOneAsync(h => h.Id == chatHistory.Id, chatHistory);

            return Ok(new { 
                success = true, 
                boxId = chatHistory.Id,
                message = "Lịch sử đã được lưu" 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    /// <summary>Lấy lịch sử chat của một box</summary>
    [HttpGet("history/{boxId}")]
    public async Task<ActionResult> GetHistory(string boxId)
    {
        try
        {
            var chatHistory = await _context.ChatHistories
                .Find(h => h.Id == boxId)
                .FirstOrDefaultAsync();

            if (chatHistory == null)
            {
                return Ok(new { messages = Array.Empty<object>() });
            }

            var messages = await _context.ChatMessages
                .Find(m => m.ChatId == chatHistory.Id)
                .SortBy(m => m.CreatedAt)
                .ToListAsync();

            var result = messages.Select(m => new
            {
                id = m.Id,
                text = m.Text,
                sender = m.Sender,
                timestamp = m.CreatedAt
            }).ToList();

            return Ok(new { 
                boxId = chatHistory.Id,
                name = chatHistory.Name,
                messages = result 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>Đổi tên chat box</summary>
    [HttpPut("history/{boxId}/name")]
    public async Task<ActionResult> RenameBox(string boxId, [FromBody] RenameBoxRequest request)
    {
        try
        {
            var chatHistory = await _context.ChatHistories
                .Find(h => h.Id == boxId)
                .FirstOrDefaultAsync();

            if (chatHistory == null)
            {
                return NotFound(new { error = "Chat box not found" });
            }

            chatHistory.Name = request.Name;
            chatHistory.UpdatedAt = DateTime.UtcNow;

            await _context.ChatHistories.ReplaceOneAsync(h => h.Id == boxId, chatHistory);

            return Ok(new { success = true, message = "Đổi tên thành công" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>Xóa chat box</summary>
    [HttpDelete("history/{boxId}")]
    public async Task<ActionResult> DeleteBox(string boxId)
    {
        try
        {
            var chatHistory = await _context.ChatHistories
                .Find(h => h.Id == boxId)
                .FirstOrDefaultAsync();

            if (chatHistory != null)
            {
                // Xóa các message
                await _context.ChatMessages.DeleteManyAsync(m => m.ChatId == chatHistory.Id);

                // Xóa history
                await _context.ChatHistories.DeleteOneAsync(h => h.Id == chatHistory.Id);
            }

            return Ok(new { success = true, message = "Xóa thành công" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

public class SaveChatHistoryRequest
{
    public string MachineId { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? BoxId { get; set; }
    public string? BoxName { get; set; }
    public List<ChatMessageDto> Messages { get; set; } = new();
}

public class ChatMessageDto
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
}

public class RenameBoxRequest
{
    public string Name { get; set; } = string.Empty;
}
