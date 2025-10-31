# CHAT – Phase 2: Design (Result)

Date: 2025-01-XX

Artifacts Produced
- Test Matrix: `tests/CHAT_TEST_PLAN.md` with ~25 Given–When–Then cases across categories:
  - HappyPath (TC01-TC10), EdgeCase (TC11-TC16), ErrorHandling (TC17-TC20), Integration (TC21-TC25)
- Traits Standard (to apply in all tests):
  - `[Trait("Feature", "CHAT")]`
  - `[Trait("Category", "HappyPath|EdgeCase|ErrorHandling|Integration|CascadeOperations")]`
  - `[Trait("Priority", "P0|P1|P2")]`
  - `[Trait("Integration", "Real")]` (for integration tests)

Global Prompt Applied (GWT Best Practice)
- Input: Function list from Phase 1 (5 endpoints)
- Output: GWT cases below (selected highlights; full list in `tests/CHAT_TEST_PLAN.md`).

GWT Cases (selected, per function)

1) ChatController.GetChatBoxes
- Given userId exists; When GET /api/v1/chat/boxes?userId={id}; Then 200 with list of boxes sorted by lastMessageAt desc. [HappyPath,P0]
- Given machineId exists; When GET /api/v1/chat/boxes?machineId={id}; Then 200 with list of boxes sorted by lastMessageAt desc. [HappyPath,P0]
- Given no userId or machineId; When GET /api/v1/chat/boxes; Then 400 BadRequest. [EdgeCase,P0]
- Given userId with no boxes; When GET boxes; Then 200 with empty list. [HappyPath,P1]
- Given both userId and machineId; When GET boxes; Then userId takes precedence. [EdgeCase,P2]
- Given MongoDB unavailable; When GET boxes; Then 500 InternalServerError. [ErrorHandling,P1]

2) ChatController.SaveHistory
- Given new box request (BoxId null); When POST /api/v1/chat/history; Then 200 with new boxId and messages saved. [HappyPath,P0]
- Given existing boxId; When POST /api/v1/chat/history with BoxId; Then 200 with old messages deleted, new messages saved. [HappyPath,P0]
- Given invalid boxId; When POST /api/v1/chat/history; Then 404 NotFound. [EdgeCase,P0]
- Given empty messages list; When POST /api/v1/chat/history; Then 200 with box created/updated with 0 messages. [EdgeCase,P1]
- Given invalid timestamp format; When POST /api/v1/chat/history; Then uses UtcNow for timestamp. [EdgeCase,P2]
- Given BoxName null; When POST /api/v1/chat/history; Then uses "Chat" as default name. [EdgeCase,P1]
- Given update existing box; When POST /api/v1/chat/history; Then old messages are deleted before new ones inserted. [CascadeOperations,P0]
- Given MongoDB write failure; When POST /api/v1/chat/history; Then 500 InternalServerError. [ErrorHandling,P1]

3) ChatController.GetHistory
- Given valid boxId with messages; When GET /api/v1/chat/history/{boxId}; Then 200 with box info and messages sorted by CreatedAt. [HappyPath,P0]
- Given boxId not found; When GET /api/v1/chat/history/{boxId}; Then 200 with empty messages (not 404). [EdgeCase,P1]
- Given boxId with no messages; When GET /api/v1/chat/history/{boxId}; Then 200 with empty messages array. [HappyPath,P1]
- Given messages out of order; When GET /api/v1/chat/history/{boxId}; Then messages sorted by CreatedAt ascending. [HappyPath,P1]
- Given invalid boxId format; When GET /api/v1/chat/history/{boxId}; Then handles gracefully (may throw FormatException). [ErrorHandling,P2]
- Given MongoDB unavailable; When GET /api/v1/chat/history/{boxId}; Then 500 InternalServerError. [ErrorHandling,P1]

