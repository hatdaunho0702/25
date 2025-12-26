USE master;
GO

-- ==================================================================================
-- 1. XÓA SẠCH DATABASE CŨ (ĐỂ TRÁNH LỖI TRÙNG LẶP)
-- ==================================================================================
-- Đặt lại trạng thái database để ngắt mọi kết nối ngay lập tức (Kể cả kết nối đang bị treo)
USE master;
GO

-- Đặt lại trạng thái database để ngắt mọi kết nối ngay lập tức (Kể cả kết nối đang bị treo)
ALTER DATABASE DB_SkinFood1 SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

-- Sau khi đá văng tất cả ra thì xóa
DROP DATABASE DB_SkinFood1;
GO


CREATE DATABASE DB_SkinFood1;
GO

USE DB_SkinFood1;
GO

-- ==================================================================================
-- 2. TẠO CẤU TRÚC BẢNG (CHUẨN)
-- ==================================================================================

-- 1. Người dùng
CREATE TABLE NguoiDung (
    MaND INT PRIMARY KEY IDENTITY(1,1),
    HoTen NVARCHAR(100) NOT NULL,
    SoDienThoai NVARCHAR(20),
    DiaChi NVARCHAR(255),
    GioiTinh NVARCHAR(10),
    NgaySinh DATE,
    Avatar NVARCHAR(50) NULL,
    NgayTao DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- 2. Tài khoản
CREATE TABLE TaiKhoan (
    MaTK INT PRIMARY KEY IDENTITY(1,1),
    TenDangNhap NVARCHAR(100) UNIQUE NOT NULL,
    MatKhauHash VARCHAR(500) NOT NULL,
    VaiTro NVARCHAR(20) DEFAULT 'KhachHang',
    MaND INT UNIQUE NOT NULL,
    FOREIGN KEY (MaND) REFERENCES NguoiDung(MaND)
);

-- 3. Danh mục
CREATE TABLE DanhMuc (MaDM INT PRIMARY KEY IDENTITY(1,1), TenDM NVARCHAR(100) NOT NULL);

-- 4. Thương hiệu
CREATE TABLE ThuongHieu (MaTH INT PRIMARY KEY IDENTITY(1,1), TenTH NVARCHAR(100), QuocGia NVARCHAR(100));

-- 5. Loại sản phẩm
CREATE TABLE LoaiSP (
    MaLoai INT PRIMARY KEY IDENTITY(1,1),
    TenLoai NVARCHAR(100) NOT NULL,
    MaDM INT NOT NULL,
    FOREIGN KEY (MaDM) REFERENCES DanhMuc(MaDM)
);

-- 6. Nhà cung cấp
CREATE TABLE NhaCungCap (
    MaNCC INT PRIMARY KEY IDENTITY(1,1),
    TenNCC NVARCHAR(100) NOT NULL,
    DiaChi NVARCHAR(255),
    SoDienThoai NVARCHAR(20),
    Email NVARCHAR(100),
    NguoiLienHe NVARCHAR(100)
);

-- 7. Sản phẩm
CREATE TABLE SanPham (
    MaSP INT PRIMARY KEY IDENTITY(1,1),
    TenSP NVARCHAR(200),
    GiamGia DECIMAL(18,2) DEFAULT 0,
    GiaBan DECIMAL(18,2),
    MoTa NVARCHAR(MAX),
    HinhAnh NVARCHAR(255),
    SoLuongTon INT DEFAULT 0,
    
    MaDM INT, 
    MaTH INT, 
    MaLoai INT,

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
);

-- 8. Khuyến mãi
CREATE TABLE KhuyenMai (
    MaKM INT PRIMARY KEY IDENTITY(1,1),
    TenChuongTrinh NVARCHAR(200) NOT NULL,
    MaCode VARCHAR(50) UNIQUE NOT NULL,     
    LoaiGiamGia NVARCHAR(20) CHECK (LoaiGiamGia IN ('PhanTram', 'TienMat')) DEFAULT 'PhanTram',
    GiaTriGiam DECIMAL(18,2) NOT NULL,
    GiamToiDa DECIMAL(18,2) NULL, 
    DonHangToiThieu DECIMAL(18,2) DEFAULT 0, 
    SoLuongPhatHanh INT DEFAULT 1000,         
    SoLuongDaDung INT DEFAULT 0,              
    NgayBatDau DATETIME DEFAULT GETDATE(),
    NgayKetThuc DATETIME,
    TrangThai BIT DEFAULT 1
);

-- 9. Đơn hàng
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
    
    MaKM INT NULL, 
    SoTienGiam DECIMAL(18,2) DEFAULT 0,

    FOREIGN KEY (MaND) REFERENCES NguoiDung(MaND),
    FOREIGN KEY (MaKM) REFERENCES KhuyenMai(MaKM)
);

