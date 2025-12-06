using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication15.Areas.Admin.Data
{
    public class TaiKhoanCreateVM
    {
        public string TenDangNhap { get; set; }
        public string MatKhau { get; set; }
        public string NhapLaiMatKhau { get; set; }
        public string VaiTro { get; set; }

        // Thêm mới:
        public string HoTenNguoiDung { get; set; }
    }
}