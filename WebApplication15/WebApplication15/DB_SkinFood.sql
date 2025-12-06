USE master;
GO

-- ==================================================================================
-- BƯỚC 1: CỨU HỘ & DỌN DẸP DATABASE CŨ (XỬ LÝ LỖI SINGLE USER)
-- ==================================================================================
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'DB_SkinFood')
BEGIN
    -- Đá văng tất cả kết nối đang bị kẹt, đưa về Single User để xóa
    ALTER DATABASE DB_SkinFood SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    -- Xóa Database cũ
    DROP DATABASE DB_SkinFood1;
END
GO

-- ==================================================================================
-- BƯỚC 2: TẠO DATABASE MỚI
-- ==================================================================================
CREATE DATABASE DB_SkinFood1;
GO

USE DB_SkinFood1;
GO

-- ==================================================================================
-- BƯỚC 3: TẠO CẤU TRÚC BẢNG (ĐÃ CẬP NHẬT ĐẦY ĐỦ CỘT)
-- ==================================================================================

-- 1. NGƯỜI DÙNG
CREATE TABLE NguoiDung (
    MaND INT PRIMARY KEY IDENTITY(1,1),
    HoTen NVARCHAR(100) NOT NULL,
    SoDienThoai NVARCHAR(20),
    DiaChi NVARCHAR(255),
    GioiTinh NVARCHAR(10),
    NgaySinh DATE,
    NgayTao DATETIME DEFAULT CURRENT_TIMESTAMP
);
GO

-- 2. TÀI KHOẢN
CREATE TABLE TaiKhoan (
    MaTK INT PRIMARY KEY IDENTITY(1,1),
    TenDangNhap NVARCHAR(100) UNIQUE NOT NULL,
    MatKhauHash VARCHAR(500) NOT NULL,
    VaiTro NVARCHAR(20) DEFAULT 'KhachHang',
    MaND INT UNIQUE NOT NULL,
    FOREIGN KEY (MaND) REFERENCES NguoiDung(MaND)
);
GO

-- 3. DANH MỤC
CREATE TABLE DanhMuc (
    MaDM INT PRIMARY KEY IDENTITY(1,1),
    TenDM NVARCHAR(100) NOT NULL
);
GO

-- 4. LOẠI SẢN PHẨM
CREATE TABLE LoaiSP (
    MaLoai INT PRIMARY KEY IDENTITY(1,1),
    TenLoai NVARCHAR(100) NOT NULL,
    MaDM INT NOT NULL,
    FOREIGN KEY (MaDM) REFERENCES DanhMuc(MaDM)
);
GO

-- 5. THƯƠNG HIỆU
CREATE TABLE ThuongHieu (
    MaTH INT PRIMARY KEY IDENTITY(1,1),
    TenTH NVARCHAR(100),
    QuocGia NVARCHAR(100)
);
GO

-- 6. SẢN PHẨM
CREATE TABLE SanPham (
    MaSP INT PRIMARY KEY IDENTITY(1,1),
    TenSP NVARCHAR(200),
    GiamGia DECIMAL(18,2),
    GiaBan DECIMAL(18,2),
    MoTa NVARCHAR(MAX),
    HinhAnh NVARCHAR(255),
    SoLuongTon INT DEFAULT 0,
    MaDM INT,
    MaTH INT,
    MaLoai INT,
    -- Các cột chi tiết mỹ phẩm
    NgaySanXuat DATETIME,
    HanSuDung DATETIME,
    LoaiDa NVARCHAR(50),
    VanDeChiRi NVARCHAR(255),
    TonDaMau NVARCHAR(50),
    ThanhPhanChiNhYeu NVARCHAR(MAX),
    SoLanSuDungMoiTuan INT DEFAULT 1,
    DoTinCay DECIMAL(3,1) DEFAULT 0,
    KichCoTieuChuan NVARCHAR(50),
    NgayNhapKho DATETIME DEFAULT GETDATE(),
    TrangThaiSanPham NVARCHAR(50) DEFAULT 'Kinh doanh',
    FOREIGN KEY (MaDM) REFERENCES DanhMuc(MaDM),
    FOREIGN KEY (MaTH) REFERENCES ThuongHieu(MaTH),
    FOREIGN KEY (MaLoai) REFERENCES LoaiSP(MaLoai)
);
GO

