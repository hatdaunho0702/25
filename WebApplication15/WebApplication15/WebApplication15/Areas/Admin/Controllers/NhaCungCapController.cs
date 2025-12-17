using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebApplication15.Models;

namespace WebApplication15.Areas.Admin.Controllers
{
    public class NhaCungCapController : Controller
    {
        private DB_SkinFood1Entities db = new DB_SkinFood1Entities();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db?.Dispose();
            base.Dispose(disposing);
        }

        // GET: Admin/NhaCungCap
        public ActionResult Index()
        {
            var list = db.NhaCungCaps.OrderByDescending(x => x.MaNCC).ToList();
            return View(list);
        }

        // GET: Admin/NhaCungCap/Create
        public ActionResult Create()
        {
            // Provide products list to allow associating existing product or creating new one
            ViewBag.Products = db.SanPhams.Select(p => new SelectListItem {
                Value = p.MaSP.ToString(),
                Text = p.TenSP
            }).ToList();
            return View();
        }

        // POST: Admin/NhaCungCap/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(NhaCungCap nhaCungCap)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.NhaCungCaps.Add(nhaCungCap);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Đã thêm nhà cung cấp thành công.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }
            return View(nhaCungCap);
        }

        // Utility action: tạo mẫu 5 nhà cung cấp, mỗi nhà cung cấp ~3 sản phẩm (dùng khi muốn nhanh tạo dữ liệu mẫu)
        public ActionResult SeedSampleSuppliers()
        {
            try
            {
                if (db.NhaCungCaps.Count() >= 5)
                {
                    TempData["ErrorMessage"] = "Cơ sở dữ liệu đã có >= 5 nhà cung cấp, không tạo thêm.";
                    return RedirectToAction("Index");
                }

                var sampleSuppliers = new List<NhaCungCap>();
                for (int i = 1; i <= 5; i++)
                {
                    sampleSuppliers.Add(new NhaCungCap
                    {
                        TenNCC = "NCC Mẫu " + i,
                        DiaChi = "Địa chỉ mẫu " + i,
                        SoDienThoai = "09000000" + i,
                        Email = $"ncc{i}@example.com",
                        NguoiLienHe = "Liên hệ " + i
                    });
                }

                db.NhaCungCaps.AddRange(sampleSuppliers);
                db.SaveChanges();

                // Tạo 3 sản phẩm cho mỗi nhà cung cấp mới
                var createdSuppliers = db.NhaCungCaps.OrderByDescending(n => n.MaNCC).Take(5).ToList();
                int sku = db.SanPhams.Any() ? db.SanPhams.Max(s => s.MaSP) + 1 : 1;

                foreach (var ncc in createdSuppliers)
                {
                    for (int j = 1; j <= 3; j++)
                    {
                        var sp = new SanPham
                        {
                            TenSP = $"Sản phẩm {ncc.TenNCC} - {j}",
                            GiaBan = 10000m * (j + 1),
                            MaDM = db.DanhMucs.Select(d => d.MaDM).FirstOrDefault(),
                            MaTH = db.ThuongHieux.Select(t => t.MaTH).FirstOrDefault(),
                            MaLoai = db.LoaiSPs.Select(l => l.MaLoai).FirstOrDefault(),
                            HinhAnh = "default.jpg",
                            MaNCC = ncc.MaNCC,
                            SoLuongTon = 10 * j,
                            // NgaySanXuat and HanSuDung are now stored in ChiTietPhieuNhap
                            // and exposed via computed properties on the partial SanPham class.
                            TrangThaiSanPham = "Kinh doanh"
                        };
                        db.SanPhams.Add(sp);
                    }
                }

                db.SaveChanges();

                TempData["SuccessMessage"] = "Đã tạo mẫu 5 nhà cung cấp cùng với ~3 sản phẩm mỗi nhà cung cấp.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tạo mẫu: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
