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

        // GET: Admin/PhieuXuat/Create
        public ActionResult Create()
        {
            ViewBag.Products = GetProductsForView();
            return View();
        }

        // POST: Admin/PhieuXuat/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PhieuXuat phieuXuat, List<ChiTietPhieuXuat> details)
        {
            if (details != null)
            {
                details.RemoveAll(x => x.MaSP <= 0 || (x.SoLuong ?? 0) <= 0);
            }

            if (details == null || details.Count == 0)
            {
                ModelState.AddModelError("", "Phi?u xu?t ph?i có ít nh?t 1 s?n ph?m h?p l?.");
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

                    // Ki?m tra t?n kho trý?c khi lýu
                    foreach (var d in details)
                    {
                        var sp = db.SanPhams.Find(d.MaSP);
                        int qty = d.SoLuong ?? 0;
                        if (sp == null)
                        {
                            tran.Rollback();
                            ModelState.AddModelError("", "S?n ph?m không t?n t?i.");
                            ViewBag.Products = GetProductsForView();
                            return View(phieuXuat);
                        }

                        if ((sp.SoLuongTon ?? 0) < qty)
                        {
                            tran.Rollback();
                            ModelState.AddModelError("", $"Không ð? t?n kho cho s?n ph?m {sp.TenSP}. (C?n: {sp.SoLuongTon ?? 0})");
                            ViewBag.Products = GetProductsForView();
                            return View(phieuXuat);
                        }
                    }

                    db.PhieuXuats.Add(phieuXuat);
                    db.SaveChanges();

                    decimal tong = 0m;

                    foreach (var d in details)
                    {
                        d.MaPX = phieuXuat.MaPX;
                        int qty = d.SoLuong ?? 0;
                        // Ensure DonGia is set (fallback to product GiaBan)
                        var spFallback = db.SanPhams.Find(d.MaSP);
                        decimal price = d.DonGia ?? (spFallback?.GiaBan ?? 0m);
                        d.DonGia = price;
                        d.ThanhTien = qty * price;

                        db.ChiTietPhieuXuats.Add(d);

                        var sp = db.SanPhams.Find(d.MaSP);
                        if (sp != null)
                        {
                            sp.SoLuongTon = (sp.SoLuongTon ?? 0) - qty;
                            db.Entry(sp).State = EntityState.Modified;
                        }

                        tong += d.ThanhTien ?? 0m;
                    }

                    phieuXuat.TongTien = tong;
                    db.Entry(phieuXuat).State = EntityState.Modified;
                    db.SaveChanges();

                    tran.Commit();

                    TempData["SuccessMessage"] = $"Ð? lýu phi?u xu?t #{phieuXuat.MaPX} thành công.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    ModelState.AddModelError("", "L?i h? th?ng: " + ex.Message);
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
