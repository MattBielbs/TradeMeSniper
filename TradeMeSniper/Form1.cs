using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using HtmlAgilityPack;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace TradeMeSniper
{
    public partial class Form1 : Form
    {
        [DllImport("user32")]
        private static extern IntPtr GetWindowDC(IntPtr hwnd);

        // you also need ReleaseDC
        [DllImport("user32")]
        private static extern IntPtr ReleaseDC(IntPtr hwnd, IntPtr hdc);

        private List<TradeMeItem> items = new List<TradeMeItem>();
        private List<TradeMeItem> fuckoff = new List<TradeMeItem>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            for (int i = 1; i < listView1.VirtualListSize; i++)
                imageList1.Images.Add(imageList1.Images[0]);

            backgroundWorker1.RunWorkerAsync();
        }

        private bool checkFilters(TradeMeItem item)
        {
            int value = 0;
            if (trackBar1.InvokeRequired)
                trackBar1.Invoke(new MethodInvoker(() => value = trackBar1.Value));
            else
                value = trackBar1.Value;

            //Price filter
            if (Convert.ToDouble(item.price.Substring(1, item.price.Length - 1)) > value)
                return false;

            //Wrecking and damaged filters
            if (item.name.ToLower().Contains("wreck") || item.name.ToLower().Contains("broken") || item.name.ToLower().Contains("faulty") || item.name.ToLower().Contains("repairs") || item.name.ToLower().Contains("not working") || item.name.ToLower().Contains("parts"))
                return false;

            return true;
        }

        private void getTradeMeItemsFromURL(string url)
        {
            WebClient wc = new WebClient();
            wc.DownloadFile(url, "temp.htm");

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.Load("temp.htm");

            File.Delete("temp.htm");

            HtmlNodeCollection nodeCollection = doc.DocumentNode.SelectNodes("//ul[@id='ListViewList']//li[@class='listingCard']");
            int counter = 0;
            foreach (HtmlNode node in nodeCollection)
            {
                HtmlNode test = node.SelectSingleNode(".//div[@class='listingImage']").SelectSingleNode(".//img");
                string name = test.GetAttributeValue("alt", "error");
                string imagesource = test.GetAttributeValue("src", "noPhoto");

                if (imagesource.Contains("noPhoto"))
                    imagesource = "http://www.trademe.co.nz/images/NewSearchCards/LVIcons/noPhoto_160x120.png";

                backgroundWorker1.ReportProgress(1, new AsyncImage(counter, imagesource));
                            
                string price = node.SelectSingleNode(".//div[@class='listingBidPrice']").InnerText;
                string time = node.SelectSingleNode(".//div[@class='listingCloseDateTime']").InnerText;

                //div class="listingTitle"
                string path = node.SelectSingleNode(".//div[@class='listingTitle']").SelectSingleNode(".//a").GetAttributeValue("href", "null");
                path = path.Insert(0, "http://www.trademe.co.nz/");
                bool shouldAdd = true;
                TradeMeItem newitem = new TradeMeItem(path, name, price, time, counter++);

                if (checkFilters(newitem))
                {
                    foreach (TradeMeItem i in fuckoff)
                    {
                        if (i.Equals(newitem))
                            shouldAdd = false;
                    }
                    if (shouldAdd)
                        items.Add(newitem);
                }

            }
        }

        private async Task<Image> LoadImage(string url)
        {
            MemoryStream content = new MemoryStream();
            WebRequest request = WebRequest.Create(url);
            using (WebResponse response = await request.GetResponseAsync())
            {
                // WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    await responseStream.CopyToAsync(content);
                }
            }

            Bitmap bmp = new Bitmap(content);

            content.Dispose();

            return bmp;
        }

        TradeMeItem getItemByURL(string url)
        {
            foreach (TradeMeItem item in items)
            {
                if (item.url == url)
                    return item;
            }

            return null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach(int i in listView1.SelectedIndices)
            {
                TradeMeItem remove = getItemByURL(items[i].url);
                fuckoff.Add(remove);
                items.Remove(remove);
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (checkBox1.Checked && progressBar1.Value < progressBar1.Maximum - 4)
                progressBar1.Value += 4;
            else
                progressBar1.PerformStep();

            if (progressBar1.Value == progressBar1.Maximum)
            {
                progressBar1.Value = 0;
                if (!backgroundWorker1.IsBusy)
                    backgroundWorker1.RunWorkerAsync();
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            Process.Start(items[listView1.SelectedIndices[0]].url);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            backgroundWorker1.ReportProgress(0);
            items.Clear();
            for (int i = 0; i < 20; i++)
                getTradeMeItemsFromURL("http://www.trademe.co.nz/listings-onedollar/page-" + i + ".htm");

            backgroundWorker1.ReportProgress(2);
        }

        private async void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                if (e.ProgressPercentage == 0)
                {
                    listView1.BeginUpdate();
                }
                if (e.ProgressPercentage == 1)
                {
                    AsyncImage image = (AsyncImage)e.UserState;
                    imageList1.Images[image.id] = (await LoadImage(image.url));
                }

                if (e.ProgressPercentage == 2)
                {
                    listView1.EndUpdate();
                }
            }
            catch { }
        }

        private void listView1_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            try
            {
                if (items.Count <= e.ItemIndex)
                {
                    e.Item = new ListViewItem();
                }
                else
                {
                    TradeMeItem item = items[e.ItemIndex];
                    ListViewItem listitem = new ListViewItem("", item.imageindex);
                    listitem.SubItems.Add(item.name);
                    listitem.SubItems.Add(item.price);
                    listitem.SubItems.Add(item.time);
                    e.Item = listitem;
                }
            }
            catch
            {
                e.Item = new ListViewItem();
            }
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            label3.Text =  "$" + trackBar1.Value;
        }
    }
}
