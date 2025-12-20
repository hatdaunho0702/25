USE master;
GO

-- ==================================================================================
-- 1. KHỞI TẠO DATABASE (RESET SẠCH)
-- ==================================================================================
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'DB_SkinFood1')
BEGIN
    ALTER DATABASE DB_SkinFood1 SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE DB_SkinFood1;
END
GO

CREATE DATABASE DB_SkinFood1;
GO

USE DB_SkinFood1;
GO

-- ==================================================================================
-- 2. TẠO BẢNG (STRUCTURE)
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
CREATE TABLE DanhMuc (MaDM INT PRIMARY KEY IDENTITY(1,1), TenDM NVARCHAR(100) NOT NULL);
GO

-- 4. THƯƠNG HIỆU
CREATE TABLE ThuongHieu (MaTH INT PRIMARY KEY IDENTITY(1,1), TenTH NVARCHAR(100), QuocGia NVARCHAR(100));
GO

-- 5. LOẠI SẢN PHẨM
CREATE TABLE LoaiSP (
    MaLoai INT PRIMARY KEY IDENTITY(1,1),
    TenLoai NVARCHAR(100) NOT NULL,
    MaDM INT NOT NULL,
    FOREIGN KEY (MaDM) REFERENCES DanhMuc(MaDM)
);
GO

-- 6. NHÀ CUNG CẤP
CREATE TABLE NhaCungCap (
    MaNCC INT PRIMARY KEY IDENTITY(1,1),
    TenNCC NVARCHAR(100) NOT NULL,
    DiaChi NVARCHAR(255),
    SoDienThoai NVARCHAR(20),
    Email NVARCHAR(100),
    NguoiLienHe NVARCHAR(100)
);
GO

-- 7. SẢN PHẨM (ĐÃ XÓA MaNCC, NSX, HSD)
CREATE TABLE SanPham (
    MaSP INT PRIMARY KEY IDENTITY(1,1),
    TenSP NVARCHAR(200),
    GiamGia DECIMAL(18,2) DEFAULT 0,
    GiaBan DECIMAL(18,2),
    MoTa NVARCHAR(MAX),
    HinhAnh NVARCHAR(255),
    SoLuongTon INT DEFAULT 0,
    
    -- Khóa ngoại phân loại
    MaDM INT, 
    MaTH INT, 
    MaLoai INT,
    -- ĐÃ XÓA MaNCC TẠI ĐÂY

    -- Thuộc tính chi tiết
    LoaiDa NVARCHAR(50), 
    VanDeChiRi NVARCHAR(255), 
    TonDaMau NVARCHAR(50),
    ThanhPhanChiNhYeu NVARCHAR(MAX),
    SoLanSuDungMoiTuan INT DEFAULT 1,
    DoTinCay DECIMAL(3,1) DEFAULT 0,
    KichCoTieuChuan NVARCHAR(50),
    NgayNhapKho DATETIME DEFAULT GETDATE(),
    TrangThaiSanPham NVARCHAR(50) DEFAULT N'Kinh doanh',
    
    FOREIGN KEY (MaDM) REFERENCES DanhMuc(MaDM),
    FOREIGN KEY (MaTH) REFERENCES ThuongHieu(MaTH),
    FOREIGN KEY (MaLoai) REFERENCES LoaiSP(MaLoai)
    -- ĐÃ XÓA CONSTRAINT REFERENCES NhaCungCap
);
GO

-- 8. ĐƠN HÀNG
CREATE TABLE DonHang (
    MaDH INT PRIMARY KEY IDENTITY(1,1), 
    MaND INT, 
    NgayDat DATETIME DEFAULT GETDATE(), 
    TongTien DECIMAL(18,2) DEFAULT 0,
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

-- 9. CHI TIẾT ĐƠN HÀNG
CREATE TABLE ChiTietDonHangs (
    MaDH INT, 
    MaSP INT, 
    SoLuong INT CHECK (SoLuong > 0), 
    DonGia DECIMAL(18,2),
    PRIMARY KEY (MaDH, MaSP), 
    FOREIGN KEY (MaDH) REFERENCES DonHang(MaDH) ON DELETE CASCADE, 
    FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP)
);
GO

-- 10. PHIẾU NHẬP (Nhà cung cấp sẽ gắn với phiếu nhập)
CREATE TABLE PhieuNhap (
    MaPN INT PRIMARY KEY IDENTITY(1,1), 
    MaND INT, 
    MaNCC INT, -- Nhà cung cấp nằm ở đây là đúng chuẩn
    NgayNhap DATETIME DEFAULT GETDATE(), 
    TongTien DECIMAL(18,2) DEFAULT 0, 
    GhiChu NVARCHAR(500),
    FOREIGN KEY (MaND) REFERENCES NguoiDung(MaND), 
    FOREIGN KEY (MaNCC) REFERENCES NhaCungCap(MaNCC)
);
GO

