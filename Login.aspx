<%@ Page Title="Login" Language="C#" MasterPageFile="~/Login.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Pura_Gaji_Viewer.Login" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main aria-labelledby="title">
        <div class="container d-flex justify-content-center align-items-center">
            <div class="text-center center-block">
                <h2 id="title"><%: Title %><br/>Puragaji</h2>
                <div runat="server" id="warningDiv" style="max-width: 500px" class="center-block form-group alert alert-dismissible alert-danger" visible=false>
                    Username atau Password salah
                </div>
                <div class="form-group mb-3">
                    <asp:label runat="server" class="control-label" for="inputUsername">Username</asp:label>
                    <asp:Textbox runat="server" id="inputUsername" class="center-block form-control text-center"/>
                </div>
                <div class="form-group mb-3">
                    <asp:label runat="server" class="control-label" for="inputPassword">Password</asp:label>
                    <asp:Textbox runat="server" id="inputPassword" TextMode="Password" class="center-block form-control text-center"/>
                </div>
                <div class="form-group mb-3">
                    <asp:label runat="server" class="control-label" for="inputDepartemen">Departemen</asp:label>
                    <asp:Textbox runat="server" id="inputDepartemen" class="center-block form-control text-center"/>
                </div>
                <div class="form-group mb-3">
                    <asp:Button id="btnLogin" runat="server" Text="Login!" CssClass="btn btn-primary center-block" OnClick="btnLogin_Click"/>
                </div>
            </div>
        </div>
    </main>
</asp:Content>
