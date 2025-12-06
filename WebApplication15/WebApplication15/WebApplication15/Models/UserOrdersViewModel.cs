using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication15.Models
{
    public class UserOrdersViewModel
    {
        public DonHang DonHang { get; set; }
        public List<ChiTietDonHang> ChiTiet { get; set; }
    }
}