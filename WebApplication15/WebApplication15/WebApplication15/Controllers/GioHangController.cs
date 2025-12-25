using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication15.Models;
using WebApplication15.Services;
using System.Web.Helpers;

namespace WebApplication15.Controllers
{
    public class GioHangController : Controller
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

        // GET: GioHang
        public ActionResult Index()
        {
            Cart cart = (Cart)Session["Cart"];
            if (cart == null)
                cart = new Cart();

            return View(cart);
        }

        // Thêm sản phẩm vào giỏ
        public ActionResult AddToCart(int id)
        {
            if (Session["User"] == null) // Bắt buộc đăng nhập
            {
                return RedirectToAction("Login", "User");
            }

            Cart cart = (Cart)Session["Cart"];
            if (cart == null)
                cart = new Cart();

            int result = cart.Them(id);
            if (result == 1)
            {
                Session["Cart"] = cart;
            }

            return RedirectToAction("Index", "Home");
        }

        // Thêm sản phẩm vào giỏ từ các trang danh sách (giữ nguyên trang hiện tại)
        public ActionResult ThemGioHang(int maSP, string url = null)
        {
            if (Session["User"] == null)
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang Login
                return RedirectToAction("Login", "User");
            }

            Cart cart = (Cart)Session["Cart"];
            if (cart == null)
                cart = new Cart();

            int result = cart.Them(maSP);
            if (result == 1)
            {
                Session["Cart"] = cart;
            }

            // Nếu url truyền vào là local, redirect về url đó để reload trang
            if (!string.IsNullOrEmpty(url) && Url.IsLocalUrl(url))
            {
                return Redirect(url);
            }

