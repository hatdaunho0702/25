using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication15.Areas.Admin.Data;
using WebApplication15.Models;
using System.Data.Entity;

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
                // ======================================
                // 1. THỐNG KÊ CƠ BẢN (COUNT)
                // ======================================
                ViewBag.TotalSanPham = db.SanPhams.Count();
                ViewBag.TotalDonHang = db.DonHangs.Count();
                ViewBag.TotalTaiKhoan = db.TaiKhoans.Count();
                ViewBag.TotalNhaCungCap = db.NhaCungCaps.Count();
                ViewBag.TotalNguoiDung = db.NguoiDungs.Count();

                // ======================================
                // 2. THỐNG KÊ KHUYẾN MÃI
                // ======================================
                ViewBag.TotalKhuyenMai = db.KhuyenMais.Count();
                ViewBag.KhuyenMaiDangHoatDong = db.KhuyenMais.Count(k => k.TrangThai == true);

                // Khuyến mãi sắp hết hạn (7 ngày tới)
                var next7Days = DateTime.Now.AddDays(7);
                ViewBag.KhuyenMaiSapHetHan = db.KhuyenMais
                    .Where(k => k.NgayKetThuc.HasValue && k.NgayKetThuc >= DateTime.Now && k.NgayKetThuc <= next7Days)
                    .ToList();

                // ======================================
                // 3. THỐNG KÊ TỒN KHO (Đã xóa phần Hạn Sử Dụng)
                // ======================================
                // Tính tổng tồn kho
                ViewBag.SoLuongTonTatCa = db.SanPhams.Sum(sp => (int?)sp.SoLuongTon) ?? 0;
                ViewBag.SanPhamHetHang = db.SanPhams.Count(sp => sp.SoLuongTon <= 0);

                // ======================================
                // 4. DOANH THU & ĐƠN HÀNG (OPTIMIZED)
                // ======================================
                var ngayDauThang = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                // Doanh thu tháng này
                ViewBag.DoanhThuThangNay = db.DonHangs
                    .Where(dh => dh.NgayDat >= ngayDauThang)
                    .Sum(dh => (decimal?)dh.TongTien) ?? 0;

                // Đơn hàng đã thanh toán (Toàn bộ)
                var daThanhToanQuery = db.DonHangs.Where(dh => dh.TrangThaiThanhToan == "Đã thanh toán" || dh.TrangThaiThanhToan == "Paid");
                ViewBag.DonHangDaThanhToan = daThanhToanQuery.Count();
                ViewBag.DoanhThuDaThanhToan = daThanhToanQuery.Sum(dh => (decimal?)dh.TongTien) ?? 0;

                // Đơn hàng chưa thanh toán (Toàn bộ)
                var chuaThanhToanQuery = db.DonHangs.Where(dh => dh.TrangThaiThanhToan != "Đã thanh toán" && dh.TrangThaiThanhToan != "Paid");
                ViewBag.DonHangChuaThanhToan = chuaThanhToanQuery.Count();
                ViewBag.DoanhThuChuaThanhToan = chuaThanhToanQuery.Sum(dh => (decimal?)dh.TongTien) ?? 0;

                // Top 10 đơn hàng chưa thanh toán mới nhất
                ViewBag.DonHangChuaThanhToanTop10 = chuaThanhToanQuery
                    .OrderByDescending(dh => dh.NgayDat)
                    .Take(10)
                    .Include(d => d.NguoiDung)
                    .ToList();

                // ======================================
                // 5. BIỂU ĐỒ (CHARTS)
                // ======================================
                // Để tối ưu, ta lấy dữ liệu thô cần thiết (Ngày + Tiền) của 5 năm trở lại đây về RAM 1 lần
                var fiveYearsAgo = DateTime.Now.AddYears(-5);
                var rawDataForChart = db.DonHangs.AsNoTracking()
                    .Where(dh => dh.NgayDat >= fiveYearsAgo)
                    .Select(dh => new { dh.NgayDat, dh.TongTien })
                    .ToList();

                // -- Chart Ngày (7 ngày qua) --
                var doanhThuTheoNgay = new List<decimal>();
                var labelTheoNgay = new List<string>();
                for (int i = 6; i >= 0; i--)
                {
                    var d = DateTime.Now.AddDays(-i).Date;
                    var sum = rawDataForChart
                        .Where(x => x.NgayDat.HasValue && x.NgayDat.Value.Date == d)
                        .Sum(x => x.TongTien ?? 0);
                    doanhThuTheoNgay.Add(sum);
                    labelTheoNgay.Add(d.ToString("dd/MM"));
                }

                // -- Chart Tháng (12 tháng qua) --
                var doanhThuTheoThang = new List<decimal>();
                var labelTheoThang = new List<string>();
                for (int i = 11; i >= 0; i--)
                {
                    var d = DateTime.Now.AddMonths(-i);
                    var sum = rawDataForChart
                        .Where(x => x.NgayDat.HasValue && x.NgayDat.Value.Month == d.Month && x.NgayDat.Value.Year == d.Year)
                        .Sum(x => x.TongTien ?? 0);
                    doanhThuTheoThang.Add(sum);
                    labelTheoThang.Add(d.ToString("MM/yyyy"));
                }

                // -- Chart Năm (5 năm qua) --
                var doanhThuTheoNam = new List<decimal>();
                var labelTheoNam = new List<string>();
                for (int i = 4; i >= 0; i--)
                {
                    var y = DateTime.Now.Year - i;
                    var sum = rawDataForChart
                        .Where(x => x.NgayDat.HasValue && x.NgayDat.Value.Year == y)
                        .Sum(x => x.TongTien ?? 0);
                    doanhThuTheoNam.Add(sum);
                    labelTheoNam.Add(y.ToString());
                }

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
                // Gán lỗi để hiển thị ở View
                ViewBag.ErrorMessage = "Lỗi tải Dashboard: " + ex.Message + (ex.InnerException != null ? " (" + ex.InnerException.Message + ")" : "");

                // Khởi tạo giá trị mặc định tránh Crash View
                ViewBag.DoanhThuTheoNgay = "[]";
                ViewBag.LabelTheoNgay = "[]";
                ViewBag.DoanhThuTheoThang = "[]";
                ViewBag.LabelTheoThang = "[]";
                ViewBag.DoanhThuTheoNam = "[]";
                ViewBag.LabelTheoNam = "[]";
                ViewBag.DonHangChuaThanhToanTop10 = new List<DonHang>();
                // Đã xóa khởi tạo list hết hạn

                return View();
            }
        }
    }
}