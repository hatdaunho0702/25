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
    public class ThuongHieuController : Controller
    {
        // GET: Admin/ThuongHieu
        DB_SkinFood1Entities db = new DB_SkinFood1Entities();

        public ActionResult Index() => View(db.ThuongHieux.ToList());

        public ActionResult Create() => View();

        [HttpPost]
        public ActionResult Create(ThuongHieu th)
        {
            db.ThuongHieux.Add(th);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id) => View(db.ThuongHieux.Find(id));

        [HttpPost]
        public ActionResult Edit(ThuongHieu th)
        {
            db.Entry(th).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            try
            {
                var th = db.ThuongHieux.Find(id);
                if (th != null)
                {
                    db.ThuongHieux.Remove(th);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
            {
                TempData["ErrorMessage"] = "Không thể xóa thương hiệu này vì còn có sản phẩm liên quan.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa thương hiệu: " + ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}