            // Nếu không có url hoặc không an toàn, dùng UrlReferrer hoặc fallback về trang hiện tại
            var refUrl = Request.UrlReferrer?.ToString();
            if (!string.IsNullOrEmpty(refUrl) && Url.IsLocalUrl(refUrl))
            {
                return Redirect(refUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // Xóa sản phẩm khỏi giỏ
        public ActionResult RemoveFromCart(int id)
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            Cart cart = (Cart)Session["Cart"];
            if (cart == null)
                cart = new Cart();

            int result = cart.Xoa(id);
            if (result == 1)
            {
                Session["Cart"] = cart;

                try
                {
                    // Try to cancel any unpaid order belonging to the current logged-in user
                    var sessionUser = Session["User"] as TaiKhoan;
                    if (sessionUser != null)
                    {
                        // Find most recent unpaid order for this user
                        var donHang = data.DonHangs
                            .Where(d => d.MaND == sessionUser.MaND && (d.TrangThaiThanhToan == null || d.TrangThaiThanhToan == "Chưa thanh toán" || d.TrangThaiThanhToan == "Pending"))
                            .OrderByDescending(d => d.NgayDat)
                            .FirstOrDefault();

                        if (donHang != null)
                        {
                            donHang.TrangThaiThanhToan = "Đã hủy";
                            donHang.GhiChu = (donHang.GhiChu ?? "") + "\nHủy do khách xóa sản phẩm khỏi giỏ.";
                            data.Entry(donHang).State = System.Data.Entity.EntityState.Modified;

                            // Remove any saved details to avoid stock triggers
                            var details = data.ChiTietDonHangs.Where(ct => ct.MaDH == donHang.MaDH).ToList();
                            if (details.Any())
                            {
                                data.ChiTietDonHangs.RemoveRange(details);
                            }

                            data.SaveChanges();
                        }
                    }
                    else if (Session["CurrentOrder"] != null)
                    {
                        // Fallback: previous behavior when user not in session (keep for compatibility)
                        int maDH = (int)Session["CurrentOrder"];
                        var donHang = data.DonHangs.FirstOrDefault(d => d.MaDH == maDH);
                        if (donHang != null)
                        {
                            var status = donHang.TrangThaiThanhToan ?? "Chưa thanh toán";
                            if (status != "Đã thanh toán" && status != "Paid")
                            {
                                donHang.TrangThaiThanhToan = "Đã hủy";
                                donHang.GhiChu = (donHang.GhiChu ?? "") + "\nHủy do khách xóa sản phẩm khỏi giỏ.";
                                data.Entry(donHang).State = System.Data.Entity.EntityState.Modified;

                                var details = data.ChiTietDonHangs.Where(ct => ct.MaDH == maDH).ToList();
                                if (details.Any()) data.ChiTietDonHangs.RemoveRange(details);

                                data.SaveChanges();
                            }
                        }

                        Session["CurrentOrder"] = null;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Lỗi khi cập nhật trạng thái đơn hàng khi xóa sản phẩm: " + ex.Message);
                }
            }

            return RedirectToAction("Index", "GioHang");
        }

        // Cập nhật số lượng
        public ActionResult UpdateSLCart(int id, int num)
        {
            int result = -1;
            Cart cart = (Cart)Session["Cart"];

            if (cart == null)
                cart = new Cart();

            if (num == -1)
                result = cart.Giam(id);
            else
                result = cart.Them(id);

            if (result == 1)
                Session["Cart"] = cart;

            return RedirectToAction("Index", "GioHang");
        }

        // New: Cập nhật số lượng theo giá trị nhập vào
        [HttpPost]
        public ActionResult UpdateQuantity(int id, int quantity)
        {
            Cart cart = (Cart)Session["Cart"];
            if (cart == null)
                cart = new Cart();

            var item = cart.list.FirstOrDefault(x => x.MaSP == id);
            if (item == null)
            {
                // If item not found, nothing to update
                return RedirectToAction("Index", "GioHang");
            }

            if (quantity <= 0)
            {
                cart.Xoa(id);
            }
            else
            {
                item.SoLuong = quantity;
            }

            Session["Cart"] = cart;
            return RedirectToAction("Index", "GioHang");
        }


        public ActionResult PaymentReview()
        {
            Cart cart = (Cart)Session["Cart"];

            if (cart == null || cart.list.Count == 0)
                return RedirectToAction("Index", "GioHang");

            return View(cart);
        }

        // Xác nhận thanh toán (Lưu hóa đơn & chi tiết)
        public ActionResult PaymentConfirm()
        {
            var kh = (TaiKhoan)Session["User"];
            Cart cart = (Cart)Session["Cart"];

            if (kh == null)
                return RedirectToAction("Login", "User");

            if (cart == null || cart.list.Count == 0)
                return RedirectToAction("Index", "GioHang");

            // --- KIỂM TRA TỒN KHO TRƯỚC KHI TẠO HÓA ĐƠN ---
            foreach (var item in cart.list)
            {
                var sp = data.SanPhams.Find(item.MaSP);
                int available = sp?.SoLuongTon ?? 0;
                if (available < item.SoLuong)
                {
                    TempData["Error"] = $"Sản phẩm '{item.TenSP}' chỉ còn {available} nhưng bạn yêu cầu {item.SoLuong}. Vui lòng điều chỉnh giỏ hàng.";
                    return RedirectToAction("PaymentReview");
                }
            }

            // TẠO HÓA ĐƠN (CHỈ LƯU HEADER) - KHÔNG LƯU CHI TIẾT NGAY
            var hoaDon = new DonHang
            {
                MaND = kh.MaND,
                NgayDat = DateTime.Now,
                TongTien = (decimal)cart.TongThanhTien(),
                DiaChiGiaoHang = kh.NguoiDung?.DiaChi ?? "",
                TrangThaiThanhToan = "Chưa thanh toán"
            };

            data.DonHangs.Add(hoaDon);
            data.SaveChanges();

            // Lưu mã đơn để dùng ở bước thanh toán
            Session["CurrentOrder"] = hoaDon.MaDH;

            // CHUYỂN SANG TRANG CHỌN PHƯƠNG THỨC THANH TOÁN
            return RedirectToAction("PaymentMethod");
        }

        public ActionResult PaymentMethod()
        {
            try
            {
                if (Session["CurrentOrder"] == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ PaymentMethod: Session['CurrentOrder'] là null");
                    return RedirectToAction("Index", "GioHang");
                }

                int maDH = (int)Session["CurrentOrder"];
                var hoaDon = data.DonHangs.FirstOrDefault(x => x.MaDH == maDH);

                if (hoaDon == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ PaymentMethod: Không tìm thấy đơn hàng MaDH={maDH}");
                    return RedirectToAction("Index", "GioHang");
                }

                return View(hoaDon);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ PaymentMethod Error: {ex.Message}");
                return RedirectToAction("Index", "GioHang");
            }
        }

        // GET: Hiển thị form thông tin nhận hàng
        public ActionResult LuuThongTinNhanHang(int? maDH)
        {
            try
            {
                if (!maDH.HasValue)
                {
                    if (Session["CurrentOrder"] == null)
                    {
                        System.Diagnostics.Debug.WriteLine("❌ LuuThongTinNhanHang: maDH không có và Session['CurrentOrder'] là null");
                        return RedirectToAction("Index", "GioHang");
                    }
                    maDH = (int)Session["CurrentOrder"];
                }

                var hoaDon = data.DonHangs.FirstOrDefault(x => x.MaDH == maDH);

                if (hoaDon == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ LuuThongTinNhanHang: Không tìm thấy đơn hàng MaDH={maDH}");
                    return RedirectToAction("Index", "GioHang");
                }

                return View(hoaDon);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ LuuThongTinNhanHang Error: {ex.Message}");
                return RedirectToAction("Index", "GioHang");
            }
        }

        // POST: Lưu thông tin nhận hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LuuThongTinNhanHang(int MaDH, string TenNguoiNhan, string SoDienThoai, string DiaChiGiaoHang, string GhiChu)
        {
            try
            {
                var hoaDon = data.DonHangs.FirstOrDefault(x => x.MaDH == MaDH);

                if (hoaDon == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ LuuThongTinNhanHang POST: Không tìm thấy đơn hàng MaDH={MaDH}");
                    return RedirectToAction("Index", "GioHang");
                }

                hoaDon.TenNguoiNhan = TenNguoiNhan;
                hoaDon.SoDienThoai = SoDienThoai;
                hoaDon.DiaChiGiaoHang = DiaChiGiaoHang;
                hoaDon.GhiChu = GhiChu;

                data.Entry(hoaDon).State = System.Data.Entity.EntityState.Modified;
                data.SaveChanges();

                System.Diagnostics.Debug.WriteLine($"✅ LuuThongTinNhanHang: Đã lưu thông tin cho MaDH={MaDH}");

                return RedirectToAction("PaymentMethod", new { maDH = MaDH });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ LuuThongTinNhanHang POST Error: {ex.Message}");
                return RedirectToAction("Index", "GioHang");
            }
        }

        public ActionResult ThanhToanCOD(int? maDH)
        {
            try
            {
                if (!maDH.HasValue)
                {
                    if (Session["CurrentOrder"] == null)
                    {
                        System.Diagnostics.Debug.WriteLine("❌ ThanhToanCOD: maDH không có và Session['CurrentOrder'] là null");
                        return RedirectToAction("Index", "GioHang");
                    }
                    maDH = (int)Session["CurrentOrder"];
                }

                var hoaDon = data.DonHangs.FirstOrDefault(x => x.MaDH == maDH);

                if (hoaDon == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ ThanhToanCOD: Không tìm thấy đơn hàng MaDH={maDH}");
                    return RedirectToAction("Index", "GioHang");
                }

                // Nếu chi tiết đơn hàng chưa được lưu (tức là chưa trừ kho), thì lưu chi tiết giờ để trigger DB trừ kho
                var existingDetails = data.ChiTietDonHangs.Any(ct => ct.MaDH == hoaDon.MaDH);
                if (!existingDetails)
                {
                    Cart cart = (Cart)Session["Cart"];
                    if (cart != null)
                    {
                        // Kiểm tra tồn kho trước khi lưu chi tiết
                        foreach (var item in cart.list)
                        {
                            var sp = data.SanPhams.Find(item.MaSP);
                            int available = sp?.SoLuongTon ?? 0;
                            if (available < item.SoLuong)
                            {
                                TempData["Error"] = $"Sản phẩm '{item.TenSP}' chỉ còn {available} nhưng bạn yêu cầu {item.SoLuong}. Vui lòng điều chỉnh giỏ hàng.";
                                return RedirectToAction("PaymentReview");
                            }
                        }

                        foreach (var item in cart.list)
                        {
                            data.ChiTietDonHangs.Add(new ChiTietDonHang
                            {
                                MaDH = hoaDon.MaDH,
                                MaSP = item.MaSP,
                                SoLuong = item.SoLuong,
                                DonGia = (decimal)item.GiaBan
                            });
                        }
                        // Save here to let DB trigger update stock
                        data.SaveChanges();
                    }
                }

                hoaDon.TrangThaiThanhToan = "Đã thanh toán";
                hoaDon.NgayThanhToan = DateTime.Now;
                hoaDon.PhuongThucThanhToan = "COD";
                data.SaveChanges();

                Session["Cart"] = null;
                Session["CurrentOrder"] = null;

                System.Diagnostics.Debug.WriteLine($"✅ ThanhToanCOD: Đã cập nhật trạng thái COD cho MaDH={maDH}");

                return RedirectToAction("PaymentSuccess", new { maDH = maDH });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ThanhToanCOD Error: {ex.Message}");
                return RedirectToAction("Index", "GioHang");
            }
        }

        public ActionResult ThanhToanChuyenKhoan(int? maDH)
        {
            try
            {
                if (!maDH.HasValue)
                {
                    if (Session["CurrentOrder"] == null)
                    {
                        System.Diagnostics.Debug.WriteLine("❌ ThanhToanChuyenKhoan: maDH không có và Session['CurrentOrder'] là null");
                        return RedirectToAction("Index", "GioHang");
                    }
                    maDH = (int)Session["CurrentOrder"];
                }

                var hoaDon = data.DonHangs.FirstOrDefault(x => x.MaDH == maDH);

                if (hoaDon == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ ThanhToanChuyenKhoan: Không tìm thấy đơn hàng MaDH={maDH}");
                    return RedirectToAction("Index", "GioHang");
                }

                hoaDon.PhuongThucThanhToan = "Chuyển Khoản";
                data.SaveChanges();

                System.Diagnostics.Debug.WriteLine($"✅ ThanhToanChuyenKhoan: Đã cập nhật phương thức Chuyển Khoản cho MaDH={maDH}");

                return View(hoaDon);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ThanhToanChuyenKhoan Error: {ex.Message}");
                return RedirectToAction("Index", "GioHang");
            }
        }

        public ActionResult ThanhToanQR(int? maDH)
        {
            try
            {
                if (!maDH.HasValue)
                {
                    if (Session["CurrentOrder"] == null)
                    {
                        System.Diagnostics.Debug.WriteLine("❌ ThanhToanQR: maDH không có và Session['CurrentOrder'] là null");
                        return RedirectToAction("Index", "GioHang");
                    }
                    maDH = (int)Session["CurrentOrder"];
                }

                var hoaDon = data.DonHangs.FirstOrDefault(x => x.MaDH == maDH);

                if (hoaDon == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ ThanhToanQR: Không tìm thấy đơn hàng MaDH={maDH}");
                    return RedirectToAction("Index", "GioHang");
                }

                hoaDon.PhuongThucThanhToan = "QR";
                data.SaveChanges();

                System.Diagnostics.Debug.WriteLine($"✅ ThanhToanQR: Đã cập nhật phương thức QR cho MaDH={maDH}");

                return View(hoaDon);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ThanhToanQR Error: {ex.Message}");
                return RedirectToAction("Index", "GioHang");
            }
        }

        // Action để xác nhận thanh toán hoàn tất (cho QR/Chuyển khoản)
        public ActionResult ConfirmPaymentComplete(int? maDH)
        {
            try
            {
                if (!maDH.HasValue)
                {
                    if (Session["CurrentOrder"] == null)
                        return RedirectToAction("Index", "GioHang");
                    maDH = (int)Session["CurrentOrder"];
                }

                var hoaDon = data.DonHangs.FirstOrDefault(x => x.MaDH == maDH);

                if (hoaDon != null)
                {
                    // Nếu chi tiết đơn hàng chưa được lưu thì lưu bây giờ (trigger DB sẽ trừ kho)
                    var existingDetails = data.ChiTietDonHangs.Any(ct => ct.MaDH == hoaDon.MaDH);
                    if (!existingDetails)
                    {
                        Cart cart = (Cart)Session["Cart"];
                        if (cart != null)
                        {
                            // Kiểm tra tồn kho trước khi lưu chi tiết
                            foreach (var item in cart.list)
                            {
                                var sp = data.SanPhams.Find(item.MaSP);
                                int available = sp?.SoLuongTon ?? 0;
                                if (available < item.SoLuong)
                                {
                                    TempData["Error"] = $"Sản phẩm '{item.TenSP}' chỉ còn {available} nhưng bạn yêu cầu {item.SoLuong}. Vui lòng điều chỉnh giỏ hàng.";
                                    return RedirectToAction("PaymentReview");
                                }
                            }

                            foreach (var item in cart.list)
                            {
                                data.ChiTietDonHangs.Add(new ChiTietDonHang
                                {
                                    MaDH = hoaDon.MaDH,
                                    MaSP = item.MaSP,
                                    SoLuong = item.SoLuong,
                                    DonGia = (decimal)item.GiaBan
                                });
                            }

                            data.SaveChanges(); // trigger sẽ trừ kho
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"⚠️ ConfirmPaymentComplete: Session Cart null when inserting details for MaDH={maDH}");
                        }
                    }

                    hoaDon.TrangThaiThanhToan = "Đã thanh toán";
                    hoaDon.NgayThanhToan = DateTime.Now;

                    data.Entry(hoaDon).State = System.Data.Entity.EntityState.Modified;

                    int result = data.SaveChanges();

                    System.Diagnostics.Debug.WriteLine($"✅ ConfirmPaymentComplete: Đã cập nhật maDH={maDH}, SaveChanges result={result}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ ConfirmPaymentComplete: Không tìm thấy đơn hàng maDH={maDH}");
                }

                Session["Cart"] = null;
                Session["CurrentOrder"] = null;
                return RedirectToAction("PaymentSuccess", new { maDH = maDH });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ConfirmPaymentComplete Error: {ex.Message}");
                throw;
            }
        }

        public ActionResult PaymentSuccess(int? maDH)
        {
            if (!maDH.HasValue && Session["CurrentOrder"] != null)
            {
                maDH = (int)Session["CurrentOrder"];
            }

            if (maDH.HasValue)
            {
                ViewBag.MaDH = maDH.Value;

                var hoaDon = data.DonHangs.FirstOrDefault(x => x.MaDH == maDH);

                if (hoaDon != null)
                {
                    data.Entry(hoaDon).Reload();

                    if (string.IsNullOrEmpty(hoaDon.TrangThaiThanhToan))
                    {
                        hoaDon.TrangThaiThanhToan = "Đã thanh toán";
                        hoaDon.NgayThanhToan = DateTime.Now;
                        data.SaveChanges();
                        System.Diagnostics.Debug.WriteLine($"✅ PaymentSuccess: Cập nhật trạng thái cho maDH={maDH}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"✅ PaymentSuccess: maDH={maDH}, TrangThaiThanhToan={hoaDon.TrangThaiThanhToan}");
                    }

                    ViewBag.TrangThaiThanhToan = hoaDon.TrangThaiThanhToan;
                    ViewBag.PhuongThucThanhToan = hoaDon.PhuongThucThanhToan;
                }
            }

            Session["Cart"] = null;
            Session["CurrentOrder"] = null;

            return View();
        }

        public ActionResult DebugPayment(int maDH)
        {
            try
            {
                var hoaDon = data.DonHangs.FirstOrDefault(x => x.MaDH == maDH);

                if (hoaDon == null)
                {
                    return Content($"❌ Không tìm thấy đơn hàng MaDH={maDH}");
                }

                string info = $@"
🔍 DEBUG INFO - MaDH: {maDH}
================================================
✅ Tìm thấy đơn hàng
- MaDH: {hoaDon.MaDH}
- MaND: {hoaDon.MaND}
- TongTien: {hoaDon.TongTien}
- NgayDat: {hoaDon.NgayDat}
- TrangThaiThanhToan: '{hoaDon.TrangThaiThanhToan}' (null={hoaDon.TrangThaiThanhToan == null})
- NgayThanhToan: {hoaDon.NgayThanhToan}
- PhuongThucThanhToan: {hoaDon.PhuongThucThanhToan}
- DiaChiGiaoHang: {hoaDon.DiaChiGiaoHang}
";

                return Content(info, "text/plain; charset=utf-8");
            }
            catch (Exception ex)
            {
                return Content($"❌ Error: {ex.Message}\n{ex.InnerException?.Message}", "text/plain; charset=utf-8");
            }
        }

        // POST: ApplyDiscount
        [HttpPost]
        // NOTE: We validate antiforgery token manually to return JSON error messages for AJAX calls
        public ActionResult ApplyDiscount(string code)
        {
            try
            {
                // Manual anti-forgery validation so we can return JSON instead of letting MVC throw a 500/400
                try
                {
                    // AntiForgery.Validate() will read token from form or header/cookie pair
                    // We keep this simple: call Validate and catch any exception
                    AntiForgery.Validate();
                }
                catch (Exception afEx)
                {
                    System.Diagnostics.Debug.WriteLine("AntiForgery validation failed: " + afEx.Message);
                    return Json(new { success = false, message = "Token bảo mật không hợp lệ. Vui lòng tải lại trang và thử lại." });
                }

                // 1. Kiểm tra mã đầu vào
                if (string.IsNullOrWhiteSpace(code))
                    return Json(new { success = false, message = "Vui lòng nhập mã giảm giá." });

                // 2. Lấy giỏ hàng an toàn
                var cartObj = Session["Cart"] as Cart;
                if (cartObj == null || cartObj.list == null || !cartObj.list.Any())
                    return Json(new { success = false, message = "Giỏ hàng đang trống!" });

                // 3. Chuyển đổi dữ liệu an toàn (Dùng Convert.ToDecimal để tránh lỗi 500 do sai kiểu)
                var items = new List<CartItem>();
                foreach (var x in cartObj.list)
                {
                    items.Add(new CartItem
                    {
                        MaSP = x.MaSP,
                        SoLuong = x.SoLuong,
                        DonGia = x.GiaBan != null ? Convert.ToDecimal(x.GiaBan) : 0m
                    });
                }

                // 4. Tính tổng tiền tạm tính
                decimal tamTinh = items.Sum(x => x.DonGia * x.SoLuong);

                // 5. Gọi Service xử lý
                var couponService = new CouponService(data);
                decimal soTienGiam = 0m;

                try
                {
                    soTienGiam = couponService.CalculateDiscount(code.Trim(), items, tamTinh);
                }
                catch (Exception serviceEx)
                {
                    return Json(new { success = false, message = serviceEx.Message });
                }

                if (soTienGiam <= 0)
                {
                    return Json(new { success = false, message = "Mã này không áp dụng được cho đơn hàng của bạn." });
                }

                Session["DiscountCode"] = code.Trim();
                Session["DiscountAmount"] = soTienGiam;

                decimal tongCongMoi = tamTinh - soTienGiam;

                return Json(new
                {
                    success = true,
                    discountAmount = soTienGiam.ToString("N0") + "₫",
                    newTotal = tongCongMoi.ToString("N0") + "₫",
                    newSubTotal = tamTinh.ToString("N0") + "₫",
                    message = "Áp dụng mã thành công!"
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CRITICAL ERROR ApplyDiscount: " + ex.ToString());
                return Json(new { success = false, message = "Lỗi xử lý: " + ex.Message });
            }
        }

        // POST: ApplyDiscountAjax (no anti-forgery) - temporary fallback for AJAX testing
        [HttpPost]
        public ActionResult ApplyDiscountAjax(string code)
        {
            try
            {
                // 1. Kiểm tra mã đầu vào
                if (string.IsNullOrWhiteSpace(code))
                    return Json(new { success = false, message = "Vui lòng nhập mã giảm giá." });

                // 2. Lấy giỏ hàng an toàn
                var cartObj = Session["Cart"] as Cart;
                if (cartObj == null || cartObj.list == null || !cartObj.list.Any())
                    return Json(new { success = false, message = "Giỏ hàng đang trống!" });

                // 3. Chuyển đổi dữ liệu an toàn
                var items = new List<CartItem>();
                foreach (var x in cartObj.list)
                {
                    items.Add(new CartItem
                    {
                        MaSP = x.MaSP,
                        SoLuong = x.SoLuong,
                        DonGia = x.GiaBan != null ? Convert.ToDecimal(x.GiaBan) : 0m
                    });
                }

                // 4. Tính tổng tiền tạm tính
                decimal tamTinh = items.Sum(x => x.DonGia * x.SoLuong);

                // 5. Gọi Service xử lý
                var couponService = new CouponService(data);
                decimal soTienGiam = 0m;

                try
                {
                    soTienGiam = couponService.CalculateDiscount(code.Trim(), items, tamTinh);
                }
                catch (Exception serviceEx)
                {
                    return Json(new { success = false, message = serviceEx.Message });
                }

                if (soTienGiam <= 0)
                {
                    return Json(new { success = false, message = "Mã này không áp dụng được cho đơn hàng của bạn." });
                }

                Session["DiscountCode"] = code.Trim();
                Session["DiscountAmount"] = soTienGiam;

                decimal tongCongMoi = tamTinh - soTienGiam;

                return Json(new
                {
                    success = true,
                    discountAmount = soTienGiam.ToString("N0") + "₫",
                    newTotal = tongCongMoi.ToString("N0") + "₫",
                    newSubTotal = tamTinh.ToString("N0") + "₫",
                    message = "Áp dụng mã thành công!"
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CRITICAL ERROR ApplyDiscountAjax: " + ex.ToString());
                return Json(new { success = false, message = "Lỗi xử lý: " + ex.Message });
            }
        }
    }
}