# Hướng Dẫn Xây Dựng Kibana Observability Dashboard (`demo-dashboard.md`)

Tài liệu này hướng dẫn chi tiết cách tự tay xây dựng một **Hệ Thống Giám Sát Tập Trung (System & API Observability Dashboard)** hoàn chỉnh trên Kibana Lens, phục vụ theo dõi sức khỏe và hiệu năng của ứng dụng `EcomSearchApi`.

---

## MỤC LỤC

1. [Tư Duy Cốt Lõi: Biến Log Thành Metrics Bằng Kibana Lens](#1-tư-duy-cốt-lõi-biến-log-thành-metrics-bằng-kibana-lens)
2. [Bố Cục Chuẩn Của Dashboard (Layout Architecture)](#2-bố-cục-chuẩn-của-dashboard-layout-architecture)
3. [Hướng Dẫn Tạo Từng Biểu Đồ (Step-by-Step Panels)](#3-hướng-dẫn-tạo-từng-biểu-đồ-step-by-step-panels)
   * [Panel 1: Thẻ Đếm Tổng Số Lỗi (Metric: `Total Errors`)](#panel-1-thẻ-đếm-tổng-số-lỗi-metric-total-errors)
   * [Panel 2: Phân Bố Log Theo Thời Gian (Stacked Bar: `Log Volume by Severity`)](#panel-2-phân-bố-log-theo-thời-gian-stacked-bar-log-volume-by-severity)
   * [Panel 3: Độ Trễ Phản Hồi API (Line Chart: `API Response Time`)](#panel-3-độ-trễ-phản-hồi-api-line-chart-api-response-time)
   * [Panel 4: Tỷ Lệ Mã Trạng Thái HTTP (Donut Chart: `HTTP Status Code Distribution`)](#panel-4-tỷ-lệ-mã-trạng-thái-http-donut-chart-http-status-code-distribution)
   * [Panel 5: Top API Được Gọi Nhiều Nhất (Horizontal Bar: `Top Requested Endpoints`)](#panel-5-top-api-được-gọi-nhiều-nhất-horizontal-bar-top-requested-endpoints)
   * [Panel 6: Bảng Truy Vết Lỗi Chi Tiết (Data Table: `Recent Error Logs & Tracing`)](#panel-6-bảng-truy-vết-lỗi-chi-tiết-data-table-recent-error-logs--tracing)
4. [Phần 4: Giám Sát Tài Nguyên Hạ Tầng (Stack Monitoring: CPU, RAM, Disk)](#phần-4-giám-sát-tài-nguyên-hạ-tầng-stack-monitoring-cpu-ram-disk)

---

## 1. Tư Duy Cốt Lõi: Biến Log Thành Metrics Bằng Kibana Lens

Khác với mô hình **Prometheus** (phải tạo endpoint `/metrics` để cào số liệu), **Elasticsearch** có bộ máy tính toán tổng hợp (**Aggregations Engine**):
* Trường số `Properties.Elapsed` $\rightarrow$ Tự tính trung bình (`Average`) để vẽ biểu đồ **Latency (ms)**.
* Trường `Level: "Error"` $\rightarrow$ Tự đếm (`Count`) để vẽ biểu đồ **Lưu lượng lỗi**.
* Trường `Properties.StatusCode` $\rightarrow$ Tự nhóm (`Group by`) để vẽ biểu đồ tròn **Tỷ lệ mã HTTP (200, 400, 500)**.

---

## 2. Bố Cục Chuẩn Của Dashboard (Layout Architecture)

```
+-----------------------------------------------------------------------------------+
|  [ 🔢 Total Errors: 3 ]        [ ⚡ Avg Latency: 18.5 ms ]   [ 📊 Total Requests: 45 ]  |  <- Hàng 1: Thẻ Metric KPI
+-----------------------------------------------------------------------------------+
| [ 📊 Log Volume by Severity (Stacked Bar) ]   | [ 🍩 HTTP Status Code (Donut) ]  |  <- Hàng 2: Lưu lượng & Tỷ lệ
+-----------------------------------------------+-----------------------------------+
| [ 📈 API Response Time ms (Line Chart) ]      | [ 📌 Top Endpoints (Bar Chart) ]  |  <- Hàng 3: Hiệu năng & Traffic
+-----------------------------------------------------------------------------------+
| [ 📋 Recent Error Logs & Request ID Tracing Table (Bảng tra cứu Trace ID) ]       |  <- Hàng 4: Bảng điều tra sự cố
+-----------------------------------------------------------------------------------+
```

---

## 3. Hướng Dẫn Tạo Từng Biểu Đồ (Step-by-Step Panels)

### Bắt đầu: Mở Dashboard & Công cụ Lens
1. Vào Menu **`☰`** $\rightarrow$ Chọn **`Dashboard`** (dưới mục Analytics).
2. Bấm nút màu xanh **`Create dashboard`** $\rightarrow$ Chọn **`Create visualization`**.
3. Đảm bảo góc trên bên trái đang chọn đúng Data View: **`App Logs`**.

---

### Panel 1: Thẻ Đếm Tổng Số Lỗi (Metric: `Total Errors`)
* **Loại biểu đồ**: Chọn **`Metric`** (góc trên bên phải).
* **Bộ lọc KQL (Filter trên cùng)**:
  ```kql
  log_level: "Error" or Level: "Error"
  ```
* **Cấu hình**:
  * **Primary metric**: Chọn `# Records` (hàm `Count`).
  * Nhấp vào ô đó $\rightarrow$ Đổi **Display name** thành: `Total Errors`.
  * **Màu sắc**: Chọn màu **Đỏ (Red)**.
* **Lưu lại**: Bấm **`Save and return`**.

---

### Panel 2: Phân Bố Log Theo Thời Gian (Stacked Bar: `Log Volume by Severity`)
* **Loại biểu đồ**: Chọn **`Bar`** $\rightarrow$ Chọn kiểu **`Stacked`**.
* **Cấu hình các trục**:
  * **Horizontal axis (Trục ngang)**: Kéo `@timestamp` vào $\rightarrow$ Display name: `Timestamp`.
  * **Vertical axis (Trục dọc)**: Kéo `# Records` vào (hàm `Count`) $\rightarrow$ Display name: `Log Count`.
  * **Breakdown (Phân loại màu)**: Kéo trường **`log_level.keyword`** (hoặc `Level.keyword`) vào.
* **Tùy biến màu sắc**:
  * Nhấp vào thanh Breakdown bên phải $\rightarrow$ Gán màu **Đỏ** cho `Error`, màu **Xanh lá/Xanh dương** cho `Information`.
* **Panel Title**: `Log Volume by Severity`.
* **Lưu lại**: Bấm **`Save and return`**.

---

### Panel 3: Độ Trễ Phản Hồi API (Line Chart: `API Response Time`)
* **Loại biểu đồ**: Chọn **`Line`** (hoặc *Area*).
* **Cấu hình các trục**:
  * **Horizontal axis**: Kéo `@timestamp` vào $\rightarrow$ Display name: `Timestamp`.
  * **Vertical axis**: Kéo trường **`Properties.Elapsed`** vào $\rightarrow$ Chọn hàm **`Average`** $\rightarrow$ Display name: `Avg Latency (ms)`.
* **Panel Title**: `API Response Time (ms)`.
* **Lưu lại**: Bấm **`Save and return`**.

---

### Panel 4: Tỷ Lệ Mã Trạng Thái HTTP (Donut Chart: `HTTP Status Code Distribution`)
* **Loại biểu đồ**: Chọn **`Donut`** (hoặc *Pie*).
* **Cấu hình**:
  * **Metric (Độ lớn lát cắt)**: `# Records` (hàm `Count`).
  * **Slices (Cắt lát theo)**: Kéo trường **`Properties.StatusCode.keyword`** (hoặc `StatusCode`) vào.
* **Tùy biến màu**: Mã `200` (Xanh lá), Mã `500` (Đỏ), Mã `400` (Vàng).
* **Panel Title**: `HTTP Status Code Distribution`.
* **Lưu lại**: Bấm **`Save and return`**.

---

### Panel 5: Top API Được Gọi Nhiều Nhất (Horizontal Bar: `Top Requested Endpoints`)
* **Loại biểu đồ**: Chọn **`Horizontal Bar`** (hoặc *Bar*).
* **Cấu hình**:
  * **Horizontal axis**: `# Records` (hàm `Count`) $\rightarrow$ Display name: `Request Count`.
  * **Vertical axis**: Kéo trường **`Properties.RequestPath.keyword`** vào $\rightarrow$ Display name: `API Endpoint`.
* **Panel Title**: `Top Requested Endpoints`.
* **Lưu lại**: Bấm **`Save and return`**.

---

### Panel 6: Bảng Truy Vết Lỗi Chi Tiết (Data Table: `Recent Error Logs & Tracing`)
* **Loại biểu đồ**: Chọn **`Table`**.
* **Bộ lọc KQL (Filter trên cùng)**:
  ```kql
  log_level: "Error" or Level: "Error"
  ```
* **Cấu hình Rows (Các cột hiển thị)**:
  1. Kéo **`Properties.RequestId.keyword`** vào $\rightarrow$ Display name: `Trace / Request ID`.
  2. Kéo **`@timestamp`** vào $\rightarrow$ Display name: `Time`.
  3. Kéo **`Properties.RequestPath.keyword`** vào $\rightarrow$ Display name: `Endpoint`.
  4. Kéo **`MessageTemplate.keyword`** vào $\rightarrow$ Display name: `Error Message`.
* **Mục Metrics**: Kéo `# Records` (hàm `Count`) $\rightarrow$ Display name: `Count`.
* **Panel Title**: `Recent Error Logs & Tracing`.
* **Lưu lại**: Bấm **`Save and return`**.

---

## 4. Giám Sát Tài Nguyên Hạ Tầng (Stack Monitoring: CPU, RAM, Disk)

Kibana có sẵn một hệ thống Dashboard chuyên sâu dành riêng cho hạ tầng (Infrastructure Monitoring):

### Cách truy cập:
1. Vào Menu **`☰`** $\rightarrow$ Tìm mục **Management** $\rightarrow$ Chọn **`Stack Monitoring`**.
2. Chọn **`Elasticsearch nodes`** $\rightarrow$ Nhấp vào node **`es01`**.

### Các thông số đo lường có sẵn:
* **JVM Heap Usage**: Theo dõi lượng RAM mà Java cấp phát cho Elasticsearch (ví dụ: `256MB / 512MB`).
* **CPU Utilization %**: Theo dõi % tải CPU của máy chủ.
* **Disk Space / Usage**: Theo dõi dung lượng ổ đĩa còn trống lưu trữ index.
* **Search / Indexing Latency & Throughput**: Tốc độ xử lý truy vấn tìm kiếm và nạp tài liệu mỗi giây.
