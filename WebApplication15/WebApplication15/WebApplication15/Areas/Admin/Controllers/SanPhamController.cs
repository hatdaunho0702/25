using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using WebApplication15.Areas.Admin.Data;
using WebApplication15.Models;
using System.Data.SqlClient;
using System.Data.Entity;

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
            try
            {
                // Load products with related data for suppliers
                var sanPhams = db.SanPhams
                    .Include("ChiTietPhieuNhaps.PhieuNhap.NhaCungCap")
                    .ToList();
                return View(sanPhams);
            }
            catch (SqlException sqlEx)
            {
                // Fallback: some DB schemas may not have recently added columns mapped in EF model
                // Use a safe SQL projection to load minimal fields to avoid column missing errors
                System.Diagnostics.Debug.WriteLine("SQL error loading SanPhams: " + sqlEx.Message);
                TempData["ErrorMessage"] = "Lỗi khi tải sản phẩm từ cơ sở dữ liệu. Sử dụng dữ liệu tối giản thay thế.";

                var safeList = db.Database.SqlQuery<SanPham>(
                    "SELECT MaSP, TenSP, GiaBan, HinhAnh, SoLuongTon FROM SanPham"
                ).ToList();

                return View(safeList);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading SanPhams: " + ex.Message);
                TempData["ErrorMessage"] = "Có lỗi khi tải danh sách sản phẩm: " + ex.Message;
                return View(new List<SanPham>());
            }
        }

        public ActionResult Create()
        {
            ViewBag.MaLoai = new SelectList(db.LoaiSPs, "MaLoai", "TenLoai");
            ViewBag.MaTH = new SelectList(db.ThuongHieux, "MaTH", "TenTH");
            ViewBag.MaDM = new SelectList(db.DanhMucs, "MaDM", "TenDM");

            var model = new SanPham();
            return View(model);
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

                    // Set stock quantity to 0 for new products
                    sp.SoLuongTon = 0;

                    db.SanPhams.Add(sp);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Thêm sản phẩm thành công! Số lượng tồn kho hiện tại là 0. Vui lòng sử dụng chức năng Nhập Hàng để cập nhật số lượng.";
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

                // Preserve stock quantity - it should only be updated through PhieuNhap
                sp.SoLuongTon = existingSp.SoLuongTon;

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

        private bool HasOrderReferences(int maSP)
        {
            return db.ChiTietDonHangs.Any(ct => ct.MaSP == maSP);
        }

        private void RemoveSafeDependents(int maSP)
        {
            // Remove reviews
            var reviews = db.DanhGias.Where(d => d.MaSP == maSP).ToList();
            if (reviews.Any())
                db.DanhGias.RemoveRange(reviews);

            // Remove inventory details (ChiTietPhieuNhap)
            var chiNhaps = db.ChiTietPhieuNhaps.Where(c => c.MaSP == maSP).ToList();
            if (chiNhaps.Any())
                db.ChiTietPhieuNhaps.RemoveRange(chiNhaps);

            // Remove link table SanPham_ThuocTinh if exists (ThuocTinhMyPhams mapping)
            // EF model likely exposes ThuocTinhMyPhams navigation; remove entries via raw SQL if needed
            try
            {
                var linkRows = db.Database.SqlQuery<int?>("SELECT MaThuocTinh FROM SanPham_ThuocTinh WHERE MaSP = @p0", maSP).ToList();
                if (linkRows.Any())
                {
                    db.Database.ExecuteSqlCommand("DELETE FROM SanPham_ThuocTinh WHERE MaSP = @p0", maSP);
                }
            }
            catch
            {
                // ignore if table not present in schema
            }
        }

        public ActionResult Delete(int id)
        {
            try
            {
                var sp = db.SanPhams.Find(id);
                if (sp == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
                    return RedirectToAction("Index");
                }

                // Count dependents
                int orderRefs = db.ChiTietDonHangs.Count(ct => ct.MaSP == id);
                int reviewRefs = db.DanhGias.Count(d => d.MaSP == id);
                int chiNhapRefs = db.ChiTietPhieuNhaps.Count(c => c.MaSP == id);
                int thuocTinhRefs = 0;
                try
                {
                    thuocTinhRefs = db.Database.SqlQuery<int>("SELECT COUNT(1) FROM SanPham_ThuocTinh WHERE MaSP = @p0", id).Single();
                }
                catch
                {
                    // ignore if mapping table doesn't exist
                }

                if (orderRefs > 0)
                {
                    TempData["ErrorMessage"] = $"Không thể xóa sản phẩm vì có {orderRefs} chi tiết đơn hàng liên quan. Nếu bạn chắc chắn, hãy kiểm tra và xóa đơn hàng trước.";
                    return RedirectToAction("Index");
                }

                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        // remove safe dependent rows first
                        if (reviewRefs > 0)
                        {
                            var reviews = db.DanhGias.Where(d => d.MaSP == id).ToList();
                            db.DanhGias.RemoveRange(reviews);
                        }

                        if (chiNhapRefs > 0)
                        {
                            var chiNhaps = db.ChiTietPhieuNhaps.Where(c => c.MaSP == id).ToList();
                            db.ChiTietPhieuNhaps.RemoveRange(chiNhaps);
                        }

                        try
                        {
                            if (thuocTinhRefs > 0)
                            {
                                db.Database.ExecuteSqlCommand("DELETE FROM SanPham_ThuocTinh WHERE MaSP = @p0", id);
                            }
                        }
                        catch { }

                        db.SaveChanges();

                        if (!string.IsNullOrEmpty(sp.HinhAnh))
                            DeleteUploadedFile(sp.HinhAnh);

                        db.SanPhams.Remove(sp);
                        db.SaveChanges();
                        tran.Commit();

                        TempData["SuccessMessage"] = "Xóa sản phẩm thành công!";
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        System.Diagnostics.Debug.WriteLine("Error deleting product with dependents: " + ex.Message);

                        // Provide detailed info to admin to help debugging
                        TempData["ErrorMessage"] = "Không thể xóa sản phẩm do có dữ liệu liên quan hoặc lỗi cơ sở dữ liệu. " +
                            $"(Đơn hàng: {orderRefs}, Nhập kho: {chiNhapRefs}, Đánh giá: {reviewRefs}, Thuộc tính: {thuocTinhRefs})";
                        return RedirectToAction("Index");
                    }
                }
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
                            if (sp == null)
                            {
                                failedCount++;
                                continue;
                            }

                            int orderRefs = db.ChiTietDonHangs.Count(ct => ct.MaSP == id);
                            if (orderRefs > 0)
                            {
                                failedCount++;
                                continue;
                            }

                            using (var tran = db.Database.BeginTransaction())
                            {
                                try
                                {
                                    // remove safe dependents
                                    RemoveSafeDependents(id);
                                    db.SaveChanges();

                                    if (!string.IsNullOrEmpty(sp.HinhAnh))
                                        DeleteUploadedFile(sp.HinhAnh);

                                    db.SanPhams.Remove(sp);
                                    db.SaveChanges();
                                    tran.Commit();
                                    deletedCount++;
                                }
                                catch
                                {
                                    tran.Rollback();
                                    failedCount++;
                                }
                            }
                        }
                        catch
                        {
                            failedCount++;
                        }
                    }

                    if (deletedCount > 0)
                        TempData["SuccessMessage"] = $"Đã xóa {deletedCount} sản phẩm thành công!";
                    
                    if (failedCount > 0)
                        TempData["ErrorMessage"] = $"Không thể xóa {failedCount} sản phẩm vì có đơn hàng hoặc dữ liệu liên quan.";
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
                        int orderRefs = db.ChiTietDonHangs.Count(ct => ct.MaSP == sp.MaSP);
                        if (orderRefs > 0)
                        {
                            failedCount++;
                            continue;
                        }

                        using (var tran = db.Database.BeginTransaction())
                        {
                            try
                            {
                                RemoveSafeDependents(sp.MaSP);
                                db.SaveChanges();

                                if (!string.IsNullOrEmpty(sp.HinhAnh))
                                    DeleteUploadedFile(sp.HinhAnh);

                                db.SanPhams.Remove(sp);
                                db.SaveChanges();
                                tran.Commit();
                                deletedCount++;
                            }
                            catch
                            {
                                tran.Rollback();
                                failedCount++;
                            }
                        }
                    }
                    catch
                    {
                        failedCount++;
                    }
                }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForceDelete(int id)
        {
            try
            {
                var sp = db.SanPhams.Find(id);
                if (sp == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
                    return RedirectToAction("Index");
                }

                int orderRefs = db.ChiTietDonHangs.Count(ct => ct.MaSP == id);
                if (orderRefs > 0)
                {
                    TempData["ErrorMessage"] = $"Không thể xóa sản phẩm vì có {orderRefs} chi tiết đơn hàng liên quan.";
                    return RedirectToAction("Index");
                }

                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        // Remove reviews
                        var reviews = db.DanhGias.Where(d => d.MaSP == id).ToList();
                        if (reviews.Any()) db.DanhGias.RemoveRange(reviews);

                        // Remove chi tiet phieu nhap
                        var chiNhaps = db.ChiTietPhieuNhaps.Where(c => c.MaSP == id).ToList();
                        if (chiNhaps.Any()) db.ChiTietPhieuNhaps.RemoveRange(chiNhaps);

                        // Remove mapping rows if table exists
                        try
                        {
                            db.Database.ExecuteSqlCommand("DELETE FROM SanPham_ThuocTinh WHERE MaSP = @p0", id);
                        }
                        catch { }

                        db.SaveChanges();

                        if (!string.IsNullOrEmpty(sp.HinhAnh))
                            DeleteUploadedFile(sp.HinhAnh);

                        db.SanPhams.Remove(sp);
                        db.SaveChanges();
                        tran.Commit();

                        TempData["SuccessMessage"] = "Đã xóa sản phẩm và các dữ liệu phụ liên quan thành công.";
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        TempData["ErrorMessage"] = "Xóa không thành công: " + ex.Message;
                        return RedirectToAction("Index");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi khi xóa sản phẩm: " + ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}