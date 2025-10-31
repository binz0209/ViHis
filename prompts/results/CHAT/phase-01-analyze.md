# CHAT – Phase 1: Analyze (Result)

Date: 2025-01-XX
Scope: Analyze end-to-end flow for Chat History Management feature, including chat box listing, saving/loading chat history, renaming boxes, and deleting boxes.

Functions/Endpoints to test (per refined Phase 1 prompt)

1) ChatController.GetChatBoxes(userId?, machineId?)
- Main purpose: Retrieve list of chat boxes for a user or machine, sorted by last message time
- Inputs: Optional `userId` string, optional `machineId` string
- Returns: `ActionResult` with list of boxes { id, name, lastMessageAt, messageCount }
- Dependencies: `IMongoContext.ChatHistories`, MongoDB
- Logic branches:
  - userId provided → Filter by userId
  - machineId provided → Filter by machineId
  - Both null/empty → 400 BadRequest ("userId hoặc machineId là bắt buộc")
  - Results sorted by LastMessageAt descending
- Edge cases:
  - Both userId and machineId null → 400 BadRequest
  - Both userId and machineId provided → userId takes precedence (current behavior)
  - No chat boxes found → Empty list (200 OK)
  - Invalid userId/machineId format
  - MongoDB connection failure → 500
- Test type: Integration (Real MongoDB)
- Suggested Test Names: TC01_GetChatBoxes_WithUserId_ReturnsBoxes, TC02_GetChatBoxes_WithMachineId_ReturnsBoxes, TC03_GetChatBoxes_NoParams_Returns400, TC04_GetChatBoxes_EmptyResults_ReturnsEmptyList, TC05_GetChatBoxes_SortedByLastMessageAt

2) ChatController.SaveHistory(request: SaveChatHistoryRequest)
- Main purpose: Save chat history for a box (create new box if BoxId null, update existing if BoxId provided)
- Inputs: `SaveChatHistoryRequest` { MachineId, UserId?, BoxId?, BoxName?, Messages[] }
- Returns: `ActionResult` with { success, boxId, message }
- Dependencies: `IMongoContext.ChatHistories`, `IMongoContext.ChatMessages`, MongoDB
- Logic branches:
  - BoxId null/empty → Create new ChatHistory → Insert messages → Return new boxId
  - BoxId provided → Find existing ChatHistory → Delete old messages → Insert new messages → Update ChatHistory
  - BoxName null → Use "Chat" as default
  - Timestamp parsing: TryParse msg.Timestamp, fallback to DateTime.UtcNow
- Edge cases:
  - BoxId null → Create new box
  - BoxId provided but not found → 404 NotFound
  - Empty messages list → Creates/updates box with 0 messages
  - Invalid timestamp format → Uses UtcNow
  - MachineId empty → Should validate (currently no validation)
  - Cascade delete: When updating, old messages are deleted before inserting new ones
  - MongoDB write failure → 500
- Test type: Integration (Real MongoDB)
- Suggested Test Names: TC01_SaveHistory_NewBox_CreatesBoxAndMessages, TC02_SaveHistory_ExistingBox_UpdatesMessages, TC03_SaveHistory_BoxIdNotFound_Returns404, TC04_SaveHistory_EmptyMessages_CreatesBox, TC05_SaveHistory_InvalidTimestamp_UsesUtcNow, TC06_SaveHistory_DeleteOldMessages_WhenUpdating

3) ChatController.GetHistory(boxId: string)
- Main purpose: Retrieve chat history (messages) for a specific box
- Inputs: `boxId` string (path parameter)
- Returns: `ActionResult` with { boxId, name, messages[] }
- Dependencies: `IMongoContext.ChatHistories`, `IMongoContext.ChatMessages`, MongoDB
- Logic branches:
  - BoxId found → Return box info + sorted messages
  - BoxId not found → Return { boxId, name: "Chat", messages: [] } (200 OK, not 404)
  - Messages sorted by CreatedAt ascending
- Edge cases:
  - BoxId not found → Returns empty result (200 OK, not 404)
  - BoxId with no messages → Returns empty messages array
  - Invalid boxId format → MongoDB may throw exception
  - Messages out of order → Should be sorted by CreatedAt
  - MongoDB connection failure → 500
