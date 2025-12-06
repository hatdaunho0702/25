using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication15.Models;

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

                Session["User"] = user;
                Session["NguoiDung"] = nd;
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
    }
}