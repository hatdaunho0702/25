using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication15.Models
{
    public class HomeViewModel
    {
        public List<SanPham> DsSanPham { get; set; }
        public List<DanhMuc> DSDanhMuc { get; set; }
        public List<LoaiSP> DsLoaiSP { get; set; }
        public List<SanPham> HotProducts { get; set; }
        public List<SanPham> SaleProducts { get; set; }
        public List<SanPham> NewProducts { get; set; }
        public List<ThuongHieu> ThuongHieuList { get; set; }
        public List<SanPham> SanPhamTheoTH { get; set; }
        public ThuongHieu ThuongHieuDangChon { get; set; }
    }
}