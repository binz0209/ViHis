# Chat History Feature

## Tổng quan

Tính năng lưu lịch sử chat vào MongoDB backend thay vì chỉ localStorage. Hỗ trợ lưu và phục hồi lịch sử theo từng máy tính riêng lẻ.

## Kiến trúc

### Backend

1. **Entities**
   - `ChatHistory`: Lưu thông tin về một chat session
   - `ChatMessage`: Lưu từng tin nhắn trong chat

2. **Collections**
   - `chatHistories`: Collection chứa các chat history
   - `chatMessages`: Collection chứa các tin nhắn

3. **API Endpoints** (`/api/v1/chat`)
   - `POST /history` - Lưu lịch sử chat
   - `GET /history/{machineId}` - Lấy lịch sử chat
   - `DELETE /history/{machineId}` - Xóa lịch sử chat

### Frontend

1. **ChatHistoryManager**
   - Quản lý lưu/tải lịch sử từ backend
   - Fallback về localStorage nếu backend lỗi
   - Tự động sync giữa backend và localStorage

2. **Storage Strategy**
   - Ưu tiên lưu vào backend
   - LocalStorage làm backup
   - Sync khi load

## Cấu trúc Database

### ChatHistory Document

```json
{
  "_id": "ObjectId",
  "machineId": "machine-123456",
  "lastMessageAt": "2024-01-01T00:00:00Z",
  "messageIds": ["msg1", "msg2"],
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z"
}
```

### ChatMessage Document

```json
{
  "_id": "ObjectId",
  "text": "Hello",
  "sender": "user",
  "chatId": "chat-123",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z"
}
```

## API Usage

### Save Chat History

```bash
POST /api/v1/chat/history
Content-Type: application/json

{
  "machineId": "machine-123",
  "messages": [
    {
      "id": "1",
      "text": "Hello",
      "sender": "user",
      "timestamp": "2024-01-01T00:00:00Z"
    }
  ]
}
```

### Get Chat History

```bash
GET /api/v1/chat/history/{machineId}
```

Response:
```json
{
  "messages": [
    {
      "id": "1",
      "text": "Hello",
      "sender": "user",
      "timestamp": "2024-01-01T00:00:00Z"
    }
  ]
}
```

### Delete Chat History

```bash
DELETE /api/v1/chat/history/{machineId}
```

## Frontend Usage

```typescript
import { getChatHistoryManager } from '../utils/ChatHistoryManager'

const historyManager = getChatHistoryManager()

// Save messages (async)
await historyManager.saveMessages(messages)

// Load messages (async)
const messages = await historyManager.loadMessages()

// Clear history (async)
await historyManager.clearHistory()
```

## Tính năng

### 1. Đồng bộ Backend + LocalStorage
- Lưu vào backend làm source of truth
- LocalStorage làm cache/backup
- Tự động sync khi load

### 2. Theo dõi theo máy tính
- Mỗi máy có machine ID riêng
- Lịch sử tách biệt giữa các máy
- Load lịch sử của máy cụ thể

### 3. Fallback Mechanism
- Nếu backend lỗi → dùng localStorage
- Nếu localStorage lỗi → hiển thị lỗi
- Graceful degradation

### 4. Export/Import
- Export lịch sử ra JSON file
- Import lịch sử từ JSON file
- Backup và restore

## Database Indexes

Để tối ưu performance, nên tạo indexes:

```javascript
// In MongoDB
db.chatHistories.createIndex({ "machineId": 1 })
db.chatMessages.createIndex({ "chatId": 1 })
db.chatMessages.createIndex({ "createdAt": 1 })
```

## Testing

### Test Backend

```bash
# Save history
curl -X POST http://localhost:5000/api/v1/chat/history \
  -H "Content-Type: application/json" \
  -d '{
    "machineId": "test-machine",
    "messages": [
      {
        "id": "1",
        "text": "Hello",
        "sender": "user",
        "timestamp": "2024-01-01T00:00:00Z"
      }
    ]
  }'

# Get history
curl http://localhost:5000/api/v1/chat/history/test-machine

# Delete history
curl -X DELETE http://localhost:5000/api/v1/chat/history/test-machine
```

## Lưu ý

1. **Performance**: Có thể tối ưu bằng cách:
   - Chỉ lưu messages mới
   - Batch save thay vì save từng message
   - Sử dụng pagination khi load

2. **Storage**: 
   - MongoDB giới hạn document size là 16MB
   - Mỗi chat message khoảng ~1-2KB
   - Một chat history có thể chứa ~8,000-16,000 tin nhắn

3. **Security**:
   - Có thể thêm authentication
   - Rate limiting cho API calls
   - Validate input data

4. **Privacy**:
   - Có thể thêm encryption cho sensitive data
   - TTL cho auto-delete old messages
   - User consent cho data collection


