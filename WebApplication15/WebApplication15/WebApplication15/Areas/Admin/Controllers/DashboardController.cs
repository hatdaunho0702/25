using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication15.Areas.Admin.Data;
using WebApplication15.Models;

namespace WebApplication15.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class DashboardController : Controller
    {
        private DB_SkinFood1Entities db = new DB_SkinFood1Entities();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db?.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            try
            {
                // Thống kê cơ bản
                var totalSanPham = db.SanPhams.Count();
                var totalDonHang = db.DonHangs.Count();
                var totalTaiKhoan = db.TaiKhoans.Count();
                var totalNhaCungCap = db.NhaCungCaps.Count();
                var totalNguoiDung = db.NguoiDungs.Count();

                ViewBag.TotalSanPham = totalSanPham;
                ViewBag.TotalDonHang = totalDonHang;
                ViewBag.TotalTaiKhoan = totalTaiKhoan;
                ViewBag.TotalNhaCungCap = totalNhaCungCap;
                ViewBag.TotalNguoiDung = totalNguoiDung;

                // Thống kê tồn kho - lấy tất cả sản phẩm
                var allSanPhams = db.SanPhams.ToList();
                var soLuongTonTatCa = allSanPhams.Sum(sp => sp.SoLuongTon ?? 0);
                var sanPhamHetHang = allSanPhams.Where(sp => (sp.SoLuongTon ?? 0) <= 0).Count();

                ViewBag.SoLuongTonTatCa = soLuongTonTatCa;
                ViewBag.SanPhamHetHang = sanPhamHetHang;

                // Doanh thu tháng này - Optimized
                var ngayDauThang = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                
                // Lấy tất cả đơn hàng vào memory một lần để tránh multiple queries
                var allDonHangs = db.DonHangs.ToList();
                
                var doanhThuThangNay = allDonHangs
                    .Where(dh => dh.NgayDat.HasValue && dh.NgayDat.Value >= ngayDauThang && dh.NgayDat.Value <= DateTime.Now)
                    .Sum(dh => dh.TongTien ?? 0m);

                ViewBag.DoanhThuThangNay = doanhThuThangNay;

                // ======================================
                // THỐNG KÊ THANH TOÁN
                // ======================================

                // Đơn hàng đã thanh toán
                var donHangDaThanhToan = allDonHangs
                    .Where(dh => dh.TrangThaiThanhToan == "Đã thanh toán" || dh.TrangThaiThanhToan == "Paid")
                    .ToList();

                // Đơn hàng chưa thanh toán
                var donHangChuaThanhToan = allDonHangs
                    .Where(dh => dh.TrangThaiThanhToan == "Chưa thanh toán" || dh.TrangThaiThanhToan == "Pending" || string.IsNullOrEmpty(dh.TrangThaiThanhToan))
                    .ToList();

                // Doanh thu từ các đơn hàng đã thanh toán
                var doanhThuDaThanhToan = donHangDaThanhToan.Sum(dh => dh.TongTien ?? 0m);

                // Doanh thu từ các đơn hàng chưa thanh toán
                var doanhThuChuaThanhToan = donHangChuaThanhToan.Sum(dh => dh.TongTien ?? 0m);

                ViewBag.DonHangDaThanhToan = donHangDaThanhToan.Count;
                ViewBag.DonHangChuaThanhToan = donHangChuaThanhToan.Count;
                ViewBag.DoanhThuDaThanhToan = doanhThuDaThanhToan;
                ViewBag.DoanhThuChuaThanhToan = doanhThuChuaThanhToan;

                // Top 10 đơn hàng chưa thanh toán (sắp xếp theo ngày cũ nhất)
                var donHangChuaThanhToanTop10 = donHangChuaThanhToan
                    .Where(dh => dh.NgayDat.HasValue)
                    .OrderBy(dh => dh.NgayDat)
                    .Take(10)
                    .ToList();

                ViewBag.DonHangChuaThanhToanTop10 = donHangChuaThanhToanTop10;

                // Đơn hàng đã thanh toán tháng này
                var donHangDaThanhToanThangNay = donHangDaThanhToan
                    .Where(dh => dh.NgayDat.HasValue && dh.NgayDat.Value >= ngayDauThang && dh.NgayDat.Value <= DateTime.Now)
                    .Count();

                ViewBag.DonHangDaThanhToanThangNay = donHangDaThanhToanThangNay;

                // ======================================
                // THỐNG KÊ DOANH THU CHO BIỂU ĐỒ
                // ======================================

                // Doanh thu theo ngày (7 ngày gần nhất)
                var doanhThuTheoNgay = new List<decimal>();
                var labelTheoNgay = new List<string>();
                
                for (int i = 6; i >= 0; i--)
                {
                    var ngay = DateTime.Now.AddDays(-i);
                    var ngayBatDau = ngay.Date;
                    var ngayKetThuc = ngayBatDau.AddDays(1);
                    
                    var doanhThu = allDonHangs
                        .Where(dh => dh.NgayDat.HasValue && dh.NgayDat.Value >= ngayBatDau && dh.NgayDat.Value < ngayKetThuc)
                        .Sum(dh => dh.TongTien ?? 0m);
                    
                    doanhThuTheoNgay.Add(doanhThu);
                    labelTheoNgay.Add(ngay.ToString("dd/MM"));
                }

                // Doanh thu theo tháng (12 tháng gần nhất)
                var doanhThuTheoThang = new List<decimal>();
                var labelTheoThang = new List<string>();
                
                for (int i = 11; i >= 0; i--)
                {
                    var thang = DateTime.Now.AddMonths(-i);
                    var ngayDauThangTemp = new DateTime(thang.Year, thang.Month, 1);
                    var ngayCuoiThang = ngayDauThangTemp.AddMonths(1);
                    
                    var doanhThu = allDonHangs
                        .Where(dh => dh.NgayDat.HasValue && dh.NgayDat.Value >= ngayDauThangTemp && dh.NgayDat.Value < ngayCuoiThang)
                        .Sum(dh => dh.TongTien ?? 0m);
                    
                    doanhThuTheoThang.Add(doanhThu);
                    labelTheoThang.Add(thang.ToString("MM/yyyy"));
                }

                // Doanh thu theo năm (5 năm gần nhất)
                var doanhThuTheoNam = new List<decimal>();
                var labelTheoNam = new List<string>();
                
                for (int i = 4; i >= 0; i--)
                {
                    var nam = DateTime.Now.Year - i;
                    var ngayDauNam = new DateTime(nam, 1, 1);
                    var ngayCuoiNam = new DateTime(nam, 12, 31).AddDays(1);
                    
                    var doanhThu = allDonHangs
                        .Where(dh => dh.NgayDat.HasValue && dh.NgayDat.Value >= ngayDauNam && dh.NgayDat.Value < ngayCuoiNam)
                        .Sum(dh => dh.TongTien ?? 0m);
                    
                    doanhThuTheoNam.Add(doanhThu);
                    labelTheoNam.Add(nam.ToString());
                }

                // Chuyển sang JSON để dùng trong JavaScript
                ViewBag.DoanhThuTheoNgay = Newtonsoft.Json.JsonConvert.SerializeObject(doanhThuTheoNgay);
                ViewBag.LabelTheoNgay = Newtonsoft.Json.JsonConvert.SerializeObject(labelTheoNgay);
                
                ViewBag.DoanhThuTheoThang = Newtonsoft.Json.JsonConvert.SerializeObject(doanhThuTheoThang);
                ViewBag.LabelTheoThang = Newtonsoft.Json.JsonConvert.SerializeObject(labelTheoThang);
                
                ViewBag.DoanhThuTheoNam = Newtonsoft.Json.JsonConvert.SerializeObject(doanhThuTheoNam);
                ViewBag.LabelTheoNam = Newtonsoft.Json.JsonConvert.SerializeObject(labelTheoNam);

                return View();
            }
            catch (Exception ex)
            {
                // Log lỗi và hiển thị thông báo cho user
                ViewBag.ErrorMessage = "Có lỗi xảy ra khi tải dashboard: " + ex.Message;
                
                // Khởi tạo các giá trị mặc định để tránh lỗi view
                ViewBag.TotalSanPham = 0;
                ViewBag.TotalDonHang = 0;
                ViewBag.TotalTaiKhoan = 0;
                ViewBag.TotalNhaCungCap = 0;
                ViewBag.TotalNguoiDung = 0;
                ViewBag.SoLuongTonTatCa = 0;
                ViewBag.SanPhamHetHang = 0;
                ViewBag.DoanhThuThangNay = 0m;
                ViewBag.DonHangDaThanhToan = 0;
                ViewBag.DonHangChuaThanhToan = 0;
                ViewBag.DoanhThuDaThanhToan = 0m;
                ViewBag.DoanhThuChuaThanhToan = 0m;
                ViewBag.DonHangChuaThanhToanTop10 = new List<DonHang>();
                ViewBag.DonHangDaThanhToanThangNay = 0;
                ViewBag.DoanhThuTheoNgay = "[]";
                ViewBag.LabelTheoNgay = "[]";
                ViewBag.DoanhThuTheoThang = "[]";
                ViewBag.LabelTheoThang = "[]";
                ViewBag.DoanhThuTheoNam = "[]";
                ViewBag.LabelTheoNam = "[]";
                
                return View();
            }
        }
    }
}