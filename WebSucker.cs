using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Net;
using HtmlAgilityPack;

namespace Ontheheap
{
    public class WebSucker
    {
        string _url;
        Stream data;
        StreamReader reader;
        string _cardId;
        IPageSaver pageSaver;

        public WebSucker(string url, string cardId, IPageSaver saver)
        {
            this._url = url;
            this._cardId = cardId;  //simple obfuscation of card id
            pageSaver = saver;
            Open();

        }


        public void Close()
        {
            data.Close();
            reader.Close();

        }

        private void Open()
        {
            WebClient client = new WebClient();

            // Add a user agent header in case the 
            // requested URI contains a query.

            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            data = client.OpenRead(_url);
            reader = new StreamReader(data);

        }

        public List<string> GetLinks()
        {
            List<string> links = new List<string>();
            HtmlDocument doc = new HtmlDocument();

            doc.Load(reader);
            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
            {
                string val = link.GetAttributeValue("href", "");
                links.Add(val);
            }

            return links;
        }



        public List<string> GetImages()
        {
            List<string> links = new List<string>();
            HtmlDocument doc = new HtmlDocument();

            doc.Load(reader);
            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//img"))
            {
                string newName = Guid.NewGuid().ToString();
                string val = link.GetAttributeValue("src", "");

                links.Add(val);
            }
            return links;
        }


        public void ProcessPage()
        {

            HtmlDocument doc = new HtmlDocument();

            doc.Load(reader);
            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//img"))
            {
                string newName = Guid.NewGuid().ToString();
                string val = link.GetAttributeValue("src", "");

                int lastIndexOfDot = val.LastIndexOf(".");
                string ext = lastIndexOfDot > 0 ? val.Substring(lastIndexOfDot) : string.Empty;
                ext = (ext.Length > 4) ? (".jpg") : (ext);

                newName = newName + ext;

                DownloadFile(NormalizeUrl(this._url, val), newName, "image/jpg");
                link.SetAttributeValue("src", newName);
            }

            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//link"))
            {
                string newName = Guid.NewGuid().ToString();
                string val = link.GetAttributeValue("href", "");

                int lastIndexOfDot = val.LastIndexOf(".");
                string ext = lastIndexOfDot > 0 ? val.Substring(lastIndexOfDot) : string.Empty;
                ext = (ext.Length > 4) ? (".css") : (ext);

                newName = newName + ext;

                DownloadFile(NormalizeUrl(this._url, val), newName, "text/css");
                link.SetAttributeValue("href", newName);
            }


            Stream s = new MemoryStream();
            doc.Save(s);
            using (s)
            {
                pageSaver.SaveStream(s, "index.html", "text/html", _cardId);
            }
        }

        private string NormalizeUrl(string pageUrl, string resourceUrl)
        {
            if (resourceUrl.IndexOf("http") >= 0)
            { return resourceUrl; }

            return "http://" + new Uri(pageUrl).Host + resourceUrl;
        }


        private void DownloadFile(string url, string newName, string mimeType)
        {
            System.Diagnostics.Debug.WriteLine(url);

            if (string.IsNullOrEmpty(url))
            { return; }

            WebClient client = new WebClient();
            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            //file based
            //client.DownloadFile(url, _savePath + @"\" + newName);


            //stream based
            try
            {
                byte[] data = client.DownloadData(url);
                Stream s = new MemoryStream(data);
                using (s)
                {
                    pageSaver.SaveStream(s, newName, mimeType, _cardId);
                }
            }
            catch (Exception ex)
            {
                //don't want to abort whole page if 1 resource file is a problem.
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

        }


    }


}
