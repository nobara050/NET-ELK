# Cẩm Nang Tìm Kiếm & Truy Vết Log Với KQL Trên Kibana Discover (`demo-search-log.md`)

Tài liệu này hướng dẫn chi tiết cách sử dụng **KQL (Kibana Query Language)** để tìm kiếm, lọc và điều tra sự cố (Troubleshooting & Log Investigation) trên giao diện **Kibana Discover**.

Mỗi phần đều được cấu trúc theo chuẩn: **Cú pháp lý thuyết (Syntax)** $\rightarrow$ **Ví dụ thực tế trong dự án (`EcomSearchApi`)** $\rightarrow$ **Giải thích chi tiết**.

---

## MỤC LỤC

1. [Tổng Quan Về KQL & Màn Hình Discover](#1-tổng-quan-về-kql--màn-hình-discover)
2. [Phần 1: Cú Pháp KQL Cơ Bản (Basic Syntax)](#phần-1-cú-pháp-kql-cơ-bản-basic-syntax)
3. [Phần 2: Cú Pháp KQL Nâng Cao (Advanced Syntax)](#phần-2-cú-pháp-kql-nâng-cao-advanced-syntax)
4. [Phần 3: Các Kịch Bản Truy Vết Sự Cố Thực Tế (Real-World Use Cases)](#phần-3-các-kịch-bản-truy-vết-sự-cố-thực-tế-real-world-use-cases)
5. [Phần 4: Kỹ Năng Thao Tác Nâng Cao Trên Kibana Discover](#phần-4-kỹ-năng-thao-tác-nâng-cao-trên-kibana-discover)

---

## 1. Tổng Quan Về KQL & Màn Hình Discover

* **Kibana Discover** là trung tâm tìm kiếm và xem log theo thời gian thực của ELK Stack.
* **KQL (Kibana Query Language)** là ngôn ngữ truy vấn mặc định, có tính năng tự động gợi ý (Auto-complete) tên trường và giá trị, giúp tìm kiếm cực kỳ trực quan và nhanh chóng.

---

## Phần 1: Cú Pháp KQL Cơ Bản (Basic Syntax)

### 1.1. Khớp chính xác trường và giá trị (`field: "value"`)
* **Cú pháp (Syntax):**
  ```kql
  <tên_trường>: "<giá_trị>"
  ```
* **Ví dụ trong dự án (Lọc toàn bộ log Error):**
  ```kql
  log_level: "Error"
  ```
* **Giải thích:** Trả về tất cả các document có trường `log_level` mang giá trị `"Error"`.

---

### 1.2. Toán tử logic `and` (Bắt buộc thỏa mãn đồng thời)
* **Cú pháp (Syntax):**
  ```kql
  <điều_kiện_1> and <điều_kiện_2>
  ```
* **Ví dụ trong dự án (Tìm log Error xảy ra trên máy chủ `LAPTOP-NOBARA`):**
  ```kql
  log_level: "Error" and Properties.MachineName: "LAPTOP-NOBARA"
  ```

---

### 1.3. Toán tử logic `or` (Thỏa mãn 1 trong các điều kiện)
* **Cú pháp (Syntax):**
  ```kql
  <điều_kiện_1> or <điều_kiện_2>
  ```
* **Ví dụ trong dự án (Bắt cả cấp độ Error lẫn Fatal):**
  ```kql
  Level: "Error" or Level: "Fatal" or log_level: "Error"
  ```

---

### 1.4. Toán tử phủ định `not` (Loại trừ giá trị)
* **Cú pháp (Syntax):**
  ```kql
  not <điều_kiện>
  ```
* **Ví dụ trong dự án (Lọc bỏ tất cả các log cấp độ Information):**
  ```kql
  not log_level: "Information"
  ```

---

## Phần 2: Cú Pháp KQL Nâng Cao (Advanced Syntax)

### 2.1. Ký tự đại diện Wildcard (`*`)
* **Cú pháp (Syntax):**
  ```kql
  <tên_trường>: <tiền_tố>*
  hoặc
  <tên_trường>: *<hậu_tố>
  ```
* **Ví dụ trong dự án (Lọc tất cả request thuộc Controller `Diagnostics`):**
  ```kql
  Properties.RequestPath: /api/Diagnostics*
  ```
* **Giải thích:** Khớp với cả `/api/Diagnostics/trigger-error`, `/api/Diagnostics/simulate-exception`, v.v.

---

### 2.2. So sánh khoảng số & ngày tháng (`>`, `<`, `>=`, `<=`, `to`)
* **Cú pháp (Syntax):**
  ```kql
  <trường_số>: > <giá_trị>
  <trường_số>: [ <min> to <max> ]
  ```
* **Ví dụ 1 (Tìm các request API phản hồi chậm trên 20ms):**
  ```kql
  Properties.Elapsed > 20
  ```
* **Ví dụ 2 (Tìm các request trong khoảng từ 10ms đến 50ms):**
  ```kql
  Properties.Elapsed >= 10 and Properties.Elapsed <= 50
  ```

---

### 2.3. Kiểm tra trường có tồn tại hay không (`field: *`)
* **Cú pháp (Syntax):**
  ```kql
  <tên_trường>: *         # Kiểm tra trường TỒN TẠI dữ liệu
  not <tên_trường>: *     # Kiểm tra trường BỊ THIẾU / NULL
  ```
* **Ví dụ trong dự án (Tìm tất cả các log có chứa thông tin Exception):**
  ```kql
  Exception: *
  ```

---

## Phần 3: Các Kịch Bản Truy Vết Sự Cố Thực Tế (Real-World Use Cases)

Dưới đây là bộ câu lệnh KQL bạn có thể gõ trực tiếp trong buổi Demo để thể hiện năng lực điều tra log chuyên nghiệp:

---

### Kịch bản 1: Điều tra toàn bộ vòng đời của 1 Request (Trace Flow Investigation)
* **Tình huống:** Khách hàng báo bị lỗi khi bấm nút trên giao diện, họ cung cấp mã lỗi hoặc hệ thống ghi nhận `RequestId: "0HNOVSES6SICU:00000015"`.
* **Câu lệnh KQL:**
  ```kql
  Properties.RequestId: "0HNOVSES6SICU:00000015"
  ```
* **Ý nghĩa:** Kibana hiển thị toàn bộ chuỗi log từ lúc request bắt đầu gửi đến API $\rightarrow$ chạy qua Service $\rightarrow$ lúc ném lỗi và kết thúc trả về HTTP 500 theo đúng thứ tự thời gian.

---

### Kịch bản 2: Tìm kiếm theo mã đơn hàng cụ thể (`OrderId`)
* **Tình huống:** Kế toán/CSKH cần kiểm tra log giao dịch của đơn hàng `ORD-998877`.
* **Câu lệnh KQL:**
  ```kql
  Properties.OrderId: "ORD-998877"
  ```
* **Hoặc tìm kiếm trong chuỗi Message:**
  ```kql
  Message: *ORD-998877*
  ```

---

### Kịch bản 3: Phát hiện các lỗi sập mã HTTP 5xx của toàn bộ hệ thống
* **Tình huống:** DevOps cần lọc nhanh tất cả các request bị lỗi phía máy chủ (500 Internal Server Error, 502 Bad Gateway, 504 Gateway Timeout).
* **Câu lệnh KQL:**
  ```kql
  Properties.StatusCode >= 500
  ```

---

### Kịch bản 4: Tìm kiếm các Exception sập Database Connection
* **Tình huống:** Kiểm tra xem hôm nay có bao nhiêu lần ứng dụng bị cạn kiệt Connection Pool kết nối PostgreSQL.
* **Câu lệnh KQL:**
  ```kql
  Exception: *Connection pool exhausted*
  ```

---

### Kịch bản 5: Tìm kiếm API bị nghẽn hiệu năng (Slow API Requests)
* **Tình huống:** Rà soát các endpoint tìm kiếm chạy chậm hơn 50 mili-giây để tối ưu chỉ mục Elasticsearch.
* **Câu lệnh KQL:**
  ```kql
  Properties.RequestPath: /api/Search* and Properties.Elapsed > 50
  ```

---

## Phần 4: Kỹ Năng Thao Tác Nâng Cao Trên Kibana Discover

Khi đứng thuyết trình, các thao tác UI mượt mà này sẽ giúp bạn ghi điểm rất lớn:

1. **Thêm/bớt cột hiển thị (Custom Column View)**:
   * Ở cột bên trái (Available fields), rê chuột vào các trường: `@timestamp`, `Properties.StatusCode`, `Properties.Elapsed`, `Properties.RequestId`, `MessageTemplate`.
   * Bấm vào dấu **`+` (Toggle column in table)** để bảng log chỉ hiển thị gọn gàng các thông tin cần xem thay vì một khối JSON dài dòng.
2. **Lưu bộ lọc tìm kiếm (Saved Search)**:
   * Sau khi gõ xong câu lệnh KQL (ví dụ: `log_level: "Error"`), nhấp vào nút **`Save`** ở góc trên bên phải $\rightarrow$ Đặt tên: `All Production Errors`.
   * Lần sau chỉ cần bấm mở lại mà không cần gõ lại câu lệnh.
3. **Mở rộng sang công cụ vẽ biểu đồ (Explore in Lens)**:
   * Khi đang lọc một tập log trên Discover, nhấp vào nút **`Explore in Lens`** ở trên cùng.
   * Kibana sẽ tự động mang toàn bộ log đó sang màn hình Lens để bạn vẽ biểu đồ chỉ trong 2 click chuột!
