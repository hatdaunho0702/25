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
    public class DanhGiasController : Controller
    {
        private DB_SkinFood1Entities db = new DB_SkinFood1Entities();

        // GET: Admin/DanhGias
        public ActionResult Index()
        {
            var danhGias = db.DanhGias
                .Include(d => d.NguoiDung)
                .Include(d => d.SanPham)
                .OrderByDescending(d => d.NgayDanhGia)
                .ToList();

            return View(danhGias);
        }

        // GET: Admin/DanhGias/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            DanhGia danhGia = db.DanhGias.Find(id);
            if (danhGia == null)
            {
                return HttpNotFound();
            }

            return View(danhGia);
        }

        // GET: Admin/DanhGias/Create
        public ActionResult Create()
        {
            ViewBag.MaND = new SelectList(db.NguoiDungs, "MaND", "HoTen");
            ViewBag.MaSP = new SelectList(db.SanPhams, "MaSP", "TenSP");
            return View();
        }

        // POST: Admin/DanhGias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaDG,MaSP,MaND,NoiDung,Diem,NgayDanhGia,DuocApprove,TraLoiAdmin,ThoiGianTraLoi")] DanhGia danhGia)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    danhGia.NgayDanhGia = DateTime.Now;
                    db.DanhGias.Add(danhGia);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Thêm đánh giá thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                }
            }

            ViewBag.MaND = new SelectList(db.NguoiDungs, "MaND", "HoTen", danhGia.MaND);
            ViewBag.MaSP = new SelectList(db.SanPhams, "MaSP", "TenSP", danhGia.MaSP);
            return View(danhGia);
        }

        // GET: Admin/DanhGias/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            DanhGia danhGia = db.DanhGias.Find(id);
            if (danhGia == null)
            {
                return HttpNotFound();
            }

            ViewBag.MaND = new SelectList(db.NguoiDungs, "MaND", "HoTen", danhGia.MaND);
            ViewBag.MaSP = new SelectList(db.SanPhams, "MaSP", "TenSP", danhGia.MaSP);
            return View(danhGia);
        }

        // POST: Admin/DanhGias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaDG,MaSP,MaND,NoiDung,Diem,NgayDanhGia,DuocApprove,TraLoiAdmin,ThoiGianTraLoi")] DanhGia danhGia)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(danhGia).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Cập nhật đánh giá thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                }
            }

            ViewBag.MaND = new SelectList(db.NguoiDungs, "MaND", "HoTen", danhGia.MaND);
            ViewBag.MaSP = new SelectList(db.SanPhams, "MaSP", "TenSP", danhGia.MaSP);
            return View(danhGia);
        }

        // GET: Admin/DanhGias/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            DanhGia danhGia = db.DanhGias.Find(id);
            if (danhGia == null)
            {
                return HttpNotFound();
            }

            return View(danhGia);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteSelected(int[] ids)
        {
            try
            {
                if (ids != null && ids.Length > 0)
                {
                    foreach (var id in ids)
                    {
                        var item = db.DanhGias.Find(id);
                        if (item != null)
                        {
                            db.DanhGias.Remove(item);
                        }
                    }

                    db.SaveChanges();
                    TempData["SuccessMessage"] = $"Đã xóa {ids.Length} đánh giá thành công!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // POST: Admin/DanhGias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                DanhGia danhGia = db.DanhGias.Find(id);
                if (danhGia != null)
                {
                    db.DanhGias.Remove(danhGia);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Xóa đánh giá thành công!";
                }
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
