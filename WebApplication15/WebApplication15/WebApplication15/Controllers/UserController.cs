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
        DB_SkinFood1Entities data = new DB_SkinFood1Entities();
        // GET: User
        public ActionResult Login()
        {
            var session = Session["User"];
            return View();
        }
        [HttpPost]
        public ActionResult LoginSubmit(FormCollection collect)
        {
            if (ModelState.IsValid)
            {
                string email = collect["Email"];
                string pass = collect["MatKhau"];

                TaiKhoan user = data.TaiKhoans
                    .FirstOrDefault(kh => kh.TenDangNhap == email
                                       && kh.MatKhauHash == pass);

                // Nếu không tìm thấy tài khoản → báo lỗi và return View
                if (user == null)
                {
                    ViewBag.Error = "Thông tin đăng nhập không hợp lệ!";
                    return View("Login");
                }

                // Tìm NguoiDung chỉ khi user != null
                NguoiDung nd = data.NguoiDungs
                    .FirstOrDefault(n => n.MaND == user.MaND);

                // Lưu session
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
        public ActionResult RegisterSubmit(FormCollection form)
        {
            string hoten = form["HoTen"];
            string sdt = form["SoDienThoai"];
            string diachi = form["DiaChi"];
            string gioitinh = form["GioiTinh"];
            DateTime ngaysinh = DateTime.Parse(form["NgaySinh"]);

            string username = form["TenDangNhap"];
            string password = form["MatKhau"];

            // 1. Kiểm tra tài khoản có tồn tại chưa
            var check = data.TaiKhoans.FirstOrDefault(t => t.TenDangNhap == username);
            if (check != null)
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại!";
                return View("Register");
            }

            // 2. Tạo bản ghi NguoiDung
            NguoiDung nd = new NguoiDung();
            nd.HoTen = hoten;
            nd.SoDienThoai = sdt;
            nd.DiaChi = diachi;
            nd.GioiTinh = gioitinh;
            nd.NgaySinh = ngaysinh;
            nd.NgayTao = DateTime.Now;

            data.NguoiDungs.Add(nd);
            data.SaveChanges();    // sau save, MaND tự sinh

            // 3. Tạo bản ghi TaiKhoan liên kết NguoiDung
            TaiKhoan tk = new TaiKhoan();
            tk.TenDangNhap = username;
            tk.MatKhauHash = password;       // nếu cần hash thì bảo mình
            tk.VaiTro = "KhachHang";         // mặc định
            tk.MaND = nd.MaND;               // khóa ngoại

            data.TaiKhoans.Add(tk);
            data.SaveChanges();

            // 4. Chuyển hướng về đăng nhập
            TempData["Success"] = "Đăng ký thành công! Bạn có thể đăng nhập.";
            return RedirectToAction("Login", "User");

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

            // Lấy danh sách đơn hàng của user
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