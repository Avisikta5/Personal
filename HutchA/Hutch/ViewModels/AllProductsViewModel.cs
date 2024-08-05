using Hutch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hutch.ViewModels
{
    public class AllProductsViewModel
    {
        public List<Product> Products { get; set; }

        public List<string> WishlistIds { get; set; }
    }
}