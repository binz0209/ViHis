# Mock Data Directory

Thư mục này chứa **mock data files** để sử dụng trong unit tests và integration tests.

## 📁 Structure

```
mock/
├── README.md                    # File này
├── mongodb/
│   ├── chunks.json             # Sample ChunkDoc data
│   └── sources.json            # Sample SourceDoc data
├── gemini/
│   ├── valid-response.json     # Gemini API success response
│   ├── empty-response.json     # Gemini API với empty candidates
│   └── error-responses.json    # Gemini API error scenarios
└── web/
    ├── wikipedia-search.json   # Wikipedia search API response
    └── google-search.json      # Google Custom Search API response
```

## 🎯 Mục đích

### 1. **Centralized Mock Data**
- Thay vì hardcode mock data trong test files
- Dễ dàng update khi API format thay đổi

### 2. **Reusability**
- Reuse mock data across multiple tests
- Giảm code duplication

### 3. **Maintainability**
- Một chỗ để quản lý tất cả mock data
- Dễ dàng version control

### 4. **Professional Testing Structure**
- Theo best practices của industry
- Separation of concerns: test logic vs test data

## 📖 How to Use

### C# Test Code Example:

```csharp
using System.IO;
using System.Text.Json;

public class GeminiStudyServiceTests
{
    private string LoadMockData(string fileName)
    {
        var path = Path.Combine("../../../tests/mock", fileName);
        return File.ReadAllText(path);
    }

    [Fact]
    public async Task TC01_AskAsync_WithMongoDBContext_ReturnsValidAnswer()
    {
        // Load mock data from file
        var chunksJson = LoadMockData("mongodb/chunks.json");
        var chunks = JsonSerializer.Deserialize<List<ChunkDoc>>(chunksJson);
        
        // Use in test...
    }
}
```

## 🔗 References

- **Test Files**: `BackEnd/VietHistory.AI.Tests/GeminiStudyServiceTests.cs`
- **Mock Data Spec**: `tests/02-test-cases.md` (section "MOCK DATA REFERENCE")
- **Tutorial**: https://tamttt14.github.io/AI4SEProject/index.html

---

**Created**: October 24, 2025  
**Purpose**: AI-assisted unit testing for VietHistory AI project

