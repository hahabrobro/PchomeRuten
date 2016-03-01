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
    public partial class WebForm1 : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            String DtStart="2016-01-01 00:00:00";//Request["DtStart"];
            String DtEnd = "2016-01-31 23:59:59";//Request["DtENd"];
            String ERPUser = "Sid";
            if ((String.IsNullOrEmpty(DtStart))||(String.IsNullOrEmpty(DtEnd)))
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

            dtsql.TableName = "pchometable";
            //商品名稱(最多64個中文字)商品名稱不能與web上已上架商品相同
            dtsql.Columns.Add("productname");
            //發票名稱(最多20個中文字)若為PChome代開發票者，此欄位內容將是發票品名；若非由PChome代開者，此欄位將無作用但仍請填寫。
            dtsql.Columns.Add("producttitle");
            /*商品館別(店家的小館名稱)
             (至多選3個，以";"隔開)
              如: 店家小館1;店家小館2
              店家小館需由店家先至店家系統後台新增小館才能使用，若未新增在上傳時會失敗。*/
            dtsql.Columns.Add("our_shop_Category");
            /*PChome商店街小館
              (請店家依據商品的屬性，將商品歸類在商店街的小館裏，顧客點入小館中可搜尋到此項商品)
              (商店街小館請參考  http://www.pcstore.com.tw/adm/opt/pexh_list.htm)
              (填入最後一層有H開頭的館別代號，只可填入1個館別代號)
               如:H100076*/
            dtsql.Columns.Add("pchome_shop_Category");
            //成本跟建議售價均售價
            dtsql.Columns.Add("product_sale_Costs");
            dtsql.Columns.Add("product_sale_Suggested");
            /*商品規格(若商品有不同規格，請按「alt+enter」 隔開，若無規格則無需填寫)
              如: 
              紅色
              黃色
              藍色*/
            dtsql.Columns.Add("product_sale_format");
            //商品售價如: 1000
            dtsql.Columns.Add("product_sale_price");
            /*商品庫存量 
              (最大數值為500)
              (若有多種規格的庫存量，請按「alt+enter」與商品規格對應)
               如:
               150
               200
               300*/
            dtsql.Columns.Add("product_In_stock");
            /*最低警告值
              (最大數值為500)
              (若有多種規格的最低警告值，請按「alt+enter」與商品規格對應)
               如 :
               2
               2
               2
             */
            dtsql.Columns.Add("product_stock_warning");
            /*付款別設定(ATM轉帳代碼為:ATM
                     信用卡一次付清:CRD
                     信用卡一般分期付款:DIV
                     信用卡6期分期0利率:Z06
                     信用卡12期分期0利率:Z12
                     超商付款：SEP
                     CCSH貨到現金
                     CCRD貨到信用卡
                     若有複選以";"隔開)如: ATM;CRD;Z12*/
            dtsql.Columns.Add("payway");
            //商品簡介(限125 個中文字)
            dtsql.Columns.Add("product_summary");
            //商品介紹1(字數上限為6000個中文字)
            dtsql.Columns.Add("product_depiction");
            //商品敘述2，空白
            dtsql.Columns.Add("product_depiction2");
            //廠商料號
            dtsql.Columns.Add("product_sku_model");
            //最多限購數量(最多數量為50(1-9、20、30、40、50)，若無填寫，系統會預設最大值 10)
            dtsql.Columns.Add("product_maxbuy");
            //商品上架日(如 2006/01/23)
            dtsql.Columns.Add("product_adddate");
            //顧客特殊需求
            dtsql.Columns.Add("product_custome_special_needs");
            //圖檔名稱(店家可在此欄位填寫商品圖的檔案名稱(不需填寫附檔名)且只限一張圖檔名稱，透過大量上傳程式，系統會將您的圖檔放入商品大圖及小圖中。圖檔類型限gif,jpg)如: RED
            dtsql.Columns.Add("product_image");

            dssql.Tables.Add(dtsql);
            // int i = 0;
            int check = 0;

            //SqlDataSource sds = new SqlDataSource();
            //sds.ProviderName = "MySql.Data.MySqlClient";
            //sds.ConnectionString = "server=61.67.218.137;user id=icshopc_twe;password=9vEqGKKGlh;database=icshopc_icshopc;persistsecurityinfo=True";
            //sds.SelectCommand = "SELECT * FROM icshopc_icshopc.products where products_model='368110800052'";

            //GridView1.DataSource = sds.Select(DataSourceSelectArguments.Empty);
            //GridView1.DataBind();

            //將時間字串轉型
            DateTime s = Convert.ToDateTime(DtStart);
            DateTime e = Convert.ToDateTime(DtEnd);
            //紀錄資料數
            int count = 0;
            //節省資源空間
            using (icshopc_icshopcEntities ent = new icshopc_icshopcEntities())
            {
                //紀錄產品PK碼，等會判斷重複不要撈
                int Products_temp_id=0;
                //Linq 相關參考Navicat SQL
                var v = (from product in ent.products

                         join pd in ent.products_description on product.products_id equals pd.products_id

                         join Ptc in ent.products_to_categories on product.products_id equals Ptc.products_id

                         join Cat in ent.categories on Ptc.categories_id equals Cat.categories_id

                         join Cd in ent.categories_description on Ptc.categories_id equals Cd.categories_id

                         join Ss in ent.shipping_status on product.products_shippingtime equals Ss.shipping_status_id

                         where (product.products_date_added >= s && product.products_date_added < e && Cd.language_id == 1 && Ss.language_id == 1 && !String.IsNullOrEmpty(product.products_model) &&product.products_status == true)

                         select 
                         new 
                         { 
                             product.products_model, product.products_price, product.products_image,product.products_storage_spaces,product.products_id,
                             pd.products_name,pd.products_short_description,pd.products_description1,pd.products_meta_keywords,
                             Cd.categories_name,
                             Ss.shipping_status_name,
                             Cat.parent_id,Cat.categories_id
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
                            dssql.Tables["pchometable"].Rows.Add();

                            //商品名稱 店內搜尋 撈商品名稱+簡短敘述+SKU
                            dssql.Tables[0].Rows[count]["productname"] = ((h.products_name + " - " + h.products_meta_keywords).Replace("/","／")).Replace("\"","＂")+ "‧" + h.products_model + "‧";
                            //商品抬頭 商品名稱
                            dssql.Tables[0].Rows[count]["producttitle"] = (h.products_name.Replace("/", "／")).Replace("\"", "＂");

                            /*商品館別(商品分類過濾)
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
                                            using(icshopc_icshopcEntities2 ent2=new icshopc_icshopcEntities2())
                                            {
                                                var three = from Cat in ent2.categories
                                                            join Cd in ent2.categories_description on Cat.categories_id equals Cd.categories_id
                                                            where Cat.categories_id == t.parent_id && Cd.language_id == 1
                                                            select new { Cat.parent_id, Cd.categories_name, Cat.categories_id };
                                                foreach(var zz in three)
                                                {
                                                    if(zz.parent_id==0)
                                                    {
                                                        dssql.Tables[0].Rows[count]["our_shop_Category"] = t.categories_name;
                                                    }
                                                    else
                                                    {
                                                        using(icshopc_icshopcEntities3 ent3=new icshopc_icshopcEntities3())
                                                        {
                                                            var four = from Cat in ent3.categories
                                                                       join Cd in ent3.categories_description on Cat.categories_id equals Cd.categories_id
                                                                       where Cat.categories_id == zz.parent_id && Cd.language_id == 1
                                                                       select new { Cat.parent_id, Cd.categories_name, Cat.categories_id };
                                                            foreach(var xx in four)
                                                            {
                                                                if(xx.parent_id==0)
                                                                {
                                                                    dssql.Tables[0].Rows[count]["our_shop_Category"] = zz.categories_name;
                                                                }
                                                                else
                                                                {
                                                                    using(icshopc_icshopcEntities4 ent4=new icshopc_icshopcEntities4())
                                                                    {
                                                                        var five = from Cat in ent4.categories
                                                                                   join Cd in ent4.categories_description on Cat.categories_id equals Cd.categories_id
                                                                                   where Cat.categories_id == xx.parent_id && Cd.language_id == 1
                                                                                   select new { Cat.parent_id, Cd.categories_name, Cat.categories_id };
                                                                        foreach(var cc in five)
                                                                        {
                                                                            if(cc.parent_id==0)
                                                                            {
                                                                                dssql.Tables[0].Rows[count]["our_shop_Category"] = xx.categories_name;
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
                            String shippingtime = "<span style=\"color: rgb(255, 50, 65); font-family: 'Times New Roman'; font-size: 16px; font-weight: bold;\">" + h.shipping_status_name + "</span></br>";
                            dssql.Tables[0].Rows[count]["product_depiction"] = shippingtime + h.products_description1;
                            //不知道
                            dssql.Tables[0].Rows[count]["product_depiction2"] = null;
                            //公司叫做貨號 官網叫做MODEL ERP叫做ProSeq 其實他有一個名字叫做SKU
                            dssql.Tables[0].Rows[count]["product_sku_model"] = h.products_model;
                            //最大購買量
                            dssql.Tables[0].Rows[count]["product_maxbuy"] = 50;
                            //上架時間
                            dssql.Tables[0].Rows[count]["product_adddate"] = DateTime.Now.ToShortDateString();
                            //客戶特殊需求 放置官網的出貨時程
                            dssql.Tables[0].Rows[count]["product_custome_special_needs"] = null;
                            //圖片名稱，FORMAT掉.JPG 在去圖庫撈
                            if (String.IsNullOrEmpty(h.products_image))
                            {
                                dssql.Tables[0].Rows[count]["product_image"] = "此商品無圖片請找官網上架人員";
                            }
                            else
                            {
                                dssql.Tables[0].Rows[count]["product_image"] = h.products_image;
                                Uri uri = new Uri("http://www.icshop.com.tw/images/product_images/original_images/" + h.products_image);
                                string saveDir = @"C:\Pchome\"+s.ToString("yyyy-MM")+"\\";    // 注意：資料夾必須已存在
                                string fileName = h.products_image;
                                string savePath = saveDir + fileName;

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
            dv = dssql.Tables["pchometable"].DefaultView;

            GridView1.DataSource = dv;
            GridView1.DataBind();

            Response.Clear();
            Response.Write("<meta http-equiv=Content-Type content=text/html;charset=utf-8>");
            Response.AddHeader("content-disposition",

                "attachment;filename=" + s.ToShortDateString() + "～～" + e.ToShortDateString() + "～～"  +ERPUser + "_Pchome日期批次轉檔" + ".xls");

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
            catch(Exception ex)
            {
                Response.Write("區間內無資料"+s.ToShortDateString()+"-----"+e.ToShortDateString());
                
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