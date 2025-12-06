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
    public class LoaiSPController : Controller
    {
        // GET: Admin/LoaiSP

        DB_SkinFood1Entities db = new DB_SkinFood1Entities();

        public ActionResult Index() => View(db.LoaiSPs.ToList());

        public ActionResult Create()
        {
            ViewBag.MaDM = new SelectList(db.DanhMucs, "MaDM", "TenDM");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(LoaiSP loai)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.LoaiSPs.Add(loai);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Thêm loại sản phẩm thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                }
            }

            ViewBag.MaDM = new SelectList(db.DanhMucs, "MaDM", "TenDM", loai.MaDM);
            return View(loai);
        }

        public ActionResult Edit(int id)
        {
            var loai = db.LoaiSPs.Find(id);
            if (loai == null)
                return HttpNotFound();

            ViewBag.MaDM = new SelectList(db.DanhMucs, "MaDM", "TenDM", loai.MaDM);
            return View(loai);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(LoaiSP loai)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(loai).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Cập nhật loại sản phẩm thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                }
            }

            ViewBag.MaDM = new SelectList(db.DanhMucs, "MaDM", "TenDM", loai.MaDM);
            return View(loai);
        }

        public ActionResult Delete(int id)
        {
            try
            {
                var loai = db.LoaiSPs.Find(id);
                if (loai != null)
                {
                    db.LoaiSPs.Remove(loai);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Xóa loại sản phẩm thành công!";
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
            {
                TempData["ErrorMessage"] = "Không thể xóa loại sản phẩm này vì còn có sản phẩm liên quan.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa: " + ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}