-- 10. Chi tiết đơn hàng
CREATE TABLE ChiTietDonHangs (
    MaDH INT, 
    MaSP INT, 
    SoLuong INT CHECK (SoLuong > 0), 
    DonGia DECIMAL(18,2),
    PRIMARY KEY (MaDH, MaSP), 
    FOREIGN KEY (MaDH) REFERENCES DonHang(MaDH) ON DELETE CASCADE, 
    FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP)
);

-- 11. Phiếu nhập
CREATE TABLE PhieuNhap (
    MaPN INT PRIMARY KEY IDENTITY(1,1), 
    MaND INT, 
    MaNCC INT, 
    NgayNhap DATETIME DEFAULT GETDATE(), 
    TongTien DECIMAL(18,2) DEFAULT 0, 
    GhiChu NVARCHAR(500),
    FOREIGN KEY (MaND) REFERENCES NguoiDung(MaND), 
    FOREIGN KEY (MaNCC) REFERENCES NhaCungCap(MaNCC)
);

-- 12. Chi tiết phiếu nhập
CREATE TABLE ChiTietPhieuNhap (
    MaPN INT, 
    MaSP INT, 
    SoLuong INT CHECK (SoLuong > 0), 
    GiaNhap DECIMAL(18,2) CHECK (GiaNhap >= 0), 
    ThanhTien DECIMAL(18,2),
    NgaySanXuat DATE,
    HanSuDung DATE,
    PRIMARY KEY (MaPN, MaSP), 
    FOREIGN KEY (MaPN) REFERENCES PhieuNhap(MaPN) ON DELETE CASCADE, 
    FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP)
);

-- 13. Phiếu xuất & Chi tiết
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

-- 14. Bảng phụ
CREATE TABLE DanhGia (
    MaDG INT PRIMARY KEY IDENTITY(1,1), MaSP INT, MaND INT, NoiDung NVARCHAR(500), Diem INT, NgayDanhGia DATETIME DEFAULT GETDATE(), DuocApprove BIT DEFAULT 0, TraLoiAdmin NVARCHAR(500), ThoiGianTraLoi DATETIME, FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP), FOREIGN KEY (MaND) REFERENCES NguoiDung(MaND)
);

CREATE TABLE LienHe (
    MaLH INT IDENTITY(1,1) PRIMARY KEY, 
    HoTen NVARCHAR(100), 
    Email NVARCHAR(100), 
    SoDienThoai NVARCHAR(20), 
    NoiDung NVARCHAR(1000), 
    NgayGui DATETIME DEFAULT GETDATE(),
    MaND INT NULL,
    TrangThai NVARCHAR(50) DEFAULT N'Chưa xử lý',
    FOREIGN KEY (MaND) REFERENCES NguoiDung(MaND) ON DELETE SET NULL
);

CREATE TABLE KhuyenMai_SanPham (
    MaKM INT,
    MaSP INT,
    PRIMARY KEY (MaKM, MaSP),
    FOREIGN KEY (MaKM) REFERENCES KhuyenMai(MaKM) ON DELETE CASCADE,
    FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP) ON DELETE CASCADE
);
GO

-- ==================================================================================
-- 3. TRIGGERS
-- ==================================================================================
-- Trigger Nhập Kho
CREATE TRIGGER trg_CapNhatKho_Nhap ON ChiTietPhieuNhap AFTER INSERT, UPDATE, DELETE AS
BEGIN
    IF EXISTS (SELECT * FROM deleted) UPDATE sp SET sp.SoLuongTon = sp.SoLuongTon - d.SoLuong FROM SanPham sp JOIN deleted d ON sp.MaSP = d.MaSP;
    IF EXISTS (SELECT * FROM inserted) UPDATE sp SET sp.SoLuongTon = sp.SoLuongTon + i.SoLuong, sp.NgayNhapKho = GETDATE() FROM SanPham sp JOIN inserted i ON sp.MaSP = i.MaSP;
