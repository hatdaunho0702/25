using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication15.Models
{
    // Lightweight CartItem used by CalculateDiscount
    public class CartItem
    {
        public int MaSP { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien => DonGia * SoLuong;
    }
}
