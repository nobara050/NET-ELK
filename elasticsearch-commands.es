# ==============================================================================================
# ELASTICSEARCH DEEP-DIVE HANDS-ON CHEATSHEET
# Chạy trực tiếp trong Kibana Dev Tools Console: http://localhost:5601/app/dev_tools#/console
# ==============================================================================================

# ==============================================================================================
# PHẦN 1: HẠ TẦNG, CLUSTER VÀ SHARDS (Cluster & Shard Management)
# ==============================================================================================

# 1.1 Kiểm tra trạng thái Cluster
# Output trả về:
# - status: "green" (tất cả primary + replica đều active), "yellow" (primary active, thiếu replica), "red" (thiếu primary)
# - number_of_nodes: số lượng node đang chạy
# - active_primary_shards: số shard chính đang hoạt động
GET /_cluster/health

# 1.2 Liệt kê thông số các Node trong Cluster (RAM, CPU, JVM Heap, Master node)
GET /_cat/nodes?v&h=name,ip,node.role,master,heap.percent,ram.percent,cpu,uptime

# 1.3 Liệt kê tất cả các Index hiện có
# Chú ý: health, status, index, docs.count, store.size
GET /_cat/indices?v&s=index

# 1.4 Kiểm tra phân bổ Shard (Primary vs Replica)
# Senior Question: "Tại sao single-node cluster lại có status yellow?"
# Trả lời: Do chỉ có 1 node nên không thể đặt Replica Shard sang node khác (tránh SPOF).
GET /_cat/shards?v


# ==============================================================================================
# PHẦN 2: SOI BẢN CHẤT INVERTED INDEX VỚI API _analyze
# Hiểu cách Analyzer, Tokenizer, Stemmer biến đổi văn bản thành Tokens
# ==============================================================================================

# 2.1 Standard Analyzer mặc định (Chuyển chữ thường, tách theo dấu cách và ký tự đặc biệt)
POST /_analyze
{
  "analyzer": "standard",
  "text": "Elasticsearch is FAST and powerful!"
}
# -> Tokens tạo ra: ["elasticsearch", "is", "fast", "and", "powerful"]

# 2.2 English Analyzer có Stemming (Đưa từ về dạng gốc)
POST /_analyze
{
  "analyzer": "english",
  "text": "Developer is developing several microservices and reporting logs"
}
# -> Tokens tạo ra: ["develop", "develop", "sever", "microservic", "report", "log"]
# Nhờ Stemming: Khi search "report", document chứa "reporting" vẫn khớp với điểm BM25 cao!

# 2.3 So sánh Keyword Tokenizer (Không bẻ từ, giữ nguyên 100% chuỗi)
POST /_analyze
{
  "tokenizer": "keyword",
  "text": "Senior .NET Developer"
}
# -> Tokens tạo ra: ["Senior .NET Developer"] (dùng cho exact match, filter, sort)


# ==============================================================================================
# PHẦN 3: TẠO INDEX VỚI CUSTOM ANALYZER (Edge N-gram Autocomplete) VÀ EXPLICIT MAPPING
# ==============================================================================================

# 3.1 Xóa index cũ nếu có
DELETE /drive_files

# 3.2 Tạo Index `drive_files` với cấu hình tối ưu + Custom Analyzer
PUT /drive_files
{
  "settings": {
    "number_of_shards": 1,
    "number_of_replicas": 0,
    "analysis": {
      "filter": {
        "autocomplete_filter": {
          "type": "edge_ngram",
          "min_gram": 2,
          "max_gram": 10
        }
      },
      "analyzer": {
        "autocomplete_analyzer": {
          "type": "custom",
          "tokenizer": "standard",
          "filter": [
            "lowercase",
            "autocomplete_filter"
          ]
        }
      }
    }
  },
  "mappings": {
    "properties": {
      "name": {
        "type": "text",
        "analyzer": "standard",
        "fields": {
          "keyword": {
            "type": "keyword"
          },
          "autocomplete": {
            "type": "text",
            "analyzer": "autocomplete_analyzer",
            "search_analyzer": "standard"
          }
        }
      },
      "content": {
        "type": "text",
        "analyzer": "english"
      },
      "extension": {
        "type": "keyword"
      },
      "size_bytes": {
        "type": "long"
      },
      "owner": {
        "type": "keyword"
      },
      "tags": {
        "type": "keyword"
      },
      "created_at": {
        "type": "date"
      }
    }
  }
}

