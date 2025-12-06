using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication15.Models;

namespace WebApplication15.Areas.Admin.Controllers
{
    public class LienHesController : Controller
    {
        private DB_SkinFood1Entities db = new DB_SkinFood1Entities();

        // GET: Admin/LienHes
        public ActionResult Index(string q)
        {
            var list = db.LienHes.AsQueryable();
            if (!String.IsNullOrEmpty(q))
            {
                list = list.Where(x => x.HoTen.Contains(q) || x.Email.Contains(q) || x.NoiDung.Contains(q));
            }
            return View(list.OrderByDescending(x => x.NgayGui).ToList());
        }

        // GET: Admin/LienHes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LienHe lienHe = db.LienHes.Find(id);
            if (lienHe == null)
            {
                return HttpNotFound();
            }
            return View(lienHe);
        }

        // GET: Admin/LienHes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/LienHes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MaLH,HoTen,Email,SoDienThoai,NoiDung,NgayGui")] LienHe lienHe)
        {
            if (ModelState.IsValid)
            {
                db.LienHes.Add(lienHe);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(lienHe);
        }

        // GET: Admin/LienHes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LienHe lienHe = db.LienHes.Find(id);
            if (lienHe == null)
            {
                return HttpNotFound();
            }
            return View(lienHe);
        }

        // POST: Admin/LienHes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MaLH,HoTen,Email,SoDienThoai,NoiDung,NgayGui")] LienHe lienHe)
        {
            if (ModelState.IsValid)
            {
                db.Entry(lienHe).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(lienHe);
        }

            // GET: Admin/LienHes/Delete/5
            public ActionResult Delete(int? id)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                LienHe lienHe = db.LienHes.Find(id);
                if (lienHe == null)
                {
                    return HttpNotFound();
                }
                return View(lienHe);
            }

            [HttpPost]

            public ActionResult DeleteSelected(int[] idsToDelete)
            {
                if (idsToDelete != null && idsToDelete.Length > 0)
                {
                    foreach (var id in idsToDelete)
                    {
                        var item = db.LienHes.Find(id);
                        if (item != null)
                            db.LienHes.Remove(item);
                    }
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }

            // POST: Admin/LienHes/Delete/5
            [HttpPost, ActionName("Delete")]

        public ActionResult DeleteConfirmed(int id)
            {
                LienHe lienHe = db.LienHes.Find(id);
                db.LienHes.Remove(lienHe);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
