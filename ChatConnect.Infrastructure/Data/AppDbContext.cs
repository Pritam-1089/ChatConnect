namespace ChatConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ChatConnect.Core.Entities;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationMember> ConversationMembers => Set<ConversationMember>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).HasMaxLength(256);
            e.Property(u => u.FullName).HasMaxLength(100);
        });

        modelBuilder.Entity<ConversationMember>(e =>
        {
            e.HasIndex(cm => new { cm.ConversationId, cm.UserId }).IsUnique();
            e.HasOne(cm => cm.Conversation).WithMany(c => c.Members).HasForeignKey(cm => cm.ConversationId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(cm => cm.User).WithMany(u => u.Conversations).HasForeignKey(cm => cm.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Message>(e =>
        {
            e.HasOne(m => m.Conversation).WithMany(c => c.Messages).HasForeignKey(m => m.ConversationId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.Sender).WithMany(u => u.Messages).HasForeignKey(m => m.SenderId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
