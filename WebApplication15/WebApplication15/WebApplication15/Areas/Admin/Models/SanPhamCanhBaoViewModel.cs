using System;

namespace WebApplication15.Areas.Admin.Models
{
    public class SanPhamCanhBaoViewModel
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public DateTime HanSuDung { get; set; }
        public int? SoLuongTon { get; set; }
    }
}
