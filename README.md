# Time Manager

## Giới thiệu
Time Manager là công cụ quản lý thời gian cá nhân được thiết kế đặc biệt dành riêng cho sinh viên năm cuối đang phải làm việc đa nhiệm với các mảng công việc song song: thực tập, khoá luận và hoàn thành môn học. Công cụ này được xây dựng cho 1 người dùng duy nhất (chính người phát triển) nhằm mục tiêu tối ưu hoá sự tập trung và nhắc nhở công việc một cách thiết thực nhất, không phải là một sản phẩm đa người dùng thương mại.

## Công nghệ sử dụng
- **Frontend:** Vue 3 (Vite)
- **Backend:** ASP.NET Core Web API
- **Database:** SQL Server

## Cách chạy dự án
Bạn có thể khởi động ứng dụng một cách nhanh chóng:
- Bấm đúp vào file `start.bat` nằm tại thư mục gốc để hệ thống tự động khởi chạy cả Backend và Frontend trong 2 cửa sổ riêng biệt.

Hoặc khởi động thủ công:
- **Backend:** Di chuyển vào thư mục `TimeManager.Api` và chạy `dotnet run`.
- **Frontend:** Di chuyển vào thư mục `time-manager-clone` và chạy `npm run dev`.

## Các quyết định thiết kế quan trọng
Dưới đây là các điểm thiết kế cốt lõi của dự án cùng với lý do áp dụng:

- **3 nhóm việc cố định (Thực tập/Khoá luận/Môn học):** Vì đây là 3 mảng công việc thật đang song song, không cần mở rộng thêm nhóm khác.
- **Tách riêng Ngày bắt đầu và Ngày hạn:** Để task hiện đúng khoảng thời gian cần chuẩn bị, không chỉ đúng 1 ngày cuối.
- **Task quá hạn vẫn hiện tới khi tick xong (không tự ẩn):** Tránh cảm giác an toàn giả khi việc thực ra chưa xong.
- **Badge đổi từ đếm ngày sang đếm tuần khi còn xa:** Giảm nhiễu thông tin khi chưa cần gấp.
- **Thông báo trình duyệt liệt kê tên task cụ thể, sắp xếp Khẩn trước:** Giúp biết ngay việc nào cần làm mà không cần mở app đọc hết danh sách.
- **Không có đăng nhập/phân quyền:** Chỉ 1 người dùng duy nhất, thêm vào là dư thừa.
- **Hiển thị hộp chọn gộp ngày & giờ hẹn chung (datetime-local):** Giúp thao tác nhập liệu tinh gọn, nhưng Backend vẫn nhận diện lưu trữ tách biệt ngày và giờ một cách an toàn.
- **Lọc và sắp xếp trực tiếp ở phía giao diện (Frontend-side):** Các hành động Lọc Danh mục/Mức độ và sắp xếp ưu tiên không gọi lại dữ liệu (API), mang lại tốc độ phản hồi tức thì không độ trễ.
- **Cơ chế chống thông báo trùng lặp theo ngày (localStorage):** Tránh trải nghiệm tồi tệ vì spam thông báo mỗi khi người dùng tắt bật hay quay lại trang (tab visibility change).
