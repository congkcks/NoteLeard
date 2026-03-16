# Tài Li?u API - NoteLearn

## T?ng Quan
API NoteLearn cung c?p các endpoint ?? qu?n lý ghi chú, n?i dung h?c t?p, b? s?u t?p và tích h?p v?i YouTube. T?t c? các API endpoint ??u có ti?n t? `/api`.

---

## 1. ?? Authentication Controller (Xác th?c)

### POST /api/auth/login
**M?c ?ích:** ??ng nh?p ng??i dùng

**Request:**
```
POST /api/auth/login
```

**Response:**
```json
{
  "userId": 3,
  "token": "fake-jwt-token"
}
```

**Mô t?:** Endpoint này tr? v? thông tin ??ng nh?p gi? l?p (demo). Trong th?c t? s? x? lý xác th?c th?t.

---

## 2. ?? Note Controller (Ghi chú)

### POST /api/note
**M?c ?ích:** T?o ghi chú m?i

**Request:**
```
POST /api/note
Content-Type: application/x-www-form-urlencoded

contentId=123&page=1&text=N?i dung ghi chú c?a tôi
```

**Parameters:**
- `contentId` (long): ID c?a n?i dung liên quan
- `page` (int?, optional): S? trang (cho PDF)  
- `text` (string): N?i dung ghi chú

**Response:**
```json
{
  "id": 456,
  "userId": 1,
  "contentId": 123,
  "pageNumber": 1,
  "text": "N?i dung ghi chú c?a tôi",
  "createdAt": "2024-01-15T10:30:00"
}
```

### GET /api/note/{contentId}
**M?c ?ích:** L?y t?t c? ghi chú c?a m?t n?i dung

**Request:**
```
GET /api/note/123
```

**Response:**
```json
[
  {
    "id": 456,
    "userId": 1,
    "contentId": 123,
    "pageNumber": 1,
    "text": "Ghi chú ??u tiên",
    "createdAt": "2024-01-15T10:30:00"
  },
  {
    "id": 457,
    "userId": 1,
    "contentId": 123,
    "pageNumber": 2,
    "text": "Ghi chú th? hai",
    "createdAt": "2024-01-15T11:00:00"
  }
]
```

### PUT /api/note/{id}
**M?c ?ích:** C?p nh?t ghi chú

**Request:**
```
PUT /api/note/456
Content-Type: application/x-www-form-urlencoded

text=N?i dung ghi chú ?ã ???c c?p nh?t
```

**Response:**
```json
{
  "id": 456,
  "userId": 1,
  "contentId": 123,
  "pageNumber": 1,
  "text": "N?i dung ghi chú ?ã ???c c?p nh?t",
  "createdAt": "2024-01-15T10:30:00"
}
```

### DELETE /api/note/{id}
**M?c ?ích:** Xóa ghi chú

**Request:**
```
DELETE /api/note/456
```

**Response:**
```json
{}
```

---

## 3. ?? Content Controller (N?i dung)

### POST /api/content/upload-pdf
**M?c ?ích:** Upload file PDF

**Request:**
```
POST /api/content/upload-pdf
Content-Type: multipart/form-data

file: [PDF file]
```

**Response:**
```json
{
  "id": 123,
  "userId": 1,
  "title": "document.pdf",
  "type": "pdf",
  "fileUrl": "/uploads/user1/content123/document.pdf",
  "createdAt": "2024-01-15T10:30:00"
}
```

### POST /api/content/add-youtube
**M?c ?ích:** Thêm video YouTube

**Request:**
```
POST /api/content/add-youtube
Content-Type: application/x-www-form-urlencoded

title=H?c ti?ng Anh c? b?n&url=https://youtube.com/watch?v=abc123
```

**Parameters:**
- `title` (string): Tiêu ?? video
- `url` (string): URL video YouTube

**Response:**
```json
{
  "id": 124,
  "userId": 1,
  "title": "H?c ti?ng Anh c? b?n",
  "type": "youtube",
  "youtubeUrl": "https://youtube.com/watch?v=abc123",
  "createdAt": "2024-01-15T10:30:00"
}
```

### GET /api/content
**M?c ?ích:** L?y t?t c? n?i dung c?a ng??i dùng

**Request:**
```
GET /api/content
```

**Response:**
```json
[
  {
    "id": 123,
    "userId": 1,
    "title": "document.pdf",
    "type": "pdf",
    "fileUrl": "/uploads/user1/content123/document.pdf",
    "createdAt": "2024-01-15T10:30:00"
  },
  {
    "id": 124,
    "userId": 1,
    "title": "H?c ti?ng Anh c? b?n",
    "type": "youtube",
    "youtubeUrl": "https://youtube.com/watch?v=abc123",
    "createdAt": "2024-01-15T11:00:00"
  }
]
```