-- 11. CHI TIẾT PHIẾU NHẬP (NSX và HSD nằm ở đây để quản lý Lô hàng)
CREATE TABLE ChiTietPhieuNhap (
    MaPN INT, 
    MaSP INT, 
    SoLuong INT CHECK (SoLuong > 0), 
    GiaNhap DECIMAL(18,2) CHECK (GiaNhap >= 0), 
    ThanhTien DECIMAL(18,2),
    
    -- Date gắn liền với lô hàng nhập
    NgaySanXuat DATE,
    HanSuDung DATE,

    PRIMARY KEY (MaPN, MaSP), 
    FOREIGN KEY (MaPN) REFERENCES PhieuNhap(MaPN) ON DELETE CASCADE, 
    FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP)
);
GO

-- 12. PHIẾU XUẤT & CHI TIẾT
CREATE TABLE PhieuXuat (
    MaPX INT PRIMARY KEY IDENTITY(1,1), 
    MaND INT, 
    NgayXuat DATETIME DEFAULT GETDATE(), 
    LyDoXuat NVARCHAR(255), 
    GhiChu NVARCHAR(500), 
    FOREIGN KEY (MaND) REFERENCES NguoiDung(MaND)
);

CREATE TABLE ChiTietPhieuXuat (
    MaPX INT, 
    MaSP INT, 
    SoLuong INT CHECK (SoLuong > 0), 
    PRIMARY KEY (MaPX, MaSP), 
    FOREIGN KEY (MaPX) REFERENCES PhieuXuat(MaPX) ON DELETE CASCADE, 
    FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP)
);
GO

-- 13. CÁC BẢNG PHỤ
CREATE TABLE DanhGia (
    MaDG INT PRIMARY KEY IDENTITY(1,1), MaSP INT, MaND INT, NoiDung NVARCHAR(500), Diem INT, NgayDanhGia DATETIME DEFAULT GETDATE(), DuocApprove BIT DEFAULT 0, TraLoiAdmin NVARCHAR(500), ThoiGianTraLoi DATETIME, FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP), FOREIGN KEY (MaND) REFERENCES NguoiDung(MaND)
);

CREATE TABLE ThuocTinhMyPham (MaThuocTinh INT PRIMARY KEY IDENTITY(1,1), TenThuocTinh NVARCHAR(100), LoaiThuocTinh NVARCHAR(50), MoTa NVARCHAR(255));
CREATE TABLE SanPham_ThuocTinh (MaSP INT, MaThuocTinh INT, PRIMARY KEY (MaSP, MaThuocTinh), FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP) ON DELETE CASCADE, FOREIGN KEY (MaThuocTinh) REFERENCES ThuocTinhMyPham(MaThuocTinh));
CREATE TABLE LienHe (MaLH INT IDENTITY(1,1) PRIMARY KEY, HoTen NVARCHAR(100), Email NVARCHAR(100), SoDienThoai NVARCHAR(20), NoiDung NVARCHAR(1000), NgayGui DATETIME DEFAULT GETDATE());
GO

-- ==================================================================================
-- 3. TRIGGERS (GIỮ NGUYÊN)
-- ==================================================================================

-- A. Trigger Nhập Kho
CREATE TRIGGER trg_CapNhatKho_Nhap ON ChiTietPhieuNhap AFTER INSERT, UPDATE, DELETE AS
BEGIN
    IF EXISTS (SELECT * FROM deleted) 
        UPDATE sp SET sp.SoLuongTon = sp.SoLuongTon - d.SoLuong 
        FROM SanPham sp JOIN deleted d ON sp.MaSP = d.MaSP;
    
    IF EXISTS (SELECT * FROM inserted) 
        UPDATE sp SET sp.SoLuongTon = sp.SoLuongTon + i.SoLuong, sp.NgayNhapKho = GETDATE() 
        FROM SanPham sp JOIN inserted i ON sp.MaSP = i.MaSP;
END
GO

-- B. Trigger Bán Hàng
CREATE TRIGGER trg_CapNhatKho_BanHang ON ChiTietDonHangs AFTER INSERT, UPDATE, DELETE AS
BEGIN
    IF EXISTS (SELECT * FROM deleted) 
        UPDATE sp SET sp.SoLuongTon = sp.SoLuongTon + d.SoLuong 
        FROM SanPham sp JOIN deleted d ON sp.MaSP = d.MaSP;
    
    IF EXISTS (SELECT * FROM inserted) 
        UPDATE sp SET sp.SoLuongTon = sp.SoLuongTon - i.SoLuong 
        FROM SanPham sp JOIN inserted i ON sp.MaSP = i.MaSP;
