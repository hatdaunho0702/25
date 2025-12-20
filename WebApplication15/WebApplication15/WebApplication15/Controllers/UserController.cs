using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication15.Models;
using System.Net;

namespace WebApplication15.Controllers
{
    public class UserController : Controller
    {
        private DB_SkinFood1Entities data = new DB_SkinFood1Entities();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                data?.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: User
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LoginSubmit(FormCollection collect)
        {
            if (ModelState.IsValid)
            {
                string email = collect["Email"];
                string pass = collect["MatKhau"];

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(pass))
                {
                    ViewBag.Error = "Vui lòng nhập đầy đủ thông tin đăng nhập!";
                    return View("Login");
                }

                TaiKhoan user = data.TaiKhoans
                    .FirstOrDefault(kh => kh.TenDangNhap == email && kh.MatKhauHash == pass);

                if (user == null)
                {
                    ViewBag.Error = "Thông tin đăng nhập không hợp lệ!";
                    return View("Login");
                }

                NguoiDung nd = data.NguoiDungs
                    .FirstOrDefault(n => n.MaND == user.MaND);

                // If avatar is stored in session previously (or from db if extended), keep it
                // Here we try to use stored avatar filename from user input if any
                // Note: Avatar is not persisted in DB by default; if you want to persist, add column.

                Session["User"] = user;
                if (nd != null)
                {
                    // Try to get persisted avatar from DB in case the EF model wasn't updated to include Avatar property
                    try
                    {
                        var avatarFromDb = data.Database.SqlQuery<string>("SELECT Avatar FROM NguoiDung WHERE MaND = @p0", nd.MaND).FirstOrDefault();
                        if (!string.IsNullOrEmpty(avatarFromDb))
                        {
                            nd.Avatar = avatarFromDb;
                        }
                    }
                    catch
                    {
                        // ignore SQL read errors
                    }

                    // If we have a SelectedAvatar in Session (just after registration), prefer it and persist to DB
                    var key = "SelectedAvatar_" + user.MaND;
                    if (Session[key] != null)
                    {
                        var sel = Session[key] as string;
                        if (!string.IsNullOrEmpty(sel) && sel != nd.Avatar)
                        {
                            nd.Avatar = sel;
                            try
                            {
                                data.Database.ExecuteSqlCommand("UPDATE NguoiDung SET Avatar = @p0 WHERE MaND = @p1", sel, nd.MaND);
                            }
                            catch { /* ignore DB save errors here */ }
                        }
                    }

                    // ensure session NguoiDung has avatar
                    Session["NguoiDung"] = nd;
                }

                Session["Role"] = user.VaiTro;

                if (user.VaiTro == "Admin")
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }

                return RedirectToAction("Index", "Home");
            }

            return View("Login");
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterSubmit(FormCollection form)
        {
            try
            {
                string hoten = form["HoTen"];
                string sdt = form["SoDienThoai"];
                string diachi = form["DiaChi"];
                string gioitinh = form["GioiTinh"];
                string username = form["TenDangNhap"];
                string password = form["MatKhau"];
                string confirmPassword = form["NhapLaiMatKhau"]; // add confirm
                string selectedAvatar = form["Avatar"]; // new field

                // Server-side check: passwords must match
                if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword) || password.Trim() != confirmPassword.Trim())
                {
                    ViewBag.Error = "Mật khẩu xác nhận không khớp. Vui lòng kiểm tra lại.";
                    return View("Register");
                }

                if (string.IsNullOrWhiteSpace(hoten) || string.IsNullOrWhiteSpace(username) || 
                    string.IsNullOrWhiteSpace(password))
                {
                    ViewBag.Error = "Vui lòng nhập đầy đủ thông tin bắt buộc!";
                    return View("Register");
                }

                var check = data.TaiKhoans.FirstOrDefault(t => t.TenDangNhap == username);
                if (check != null)
                {
                    ViewBag.Error = "Tên đăng nhập đã tồn tại!";
                    return View("Register");
                }

                DateTime ngaysinh = DateTime.Now;
                if (!string.IsNullOrEmpty(form["NgaySinh"]))
                {
                    DateTime.TryParse(form["NgaySinh"], out ngaysinh);
                }

                NguoiDung nd = new NguoiDung
                {
                    HoTen = hoten,
                    SoDienThoai = sdt,
                    DiaChi = diachi,
                    GioiTinh = gioitinh,
                    NgaySinh = ngaysinh,
                    NgayTao = DateTime.Now
                };

                data.NguoiDungs.Add(nd);
                data.SaveChanges();

                TaiKhoan tk = new TaiKhoan
                {
                    TenDangNhap = username,
                    MatKhauHash = password,
                    VaiTro = "KhachHang",
                    MaND = nd.MaND
                };

                data.TaiKhoans.Add(tk);
                data.SaveChanges();

