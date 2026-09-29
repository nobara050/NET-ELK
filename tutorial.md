# HƯỚNG DẪN DEMO TOÀN DIỆN DỰ ÁN ECOMSEARCHAPI

## Hệ Thống E-Commerce Search & Log Analytics (.NET 10 + Elasticsearch 8.17 + Logstash + Kibana + PostgreSQL)

---

## MỤC LỤC

1. [Tổng Quan Kiến Trúc & Luồng Dữ Liệu](#1-tổng-quan-kiến-trúc--luồng-dữ-liệu)
2. [Bước 1: Khởi Động Hạ Tầng (Docker ELK & Database)](#2-bước-1-khởi-động-hạ-tầng-docker-elk--database)
3. [Bước 2: Khởi Động ASP.NET Core Web API](#3-bước-2-khởi-động-aspnet-core-web-api)
4. [Bước 3: 1-Click Seed Dữ Liệu (POST /api/reset)](#4-bước-3-1-click-seed-dữ-liệu-post-apireset)
5. [Bước 4: Demo 6 Tính Năng Tìm Kiếm Chuyên Sâu Trên Swagger UI](#5-bước-4-demo-6-tính-năng-tìm-kiếm-chuyên-sâu-trên-swagger-ui)
6. [Bước 5: Demo Observability Trên Kibana (Discover & Log Routing)](#6-bước-5-demo-observability-trên-kibana-discover--log-routing)
7. [Bước 6: Demo Chuyên Sâu Trong Kibana Dev Tools Console](#7-bước-6-demo-chuyên-sâu-trong-kibana-dev-tools-console)
8. [Bộ Câu Hỏi Thuyết Trình & Trả Lời (Q&A Phản Biện)](#8-bộ-câu-hỏi-thuyết-trình--trả-lời-qa-phản-biện)

---

## 1. TỔNG QUAN KIẾN TRÚC & LUỒNG DỮ LIỆU

### 1.1. Các thành phần công nghệ trong dự án

- **ASP.NET Core Web API (.NET 10)**: Backend xử lý API, Controllers, DI, kết nối song song PostgreSQL và Elasticsearch.
- **PostgreSQL Local (Port 5432, database `ecom_search_db`)**: Đóng vai trò **Single Source of Truth** đảm bảo tính toàn vẹn dữ liệu ACID.
- **Elasticsearch 8.17 (Docker Port 9200)**: Search Engine phân tán với Inverted Index, BM25 Scoring, Edge N-Gram Autocomplete và Aggregations.
- **Logstash 8.17 (Docker Port 5000 TCP, Port 9600)**: Log collector tiếp nhận stream JSON log từ Serilog, dùng Grok filter bóc tách access log và bulk index sang Elasticsearch.
- **Kibana 8.17 (Docker Port 5601)**: Giao diện trực quan hóa log (Discover), quản trị chỉ mục và chạy lệnh Dev Tools Console.

### 1.2. Mô tả luồng dữ liệu (Data Flow)

1. **Luồng Ghi (Dual-Write)**:
   Client $\rightarrow$ API $\rightarrow$ Ghi vào PostgreSQL trước để sinh `Id` Primary Key $\rightarrow$ Ngay sau đó Dual-Write sang Elasticsearch với document `_id = product.Id`.
2. **Luồng Tìm kiếm (Read Path)**:
   Client $\rightarrow$ API $\rightarrow$ Truy vấn trực tiếp vào Elasticsearch (Port 9200) $\rightarrow$ Trả về kết quả siêu tốc (< 5ms). PostgreSQL hoàn toàn không phải chịu tải tìm kiếm.
3. **Luồng Giám sát & Ghi log (Observability Flow)**:
   Mỗi HTTP Request đến API $\rightarrow$ Serilog ghi log và stream qua socket TCP (Port 5000) vào Logstash $\rightarrow$ Logstash bóc tách Grok (method, url, status code, elapsed ms) $\rightarrow$ Đẩy vào Elasticsearch index `app-logs-*` $\rightarrow$ Kibana đọc dữ liệu từ Elasticsearch và hiển thị thời gian thực trên Discover.

---

## 2. BƯỚC 1: KHỞI ĐỘNG HẠ TẦNG (DOCKER ELK & DATABASE)

### 2.1. Bật Docker Desktop

- Mở ứng dụng **Docker Desktop** trên Windows.
- Chờ đến khi góc dưới bên trái của Docker Desktop chuyển sang màu **xanh lá cây** _(Engine running)_.

### 2.2. Khởi động cụm ELK

Mở PowerShell tại thư mục gốc `d:\FPT\OJT_Elasticsearch` và chạy:

```powershell
docker compose up -d
```

### 2.3. Kiểm tra cụm container đang chạy

Chạy lệnh:

```powershell
docker compose ps
```

**Kết quả chuẩn:**

- `elasticsearch`: `Up` (cổng `0.0.0.0:9200->9200/tcp`)
- `logstash`: `Up` (cổng `0.0.0.0:5000->5000/tcp`, `0.0.0.0:9600->9600/tcp`)
- `kibana`: `Up` (cổng `0.0.0.0:5601->5601/tcp`)

---

## 3. BƯỚC 2: KHỞI ĐỘNG ASP.NET CORE WEB API

### 3.1. Chạy API

- Trong **Visual Studio tím**, bấm **F5** (hoặc nút Run HTTPS `https`).
- Hoặc chạy qua terminal:
  ```powershell
  cd d:\FPT\OJT_Elasticsearch\EcomSearchApi
  dotnet run
  ```

### 3.2. Kiểm tra Swagger UI

Trình duyệt sẽ tự động mở giao diện Swagger UI tại địa chỉ:
`https://localhost:7076` (hoặc `http://localhost:5062`).

**Quan sát trên giao diện Swagger:**

- Nhóm **Products**: Các endpoint CRUD (`GET`, `POST`, `PUT`, `DELETE`).
- Nhóm **Reset**: Endpoint `POST /api/reset`.
- Nhóm **Search**: 6 endpoints tìm kiếm chuyên sâu (`fulltext`, `filter`, `advanced`, `autocomplete`, `fuzzy`, `stats`).

---

## 4. BƯỚC 3: 1-CLICK SEED DỮ LIỆU (POST /api/reset)

Đây là thao tác quan trọng nhất để chuẩn bị dữ liệu demo sạch sẽ cho toàn bộ hệ thống.

### 4.1. Thao tác trên Swagger UI

1. Cuộn đến nhóm **Reset** $\rightarrow$ Click vào `POST /api/reset`.
2. Bấm nút **Try it out** $\rightarrow$ Bấm nút **Execute**.

### 4.2. Giải thích cho người nghe / giám khảo những gì đang diễn ra:

- **Tầng PostgreSQL**: API thực thi câu lệnh SQL:
  ```sql
  TRUNCATE TABLE "Products" RESTART IDENTITY CASCADE;
  ```
  Xóa sạch dữ liệu cũ và reset ID tự tăng về 1.
- **Tầng Elasticsearch**:
  - Xóa index `products` cũ nếu tồn tại.
  - Tái tạo lại index `products` mới tinh với **Custom Edge N-Gram Analyzer** (min gram 2, max gram 10) và Multi-field mapping (`name.autocomplete`, `name.keyword`, `description` english analyzer).
- **Tầng Seed Dữ liệu**:
  - Nạp 10 sản phẩm mẫu đa ngành hàng từ `SampleData.cs` vào PostgreSQL.
  - Sử dụng `BulkAsync` nạp đồng loạt 10 sản phẩm đó sang Elasticsearch với document `_id = product.Id`.
  - Refresh index để dữ liệu có hiệu lực tìm kiếm ngay lập tức.

### 4.3. Kết quả mong đợi (Response 200 OK):

```json
{
  "status": "SUCCESS",
  "message": "PostgreSQL and Elasticsearch reset and seeded successfully!",
  "postgresCount": 10,
  "elasticsearchCount": 10
}
```

---

## 5. BƯỚC 4: DEMO 6 TÍNH NĂNG TÌM KIẾM CHUYÊN SÂU TRÊN SWAGGER UI

Mỗi endpoint dưới đây đại diện cho một thế mạnh vượt trội của Elasticsearch so với cơ sở dữ liệu quan hệ truyền thống:

---

### Demo 1: Full-Text Search (BM25 Relevance + Boost `name^2` + Highlighting)

- **URL**: `GET /api/search/fulltext?q=wireless`
- **Thao tác**: Nhập `q = wireless` $\rightarrow$ Execute.
- **Kịch bản thuyết trình**:
  - _"Nếu dùng SQL: Ta phải viết `WHERE Description LIKE '%wireless%' OR Name LIKE '%wireless%'`. SQL chỉ trả về Đúng hoặc Sai, quét toàn bảng rất chậm và không biết sản phẩm nào liên quan hơn."_
  - _"Elasticsearch sử dụng thuật toán **BM25** để tính điểm phù hợp (`_score`)."_
  - _"Hệ thống cấu hình **Field Boosting `name^2`**: sản phẩm nào có chữ 'wireless' ở Tên sẽ được nhân đôi trọng số và xếp lên trước sản phẩm chỉ có chữ đó ở Mô tả."_
  - _"Tính năng **Highlighting** tự động bọc thẻ `<b>wireless</b>` xung quanh từ khóa tìm thấy trong mô tả sản phẩm để hiển thị nổi bật trên UI."_
- **Response mẫu:**
  ```json
  {
    "total": 2,
    "tookMs": 3,
    "results": [
      {
        "score": 1.84,
        "product": {
          "id": 1,
          "name": "Sony WH-1000XM5 Wireless Headphones",
          "price": 399.99
        },
        "highlights": {
          "description": [
            "Industry leading noise canceling <b>wireless</b> over-ear headphones..."
          ]
        }
      }
    ]
  }
  ```

---

### Demo 2: Filter Search (Bool Filter Context - Bitset Cache RAM siêu tốc)

- **URL**: `GET /api/search/filter?category=Accessories&minPrice=50&maxPrice=120`
- **Thao tác**: Nhập `category = Accessories`, `minPrice = 50`, `maxPrice = 120` $\rightarrow$ Execute.
- **Kịch bản thuyết trình**:
  - _"Khi người dùng lọc danh mục hoặc khoảng giá, họ không cần tính điểm relevance."_
  - _"Truy vấn này được đặt hoàn toàn trong **Filter Context** (`b.Filter(...)`). Điểm `_score` bằng 0."_
  - _"Elasticsearch tự động kích hoạt cơ chế **Bitset Cache** trong RAM. Các lần lọc sau với cùng điều kiện sẽ được trả về gần như ngay lập tức (< 2ms) dù dữ liệu có hàng chục triệu bản ghi."_
- **Response mẫu:** Trả về chuột `Logitech MX Master 3S Wireless Mouse` ($99.99).

---

### Demo 3: Compound Bool Query (Senior Level Multi-Clause)

- **URL**: `GET /api/search/advanced?q=noise&category=Electronics&excludeBrand=Bose`
- **Thao tác**: Nhập `q = noise`, `category = Electronics`, `excludeBrand = Bose` $\rightarrow$ Execute.
- **Kịch bản thuyết trình**:
  - _"Đây là câu truy vấn phức hợp kết hợp đồng thời cả 4 mệnh đề cốt lõi của Elasticsearch Bool Query:"_
    1. **`must`**: Bắt buộc phải khớp từ khóa "noise" (đóng góp vào điểm BM25).
    2. **`filter`**: Bắt buộc phải thuộc danh mục "Electronics" (được cache trong RAM).
    3. **`should`**: Nếu sản phẩm có tag `"premium"` thì được cộng thêm điểm thưởng xếp hạng lên đầu.
    4. **`must_not`**: Loại bỏ hoàn toàn các sản phẩm thuộc thương hiệu "Bose".

---

### Demo 4: Search-As-You-Type Autocomplete (Edge N-Gram Tokenizer)

- **URL**: `GET /api/search/autocomplete?prefix=son`
- **Thao tác**:
  - Thử lần 1: Nhập `prefix = son` $\rightarrow$ Gợi ý ngay `Sony WH-1000XM5`.
  - Thử lần 2: Nhập `prefix = mac` $\rightarrow$ Gợi ý ngay `Apple MacBook Pro 16"`.
  - Thử lần 3: Nhập `prefix = dys` $\rightarrow$ Gợi ý ngay `Dyson V15 Detect`.
- **Kịch bản thuyết trình**:
  - _"Tính năng gợi ý tìm kiếm tức thì khi người dùng mới gõ 2-3 ký tự đầu."_
  - _"Bí quyết kỹ thuật: Trong `ElasticIndexManager.cs`, ta tạo ra **Custom Analyzer** sử dụng token filter `edge_ngram` với `min_gram = 2`, `max_gram = 10`."_
  - _"Khi từ 'Sony' được nạp vào, Elasticsearch tự động băm thành các mảnh: `so`, `son`, `sony`. Do đó người dùng chỉ cần gõ 'so' hoặc 'son' là document khớp ngay lập tức mà không cần gõ đủ tên!"_

---

### Demo 5: Fuzzy Typo-Tolerant Search (Chịu lỗi chính tả Levenshtein)

- **URL**: `GET /api/search/fuzzy?q=Samsng`
- **Thao tác**: Cố tình gõ sai chữ `Samsng` (thiếu chữ 'u') $\rightarrow$ Execute.
- **Kịch bản thuyết trình**:
  - _"Khách hàng mua sắm trên di động rất hay gõ sai chính tả."_
  - _"Elasticsearch hỗ trợ thuật toán khoảng cách chỉnh sửa **Levenshtein Distance** với tham số `fuzziness: AUTO`."_
  - _"Dù người dùng gõ thiếu chữ 'u' (`Samsng`), Elasticsearch vẫn tự động hiểu và trả về chính xác sản phẩm `Samsung Galaxy S24 Ultra`!"_

---

### Demo 6: Real-time Aggregations & Analytics (OLAP tức thời)

- **URL**: `GET /api/search/stats`
- **Thao tác**: Bấm Execute.
- **Kịch bản thuyết trình**:
  - _"Elasticsearch không chỉ tìm kiếm văn bản mà còn là một công cụ phân tích OLAP cực mạnh."_
  - _"API truyền tham số `Size(0)`: Không tải bất kỳ document nào về, chỉ yêu cầu Elasticsearch tính toán thống kê tức thì:"_
    - **Bucket Aggregation**: Phân nhóm và đếm số lượng sản phẩm theo từng `Category` và `Brand`.
    - **Sub-Aggregation**: Tính giá trung bình (`avg_price`) của từng nhóm đó.
    - **Metric Stats**: Trả về thống kê giá toàn sàn: `min`, `max`, `avg`, `sum`, `count`.
  - _"Toàn bộ việc tính toán này diễn ra trong RAM chỉ mất khoảng 4-5ms."_

---

## 6. BƯỚC 5: DEMO OBSERVABILITY TRÊN KIBANA (DISCOVER & LOG ROUTING)

### 6.1. Mở Kibana

Truy cập trình duyệt: `http://localhost:5601`.

### 6.2. Tạo Data View (chỉ cần làm lần đầu tiên)

1. Trên thanh menu Kibana, vào **Management** $\rightarrow$ **Stack Management**.
2. Chọn mục **Data Views** (dưới thẻ Kibana) $\rightarrow$ Bấm nút **Create data view**.
3. Điền thông tin:
   - **Name**: `App Logs`
   - **Index pattern**: `app-logs-*`
   - **Timestamp field**: Chọn `@timestamp`
4. Bấm **Save data view to Kibana**.

### 6.3. Khám phá log trên Discover

1. Mở menu góc trái $\rightarrow$ Chọn **Analytics** $\rightarrow$ **Discover**.
2. Chọn Data View `App Logs`.
3. **Thuyết trình:**
   - _"Tất cả các lượt click tìm kiếm vừa rồi trên Swagger đều đã được Serilog bắn qua TCP socket 5000 vào Logstash."_
   - _"Logstash đã dùng bộ lọc **Grok** để bóc tách thành các trường dữ liệu độc lập:"_
     - `http_method`: `GET`
     - `request_path`: `/api/search/fulltext?q=wireless`
     - `status_code`: `200`
     - `elapsed_ms`: Thời gian xử lý của API.
   - _"Người quản trị hệ thống có thể lọc ra ngay các request chậm hơn 100ms hoặc các request bị lỗi mã 4xx/5xx trong tích tắc."_

### 6.4. Demo phân luồng log lỗi (Error Routing)

1. Quay lại Swagger, gọi một ID không tồn tại: `GET /api/products/99999` $\rightarrow$ Trả về mã `404 Not Found`.
2. Trên Kibana, Logstash tự động phân loại mức độ nghiêm trọng: nếu có ngoại lệ hoặc lỗi, Logstash sẽ định tuyến bản ghi log sang index riêng `app-logs-errors-*` để đội ngũ DevOps nhận diện sự cố ngay lập tức.

---

## 7. BƯỚC 6: DEMO CHUYÊN SÂU TRONG KIBANA DEV TOOLS CONSOLE

Vào Kibana $\rightarrow$ Chọn **Management** $\rightarrow$ **Dev Tools** (hoặc truy cập trực tiếp `http://localhost:5601/app/dev_tools#/console`).

Tại đây, bạn mở file [`elasticsearch-commands.es`](file:///d:/FPT/OJT_Elasticsearch/elasticsearch-commands.es) trong thư mục dự án và copy các lệnh sau để chạy trước mặt người xem:

### 1. Kiểm tra sức khỏe cụm phân tán (Cluster Health & Shards):

```http
GET _cluster/health
```

_Giải thích: Hiển thị trạng thái cụm (`status: green`/`yellow`), số lượng Data Nodes và Shards đang hoạt động._

### 2. Xem cấu hình Mapping & Analyzer của Index `products`:

```http
GET products/_mapping
```

_Giải thích: Cho thấy trường `name` có 2 sub-fields (`name.keyword` và `name.autocomplete`), trường `description` dùng analyzer `english`._

### 3. Trực quan hóa cơ chế băm từ của Edge N-Gram Autocomplete:

Chạy lệnh phân tích thử một từ khóa:

```http
POST products/_analyze
{
  "analyzer": "autocomplete_analyzer",
  "text": "Sony"
}
```

**Kết quả hiển thị trên Console:**

```json
{
  "tokens": [
    { "token": "so", "start_offset": 0, "end_offset": 2 },
    { "token": "son", "start_offset": 0, "end_offset": 3 },
    { "token": "sony", "start_offset": 0, "end_offset": 4 }
  ]
}
```

_Điểm nhấn thuyết trình: Đây là bằng chứng trực quan nhất giải thích tại sao khi người dùng mới gõ chữ "so", Elasticsearch đã lập tức tìm ra sản phẩm Sony!_

---

## 8. BỘ CÂU HỎI THUYẾT TRÌNH & TRẢ LỜI (Q&A PHẢN BIỆN)

Dưới đây là các câu hỏi thường gặp nhất khi bảo vệ đồ án/demo dự án:

#### Câu 1: Tại sao không dùng PostgreSQL để tìm kiếm luôn mà phải tích hợp thêm Elasticsearch?

> **Trả lời:**
>
> 1. **Về mặt hiệu năng (Throughput & Latency):** PostgreSQL tối ưu cho giao dịch ACID (OLTP). Khi người dùng tìm kiếm dạng `LIKE '%keyword%'`, PostgreSQL buộc phải Full Table Scan, làm khóa dòng/bảng và nghẽn tài nguyên. Elasticsearch sử dụng cấu trúc **Inverted Index**, giúp tìm kiếm trên hàng triệu bản ghi chỉ mất vài mili-giây.
> 2. **Về tính năng tìm kiếm:** PostgreSQL không hỗ trợ thuật toán tính điểm relevance (BM25), không hỗ trợ Edge N-Gram Autocomplete, và không xử lý tốt lỗi chính tả (Fuzzy Levenshtein) linh hoạt như Elasticsearch.

#### Câu 2: Cơ chế Dual-Write trong dự án xử lý đồng bộ dữ liệu như thế nào?

> **Trả lời:**
> Trong `ProductService.cs`, ứng dụng ghi dữ liệu thành công vào PostgreSQL trước để đảm bảo tính toàn vẹn dữ liệu ACID và lấy ra Primary Key (`product.Id`). Ngay sau đó, API dùng chính ID đó để index sang Elasticsearch (`_id = product.Id`). Điều này đảm bảo ID của document trong Elasticsearch luôn đồng nhất 1-1 với ID trong PostgreSQL.

#### Câu 3: Tại sao lại đẩy log từ Serilog qua Logstash (cổng 5000) mà không ghi thẳng vào Elasticsearch?

> **Trả lời:**
>
> 1. **Giảm tải cho Web API:** Ghi log qua socket TCP là thao tác bất đồng bộ, cực kỳ nhẹ và không làm nghẽn luồng xử lý HTTP request của người dùng.
> 2. **Xử lý dữ liệu tập trung (Data Processing):** Logstash đảm nhận việc bóc tách chuỗi message bằng Grok Filter thành các trường có cấu trúc (`status_code`, `elapsed_ms`), gắn tag và phân loại log lỗi sang các index riêng biệt trước khi nạp vào Elasticsearch.
> 3. **Khả năng đệm (Buffering & Decoupling):** Nếu Elasticsearch tạm thời bảo trì, Logstash có thể lưu đệm log mà không làm ứng dụng API bị lỗi.
