using Hutch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hutch.ViewModels
{
    public class IndexViewModel
    {
        public List<User> Users { get; set; }

        public List<Category> Categories { get; set; }

        public List<Product> TopSellingProducts { get; set;}

        public List<Product> NewArrivals { get; set; }

    }
}