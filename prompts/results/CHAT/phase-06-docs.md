# CHAT – Phase 6: Documentation & Demo (Result)

Date: 2025-01-XX

Artifacts
- Test Plan: `tests/CHAT_TEST_PLAN.md`
- Test Code:
  - `BackEnd/VietHistory.AI.Tests/CHAT_IntegrationTests.cs`
- Controller:
  - `BackEnd/VietHistory.Api/Controllers/ChatController.cs`
- Entities:
  - `BackEnd/VietHistory.Domain/Entities/ChatHistory.cs`
  - `BackEnd/VietHistory.Domain/Entities/ChatMessage.cs`

How to Run Tests

All CHAT Tests
```bash
cd BackEnd
dotnet test --filter "Feature=CHAT"
```

With Coverage
```bash
dotnet test --filter "Feature=CHAT" --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport-chat" "-reporttypes:Html;HtmlSummary"
open coveragereport-chat/index.html
```

Test Status Summary
- ✅ Total: 25 test cases
- ✅ Passed: 25/25 (100%)
- ✅ Failed: 0
- ⏱️ Duration: ~10s
- 📦 Categories: Integration (25)

Feature Coverage

Endpoints Tested
1. ✅ `GET /api/v1/chat/boxes` - GetChatBoxes (5 tests)
2. ✅ `POST /api/v1/chat/history` - SaveHistory (7 tests)
3. ✅ `GET /api/v1/chat/history/{boxId}` - GetHistory (4 tests)
4. ✅ `PUT /api/v1/chat/history/{boxId}/name` - RenameBox (4 tests)
5. ✅ `DELETE /api/v1/chat/history/{boxId}` - DeleteBox (4 tests)

Key Test Scenarios

Happy Path
- Get chat boxes by userId or machineId
- Create new chat box with messages
- Update existing chat box (deletes old messages)
- Retrieve chat history with messages sorted
- Rename chat box
- Delete chat box and all messages (cascade)

Edge Cases
- No userId or machineId → 400 BadRequest
- Empty results → Empty list (200 OK)
- BoxId not found → 404 NotFound (for update/rename) or 200 OK (for delete/get)
- Empty messages list → Creates box with 0 messages
- Invalid timestamp → Uses UtcNow
- Empty name → Updates to empty (no validation)

Error Handling
- BoxId not found validation
- MongoDB connection handling
- Timestamp parsing errors
- Invalid parameters

Cascade Operations
- SaveHistory update: Old messages deleted before new ones inserted
- DeleteBox: All messages deleted before box deleted
- MessageIds list updated when messages change

Known Behaviors Documented

1. GetHistory NotFound Behavior
   - Returns 200 OK with empty messages (not 404)
   - Idempotent behavior for non-existent boxId

2. DeleteBox Idempotent Behavior
   - Returns 200 OK even if boxId not found
   - Safe to call multiple times

3. SaveHistory Cascade Delete
   - Old messages are deleted before new ones inserted
   - MessageIds list is cleared before adding new IDs

4. GetChatBoxes Parameters
   - Both userId and machineId are required parameters (nullable)
   - userId takes precedence when both provided

Demo Flows

Flow 1: Create New Chat Box and Save Messages
```
1. POST /api/v1/chat/history
   Body: { machineId: "machine123", messages: [...] }
   Response: 200 { success: true, boxId: "box456" }
   
2. GET /api/v1/chat/history/{boxId}
   Response: 200 { boxId, name, messages: [...] }
```

Flow 2: Update Existing Chat Box
```
1. POST /api/v1/chat/history
   Body: { boxId: "box456", machineId: "machine123", messages: [new messages] }
   Response: 200 { success: true, boxId: "box456" }
   Note: Old messages are deleted, new messages inserted
```

Flow 3: List Chat Boxes
```
1. GET /api/v1/chat/boxes?userId={userId}
   Response: 200 { boxes: [{ id, name, lastMessageAt, messageCount }] }
   
2. GET /api/v1/chat/boxes?machineId={machineId}
   Response: 200 { boxes: [...] }
```

Flow 4: Rename and Delete Chat Box
```
1. PUT /api/v1/chat/history/{boxId}/name
   Body: { name: "New Name" }
   Response: 200 { success: true, message: "Đổi tên thành công" }
   
2. DELETE /api/v1/chat/history/{boxId}
   Response: 200 { success: true, message: "Xóa thành công" }
   Note: All messages are also deleted (cascade delete)
```

Documentation Links
- Test Plan: `tests/CHAT_TEST_PLAN.md`
- Phase Results: `prompts/results/CHAT/phase-*.md`
- API Documentation: See `ChatController.cs` for endpoint details

Quick Reference

Test Naming Convention
- Format: `TCxx_Given_When_Then`
- Example: `TC01_GetChatBoxes_WithUserId_ReturnsBoxes`

Traits Used
- Feature: `CHAT`
- Category: `HappyPath`, `EdgeCase`, `ErrorHandling`, `Integration`, `CascadeOperations`
- Priority: `P0`, `P1`, `P2`
- Integration: `Real` (for integration tests)

Exit Criteria
- ✅ Test documentation complete
- ✅ Demo flows documented
- ✅ Quick reference guide provided
- ✅ Known behaviors documented
- ✅ Test status summary provided

