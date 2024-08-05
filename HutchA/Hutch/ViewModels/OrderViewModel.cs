using Hutch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hutch.ViewModels
{
    public class OrderViewModel
    {
        public List<OrderItem> pending {  get; set; }
        public List<OrderItem> outForDelivery {  get; set; }
        public List<OrderItem> delivered {  get; set; }
    }
}