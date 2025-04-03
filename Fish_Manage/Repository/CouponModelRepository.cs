using Fish_Manage.Models;
using Fish_Manage.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Fish_Manage.Repository
{
    public class CouponModelRepository : Repository<CouponModel>, ICouponModelRepository
    {
        private readonly FishManageContext _context;
        private readonly IOrderRepository _dbOrder;

        public CouponModelRepository(FishManageContext context, IOrderRepository dbOrder) : base(context)
        {
            _context = context;
            _dbOrder = dbOrder;
        }

        public async Task<string> ApplyDiscount(double money, string couponId)
        {
            var coupon = await this.GetAsync(u => u.CouponId == couponId);
            if (coupon == null)
            {
                return "Coupon not found";
            }

            if (coupon.DateExpired <= DateTime.Today)
            {
                return "Coupon expired";
            }

            if (coupon.Quantity <= 0)
            {
                return "Coupon is no longer valid";
            }

            Match match = Regex.Match(coupon.CouponDescription, @"(\d+)%");
            if (!match.Success || !double.TryParse(match.Groups[1].Value, out double discount))
            {
                return "Invalid discount format in coupon";
            }

            double discountAmount = money * (discount / 100);
            double finalAmount = money - discountAmount;

            // Decrease coupon quantity
            coupon.Quantity--;
            await this.UpdateAsync(coupon);

            return JsonConvert.SerializeObject(new { newTotal = finalAmount });
        }


        public async Task<CouponModel> UpdateAsync(CouponModel entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }
    }
}
