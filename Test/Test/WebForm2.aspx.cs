using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Odbc;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Data.Objects;
using System.Text.RegularExpressions;

namespace Test
{
    public partial class WebForm2 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            String DtStart  = "2015-10-01 00:00:00";//"2015-01-01 00:00:00";
            String DtEnd    = "2015-10-31 11:59:59";//"2015-01-05 23:59:59";
            String ERPUser = "Sid";
            if ((String.IsNullOrEmpty(DtStart)) || (String.IsNullOrEmpty(DtEnd)))
            {
                Response.Write("未輸入起始使時間");
            }
            else
            {
                SelectPchome(DtStart, DtEnd, ERPUser);
            }
        }
        private void SelectPchome(String DtStart, String DtEnd, String ERPUser)
        {
            DataSet dssql = new DataSet();//產生資料集
            DataTable dtsql = new DataTable();//產生單一表格

            dtsql.TableName = "rakutenTable";

            //"庫存量單位。商品的規格唯一識別碼"
            dtsql.Columns.Add("sku");

            //"基本規格單位商品的唯一識別碼    字元數上限：40允許使用的字元：0-9、a-z、A-Z、-、_例如：CD012345678"
            dtsql.Columns.Add("base_sku");

            /*要顯示在購物中心的商品名稱。　　字元數上限：255 例如：炒鍋"*/
            dtsql.Columns.Add("name");

            /* 寫入簡短敘述+關鍵字  
             * 要顯示在購物中心的商品描述 (以簡短吸睛為要)    字元數上限：255例如：多種顏色任君選擇！ */
            dtsql.Columns.Add("tagline");

            //寫入HTML 
            //要顯示在購物中心的商品描述。Rakuten Ichiba 只會顯示本欄位的前 10240 位元資料。 字元數上限：20000例如：這是高品質的不鏽鋼炒鍋。"
            dtsql.Columns.Add("description_1");

            //商品價格(售價)。　數值最低：0.01最高：999999999999.99例如：5.99
            dtsql.Columns.Add("price");

            /* 寫死4906 商品的台灣樂天市場類別的類別 ID   數值 例如：160請參閱 */
            dtsql.Columns.Add("rakuten_product_category_id");

            // 寫死No 
            //啟動商品免運費。
            dtsql.Columns.Add("free_shipping");

            /* 寫死display 
             * 設定商品的目前數量是否會在購物中心顯示 。   顯示：「顯示」、「y」、「是」
             * 隱藏：「隱藏」、「n」、「否」
             * 例如：顯示"*/
            dtsql.Columns.Add("display_quantity");

            /* 寫死= 
             * 定是否要增加、減少或不變更庫存數量。　　　　　　　　　
             * 「+」：增加數量欄中目前庫存的數量
             * 「-」：減少數量欄中目前庫存的數量
             * 「=」：以數量欄中的數量作為目前庫存量
             * 一律必須搭配數量欄位使用
             * 例如：+"
             */
            dtsql.Columns.Add("operator_for_quantity");

            /* 寫死50
             * 設定要從目前庫存增加或減少的商品數量
             * 一律必須搭配 operator_for_quantity 欄位使用
             * 預設值：0        整數
             * 最低：0
             * 最高：999,999,999
             * 例如：8"
             */
            dtsql.Columns.Add("quantity");

            /* 寫死yes
             * 當顧客『取消訂單』時，設定是否要將商品退回庫存　　　　　　　　
             * 「是」
             * 「否」
             * 字元數上限：10
             * 例如：yes
             * 預設值：是"
             */
            dtsql.Columns.Add("return_quantity_in_cancel");

            /* 寫死50
             * 設定顧客一次可以購買的商品數目限制    
             * 預設值：無限制                
             * 整數
             * 最低：0
             * 最高：999,999,999 
             * 例如：6"
            */
            dtsql.Columns.Add("purchase_quantity_limit");

            /* 去撈第二層的分類代碼 樂天已對應好
             * 商品的商品類別唯一識別碼 (分類)    字元數上限：50
             * 例如：分類 : 依照店家類別的[唯一識別碼]進行填入*/
            dtsql.Columns.Add("shop_product_unique_identifier_1");

            dssql.Tables.Add(dtsql);
            int check = 0;

            //將時間字串轉型
            DateTime s = Convert.ToDateTime(DtStart);
            DateTime e = Convert.ToDateTime(DtEnd);
            //紀錄資料數
            int count = 0;
            //節省資源空間
            using (icshopc_icshopcEntities ent = new icshopc_icshopcEntities())
            {
                //紀錄產品PK碼，等會判斷重複不要撈
                int Products_temp_id = 0;
                //Linq 相關參考Navicat SQL
                var v = (from product in ent.products

                         join pd in ent.products_description on product.products_id equals pd.products_id

                         join Ptc in ent.products_to_categories on product.products_id equals Ptc.products_id

                         join Cat in ent.categories on Ptc.categories_id equals Cat.categories_id

                         join Cd in ent.categories_description on Ptc.categories_id equals Cd.categories_id

                         join Ss in ent.shipping_status on product.products_shippingtime equals Ss.shipping_status_id

                         where (product.products_date_added >= s && product.products_date_added < e && Cd.language_id == 1 && Ss.language_id == 1 && !String.IsNullOrEmpty(product.products_model) && product.products_status == true)

                         select
                         new
                         {
                             product.products_model,
                             product.products_price,
                             product.products_image,
                             product.products_storage_spaces,
                             product.products_id,
                             pd.products_name,
                             pd.products_short_description,
                             pd.products_description1,
                             pd.products_meta_keywords,
                             Cd.categories_name,
                             Ss.shipping_status_name,
                             Cat.parent_id,
                             Cat.categories_id
                         }

                         );
                foreach (var h in v)
                {
                    bool a = h.products_id == Products_temp_id;
                    bool b = !h.categories_name.Equals("本月優惠");
                    bool c = !h.categories_name.Equals("出清特價");
                    //本月優惠與出清特價不撈
                    if (b && c)
                    {
                        //若是都沒出清與優惠，在看看商品是否重複
                        if (a)
                        {
                            continue;
                        }
                        else
                        {
                            //條件過濾後開始確認計數，並把第一筆產品PK寫入TEMPID以做之後回圈的確認
                            check++;
                            Products_temp_id = h.products_id;
                            //新增一列
                            dssql.Tables["rakutenTable"].Rows.Add();
                            //公司叫做貨號 官網叫做MODEL ERP叫做ProSeq 其實他有一個名字叫做SKU
                            dssql.Tables[0].Rows[count]["sku"] = h.products_model;
                            dssql.Tables[0].Rows[count]["base_sku"] = h.products_model;
                            
                            //商品名稱 店內搜尋 撈商品名稱+關鍵字+SKU
                            dssql.Tables[0].Rows[count]["name"] = ((h.products_name + " - " + h.products_meta_keywords).Replace("/", "／")).Replace("\"", "＂") + "‧" + h.products_model + "‧";

                            //簡短敘述+關鍵字
                            dssql.Tables[0].Rows[count]["tagline"] = ((h.products_short_description + "," + h.products_meta_keywords).Replace("/", "／")).Replace("\"", "＂");
                            


                            /*商品館別(商品分類過濾) (剛好官網與PCHOME都是三階層) 
                            先做商品第一層判斷，如果分類為0代表最高層
                            便可直接寫入Table
                            */
                            if (h.parent_id == 0)
                            {
                                dssql.Tables[0].Rows[count]["shop_product_unique_identifier_1"] = h.categories_id;
                            }

                            else
                            {
                                //若不是則則代表還有上層分類，必須重撈，這邊必須新增一個連線並且過濾語系
                                //parent_id 代表的是 categories_id 
                                using (icshopc_icshopcEntities1 ent1 = new icshopc_icshopcEntities1())
                                {
                                    var Two = from Cat in ent1.categories
                                              join Cd in ent1.categories_description on Cat.categories_id equals Cd.categories_id
                                              where Cat.categories_id == h.parent_id && Cd.language_id == 1
                                              select new { Cat.parent_id, Cat.categories_id };

                                    foreach (var zz in Two)
                                    {
                                        if (zz.parent_id == 0)
                                        {
                                            dssql.Tables[0].Rows[count]["shop_product_unique_identifier_1"] = h.categories_id;
                                        }
                                        else
                                        {
                                            using(icshopc_icshopcEntities2 ent2=new icshopc_icshopcEntities2())
                                            {
                                                var three = from Cat in ent2.categories
                                                            join Cd in ent2.categories_description on Cat.categories_id equals Cd.categories_id
                                                            where Cat.categories_id == zz.parent_id && Cd.language_id == 1
                                                            select new { Cat.parent_id, Cat.categories_id };
                                                foreach(var xx in three)
                                                {
                                                    if(xx.parent_id==0)
                                                    {
                                                        dssql.Tables[0].Rows[count]["shop_product_unique_identifier_1"] = zz.categories_id;
                                                    }
                                                    else
                                                    {
                                                        using (icshopc_icshopcEntities3 ent3 = new icshopc_icshopcEntities3())
                                                        {
                                                            var four = from Cat in ent3.categories
                                                                       join Cd in ent3.categories_description on Cat.categories_id equals Cd.categories_id
                                                                        where Cat.categories_id == xx.parent_id && Cd.language_id == 1
                                                                        select new { Cat.parent_id, Cat.categories_id };
                                                            foreach (var cc in four)
                                                            {
                                                                if (cc.parent_id == 0)
                                                                {
                                                                    dssql.Tables[0].Rows[count]["shop_product_unique_identifier_1"] = xx.categories_id;
                                                                }
                                                                else
                                                                {
                                                                    using (icshopc_icshopcEntities4 ent4 = new icshopc_icshopcEntities4())
                                                                    {
                                                                        var five = from Cat in ent4.categories
                                                                                   join Cd in ent4.categories_description on Cat.categories_id equals Cd.categories_id
                                                                                   where Cat.categories_id == cc.parent_id && Cd.language_id == 1
                                                                                   select new { Cat.parent_id, Cat.categories_id };
                                                                        foreach (var vv in five)
                                                                        {
                                                                            if (vv.parent_id == 0)
                                                                            {
                                                                                dssql.Tables[0].Rows[count]["shop_product_unique_identifier_1"] = cc.categories_id;
                                                                            }
                                                                            else
                                                                            {

                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            //Html介紹 把nowrap標籤拿掉，不知道啥鬼
                            //把.rar拿掉，樂天不允許
                            //shippingtime為出貨時程加在最前面
                            //把href也拿掉，樂天不允許
                            //String test = "http://www.icshopping.com.tw/" + h.products_model + "/" + h.products_model + ".rar";
                            string[] resultString = Regex.Split(h.products_description1.Replace("nowrap=\"\"", "").Replace("http://www.icshopping.com.tw/" + h.products_model + "/" + h.products_model + ".rar", "").Replace("參考文件下載","").Replace("href",""), "<a href", RegexOptions.IgnoreCase);
                            String shippingtime = "<span style=\"color: rgb(255, 50, 65); font-family: 'Times New Roman'; font-size: 16px; font-weight: bold;\">" + h.shipping_status_name + "</span></br>";
                            dssql.Tables[0].Rows[count]["description_1"] = shippingtime + resultString[0];
                            
                            //售價
                            dssql.Tables[0].Rows[count]["price"] = Convert.ToInt32(h.products_price);

                            //樂天大分類
                            dssql.Tables[0].Rows[count]["rakuten_product_category_id"] = "4906";


                            //是否免運
                            dssql.Tables[0].Rows[count]["free_shipping"] = "no";

                            //是否顯示庫存
                            dssql.Tables[0].Rows[count]["display_quantity"] = "display";

                            //是否增加減少數存 =
                            dssql.Tables[0].Rows[count]["operator_for_quantity"] = "=";
                            
                            
                            //庫存從多少開始倒扣
                            dssql.Tables[0].Rows[count]["quantity"] = 50;
                            //客人取消訂單後數量是否到扣
                            dssql.Tables[0].Rows[count]["return_quantity_in_cancel"] = "yes";
                            //最大購買量
                            dssql.Tables[0].Rows[count]["purchase_quantity_limit"] = 50;

                            //圖片名稱，FORMAT掉.JPG 在去圖庫撈
                            if (String.IsNullOrEmpty(h.products_image))
                            {
                                String tmp = "此商品無圖片請找官網上架人員";
                            }
                            else
                            {
                                Uri uri = new Uri("http://www.icshop.com.tw/images/product_images/original_images/" + h.products_image);
                                string saveDir = @"C:\rakuten\" + s.ToString("yyyy-MM") + "\\";    // 注意：資料夾必須已存在
                                string fileName = h.products_model + "_1" + ".jpg";
                                string savePath = (saveDir + fileName).Replace("*","");

                                System.Net.WebRequest request = System.Net.WebRequest.Create(uri);
                                System.Net.WebResponse response = request.GetResponse();
                                System.IO.Stream stream = response.GetResponseStream();
                                System.Drawing.Image img = System.Drawing.Image.FromStream(stream);
                                if (Directory.Exists(saveDir))
                                {
                                    img.Save(savePath);
                                }
                                else
                                {
                                    //新增資料夾
                                    Directory.CreateDirectory(saveDir);
                                    img.Save(savePath);
                                }
                            }
                            count++;
                        }
                    }
                    else
                    {

                    }

                }
            }
            //轉excel BJ4
            DataView dv = new DataView();
            dv = dssql.Tables["rakutenTable"].DefaultView;

            GridView1.DataSource = dv;
            GridView1.DataBind();

            Response.Clear();
            Response.Write("<meta http-equiv=Content-Type content=text/html;charset=utf-8>");
            Response.AddHeader("content-disposition",

                "attachment;filename=" + s.ToShortDateString() + "～～" + e.ToShortDateString() + "～～" + ERPUser + "_樂天日期批次轉檔" + ".xls");

            Response.ContentType = "application/vnd.xls";

            System.IO.StringWriter sw = new System.IO.StringWriter();

            System.Web.UI.HtmlTextWriter htw = new HtmlTextWriter(sw);

            //移去不要的欄位

            //GridView1.Columns.RemoveAt(GridView1.Columns.Count - 1);

            //GridView1.DataBind();

            //建立假HtmlForm避免以下錯誤

            //Control 'GridView1' of type 'GridView' must be placed inside 

            //a form tag with runat=server. 

            //另一種做法是override VerifyRenderingInServerForm後不做任何事

            //這樣就可以直接GridView1.RenderControl(htw);



            HtmlForm hf = new HtmlForm();

            Controls.Add(hf);

            hf.Controls.Add(GridView1);

            hf.RenderControl(htw);


            try
            {
                Response.Write(sw.ToString().Substring(sw.ToString().IndexOf("<table")));
            }
            catch (Exception ex)
            {
                Response.Write("區間內無資料" + s.ToShortDateString() + "-----" + e.ToShortDateString());

            }
            Response.End();

            /* To Excel */

            //Response.Clear();
            //Response.Write("<meta http-equiv=Content-Type content=text/html;charset=utf-8>");
            //Response.AddHeader("content-disposition", "attachment;filename=" + DateTime.Now.ToShortDateString() + "Store.xls");//excel檔名
            //Response.ContentType = "application/vnd.ms-excel";
            //System.IO.StringWriter sw = new System.IO.StringWriter();
            //System.Web.UI.HtmlTextWriter htw = new HtmlTextWriter(sw);
            //Response.Write(sw.ToString());

            /* To Excel end */

            /*** ToCsv ****/
            //Response.Clear();
            //Response.Buffer = true;

            //Response.AddHeader("content-disposition",
            //"attachment;filename=GridViewExport.csv");
            //Response.Charset = "";
            //Response.ContentType = "text/text";

            //GridView1.AllowPaging = false;
            ////grv.DataBind();

            //StringBuilder sb = new StringBuilder();
            //for (int k = 0; k < GridView1.Columns.Count; k++)
            //{
            //    //add separator

            //    sb.Append(((char)(9)).ToString() + GridView1.Columns[k].HeaderText + ',');
            //}
            ////append new line
            //sb.Append("\r\n");
            //for (int i = 0; i < GridView1.Rows.Count; i++)
            //{
            //    for (int k = 0; k < GridView1.Columns.Count; k++)
            //    {
            //        //add separator
            //        //sb.Append(GridView1.Rows[i].Cells[k].Text + ',');
            //        sb.Append(HttpUtility.HtmlDecode(GridView1.Rows[i].Cells[k].Text) + ',');

            //    }

            //    //append new line
            //    sb.Append("\r\n");
            //}
            ////Byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());
            //Response.Output.Write(sb.ToString());
            //Response.Flush();
            //Response.End();
            /*** ToCsv end ****/
        }
        
    }
}