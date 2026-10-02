# Hướng Dẫn Elasticsearch Dev Tools Console

Tài liệu này tổng hợp các câu lệnh API của Elasticsearch

**Kibana Dev Tools Console** (`http://localhost:5601/app/dev_tools#/console`).

---

## MỤC LỤC

1. [Phần 1: Quản Trị Cluster, Nodes & Shards](#phần-1-quản-trị-cluster-nodes--shards)
2. [Phần 2: Inverted Index & Text Analysis (\_analyze)](#phần-2-inverted-index--text-analysis-_analyze)
3. [Phần 3: Tạo Index Với Custom Analyzer & Explicit Mapping](#phần-3-tạo-index-với-custom-analyzer--explicit-mapping)
4. [Phần 4: Document CRUD & Bulk API](#phần-4-document-crud--bulk-api)
5. [Phần 5: Query DSL - Tìm Kiếm Chuyên Sâu & BM25 Relevance](#phần-5-query-dsl---tìm-kiếm-chuyên-sâu--bm25-relevance)
6. [Phần 6: Aggregations - Thống Kê & Phân Tích Dữ Liệu](#phần-6-aggregations---thống-kê--phân-tích-dữ-liệu)

---

## Phần 1: Quản Trị Cluster, Nodes & Shards

### 1.1. Kiểm tra sức khỏe Cluster (`_cluster/health`)

- **Cú pháp (Syntax):**

  ```http
  GET /_cluster/health
  ```

- **Giải thích:**
  - `status: "green"`: Toàn bộ Primary Shard và Replica Shard đều hoạt động bình thường.
  - `status: "yellow"`: Tất cả Primary Shard hoạt động tốt, nhưng một số Replica Shard chưa được phân bổ (thường gặp khi chạy Single-Node Docker).
  - `status: "red"`: Có Primary Shard bị mất hoặc hỏng dữ liệu.

---

### 1.2. Liệt kê thông số các Node (`_cat/nodes`)

- **Cú pháp (Syntax):**
  ```http
  GET /_cat/nodes?v&h=<danh_sách_cột>
  ```
- **Ví dụ trong Dev Tools:**
  ```http
  GET /_cat/nodes?v&h=name,ip,node.role,master,heap.percent,ram.percent,cpu,uptime
  ```
- **Giải thích:** Giúp kiểm tra tài nguyên của Node `es01`: % dung lượng bộ nhớ JVM Heap, % RAM, % CPU và thời gian hoạt động liên tục (uptime).

---

### 1.3. Liệt kê và kiểm tra trạng thái các Index (`_cat/indices`)

- **Cú pháp (Syntax):**
  ```http
  GET /_cat/indices?v&s=<cột_sắp_xếp>
  ```
- **Ví dụ trong Dev Tools:**
  ```http
  GET /_cat/indices?v&s=index
  ```
- **Giải thích:** Hiển thị danh sách tất cả các index (như `products`, `app-logs-*`, `.kibana`), số lượng document (`docs.count`), dung lượng lưu trữ trên đĩa (`store.size`).

---

### 1.4. Kiểm tra phân bổ Shard (`_cat/shards`)

- **Cú pháp (Syntax):**
  ```http
  GET /_cat/shards?v
  ```
- **Ví dụ trong Dev Tools:**
  ```http
  GET /_cat/shards?v
  ```
- **Giải thích:** Kiểm tra xem mỗi index có bao nhiêu Primary Shard (`p`) và Replica Shard (`r`), đang ở trạng thái `STARTED` hay `UNASSIGNED`.

---

## Phần 2: Inverted Index & Text Analysis (\_analyze)

API `_analyze` giúp bạn nhìn thấy chính xác cách Elasticsearch bẻ nhỏ chuỗi văn bản thành các **Tokens** để lưu vào **Inverted Index**.

---

### 2.1. Standard Analyzer (Mặc định - Bẻ từ theo khoảng trắng, dấu và chuyển chữ thường)

- **Cú pháp (Syntax):**
  ```http
  POST /_analyze
  {
    "analyzer": "<tên_analyzer>",
    "text": "<nội_dung_cần_phân_tích>"
  }
  ```
- **Ví dụ trong Dev Tools:**
  ```http
  POST /_analyze
  {
    "analyzer": "standard",
    "text": "Laptop Gaming ASUS ROG Strix G16 - Siêu Mạnh Mẽ!"
  }
  ```
- **Kết quả Tokens:** `["laptop", "gaming", "asus", "rog", "strix", "g16", "siêu", "mạnh", "mẽ"]`

---

### 2.2. English Analyzer có Stemming (Rút gọn từ về dạng gốc)

- **Cú pháp (Syntax):**
  ```http
  POST /_analyze
  {
    "analyzer": "english",
    "text": "<nội_dung_tiếng_anh>"
  }
  ```
- **Ví dụ trong Dev Tools:**
  ```http
  POST /_analyze
  {
    "analyzer": "english",
    "text": "Developers are developing high performance searches with Elasticsearch"
  }
  ```
- **Kết quả Tokens:** `["develop", "develop", "high", "perform", "search", "elasticsearch"]`
- **Ý nghĩa:** Nhờ tính năng **Stemming** (đưa `developing` $\rightarrow$ `develop`, `searches` $\rightarrow$ `search`), khi người dùng gõ tìm kiếm _"search"_, tài liệu chứa _"searches"_ vẫn được tìm thấy với điểm BM25 cao.

---

### 2.3. Keyword Tokenizer (Không bẻ từ, giữ nguyên 100% chuỗi)

- **Cú pháp (Syntax):**
  ```http
  POST /_analyze
  {
    "tokenizer": "keyword",
    "text": "<chuỗi_giá_trị>"
  }
  ```
- **Ví dụ trong Dev Tools:**
  ```http
  POST /_analyze
  {
    "tokenizer": "keyword",
    "text": "Laptop & Linh Kiện Điện Tử"
  }
  ```
- **Kết quả Tokens:** `["Laptop & Linh Kiện Điện Tử"]`
- **Ý nghĩa:** Dùng cho kiểu `keyword`, phục vụ lọc chính xác (Exact Filter), phân loại (Aggregations) hoặc sắp xếp (Sorting).

---

## Phần 3: Tạo Index Với Custom Analyzer & Explicit Mapping

### 3.1. Xóa và Khởi tạo Index `products`

- **Cú pháp (Syntax):**

  ```http
  DELETE /<index_name>

  PUT /<index_name>
  {
    "settings": { ... },
    "mappings": { ... }
  }
  ```

- **Ví dụ trong Dev Tools (Cấu hình chuẩn của dự án `EcomSearchApi`):**

  ```http
  DELETE /products

  PUT /products
  {
    "settings": {
      "number_of_shards": 1,
      "number_of_replicas": 0,
      "analysis": {
        "filter": {
          "edge_ngram_filter": {
            "type": "edge_ngram",
            "min_gram": 2,
            "max_gram": 15
          }
        },
        "analyzer": {
          "autocomplete_analyzer": {
            "type": "custom",
            "tokenizer": "standard",
            "filter": [
              "lowercase",
              "edge_ngram_filter"
            ]
          }
        }
      }
    },
    "mappings": {
      "properties": {
        "id": { "type": "integer" },
        "name": {
          "type": "text",
          "analyzer": "standard",
          "fields": {
            "keyword": { "type": "keyword" },
            "autocomplete": {
              "type": "text",
              "analyzer": "autocomplete_analyzer",
              "search_analyzer": "standard"
            }
          }
        },
        "description": { "type": "text", "analyzer": "standard" },
        "category": { "type": "keyword" },
        "brand": { "type": "keyword" },
        "price": { "type": "double" },
        "stock": { "type": "integer" },
        "rating": { "type": "double" },
        "tags": { "type": "keyword" },
        "is_active": { "type": "boolean" },
        "created_at": { "type": "date" }
      }
    }
  }
  ```

---

### 3.2. Kiểm tra Custom Edge N-Gram Analyzer vừa tạo

- **Ví dụ trong Dev Tools:**
  ```http
  POST /products/_analyze
  {
    "analyzer": "autocomplete_analyzer",
    "text": "MacBook"
  }
  ```
- **Kết quả Tokens sinh ra:** `["ma", "mac", "macb", "macbo", "macboo", "macbook"]`
- **Ý nghĩa:** Người dùng chỉ cần gõ tiền tố 2 ký tự `"ma"` hay `"mac"` là hệ thống đã có thể gợi ý ngay sản phẩm _"MacBook Pro"_.

---

## Phần 4: Document CRUD & Bulk API

### 4.1. Tạo / Thêm mới Document có chỉ định ID (`POST /index/_doc/id`)

- **Cú pháp (Syntax):**
  ```http
  POST /<index_name>/_doc/<id>
  {
    "<field1>": "<value1>",
    "<field2>": "<value2>"
  }
  ```
- **Ví dụ trong Dev Tools:**
  ```http
  POST /products/_doc/1
  {
    "id": 1,
    "name": "Laptop Apple MacBook Pro 14 M3 Pro",
    "description": "Chip Apple M3 Pro 18GB RAM 512GB SSD màn hình Liquid Retina XDR sắc nét chuyên đồ họa.",
    "category": "Laptop",
    "brand": "Apple",
    "price": 49990000,
    "stock": 15,
    "rating": 4.9,
    "tags": ["apple", "macbook", "premium", "m3"],
    "is_active": true,
    "created_at": "2026-01-10T08:00:00Z"
  }
  ```

---

### 4.2. Lấy thông tin Document theo ID (`GET /index/_doc/id`)

- **Cú pháp (Syntax):**
  ```http
  GET /<index_name>/_doc/<id>
  ```
- **Ví dụ trong Dev Tools:**
  ```http
  GET /products/_doc/1
  ```

---

### 4.3. Cập nhật một phần Document (`POST /index/_update/id`)

- **Cú pháp (Syntax):**
  ```http
  POST /<index_name>/_update/<id>
  {
    "doc": {
      "<field_to_update>": "<new_value>"
    }
  }
  ```
- **Ví dụ trong Dev Tools:**
  ```http
  POST /products/_update/1
  {
    "doc": {
      "price": 47990000,
      "stock": 12
    }
  }
  ```

---

### 4.4. Nạp dữ liệu hàng loạt bằng Bulk API (`POST /_bulk`)

- **Cú pháp (Syntax):**
  ```http
  POST /_bulk
  { "index" : { "_index" : "<index_name>", "_id" : "<id1>" } }
  { <document_json_1> }
  { "index" : { "_index" : "<index_name>", "_id" : "<id2>" } }
  { <document_json_2> }
  ```
- **Ví dụ trong Dev Tools:**
  ```http
  POST /_bulk
  { "index" : { "_index" : "products", "_id" : "2" } }
  { "id": 2, "name": "Laptop Gaming ASUS ROG Strix G16", "description": "Intel Core i9 14900HX RTX 4070 màn hình 240Hz siêu mượt cho game thủ.", "category": "Laptop", "brand": "ASUS", "price": 45990000, "stock": 8, "rating": 4.8, "tags": ["gaming", "rog", "asus"], "is_active": true, "created_at": "2026-02-01T10:00:00Z" }
  { "index" : { "_index" : "products", "_id" : "3" } }
  { "id": 3, "name": "Điện thoại Samsung Galaxy S24 Ultra 512GB", "description": "Snapdragon 8 Gen 3 camera 200MP tích hợp Galaxy AI thông minh vượt trội.", "category": "Điện thoại", "brand": "Samsung", "price": 31990000, "stock": 25, "rating": 4.7, "tags": ["samsung", "flagship", "ai"], "is_active": true, "created_at": "2026-02-15T09:30:00Z" }
  { "index" : { "_index" : "products", "_id" : "4" } }
  { "id": 4, "name": "Bàn phím cơ không dây Keychron Q1 Pro", "description": "Khung nhôm CNC kết nối Bluetooth/Type-C switch Gateron Jupiter gõ cực êm.", "category": "Phụ kiện", "brand": "Keychron", "price": 4290000, "stock": 30, "rating": 4.9, "tags": ["keyboard", "custom", "wireless"], "is_active": true, "created_at": "2026-03-01T14:00:00Z" }
  { "index" : { "_index" : "products", "_id" : "5" } }
  { "id": 5, "name": "Màn hình Dell UltraSharp U2724D 27 inch 2K", "description": "Tấm nền IPS Black độ tương phản 2000:1 tần số quét 120Hz chuẩn màu đồ họa.", "category": "Màn hình", "brand": "Dell", "price": 11500000, "stock": 10, "rating": 4.8, "tags": ["monitor", "dell", "ultrasharp"], "is_active": true, "created_at": "2026-03-10T11:00:00Z" }
  ```

---

## Phần 5: Query DSL - Tìm Kiếm Chuyên Sâu & BM25 Relevance

### 5.1. Full-Text Search với BM25 Relevance & Boosting (`match` & `multi_match`)

- **Cú pháp (Syntax):**
  ```http
  GET /<index_name>/_search
  {
    "query": {
      "multi_match": {
        "query": "<từ_khóa>",
        "fields": ["<field1>^<hệ_số_boost>", "<field2>"]
      }
    },
    "highlight": {
      "fields": {
        "<field_name>": {}
      }
    }
  }
  ```
- **Ví dụ trong Dev Tools (Ưu tiên điểm số cho tên sản phẩm `name^2` hơn `description`):**
  ```http
  GET /products/_search
  {
    "query": {
      "multi_match": {
        "query": "laptop màn hình đồ họa",
        "fields": ["name^2", "description"]
      }
    },
    "highlight": {
      "pre_tags": ["<mark>"],
      "post_tags": ["</mark>"],
      "fields": {
        "description": {}
      }
    }
  }
  ```

---

### 5.2. Autocomplete Search (Tìm kiếm theo tiền tố với Edge N-Gram)

- **Cú pháp (Syntax):**
  ```http
  GET /<index_name>/_search
  {
    "query": {
      "match": {
        "<field>.autocomplete": "<tiền_tố>"
      }
    }
  }
  ```
- **Ví dụ trong Dev Tools (Gõ `"mac"` tìm ra MacBook, gõ `"key"` tìm ra Keychron):**
  ```http
  GET /products/_search
  {
    "query": {
      "match": {
        "name.autocomplete": "mac"
      }
    }
  }
  ```

---

### 5.3. Fuzzy Query (Tìm kiếm chịu lỗi chính tả - Typo Tolerance)

- **Cú pháp (Syntax):**
  ```http
  GET /<index_name>/_search
  {
    "query": {
      "fuzzy": {
        "<field_name>": {
          "value": "<từ_gõ_sai>",
          "fuzziness": "AUTO"
        }
      }
    }
  }
  ```
- **Ví dụ trong Dev Tools (Gõ sai `"Samssung"` hoặc `"Samsng"` vẫn ra Samsung):**
  ```http
  GET /products/_search
  {
    "query": {
      "fuzzy": {
        "name": {
          "value": "Samssung",
          "fuzziness": "AUTO"
        }
      }
    }
  }
  ```

---

### 5.4. Compound Bool Query: Kết hợp `must`, `filter`, `should`, `must_not`

- **Cú pháp (Syntax):**
  ```http
  GET /<index_name>/_search
  {
    "query": {
      "bool": {
        "must": [ ... ],       // Phải thỏa mãn & TÍNH ĐIỂM BM25
        "filter": [ ... ],     // Phải thỏa mãn & KHÔNG TÍNH ĐIỂM (Được Cache trong RAM)
        "should": [ ... ],     // Có thì được CỘNG ĐIỂM ƯU TIÊN
        "must_not": [ ... ]    // BẮT BUỘC LOẠI BỎ
      }
    }
  }
  ```
- **Ví dụ trong Dev Tools:**
  - Tìm sản phẩm có từ khóa _"gaming"_ trong tên (`must`).
  - Thuộc danh mục _"Laptop"_ và giá trong khoảng 30M đến 50M (`filter`).
  - Ưu tiên đưa sản phẩm có tag _"rog"_ lên đầu (`should`).
  - Loại bỏ thương hiệu _"Dell"_ (`must_not`).

  ```http
  GET /products/_search
  {
    "query": {
      "bool": {
        "must": [
          { "match": { "name": "gaming" } }
        ],
        "filter": [
          { "term": { "category": "Laptop" } },
          {
            "range": {
              "price": {
                "gte": 30000000,
                "lte": 50000000
              }
            }
          }
        ],
        "should": [
          { "term": { "tags": "rog" } }
        ],
        "must_not": [
          { "term": { "brand": "Dell" } }
        ]
      }
    }
  }
  ```

---

## Phần 6: Aggregations - Thống Kê & Phân Tích Dữ Liệu

Aggregations là tính năng giúp Elasticsearch phân tích thống kê hàng triệu bản ghi chỉ trong vài mili-giây.

---

### 6.1. Terms Aggregation (Phân nhóm & đếm theo danh mục / thương hiệu)

- **Cú pháp (Syntax):**
  ```http
  GET /<index_name>/_search
  {
    "size": 0,
    "aggs": {
      "<tên_thống_kê>": {
        "terms": {
          "field": "<keyword_field>"
        }
      }
    }
  }
  ```
- **Ví dụ trong Dev Tools:**
  ```http
  GET /products/_search
  {
    "size": 0,
    "aggs": {
      "categories_count": {
        "terms": {
          "field": "category"
        }
      },
      "brands_count": {
        "terms": {
          "field": "brand"
        }
      }
    }
  }
  ```

---

### 6.2. Stats Aggregation (Tính Giá Min, Max, Avg, Sum)

- **Cú pháp (Syntax):**
  ```http
  GET /<index_name>/_search
  {
    "size": 0,
    "aggs": {
      "<tên_thống_kê>": {
        "stats": {
          "field": "<numeric_field>"
        }
      }
    }
  }
  ```
- **Ví dụ trong Dev Tools (Thống kê giá sản phẩm):**
  ```http
  GET /products/_search
  {
    "size": 0,
    "aggs": {
      "price_statistics": {
        "stats": {
          "field": "price"
        }
      }
    }
  }
  ```
- **Kết quả trả về:**
  ```json
  "price_statistics": {
    "count": 5,
    "min": 4290000.0,
    "max": 49990000.0,
    "avg": 28752000.0,
    "sum": 143760000.0
  }
  ```
