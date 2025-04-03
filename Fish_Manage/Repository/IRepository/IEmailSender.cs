namespace Fish_Manage.Repository.IRepository
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
