export interface AuthResponse { token: string; userId: number; fullName: string; email: string; avatarUrl?: string | null; }
export interface UserDto { id: number; fullName: string; email: string; avatarUrl?: string; isOnline: boolean; lastSeen: string; }
export interface ConversationDto { id: number; name: string; isGroup: boolean; members: UserDto[]; lastMessage?: MessageDto; unreadCount: number; }
export interface MessageDto { id: number; content: string; senderId: number; senderName: string; senderAvatar?: string | null; isRead: boolean; sentAt: string; }
