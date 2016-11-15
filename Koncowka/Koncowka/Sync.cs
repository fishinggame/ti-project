using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Koncowka
{
    public static class Sync
    {
        static public async Task<int> SynchronizeFiles(string url, string userName, bool info)
        {
            Picture picturesOnServer = new Picture(GetResponse(url, userName));

            DirectoryInfo di = new DirectoryInfo(userName);
            if (!di.Exists)
                Directory.CreateDirectory(userName);

            string[] fileEntries = Directory.GetFiles(userName);
            List<string> filesToDownload = new List<string>();
            //pliki do pobrania
            if (fileEntries.Length > 0 && picturesOnServer.ArrayOfSrcs.Length > 0)
            {
                foreach (string fileOnServer in picturesOnServer.ArrayOfSrcs)
                {
                    foreach (string fileOnDisk in fileEntries)
                    {
                        if (fileOnServer.Contains(Path.GetFileName(fileOnDisk)))
                            break;
                        if (fileOnDisk == fileEntries.Last())
                            filesToDownload.Add(fileOnServer);
                    }
                }
                //usuwanie zbednych plikow
                foreach (string fileOnDisk in fileEntries)
                {
                    foreach (string fileOnServer in picturesOnServer.ArrayOfSrcs)
                    {
                        if (fileOnDisk.Contains(Path.GetFileName(fileOnServer)))
                            break;
                        if (fileOnServer == picturesOnServer.ArrayOfSrcs.Last())
                            File.Delete(fileOnDisk);
                    }
                }
            }
            else
            {
                filesToDownload = picturesOnServer.ArrayOfSrcs.ToList<string>();
            }

            if (filesToDownload.Count > 0)
            {
                await Downloader(filesToDownload, userName, info);
            }
            return picturesOnServer.ArrayOfSrcs.Length;
        }

        private static string GetResponse(string url, string userName)
        {
            // Create a request using a URL that can receive a post. 
            WebRequest request = WebRequest.Create(url + userName);
            // Set the Method property of the request to POST.
            request.Method = "POST";
            // Get the response.
            WebResponse response = request.GetResponse();
            // Display the status.
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Display the content.
            // Clean up the streams.
            reader.Close();
            dataStream.Close();
            response.Close();
            //////////////////////////////////////////////////////////////
            responseFromServer = responseFromServer.Replace('\"', '\'');
            return responseFromServer;
        }

        private static async Task Downloader(List<string> filesToDownload, string userName, bool info)
        {
            using (WebClient wc = new WebClient())
            {
                if (info == true)
                {
                    wc.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted);
                    wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);
                }               
                foreach (var file in filesToDownload)
                {
                    await wc.DownloadFileTaskAsync(new Uri(file.ToString()),
                                   string.Format("{0}\\{1}", userName, Path.GetFileName(file.ToString())));
                }
                await Task.Delay(500);
            }
        }

        private static void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            TextBox t = Application.OpenForms["MainForm"].Controls["txtInfo"] as TextBox;
            t.Text += string.Format(" downloaded {0} of {1} bytes. {2} % complete...\r\n",
                e.BytesReceived,
                e.TotalBytesToReceive,
                e.ProgressPercentage);
        }

        private static void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            TextBox t = Application.OpenForms["MainForm"].Controls["txtInfo"] as TextBox;
            t.Text += "Ukończono pobieranie pliku.\r\n";
        }
    }
}
