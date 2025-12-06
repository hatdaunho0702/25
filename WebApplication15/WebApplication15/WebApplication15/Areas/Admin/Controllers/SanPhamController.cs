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
        // GET: Admin/SanPham
        DB_SkinFood1Entities db = new DB_SkinFood1Entities();

        private string GetUploadPath()
        {
            return Path.Combine(Server.MapPath("~/"), "Content", "images", "products");
        }

        private string SaveUploadedFile(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
                return null;

            // Validate file extension
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            string fileExtension = Path.GetExtension(file.FileName).ToLower();
            
            if (!allowedExtensions.Contains(fileExtension))
                throw new Exception("Định dạng file không hỗ trợ. Vui lòng tải lên JPG, PNG, GIF hoặc WebP.");

            // Validate file size (max 5MB)
            if (file.ContentLength > 5 * 1024 * 1024)
                throw new Exception("File quá lớn. Kích thước tối đa là 5MB.");

            string uploadPath = GetUploadPath();
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            // Generate unique filename
            string fileName = Guid.NewGuid().ToString() + fileExtension;
            string filePath = Path.Combine(uploadPath, fileName);

            // Save the file
            file.SaveAs(filePath);

            // Return relative path for storage in database
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
                // Log error but don't throw
                System.Diagnostics.Debug.WriteLine("Error deleting file: " + ex.Message);
            }
        }

        public ActionResult Index()
        {
            return View(db.SanPhams.ToList());
        }

        public ActionResult Create()
        {
            ViewBag.MaLoai = new SelectList(db.LoaiSPs, "MaLoai", "TenLoai");
            ViewBag.MaTH = new SelectList(db.ThuongHieux, "MaTH", "TenTH");
            return View();
        }

        [HttpPost]
        public ActionResult Create(SanPham sp, HttpPostedFileBase imageFile)
        {
            try
            {
                // Handle image upload
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    sp.HinhAnh = SaveUploadedFile(imageFile);
                }

                if (ModelState.IsValid)
                {
                    db.SanPhams.Add(sp);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi tải lên: " + ex.Message);
            }

            ViewBag.MaLoai = new SelectList(db.LoaiSPs, "MaLoai", "TenLoai", sp.MaLoai);
            ViewBag.MaTH = new SelectList(db.ThuongHieux, "MaTH", "TenTH", sp.MaTH);
            return View(sp);
        }

        public ActionResult Edit(int id)
        {
            var sp = db.SanPhams.Find(id);
            ViewBag.MaLoai = new SelectList(db.LoaiSPs, "MaLoai", "TenLoai", sp.MaLoai);
            ViewBag.MaTH = new SelectList(db.ThuongHieux, "MaTH", "TenTH", sp.MaTH);
            return View(sp);
        }

        [HttpPost]
        public ActionResult Edit(SanPham sp, HttpPostedFileBase imageFile)
        {
            try
            {
                var existingSp = db.SanPhams.Find(sp.MaSP);
                if (existingSp != null)
                {
                    // Handle image upload
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(existingSp.HinhAnh))
                            DeleteUploadedFile(existingSp.HinhAnh);

                        sp.HinhAnh = SaveUploadedFile(imageFile);
                    }
                    else
                    {
                        // Keep existing image if no new image uploaded
                        sp.HinhAnh = existingSp.HinhAnh;
                    }

                    db.Entry(existingSp).CurrentValues.SetValues(sp);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật: " + ex.Message);
            }

            ViewBag.MaLoai = new SelectList(db.LoaiSPs, "MaLoai", "TenLoai", sp.MaLoai);
            ViewBag.MaTH = new SelectList(db.ThuongHieux, "MaTH", "TenTH", sp.MaTH);
            return View(sp);
        }

        public ActionResult Delete(int id)
        {
            try
            {
                var sp = db.SanPhams.Find(id);
                if (sp != null)
                {
                    // Delete uploaded image if exists
                    if (!string.IsNullOrEmpty(sp.HinhAnh))
                        DeleteUploadedFile(sp.HinhAnh);

                    db.SanPhams.Remove(sp);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
            {
                // Xóa thất bại do có dữ liệu liên quan
                TempData["ErrorMessage"] = "Không thể xóa sản phẩm này vì còn có đơn hàng hoặc đánh giá liên quan.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa sản phẩm: " + ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}