# Hướng dẫn Triển khai Login & Multi-Chat Boxes

## Kiến trúc Backend (Đã hoàn thành ✅)

### 1. Database Schema

**ChatHistory** (đã cập nhật):
- `MachineId` - ID của máy (nếu không login)
- `UserId` - ID của user (nếu login)
- `Name` - Tên chat box
- `LastMessageAt` - Thời gian tin nhắn cuối
- `MessageIds[]` - Danh sách message IDs

**ChatMessage**:
- `Text` - Nội dung
- `Sender` - "user" hoặc "assistant"
- `ChatId` - Reference đến ChatHistory

**AppUser**:
- `Username`, `Email`, `PasswordHash`
- Đã có sẵn trong Entities.cs

### 2. API Endpoints

**Auth** (`/api/v1/auth`):
- ✅ `POST /register` - Đăng ký
- ✅ `POST /login` - Đăng nhập  
- ✅ `GET /me` - Lấy thông tin user hiện tại

**Chat** (`/api/v1/chat`):
- ✅ `GET /boxes?userId={id}&machineId={id}` - Lấy danh sách chat boxes
- ✅ `POST /history` - Lưu lịch sử (với userId hoặc machineId)
- ✅ `GET /history/{boxId}` - Lấy lịch sử của một box
- ✅ `PUT /history/{boxId}/name` - Đổi tên box
- ✅ `DELETE /history/{boxId}` - Xóa box

### 3. JWT Authentication

- ✅ Đã setup JWT service
- ✅ Password hashing (SHA256)
- ✅ Token validation middleware

## Frontend Implementation

### 1. Cấu trúc Components

```
src/
├── components/
│   ├── LoginModal.tsx         # Modal login/register
│   ├── ChatBoxSelector.tsx    # Chọn chat box
│   ├── ChatBox.tsx            # Chat interface (update)
│   └── ChatHistoryManager.ts  # (update để support userId)
├── services/
│   ├── auth.ts                # ✅ Đã tạo
│   └── api.ts                 # Update
├── App.tsx                    # Update
└── pages/
    └── Login.tsx              # ✅ Đã tạo
```

### 2. Flow Logic

```
User mở app
  ↓
Có login? 
  ├─ YES → Load chat boxes theo userId
  └─ NO → Load chat boxes theo machineId
  ↓
Hiển thị danh sách chat boxes
  ↓
User chọn box hoặc tạo mới
  ↓
Hiển thị ChatBox với boxId
  ↓
User chat → Auto save theo userId hoặc machineId
```

### 3. State Management

Cần tạo `AuthContext` để manage:
- `isAuthenticated` - User đã login chưa
- `user` - Thông tin user
- `token` - JWT token
- `login()` - Login function
- `logout()` - Logout function

### 4. Update ChatHistoryManager

```typescript
class ChatHistoryManager {
  private machineId: string
  private userId?: string

  // Load boxes
  async loadChatBoxes(): Promise<ChatBox[]>
  
  // Save messages
  async saveMessages(boxId: string, messages: Message[]): Promise<void>
  
  // Create new box
  async createBox(name: string): Promise<string>
  
  // Rename box
  async renameBox(boxId: string, newName: string): Promise<void>
  
  // Delete box
  async deleteBox(boxId: string): Promise<void>
}
```

### 5. UI Flow

**Header**:
```
[VietHistory] [User Info/Login Button] [+New Chat]
```

**Sidebar** (Optional):
```
Chat Boxes:
- Chat 1 (rename, delete)
- Chat 2 (rename, delete)
- + New Chat
```

**Main Area**:
- ChatBox component hiện tại

### 6. Login Modal

```typescript
interface LoginModalProps {
  onLogin: (credentials) => void
  onRegister: (credentials) => void
  onClose: () => void
}
```

## Các bước triển khai

### Step 1: Setup AuthContext

```typescript
// src/context/AuthContext.tsx
const AuthContext = createContext()

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null)
  const [isAuthenticated, setIsAuthenticated] = useState(false)
  
  // login, logout, checkAuth logic
}
```

### Step 2: Update App.tsx

```typescript
function App() {
  return (
    <AuthProvider>
      <div className="App">
        <Header /> {/* With login button */}
        <ChatBoxSelector /> {/* Left sidebar */}
        <ChatBox /> {/* Main chat */}
      </div>
    </AuthProvider>
  )
}
```

### Step 3: Update ChatBox Component

- Thêm props `boxId`
- Load messages theo boxId
- Save messages với userId hoặc machineId

### Step 4: Create ChatBoxSelector

- List các boxes
- Buttons: New, Rename, Delete
- Click để switch box

### Step 5: Test Flow

1. Không login → chat → lưu theo machineId
2. Login → tạo box mới → chat → lưu theo userId
3. Switch giữa các boxes
4. Rename box
5. Delete box

## Cấu hình cần thiết

### 1. Backend (Đã xong ✅)

### 2. Frontend Dependencies

```bash
npm install react-router-dom
# (Optional) npm install @types/react-router-dom
```

### 3. Environment Variables

`.env`:
```
VITE_API_URL=http://localhost:5000
```

## Testing

### Test Backend

```bash
# Register
curl -X POST http://localhost:5000/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"test","email":"test@test.com","password":"123456"}'

# Login
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"test","password":"123456"}'

# Get chat boxes
curl http://localhost:5000/api/v1/chat/boxes?userId=USER_ID
```

## Lưu ý

1. **Security**: 
   - Password nên hash bằng bcrypt (hiện tại dùng SHA256)
   - JWT token expire sau 60 phút
   - CORS đã setup

2. **Performance**:
   - Cache user info trong localStorage
   - Debounce save messages
   - Lazy load chat boxes

3. **UX**:
   - Cho phép user chat ngay mà không cần login
   - Prompt login khi cần sync across devices
   - Auto-save khi có tin nhắn mới

## Next Steps

1. ✅ Backend APIs
2. ✅ JWT Authentication
3. ⏳ Frontend AuthContext
4. ⏳ Update ChatBox component
5. ⏳ Create ChatBoxSelector
6. ⏳ Add rename/delete UI
7. ⏳ Test end-to-end


