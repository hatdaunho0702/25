using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WebApplication15.Models; // Đảm bảo namespace đúng

namespace WebApplication15.Areas.Admin.Controllers
{
    // [AuthorizeAdmin] // Bỏ comment nếu bạn đã có class AuthorizeAdmin
    public class PhieuNhapController : Controller
    {
        private DB_SkinFood1Entities db = new DB_SkinFood1Entities();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db?.Dispose();
            base.Dispose(disposing);
        }

        // 1. TRANG DANH SÁCH (Sửa lỗi 404)
        // GET: Admin/PhieuNhap
        public ActionResult Index()
        {
            var list = db.PhieuNhaps.OrderByDescending(x => x.NgayNhap).ToList();
            return View(list);
        }

        // Details: Hiển thị chi tiết một phiếu nhập
        // GET: Admin/PhieuNhap/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(400);
            }

            // Eager load chi tiết và sản phẩm để tránh lazy loading khi context đã bị dispose
            var phieu = db.PhieuNhaps
                          .Include("ChiTietPhieuNhaps.SanPham")
                          .Include("NhaCungCap")
                          .FirstOrDefault(p => p.MaPN == id.Value);

            if (phieu == null)
            {
                return HttpNotFound();
            }

            return View(phieu);
        }

        // 2. TRANG TẠO MỚI
        // GET: Admin/PhieuNhap/Create
        public ActionResult Create()
        {
            ViewBag.MaNCC = new SelectList(db.NhaCungCaps.ToList(), "MaNCC", "TenNCC");
            // Lấy danh sách sản phẩm đưa sang View để hiện trong dropdown chọn hàng
            ViewBag.Products = db.SanPhams.ToList();
            return View();
        }

        // 3. XỬ LÝ LƯU (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PhieuNhap phieuNhap, List<ChiTietPhieuNhap> details)
        {
            // Xóa các dòng detail rỗng hoặc không hợp lệ
            if (details != null)
            {
                details.RemoveAll(x => (x.MaSP <= 0) || ((x.SoLuong ?? 0) <= 0));
            }

            // Sau khi loại bỏ, kiểm tra lại danh sách
            if (details == null || details.Count == 0)
            {
                ModelState.AddModelError("", "Phiếu nhập phải có ít nhất 1 sản phẩm hợp lệ.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.MaNCC = new SelectList(db.NhaCungCaps.ToList(), "MaNCC", "TenNCC", phieuNhap?.MaNCC);
                ViewBag.Products = db.SanPhams.ToList();
                return View(phieuNhap);
            }

            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    phieuNhap.NgayNhap = DateTime.Now;

                    // Lưu phiếu nhập trước để lấy ID (MaPN)
                    db.PhieuNhaps.Add(phieuNhap);
                    db.SaveChanges();

                    decimal tong = 0m;

                    foreach (var d in details)
                    {
                        d.MaPN = phieuNhap.MaPN; // Gán ID phiếu nhập vừa tạo

                        int qty = d.SoLuong ?? 0;
                        decimal price = d.GiaNhap ?? 0;

                        // Tính thành tiền từng dòng
                        d.ThanhTien = qty * price;

                        db.ChiTietPhieuNhaps.Add(d);

                        // --- CẬP NHẬT KHO (Lưu ý: Nếu dùng Trigger SQL thì xóa đoạn này đi) ---
                        var sp = db.SanPhams.Find(d.MaSP);
                        if (sp != null)
                        {
                            // Cộng dồn số lượng
                            sp.SoLuongTon = (sp.SoLuongTon ?? 0) + qty;

                            // Cập nhật giá nhập mới nhất (nếu cần)
                            // sp.GiaNhap = price; 

                            db.Entry(sp).State = EntityState.Modified;
                        }
                        // ----------------------------------------------------------------------

                        tong += d.ThanhTien ?? 0m;
                    }

                    // Cập nhật lại Tổng tiền cho phiếu nhập
                    phieuNhap.TongTien = tong;
                    db.Entry(phieuNhap).State = EntityState.Modified;
                    db.SaveChanges();

                    tran.Commit();

                    TempData["SuccessMessage"] = $"Đã lưu phiếu nhập #{phieuNhap.MaPN} thành công.";

                    // Sửa lại: Quay về trang danh sách phiếu nhập
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);

                    TempData["ErrorMessage"] = "Lỗi khi lưu phiếu nhập: " + ex.Message;

                    ViewBag.MaNCC = new SelectList(db.NhaCungCaps.ToList(), "MaNCC", "TenNCC", phieuNhap?.MaNCC);
                    ViewBag.Products = db.SanPhams.ToList();
                    return View(phieuNhap);
                }
            }
        }
    }
}