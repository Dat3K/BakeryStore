# Bakery Store Project

## Giới thiệu
Bakery Store là một ứng dụng web được xây dựng bằng ASP.NET Core, cho phép quản lý và vận hành cửa hàng bánh trực tuyến. Project sử dụng các công nghệ hiện đại và được tổ chức theo mô hình MVC.

## Yêu cầu hệ thống
- .NET Core SDK (phiên bản 6.0 trở lên)
- Visual Studio 2019/2022, Visual Studio Code, hoặc JetBrains Rider
- Node.js và npm (cho phát triển front-end)

## Cài đặt và Chạy Project

### Sử dụng Visual Studio 2019/2022
1. Clone repository về máy local
2. Mở file `Web.sln` bằng Visual Studio
4. Nhấn F5 hoặc click vào nút Start để chạy project

### Sử dụng Visual Studio Code
1. Cài đặt C# extension cho VS Code
2. Mở folder project trong VS Code
3. Mở Terminal và chạy các lệnh sau:
   ```bash
   dotnet run
   hoặc
   dotnet watch --launch-profile "https"
   ```

### Sử dụng JetBrains Rider
1. Mở project bằng cách chọn file `Web.sln`
2. Đợi Rider restore các NuGet packages
4. Nhấn Run hoặc Debug để khởi động project

## Cấu hình

### Database
1. Mở hoặc file `appsettings.json` (hoặc tạo file nếu chưa có)
2. Điều chỉnh connection string phù hợp với môi trường local:
   ```json
   {
      "Logging": {
         "LogLevel": {
            "Default": "Information",
            "Microsoft.AspNetCore": "Warning"
         }
      },
      "AllowedHosts": "*",
      "ConnectionStrings": {
         "DefaultConnection": "Host=aspnet-dat3k.c.aivencloud.com;Port=21085;Username=avnadmin;Password=AVNS_1WZdxDR6KNlu5Esdf-Z;Database=defaultdb;SslMode=Require;Timeout=30;CommandTimeout=30;Maximum Pool Size=50;Minimum Pool Size=5;Connection Idle Lifetime=300;"
      },
      "Auth0": {
         "Domain": "dat3k.jp.auth0.com",
         "ClientId": "wOOzkwrwpGU6Xv4j5nJreMg8Zik7kebB",
         "ClientSecret": "AVNS_1WZdxDR6KNlu5Esdf-Z",
         "ManagementApiClientId": "FI3yLoRSmch0zjzvRqa8jmBYuIztFS8K",
         "ManagementApiClientSecret": "QVehwNNWuCYdw6OYDxKlOs1ZWkn1Dg8GtaO8b67A4kMhRRddP6gdfXEM0N4AbugT"
      },
      "Cloudinary": {
         "CloudName": "dr0p8brop",
         "ApiKey": "374948317124553",
         "ApiSecret": "PnuDIZ_-qIPPh1SDM4uMbSw-73E"
      },
      "Jwt": {
         "Secret": "your-super-secret-key-with-at-least-32-characters",
         "Issuer": "dat3k.jp.auth0.com",
         "Audience": "wOOzkwrwpGU6Xv4j5nJreMg8Zik7kebB",
         "ExpirationInMinutes": 60
      }
   }
   ```

## Cấu trúc Project
- `Areas`: Chứa các module của admin
- `Controllers`: Xử lý logic điều hướng
- `Models`: Định nghĩa các entity
- `Services`: Chứa business logic
- `Views`: Giao diện người dùng
- `wwwroot`: File tĩnh (CSS, JS, Images)

## Các tính năng chính
- Quản lý sản phẩm
- Giỏ hàng
- Xử lý đơn hàng
- Upload và quản lý hình ảnh
- Phân quyền người dùng

## Hỗ trợ
Nếu bạn gặp vấn đề trong quá trình cài đặt hoặc chạy project, vui lòng tạo issue trong repository hoặc liên hệ với team phát triển.

## License
[MIT License](LICENSE)
