using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication15.Models;

namespace WebApplication15.Controllers
{
    public class HomeController : Controller
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

        private IQueryable<SanPham> PublicProducts()
        {
            // Trả về các sản phẩm được phép hiển thị cho người dùng (không hiển thị Tạm ngưng / Ngừng kinh doanh)
            return data.SanPhams.Where(s => s.TrangThaiSanPham == null
                                             || (!s.TrangThaiSanPham.ToLower().Contains("tạm") && !s.TrangThaiSanPham.ToLower().Contains("ngừng")));
        }

        public ActionResult Index(int? maTH)
        {
            try
            {
                var spHot = PublicProducts()
                    .OrderByDescending(x => x.GiaBan)
                    .Take(10)
                    .ToList();

                // Thay đổi: lấy cả sản phẩm có GiamGia > 0 HOẶC được áp dụng trong các chương trình Khuyến mãi đang phát hành
                var now = DateTime.Now;

                // Lấy danh sách MaSP từ các khuyến mại đang hoạt động
                var activeKmProductIds = data.KhuyenMais
                    .Where(k => k.TrangThai == true
                                && (k.NgayBatDau == null || k.NgayBatDau <= now)
                                && (k.NgayKetThuc == null || k.NgayKetThuc >= now))
                    .SelectMany(k => k.SanPhams.Select(s => s.MaSP))
                    .Distinct()
                    .ToList();

                var spSale = PublicProducts()
                    .Where(x => (x.GiamGia ?? 0) > 0 || activeKmProductIds.Contains(x.MaSP))
                    .Take(10)
                    .ToList();

                var spNew = PublicProducts()
                    .OrderByDescending(x => x.MaSP)
                    .Take(10)
                    .ToList();

                var thuongHieuList = data.ThuongHieux.ToList();
                List<SanPham> spTheoTH;
                ThuongHieu thChon = null;

                if (maTH == null)
                {
                    spTheoTH = new List<SanPham>();
                }
                else
                {
                    spTheoTH = PublicProducts().Where(s => s.MaTH == maTH).ToList();
                    thChon = data.ThuongHieux.Find(maTH);
                }

                var viewModel = new HomeViewModel
                {
                    DsSanPham = PublicProducts()
                        .OrderByDescending(s => s.MaSP)
                        .Take(20)
                        .ToList(),
                    HotProducts = spHot,
                    SaleProducts = spSale,
                    NewProducts = spNew,
                    ThuongHieuList = thuongHieuList,
                    SanPhamTheoTH = spTheoTH,
                    ThuongHieuDangChon = thChon
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi trong Index: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                var emptyViewModel = new HomeViewModel
                {
                    DsSanPham = new List<SanPham>(),
                    HotProducts = new List<SanPham>(),
                    SaleProducts = new List<SanPham>(),
                    NewProducts = new List<SanPham>(),
                    ThuongHieuList = new List<ThuongHieu>(),
                    SanPhamTheoTH = new List<SanPham>(),
                    ThuongHieuDangChon = null
                };

                ViewBag.ErrorMessage = "Không thể kết nối cơ sở dữ liệu. Vui lòng kiểm tra lại kết nối.";
                return View(emptyViewModel);
            }
        }

        public ActionResult DanhMucSP()
        {
            var danhMucs = data.DanhMucs.ToList();
            return View(danhMucs);
        }

        public ActionResult LoaiSP()
        {
            var loai = data.LoaiSPs.ToList();
            return View(loai);
        }

        public ActionResult SanPhamTheoDanhMuc(int maDM)
        {
            var danhMuc = data.DanhMucs.ToList();
            var loaiSP = data.LoaiSPs.ToList();

            var sanPham = PublicProducts()
                .Where(sp => sp.MaDM == maDM)
                .ToList();

            var model = new HomeViewModel
            {
                DsSanPham = sanPham ?? new List<SanPham>(),
                DSDanhMuc = danhMuc,
                DsLoaiSP = loaiSP
            };

            ViewBag.TenDanhMuc = danhMuc.FirstOrDefault(dm => dm.MaDM == maDM)?.TenDM ?? "Không xác định";

            return View(model);
        }

        public ActionResult SanPhamTheoLoai(int id)
        {
            var sp = PublicProducts().Where(s => s.MaLoai == id).ToList();
            var loai = data.LoaiSPs.FirstOrDefault(l => l.MaLoai == id);

            ViewBag.TenLoai = loai?.TenLoai ?? "Không xác định";

            return View(sp);
        }

        [ChildActionOnly]
        public ActionResult MenuChinh()
        {
            try
            {
                var danhMucList = data.DanhMucs
                    .Where(dm => dm.MaDM >= 1 && dm.MaDM <= 6)
                    .Take(5)
                    .OrderBy(dm => dm.MaDM)
                    .ToList();

                return PartialView("_MenuChinh", danhMucList);
            }
            catch (Exception)
            {
                return PartialView("_MenuChinh", new List<DanhMuc>());
            }
        }

        [ChildActionOnly]
        public ActionResult MenuLoai(int maDM)
        {
            try
            {
                var loaiList = data.LoaiSPs
                    .Where(l => l.MaDM == maDM)
                    .ToList();

                return PartialView("_MenuLoai", loaiList);
            }
            catch (Exception)
            {
                return PartialView("_MenuLoai", new List<LoaiSP>());
            }
        }

        [ChildActionOnly]
        public ActionResult MenuDanhMuc()
        {
            try
            {
                var danhMucList = data.DanhMucs.ToList();
                return PartialView("_MenuDanhMuc", danhMucList);
            }
            catch (Exception)
            {
                return PartialView("_MenuDanhMuc", new List<DanhMuc>());
            }
        }

        public ActionResult ChiTietSP(int? maSP)
        {
            if (!maSP.HasValue)
            {
                // If no product id provided, redirect to product list to avoid parameter binding errors
                return RedirectToAction("Index");
            }

            var sanPham = data.SanPhams.FirstOrDefault(sp => sp.MaSP == maSP.Value);

            if (sanPham == null)
            {
                return HttpNotFound(); 
            }

            // Không cho người dùng xem các sản phẩm Tạm ngưng / Ngừng kinh doanh
            if (!string.IsNullOrEmpty(sanPham.TrangThaiSanPham))
            {
                var tt = sanPham.TrangThaiSanPham.Trim().ToLower();
                if (tt.Contains("tạm") || tt.Contains("ngừng"))
                {
                    return HttpNotFound();
                }
            }

            var danhGiaList = data.DanhGias
                .Where(dg => dg.MaSP == maSP.Value)
                .OrderByDescending(dg => dg.NgayDanhGia)
                .ToList();

            var spLienQuan = PublicProducts()
                .Where(s => s.MaLoai == sanPham.MaLoai && s.MaSP != maSP.Value)
                .Take(4)
                .ToList();

            ViewBag.DanhSachDanhGia = danhGiaList;
            ViewBag.SanPhamLienQuan = spLienQuan;
            ViewBag.TenDanhMuc = sanPham.DanhMuc?.TenDM ?? "Không xác định";
            ViewBag.TenThuongHieu = sanPham.ThuongHieu?.TenTH ?? "Không xác định";

            return View(sanPham);
        }

        public ActionResult TatCaSanPham(string searchString, decimal? minPrice, decimal? maxPrice, string sortOrder)
        {
            // 1. Lấy nguồn dữ liệu (chưa execute)
            var query = PublicProducts().AsQueryable();

            // 2. Tìm kiếm theo tên (nếu có)
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(x => x.TenSP.Contains(searchString));
            }

            // 3. LOGIC LỌC GIÁ MỚI: Tính toán giá sau giảm
            // Công thức: Giá Thực = GiaBan * (1 - GiamGia / 100)
            if (minPrice.HasValue)
            {
                query = query.Where(x => (x.GiaBan ?? 0) * (1 - (x.GiamGia ?? 0) / 100m) >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(x => (x.GiaBan ?? 0) * (1 - (x.GiamGia ?? 0) / 100m) <= maxPrice.Value);
            }

            // 4. Sắp xếp (Sort)
            ViewBag.CurrentSort = sortOrder; // Lưu lại để hiện lên View
            switch (sortOrder)
            {
                case "price_asc": // Giá tăng dần (theo giá thực tế)
                    query = query.OrderBy(x => (x.GiaBan ?? 0) * (1 - (x.GiamGia ?? 0) / 100m));
                    break;
                case "price_desc": // Giá giảm dần
                    query = query.OrderByDescending(x => (x.GiaBan ?? 0) * (1 - (x.GiamGia ?? 0) / 100m));
                    break;
                case "name_asc":
                    query = query.OrderBy(x => x.TenSP);
                    break;
                default: // Mặc định: Mới nhất lên đầu
                    query = query.OrderByDescending(x => x.MaSP);
                    break;
            }

            // 5. Lưu lại giá trị bộ lọc để hiển thị lại trên View
            ViewBag.SearchString = searchString;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            return View(query.ToList());
        }

        public ActionResult SanPhamHot(string searchString, decimal? minPrice, decimal? maxPrice, string sortOrder)
        {
            // 1. Lấy dữ liệu
            var query = PublicProducts().AsQueryable();

            // 2. Tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(x => x.TenSP.Contains(searchString));
            }

            // 3. Lọc theo GIÁ THỰC TẾ (Giá sau giảm)
            if (minPrice.HasValue)
            {
                query = query.Where(x => (x.GiaBan ?? 0) * (1 - (x.GiamGia ?? 0) / 100m) >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(x => (x.GiaBan ?? 0) * (1 - (x.GiamGia ?? 0) / 100m) <= maxPrice.Value);
            }

            // 4. Sắp xếp
            ViewBag.CurrentSort = sortOrder;
            switch (sortOrder)
            {
                case "price_asc":
                    query = query.OrderBy(x => (x.GiaBan ?? 0) * (1 - (x.GiamGia ?? 0) / 100m));
                    break;
                case "price_desc":
                    query = query.OrderByDescending(x => (x.GiaBan ?? 0) * (1 - (x.GiamGia ?? 0) / 100m));
                    break;
                case "name_asc":
                    query = query.OrderBy(x => x.TenSP);
                    break;
                default:
                    // Mặc định HOT: Ưu tiên giá trị cao (hoặc bạn có thể sort theo lượt xem nếu có)
                    query = query.OrderByDescending(x => x.GiaBan);
                    break;
            }

            // 5. Lưu ViewBag
            ViewBag.SearchString = searchString;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            return View(query.ToList());
        }

        public ActionResult SanPhamSale(string searchString, decimal? minPrice, decimal? maxPrice, string sortOrder)
        {
            var now = DateTime.Now;
            // Lấy danh sách MaSP từ các khuyến mại đang hoạt động (đã phát hành và trong khoảng thời gian)
            var activeKmProductIds = data.KhuyenMais
                .Where(k => k.TrangThai == true
                            && (k.NgayBatDau == null || k.NgayBatDau <= now)
                            && (k.NgayKetThuc == null || k.NgayKetThuc >= now))
                .SelectMany(k => k.SanPhams.Select(s => s.MaSP))
                .Distinct()
                .ToList();

            // 1. Chỉ lấy sản phẩm đang có giảm giá (> 0) hoặc nằm trong chương trình khuyến mãi đang phát hành
            var query = PublicProducts().Where(x => (x.GiamGia ?? 0) > 0 || activeKmProductIds.Contains(x.MaSP)).AsQueryable();

            // 2. Tìm kiếm theo tên (nếu có nhập)
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(x => x.TenSP.Contains(searchString));
            }

            // 3. Lọc theo GIÁ THỰC TẾ (Giá sau khi trừ % giảm)
            if (minPrice.HasValue)
            {
                query = query.Where(x => (x.GiaBan ?? 0) * (1 - (x.GiamGia ?? 0) / 100m) >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(x => (x.GiaBan ?? 0) * (1 - (x.GiamGia ?? 0) / 100m) <= maxPrice.Value);
            }

            // 4. Sắp xếp
            ViewBag.CurrentSort = sortOrder;
            switch (sortOrder)
            {
                case "price_asc": // Giá thấp đến cao
                    query = query.OrderBy(x => (x.GiaBan ?? 0) * (1 - (x.GiamGia ?? 0) / 100m));
                    break;
                case "price_desc": // Giá cao đến thấp
                    query = query.OrderByDescending(x => (x.GiaBan ?? 0) * (1 - (x.GiamGia ?? 0) / 100m));
                    break;
                case "name_asc": // Tên A-Z
                    query = query.OrderBy(x => x.TenSP);
                    break;
                default: // Mặc định: Giảm giá sâu nhất lên đầu (Ưu tiên sản phẩm sale mạnh)
                    query = query.OrderByDescending(x => x.GiamGia);
                    break;
            }

            // 5. Lưu lại dữ liệu lọc để hiện lại trên View
            ViewBag.SearchString = searchString;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            return View(query.ToList());
        }

        public ActionResult TimKiem(string keyword)
        {
            keyword = keyword?.Trim();

            if (string.IsNullOrEmpty(keyword))
                return View(new List<SanPham>());

            var ketQua = PublicProducts()
                .Where(sp =>
                    sp.TenSP.Contains(keyword) ||
                    sp.ThuongHieu.TenTH.Contains(keyword) ||
                    sp.LoaiSP.TenLoai.Contains(keyword)
                )
                .ToList();

            ViewBag.TuKhoa = keyword;

            return View(ketQua);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemDanhGia(int MaSP, int Diem, string NoiDung)
        {
            if (Session["User"] == null || Session["NguoiDung"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            var nd = (NguoiDung)Session["NguoiDung"];
            int maND = nd.MaND;

            DanhGia dg = new DanhGia
            {
                MaSP = MaSP,
                MaND = maND,
                Diem = Diem,
                NoiDung = NoiDung,
                NgayDanhGia = DateTime.Now
            };

            data.DanhGias.Add(dg);
            data.SaveChanges();

            return RedirectToAction("ChiTietSP", new { maSP = MaSP });
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}