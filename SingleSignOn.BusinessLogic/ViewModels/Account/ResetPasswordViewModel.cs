namespace SingleSignOn.BusinessLogic.ViewModels.Account
{
    public class ResetPasswordViewModel
    {
        //[EmailAddress]
        //[Required(ErrorMessage = "Email is Required")]
        //[RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "Please enter correct Email")]
        //[Display(Name = "Email")]
        public string Email { get; set; }
        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        //[DataType(DataType.Password)]
        //[Display(Name = "Password")]
        //[Required(ErrorMessage = "Password is Required")]
        //[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*)(?!\s*$).+(?=.*[^a-zA-Z]).{6,15}$", ErrorMessage = "Error Format ! ")]
        public string Password { get; set; }
        //[DataType(DataType.Password)]
        //[Display(Name = "Confirm password")]
        //[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        //[Required(ErrorMessage = "Password сonfirmation is Required")]
        //[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*)(?!\s*$).+(?=.*[^a-zA-Z]).{6,15}$", ErrorMessage = "Error Format ! ")]
        public string ConfirmPassword { get; set; }
        public string Code { get; set; }
        public string UserId { get; set; }

        //[Required]
        //[EmailAddress]
        //[Display(Name = "Email")]
        //public string Email { get; set; }

        //[Required]
        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        //[DataType(DataType.Password)]
        //[Display(Name = "Password")]
        //public string Password { get; set; }

        //[DataType(DataType.Password)]
        //[Display(Name = "Confirm password")]
        //[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        //public string ConfirmPassword { get; set; }

        //public string Code { get; set; }
    }
}
