<%@ Page Title="Master" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Master.aspx.cs" Inherits="Pura_Gaji_Viewer.Master" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main aria-labelledby="title">
        <h2 class="text-center" id="title"><%: Title %></h2>
        <div>
            <p>SILAHKAN PILIH MASTER YANG AKAN DIPILIH</p>
            <div class ="m-auto">
                <select class="form-control" id="select">
                    <option>1</option>
                    <option>2</option>
                    <option>3</option>
                    <option>4</option>
                    <option>5</option>
                </select>
            </div>            
        </div>
    </main>
</asp:Content>
