using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ChatConnect.API.Hubs;
using ChatConnect.Application.Interfaces;
using ChatConnect.Application.Services;
using ChatConnect.Core.Entities;
using ChatConnect.Core.Interfaces;
using ChatConnect.Infrastructure.Data;
using ChatConnect.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("ChatConnectDb"));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IChatService, ChatService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) && context.HttpContext.Request.Path.StartsWithSegments("/hubs/chat"))
                    context.Token = token;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddSignalR(o => { o.MaximumReceiveMessageSize = 5 * 1024 * 1024; });
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200", "http://10.93.149.246:4200").AllowAnyHeader().AllowAnyMethod().AllowCredentials());
});

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler(err => err.Run(async context =>
{
    context.Response.ContentType = "application/json";
    var ex = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    context.Response.StatusCode = ex switch
    {
        UnauthorizedAccessException => 401,
        KeyNotFoundException => 404,
        InvalidOperationException => 400,
        _ => 500
    };
    await context.Response.WriteAsJsonAsync(new { message = ex?.Message ?? "An error occurred" });
}));

// Seed demo data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!db.Users.Any())
    {
        var pritam = new User { FullName = "Pritam Chavan", Email = "pritam@chat.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), IsOnline = true };
        var mayuri = new User { FullName = "Mayuri Kawar", Email = "mayuri@chat.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), IsOnline = true };
        var shivani = new User { FullName = "Shivani Patil", Email = "shivani@chat.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456") };
        var rohan = new User { FullName = "Rohan Deshmukh", Email = "rohan@chat.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456") };
        var aniket = new User { FullName = "Aniket", Email = "aniket@chat.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456") };
        db.Users.AddRange(pritam, mayuri, shivani, rohan, aniket);
        db.SaveChanges();

        var chat1 = new Conversation { IsGroup = false };
        db.Conversations.Add(chat1);
        db.SaveChanges();
        db.ConversationMembers.AddRange(
            new ConversationMember { ConversationId = chat1.Id, UserId = pritam.Id },
            new ConversationMember { ConversationId = chat1.Id, UserId = mayuri.Id });
        db.Messages.AddRange(
            new Message { ConversationId = chat1.Id, SenderId = pritam.Id, Content = "Hi Mayuri, shopping cart API ka PR ready hai?", SentAt = DateTime.UtcNow.AddHours(-2) },
            new Message { ConversationId = chat1.Id, SenderId = mayuri.Id, Content = "Haan, abhi push kar rahi hoon. 10 min me review kar lena.", SentAt = DateTime.UtcNow.AddHours(-1.5) },
            new Message { ConversationId = chat1.Id, SenderId = pritam.Id, Content = "Ok done, stock validation check karna mat bhulna", SentAt = DateTime.UtcNow.AddHours(-1) },
            new Message { ConversationId = chat1.Id, SenderId = mayuri.Id, Content = "Haan wo add kiya hai, edge cases bhi handle kiye", SentAt = DateTime.UtcNow.AddMinutes(-30) });
        db.SaveChanges();

        var chat2 = new Conversation { IsGroup = false };
        db.Conversations.Add(chat2);
        db.SaveChanges();
        db.ConversationMembers.AddRange(
            new ConversationMember { ConversationId = chat2.Id, UserId = pritam.Id },
            new ConversationMember { ConversationId = chat2.Id, UserId = shivani.Id });
        db.Messages.AddRange(
            new Message { ConversationId = chat2.Id, SenderId = shivani.Id, Content = "Pritam, product CRUD me image upload add karu?", SentAt = DateTime.UtcNow.AddHours(-3) },
            new Message { ConversationId = chat2.Id, SenderId = pritam.Id, Content = "Abhi nahi, pehle basic CRUD complete karo. Image baad me", SentAt = DateTime.UtcNow.AddHours(-2.5) },
            new Message { ConversationId = chat2.Id, SenderId = shivani.Id, Content = "Ok, basic CRUD almost done. Testing kar rahi hoon", SentAt = DateTime.UtcNow.AddMinutes(-45) });
        db.SaveChanges();

        var group = new Conversation { IsGroup = true, Name = "Team ShopEase" };
        db.Conversations.Add(group);
        db.SaveChanges();
        db.ConversationMembers.AddRange(
            new ConversationMember { ConversationId = group.Id, UserId = pritam.Id, IsAdmin = true },
            new ConversationMember { ConversationId = group.Id, UserId = mayuri.Id },
            new ConversationMember { ConversationId = group.Id, UserId = shivani.Id },
            new ConversationMember { ConversationId = group.Id, UserId = rohan.Id });
        db.Messages.AddRange(
            new Message { ConversationId = group.Id, SenderId = pritam.Id, Content = "Team, sprint review kal 3pm pe hai", SentAt = DateTime.UtcNow.AddHours(-5) },
            new Message { ConversationId = group.Id, SenderId = mayuri.Id, Content = "Shopping cart API done, PR raised #39", SentAt = DateTime.UtcNow.AddHours(-4) },
            new Message { ConversationId = group.Id, SenderId = shivani.Id, Content = "Product CRUD 90% done, sirf pagination pending", SentAt = DateTime.UtcNow.AddHours(-3.5) },
            new Message { ConversationId = group.Id, SenderId = rohan.Id, Content = "Category API with subcategories done and tested", SentAt = DateTime.UtcNow.AddHours(-3) },
            new Message { ConversationId = group.Id, SenderId = pritam.Id, Content = "Great work! Payment gateway next sprint me", SentAt = DateTime.UtcNow.AddHours(-2) },
            new Message { ConversationId = group.Id, SenderId = mayuri.Id, Content = "Razorpay ya Stripe?", SentAt = DateTime.UtcNow.AddHours(-1) },
            new Message { ConversationId = group.Id, SenderId = pritam.Id, Content = "Razorpay, Indian market ke liye better hai", SentAt = DateTime.UtcNow.AddMinutes(-20) });
        db.SaveChanges();

        var group2 = new Conversation { IsGroup = true, Name = "Backend Devs" };
        db.Conversations.Add(group2);
        db.SaveChanges();
        db.ConversationMembers.AddRange(
            new ConversationMember { ConversationId = group2.Id, UserId = pritam.Id, IsAdmin = true },
            new ConversationMember { ConversationId = group2.Id, UserId = rohan.Id });
        db.Messages.AddRange(
            new Message { ConversationId = group2.Id, SenderId = pritam.Id, Content = "Rohan, EF Core migrations me koi issue?", SentAt = DateTime.UtcNow.AddHours(-6) },
            new Message { ConversationId = group2.Id, SenderId = rohan.Id, Content = "Nahi, sab smooth chal raha hai", SentAt = DateTime.UtcNow.AddHours(-5.5) });
        db.SaveChanges();
    }
}

app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