- Test type: Integration (Real MongoDB)
- Suggested Test Names: TC01_GetHistory_ValidBoxId_ReturnsMessages, TC02_GetHistory_BoxIdNotFound_ReturnsEmpty, TC03_GetHistory_MessagesSortedByCreatedAt, TC04_GetHistory_BoxWithNoMessages_ReturnsEmptyArray, TC05_GetHistory_InvalidBoxId_HandlesGracefully

4) ChatController.RenameBox(boxId: string, request: RenameBoxRequest)
- Main purpose: Rename a chat box
- Inputs: `boxId` string (path parameter), `RenameBoxRequest` { Name }
- Returns: `ActionResult` with { success, message }
- Dependencies: `IMongoContext.ChatHistories`, MongoDB
- Logic branches:
  - BoxId found → Update Name → Update UpdatedAt → Replace document → 200 OK
  - BoxId not found → 404 NotFound
- Edge cases:
  - BoxId not found → 404 NotFound
  - Empty Name → Should validate (currently no validation, allows empty)
  - Invalid boxId format → MongoDB may throw exception
  - MongoDB write failure → 500
- Test type: Integration (Real MongoDB)
- Suggested Test Names: TC01_RenameBox_ValidBoxId_UpdatesName, TC02_RenameBox_BoxIdNotFound_Returns404, TC03_RenameBox_EmptyName_UpdatesToEmpty, TC04_RenameBox_UpdatesUpdatedAt

5) ChatController.DeleteBox(boxId: string)
- Main purpose: Delete a chat box and all its messages (cascade delete)
- Inputs: `boxId` string (path parameter)
- Returns: `ActionResult` with { success, message }
- Dependencies: `IMongoContext.ChatHistories`, `IMongoContext.ChatMessages`, MongoDB
- Logic branches:
  - BoxId found → Delete all ChatMessages with matching ChatId → Delete ChatHistory → 200 OK
  - BoxId not found → Still returns 200 OK (idempotent behavior)
- Edge cases:
  - BoxId not found → Returns 200 OK (idempotent, no error)
  - BoxId with no messages → Still deletes box successfully
  - Cascade delete: All messages are deleted before box is deleted
  - Invalid boxId format → MongoDB may throw exception
  - MongoDB delete failure → 500
- Test type: Integration (Real MongoDB)
- Suggested Test Names: TC01_DeleteBox_ValidBoxId_DeletesBoxAndMessages, TC02_DeleteBox_BoxIdNotFound_Returns200, TC03_DeleteBox_CascadeDeleteMessages, TC04_DeleteBox_BoxWithNoMessages_DeletesBox

Prioritization
- High Priority (P0): 
  - GetChatBoxes (user-facing list)
  - SaveHistory (core CRUD operation)
  - GetHistory (core read operation)
- Medium Priority (P1):
  - RenameBox (update operation)
  - DeleteBox (delete operation)

Assumptions & Invariants
- ChatHistory and ChatMessage are stored in separate MongoDB collections
- ChatHistory.MessageIds contains list of ChatMessage.Id references
- BoxId is MongoDB ObjectId (24 hex characters)
- MachineId is required for guest users
- UserId is optional (null for guest users)
- Messages are sorted by CreatedAt ascending when retrieved
- GetHistory returns 200 OK even if box not found (empty result, not 404)
- DeleteBox is idempotent (returns 200 OK even if box not found)

Risk Register
- MongoDB connection failures (500 errors)
- Invalid ObjectId format (FormatException)
- Cascade delete may fail partially (messages deleted but box not deleted)
- Concurrent updates to same box (race conditions)
- Large number of messages per box (performance)

Categories (mapping to test plan)
- HappyPath, EdgeCase, ErrorHandling, Integration, CascadeOperations

Exit Criteria
- Function list analyzed with inputs/returns/deps/branches/edge cases + ranking
- Coverage targets: ≥85% for ChatController
- Real API dependencies noted: MongoDB (Real connection required)
- All 5 endpoints identified with full test requirements

