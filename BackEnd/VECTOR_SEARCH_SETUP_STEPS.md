# MongoDB Atlas Vector Search - Step by Step

## 🎯 Chọn Vector Search (Not Atlas Search)

---

## 📋 Bước 1: Tạo Vector Search Index

### Trên MongoDB Atlas UI:

1. **Vào MongoDB Atlas**: https://cloud.mongodb.com/
2. **Chọn project** → **Database**: `viet_history`
3. **Click tab "Search"** (bên trái sidebar)
4. **Click "Create Search Index"**
5. **Chọn "Vector Search"** (KHÔNG chọn "Atlas Search")
6. **Chọn "JSON Editor"**
7. **Paste config:**

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

8. **Index name**: `vector_index`
9. **Database**: `viet_history`
10. **Collection**: `chunks`
11. **Click "Create Search Index"**
12. **Nếu thấy message**: *"No vector embeddings were detected in this collection..."*
    → **Click "Create anyway"** ✅ (Normal - chúng ta sẽ generate embeddings sau)

⏰ **Đợi 5-10 phút để index build xong!**

Check status:
- Tab "Search" → Xem trạng thái index
- Status: "Active" = Ready to use!

---

## 📊 Bước 2: Check Embeddings Status

```bash
curl https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net/api/v1/admin/embeddings/status
```

Response:
```json
{
  "total": 1000,
  "withEmbedding": 0,
  "withoutEmbedding": 1000,
  "percentage": "0.00"
}
```

---

## 🚀 Bước 3: Generate Embeddings

### Test với 10 chunks trước:

```bash
curl -X POST "https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net/api/v1/admin/embeddings/generate-all?limit=10"
```

Response:
```json
{
  "message": "Embeddings generated successfully",
  "processed": 10,
  "errors": 0,
  "total": 1000,
  "duration": "15.30s",
  "averageTime": "1.53s per chunk"
}
```

### Generate TẤT CẢ:

```bash
curl -X POST https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net/api/v1/admin/embeddings/generate-all
```

⏰ **Thời gian**: ~15-30 phút cho 1000 chunks (tùy rate limit)

---

## 🧪 Bước 4: Test Vector Search

1. Mở: https://vihis.vercel.app
2. Gửi câu hỏi bất kỳ
3. System sẽ tự động:
   - ✅ Dùng **Vector Search** (nếu có embeddings)
   - ✅ Fallback **Text Search** (nếu không có)

---

## ✅ Checklist

- [ ] Tạo Vector Search Index trên Atlas
- [ ] Index status = "Active"
- [ ] Generate embeddings (test 10 trước)
- [ ] Check status shows embeddings created
- [ ] Test câu hỏi trên frontend
- [ ] Verify results tốt hơn với semantic search

---

## 🐛 Troubleshooting

### Index không tạo được
- Check MongoDB Atlas version (cần M10+ hoặc Data Lake)
- Xem error message trong Atlas UI

### Embeddings API failed
- Check Gemini API key
- Check rate limit (wait few minutes)
- Check backend logs

### Vector search không hoạt động
- Verify index status = "Active"
- Check chunks có embeddings
- Check backend logs for errors

---

## 📚 Key Differences

### Atlas Search vs Vector Search

| Feature | Atlas Search | Vector Search |
|---------|-------------|---------------|
| Type | Full-text search | Semantic search |
| Matching | Keyword-based | Meaning-based |
| Best for | Exact text search | AI/ML applications |
| Our use | ❌ | ✅ |

**→ Chọn Vector Search cho project này! ✅**

