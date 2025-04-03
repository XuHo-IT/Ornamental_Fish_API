namespace Fish_Manage.Models.Momo
{
    public class MomoOptionModel
    {
        public string PartnerCode { get; set; } = string.Empty;
        public string Accesskey { get; set; } = string.Empty;
        public string Secretkey { get; set; } = string.Empty;
        public string MomoApiUrl { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string NotifyUrl { get; set; } = string.Empty;
        public string RequestType { get; set; } = "captureMoMoWallet";
    }

}
