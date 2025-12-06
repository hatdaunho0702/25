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
        DB_SkinFood1Entities db = new DB_SkinFood1Entities();

        public ActionResult Index()
        {
            return View(db.TaiKhoans.ToList());
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(TaiKhoanCreateVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.MatKhau != model.NhapLaiMatKhau)
            {
                ModelState.AddModelError("", "Mật khẩu không khớp.");
                return View(model);
            }

            // 1. Tạo mới người dùng
            var nd = new NguoiDung
            {
                HoTen = model.HoTenNguoiDung,
                NgayTao = DateTime.Now
            };

            db.NguoiDungs.Add(nd);
            db.SaveChanges();  // => Sinh ra MaND

            // 2. Tạo tài khoản
            var tk = new TaiKhoan
            {
                TenDangNhap = model.TenDangNhap,
                MatKhauHash = model.MatKhau,     // Nếu có hash thì hash trước
                VaiTro = model.VaiTro,
                MaND = nd.MaND
            };

            db.TaiKhoans.Add(tk);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        //public ActionResult Create()
        //{
        //    ViewBag.MaND = new SelectList(db.NguoiDungs, "MaND", "HoTen");
        //    return View();
        //}

        //[HttpPost]
        //public ActionResult Create(TaiKhoan tk)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.TaiKhoans.Add(tk);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.MaND = new SelectList(db.NguoiDungs, "MaND", "HoTen", tk.MaND);
        //    return View(tk);
        //}

        public ActionResult Edit(int id)
        {
            var tk = db.TaiKhoans.Find(id);
            ViewBag.MaND = new SelectList(db.NguoiDungs, "MaND", "HoTen", tk.MaND);
            return View(tk);
        }

        [HttpPost]
        public ActionResult Edit(TaiKhoan tk)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tk).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
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
                    return RedirectToAction("Index");
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
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