# 3.3 Test thử Custom Autocomplete Analyzer vừa tạo
POST /drive_files/_analyze
{
  "analyzer": "autocomplete_analyzer",
  "text": "Report"
}
# -> Tokens tạo ra: ["re", "rep", "repo", "repor", "report"]
# Giúp người dùng gõ "rep" là đã tìm ra ngay file "Report.pdf" (Search-as-you-type)!


# ==============================================================================================
# PHẦN 4: DOCUMENT CRUD VÀ BULK API
# ==============================================================================================

# 4.1 Tạo 1 Document với Custom ID = 1
POST /drive_files/_doc/1
{
  "name": "Annual Financial Report 2026.pdf",
  "content": "Comprehensive financial analysis and budget planning for corporate division.",
  "extension": "pdf",
  "size_bytes": 10485760,
  "owner": "alice@company.com",
  "tags": ["finance", "confidential", "q4"],
  "created_at": "2026-01-15T08:30:00Z"
}

# 4.2 Lấy thông tin Document theo ID
GET /drive_files/_doc/1

# 4.3 Update 1 phần Document (Partial Update)
POST /drive_files/_update/1
{
  "doc": {
    "size_bytes": 11534336
  }
}

# 4.4 Nạp dữ liệu hàng loạt bằng BULK API (Cơ chế Logstash sử dụng để tối đa hóa throughput)
POST /_bulk
{ "index" : { "_index" : "drive_files", "_id" : "2" } }
{ "name": "System Architecture Design.docx", "content": "Microservices with .NET 10, Kafka event streaming, and Elasticsearch inverted index.", "extension": "docx", "size_bytes": 4194304, "owner": "bob@company.com", "tags": ["tech", "architecture"], "created_at": "2026-02-10T14:20:00Z" }
{ "index" : { "_index" : "drive_files", "_id" : "3" } }
{ "name": "Employee Payroll Summary.xlsx", "content": "Monthly salary calculation, tax deductions, and bonus rewards distribution.", "extension": "xlsx", "size_bytes": 2097152, "owner": "alice@company.com", "tags": ["finance", "payroll"], "created_at": "2026-03-01T09:00:00Z" }
{ "index" : { "_index" : "drive_files", "_id" : "4" } }
{ "name": "Kubernetes Cluster Deployment.pdf", "content": "Step by step setup for high availability k8s clusters with Docker containers.", "extension": "pdf", "size_bytes": 8388608, "owner": "charlie@company.com", "tags": ["devops", "k8s"], "created_at": "2026-03-15T11:45:00Z" }
{ "index" : { "_index" : "drive_files", "_id" : "5" } }
{ "name": "Product Roadmap Presentation.pptx", "content": "Quarterly strategic product features release notes and customer feedback.", "extension": "pptx", "size_bytes": 15728640, "owner": "bob@company.com", "tags": ["product", "strategy"], "created_at": "2026-03-20T16:10:00Z" }
{ "index" : { "_index" : "drive_files", "_id" : "6" } }
{ "name": "Elasticsearch Performance Tuning.pdf", "content": "Deep dive into JVM heap, Lucene segment merging, and query caching techniques.", "extension": "pdf", "size_bytes": 6291456, "owner": "charlie@company.com", "tags": ["tech", "database"], "created_at": "2026-04-02T10:00:00Z" }


