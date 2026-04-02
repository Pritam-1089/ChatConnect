namespace ChatConnect.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using ChatConnect.Core.Entities;
using ChatConnect.Core.Interfaces;
using ChatConnect.Infrastructure.Data;

public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _context;
    public MessageRepository(AppDbContext context) => _context = context;

    public async Task<Message> AddAsync(Message message) { _context.Messages.Add(message); await _context.SaveChangesAsync(); return message; }

    public async Task<IEnumerable<Message>> GetConversationMessagesAsync(int conversationId, int skip = 0, int take = 50) =>
        await _context.Messages
            .Where(m => m.ConversationId == conversationId)
            .Include(m => m.Sender)
            .OrderByDescending(m => m.SentAt)
            .Skip(skip).Take(take)
            .ToListAsync();

    public async Task MarkAsReadAsync(int conversationId, int userId) {
        var unread = await _context.Messages.Where(m => m.ConversationId == conversationId && m.SenderId != userId && !m.IsRead).ToListAsync();
        unread.ForEach(m => m.IsRead = true);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCountAsync(int conversationId, int userId) =>
        await _context.Messages.CountAsync(m => m.ConversationId == conversationId && m.SenderId != userId && !m.IsRead);

    public async Task<Message?> GetByIdAsync(int id) => await _context.Messages.FindAsync(id);
    public async Task UpdateAsync(Message message) { _context.Messages.Update(message); await _context.SaveChangesAsync(); }
}