-- 7. ĐƠN HÀNG (ĐÃ TÍCH HỢP CỘT THANH TOÁN - KHÔNG CẦN ALTER NỮA)
CREATE TABLE DonHang (
    MaDH INT PRIMARY KEY IDENTITY(1,1),
    MaND INT,
    NgayDat DATETIME DEFAULT GETDATE(),
    TongTien DECIMAL(18,2),
    DiaChiGiaoHang NVARCHAR(255),
    -- Cột mới thêm trực tiếp tại đây
    TrangThaiThanhToan NVARCHAR(50) DEFAULT N'Chưa thanh toán',
    NgayThanhToan DATETIME NULL,
    PhuongThucThanhToan NVARCHAR(100) NULL,
    FOREIGN KEY (MaND) REFERENCES NguoiDung(MaND)
);
ALTER TABLE DonHang
ADD TenNguoiNhan NVARCHAR(100),
    SoDienThoai NVARCHAR(20),
    GhiChu NVARCHAR(255);

GO

-- 8. CHI TIẾT ĐƠN HÀNG
CREATE TABLE ChiTietDonHang (
    MaDH INT,
    MaSP INT,
    SoLuong INT,
    DonGia DECIMAL(18,2),
    PRIMARY KEY (MaDH, MaSP),
    FOREIGN KEY (MaDH) REFERENCES DonHang(MaDH),
    FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP)
);
GO

-- 9. ĐÁNH GIÁ
CREATE TABLE DanhGia (
    MaDG INT PRIMARY KEY IDENTITY(1,1),
    MaSP INT,
    MaND INT,
    NoiDung NVARCHAR(500),
    Diem INT CHECK (Diem BETWEEN 1 AND 5),
    NgayDanhGia DATETIME DEFAULT GETDATE(),
    DuocApprove BIT DEFAULT 0,
    TraLoiAdmin NVARCHAR(500),
    ThoiGianTraLoi DATETIME,
    FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP),
    FOREIGN KEY (MaND) REFERENCES NguoiDung(MaND)
);
GO

-- 10. CÁC BẢNG PHỤ TRỢ (THUỘC TÍNH, KHO)
CREATE TABLE ThuocTinhMyPham (
    MaThuocTinh INT PRIMARY KEY IDENTITY(1,1),
    TenThuocTinh NVARCHAR(100) NOT NULL,
    LoaiThuocTinh NVARCHAR(50) NOT NULL,
    MoTa NVARCHAR(255)
);
GO

CREATE TABLE SanPham_ThuocTinh (
    MaSP INT,
    MaThuocTinh INT,
    PRIMARY KEY (MaSP, MaThuocTinh),
    FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP) ON DELETE CASCADE,
    FOREIGN KEY (MaThuocTinh) REFERENCES ThuocTinhMyPham(MaThuocTinh)
);
GO

CREATE TABLE NhapHang (
    MaNhap INT PRIMARY KEY IDENTITY(1,1),
    MaSP INT NOT NULL,
    SoLuongNhap INT NOT NULL,
    NgayNhap DATETIME DEFAULT GETDATE(),
    NhaCungCap NVARCHAR(100),
    GiaVon DECIMAL(18,2),
    GhiChu NVARCHAR(255),
    FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP)
);
GO

CREATE TABLE XuatKho (
    MaXuat INT PRIMARY KEY IDENTITY(1,1),
    MaSP INT NOT NULL,
    SoLuongXuat INT NOT NULL,
    NgayXuat DATETIME DEFAULT GETDATE(),
    LyDoXuat NVARCHAR(50),
    GhiChu NVARCHAR(255),
    FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP)
);
GO

CREATE TABLE LienHe (
    MaLH INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100),
    Email NVARCHAR(100),
    SoDienThoai NVARCHAR(20),
    NoiDung NVARCHAR(1000),
    NgayGui DATETIME DEFAULT GETDATE()
);



-- ==================================================================================
-- BƯỚC 4: CHÈN DỮ LIỆU MẪU
-- ==================================================================================

-- NGƯỜI DÙNG
INSERT INTO NguoiDung (HoTen, SoDienThoai, DiaChi, GioiTinh, NgaySinh)
VALUES
    (N'Nguyễn Quỳnh Như', '0901234567', N'TP. Hồ Chí Minh', N'Nữ', '2003-05-12'),
    (N'Nguyễn Văn An', '0912345678', N'Hà Nội', N'Nam', '1999-08-22'),
    (N'Lê Thị Thu', '0987654321', N'Đà Nẵng', N'Nữ', '2000-10-05'),
    (N'Trần Minh Tâm', '0933222111', N'Cần Thơ', N'Nam', '1998-12-30'),
    (N'Phạm Hồng Hoa', '0977334455', N'Nha Trang', N'Nữ', '2001-06-18');
GO