END
GO

-- C. Trigger Xuất Kho
CREATE TRIGGER trg_CapNhatKho_Xuat ON ChiTietPhieuXuat AFTER INSERT, UPDATE, DELETE AS 
BEGIN
    SET NOCOUNT ON;
    WITH ThayDoiKho AS (
        SELECT MaSP, SoLuong AS SoLuongThayDoi FROM deleted
        UNION ALL
        SELECT MaSP, -SoLuong AS SoLuongThayDoi FROM inserted
    )
    UPDATE sp
    SET sp.SoLuongTon = sp.SoLuongTon + t.TongThayDoi
    FROM SanPham sp
    JOIN (SELECT MaSP, SUM(SoLuongThayDoi) AS TongThayDoi FROM ThayDoiKho GROUP BY MaSP) t ON sp.MaSP = t.MaSP;
END
GO

-- D. Trigger Trạng Thái
CREATE TRIGGER trg_TuDongCapNhatTrangThai ON SanPham AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF UPDATE(SoLuongTon)
    BEGIN
        UPDATE sp SET sp.TrangThaiSanPham = N'Hết hàng'
        FROM SanPham sp JOIN inserted i ON sp.MaSP = i.MaSP
        WHERE i.SoLuongTon <= 0 AND sp.TrangThaiSanPham <> N'Hết hàng';

        UPDATE sp SET sp.TrangThaiSanPham = N'Kinh doanh'
        FROM SanPham sp JOIN inserted i ON sp.MaSP = i.MaSP
        WHERE i.SoLuongTon > 0 AND sp.TrangThaiSanPham = N'Hết hàng';
    END
END
GO

-- ==================================================================================
-- 4. DỮ LIỆU MẪU (ĐÃ CẬP NHẬT INSERT)
-- ==================================================================================

-- 1. Admin & User
INSERT INTO NguoiDung (HoTen, SoDienThoai, DiaChi, GioiTinh, NgaySinh) VALUES 
(N'Quản Trị Viên', '0909000999', N'SkinFood HQ', N'Nam', '1995-01-01');
INSERT INTO TaiKhoan (TenDangNhap, MatKhauHash, VaiTro, MaND) VALUES 
('admin', '123456', 'Admin', 1);

-- 2. Danh mục & Brand
INSERT INTO DanhMuc (TenDM) VALUES (N'Chăm sóc da'), (N'Trang điểm'), (N'Tóc');
INSERT INTO ThuongHieu (TenTH, QuocGia) VALUES (N'Innisfree', N'Hàn Quốc'), (N'L''Oreal', N'Pháp'), (N'The Ordinary', N'Canada');
INSERT INTO NhaCungCap (TenNCC, DiaChi) VALUES (N'Innisfree VN', N'HCM');
INSERT INTO LoaiSP (TenLoai, MaDM) VALUES (N'Serum', 1), (N'Kem dưỡng', 1), (N'Son môi', 2), (N'Dầu gội', 3);

-- 3. Sản Phẩm (Đã xóa cột MaNCC trong câu lệnh Insert)
INSERT INTO SanPham (TenSP, GiaBan, MaDM, MaTH, MaLoai, HinhAnh, MoTa) VALUES
(N'Serum The Ordinary Niacinamide', 250000, 1, 3, 1, '1.jpg', N'Giảm mụn.'),
(N'Bộ dưỡng da Trà Xanh', 1200000, 1, 1, 2, '2.jpg', N'Cấp ẩm.'),
(N'Sữa rửa mặt L''Oreal', 150000, 1, 2, 1, '3.jpg', N'Sạch sâu.'),
(N'Dầu gội thảo dược', 180000, 3, 1, 4, '4.jpg', N'Mượt tóc.'),
(N'Nước hoa hồng Mamonde', 220000, 1, 1, 1, '5.jpg', N'Cân bằng pH.'),
(N'Sữa dưỡng thể Vaseline', 150000, 1, 2, 2, '6.jpg', N'Trắng da.'),
(N'Mặt nạ Kiehl''s', 850000, 1, 3, 2, '7.jpg', N'Sáng da.'),
(N'Kem chống nắng Anessa', 450000, 1, 1, 2, '8.jpg', N'Chống UV.'),
(N'Son Dior', 750000, 2, 2, 3, '9.jpg', N'Dưỡng môi.'),
(N'Kem trị mụn La Roche-Posay', 350000, 1, 2, 2, '10.jpg', N'Giảm viêm.');

-- 4. Nhập Kho
-- Phiếu 1
INSERT INTO PhieuNhap (MaND, MaNCC, TongTien) VALUES (1, 1, 50000000);

