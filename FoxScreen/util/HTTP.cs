using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Web;
using System.Windows.Forms;
using System.Net.Cache;

namespace FoxScreen
{
    class HTTP
    {
        private static string req(string url, string method, string data)
        {
            method = method.ToUpper();
            try
            {
                HttpWebRequest hwr = (HttpWebRequest)HttpWebRequest.Create(url);
                hwr.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
                hwr.Method = method;
                hwr.Proxy = null;
                if (data != null)
                {
                    hwr.ContentType = "application/x-www-form-urlencoded";
                    hwr.ContentLength = data.Length;
                    StreamWriter sw = new StreamWriter(hwr.GetRequestStream());
                    sw.Write(data, 0, data.Length);
                    sw.Flush();
                    sw.Close();
                }

                string ret;
                HttpWebResponse hwres = (HttpWebResponse)hwr.GetResponse();
                if(method == "HEAD") {
                    ret = hwres.ContentLength.ToString();
                } else {
                    StreamReader sr = new StreamReader(hwres.GetResponseStream());
                    ret = sr.ReadToEnd();
                    sr.Close();
                    hwres.Close();
                }
                if (ret == null) return "";
                return ret;
            }
            catch
            {
                return "";
            }
        }

        public static string GET(string url)
        {
            return req(url, "GET", null);
        }

        public static string POST(string url, string data)
        {
            return req(url, "POST", data);
        }

        public static int GetLength(string url)
        {
            return Convert.ToInt32(req(url, "HEAD", null));
        }

        public static string Escape(string str)
        {
            return Uri.EscapeDataString(str).Replace("%20", "+");
        }
    }
}
