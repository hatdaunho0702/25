using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication15.Areas.Admin.Data;
using WebApplication15.Models;

namespace WebApplication15.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class TaiKhoanController : Controller
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

        public ActionResult Index()
        {
            var taiKhoans = db.TaiKhoans.ToList();
            return View(taiKhoans);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TaiKhoanCreateVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.MatKhau != model.NhapLaiMatKhau)
            {
                ModelState.AddModelError("", "Mật khẩu không khớp.");
                return View(model);
            }

            try
            {
                // Kiểm tra tên đăng nhập đã tồn tại chưa
                var existingUser = db.TaiKhoans.FirstOrDefault(t => t.TenDangNhap == model.TenDangNhap);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Tên đăng nhập đã tồn tại.");
                    return View(model);
                }

                // Tạo mới người dùng
                var nd = new NguoiDung
                {
                    HoTen = model.HoTenNguoiDung,
                    NgayTao = DateTime.Now
                };

                db.NguoiDungs.Add(nd);
                db.SaveChanges();

                // Tạo tài khoản
                var tk = new TaiKhoan
                {
                    TenDangNhap = model.TenDangNhap,
                    MatKhauHash = model.MatKhau,
                    VaiTro = model.VaiTro,
                    MaND = nd.MaND
                };

                db.TaiKhoans.Add(tk);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Tạo tài khoản thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                return View(model);
            }
        }

        public ActionResult Edit(int id)
        {
            var tk = db.TaiKhoans.Find(id);
            if (tk == null)
                return HttpNotFound();

            ViewBag.MaND = new SelectList(db.NguoiDungs, "MaND", "HoTen", tk.MaND);
            return View(tk);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TaiKhoan tk)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(tk).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Cập nhật tài khoản thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                }
            }

            ViewBag.MaND = new SelectList(db.NguoiDungs, "MaND", "HoTen", tk.MaND);
            return View(tk);
        }

        public ActionResult Delete(int id)
        {
            try
            {
                var tk = db.TaiKhoans.Find(id);
                if (tk != null)
                {
                    db.TaiKhoans.Remove(tk);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Xóa tài khoản thành công!";
                    return RedirectToAction("Index");
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                TempData["ErrorMessage"] = "Không thể xóa tài khoản này vì còn có dữ liệu liên quan.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa tài khoản: " + ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}