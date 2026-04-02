# ChatConnect - Real-time Chat Application

A full-stack real-time chat application with one-on-one messaging, group chats, and live notifications, built with ASP.NET Core and Angular.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| **Backend** | ASP.NET Core 9, Entity Framework Core, SQL Server |
| **Frontend** | Angular 20, TypeScript, SCSS |
| **Real-time** | SignalR WebSockets |
| **Auth** | JWT Authentication |

## Features

- **One-on-One Chat** — Private messaging between users with message history
- **Group Messaging** — Create group chats, add/remove members, set group admin
- **Real-time Communication** — Instant message delivery using SignalR WebSockets
- **Online/Offline Status** — See who's online with live presence indicators
- **Typing Indicator** — Real-time "user is typing..." notifications
- **File & Image Sharing** — Upload and share files within conversations
- **Message Search** — Search through chat history across all conversations
- **Read Receipts** — Know when your messages have been seen
- **JWT Authentication** — Secure user authentication and session management

## Architecture

```
ChatConnect/
├── ChatConnect.API/              # Web API + SignalR Hubs
├── ChatConnect.Application/      # Business logic - Services, DTOs
├── ChatConnect.Core/             # Domain layer - Entities, Interfaces
├── ChatConnect.Infrastructure/   # Data access - EF Core, Repositories
└── ChatConnect.Frontend/         # Angular 20 SPA
```

## Getting Started

### Prerequisites
- .NET 9 SDK
- Node.js 20+
- SQL Server
- Angular CLI

### Run Backend
```bash
cd ChatConnect.API
dotnet restore
dotnet ef database update
dotnet run
```

### Run Frontend
```bash
cd ChatConnect.Frontend
npm install
ng serve
```

API will run on `https://localhost:7002` and frontend on `http://localhost:4200`

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register new user |
| POST | `/api/auth/login` | Login & get JWT token |
| GET | `/api/conversations` | Get user conversations |
| POST | `/api/conversations` | Create new conversation |
| GET | `/api/conversations/{id}/messages` | Get messages |
| POST | `/api/messages` | Send message |
| POST | `/api/messages/upload` | Upload file attachment |
| GET | `/api/users/online` | Get online users |

## SignalR Hubs

| Hub | Event | Description |
|-----|-------|-------------|
| `ChatHub` | `ReceiveMessage` | New message received |
| `ChatHub` | `UserTyping` | Typing indicator |
| `ChatHub` | `UserOnline` | User came online |
| `ChatHub` | `UserOffline` | User went offline |
| `ChatHub` | `MessageRead` | Read receipt update |

## Screenshots

> Coming soon

## License

This project is licensed under the MIT License.
