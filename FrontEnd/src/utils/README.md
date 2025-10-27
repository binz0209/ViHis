# Frontend Utilities

Các utility functions và classes hỗ trợ cho frontend.

## ChatHistoryManager

Quản lý lịch sử chat theo từng máy tính riêng lẻ sử dụng localStorage.

### Tính năng

- ✅ **Mỗi máy một ID riêng** - Tự động tạo ID unique cho từng máy
- ✅ **Lưu tự động** - Tự động lưu lịch sử khi có tin nhắn mới
- ✅ **Khôi phục lịch sử** - Tự động load lịch sử khi mở lại trang
- ✅ **Export** - Xuất lịch sử ra file JSON
- ✅ **Clear** - Xóa lịch sử
- ✅ **Đa máy** - Hỗ trợ nhiều máy khác nhau lưu lịch sử riêng

### Cách sử dụng

```typescript
import { getChatHistoryManager } from '../utils/ChatHistoryManager'

const historyManager = getChatHistoryManager()

// Lưu messages
historyManager.saveMessages(messages)

// Load messages
const messages = historyManager.loadMessages()

// Xóa lịch sử
historyManager.clearHistory()

// Export ra file
const json = historyManager.exportHistory()

// Lấy ID máy
const machineId = historyManager.getMachineId()
```

### Storage Keys

- `viet-history-machine-id` - ID của máy hiện tại
- `viet-history-chat-{machineId}` - Lịch sử chat của máy

### Example

```json
{
  "machineId": "machine-1234567890-abc123",
  "exportedAt": "2024-01-01T00:00:00.000Z",
  "messageCount": 10,
  "messages": [
    {
      "id": "1234567890",
      "text": "Câu hỏi...",
      "sender": "user",
      "timestamp": "2024-01-01T00:00:00.000Z"
    }
  ]
}
```

## htmlDecoder

Decode HTML entities trong câu trả lời AI.

### Functions

- `decodeHtmlEntities(text)` - Decode HTML entities
- `decodeHtmlEntitiesSimple(text)` - Decode đơn giản hơn

### Example

```typescript
import { decodeHtmlEntities } from '../utils/htmlDecoder'

const decoded = decodeHtmlEntities('&ndash;') // → –
```

## renderText

Format markdown text và render thành HTML.

### Functions

- `formatTextWithMarkdown(text)` - Convert markdown to HTML
- `renderMarkdownText(text)` - Render markdown thành React elements
- `renderInlineMarkdown(text)` - Render inline markdown

### Example

```typescript
import { formatTextWithMarkdown } from '../utils/renderText'

const html = formatTextWithMarkdown('**bold** text')
// → <p><strong>bold</strong> text</p>
```


