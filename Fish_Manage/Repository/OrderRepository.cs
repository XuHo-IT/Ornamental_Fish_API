using Fish_Manage.Models;
using Fish_Manage.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Fish_Manage.Repository
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        private readonly FishManageContext _context;

        public OrderRepository(FishManageContext context) : base(context)
        {
            _context = context;
        }

        public decimal GetMoneyPerTerm(int term)
        {
            switch (term)
            {
                case 1:
                    return _context.Orders.Where(order => order.OrderDate.Date == DateTime.Now.Date)
                    .Sum(order => decimal.Parse(order.TotalAmount));
                case 2:
                    return _context.Orders.Where(order => order.OrderDate.Month == DateTime.Now.Month && order.OrderDate.Year == DateTime.Now.Year)
                    .Sum(order => decimal.Parse(order.TotalAmount));
                case 3:
                    return _context.Orders.Where(order => order.OrderDate.Year == DateTime.Now.Year)
                    .Sum(order => decimal.Parse(order.TotalAmount));
                default:
                    return default(decimal);
            }
        }

        public ApplicationUser UserBuyMost(int term)
        {
            switch (term)
            {
                case 1:
                    return _context.ApplicationUsers.Include(user =>
                    user.Orders).OrderByDescending(u => u.Orders.Count(o => o.OrderDate.Date == DateTime.Now.Date)).FirstOrDefault();
                case 2:
                    return _context.ApplicationUsers.Include(user =>
                   user.Orders).OrderByDescending(u => u.Orders.Count(o => o.OrderDate.Month == DateTime.Now.Month && o.OrderDate.Year == DateTime.Now.Year)).FirstOrDefault();
                case 3:
                    return _context.ApplicationUsers.Include(user =>
                                      user.Orders).OrderByDescending(u => u.Orders.Count(o => o.OrderDate.Year == DateTime.Now.Year)).FirstOrDefault();
                default:
                    return null;
            }

        }

        async Task<Order> IOrderRepository.UpdateAsync(Order entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }
    }
}
