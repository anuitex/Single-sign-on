﻿namespace SingleSignOn.Configuration
{
    public class EmailConfiguration
    {
        public string EmailDeliverySmptServer = "smtp.gmail.com";
        public string DisplayName = "AnuITex";
        public string EmailDeliveryLogin = "sso.test.mail.sender@gmail.com";
        public int EmailDeliveryPort = 25;
        public string EmailDeliveryPassword = "aBCC7bGSXYgg";
        public string Subject = "New Registration Info";
        public string AdminStartBody = "A new Diamond Club member has registered. ";
        public string AdminDividerBody = "-";
        public string UserBody = "Thanks for registering your Kingsbridge Diamond Club Card. Your registration is complete and you can now start accessing the exclusive Diamond Club offers.";
        public string ForgotPasswordBodyStart = "Please reset your password by clicking <a href=\"";
        public string ForgotPasswordBodyEnd = "\">here</a>";
        public string ConfirmAccountBodyStart = "Please confirm your register by clicking <a href=\"";
        public string ConfirmRegisterBodyEnd = "\">here</a>";
    }
}
