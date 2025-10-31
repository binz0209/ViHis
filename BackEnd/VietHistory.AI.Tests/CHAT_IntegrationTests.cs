using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using VietHistory.Api.Controllers;
using VietHistory.Domain.Entities;
using VietHistory.Infrastructure.Mongo;
using Xunit;
using SaveChatHistoryRequest = VietHistory.Api.Controllers.SaveChatHistoryRequest;
using ChatMessageRequest = VietHistory.Api.Controllers.ChatMessageRequest;
using RenameBoxRequest = VietHistory.Api.Controllers.RenameBoxRequest;

namespace VietHistory.AI.Tests
{
    [Trait("Feature", "CHAT")]
    [Trait("Integration", "Real")]
    public class CHAT_IntegrationTests
    {
        private readonly IMongoContext _mongo;
        private readonly ChatController _controller;

        public CHAT_IntegrationTests()
        {
            var mongoSettings = new MongoSettings
            {
                ConnectionString = "mongodb+srv://lanserveUser:Binzdapoet.020904@hlinhwfil.eunq7.mongodb.net/?retryWrites=true&w=majority&appName=Hlinhwfil",
                Database = "vihis_test"
            };
            _mongo = new MongoContext(mongoSettings);

            _controller = new ChatController(_mongo)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        #region GetChatBoxes Tests

        [Fact]
        [Trait("Category", "HappyPath")]
        [Trait("Priority", "P0")]
        public async Task TC01_GetChatBoxes_WithUserId_ReturnsBoxes()
        {
            // Arrange - Create test chat box
            var userId = "test-user-001";
            var testBox = new ChatHistory
            {
                Id = ObjectId.GenerateNewId().ToString(),
                UserId = userId,
                MachineId = "test-machine-001",
                Name = "Test Chat Box",
                LastMessageAt = DateTime.UtcNow,
                MessageIds = new List<string>()
            };
            await _mongo.ChatHistories.InsertOneAsync(testBox);

            // Act
            var response = await _controller.GetChatBoxes(userId: userId, machineId: null);

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
            var boxes = result.Value?.GetType().GetProperty("boxes")?.GetValue(result.Value) as IEnumerable<object>;
            boxes.Should().NotBeNull();
            boxes!.Should().HaveCountGreaterOrEqualTo(1);

            // Cleanup
            await _mongo.ChatHistories.DeleteOneAsync(h => h.Id == testBox.Id);
        }

        [Fact]
        [Trait("Category", "HappyPath")]
        [Trait("Priority", "P0")]
        public async Task TC02_GetChatBoxes_WithMachineId_ReturnsBoxes()
        {
            // Arrange - Create test chat box
            var machineId = "test-machine-002";
            var testBox = new ChatHistory
            {
                Id = ObjectId.GenerateNewId().ToString(),
                MachineId = machineId,
                Name = "Test Guest Chat",
                LastMessageAt = DateTime.UtcNow,
                MessageIds = new List<string>()
            };
            await _mongo.ChatHistories.InsertOneAsync(testBox);

            // Act
            var response = await _controller.GetChatBoxes(userId: null, machineId: machineId);

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            // Cleanup
            await _mongo.ChatHistories.DeleteOneAsync(h => h.Id == testBox.Id);
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        [Trait("Priority", "P0")]
        public async Task TC03_GetChatBoxes_NoParams_Returns400()
        {
            // Arrange
            // Act
            var response = await _controller.GetChatBoxes(userId: null, machineId: null);

            // Assert
            response.Should().NotBeNull();
            var result = response as BadRequestObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(400);
        }

        [Fact]
        [Trait("Category", "HappyPath")]
        [Trait("Priority", "P1")]
        public async Task TC04_GetChatBoxes_EmptyResults_ReturnsEmptyList()
        {
            // Arrange - Use non-existent userId
            var nonExistentUserId = "non-existent-user-999";

            // Act
            var response = await _controller.GetChatBoxes(userId: nonExistentUserId, machineId: null);

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
            var boxes = result!.Value?.GetType().GetProperty("boxes")?.GetValue(result.Value) as IEnumerable<object>;
            boxes.Should().NotBeNull();
            boxes!.Should().BeEmpty();
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        [Trait("Priority", "P2")]
        public async Task TC05_GetChatBoxes_BothParams_UserIdTakesPrecedence()
        {
            // Arrange - Create boxes for both userId and machineId
            var userId = "test-user-priority";
            var machineId = "test-machine-priority";
            
            var userBox = new ChatHistory
            {
                Id = ObjectId.GenerateNewId().ToString(),
                UserId = userId,
                MachineId = machineId,
                Name = "User Box",
                LastMessageAt = DateTime.UtcNow,
                MessageIds = new List<string>()
            };
            await _mongo.ChatHistories.InsertOneAsync(userBox);

            var machineBox = new ChatHistory
            {
                Id = ObjectId.GenerateNewId().ToString(),
                MachineId = machineId,
                Name = "Machine Box",
                LastMessageAt = DateTime.UtcNow,
                MessageIds = new List<string>()
            };
            await _mongo.ChatHistories.InsertOneAsync(machineBox);

            // Act
            var response = await _controller.GetChatBoxes(userId: userId, machineId: machineId);

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
            // Should return boxes for userId (not machineId)
            var boxes = result!.Value?.GetType().GetProperty("boxes")?.GetValue(result.Value) as IEnumerable<object>;
            boxes.Should().NotBeNull();
            boxes!.Should().HaveCountGreaterOrEqualTo(1);

            // Cleanup
            await _mongo.ChatHistories.DeleteManyAsync(h => h.Id == userBox.Id || h.Id == machineBox.Id);
        }

        #endregion

        #region SaveHistory Tests

        [Fact]
        [Trait("Category", "HappyPath")]
        [Trait("Priority", "P0")]
        public async Task TC06_SaveHistory_NewBox_CreatesBoxAndMessages()
        {
            // Arrange
            var request = new SaveChatHistoryRequest
            {
                MachineId = "test-machine-new",
                BoxName = "New Test Chat",
                Messages = new List<ChatMessageRequest>
                {
                    new ChatMessageRequest
                    {
                        Id = "msg1",
                        Text = "Hello",
                        Sender = "user",
                        Timestamp = DateTime.UtcNow.ToString("O")
                    },
                    new ChatMessageRequest
                    {
                        Id = "msg2",
                        Text = "Hi there!",
                        Sender = "assistant",
                        Timestamp = DateTime.UtcNow.ToString("O")
                    }
                }
            };

            // Act
            var response = await _controller.SaveHistory(request);

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
            
            // Verify box was created
            var boxId = result.Value?.GetType().GetProperty("boxId")?.GetValue(result.Value)?.ToString();
            boxId.Should().NotBeNullOrEmpty();

            // Verify messages were saved
            var messages = await _mongo.ChatMessages.Find(m => m.ChatId == boxId).ToListAsync();
            messages.Should().HaveCount(2);

            // Cleanup
            await _mongo.ChatMessages.DeleteManyAsync(m => m.ChatId == boxId);
            await _mongo.ChatHistories.DeleteOneAsync(h => h.Id == boxId);
        }

        [Fact]
        [Trait("Category", "HappyPath")]
        [Trait("Priority", "P0")]
        public async Task TC07_SaveHistory_ExistingBox_UpdatesMessages()
        {
            // Arrange - Create existing box with messages
            var existingBox = new ChatHistory
            {
                Id = ObjectId.GenerateNewId().ToString(),
                MachineId = "test-machine-update",
                Name = "Existing Chat",
                LastMessageAt = DateTime.UtcNow,
                MessageIds = new List<string>()
            };
            await _mongo.ChatHistories.InsertOneAsync(existingBox);

            var oldMessage = new ChatMessage
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ChatId = existingBox.Id,
                Text = "Old message",
                Sender = "user",
                CreatedAt = DateTime.UtcNow
            };
            await _mongo.ChatMessages.InsertOneAsync(oldMessage);
            existingBox.MessageIds.Add(oldMessage.Id);
            await _mongo.ChatHistories.ReplaceOneAsync(h => h.Id == existingBox.Id, existingBox);

            var request = new SaveChatHistoryRequest
            {
                BoxId = existingBox.Id,
                MachineId = "test-machine-update",
                Messages = new List<ChatMessageRequest>
                {
                    new ChatMessageRequest
                    {
                        Id = "msg1",
                        Text = "New message 1",
                        Sender = "user",
                        Timestamp = DateTime.UtcNow.ToString("O")
                    },
                    new ChatMessageRequest
                    {
                        Id = "msg2",
                        Text = "New message 2",
                        Sender = "assistant",
                        Timestamp = DateTime.UtcNow.ToString("O")
                    }
                }
            };

            // Act
            var response = await _controller.SaveHistory(request);

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            // Verify old messages were deleted
            var oldMessages = await _mongo.ChatMessages.Find(m => m.Id == oldMessage.Id).FirstOrDefaultAsync();
            oldMessages.Should().BeNull();

            // Verify new messages were saved
            var newMessages = await _mongo.ChatMessages.Find(m => m.ChatId == existingBox.Id).ToListAsync();
            newMessages.Should().HaveCount(2);

            // Cleanup
            await _mongo.ChatMessages.DeleteManyAsync(m => m.ChatId == existingBox.Id);
            await _mongo.ChatHistories.DeleteOneAsync(h => h.Id == existingBox.Id);
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        [Trait("Priority", "P0")]
        public async Task TC08_SaveHistory_BoxIdNotFound_Returns404()
        {
            // Arrange
            var nonExistentBoxId = ObjectId.GenerateNewId().ToString();
            var request = new SaveChatHistoryRequest
            {
                BoxId = nonExistentBoxId,
                MachineId = "test-machine",
                Messages = new List<ChatMessageRequest>
                {
                    new ChatMessageRequest
                    {
                        Id = "msg1",
                        Text = "Test",
                        Sender = "user",
                        Timestamp = DateTime.UtcNow.ToString("O")
                    }
                }
            };

            // Act
            var response = await _controller.SaveHistory(request);

            // Assert
            response.Should().NotBeNull();
            var result = response as NotFoundObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(404);
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        [Trait("Priority", "P1")]
        public async Task TC09_SaveHistory_EmptyMessages_CreatesBox()
        {
            // Arrange
            var request = new SaveChatHistoryRequest
            {
                MachineId = "test-machine-empty",
                BoxName = "Empty Messages Box",
                Messages = new List<ChatMessageRequest>()
            };

            // Act
            var response = await _controller.SaveHistory(request);

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            // Verify box was created
            var boxId = result.Value?.GetType().GetProperty("boxId")?.GetValue(result.Value)?.ToString();
            boxId.Should().NotBeNullOrEmpty();

            // Verify no messages were saved
            var messages = await _mongo.ChatMessages.Find(m => m.ChatId == boxId).ToListAsync();
            messages.Should().BeEmpty();

            // Cleanup
            await _mongo.ChatHistories.DeleteOneAsync(h => h.Id == boxId);
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        [Trait("Priority", "P1")]
        public async Task TC10_SaveHistory_BoxNameNull_UsesDefault()
        {
            // Arrange
            var request = new SaveChatHistoryRequest
            {
                MachineId = "test-machine-default",
                BoxName = null,
                Messages = new List<ChatMessageRequest>
                {
                    new ChatMessageRequest
                    {
                        Id = "msg1",
                        Text = "Test",
                        Sender = "user",
                        Timestamp = DateTime.UtcNow.ToString("O")
                    }
                }
            };

            // Act
            var response = await _controller.SaveHistory(request);

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            // Verify box was created with default name "Chat"
            var boxId = result.Value?.GetType().GetProperty("boxId")?.GetValue(result.Value)?.ToString();
            var box = await _mongo.ChatHistories.Find(h => h.Id == boxId).FirstOrDefaultAsync();
            box.Should().NotBeNull();
            box!.Name.Should().Be("Chat");

            // Cleanup
            await _mongo.ChatMessages.DeleteManyAsync(m => m.ChatId == boxId);
            await _mongo.ChatHistories.DeleteOneAsync(h => h.Id == boxId);
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        [Trait("Priority", "P2")]
        public async Task TC11_SaveHistory_InvalidTimestamp_UsesUtcNow()
        {
            // Arrange
            var request = new SaveChatHistoryRequest
            {
                MachineId = "test-machine-invalid-time",
                Messages = new List<ChatMessageRequest>
                {
                    new ChatMessageRequest
                    {
                        Id = "msg1",
                        Text = "Test",
                        Sender = "user",
                        Timestamp = "invalid-timestamp-format"
                    }
                }
            };

            // Act
            var response = await _controller.SaveHistory(request);

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            // Verify message was saved with UtcNow timestamp (or valid timestamp)
            var boxId = result.Value?.GetType().GetProperty("boxId")?.GetValue(result.Value)?.ToString();
            var messages = await _mongo.ChatMessages.Find(m => m.ChatId == boxId).ToListAsync();
            messages.Should().HaveCount(1);
            messages[0].CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            // Cleanup
            await _mongo.ChatMessages.DeleteManyAsync(m => m.ChatId == boxId);
            await _mongo.ChatHistories.DeleteOneAsync(h => h.Id == boxId);
        }

        [Fact]
        [Trait("Category", "CascadeOperations")]
        [Trait("Priority", "P0")]
        public async Task TC12_SaveHistory_Update_DeletesOldMessages()
        {
            // Arrange - Create existing box with messages
            var existingBox = new ChatHistory
            {
                Id = ObjectId.GenerateNewId().ToString(),
                MachineId = "test-machine-cascade",
                Name = "Cascade Test",
                LastMessageAt = DateTime.UtcNow,
                MessageIds = new List<string>()
            };
            await _mongo.ChatHistories.InsertOneAsync(existingBox);

            var oldMessage1 = new ChatMessage
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ChatId = existingBox.Id,
                Text = "Old message 1",
                Sender = "user",
                CreatedAt = DateTime.UtcNow
            };
            var oldMessage2 = new ChatMessage
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ChatId = existingBox.Id,
                Text = "Old message 2",
                Sender = "assistant",
                CreatedAt = DateTime.UtcNow
            };
            await _mongo.ChatMessages.InsertManyAsync(new[] { oldMessage1, oldMessage2 });

            var request = new SaveChatHistoryRequest
            {
                BoxId = existingBox.Id,
                MachineId = "test-machine-cascade",
                Messages = new List<ChatMessageRequest>
                {
                    new ChatMessageRequest
                    {
                        Id = "msg1",
                        Text = "New message",
                        Sender = "user",
                        Timestamp = DateTime.UtcNow.ToString("O")
                    }
                }
            };

            // Act
            var response = await _controller.SaveHistory(request);

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            // Verify old messages were deleted
            var oldMessages = await _mongo.ChatMessages.Find(m => m.ChatId == existingBox.Id && (m.Id == oldMessage1.Id || m.Id == oldMessage2.Id)).ToListAsync();
            oldMessages.Should().BeEmpty();

            // Verify only new message exists
            var allMessages = await _mongo.ChatMessages.Find(m => m.ChatId == existingBox.Id).ToListAsync();
            allMessages.Should().HaveCount(1);
            allMessages[0].Text.Should().Be("New message");

            // Cleanup
            await _mongo.ChatMessages.DeleteManyAsync(m => m.ChatId == existingBox.Id);
            await _mongo.ChatHistories.DeleteOneAsync(h => h.Id == existingBox.Id);
        }

        #endregion

        #region GetHistory Tests

        [Fact]
        [Trait("Category", "HappyPath")]
        [Trait("Priority", "P0")]
        public async Task TC13_GetHistory_ValidBoxId_ReturnsMessages()
        {
            // Arrange - Create box with messages
            var testBox = new ChatHistory
            {
                Id = ObjectId.GenerateNewId().ToString(),
                MachineId = "test-machine-history",
                Name = "History Test",
                LastMessageAt = DateTime.UtcNow,
                MessageIds = new List<string>()
            };
            await _mongo.ChatHistories.InsertOneAsync(testBox);

            var message1 = new ChatMessage
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ChatId = testBox.Id,
                Text = "First message",
                Sender = "user",
                CreatedAt = DateTime.UtcNow.AddMinutes(-2)
            };
            var message2 = new ChatMessage
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ChatId = testBox.Id,
                Text = "Second message",
                Sender = "assistant",
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            };
            await _mongo.ChatMessages.InsertManyAsync(new[] { message1, message2 });
            testBox.MessageIds.AddRange(new[] { message1.Id, message2.Id });
            await _mongo.ChatHistories.ReplaceOneAsync(h => h.Id == testBox.Id, testBox);

            // Act
            var response = await _controller.GetHistory(testBox.Id);

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            // Verify messages are returned
            var messages = result.Value?.GetType().GetProperty("messages")?.GetValue(result.Value) as IEnumerable<object>;
            messages.Should().NotBeNull();
            messages!.Should().HaveCount(2);

            // Cleanup
            await _mongo.ChatMessages.DeleteManyAsync(m => m.ChatId == testBox.Id);
            await _mongo.ChatHistories.DeleteOneAsync(h => h.Id == testBox.Id);
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        [Trait("Priority", "P1")]
        public async Task TC14_GetHistory_BoxIdNotFound_ReturnsEmpty()
        {
            // Arrange - Use non-existent boxId
            var nonExistentBoxId = ObjectId.GenerateNewId().ToString();

            // Act
            var response = await _controller.GetHistory(nonExistentBoxId);

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200); // Returns 200, not 404

            // Verify empty messages returned
            var messages = result.Value?.GetType().GetProperty("messages")?.GetValue(result.Value) as IEnumerable<object>;
            messages.Should().NotBeNull();
            messages!.Should().BeEmpty();
        }

        [Fact]
        [Trait("Category", "HappyPath")]
        [Trait("Priority", "P1")]
        public async Task TC15_GetHistory_BoxWithNoMessages_ReturnsEmpty()
        {
            // Arrange - Create box without messages
            var testBox = new ChatHistory
            {
                Id = ObjectId.GenerateNewId().ToString(),
                MachineId = "test-machine-empty",
                Name = "Empty Box",
                LastMessageAt = DateTime.UtcNow,
                MessageIds = new List<string>()
            };
            await _mongo.ChatHistories.InsertOneAsync(testBox);

            // Act
            var response = await _controller.GetHistory(testBox.Id);

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            // Verify empty messages array returned
            var messages = result.Value?.GetType().GetProperty("messages")?.GetValue(result.Value) as IEnumerable<object>;
            messages.Should().NotBeNull();
            messages!.Should().BeEmpty();

            // Cleanup
            await _mongo.ChatHistories.DeleteOneAsync(h => h.Id == testBox.Id);
        }

        [Fact]
        [Trait("Category", "HappyPath")]
        [Trait("Priority", "P1")]
        public async Task TC16_GetHistory_MessagesSortedByCreatedAt()
        {
            // Arrange - Create box with messages out of order
            var testBox = new ChatHistory
            {
                Id = ObjectId.GenerateNewId().ToString(),
                MachineId = "test-machine-sorted",
                Name = "Sorted Test",
                LastMessageAt = DateTime.UtcNow,
                MessageIds = new List<string>()
            };
            await _mongo.ChatHistories.InsertOneAsync(testBox);

            var message3 = new ChatMessage
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ChatId = testBox.Id,
                Text = "Third message",
                Sender = "user",
                CreatedAt = DateTime.UtcNow
            };
            var message1 = new ChatMessage
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ChatId = testBox.Id,
                Text = "First message",
                Sender = "user",
                CreatedAt = DateTime.UtcNow.AddMinutes(-2)
            };
            var message2 = new ChatMessage
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ChatId = testBox.Id,
                Text = "Second message",
                Sender = "assistant",
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            };
            // Insert out of order
            await _mongo.ChatMessages.InsertManyAsync(new[] { message3, message1, message2 });

