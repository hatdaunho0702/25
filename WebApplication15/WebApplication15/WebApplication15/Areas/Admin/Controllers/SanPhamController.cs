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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SanPham sp, HttpPostedFileBase imageFile)
        {
            try
            {
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    sp.HinhAnh = SaveUploadedFile(imageFile);
                }

                if (ModelState.IsValid)
                {
                    db.SanPhams.Add(sp);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi tải lên: " + ex.Message);
            }

            ViewBag.MaLoai = new SelectList(db.LoaiSPs, "MaLoai", "TenLoai", sp.MaLoai);
            ViewBag.MaTH = new SelectList(db.ThuongHieux, "MaTH", "TenTH", sp.MaTH);
            ViewBag.MaDM = new SelectList(db.DanhMucs, "MaDM", "TenDM", sp.MaDM);
            return View(sp);
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
            try
            {
                var existingSp = db.SanPhams.Find(sp.MaSP);
                if (existingSp != null)
                {
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        if (!string.IsNullOrEmpty(existingSp.HinhAnh))
                            DeleteUploadedFile(existingSp.HinhAnh);

                        sp.HinhAnh = SaveUploadedFile(imageFile);
                    }
                    else
                    {
                        sp.HinhAnh = existingSp.HinhAnh;
                    }

                    db.Entry(existingSp).CurrentValues.SetValues(sp);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật: " + ex.Message);
            }

            ViewBag.MaLoai = new SelectList(db.LoaiSPs, "MaLoai", "TenLoai", sp.MaLoai);
            ViewBag.MaTH = new SelectList(db.ThuongHieux, "MaTH", "TenTH", sp.MaTH);
            ViewBag.MaDM = new SelectList(db.DanhMucs, "MaDM", "TenDM", sp.MaDM);
            return View(sp);
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
    }
}