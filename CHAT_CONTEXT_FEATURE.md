# Chat Context Feature

## Overview
AI bây giờ nhớ lịch sử chat (6 tin nhắn gần nhất) để tránh lạc đề khi hỏi nhiều câu.

## Changes

### Backend
1. **DTOs.cs**: Added `ChatMessageDto` và `chatHistory` param to `AiAskRequest`
2. **GeminiClient.cs**: 
   - Process chat history from request
   - Build chat context string with last 6 messages
   - Add context to prompt: "Lịch sử cuộc trò chuyện gần đây: ..."
   - Instruct AI to use context for pronouns like "ông", "bà", etc.

### Frontend
1. **api.ts**: Update `sendMessage` to accept `chatHistory` and `boxId`
2. **ChatBox.tsx**: Send all previous messages as context to AI

## How It Works

1. User sends new message
2. Frontend collects all previous messages from current chat box
3. Frontend sends message + history to backend
4. Backend adds chat history to prompt as context
5. AI receives full context and understands pronouns/previous topics
6. AI responds with relevant context

## Testing

✅ Backend đã chạy với code mới!

**Test flow:**
1. Open frontend: http://localhost:5173
2. Chat 1: "Quang Trung là ai?"
3. Chat 2: "Ông có trận đánh lớn nào?"
4. AI sẽ hiểu "Ông" = "Quang Trung" và trả lời về các trận đánh của ông

