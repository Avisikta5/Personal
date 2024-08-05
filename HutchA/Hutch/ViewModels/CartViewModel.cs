using Hutch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hutch.ViewModels
{
    public class CartViewModel
    {
        public List<Cart> Carts {  get; set; }

        public int totalItem { get; set; }

        public int totalAmount { get; set; }
    }
}