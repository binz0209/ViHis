# Frontend Changelog

## [Latest] - HTML Entities & Markdown Support

### Added
- ✅ HTML entity decoder (`utils/htmlDecoder.ts`)
  - Decodes HTML entities like `&ndash;`, `&mdash;`, `&nbsp;`, etc.
  - Automatically applied to all AI responses

- ✅ Markdown formatter (`utils/renderText.tsx`)
  - Supports bold text: `**text**`
  - Supports italic text: `*text*`
  - Supports bullet points: `* item` or `- item`
  - Converts markdown to safe HTML

- ✅ Enhanced message rendering with HTML support
  - Uses `dangerouslySetInnerHTML` to render formatted HTML
  - Proper styling for headings, paragraphs, lists, etc.

- ✅ CSS styling for markdown elements
  - Headings (h1, h2, h3)
  - Paragraphs
  - Unordered and ordered lists
  - Bold and italic text
  - Code blocks

### Fixed
- 🔧 HTML entities now display correctly
  - Before: `&ndash;`, `&mdash;` 
  - After: `–`, `—`

- 🔧 Bullet points render as proper lists
  - Before: Plain text with `*`
  - After: HTML `<ul><li>` elements

- 🔧 Bold and italic text render correctly
  - Before: `**text**`
  - After: **text** (bold)

### Files Changed
- `src/components/ChatBox.tsx` - Added HTML decoding and markdown formatting
- `src/components/ChatBox.css` - Added markdown styling
- `src/utils/htmlDecoder.ts` - New utility for HTML entity decoding
- `src/utils/renderText.tsx` - New utility for markdown formatting

## How It Works

1. User sends a question
2. Backend returns AI response
3. Frontend decodes HTML entities
4. Frontend converts markdown to HTML
5. React renders HTML safely with proper styling

## Example

**Input:**
```
Trần Hưng Đạo, tên thật là **Trần Quốc Tuấn**, là một danh tướng.

* Chức vụ: Tiết chế
* Chiến lược: Phòng giữ
* Tài năng: Đại tướng
```

**Output:**
- Decoded: `–` instead of `&ndash;`
- Rendered: **Bold text** instead of `**text**`
- Lists: Proper bullet points instead of `* item`


