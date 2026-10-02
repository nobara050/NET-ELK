# Cẩm Nang Cú Pháp KQL (Kibana Query Language) Toàn Diện (`demo-kql.md`)

Tài liệu này tổng hợp chi tiết toàn bộ cú pháp **KQL (Kibana Query Language)** từ cơ bản đến nâng cao, kèm bảng tra cứu nhanh (Cheatsheet) và các kịch bản truy vấn thực chiến trong dự án **`EcomSearchApi`** trên **Kibana Discover**.

---

## MỤC LỤC

1. [Tổng Quan Về KQL (Kibana Query Language)](#1-tổng-quan-về-kql-kibana-query-language)
2. [Phần 1: Cú Pháp KQL Cơ Bản (Basic Syntax)](#phần-1-cú-pháp-kql-cơ-bản-basic-syntax)
3. [Phần 2: Cú Pháp KQL Nâng Cao (Advanced Syntax)](#phần-2-cú-pháp-kql-nâng-cao-advanced-syntax)
4. [Phần 3: Bảng Tra Cứu Nhanh KQL (Quick Reference Cheatsheet)](#phần-3-bảng-tra-cứu-nhanh-kql-quick-reference-cheatsheet)
5. [Phần 4: Các Kịch Bản Truy Vấn Thực Chiến Trong Dự Án](#phần-4-các-kịch-bản-truy-vấn-thực-chiến-trong-dự-án)
6. [Phần 5: Mẹo Sử Dụng KQL Hiệu Quả Trên Kibana Discover](#phần-5-mẹo-sử-dụng-kql-hiệu-quả-trên-kibana-discover)

---

## 1. Tổng Quan Về KQL (Kibana Query Language)

- **KQL** là ngôn ngữ truy vấn mặc định trên thanh tìm kiếm của Kibana (từ phiên bản 7.x/8.x).
- **Đặc điểm nổi bật**:
  - Có tính năng **Gợi ý tự động (Auto-complete)** tên trường và giá trị trực tiếp khi gõ.
  - Cú pháp ngắn gọn, dễ đọc, không yêu cầu viết cú pháp JSON phức tạp như Elasticsearch Query DSL.
  - Tự động xử lý kiểu dữ liệu (Text, Keyword, Số, Ngày tháng, Boolean).

---

## Phần 1: Cú Pháp KQL Cơ Bản (Basic Syntax)

### 1.1. Tìm kiếm theo trường cụ thể (`field: value`)

- **Cú pháp (Syntax):**
  ```kql
  <tên_trường>: "<giá_trị>"
  ```
- **Ví dụ:**
  - Tìm log có `log_level` là `Error`:
    ```kql
    log_level: "Error"
    ```
  - Tìm log của ứng dụng `EcomSearchApi`:
    ```kql
    Properties.Application: "EcomSearchApi"
    ```

---

### 1.2. Tìm kiếm tự do không chỉ định trường (Free Text Search)

- **Cú pháp (Syntax):**
  ```kql
  "<từ_khóa>"
  ```
- **Ví dụ:**
  - Tìm bất kỳ log nào có chứa chữ `timeout`:
    ```kql
    "timeout"
    ```
- _Lưu ý:_ Khi tìm tự do, Kibana sẽ quét qua tất cả các trường text của document, tốc độ sẽ chậm hơn so với chỉ định tên trường cụ thể.

---

### 1.3. Toán tử logic `and` (Và - Bắt buộc thỏa mãn đồng thời)

- **Cú pháp (Syntax):**
  ```kql
  <điều_kiện_1> and <điều_kiện_2>
  ```
- **Ví dụ:**
  - Tìm log `Error` phát sinh trên server `LAPTOP-NOBARA`:
    ```kql
    log_level: "Error" and Properties.MachineName: "LAPTOP-NOBARA"
    ```

---

### 1.4. Toán tử logic `or` (Hoặc - Thỏa mãn 1 trong các điều kiện)

- **Cú pháp (Syntax):**
  ```kql
  <điều_kiện_1> or <điều_kiện_2>
  ```
- **Ví dụ:**
  - Bắt cả trường `Level` gốc và `log_level` do Logstash tạo:
    ```kql
    Level: "Error" or log_level: "Error" or Level: "Fatal"
    ```

---

### 1.5. Toán tử phủ định `not` (Loại trừ điều kiện)

- **Cú pháp (Syntax):**
  ```kql
  not <điều_kiện>
  ```
- **Ví dụ:**
  - Lọc tất cả các log ngoại trừ log `Information`:
    ```kql
    not log_level: "Information"
    ```

---

### 1.6. Sử dụng dấu ngoặc đơn `(...)` để nhóm điều kiện

- **Cú pháp (Syntax):**
  ```kql
  <điều_kiện_A> and (<điều_kiện_B1> or <điều_kiện_B2>)
  ```
- **Ví dụ:**
  - Tìm request vào Controller `Diagnostics` mà trả về mã lỗi 500 hoặc 502:
    ```kql
    Properties.RequestPath: /api/Diagnostics* and (Properties.StatusCode: 500 or Properties.StatusCode: 502)
    ```

---

## Phần 2: Cú Pháp KQL Nâng Cao (Advanced Syntax)

### 2.1. Ký tự đại diện Wildcard (`*`)

KQL hỗ trợ dấu sao `*` để đại diện cho 0 hoặc nhiều ký tự bất kỳ.

- **Cú pháp (Syntax):**
  ```kql
  <tên_trường>: <tiền_tố>*
  <tên_trường>: *<hậu_tố>
  <tên_trường>: *<chứa_chuỗi>*
  ```
- **Ví dụ:**
  - Khớp tất cả các đường dẫn API bắt đầu bằng `/api/Diagnostics`:
    ```kql
    Properties.RequestPath: /api/Diagnostics*
    ```
  - Tìm log có chứa từ `Payment` trong nội dung thông điệp:
    ```kql
    Message: *Payment*
    ```

---

### 2.2. So sánh số và khoảng giá trị (`>`, `<`, `>=`, `<=`, `to`)

KQL hỗ trợ so sánh trực tiếp trên các trường kiểu Số (Numeric) và Ngày tháng (Date).

- **Cú pháp (Syntax):**
  ```kql
  <trường_số>: > <giá_trị>
  <trường_số>: >= <giá_trị>
  <trường_số>: < <giá_trị>
  <trường_số>: <= <giá_trị>
  <trường_số>: [ <min> to <max> ]
  ```
- **Ví dụ:**
  - Tìm các request có độ trễ lớn hơn 25 mili-giây:
    ```kql
    Properties.Elapsed > 25
    ```
  - Tìm các request có mã HTTP từ 400 đến 499 (Client Error):
    ```kql
    Properties.StatusCode: [400 to 499]
    ```
  - Tìm các request có mã HTTP từ 500 trở lên (Server Error):
    ```kql
    Properties.StatusCode >= 500
    ```

---

### 2.3. Kiểm tra trường có tồn tại / Có dữ liệu hay không (`field: *`)

- **Cú pháp (Syntax):**
  ```kql
  <tên_trường>: *        # Tìm bản ghi CÓ trường này (Not Null)
  not <tên_trường>: *    # Tìm bản ghi BỊ THIẾU trường này (Is Null)
  ```
- **Ví dụ:**
  - Tìm tất cả các log có gắn Exception StackTrace:
    ```kql
    Exception: *
    ```
  - Tìm các log có chứa mã đơn hàng `OrderId`:
    ```kql
    Properties.OrderId: *
    ```

---

### 2.4. Truy vấn trường phân cấp lồng nhau (Nested / Dot Notation)

Trong Serilog và Logstash, các thuộc tính tùy chỉnh được đặt bên trong object `Properties`. KQL sử dụng dấu chấm `.` để truy cập:

- **Cú pháp (Syntax):**
  ```kql
  Properties.<tên_thuộc_tính>: "<giá_trị>"
  ```
- **Ví dụ:**
  ```kql
  Properties.ServiceName: "InventorySyncService"
  ```

---

## Phần 3: Bảng Tra Cứu Nhanh KQL (Quick Reference Cheatsheet)

| Nhu cầu tìm kiếm      | Cú pháp KQL mẫu                                  | Ý nghĩa                                         |
| :-------------------- | :----------------------------------------------- | :---------------------------------------------- |
| **Khớp chính xác**    | `log_level: "Error"`                             | Lấy các log có mức độ là Error.                 |
| **Tìm nhiều giá trị** | `log_level: ("Error" or "Fatal")`                | Lấy log Error hoặc Fatal.                       |
| **Loại trừ**          | `not log_level: "Information"`                   | Bỏ qua toàn bộ log Info.                        |
| **Bắt đầu bằng**      | `Properties.RequestPath: /api/Search*`           | Lấy tất cả endpoint bắt đầu bằng `/api/Search`. |
| **Chứa chuỗi**        | `Message: *VNPAY*`                               | Lấy các log có chữ VNPAY trong nội dung.        |
| **So sánh lớn hơn**   | `Properties.Elapsed > 50`                        | Request chạy chậm trên 50ms.                    |
| **Khoảng giá trị**    | `Properties.StatusCode: [500 to 599]`            | Lọc toàn bộ mã lỗi HTTP 5xx.                    |
| **Kiểm tra tồn tại**  | `Exception: *`                                   | Lọc các log có Exception.                       |
| **Kết hợp phức tạp**  | `log_level: "Error" and Properties.Elapsed > 20` | Lỗi VÀ chạy trên 20ms.                          |

---

## Phần 4: Các Kịch Bản Truy Vấn Thực Chiến Trong Dự Án

Dưới đây là bộ câu lệnh KQL chuẩn bạn có thể copy và paste trực tiếp vào ô tìm kiếm trên Kibana Discover trong lúc Demo:

---

### Kịch bản 1: Truy vết toàn bộ hành trình của 1 Request (Trace ID Flow)

- **Mục đích:** Khi nhận được phản ánh sự cố kèm mã RequestId, xem toàn bộ log từ đầu đến cuối của request đó.
- **Câu lệnh KQL:**
  ```kql
  Properties.RequestId: "0HNOVSES6SICU:00000015"
  ```

---

### Kịch bản 2: Tìm kiếm theo mã đơn hàng (`OrderId`)

- **Mục đích:** CSKH cần kiểm tra giao dịch của đơn hàng `ORD-998877`.
- **Câu lệnh KQL:**
  ```kql
  Properties.OrderId: "ORD-998877"
  ```

---

### Kịch bản 3: Lọc toàn bộ lỗi sập mã HTTP 5xx của hệ thống

- **Mục đích:** Kiểm tra các lỗi máy chủ nghiêm trọng (500 Internal Server Error, 502, 503).
- **Câu lệnh KQL:**
  ```kql
  Properties.StatusCode >= 500
  ```

---

### Kịch bản 4: Tìm kiếm các Exception nghẽn kết nối Database

- **Mục đích:** Kiểm tra các lỗi sập kết nối PostgreSQL (`Connection pool exhausted`).
- **Câu lệnh KQL:**
  ```kql
  Exception: *Connection pool exhausted*
  ```

---

### Kịch bản 5: Tìm các API tìm kiếm chạy chậm (Slow Search Queries)

- **Mục đích:** Tối ưu hóa hiệu năng câu truy vấn tìm kiếm sản phẩm.
- **Câu lệnh KQL:**
  ```kql
  Properties.RequestPath: /api/Search* and Properties.Elapsed > 30
  ```

---

### Kịch bản 6: Lọc tất cả log của một Service cụ thể

- **Mục đích:** Kiểm tra riêng dịch vụ đồng bộ kho `InventorySyncService`.
- **Câu lệnh KQL:**
  ```kql
  Properties.ServiceName: "InventorySyncService"
  ```

---

## Phần 5: Mẹo Sử Dụng KQL Hiệu Quả Trên Kibana Discover

1. **Sử dụng Auto-complete (Phím Tab / Enter)**:
   - Khi bạn gõ `Prop...`, Kibana sẽ tự động xổ danh sách các trường như `Properties.Elapsed`, `Properties.RequestId`. Bạn chỉ cần dùng phím mũi tên và ấn `Tab` để chọn.
2. **Thêm Cột Nhanh (Toggle Column)**:
   - Ở cột bên trái (Available fields), rê chuột vào `Properties.RequestId` $\rightarrow$ Nhấp dấu `+` để thêm cột vào bảng xem log.
3. **Lưu Truy Vấn Thường Dùng (Saved Search)**:
   - Sau khi gõ `log_level: "Error"`, nhấp nút **Save** góc trên bên phải $\rightarrow$ Đặt tên `Production Error Logs` để mở lại trong 1 click.
4. **Chia Sẻ Truy Vấn Trực Tiếp (Share Permalink)**:
   - Nhấp nút **Share** $\rightarrow$ **Permalinks** $\rightarrow$ Copy link để gửi chính xác tập log đang xem cho đồng nghiệp mà không cần chụp ảnh màn hình.
