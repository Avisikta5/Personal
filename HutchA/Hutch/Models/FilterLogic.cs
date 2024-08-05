using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Web;

namespace Hutch.Models
{
    public class FilterLogic
    {
        public List<Product> filter(List<string> filters, List<Product> products)
        {

            if (filters != null)
            {
                    
                foreach (var item in filters)
                {
                    if (item.ToString() == "Clear")
                    {
                        return products;
                    }
                    if (item.ToString() == "Low")
                    {
                        products = products.OrderBy(p => p.p_price).ToList();
                    }
                    if (item.ToString() == "High")
                    {
                        products = products.OrderByDescending(p => p.p_price).ToList();
                    }
                    if (item.ToString() == "New")
                    {
                        products = products.Where(p => p.p_arrival == "New").ToList();
                    }
                }
                
            }
            return products.ToList();
        }
    }
}