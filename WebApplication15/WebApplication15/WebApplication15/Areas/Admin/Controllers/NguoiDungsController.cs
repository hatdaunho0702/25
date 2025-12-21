using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication15.Areas.Admin.Data;
using WebApplication15.Models;

namespace WebApplication15.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class NguoiDungsController : Controller
    {
        private DB_SkinFood1Entities db = new DB_SkinFood1Entities();

        // GET: Admin/NguoiDungs
        public ActionResult Index(string search)
        {
            ViewBag.Search = search;

            var nguoiDungs = db.NguoiDungs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                nguoiDungs = nguoiDungs.Where(n => ((n.HoTen ?? "").ToLower().Contains(s)
                                                   || (n.SoDienThoai ?? "").Contains(s)
                                                   || ((n.DiaChi ?? "").ToLower().Contains(s))));
            }

            return View(nguoiDungs.ToList());
        }

        // GET: Admin/NguoiDungs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            NguoiDung nguoiDung = db.NguoiDungs.Find(id);
            if (nguoiDung == null)
            {
                return HttpNotFound();
            }

            return View(nguoiDung);
        }

        // GET: Admin/NguoiDungs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/NguoiDungs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaND,HoTen,SoDienThoai,DiaChi,GioiTinh,NgaySinh,NgayTao")] NguoiDung nguoiDung)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    nguoiDung.NgayTao = DateTime.Now;
                    db.NguoiDungs.Add(nguoiDung);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Thêm người dùng thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                }
            }

            return View(nguoiDung);
        }

        // GET: Admin/NguoiDungs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            NguoiDung nguoiDung = db.NguoiDungs.Find(id);
            if (nguoiDung == null)
            {
                return HttpNotFound();
            }

            return View(nguoiDung);
        }

        // POST: Admin/NguoiDungs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaND,HoTen,SoDienThoai,DiaChi,GioiTinh,NgaySinh,NgayTao")] NguoiDung nguoiDung)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(nguoiDung).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Cập nhật người dùng thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                }
            }

            return View(nguoiDung);
        }

        // GET: Admin/NguoiDungs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            NguoiDung nguoiDung = db.NguoiDungs.Find(id);
            if (nguoiDung == null)
            {
                return HttpNotFound();
            }

            return View(nguoiDung);
        }

        // POST: Admin/NguoiDungs/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                NguoiDung nguoiDung = db.NguoiDungs.Find(id);
                if (nguoiDung != null)
                {
                    db.NguoiDungs.Remove(nguoiDung);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Xóa người dùng thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy người dùng để xóa.";
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                TempData["ErrorMessage"] = "Không thể xóa người dùng này vì còn có dữ liệu liên quan (tài khoản, đơn hàng).";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
