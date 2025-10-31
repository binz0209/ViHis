# CHAT Test Plan

## Phase 1 – Testing Specification (Analysis & Selection)

### Functions to Test (Ranked)

#### 1. ChatController.GetChatBoxes(userId?, machineId?)
   - **Main purpose**: Retrieve list of chat boxes for a user or machine, sorted by last message time
   - **Inputs**: Optional `userId` string, optional `machineId` string
   - **Returns**: `ActionResult` with list of boxes { id, name, lastMessageAt, messageCount }
   - **Dependencies**: `IMongoContext.ChatHistories`, MongoDB
   - **Logic branches**: 
     - userId provided → Filter by userId
     - machineId provided → Filter by machineId
     - Both null/empty → 400 BadRequest
     - Results sorted by LastMessageAt descending
   - **Edge cases**: 
     - Both userId and machineId null → 400 BadRequest
     - Both provided → userId takes precedence
     - No boxes found → Empty list (200 OK)
     - Invalid userId/machineId format
   - **Test type**: Integration (Real MongoDB)
   - **Suggested Test Names**: TC01_GetChatBoxes_WithUserId_ReturnsBoxes, TC02_GetChatBoxes_WithMachineId_ReturnsBoxes, TC03_GetChatBoxes_NoParams_Returns400

#### 2. ChatController.SaveHistory(request: SaveChatHistoryRequest)
   - **Main purpose**: Save chat history for a box (create new if BoxId null, update existing if BoxId provided)
   - **Inputs**: `SaveChatHistoryRequest` { MachineId, UserId?, BoxId?, BoxName?, Messages[] }
   - **Returns**: `ActionResult` with { success, boxId, message }
   - **Dependencies**: `IMongoContext.ChatHistories`, `IMongoContext.ChatMessages`, MongoDB
   - **Logic branches**:
     - BoxId null/empty → Create new ChatHistory → Insert messages → Return new boxId
     - BoxId provided → Find existing → Delete old messages → Insert new messages → Update ChatHistory
     - BoxName null → Use "Chat" as default
     - Timestamp parsing: TryParse msg.Timestamp, fallback to UtcNow
   - **Edge cases**:
     - BoxId null → Create new box
     - BoxId provided but not found → 404 NotFound
     - Empty messages list → Creates box with 0 messages
     - Invalid timestamp format → Uses UtcNow
   - **Test type**: Integration (Real MongoDB)
   - **Suggested Test Names**: TC01_SaveHistory_NewBox_CreatesBoxAndMessages, TC02_SaveHistory_ExistingBox_UpdatesMessages, TC03_SaveHistory_BoxIdNotFound_Returns404

#### 3. ChatController.GetHistory(boxId: string)
   - **Main purpose**: Retrieve chat history (messages) for a specific box
   - **Inputs**: `boxId` string (path parameter)
   - **Returns**: `ActionResult` with { boxId, name, messages[] }
   - **Dependencies**: `IMongoContext.ChatHistories`, `IMongoContext.ChatMessages`, MongoDB
   - **Logic branches**:
     - BoxId found → Return box info + sorted messages
     - BoxId not found → Return { boxId, name: "Chat", messages: [] } (200 OK, not 404)
     - Messages sorted by CreatedAt ascending
   - **Edge cases**:
     - BoxId not found → Returns empty result (200 OK, not 404)
     - BoxId with no messages → Returns empty messages array
     - Messages out of order → Should be sorted by CreatedAt
   - **Test type**: Integration (Real MongoDB)
   - **Suggested Test Names**: TC01_GetHistory_ValidBoxId_ReturnsMessages, TC02_GetHistory_BoxIdNotFound_ReturnsEmpty

#### 4. ChatController.RenameBox(boxId: string, request: RenameBoxRequest)
   - **Main purpose**: Rename a chat box
   - **Inputs**: `boxId` string (path parameter), `RenameBoxRequest` { Name }
   - **Returns**: `ActionResult` with { success, message }
   - **Dependencies**: `IMongoContext.ChatHistories`, MongoDB
   - **Logic branches**:
     - BoxId found → Update Name → Update UpdatedAt → Replace document → 200 OK
     - BoxId not found → 404 NotFound
   - **Edge cases**: BoxId not found → 404, Empty Name → No validation (allows empty)
   - **Test type**: Integration (Real MongoDB)
   - **Suggested Test Names**: TC01_RenameBox_ValidBoxId_UpdatesName, TC02_RenameBox_BoxIdNotFound_Returns404

#### 5. ChatController.DeleteBox(boxId: string)
   - **Main purpose**: Delete a chat box and all its messages (cascade delete)
   - **Inputs**: `boxId` string (path parameter)
   - **Returns**: `ActionResult` with { success, message }
   - **Dependencies**: `IMongoContext.ChatHistories`, `IMongoContext.ChatMessages`, MongoDB
   - **Logic branches**:
     - BoxId found → Delete all ChatMessages → Delete ChatHistory → 200 OK
     - BoxId not found → Returns 200 OK (idempotent behavior)
   - **Edge cases**: BoxId not found → Returns 200 OK (idempotent), Cascade delete: All messages deleted before box
   - **Test type**: Integration (Real MongoDB)
   - **Suggested Test Names**: TC01_DeleteBox_ValidBoxId_DeletesBoxAndMessages, TC02_DeleteBox_BoxIdNotFound_Returns200, TC03_DeleteBox_CascadeDeleteMessages

### Prioritization
- **High Priority (P0)**: GetChatBoxes, SaveHistory, GetHistory
- **Medium Priority (P1)**: RenameBox, DeleteBox

### Acceptance (Phase 1)
- ✅ 5 endpoints identified with inputs/returns/deps/branches/edge cases
- ✅ Coverage targets: ≥85% for ChatController
- ✅ Real API dependencies noted: MongoDB (Real connection required)
- ✅ All endpoints fully analyzed with test requirements