-- TÀI KHOẢN
INSERT INTO TaiKhoan (TenDangNhap, MatKhauHash, VaiTro, MaND)
VALUES
    ('admin@skinfood.vn', 'admin123', N'Admin', 1),
    ('an@gmail.com', '123456', N'KhachHang', 2),
    ('thu@gmail.com', '123456', N'KhachHang', 3),
    ('tam@gmail.com', '123456', N'KhachHang', 4),
    ('hoa@gmail.com', '123456', N'KhachHang', 5);
GO

-- DANH MỤC
INSERT INTO DanhMuc (TenDM)
VALUES (N'Mỹ phẩm chăm sóc da mặt'), (N'Mỹ phẩm trang điểm'), (N'Mỹ phẩm tóc'),
       (N'Dược phẩm'), (N'Mỹ phẩm chăm sóc cơ thể'), (N'Nước hoa và phụ kiện');
GO

-- LOẠI SẢN PHẨM
INSERT INTO LoaiSP (TenLoai, MaDM)
VALUES
    (N'Sữa rửa mặt', 1), (N'Toner', 1), (N'Serum', 1), (N'Kem dưỡng da', 1), (N'Mặt nạ', 1),
    (N'Kem nền', 2), (N'Son môi', 2), (N'Phấn phủ', 2),
    (N'Dầu gội', 3), (N'Dầu xả', 3), (N'Tinh dầu dưỡng tóc', 3),
    (N'Kem trị mụn', 4), (N'Kem chống nắng', 4),
    (N'Sữa tắm', 5), (N'Kem dưỡng thể', 5),
    (N'Nước hoa', 6), (N'Phụ kiện làm đẹp', 6);
GO

-- THƯƠNG HIỆU
INSERT INTO ThuongHieu (TenTH, QuocGia)
VALUES (N'Innisfree', N'Hàn Quốc'), (N'The Face Shop', N'Hàn Quốc'), (N'Lancome', N'Pháp'),
       (N'L''Oreal', N'Pháp'), (N'Nature Republic', N'Hàn Quốc');
GO

-- SẢN PHẨM
INSERT INTO SanPham (TenSP, GiamGia, GiaBan, MoTa, HinhAnh, SoLuongTon, MaDM, MaTH, MaLoai,
                     NgaySanXuat, HanSuDung, LoaiDa, VanDeChiRi, ThanhPhanChiNhYeu, 
                     SoLanSuDungMoiTuan, KichCoTieuChuan)
VALUES
    (N'Sữa rửa mặt trà xanh', 0, 150000, N'Sữa rửa mặt chiết xuất trà xanh.', 'srm_traxanh.jpg', 50, 1, 1, 1, '2024-01-15', '2026-01-15', N'Dầu', N'Mụn|Thâm', N'Trà xanh, BHA', 2, '150ml'),
    (N'Kem dưỡng ẩm ban đêm', 10, 250000, N'Kem dưỡng ẩm phục hồi.', 'kemduong_bandem.jpg', 30, 1, 2, 4, '2024-02-01', '2026-02-01', N'Khô', N'Lão hóa', N'Retinol, Collagen', 1, '50ml'),
    (N'Mặt nạ đất sét', 5, 180000, N'Làm sạch sâu.', 'matna_datshet.jpg', 40, 1, 1, 5, '2024-01-20', '2025-01-20', N'Hỗn hợp', N'Mụn', N'Đất sét', 1, '100ml'),
    (N'Toner hoa hồng', 0, 200000, N'Cân bằng độ ẩm.', 'toner_hoahong.jpg', 60, 1, 3, 2, '2024-01-10', '2026-01-10', N'Khô', NULL, N'Hoa hồng', 1, '200ml'),
    (N'Serum vitamin C', 15, 350000, N'Làm sáng da.', 'serum_vitc.jpg', 25, 1, 4, 3, '2024-03-01', '2026-03-01', N'Dầu', N'Thâm', N'Vitamin C', 2, '30ml'),
    (N'Son môi đỏ tươi', 0, 180000, N'Son lì.', 'son_do_tuoi.jpg', 40, 2, 2, 7, '2024-02-15', '2025-02-15', NULL, NULL, N'Pigment', NULL, '4.5g'),
    (N'Dầu gội dưỡng tóc', 5, 120000, N'Giúp tóc mềm mượt.', 'daugoi_duongtoc.jpg', 30, 3, 5, 9, '2024-01-25', '2025-07-25', NULL, NULL, N'Biotin', NULL, '200ml'),
    (N'Nước hoa hồng nhẹ dịu', 10, 220000, N'Dịu da.', 'nuochoa_hoahong.jpg', 20, 6, 3, 17, '2024-01-05', '2025-01-05', N'Nhạy cảm', NULL, N'Hoa hồng', 2, '100ml'),
    (N'Nước hoa hồng', 10, 220000, N'Dịu da.', 'nuochoa_hoahong1.jpg', 20, 6, 3, 17, '2024-01-10', '2025-01-10', N'Hỗn hợp', NULL, N'Hoa hồng', 2, '100ml');
