using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeMeSniper
{
    class TradeMeItem
    {
        public string url;
        public string name;
        public string price;
        public string time;
        public int imageindex;

        public TradeMeItem(string url, string name, string price, string time, int imageindex)
        {
            this.url = url;
            this.name = name;
            this.price = price;
            this.time = time;
            this.imageindex = imageindex;
        }

        public override bool Equals(object obj)
        {
            if(obj.GetType() == typeof(TradeMeItem))
                return ((TradeMeItem)obj).url == this.url;
            return false;
        }
    }
}
