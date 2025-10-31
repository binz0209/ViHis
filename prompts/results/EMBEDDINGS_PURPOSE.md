# EMBEDDINGS Feature - Tác dụng và Vai trò trong ViHis

## 🎯 Tác dụng chính

**EMBEDDINGS** feature có tác dụng **chuyển đổi text thành vector representations** để hỗ trợ **Vector Search (Semantic Search)** trong RAG (Retrieval-Augmented Generation) system.

---

## 📋 Embeddings là gì?

**Embeddings** = Vector representations của text (mảng số float, thường 768 dimensions)

**Ví dụ**:
- Text: "Trận Bạch Đằng năm 938"
- Embedding: `[0.123, -0.456, 0.789, ..., 0.234]` (768 số float)

**Tính chất**:
- ✅ Texts tương tự → Embeddings tương tự (cosine similarity cao)
- ✅ Texts khác nhau → Embeddings khác nhau (cosine similarity thấp)
- ✅ Dùng để tìm chunks **semantically similar** với câu hỏi

---

## 🔄 Vai trò trong ViHis System

### Workflow tổng quan:

```
1. TEXT_INGEST: PDF → Text → Chunks → Embeddings (optional)
                        ↓
2. EMBEDDINGS (Admin): Generate embeddings cho chunks chưa có
                        ↓
3. AI_QA: User hỏi → Generate query embedding → Vector Search → Retrieve similar chunks → RAG → Answer
```

### Chi tiết từng bước:

#### 1️⃣ TEXT_INGEST - Tạo Chunks

**Flow**:
```
PDF File
  ↓
PdfTextExtractor → Extract text
  ↓
TextNormalizer → Clean text
  ↓
ChunkPack → Split thành chunks
  ↓
FallbackAIngestor → Generate embeddings (optional) → Save to MongoDB
```

**Kết quả**:
- ✅ Chunks được lưu vào MongoDB với `content` (text)
- ⚠️ `embedding` field có thể **null** hoặc **có giá trị** (tùy vào có gọi EmbeddingService không)

#### 2️⃣ EMBEDDINGS (Admin) - Generate Embeddings cho Existing Chunks

**Tác dụng**: **Migration/Backfill** - Generate embeddings cho chunks đã có trong DB nhưng chưa có embeddings.

**Endpoints**:
- `POST /api/v1/admin/embeddings/generate-all` - Generate embeddings cho tất cả chunks chưa có
- `GET /api/v1/admin/embeddings/status` - Check embedding status (% chunks có embeddings)
- `DELETE /api/v1/admin/embeddings/all` - Xóa tất cả embeddings (để regenerate)

**Flow**:
```
1. Query MongoDB: Find chunks WHERE embedding IS NULL
2. For each chunk:
   - Call EmbeddingService.GenerateEmbeddingAsync(chunk.Content)
   - EmbeddingService calls Gemini API: text-embedding-004
   - Gemini returns: 768-dimension vector
   - Update chunk: SET embedding = [vector]
3. Repeat cho tất cả chunks
```

**Use case**:
- ✅ Khi TEXT_INGEST chưa generate embeddings (vì chưa có API key hoặc muốn tách riêng)
- ✅ Khi cần regenerate embeddings với model mới
- ✅ Khi migrate data từ text-only sang vector-enabled

#### 3️⃣ AI_QA - Vector Search trong RAG

**Flow**:
```
User Question: "Trận Bạch Đằng năm nào?"
  ↓
KWideRetriever.GetKWideChunksAsync(question)
  ↓
Generate Query Embedding (EmbeddingService.GenerateQueryEmbeddingAsync)
  ↓
Vector Search (MongoDB $vectorSearch)
  ↓
Find chunks with similar embeddings (cosine similarity)
  ↓
Retrieve top K chunks
  ↓
Send to Gemini API với context
  ↓
Return answer
```

**Ưu điểm của Vector Search so với Text Search**:
- ✅ **Semantic Search**: Hiểu nghĩa, không chỉ keywords
  - Ví dụ: "năm nào" có thể match "938", "năm 938", "năm ấy", etc.
- ✅ **Better accuracy**: Tìm chunks liên quan hơn so với keyword matching
- ✅ **Multilingual**: Works với Vietnamese, English, French