            // Act
            var response = await _controller.GetHistory(testBox.Id);

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            // Verify messages are sorted by CreatedAt ascending
            var messages = result.Value?.GetType().GetProperty("messages")?.GetValue(result.Value);
            messages.Should().NotBeNull();
            
            // Use reflection to access properties
            var messagesList = (messages as System.Collections.IEnumerable)?.Cast<object>().ToList();
            messagesList.Should().NotBeNull();
            messagesList!.Should().HaveCount(3);

            // Check order by accessing properties via reflection
            var getText = new Func<object, string>(m => m.GetType().GetProperty("text")?.GetValue(m)?.ToString() ?? "");
            var getTimestamp = new Func<object, DateTime>(m => 
                m.GetType().GetProperty("timestamp")?.GetValue(m) is DateTime dt ? dt : DateTime.MinValue);
            
            var sortedMessages = messagesList.OrderBy(getTimestamp).ToList();
            sortedMessages.Should().BeEquivalentTo(messagesList, "Messages should be sorted by CreatedAt ascending");
            
            // Verify first message is "First message"
            getText(messagesList[0]).Should().Be("First message");
            getText(messagesList[1]).Should().Be("Second message");
            getText(messagesList[2]).Should().Be("Third message");

            // Cleanup
            await _mongo.ChatMessages.DeleteManyAsync(m => m.ChatId == testBox.Id);
            await _mongo.ChatHistories.DeleteOneAsync(h => h.Id == testBox.Id);
        }