# ==============================================================================================
# PHẦN 5: ADVANCED QUERY DSL (THỰC CHIẾN TÌM KIẾM & THUẬT TOÁN BM25)
# ==============================================================================================

# 5.1 Match Query (Full-Text Search có tính điểm Relevance BM25)
# Tìm từ khóa "report planning" trong content
GET /drive_files/_search
{
  "query": {
    "match": {
      "content": "report planning"
    }
  }
}

# 5.2 Term Query (Exact Match - Không qua Analyzer, tìm chính xác 100%)
# Tìm tất cả file có extension đúng là "pdf"
GET /drive_files/_search
{
  "query": {
    "term": {
      "extension": "pdf"
    }
  }
}

# 5.3 Autocomplete / Search-As-You-Type (Tìm theo trường Edge N-gram)
# Người dùng gõ "archit" -> Ra "System Architecture Design.docx"
GET /drive_files/_search
{
  "query": {
    "match": {
      "name.autocomplete": "archit"
    }
  }
}

# 5.4 Fuzzy Query (Chấp nhận gõ sai chính tả)
# Gõ sai "fiancial" -> ES tự sửa thành "financial"
GET /drive_files/_search
{
  "query": {
    "fuzzy": {
      "name": {
        "value": "fiancial",
        "fuzziness": "AUTO"
      }
    }
  }
}

# 5.5 Compound Query: Bool Query (must vs filter vs should vs must_not)
# Senior Question: "Khác biệt giữa must và filter?"
# - MUST: Tính điểm relevance (_score), không được cache.
# - FILTER: Khớp Yes/No chính xác, KHÔNG tính _score, ĐƯỢC CACHED trong RAM -> Hiệu năng cực nhanh!
GET /drive_files/_search
{
  "query": {
    "bool": {
      "must": [
        {
          "match": {
            "content": "elasticsearch performance"
          }
        }
      ],
      "filter": [
        {
          "term": {
            "extension": "pdf"
          }
        },
        {
          "range": {
            "size_bytes": {
              "gte": 1000000,
              "lte": 20000000
            }
          }
        }
      ],
      "should": [
        {
          "term": {
            "tags": "database"
          }
        }
      ],
      "must_not": [
        {
          "term": {
            "owner": "alice@company.com"
          }
        }
      ]
    }
  },
  "highlight": {
    "fields": {
      "content": {
        "pre_tags": ["<mark>"],
        "post_tags": ["</mark>"]
      }
    }
  }
}


# ==============================================================================================
# PHẦN 6: AGGREGATIONS (THỐNG KÊ & PHÂN TÍCH DỮ LIỆU)
# ==============================================================================================

# 6.1 Metric Aggregation: Thống kê dung lượng file (Min, Max, Avg, Sum, Percentiles P95/P99)
GET /drive_files/_search
{
  "size": 0,
  "aggs": {
    "file_size_stats": {
      "stats": {
        "field": "size_bytes"
      }
    },
    "file_size_percentiles": {
      "percentiles": {
        "field": "size_bytes",
        "percents": [50, 90, 95, 99]
      }
    }
  }
}

# 6.2 Bucket Aggregation: Group by theo định dạng file (extension)
# Tương đương: SELECT extension, COUNT(*) FROM drive_files GROUP BY extension
GET /drive_files/_search
{
  "size": 0,
  "aggs": {
    "group_by_extension": {
      "terms": {
        "field": "extension",
        "size": 10
      }
    }
  }
}

# 6.3 Sub-Aggregation (Nested Aggregation):
# Nhóm theo extension, VÀ trong mỗi nhóm tính dung lượng trung bình (avg size)
# Tương đương: SELECT extension, COUNT(*), AVG(size_bytes) FROM drive_files GROUP BY extension
GET /drive_files/_search
{
  "size": 0,
  "aggs": {
    "by_extension": {
      "terms": {
        "field": "extension"
      },
      "aggs": {
        "average_size": {
          "avg": {
            "field": "size_bytes"
          }
        }
      }
    }
  }
}