---

## 📊 So sánh: Text Search vs Vector Search

### Text Search (MongoDB Text Index)

**Cách hoạt động**:
- Tìm chunks chứa **exact keywords** trong question
- Match theo **text matching** (word-by-word)
- Fallback: Regex search nếu không có text index

**Ví dụ**:
- Question: "Trận Bạch Đằng năm nào?"
- Text Search: Tìm chunks chứa keywords "Trận", "Bạch", "Đằng", "năm"
- **Vấn đề**: Có thể miss chunks dùng từ khác như "năm 938" thay vì "năm nào"

### Vector Search (MongoDB Vector Index)

**Cách hoạt động**:
- Convert question → Query embedding (768 dimensions)
- Tìm chunks có **embedding tương tự** (cosine similarity cao)
- **Semantic matching** - hiểu nghĩa, không chỉ keywords

**Ví dụ**:
- Question: "Trận Bạch Đằng năm nào?"
- Query Embedding: `[0.123, -0.456, ...]`
- Vector Search: Tìm chunks có embedding **similar** (cosine similarity)
- **Kết quả**: Tìm được chunks về "Bạch Đằng", "938", "năm 938", "năm ấy", etc.

**Ưu điểm**:
- ✅ **Semantic understanding**: Hiểu nghĩa, không chỉ keywords
- ✅ **Better recall**: Tìm được chunks liên quan hơn
- ✅ **Multilingual**: Works tốt với Vietnamese

---

## 🔧 Technical Implementation

### EmbeddingService

**Methods**:
1. `GenerateEmbeddingAsync(text)` - Generate embedding cho **document** (task_type: RETRIEVAL_DOCUMENT)
2. `GenerateQueryEmbeddingAsync(text)` - Generate embedding cho **query** (task_type: RETRIEVAL_QUERY)

**API Call**:
```
POST https://generativelanguage.googleapis.com/v1beta/models/text-embedding-004:embedContent?key={apiKey}
Body: {
  "task_type": "RETRIEVAL_QUERY" hoặc "RETRIEVAL_DOCUMENT",
  "content": {
    "parts": [{"text": "..."}]
  }
}
Response: {
  "embedding": {
    "values": [0.123, -0.456, ...] // 768 dimensions
  }
}
```

**Model**: `text-embedding-004` (Gemini embedding model)
**Dimensions**: 768

### KWideRetriever - Vector Search Integration

**Flow trong `GetKWideChunksAsync`**:

```csharp
if (_embeddingService != null)
{
    try
    {
        // 1. Generate query embedding
        var queryEmbedding = await _embeddingService.GenerateQueryEmbeddingAsync(question, ct);
        
        // 2. Vector Search (MongoDB $vectorSearch)
        baseChunks = await VectorSearchAsync(queryEmbedding, k, ct);
    }
    catch (Exception ex)
    {
        // Fallback to text search if vector search fails
        baseChunks = await TextSearchAsync(question, k, ct);
    }
}
else
{
    // No embedding service → Use text search only
    baseChunks = await TextSearchAsync(question, k, ct);
}
```

**MongoDB Vector Search** (trong `VectorSearchAsync`):

```javascript
db.chunks.aggregate([
  {
    $vectorSearch: {
      index: "vector_index",
      path: "embedding",
      queryVector: [0.123, -0.456, ...], // Query embedding
      numCandidates: k * 2,
      limit: k
    }
  },
  {
    $project: {
      _id: 1,
      sourceId: 1,
      chunkIndex: 1,
      content: 1,
      pageFrom: 1,
      pageTo: 1
    }
  }
])
```

**MongoDB Vector Index** (cần setup trước):
- Index name: `vector_index`
- Path: `embedding`
- Dimensions: 768
- Similarity: cosine

---

## 💡 Use Cases

### 1. Initial Setup (First Time)

**Scenario**: Database có 1000 chunks nhưng chưa có embeddings.

**Steps**:
1. Check status: `GET /api/v1/admin/embeddings/status`
   - Response: `{ total: 1000, withEmbedding: 0, withoutEmbedding: 1000, percentage: "0.00" }`
2. Generate embeddings: `POST /api/v1/admin/embeddings/generate-all`
   - Process tất cả 1000 chunks
   - Time: ~15-30 phút (tùy rate limit)
