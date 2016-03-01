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

namespace Test
{
    public partial class QuaTop : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            String DtStart = "2015-07-01 00:00:00";//Request["DtStart"];
            String DtEnd = "2015-07-31 23:59:59";//Request["DtENd"];
            String ERPUser = "Sid";
            if ((String.IsNullOrEmpty(DtStart)) || (String.IsNullOrEmpty(DtEnd)))
            {
                Response.Write("未輸入起始使時間");
            }
            else
            {
                SelectQuaTop(DtStart, DtEnd, ERPUser);
            }
        }
        private void SelectQuaTop(String DtStart, String DtEnd, String ERPUser)
        {
            DataSet dssql = new DataSet();//產生資料集
            DataTable dtsql = new DataTable();//產生單一表格

            dtsql.TableName = "QuaTop";
            //商品名稱(最多64個中文字)商品名稱不能與web上已上架商品相同
            dtsql.Columns.Add("emp");
            //發票名稱(最多20個中文字)若為PChome代開發票者，此欄位內容將是發票品名；若非由PChome代開者，此欄位將無作用但仍請填寫。
            dtsql.Columns.Add("sellstore");
            /*商品館別(店家的小館名稱)
             (至多選3個，以";"隔開)
              如: 店家小館1;店家小館2
              店家小館需由店家先至店家系統後台新增小館才能使用，若未新增在上傳時會失敗。*/
            dtsql.Columns.Add("product_name");
            /*PChome商店街小館
              (請店家依據商品的屬性，將商品歸類在商店街的小館裏，顧客點入小館中可搜尋到此項商品)
              (商店街小館請參考  http://www.pcstore.com.tw/adm/opt/pexh_list.htm)
              (填入最後一層有H開頭的館別代號，只可填入1個館別代號)
               如:H100076*/
            dtsql.Columns.Add("proseq");
            //成本跟建議售價軍空白
            dtsql.Columns.Add("sell_total");
            dtsql.Columns.Add("product_price");
            dtsql.Columns.Add("sell_price_total");
            dssql.Tables.Add(dtsql);
            // int i = 0;
            int check = 0;

            //將時間字串轉型
            DateTime s = Convert.ToDateTime(DtStart);
            DateTime e = Convert.ToDateTime(DtEnd);
            //紀錄資料數
            int count = 0;
            //節省資源空間
            using (inikierpEntities ent = new inikierpEntities())
            {
                //紀錄產品PK碼，等會判斷重複不要撈
                int Products_temp_id = 0;
                //Linq 相關參考Navicat SQL
               var v=from n in ent.QuaTop(s,e,22,2) select n ;
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
                            dssql.Tables["pchometable"].Rows.Add();

                            //商品名稱 店內搜尋 撈商品名稱+簡短敘述+SKU
                            dssql.Tables[0].Rows[count]["productname"] = ((h.products_name + " - " + h.products_meta_keywords).Replace("/", "／")).Replace("\"", "＂") + "‧" + h.products_model + "‧";
                            //商品抬頭 商品名稱
                            dssql.Tables[0].Rows[count]["producttitle"] = (h.products_name.Replace("/", "／")).Replace("\"", "＂");

                            /*商品館別(商品分類過濾) (剛好官網與PCHOME都是三階層) 
                            先做商品第一層判斷，如果分類為0代表最高層
                            便可直接寫入Table
                            */
                            if (h.parent_id == 0)
                            {
                                dssql.Tables[0].Rows[count]["our_shop_Category"] = h.categories_name;
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
                                              select new { Cat.parent_id, Cd.categories_name, Cat.categories_id };

                                    foreach (var t in Two)
                                    {
                                        if (t.parent_id == 0)
                                        {
                                            dssql.Tables[0].Rows[count]["our_shop_Category"] = h.categories_name;
                                        }
                                        else
                                        {
                                            //    using (icshopc_icshopcEntities2 ent2 = new icshopc_icshopcEntities2())
                                            //    {
                                            //        var Three = from Cat in ent2.categories
                                            //                    join Cd in ent2.categories_description on Cat.categories_id equals Cd.categories_id
                                            //                    where Cat.categories_id == t.parent_id && Cd.language_id == 1
                                            //                    select new { Cat.parent_id, Cd.categories_name };
                                            //        foreach (var y in Three)
                                            //        {
                                            //            if (y.parent_id == 0)
                                            //            {
                                            dssql.Tables[0].Rows[count]["our_shop_Category"] = t.categories_name + ";" + h.categories_name;
                                            //            }
                                            //            else
                                            //            {
                                            //                String qweqwe = "是又怎樣了";
                                            //            }
                                            //        }
                                            //    }
                                        }

                                    }
                                }
                            }
                            //Pchome商店小館
                            dssql.Tables[0].Rows[count]["pchome_shop_Category"] = "H211745";
                            //成本.建議售價，垃圾欄位
                            dssql.Tables[0].Rows[count]["product_sale_Costs"] = Convert.ToInt32(h.products_price);
                            dssql.Tables[0].Rows[count]["product_sale_Suggested"] = Convert.ToInt32(h.products_price);
                            //售價
                            dssql.Tables[0].Rows[count]["product_sale_price"] = Convert.ToInt32(h.products_price);
                            //規格
                            dssql.Tables[0].Rows[count]["product_sale_format"] = null;
                            //現貨與警告值
                            dssql.Tables[0].Rows[count]["product_In_stock"] = 50;
                            dssql.Tables[0].Rows[count]["product_stock_warning"] = 10;
                            //可選擇的付款方式
                            dssql.Tables[0].Rows[count]["payway"] = "ATM";
                            //必填 外部搜尋用 簡短敘述+關鍵字
                            dssql.Tables[0].Rows[count]["product_summary"] = ((h.products_name + "," + h.products_short_description + "," + h.products_meta_keywords).Replace("/", "／")).Replace("\"", "＂");
                            //Html介紹
                            dssql.Tables[0].Rows[count]["product_depiction"] = h.products_description1;
                            //不知道
                            dssql.Tables[0].Rows[count]["product_depiction2"] = null;
                            //公司叫做貨號 官網叫做MODEL 其實他有一個名字叫做SKU
                            dssql.Tables[0].Rows[count]["product_sku_model"] = h.products_model;
                            //最大購買量
                            dssql.Tables[0].Rows[count]["product_maxbuy"] = 50;
                            //上架時間
                            dssql.Tables[0].Rows[count]["product_adddate"] = DateTime.Now.ToShortDateString();
                            //客戶特殊需求 放置官網的出貨時程
                            dssql.Tables[0].Rows[count]["product_custome_special_needs"] = h.shipping_status_name;
                            //圖片名稱，FORMAT掉.JPG 在去圖庫撈
                            if (String.IsNullOrEmpty(h.products_image))
                            {
                                dssql.Tables[0].Rows[count]["product_image"] = "此商品無圖片請找官網上架人員";
                            }
                            else
                            {
                                dssql.Tables[0].Rows[count]["product_image"] = h.products_image;
                                Uri uri = new Uri("http://www.icshop.com.tw/images/product_images/original_images/" + h.products_image);
                                string saveDir = @"C:\Pchome\";    // 注意：資料夾必須已存在
                                string fileName = h.products_image;
                                string savePath = saveDir + fileName;

                                System.Net.WebRequest request = System.Net.WebRequest.Create(uri);
                                System.Net.WebResponse response = request.GetResponse();
                                System.IO.Stream stream = response.GetResponseStream();
                                System.Drawing.Image img = System.Drawing.Image.FromStream(stream);
                                img.Save(savePath);
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
            dv = dssql.Tables["pchometable"].DefaultView;

            GridView1.DataSource = dv;
            GridView1.DataBind();

            Response.Clear();
            Response.Write("<meta http-equiv=Content-Type content=text/html;charset=utf-8>");
            Response.AddHeader("content-disposition",

                "attachment;filename=" + s.ToShortDateString() + "～～" + e.ToShortDateString() + "～～" + ERPUser + "_Pchome日期批次轉檔" + ".xls");

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