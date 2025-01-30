<%@ Page Title="Laporan" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Laporan.aspx.cs" Inherits="Pura_Gaji_Viewer.Gaji" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main aria-labelledby="title">        
        <div class="container d-flex justify-content-center align-items-center min-vh-100">
            <div class="text-center">
                <!--=========== TITLE AND INSTRUCTION ==========-->
                <h1 id="title"><%: Title %></h1>
                <h4>Silahkan pilih laporan yang ingin ditampilkan</h4>

                <!--=========== ALERTS ==========-->
                <div runat="server" id="warningPilihLaporan" 
                     style="max-width: 300px; display: none;" 
                     class="center-block form-group alert alert-danger mx-auto">
                    Warning belum pilih Laporan
                </div>
                <div runat="server" id="successPilihLaporan" 
                     style="max-width: 300px; display: none;" 
                     class="center-block form-group alert alert-success mx-auto">
                    Laporan Berhasil dipilih!
                </div>

                <!--=========== DROPDOWN AND BUTTON ==========-->
                <div class="form-group center-block mb-3">
                    <asp:DropDownList ID="dropdownLaporan" runat="server" 
                                      Enabled="true" CssClass="form-control mx-auto center-block"></asp:DropDownList>
                </div>
                <div class="center-block form-group">
                    <asp:Button ID="btnPilihLaporan" runat="server" 
                                Enabled="true" Text="Pilih!" 
                                CssClass="btn btn-primary mx-auto" 
                                OnClick="btnPilihLaporan_Click" />
                </div>

                <!--=========== FORM PANEL ==========-->
                <div runat="server" id="formLaporan" 
                     style="display: none; max-width: 500px;" 
                     class="form-group mt-4 mb-3 jumbotron mx-auto center-block">
                    <asp:Label ID="judulForm" runat="server" 
                               Text="Judul Laporan" 
                               CssClass="text-primary" Font-Size="XX-Large"></asp:Label>

                    <!-- ALERTS INSIDE FORM -->
                    <div runat="server" id="warningForm" 
                         style="display: none; text-align: center;" 
                         class="center-block form-group alert alert-danger">
                        Inner text warning
                    </div>
                    <div runat="server" id="successForm" 
                         style="display: none; text-align: center;" 
                         class="center-block form-group alert alert-success">
                        Laporan Berhasil ditunjukkan!
                    </div>

                    <!-- DYNAMIC FORM -->
                    <asp:PlaceHolder ID="phDynamicForm" runat="server"></asp:PlaceHolder>

                    <!-- DYNAMIC CONTROL PANEL -->
                    <asp:Panel ID="dynamicControlsPanel" runat="server" CssClass="dynamic-panel"></asp:Panel>

                    <!-- PROGRESS BAR FOR DOWNLOAD -->
                    <div runat="server" class="progress mt-3" style="height: 25px; display: none;" id="downloadProgressBar">
                        <div runat="server" class="progress-bar" 
                             id="progressBar"
                             role="progressbar" 
                             style="width: 0%;" 
                             aria-valuenow="0" 
                             aria-valuemin="0" 
                             aria-valuemax="100">
                            0%
                        </div>
                    </div>

                    <!-- PROGRESS LABEL -->
                    <div runat="server" class="mt-2" id="progressBarLabel" style="display: none; text-align: center;">
                        <span>Menyiapkan Laporan, harap tunggu...</span>
                    </div>

                    <div class="form-group mb-3">
                        <asp:Button ID="btnLihat" runat="server" 
                                    Text="Lihat Tabel!" 
                                    CssClass="btn btn-primary mx-auto" 
                                    OnClick="btnLihat_Click" />
                    </div>
                </div>
                <!-- Preview Panel for Table -->
                <asp:Panel ID="reportPreviewPanel" runat="server" style="display: none" CssClass="preview-panel">
                   
                    <h3 class="text-center" id="previewTitle" runat="server"></h3>

                    <div class="text-center mb-3 form-group">
                        <asp:Button ID="btnDownloadPDF" runat="server"
                                    Text="Download PDF" 
                                    CssClass="btn btn-success" 
                                    OnClick="btnDownload_Click" />
                    </div>

                    <div class="table-responsive">
                        <asp:Literal ID="tablePreviewLiteral" runat="server"></asp:Literal>
                    </div>

                </asp:Panel>
            </div>
        </div>
    </main>
</asp:Content>
