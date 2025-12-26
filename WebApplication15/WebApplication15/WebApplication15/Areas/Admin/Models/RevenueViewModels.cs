using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication15.Areas.Admin.Models
{
    public class ProductRevenueVM
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
    }

    public class CategoryRevenueVM
    {
        public int MaDM { get; set; }
        public string TenDM { get; set; }
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
    }
}
