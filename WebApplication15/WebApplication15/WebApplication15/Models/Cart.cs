using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication15.Models
{
    public class Cart
    {
        DB_SkinFood1Entities data = new DB_SkinFood1Entities();
        public List<GioHang> list { get; set; }

        public Cart()
        {
            list = new List<GioHang>();
        }

        public Cart(List<GioHang> listGH)
        {
            list = listGH ?? new List<GioHang>();
        }

        // Tính số mặt hàng trong giỏ
        public int SoMatHang()
        {
            return list.Count;
        }

        // Tính tổng số lượng sản phẩm
        public int TongSLHang()
        {
            return list.Sum(x => x.SoLuong);
        }

        // Tính tổng thành tiền
        public double TongThanhTien()
        {
            return list.Sum(sp => sp.ThanhTien);
        }

        // Thêm sản phẩm vào giỏ hàng
        public int Them(int id)
        {
            var sp = list.FirstOrDefault(x => x.MaSP == id);
            if (sp == null)
            {
                var newItem = new GioHang(id);
                if (newItem == null)
                    return -1;
                list.Add(newItem);
            }
            else
            {
                sp.SoLuong++;
            }
            return 1;
        }

        // Giảm số lượng sản phẩm
        public int Giam(int id)
        {
            var sp = list.FirstOrDefault(x => x.MaSP == id);
            if (sp == null)
                return -1;

            sp.SoLuong--;
            if (sp.SoLuong <= 0)
                list.Remove(sp);

            return 1;
        }

        // Xóa sản phẩm khỏi giỏ hàng
        public int Xoa(int id)
        {
            var sp = list.FirstOrDefault(x => x.MaSP == id);
            if (sp == null)
                return -1;

            list.Remove(sp);
            return 1;
        }

        // Làm trống giỏ hàng
        public void XoaHet()
        {
            list.Clear();
        }
    }
}