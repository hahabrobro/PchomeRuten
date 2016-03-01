using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Net;

namespace Test
{
    public partial class WebForm5 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Uri uri = new Uri("http://www.icshop.com.tw/images/product_images/original_images/11758_0.jpg");
            string saveDir = @"C:\";    // 注意：資料夾必須已存在
            string fileName = "11758_0.jpg";
            string savePath = saveDir + fileName;

            System.Net.WebClient WC = new System.Net.WebClient();
            System.IO.MemoryStream Ms = new System.IO.MemoryStream(WC.DownloadData(uri));
            System.Drawing.Image img = System.Drawing.Image.FromStream(Ms);
            try
            {
                img.Save(savePath);
            }catch(Exception Ex)
            {
                Response.Write(Ex.ToString());
            }
            

        }
    }
}