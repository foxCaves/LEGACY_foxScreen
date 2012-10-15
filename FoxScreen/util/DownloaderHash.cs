using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Net.Cache;
using System.Diagnostics;

namespace FoxScreen
{
    class DownloaderHash
    {
        public static string HashStream(Stream str)
        {            
            MD5 md5 = new MD5CryptoServiceProvider();
            str.Seek(0, SeekOrigin.Begin);
            byte[] md5Hash = md5.ComputeHash(str);
            str.Seek(0, SeekOrigin.Begin);
            return BitConverter.ToString(md5Hash).Replace("-", "").ToLower();
        }

        public static string HashFile(string filename)
        {
            if (!File.Exists(filename)) return "$INVALID$";
            FileStream str = File.OpenRead(filename);
            string ret = HashStream(str);
            str.Close();
            return ret;
        }

        public static MemoryStream DonwloadFile(string url, string hash)
        {
            double fileSize = 0;
            double filePos = 0;
            int tries = 0;
            while (tries < 3)
            {
                tries++;

                try
                {
                    HttpWebRequest hwr = (HttpWebRequest)HttpWebRequest.Create(url);
                    hwr.Method = "GET";
                    hwr.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
                    hwr.Proxy = null;
                    MemoryStream ms = new MemoryStream((int)fileSize);
                    HttpWebResponse hwres = (HttpWebResponse)hwr.GetResponse();
                    Stream sr = hwres.GetResponseStream();
                    fileSize = hwres.ContentLength;

                    byte[] buffer = new byte[4096];
                    int bRead = 0;
                    while (sr.CanRead && filePos < fileSize)
                    {
                        bRead = sr.Read(buffer, 0, 4096);
                        ms.Write(buffer, 0, bRead);
                        filePos += bRead;
                    }

                    sr.Close();
                    hwres.Close();

                    if (hash == null || HashStream(ms) == hash)
                    {
                        return ms;
                    }
                }
                catch(Exception e) { MessageBox.Show(url + "\n" + e.ToString()); }
            }

            return null;
        }

        public static int DownloadFileTo(string filename, string folder, string url, string hash)
        {
            return DownloadFileTo(folder + "/" + filename, url, hash);
        }

        public static int DownloadFileTo(string dest, string url, string hash)
        {
            if (HashFile(dest) == hash) return 0;
            MemoryStream ms = DonwloadFile(url, hash);
            if (ms != null)
            {
                FileStream fs = File.Open(dest, FileMode.Create);
                ms.WriteTo(fs);
                fs.Close();
                return 1;
            }
            return -1;
        }
    }
}
