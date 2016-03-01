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
    public partial class WebForm3 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string srcImageUrl = "http://www.icshop.com.tw/images/product_images/original_images/11758_0.jpg";

                //儲存圖片在Server上
                this.saveThumbPic(srcImageUrl, 600, Server.MapPath("~/Pchome圖檔/test.jpg"));

                //Response給用戶端下載
                FileStream fs = new FileStream(Server.MapPath("~/Pchome圖檔/test.jpg"), FileMode.Open);
                byte[] file = new byte[fs.Length];
                fs.Read(file, 0, file.Length);
                fs.Close();

                Response.Clear();
                Response.AddHeader("content-disposition", "attachment; filename=test.jpg");//強制下載
                Response.ContentType = "image/jpg";
                Response.BinaryWrite(file);

            }
        }

        #region 取得網路上的圖片
        /// <summary>
        /// 取得網路上的圖片
        /// </summary>
        /// <param name="strUrl">圖片的Url路徑</param>
        /// <returns>回傳 System.Drawing.Image物件</returns>
        public System.Drawing.Image getImageFromURL(string strUrl)
        {
            System.Drawing.Image MyImage = null;

            try
            {
                //建立一個 Web Request
                WebRequest MyWebRequest = WebRequest.Create(strUrl);
                //由 Web Request 取得 Web Response
                WebResponse MyWebResponse = MyWebRequest.GetResponse();
                //由 Web Response 取得 Stream
                Stream MyStream = MyWebResponse.GetResponseStream();
                //由 Stream 取得 Image
                MyImage = System.Drawing.Image.FromStream(MyStream);

                //該關的關一關, 該放的放一放
                MyStream.Close();
                MyStream.Dispose();
                MyWebResponse.Close();
                MyWebResponse = null;
                MyWebRequest = null;

            }
            catch (Exception ex)
            {
                throw new Exception("getImageFromURL(string strUrl)發生例外，可能抓不到網路上的圖片" + strUrl);
            }

            //回傳 Image
            return MyImage;
        }

        #endregion


        #region [ASP.net程式使用]圖片等比例縮圖後的寬和高像素
         //<summary>
         //[ASP.net程式使用]取得圖片等比例縮圖後的寬和高像素
         //</summary>
         //<param name="image">System.Drawing.Image 的物件</param>
         //<param name="maxPx">寬或高超過多少像素就要縮圖</param>
         //<returns>回傳int陣列，索引0為縮圖後的寬度、索引1為縮圖後的高度</returns>
        public int[] getThumbPic_WidthAndHeight(System.Drawing.Image image, int maxPx)
        {

            int fixWidth = 0;

            int fixHeight = 0;

            if (image.Width > maxPx || image.Height > maxPx)
            //如果圖片的寬大於最大值或高大於最大值就往下執行  
            {

                if (image.Width >= image.Height)
                //圖片的寬大於圖片的高  
                {

                    fixHeight = Convert.ToInt32((Convert.ToDouble(maxPx) / Convert.ToDouble(image.Width)) * Convert.ToDouble(image.Height));
                    //設定修改後的圖高  
                    fixWidth = maxPx;
                }
                else
                {

                    fixWidth = Convert.ToInt32((Convert.ToDouble(maxPx) / Convert.ToDouble(image.Height)) * Convert.ToDouble(image.Width));
                    //設定修改後的圖寬  
                    fixHeight = maxPx;

                }



            }
            else
            {//圖片沒有超過設定值，不執行縮圖  

                fixHeight = image.Height;

                fixWidth = image.Width;

            }

            int[] fixWidthAndfixHeight = { fixWidth, fixHeight };



            return fixWidthAndfixHeight;
        }

        #endregion

        #region 產生縮圖並儲存
        /// <summary>
        /// 產生縮圖並儲存
        /// </summary>
        /// <param name="srcImageUrl">來源圖片的Url</param>
        /// <param name="maxPix">超過多少像素就要等比例縮圖</param>
        /// <param name="saveThumbFilePath">縮圖的儲存檔案路徑</param>
        public void saveThumbPic(string srcImageUrl, int maxPix, string saveThumbFilePath)
        {
            //為了callBack而callBack的寫法
            System.Drawing.Image.GetThumbnailImageAbort callBack =
                 new System.Drawing.Image.GetThumbnailImageAbort(ThumbnailCallback);


            //取得原始圖片
            System.Drawing.Image image = this.getImageFromURL(srcImageUrl);
            // 計算維持比例的縮圖大小
            //int[] thumbnailScale = this.getThumbPic_WidthAndHeight(image, maxPix);
            //// 產生縮圖
            //System.Drawing.Image smallImage =
            //image.GetThumbnailImage(thumbnailScale[0], thumbnailScale[1], callBack, IntPtr.Zero);
            //// 將縮圖存檔
            //smallImage.Save(saveThumbFilePath);
            //// 釋放
            //image.Dispose();
        }
        private bool ThumbnailCallback()
        {
            return false;
        }
        #endregion

    }
}
