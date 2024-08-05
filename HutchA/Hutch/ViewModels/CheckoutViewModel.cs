using Hutch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hutch.ViewModels
{
    public class CheckoutViewModel
    {
        public Product CheckoutItem { get; set; }

        public List<Feedback> Feedbacks { get; set; }

        public  bool isWishlisted { get; set; }

        public bool isCarted {  get; set; }

        public bool isBought { get; set; }
    }
}