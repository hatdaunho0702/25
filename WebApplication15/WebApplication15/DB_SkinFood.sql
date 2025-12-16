USE master;
GO

-- ==================================================================================
-- BƯỚC 1: DỌN DẸP DATABASE CŨ
-- ==================================================================================
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'DB_SkinFood1')
BEGIN
    ALTER DATABASE DB_SkinFood1 SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
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
-- BƯỚC 3: TẠO CẤU TRÚC BẢNG
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

-- 5. THƯƠNG HIỆU (BRAND)
CREATE TABLE ThuongHieu (
    MaTH INT PRIMARY KEY IDENTITY(1,1),
    TenTH NVARCHAR(100),
    QuocGia NVARCHAR(100)
);
GO

-- 6. NHÀ CUNG CẤP (SUPPLIER - MỚI THÊM)
-- Đây là đơn vị cung cấp hàng hóa cho shop (khác với Thương hiệu)
CREATE TABLE NhaCungCap (
    MaNCC INT PRIMARY KEY IDENTITY(1,1),
    TenNCC NVARCHAR(100) NOT NULL,
    DiaChi NVARCHAR(255),
    SoDienThoai NVARCHAR(20),
    Email NVARCHAR(100),
    NguoiLienHe NVARCHAR(100)
);
GO

-- 7. SẢN PHẨM
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
    -- Thông tin chi tiết
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

-- 8. ĐƠN HÀNG
CREATE TABLE DonHang (
    MaDH INT PRIMARY KEY IDENTITY(1,1),
    MaND INT,
    NgayDat DATETIME DEFAULT GETDATE(),
    TongTien DECIMAL(18,2),
    DiaChiGiaoHang NVARCHAR(255),
    TenNguoiNhan NVARCHAR(100),
    SoDienThoai NVARCHAR(20),
    GhiChu NVARCHAR(255),
    TrangThaiThanhToan NVARCHAR(50) DEFAULT N'Chưa thanh toán',
    NgayThanhToan DATETIME NULL,
    PhuongThucThanhToan NVARCHAR(100) NULL,
    FOREIGN KEY (MaND) REFERENCES NguoiDung(MaND)
);
GO

-- 9. CHI TIẾT ĐƠN HÀNG (Đổi tên bảng số nhiều cho chuẩn logic)
CREATE TABLE ChiTietDonHangs (
    MaDH INT,
    MaSP INT,
    SoLuong INT,
    DonGia DECIMAL(18,2),
    PRIMARY KEY (MaDH, MaSP),
    FOREIGN KEY (MaDH) REFERENCES DonHang(MaDH),
    FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP)
);
GO

-- 10. ĐÁNH GIÁ
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

-- 11. CÁC BẢNG PHỤ TRỢ (THUỘC TÍNH)
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

-- ==================================================================================
-- 12. HỆ THỐNG KHO VÀ NHÀ CUNG CẤP (ĐÃ CHỈNH SỬA)
-- ==================================================================================

-- A. PHIẾU NHẬP (Lưu thông tin nhập từ Nhà Cung Cấp nào)
CREATE TABLE PhieuNhap (
    MaPN INT PRIMARY KEY IDENTITY(1,1),
    MaND INT, -- Người nhập kho
    MaNCC INT, -- Nhập từ Nhà Cung Cấp nào (MỚI)
    NgayNhap DATETIME DEFAULT GETDATE(),
    TongTien DECIMAL(18,2) DEFAULT 0,
    GhiChu NVARCHAR(500),
    FOREIGN KEY (MaND) REFERENCES NguoiDung(MaND),
    FOREIGN KEY (MaNCC) REFERENCES NhaCungCap(MaNCC)
);
GO

-- B. CHI TIẾT PHIẾU NHẬP
CREATE TABLE ChiTietPhieuNhap (
    MaPN INT,
    MaSP INT,
    SoLuong INT CHECK (SoLuong > 0),
    GiaNhap DECIMAL(18,2) CHECK (GiaNhap >= 0),
    ThanhTien DECIMAL(18,2),
    PRIMARY KEY (MaPN, MaSP),
    FOREIGN KEY (MaPN) REFERENCES PhieuNhap(MaPN) ON DELETE CASCADE,
    FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP)
);
GO

