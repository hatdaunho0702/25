using System;
using System.Collections.Generic;
using System.Linq;
using WebApplication15.Models;
using System.Data.Entity;

namespace WebApplication15.Services
{
    public class CouponService
    {
        private readonly DB_SkinFood1Entities _db;

        public CouponService(DB_SkinFood1Entities db)
        {
            _db = db ?? new DB_SkinFood1Entities();
        }

        // CalculateDiscount as requested: accepts couponCode, List<CartItem>, totalOrderValue
        public decimal CalculateDiscount(string couponCode, List<CartItem> cartItems, decimal totalOrderValue)
        {
            if (string.IsNullOrEmpty(couponCode))
                throw new ArgumentException("Mã không được để trống", nameof(couponCode));

            var km = _db.KhuyenMais.FirstOrDefault(x => x.MaCode == couponCode);
            if (km == null)
                throw new InvalidOperationException("Mã không tồn tại");

            var now = DateTime.Now;
            if (km.NgayBatDau.HasValue && now < km.NgayBatDau.Value)
                throw new InvalidOperationException("Mã chưa đến thời gian áp dụng");

            if (km.NgayKetThuc.HasValue && now > km.NgayKetThuc.Value)
                throw new InvalidOperationException("Mã đã hết hạn");

            if (km.TrangThai.HasValue && km.TrangThai.Value == false)
                throw new InvalidOperationException("Mã không còn hoạt động");

            if ((km.SoLuongDaDung ?? 0) >= (km.SoLuongPhatHanh ?? 0))
                throw new InvalidOperationException("Mã đã hết lượt dùng");

            if (totalOrderValue < (km.DonHangToiThieu ?? 0m))
                throw new InvalidOperationException("Đơn hàng chưa đủ giá trị tối thiểu");

            // Load related SanPhams (many-to-many) to check scope
            _db.Entry(km).Collection(k => k.SanPhams).Load();
            var appliedProductIds = km.SanPhams?.Select(s => s.MaSP).ToList() ?? new List<int>();

            decimal baseAmount = 0m;

            if (appliedProductIds.Count == 0)
            {
                baseAmount = totalOrderValue;
            }
            else
            {
                if (cartItems != null && cartItems.Count > 0)
                {
                    foreach (var item in cartItems)
                    {
                        if (appliedProductIds.Contains(item.MaSP))
                        {
                            baseAmount += item.ThanhTien;
                        }
                    }
                }
            }

            decimal discount = 0m;

            if (!string.IsNullOrEmpty(km.LoaiGiamGia) && km.LoaiGiamGia.Equals("TienMat", StringComparison.OrdinalIgnoreCase))
            {
                discount = km.GiaTriGiam;
            }
            else
            {
                // PhanTram
                discount = (baseAmount * (km.GiaTriGiam / 100m));
                if (km.GiamToiDa.HasValue && discount > km.GiamToiDa.Value)
                    discount = km.GiamToiDa.Value;
            }

            if (discount < 0) discount = 0m;
            if (discount > baseAmount) discount = baseAmount;

            return discount;
        }
    }
}
