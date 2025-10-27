# Frontend Components

## ChatBox.tsx

Component chính của chatbox, bao gồm:
- Giao diện chat với messages (user và assistant)
- Input area với textarea
- Loading state khi đang xử lý câu hỏi
- Auto scroll đến message cuối cùng
- Welcome message với ví dụ câu hỏi

### Props
Không có props (là root component của chatbox)

### State
- `messages`: Danh sách các tin nhắn đã gửi/nhận
- `input`: Nội dung trong textarea
- `loading`: Trạng thái đang xử lý

### API Integration
Sử dụng service `api.ts` để gửi câu hỏi đến backend `/api/v1/ai/ask`


