namespace SingleSignOn.DataAccess.Entities
{
    public class EmailCredential
    {
        public string EmailDeliveryLogin { get; set; }
        public string DisplayName { get; set; }
        public string EmailDeliverySmptServer { get; set; }
        public int EmailDeliveryPort { get; set; }
        public string EmailDeliveryPassword { get; set; }
    }
}
