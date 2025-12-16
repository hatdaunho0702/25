using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication15.Models
{
    public class GioHang
    {
        DB_SkinFood1Entities DB = new DB_SkinFood1Entities();
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public string AnhBia { get; set; }
        // Use decimal to match SanPham.GiaBan (nullable decimal) and avoid precision issues
        public decimal GiaBan { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien => SoLuong * GiaBan;

        public GioHang(int maSP)
        {
            MaSP = maSP;

            // Use SingleOrDefault to avoid exception if product not found
            var sp = DB.SanPhams.SingleOrDefault(s => s.MaSP == maSP);
            if (sp != null)
            {
                TenSP = sp.TenSP ?? string.Empty;
                AnhBia = sp.HinhAnh ?? string.Empty;
                GiaBan = sp.GiaBan ?? 0m;
            }
            else
            {
                // Safe defaults if product not found
                TenSP = string.Empty;
                AnhBia = string.Empty;
                GiaBan = 0m;
            }

            SoLuong = 1;
        }
    }
}