-- Chi tiết phiếu 1 (Vẫn giữ Date ở đây)
INSERT INTO ChiTietPhieuNhap (MaPN, MaSP, SoLuong, GiaNhap, ThanhTien, NgaySanXuat, HanSuDung) VALUES 
(1, 1, 100, 200000, 20000000, '2023-01-01', '2026-01-01'),
(1, 2, 50, 800000, 40000000, '2023-05-15', '2026-05-15'),
(1, 3, 100, 100000, 10000000, '2023-02-20', '2025-02-20'),
(1, 4, 100, 120000, 12000000, '2023-06-10', '2025-06-10'),
(1, 5, 100, 150000, 15000000, '2023-03-01', '2026-03-01'),
(1, 6, 100, 100000, 10000000, '2023-08-01', '2025-08-01'),
(1, 7, 50, 600000, 30000000, '2023-04-12', '2024-04-12'),
(1, 8, 100, 300000, 30000000, '2023-09-09', '2026-09-09'),
(1, 9, 50, 500000, 25000000, '2023-10-20', '2028-10-20'),
(1, 10, 100, 250000, 25000000, '2023-11-11', '2026-11-11');

-- Nhập thêm hàng (Lần 2)
INSERT INTO PhieuNhap (MaND, MaNCC, TongTien) VALUES (1, 1, 8000000);
INSERT INTO ChiTietPhieuNhap (MaPN, MaSP, SoLuong, GiaNhap, ThanhTien, NgaySanXuat, HanSuDung) VALUES 
(2, 1, 20, 200000, 4000000, '2024-01-01', '2027-01-01');

-- ==================================================================================
-- 5. KIỂM TRA KẾT QUẢ
-- ==================================================================================
PRINT N'✅ Database DB_SkinFood1 đã được khởi tạo thành công!';
PRINT N'--- Kiểm tra cấu trúc bảng SanPham (Không còn MaNCC, NSX, HSD) ---';
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SanPham';

PRINT N'--- Kiểm tra dữ liệu SanPham ---';
SELECT MaSP, TenSP, SoLuongTon FROM SanPham;


-- 14. BẢNG KHUYẾN MÃI (MÃ GIẢM GIÁ)
CREATE TABLE KhuyenMai (
    MaKM INT PRIMARY KEY IDENTITY(1,1),
    TenChuongTrinh NVARCHAR(200) NOT NULL, -- Tên đợt KM
    MaCode VARCHAR(50) UNIQUE NOT NULL,    -- Mã user nhập vào (VD: SALE10, TET2025)
    
    -- Loại giảm: 1 là % (PhanTram), 2 là Tiền mặt (TienMat)
    LoaiGiamGia NVARCHAR(20) CHECK (LoaiGiamGia IN ('PhanTram', 'TienMat')) DEFAULT 'PhanTram',
    
    -- Giá trị giảm: VD 10 (là 10%) hoặc 50000 (là 50k)
    GiaTriGiam DECIMAL(18,2) NOT NULL,

    -- Số tiền giảm tối đa (Chỉ dùng cho loại %)
    -- VD: Giảm 10% nhưng tối đa chỉ giảm 50k
    GiamToiDa DECIMAL(18,2) NULL, 

    -- Điều kiện áp dụng
    DonHangToiThieu DECIMAL(18,2) DEFAULT 0, -- Đơn tối thiểu để áp mã
    
    -- Quản lý thời gian & Số lượng
    SoLuongPhatHanh INT DEFAULT 1000,        -- Tổng số mã tung ra
    SoLuongDaDung INT DEFAULT 0,             -- Số mã đã được dùng
    NgayBatDau DATETIME DEFAULT GETDATE(),
    NgayKetThuc DATETIME,
    
    TrangThai BIT DEFAULT 1 -- 1: Đang hoạt động, 0: Ngưng
);
GO

-- 15. BẢNG CHI TIẾT SẢN PHẨM ĐƯỢC ÁP DỤNG MÃ
CREATE TABLE KhuyenMai_SanPham (
    MaKM INT,
    MaSP INT,
    PRIMARY KEY (MaKM, MaSP),
    FOREIGN KEY (MaKM) REFERENCES KhuyenMai(MaKM) ON DELETE CASCADE,
    FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP) ON DELETE CASCADE
);
GO

-- Cập nhật bảng DonHang để liên kết với KhuyenMai
ALTER TABLE DonHang
ADD MaKM INT NULL, -- Mã khuyến mãi đã áp dụng (nếu có)
    SoTienGiam DECIMAL(18,2) DEFAULT 0; -- Lưu lại số tiền đã được trừ
GO

ALTER TABLE DonHang
ADD CONSTRAINT FK_DonHang_KhuyenMai 
FOREIGN KEY (MaKM) REFERENCES KhuyenMai(MaKM);
GO

ALTER TABLE NguoiDung
ADD Avatar NVARCHAR(50) NULL;