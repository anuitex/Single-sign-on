﻿using SingleSignOn.DataAccess.Entities;

namespace SingleSignOn.ViewModels.Account
{
    public class UserInfoViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Token { get; set; }

        public UserInfoViewModel()
        {

        }

        public UserInfoViewModel(ApplicationUser applicationUser, string token = null)
        {
            Id = applicationUser.Id;
            UserName = applicationUser.UserName;
            Token = token;
        }
    }
}
