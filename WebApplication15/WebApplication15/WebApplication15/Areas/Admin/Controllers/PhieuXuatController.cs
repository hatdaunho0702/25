using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WebApplication15.Models;

namespace WebApplication15.Areas.Admin.Controllers
{
    public class PhieuXuatController : Controller
    {
        private DB_SkinFood1Entities db = new DB_SkinFood1Entities();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db?.Dispose();
            base.Dispose(disposing);
        }

        // Helper to provide a safe product list for serialization in views
        private object GetProductsForView()
        {
            return db.SanPhams
                     .Select(p => new
                     {
                         p.MaSP,
                         p.TenSP,
                         GiaBan = p.GiaBan ?? 0m,
                         SoLuongTon = p.SoLuongTon ?? 0
                     })
                     .ToList();
        }

        // GET: Admin/PhieuXuat
        public ActionResult Index()
        {
            // Eager load details and product to allow computing totals in view
            var list = db.PhieuXuats.Include("ChiTietPhieuXuats.SanPham").OrderByDescending(x => x.NgayXuat).ToList();
            return View(list);
        }

        // Utility: Recalculate totals for existing PhieuXuats from their ChiTietPhieuXuats
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RecalculateTotals()
        {
            try
            {
                var phieus = db.PhieuXuats.Include("ChiTietPhieuXuats").ToList();
                int updated = 0;
                foreach (var p in phieus)
                {
                    decimal sum = 0m;
                    if (p.ChiTietPhieuXuats != null && p.ChiTietPhieuXuats.Any())
                    {
                        sum = p.ChiTietPhieuXuats.Sum(ct => (ct.ThanhTien ?? 0m));
                    }

                    if (p.TongTien == null || p.TongTien != sum)
                    {
                        p.TongTien = sum;
                        db.Entry(p).State = EntityState.Modified;
                        updated++;
                    }
                }

                db.SaveChanges();
                TempData["SuccessMessage"] = $"Đã cập nhật tổng tiền cho {updated} phiếu xuất.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi cập nhật tổng tiền: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // GET: Admin/PhieuXuat/Create
        public ActionResult Create()
        {
            ViewBag.Products = GetProductsForView();
            return View();
        }

        // POST: Admin/PhieuXuat/Create
        // POST: Admin/PhieuXuat/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PhieuXuat phieuXuat, List<ChiTietPhieuXuat> details)
        {
            // 1. Lọc dữ liệu rác (Sản phẩm không hợp lệ hoặc số lượng <= 0)
            if (details != null)
            {
                details.RemoveAll(x => x.MaSP <= 0 || (x.SoLuong ?? 0) <= 0);
            }

            // 2. Validate danh sách chi tiết
            if (details == null || details.Count == 0)
            {
                ModelState.AddModelError("", "Phiếu xuất phải có ít nhất 1 sản phẩm hợp lệ.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Products = GetProductsForView();
                return View(phieuXuat);
            }

            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    phieuXuat.NgayXuat = DateTime.Now;

                    // -----------------------------------------------------------------
                    // BƯỚC 1: KIỂM TRA TỒN KHO (CHỈ KIỂM TRA, KHÔNG TRỪ)
                    // -----------------------------------------------------------------
                    foreach (var d in details)
                    {
                        var sp = db.SanPhams.Find(d.MaSP);
                        int qty = d.SoLuong ?? 0;

                        if (sp == null)
                        {
                            tran.Rollback();
                            ModelState.AddModelError("", $"Sản phẩm ID {d.MaSP} không tồn tại.");
                            ViewBag.Products = GetProductsForView();
                            return View(phieuXuat);
                        }

                        // Nếu số lượng muốn xuất lớn hơn tồn kho hiện tại -> Báo lỗi ngay
                        if ((sp.SoLuongTon ?? 0) < qty)
                        {
                            tran.Rollback();
                            ModelState.AddModelError("", $"Sản phẩm '{sp.TenSP}' không đủ hàng. (Tồn: {sp.SoLuongTon ?? 0}, Muốn xuất: {qty})");
                            ViewBag.Products = GetProductsForView();
                            return View(phieuXuat);
                        }
                    }

                    // -----------------------------------------------------------------
                    // BƯỚC 2: LƯU PHIẾU XUẤT (HEADER)
                    // -----------------------------------------------------------------
                    db.PhieuXuats.Add(phieuXuat);
                    db.SaveChanges(); // Lưu để lấy MaPX tự tăng

                    decimal tong = 0m;

                    // -----------------------------------------------------------------
                    // BƯỚC 3: LƯU CHI TIẾT (DETAILS)
                    // -----------------------------------------------------------------
                    foreach (var d in details)
                    {
                        d.MaPX = phieuXuat.MaPX; // Gán ID phiếu vừa tạo
                        int qty = d.SoLuong ?? 0;

                        // Lấy giá bán hiện tại làm đơn giá xuất (nếu form không gửi lên)
                        var spCurrent = db.SanPhams.Find(d.MaSP);
                        decimal price = d.DonGia ?? (spCurrent?.GiaBan ?? 0m);

                        d.DonGia = price;
                        d.ThanhTien = qty * price;

                        db.ChiTietPhieuXuats.Add(d);

                        // *** QUAN TRỌNG: ĐÃ XÓA CODE TRỪ KHO Ở ĐÂY ***
                        // Lý do: Trigger SQL (trg_CapNhatKho_Xuat) sẽ tự động trừ khi dòng này được lưu xuống DB.
                        // Nếu để code C# trừ nữa sẽ bị trừ 2 lần -> Gây lỗi âm kho (-50).

                        tong += d.ThanhTien ?? 0m;
                    }

                    // -----------------------------------------------------------------
                    // BƯỚC 4: CẬP NHẬT TỔNG TIỀN VÀ COMMIT
                    // -----------------------------------------------------------------
                    phieuXuat.TongTien = tong;
                    db.Entry(phieuXuat).State = EntityState.Modified;

                    db.SaveChanges(); // Lúc này Trigger SQL sẽ chạy và trừ kho chính xác
                    tran.Commit();

                    TempData["SuccessMessage"] = $"Đã tạo phiếu xuất #{phieuXuat.MaPX} thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                    ViewBag.Products = GetProductsForView();
                    return View(phieuXuat);
                }
            }
        }

        // GET: Admin/PhieuXuat/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(400);

            var phieu = db.PhieuXuats
                         .Include("ChiTietPhieuXuats.SanPham")
                         .FirstOrDefault(p => p.MaPX == id.Value);

            if (phieu == null) return HttpNotFound();

            return View(phieu);
        }
    }
}
