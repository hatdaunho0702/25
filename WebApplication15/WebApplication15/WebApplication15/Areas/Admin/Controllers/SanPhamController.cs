using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using WebApplication15.Areas.Admin.Data;
using WebApplication15.Models;

namespace WebApplication15.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class SanPhamController : Controller
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

        private string GetUploadPath()
        {
            return Path.Combine(Server.MapPath("~/"), "Content", "images", "products");
        }

        private string SaveUploadedFile(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
                return null;

            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            string fileExtension = Path.GetExtension(file.FileName).ToLower();
            
            if (!allowedExtensions.Contains(fileExtension))
                throw new Exception("Định dạng file không hỗ trợ. Vui lòng tải lên JPG, PNG, GIF hoặc WebP.");

            if (file.ContentLength > 5 * 1024 * 1024)
                throw new Exception("File quá lớn. Kích thước tối đa là 5MB.");

            string uploadPath = GetUploadPath();
            
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            string fileName = Guid.NewGuid().ToString() + fileExtension;
            string filePath = Path.Combine(uploadPath, fileName);

            file.SaveAs(filePath);

            return "products/" + fileName;
        }

        private void DeleteUploadedFile(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return;

            try
            {
                string filePath = Path.Combine(Server.MapPath("~/Content/images"), relativePath);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error deleting file: " + ex.Message);
            }
        }

        public ActionResult Index()
        {
            var sanPhams = db.SanPhams.ToList();
            return View(sanPhams);
        }

        public ActionResult Create()
        {
            ViewBag.MaLoai = new SelectList(db.LoaiSPs, "MaLoai", "TenLoai");
            ViewBag.MaTH = new SelectList(db.ThuongHieux, "MaTH", "TenTH");
            ViewBag.MaDM = new SelectList(db.DanhMucs, "MaDM", "TenDM");
            return View();
        }

        // Unified Save action: Insert when MaSP <= 0, Update when MaSP > 0
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(SanPham sp, HttpPostedFileBase imageFile)
        {
            try
            {
                if (sp == null)
                {
                    ModelState.AddModelError("", "Dữ liệu sản phẩm không hợp lệ.");
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.MaLoai = new SelectList(db.LoaiSPs, "MaLoai", "TenLoai", sp?.MaLoai);
                    ViewBag.MaTH = new SelectList(db.ThuongHieux, "MaTH", "TenTH", sp?.MaTH);
                    ViewBag.MaDM = new SelectList(db.DanhMucs, "MaDM", "TenDM", sp?.MaDM);
                    return View(sp == null || sp.MaSP <= 0 ? "Create" : "Edit", sp);
                }

                // Insert
                if (sp.MaSP <= 0)
                {
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        sp.HinhAnh = SaveUploadedFile(imageFile);
                    }

                    db.SanPhams.Add(sp);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                    return RedirectToAction("Index");
                }

                // Update
                var existingSp = db.SanPhams.Find(sp.MaSP);
                if (existingSp == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy sản phẩm để cập nhật.");
                    ViewBag.MaLoai = new SelectList(db.LoaiSPs, "MaLoai", "TenLoai", sp.MaLoai);
                    ViewBag.MaTH = new SelectList(db.ThuongHieux, "MaTH", "TenTH", sp.MaTH);
                    ViewBag.MaDM = new SelectList(db.DanhMucs, "MaDM", "TenDM", sp.MaDM);
                    return View("Edit", sp);
                }

                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    if (!string.IsNullOrEmpty(existingSp.HinhAnh))
                        DeleteUploadedFile(existingSp.HinhAnh);

                    sp.HinhAnh = SaveUploadedFile(imageFile);
                }
                else
                {
                    // keep existing image
                    sp.HinhAnh = existingSp.HinhAnh;
                }

                db.Entry(existingSp).CurrentValues.SetValues(sp);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi lưu sản phẩm: " + ex.Message);
            }

            ViewBag.MaLoai = new SelectList(db.LoaiSPs, "MaLoai", "TenLoai", sp?.MaLoai);
            ViewBag.MaTH = new SelectList(db.ThuongHieux, "MaTH", "TenTH", sp?.MaTH);
            ViewBag.MaDM = new SelectList(db.DanhMucs, "MaDM", "TenDM", sp?.MaDM);
            return View(sp == null || sp.MaSP <= 0 ? "Create" : "Edit", sp);
        }

        // Keep existing Create/Edit/Delete actions for backward compatibility (they will still work)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SanPham sp, HttpPostedFileBase imageFile)
        {
            // delegate to Save to keep logic consistent
            return Save(sp, imageFile);
        }

        public ActionResult Edit(int id)
        {
            var sp = db.SanPhams.Find(id);
            if (sp == null)
                return HttpNotFound();

            ViewBag.MaLoai = new SelectList(db.LoaiSPs, "MaLoai", "TenLoai", sp.MaLoai);
            ViewBag.MaTH = new SelectList(db.ThuongHieux, "MaTH", "TenTH", sp.MaTH);
            ViewBag.MaDM = new SelectList(db.DanhMucs, "MaDM", "TenDM", sp.MaDM);
            return View(sp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SanPham sp, HttpPostedFileBase imageFile)
        {
            // delegate to Save
            return Save(sp, imageFile);
        }

        public ActionResult Delete(int id)
        {
            try
            {
                var sp = db.SanPhams.Find(id);
                if (sp != null)
                {
                    if (!string.IsNullOrEmpty(sp.HinhAnh))
                        DeleteUploadedFile(sp.HinhAnh);

                    db.SanPhams.Remove(sp);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Xóa sản phẩm thành công!";
                    return RedirectToAction("Index");
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                TempData["ErrorMessage"] = "Không thể xóa sản phẩm này vì còn có đơn hàng hoặc đánh giá liên quan.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa sản phẩm: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteSelected(int[] idsToDelete)
        {
            try
            {
                if (idsToDelete != null && idsToDelete.Length > 0)
                {
                    int deletedCount = 0;
                    int failedCount = 0;

                    foreach (var id in idsToDelete)
                    {
                        try
                        {
                            var sp = db.SanPhams.Find(id);
                            if (sp != null)
                            {
                                if (!string.IsNullOrEmpty(sp.HinhAnh))
                                    DeleteUploadedFile(sp.HinhAnh);

                                db.SanPhams.Remove(sp);
                                deletedCount++;
                            }
                        }
                        catch
                        {
                            failedCount++;
                        }
                    }

                    db.SaveChanges();

                    if (deletedCount > 0)
                        TempData["SuccessMessage"] = $"Đã xóa {deletedCount} sản phẩm thành công!";
                    
                    if (failedCount > 0)
                        TempData["ErrorMessage"] = $"Không thể xóa {failedCount} sản phẩm vì còn có đơn hàng hoặc đánh giá liên quan.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không có sản phẩm nào được chọn để xóa.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAll()
        {
            try
            {
                var allProducts = db.SanPhams.ToList();
                int deletedCount = 0;
                int failedCount = 0;

                foreach (var sp in allProducts)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(sp.HinhAnh))
                            DeleteUploadedFile(sp.HinhAnh);

                        db.SanPhams.Remove(sp);
                        deletedCount++;
                    }
                    catch
                    {
                        failedCount++;
                    }
                }

                db.SaveChanges();

                if (deletedCount > 0)
                    TempData["SuccessMessage"] = $"Đã xóa {deletedCount} sản phẩm thành công!";
                
                if (failedCount > 0)
                    TempData["ErrorMessage"] = $"Không thể xóa {failedCount} sản phẩm vì còn có đơn hàng hoặc đánh giá liên quan.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa tất cả: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}