GO

-- ĐƠN HÀNG (Sẽ tự nhận giá trị mặc định 'Chưa thanh toán' cho cột TrangThaiThanhToan)
INSERT INTO DonHang (MaND, TongTien, DiaChiGiaoHang)
VALUES
    (2, 350000, N'Hà Nội'),
    (3, 530000, N'Đà Nẵng'),
    (4, 200000, N'Cần Thơ'),
    (5, 700000, N'Nha Trang');
GO

-- CHI TIẾT ĐƠN HÀNG
INSERT INTO ChiTietDonHang (MaDH, MaSP, SoLuong, DonGia)
VALUES
    (1, 1, 2, 150000), (1, 4, 1, 200000),
    (2, 3, 1, 180000), (2, 5, 1, 350000),
    (3, 4, 1, 200000), (4, 2, 2, 250000);
GO

-- ĐÁNH GIÁ
INSERT INTO DanhGia (MaSP, MaND, NoiDung, Diem, DuocApprove)
VALUES
    (1, 2, N'Tốt!', 5, 1), (2, 3, N'Ổn.', 4, 1),
    (3, 4, N'Hơi khô.', 3, 0), (5, 5, N'Sáng da.', 5, 1);
GO

-- THUỘC TÍNH VÀ LIÊN KẾT
INSERT INTO ThuocTinhMyPham (TenThuocTinh, LoaiThuocTinh, MoTa)
VALUES
    (N'Dành cho da dầu', N'LoaiDa', NULL), (N'Dành cho da khô', N'LoaiDa', NULL),
    (N'Trị mụn', N'VanDe', NULL), (N'Chống lão hóa', N'VanDe', NULL),
    (N'Retinol', N'CongNghe', NULL), (N'Vegan', N'ChungChi', NULL);
GO

INSERT INTO SanPham_ThuocTinh (MaSP, MaThuocTinh)
VALUES (1, 1), (1, 3), (2, 2), (2, 4), (5, 1), (5, 5);
GO

-- NHẬP HÀNG
INSERT INTO NhapHang (MaSP, SoLuongNhap, NgayNhap, NhaCungCap, GiaVon, GhiChu)
VALUES
    (1, 100, '2024-01-15', N'ABC Corp', 80000, N'Lô A'),
    (2, 50, '2024-02-01', N'XYZ Ltd', 150000, N'Lô B'),
    (3, 80, '2024-01-20', N'DEF Inc', 90000, N'Lô C'),
    (5, 30, '2024-03-01', N'GHI Co', 200000, N'Lô D'),
    (6, 120, '2024-02-15', N'JKL Group', 100000, N'Lô E');
GO

-- ==================================================================================
-- BƯỚC 5: CẬP NHẬT TRẠNG THÁI TỒN KHO & HẠN DÙNG (GIẢ LẬP)
-- ==================================================================================
UPDATE SanPham SET SoLuongTon = 150, NgaySanXuat = '2025-01-01', HanSuDung = '2027-01-01', TrangThaiSanPham = N'Kinh doanh' WHERE MaSP = 1;
UPDATE SanPham SET SoLuongTon = 45, NgaySanXuat = '2023-01-20', HanSuDung = '2026-01-20', TrangThaiSanPham = N'Sale xả hàng' WHERE MaSP = 2;
UPDATE SanPham SET SoLuongTon = 10, NgaySanXuat = '2023-10-01', HanSuDung = '2025-10-01', TrangThaiSanPham = N'Ngừng kinh doanh' WHERE MaSP = 5;
UPDATE SanPham SET SoLuongTon = 0, TrangThaiSanPham = N'Hết hàng' WHERE MaSP = 6;
GO

-- ==================================================================================
-- HOÀN TẤT VÀ KIỂM TRA
-- ==================================================================================
PRINT N'✅ Đã tạo lại Database DB_SkinFood thành công!';
PRINT N'✅ Đã có sẵn cột TrangThaiThanhToan trong bảng DonHang!';

-- Kiểm tra xem cột đã có chưa
SELECT * FROM DonHang;
-- Đổi tên bảng ChiTietDonHang thành ChiTietDonHangs
EXEC sp_rename 'ChiTietDonHang', 'ChiTietDonHangs';
GO