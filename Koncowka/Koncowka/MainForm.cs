using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Koncowka
{
    public partial class MainForm : Form
    {
        private string url;

        public MainForm()
        {
            InitializeComponent();

            userComboBox.DisplayMember = "Text";
            userComboBox.ValueMember = "Value";
            var items = new[]
            {
                new { Text = "tomek1234", Value = "tomek1234" },
                new { Text = "michal5678", Value = "michal5678" },
                new { Text = "michal54543543534", Value = "michal54543543534" }
            };

            userComboBox.DataSource = items;
            userComboBox.SelectedIndex = 0;

            url = "http://ti.tambou.pl:3000/user-images/";
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                {
                    int picturesOnServer = await Sync.SynchronizeFiles(url, (string)userComboBox.SelectedValue, true);
                    if (picturesOnServer > 0)
                    {
                        txtInfo.Text = "";
                        ImageViewer(url, (string)userComboBox.SelectedValue);
                    }
                    else
                        txtInfo.Text = "Brak plików do wyświetlenia:\r\n";
                }
                else
                {
                    DirectoryInfo di = new DirectoryInfo((string)userComboBox.SelectedValue);
                    if (di.Exists && !DirectoryIsEmpty(di))
                    {
                        MessageBox.Show("Wyświetlanie plików w trybie offline");
                        ImageViewer(url, (string)userComboBox.SelectedValue);
                    }
                    else
                        txtInfo.Text = "Brak plików do wyświetlenia:\r\n";
                }
            }
            catch (WebException)
            {
                MessageBox.Show("Błąd połączenia z serwerem");
            }
            catch (UriFormatException)
            {
                MessageBox.Show("Zła nazwa użytkownika");
            }
            catch (JsonReaderException)
            {
                MessageBox.Show("Wystąpił bład podczas pobierania danych.");
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ImageViewer(string url, string userName)
        {
            Viewer viewer = new Viewer(url, userName);
            viewer.ShowDialog();
        }

        private void txtInfo_TextChanged(object sender, EventArgs e)
        {
            txtInfo.SelectionStart = txtInfo.Text.Length;
            txtInfo.ScrollToCaret();
        }
        private bool DirectoryIsEmpty(DirectoryInfo di)
        {
            int fileCount = Directory.GetFiles(di.Name).Length;
            if (fileCount > 0)
            {
                return false;
            }
            return true;
        }
    }
}
