using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication15.Areas.Admin.Data;
using WebApplication15.Models;
using System.Data.Entity;
using System.Data.Entity.SqlServer;

namespace WebApplication15.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class KhuyenMaisController : Controller
    {
        private DB_SkinFood1Entities db = new DB_SkinFood1Entities();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db?.Dispose();
            base.Dispose(disposing);
        }

        // GET: Admin/KhuyenMais
        public ActionResult Index()
        {
            var list = db.KhuyenMais.OrderByDescending(k => k.NgayBatDau).ToList();
            return View(list);
        }

        // GET: Admin/KhuyenMais/Details/5
        public ActionResult Details(int? id)
        {
            if (!id.HasValue) return new HttpStatusCodeResult(400);
            var km = db.KhuyenMais.Find(id.Value);
            if (km == null) return HttpNotFound();
            return View(km);
        }

        // GET: Admin/KhuyenMais/Create
        public ActionResult Create()
        {
            // Pass product list for selection in view if needed
            ViewBag.Products = db.SanPhams
                .Select(p => new SelectListItem { Value = SqlFunctions.StringConvert((double)p.MaSP).Trim(), Text = p.TenSP })
                .ToList();
            return View();
        }

        // POST: Admin/KhuyenMais/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(KhuyenMai model, List<int> SelectedProductIds)
        {
            try
            {
                // Validation
                if (model.NgayBatDau.HasValue && model.NgayKetThuc.HasValue && model.NgayBatDau.Value >= model.NgayKetThuc.Value)
                {
                    ModelState.AddModelError("NgayKetThuc", "Ngày kết thúc phải lớn hơn ngày bắt đầu.");
                }

                if (model.GiaTriGiam <= 0)
                {
                    ModelState.AddModelError("GiaTriGiam", "Giá trị giảm phải lớn hơn 0.");
                }

                if (!string.IsNullOrEmpty(model.LoaiGiamGia) && model.LoaiGiamGia.Trim().Equals("PhanTram", StringComparison.OrdinalIgnoreCase))
                {
                    if (model.GiaTriGiam > 100)
                        ModelState.AddModelError("GiaTriGiam", "Phần trăm giảm không được lớn hơn 100.");
                }

                // MaCode unique
                if (!string.IsNullOrEmpty(model.MaCode) && db.KhuyenMais.Any(k => k.MaCode == model.MaCode))
                {
                    ModelState.AddModelError("MaCode", "Mã code đã tồn tại trong hệ thống.");
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Products = db.SanPhams
                        .Select(p => new SelectListItem { Value = SqlFunctions.StringConvert((double)p.MaSP).Trim(), Text = p.TenSP })
                        .ToList();
                    return View(model);
                }

                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.KhuyenMais.Add(model);
                        db.SaveChanges(); // để có MaKM

                        if (SelectedProductIds != null && SelectedProductIds.Count > 0)
                        {
                            // Load navigation collection and add products
                            db.Entry(model).Collection(m => m.SanPhams).Load();

                            foreach (var maSP in SelectedProductIds.Distinct())
                            {
                                var sp = db.SanPhams.Find(maSP);
                                if (sp != null)
                                {
                                    model.SanPhams.Add(sp);
                                }
                            }

                            db.SaveChanges();
                        }

                        tran.Commit();
                        TempData["SuccessMessage"] = "Thêm khuyến mãi thành công";
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        ModelState.AddModelError("", "Lỗi khi lưu khuyến mãi: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi: " + ex.Message);
            }

            ViewBag.Products = db.SanPhams
                .Select(p => new SelectListItem { Value = SqlFunctions.StringConvert((double)p.MaSP).Trim(), Text = p.TenSP })
                .ToList();
            return View(model);
        }

        // GET: Admin/KhuyenMais/Edit/5
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue) return new HttpStatusCodeResult(400);
            var km = db.KhuyenMais.Find(id.Value);
            if (km == null) return HttpNotFound();
            return View(km);
        }

        // POST: Admin/KhuyenMais/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(KhuyenMai model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Ensure TrangThai binds correctly from dropdown string
                    // Model.TrangThai is nullable bool, if form posts "true" or "false" it will bind automatically.

                    db.Entry(model).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Cập nhật khuyến mãi thành công";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật: " + ex.Message);
            }
            // repopulate any needed ViewBag
            return View(model);
        }

        // GET: Admin/KhuyenMais/Delete/5
        public ActionResult Delete(int? id)
        {
            if (!id.HasValue) return new HttpStatusCodeResult(400);
            var km = db.KhuyenMais.Find(id.Value);
            if (km == null) return HttpNotFound();
            return View(km);
        }

        // POST: Admin/KhuyenMais/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                var km = db.KhuyenMais.Find(id);
                if (km != null)
                {
                    db.KhuyenMais.Remove(km);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Xóa khuyến mãi thành công";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa: " + ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}
