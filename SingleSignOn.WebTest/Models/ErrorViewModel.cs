
namespace SingleSignOn.WebTest.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId()
        {
            var result = !string.IsNullOrEmpty(RequestId);
            return result;
        }
    }
}