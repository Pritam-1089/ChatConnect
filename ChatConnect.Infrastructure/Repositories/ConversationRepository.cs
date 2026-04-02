namespace ChatConnect.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using ChatConnect.Core.Entities;
using ChatConnect.Core.Interfaces;
using ChatConnect.Infrastructure.Data;

public class ConversationRepository : IConversationRepository
{
    private readonly AppDbContext _context;
    public ConversationRepository(AppDbContext context) => _context = context;

    public async Task<Conversation?> GetByIdAsync(int id) => await _context.Conversations.FindAsync(id);

    public async Task<Conversation?> GetWithMembersAsync(int id) =>
        await _context.Conversations.Include(c => c.Members).ThenInclude(m => m.User).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Conversation?> GetPrivateConversationAsync(int userId1, int userId2) =>
        await _context.Conversations
            .Where(c => !c.IsGroup && c.Members.Any(m => m.UserId == userId1) && c.Members.Any(m => m.UserId == userId2))
            .FirstOrDefaultAsync();

    public async Task<IEnumerable<Conversation>> GetUserConversationsAsync(int userId) =>
        await _context.Conversations
            .Where(c => c.Members.Any(m => m.UserId == userId))
            .Include(c => c.Members).ThenInclude(m => m.User)
            .Include(c => c.Messages.OrderByDescending(msg => msg.SentAt).Take(1)).ThenInclude(m => m.Sender)
            .OrderByDescending(c => c.Messages.Max(m => (DateTime?)m.SentAt) ?? c.CreatedAt)
            .ToListAsync();

    public async Task<Conversation> AddAsync(Conversation conversation) { _context.Conversations.Add(conversation); await _context.SaveChangesAsync(); return conversation; }
    public async Task UpdateAsync(Conversation conversation) { _context.Conversations.Update(conversation); await _context.SaveChangesAsync(); }
}