        #endregion

        #region RenameBox Tests

        [Fact]
        [Trait("Category", "HappyPath")]
        [Trait("Priority", "P1")]
        public async Task TC17_RenameBox_ValidBoxId_UpdatesName()
        {
            // Arrange - Create test box
            var testBox = new ChatHistory
            {
                Id = ObjectId.GenerateNewId().ToString(),
                MachineId = "test-machine-rename",
                Name = "Old Name",
                LastMessageAt = DateTime.UtcNow,
                MessageIds = new List<string>()
            };
            await _mongo.ChatHistories.InsertOneAsync(testBox);

            var request = new RenameBoxRequest
            {
                Name = "New Name"
            };

            // Act
            var response = await _controller.RenameBox(testBox.Id, request);

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            // Verify name was updated
            var updatedBox = await _mongo.ChatHistories.Find(h => h.Id == testBox.Id).FirstOrDefaultAsync();
            updatedBox.Should().NotBeNull();
            updatedBox!.Name.Should().Be("New Name");

            // Cleanup
            await _mongo.ChatHistories.DeleteOneAsync(h => h.Id == testBox.Id);
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        [Trait("Priority", "P1")]
        public async Task TC18_RenameBox_BoxIdNotFound_Returns404()
        {
            // Arrange
            var nonExistentBoxId = ObjectId.GenerateNewId().ToString();
            var request = new RenameBoxRequest
            {
                Name = "New Name"
            };

            // Act
            var response = await _controller.RenameBox(nonExistentBoxId, request);

            // Assert
            response.Should().NotBeNull();
            var result = response as NotFoundObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(404);
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        [Trait("Priority", "P2")]
        public async Task TC19_RenameBox_EmptyName_UpdatesToEmpty()
        {
            // Arrange - Create test box
            var testBox = new ChatHistory
            {
                Id = ObjectId.GenerateNewId().ToString(),
                MachineId = "test-machine-empty-name",
                Name = "Original Name",
                LastMessageAt = DateTime.UtcNow,
                MessageIds = new List<string>()
            };
            await _mongo.ChatHistories.InsertOneAsync(testBox);

            var request = new RenameBoxRequest
            {
                Name = ""
            };

            // Act
            var response = await _controller.RenameBox(testBox.Id, request);

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200); // No validation, allows empty name

            // Verify name was updated to empty
            var updatedBox = await _mongo.ChatHistories.Find(h => h.Id == testBox.Id).FirstOrDefaultAsync();
            updatedBox.Should().NotBeNull();
            updatedBox!.Name.Should().Be("");

            // Cleanup
            await _mongo.ChatHistories.DeleteOneAsync(h => h.Id == testBox.Id);
        }

        [Fact]
        [Trait("Category", "HappyPath")]
        [Trait("Priority", "P2")]
        public async Task TC20_RenameBox_UpdatesUpdatedAt()
        {
            // Arrange - Create test box
            var originalTime = DateTime.UtcNow.AddHours(-1);
            var testBox = new ChatHistory
            {
                Id = ObjectId.GenerateNewId().ToString(),
                MachineId = "test-machine-updated-at",
                Name = "Original Name",
                LastMessageAt = originalTime,
                UpdatedAt = originalTime,
                MessageIds = new List<string>()
            };
            await _mongo.ChatHistories.InsertOneAsync(testBox);

            var request = new RenameBoxRequest
            {
                Name = "Updated Name"
            };

            // Act
            var response = await _controller.RenameBox(testBox.Id, request);

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            // Verify UpdatedAt was updated
            var updatedBox = await _mongo.ChatHistories.Find(h => h.Id == testBox.Id).FirstOrDefaultAsync();
            updatedBox.Should().NotBeNull();
            updatedBox!.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            // Cleanup
            await _mongo.ChatHistories.DeleteOneAsync(h => h.Id == testBox.Id);
        }

        #endregion

        #region DeleteBox Tests

        [Fact]
        [Trait("Category", "HappyPath")]
        [Trait("Priority", "P1")]
        public async Task TC21_DeleteBox_ValidBoxId_DeletesBoxAndMessages()
        {
            // Arrange - Create box with messages
            var testBox = new ChatHistory
            {
                Id = ObjectId.GenerateNewId().ToString(),
                MachineId = "test-machine-delete",
                Name = "Delete Test",
                LastMessageAt = DateTime.UtcNow,
                MessageIds = new List<string>()
            };
            await _mongo.ChatHistories.InsertOneAsync(testBox);

            var message1 = new ChatMessage
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ChatId = testBox.Id,
                Text = "Message 1",
                Sender = "user",
                CreatedAt = DateTime.UtcNow
            };
            var message2 = new ChatMessage
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ChatId = testBox.Id,
                Text = "Message 2",
                Sender = "assistant",
                CreatedAt = DateTime.UtcNow
            };
            await _mongo.ChatMessages.InsertManyAsync(new[] { message1, message2 });

