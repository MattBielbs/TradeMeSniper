using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeMeSniper
{
    class AsyncImage
    {
        public string url;
        public int id;

        public AsyncImage(int id, string url)
        {
            this.id = id;
            this.url = url;
        }
    }
}