END
GO
-- Trigger Bán Hàng
CREATE TRIGGER trg_CapNhatKho_BanHang
ON ChiTietDonHangs  -- <--- QUAN TRỌNG: Phải thêm dòng này để biết trigger gắn vào bảng nào
AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Trừ tồn kho
    UPDATE sp 
    SET sp.SoLuongTon = sp.SoLuongTon - i.SoLuong 
    FROM SanPham sp 
    JOIN inserted i ON sp.MaSP = i.MaSP;

    -- 2. KIỂM TRA NGAY LẬP TỨC: Nếu có sản phẩm nào bị âm kho -> Hủy và Báo lỗi
    IF EXISTS (SELECT 1 FROM SanPham WHERE SoLuongTon < 0)
    BEGIN
        ROLLBACK TRANSACTION; -- Hủy lệnh Insert ChiTietDonHang vừa rồi
        RAISERROR (N'Lỗi: Sản phẩm đã hết hàng hoặc không đủ số lượng tồn kho!', 16, 1);
        RETURN;
    END
END
GO
-- Trigger dành cho Xuất Kho Nội Bộ (Áp dụng cho bảng ChiTietPhieuXuats)
CREATE TRIGGER trg_CapNhatKho_XuatKhoNoiBo ON ChiTietPhieuXuat
AFTER INSERT, UPDATE, DELETE AS
BEGIN
    -- 1. Nếu xóa phiếu xuất hoặc xóa dòng chi tiết -> Cộng lại số lượng vào kho
    IF EXISTS (SELECT * FROM deleted) 
    BEGIN
        UPDATE sp 
        SET sp.SoLuongTon = sp.SoLuongTon + d.SoLuong 
        FROM SanPham sp 
        JOIN deleted d ON sp.MaSP = d.MaSP;
    END

    -- 2. Nếu thêm phiếu xuất mới hoặc thêm dòng chi tiết -> Trừ số lượng trong kho
    IF EXISTS (SELECT * FROM inserted) 
    BEGIN
        UPDATE sp 
        SET sp.SoLuongTon = sp.SoLuongTon - i.SoLuong 
        FROM SanPham sp 
        JOIN inserted i ON sp.MaSP = i.MaSP;
    END
END
GO

-- ==================================================================================
-- 4. NHẬP LIỆU (THEO ĐÚNG THỨ TỰ CHA -> CON)
-- ==================================================================================

-- 1. Người dùng & Admin (BẮT BUỘC CHẠY TRƯỚC)
INSERT INTO NguoiDung (HoTen, SoDienThoai, DiaChi, GioiTinh, NgaySinh) 
VALUES (N'Nguyễn Huy Hoàng', '0909000999', N'TP.HCM', N'Nam', '2000-01-01');

INSERT INTO TaiKhoan (TenDangNhap, MatKhauHash, VaiTro, MaND) 
VALUES ('admin@skinfood.com', '123456', 'Admin', 1);

-- 2. Nhà cung cấp
INSERT INTO NhaCungCap (TenNCC, DiaChi, SoDienThoai) VALUES 
(N'Tổng kho Mỹ Phẩm Chính Hãng', N'Quận 7, TP.HCM', '0909123123');

-- 3. Danh mục (Gộp tất cả vào 1 lệnh để ID liên tục: 1->7)
INSERT INTO DanhMuc (TenDM) VALUES 
(N'Chăm sóc da'),           -- ID 1
(N'Trang điểm'),            -- ID 2
(N'Chăm sóc tóc'),          -- ID 3
(N'Nước hoa'),              -- ID 4
(N'Chăm sóc cơ thể'),       -- ID 5
(N'Thực phẩm chức năng'),   -- ID 6
(N'Dụng cụ làm đẹp');       -- ID 7

