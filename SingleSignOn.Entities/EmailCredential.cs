using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SingleSignOn.Entities
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
