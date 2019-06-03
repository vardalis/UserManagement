using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Models
{
    public class PageUsersModel
    {
        public int TotalUsers { get; set; }
        public List<UserModel> Users { get; set; }
            = new List<UserModel>();
    }
}
