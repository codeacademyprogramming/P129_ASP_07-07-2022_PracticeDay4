using P129Allup.Models;
using P129Allup.ViewModels.BasketViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P129Allup.ViewModels.OrderViewModels
{
    public class OrderVM
    {
        public Order Order { get; set; }

        public List<Basket> Baskets { get; set; }
    }
}