3. Verify: `GET /api/v1/admin/embeddings/status`
   - Response: `{ total: 1000, withEmbedding: 1000, withoutEmbedding: 0, percentage: "100.00" }`

### 2. Incremental Update

**Scenario**: Có 100 chunks mới từ TEXT_INGEST nhưng chưa có embeddings.

**Steps**:
1. Check status để xem chunks nào cần embeddings
2. Generate chỉ cho chunks mới (hoặc tất cả chunks chưa có)
3. AI_QA sẽ tự động dùng vector search nếu chunks có embeddings

### 3. Regenerate Embeddings

**Scenario**: Muốn regenerate embeddings với model mới hoặc config mới.

**Steps**:
1. Delete all: `DELETE /api/v1/admin/embeddings/all`
2. Regenerate: `POST /api/v1/admin/embeddings/generate-all`

---

## 🎯 Tác dụng tóm tắt

### 1. **Enable Vector Search (Semantic Search)**
- ✅ Chuyển đổi text → vectors
- ✅ Hỗ trợ **semantic search** thay vì chỉ keyword matching
- ✅ **Better accuracy** trong RAG retrieval

### 2. **Improve AI_QA Accuracy**
- ✅ AI_QA sử dụng vector search để tìm chunks liên quan
- ✅ **Better context retrieval** → Better answers
- ✅ **Multilingual support** tốt hơn

### 3. **Migration/Backfill Tool**
- ✅ Generate embeddings cho chunks đã có trong DB
- ✅ Không cần re-ingest PDF files
- ✅ Flexible: Có thể generate sau khi TEXT_INGEST

### 4. **Admin Management**
- ✅ Check embedding status (% chunks có embeddings)
- ✅ Generate embeddings theo batch (có thể limit)
- ✅ Delete/Regenerate embeddings

---

## ⚙️ Mối quan hệ với các Features khác

### TEXT_INGEST ← EMBEDDINGS

**TEXT_INGEST** có thể:
- ✅ Generate embeddings ngay khi ingest (nếu có EmbeddingService)
- ✅ Hoặc để null → Admin generate sau bằng EMBEDDINGS

**EMBEDDINGS** có thể:
- ✅ Generate embeddings cho chunks từ TEXT_INGEST
- ✅ Backfill embeddings cho chunks đã có

### EMBEDDINGS → AI_QA

**AI_QA** sử dụng:
- ✅ **KWideRetriever** với EmbeddingService
- ✅ **Vector Search** nếu chunks có embeddings
- ✅ **Text Search** fallback nếu không có embeddings hoặc vector search fails

**Impact**:
- ✅ Vector search → **Better retrieval** → **Better answers**
- ✅ Text search fallback → **System vẫn hoạt động** nếu không có embeddings

---

## 📊 MongoDB Vector Search Setup

### Prerequisites

1. **MongoDB Atlas** (M10+ cluster)
2. **Vector Search Index** (cần setup trước):

```json
{
  "fields": [
    {
      "type": "vector",
      "path": "embedding",
      "numDimensions": 768,
      "similarity": "cosine"
    }
  ]
}
```

Index name: `vector_index`
Collection: `chunks`

### Setup Steps

1. Tạo Vector Search Index trên MongoDB Atlas UI
2. Generate embeddings cho chunks: `POST /api/v1/admin/embeddings/generate-all`
3. Verify: `GET /api/v1/admin/embeddings/status`
4. AI_QA sẽ tự động dùng vector search

---

## ✅ Kết luận

**EMBEDDINGS feature** có tác dụng:

1. ✅ **Enable Vector Search**: Chuyển text → vectors để hỗ trợ semantic search
2. ✅ **Improve AI_QA**: Better retrieval → Better answers
3. ✅ **Migration Tool**: Generate embeddings cho existing chunks
4. ✅ **Admin Management**: Check status, generate, delete embeddings

**Tác dụng chính**: **Nâng cao chất lượng RAG retrieval** trong AI_QA feature bằng cách dùng semantic search thay vì keyword matching.

---

**Tóm lại**: EMBEDDINGS là **enabler** cho Vector Search, giúp AI_QA tìm được chunks liên quan hơn và trả lời chính xác hơn.