4) ChatController.RenameBox
- Given valid boxId; When PUT /api/v1/chat/history/{boxId}/name; Then 200 with name updated. [HappyPath,P1]
- Given boxId not found; When PUT /api/v1/chat/history/{boxId}/name; Then 404 NotFound. [EdgeCase,P1]
- Given empty name; When PUT /api/v1/chat/history/{boxId}/name; Then updates name to empty (no validation). [EdgeCase,P2]
- Given update; When PUT /api/v1/chat/history/{boxId}/name; Then UpdatedAt is updated. [HappyPath,P2]
- Given MongoDB write failure; When PUT /api/v1/chat/history/{boxId}/name; Then 500 InternalServerError. [ErrorHandling,P1]

5) ChatController.DeleteBox
- Given valid boxId with messages; When DELETE /api/v1/chat/history/{boxId}; Then 200 with box and all messages deleted. [HappyPath,P1]
- Given boxId not found; When DELETE /api/v1/chat/history/{boxId}; Then 200 OK (idempotent, no error). [EdgeCase,P1]
- Given boxId with no messages; When DELETE /api/v1/chat/history/{boxId}; Then 200 with box deleted. [HappyPath,P2]
- Given cascade delete; When DELETE /api/v1/chat/history/{boxId}; Then all messages deleted before box deleted. [CascadeOperations,P0]
- Given MongoDB delete failure; When DELETE /api/v1/chat/history/{boxId}; Then 500 InternalServerError. [ErrorHandling,P1]

Full Matrix (25 test cases)