                // Persist avatar into DB column directly in case EF model doesn't map it yet
                if (!string.IsNullOrEmpty(selectedAvatar))
                {
                    try
                    {
                        data.Database.ExecuteSqlCommand("UPDATE NguoiDung SET Avatar = @p0 WHERE MaND = @p1", selectedAvatar, nd.MaND);
                    }
                    catch
                    {
                        // ignore
                    }
                }

                // Save avatar selection linked with MaND so after login we can set it in session
                if (!string.IsNullOrEmpty(selectedAvatar))
                {
                    // store in Session (persists until session ends) and TempData (one-time) as fallback
                    Session["SelectedAvatar_" + nd.MaND] = selectedAvatar;
                    TempData["SelectedAvatar_" + nd.MaND] = selectedAvatar;
                }

                TempData["Success"] = "Đăng ký thành công! Bạn có thể đăng nhập.";
                return RedirectToAction("Login", "User");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi đăng ký: {ex.Message}");
                ViewBag.Error = "Đã xảy ra lỗi khi đăng ký. Vui lòng thử lại!";
                return View("Register");
            }
        }

        public ActionResult Profile()
        {
            if (Session["User"] == null)
                return RedirectToAction("Login", "User");

            TaiKhoan tk = Session["User"] as TaiKhoan;
            NguoiDung nd = Session["NguoiDung"] as NguoiDung;

            var model = new UserProfileViewModel
            {
                TaiKhoan = tk,
                NguoiDung = nd
            };

            return View(model);
        }

        // GET: allow logged-in user to edit their profile
        [HttpGet]
        public ActionResult EditProfile()
        {
            if (Session["User"] == null)
                return RedirectToAction("Login", "User");

            TaiKhoan tk = Session["User"] as TaiKhoan;
            var nd = data.NguoiDungs.FirstOrDefault(n => n.MaND == tk.MaND);
            if (nd == null)
                return HttpNotFound();

            return View(nd);
        }

        // POST: update user profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(NguoiDung model)
        {
            if (Session["User"] == null)
                return RedirectToAction("Login", "User");

            TaiKhoan tk = Session["User"] as TaiKhoan;
            if (model == null || model.MaND != tk.MaND)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (ModelState.IsValid)
            {
                var nd = data.NguoiDungs.Find(model.MaND);
                if (nd == null)
                    return HttpNotFound();

                nd.HoTen = model.HoTen;
                nd.SoDienThoai = model.SoDienThoai;
                nd.DiaChi = model.DiaChi;
                nd.GioiTinh = model.GioiTinh;
                nd.NgaySinh = model.NgaySinh;

                data.SaveChanges();

                // update session
                Session["NguoiDung"] = nd;

                TempData["Success"] = "Cập nhật thông tin thành công.";
                return RedirectToAction("Profile");
            }

            return View(model);
        }

        public ActionResult Account()
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            return RedirectToAction("Profile", "User");
        }

        public ActionResult Logout()
        {
            Session.Remove("User");
            Session.Remove("NguoiDung");
            Session.Remove("Role");
            Session.Remove("Cart");
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DonHang()
        {
            if (Session["User"] == null)
                return RedirectToAction("Login", "User");

            TaiKhoan tk = Session["User"] as TaiKhoan;

            var list = data.DonHangs
                .Where(d => d.MaND == tk.MaND)
                .OrderByDescending(d => d.NgayDat)
                .ToList();

            return View(list);
        }

        public ActionResult DonHangChiTiet(int id)
        {
            if (Session["User"] == null)
                return RedirectToAction("Login", "User");

            var dh = data.DonHangs.FirstOrDefault(d => d.MaDH == id);

            if (dh == null)
                return HttpNotFound();

            var ct = data.ChiTietDonHangs.Where(c => c.MaDH == id).ToList();

            var model = new UserOrdersViewModel
            {
                DonHang = dh,
                ChiTiet = ct
            };

            return View(model);
        }

        // Returns a redirect to the avatar image for the given user (maND).
        // Layout will use this action as the <img src="..."> so server decides path.
        public ActionResult Avatar(int maND)
        {
            try
            {
                // Check session selected avatar first
                var key = "SelectedAvatar_" + maND;
                if (Session[key] != null)
                {
                    var sel = Session[key] as string;
                    if (!string.IsNullOrEmpty(sel))
                        return Redirect(Url.Content("~/Content/avatars/" + sel));
                }

                // Otherwise read from DB
                var nd = data.NguoiDungs.FirstOrDefault(n => n.MaND == maND);
                if (nd != null && !string.IsNullOrEmpty(nd.Avatar))
                {
                    return Redirect(Url.Content("~/Content/avatars/" + nd.Avatar));
                }
            }
            catch { }

            // fallback default avatar
            return Redirect(Url.Content("~/Content/avatars/default.png"));
        }
    }
}