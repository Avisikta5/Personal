using Hutch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hutch.ViewModels
{
    public class WishlistViewModel
    {
        public int totalItems { get; set; }
        
        public List<Wishlist> wishlists { get; set; }
    }
}