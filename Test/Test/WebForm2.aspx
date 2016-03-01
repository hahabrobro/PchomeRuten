<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm2.aspx.cs" Inherits="Test.WebForm2" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false">
            <Columns>
                <asp:BoundField DataField="sku" HeaderText="sku"  />
                <asp:BoundField DataField="base_sku" HeaderText="base_sku" />
                <asp:BoundField DataField="name" HeaderText="name" />
                <asp:BoundField DataField="tagline" HeaderText="tagline" />
                <asp:BoundField DataField="description_1" HeaderText="description_1" />
                <asp:BoundField DataField="price" HeaderText="price" />
                <asp:BoundField DataField="rakuten_product_category_id" HeaderText="rakuten_product_category_id" />
                <asp:BoundField DataField="free_shipping" HeaderText="free_shipping" />
                <asp:BoundField DataField="display_quantity" HeaderText="display_quantity" />
                <asp:BoundField DataField="operator_for_quantity" HeaderText="operator_for_quantity" />
                <asp:BoundField DataField="quantity" HeaderText="quantity"/>
                <asp:BoundField DataField="return_quantity_in_cancel" HeaderText="return_quantity_in_cancel" />
                <asp:BoundField DataField="purchase_quantity_limit" HeaderText="purchase_quantity_limit" />
                <asp:BoundField DataField="shop_product_unique_identifier_1" HeaderText="shop_product_unique_identifier_1" />
            </Columns>
        </asp:GridView>
    </form>
</body>
</html>
