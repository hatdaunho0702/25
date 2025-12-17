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

            // Truyền danh sách sản phẩm đã được project để tránh vòng tham chiếu khi serialize JSON
            var safeProducts = db.SanPhams
                .Select(p => new
                {
                    p.MaSP,
                    p.TenSP,
                    GiaBan = p.GiaBan ?? 0m,
                    SoLuongTon = p.SoLuongTon ?? 0,
                    MaNCC = p.MaNCC
                })
                .ToList();

            ViewBag.Products = safeProducts;

            // Thêm danh sách suppliers để dùng trong JS (cascading dropdown)
            ViewBag.Suppliers = db.NhaCungCaps.ToList();
            return View();
        }

        // 3. XỬ LÝ LƯU (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PhieuNhap phieuNhap, List<ChiTietPhieuNhap> details)
        {
            // --- 1. Lọc và kiểm tra danh sách chi tiết ---
            if (details != null)
            {
                details.RemoveAll(x => (x.MaSP <= 0) || ((x.SoLuong ?? 0) <= 0));
            }

            if (details == null || details.Count == 0)
            {
                ModelState.AddModelError("", "Phiếu nhập phải có ít nhất 1 sản phẩm hợp lệ.");
            }

            // --- 2. Nếu dữ liệu hợp lệ thì thực hiện Transaction ---
            if (ModelState.IsValid)
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        // Lưu Header
                        phieuNhap.NgayNhap = DateTime.Now; // (Optional) Gán ngày nhập nếu cần
                        db.PhieuNhaps.Add(phieuNhap);
                        db.SaveChanges(); // Save để lấy MaPN tự tăng

                        foreach (var d in details)
                        {
                            d.MaPN = phieuNhap.MaPN; // Gán MaPN vừa sinh ra
                            d.ThanhTien = (d.SoLuong ?? 0) * (d.GiaNhap ?? 0);

                            db.ChiTietPhieuNhaps.Add(d);

                            // Cập nhật số lượng tồn vào bảng SanPham (nếu không dùng Trigger)
                            var sp = db.SanPhams.Find(d.MaSP);
                            if (sp != null)
                            {
                                sp.SoLuongTon = (sp.SoLuongTon ?? 0) + (d.SoLuong ?? 0);
                            }
                        }

                        db.SaveChanges();
                        tran.Commit();

                        // >>> THÀNH CÔNG: Chuyển hướng
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        // Ghi lỗi vào ModelState để hiển thị ra View
                        ModelState.AddModelError("", "Có lỗi xảy ra khi lưu: " + ex.Message);
                    }
                }
            }

            // --- 3. XỬ LÝ KHI CÓ LỖI (Validation sai HOẶC Exception) ---
            // Code sẽ chạy xuống đây nếu:
            // a. ModelState.IsValid == false
            // b. Bị nhảy vào catch (Exception)

            // Phải nạp lại ViewBag để Dropdown không bị lỗi null
            ViewBag.MaNCC = new SelectList(db.NhaCungCaps.ToList(), "MaNCC", "TenNCC", phieuNhap?.MaNCC);

            ViewBag.Products = db.SanPhams
                .Select(p => new
                {
                    p.MaSP,
                    p.TenSP,
                    GiaBan = p.GiaBan ?? 0m,
                    SoLuongTon = p.SoLuongTon ?? 0,
                    MaNCC = p.MaNCC
                })
                .ToList();

            ViewBag.Suppliers = db.NhaCungCaps.ToList();

            // >>> TRẢ VỀ VIEW ĐỂ SỬA LỖI
            return View(phieuNhap);
        }
    }
}