-- 4. Thương hiệu 
INSERT INTO ThuongHieu (TenTH, QuocGia) VALUES 
(N'Innisfree', N'Hàn Quốc'),    -- 1
(N'L''Oreal', N'Pháp'),         -- 2
(N'The Ordinary', N'Canada'),   -- 3
(N'Cocoon', N'Việt Nam'),       -- 4 
(N'Pond''s', N'Mỹ'),            -- 5 
(N'La Roche-Posay', N'Pháp'),   -- 6 
(N'Romand', N'Hàn Quốc'),       -- 7 
(N'Black Rouge', N'Hàn Quốc'),  -- 8
(N'Emmié', N'Việt Nam');        -- 9

-- 5. Loại sản phẩm (Mapping đúng với ID Danh mục ở trên)
INSERT INTO LoaiSP (TenLoai, MaDM) VALUES 
-- DM 1: Da
(N'Serum', 1),          -- 1
(N'Kem dưỡng', 1),      -- 2
-- DM 2: Trang điểm
(N'Son môi', 2),        -- 3
-- DM 3: Tóc
(N'Dầu gội', 3),        -- 4
-- DM 1: Da (Tiếp)
(N'Sữa rửa mặt', 1),    -- 5
(N'Kem chống nắng', 1), -- 6
-- DM 4: Nước hoa
(N'Nước hoa nam', 4),   -- 7
(N'Nước hoa nữ', 4),    -- 8
-- DM 5: Body
(N'Sữa tắm', 5),        -- 9
(N'Dưỡng thể', 5),      -- 10
-- DM 6: TPCN
(N'Viên uống', 6),      -- 11
-- DM 7: Dụng cụ
(N'Máy rửa mặt', 7);    -- 12
GO

-- 6. SẢN PHẨM (ĐƯỜNG DẪN ẢNH CHUẨN)
INSERT INTO SanPham (TenSP, GiaBan, MaDM, MaTH, MaLoai, HinhAnh, MoTa, GiamGia, ThanhPhanChiNhYeu) VALUES

-- === DẦU GỘI (Folder: dau_goi) ===
(N'Dầu gội Pond''s Phục hồi hư tổn Soft & Smooth', 185000, 3, 5, 4, 'dau_goi/1.jpg', N'Phục hồi cấu trúc tóc hư tổn từ sâu bên trong.', 0, N'Keratin, Vitamin E'),
(N'Dầu gội Gừng Thảo mộc Ngăn rụng tóc', 250000, 3, 4, 4, 'dau_goi/2.jpg', N'Chiết xuất từ gừng tươi giúp làm ấm da đầu.', 0, N'Gừng tươi, Rễ nhân sâm'),
(N'Dầu gội Dược liệu cổ truyền Dáng Diễm', 160000, 3, 4, 4, 'dau_goi/3.jpg', N'Kết hợp bồ kết, hương nhu và cỏ mần trầu.', 0, N'Bồ kết, Hương nhu'),
(N'Bộ đôi Gội Xả Thảo mộc Wings Up', 320000, 3, 4, 4, 'dau_goi/4.jpg', N'Giải pháp chăm sóc tóc toàn diện 3 không.', 0, N'Trà xanh, Bạc hà'),
(N'Dầu gội nước hoa cao cấp I.D Salon Grade', 450000, 3, 5, 4, 'dau_goi/5.jpg', N'Hương nước hoa Pháp sang trọng, lưu hương 72h.', 0, N'Collagen, Argan Oil'),
(N'Dầu gội phủ bạc thảo dược thiên nhiên', 290000, 3, 4, 4, 'dau_goi/6.jpg', N'Phủ bạc an toàn, không gây xót da đầu.', 0, N'Hà thủ ô, Bồ kết'),
(N'Dầu gội cốt Dừa tươi dưỡng ẩm Coconut', 140000, 3, 5, 4, 'dau_goi/7.jpg', N'Tinh dầu dừa nguyên chất giúp cấp ẩm siêu việt.', 0, N'Tinh dầu dừa'),
(N'Dầu gội xả 2 trong 1 Herbal Care', 150000, 3, 4, 4, 'dau_goi/8.jpg', N'Làm sạch và dưỡng xả trong một bước.', 0, N'12 loại thảo mộc'),
(N'Dầu gội Bưởi Cocoon Kích thích mọc tóc', 195000, 3, 4, 4, 'dau_goi/9.jpg', N'Tinh dầu vỏ bưởi giúp giảm gãy rụng.', 0, N'Tinh dầu vỏ bưởi, B5'),
(N'Dầu gội Kelina Bạch quả & Trà xanh', 280000, 3, 4, 4, 'dau_goi/10.jpg', N'Chống oxy hóa, bảo vệ tóc khỏi tia UV.', 0, N'Bạch quả, Trà xanh'),