-- C. PHIẾU XUẤT (Xuất hủy, xuất tặng,...)
CREATE TABLE PhieuXuat (
    MaPX INT PRIMARY KEY IDENTITY(1,1),
    MaND INT,
    NgayXuat DATETIME DEFAULT GETDATE(),
    LyDoXuat NVARCHAR(255),
    GhiChu NVARCHAR(500),
    FOREIGN KEY (MaND) REFERENCES NguoiDung(MaND)
);
GO

-- D. CHI TIẾT PHIẾU XUẤT
CREATE TABLE ChiTietPhieuXuat (
    MaPX INT,
    MaSP INT,
    SoLuong INT CHECK (SoLuong > 0),
    PRIMARY KEY (MaPX, MaSP),
    FOREIGN KEY (MaPX) REFERENCES PhieuXuat(MaPX) ON DELETE CASCADE,
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
GO

-- ==================================================================================
-- BƯỚC 4: TẠO TRIGGER TỰ ĐỘNG CẬP NHẬT KHO
-- ==================================================================================
-- Trigger 1: Nhập hàng -> Tăng tồn kho
CREATE TRIGGER trg_CapNhatKho_Nhap
ON ChiTietPhieuNhap
AFTER INSERT
AS
BEGIN
    UPDATE sp
    SET sp.SoLuongTon = sp.SoLuongTon + i.SoLuong,
        sp.NgayNhapKho = GETDATE()
    FROM SanPham sp
    INNER JOIN inserted i ON sp.MaSP = i.MaSP;
END
GO

-- Trigger 2: Bán hàng (Đặt đơn) -> Giảm tồn kho
CREATE TRIGGER trg_CapNhatKho_BanHang
ON ChiTietDonHangs
AFTER INSERT
AS
BEGIN
    UPDATE sp
    SET sp.SoLuongTon = sp.SoLuongTon - i.SoLuong
    FROM SanPham sp
    INNER JOIN inserted i ON sp.MaSP = i.MaSP;
END
GO

-- Trigger 3: Xuất kho (Hủy/Tặng) -> Giảm tồn kho
CREATE TRIGGER trg_CapNhatKho_Xuat
ON ChiTietPhieuXuat
AFTER INSERT
AS
BEGIN
    UPDATE sp
    SET sp.SoLuongTon = sp.SoLuongTon - i.SoLuong
    FROM SanPham sp
    INNER JOIN inserted i ON sp.MaSP = i.MaSP;
END
GO

-- ==================================================================================
-- BƯỚC 5: CHÈN DỮ LIỆU MẪU
-- ==================================================================================

-- 1. NGƯỜI DÙNG
INSERT INTO NguoiDung (HoTen, SoDienThoai, DiaChi, GioiTinh, NgaySinh) VALUES
(N'Nguyễn Quỳnh Như', '0901234567', N'TP. Hồ Chí Minh', N'Nữ', '2003-05-12'),
(N'Nguyễn Văn An', '0912345678', N'Hà Nội', N'Nam', '1999-08-22'),
(N'Lê Thị Thu', '0987654321', N'Đà Nẵng', N'Nữ', '2000-10-05'),
(N'Trần Minh Tâm', '0933222111', N'Cần Thơ', N'Nam', '1998-12-30'),
(N'Phạm Hồng Hoa', '0977334455', N'Nha Trang', N'Nữ', '2001-06-18');

-- 2. TÀI KHOẢN
INSERT INTO TaiKhoan (TenDangNhap, MatKhauHash, VaiTro, MaND) VALUES
('admin@skinfood.vn', 'admin123', N'Admin', 1),
('an@gmail.com', '123456', N'KhachHang', 2),
('thu@gmail.com', '123456', N'KhachHang', 3),
('tam@gmail.com', '123456', N'KhachHang', 4),
('hoa@gmail.com', '123456', N'KhachHang', 5);

-- 3. DANH MỤC & THƯƠNG HIỆU & LOẠI SP
INSERT INTO DanhMuc (TenDM) VALUES (N'Mỹ phẩm chăm sóc da mặt'), (N'Mỹ phẩm trang điểm'), (N'Mỹ phẩm tóc'), (N'Dược phẩm'), (N'Mỹ phẩm chăm sóc cơ thể'), (N'Nước hoa');
INSERT INTO ThuongHieu (TenTH, QuocGia) VALUES (N'Innisfree', N'Hàn Quốc'), (N'The Face Shop', N'Hàn Quốc'), (N'Lancome', N'Pháp'), (N'L''Oreal', N'Pháp'), (N'Nature Republic', N'Hàn Quốc');
INSERT INTO LoaiSP (TenLoai, MaDM) VALUES (N'Sữa rửa mặt', 1), (N'Toner', 1), (N'Serum', 1), (N'Kem dưỡng da', 1), (N'Mặt nạ', 1), (N'Son môi', 2), (N'Dầu gội', 3);

-- 4. NHÀ CUNG CẤP (DỮ LIỆU MỚI)
INSERT INTO NhaCungCap (TenNCC, DiaChi, SoDienThoai, Email, NguoiLienHe) VALUES 
(N'Cty TNHH Innisfree Việt Nam', N'Q1, TP.HCM', '02838221122', 'contact@innisfree.vn', N'Ms. Lan'),
(N'Nhà Phân Phối Mỹ Phẩm Tâm An', N'Q3, TP.HCM', '0909998887', 'sales@taman.com', N'Mr. Hùng'),
(N'Cty XNK L''Oreal Group', N'Hoàn Kiếm, Hà Nội', '02439334455', 'support@loreal.com.vn', N'Ms. Mai');

-- 5. SẢN PHẨM (Khởi tạo số lượng tồn 0, sẽ tự tăng khi nhập hàng)
INSERT INTO SanPham (TenSP, GiaBan, SoLuongTon, MaDM, MaTH, MaLoai) VALUES
(N'Sữa rửa mặt trà xanh', 150000, 0, 1, 1, 1),
(N'Kem dưỡng ẩm ban đêm', 250000, 0, 1, 2, 4),
(N'Mặt nạ đất sét', 180000, 0, 1, 1, 5),
(N'Toner hoa hồng', 200000, 0, 1, 3, 2),
(N'Serum vitamin C', 350000, 0, 1, 4, 3),
(N'Son môi đỏ tươi', 180000, 0, 2, 2, 6);

-- 6. NHẬP HÀNG (SỬ DỤNG NHÀ CUNG CẤP)
-- Phiếu nhập 1: Nhập từ Innisfree VN
INSERT INTO PhieuNhap (MaND, MaNCC, TongTien, GhiChu) VALUES (1, 1, 15000000, N'Nhập hàng lô đầu năm');
-- Chi tiết phiếu nhập 1 (Trigger sẽ tự cộng kho: SP1 lên 100, SP3 lên 50)
INSERT INTO ChiTietPhieuNhap (MaPN, MaSP, SoLuong, GiaNhap, ThanhTien) VALUES 
(1, 1, 100, 80000, 8000000), 
(1, 3, 50, 90000, 4500000);

-- Phiếu nhập 2: Nhập từ Tâm An
INSERT INTO PhieuNhap (MaND, MaNCC, TongTien, GhiChu) VALUES (1, 2, 8000000, N'Nhập bổ sung serum');
-- Chi tiết phiếu nhập 2 (SP5 lên 60)
INSERT INTO ChiTietPhieuNhap (MaPN, MaSP, SoLuong, GiaNhap, ThanhTien) VALUES 
(2, 5, 60, 200000, 12000000);

-- 7. ĐƠN HÀNG (Trigger sẽ tự trừ kho)
INSERT INTO DonHang (MaND, TongTien, DiaChiGiaoHang) VALUES (2, 350000, N'Hà Nội');
INSERT INTO ChiTietDonHangs (MaDH, MaSP, SoLuong, DonGia) VALUES (1, 1, 2, 150000); -- Trừ kho SP1 đi 2 cái

-- ==================================================================================
-- HOÀN TẤT
-- ==================================================================================
PRINT N'✅ Database DB_SkinFood1 đã được tạo lại!';
PRINT N'✅ Đã thêm bảng NhaCungCap và liên kết vào PhieuNhap!';
PRINT N'✅ Trigger quản lý kho đã hoạt động!';

-- Kiểm tra kết quả
SELECT * FROM NhaCungCap;
SELECT * FROM PhieuNhap;
SELECT MaSP, TenSP, SoLuongTon FROM SanPham; -- Kiểm tra số lượng tồn đã tự nhảy chưa