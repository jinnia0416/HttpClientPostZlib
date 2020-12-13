using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;
using zlib;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HttpClientRequest
{
    public partial class Form1 : Form
    {
        private static HttpClient hc;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            //檢查輸入是否正確
            if (string.IsNullOrWhiteSpace(comBoxMethod.Text) || string.IsNullOrWhiteSpace(comBoxIsZip.Text) ||
                !Regex.IsMatch(txbSendNumber.Text, "^[0-9]*$") ||
                (!txbUrl.Text.StartsWith("http://") && !txbUrl.Text.StartsWith("https://")))
            {
                MessageBox.Show("請輸入正確的參數!");
                return;
            }
            
            using (hc)
            {
                hc = new HttpClient();
                //將標頭加入HttpCleint
                for (int i = 1; i <= 10; i++)
                {
                    TextBox key = Controls.Find("txbHeadKey" + i.ToString(), true).FirstOrDefault() as TextBox;
                    TextBox val = Controls.Find("txbHeadVal" + i.ToString(), true).FirstOrDefault() as TextBox;
                    if (!string.IsNullOrWhiteSpace(key.Text) && !string.IsNullOrWhiteSpace(val.Text))
                    {
                        try{ hc.DefaultRequestHeaders.Add(key.Text, val.Text); }
                        //不做例外處理
                        catch { }
                    }
                }

                //送出x次
                int.TryParse(txbSendNumber.Text, out int sendNumber);
                for (int i = 0; i < sendNumber; i++)
                {
                    if (comBoxIsZip.Text == "內容不壓縮")
                    {
                        Task.Run(() => SendNotZip(comBoxMethod.Text));
                    }
                    else
                    {
                        Task.Run(() => SendZip(comBoxMethod.Text));
                    }
                }

            }
        }

        //不壓縮送出
        private void SendNotZip(string method)
        {
            try
            {
                if (method == "POST")
                {
                    hc.PostAsync(txbUrl.Text, new StringContent(txbBody.Text));
                }
                else
                {
                    hc.GetAsync(txbUrl.Text);
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        //壓縮送出
        private void SendZip(string method)
        {
            try
            {
                if (method == "POST")
                {
                    //將body壓縮
                    byte[] bodyBytes = UTF8Encoding.UTF8.GetBytes(txbBody.Text);
                    MemoryStream ms = new MemoryStream();
                    Stream bodyZip = new ZOutputStream(ms, zlibConst.Z_STREAM_END);
                    bodyZip.Write(bodyBytes, 0, bodyBytes.Length);
                    bodyZip.Close();
                    byte[] compressData = (byte[])ms.ToArray();
                    ms.Flush();
                    ms.Close();

                    //送出請求
                    hc.PostAsync(txbUrl.Text, new StreamContent(new MemoryStream(compressData)));
                }
                else
                {
                    hc.GetAsync(txbUrl.Text);
                }
            }catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}