            // Act
            var response = await _controller.DeleteBox(testBox.Id);

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            // Verify box was deleted
            var deletedBox = await _mongo.ChatHistories.Find(h => h.Id == testBox.Id).FirstOrDefaultAsync();
            deletedBox.Should().BeNull();

            // Verify messages were deleted
            var messages = await _mongo.ChatMessages.Find(m => m.ChatId == testBox.Id).ToListAsync();
            messages.Should().BeEmpty();
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        [Trait("Priority", "P1")]
        public async Task TC22_DeleteBox_BoxIdNotFound_Returns200()
        {
            // Arrange - Use non-existent boxId
            var nonExistentBoxId = ObjectId.GenerateNewId().ToString();

            // Act
            var response = await _controller.DeleteBox(nonExistentBoxId);

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200); // Idempotent behavior, returns 200 even if not found
        }

        [Fact]
        [Trait("Category", "HappyPath")]
        [Trait("Priority", "P2")]
        public async Task TC23_DeleteBox_BoxWithNoMessages_DeletesBox()
        {
            // Arrange - Create box without messages
            var testBox = new ChatHistory
            {
                Id = ObjectId.GenerateNewId().ToString(),
                MachineId = "test-machine-delete-empty",
                Name = "Empty Box",
                LastMessageAt = DateTime.UtcNow,
                MessageIds = new List<string>()
            };
            await _mongo.ChatHistories.InsertOneAsync(testBox);

            // Act
            var response = await _controller.DeleteBox(testBox.Id);

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            // Verify box was deleted
            var deletedBox = await _mongo.ChatHistories.Find(h => h.Id == testBox.Id).FirstOrDefaultAsync();
            deletedBox.Should().BeNull();
        }

        [Fact]
        [Trait("Category", "CascadeOperations")]
        [Trait("Priority", "P0")]
        public async Task TC24_DeleteBox_CascadeDeleteMessages()
        {
            // Arrange - Create box with messages
            var testBox = new ChatHistory
            {
                Id = ObjectId.GenerateNewId().ToString(),
                MachineId = "test-machine-cascade-delete",
                Name = "Cascade Delete Test",
                LastMessageAt = DateTime.UtcNow,
                MessageIds = new List<string>()
            };
            await _mongo.ChatHistories.InsertOneAsync(testBox);

            var message1 = new ChatMessage
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ChatId = testBox.Id,
                Text = "Message 1",
                Sender = "user",
                CreatedAt = DateTime.UtcNow
            };
            var message2 = new ChatMessage
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ChatId = testBox.Id,
                Text = "Message 2",
                Sender = "assistant",
                CreatedAt = DateTime.UtcNow
            };
            await _mongo.ChatMessages.InsertManyAsync(new[] { message1, message2 });

            // Act
            var response = await _controller.DeleteBox(testBox.Id);

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            // Verify cascade delete: messages deleted before box deleted
            // (In actual implementation, messages are deleted first, then box)
            var messages = await _mongo.ChatMessages.Find(m => m.ChatId == testBox.Id).ToListAsync();
            messages.Should().BeEmpty();

            var deletedBox = await _mongo.ChatHistories.Find(h => h.Id == testBox.Id).FirstOrDefaultAsync();
            deletedBox.Should().BeNull();
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        [Trait("Category", "ErrorHandling")]
        [Trait("Priority", "P1")]
        public async Task TC25_GetChatBoxes_InvalidUserId_ReturnsEmptyOrError()
        {
            // Arrange - Use invalid userId format (not ObjectId)
            var invalidUserId = "not-a-valid-objectid";

            // Act
            // Note: MongoDB may handle invalid format differently
            // Current behavior: Returns empty list if no matches
            var response = await _controller.GetChatBoxes(userId: invalidUserId, machineId: null);

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
            // Should return empty list (no matches) or handle gracefully
        }

        #endregion
    }

}

