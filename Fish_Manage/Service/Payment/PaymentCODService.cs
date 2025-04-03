using Fish_Manage.Models;
using Fish_Manage.Repository;
using Fish_Manage.Service.IService;

namespace Fish_Manage.Service.Payment
{
    public class PaymentCODService : Repository<Order>, IPaymentCODService
    {
        private readonly FishManageContext _context;

        public PaymentCODService(FishManageContext context) : base(context)
        {
            _context = context;
        }

    }

}
