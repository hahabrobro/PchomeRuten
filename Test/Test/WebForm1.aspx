<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" ResponseEncoding="UTF-8" Inherits="Test.WebForm1" %>

<!DOCTYPE html>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Import Excel to Gridview - Dotnetspan.com</title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false">
            <Columns>
                <asp:BoundField DataField="productname" HeaderText="商品名稱(最多64個中文字)商品名稱不能與web上已上架商品相同" />
                <asp:BoundField DataField="producttitle" HeaderText="發票名稱(最多20個中文字)
若為PChome代開發票者，此欄位內容將是發票品名；若非由PChome代開者，此欄位將無作用但仍請填寫。" />
                <asp:BoundField DataField="our_shop_Category" HeaderText="商品館別(店家的小館名稱)" />
                <asp:BoundField DataField="pchome_shop_Category" HeaderText="PChome商店街小館
(請店家依據商品的屬性，將商品歸類在商店街的小館裏，顧客點入小館中可搜尋到此項商品)
(商店街小館請參考  http://www.pcstore.com.tw/adm/opt/pexh_list.htm)
(填入最後一層有H開頭的館別代號，只可填入1個館別代號)
如:H100076" />
                <asp:BoundField DataField="product_sale_Costs" HeaderText="商品成本如: 360" />
                <asp:BoundField DataField="product_sale_Suggested" HeaderText="商品建議售價如: 1200" />
                <asp:BoundField DataField="product_sale_price" HeaderText="商品售價如: 1000" />
                <asp:BoundField DataField="product_sale_format" HeaderText="商品規格(若商品有不同規格，請按「alt+enter」 隔開，若無規格則無需填寫)如: 紅色黃色藍色" />
                <asp:BoundField DataField="product_In_stock" HeaderText="商品庫存量 (最大數值為500)(若有多種規格的庫存量，請按「alt+enter」與商品規格對應)如:150 200 300" />
                <asp:BoundField DataField="product_stock_warning" HeaderText="最低警告值
(最大數值為500)
(若有多種規格的最低警告值，請按「alt+enter」與商品規格對應)
如 :
2
2
2" />
                <asp:BoundField DataField="payway" HeaderText="付款別設定"/>
                <asp:BoundField DataField="product_summary" HeaderText="商品簡介(限125 個中文字)" />
                <asp:BoundField DataField="product_depiction" HeaderText="商品介紹1(字數上限為6000個中文字)" />
                <asp:BoundField DataField="product_depiction2" HeaderText="商品介紹2(限1000 個中文字)" />
                <asp:BoundField DataField="product_sku_model" HeaderText="廠商料號" />
                <asp:BoundField DataField="product_maxbuy" HeaderText="最多限購數量(最多數量為50(1-9、20、30、40、50)，若無填寫，系統會預設最大值 10)" />
                <asp:BoundField DataField="product_adddate" HeaderText="商品上架日(如 2006/01/23)" />
                <asp:BoundField DataField="product_custome_special_needs" HeaderText="顧客特殊需求" />
                <asp:BoundField DataField="product_image" HeaderText="圖檔名稱" />

            </Columns>
        </asp:GridView>
        <br />
    </form>
</body>
</html>