-- === KEM DƯỠNG (Folder: Kem) ===
(N'Kem chống nắng phục hồi Emmié', 320000, 1, 9, 6, 'Kem/1.jpg', N'KCN vật lý lai hóa học bảo vệ da toàn diện.', 0, N'Vitamin B5, Niacinamide'),
(N'Tinh chất Vitamin C tươi dưỡng trắng', 280000, 1, 3, 1, 'Kem/2.jpg', N'Vitamin C nồng độ cao làm mờ thâm.', 0, N'Vitamin C, HA'),
(N'Kem dưỡng ẩm Skintific 5X Ceramide', 350000, 1, 3, 2, 'Kem/3.jpg', N'Phục hồi hàng rào bảo vệ da thần tốc.', 0, N'5 loại Ceramide'),
(N'Gel dưỡng ẩm Pond''s Hydra Active 72h', 195000, 1, 5, 2, 'Kem/4.jpg', N'Dạng thạch Jelly cấp nước suốt 72h.', 0, N'Nước khoáng, Vitamin E'),
(N'Kem dưỡng ẩm Cetaphil Moisturizing Cream', 285000, 1, 6, 2, 'Kem/5.jpg', N'Dành cho da khô và da nhạy cảm.', 0, N'Dầu hạnh nhân, Vitamin E'),
(N'Serum phục hồi La Roche-Posay Hyalu B5', 980000, 1, 6, 1, 'Kem/6.jpg', N'Cấp ẩm tầng sâu, phục hồi da kích ứng.', 0, N'B5, Hyaluronic Acid'),
(N'Serum trắng da Balance Active Formula', 160000, 1, 3, 1, 'Kem/7.jpg', N'Dưỡng trắng bình dân hiệu quả cao.', 0, N'Vitamin C ổn định'),

-- === SON MÔI (Folder: Son_Moi) ===
(N'Son Kem Lì Lilybyred Mood Liar', 180000, 2, 7, 3, 'Son_Moi/1.jpg', N'Chất son velvet xốp mềm, hương trái cây.', 0, N'Tinh dầu Jojoba'),
(N'Son Romand Milk Tea Velvet Tint', 190000, 2, 7, 3, 'Son_Moi/2.jpg', N'Tông màu trà sữa ấm áp tôn da.', 0, N'Vitamin E'),
(N'Son Romand Zero Velvet Tint', 175000, 2, 7, 3, 'Son_Moi/3.jpg', N'Nhẹ tênh như không thoa son.', 0, N'Macadamia Oil'),
(N'Son Peripera Ink The Velvet', 150000, 2, 7, 3, 'Son_Moi/4.jpg', N'Bám màu siêu đỉnh 8 tiếng.', 0, N'Marine Collagen'),
(N'Son Bóng Black Rouge Half N Half', 210000, 2, 8, 3, 'Son_Moi/5.jpg', N'Hiệu ứng căng mọng tráng gương.', 0, N'Tinh dầu bơ');
GO

-- 7. TỰ ĐỘNG NHẬP KHO (FULL SẢN PHẨM)
INSERT INTO PhieuNhap (MaND, MaNCC, TongTien, GhiChu) 
VALUES (1, 1, 0, N'Nhập hàng khai trương (Full kho)');
DECLARE @MaPN INT = SCOPE_IDENTITY();

INSERT INTO ChiTietPhieuNhap (MaPN, MaSP, SoLuong, GiaNhap, ThanhTien, NgaySanXuat, HanSuDung)
SELECT @MaPN, MaSP, 100, GiaBan * 0.6, (100 * GiaBan * 0.6), '2024-01-01', '2028-01-01' FROM SanPham;

UPDATE PhieuNhap SET TongTien = (SELECT SUM(ThanhTien) FROM ChiTietPhieuNhap WHERE MaPN = @MaPN) WHERE MaPN = @MaPN;
GO

SELECT * FROM SanPham;