| Category | TC Name (TCxx_) | Given | When | Then | Traits |
|---|---|---|---|---|---|
| HappyPath | TC01_GetChatBoxes_WithUserId_ReturnsBoxes | userId exists | GET /api/v1/chat/boxes?userId={id} | 200 + boxes sorted desc | Feature=CHAT; P0; Integration=Real |
| HappyPath | TC02_GetChatBoxes_WithMachineId_ReturnsBoxes | machineId exists | GET /api/v1/chat/boxes?machineId={id} | 200 + boxes sorted desc | Feature=CHAT; P0; Integration=Real |
| EdgeCase | TC03_GetChatBoxes_NoParams_Returns400 | no userId or machineId | GET /api/v1/chat/boxes | 400 BadRequest | Feature=CHAT; P0; Integration=Real |
| HappyPath | TC04_GetChatBoxes_EmptyResults_ReturnsEmptyList | userId with no boxes | GET /api/v1/chat/boxes?userId={id} | 200 + empty list | Feature=CHAT; P1; Integration=Real |
| EdgeCase | TC05_GetChatBoxes_BothParams_UserIdTakesPrecedence | both userId and machineId | GET /api/v1/chat/boxes?userId={id}&machineId={id2} | 200 + boxes for userId | Feature=CHAT; P2; Integration=Real |
| HappyPath | TC06_SaveHistory_NewBox_CreatesBoxAndMessages | BoxId null, messages provided | POST /api/v1/chat/history | 200 + new boxId | Feature=CHAT; P0; Integration=Real |
| HappyPath | TC07_SaveHistory_ExistingBox_UpdatesMessages | existing BoxId | POST /api/v1/chat/history | 200 + old messages deleted, new saved | Feature=CHAT; P0; Integration=Real |
| EdgeCase | TC08_SaveHistory_BoxIdNotFound_Returns404 | invalid BoxId | POST /api/v1/chat/history | 404 NotFound | Feature=CHAT; P0; Integration=Real |
| EdgeCase | TC09_SaveHistory_EmptyMessages_CreatesBox | empty messages list | POST /api/v1/chat/history | 200 + box with 0 messages | Feature=CHAT; P1; Integration=Real |
| EdgeCase | TC10_SaveHistory_BoxNameNull_UsesDefault | BoxName null | POST /api/v1/chat/history | 200 + name = "Chat" | Feature=CHAT; P1; Integration=Real |
| EdgeCase | TC11_SaveHistory_InvalidTimestamp_UsesUtcNow | invalid timestamp format | POST /api/v1/chat/history | 200 + uses UtcNow | Feature=CHAT; P2; Integration=Real |
| CascadeOperations | TC12_SaveHistory_Update_DeletesOldMessages | update existing box | POST /api/v1/chat/history | old messages deleted first | Feature=CHAT; P0; Integration=Real |
| HappyPath | TC13_GetHistory_ValidBoxId_ReturnsMessages | boxId with messages | GET /api/v1/chat/history/{boxId} | 200 + messages sorted | Feature=CHAT; P0; Integration=Real |
| EdgeCase | TC14_GetHistory_BoxIdNotFound_ReturnsEmpty | boxId not found | GET /api/v1/chat/history/{boxId} | 200 + empty messages (not 404) | Feature=CHAT; P1; Integration=Real |
| HappyPath | TC15_GetHistory_BoxWithNoMessages_ReturnsEmpty | boxId with no messages | GET /api/v1/chat/history/{boxId} | 200 + empty array | Feature=CHAT; P1; Integration=Real |
| HappyPath | TC16_GetHistory_MessagesSortedByCreatedAt | messages out of order | GET /api/v1/chat/history/{boxId} | 200 + sorted by CreatedAt asc | Feature=CHAT; P1; Integration=Real |
| HappyPath | TC17_RenameBox_ValidBoxId_UpdatesName | valid boxId | PUT /api/v1/chat/history/{boxId}/name | 200 + name updated | Feature=CHAT; P1; Integration=Real |
| EdgeCase | TC18_RenameBox_BoxIdNotFound_Returns404 | boxId not found | PUT /api/v1/chat/history/{boxId}/name | 404 NotFound | Feature=CHAT; P1; Integration=Real |
| EdgeCase | TC19_RenameBox_EmptyName_UpdatesToEmpty | empty name | PUT /api/v1/chat/history/{boxId}/name | 200 + empty name (no validation) | Feature=CHAT; P2; Integration=Real |
| HappyPath | TC20_RenameBox_UpdatesUpdatedAt | valid boxId | PUT /api/v1/chat/history/{boxId}/name | UpdatedAt field updated | Feature=CHAT; P2; Integration=Real |
| HappyPath | TC21_DeleteBox_ValidBoxId_DeletesBoxAndMessages | valid boxId with messages | DELETE /api/v1/chat/history/{boxId} | 200 + box and messages deleted | Feature=CHAT; P1; Integration=Real |
| EdgeCase | TC22_DeleteBox_BoxIdNotFound_Returns200 | boxId not found | DELETE /api/v1/chat/history/{boxId} | 200 OK (idempotent) | Feature=CHAT; P1; Integration=Real |
| HappyPath | TC23_DeleteBox_BoxWithNoMessages_DeletesBox | boxId with no messages | DELETE /api/v1/chat/history/{boxId} | 200 + box deleted | Feature=CHAT; P2; Integration=Real |
| CascadeOperations | TC24_DeleteBox_CascadeDeleteMessages | valid boxId | DELETE /api/v1/chat/history/{boxId} | messages deleted before box | Feature=CHAT; P0; Integration=Real |
| ErrorHandling | TC25_SaveHistory_MongoDBFailure_Returns500 | MongoDB unavailable | POST /api/v1/chat/history | 500 InternalServerError | Feature=CHAT; P1; Integration=Real |

Assertions & Idempotence
- Prefer structural patterns: count, non-empty, contains, sorted order
- Integration tests use Real MongoDB and clean up created data
- Tests are idempotent (can run multiple times without side effects)
- Edge cases cover null/empty handling, validation errors, and MongoDB failures

Exit Criteria
- Test matrix created with 25 GWT cases covering all functions from Phase 1
- Traits assigned correctly (Feature, Category, Priority, Integration)
- Test names follow TCxx_Given_When_Then format
- All branches and edge cases covered