---

## 4. ?? Collection Controller (B? s?u t?p)

### POST /api/collection
**M?c ?ích:** T?o b? s?u t?p m?i

**Request:**
```
POST /api/collection
Content-Type: application/x-www-form-urlencoded

name=Ti?ng Anh Giao Ti?p
```

**Response:**
```json
{
  "id": 789,
  "name": "Ti?ng Anh Giao Ti?p",
  "userId": 1
}
```

### POST /api/collection/{collectionId}/add/{contentId}
**M?c ?ích:** Thêm n?i dung vào b? s?u t?p

**Request:**
```
POST /api/collection/789/add/123
```

**Response:**
```json
{}
```

**Mô t?:** Thêm n?i dung có ID 123 vào b? s?u t?p có ID 789.

### GET /api/collection
**M?c ?ích:** L?y t?t c? b? s?u t?p c?a ng??i dùng

**Request:**
```
GET /api/collection
```

**Response:**
```json
[
  {
    "id": 789,
    "name": "Ti?ng Anh Giao Ti?p",
    "userId": 1
  },
  {
    "id": 790,
    "name": "Tài Li?u H?c T?p",
    "userId": 1
  }
]
```

---

## 5. ?? Document Controller (Tài li?u)

### GET /api/document/{contentId}/view
**M?c ?ích:** L?y URL xem PDF

**Request:**
```
GET /api/document/123/view
```

**Response:**
```json
{
  "url": "https://storage.example.com/signed-url-for-pdf"
}
```

**L?u ý:** Ch? ho?t ??ng v?i n?i dung có type = "pdf".

---

## 6. ?? AI Controller (Trí tu? nhân t?o)

### POST /api/ai/summary/{contentId}
**M?c ?ích:** L?u tóm t?t AI cho n?i dung

**Request:**
```
POST /api/ai/summary/123
Content-Type: application/x-www-form-urlencoded

summary=?ây là tóm t?t n?i dung ???c t?o b?i AI
```

**Response:**
```json
{}
```

### GET /api/ai/summary/{contentId}
**M?c ?ích:** L?y tóm t?t AI c?a n?i dung

**Request:**
```
GET /api/ai/summary/123
```

**Response:**
```json
{
  "id": 1,
  "contentId": 123,
  "summary": "?ây là tóm t?t n?i dung ???c t?o b?i AI"
}
```

---

## 7. ?? YouTube Controller (YouTube)

### POST /api/youtube/transcript
**M?c ?ích:** L?y ph? ?? t? video YouTube

**Request:**
```
POST /api/youtube/transcript
Content-Type: application/json

{
  "youtubeUrl": "https://www.youtube.com/watch?v=abc123"
}
```

**Response:**
```json
{
  "videoId": "abc123",
  "transcriptRaw": {
    "transcript_data": "D? li?u ph? ?? video..."
  }
}
```

**L?u ý:** Endpoint này trích xu?t ID video t? URL YouTube và l?y ph? ?? t??ng ?ng.

---

## ?? L?u Ý Quan Tr?ng

1. **Xác th?c:** Hi?n t?i API ?ang s? d?ng UserId c? ??nh (=1) cho demo. Trong production c?n implement JWT authentication th?t.

2. **Content-Type:**
   - Các endpoint upload file: `multipart/form-data`
   - Các endpoint JSON: `application/json`
   - Các endpoint form: `application/x-www-form-urlencoded`

3. **Error Responses:**
   - `404 Not Found`: Khi resource không t?n t?i
   - `400 Bad Request`: Khi d? li?u ??u vào không h?p l?

4. **Base URL:** T?t c? endpoint ??u có ti?n t? `/api`

---

## ?? Ví D? Workflow S? D?ng

### 1. Workflow Upload PDF và Ghi Chú
```
1. POST /api/auth/login                     ? ??ng nh?p
2. POST /api/content/upload-pdf            ? Upload PDF (nh?n contentId)
3. POST /api/note (v?i contentId)          ? T?o ghi chú cho PDF
4. GET /api/note/{contentId}               ? Xem t?t c? ghi chú
```

### 2. Workflow YouTube và Collection  
```
1. POST /api/content/add-youtube           ? Thêm video YouTube
2. POST /api/collection                    ? T?o b? s?u t?p
3. POST /api/collection/{id}/add/{contentId} ? Thêm video vào collection
4. POST /api/youtube/transcript            ? L?y ph? ?? video
```