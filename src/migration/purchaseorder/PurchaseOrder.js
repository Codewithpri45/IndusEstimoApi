"use strict";
var GBLProductionUnitID = getProductionUnitID('ProductionUnitID');
var GBLCompanyID = getProductionUnitID('CompanyID');
var SelectedProductionUnitID = 0;
var SelectedProductionUnitName = "";
var GridSelectedData = [];
var prefix = "PO", GblStatus = "";
var SupplierData = [], newSUPllierArray = [], SupplierDDL = [];
var VarItemApproved = "", Groupdata = "", GetPendingData = "";
var ObjItemRate = [], ItemRateString = "";

var existReq = []; var MasterGridData = []; //PendingMasterGridData
var SubGridData = [];   //PendingSubGridData

//For Grid Calculation
var GetRow = "", GblCompanyStateTin = "";
var GblGSTApplicable = true;

//Schdule Grid
var ScheduleListOBJ = []; var DistinctArray = []; var RemID = "";

//OtherHeads
var OtherHead = [];

//Additional Charges
var ChargesGrid = []; var updateTotalTax = 0; var TotalGstAmt = 0, FrmUpdateTotalGstAmt = 0;

//Terms Of Payment
var PaymentTermsGrid = []; var optTerms = {}; var PaymentTermsString = "Payment in 30 Days,Payment in 60 Days,Payment in 90 Days";
var GblJobCardRES = []; //added by pKp for job card selection in create PO grid
var GblClientName = []; //added by Yash for job card selection in create PO grid
var FlagEditPurchaseRate = false; //Admin only can change the rate

var GblUnitConversionFormula = [];
let GblCompanyConfiguration = [];
let purchaseConfiguration = [];

var PurchaseDivisionText = ["COM", "CRD", "EXP"];
var ModeOfTransportText = ["By Road", "By Rail", "By Air", "By Ocean", "Other"];
var priorities = ["Pending Requisitions", "Purchase Orders"];
var GetOverFlowGrid = [];
// Initialize the current date
var currentDate = new Date();
let ProcessedGridSelectData = [];
// Subtract 7 days from the current date
var sevenDaysAgo = new Date(currentDate);
sevenDaysAgo.setDate(currentDate.getDate() - 15);
let validateUserData = { moduleName: "", userName: "", password: "", actionType: "Update", RecordID: 0, transactionRemark: "", isUserInfoFilled: false, documentNo: "" };

var selectedRowsDtForSchedule = [];

var StkUnit = [{
    "ID": 1,
    "Name": "Unit"
},
{
    "ID": 0,
    "Name": "Kg"
},
{
    "ID": 2,
    "Name": "Sheets"
}, {
    "ID": 3,
    "Name": "Sheet"
}
];

var queryString = new Array();
$(function () {

    if (queryString.length === 0) {
        if (window.location.search.split('?').length > 1) {
            var params = window.location.search.split('?')[1].split('&');
            for (var i = 0; i < params.length; i++) {
                var key = params[i].split('=')[0];
                var value = decodeURIComponent(params[i].split('=')[1]).replace(/"/g, '');
                queryString[key] = value;
            }
        }
    }

    if (queryString["TransactionID"] !== null && queryString["TransactionID"] !== undefined) {
        let TransactionID = Number(queryString["TransactionID"]);
        if (TransactionID <= 0) return;
        document.getElementById("TxtPOID").value = TransactionID;
        $("#EditPOButton").click();

        document.getElementById("BtnNew").disabled = true;
        document.getElementById("BtnSave").disabled = true;
        document.getElementById("BtnSaveAS").disabled = true;
        document.getElementById("POPrintButton").disabled = true;
        document.getElementById("BtnDeletePopUp").disabled = true;
        document.getElementById("BtnopenPop").disabled = true;
        $("#MainIndex").hide();
    };
});


//Load Panel Setting
$("#LoadIndicator").dxLoadPanel({
    shadingColor: "rgba(0,0,0,0.4)",
    indicatorSrc: "images/Indus Logo.png",
    message: "Loading ...",
    width: 320,
    showPane: true,
    showIndicator: true,
    shading: true,
    closeOnOutsideClick: false,
    visible: false
});


//Edit condition as per admin authority
/*$.ajax({
    type: "POST",
    async: false,
    url: "WebService_PurchaseOrder.asmx/CheckIsAdmin",
    data: '{}',
    contentType: "application/json; charset=utf-8",
    dataType: "json",
    success: function (results) {
        if (results.d === "True") {
            FlagEditPurchaseRate = true;
        } else
            FlagEditPurchaseRate = false;
    }
});*/

//Load the default purchase configurations
$.ajax({
    type: "POST",
    async: false,
    url: "WebServiceOthers.asmx/GetDefaultPurchaseConfigurations",
    data: '{}',
    contentType: "application/json; charset=utf-8",
    dataType: "text",
    success: function (results) {
        let rs = results.replace(/\\/g, '');
        rs = rs.replace(/"d":""/g, '');
        rs = rs.replace(/""/g, '');
        rs = rs.replace(/u0027/g, "'");
        rs = rs.replace(/u0026/g, "&");
        rs = rs.replace(/:,/g, ":null,");
        rs = rs.replace(/,}/g, ",null}");
        rs = rs.substr(1);
        rs = rs.slice(0, -1);
        purchaseConfiguration = JSON.parse(rs);

        (purchaseConfiguration.PurchaseDivision.length > 0) ? PurchaseDivisionText = purchaseConfiguration.PurchaseDivision.map(function (el) { return el.PurchaseDivision; }) : PurchaseDivisionText = ["COM", "CRD", "EXP"];
        (purchaseConfiguration.PaymentTerms.length > 0) ? PaymentTermsString = purchaseConfiguration.PaymentTerms.map(function (el) { return el.PaymentTerms; }).join(',') : PaymentTermsString = "Payment in 30 Days,Payment in 60 Days,Payment in 90 Days";
        (purchaseConfiguration.TransportMode.length > 0) ? ModeOfTransportText = purchaseConfiguration.TransportMode.map(function (el) { return el.TransportMode; }) : ModeOfTransportText = ["By Road", "By Rail", "By Air", "By Ocean", "Other"];
    }
});


$.ajax({
    type: "POST",
    async: false,
    url: "WebService_PurchaseRequisition.asmx/GetJobCardList",
    data: '{}',
    contentType: "application/json; charset=utf-8",
    dataType: "text",
    success: function (results) {
        var res = results.replace(/\\/g, '');
        res = res.replace(/"d":""/g, '');
        res = res.replace(/""/g, '');
        res = res.replace(/u0027/g, "'");
        res = res.replace(/u0026/g, "&");
        res = res.replace(/:,/g, ":null,");
        res = res.replace(/,}/g, ",null}");
        res = res.substr(1);
        res = res.slice(0, -1);
        GblJobCardRES = JSON.parse(res);

    }
});
var JobCardStore = new DevExpress.data.CustomStore({
    key: "RefJobBookingJobCardContentsID",
    loadMode: "raw",
    load: function (loadOptions) {
        return new Promise(function (resolve) {
            let allData = GblJobCardRES || [];

            if (loadOptions.searchValue) {
                let searchStr = loadOptions.searchValue.toLowerCase();
                let filteredData = allData.filter(x =>
                    (x.RefJobCardContentNo || "").toLowerCase().includes(searchStr)
                );
                resolve(filteredData);
            } else {
                resolve(allData.slice(0, 1000));
            }
        });
    }
});

$.ajax({
    type: "POST",
    async: false,
    url: "WebService_PurchaseRequisition.asmx/GetClientList",
    data: '{}',
    contentType: "application/json; charset=utf-8",
    dataType: "text",
    success: function (results) {
        var res = results.replace(/\\/g, '');
        res = res.replace(/"d":""/g, '');
        res = res.replace(/""/g, '');
        res = res.replace(/u0027/g, "'");
        res = res.replace(/u0026/g, "&");
        res = res.replace(/:,/g, ":null,");
        res = res.replace(/,}/g, ",null}");
        res = res.substr(1);
        res = res.slice(0, -1);
        GblClientName = JSON.parse(res);
    }
});

//Tax Configuration Setting
$.ajax({
    type: "POST",
    async: false,
    url: "WebServiceOthers.asmx/GetCompanyConfigurations",
    data: '{}',
    contentType: "application/json; charset=utf-8",
    dataType: "json",
    success: function (results) {
        let res1 = results.d.replace(/\\/g, '');
        res1 = res1.replace(/"d":""/g, '');
        res1 = res1.replace(/""/g, '');
        res1 = res1.replace(/u0027/g, "'");
        res1 = res1.replace(/u0026/g, "&");
        res1 = res1.replace(/:,/g, ":null,");
        res1 = res1.replace(/,}/g, ",null}");
        res1 = res1.replace(/:}/g, ":null}");
        res1 = res1.substr(1);
        res1 = res1.slice(0, -1);
        GblCompanyConfiguration = JSON.parse(res1);
        //GblCompanyConfiguration
        if (GblCompanyConfiguration.length > 0) {
            if (GblCompanyConfiguration[0].CanEditPOQuantityAndRate === 1 || GblCompanyConfiguration[0].CanEditPOQuantityAndRate === true) {
                FlagEditPurchaseRate = true;
            } else {
                FlagEditPurchaseRate = false;
            }
        }
    }
});

$("#BtnOpenProductHSNPopUp").click(function () {
    document.getElementById("BtnOpenProductHSNPopUp").setAttribute("data-toggle", "modal");
    document.getElementById("BtnOpenProductHSNPopUp").setAttribute("data-target", "#largeModalHSNGroup");
});

//// init datagrid
var ProductHSNGridRES1 = [], SelectedProductHSNList = [], GetPOGridRow = "";
$("#ProductHSNGrid").dxDataGrid({
    dataSource: ProductHSNGridRES1,
    columnAutoWidth: true,
    showBorders: true,
    showRowLines: true,
    allowColumnReordering: true,
    //allowColumnResizing: true,
    paging: {
        pageSize: 15
    },
    pager: {
        showPageSizeSelector: true,
        allowedPageSizes: [15, 25, 50, 100]
    },
    height: function () {
        return window.innerHeight / 1.3;
    },
    selection: { mode: "single" },
    grouping: {
        autoExpandAll: true
    },
    filterRow: { visible: true, applyFilter: "auto" },
    //columnChooser: { enabled: true },
    headerFilter: { visible: true },
    //rowAlternationEnabled: true,
    searchPanel: { visible: true },
    loadPanel: {
        enabled: true,
        height: 90,
        width: 200,
        text: 'Data is loading...'
    },
    export: {
        enabled: true,
        fileName: "Department Master",
        allowExportSelectedData: true,
    },
    onRowPrepared: function (e) {
        if (e.rowType === "header") {
            e.rowElement.css('background', '#42909A');
            e.rowElement.css('color', 'white');
        }
        e.rowElement.css('fontSize', '11px');
    },
    onSelectionChanged: function (Sel) {
        var data = Sel.selectedRowsData;
        SelectedProductHSNList = [];
        SelectedProductHSNList = Sel.selectedRowsData;
    },
    columns: [
        { dataField: "ProductHSNID", visible: false, caption: "ProductHSNID" },
        { dataField: "HSNCode", visible: true, caption: "HSNCode", width: 80 },
        { dataField: "ProductHSNName", visible: true, caption: "ProductHSNName", width: 280 },
        { dataField: "GSTTaxPercentage", visible: true, caption: "GSTTaxPercentage", width: 80 },
        { dataField: "CGSTTaxPercentage", visible: true, caption: "CGSTTaxPercentage", width: 80 },
        { dataField: "SGSTTaxPercentage", visible: true, caption: "SGSTTaxPercentage", width: 80 },
        { dataField: "IGSTTaxPercentage", visible: true, caption: "IGSTTaxPercentage", width: 80 }
    ]
});


$("#CreatePOGrid").dxDataGrid({
    keyExpr: "ItemID",
    showBorders: true,
    paging: {
        enabled: false
    },
    height: function () {
        return window.innerHeight / 3.2;
    },
    columnAutoWidth: true,
    showRowLines: true,
    allowSorting: false,
    allowColumnResizing: true,
    columnResizingMode: "widget",
    wordWrapEnabled: true,
    sorting: {
        mode: "none" // or "multiple" | "single"
    },
    editing: {
        mode: "cell",
        allowDeleting: true,
        //allowAdding: true,
        allowUpdating: true,
        useIcons: true,
    },
    //onRowUpdated: function (e) {
    //    var grid = $('#CreatePOGrid').dxDataGrid('instance');
    //    grid.refresh();
    //},
    onRowPrepared: function (e) {
        if (e.rowType === "header") {
            e.rowElement.css('background', '#509EBC');
            e.rowElement.css('color', 'white');
            e.rowElement.css('font-weight', 'bold');
        }
        e.rowElement.css('fontSize', '11px');
    },
    onRowRemoved: function (e) {
        if (ScheduleListOBJ !== "" && ScheduleListOBJ !== [] && ScheduleListOBJ !== undefined && ScheduleListOBJ !== null) {
            ScheduleListOBJ = ScheduleListOBJ.filter(function (obj) {
                return obj.ItemID !== e.data.ItemID;
            });
        }
        //existReq = existReq.filter(function (obj) {
        //    return obj.ItemID !== e.data.ItemID;
        //});
        if (ChargesGrid.length > 0) {
            AddItemCalculation();
            GridColumnCal();
            //AddItemWithChargessGrid();
            CalculateAmount();
        } else {
            AddItemCalculation();
        }
        SubGridData = SubGridData.filter(function (objReq) {
            return objReq.ItemID !== e.data.ItemID;
        });
    },
    //onEditingStart: function (e) {
    //    if (e.column.visibleIndex < 14 || e.column.visibleIndex === 15 || e.column.visibleIndex === 17 || e.column.visibleIndex === 18 || e.column.visibleIndex === 19 || e.column.visibleIndex === 23 || e.column.visibleIndex === 25 || e.column.visibleIndex === 26 || e.column.visibleIndex === 27 || e.column.visibleIndex === 28 || e.column.visibleIndex === 29 || e.column.visibleIndex === 30 || e.column.visibleIndex === 31 || e.column.visibleIndex === 32 || e.column.visibleIndex === 33 || e.column.visibleIndex === 34 || e.column.visibleIndex === 39 || e.column.visibleIndex === 40 || e.column.visibleIndex === 47) {
    //        e.cancel = true;
    //    }
    //},
    onRowClick: function (e) {
        $(".dx-row").css("background", "");
        $(e.rowElement).css("background", "#87CEFA");
    },
    onCellClick: function (AddClick) {
        if (AddClick.rowType !== "data") return false;

        if (AddClick.column.visibleIndex === 40) {
            GetRow = AddClick.rowIndex;

            if (ScheduleListOBJ === [] || ScheduleListOBJ === "" || ScheduleListOBJ === undefined || ScheduleListOBJ === null) {
                DistinctArray = [];
            } else {
                var MakeArray = { 'ExistRec': ScheduleListOBJ };
                DistinctArray = [];
                DistinctArray = MakeArray.ExistRec.filter(function (el) {
                    return el.ItemID === AddClick.data.ItemID;
                });
            }

            $("#ScheduleGrid").dxDataGrid({
                dataSource: DistinctArray
            });

            document.getElementById("SchQtyLbl").innerHTML = "";
            document.getElementById("TxtUnitSch").value = "";
            document.getElementById("SchDelDateLbl").innerHtml = "";
            document.getElementById("SchItemIDLbl").innerHtml = "";
            document.getElementById("SchItemCodeLbl").innerHtml = "";
            document.getElementById("TxtQtySch").innerHtml = "";

            document.getElementById("SchQtyLbl").innerHtml = AddClick.data.PurchaseQuantity;
            document.getElementById("TxtPurchaseQtySch").value = AddClick.data.PurchaseQuantity;
            document.getElementById("TxtUnitSch").value = AddClick.data.PurchaseUnit;
            document.getElementById("SchDelDateLbl").innerHtml = AddClick.data.ExpectedDeliveryDate;
            document.getElementById("SchItemIDLbl").innerHtml = AddClick.data.ItemID;
            document.getElementById("SchItemCodeLbl").innerHtml = AddClick.data.ItemCode;
            //  }
        }

        if (AddClick.column.dataField === "ProductHSNName") {
            GetPOGridRow = AddClick.rowIndex;
            // $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
            $.ajax({
                async: false,
                type: "POST",
                url: "WebService_PurchaseOrder.asmx/GetAllHSN",
                data: '{}',
                contentType: "application/json; charset=utf-8",
                dataType: "text",
                success: function (results) {
                    var res = results.replace(/\\/g, '');
                    res = res.replace(/"d":""/g, '');
                    res = res.replace(/""/g, '');
                    res = res.replace(/u0026/g, '&');
                    res = res.replace(/u0027/g, "'");
                    res = res.replace(/:,/g, ":null,");
                    res = res.replace(/,}/g, ",null}");
                    res = res.substr(1);
                    res = res.slice(0, -1);
                    //   $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                    ProductHSNGridRES1 = [];
                    ProductHSNGridRES1 = JSON.parse(res);

                    $("#ProductHSNGrid").dxDataGrid({
                        dataSource: ProductHSNGridRES1
                    });
                }
            });

            $("#BtnOpenProductHSNPopUp").click();
        }
    },
    onRowUpdated: function (editcell) {
        let reqDate = $("#VoucherDate").dxDateBox("instance").option("value");
        if (reqDate) {
            //// Convert DateBox value to a Date object
            //let reqDate1 = new Date(reqDate);
            //// Strip time component from reqDate1
            //reqDate1 = new Date(reqDate1.getFullYear(), reqDate1.getMonth(), reqDate1.getDate());

            //if (editcell.data.ExpectedDeliveryDate) {
            //    let expDate = new Date(editcell.data.ExpectedDeliveryDate);
            //    // Strip time component from expDate
            //    expDate = new Date(expDate.getFullYear(), expDate.getMonth(), expDate.getDate());

            //    if (expDate < reqDate1) {
            //        //DevExpress.ui.notify("Expected delivery date should not be less than the purchase order date.", "warning", 1500);
            //        DevExpress.ui.notify({
            //            message: "Expected delivery date should not be less than the purchase order date.", type: "warning", displayTime: 5000, width: "900px",
            //            onContentReady: function (e) {
            //                e.component.$content().find(".dx-toast-message").css({
            //                    "font-size": "13px",
            //                    "font-weight": "bold",
            //                });
            //                const closeButton = $("<div>")
            //                    .addClass("dx-notification-close")
            //                    .text("×")
            //                    .css({
            //                        "position": "absolute",
            //                        "top": "5px",
            //                        "right": "5px",
            //                        "cursor": "pointer",
            //                        "font-size": "25px",
            //                    })
            //                    .appendTo(e.component.$content());
            //                closeButton.on("click", function () {
            //                    e.component.hide();
            //                });
            //            }
            //        });
            //        editcell.data.ExpectedDeliveryDate = reqDate1; // Ensure it's set as a Date object
            //    }
            //}

            let poDate = new Date(reqDate);
            poDate.setHours(0, 0, 0, 0); // Remove time component

            if (editcell.data.ExpectedDeliveryDate) {
                let deliveryDate = new Date(editcell.data.ExpectedDeliveryDate);
                deliveryDate.setHours(0, 0, 0, 0); // Remove time component

                if (deliveryDate < poDate) {
                    DevExpress.ui.notify({
                        message: "Expected delivery date should not be earlier than the purchase order date.",
                        type: "warning",
                        displayTime: 5000,
                        width: "900px",
                        onContentReady: function (e) {
                            e.component.$content().find(".dx-toast-message").css({
                                "font-size": "13px",
                                "font-weight": "bold",
                            });
                        }
                    });

                    editcell.data.ExpectedDeliveryDate = poDate; // Set it to the PO date
                }
            }
        }

        if (ChargesGrid.length > 0) {
            AddItemCalculation();
            GridColumnCal();
            //AddItemWithChargessGrid();
            CalculateAmount();
            SupplierWiseCharges();
        } else {
            AddItemCalculation();
            GridColumnCal();
            SupplierWiseCharges();
        }
    },
    onCellPrepared: function (CEll) {
        GridColumnCal();
    },
    columns: [
        { dataField: "TransactionID", visible: false, caption: "TransactionID", width: 120 },
        { dataField: "TransID", visible: false, caption: "TransID", width: 120 },
        { dataField: "VoucherID", visible: false, caption: "VoucherID", width: 120 },
        { dataField: "ItemGroupID", visible: false, caption: "ItemGroupID", width: 120 },
        { dataField: "MaxVoucherNo", visible: false, caption: "MaxVoucherNo", width: 120 },
        { dataField: "ItemID", visible: false, caption: "ItemID", width: 120 },
        { dataField: "VoucherNo", visible: false, caption: "RequisitionNo", width: 120 },
        { dataField: "VoucherDate", visible: false, caption: "Requisition Date", width: 120 },
        { dataField: "ItemGroupName", visible: true, allowEditing: false, caption: "Group Name" },
        { dataField: "ItemCode", visible: true, allowEditing: false, caption: "Item Code", width: 80 },
        { dataField: "ItemName", visible: true, allowEditing: false, caption: "Item Name", width: 200 },
        ///RequiredQuantity In Stock Unit
        { dataField: "RequiredQuantity", visible: true, allowEditing: false, caption: "Req.Qty (In S.U.)", width: 80 },
        { dataField: "StockUnit", visible: true, allowEditing: false, caption: "Stock Unit", width: 80 },
        //Added field : RequiredQuantityInPurchaseUnit By Minesh Jain On 01-Oct-2019
        { dataField: "RequiredQuantityInPurchaseUnit", visible: true, allowEditing: false, caption: "Req.Qty (In P.U.)", width: 80 },

        {
            dataField: "RequiredNoOfPacks", visible: true, allowEditing: true, width: 80, caption: "No. Of Packs|Rolls)", validationRules: [{ type: "required" }, { type: "numeric" }],
            setCellValue: function (newData, value, currentRowData) {
                if (value === null || value === undefined || isNaN(value) === true) return false;
                newData.RequiredNoOfPacks = value;
                let newPurchaseData = calculatePurchaseQuantity(newData, value, currentRowData)

                newData.RequiredNoOfPacks = value;
                newData.QuantityPerPack = newPurchaseData.QuantityPerPack;
                newData.PurchaseQuantity = newPurchaseData.PurchaseQuantity;
                newData.PurchaseQuantityInStockUnit = newPurchaseData.PurchaseQuantityInStockUnit;

            }

        },
        {
            dataField: "QuantityPerPack", visible: true, allowEditing: true, caption: "Qty / (Pack|Roll))", validationRules: [{ type: "required" }, { type: "numeric" }], width: 80,
            setCellValue: function (newData, value, currentRowData) {
                if (value === null || value === undefined || isNaN(value) === true) return false;
                newData.QuantityPerPack = value;
                let newPurchaseDate = calculatePurchaseQuantity(newData, value, currentRowData)
                newData.QuantityPerPack = value;
                newData.RequiredNoOfPacks = newPurchaseDate.RequiredNoOfPacks;
                newData.PurchaseQuantity = newPurchaseDate.PurchaseQuantity;
                newData.PurchaseQuantityInStockUnit = newPurchaseDate.PurchaseQuantityInStockUnit;

            }
        },

        {
            dataField: "PurchaseQuantity", visible: true, allowEditing: true, caption: "P.O.Qty (In P.U.)", width: 60,
            validationRules: [{ type: "required" }, { type: "numeric" }],
            setCellValue: function (newData, value, currentRowData) {
                if (value === null || value === undefined || isNaN(value) === true) return false;
                newData.PurchaseQuantity = Number(value);
                let newPurchaseData = calculatePurchaseQuantity(newData, value, currentRowData)

                newData.PurchaseQuantity = value;
                newData.RequiredNoOfPacks = newPurchaseData.RequiredNoOfPacks;
                newData.QuantityPerPack = newPurchaseData.QuantityPerPack;
                newData.PurchaseQuantityInStockUnit = newPurchaseData.PurchaseQuantityInStockUnit;
            }

            //setCellValue: function (newData, value, currentRowData) {
            //    newData.PurchaseQuantity = value;
            //    //newData.TotalPrice = currentRowData.Price * value;
            //    newData.PurchaseQuantityInStockUnit = Number(StockUnitConversion(currentRowData.ConversionFormulaStockUnit, value, currentRowData.UnitPerPacking, currentRowData.WtPerPacking, currentRowData.ConversionFactor, currentRowData.SizeW, currentRowData.UnitDecimalPlaceStockUnit, currentRowData.PurchaseUnit, currentRowData.StockUnit, Number(currentRowData.GSM), Number(currentRowData.ReleaseGSM), Number(currentRowData.AdhesiveGSM), Number(currentRowData.Thickness), Number(currentRowData.Density)));
            //}
        },
        { dataField: "PurchaseQuantityInStockUnit", visible: true, allowEditing: false, caption: "P.O.Qty (In S.U.)", width: 80 },
        {
            dataField: "PurchaseRate", visible: true, allowEditing: FlagEditPurchaseRate, caption: "Rate", width: 50, //16
            validationRules: [{ type: "required" }, { type: "numeric" }]
        },
        {
            dataField: "PurchaseUnit", visible: true, allowEditing: false, caption: "Purchase Unit", width: 60
            //lookup: {
            //    dataSource: StkUnit,
            //    displayExpr: "Name",
            //    valueExpr: "Name"
            //}
        },
        {
            dataField: "ProductHSNName", visible: true, allowEditing: false, caption: "ProductHSNName", width: 120
            //cellTemplate: function (container, options) {
            //$('<div>').addClass('fa fa-plus customgridbtn')
            //    .on('dxclick', function () {
            //        this.setAttribute("data-toggle", "modal");
            //        this.setAttribute("data-target", "#largeModalHSNGroup");
            //    }).appendTo(container);
            // }

        }, //18
        { dataField: "HSNCode", visible: true, allowEditing: false, caption: "HSNCode", width: 80 }, //19
        {
            dataField: "ExpectedDeliveryDate", visible: true, allowEditing: true, caption: "Expec. Delivery Date", width: 120, //20
            dataType: "date", format: "dd-MMM-yyyy",
            showEditorAlways: false
        },
        {
            dataField: "Tolerance", visible: true, allowEditing: true, caption: "Tole. %", width: 40, //21
            validationRules: [{ type: "required" }, { type: "numeric" }]
        },
        { dataField: "ItemNarration", visible: false, caption: "Item Narration", width: 100 }, //22
        { dataField: "BasicAmount", visible: true, allowEditing: false, caption: "Basic Amt" }, //23
        {
            dataField: "Disc", visible: true, allowEditing: true, caption: "Disc. %", width: 40, //24
            validationRules: [{ type: "required" }, { type: "numeric" }]
        },
        { dataField: "AfterDisAmt", visible: false, caption: "AfterDisAmt" },
        { dataField: "TaxableAmount", visible: true, allowEditing: false, caption: "Taxable Amt" },
        { dataField: "GSTTaxPercentage", visible: false, caption: "GST %", width: 40 },
        { dataField: "CGSTTaxPercentage", visible: true, allowEditing: false, caption: "CGST %", width: 40 },
        { dataField: "SGSTTaxPercentage", visible: true, allowEditing: false, caption: "SGST %", width: 40 },
        { dataField: "IGSTTaxPercentage", visible: true, allowEditing: false, caption: "IGST %", width: 50 },

        { dataField: "CGSTAmt", visible: true, allowEditing: false, caption: "CGST Amt" },
        { dataField: "SGSTAmt", visible: true, allowEditing: false, caption: "SGST Amt" },
        { dataField: "IGSTAmt", visible: true, allowEditing: false, caption: "IGST Amt" },
        { dataField: "TotalAmount", visible: true, allowEditing: false, caption: "Total Amt" },

        { dataField: "PurchaseQuantityComp", visible: false, caption: "Pending Quantity", width: 120 }, //For Compair of Purchase qty
        { dataField: "CreatedBy", visible: false, caption: "CreatedBy", width: 120 },
        { dataField: "Narration", visible: false, caption: "Narration", width: 120 },
        { dataField: "FYear", visible: false, caption: "FYear", width: 120 },

        { dataField: "ItemDescription", visible: false, caption: "Item Description", width: 120 },
        {
            dataField: "Schedule", visible: true, allowEditing: false, caption: "Schedule", width: 60,
            cellTemplate: function (container, options) {
                $('<div>').addClass('fa fa-plus customgridbtn')
                    .on('dxclick', function () {
                        this.setAttribute("data-toggle", "modal");
                        this.setAttribute("data-target", "#largeModalSchedule");
                        //  $("p").text("Hello world!");
                    }).appendTo(container);
            }
        },

        {
            dataField: "Schedule", visible: true, allowEditing: false, caption: "Schedule", width: 60,
            cellTemplate: function (container, options) {

                $('<div>').addClass('fa fa-plus customgridbtn')

                    .on('dxclick', function () {
                        document.getElementById("SchQtyLbl").innerHTML = "";
                        document.getElementById("TxtUnitSch").value = "";
                        document.getElementById("SchDelDateLbl").innerHtml = "";
                        document.getElementById("SchItemIDLbl").innerHtml = "";
                        document.getElementById("SchItemCodeLbl").innerHtml = "";
                        document.getElementById("TxtQtySch").innerHtml = "";

                        if (GblStatus === "Update") {
                            selectedRowsDtForSchedule.push(options.data);
                            if (selectedRowsDtForSchedule.length > 0) {
                                var lastSelected = selectedRowsDtForSchedule[selectedRowsDtForSchedule.length - 1];
                                document.getElementById("TxtPurchaseQtySch").value = lastSelected.PurchaseQuantity || "";
                                document.getElementById("TxtUnitSch").value = lastSelected.PurchaseUnit || "";
                                document.getElementById("SchItemIDLbl").value = lastSelected.ItemID || "";
                                document.getElementById("SchItemCodeLbl").value = lastSelected.ItemCode || "";

                                $.ajax({
                                    type: "POST",
                                    url: "WebService_PurchaseOrder.asmx/RetrivePoSchedule",
                                    data: JSON.stringify({ transactionID: lastSelected.TransactionID, ItemID: lastSelected.ItemID }),
                                    contentType: "application/json; charset=utf-8",
                                    dataType: "json",
                                    success: function (results) {
                                        var res = results.d.replace(/\\/g, '');
                                        res = res.replace(/"d":""/g, '');
                                        res = res.replace(/""/g, '');
                                        res = res.replace(/u0026/g, '&');
                                        res = res.replace(/u0027/g, "'");
                                        res = res.replace(/:,/g, ":null,");
                                        res = res.replace(/,}/g, ",null}");
                                        res = res.substr(1);
                                        res = res.slice(0, -1);
                                        var ProcessRetriveSch = JSON.parse(res);
                                        ScheduleListOBJ = [];
                                        ScheduleListOBJ = ProcessRetriveSch;
                                        $("#ScheduleGrid").dxDataGrid({
                                            dataSource: ScheduleListOBJ,
                                        });
                                        $('#largeModalSchedule').modal('show');
                                    }
                                });
                            }
                        } else {
                            selectedRowsDtForSchedule.push(options.data);
                            if (selectedRowsDtForSchedule.length > 0) {
                                var lastSelected = selectedRowsDtForSchedule[selectedRowsDtForSchedule.length - 1];
                                document.getElementById("SchQtyLbl").innerHTML = lastSelected.PurchaseQuantity || "";
                                document.getElementById("TxtPurchaseQtySch").value = lastSelected.PurchaseQuantity || "";
                                document.getElementById("TxtUnitSch").value = lastSelected.PurchaseUnit || "";
                                document.getElementById("SchDelDateLbl").innerHTML = lastSelected.ExpectedDeliveryDate || "";
                                document.getElementById("SchItemIDLbl").value = lastSelected.ItemID || "";
                                document.getElementById("SchItemCodeLbl").value = lastSelected.ItemCode || "";
                                document.getElementById("TxtQtySch").value = "";
                                $('#largeModalSchedule').modal('show');
                            }
                        }
                        $('#largeModalSchedule').modal('show');
                    }).appendTo(container);
            }
        },

        { dataField: "WtPerPacking", visible: false, caption: "WtPerPacking", width: 120 },
        { dataField: "UnitPerPacking", visible: false, caption: "UnitPerPacking", width: 120 },
        { dataField: "ConversionFactor", visible: false, caption: "ConversionFactor", width: 120 },
        { dataField: "ConversionFormula", visible: false, caption: "ConversionFormula", width: 120 },
        { dataField: "UnitDecimalPlace", visible: false, caption: "UnitDecimalPlace", width: 120 },
        {
            dataField: "RefJobBookingJobCardContentsID", visible: true, allowEditing: true, caption: "Ref.J.C.No.", width: 150,
            lookup: {
                dataSource: JobCardStore,
                displayExpr: "RefJobCardContentNo",
                valueExpr: "RefJobBookingJobCardContentsID",
                searchEnabled: true,
                searchExpr: "RefJobCardContentNo",
                minSearchLength: 2
            },
            setCellValue: function (newData, value) {
                var result = $.grep(GblJobCardRES, function (e) { return e.RefJobBookingJobCardContentsID === value; });
                newData.RefJobBookingJobCardContentsID = value;
                newData.RefJobCardContentNo = result[0].RefJobCardContentNo;
            }
        },
        {
            dataField: "ClientID", visible: true, allowEditing: true, caption: "Ref.Client", width: 150,
            lookup: {
                dataSource: GblClientName,
                displayExpr: "LedgerName",
                valueExpr: "ClientID"
            },
            setCellValue: function (newData, value) {
                var result = $.grep(GblClientName, function (e) { return e.ClientID === value; });
                newData.ClientID = value;
                newData.ClientID = result[0].ClientID;
            }
        },
        { dataField: "RefJobCardContentNo", visible: false, allowEditing: false, caption: "Ref.J.C.No.", width: 120 },
        { dataField: "ConversionFormulaStockUnit", visible: false, caption: "ConversionFormulaStockUnit", width: 120 },
        { dataField: "UnitDecimalPlaceStockUnit", visible: false, caption: "UnitDecimalPlaceStockUnit", width: 120 },
        { dataField: "ProductHSNID", visible: false, caption: "ProductHSNID", width: 120 },
        { dataField: "Remark", visible: true, caption: "Remark", width: 120, allowEditing: true },
        {
            type: "buttons", fixed: true, alignment: "right",
            buttons: [
                "delete" // Default delete button with icons
            ],
            width: 30,
        },
    ],
    summary: {
        totalItems: [{
            column: "PurchaseQuantity",
            summaryType: "sum",
            displayFormat: "Total: {0}",
        }]
    }
});

GridColumnSetting();
function GridColumnSetting() {
    if (GblCompanyConfiguration.length > 0) {
        if (GblCompanyConfiguration[0].IsGstApplicable === true) {
            $("#CreatePOGrid").dxDataGrid('columnOption', 'CGSTTaxPercentage', 'visible', true);
            $("#CreatePOGrid").dxDataGrid('columnOption', 'SGSTTaxPercentage', 'visible', true);
            $("#CreatePOGrid").dxDataGrid('columnOption', 'IGSTTaxPercentage', 'visible', true);
            $("#CreatePOGrid").dxDataGrid('columnOption', 'IGSTTaxPercentage', 'caption', "IGST %");
            $("#CreatePOGrid").dxDataGrid('columnOption', 'CGSTAmt', 'visible', true);
            $("#CreatePOGrid").dxDataGrid('columnOption', 'SGSTAmt', 'visible', true);
            $("#CreatePOGrid").dxDataGrid('columnOption', 'IGSTAmt', 'visible', true);
            $("#CreatePOGrid").dxDataGrid('columnOption', 'IGSTAmt', 'caption', "IGST Amt");

            $("#ProductHSNGrid").dxDataGrid('columnOption', 'GSTTaxPercentage', 'visible', true);
            $("#ProductHSNGrid").dxDataGrid('columnOption', 'CGSTTaxPercentage', 'visible', true);
            $("#ProductHSNGrid").dxDataGrid('columnOption', 'SGSTTaxPercentage', 'visible', true);
            $("#ProductHSNGrid").dxDataGrid('columnOption', 'IGSTTaxPercentage', 'visible', true);
            $("#ProductHSNGrid").dxDataGrid('columnOption', 'GSTTaxPercentage', 'caption', "GSTTaxPercentage");

            document.getElementById("lblGSTAmount").innerHTML = "GST Amount";

        } else if (GblCompanyConfiguration[0].IsVatApplicable === true) {
            $("#CreatePOGrid").dxDataGrid('columnOption', 'CGSTTaxPercentage', 'visible', false);
            $("#CreatePOGrid").dxDataGrid('columnOption', 'SGSTTaxPercentage', 'visible', false);
            $("#CreatePOGrid").dxDataGrid('columnOption', 'IGSTTaxPercentage', 'visible', true);
            $("#CreatePOGrid").dxDataGrid('columnOption', 'IGSTTaxPercentage', 'caption', (GblCompanyConfiguration[0].DefaultTaxLedgerTypeName + " %"));
            $("#CreatePOGrid").dxDataGrid('columnOption', 'CGSTAmt', 'visible', false);
            $("#CreatePOGrid").dxDataGrid('columnOption', 'SGSTAmt', 'visible', false);
            $("#CreatePOGrid").dxDataGrid('columnOption', 'IGSTAmt', 'visible', true);
            $("#CreatePOGrid").dxDataGrid('columnOption', 'IGSTAmt', 'caption', (GblCompanyConfiguration[0].DefaultTaxLedgerTypeName + " Amt"));

            $("#ProductHSNGrid").dxDataGrid('columnOption', 'GSTTaxPercentage', 'visible', true);
            $("#ProductHSNGrid").dxDataGrid('columnOption', 'CGSTTaxPercentage', 'visible', false);
            $("#ProductHSNGrid").dxDataGrid('columnOption', 'SGSTTaxPercentage', 'visible', false);
            $("#ProductHSNGrid").dxDataGrid('columnOption', 'IGSTTaxPercentage', 'visible', false);
            $("#ProductHSNGrid").dxDataGrid('columnOption', 'GSTTaxPercentage', 'caption', "Tax %");

            document.getElementById("lblGSTAmount").innerHTML = "VAT Amount";
        }
    }

}
$("#ScheduleGrid").dxDataGrid({
    dataSource: [],
    columnAutoWidth: true,
    showBorders: true,
    showRowLines: true,
    allowColumnReordering: true,
    allowColumnResizing: true,
    filterRow: { visible: false, applyFilter: "auto" },
    height: function () {
        return window.innerHeight / 1.3;
    },
    loadPanel: {
        enabled: true,
        height: 90,
        width: 200,
        text: 'Data is loading...'
    },
    editing: {
        mode: "row",
        allowDeleting: true
    },
    onRowRemoving: function (e) {
        RemID = "";
        RemID = e.data.id;
    },
    onRowRemoved: function (e) {
        ScheduleListOBJ = ScheduleListOBJ.filter(function (obj) {
            return obj.id !== RemID;
        });
    },
    onRowPrepared: function (e) {
        if (e.rowType === "header") {
            e.rowElement.css('background', '#509EBC');
            e.rowElement.css('color', 'white');
            e.rowElement.css('font-weight', 'bold');
        }
        e.rowElement.css('fontSize', '11px');
    },
    columns: [{ dataField: "id", visible: false, caption: "Seq.No", alignment: "center" },
    { dataField: "ItemID", visible: false, caption: "ItemID", alignment: "center" },
    { dataField: "ItemCode", visible: true, caption: "Item Code", alignment: "center" },
    { dataField: "Quantity", visible: true, caption: "Quantity", alignment: "center" },
    { dataField: "PurchaseUnit", visible: true, caption: "Purchase Unit", alignment: "center" },
    { dataField: "SchDate", visible: true, caption: "Schedule Date", dataType: "date", format: "dd-MMM-yyyy", alignment: "center" }
    ]
});

$("#BtnProductHSN").click(function () {
    if (SelectedProductHSNList.length > 0) {
        var CreatePOGriddataGrid = $("#CreatePOGrid").dxDataGrid('instance');
        var ProductHSNID = 0, ProductHSNName = "", HSNCode = "", HSNGSTTaxPercentage = 0, HSNCGSTTaxPercentage = 0, HSNSGSTTaxPercentage = 0, HSNSIGSTTaxPercentage = 0;

        ProductHSNID = SelectedProductHSNList[0].ProductHSNID;
        ProductHSNName = SelectedProductHSNList[0].ProductHSNName;
        HSNCode = SelectedProductHSNList[0].HSNCode;
        HSNGSTTaxPercentage = SelectedProductHSNList[0].GSTTaxPercentage;
        HSNCGSTTaxPercentage = SelectedProductHSNList[0].CGSTTaxPercentage;
        HSNSGSTTaxPercentage = SelectedProductHSNList[0].SGSTTaxPercentage;
        HSNSIGSTTaxPercentage = SelectedProductHSNList[0].IGSTTaxPercentage;

        CreatePOGriddataGrid.cellValue(GetPOGridRow, "ProductHSNID", ProductHSNID);
        CreatePOGriddataGrid.cellValue(GetPOGridRow, "ProductHSNName", ProductHSNName);
        CreatePOGriddataGrid.cellValue(GetPOGridRow, "HSNCode", HSNCode);
        CreatePOGriddataGrid.cellValue(GetPOGridRow, "GSTTaxPercentage", HSNGSTTaxPercentage);
        CreatePOGriddataGrid.cellValue(GetPOGridRow, "CGSTTaxPercentage", HSNCGSTTaxPercentage);
        CreatePOGriddataGrid.cellValue(GetPOGridRow, "SGSTTaxPercentage", HSNSGSTTaxPercentage);
        CreatePOGriddataGrid.cellValue(GetPOGridRow, "IGSTTaxPercentage", HSNSIGSTTaxPercentage);

        CreatePOGriddataGrid.saveEditData();

        AddItemCalculation();
        GridColumnCal();
        // CreatePOGriddataGrid.cellValue(GetPOGridRow, "PurchaseQuantity", ReqQty)
        //for (var xx = 0; xx < CreatePOGriddataGrid.totalCount() ; xx++) {
        //    ReqQty = CreatePOGriddataGrid._options.dataSource[xx].PurchaseQuantityComp;
        //    PerQty = CreatePOGriddataGrid._options.dataSource[xx].PurchaseQuantity;
        //    if (ReqQty < PerQty) {
        //        CreatePOGriddataGrid.cellValue(xx, "PurchaseQuantity", ReqQty)
        //        DevExpress.ui.notify("Purchase Quantity should not be greater then Pending Quantity..!", "error", 1000);
        //        return false;
        //    }
        //}

        //alert(GetPOGridRow);
        //alert(SelectedProductHSNList[0].HSNCode);
        //alert(SelectedProductHSNList[0].ProductHSNName);
    }
});

///init selectbox and datebox
$("#PurchaseDivision").dxSelectBox({
    items: PurchaseDivisionText,
    //value: PurchaseDivisionText[0],
    placeholder: "Select --",
    searchEnabled: true,
    showClearButton: false
});

$("#ModeOfTransport").dxSelectBox({
    items: ModeOfTransportText,
    placeholder: "Select --",
    searchEnabled: true,
    showClearButton: true
});

$("#ContactPersonName").dxSelectBox({
    items: [],
    placeholder: "Select--",
    displayExpr: 'Name',
    valueExpr: 'ConcernPersonID',
    searchEnabled: true,
    showClearButton: true
});

$("#VoucherDate").dxDateBox({
    pickerType: 'calendar',
    type: 'date',
    displayFormat: 'dd-MMM-yyyy',
    valueExpr: 'value',
    value: new Date().toISOString().substr(0, 10),
    acceptCustomValue: false,
    onValueChanged: function (e) {
        let dateBoxDate = new Date(e.value);
        dateBoxDate.setHours(0, 0, 0, 0);

        // Get the current date and set time to 00:00:00 for comparison
        let currentDate = new Date();
        currentDate.setHours(0, 0, 0, 0);

        // Validate that the DateBox date is not in the future
        if (dateBoxDate > currentDate) {
            //DevExpress.ui.notify("The selected date cannot be greater than today’s date.", "warning", 1500);
            DevExpress.ui.notify({
                message: "The selected date cannot be greater than today’s date.", type: "warning", displayTime: 5000, width: "900px",
                onContentReady: function (e) {
                    e.component.$content().find(".dx-toast-message").css({
                        "font-size": "13px",
                        "font-weight": "bold",
                    });
                    const closeButton = $("<div>")
                        .addClass("dx-notification-close")
                        .text("×")
                        .css({
                            "position": "absolute",
                            "top": "5px",
                            "right": "5px",
                            "cursor": "pointer",
                            "font-size": "25px",
                        })
                        .appendTo(e.component.$content());
                    closeButton.on("click", function () {
                        e.component.hide();
                    });
                }
            });
            // Optionally reset the DateBox value to clear the invalid input
            $("#VoucherDate").dxDateBox({
                value: new Date().toISOString().substr(0, 10),
            });
            return;
        } else {
            let grid = $('#CreatePOGrid').dxDataGrid('instance');

            // Load all data from the grid
            grid.getDataSource().store().load().done(function (data) {
                data.forEach((rowData, index) => {
                    // Get the date value from the grid's date column
                    let gridDateValue = rowData['ExpectedDeliveryDate'];

                    // Convert the grid date value to a Date object
                    let gridDate = new Date(gridDateValue);
                    gridDate.setHours(0, 0, 0, 0);

                    // Check if the grid date is less than the DateBox date
                    if (gridDate < dateBoxDate) {
                        //DevExpress.ui.notify("The selected date cannot be less than expected delivery date.", "warning", 1500);
                        DevExpress.ui.notify({
                            message: "The selected date cannot be less than expected delivery date.", type: "warning", displayTime: 5000, width: "900px",
                            onContentReady: function (e) {
                                e.component.$content().find(".dx-toast-message").css({
                                    "font-size": "13px",
                                    "font-weight": "bold",
                                });
                                const closeButton = $("<div>")
                                    .addClass("dx-notification-close")
                                    .text("×")
                                    .css({
                                        "position": "absolute",
                                        "top": "5px",
                                        "right": "5px",
                                        "cursor": "pointer",
                                        "font-size": "25px",
                                    })
                                    .appendTo(e.component.$content());
                                closeButton.on("click", function () {
                                    e.component.hide();
                                });
                            }
                        });
                        // Optionally reset the DateBox value to clear the invalid input
                        $("#VoucherDate").dxDateBox({
                            value: new Date(gridDateValue).toISOString().substr(0, 10),
                        });
                        return;
                    }
                });
            });
        }
    }
});

$("#SelCurrencyCode").dxSelectBox({
    items: [],
    placeholder: "Select --",
    displayExpr: 'CurrencyCode',
    valueExpr: 'CurrencyCode',
    searchEnabled: true,
    showClearButton: false
});

$("#SelPOApprovalBy").dxSelectBox({
    items: [],
    placeholder: "Select --",
    displayExpr: 'LedgerName',
    valueExpr: 'LedgerID',
    searchEnabled: true,
    showClearButton: true
});

$("#DealerName").dxSelectBox({
    items: [],
    placeholder: "Select--",
    displayExpr: 'LedgerName',
    valueExpr: 'LedgerID',
    searchEnabled: true,
    showClearButton: true
});

$("#SupplierName").dxSelectBox({
    items: [],
    placeholder: "Select--",
    displayExpr: 'LedgerName',
    valueExpr: 'LedgerID',
    searchEnabled: true,
    showClearButton: true
});
$("#FromDate").dxDateBox({
    dataType: "date",
    displayFormat: 'dd-MMM-yyyy',
    value: sevenDaysAgo
});

// Set the dxDateBox value to the current date
$("#ToDate").dxDateBox({
    dataType: "date",
    displayFormat: 'dd-MMM-yyyy',
    value: new Date().toISOString().substr(0, 10)
});

var fifteenDaysAgo = new Date();
fifteenDaysAgo.setDate(fifteenDaysAgo.getDate() - 15);

$("#FromDateOldPO").dxDateBox({
    dataType: "date",
    displayFormat: 'dd-MMM-yyyy',
    value: fifteenDaysAgo
});


// Set the dxDateBox value to the current date
$("#ToDateOldPO").dxDateBox({
    dataType: "date",
    displayFormat: 'dd-MMM-yyyy',
    value: new Date().toISOString().substr(0, 10)
});

loadCurrency();
function loadCurrency() {
    try {
        $.ajax({
            type: "POST",
            url: "WebService_PurchaseOrder.asmx/GetCurrencyList",
            data: "",
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            success: function (results) {
                var res = results.replace(/\\/g, '');
                res = res.replace(/"d":""/g, '');
                res = res.replace(/""/g, '');
                res = res.replace(/u0026/g, '&');
                res = res.replace(/u0027/g, "'");
                res = res.replace(/:,/g, ":null,");
                res = res.replace(/,}/g, ",null}");
                res = res.substr(1);
                res = res.slice(0, -1);
                if (res !== "fail") {
                    var currencyList = JSON.parse(res);
                    $("#SelCurrencyCode").dxSelectBox({
                        items: currencyList
                    });
                }
            }
        });
    } catch (e) {
        console.log(e);
    }
}

loadPOApprovalBy();
function loadPOApprovalBy() {
    try {
        $.ajax({
            type: "POST",
            url: "WebService_PurchaseOrder.asmx/GetPOApprovalBy",
            data: "",
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            success: function (results) {
                var res = results.replace(/\\/g, '');
                res = res.replace(/"d":""/g, '');
                res = res.replace(/""/g, '');
                res = res.replace(/u0026/g, '&');
                res = res.replace(/u0027/g, "'");
                res = res.replace(/:,/g, ":null,");
                res = res.replace(/,}/g, ",null}");
                res = res.substr(1);
                res = res.slice(0, -1);
                if (res !== "fail") {
                    var EmployeeList = JSON.parse(res);
                    $("#SelPOApprovalBy").dxSelectBox({
                        items: EmployeeList
                    });
                }
            }
        });
    } catch (e) {
        console.log(e);
    }
}

CreatePONO();

function CreatePONO() {
    $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
    $.ajax({
        type: "POST",
        url: "WebService_PurchaseOrder.asmx/GetPONO",
        data: '{prefix:' + JSON.stringify(prefix) + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (results) {
            var res = JSON.stringify(results);
            res = res.replace(/"d":/g, '');
            res = res.replace(/{/g, '');
            res = res.replace(/}/g, '');
            res = res.replace(/:,/g, ":null,");
            res = res.replace(/,}/g, ",null}");
            res = res.substr(1);
            res = res.slice(0, -1);
            if (res !== "fail") {
                document.getElementById("LblPONo").value = res;
            }
        }
    });
}
$("#BtnCreateNewSupplier").click(function () {
    window.open('LedgerMaster.aspx', "_newtab");
});

$("#ButtonRefresh").click(function () {
    GetSupplier();
});

GetSupplier();
function GetSupplier() {
    $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
    //Supplere Get
    $.ajax({
        type: "POST",
        url: "WebService_PurchaseOrder.asmx/Supplier",
        data: '{}',
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            var res = results.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/""/g, '');
            res = res.replace(/u0027/g, "'");
            res = res.replace(/u0026/g, "&");
            res = res.replace(/:,/g, ":null,");
            res = res.replace(/,}/g, ",null}");
            res = res.replace(/:}/g, ":null}");
            res = res.substr(1);
            res = res.slice(0, -1);
            $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
            var Supplier = JSON.parse(res);
            GblCompanyStateTin = "";
            GblGSTApplicable = true;
            if (Supplier.length > 0) {
                GblCompanyStateTin = Supplier[0].CompanyStateTinNo;
                SupplierDDL = [];
                var OptSupplierDDL = {};
                for (var ee = 0; ee < Supplier.length; ee++) {
                    OptSupplierDDL = {};
                    OptSupplierDDL.LedgerID = Supplier[ee].LedgerID;
                    OptSupplierDDL.LedgerName = Supplier[ee].LedgerName;
                    OptSupplierDDL.GSTApplicable = Supplier[ee].GSTApplicable;
                    SupplierDDL.push(OptSupplierDDL);
                }
            }
            else {
                SupplierDDL = [];
            }

            SupplierData = { 'AllSup': Supplier };

            let SupplierdataNew = new DevExpress.data.DataSource({

                store: new DevExpress.data.ArrayStore({

                    data: SupplierDDL,

                    key: "LedgerName"

                })

            });
            $("#SupplierName").dxSelectBox({
                dataSource: SupplierdataNew,
                onValueChanged: function (data) {
                    document.getElementById("LblState").innerHTML = "State : ";
                    document.getElementById("LblCountry").innerHTML = "Country : ";
                    document.getElementById("CurrentCurrency").innerHTML = (GblCompanyConfiguration !== undefined && GblCompanyConfiguration !== null && GblCompanyConfiguration.length > 0) ? GblCompanyConfiguration[0].CurrencyCode : "INR";
                    document.getElementById("VatGSTApplicable").innerHTML = "";
                    document.getElementById("ConversionRate").innerHTML = 1;
                    document.getElementById("LblSupplierStateTin").innerHTML = "";
                    GblGSTApplicable = false;

                    if (data.value === "" || data.value === undefined || data.value === null) {
                        //document.getElementById("DivContactPerson").style.display = "none";
                        $("#ContactPersonName").dxSelectBox({
                            disabled: true
                        });
                    }
                    else {
                        // document.getElementById("DivContactPerson").style.display = "block";
                        $("#ContactPersonName").dxSelectBox({
                            disabled: false
                        });
                    }
                    newSUPllierArray = SupplierData.AllSup.filter(function (el) {
                        return el.LedgerID === data.value;
                    });

                    if (newSUPllierArray !== "" && newSUPllierArray !== [] && newSUPllierArray.length > 0) {
                        document.getElementById("LblState").innerHTML = "State : " + newSUPllierArray[0].SupState;
                        document.getElementById("LblCountry").innerHTML = "Country : " + newSUPllierArray[0].Country;
                        document.getElementById("CurrentCurrency").innerHTML = newSUPllierArray[0].CurrencyCode;
                        document.getElementById("VatGSTApplicable").innerHTML = newSUPllierArray[0].GSTApplicable;
                        document.getElementById("ConversionRate").innerHTML = 1;
                        document.getElementById("LblSupplierStateTin").innerHTML = newSUPllierArray[0].StateTinNo;
                        if (newSUPllierArray[0].GSTApplicable === true || newSUPllierArray[0].GSTApplicable === 1 || newSUPllierArray[0].GSTApplicable.toLowerCase() === 'true') {
                            GblGSTApplicable = true;
                        } else {
                            GblGSTApplicable = false;
                        }

                    }
                    /*
                    $.ajax({
                        type: "POST",
                        url: "WebService_PurchaseOrder.asmx/GetItemRate",
                        data: '{LedgerId:' + JSON.stringify($('#SupplierName').dxSelectBox('instance').option('value')) + '}',
                        contentType: "application/json; charset=utf-8",
                        dataType: "text",
                        success: function (results) {
                            ////console.debug(results);
                            var res = results.replace(/\\/g, '');
                            res = res.replace(/"d":""/g, '');
                            res = res.replace(/""/g, '');
                            res = res.replace(/u0026/g, '&');
                            res = res.replace(/u0027/g, "'");
                            res = res.replace(/:,/g, ":null,");
                            res = res.replace(/,}/g, ",null}");
                            res = res.substr(1);
                            res = res.slice(0, -1);
                            var I_RateRESS = JSON.parse(res);
                            ItemRateString = { 'ItemRateObj': I_RateRESS };
                            var dtGrid = $("#CreatePOGrid").dxDataGrid('instance');
                            var purRate = 0, QuantityTolerance = 0;
                            for (var x = 0; x < dtGrid._options.dataSource.length; x++) {
                                ObjItemRate = [];

                                ObjItemRate = ItemRateString.ItemRateObj.filter(function (el) {
                                    return el.LedgerID === data.value &&
                                        el.ItemID === dtGrid._options.dataSource[x].ItemID;
                                });
                                if (ObjItemRate === [] || ObjItemRate === "" || ObjItemRate.length === 0) {
                                    purRate = Number(dtGrid._options.dataSource[x].PurchaseRate);
                                } else {
                                    purRate = Number(ObjItemRate[0].PurchaseRate);
                                    QuantityTolerance = Number(ObjItemRate[0].QuantityTolerance);
                                }
                                if (Number(purRate) > 0) {
                                    dtGrid._options.dataSource[x].PurchaseRate = purRate;
                                    dtGrid._options.dataSource[x].Tolerance = QuantityTolerance;

                                    dtGrid._options.dataSource[x].BasicAmount = (Number(dtGrid._options.dataSource[x].PurchaseQuantity) * Number(dtGrid._options.dataSource[x].PurchaseRate)).toFixed(2);
                                    if (Number(dtGrid._options.dataSource[x].Disc) > 0) {
                                        dtGrid._options.dataSource[x].AfterDisAmt = Number(dtGrid._options.dataSource[x].BasicAmount) - (Number(dtGrid._options.dataSource[x].BasicAmount) * Number(dtGrid._options.dataSource[x].Disc) / 100).toFixed(2);
                                        dtGrid._options.dataSource[x].TaxableAmount = Number(dtGrid._options.dataSource[x].BasicAmount) - (Number(dtGrid._options.dataSource[x].BasicAmount) * Number(dtGrid._options.dataSource[x].Disc) / 100).toFixed(2);
                                    } else {
                                        dtGrid._options.dataSource[x].AfterDisAmt = dtGrid._options.dataSource[x].BasicAmount;
                                        dtGrid._options.dataSource[x].TaxableAmount = dtGrid._options.dataSource[x].BasicAmount;
                                    }
                                }
                                dtGrid.refresh();
                            }
                            if (ChargesGrid.length > 0) {
                                AddItemCalculation();
                                GridColumnCal();
                                //AddItemWithChargessGrid();
                                CalculateAmount();
                                AddAditionalCharges(existReq);
                            } else {
                                AddItemCalculation();
                                GridColumnCal();
                                AddAditionalCharges(existReq);
                            }
                        }
                    });
                    */
                    SupplierWiseCharges();
                    let supID = (data.value !== undefined && data.value !== null && data.value !== "") ? data.value : 0;
                    $.ajax({
                        type: "POST",
                        url: "WebService_PurchaseOrder.asmx/GetContactPerson",
                        data: '{ContactPerson:' + JSON.stringify(supID) + '}',
                        contentType: "application/json; charset=utf-8",
                        dataType: "text",
                        success: function (results) {
                            var res = results.replace(/\\/g, '');
                            res = res.replace(/"d":""/g, '');
                            res = res.replace(/""/g, '');
                            res = res.replace(/u0026/g, '&');
                            res = res.replace(/u0027/g, "'");
                            res = res.replace(/:,/g, ":null,");
                            res = res.replace(/,}/g, ",null}");
                            res = res.substr(1);
                            res = res.slice(0, -1);
                            var ContactPerson = JSON.parse(res);
                            $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);

                            $("#ContactPersonName").dxSelectBox({
                                items: ContactPerson
                            });
                        }
                    });
                }
            });

            $("#DealerName").dxSelectBox({
                items: SupplierDDL
            });
        }
    });
}

function SupplierWiseCharges() {
    let ledID = $('#SupplierName').dxSelectBox('instance').option('value');
    if (ledID === "" || ledID === undefined || ledID === null) { ledID = 0; }
    $.ajax({
        type: "POST",
        url: "WebService_PurchaseOrder.asmx/GetItemRate",
        data: '{LedgerId:' + JSON.stringify(ledID) + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            ////console.debug(results);
            var res = results.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/""/g, '');
            res = res.replace(/u0026/g, '&');
            res = res.replace(/u0027/g, "'");
            res = res.replace(/:,/g, ":null,");
            res = res.replace(/,}/g, ",null}");
            res = res.substr(1);
            res = res.slice(0, -1);
            var I_RateRESS = JSON.parse(res);
            ItemRateString = { 'ItemRateObj': I_RateRESS };
            var dtGrid = $("#CreatePOGrid").dxDataGrid('instance');
            var purRate = 0, QuantityTolerance = 0;
            for (var x = 0; x < dtGrid._options.dataSource.length; x++) {
                ObjItemRate = [];

                ObjItemRate = ItemRateString.ItemRateObj.filter(function (el) {
                    return el.LedgerID === ledID &&
                        el.ItemID === dtGrid._options.dataSource[x].ItemID;
                });
                if (ObjItemRate === [] || ObjItemRate === "" || ObjItemRate.length === 0) {
                    if (Number(dtGrid._options.dataSource[x].PurchaseRate) > 0) {
                        purRate = Number(dtGrid._options.dataSource[x].PurchaseRate);
                    } else {
                        let itemData = GetOverFlowGrid.filter(function (xx) {
                            return xx.ItemID === dtGrid._options.dataSource[x].ItemID;
                        });
                        if (itemData.length > 0) {
                            purRate = Number(itemData[0].PurchaseRate);
                        }
                    }
                } else {
                    purRate = Number(ObjItemRate[0].PurchaseRate);
                    QuantityTolerance = Number(ObjItemRate[0].QuantityTolerance);
                }
                if (Number(purRate) >= 0) {
                    dtGrid._options.dataSource[x].PurchaseRate = purRate;
                    //Comment because the tolerance quantity is not being updated in the grid.
                    /* dtGrid._options.dataSource[x].Tolerance = QuantityTolerance;*/

                    dtGrid._options.dataSource[x].BasicAmount = (Number(dtGrid._options.dataSource[x].PurchaseQuantity) * Number(dtGrid._options.dataSource[x].PurchaseRate)).toFixed(2);
                    if (Number(dtGrid._options.dataSource[x].Disc) > 0) {
                        dtGrid._options.dataSource[x].AfterDisAmt = Number(dtGrid._options.dataSource[x].BasicAmount) - (Number(dtGrid._options.dataSource[x].BasicAmount) * Number(dtGrid._options.dataSource[x].Disc) / 100).toFixed(2);
                        dtGrid._options.dataSource[x].TaxableAmount = Number(dtGrid._options.dataSource[x].BasicAmount) - (Number(dtGrid._options.dataSource[x].BasicAmount) * Number(dtGrid._options.dataSource[x].Disc) / 100).toFixed(2);
                    } else {
                        dtGrid._options.dataSource[x].AfterDisAmt = dtGrid._options.dataSource[x].BasicAmount;
                        dtGrid._options.dataSource[x].TaxableAmount = dtGrid._options.dataSource[x].BasicAmount;
                    }
                }
                dtGrid.refresh();
            }
            if (ChargesGrid.length > 0) {
                AddItemCalculation();
                GridColumnCal();
                //AddItemWithChargessGrid();
                CalculateAmount();
                AddAditionalCharges(existReq);
            } else {
                AddItemCalculation();
                GridColumnCal();
                AddAditionalCharges(existReq);
            }
        }
    });
}

var RadioValue = "Pending Requisitions";
GetDataGrid();

$("#RadioButtonPO").dxRadioGroup({
    items: priorities,
    value: priorities[0],
    layout: 'horizontal',
    onValueChanged: function (e) {
        RadioValue = "";
        RadioValue = e.value;
        GblStatus = "";
        GetDataGrid();
    }
});

function GetDataGrid() {
    if (RadioValue === "Pending Requisitions") {
        GetPendingData = "";
        GblStatus = "";
        document.getElementById("dateFilter").style.display = "none";
        SubGridData = []; ChargesGrid = []; PaymentTermsGrid = []; ScheduleListOBJ = []; existReq = [];

        document.getElementById("DivDateChk").style.display = "none";
        document.getElementById("DivEdit").style.display = "none";
        document.getElementById("DivCretbtn").style.display = "block";

        $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);

        document.getElementById("POGridPending").style.display = "block";
        document.getElementById("POGridProcess").style.display = "none";

        document.getElementById("TxtPOID").value = "";

        $.ajax({
            type: "POST",
            url: "WebService_PurchaseOrder.asmx/FillGrid",
            data: '{RadioValue:' + JSON.stringify(RadioValue) + '}',
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            success: function (results) {
                ////console.debug(results);
                var res = results.replace(/\\/g, '');
                res = res.replace(/"d":""/g, '');
                res = res.replace(/""/g, '');
                res = res.replace(/u0026/g, '&');
                res = res.replace(/u0027/g, "'");
                res = res.replace(/:,/g, ":null,");
                res = res.replace(/,}/g, ",null}");
                res = res.substr(1);
                res = res.slice(0, -1);
                var PendingList = JSON.parse(res);
                $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                fillGridPending(PendingList);
            }
        });
    }

    else if (RadioValue === "Purchase Orders") {
        SubGridData = [];
        GetPendingData = "";
        GblStatus = "Update";
        document.getElementById("dateFilter").style.display = "block";
        ChargesGrid = []; PaymentTermsGrid = []; ScheduleListOBJ = []; existReq = [];

        document.getElementById("DivDateChk").style.display = "block";
        document.getElementById("DivEdit").style.display = "block";
        document.getElementById("DivCretbtn").style.display = "none";

        document.getElementById("POGridPending").style.display = "none";
        document.getElementById("POGridProcess").style.display = "block";

        document.getElementById("TxtPOID").value = "";
        //$("#RadioButtonStatus").dxRadioGroup({
        //    value: "ApprovalPending"
        //});
        POProcessFillGrid();
    }
}

try {
    $("#RadioButtonStatus").dxRadioGroup({
        items: [{ value: "All", text: "All P.O." }, { value: "ApprovalPending", text: "Approval Pending P.O." },
        { value: "ApprovedOrders", text: "Approved P.O." }, { value: "CancelledOrders", text: "Cancelled P.O." }],
        displayExpr: "text",
        valueExpr: "value",
        layout: "horizontal",
        value: "All",
        itemTemplate: function (itemData, _, itemElement) {
            itemElement
                .parent().addClass(itemData.value.toLowerCase()).addClass("font-bold")
                .text(itemData.text);
        },
        onValueChanged: function (data) {
            $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
            var FDate = "", ToDate = "", CHKPODate = "";
            var CHKPODetail = document.getElementById("CHKPODetail").checked;
            var FilterStr = "";
            if (data.value === "All") {
                FilterStr = "";
            } else if (data.value === "ApprovalPending") {
                FilterStr = " And (Isnull(ITD.IsVoucherItemApproved,0)=0 And Isnull(ITD.IsCancelled,0)=0) ";
            } else if (data.value === "ApprovedOrders") {
                FilterStr = " And (Isnull(ITD.IsVoucherItemApproved,0)=1) ";
            } else if (data.value === "CancelledOrders") {
                FilterStr = " And (Isnull(ITD.IsCancelled,0)=1) ";
            } else {
                FilterStr = "";
            }
            let fromDateValue = new Date($("#FromDate").dxDateBox("instance").option("value"));
            fromDateValue = fromDateValue.toISOString().substr(0, 10);

            let ToDateValue = new Date($("#ToDate").dxDateBox("instance").option("value"));
            ToDateValue = ToDateValue.toISOString().substr(0, 10);

            $.ajax({
                type: "POST",
                url: "WebService_PurchaseOrder.asmx/ProcessFillGrid",
                data: '{fromDateValue:' + JSON.stringify(fromDateValue) + ',ToDateValue:' + JSON.stringify(ToDateValue) + ',chk:' + JSON.stringify(CHKPODate) + ',Detail:' + JSON.stringify(CHKPODetail) + ',FilterStr:' + JSON.stringify(FilterStr) + '}',
                contentType: "application/json; charset=utf-8",
                dataType: "text",
                crossDomain: true,
                success: function (results) {
                    var res = results.replace(/\\/g, '');
                    res = res.replace(/"d":""/g, '');
                    res = res.replace(/""/g, '');
                    res = res.replace(/u0026/g, '&');
                    res = res.replace(/u0027/g, "'");
                    res = res.replace(/:,/g, ":null,");
                    res = res.replace(/,}/g, ",null}");
                    res = res.substr(1);
                    res = res.slice(0, -1);

                    var ProcessRESS = JSON.parse(res);
                    fillGridPOSuccess(ProcessRESS);

                    var gridInstance = $("#POGridProcess").dxDataGrid('instance');
                    gridInstance.columnOption("ItemCode", "visible", document.getElementById("CHKPODetail").checked);
                    gridInstance.columnOption("ItemGroupName", "visible", document.getElementById("CHKPODetail").checked);
                    gridInstance.columnOption("ItemSubGroupName", "visible", document.getElementById("CHKPODetail").checked);
                    gridInstance.columnOption("ItemName", "visible", document.getElementById("CHKPODetail").checked);
                    gridInstance.columnOption("PurchaseUnit", "visible", document.getElementById("CHKPODetail").checked);
                    gridInstance.columnOption("ExpectedDeliveryDate", "visible", document.getElementById("CHKPODetail").checked);
                    gridInstance.columnOption("RefJobCardContentNo", "visible", document.getElementById("CHKPODetail").checked);

                    gridInstance.columnOption("PurchaseRate", "visible", document.getElementById("CHKPODetail").checked);
                    gridInstance.columnOption("PendingToReceiveQty", "visible", document.getElementById("CHKPODetail").checked);

                    $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                },
                error: function errorFunc(jqXHR) {
                    //DevExpress.ui.notify(jqXHR.statusText, "error", 500);
                }
            });

        }
    });
} catch (e) {
    console.log(e);
}

//$("#RefreshBtn").click(function () {
//    GetDataGrid();
//    POProcessFillGrid();
//});

$("#RefreshPOButton").click(function () {
    POProcessFillGrid();
});

function POProcessFillGrid() {
    var FDate = "", ToDate = "", CHKPODate = "";
    //var FDate = $('#FromDate').dxDateBox('instance').option('value');;
    //var ToDate = $('#ToDate').dxDateBox('instance').option('value');;
    //var CHKPODate = document.getElementById("CHKPODate").checked;
    var CHKPODetail = document.getElementById("CHKPODetail").checked;
    var optionStatus = $("#RadioButtonStatus").dxRadioGroup('instance').option('value');
    var FilterStr = "";
    if (optionStatus === "All") {
        FilterStr = "";
    } else if (optionStatus === "ApprovalPending") {
        FilterStr = " And (Isnull(ITD.IsVoucherItemApproved,0)=0 And Isnull(ITD.IsCancelled,0)=0) ";
    } else if (optionStatus === "ApprovedOrders") {
        FilterStr = " And (Isnull(ITD.IsVoucherItemApproved,0)=1) ";
    } else if (optionStatus === "CancelledOrders") {
        FilterStr = " And (Isnull(ITD.IsCancelled,0)=1) ";
    } else {
        FilterStr = "";
    }
    let fromDateValue = new Date($("#FromDate").dxDateBox("instance").option("value"));
    fromDateValue = fromDateValue.toISOString().substr(0, 10);

    let ToDateValue = new Date($("#ToDate").dxDateBox("instance").option("value"));
    ToDateValue = ToDateValue.toISOString().substr(0, 10);
    //console.log(optionStatus);
    $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
    $.ajax({
        type: "POST",
        url: "WebService_PurchaseOrder.asmx/ProcessFillGrid",
        data: '{fromDateValue:' + JSON.stringify(fromDateValue) + ',ToDateValue:' + JSON.stringify(ToDateValue) + ',chk:' + JSON.stringify(CHKPODate) + ',Detail:' + JSON.stringify(CHKPODetail) + ',FilterStr:' + JSON.stringify(FilterStr) + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            var res = results.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/""/g, '');
            res = res.replace(/u0026/g, '&');
            res = res.replace(/u0027/g, "'");
            res = res.replace(/:,/g, ":null,");
            res = res.replace(/,}/g, ",null}");
            res = res.substr(1);
            res = res.slice(0, -1);
            $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
            var ProcessRESS = JSON.parse(res);
            fillGridPOSuccess(ProcessRESS);

            var gridInstance = $("#POGridProcess").dxDataGrid('instance');
            gridInstance.columnOption("ItemCode", "visible", document.getElementById("CHKPODetail").checked);
            gridInstance.columnOption("ItemGroupName", "visible", document.getElementById("CHKPODetail").checked);
            gridInstance.columnOption("ItemSubGroupName", "visible", document.getElementById("CHKPODetail").checked);
            gridInstance.columnOption("ItemName", "visible", document.getElementById("CHKPODetail").checked);
            gridInstance.columnOption("PurchaseUnit", "visible", document.getElementById("CHKPODetail").checked);
            gridInstance.columnOption("ExpectedDeliveryDate", "visible", document.getElementById("CHKPODetail").checked);
            gridInstance.columnOption("RefJobCardContentNo", "visible", document.getElementById("CHKPODetail").checked);

            gridInstance.columnOption("PurchaseRate", "visible", document.getElementById("CHKPODetail").checked);
            gridInstance.columnOption("PendingToReceiveQty", "visible", document.getElementById("CHKPODetail").checked);

        }
    });
}

$("#POGridPending").dxDataGrid({
    dataSource: [],
    columnAutoWidth: true,
    showBorders: true,
    showRowLines: true,
    allowColumnReordering: true,
    allowColumnResizing: true,
    columnResizingMode: "widget",
    sorting: {
        mode: "multiple"
    },
    selection: { mode: "multiple", showCheckBoxesMode: "always" },
    paging: {
        pageSize: 50
    },
    pager: {
        showPageSizeSelector: true,
        allowedPageSizes: [50, 100, 500, 1000]
    },
    filterRow: { visible: true, applyFilter: "auto" },
    headerFilter: { visible: true },
    //rowAlternationEnabled: true,
    searchPanel: { visible: true },
    loadPanel: {
        enabled: true,
        text: 'Data is loading...'
    },
    export: {
        enabled: true,
        fileName: "Pending Requisition",
        allowExportSelectedData: true
    },
    onExporting(e) {
        const workbook = new ExcelJS.Workbook();
        const worksheet = workbook.addWorksheet('PendingRequisition');
        DevExpress.excelExporter.exportDataGrid({
            component: e.component,
            worksheet,
            autoFilterEnabled: true,
        }).then(() => {
            workbook.xlsx.writeBuffer().then((buffer) => {
                saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'PendingRequisition.xlsx');
            });
        });
        e.cancel = true;
    },
    onRowPrepared: function (e) {
        if (e.rowType === "header") {
            e.rowElement.css('background', '#1B555F');
            e.rowElement.css('color', 'white');
            e.rowElement.css('font-weight', 'bold');
        }
        e.rowElement.css('fontSize', '11px');
    },
    onSelectionChanged: function (clickedIndentCell) {
        if (clickedIndentCell.currentSelectedRowKeys.length > 0) {
            if (GBLCompanyID != clickedIndentCell.currentSelectedRowKeys[0].CompanyID) {
                clickedIndentCell.component.deselectRows((clickedIndentCell || {}).currentSelectedRowKeys[0]);

                swal({
                    title: "Access Denied!",
                    text: "You cannot select transactions related to different companies.",
                    button: "OK",
                });
                clickedIndentCell.currentSelectedRowKeys = [];
                return false;
            }
            GetPendingData = clickedIndentCell.selectedRowsData;
        } else {
            SelectedProductionUnitID = 0;
            SelectedProductionUnitName = "";
            GetPendingData = [];
        }
    },

    editing: {
        mode: "cell",
        allowUpdating: true
    },
    onEditingStart: function (e) {
        if (e.column.dataField === "PurchaseQuantity") {
            e.cancel = false;
        }
        else {
            e.cancel = true;
        }
    },
    onRowUpdated: function (e) {
        var ReqQty = Number(e.data.PurchaseQuantityComp);
        var PerQty = Number(e.data.PurchaseQuantity);
        if (ReqQty < PerQty) {
            e.data.PurchaseQuantity = ReqQty;
            //DevExpress.ui.notify("Purchase quantity should not be greater then pending quantity..!", "error", 1000);
            DevExpress.ui.notify({
                message: "Purchase quantity should not be greater then pending quantity..!", type: "error", displayTime: 5000, width: "900px",
                onContentReady: function (e) {
                    e.component.$content().find(".dx-toast-message").css({
                        "font-size": "13px",
                        "font-weight": "bold",
                    });
                    const closeButton = $("<div>")
                        .addClass("dx-notification-close")
                        .text("×")
                        .css({
                            "position": "absolute",
                            "top": "5px",
                            "right": "5px",
                            "cursor": "pointer",
                            "font-size": "25px",
                        })
                        .appendTo(e.component.$content());
                    closeButton.on("click", function () {
                        e.component.hide();
                    });
                }
            });
        }
    },
    columns: [
        { dataField: "MaxVoucherNo", visible: false, caption: "Ref.Req.No.", width: 100, fixed: true },
        { dataField: "VoucherNo", visible: true, caption: "Req. No.", width: 100, fixed: true },
        { dataField: "VoucherDate", visible: true, caption: "Req. Date", width: 100, fixed: true },
        { dataField: "ItemCode", visible: true, caption: "Item Code", width: 100 },
        { dataField: "ItemGroupName", visible: true, caption: "Item Group", width: 100 },
        { dataField: "ItemSubGroupName", visible: true, caption: "Sub Group", width: 120 },
        { dataField: "ItemName", visible: true, caption: "Item Name", width: 300 },
        { dataField: "RefJobCardContentNo", visible: true, caption: "Ref.J.C.No.", width: 120 },
        { dataField: "ItemDescription", visible: false, caption: "Item Description", width: 120 },
        { dataField: "RequiredQuantity", visible: true, caption: "Req. Qty", width: 100, dataType: "string" },
        { dataField: "PurchaseQuantityComp", visible: true, caption: "Pending Qty", width: 100, dataType: "string" }, //For Compair of Purchase qty
        { dataField: "PurchaseQuantity", visible: true, caption: "Purchase Qty", width: 100, dataType: "string" },
        { dataField: "OrderUnit", visible: true, caption: "Order Unit", width: 80 },
        { dataField: "CreatedBy", visible: true, caption: "CreatedBy", width: 120 },
        { dataField: "ItemNarration", visible: true, caption: "Item Narration", width: 120 },
        { dataField: "Narration", visible: true, caption: "Narration", width: 120 },
        { dataField: "PurchaseRate", visible: false, caption: "PurchaseRate", width: 120 },
        { dataField: "PurchaseUnit", visible: false, caption: "PurchaseUnit", width: 120 },
        { dataField: "ProductHSNName", visible: false, caption: "ProductHSNName", width: 120 },
        { dataField: "HSNCode", visible: false, caption: "HSNCode", width: 120 },
        { dataField: "GSTTaxPercentage", visible: false, caption: "GSTTaxPercentage", width: 120 },
        { dataField: "CGSTTaxPercentage", visible: false, caption: "CGSTTaxPercentage", width: 120 },
        { dataField: "SGSTTaxPercentage", visible: false, caption: "SGSTTaxPercentage", width: 120 },
        { dataField: "IGSTTaxPercentage", visible: false, caption: "IGSTTaxPercentage", width: 120 },
        { dataField: "WtPerPacking", visible: false, caption: "WtPerPacking", width: 120 },
        { dataField: "UnitPerPacking", visible: false, caption: "UnitPerPacking", width: 120 },
        { dataField: "ConversionFactor", visible: false, caption: "ConversionFactor", width: 120 },
        { dataField: "SizeW", visible: false, caption: "SizeW", width: 120 },
        { dataField: "ConversionFormula", visible: false, caption: "ConversionFormula", width: 120 },
        { dataField: "UnitDecimalPlace", visible: false, caption: "UnitDecimalPlace", width: 120 },
        { dataField: "ConversionFormulaStockUnit", visible: false, caption: "ConversionFormulaStockUnit", width: 120 },
        { dataField: "UnitDecimalPlaceStockUnit", visible: false, caption: "UnitDecimalPlaceStockUnit", width: 120 },
        { dataField: "StockUnit", visible: false, caption: "StockUnit", width: 80 },
        { dataField: "PurchaseUnit", visible: false, caption: "PurchaseUnit", width: 80 },
        { dataField: "ProductionUnitID", visible: false, caption: "ProductionUnitID", width: 100 },
        { dataField: "ProductionUnitName", visible: true, caption: "Production Unit Name", width: 150 },
        { dataField: "CompanyName", visible: true, caption: "Company Name", width: 300 },
    ]
});

$("#POGridProcess").dxDataGrid({
    dataSource: [],
    columnAutoWidth: true,
    showBorders: true,
    showRowLines: true,
    allowColumnReordering: true,
    allowColumnResizing: true,
    columnResizingMode: "widget",
    sorting: {
        mode: "multiple"
    },
    selection: { mode: "single" },
    paging: {
        pageSize: 50
    },
    pager: {
        showPageSizeSelector: true,
        allowedPageSizes: [50, 100, 500, 1000]
    },
    filterRow: { visible: true, applyFilter: "auto" },
    headerFilter: { visible: true },
    //rowAlternationEnabled: true,
    searchPanel: { visible: true },
    loadPanel: {
        enabled: true,
        text: 'Data is loading...'
    },
    export: {
        enabled: true,
        fileName: "Purchase Orders",
        allowExportSelectedData: true
    },
    onExporting(e) {
        const workbook = new ExcelJS.Workbook();
        const worksheet = workbook.addWorksheet('PurchaseOrders');
        DevExpress.excelExporter.exportDataGrid({
            component: e.component,
            worksheet,
            autoFilterEnabled: true,
        }).then(() => {
            workbook.xlsx.writeBuffer().then((buffer) => {
                saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'PurchaseOrders.xlsx');
            });
        });
        e.cancel = true;
    },
    onRowPrepared: function (e) {
        if (e.rowType === "header") {
            e.rowElement.css('background', '#509EBC');
            e.rowElement.css('color', 'white');
            e.rowElement.css('font-weight', 'bold');
        }
        e.rowElement.css('fontSize', '11px');

        if (e.rowType === "data") {
            if (e.data.VoucherItemApproved === false && e.data.VoucherCancelled === false) {
                e.rowElement.addClass('approvalpending');
            } else if (e.data.VoucherItemApproved === true) {
                e.rowElement.addClass('approvedorders');
            } else if (e.data.VoucherCancelled === true) {
                e.rowElement.addClass('cancelledorders');
            }
        }
    },
    onSelectionChanged: function (clickedCell) {
        SelectedProductionUnitID = 0;
        SelectedProductionUnitName = "";
        if (clickedCell.selectedRowsData.length <= 0) return false;
        SelectedProductionUnitID = clickedCell.selectedRowsData[0].ProductionUnitID;
        SelectedProductionUnitName = clickedCell.selectedRowsData[0].ProductionUnitName;
        ModalPopupScreencontrols();
        document.getElementById("TxtPOID").value = clickedCell.selectedRowsData[0].TransactionID;
        document.getElementById("LblPONo").value = clickedCell.selectedRowsData[0].VoucherNo;
        validateUserData.userName = ""; validateUserData.password = ""; validateUserData.RecordID = document.getElementById("TxtPOID").value; validateUserData.transactionRemark = ""; validateUserData.isUserInfoFilled = false;
        //document.getElementById("textNaretion").innerHTML = clickedCell.selectedRowsData[0].Narration;

        VarItemApproved = clickedCell.selectedRowsData[0].IsVoucherItemApproved;

        /*$("#VoucherDate").dxDateBox({ value: clickedCell.selectedRowsData[0].VoucherDate });*/
        $("#VoucherDate").dxDateBox({
            value: clickedCell.selectedRowsData[0].VoucherDate,
            min: null
        });

        $("#SupplierName").dxSelectBox({ value: clickedCell.selectedRowsData[0].LedgerID });
        $("#ContactPersonName").dxSelectBox({ value: clickedCell.selectedRowsData[0].ContactPersonID });
        $("#PurchaseDivision").dxSelectBox({ value: clickedCell.selectedRowsData[0].PurchaseDivision });
        $("#SelCurrencyCode").dxSelectBox({ value: clickedCell.selectedRowsData[0].CurrencyCode });
        $("#SelPOApprovalBy").dxSelectBox({ value: clickedCell.selectedRowsData[0].VoucherApprovalByEmployeeID });

        updateTotalTax = clickedCell.selectedRowsData[0].TotalTaxAmount;
        document.getElementById("TxtBasicAmt").value = clickedCell.selectedRowsData[0].BasicAmount;
        document.getElementById("TxtAfterDisAmt").value = clickedCell.selectedRowsData[0].AfterDisAmt;

        document.getElementById("TxtNetAmt").value = clickedCell.selectedRowsData[0].NetAmount;
        document.getElementById("PORefernce").value = clickedCell.selectedRowsData[0].PurchaseReference;
        document.getElementById("TxtTotalQty").value = clickedCell.selectedRowsData[0].TotalQuantity;

        //document.getElementById("TxtCGSTAmt").value = clickedCell.selectedRowsData[0].ContactPersonID;
        //document.getElementById("TxtSGSTAmt").value = clickedCell.selectedRowsData[0].ContactPersonID;
        //document.getElementById("TxtIGSTAmt").value = clickedCell.selectedRowsData[0].ContactPersonID;            
        document.getElementById("Txt_TaxAbleSum").value = clickedCell.selectedRowsData[0].TaxableAmount;

        PaymentTermsString = clickedCell.selectedRowsData[0].TermsOfPayment;

        document.getElementById("textDeliverAt").value = clickedCell.selectedRowsData[0].DeliveryAddress;
        //document.getElementById("textNaretion").value = clickedCell.selectedRowsData[0].Narration;

        $("#DealerName").dxSelectBox({ value: clickedCell.selectedRowsData[0].DealerID });

        $("#ModeOfTransport").dxSelectBox({ value: clickedCell.selectedRowsData[0].ModeOfTransport });
    },
    onRowDblClick: function (e) {
        let data = e.data;
        if (data === undefined || data === null) return false;
        if (data) {
            ProcessedGridSelectData = [];
            ProcessedGridSelectData.push(data)
            $("#EditPOButton").click();
        }
    },
    columns: [
        { dataField: "LedgerName", visible: true, caption: "Supplier Name", width: 200 },
        { dataField: "VoucherNo", visible: true, caption: "P.O. No", width: 100 },
        { dataField: "VoucherDate", visible: true, caption: "P.O. Date", width: 120 },
        { dataField: "ItemCode", visible: true, caption: "Item Code", width: 120 },
        { dataField: "ItemGroupName", visible: true, caption: "Item Group", width: 120 },
        { dataField: "ItemSubGroupName", visible: true, caption: "Sub Group", width: 150 },
        { dataField: "ItemName", visible: true, caption: "Item Name", width: 300 },
        { dataField: "ItemDescription", visible: false, caption: "Item Description", width: 300 },
        { dataField: "PurchaseQuantity", visible: true, caption: "P.O. Qty", width: 100 },
        { dataField: "PurchaseUnit", visible: true, caption: "Unit", width: 80 },
        { dataField: "ExpectedDeliveryDate", visible: true, caption: "ExpectedDeliveryDate", width: 80 },
        { dataField: "PurchaseRate", visible: false, caption: "Rate", width: 80 },
        { dataField: "PendingToReceiveQty", visible: false, width: 100 },
        { dataField: "GrossAmount", visible: false, caption: "Gross Amount", width: 100 },
        { dataField: "DiscountAmount", visible: false, caption: "Disc. Amount", width: 100 },
        { dataField: "BasicAmount", visible: false, caption: "Basic Amount", width: 100 },
        { dataField: "GSTPercentage", visible: false, caption: "GST %", width: 80 },
        { dataField: "GSTTaxAmount", visible: false, caption: "GST Amount", width: 100 },
        { dataField: "NetAmount", visible: true, caption: "Net Amount", width: 100 },
        { dataField: "RefJobCardContentNo", visible: true, caption: "Ref.J.C.No.", width: 120 },
        { dataField: "CreatedBy", visible: true, caption: "Created By", width: 100 },
        { dataField: "ApprovedBy", visible: true, caption: "Approved By", width: 100 },
        { dataField: "ReceiptTransactionID", visible: false, caption: "ReceiptTransactionID", width: 120 },
        { dataField: "IsVoucherItemApproved", visible: false, caption: "IsVoucherItemApproved", width: 120 },
        { dataField: "IsReworked", visible: false, caption: "Is Reworked", width: 120 },
        { dataField: "ReworkRemark", visible: false, caption: "Rework Remark", width: 120 },
        { dataField: "PurchaseDivision", visible: true, caption: "Purchase Division", width: 120 },
        { dataField: "PurchaseReference", visible: true, caption: "Purchase Reference", width: 120 },
        { dataField: "Narration", visible: true, caption: "Narration", width: 120 },
        { dataField: "Details", visible: true, caption: "Update Remark", width: 120 },
        { dataField: "CancelRemark", visible: true, caption: "Voucher Cancel Remark",width:150 },
        { dataField: "ProductionUnitID", visible: false, caption: "ProductionUnitID", width: 100 },
        { dataField: "ProductionUnitName", visible: true, caption: "Production Unit Name", width: 150 },
        { dataField: "CompanyName", visible: true, caption: "Company Name", width: 300 },
        { dataField: "TaxableAmount", visible: false, caption: "TaxableAmount", width: 80 },
        { dataField: "ContactPersonID", visible: false, caption: "Contact PersonID", width: 120 },
        { dataField: "RequiredQuantity", visible: false, caption: "RequiredQuantity", width: 80 },
        { dataField: "TotalTaxAmount", visible: false, caption: "TotalTaxAmount", width: 80 },
        { dataField: "TotalOverheadAmount", visible: false, caption: "TotalOverheadAmount", width: 80 },
        { dataField: "DeliveryAddress", visible: false, caption: "DeliveryAddress", width: 120 },
        { dataField: "TotalQuantity", visible: false, caption: "TotalQuantity", width: 120 },
        { dataField: "TermsOfPayment", visible: false, caption: "TermsOfPayment", width: 120 },
        { dataField: "ModeOfTransport ", visible: false, caption: "ModeOfTransport", width: 120 },
        { dataField: "DealerID", visible: false, caption: "DealerID", width: 120 },
        { dataField: "VoucherItemApproved", visible: false, caption: "VoucherItemApproved", dataType: "boolean" },
        { dataField: "VoucherCancelled", visible: false, caption: "VoucherCancelled", dataType: "boolean" },
      
        { dataField: "CurrencyCode", visible: false, caption: "CurrencyCode" },

        {
            caption: "", fixedPosition: "right", fixed: true, width: 30,
            cellTemplate: function (container, options) {
                $('<a title="Print PO">').addClass('fa fa-print dx-link customgridbtn')
                    .on('dxclick', function () {
                        if (options.data.TransactionID !== 0 && options.data.TransactionID !== null && options.data.TransactionID !== undefined) {
                            document.getElementById("TxtPOID").value = options.data.TransactionID;
                            ProcessedGridSelectData = [];
                            ProcessedGridSelectData.push(options.data)
                            $("#PrintPOButton").click();
                        } else {
                            alert("Please Select Data ");
                        }
                    }).appendTo(container);

            }
        },
        {
            caption: "", fixedPosition: "right", fixed: true, width: 30,
            cellTemplate: function (container, options) {
                $('<div title="Edit PO" style="color:blue;">').addClass('fa fa-edit customgridbtn dx-link')
                    .on('dxclick', function () {
                        ProcessedGridSelectData = [];
                        ProcessedGridSelectData.push(options.data)
                        SelectedProductionUnitID = ProcessedGridSelectData[0].ProductionUnitID;
                        SelectedProductionUnitName = ProcessedGridSelectData[0].ProductionUnitName;
                        $("#EditPOButton").click();
                        //if (options.data.TransactionID !== 0 && options.data.TransactionID !== null && options.data.TransactionID !== undefined) {
                        //    document.getElementById("TxtPOID").value = options.data.TransactionID;
                        //    $("#DeletePOButton").click();
                        //} else {
                        //    alert("Please Select Data ");
                        //}
                        //deleteQuotationDetails(options.key);
                    }).appendTo(container);

            }
        }, {
            caption: "", fixedPosition: "right", fixed: true, width: 30,
            cellTemplate: function (container, options) {
                $('<div title="Delete PO" style="color:red;">').addClass('fa fa-trash customgridbtn dx-link')
                    .on('dxclick', function () {
                        if (options.data.TransactionID !== 0 && options.data.TransactionID !== null && options.data.TransactionID !== undefined) {
                            SelectedProductionUnitID = options.data.ProductionUnitID;
                            SelectedProductionUnitName = options.data.ProductionUnitName;
                            document.getElementById("TxtPOID").value = options.data.TransactionID;
                            $("#DeletePOButton").click();
                        } else {
                            alert("Please Select Data ");
                        }
                        //deleteQuotationDetails(options.key);
                    }).appendTo(container);

            }
        }
    ]
});

$("#BtnSelect").click(function () {
    document.getElementById("BtnSelect").setAttribute("data-toggle", "modal");
    document.getElementById("BtnSelect").setAttribute("data-target", "#Select");
});

DataLoad();
function DataLoad() {
    $.ajax({
        type: "POST",
        url: "WebService_PurchaseOrder.asmx/SelectAddressGetData",
        data: '{}',
        contentType: "application/json; charset=utf-8",
        dataType: 'json',
        success: function (results) {
            var res = results.d.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/"d":null/g, '');
            res = res.replace(/u0026/g, '&');
            res = res.replace(/:}/g, ":null}");
            res = res.substr(1);
            res = res.slice(0, -1);
            var res1 = JSON.parse(res);
            $("#SelectAddressDataGrid").dxDataGrid({
                dataSource: res1
            });
        }
    })
}


$('#BtnOKK').click(function () {
    if (GridSelectedData.length === 0 || !GridSelectedData) {
        //DevExpress.ui.notify('Please select row ', 'warning', 1000);
        DevExpress.ui.notify({
            message: "Please select row", type: "warning", displayTime: 5000, width: "900px",
            onContentReady: function (e) {
                e.component.$content().find(".dx-toast-message").css({
                    "font-size": "13px",
                    "font-weight": "bold",
                });
                const closeButton = $("<div>")
                    .addClass("dx-notification-close")
                    .text("×")
                    .css({
                        "position": "absolute",
                        "top": "5px",
                        "right": "5px",
                        "cursor": "pointer",
                        "font-size": "25px",
                    })
                    .appendTo(e.component.$content());
                closeButton.on("click", function () {
                    e.component.hide();
                });
            }
        });
        return;
    }

    var deliveryText = '';
    deliveryText = GridSelectedData[0].DeliveryAddress;
    //GridSelectedData.forEach(function (row) {
    //    Object.keys(row).forEach(function (key) {
    //        deliveryText += key + ': ' + row[key] + '\n';
    //    });
    //    deliveryText += '\n'; // Add a new line after each row
    //});
    // Set the combined text into the textarea
    $('#textDeliverAt').val(deliveryText);
});


$("#SelectAddressDataGrid").dxDataGrid({
    columns: [
        { dataField: "DeliveryAddress", caption: "Delivery Address", alignment: "left" },
        { dataField: "CompanyID", caption: "Company ID", visible: false, alignment: "left" },
    ],
    dataSource: [],
    // Other configurations remain unchanged
    allowColumnReordering: true,
    allowColumnResizing: true,
    columnAutoWidth: true,
    showBorders: true,
    showRowLines: true,
    scrolling: { mode: 'infinite' },
    selection: { mode: 'single' },
    filterRow: { visible: false },
    rowAlternationEnabled: false,
    headerFilter: { visible: false },
    height: function () {
        return window.innerHeight / 1.4;
    },
    editing: {
        mode: 'cell',
        allowUpdating: false,
        allowDeleting: false,
        allowAdding: false,
        allowRowEditing: false,
    },
    loadPanel: {
        enabled: true,
        height: 90,
        width: 200,
        text: 'Data is loading.....'
    },
    dataField: 'PMQty',
    validationRules: [{ type: 'required' }, {
        message: 'You are requested to first enter the value in the grid of the  PMQty.!',
    }],
    onRowPrepared: function (e) {
        if (e.rowType == "header") {
            e.rowElement.css('background', '#42909A');
            e.rowElement.css('color', ' white');
            e.rowElement.css('font-black', 'bold');
        }
        e.rowElement.css('fontSize', '15px');
    },

    onEditorPreparing: function (e) {
        if (e.parentType == 'headerRow' && e.command == 'select') {
            e.editorElement.remove();
        }
    },
    onSelectionChanged: function (clicked) {
        GridSelectedData = [];
        GridSelectedData = clicked.selectedRowsData;
    },

});

function fillGridPending(PendingList) {
    $("#POGridPending").dxDataGrid({
        dataSource: PendingList
    });
    var grid = $("#POGridPending").dxDataGrid('instance');
    grid.clearSelection();
}

function fillGridPOSuccess(ProcessRESS) {
    $("#POGridProcess").dxDataGrid({ dataSource: ProcessRESS });
    var grid = $("#POGridProcess").dxDataGrid('instance');
    grid.clearSelection();
}

$("#TxtAddPayTerms").dxSelectBox({
    items: [
        "Payment in 30 Days",
        "Payment in 60 Days",
        "Payment in 90 Days",
        "Against delivery",
        "Advance"
    ],
    value: null,
    placeholder: "Select Payment Terms",
    searchEnabled: true,
    searchMode: "contains",
    showClearButton: true,
    acceptCustomValue: true, // ✅ Required for allowing typed custom values change by ankit 25-05-2025
    onCustomItemCreating: function (e) {
        let newValue = e.text;
        let currentItems = e.component.option("items");
        if (!currentItems.includes(newValue)) {
            currentItems.push(newValue);
            e.component.option("items", currentItems);
        }

        e.customItem = newValue;
    }
});



$("#CreatePOButton").click(function () {
    AuthenticateCurdActions(GBLProductionUnitID).then(isAuthorized => {
        if (!isAuthorized) {
            return;
        }

        SelectedProductionUnitID = 0;
        SelectedProductionUnitName = '';
        TotalGstAmt = 0; //Edit By Pradeep 06 sept 2019
        VarItemApproved = "";
        $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
        document.getElementById("POPrintButton").disabled = true;
        $("#TxtAddPayTerms").dxSelectBox("instance").option("value", null);
        let currencyCode = (GblCompanyConfiguration !== undefined && GblCompanyConfiguration !== null && GblCompanyConfiguration.length > 0) ? GblCompanyConfiguration[0].CurrencyCode : "INR";
        $("#SelLnameChargesGrid").dxSelectBox({ value: '' });
        $("#SelCurrencyCode").dxSelectBox({ value: currencyCode });
        ModalPopupScreencontrols();
        DistinctArray = [];
        ScheduleListOBJ = [];
        document.getElementById("BtnDeletePopUp").disabled = true;
        document.getElementById("BtnSaveAS").disabled = true;
        GblStatus = "";
        //PaymentTermsString = "Payment in 30 Days,Payment in 60 Days,Payment in 90 Days";
        (purchaseConfiguration.PaymentTerms.length > 0) ? PaymentTermsString = purchaseConfiguration.PaymentTerms.map(function (ele) { return ele.PaymentTerms; }).join(',') : PaymentTermsString = "Payment in 30 Days,Payment in 60 Days,Payment in 90 Days";
        setLastTransactiondate();
        // fillPayTermsGrid();  change by ankit 25-05-2025
        if (GetPendingData.length === 0 || GetPendingData === [] || GetPendingData === "") {
            existReq = [];
            $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
            //existReq = existReq;

            //$("#SupplierName").dxSelectBox({
            //    items: SupplierDDL,
            //    valueExpr: "LedgerID", // Assuming this is the unique identifier
            //    displayExpr: "LedgerName", // Field to display in the dropdown
            //    //value: SupplierDDL.find(item => item.GSTApplicable === 'True')?.LedgerID, // Checks for 'True' as a string
            //    searchEnabled: true
            //});

            $("#SupplierName").dxSelectBox({
                items: SupplierDDL,
                valueExpr: "LedgerID", // Unique identifier
                displayExpr: "LedgerName", // Field to display in the dropdown
                value: null, // Default value is null
                searchEnabled: true,
                placeholder: "Select Supplier", // Placeholder text
                //onOpened: function (e) {
                //    let defaultItem = SupplierDDL.find(item => item.GSTApplicable === 'True')?.LedgerID;
                //    if (defaultItem) {
                //        e.component.option("value", defaultItem.LedgerID);
                //    }
                //}
            });
        }
        else {
            $.ajax({
                type: "POST",
                //url: "WebService_PurchaseOrder.asmx/GetAllotedSupp",
                //data: '{ItemGroupID:' + JSON.stringify(GetPendingData[0].ItemGroupID) + '}',
                url: "WebService_PurchaseOrder.asmx/Supplier",
                data: '{}',
                contentType: "application/json; charset=utf-8",
                dataType: "text",
                success: function (results) {
                    var res = results.replace(/\\/g, '');
                    res = res.replace(/"d":""/g, '');
                    res = res.replace(/""/g, '');
                    res = res.replace(/u0026/g, '&');
                    res = res.replace(/u0027/g, "'");
                    res = res.replace(/:,/g, ":null,");
                    res = res.replace(/,}/g, ",null}");
                    res = res.substr(1);
                    res = res.slice(0, -1);
                    var FilteredSupp = JSON.parse(res);

                    //$("#SupplierName").dxSelectBox({
                    //    items: FilteredSupp,
                    //    valueExpr: "LedgerID",
                    //    displayExpr: "LedgerName",
                    //    value: FilteredSupp.find(item => item.GSTApplicable === 'True')?.LedgerID,
                    //    searchEnabled: true
                    //});

                    $("#SupplierName").dxSelectBox({
                        items: FilteredSupp,
                        valueExpr: "LedgerID",
                        displayExpr: "LedgerName",
                        value: null,
                        searchEnabled: true,
                        placeholder: "Select Supplier",
                        //onOpened: function (e) {
                        //    let defaultItem = FilteredSupp.find(item => item.GSTApplicable === 'True')?.LedgerID;
                        //    if (defaultItem) {
                        //        e.component.option("value", defaultItem.LedgerID);
                        //    }
                        //}
                    });

                    if (FilteredSupp.length == 1) {
                        $("#SupplierName").dxSelectBox({
                            value: FilteredSupp[0].LedgerID
                        });
                    }
                }
            });

            MasterGridData = [];
            SubGridData = [];
            var MasterGridOpt = {};
            var SubGridOpt = {};

            var FinalQty = 0;
            var FinalQtyInStockUnit = 0;
            var FinalQtyInPurchaseUnit = 0;
            var RequiredQuantity = 0;
            var RequiredQuantityInStockUnit = 0;
            var RequiredQuantityInPurchaseUnit = 0;
            var WholeGetPendingData = "";
            var newArray = [];
            var currentdate = new Date().toISOString().substr(0, 10);

            var Qty = "";
            var QtyInStockUnit = "";
            var Var_ItemGroupNameID = 0;
            var Var_WtPerPacking = 0;
            var Var_UnitPerPacking = 0;
            var Var_SizeW = 0;
            var Var_ConversionFactor = 0;
            var Var_ConversionFormula = "", Var_UnitDecimalPlace = "";
            var JobCardNumbers = "";
            var JobCardList = [];
            var JobCardIDs = "";
            var JobCardIDList = [];
            WholeGetPendingData = { 'AllGetPendingData': GetPendingData };

            for (var d = 0; d < GetPendingData.length; d++) {
                FinalQty = 0;
                FinalQtyInStockUnit = 0;
                FinalQtyInPurchaseUnit = 0;
                RequiredQuantity = 0;
                RequiredQuantityInStockUnit = 0;
                RequiredQuantityInPurchaseUnit = 0;
                newArray = [];

                if (MasterGridData.length === 0 || MasterGridData === "" || MasterGridData === undefined || MasterGridData === null) {

                    newArray = WholeGetPendingData.AllGetPendingData.filter(function (el) {
                        return el.ItemID === GetPendingData[d].ItemID;
                    });

                    JobCardNumbers = "";
                    JobCardList = [];
                    JobCardIDs = "";
                    JobCardIDList = [];
                    var ddlSupplierId = $('#SupplierName').dxSelectBox('instance').option('value');
                    let LblSupplierStateTin = document.getElementById("LblSupplierStateTin").innerHTML;

                    for (var f = 0; f < newArray.length; f++) {
                        if (newArray[f].OrderUnit.toString().toUpperCase() === newArray[f].PurchaseUnit.toString().toUpperCase() && newArray[f].OrderUnit.toString().toUpperCase() === newArray[f].StockUnit.toString().toUpperCase()) {
                            FinalQtyInPurchaseUnit = FinalQtyInPurchaseUnit + Number(newArray[f].PurchaseQuantity);
                            FinalQtyInStockUnit = FinalQtyInStockUnit + Number(newArray[f].PurchaseQuantity);
                            RequiredQuantityInPurchaseUnit = RequiredQuantityInPurchaseUnit + Number(newArray[f].RequiredQuantity);
                            RequiredQuantityInStockUnit = RequiredQuantityInStockUnit + Number(newArray[f].RequiredQuantity);
                        } else if (newArray[f].OrderUnit.toString().toUpperCase() !== newArray[f].PurchaseUnit.toString().toUpperCase()) {
                            if (newArray[f].ConversionFormula === undefined || newArray[f].ConversionFormula === null) newArray[f].ConversionFormula = "";
                            FinalQtyInPurchaseUnit = FinalQtyInPurchaseUnit + Number(StockUnitConversion(newArray[f].ConversionFormula.toString(), Number(newArray[f].PurchaseQuantity), Number(newArray[f].UnitPerPacking), Number(newArray[f].WtPerPacking), Number(newArray[f].ConversionFactor), Number(newArray[f].SizeW), Number(newArray[f].UnitDecimalPlace), newArray[f].StockUnit.toString(), newArray[f].PurchaseUnit.toString(), Number(newArray[f].GSM), Number(newArray[f].ReleaseGSM), Number(newArray[f].AdhesiveGSM), Number(newArray[f].Thickness), Number(newArray[f].Density)));
                            FinalQtyInStockUnit = FinalQtyInStockUnit + Number(newArray[f].PurchaseQuantity);
                            RequiredQuantityInPurchaseUnit = RequiredQuantityInPurchaseUnit + Number(StockUnitConversion(newArray[f].ConversionFormula.toString(), Number(newArray[f].RequiredQuantity), Number(newArray[f].UnitPerPacking), Number(newArray[f].WtPerPacking), Number(newArray[f].ConversionFactor), Number(newArray[f].SizeW), Number(newArray[f].UnitDecimalPlace), newArray[f].StockUnit.toString(), newArray[f].PurchaseUnit.toString(), Number(newArray[f].GSM), Number(newArray[f].ReleaseGSM), Number(newArray[f].AdhesiveGSM), Number(newArray[f].Thickness), Number(newArray[f].Density)));
                            RequiredQuantityInStockUnit = RequiredQuantityInStockUnit + Number(newArray[f].RequiredQuantity);
                        } else if (newArray[f].OrderUnit.toString().toUpperCase() === newArray[f].PurchaseUnit.toString().toUpperCase() && newArray[f].OrderUnit.toString().toUpperCase() !== newArray[f].StockUnit.toString().toUpperCase()) {
                            FinalQtyInPurchaseUnit = FinalQtyInPurchaseUnit + Number(newArray[f].PurchaseQuantity);
                            FinalQtyInStockUnit = FinalQtyInStockUnit + Number(StockUnitConversion(newArray[f].ConversionFormulaStockUnit.toString(), Number(newArray[f].PurchaseQuantity), Number(newArray[f].UnitPerPacking), Number(newArray[f].WtPerPacking), Number(newArray[f].ConversionFactor), Number(newArray[f].SizeW), Number(newArray[f].UnitDecimalPlaceStockUnit), newArray[f].PurchaseUnit.toString(), newArray[f].StockUnit.toString(), Number(newArray[f].GSM), Number(newArray[f].ReleaseGSM), Number(newArray[f].AdhesiveGSM), Number(newArray[f].Thickness), Number(newArray[f].Density)));
                            RequiredQuantityInPurchaseUnit = RequiredQuantityInPurchaseUnit + Number(newArray[f].RequiredQuantity);
                            RequiredQuantityInStockUnit = RequiredQuantityInStockUnit + Number(StockUnitConversion(newArray[f].ConversionFormulaStockUnit.toString(), Number(newArray[f].RequiredQuantity), Number(newArray[f].UnitPerPacking), Number(newArray[f].WtPerPacking), Number(newArray[f].ConversionFactor), Number(newArray[f].SizeW), Number(newArray[f].UnitDecimalPlaceStockUnit), newArray[f].PurchaseUnit.toString(), newArray[f].StockUnit.toString(), Number(newArray[f].GSM), Number(newArray[f].ReleaseGSM), Number(newArray[f].AdhesiveGSM), Number(newArray[f].Thickness), Number(newArray[f].Density)));
                        }
                        FinalQty = FinalQty + Number(newArray[f].PurchaseQuantity);
                        RequiredQuantity = RequiredQuantity + Number(newArray[f].RequiredQuantity);
                        if (newArray[f].RefJobCardContentNo !== "" && newArray[f].RefJobCardContentNo !== null && newArray[f].RefJobCardContentNo !== undefined) {
                            var JCNo = newArray[f].RefJobCardContentNo;
                            var JobCardArray = JCNo.split(",");
                            if (JobCardArray.length > 0) {
                                for (var ccount = 0; ccount < JobCardArray.length; ccount++) {
                                    var found = JobCardList.includes(JobCardArray[ccount]);
                                    if (found === false) {
                                        JobCardList.push(JobCardArray[ccount]);
                                    }
                                }
                            }
                        }

                        if (newArray[f].RefJobBookingJobCardContentsID !== "" && newArray[f].RefJobBookingJobCardContentsID !== null && newArray[f].RefJobBookingJobCardContentsID !== undefined) {
                            var JCID = newArray[f].RefJobBookingJobCardContentsID.toString();
                            var JobCardArray1 = JCID.split(",");
                            if (JobCardArray1.length > 0) {
                                for (var count = 0; count < JobCardArray1.length; count++) {
                                    var found1 = JobCardIDList.includes(JobCardArray1[count]);
                                    if (found1 === false) {
                                        JobCardIDList.push(JobCardArray1[count]);
                                    }
                                }
                            }
                        }
                        SubGridOpt = {};
                        SubGridOpt.TransactionID = newArray[f].TransactionID;//SubGrid  
                        SubGridOpt.TransID = newArray[f].TransID;
                        SubGridOpt.VoucherID = newArray[f].VoucherID;
                        SubGridOpt.ItemGroupID = newArray[f].ItemGroupID;
                        SubGridOpt.ItemGroupNameID = newArray[f].ItemGroupNameID;
                        SubGridOpt.ItemSubGroupID = newArray[f].ItemSubGroupID;
                        SubGridOpt.ItemID = newArray[f].ItemID;
                        SubGridOpt.MaxVoucherNo = newArray[f].MaxVoucherNo;
                        //SubGridOpt.ItemGroupNameID = newArray[f].ItemGroupNameID;
                        SubGridOpt.VoucherNo = newArray[f].VoucherNo;
                        SubGridOpt.VoucherDate = newArray[f].VoucherDate;
                        SubGridOpt.ItemCode = newArray[f].ItemCode;
                        SubGridOpt.ItemGroupName = newArray[f].ItemGroupName;
                        SubGridOpt.ItemSubGroupName = newArray[f].ItemSubGroupName;
                        SubGridOpt.ItemName = newArray[f].ItemName;
                        SubGridOpt.RefJobCardContentNo = newArray[f].RefJobCardContentNo;
                        SubGridOpt.ItemDescription = newArray[f].ItemDescription;
                        SubGridOpt.RequisitionQty = newArray[f].RequiredQuantity;
                        SubGridOpt.PurchaseQuantityComp = newArray[f].PurchaseQuantity;
                        SubGridOpt.RequiredQuantity = newArray[f].PurchaseQuantity;
                        SubGridOpt.StockUnit = newArray[f].OrderUnit;
                        SubGridOpt.ItemNarration = newArray[f].ItemNarration;
                        SubGridOpt.CreatedBy = newArray[f].CreatedBy;
                        SubGridOpt.JobName = newArray[f].JobName;
                        SubGridOpt.PurchaseUnit = newArray[f].PurchaseUnit;
                        SubGridOpt.GSTTaxPercentage = newArray[f].GSTTaxPercentage;
                        SubGridOpt.CGSTTaxPercentage = newArray[f].CGSTTaxPercentage;
                        SubGridOpt.SGSTTaxPercentage = newArray[f].SGSTTaxPercentage;
                        SubGridOpt.IGSTTaxPercentage = newArray[f].IGSTTaxPercentage;
                        SubGridOpt.Narration = newArray[f].Narration;
                        SubGridOpt.FYear = newArray[f].FYear;
                        SubGridOpt.PurchaseRate = newArray[f].PurchaseRate;
                        SubGridOpt.ProductHSNName = newArray[f].ProductHSNName;
                        SubGridOpt.HSNCode = newArray[f].HSNCode;

                        SubGridData.push(SubGridOpt);

                    }
                    MasterGridOpt = {};

                    MasterGridOpt.TransactionID = newArray[0].TransactionID;//MasterGrid  
                    MasterGridOpt.TransID = newArray[0].TransID;
                    MasterGridOpt.VoucherID = newArray[0].VoucherID;
                    MasterGridOpt.ItemGroupID = newArray[0].ItemGroupID;
                    MasterGridOpt.ItemGroupNameID = newArray[0].ItemGroupNameID;
                    MasterGridOpt.ItemSubGroupID = newArray[0].ItemSubGroupID;
                    MasterGridOpt.ItemID = newArray[0].ItemID;
                    MasterGridOpt.MaxVoucherNo = newArray[0].MaxVoucherNo;
                    if (currentdate > Date.parse(newArray[0].ExpectedDeliveryDate)) {
                        MasterGridOpt.ExpectedDeliveryDate = currentdate;
                    } else {
                        MasterGridOpt.ExpectedDeliveryDate = newArray[0].ExpectedDeliveryDate;
                    }

                    MasterGridOpt.VoucherNo = newArray[0].VoucherNo;
                    MasterGridOpt.VoucherDate = newArray[0].VoucherDate;
                    MasterGridOpt.ItemCode = newArray[0].ItemCode;
                    MasterGridOpt.ItemGroupName = newArray[0].ItemGroupName;
                    MasterGridOpt.ItemSubGroupName = newArray[0].ItemSubGroupName;
                    MasterGridOpt.ItemName = newArray[0].ItemName;
                    MasterGridOpt.ItemDescription = newArray[0].ItemDescription;
                    MasterGridOpt.RefJobCardContentNo = JobCardList.join();
                    MasterGridOpt.RefJobBookingJobCardContentsID = JobCardIDList.join();
                    //MasterGridOpt.RequiredQuantity = newArray[0].PurchaseQuantity;//RequiredQuantity

                    Qty = "";
                    QtyInStockUnit = "";
                    Var_ItemGroupNameID = 0;
                    Var_WtPerPacking = 0;
                    Var_UnitPerPacking = 0;
                    Var_SizeW = 0;
                    Var_ConversionFactor = 0;
                    Var_ConversionFormula = "";

                    //if (FinalQty === "" || FinalQty === undefined || FinalQty === null || FinalQty === "NULL") {
                    //    Qty = 0;
                    //    MasterGridOpt.RequiredQuantity = Qty;
                    //}
                    if (FinalQtyInStockUnit === "" || FinalQtyInStockUnit === undefined || FinalQtyInStockUnit === null || FinalQtyInStockUnit === "NULL") {
                        Qty = 0;
                        MasterGridOpt.RequiredQuantity = Qty;
                        MasterGridOpt.RequiredQuantityInPurchaseUnit = Qty;
                    }
                    else {
                        //Start New Code for Change Sheets
                        Var_ItemGroupNameID = newArray[0].ItemGroupNameID;
                        Var_WtPerPacking = newArray[0].WtPerPacking;
                        Var_UnitPerPacking = newArray[0].UnitPerPacking;
                        Var_SizeW = newArray[0].SizeW;
                        Var_ConversionFactor = newArray[0].ConversionFactor;
                        Var_ConversionFormula = newArray[0].ConversionFormula;
                        Var_UnitDecimalPlace = newArray[0].UnitDecimalPlace;

                        if (Var_ConversionFormula === "" || Var_ConversionFormula === undefined || Var_ConversionFormula === null) {
                            //Qty = parseFloat(Number(FinalQty)).toFixed(Number(Var_UnitDecimalPlace));
                            Qty = parseFloat(Number(FinalQtyInPurchaseUnit)).toFixed(Number(Var_UnitDecimalPlace));
                            MasterGridOpt.RequiredQuantity = parseFloat(Number(FinalQtyInStockUnit)).toFixed(Number(newArray[0].UnitDecimalPlaceStockUnit));
                            QtyInStockUnit = parseFloat(Number(FinalQtyInStockUnit)).toFixed(Number(newArray[0].UnitDecimalPlaceStockUnit));
                            MasterGridOpt.RequiredQuantityInPurchaseUnit = Qty;
                        } else {
                            if (Var_UnitPerPacking === 0) { Var_UnitPerPacking = 1; }
                            if (Var_ConversionFactor === 0) { Var_ConversionFactor = 1; }
                            if (Var_SizeW === 0) { Var_SizeW = 1; }
                            MasterGridOpt.RequiredQuantity = FinalQtyInStockUnit;
                            QtyInStockUnit = FinalQtyInStockUnit;
                            Qty = parseFloat(Number(FinalQtyInPurchaseUnit)).toFixed(Number(Var_UnitDecimalPlace));
                            MasterGridOpt.RequiredQuantityInPurchaseUnit = Qty;
                            ////Commented By Minesh Jain on 01-Oct-19 Conversion done by function at the start of loop
                            //MasterGridOpt.RequiredQuantity = FinalQty;
                            //Var_ConversionFormula = Var_ConversionFormula.split('e.').join('')
                            //Var_ConversionFormula = Var_ConversionFormula.replace("Quantity", "FinalQty");
                            //Var_ConversionFormula = Var_ConversionFormula.replace("UnitPerPacking", "Var_UnitPerPacking");
                            //Var_ConversionFormula = Var_ConversionFormula.replace("WtPerPacking", "Var_WtPerPacking");
                            //Var_ConversionFormula = Var_ConversionFormula.replace("SizeW", "Var_SizeW");
                            //Var_ConversionFormula = Var_ConversionFormula.replace("UnitDecimalPlace", "Var_UnitDecimalPlace");

                            //Qty = parseFloat(Number(eval(Var_ConversionFormula))).toFixed(Number(Var_UnitDecimalPlace));
                        }

                        //Close New Code for Change Sheets
                        //Qty = FinalQty;
                    }

                    var PurchaseRate = "";
                    if (newArray[0].PurchaseRate === "" || newArray[0].PurchaseRate === undefined || newArray[0].PurchaseRate === null || newArray[0].PurchaseRate === "NULL") {
                        PurchaseRate = 0;
                    }
                    else {
                        PurchaseRate = newArray[0].PurchaseRate;
                    }

                    /////***************

                    if (ddlSupplierId === "" || ddlSupplierId === null || ddlSupplierId === undefined || ddlSupplierId === "NULL") {
                        PurchaseRate = PurchaseRate;
                    }
                    else {
                        ObjItemRate = [];

                        ObjItemRate = ItemRateString.ItemRateObj.filter(function (el) {
                            return el.LedgerID === ddlSupplierId &&
                                el.ItemID === newArray[0].ItemID;
                        });
                        if (ObjItemRate === [] || ObjItemRate === "" || ObjItemRate === undefined) {
                            PurchaseRate = PurchaseRate;
                        } else {
                            PurchaseRate = ObjItemRate[0].PurchaseRate;
                        }
                    }

                    //PurchaseRate = 65;

                    var BasicAmt = 0;
                    BasicAmt = parseFloat(Number(Number(Qty) * Number(PurchaseRate))).toFixed(2);

                    var DisPercentage = 0;
                    var TaxAbleAmt = 0;
                    var DiscountAmt = Number(Number(BasicAmt) * Number(DisPercentage)) / 100;

                    TaxAbleAmt = Number(BasicAmt) - Number(DiscountAmt);

                    var IGSTPER = 0, SGSTPER = 0, CGSTPER = 0;
                    var IGSTAMT = 0, SGSTAMT = 0, CGSTAMT = 0;
                    var TotalAmount = 0;
                    if (GblCompanyConfiguration[0].IsGstApplicable === true) {
                        if (Number(LblSupplierStateTin) === Number(GblCompanyStateTin)) {
                            if (newArray[0].CGSTTaxPercentage === "" || newArray[0].CGSTTaxPercentage === undefined || newArray[0].CGSTTaxPercentage === null || newArray[0].CGSTTaxPercentage === "NULL") {
                                CGSTPER = 0;
                            }
                            else {
                                CGSTPER = newArray[0].CGSTTaxPercentage;
                            }
                            if (newArray[0].SGSTTaxPercentage === "" || newArray[0].SGSTTaxPercentage === undefined || newArray[0].SGSTTaxPercentage === null || newArray[0].SGSTTaxPercentage === "NULL") {
                                SGSTPER = 0;
                            }
                            else {
                                SGSTPER = newArray[0].SGSTTaxPercentage;
                            }
                            SGSTAMT = Number(Number(TaxAbleAmt) * Number(SGSTPER)) / 100;
                            CGSTAMT = Number(Number(TaxAbleAmt) * Number(CGSTPER)) / 100;
                            TotalAmount = Number(SGSTAMT) + Number(CGSTAMT) + Number(TaxAbleAmt);
                        }
                        else {
                            if (newArray[0].IGSTTaxPercentage === "" || newArray[0].IGSTTaxPercentage === undefined || newArray[0].IGSTTaxPercentage === null || newArray[0].IGSTTaxPercentage === "NULL") {
                                IGSTPER = 0;
                            }
                            else {
                                IGSTPER = newArray[0].IGSTTaxPercentage;
                            }

                            IGSTAMT = Number(Number(TaxAbleAmt) * Number(IGSTPER)) / 100;
                            TotalAmount = Number(IGSTAMT) + Number(TaxAbleAmt);
                        }
                    } else if (GblCompanyConfiguration[0].IsVatApplicable === true) {
                        if (newArray[0].IGSTTaxPercentage === "" || newArray[0].IGSTTaxPercentage === undefined || newArray[0].IGSTTaxPercentage === null || newArray[0].IGSTTaxPercentage === "NULL") {
                            IGSTPER = 0;
                        }
                        else {
                            IGSTPER = newArray[0].IGSTTaxPercentage;
                        }

                        IGSTAMT = Number(Number(TaxAbleAmt) * Number(IGSTPER)) / 100;
                        TotalAmount = Number(IGSTAMT) + Number(TaxAbleAmt);
                    }


                    MasterGridOpt.PurchaseQuantityComp = Qty;
                    MasterGridOpt.PurchaseQuantity = Qty;
                    MasterGridOpt.PurchaseQuantityInStockUnit = QtyInStockUnit;

                    if (Number(newArray[0].QuantityPerPack) > 0) {
                        MasterGridOpt.QuantityPerPack = Number(newArray[0].QuantityPerPack);
                    } else {
                        MasterGridOpt.QuantityPerPack = Number(newArray[0].UnitPerPacking);
                    }
                    (Number(MasterGridOpt.QuantityPerPack) > 0) ? MasterGridOpt.QuantityPerPack = Number(MasterGridOpt.QuantityPerPack) : MasterGridOpt.QuantityPerPack = 1;

                    MasterGridOpt.RequiredNoOfPacks = (Number(MasterGridOpt.PurchaseQuantityInStockUnit) / Number(MasterGridOpt.QuantityPerPack)).toFixed(2);

                    MasterGridOpt.StockUnit = newArray[0].StockUnit;
                    MasterGridOpt.CreatedBy = newArray[0].CreatedBy;
                    MasterGridOpt.ItemNarration = newArray[0].ItemNarration;
                    MasterGridOpt.JobName = newArray[0].JobName;
                    MasterGridOpt.PurchaseUnit = newArray[0].PurchaseUnit;
                    MasterGridOpt.BasicAmount = BasicAmt;
                    MasterGridOpt.Disc = DisPercentage;
                    MasterGridOpt.Tolerance = newArray[0].Tolerance;
                    MasterGridOpt.AfterDisAmt = TaxAbleAmt;
                    MasterGridOpt.TaxableAmount = TaxAbleAmt;

                    MasterGridOpt.GSTTaxPercentage = newArray[0].GSTTaxPercentage;
                    MasterGridOpt.CGSTTaxPercentage = newArray[0].CGSTTaxPercentage;
                    MasterGridOpt.SGSTTaxPercentage = newArray[0].SGSTTaxPercentage;
                    MasterGridOpt.IGSTTaxPercentage = newArray[0].IGSTTaxPercentage;

                    MasterGridOpt.CGSTAmt = CGSTAMT;
                    MasterGridOpt.SGSTAmt = SGSTAMT;
                    MasterGridOpt.IGSTAmt = IGSTAMT;
                    MasterGridOpt.TotalAmount = TotalAmount;

                    MasterGridOpt.Narration = newArray[0].Narration;
                    MasterGridOpt.FYear = newArray[0].FYear;
                    MasterGridOpt.PurchaseRate = PurchaseRate;
                    MasterGridOpt.ProductHSNName = newArray[0].ProductHSNName;
                    MasterGridOpt.HSNCode = newArray[0].HSNCode;
                    MasterGridOpt.WtPerPacking = newArray[0].WtPerPacking;
                    MasterGridOpt.UnitPerPacking = newArray[0].UnitPerPacking;
                    MasterGridOpt.ConversionFactor = newArray[0].ConversionFactor;
                    MasterGridOpt.ConversionFormula = newArray[0].ConversionFormula;
                    MasterGridOpt.UnitDecimalPlace = newArray[0].UnitDecimalPlace;
                    MasterGridOpt.ConversionFormulaStockUnit = newArray[0].ConversionFormulaStockUnit;
                    MasterGridOpt.UnitDecimalPlaceStockUnit = newArray[0].UnitDecimalPlaceStockUnit;
                    MasterGridOpt.SizeW = newArray[0].SizeW;
                    MasterGridOpt.GSM = newArray[0].GSM;
                    MasterGridOpt.ReleaseGSM = newArray[0].ReleaseGSM;
                    MasterGridOpt.AdhesiveGSM = newArray[0].AdhesiveGSM;
                    MasterGridOpt.Thickness = newArray[0].Thickness;
                    MasterGridOpt.Density = newArray[0].Density;
                    // MasterGridOpt.Schedule = Addfun;

                    MasterGridData.push(MasterGridOpt);
                }
                else {
                    //var extobj = JSON.stringify(MasterGridData);

                    //var existID = extobj.includes(GetPendingData[d].ItemID);

                    //if (existID !== true) {
                    //    newArray = WholeGetPendingData.AllGetPendingData.filter(function (el) {
                    //        return el.ItemID === GetPendingData[d].ItemID;
                    //    });
                    var existID = MasterGridData.filter(function (el) {
                        return el.ItemID === GetPendingData[d].ItemID;
                    });
                    JobCardNumbers = "";
                    JobCardList = [];
                    JobCardIDs = "";
                    JobCardIDList = [];

                    if (existID.length === 0) {
                        newArray = WholeGetPendingData.AllGetPendingData.filter(function (el) {
                            return el.ItemID === GetPendingData[d].ItemID;
                        });
                        for (f = 0; f < newArray.length; f++) {
                            if (newArray[f].ConversionFormula === undefined || newArray[f].ConversionFormula === null) newArray[f].ConversionFormula = "";
                            if (newArray[f].OrderUnit.toString().toUpperCase() === newArray[f].PurchaseUnit.toString().toUpperCase() && newArray[f].OrderUnit.toString().toUpperCase() === newArray[f].StockUnit.toString().toUpperCase()) {
                                FinalQtyInPurchaseUnit = FinalQtyInPurchaseUnit + Number(newArray[f].PurchaseQuantity);
                                FinalQtyInStockUnit = FinalQtyInStockUnit + Number(newArray[f].PurchaseQuantity);
                                RequiredQuantityInPurchaseUnit = RequiredQuantityInPurchaseUnit + Number(newArray[f].RequiredQuantity);
                                RequiredQuantityInStockUnit = RequiredQuantityInStockUnit + Number(newArray[f].RequiredQuantity);
                            } else if (newArray[f].OrderUnit.toString().toUpperCase() !== newArray[f].PurchaseUnit.toString().toUpperCase()) {
                                FinalQtyInPurchaseUnit = FinalQtyInPurchaseUnit + Number(StockUnitConversion(newArray[f].ConversionFormula.toString(), Number(newArray[f].PurchaseQuantity), Number(newArray[f].UnitPerPacking), Number(newArray[f].WtPerPacking), Number(newArray[f].ConversionFactor), Number(newArray[f].SizeW), Number(newArray[f].UnitDecimalPlace), newArray[f].StockUnit.toString(), newArray[f].PurchaseUnit.toString(), Number(newArray[f].GSM), Number(newArray[f].ReleaseGSM), Number(newArray[f].AdhesiveGSM), Number(newArray[f].Thickness), Number(newArray[f].Density)));
                                FinalQtyInStockUnit = FinalQtyInStockUnit + Number(newArray[f].PurchaseQuantity);
                                RequiredQuantityInPurchaseUnit = RequiredQuantityInPurchaseUnit + Number(StockUnitConversion(newArray[f].ConversionFormula.toString(), Number(newArray[f].RequiredQuantity), Number(newArray[f].UnitPerPacking), Number(newArray[f].WtPerPacking), Number(newArray[f].ConversionFactor), Number(newArray[f].SizeW), Number(newArray[f].UnitDecimalPlace), newArray[f].StockUnit.toString(), newArray[f].PurchaseUnit.toString(), Number(newArray[f].GSM), Number(newArray[f].ReleaseGSM), Number(newArray[f].AdhesiveGSM), Number(newArray[f].Thickness), Number(newArray[f].Density)));
                                RequiredQuantityInStockUnit = RequiredQuantityInStockUnit + Number(newArray[f].RequiredQuantity);
                            } else if (newArray[f].OrderUnit.toString().toUpperCase() === newArray[f].PurchaseUnit.toString().toUpperCase() && newArray[f].OrderUnit.toString().toUpperCase() !== newArray[f].StockUnit.toString().toUpperCase()) {
                                FinalQtyInPurchaseUnit = FinalQtyInPurchaseUnit + Number(newArray[f].PurchaseQuantity);
                                FinalQtyInStockUnit = FinalQtyInStockUnit + Number(StockUnitConversion(newArray[f].ConversionFormulaStockUnit.toString(), Number(newArray[f].PurchaseQuantity), Number(newArray[f].UnitPerPacking), Number(newArray[f].WtPerPacking), Number(newArray[f].ConversionFactor), Number(newArray[f].SizeW), Number(newArray[f].UnitDecimalPlaceStockUnit), newArray[f].PurchaseUnit.toString(), newArray[f].StockUnit.toString(), Number(newArray[f].GSM), Number(newArray[f].ReleaseGSM), Number(newArray[f].AdhesiveGSM), Number(newArray[f].Thickness), Number(newArray[f].Density)));
                                RequiredQuantityInPurchaseUnit = RequiredQuantityInPurchaseUnit + Number(newArray[f].RequiredQuantity);
                                RequiredQuantityInStockUnit = RequiredQuantityInStockUnit + Number(StockUnitConversion(newArray[f].ConversionFormulaStockUnit.toString(), Number(newArray[f].RequiredQuantity), Number(newArray[f].UnitPerPacking), Number(newArray[f].WtPerPacking), Number(newArray[f].ConversionFactor), Number(newArray[f].SizeW), Number(newArray[f].UnitDecimalPlaceStockUnit), newArray[f].PurchaseUnit.toString(), newArray[f].StockUnit.toString(), Number(newArray[f].GSM), Number(newArray[f].ReleaseGSM), Number(newArray[f].AdhesiveGSM), Number(newArray[f].Thickness), Number(newArray[f].Density)));
                            }

                            FinalQty = FinalQty + Number(newArray[f].PurchaseQuantity);
                            RequiredQuantity = RequiredQuantity + Number(newArray[f].RequiredQuantity);
                            if (newArray[f].RefJobCardContentNo !== "" && newArray[f].RefJobCardContentNo !== null && newArray[f].RefJobCardContentNo !== undefined) {
                                var JobCardArrayx = newArray[f].RefJobCardContentNo.split(",");
                                if (JobCardArrayx.length > 0) {
                                    for (var ccounti = 0; ccounti < JobCardArrayx.length; ccounti++) {
                                        var foundx = JobCardList.includes(JobCardArrayx[ccounti]);
                                        if (foundx === false) {
                                            JobCardList.push(JobCardArrayx[ccounti]);
                                        }
                                    }
                                }
                            }

                            if (newArray[f].RefJobBookingJobCardContentsID !== "" && newArray[f].RefJobBookingJobCardContentsID !== null && newArray[f].RefJobBookingJobCardContentsID !== undefined) {
                                var JobCardArray2 = newArray[f].RefJobBookingJobCardContentsID.toString().split(",");
                                if (JobCardArray2.length > 0) {
                                    for (var count1 = 0; count1 < JobCardArray2.length; count1++) {
                                        var foundE = JobCardIDList.includes(JobCardArray2[count1]);
                                        if (foundE === false) {
                                            JobCardIDList.push(JobCardArray2[count1]);
                                        }
                                    }
                                }
                            }

                            SubGridOpt = {};
                            SubGridOpt.TransactionID = newArray[f].TransactionID;//SubGrid  
                            SubGridOpt.TransID = newArray[f].TransID;
                            SubGridOpt.VoucherID = newArray[f].VoucherID;
                            SubGridOpt.ItemGroupID = newArray[f].ItemGroupID;
                            SubGridOpt.ItemGroupNameID = newArray[f].ItemGroupNameID;
                            SubGridOpt.ItemSubGroupID = newArray[f].ItemSubGroupID;
                            SubGridOpt.ItemID = newArray[f].ItemID;
                            SubGridOpt.MaxVoucherNo = newArray[f].MaxVoucherNo;
                            //SubGridOpt.ItemGroupNameID = newArray[f].ItemGroupNameID;
                            SubGridOpt.VoucherNo = newArray[f].VoucherNo;
                            SubGridOpt.VoucherDate = newArray[f].VoucherDate;
                            SubGridOpt.ItemCode = newArray[f].ItemCode;
                            SubGridOpt.ItemGroupName = newArray[f].ItemGroupName;
                            SubGridOpt.ItemSubGroupName = newArray[f].ItemSubGroupName;
                            SubGridOpt.ItemName = newArray[f].ItemName;
                            SubGridOpt.RefJobCardContentNo = newArray[f].RefJobCardContentNo;
                            SubGridOpt.ItemDescription = newArray[f].ItemDescription;
                            SubGridOpt.RequisitionQty = newArray[f].RequiredQuantity;
                            SubGridOpt.PurchaseQuantityComp = newArray[f].PurchaseQuantity;
                            SubGridOpt.RequiredQuantity = newArray[f].PurchaseQuantity;
                            SubGridOpt.StockUnit = newArray[f].OrderUnit;
                            SubGridOpt.CreatedBy = newArray[f].CreatedBy;
                            SubGridOpt.ItemNarration = newArray[f].ItemNarration;
                            SubGridOpt.JobName = newArray[f].JobName;
                            SubGridOpt.PurchaseUnit = newArray[f].PurchaseUnit;

                            SubGridOpt.GSTTaxPercentage = newArray[f].GSTTaxPercentage;
                            SubGridOpt.CGSTTaxPercentage = newArray[f].CGSTTaxPercentage;
                            SubGridOpt.SGSTTaxPercentage = newArray[f].SGSTTaxPercentage;
                            SubGridOpt.IGSTTaxPercentage = newArray[f].IGSTTaxPercentage;
                            SubGridOpt.Narration = newArray[f].Narration;
                            SubGridOpt.FYear = newArray[f].FYear;
                            SubGridOpt.PurchaseRate = newArray[f].PurchaseRate;
                            SubGridOpt.ProductHSNName = newArray[f].ProductHSNName;
                            SubGridOpt.HSNCode = newArray[f].HSNCode;

                            SubGridData.push(SubGridOpt);
                        }

                        MasterGridOpt = {};

                        MasterGridOpt.TransactionID = newArray[0].TransactionID;//MasterGrid  
                        MasterGridOpt.TransID = newArray[0].TransID;
                        MasterGridOpt.VoucherID = newArray[0].VoucherID;
                        MasterGridOpt.ItemGroupID = newArray[0].ItemGroupID;
                        MasterGridOpt.ItemGroupNameID = newArray[0].ItemGroupNameID;
                        MasterGridOpt.ItemSubGroupID = newArray[0].ItemSubGroupID;
                        MasterGridOpt.ItemID = newArray[0].ItemID;
                        MasterGridOpt.MaxVoucherNo = newArray[0].MaxVoucherNo;
                        if (currentdate > Date.parse(newArray[0].ExpectedDeliveryDate)) {
                            MasterGridOpt.ExpectedDeliveryDate = currentdate;
                        } else {
                            MasterGridOpt.ExpectedDeliveryDate = newArray[0].ExpectedDeliveryDate;
                        }
                        //MasterGridOpt.ExpectedDeliveryDate = newArray[0].ExpectedDeliveryDate;
                        MasterGridOpt.VoucherNo = newArray[0].VoucherNo;
                        MasterGridOpt.VoucherDate = newArray[0].VoucherDate;
                        MasterGridOpt.ItemCode = newArray[0].ItemCode;
                        MasterGridOpt.ItemGroupName = newArray[0].ItemGroupName;
                        MasterGridOpt.ItemSubGroupName = newArray[0].ItemSubGroupName;
                        MasterGridOpt.ItemName = newArray[0].ItemName;
                        MasterGridOpt.ItemDescription = newArray[0].ItemDescription;
                        MasterGridOpt.RefJobCardContentNo = JobCardList.join();
                        MasterGridOpt.RefJobBookingJobCardContentsID = JobCardIDList.join();
                        //MasterGridOpt.RequiredQuantity = newArray[0].PurchaseQuantity; //RequiredQuantity;

                        Qty = "";
                        QtyInStockUnit = "";
                        //if (FinalQty === "" || FinalQty === undefined || FinalQty === null || FinalQty === "NULL") {
                        //    Qty = 0;
                        //    MasterGridOpt.RequiredQuantity = Qty;
                        //}
                        if (FinalQtyInStockUnit === "" || FinalQtyInStockUnit === undefined || FinalQtyInStockUnit === null || FinalQtyInStockUnit === "NULL") {
                            Qty = 0;
                            QtyInStockUnit = 0;
                            MasterGridOpt.RequiredQuantity = Qty;
                            MasterGridOpt.RequiredQuantityInPurchaseUnit = Qty;
                        }
                        else {
                            //Start New Code for Change Sheets
                            Var_ItemGroupNameID = newArray[0].ItemGroupNameID;
                            Var_WtPerPacking = newArray[0].WtPerPacking;
                            Var_UnitPerPacking = newArray[0].UnitPerPacking;
                            Var_ConversionFactor = newArray[0].ConversionFactor;
                            Var_SizeW = newArray[0].SizeW;
                            Var_ConversionFormula = newArray[0].ConversionFormula;
                            Var_UnitDecimalPlace = newArray[0].UnitDecimalPlace;

                            //MasterGridOpt.RequiredQuantity = FinalQty;
                            MasterGridOpt.RequiredQuantity = FinalQtyInStockUnit;

                            if (Var_ConversionFormula === "" || Var_ConversionFormula === undefined || Var_ConversionFormula === null) {
                                //Qty = parseFloat(Number(FinalQty)).toFixed(Number(Var_UnitDecimalPlace));
                                Qty = parseFloat(Number(FinalQtyInPurchaseUnit)).toFixed(Number(Var_UnitDecimalPlace));
                                QtyInStockUnit = parseFloat(Number(FinalQtyInStockUnit)).toFixed(Number(newArray[0].UnitDecimalPlaceStockUnit));
                                MasterGridOpt.RequiredQuantityInPurchaseUnit = Qty;
                            } else {
                                if (Var_UnitPerPacking === 0) { Var_UnitPerPacking = 1; }
                                if (Var_ConversionFactor === 0) { Var_ConversionFactor = 1; }
                                if (Var_SizeW === 0) { Var_SizeW = 1; }
                                //MasterGridOpt.RequiredQuantity = FinalQty;
                                MasterGridOpt.RequiredQuantity = FinalQtyInStockUnit;
                                QtyInStockUnit = FinalQtyInStockUnit;
                                Qty = parseFloat(Number(FinalQtyInPurchaseUnit)).toFixed(Number(Var_UnitDecimalPlace));
                                MasterGridOpt.RequiredQuantityInPurchaseUnit = Qty;
                            }
                            //Close New Code for Change Sheets
                        }

                        PurchaseRate = "";
                        if (newArray[0].PurchaseRate === "" || newArray[0].PurchaseRate === undefined || newArray[0].PurchaseRate === null || newArray[0].PurchaseRate === "NULL") {
                            PurchaseRate = 0;
                        }
                        else {
                            PurchaseRate = newArray[0].PurchaseRate;
                        }

                        /////***************
                        if (ddlSupplierId === "" || ddlSupplierId === null || ddlSupplierId === undefined || ddlSupplierId === "NULL") {
                            PurchaseRate = PurchaseRate;
                        }
                        else {
                            ObjItemRate = [];
                            ObjItemRate = ItemRateString.ItemRateObj.filter(function (el) {
                                return el.LedgerID === ddlSupplierId &&
                                    el.ItemID === newArray[0].ItemID;
                            });
                            if (ObjItemRate === [] || ObjItemRate === "" || ObjItemRate === undefined) {
                                PurchaseRate = PurchaseRate;
                            } else {
                                PurchaseRate = ObjItemRate[0].PurchaseRate;
                            }
                        }

                        //PurchaseRate = 65;
                        var BasicAmtEl = 0;
                        BasicAmtEl = parseFloat(Number(Number(Qty) * Number(PurchaseRate))).toFixed(2);

                        var DisPercentage = 0;
                        var TaxAbleAmt = 0;
                        var DiscountAmt = parseFloat(Number((Number(BasicAmtEl) * Number(DisPercentage)) / 100)).toFixed(2);

                        TaxAbleAmt = parseFloat(Number(Number(BasicAmtEl) - Number(DiscountAmt))).toFixed(2);

                        var IGSTPER = 0, SGSTPER = 0, CGSTPER = 0;
                        var IGSTAMT = 0, SGSTAMT = 0, CGSTAMT = 0;
                        var TotalAmount = 0;
                        if (GblCompanyConfiguration[0].IsGstApplicable === true) {
                            if (Number(LblSupplierStateTin) === Number(GblCompanyStateTin)) {
                                if (newArray[0].CGSTTaxPercentage === "" || newArray[0].CGSTTaxPercentage === undefined || newArray[0].CGSTTaxPercentage === null || newArray[0].CGSTTaxPercentage === "NULL") {
                                    CGSTPER = 0;
                                }
                                else {
                                    CGSTPER = newArray[0].CGSTTaxPercentage;
                                }
                                if (newArray[0].SGSTTaxPercentage === "" || newArray[0].SGSTTaxPercentage === undefined || newArray[0].SGSTTaxPercentage === null || newArray[0].SGSTTaxPercentage === "NULL") {
                                    SGSTPER = 0;
                                }
                                else {
                                    SGSTPER = newArray[0].SGSTTaxPercentage;
                                }
                                SGSTAMT = parseFloat(Number(((Number(TaxAbleAmt) * Number(SGSTPER)) / 100))).toFixed(2);
                                CGSTAMT = parseFloat(Number(((Number(TaxAbleAmt) * Number(CGSTPER)) / 100))).toFixed(2);
                                TotalAmount = parseFloat(Number(Number(SGSTAMT) + Number(CGSTAMT) + Number(TaxAbleAmt))).toFixed(2);
                            }
                            else {
                                if (newArray[0].IGSTTaxPercentage === "" || newArray[0].IGSTTaxPercentage === undefined || newArray[0].IGSTTaxPercentage === null || newArray[0].IGSTTaxPercentage === "NULL") {
                                    IGSTPER = 0;
                                }
                                else {
                                    IGSTPER = newArray[0].IGSTTaxPercentage;
                                }
                                IGSTAMT = parseFloat(Number(((Number(TaxAbleAmt) * Number(IGSTPER)) / 100))).toFixed(2);
                                TotalAmount = parseFloat(Number(Number(IGSTAMT) + Number(TaxAbleAmt))).toFixed(2);
                            }
                        } else if (GblCompanyConfiguration[0].IsVatApplicable === true) {
                            if (newArray[0].IGSTTaxPercentage === "" || newArray[0].IGSTTaxPercentage === undefined || newArray[0].IGSTTaxPercentage === null || newArray[0].IGSTTaxPercentage === "NULL") {
                                IGSTPER = 0;
                            }
                            else {
                                IGSTPER = newArray[0].IGSTTaxPercentage;
                            }
                            IGSTAMT = parseFloat(Number(((Number(TaxAbleAmt) * Number(IGSTPER)) / 100))).toFixed(2);
                            TotalAmount = parseFloat(Number(Number(IGSTAMT) + Number(TaxAbleAmt))).toFixed(2);

                        }


                        MasterGridOpt.PurchaseQuantityComp = Qty;
                        MasterGridOpt.PurchaseQuantity = Qty;
                        MasterGridOpt.PurchaseQuantityInStockUnit = QtyInStockUnit;

                        if (Number(newArray[0].QuantityPerPack) > 0) {
                            MasterGridOpt.QuantityPerPack = Number(newArray[0].QuantityPerPack);
                        } else {
                            MasterGridOpt.QuantityPerPack = Number(newArray[0].UnitPerPacking);
                        }
                        (Number(MasterGridOpt.QuantityPerPack) > 0) ? MasterGridOpt.QuantityPerPack = Number(MasterGridOpt.QuantityPerPack) : MasterGridOpt.QuantityPerPack = 1;

                        MasterGridOpt.RequiredNoOfPacks = (Number(MasterGridOpt.PurchaseQuantityInStockUnit) / Number(MasterGridOpt.QuantityPerPack)).toFixed(2);
                        MasterGridOpt.StockUnit = newArray[0].StockUnit;
                        MasterGridOpt.CreatedBy = newArray[0].CreatedBy;
                        MasterGridOpt.ItemNarration = newArray[0].ItemNarration;
                        MasterGridOpt.PurchaseUnit = newArray[0].PurchaseUnit;
                        MasterGridOpt.BasicAmount = BasicAmtEl;
                        MasterGridOpt.Disc = DisPercentage;
                        MasterGridOpt.AfterDisAmt = TaxAbleAmt;
                        MasterGridOpt.TaxableAmount = TaxAbleAmt;
                        MasterGridOpt.Tolerance = newArray[0].Tolerance;
                        MasterGridOpt.GSTTaxPercentage = newArray[0].GSTTaxPercentage;
                        MasterGridOpt.CGSTTaxPercentage = newArray[0].CGSTTaxPercentage;
                        MasterGridOpt.SGSTTaxPercentage = newArray[0].SGSTTaxPercentage;
                        MasterGridOpt.IGSTTaxPercentage = newArray[0].IGSTTaxPercentage;

                        MasterGridOpt.CGSTAmt = CGSTAMT;
                        MasterGridOpt.SGSTAmt = SGSTAMT;
                        MasterGridOpt.IGSTAmt = IGSTAMT;
                        MasterGridOpt.TotalAmount = TotalAmount;

                        MasterGridOpt.Narration = newArray[0].Narration;
                        MasterGridOpt.FYear = newArray[0].FYear;
                        MasterGridOpt.PurchaseRate = PurchaseRate;
                        MasterGridOpt.ProductHSNName = newArray[0].ProductHSNName;
                        MasterGridOpt.HSNCode = newArray[0].HSNCode;
                        MasterGridOpt.WtPerPacking = newArray[0].WtPerPacking;
                        MasterGridOpt.UnitPerPacking = newArray[0].UnitPerPacking;
                        MasterGridOpt.ConversionFactor = newArray[0].ConversionFactor;
                        MasterGridOpt.ConversionFormula = newArray[0].ConversionFormula;
                        MasterGridOpt.UnitDecimalPlace = newArray[0].UnitDecimalPlace;
                        MasterGridOpt.ConversionFormulaStockUnit = newArray[0].ConversionFormulaStockUnit;
                        MasterGridOpt.UnitDecimalPlaceStockUnit = newArray[0].UnitDecimalPlaceStockUnit;
                        MasterGridOpt.SizeW = newArray[0].SizeW;
                        MasterGridOpt.GSM = newArray[0].GSM;
                        MasterGridOpt.ReleaseGSM = newArray[0].ReleaseGSM;
                        MasterGridOpt.AdhesiveGSM = newArray[0].AdhesiveGSM;
                        MasterGridOpt.Thickness = newArray[0].Thickness;
                        MasterGridOpt.Density = newArray[0].Density;
                        MasterGridOpt.UnitDecimalPlaceStockUnit = newArray[0].UnitDecimalPlaceStockUnit;

                        //MasterGridOpt.ExpectedDeliveryDate = newArray[0].ExpectedDeliveryDate;
                        if (currentdate > Date.parse(newArray[0].ExpectedDeliveryDate)) {
                            MasterGridOpt.ExpectedDeliveryDate = currentdate;
                        } else {
                            MasterGridOpt.ExpectedDeliveryDate = newArray[0].ExpectedDeliveryDate;
                        }
                        // MasterGridOpt.Schedule = Addfun;

                        MasterGridData.push(MasterGridOpt);

                    }
                }
            }
            $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
            existReq = MasterGridData;
            AddAditionalCharges(existReq);

        }

        //$("#LoadIndicator").dxLoadPanel("instance").option("visible", true);

        //document.getElementById("CreatePOButton").setAttribute("data-toggle", "modal");
        //document.getElementById("CreatePOButton").setAttribute("data-target", "#largeModal");
        $('#largeModal').modal({
            show: 'true'
        });
        //CreatePONO();

        ShowCreatePOGrid();
        //document.getElementById("textDeliverAt").value = "D.No. 5/208, 5/209, 5/209-A,B,C,D and E, Poovanathapuram Road, Enjar, Sivakasi West - 626124";
        (purchaseConfiguration.DeliveryAddress.length > 0) ? document.getElementById("textDeliverAt").value = purchaseConfiguration.DeliveryAddress[0].MailingAddress : document.getElementById("textDeliverAt").value = "";

        var gridInstance = $("#SelectAddressDataGrid").dxDataGrid("instance");
        var gridData = gridInstance.option("dataSource");

        if (gridData && gridData.length === 1) {
            document.getElementById("textDeliverAt").value = gridData[0].DeliveryAddress;
        } else {
            document.getElementById("textDeliverAt").value = "";
        }
    });
});

var LblSupplierStateTin = document.getElementById("LblSupplierStateTin").innerHTML;
var BasicAmt = 0;
var TaxAbleAmt = 0;
var IGSTPER = 0, SGSTPER = 0, CGSTPER = 0;
var IGSTAMT = 0, SGSTAMT = 0, CGSTAMT = 0;
var TotalAmount = 0;

function ShowCreatePOGrid() {

    var gridInstance = $("#CreatePOGrid").dxDataGrid({
        dataSource: existReq,
        masterDetail: {
            enabled: true,
            template: function (container, options) {
                var currentEmployeeData = options.data;

                //$("<div>")
                //    .addClass("master-detail-caption")
                //    .text(currentEmployeeData.VoucherNo + "  (Voucher No.)")
                //    .appendTo(container);

                $("<div>")
                    .dxDataGrid({
                        columnAutoWidth: true,
                        showBorders: true,
                        allowColumnResizing: true,
                        columnResizingMode: "widget",
                        sorting: {
                            mode: "none"
                        },
                        columns: [{ dataField: "TransactionID", visible: false }, { dataField: "TransID", visible: false },
                        { dataField: "VoucherID", visible: false }, { dataField: "ItemGroupID", visible: false },
                        { dataField: "ItemID", visible: false }, { dataField: "MaxVoucherNo", caption: "Ref.Req.No.", width: 80, visible: false },
                        { dataField: "VoucherNo", caption: "Req.No.", width: 100 }, { dataField: "VoucherDate", caption: "Req.Date", width: 90, dataType: "date", format: "dd-MMM-yyyy" },
                        { dataField: "ItemCode", caption: "Item Code", width: 70 }, { dataField: "ItemGroupName", caption: "Item Group", width: 100 },
                        { dataField: "ItemSubGroupName", caption: "Sub Group", width: 100 }, { dataField: "ItemName", caption: "Item Name", width: 180 },
                        { dataField: "RefJobCardContentNo", caption: "Ref.J.C.No.", width: 120 }, { dataField: "JobName", caption: "Job Name", width: 150 }, { dataField: "ItemNarration", caption: "Item Remark", width: 100 }, { dataField: "RequiredQuantity", caption: "Req.Qty", width: 80 },
                        { dataField: "RequisitionQty", caption: "Total Req.Qty", width: 100 }, { dataField: "PurchaseQuantity", caption: "Purchase Qty", visible: false },
                        { dataField: "StockUnit", caption: "Unit", width: 80 }, { dataField: "CreatedBy", caption: "Created By", width: 80 }, { dataField: "Narration", caption: "Narration" },
                        { dataField: "FYear", caption: "FYear", visible: false }, { dataField: "PurchaseRate", caption: "Purchase Rate", visible: false },
                        { dataField: "PurchaseUnit", caption: "Purchase Unit", visible: false }, { dataField: "ProductHSNName", visible: false },
                        { dataField: "HSNCode", visible: false }, { dataField: "GSTTaxPercentage", caption: "GSTTaxPercentage", visible: false },
                        { dataField: "CGSTTaxPercentage", caption: "CGSTTaxPercentage", visible: false }, { dataField: "SGSTTaxPercentage", caption: "SGSTTaxPercentage", visible: false },
                        { dataField: "IGSTTaxPercentage", caption: "IGSTTaxPercentage", visible: false }],
                        dataSource: new DevExpress.data.DataSource({
                            store: new DevExpress.data.ArrayStore({
                                key: "ItemID",
                                data: SubGridData
                            }),
                            filter: [["ItemID", "=", options.key], "and", ["RequiredQuantity", ">", 0], "and", ["TransactionID", ">", 0]]
                        }),
                        onRowPrepared: function (e) {
                            if (e.rowType === "header") {
                                e.rowElement.css('font-weight', 'bold');
                            }
                            e.rowElement.css('fontSize', '11px');
                        }
                    }).appendTo(container);
            }
        }

    }).dxDataGrid("instance");
}

function AddItemWithChargessGrid() {

    var CreatePOGrid = $("#CreatePOGrid").dxDataGrid('instance');
    //var CreatePOGrid_RowCount = CreatePOGrid.totalCount();
    var CreatePOGrid_RowCount = CreatePOGrid._options.dataSource.length;
    var AdditionalChargesGrid = $("#AdditionalChargesGrid").dxDataGrid('instance');
    var Charge_RowCount = AdditionalChargesGrid.totalCount();
    var t = 0;
    AdditionalChargesGrid.refresh();
    var LblSupplierStateTin = document.getElementById("LblSupplierStateTin").innerHTML;
    var NewAfterDisAmt = 0;
    if (CreatePOGrid_RowCount > 0) {
        for (t = 0; t < CreatePOGrid_RowCount; t++) {
            CreatePOGrid._options.dataSource[t].TaxableAmount = existReq[t].AfterDisAmt;
            NewAfterDisAmt = Number(NewAfterDisAmt) + Number(existReq[t].AfterDisAmt);
        }
    }

    document.getElementById("TxtAfterDisAmt").value = NewAfterDisAmt;
    BasicAmt = 0; IGSTPER = 0; SGSTPER = 0; CGSTPER = 0;
    TaxAbleAmt = 0; IGSTAMT = 0; SGSTAMT = 0; CGSTAMT = 0; TotalAmount = 0;

    var AfterDisAmt_WithGstApplicable = 0;

    //var AdditionalChargesGrid = $('#AdditionalChargesGrid').dxDataGrid('instance');
    //var AdditionalChargesGrid_RowCount = AdditionalChargesGrid.totalCount();
    for (var c = 0; c < ChargesGrid.length; c++) {
        var GridTaxType = ChargesGrid[c].TaxType;
        var TaxRatePer = ChargesGrid[c].TaxRatePer;
        var GSTLedgerType = ChargesGrid[c].GSTLedgerType;
        var CalculateON = ChargesGrid[c].CalculateON;
        var taxAmt = 0;
        var g = 0;

        //if (GridTaxType === "GST") {
        if (GridTaxType === GblCompanyConfiguration[0].DefaultTaxLedgerTypeName.toUpperCase()) {
            taxAmt = 0;
            if (TaxRatePer === 0 || TaxRatePer === "0") {
                taxAmt = 0;
                if (GblCompanyConfiguration[0].IsGstApplicable === true) {
                    if (GSTLedgerType.toUpperCase().trim() === "CENTRAL TAX") {
                        taxAmt = Number(document.getElementById("TxtCGSTAmt").value);
                    }
                    else if (GSTLedgerType.toUpperCase().trim() === "STATE TAX") {
                        taxAmt = Number(document.getElementById("TxtSGSTAmt").value);
                    }
                    else if (GSTLedgerType.toUpperCase().trim() === "INTEGRATED TAX") {
                        taxAmt = Number(document.getElementById("TxtIGSTAmt").value);
                    }
                } else {
                    taxAmt = Number(document.getElementById("TxtIGSTAmt").value);
                }

                if (ChargesGrid[c].InAmount === true || ChargesGrid[c].InAmount === "1" || ChargesGrid[c].InAmount === 1 || ChargesGrid[c].InAmount === "true") {
                    taxAmt = ChargesGrid[c].ChargesAmount;
                }
                else {
                    taxAmt = taxAmt;
                }
                ChargesGrid[c].ChargesAmount = taxAmt;
            }
            else {
                taxAmt = 0;
                var FilterAmt = 0;
                if (GblCompanyConfiguration[0].IsGstApplicable === true) {
                    if (GSTLedgerType.toUpperCase().trim() === "CENTRAL TAX") {
                        if (existReq.length > 0) {
                            for (g = 0; g < existReq.length; g++) {
                                if (TaxRatePer === existReq[g].CGSTTaxPercentage) {
                                    FilterAmt = FilterAmt + Number(existReq[g].CGSTAmt);
                                }
                            }
                        }
                        // taxAmt = FilterAmt * TaxRatePer;
                        taxAmt = parseFloat(Number(FilterAmt)).toFixed(2);
                    }
                    else if (GSTLedgerType.toUpperCase().trim() === "STATE TAX") {
                        if (existReq.length > 0) {
                            for (g = 0; g < existReq.length; g++) {
                                if (TaxRatePer === existReq[g].SGSTTaxPercentage) {
                                    FilterAmt = FilterAmt + Number(existReq[g].SGSTAmt);
                                }
                            }
                        }
                        //taxAmt = FilterAmt * TaxRatePer;
                        taxAmt = parseFloat(Number(FilterAmt)).toFixed(2);
                    }
                    else if (GSTLedgerType.toUpperCase().trim() === "INTEGRATED TAX") {
                        if (existReq.length > 0) {
                            for (g = 0; g < existReq.length; g++) {
                                if (TaxRatePer === existReq[g].IGSTTaxPercentage) {
                                    FilterAmt = FilterAmt + Number(existReq[g].IGSTAmt);
                                }
                            }
                        }
                        // taxAmt = FilterAmt * TaxRatePer;
                        taxAmt = parseFloat(Number(FilterAmt)).toFixed(2);
                    }
                } else {
                    if (existReq.length > 0) {
                        for (g = 0; g < existReq.length; g++) {
                            if (TaxRatePer === existReq[g].IGSTTaxPercentage) {
                                FilterAmt = FilterAmt + Number(existReq[g].IGSTAmt);
                            }
                        }
                    }
                    // taxAmt = FilterAmt * TaxRatePer;
                    taxAmt = parseFloat(Number(FilterAmt)).toFixed(2);
                }
                if (ChargesGrid[c].InAmount === true || ChargesGrid[c].InAmount === "1" || ChargesGrid[c].InAmount === 1 || ChargesGrid[c].InAmount === "true") {
                    taxAmt = ChargesGrid[c].ChargesAmount;
                }
                else {
                    taxAmt = taxAmt;
                }
                ChargesGrid[c].ChargesAmount = parseFloat(Number(taxAmt)).toFixed(2);
            }
        } else {
            taxAmt = 0;
            var OtherAmt = 0;
            if (ChargesGrid[c].InAmount === true || ChargesGrid[c].InAmount === "1" || ChargesGrid[c].InAmount === 1 || ChargesGrid[c].InAmount === "true") {
                OtherAmt = ChargesGrid[c].ChargesAmount;
            }
            else {
                OtherAmt = Number(Number(document.getElementById("TxtAfterDisAmt").value) * TaxRatePer) / 100;
            }

            var GSTAplicable = ChargesGrid[c].GSTApplicable;

            if (GSTAplicable === true || GSTAplicable === "1" || GSTAplicable === 1 || GSTAplicable === "true") {
                if (CalculateON.toUpperCase().trim() === "VALUE") {
                    if (existReq.length > 0) {
                        for (g = 0; g < existReq.length; g++) {
                            AfterDisAmt_WithGstApplicable = 0;
                            AfterDisAmt_WithGstApplicable = Number(OtherAmt / Number(document.getElementById("TxtAfterDisAmt").value) * Number(existReq[g].AfterDisAmt)) + Number(existReq[g].TaxableAmount);
                            if (GblCompanyConfiguration[0].IsGstApplicable === true) {
                                if (Number(LblSupplierStateTin) === Number(GblCompanyStateTin)) {
                                    if (existReq[g].CGSTTaxPercentage === "" || existReq[g].CGSTTaxPercentage === undefined || existReq[g].CGSTTaxPercentage === null || existReq[g].CGSTTaxPercentage === "NULL") {
                                        CGSTPER = 0;
                                    }
                                    else {
                                        CGSTPER = existReq[g].CGSTTaxPercentage;// newArray[0].CGSTTaxPercentage;
                                    }
                                    if (existReq[g].SGSTTaxPercentage === "" || existReq[g].SGSTTaxPercentage === undefined || existReq[g].SGSTTaxPercentage === null || existReq[g].SGSTTaxPercentage === "NULL") {
                                        SGSTPER = 0;
                                    }
                                    else {
                                        SGSTPER = existReq[g].SGSTTaxPercentage;
                                    }
                                    SGSTAMT = Number(Number(AfterDisAmt_WithGstApplicable) * Number(SGSTPER)) / 100;
                                    CGSTAMT = Number(Number(AfterDisAmt_WithGstApplicable) * Number(CGSTPER)) / 100;
                                    TotalAmount = Number(SGSTAMT) + Number(CGSTAMT) + Number(AfterDisAmt_WithGstApplicable);
                                }
                                else {
                                    if (existReq[g].IGSTTaxPercentage === "" || existReq[g].IGSTTaxPercentage === undefined || existReq[g].IGSTTaxPercentage === null || existReq[g].IGSTTaxPercentage === "NULL") {
                                        IGSTPER = 0;
                                    }
                                    else {
                                        IGSTPER = existReq[g].IGSTTaxPercentage;
                                    }

                                    IGSTAMT = Number(Number(Number(AfterDisAmt_WithGstApplicable) * Number(IGSTPER)) / 100);
                                    TotalAmount = Number(IGSTAMT) + Number(AfterDisAmt_WithGstApplicable);
                                }
                            } else if (GblCompanyConfiguration[0].IsVatApplicable === true) {
                                if (existReq[g].IGSTTaxPercentage === "" || existReq[g].IGSTTaxPercentage === undefined || existReq[g].IGSTTaxPercentage === null || existReq[g].IGSTTaxPercentage === "NULL") {
                                    IGSTPER = 0;
                                }
                                else {
                                    IGSTPER = existReq[g].IGSTTaxPercentage;
                                }

                                IGSTAMT = Number(Number(Number(AfterDisAmt_WithGstApplicable) * Number(IGSTPER)) / 100);
                                TotalAmount = Number(IGSTAMT) + Number(AfterDisAmt_WithGstApplicable);
                            }

                            existReq[g].CGSTAmt = parseFloat(Number(CGSTAMT)).toFixed(2);
                            existReq[g].SGSTAmt = parseFloat(Number(SGSTAMT)).toFixed(2);
                            existReq[g].IGSTAmt = parseFloat(Number(IGSTAMT)).toFixed(2);
                            existReq[g].TotalAmount = parseFloat(Number(TotalAmount)).toFixed(2);
                            existReq[g].TaxableAmount = parseFloat(Number(AfterDisAmt_WithGstApplicable)).toFixed(2);
                        }
                    }
                    ChargesGrid[c].ChargesAmount = parseFloat(Number(OtherAmt)).toFixed(2);
                }
                else {
                    if (existReq.length > 0) {
                        for (g = 0; g < existReq.length; g++) {
                            AfterDisAmt_WithGstApplicable = 0;
                            AfterDisAmt_WithGstApplicable = Number(OtherAmt / Number(document.getElementById("TxtTotalQty").value) * Number(existReq[g].PurchaseQuantity)) + Number(existReq[g].TaxableAmount);

                            existReq[g].TaxableAmount = parseFloat(Number(AfterDisAmt_WithGstApplicable)).toFixed(2);
                        }
                    }
                    ChargesGrid[c].ChargesAmount = parseFloat(Number(OtherAmt)).toFixed(2);
                }
            }
            else {
                if (CalculateON.toUpperCase().trim() === "VALUE") {
                    if (existReq.length > 0) {
                        for (g = 0; g < existReq.length; g++) {
                            AfterDisAmt_WithGstApplicable = 0;
                            //AfterDisAmt_WithGstApplicable = Number(existReq[g].BasicAmount) + (Number(existReq[g].BasicAmount) * Number(existReq[g].Disc) / 100);
                            AfterDisAmt_WithGstApplicable = Number(existReq[g].BasicAmount) - (Number(existReq[g].BasicAmount) * Number(existReq[g].Disc) / 100);

                            if (GblCompanyConfiguration[0].IsGstApplicable === true) {
                                if (Number(LblSupplierStateTin) === Number(GblCompanyStateTin)) {
                                    if (existReq[g].CGSTTaxPercentage === "" || existReq[g].CGSTTaxPercentage === undefined || existReq[g].CGSTTaxPercentage === null || existReq[g].CGSTTaxPercentage === "NULL") {
                                        CGSTPER = 0;
                                    }
                                    else {
                                        CGSTPER = existReq[g].CGSTTaxPercentage;// newArray[0].CGSTTaxPercentage;
                                    }
                                    if (existReq[g].SGSTTaxPercentage === "" || existReq[g].SGSTTaxPercentage === undefined || existReq[g].SGSTTaxPercentage === null || existReq[g].SGSTTaxPercentage === "NULL") {
                                        SGSTPER = 0;
                                    }
                                    else {
                                        SGSTPER = existReq[g].SGSTTaxPercentage;
                                    }
                                    SGSTAMT = ((Number(AfterDisAmt_WithGstApplicable) * Number(SGSTPER)) / 100);
                                    CGSTAMT = ((Number(AfterDisAmt_WithGstApplicable) * Number(CGSTPER)) / 100);
                                    TotalAmount = Number(SGSTAMT) + Number(CGSTAMT) + Number(AfterDisAmt_WithGstApplicable);
                                }
                                else {
                                    if (existReq[g].IGSTTaxPercentage === "" || existReq[g].IGSTTaxPercentage === undefined || existReq[g].IGSTTaxPercentage === null || existReq[g].IGSTTaxPercentage === "NULL") {
                                        IGSTPER = 0;
                                    }
                                    else {
                                        IGSTPER = existReq[g].IGSTTaxPercentage;
                                    }

                                    IGSTAMT = ((Number(AfterDisAmt_WithGstApplicable) * Number(IGSTPER)) / 100);
                                    TotalAmount = Number(IGSTAMT) + Number(AfterDisAmt_WithGstApplicable);
                                }
                            } else if (GblCompanyConfiguration[0].IsVatApplicable === true) {
                                if (existReq[g].IGSTTaxPercentage === "" || existReq[g].IGSTTaxPercentage === undefined || existReq[g].IGSTTaxPercentage === null || existReq[g].IGSTTaxPercentage === "NULL") {
                                    IGSTPER = 0;
                                }
                                else {
                                    IGSTPER = existReq[g].IGSTTaxPercentage;
                                }

                                IGSTAMT = ((Number(AfterDisAmt_WithGstApplicable) * Number(IGSTPER)) / 100);
                                TotalAmount = Number(IGSTAMT) + Number(AfterDisAmt_WithGstApplicable);
                            }

                            existReq[g].CGSTAmt = parseFloat(Number(CGSTAMT)).toFixed(2);
                            existReq[g].SGSTAmt = parseFloat(Number(SGSTAMT)).toFixed(2);
                            existReq[g].IGSTAmt = parseFloat(Number(IGSTAMT)).toFixed(2);
                            existReq[g].TotalAmount = parseFloat(Number(TotalAmount)).toFixed(2);
                            existReq[g].TaxableAmount = parseFloat(Number(AfterDisAmt_WithGstApplicable)).toFixed(2);
                        }
                    }

                    ChargesGrid[c].ChargesAmount = parseFloat(Number(OtherAmt)).toFixed(2);
                }
                else {
                    if (existReq.length > 0) {
                        for (g = 0; g < existReq.length; g++) {
                            AfterDisAmt_WithGstApplicable = 0;
                            AfterDisAmt_WithGstApplicable = Number(document.getElementById("TxtAfterDisAmt").value);

                            existReq[g].TaxableAmount = parseFloat(Number(AfterDisAmt_WithGstApplicable)).toFixed(2);
                        }
                    }
                    //ChargesGrid[c].ChargesAmount = OtherAmt;
                }

                taxAmt = (TaxRatePer * Number(document.getElementById("TxtAfterDisAmt").value)) / 100;

                if (ChargesGrid[c].InAmount === true || ChargesGrid[c].InAmount === "1" || ChargesGrid[c].InAmount === 1 || ChargesGrid[c].InAmount === "true") {
                    taxAmt = ChargesGrid[c].ChargesAmount;
                }
                else {
                    taxAmt = taxAmt;
                }

                ChargesGrid[c].ChargesAmount = parseFloat(Number(taxAmt)).toFixed(2);
            }
        }

    }
    AdditionalChargesGrid.refresh();
    GridColumnCal();

    for (var u = 0; u < AdditionalChargesGrid._options.dataSource.length; u++) {
        for (t = 0; t < ChargesGrid.length; t++) {
            if (ChargesGrid[t].LedgerID === AdditionalChargesGrid._options.dataSource[u].LedgerID) {
                AdditionalChargesGrid._options.dataSource[u].ChargesAmount = ChargesGrid[t].ChargesAmount;
                break;
            }
        }
    }
    AdditionalChargesGrid.refresh();
}

function CalculateAmount() {
    var CreatePOGrid = $("#CreatePOGrid").dxDataGrid('instance');
    var CreatePOGrid_RowCount = CreatePOGrid._options.dataSource.length;
    var AdditionalChargesGrid = $("#AdditionalChargesGrid").dxDataGrid('instance');
    AdditionalChargesGrid.refresh();
    var Charge_RowCount = ChargesGrid.length;
    var t = 0;
    var LblSupplierStateTin = document.getElementById("LblSupplierStateTin").innerHTML;
    var NewAfterDisAmt = 0;
    if (CreatePOGrid_RowCount > 0) {
        for (t = 0; t < CreatePOGrid_RowCount; t++) {
            CreatePOGrid._options.dataSource[t].TaxableAmount = existReq[t].AfterDisAmt;
            NewAfterDisAmt = Number(NewAfterDisAmt) + Number(existReq[t].AfterDisAmt);
        }
    }

    document.getElementById("TxtAfterDisAmt").value = NewAfterDisAmt;
    BasicAmt = 0; IGSTPER = 0; SGSTPER = 0; CGSTPER = 0;
    TaxAbleAmt = 0; IGSTAMT = 0; SGSTAMT = 0; CGSTAMT = 0; TotalAmount = 0;

    var AfterDisAmt_WithGstApplicable = 0;
    var GridTaxType = "";
    var TaxRatePer = 0;
    var afterDisAmt = 0;
    var GSTLedgerType = "";
    var CalculateON = "Value";
    var taxAmt = 0;
    //var AdditionalChargesGrid = $('#AdditionalChargesGrid').dxDataGrid('instance');
    //var AdditionalChargesGrid_RowCount = AdditionalChargesGrid.totalCount();
    //For j = 2 To VS_Material.rows - 1
    for (t = 0; t < CreatePOGrid_RowCount; t++) {
        TaxAbleAmt = 0;
        afterDisAmt = 0;
        existReq[t].TaxableAmount = existReq[t].AfterDisAmt;
        afterDisAmt = existReq[t].AfterDisAmt;
        //TaxAbleAmt = Number(CreatePOGrid._options.dataSource[t].AfterDisAmt);
        for (var m = 0; m < Charge_RowCount; m++) {
            GridTaxType = (ChargesGrid[m].TaxType === undefined || ChargesGrid[m].TaxType === null) ? "" : ChargesGrid[m].TaxType;
            TaxRatePer = (ChargesGrid[m].TaxRatePer === undefined || ChargesGrid[m].TaxRatePer === null) ? 0 : ChargesGrid[m].TaxRatePer;
            GSTLedgerType = (ChargesGrid[m].GSTLedgerType === undefined || ChargesGrid[m].GSTLedgerType === null) ? "" : ChargesGrid[m].GSTLedgerType;
            CalculateON = (ChargesGrid[m].CalculateON === undefined || ChargesGrid[m].CalculateON === null) ? "" : ChargesGrid[m].CalculateON;

            taxAmt = 0;
            //if (GridTaxType.toString() !== "GST") {
            if (GridTaxType.toString() !== GblCompanyConfiguration[0].DefaultTaxLedgerTypeName.toUpperCase()) {
                if (Number(TaxRatePer) === 0 && (ChargesGrid[m].InAmount === false || ChargesGrid[m].InAmount === "0" || ChargesGrid[m].InAmount === 0 || ChargesGrid[m].InAmount === "false")) {
                    ChargesGrid[m].InAmount = false;
                } else {
                    if ((ChargesGrid[m].InAmount === false || ChargesGrid[m].InAmount === "0" || ChargesGrid[m].InAmount === 0 || ChargesGrid[m].InAmount === "false")) {
                        ChargesGrid[m].ChargesAmount = ((Number(document.getElementById("TxtAfterDisAmt").value) * TaxRatePer) / 100).toFixed(2);
                    }
                }
                if (CalculateON.toUpperCase().trim() === "VALUE" && (ChargesGrid[m].GSTApplicable === true || ChargesGrid[m].GSTApplicable === "1" || ChargesGrid[m].GSTApplicable === 1 || ChargesGrid[m].GSTApplicable === "true")) {
                    TaxAbleAmt = (Number(TaxAbleAmt) + (Number(ChargesGrid[m].ChargesAmount) / Number(document.getElementById("TxtAfterDisAmt").value) * Number(existReq[t].AfterDisAmt))).toFixed(2);
                } else if (CalculateON.toUpperCase().trim() === "QUANTITY" && (ChargesGrid[m].GSTApplicable === true || ChargesGrid[m].GSTApplicable === "1" || ChargesGrid[m].GSTApplicable === 1 || ChargesGrid[m].GSTApplicable === "true")) {
                    TaxAbleAmt = (Number(TaxAbleAmt) + (Number(ChargesGrid[m].ChargesAmount) / Number(document.getElementById("TxtTotalQty").value) * Number(existReq[t].PurchaseQuantity))).toFixed(2);
                }

            }
        }
        existReq[t].TaxableAmount = (Number(existReq[t].AfterDisAmt) + Number(TaxAbleAmt)).toFixed(2);
        TaxAbleAmt = Number(existReq[t].TaxableAmount);
        if (GblCompanyConfiguration[0].IsGstApplicable === true) {
            if (GblGSTApplicable === true) {
                if (Number(LblSupplierStateTin) === Number(GblCompanyStateTin)) {
                    if (existReq[t].CGSTTaxPercentage === "" || existReq[t].CGSTTaxPercentage === undefined || existReq[t].CGSTTaxPercentage === null || existReq[t].CGSTTaxPercentage === "NULL") {
                        CGSTPER = 0;
                    }
                    else {
                        CGSTPER = existReq[t].CGSTTaxPercentage;// newArray[0].CGSTTaxPercentage;
                    }
                    if (existReq[t].SGSTTaxPercentage === "" || existReq[t].SGSTTaxPercentage === undefined || existReq[t].SGSTTaxPercentage === null || existReq[t].SGSTTaxPercentage === "NULL") {
                        SGSTPER = 0;
                    }
                    else {
                        SGSTPER = existReq[t].SGSTTaxPercentage;
                    }
                    SGSTAMT = ((Number(TaxAbleAmt) * Number(SGSTPER)) / 100);
                    CGSTAMT = ((Number(TaxAbleAmt) * Number(CGSTPER)) / 100);
                    TotalAmount = Number(SGSTAMT) + Number(CGSTAMT) + Number(afterDisAmt);
                }
                else {
                    if (existReq[t].IGSTTaxPercentage === "" || existReq[t].IGSTTaxPercentage === undefined || existReq[t].IGSTTaxPercentage === null || existReq[t].IGSTTaxPercentage === "NULL") {
                        IGSTPER = 0;
                    }
                    else {
                        IGSTPER = existReq[t].IGSTTaxPercentage;
                    }

                    IGSTAMT = Number((Number(TaxAbleAmt) * Number(IGSTPER)) / 100);
                    TotalAmount = Number(IGSTAMT) + Number(afterDisAmt);
                }
            } else {
                SGSTAMT = 0;
                CGSTAMT = 0;
                IGSTAMT = 0;
                TotalAmount = Number(afterDisAmt).toFixed(2);
            }
        } else if (GblCompanyConfiguration[0].IsVatApplicable === true) {
            if (GblGSTApplicable === true) {
                if (existReq[t].IGSTTaxPercentage === "" || existReq[t].IGSTTaxPercentage === undefined || existReq[t].IGSTTaxPercentage === null || existReq[t].IGSTTaxPercentage === "NULL") {
                    IGSTPER = 0;
                }
                else {
                    IGSTPER = existReq[t].IGSTTaxPercentage;
                }

                IGSTAMT = Number((Number(TaxAbleAmt) * Number(IGSTPER)) / 100);
                TotalAmount = Number(IGSTAMT) + Number(afterDisAmt);
            } else {
                SGSTAMT = 0;
                CGSTAMT = 0;
                IGSTAMT = 0;
                TotalAmount = Number(afterDisAmt).toFixed(2);
            }
        }


        existReq[t].TotalAmount = parseFloat(Number(TotalAmount)).toFixed(2);
        existReq[t].CGSTAmt = parseFloat(Number(CGSTAMT)).toFixed(2);
        existReq[t].SGSTAmt = parseFloat(Number(SGSTAMT)).toFixed(2);
        existReq[t].IGSTAmt = parseFloat(Number(IGSTAMT)).toFixed(2);
    }

    for (t = 0; t < Charge_RowCount; t++) {
        GridTaxType = (ChargesGrid[t].TaxType === undefined || ChargesGrid[t].TaxType === null) ? "" : ChargesGrid[t].TaxType;
        TaxRatePer = (ChargesGrid[t].TaxRatePer === undefined || ChargesGrid[t].TaxRatePer === null) ? 0 : ChargesGrid[t].TaxRatePer;
        GSTLedgerType = (ChargesGrid[t].GSTLedgerType === undefined || ChargesGrid[t].GSTLedgerType === null) ? "" : ChargesGrid[t].GSTLedgerType;
        CalculateON = (ChargesGrid[t].CalculateON === undefined || ChargesGrid[t].CalculateON === null) ? "" : ChargesGrid[t].CalculateON;
        taxAmt = 0;
        var FilterAmt = 0;
        //if (GridTaxType === "GST") {
        if (GridTaxType === GblCompanyConfiguration[0].DefaultTaxLedgerTypeName.toUpperCase()) {
            if (GridTaxType === GblCompanyConfiguration[0].DefaultTaxLedgerTypeName.toUpperCase() && GSTLedgerType.toUpperCase().trim() === "CENTRAL TAX") {
                if (CreatePOGrid_RowCount > 0) {
                    for (var gc = 0; gc < CreatePOGrid_RowCount; gc++) {
                        if (Number(TaxRatePer) > 0) {
                            if (TaxRatePer === existReq[gc].CGSTTaxPercentage) {
                                FilterAmt = FilterAmt + Number(existReq[gc].CGSTAmt);
                            }
                        } else {
                            FilterAmt = FilterAmt + Number(existReq[gc].CGSTAmt);
                        }

                    }
                }
                // taxAmt = FilterAmt * TaxRatePer;
                taxAmt = parseFloat(Number(FilterAmt)).toFixed(2);
            }
            else if (GridTaxType === GblCompanyConfiguration[0].DefaultTaxLedgerTypeName.toUpperCase() && GSTLedgerType.toUpperCase().trim() === "STATE TAX") {
                if (CreatePOGrid_RowCount > 0) {
                    for (var gs = 0; gs < CreatePOGrid_RowCount; gs++) {
                        if (Number(TaxRatePer) > 0) {
                            if (TaxRatePer === existReq[gs].SGSTTaxPercentage) {
                                FilterAmt = FilterAmt + Number(existReq[gs].SGSTAmt);
                            }
                        } else {
                            FilterAmt = FilterAmt + Number(existReq[gs].SGSTAmt);
                        }
                    }
                }
                //taxAmt = FilterAmt * TaxRatePer;
                taxAmt = parseFloat(Number(FilterAmt)).toFixed(2);
            }
            else if (GridTaxType === GblCompanyConfiguration[0].DefaultTaxLedgerTypeName.toUpperCase() && GSTLedgerType.toUpperCase().trim() === "INTEGRATED TAX") {
                if (CreatePOGrid_RowCount > 0) {
                    for (var gi = 0; gi < CreatePOGrid_RowCount; gi++) {
                        if (Number(TaxRatePer) > 0) {
                            if (TaxRatePer === existReq[gi].IGSTTaxPercentage) {
                                FilterAmt = FilterAmt + Number(existReq[gi].IGSTAmt);
                            }
                        } else {
                            FilterAmt = FilterAmt + Number(existReq[gi].IGSTAmt);
                        }
                    }
                }
                // taxAmt = FilterAmt * TaxRatePer;
                taxAmt = parseFloat(Number(FilterAmt)).toFixed(2);
            } else if (GridTaxType === GblCompanyConfiguration[0].DefaultTaxLedgerTypeName.toUpperCase() && GridTaxType !== "GST") {
                if (CreatePOGrid_RowCount > 0) {
                    for (var gi = 0; gi < CreatePOGrid_RowCount; gi++) {
                        if (Number(TaxRatePer) > 0) {
                            if (TaxRatePer === existReq[gi].IGSTTaxPercentage) {
                                FilterAmt = FilterAmt + Number(existReq[gi].IGSTAmt);
                            }
                        } else {
                            FilterAmt = FilterAmt + Number(existReq[gi].IGSTAmt);
                        }
                    }
                }
                // taxAmt = FilterAmt * TaxRatePer;
                taxAmt = parseFloat(Number(FilterAmt)).toFixed(2);
            }
            ChargesGrid[t].ChargesAmount = parseFloat(Number(taxAmt)).toFixed(2);
        }
    }

    AdditionalChargesGrid.refresh();
    for (var u = 0; u < AdditionalChargesGrid._options.dataSource.length; u++) {
        for (t = 0; t < ChargesGrid.length; t++) {
            if (ChargesGrid[t].LedgerID === AdditionalChargesGrid._options.dataSource[u].LedgerID) {
                AdditionalChargesGrid._options.dataSource[u].ChargesAmount = ChargesGrid[t].ChargesAmount;
                break;

            }
        }
    }
    AdditionalChargesGrid.refresh();
    GridColumnCal();
}

function AddItemCalculation() {

    let CreatePOGrid = $("#CreatePOGrid").dxDataGrid('instance');
    let CreatePOGrid_RowCount = CreatePOGrid._options.dataSource.length;

    let AdditionalChargesGrid = $("#AdditionalChargesGrid").dxDataGrid('instance');
    let Charge_RowCount = AdditionalChargesGrid.totalCount();

    let LblSupplierStateTin = document.getElementById("LblSupplierStateTin").innerHTML;

    if (CreatePOGrid_RowCount > 0) {
        for (var t = 0; t < CreatePOGrid_RowCount; t++) {
            CreatePOGrid._options.dataSource[t].TaxableAmount = existReq[t].AfterDisAmt;
        }
    }

    for (var zz = 0; zz < existReq.length; zz++) {
        var CreatePOGridRow = zz;

        var PurchaseRate = existReq[CreatePOGridRow].PurchaseRate;
        var Qty = existReq[CreatePOGridRow].PurchaseQuantity;
        var DisPercentage = existReq[CreatePOGridRow].Disc;

        if (Qty === "" || Qty === undefined || Qty === null || Qty === "NULL") {
            Qty = 0;
        } else {
            Qty = Qty;
        }
        if (DisPercentage === "" || DisPercentage === undefined || DisPercentage === null || DisPercentage === "NULL") {
            DisPercentage = 0;
        } else {
            DisPercentage = DisPercentage;
        }
        if (PurchaseRate === "" || PurchaseRate === undefined || PurchaseRate === null || PurchaseRate === "NULL") {
            PurchaseRate = 0;
        } else {
            PurchaseRate = PurchaseRate;
        }

        BasicAmt = parseFloat(Number(Qty) * Number(PurchaseRate)).toFixed(2);

        var DiscountAmt = parseFloat((Number(BasicAmt) * Number(DisPercentage)) / 100).toFixed(2);

        var afterDiscountAmt = 0;
        afterDiscountAmt = parseFloat(Number(BasicAmt) - Number(DiscountAmt)).toFixed(2);

        TaxAbleAmt = parseFloat(Number(afterDiscountAmt)).toFixed(2);
        if (GblCompanyConfiguration[0].IsGstApplicable === true) {
            if (GblGSTApplicable === true) {

                if (Number(LblSupplierStateTin) === Number(GblCompanyStateTin)) {
                    if (existReq[CreatePOGridRow].CGSTTaxPercentage === "" || existReq[CreatePOGridRow].CGSTTaxPercentage === undefined || existReq[CreatePOGridRow].CGSTTaxPercentage === null || existReq[CreatePOGridRow].CGSTTaxPercentage === "NULL") {
                        CGSTPER = 0;
                    }
                    else {
                        CGSTPER = existReq[CreatePOGridRow].CGSTTaxPercentage;// newArray[0].CGSTTaxPercentage;
                    }
                    if (existReq[CreatePOGridRow].SGSTTaxPercentage === "" || existReq[CreatePOGridRow].SGSTTaxPercentage === undefined || existReq[CreatePOGridRow].SGSTTaxPercentage === null || existReq[CreatePOGridRow].SGSTTaxPercentage === "NULL") {
                        SGSTPER = 0;
                    }
                    else {
                        SGSTPER = existReq[CreatePOGridRow].SGSTTaxPercentage;
                    }
                    SGSTAMT = parseFloat(((Number(TaxAbleAmt) * Number(SGSTPER)) / 100)).toFixed(2);
                    CGSTAMT = parseFloat(((Number(TaxAbleAmt) * Number(CGSTPER)) / 100)).toFixed(2);
                    //Cal Corection
                    IGSTAMT = 0;
                    TotalAmount = parseFloat(Number(SGSTAMT) + Number(CGSTAMT) + Number(afterDiscountAmt)).toFixed(2);
                }
                else {
                    if (existReq[CreatePOGridRow].IGSTTaxPercentage === "" || existReq[CreatePOGridRow].IGSTTaxPercentage === undefined || existReq[CreatePOGridRow].IGSTTaxPercentage === null || existReq[CreatePOGridRow].IGSTTaxPercentage === "NULL") {
                        IGSTPER = 0;
                    }
                    else {
                        IGSTPER = existReq[CreatePOGridRow].IGSTTaxPercentage;
                    }
                    //Cal Corection
                    SGSTAMT = 0;
                    CGSTAMT = 0;
                    IGSTAMT = parseFloat(((Number(TaxAbleAmt) * Number(IGSTPER)) / 100)).toFixed(2);
                    TotalAmount = parseFloat(Number(IGSTAMT) + Number(afterDiscountAmt)).toFixed(2);
                }
            } else {
                SGSTAMT = 0;
                CGSTAMT = 0;
                IGSTAMT = 0;
                TotalAmount = Number(afterDiscountAmt).toFixed(2);
            }
        } else if (GblCompanyConfiguration[0].IsVatApplicable === true) {
            if (GblGSTApplicable === true) {
                CGSTPER = 0;
                SGSTPER = 0;
                SGSTAMT = 0;
                CGSTAMT = 0;

                if (existReq[CreatePOGridRow].IGSTTaxPercentage === "" || existReq[CreatePOGridRow].IGSTTaxPercentage === undefined || existReq[CreatePOGridRow].IGSTTaxPercentage === null || existReq[CreatePOGridRow].IGSTTaxPercentage === "NULL") {
                    IGSTPER = 0;
                }
                else {
                    IGSTPER = existReq[CreatePOGridRow].IGSTTaxPercentage;

                }
                IGSTAMT = parseFloat(((Number(TaxAbleAmt) * Number(IGSTPER)) / 100)).toFixed(2);
                TotalAmount = parseFloat(Number(IGSTAMT) + Number(afterDiscountAmt)).toFixed(2);

            } else {
                SGSTAMT = 0;
                CGSTAMT = 0;
                IGSTAMT = 0;
                TotalAmount = Number(afterDiscountAmt).toFixed(2);
            }
        }


        existReq[CreatePOGridRow].BasicAmount = BasicAmt;
        existReq[CreatePOGridRow].AfterDisAmt = parseFloat(afterDiscountAmt).toFixed(2);
        existReq[CreatePOGridRow].TaxableAmount = TaxAbleAmt;
        existReq[CreatePOGridRow].CGSTAmt = CGSTAMT;
        existReq[CreatePOGridRow].SGSTAmt = SGSTAMT;
        existReq[CreatePOGridRow].IGSTAmt = IGSTAMT;
        existReq[CreatePOGridRow].TotalAmount = TotalAmount;
        existReq[CreatePOGridRow].PurchaseRate = PurchaseRate;

        if (CGSTAMT > 0 || SGSTAMT > 0) {
            existReq[CreatePOGridRow].IGSTTaxPercentage = 0;
        } else if (IGSTAMT > 0) {
            existReq[CreatePOGridRow].CGSTTaxPercentage = 0;
            existReq[CreatePOGridRow].SGSTTaxPercentage = 0;
        } else {
            if (Groupdata > 0) {
                var matchData = Groupdata.find(function (item) {
                    return item.ItemID === existReq[CreatePOGridRow].ItemID;
                });
            }
            if (SubGridData.length > 0) {
                if (!matchData) {
                    matchData = SubGridData.find(function (item) {
                        return item.ItemID === existReq[CreatePOGridRow].ItemID;
                    });
                }
            }

            if (matchData) {
                existReq[CreatePOGridRow].CGSTTaxPercentage = matchData.CGSTTaxPercentage || 0;
                existReq[CreatePOGridRow].SGSTTaxPercentage = matchData.SGSTTaxPercentage || 0;
                existReq[CreatePOGridRow].IGSTTaxPercentage = matchData.IGSTTaxPercentage || 0;
            }
        }
        //CreatePOGrid.refresh();
    }

}

function GridColumnCal() {
    // var AddCHGrid = $('#AdditionalChargesGrid').dxDataGrid('instance');
    var dataGrid = $("#CreatePOGrid").dxDataGrid('instance');
    var gridColBasicAmt = 0;
    var gridColTotalAmt = 0;
    var gridColCGSTAmt = 0;
    var gridColSGSTAmt = 0;
    var gridColIGSTAmt = 0;
    var gridAfterDisAmt = 0;
    var gridTaxAbleSum = 0;
    var gridColTotalQty = 0;
    var gridColTotalTax = 0;

    if (ChargesGrid.length > 0) {//Edit By Pradeep Yadav 06 sept 2019 
        TotalGstAmt = 0;
        for (var CH = 0; CH < ChargesGrid.length; CH++) {
            gridColTotalTax = parseFloat(Number(gridColTotalTax) + Number(ChargesGrid[CH].ChargesAmount)).toFixed(2);
            //if (ChargesGrid[CH].TaxType === "GST") {
            if (ChargesGrid[CH].TaxType === GblCompanyConfiguration[0].DefaultTaxLedgerTypeName.toUpperCase()) {
                var Chamt = 0;
                if (ChargesGrid[CH].ChargesAmount === undefined || ChargesGrid[CH].ChargesAmount === "undefined" || ChargesGrid[CH].ChargesAmount === "" || ChargesGrid[CH].ChargesAmount === "undefined" || ChargesGrid[CH].ChargesAmount === "null") {
                    Chamt = 0;
                }
                else {
                    Chamt = ChargesGrid[CH].ChargesAmount;
                }
                TotalGstAmt = Number(TotalGstAmt) + Number(Chamt);  //Edit By Pradeep Yadav  06 sept 2019
            }
        }
    }

    if (dataGrid._options.dataSource) {
        for (var cal = 0; cal < dataGrid._options.dataSource.length; cal++) {
            gridColBasicAmt = parseFloat(Number(gridColBasicAmt) + Number(dataGrid._options.dataSource[cal].BasicAmount)).toFixed(2);
            gridColTotalAmt = parseFloat(Number(gridColTotalAmt) + Number(dataGrid._options.dataSource[cal].TotalAmount)).toFixed(2);
            gridColTotalQty = parseFloat(Number(gridColTotalQty) + Number(dataGrid._options.dataSource[cal].PurchaseQuantity)).toFixed(2);

            gridColCGSTAmt = parseFloat(Number(gridColCGSTAmt) + Number(dataGrid._options.dataSource[cal].CGSTAmt)).toFixed(2);
            gridColSGSTAmt = parseFloat(Number(gridColSGSTAmt) + Number(dataGrid._options.dataSource[cal].SGSTAmt)).toFixed(2);
            gridColIGSTAmt = parseFloat(Number(gridColIGSTAmt) + Number(dataGrid._options.dataSource[cal].IGSTAmt)).toFixed(2);

            gridAfterDisAmt = parseFloat(Number(gridAfterDisAmt) + Number(dataGrid._options.dataSource[cal].AfterDisAmt)).toFixed(2);
            gridTaxAbleSum = parseFloat(Number(gridTaxAbleSum) + Number(dataGrid._options.dataSource[cal].TaxableAmount)).toFixed(2);
        }
    }

    document.getElementById("TxtBasicAmt").value = gridColBasicAmt;
    document.getElementById("TxtNetAmt").value = (Number(gridAfterDisAmt) + Number(gridColTotalTax)).toFixed(2);

    document.getElementById("TxtTotalQty").value = gridColTotalQty;

    document.getElementById("TxtCGSTAmt").value = gridColCGSTAmt;
    document.getElementById("TxtSGSTAmt").value = gridColSGSTAmt;
    document.getElementById("TxtIGSTAmt").value = gridColIGSTAmt;

    document.getElementById("TxtAfterDisAmt").value = gridAfterDisAmt;
    document.getElementById("Txt_TaxAbleSum").value = gridTaxAbleSum;

    document.getElementById("TxtTaxAmt").value = parseFloat(Number(gridColTotalTax)).toFixed(2);

    document.getElementById("TxtGstamt").value = parseFloat(Number(TotalGstAmt)).toFixed(2);  //Edit By Pradeep Yadav 06 sept 2019
    document.getElementById("TxtOtheramt").value = parseFloat(Number(gridColTotalTax) - Number(TotalGstAmt)).toFixed(2); //Edit By Pradeep Yadav 06 sept 2019
}

$("#BtnRefreshList").click(function () {
    OverFlowGrid();
});

$("#BtnCreateNewItem").click(function () {
    window.open('Masters.aspx', "_newtab");
});

$("#BtnopenPop").click(function () {
    Groupdata = "";
    var grid = $("#OverFlowGrid").dxDataGrid('instance');
    grid.clearSelection();

    var SelSupplierName = $('#SupplierName').dxSelectBox('instance').option('value');
    if (SelSupplierName !== "" && SelSupplierName !== null) {
        OverFlowGrid();
    }

    document.getElementById("BtnopenPop").setAttribute("data-toggle", "modal");
    document.getElementById("BtnopenPop").setAttribute("data-target", "#largeModalOverFlow");
});

$("#OverFlowGrid").dxDataGrid({
    dataSource: [],
    // columnAutoWidth: true,
    showBorders: true,
    showRowLines: true,
    allowColumnReordering: true,
    allowColumnResizing: true,
    columnResizingMode: "widget",
    hoverStateEnabled: true,
    paging: {
        pageSize: 150
    },
    pager: {
        showPageSizeSelector: true,
        allowedPageSizes: [150, 250, 500, 1000]
    },
    height: function () {
        return window.innerHeight / 1.3;
    },
    selection: { mode: "multiple" },
    //   filterRow: { visible: true, applyFilter: "onClick" }, // applyFilter ko onClick mein set karein
    headerFilter: { visible: true },
    filterRow: { visible: true, applyFilter: "auto" },
    searchPanel: { visible: false },
    loadPanel: {
        enabled: true,
        height: 90,
        width: 200,
        text: 'Data is loading...'
    },
    export: {
        enabled: true,
        fileName: "Exist Group",
        allowExportSelectedData: true
    },
    onExporting(e) {
        const workbook = new ExcelJS.Workbook();
        const worksheet = workbook.addWorksheet("StockDetail");

        DevExpress.excelExporter.exportDataGrid({
            component: e.component,
            worksheet,
            autoFilterEnabled: true,
        }).then(() => {
            workbook.xlsx.writeBuffer().then((buffer) => {
                saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'StockDeatil.xlsx');
            });
        });
        e.cancel = true;
    },
    onRowPrepared: function (e) {
        if (e.rowType === "header") {
            e.rowElement.css('background', '#509EBC');
            e.rowElement.css('color', 'white');
            e.rowElement.css('font-weight', 'bold');
        }
        e.rowElement.css('fontSize', '11px');
    },
    customizeColumns: function (columns) {
        columns.forEach(function (column) {
            column.calculateFilterExpression = function (filterValue, selectedFilterOperation) {
                if (typeof filterValue === 'string') {
                    var filterValues = filterValue.split(' ').filter(v => v);
                    var filterExpressions = filterValues.map(value => [this.dataField, "contains", value]);
                    return filterExpressions.reduce((prev, current) => prev ? [prev, "and", current] : current);
                } else if (typeof filterValue === 'number') {
                    return [this.dataField, "=", filterValue];
                }
                return null;
            };
        });
    },
    onSelectionChanged: function (selectedItems) {
        Groupdata = selectedItems.selectedRowsData;
        try {
            if (selectedItems.currentSelectedRowKeys.length > 0) {
                var dataGrid = $("#CreatePOGrid").dxDataGrid('instance');
                if (dataGrid._options.dataSource.length > 0) {
                    for (var k = 0; k < dataGrid._options.dataSource.length; k++) {
                        var cellvalItemID = dataGrid._options.dataSource[k].ItemID;
                        //if (clickedCell.data.ItemID === cellvalItemID) {
                        if (selectedItems.currentSelectedRowKeys[0].ItemID === cellvalItemID) {
                            //DevExpress.ui.notify("This item is already added,Please add another item..!", "warning", 1500);
                            DevExpress.ui.notify({
                                message: "This item is already added,Please add another item..!", type: "warning", displayTime: 5000, width: "900px",
                                onContentReady: function (e) {
                                    e.component.$content().find(".dx-toast-message").css({
                                        "font-size": "13px",
                                        "font-weight": "bold",
                                    });
                                    const closeButton = $("<div>")
                                        .addClass("dx-notification-close")
                                        .text("×")
                                        .css({
                                            "position": "absolute",
                                            "top": "5px",
                                            "right": "5px",
                                            "cursor": "pointer",
                                            "font-size": "25px",
                                        })
                                        .appendTo(e.component.$content());
                                    closeButton.on("click", function () {
                                        e.component.hide();
                                    });
                                }
                            });
                            selectedItems.component.deselectRows((selectedItems || {}).currentSelectedRowKeys[0]);
                            selectedItems.currentSelectedRowKeys = [];
                            return false;
                        }
                    }
                }
            }
        } catch (e) {
            console.log(e);
        }
    },
    columns: [
        { dataField: "ItemID", visible: false, caption: "Item ID", width: 100 },
        { dataField: "ItemGroupID", visible: false, caption: "Item Group ID", width: 100 },
        { dataField: "ItemGroupNameID", visible: false, caption: "Item Group Name ID", width: 100 },
        { dataField: "ItemCode", visible: true, caption: "Item Code", width: 80 },
        { dataField: "StockRefCode", visible: true, caption: " Stock Ref Code", width: 110 },
        { dataField: "ItemGroupName", visible: true, caption: "Item Group", width: 100 },
        { dataField: "ItemName", visible: true, caption: "Item Name", width: 490 },
        { dataField: "Quality", visible: true, caption: "Quality", width: 100 },
        { dataField: "GSM", visible: true, caption: "GSM", width: 80 },
        { dataField: "Manufecturer", visible: true, caption: "Manufecturer", width: 100 },
        { dataField: "Finish", visible: true, caption: "Finish", width: 80 },
        { dataField: "SizeW", visible: true, caption: "Size W", width: 80 },
        { dataField: "SizeL", visible: true, caption: "Size L", width: 80 },
        { dataField: "ItemDescription", visible: false, caption: "Item Description", width: 500 },
        { dataField: "BookedStock", visible: true, caption: "Total Booked", width: 100 },
        { dataField: "AllocatedStock", visible: true, caption: "Allocated Stock", width: 100 },
        { dataField: "PhysicalStock", visible: true, caption: "Current Stock", width: 100 },
        { dataField: "StockUnit", visible: true, caption: "Stock Unit", width: 80 },
        { dataField: "HSNCode", visible: true, caption: "HSN Code", width: 100 },
        { dataField: "ProductHSNName", visible: true, caption: "HSN Name", width: 130 },
        { dataField: "GSTTaxPercentage", visible: false, caption: "GSTTaxPercentage", width: 100 },
        { dataField: "CGSTTaxPercentage", visible: false, caption: "CGSTTaxPercentage", width: 100 },
        { dataField: "SGSTTaxPercentage", visible: false, caption: "SGSTTaxPercentage", width: 100 },
        { dataField: "IGSTTaxPercentage", visible: false, caption: "IGSTTaxPercentage", width: 100 },
        { dataField: "UnitDecimalPlace", visible: false, caption: "UnitDecimalPlace", width: 100 },
        { dataField: "WtPerPacking", visible: false, caption: "WtPerPacking", width: 100 },
        { dataField: "UnitPerPacking", visible: false, caption: "UnitPerPacking", width: 100 },
        { dataField: "ConversionFactor", visible: false, caption: "ConversionFactor", width: 100 },
        { dataField: "ConversionFormula", visible: false, caption: "ConversionFormula", width: 100 },
        { dataField: "UnitDecimalPlace", visible: false, caption: "UnitDecimalPlace", width: 100 }]
});

OverFlowGrid();
function OverFlowGrid() {
    var SelSupplierName = $('#SupplierName').dxSelectBox('instance').option('value');

    $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
    if (SelSupplierName === "" || SelSupplierName === undefined || SelSupplierName === null) {
        SelSupplierName = "";
    }
    $.ajax({ 
        type: "POST",
        url: "WebService_PurchaseOrder.asmx/GetOverFlowGrid",
        data: JSON.stringify({ SelSupplierName: SelSupplierName }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (results) { 
            let res1 = results.d.replace(/\\/g, '');
            res1 = res1.replace(/"d":""/g, '');
            res1 = res1.replace(/""/g, '');
            res1 = res1.replace(/u0027/g, "'");
            res1 = res1.replace(/u0026/g, "&");
            res1 = res1.replace(/:,/g, ":null,");
            res1 = res1.replace(/,}/g, ",null}");
            res1 = res1.replace(/:}/g, ":null}");
            res1 = res1.substr(1);
            res1 = res1.slice(0, -1); 
            var GetOverFlowGrid = JSON.parse(res1); 

            $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
            $("#OverFlowGrid").dxDataGrid({
                dataSource: GetOverFlowGrid
            });
        },
        error: function (xhr, status, error) {
            console.error("AJAX Error:", error);
            console.log("Response:", xhr.responseText);
        }
    });
}

$("#BtnNext").click(function () {
    var dataGrid = $("#CreatePOGrid").dxDataGrid('instance');
    $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
    if (Groupdata.length > 0) {
        for (var i = 0; i < Groupdata.length; i++) {
            var found = false;
            for (var k = 0; k <= dataGrid._options.dataSource.length - 1; k++) {
                var cellvalItemID = dataGrid._options.dataSource[k].ItemID;
                if (Groupdata[i].ItemID === cellvalItemID) {
                    // DevExpress.ui.notify("This row already added..Please add another row..!", "error", 1000);
                    found = true;
                }
            }
            if (found === false) {
                //*****************
                var ddlSupplierId = $('#SupplierName').dxSelectBox('instance').option('value');
                var PORate = 0;
                PORate = (Groupdata[i].PurchaseRate !== undefined && Groupdata[i].PurchaseRate !== null) ? Number(Groupdata[i].PurchaseRate) : 0;
                if (ddlSupplierId === "" || ddlSupplierId === null || ddlSupplierId === undefined || ddlSupplierId === "NULL") {
                    PORate = PORate;
                }
                else {
                    ObjItemRate = [];
                    ObjItemRate = ItemRateString.ItemRateObj.filter(function (el) {
                        return el.LedgerID === ddlSupplierId &&
                            el.ItemID === Groupdata[i].ItemID;
                    });
                    if (ObjItemRate === [] || ObjItemRate === "" || ObjItemRate === undefined || ObjItemRate.length <= 0) {
                        PORate = PORate;
                    } else {
                        PORate = ObjItemRate[0].PurchaseRate;
                    }
                }
                Groupdata[i].TransactionID = Number(document.getElementById("TxtPOID").value);
                Groupdata[i].TransID = 0;
                Groupdata[i].OrderUnit = Groupdata[i].StockUnit;
                Groupdata[i].PurchaseQty = 0;
                Groupdata[i].ExpectedDeliveryDate = "";
                Groupdata[i].ItemNarration = "";
                Groupdata[i].RequiredQuantity = 0;
                Groupdata[i].QuantityPerPack = (Groupdata[i].UnitPerPacking !== undefined && Groupdata[i].UnitPerPacking !== null && Number(Groupdata[i].UnitPerPacking) > 0) ? Number(Groupdata[i].UnitPerPacking) : 1;
                Groupdata[i].RequiredQuantityInPurchaseUnit = 0;

                Groupdata[i].VoucherID = "";
                Groupdata[i].MaxVoucherNo = "";
                Groupdata[i].VoucherNo = "";
                Groupdata[i].VoucherDate = "";
                Groupdata[i].PurchaseQuantity = 0;
                Groupdata[i].CreatedBy = "";
                Groupdata[i].Narration = "";
                Groupdata[i].FYear = "";
                Groupdata[i].PurchaseRate = PORate;
                /*Groupdata[i].Tolerance = 0;*/
                Groupdata[i].BasicAmount = 0;
                Groupdata[i].AfterDisAmt = 0;
                Groupdata[i].TaxableAmount = 0;
                Groupdata[i].CGSTAmt = 0;
                Groupdata[i].SGSTAmt = 0;
                Groupdata[i].IGSTAmt = 0;
                Groupdata[i].TotalAmount = 0;
                Groupdata[i].Disc = 0;
                Groupdata[i].RefJobBookingJobCardContentsID = "";
                Groupdata[i].RefJobCardContentNo = "";
                //Groupdata[i].PurchaseUnit = "";
                //Groupdata[i].ProductHSNName = Groupdata[i].ProductHSNName;
                //Groupdata[i].HSNCode = Groupdata[i].HSNCode;
                //Groupdata[i].GSTTaxPercentage = Groupdata[i].GSTTaxPercentage;
                //Groupdata[i].CGSTTaxPercentage = Groupdata[i].CGSTTaxPercentage;
                //Groupdata[i].SGSTTaxPercentage = Groupdata[i].SGSTTaxPercentage;
                //Groupdata[i].IGSTTaxPercentage = Groupdata[i].IGSTTaxPercentage;

                var clonedItem = $.extend({}, Groupdata[i]);
                dataGrid._options.dataSource.splice(dataGrid._options.dataSource.length, 0, clonedItem);
                //existReq.push(Groupdata[i]);
                dataGrid.refresh(true);
            }
        }
        $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
        //DevExpress.ui.notify("Item added in purchase item list..!", "success", 1000);
        DevExpress.ui.notify({
            message: "Item added in purchase item list..!", type: "success", displayTime: 5000, width: "900px",
            onContentReady: function (e) {
                e.component.$content().find(".dx-toast-message").css({
                    "font-size": "13px",
                    "font-weight": "bold",
                });
                const closeButton = $("<div>")
                    .addClass("dx-notification-close")
                    .text("×")
                    .css({
                        "position": "absolute",
                        "top": "5px",
                        "right": "5px",
                        "cursor": "pointer",
                        "font-size": "25px",
                    })
                    .appendTo(e.component.$content());
                closeButton.on("click", function () {
                    e.component.hide();
                });
            }
        });
        //clickedCell.component.clearFilter();

        document.getElementById("BtnNext").setAttribute("data-dismiss", "modal");
    }
    else {
        $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
        //DevExpress.ui.notify("please choose minimum one row from above Grid..!", "error", 1000);
        DevExpress.ui.notify({
            message: "please choose minimum one row from above Grid..!", type: "error", displayTime: 5000, width: "900px",
            onContentReady: function (e) {
                e.component.$content().find(".dx-toast-message").css({
                    "font-size": "13px",
                    "font-weight": "bold",
                });
                const closeButton = $("<div>")
                    .addClass("dx-notification-close")
                    .text("×")
                    .css({
                        "position": "absolute",
                        "top": "5px",
                        "right": "5px",
                        "cursor": "pointer",
                        "font-size": "25px",
                    })
                    .appendTo(e.component.$content());
                closeButton.on("click", function () {
                    e.component.hide();
                });
            }
        });
    }
});

$("#EditPOButton").click(function () {

    //let insPOGridProcess = $("#POGridProcess").dxDataGrid('instance');
    //let insPOGridProcessSelData = insPOGridProcess.getSelectedRowsData();
    let insPOGridProcessSelData = ProcessedGridSelectData;
    if (insPOGridProcessSelData.length <= 0) {
        alert("Please select any row to edit the P.O...!");
        return false;
    }

    $('#HalfPortions').html('');
    $('#HalfPortions').hide();
    ModalPopupScreencontrols();
    document.getElementById("BtnSave").disabled = false;
    document.getElementById("TxtPOID").value = insPOGridProcessSelData[0].TransactionID;
    document.getElementById("LblPONo").value = insPOGridProcessSelData[0].VoucherNo;
    validateUserData.userName = ""; validateUserData.password = ""; validateUserData.RecordID = document.getElementById("TxtPOID").value; validateUserData.transactionRemark = ""; validateUserData.isUserInfoFilled = false; validateUserData.documentNo = insPOGridProcessSelData[0].VoucherNo;

    VarItemApproved = insPOGridProcessSelData[0].IsVoucherItemApproved;
    $("#VoucherDate").dxDateBox({
        value: insPOGridProcessSelData[0].VoucherDate,
        min: null
    });
    $("#SupplierName").dxSelectBox({ value: insPOGridProcessSelData[0].LedgerID });
    $("#ContactPersonName").dxSelectBox({ value: insPOGridProcessSelData[0].ContactPersonID });
    $("#PurchaseDivision").dxSelectBox({ value: insPOGridProcessSelData[0].PurchaseDivision });
    $("#SelCurrencyCode").dxSelectBox({ value: insPOGridProcessSelData[0].CurrencyCode });
    $("#SelPOApprovalBy").dxSelectBox({ value: insPOGridProcessSelData[0].VoucherApprovalByEmployeeID });

    updateTotalTax = insPOGridProcessSelData[0].TotalTaxAmount;
    document.getElementById("TxtBasicAmt").value = insPOGridProcessSelData[0].BasicAmount;
    document.getElementById("TxtAfterDisAmt").value = insPOGridProcessSelData[0].AfterDisAmt;

    document.getElementById("TxtNetAmt").value = insPOGridProcessSelData[0].NetAmount;
    document.getElementById("PORefernce").value = insPOGridProcessSelData[0].PurchaseReference;
    document.getElementById("TxtTotalQty").value = insPOGridProcessSelData[0].TotalQuantity;
    document.getElementById("Txt_TaxAbleSum").value = insPOGridProcessSelData[0].TaxableAmount;

    PaymentTermsString = insPOGridProcessSelData[0].TermsOfPayment;

    document.getElementById("textDeliverAt").value = insPOGridProcessSelData[0].DeliveryAddress;

    $("#DealerName").dxSelectBox({ value: ((insPOGridProcessSelData[0].DealerID !== undefined && insPOGridProcessSelData[0].DealerID !== null) ? insPOGridProcessSelData[0].DealerID : 0) });

    $("#ModeOfTransport").dxSelectBox({ value: ((insPOGridProcessSelData[0].ModeOfTransport !== undefined && insPOGridProcessSelData[0].ModeOfTransport !== null) ? insPOGridProcessSelData[0].ModeOfTransport : "") });


    var TxtPOID = document.getElementById("TxtPOID").value;
    if (TxtPOID === "" || TxtPOID === null || TxtPOID === undefined) {
        alert("Please select any purchase order to edit or view !");
        return false;
    }
    $("#TxtAddPayTerms").dxSelectBox("instance").option("value", null);
    $("#SelLnameChargesGrid").dxSelectBox({
        value: ''
    });
    if (VarItemApproved === "true" || VarItemApproved === true) {
        document.getElementById("POPrintButton").disabled = false;
    } else {
        document.getElementById("POPrintButton").disabled = true;
    }
    validateUserData.userName = ""; validateUserData.password = ""; validateUserData.RecordID = document.getElementById("TxtPOID").value; validateUserData.actionType = "Update"; validateUserData.transactionRemark = ""; validateUserData.isUserInfoFilled = false; validateUserData.documentNo = document.getElementById("LblPONo").value;
    SubGridData = []; ChargesGrid = []; PaymentTermsGrid = []; ScheduleListOBJ = []; existReq = [];

    TotalGstAmt = 0; //Edit By Pradeep 06 sept 2019

    // $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);

    $.ajax({
        type: "POST",
        url: "WebService_PurchaseOrder.asmx/RetrivePoCreateGrid",
        data: '{transactionID:' + JSON.stringify(TxtPOID) + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            var res = results.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/""/g, '');
            res = res.replace(/u0026/g, '&');
            res = res.replace(/u0027/g, "'");
            res = res.replace(/:,/g, ":null,");
            res = res.replace(/,}/g, ",null}");
            res = res.substr(1);
            res = res.slice(0, -1);
            var ProcessRetrive = JSON.parse(res);
            existReq = [];
            var ProcessRetrive1 = {};
            if (ProcessRetrive.length > 0) {
                for (var i = 0; i < ProcessRetrive.length; i++) {
                    var result = $.grep(existReq, function (e) { return e.ItemID === ProcessRetrive[i].ItemID; });
                    if (result.length === 0) {
                        ProcessRetrive1 = {};
                        ProcessRetrive1.TransactionID = ProcessRetrive[i].PurchaseTransactionID;
                        ProcessRetrive1.TransID = ProcessRetrive[i].TransID;
                        ProcessRetrive1.VoucherID = ProcessRetrive[i].PurchaseVoucherID;
                        ProcessRetrive1.MaxVoucherNo = ProcessRetrive[i].PurchaseMaxVoucherNo;
                        ProcessRetrive1.ItemID = ProcessRetrive[i].ItemID;
                        ProcessRetrive1.ItemGroupID = ProcessRetrive[i].ItemGroupID;
                        ProcessRetrive1.ItemSubGroupID = ProcessRetrive[i].ItemSubGroupID;
                        ProcessRetrive1.ItemGroupNameID = ProcessRetrive[i].ItemGroupNameID;
                        ProcessRetrive1.VoucherNo = "";
                        ProcessRetrive1.VoucherDate = ProcessRetrive[i].PurchaseVoucherDate;
                        ProcessRetrive1.ItemCode = ProcessRetrive[i].ItemCode;
                        ProcessRetrive1.ItemGroupName = ProcessRetrive[i].ItemGroupName;
                        ProcessRetrive1.ItemSubGroupName = ProcessRetrive[i].ItemSubGroupName;
                        ProcessRetrive1.ItemName = ProcessRetrive[i].ItemName;
                        ProcessRetrive1.RefJobBookingJobCardContentsID = ProcessRetrive[i].PORefJobBookingJobCardContentsID;
                        ProcessRetrive1.RefJobCardContentNo = ProcessRetrive[i].PORefJobCardContentNo;
                        ProcessRetrive1.ClientID = ProcessRetrive[i].ClientID || 0;
                        ProcessRetrive1.ItemDescription = ProcessRetrive[i].ItemDescription;
                        ProcessRetrive1.RequiredQuantity = ProcessRetrive[i].TotalRequiredQuantity;
                        ProcessRetrive1.QuantityPerPack = ProcessRetrive[i].QuantityPerPack;
                        ProcessRetrive1.RequiredNoOfPacks = ProcessRetrive[i].RequiredNoOfPacks;
                        ProcessRetrive1.RequiredQuantityInPurchaseUnit = Number(StockUnitConversion(ProcessRetrive[i].ConversionFormula, Number(ProcessRetrive[i].TotalRequiredQuantity), Number(ProcessRetrive[i].UnitPerPacking), Number(ProcessRetrive[i].WtPerPacking), Number(ProcessRetrive[i].ConversionFactor), Number(ProcessRetrive[i].SizeW), Number(ProcessRetrive[i].UnitDecimalPlace), ProcessRetrive[i].StockUnit.toString(), ProcessRetrive[i].PurchaseUnit.toString(), Number(ProcessRetrive[i].GSM), Number(ProcessRetrive[i].ReleaseGSM), Number(ProcessRetrive[i].AdhesiveGSM), Number(ProcessRetrive[i].Thickness), Number(ProcessRetrive[i].Density)));
                        ProcessRetrive1.StockUnit = ProcessRetrive[i].PurchaseStockUnit;
                        ProcessRetrive1.PurchaseQuantityInStockUnit = Number(StockUnitConversion(ProcessRetrive[i].ConversionFormulaStockUnit, Number(ProcessRetrive[i].PurchaseQuantity), Number(ProcessRetrive[i].UnitPerPacking), Number(ProcessRetrive[i].WtPerPacking), Number(ProcessRetrive[i].ConversionFactor), Number(ProcessRetrive[i].SizeW), Number(ProcessRetrive[i].UnitDecimalPlaceStockUnit), ProcessRetrive[i].PurchaseUnit.toString(), ProcessRetrive[i].StockUnit.toString(), Number(ProcessRetrive[i].GSM), Number(ProcessRetrive[i].ReleaseGSM), Number(ProcessRetrive[i].AdhesiveGSM), Number(ProcessRetrive[i].Thickness), Number(ProcessRetrive[i].Density)));
                        ProcessRetrive1.PurchaseQuantity = ProcessRetrive[i].PurchaseQuantity;
                        ProcessRetrive1.PurchaseRate = ProcessRetrive[i].PurchaseRate;
                        ProcessRetrive1.PurchaseUnit = ProcessRetrive[i].PurchaseUnit;
                        ProcessRetrive1.ExpectedDeliveryDate = ProcessRetrive[i].ExpectedDeliveryDate;
                        ProcessRetrive1.Tolerance = ProcessRetrive[i].Tolerance;
                        ProcessRetrive1.ItemNarration = ProcessRetrive[i].ItemNarration;
                        ProcessRetrive1.JobName = ProcessRetrive[i].JobName;
                        ProcessRetrive1.BasicAmount = ProcessRetrive[i].BasicAmount;
                        ProcessRetrive1.Disc = ProcessRetrive[i].Disc;
                        ProcessRetrive1.AfterDisAmt = ProcessRetrive[i].AfterDisAmt;
                        ProcessRetrive1.TaxableAmount = ProcessRetrive[i].TaxableAmount;
                        ProcessRetrive1.GSTTaxPercentage = ProcessRetrive[i].GSTTaxPercentage;
                        ProcessRetrive1.CGSTTaxPercentage = ProcessRetrive[i].CGSTTaxPercentage;
                        ProcessRetrive1.SGSTTaxPercentage = ProcessRetrive[i].SGSTTaxPercentage;
                        ProcessRetrive1.IGSTTaxPercentage = ProcessRetrive[i].IGSTTaxPercentage;
                        ProcessRetrive1.CGSTAmt = ProcessRetrive[i].CGSTAmt;
                        ProcessRetrive1.SGSTAmt = ProcessRetrive[i].SGSTAmt;
                        ProcessRetrive1.IGSTAmt = ProcessRetrive[i].IGSTAmt;
                        ProcessRetrive1.TotalAmount = ProcessRetrive[i].TotalAmount;
                        ProcessRetrive1.PurchaseQuantityComp = ProcessRetrive[i].RequisitionQty;
                        ProcessRetrive1.CreatedBy = ProcessRetrive[i].CreatedBy;
                        ProcessRetrive1.Narration = ProcessRetrive[i].Narration;
                        ProcessRetrive1.FYear = ProcessRetrive[i].FYear;
                        ProcessRetrive1.ProductHSNName = ProcessRetrive[i].ProductHSNName;
                        ProcessRetrive1.ProductHSNID = ProcessRetrive[i].ProductHSNID;
                        ProcessRetrive1.HSNCode = ProcessRetrive[i].HSNCode;
                        ProcessRetrive1.WtPerPacking = ProcessRetrive[i].WtPerPacking;
                        ProcessRetrive1.UnitPerPacking = ProcessRetrive[i].UnitPerPacking;
                        ProcessRetrive1.ConversionFactor = ProcessRetrive[i].ConversionFactor;
                        ProcessRetrive1.ConversionFormula = ProcessRetrive[i].ConversionFormula;
                        ProcessRetrive1.UnitDecimalPlace = ProcessRetrive[i].UnitDecimalPlace;
                        ProcessRetrive1.ConversionFormulaStockUnit = ProcessRetrive[i].ConversionFormulaStockUnit;
                        ProcessRetrive1.UnitDecimalPlaceStockUnit = ProcessRetrive[i].UnitDecimalPlaceStockUnit;
                        ProcessRetrive1.SizeW = ProcessRetrive[i].SizeW;
                        ProcessRetrive1.GSM = ProcessRetrive[i].GSM;
                        ProcessRetrive1.ReleaseGSM = ProcessRetrive[i].ReleaseGSM;
                        ProcessRetrive1.AdhesiveGSM = ProcessRetrive[i].AdhesiveGSM;
                        ProcessRetrive1.Thickness = ProcessRetrive[i].Thickness;
                        ProcessRetrive1.Density = ProcessRetrive[i].Density;
                        ProcessRetrive1.Remark = ProcessRetrive[i].Remark;
                        existReq.push(ProcessRetrive1);
                    }
                }
                SubGridData = [];
                SubGridData = ProcessRetrive;
                //existReq = ProcessRetrive;

                //$.ajax({
                //    type: "POST",
                //    url: "WebService_PurchaseOrder.asmx/RetriveRequisitionDetail",
                //    data: '{transactionID:' + JSON.stringify(TxtPOID) + '}',
                //    contentType: "application/json; charset=utf-8",
                //    dataType: "text",
                //    success: function (results) {
                //        ////console.debug(results);
                //        var res = results.replace(/\\/g, '');
                //        res = res.replace(/"d":""/g, '');
                //        res = res.replace(/""/g, '');
                //        res = res.substr(1);
                //        res = res.slice(0, -1);
                //        var RetriveRequisition = JSON.parse(res);
                //        SubGridData = [];
                //        SubGridData = RetriveRequisition;

                $.ajax({
                    type: "POST",
                    url: "WebService_PurchaseOrder.asmx/RetrivePoSchedule",
                    data: '{transactionID:' + JSON.stringify(TxtPOID) + '}',
                    contentType: "application/json; charset=utf-8",
                    dataType: "text",
                    success: function (results) {
                        var res = results.replace(/\\/g, '');
                        res = res.replace(/"d":""/g, '');
                        res = res.replace(/""/g, '');
                        res = res.replace(/u0026/g, '&');
                        res = res.replace(/u0027/g, "'");
                        res = res.replace(/:,/g, ":null,");
                        res = res.replace(/,}/g, ",null}");
                        res = res.substr(1);
                        res = res.slice(0, -1);
                        var ProcessRetriveSch = JSON.parse(res);
                        ScheduleListOBJ = [];
                        ScheduleListOBJ = ProcessRetriveSch;
                    }
                });

                $.ajax({
                    type: "POST",
                    url: "WebService_PurchaseOrder.asmx/RetrivePoOverHead",
                    data: '{transactionID:' + JSON.stringify(TxtPOID) + '}',
                    contentType: "application/json; charset=utf-8",
                    dataType: "text",
                    success: function (results) {
                        var res = results.replace(/\\/g, '');
                        res = res.replace(/"d":""/g, '');
                        res = res.replace(/""/g, '');
                        res = res.replace(/u0026/g, '&');
                        res = res.replace(/u0027/g, "'");
                        res = res.replace(/:,/g, ":null,");
                        res = res.replace(/,}/g, ",null}");
                        res = res.substr(1);
                        res = res.slice(0, -1);
                        var ProcessRetriveOverHead = JSON.parse(res);
                        var OtherHeadsGrid = $("#OtherHeadsGrid").dxDataGrid('instance');

                        if (ProcessRetriveOverHead.length > 0) {
                            for (var p = 0; p < ProcessRetriveOverHead.length; p++) {
                                var HeadId = ProcessRetriveOverHead[p].HeadID;
                                for (var j = 0; j < OtherHead.length; j++) {
                                    var Exist_HeadId = OtherHead[j].HeadID;
                                    if (Exist_HeadId === HeadId) {
                                        OtherHead[j].Weight = ProcessRetriveOverHead[p].Weight;
                                        OtherHead[j].Rate = ProcessRetriveOverHead[p].Rate;
                                        OtherHead[j].HeadAmount = ProcessRetriveOverHead[p].HeadAmount;
                                        OtherHead[j].Sel = true;
                                    }
                                }
                            }
                        }

                        fillOtherHeadsGrid(OtherHead);
                    }
                });

                $.ajax({
                    type: "POST",
                    url: "WebService_PurchaseOrder.asmx/RetrivePoCreateTaxChares",
                    data: '{transactionID:' + JSON.stringify(TxtPOID) + '}',
                    contentType: "application/json; charset=utf-8",
                    dataType: "text",
                    success: function (results) {
                        ////console.debug(results);
                        var res = results.replace(/\\/g, '');
                        res = res.replace(/"d":""/g, '');
                        res = res.replace(/""/g, '');
                        res = res.replace(/u0026/g, '&');
                        res = res.replace(/u0027/g, "'");
                        res = res.replace(/:,/g, ":null,");
                        res = res.replace(/,}/g, ",null}");
                        res = res.substr(1);
                        res = res.slice(0, -1);
                        var ProcessRetriveTaxCharges = JSON.parse(res);
                        $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);

                        ChargesGrid = [];
                        ChargesGrid = ProcessRetriveTaxCharges;

                        FrmUpdateTotalGstAmt = 0;////Edit By Pradeep Yadav 06 sept 2019                         
                        for (var CH = 0; CH < ChargesGrid.length; CH++) {
                            //if (ChargesGrid[CH].TaxType === "GST") {
                            if (ChargesGrid[CH].TaxType === GblCompanyConfiguration[0].DefaultTaxLedgerTypeName.toUpperCase()) {
                                var Chamt = 0;
                                if (ChargesGrid[CH].ChargesAmount === undefined || ChargesGrid[CH].ChargesAmount === "undefined" || ChargesGrid[CH].ChargesAmount === "" || ChargesGrid[CH].ChargesAmount === "undefined" || ChargesGrid[CH].ChargesAmount === "null") {
                                    Chamt = 0;
                                }
                                else {
                                    Chamt = ChargesGrid[CH].ChargesAmount;
                                }
                                FrmUpdateTotalGstAmt = Number(FrmUpdateTotalGstAmt) + Number(Chamt);  //Edit By Pradeep Yadav  06 sept 2019
                            }
                        }////Edit By Pradeep Yadav 06 sept 2019  

                        fillChargesGrid();

                        document.getElementById("TxtTaxAmt").value = parseFloat(Number(updateTotalTax)).toFixed(2);

                        document.getElementById("TxtGstamt").value = parseFloat(Number(FrmUpdateTotalGstAmt)).toFixed(2);  //Edit By Pradeep Yadav 06 sept 2019
                        document.getElementById("TxtOtheramt").value = parseFloat(Number(updateTotalTax) - Number(FrmUpdateTotalGstAmt)).toFixed(2); //Edit By Pradeep Yadav 06 sept 2019

                        var gridAfterDisAmt = parseFloat(Number(document.getElementById("TxtAfterDisAmt").value)).toFixed(2);
                        var gridColTotalTax = parseFloat(Number(document.getElementById("TxtTaxAmt").value)).toFixed(2);
                        document.getElementById("TxtNetAmt").value = (Number(gridAfterDisAmt) + Number(gridColTotalTax)).toFixed(2);

                    }
                });


                ShowCreatePOGrid();
                fillPayTermsGrid();
                LoadFileData();
                //    }
                //});
                //    }
                //});
            }
        }
    });

    document.getElementById("BtnDeletePopUp").disabled = false;
    document.getElementById("BtnSaveAS").disabled = false;
    GblStatus = "Update";

    //document.getElementById("EditPOButton").setAttribute("data-toggle", "modal");
    //document.getElementById("EditPOButton").setAttribute("data-target", "#largeModal");
    $('#largeModal').modal({
        show: 'true'
    });

});

$("#DeletePOButton").click(async function () {
    var TxtPOID = document.getElementById("TxtPOID").value;
    if (TxtPOID === "" || TxtPOID === null || TxtPOID === undefined) {
        alert("Please select any purchase order to delete..!");
        return false;
    }
    var selectedRow = $("#POGridProcess").dxDataGrid("instance").getSelectedRowsData()[0];
    if (isCurrentFinancialYear(selectedRow.FYear) == false) {
        swal("Warning", "Selected Purchase Order is not allowed to Delete  in the logged-in financial year.", "warning");
        return false;
    }
    if (Number(SelectedProductionUnitID) != 0) {
        if (GBLProductionUnitID != SelectedProductionUnitID) {
            swal("Attention!", "Selected transaction is related to unit " + SelectedProductionUnitName + ", Kindly login with unit " + SelectedProductionUnitName + " to process.", "warning");
            $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
            return;
        }
    }
    if (validateUserData.isUserInfoFilled === false) {
        validateUserData.userName = ""; validateUserData.password = ""; validateUserData.RecordID = document.getElementById("TxtPOID").value; validateUserData.actionType = "Delete"; validateUserData.transactionRemark = ""; validateUserData.isUserInfoFilled = false;
        let result = await openSecurityPanelModal(validateUserData);
    }

    swal({
        title: "Are you sure to delete this transaction..?",
        text: 'You will not be able to recover this transaction..!',
        type: "warning",
        showCancelButton: true,
        confirmButtonColor: "#DD6B55",
        confirmButtonText: "Yes, delete it!",
        closeOnConfirm: true
    },
        function () {
            $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
            $.ajax({
                type: "POST",
                url: "WebService_PurchaseOrder.asmx/DeletePaperPurchaseOrder",
                data: '{TxtPOID:' + JSON.stringify(TxtPOID) + ',ObjvalidateLoginUser:' + JSON.stringify(validateUserData) + '}',
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (results) {
                    var res = JSON.stringify(results);
                    res = res.replace(/"d":/g, '');
                    res = res.replace(/{/g, '');
                    res = res.replace(/}/g, '');
                    res = res.substr(1);
                    res = res.slice(0, -1);

                    $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                    var Title, Text, Type;
                    if (res === "Success") {
                        Text = "Your data deleted successfully..";
                        Title = "Deleted...";
                        Type = "success";
                        validateUserData.isUserInfoFilled = false;
                    } else if (res.includes("not authorized")) {
                        Title = "Not Deleted..!";
                        Text = res;
                        Type = "warning";
                    } else if (res === "TransactionUsed") {
                        swal("error", "This item is used in another process..! Record can not be delete.", "error");
                        return false;
                    } else if (res === "PurchaseOrderApproved") {
                        swal("error", "Sorry Your Purchase Order is Approved. Please go and Unapprove this Purchase Order first and then Delete it.", "error");
                        return false;
                    } else if (res === "InvalidUser") {
                        swal("Invalid User!", "Invalid user credentials, please enter valid username or password to delete the information.", "error");
                        validateUserData.isUserInfoFilled = false;
                        return false;
                    } else {
                        swal.close();
                        setTimeout(() => {
                            swal("Warning..!", res, "warning");
                        }, 100);
                    }
                    swal.close();
                    setTimeout(() => {
                        swal(Title, Text, Type);
                    }, 100);

                    if (Type === "success") location.reload();
                },
                error: function errorFunc(jqXHR) {
                    $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                    console.log(jqXHR);
                }
            });

        });
});


$("#BtnNew").click(function () {
    location.reload();
});

$("#BtnDeletePopUp").click(function () {
    $("#DeletePOButton").click();
});

$("#BtnSaveAS").click(function () {
    GblStatus = "Save";
    if (GblStatus === "Save") {
        $("#BtnSave").click();
    }
});

$("#reloadDisplayNone").click(function () {
    document.getElementById("reloadDisplayNone").setAttribute("data-dismiss", "modal");
});

//Schedule...
$("#SelDelDate").dxDateBox({
    pickerType: "calendar",
    displayFormat: 'dd-MMM-yyyy',
    value: new Date().toISOString().substr(0, 10)
});

$("#BtnScheduleAdd").click(function () {
    var dataGrid = $("#CreatePOGrid").dxDataGrid('instance');
    var CompItemID = document.getElementById("SchItemIDLbl").value;
    var CompQty = Number(document.getElementById("SchQtyLbl").innerHtml);
    var CompStocUnit = document.getElementById("TxtUnitSch").value;
    var CompItemCode = document.getElementById("SchItemCodeLbl").value;

    var expDate = $('#SelDelDate').dxDateBox('instance').option('value');
    var QtySlot = Number(document.getElementById("TxtQtySch").value);

    if (QtySlot === "") {
        document.getElementById("TxtQtySch").focus();
        //DevExpress.ui.notify("Please eneter Quantity..!", "error", 1000);
        DevExpress.ui.notify({
            message: "Please eneter Quantity..!", type: "warning", displayTime: 5000, width: "900px",
            onContentReady: function (e) {
                e.component.$content().find(".dx-toast-message").css({
                    "font-size": "13px",
                    "font-weight": "bold",
                });
                const closeButton = $("<div>")
                    .addClass("dx-notification-close")
                    .text("×")
                    .css({
                        "position": "absolute",
                        "top": "5px",
                        "right": "5px",
                        "cursor": "pointer",
                        "font-size": "25px",
                    })
                    .appendTo(e.component.$content());
                closeButton.on("click", function () {
                    e.component.hide();
                });
            }
        });
        return false;
    }
    if (isNaN(QtySlot)) {
        document.getElementById("TxtQtySch").focus();
        //DevExpress.ui.notify("Please eneter only numeric value..!", "error", 1000);
        DevExpress.ui.notify({
            message: "Please eneter only numeric value..!", type: "warning", displayTime: 5000, width: "900px",
            onContentReady: function (e) {
                e.component.$content().find(".dx-toast-message").css({
                    "font-size": "13px",
                    "font-weight": "bold",
                });
                const closeButton = $("<div>")
                    .addClass("dx-notification-close")
                    .text("×")
                    .css({
                        "position": "absolute",
                        "top": "5px",
                        "right": "5px",
                        "cursor": "pointer",
                        "font-size": "25px",
                    })
                    .appendTo(e.component.$content());
                closeButton.on("click", function () {
                    e.component.hide();
                });
            }
        });
        return false;
    }

    var MakeNewArray = "";

    var AllotedQty = 0;
    var optSch = {};
    if (ScheduleListOBJ === [] || ScheduleListOBJ === "" || ScheduleListOBJ === undefined || ScheduleListOBJ === null) {
        if (QtySlot > CompQty) {
            //DevExpress.ui.notify("Quantity should not be greater then Purchase Quantity..!", "error", 1000);
            DevExpress.ui.notify({
                message: "Quantity should not be greater then Purchase Quantity..!", type: "error", displayTime: 5000, width: "900px",
                onContentReady: function (e) {
                    e.component.$content().find(".dx-toast-message").css({
                        "font-size": "13px",
                        "font-weight": "bold",
                    });
                    const closeButton = $("<div>")
                        .addClass("dx-notification-close")
                        .text("×")
                        .css({
                            "position": "absolute",
                            "top": "5px",
                            "right": "5px",
                            "cursor": "pointer",
                            "font-size": "25px",
                        })
                        .appendTo(e.component.$content());
                    closeButton.on("click", function () {
                        e.component.hide();
                    });
                }
            });
            return false;
        }
        else {
            optSch = {};
            optSch.id = 1;
            optSch.ItemID = CompItemID;
            optSch.ItemCode = CompItemCode;
            optSch.Quantity = QtySlot;
            optSch.PurchaseUnit = CompStocUnit;
            optSch.SchDate = expDate;

            ScheduleListOBJ.push(optSch);
            DistinctArray.push(optSch);
        }
    } else {
        var arr = [];
        for (var s = 0; s < ScheduleListOBJ.length; s++) {
            arr.push(ScheduleListOBJ[s].id);
        }
        var max = "";
        if (arr !== "" && arr !== []) {
            max = Math.max.apply(null, arr);
        }

        MakeNewArray = { 'ExistRec': ScheduleListOBJ };
        DistinctArray = MakeNewArray.ExistRec.filter(function (el) {
            return el.ItemID === CompItemID;
        });
        for (var x = 0; x < DistinctArray.length; x++) {
            AllotedQty = AllotedQty + Number(DistinctArray[x].Quantity);
        }

        var ttlQty = AllotedQty + QtySlot;

        var IncludeString = JSON.stringify(DistinctArray);
        var confirmInclude = IncludeString.includes(expDate);

        if (Number(ttlQty) > CompQty) {
            //DevExpress.ui.notify("Quantity should not be greater then Purchase Quantity..!", "error", 1000);
            DevExpress.ui.notify({
                message: "Quantity should not be greater then Purchase Quantity..!", type: "error", displayTime: 5000, width: "900px",
                onContentReady: function (e) {
                    e.component.$content().find(".dx-toast-message").css({
                        "font-size": "13px",
                        "font-weight": "bold",
                    });
                    const closeButton = $("<div>")
                        .addClass("dx-notification-close")
                        .text("×")
                        .css({
                            "position": "absolute",
                            "top": "5px",
                            "right": "5px",
                            "cursor": "pointer",
                            "font-size": "25px",
                        })
                        .appendTo(e.component.$content());
                    closeButton.on("click", function () {
                        e.component.hide();
                    });
                }
            });
            return false;
        }
        else if (confirmInclude === true) {
            //DevExpress.ui.notify("This date alrady Booked to another delivery..! Please Choose another Date...", "error", 1000);
            DevExpress.ui.notify({
                message: "This date alrady Booked to another delivery..! Please Choose another Date...", type: "error", displayTime: 5000, width: "900px",
                onContentReady: function (e) {
                    e.component.$content().find(".dx-toast-message").css({
                        "font-size": "13px",
                        "font-weight": "bold",
                    });
                    const closeButton = $("<div>")
                        .addClass("dx-notification-close")
                        .text("×")
                        .css({
                            "position": "absolute",
                            "top": "5px",
                            "right": "5px",
                            "cursor": "pointer",
                            "font-size": "25px",
                        })
                        .appendTo(e.component.$content());
                    closeButton.on("click", function () {
                        e.component.hide();
                    });
                }
            });
            return false;
        }
        else {
            optSch = {};
            optSch.id = max + 1;
            optSch.ItemID = CompItemID;
            optSch.ItemCode = CompItemCode;
            optSch.Quantity = QtySlot;
            optSch.PurchaseUnit = CompStocUnit;
            optSch.SchDate = expDate;

            DistinctArray.push(optSch);
            ScheduleListOBJ.push(optSch);
        }
    }

    document.getElementById("TxtQtySch").value = "";

    //$("#ScheduleGrid").dxDataGrid({
    //    dataSource: DistinctArray,
    //});
    fillGridSchedule(DistinctArray);
    //document.getElementById("SchQtyLbl").innerHtml = dataGrid._options.dataSource[GetRow].PurchaseQuantity;
    //document.getElementById("SchStockUnitLbl").innerHtml = dataGrid._options.dataSource[GetRow].StockUnit;
    //document.getElementById("SchDelDateLbl").innerHtml = dataGrid._options.dataSource[GetRow].ExpectedDeliveryDate;
    //document.getElementById("SchItemIDLbl").innerHtml = dataGrid._options.dataSource[GetRow].ItemID;

});

//fillGridSchedule();
function fillGridSchedule(DistinctArray) {
    $("#ScheduleGrid").dxDataGrid({
        dataSource: DistinctArray,
        columnAutoWidth: true,
        showBorders: true,
        showRowLines: true,
        allowColumnReordering: true,
        allowColumnResizing: true,
        sorting: {
            mode: "none" // or "multiple" | "single"
        },
        filterRow: { visible: false, applyFilter: "auto" },
        loadPanel: {
            enabled: true,
            height: 90,
            width: 200,
            text: 'Data is loading...'
        },
        editing: {
            mode: "cell",
            allowDeleting: true
            //allowAdding: true,
            // allowUpdating: true
        },
        onRowRemoving: function (e) {
            RemID = "";
            RemID = e.data.id;
        },
        onRowRemoved: function (e) {
            ScheduleListOBJ = ScheduleListOBJ.filter(function (obj) {
                return obj.id !== RemID;
            });
        },
        onRowPrepared: function (e) {
            if (e.rowType === "header") {
                e.rowElement.css('background', '#509EBC');
                e.rowElement.css('color', 'white');
                e.rowElement.css('font-weight', 'bold');
            }
            e.rowElement.css('fontSize', '11px');
        },
        columns: [{ dataField: "id", visible: false, caption: "Seq.No" },
        { dataField: "ItemID", visible: false, caption: "ItemID" },
        { dataField: "ItemCode", visible: true, caption: "Item Code" },
        { dataField: "Quantity", visible: true, caption: "Quantity" },
        { dataField: "PurchaseUnit", visible: true, caption: "Purchase Unit" },
        { dataField: "SchDate", visible: true, caption: "Schedule Date", dataType: "date", format: "dd-MMM-yyyy" }
        ]
    });
}

//Additional Charges

var CalculateOnLookup = [{ "ID": 1, "Name": "Value" }, { "ID": 2, "Name": "Quantity" }];

fillChargesGrid();

var CHRow = "", CHCol = "";
var Var_ChargeHead = "";
var ObjChargeHead = [];

function fillChargesGrid() {
    var LName = [];
    $.ajax({
        type: "POST",
        url: "WebService_PurchaseOrder.asmx/CHLname",
        data: '{}',
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            var res = results.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/""/g, '');
            res = res.replace(/u0026/g, '&');
            res = res.replace(/u0027/g, "'");
            res = res.replace(/:,/g, ":null,");
            res = res.replace(/,}/g, ",null}");
            res = res.substr(1);
            res = res.slice(0, -1);

            var CHLname_RESS = JSON.parse(res);

            for (var z = 0; z < CHLname_RESS.length; z++) {
                var optLN = {};
                optLN.LedgerID = CHLname_RESS[z].LedgerID;
                optLN.LedgerName = CHLname_RESS[z].LedgerName;
                LName.push(optLN);
            }
            Var_ChargeHead = { 'LedgerDetail': CHLname_RESS };

            $("#SelLnameChargesGrid").dxSelectBox({
                items: LName,
                placeholder: "Choose Ledger Name--",
                displayExpr: 'LedgerName',
                valueExpr: 'LedgerID',
                searchEnabled: true,
                showClearButton: true
                //onValueChanged: function (data) {
                //    if (document.getElementById("TxtAfterDisAmt").value === 0) {
                //        $("#SelLnameChargesGrid").dxSelectBox({
                //            value: '',
                //        });
                //        DevExpress.ui.notify("Please enter Purchase rate in above grid before add charges..!", "error", 1000);
                //        window.setTimeout(function () { e.component.cancelEditData(); }, 0)
                //    } else {
                //        var optCH = {};
                //        var ChooseText = $("#SelLnameChargesGrid").dxSelectBox('instance').option('text');
                //        if (ChooseText !== "" && ChooseText !== undefined && ChooseText !== null) {
                //            ObjChargeHead = Var_ChargeHead.LedgerDetail.filter(function (el) {
                //                return el.LedgerID === data.value;
                //            });
                //            optCH.LedgerName = ChooseText;
                //            optCH.LedgerID = ObjChargeHead[0].LedgerID;
                //            var gstapl = ObjChargeHead[0].GSTApplicable;
                //            if (gstapl === "False" || gstapl === false) {
                //                gstapl = false;
                //            }
                //            else if (gstapl === "True" || gstapl === true) {
                //                gstapl = true
                //            }
                //            optCH.GSTApplicable = gstapl;
                //            optCH.TaxType = ObjChargeHead[0].TaxType;
                //            optCH.GSTLedgerType = ObjChargeHead[0].GSTLedgerType;
                //            optCH.CalculateON = ObjChargeHead[0].GSTCalculationOn;
                //            optCH.TaxRatePer = ObjChargeHead[0].TaxPercentage;
                //            ChargesGrid.push(optCH);


                //            $("#AdditionalChargesGrid").dxDataGrid({
                //                dataSource: ChargesGrid,
                //            });                            

                //            AddItemCalculation();
                //            GridColumnCal();
                //            AddItemWithChargessGrid();
                //            var CreatePOGrid = $('#CreatePOGrid').dxDataGrid('instance');
                //            CreatePOGrid.refresh();
                //            var AdditionalChargesGrid = $('#AdditionalChargesGrid').dxDataGrid('instance');
                //            AdditionalChargesGrid.refresh();
                //        }
                //    }
                //},

            });
            var t = 0;
            var chGrid = {};
            $("#AdditionalChargesGrid").dxDataGrid({
                dataSource: ChargesGrid,
                columnAutoWidth: true,
                showBorders: true,
                showRowLines: true,
                allowColumnReordering: true,
                allowColumnResizing: true,
                //filterRow: { visible: true, applyFilter: "auto" },
                sorting: {
                    mode: "none" // or "multiple" | "single"
                },
                //loadPanel: {
                //    enabled: true,
                //    height: 90,
                //    width: 200,
                //    text: 'Data is loading...'
                //},
                editing: {
                    mode: "cell",
                    allowDeleting: true,
                    // allowAdding: true,
                    allowUpdating: true,
                    useIcons: true,
                },
                onRowPrepared: function (e) {
                    if (e.rowType === "header") {
                        e.rowElement.css('background', '#509EBC');
                        e.rowElement.css('color', 'white');
                        e.rowElement.css('font-weight', 'bold');
                    }
                    e.rowElement.css('fontSize', '11px');
                },
                onEditingStart: function (e) {
                    if (e.data.InAmount === true || e.data.InAmount === 1) {
                        if (e.column.dataField === "ChargesAmount") {
                            e.cancel = false;
                        }
                    }
                    else {
                        if (e.column.dataField !== "ChargesAmount" && e.column.dataField !== "TaxType") {
                            e.cancel = false;
                        } else {
                            e.cancel = true;
                        }
                    }
                },
                onRowRemoved: function (e) {
                    //ChargesGrid = ChargesGrid.filter(function (obj) {
                    //    return obj.LedgerID !== e.data.LedgerID;

                    //if (ChargesGrid.length > 0) {
                    //    AddItemWithChargessGrid();
                    //} else {
                    //    AddItemCalculation();
                    //}

                    AddItemCalculation();
                    GridColumnCal();
                    var CreatePOGrid = $("#CreatePOGrid").dxDataGrid('instance');
                    CreatePOGrid.refresh();
                    let AdditionalChargesGrid = $("#AdditionalChargesGrid").dxDataGrid('instance');
                    AdditionalChargesGrid.refresh();
                    ChargesGrid = [];

                    for (t = 0; t < AdditionalChargesGrid._options.dataSource.length; t++) {
                        chGrid = {};
                        chGrid.CalculateON = AdditionalChargesGrid._options.dataSource[t].CalculateON;
                        chGrid.ChargesAmount = AdditionalChargesGrid._options.dataSource[t].ChargesAmount;
                        chGrid.GSTApplicable = AdditionalChargesGrid._options.dataSource[t].GSTApplicable;
                        chGrid.GSTLedgerType = AdditionalChargesGrid._options.dataSource[t].GSTLedgerType;
                        chGrid.LedgerID = AdditionalChargesGrid._options.dataSource[t].LedgerID;
                        chGrid.LedgerName = AdditionalChargesGrid._options.dataSource[t].LedgerName;
                        chGrid.TaxRatePer = AdditionalChargesGrid._options.dataSource[t].TaxRatePer;
                        chGrid.TaxType = AdditionalChargesGrid._options.dataSource[t].TaxType;
                        chGrid.InAmount = AdditionalChargesGrid._options.dataSource[t].InAmount;
                        ChargesGrid.push(chGrid);
                    }

                    //AddItemWithChargessGrid();
                    CalculateAmount();
                    //});
                },
                onRowUpdated: function (CHGRID) {
                    AddItemCalculation();
                    GridColumnCal();
                    //AddItemWithChargessGrid();
                    var CreatePOGrid = $("#CreatePOGrid").dxDataGrid('instance');
                    CreatePOGrid.refresh();
                    let AdditionalChargesGrid = $("#AdditionalChargesGrid").dxDataGrid('instance');
                    AdditionalChargesGrid.refresh();
                    ChargesGrid = [];
                    for (t = 0; t < AdditionalChargesGrid._options.dataSource.length; t++) {
                        chGrid = {};
                        chGrid.CalculateON = AdditionalChargesGrid._options.dataSource[t].CalculateON;
                        chGrid.ChargesAmount = AdditionalChargesGrid._options.dataSource[t].ChargesAmount;
                        chGrid.GSTApplicable = AdditionalChargesGrid._options.dataSource[t].GSTApplicable;
                        chGrid.GSTLedgerType = AdditionalChargesGrid._options.dataSource[t].GSTLedgerType;
                        chGrid.LedgerID = AdditionalChargesGrid._options.dataSource[t].LedgerID;
                        chGrid.LedgerName = AdditionalChargesGrid._options.dataSource[t].LedgerName;
                        chGrid.TaxRatePer = AdditionalChargesGrid._options.dataSource[t].TaxRatePer;
                        chGrid.TaxType = AdditionalChargesGrid._options.dataSource[t].TaxType;
                        chGrid.InAmount = AdditionalChargesGrid._options.dataSource[t].InAmount;
                        ChargesGrid.push(chGrid);
                    }

                    //AddItemWithChargessGrid();
                    CalculateAmount();
                },
                //onCellPrepared: function (CHGRID) {
                //    var AdditionalChargesGrid = $('#AdditionalChargesGrid').dxDataGrid('instance');
                //    if (CHGRID.rowType === undefined || CHGRID.rowType === "header" || CHGRID.rowType !== "data") return false;

                //    if (CHGRID.columnIndex === 7 && CHGRID.data.TaxType !== "" && CHGRID.data.TaxType !== undefined && CHGRID.data.TaxType !== null) {

                //        AddItemCalculation();
                //        GridColumnCal();
                //        AddItemWithChargessGrid();
                //        var CreatePOGrid = $('#CreatePOGrid').dxDataGrid('instance');
                //        CreatePOGrid.refresh();
                //        var AdditionalChargesGrid = $('#AdditionalChargesGrid').dxDataGrid('instance');
                //        AdditionalChargesGrid.refresh();
                //    }

                //},
                columns: [
                    { dataField: "LedgerName", visible: true, caption: "Tax / Addtional Charges Ledger", allowEditing: false },
                    { dataField: "TaxRatePer", visible: true, caption: "%", width: 50 },
                    {
                        dataField: "CalculateON", visible: true, caption: "Calcu. ON", width: 80,
                        lookup: {
                            dataSource: CalculateOnLookup,
                            displayExpr: "Name",
                            valueExpr: "Name"
                        }
                    },
                    { dataField: "GSTApplicable", visible: true, caption: "GST Applicable", width: 100, dataType: "boolean" },
                    { dataField: "InAmount", visible: true, caption: "In Amount", width: 80, dataType: "boolean" },
                    { dataField: "ChargesAmount", visible: true, caption: "Amount", width: 80 },
                    { dataField: "IsCumulative", visible: false, caption: "Is Cumulative", width: 80 },
                    { dataField: "TaxType", visible: true, caption: "Tax Type", width: .1 },
                    { dataField: "GSTLedgerType", visible: false, caption: "GST Ledger Type", width: 80 },
                    { dataField: "LedgerID", visible: false, caption: "LedgerID", width: 80 },
                    {
                        type: "buttons", fixed: true, alignment: "right",
                        buttons: [
                            "delete" // Default delete button with icons
                        ],
                        width: 30,
                    },

                ]
            });
            if (GblCompanyConfiguration.length > 0) {
                if (GblCompanyConfiguration[0].IsGstApplicable === true) {
                    $("#AdditionalChargesGrid").dxDataGrid('columnOption', 'GSTApplicable', 'caption', "GST Applicable");
                } else if (GblCompanyConfiguration[0].IsVatApplicable === true) {
                    $("#AdditionalChargesGrid").dxDataGrid('columnOption', 'GSTApplicable', 'caption', "VAT Applicable");
                }
            }
        }
    });
}

//Terms Of Payment
//fillPayTermsGrid();
function fillPayTermsGrid() {
    var string = PaymentTermsString;
    var TermsID = 0;
    if (string !== "" && string !== null) {
        PaymentTermsGrid = [];
        string = string.split(",");
        for (var str in string) {
            optTerms = {};
            TermsID = TermsID + 1;
            optTerms.TermsID = TermsID;
            optTerms.Terms = string[str];

            PaymentTermsGrid.push(optTerms);
        }
        $("#PaymentTermsGrid").dxDataGrid({
            dataSource: PaymentTermsGrid
        });
    }
}

//add by ankit 25-05 - 2025
$("#PaymentTermsGrid").dxDataGrid({
    dataSource: PaymentTermsGrid,
    columnAutoWidth: true,
    showBorders: true,
    showRowLines: true,
    allowColumnReordering: true,
    allowColumnResizing: true,
    sorting: {
        mode: "none" // or "multiple" | "single"
    },
    //filterRow: { visible: true, applyFilter: "auto" },
    //  selection: { mode: "multiple", showCheckBoxesMode: "always" },
    loadPanel: {
        enabled: true,
        height: 90,
        width: 200,
        text: 'Data is loading...'
    },
    editing: {
        mode: "cell",
        allowDeleting: true,
        //allowAdding: true,
        allowUpdating: false,
        useIcons: true,
    },
    onRowPrepared: function (e) {
        if (e.rowType === "header") {
            e.rowElement.css('background', '#509EBC');
            e.rowElement.css('color', 'white');
            e.rowElement.css('font-weight', 'bold');
        }
        e.rowElement.css('fontSize', '11px');
    },
    columns: [{ dataField: "TermsID", visible: false, caption: "TermsID" },
    { dataField: "Terms", visible: true, caption: "Terms" },
    {
        type: "buttons", fixed: true, alignment: "right",
        buttons: [
            "delete" // Default delete button with icons
        ],
        width: 30,
    }
    ]
});

//OtherHeads
HeadFun();
function HeadFun() {
    $.ajax({
        type: "POST",
        url: "WebService_PurchaseOrder.asmx/HeadFun",
        data: '{}',
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            ////console.debug(results);
            var res = results.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/""/g, '');
            res = res.replace(/u0026/g, '&');
            res = res.replace(/u0027/g, "'");
            res = res.replace(/:,/g, ":null,");
            res = res.replace(/,}/g, ",null}");
            res = res.substr(1);
            res = res.slice(0, -1);
            OtherHead = JSON.parse(res);

            fillOtherHeadsGrid(OtherHead);
        }
    });
}

$("#BtnOtherHeadPop").click(function () {
    document.getElementById("BtnOtherHeadPop").setAttribute("data-toggle", "modal");
    document.getElementById("BtnOtherHeadPop").setAttribute("data-target", "#largeModalHeads");
});

function fillOtherHeadsGrid(OtherHead) {
    $("#OtherHeadsGrid").dxDataGrid({
        dataSource: OtherHead,
        columnAutoWidth: true,
        showBorders: true,
        showRowLines: true,
        allowColumnReordering: true,
        allowColumnResizing: true,
        filterRow: { visible: true, applyFilter: "auto" },
        height: function () {
            return window.innerHeight / 1.3;
        },
        sorting: {
            mode: "none" // or "multiple" | "single"
        },
        loadPanel: {
            enabled: true,
            height: 90,
            width: 200,
            text: 'Data is loading...'
        },
        editing: {
            mode: "cell",
            allowUpdating: true
        },
        onEditingStart: function (e) {
            if (e.column.visibleIndex === 0 || e.column.visibleIndex === 1 || e.column.visibleIndex === 3 || e.column.visibleIndex === 6) {
                e.cancel = true;
            }
        },
        onCellPrepared: function (cellPree) {
            var cellPreedataGrid = $("#OtherHeadsGrid").dxDataGrid('instance');

            if (cellPree.rowType === undefined || cellPree.rowType !== "data") return false;
            if (cellPree.columnIndex === 4 || cellPree.columnIndex === 5) {
                var RateType = "";
                var Weight = 0;
                var Rate = 0;
                var HeadAmount = 0;

                var CellRow = cellPree.row.rowIndex;

                RateType = cellPreedataGrid._options.dataSource[CellRow].RateType;
                Weight = cellPreedataGrid._options.dataSource[CellRow].Weight;
                Rate = cellPreedataGrid._options.dataSource[CellRow].Rate;

                if (RateType === "Rate/Kg") {
                    HeadAmount = Number(Weight) * Number(Rate);
                } else if (RateType === "Rate/Ton") {
                    HeadAmount = Number(Number(Weight) / 1000) * Number(Rate);
                } else if (RateType === "Amount") {
                    HeadAmount = Number(Rate);
                }

                cellPreedataGrid._options.dataSource[CellRow].Weight = parseFloat(Number(Weight)).toFixed(2);
                cellPreedataGrid._options.dataSource[CellRow].Rate = parseFloat(Number(Rate)).toFixed(2);
                cellPreedataGrid._options.dataSource[CellRow].HeadAmount = parseFloat(Number(HeadAmount)).toFixed(2);
            }
        },
        onRowPrepared: function (e) {
            if (e.rowType === "header") {
                e.rowElement.css('background', '#509EBC');
                e.rowElement.css('color', 'white');
                e.rowElement.css('font-weight', 'bold');
            }
            e.rowElement.css('fontSize', '11px');
        },
        columns: [{ dataField: "HeadID", visible: false, caption: "HeadID" },
        { dataField: "RateType", visible: false, caption: "RateType" },
        { dataField: "Sel", visible: true, caption: "", dataType: "boolean", width: 30 },
        { dataField: "Head", visible: true, caption: "Head", width: 450 },
        { dataField: "Weight", visible: true, caption: "Weight", width: 30 },
        { dataField: "Rate", visible: true, caption: "Rate", width: 30 },
        { dataField: "HeadAmount", visible: true, caption: "Amount", width: 30 }]
    });
}

$("#BtnSave").click(async function () {
    let ContinueSaveFlow = false;

    var $fileInput = $("#MultipleFileAdds").dxDataGrid("instance");
    var data = $fileInput.option("dataSource");

    var grid = $("#CreatePOGrid").dxDataGrid("instance");
    await grid.saveEditData();

    var rows = grid.getDataSource().items();
    ContinueSaveFlow = true;

    // JSON object banane ka part
    var FilejsonObjectsTransactionMain = [];
    for (var i = 0; i < data.length; i++) {
        var FIleTransactionMainRecord = {};
        FIleTransactionMainRecord.AttachmentFilesName = data[i].AttachedFileName;
        FIleTransactionMainRecord.AttachedFileRemark = data[i].AttachedFileRemark;
        FilejsonObjectsTransactionMain.push(FIleTransactionMainRecord);
    }
    FilejsonObjectsTransactionMain = JSON.stringify(FilejsonObjectsTransactionMain);

    var AttachmentFileNamestr = "";
    var formData = new FormData();

    for (var i = 0; i < data.length; i++) {
        var fileUrl = data[i].AttachedFileUrl;
        var fileName = data[i].AttachedFileName;
        if (AttachmentFileNamestr === "") {
            AttachmentFileNamestr = fileName;
        } else {
            AttachmentFileNamestr += "," + fileName;
        }
        if (fileUrl && fileUrl.includes("base64")) {
            var byteString = atob(fileUrl.split(",")[1]); // base64 decod
            var ab = new ArrayBuffer(byteString.length);
            var ia = new Uint8Array(ab);

            for (var j = 0; j < byteString.length; j++) {
                ia[j] = byteString.charCodeAt(j);
            }
            var mimeType = fileUrl.split(",")[0].split(":")[1].split(";")[0];
            var blob = new Blob([ab], { type: mimeType });

            formData.append("UserAttchedFiles[]", blob, fileName);
        }
    }
    formData.append("FileJsonData", FilejsonObjectsTransactionMain);
    formData.append("AttachmentFileNames", AttachmentFileNamestr);

    $.ajax({
        url: "POAttchmentFile.ashx",
        type: "POST",
        data: formData,
        processData: false,
        contentType: false,
        success: function (res) {
            console.log("Uploaded: " + res);
        },
        error: function (err) {
            console.error("Upload error:", err);
        }
    });

    var CreatePOGrid = $("#CreatePOGrid").dxDataGrid('instance');
    var CreatePOGridRow = CreatePOGrid._options.dataSource.length;

    var VoucherDate = $('#VoucherDate').dxDateBox('instance').option('value');
    var grid = $("#POGridPending").dxDataGrid("instance");
    var selectedData = grid.getSelectedRowsData();
    var PODate = new Date(VoucherDate);
    for (var i = 0; i < selectedData.length; i++) {
        var RequisitionDate = new Date(selectedData[i]["VoucherDate"]);
        if (PODate < RequisitionDate) {
            showDevExpressNotification("PO Date should be later than the Requisition Date!.....", "error");
            $('#VoucherDate').dxDateBox('instance').option('value', new Date());
            return;
        }
    }
    if (GblStatus === "Update") {
        var selectedRow = $("#POGridProcess").dxDataGrid("instance").getSelectedRowsData()[0];
        if (isCurrentFinancialYear(selectedRow.FYear) == false) {
            swal("Warning", "Selected Purchase Order is not allowed to Upadate  in the logged-in financial year.", "warning");
            return false;
        }
    } else {
        if (isCurrentFinancialYear("") == false) {
            swal("Warning", "Selected Purchase Order is not allowed to Create  in the logged-in financial year.", "warning");
            return false;
        }
    }
    var gridData = CreatePOGrid._options.dataSource;
    PODate.setHours(0, 0, 0, 0);
    for (var i = 0; i < gridData.length; i++) {
        var expectedDeliveryDate = new Date(gridData[i].ExpectedDeliveryDate);
        expectedDeliveryDate.setHours(0, 0, 0, 0);
        if (expectedDeliveryDate < PODate) {
            gridData[i].ExpectedDeliveryDate = PODate;
            CreatePOGrid.refresh();
            showDevExpressNotification("Expected Delivery Date must not be greater than PO Date.....!", "error");
            return;
        }
    }

    var SupplierName = $('#SupplierName').dxSelectBox('instance').option('value');
    var SelPOApprovalBy = $('#SelPOApprovalBy').dxSelectBox('instance').option('value');

    //if (GblStatus === "Update") {
    //    if (VarItemApproved === "true" || VarItemApproved === true) {
    //        DevExpress.ui.notify("This purchase order has been used in further transactions, Can't edit !", "error", 1000);
    //        return false;
    //    }
    //}

    if (SupplierName === "" || SupplierName === undefined || SupplierName === null) {
        //swal("Error!", "Please Choose Supplier Name..", "");
        //DevExpress.ui.notify("Please Choose Supplier Name..!", "error", 1000);
        DevExpress.ui.notify({
            message: "Please Choose Supplier Name..!", type: "error", displayTime: 5000, width: "900px",
            onContentReady: function (e) {
                e.component.$content().find(".dx-toast-message").css({
                    "font-size": "13px",
                    "font-weight": "bold",
                });
                const closeButton = $("<div>")
                    .addClass("dx-notification-close")
                    .text("×")
                    .css({
                        "position": "absolute",
                        "top": "5px",
                        "right": "5px",
                        "cursor": "pointer",
                        "font-size": "25px",
                    })
                    .appendTo(e.component.$content());
                closeButton.on("click", function () {
                    e.component.hide();
                });
            }
        });
        document.getElementById("ValStrSupplierName").style.fontSize = "10px";
        document.getElementById("ValStrSupplierName").style.display = "block";
        document.getElementById("ValStrSupplierName").innerHTML = 'This field should not be empty..Supplier Name';
        return false;
    }
    else {
        document.getElementById("ValStrSupplierName").style.display = "none";
    }

    var ContactPersonName = $('#ContactPersonName').dxSelectBox('instance').option('value');

    var PurchaseDivision = $('#PurchaseDivision').dxSelectBox('instance').option('value');

    var LblState = document.getElementById("LblState").innerHTML.replace(/State : /g, '');
    var LblCountry = document.getElementById("LblCountry").innerHTML.replace(/Country : /g, '');

    var PORefernce = document.getElementById("PORefernce").value.trim();

    var DealerName = $('#DealerName').dxSelectBox('instance').option('value');
    var ModeOfTransport = $('#ModeOfTransport').dxSelectBox('instance').option('value');

    var textDeliverAt = document.getElementById("textDeliverAt").value.trim();
    //var textNaretion = document.getElementById("textNaretion").value.trim();

    var TxtTaxAmt = document.getElementById("TxtTaxAmt").value;
    var TxtNetAmt = document.getElementById("TxtNetAmt").value;
    var TxtBasicAmt = document.getElementById("TxtBasicAmt").value;
    var TxtCGSTAmt = document.getElementById("TxtCGSTAmt").value;
    var TxtSGSTAmt = document.getElementById("TxtSGSTAmt").value;
    var TxtIGSTAmt = document.getElementById("TxtIGSTAmt").value;
    var TxtAfterDisAmt = document.getElementById("TxtAfterDisAmt").value;
    var Txt_TaxAbleSum = document.getElementById("Txt_TaxAbleSum").value;
    var TxtTotalQty = document.getElementById("TxtTotalQty").value;

    //if (ContactPersonName === "" || ContactPersonName === undefined || ContactPersonName === null) {
    //    //swal("Error!", "Please Choose Cont.Person..", "");
    //    DevExpress.ui.notify("Please Choose Cont.Person..!", "error", 1000);
    //    document.getElementById("ValStrContactPersonName").style.fontSize = "10px";
    //    document.getElementById("ValStrContactPersonName").style.display = "block";
    //    document.getElementById("ValStrContactPersonName").innerHTML = 'This field should not be empty..Cont.Person';
    //    return false;
    //}
    //else {
    //    document.getElementById("ValStrContactPersonName").style.display = "none";
    //}

    //if (PurchaseDivision === "" || PurchaseDivision === undefined || PurchaseDivision === null) {
    //    //swal("Error!", "Please Choose Supplier Name..", "");
    //    DevExpress.ui.notify("Please Choose Purchase Division..!", "error", 1000);
    //    document.getElementById("ValStrPurchaseDivision").style.fontSize = "10px";
    //    document.getElementById("ValStrPurchaseDivision").style.display = "block";
    //    document.getElementById("ValStrPurchaseDivision").innerHTML = 'This field should not be empty..Purchase Division';
    //    return false;
    //}
    //else {
    //    document.getElementById("ValStrPurchaseDivision").style.display = "none";
    //}

    if (CreatePOGridRow < 1) {
        //swal("Error!", "Please add Item in given below Grid..", "");
        //DevExpress.ui.notify("Please add Item in given below Grid..!", "error", 1000);
        DevExpress.ui.notify({
            message: "Please add Item in given below Grid..!", type: "error", displayTime: 5000, width: "900px",
            onContentReady: function (e) {
                e.component.$content().find(".dx-toast-message").css({
                    "font-size": "13px",
                    "font-weight": "bold",
                });
                const closeButton = $("<div>")
                    .addClass("dx-notification-close")
                    .text("×")
                    .css({
                        "position": "absolute",
                        "top": "5px",
                        "right": "5px",
                        "cursor": "pointer",
                        "font-size": "25px",
                    })
                    .appendTo(e.component.$content());
                closeButton.on("click", function () {
                    e.component.hide();
                });
            }
        });
        return false;
    }

    for (var x = 0; x < CreatePOGridRow; x++) {
        if (Number(CreatePOGrid._options.dataSource[x].ItemID) <= 0) {
            //DevExpress.ui.notify("Please select valid item from requisition or master list..!", "error", 1000);
            DevExpress.ui.notify({
                message: "Please select valid item from requisition or master list..!", type: "error", displayTime: 5000, width: "900px",
                onContentReady: function (e) {
                    e.component.$content().find(".dx-toast-message").css({
                        "font-size": "13px",
                        "font-weight": "bold",
                    });
                    const closeButton = $("<div>")
                        .addClass("dx-notification-close")
                        .text("×")
                        .css({
                            "position": "absolute",
                            "top": "5px",
                            "right": "5px",
                            "cursor": "pointer",
                            "font-size": "25px",
                        })
                        .appendTo(e.component.$content());
                    closeButton.on("click", function () {
                        e.component.hide();
                    });
                }
            });
            return false;
        } else if (Number(CreatePOGrid._options.dataSource[x].PurchaseQuantity) <= 0) {
            //DevExpress.ui.notify("Please enter valid purchase order quantity..!", "error", 1000);
            DevExpress.ui.notify({
                message: "Please enter valid purchase order quantity..!", type: "error", displayTime: 5000, width: "900px",
                onContentReady: function (e) {
                    e.component.$content().find(".dx-toast-message").css({
                        "font-size": "13px",
                        "font-weight": "bold",
                    });
                    const closeButton = $("<div>")
                        .addClass("dx-notification-close")
                        .text("×")
                        .css({
                            "position": "absolute",
                            "top": "5px",
                            "right": "5px",
                            "cursor": "pointer",
                            "font-size": "25px",
                        })
                        .appendTo(e.component.$content());
                    closeButton.on("click", function () {
                        e.component.hide();
                    });
                }
            });
            return false;
        } else if (Number(CreatePOGrid._options.dataSource[x].PurchaseRate) <= 0) {
            //DevExpress.ui.notify("Please enter valid purchase order rate..!", "error", 1000);
            DevExpress.ui.notify({
                message: "Please enter valid purchase order rate..!", type: "error", displayTime: 5000, width: "900px",
                onContentReady: function (e) {
                    e.component.$content().find(".dx-toast-message").css({
                        "font-size": "13px",
                        "font-weight": "bold",
                    });
                    const closeButton = $("<div>")
                        .addClass("dx-notification-close")
                        .text("×")
                        .css({
                            "position": "absolute",
                            "top": "5px",
                            "right": "5px",
                            "cursor": "pointer",
                            "font-size": "25px",
                        })
                        .appendTo(e.component.$content());
                    closeButton.on("click", function () {
                        e.component.hide();
                    });
                }
            });
            return false;
        } else if (Number(CreatePOGrid._options.dataSource[x].TaxableAmount) <= 0) {
            //DevExpress.ui.notify("Please enter valid purchase order quantity/rate..!", "error", 1000);
            DevExpress.ui.notify({
                message: "Please enter valid purchase order quantity/rate..!", type: "error", displayTime: 5000, width: "900px",
                onContentReady: function (e) {
                    e.component.$content().find(".dx-toast-message").css({
                        "font-size": "13px",
                        "font-weight": "bold",
                    });
                    const closeButton = $("<div>")
                        .addClass("dx-notification-close")
                        .text("×")
                        .css({
                            "position": "absolute",
                            "top": "5px",
                            "right": "5px",
                            "cursor": "pointer",
                            "font-size": "25px",
                        })
                        .appendTo(e.component.$content());
                    closeButton.on("click", function () {
                        e.component.hide();
                    });
                }
            });
            return false;
        } else if (CreatePOGrid._options.dataSource[x].ExpectedDeliveryDate === "" || CreatePOGrid._options.dataSource[x].ExpectedDeliveryDate === null) {
            //DevExpress.ui.notify("Please select expected delivery date..!", "error", 1000);
            DevExpress.ui.notify({
                message: "Please select expected delivery date..!", type: "error", displayTime: 5000, width: "900px",
                onContentReady: function (e) {
                    e.component.$content().find(".dx-toast-message").css({
                        "font-size": "13px",
                        "font-weight": "bold",
                    });
                    const closeButton = $("<div>")
                        .addClass("dx-notification-close")
                        .text("×")
                        .css({
                            "position": "absolute",
                            "top": "5px",
                            "right": "5px",
                            "cursor": "pointer",
                            "font-size": "25px",
                        })
                        .appendTo(e.component.$content());
                    closeButton.on("click", function () {
                        e.component.hide();
                    });
                }
            });
            return false;
        }

        //let poDate = $("#VoucherDate").dxDateBox("instance").option("value");
        //if (poDate) {
        //    let dateBoxDate = new Date(poDate);
        //    dateBoxDate.setHours(0, 0, 0, 0);

        //    let gridDateValue = CreatePOGrid._options.dataSource[x].ExpectedDeliveryDate;

        //    // Convert the grid date value to a Date object
        //    let gridDate = new Date(gridDateValue);
        //    gridDate.setHours(0, 0, 0, 0);

        //    // Check if the grid date is less than the DateBox date
        //    if (gridDate < dateBoxDate) {
        //        //DevExpress.ui.notify("The selected date cannot be less than expected delivery date.", "warning", 1500);
        //        DevExpress.ui.notify({
        //            message: "The selected date cannot be less than expected delivery date.", type: "warning", displayTime: 5000, width: "900px",
        //            onContentReady: function (e) {
        //                e.component.$content().find(".dx-toast-message").css({
        //                    "font-size": "13px",
        //                    "font-weight": "bold",
        //                });
        //                const closeButton = $("<div>")
        //                    .addClass("dx-notification-close")
        //                    .text("×")
        //                    .css({
        //                        "position": "absolute",
        //                        "top": "5px",
        //                        "right": "5px",
        //                        "cursor": "pointer",
        //                        "font-size": "25px",
        //                    })
        //                    .appendTo(e.component.$content());
        //                closeButton.on("click", function () {
        //                    e.component.hide();
        //                });
        //            }
        //        });
        //        // Optionally reset the DateBox value to clear the invalid input
        //        //$("#VoucherDate").dxDateBox({
        //        //    value: new Date(gridDateValue).toISOString().substr(0, 10),
        //        //});
        //        return;
        //    }
        //}

    }

    if (GblCompanyConfiguration[0].IsGstApplicable === true) {
        if (GblGSTApplicable === true) {

            if (ChargesGrid.length <= 0) {
                //swal("Error!", "Please add Item in given below Grid..", "");
                //DevExpress.ui.notify("Please add tax ledger..!", "error", 1000);
                DevExpress.ui.notify({
                    message: "Please add tax ledger..!", type: "error", displayTime: 5000, width: "900px",
                    onContentReady: function (e) {
                        e.component.$content().find(".dx-toast-message").css({
                            "font-size": "13px",
                            "font-weight": "bold",
                        });
                        const closeButton = $("<div>")
                            .addClass("dx-notification-close")
                            .text("×")
                            .css({
                                "position": "absolute",
                                "top": "5px",
                                "right": "5px",
                                "cursor": "pointer",
                                "font-size": "25px",
                            })
                            .appendTo(e.component.$content());
                        closeButton.on("click", function () {
                            e.component.hide();
                        });
                    }
                });
                return false;
            }
            var results = $.grep(ChargesGrid, function (e) { return e.TaxType === "GST"; });
            if (results.length === 0) {
                //DevExpress.ui.notify("Please add GST tax ledger..!", "error", 1000);
                DevExpress.ui.notify({
                    message: "Please add GST tax ledger..!", type: "error", displayTime: 5000, width: "900px",
                    onContentReady: function (e) {
                        e.component.$content().find(".dx-toast-message").css({
                            "font-size": "13px",
                            "font-weight": "bold",
                        });
                        const closeButton = $("<div>")
                            .addClass("dx-notification-close")
                            .text("×")
                            .css({
                                "position": "absolute",
                                "top": "5px",
                                "right": "5px",
                                "cursor": "pointer",
                                "font-size": "25px",
                            })
                            .appendTo(e.component.$content());
                        closeButton.on("click", function () {
                            e.component.hide();
                        });
                    }
                });
                return false;
            }
            var SupplierStateTin = document.getElementById("LblSupplierStateTin").innerHTML;
            results = 0;
            if (Number(SupplierStateTin) === Number(GblCompanyStateTin)) {
                //var GridTaxType = ChargesGrid[c].TaxType;
                //var TaxRatePer = ChargesGrid[c].TaxRatePer;
                //var GSTLedgerType = ChargesGrid[c].GSTLedgerType;
                results = $.grep(ChargesGrid, function (e) { return e.GSTLedgerType === "Integrated Tax"; });
                if (results.length > 0) {
                    //DevExpress.ui.notify("You can't add integrated GST tax ledger..!", "error", 1000);
                    DevExpress.ui.notify({
                        message: "You can't add integrated GST tax ledger..!", type: "error", displayTime: 5000, width: "900px",
                        onContentReady: function (e) {
                            e.component.$content().find(".dx-toast-message").css({
                                "font-size": "13px",
                                "font-weight": "bold",
                            });
                            const closeButton = $("<div>")
                                .addClass("dx-notification-close")
                                .text("×")
                                .css({
                                    "position": "absolute",
                                    "top": "5px",
                                    "right": "5px",
                                    "cursor": "pointer",
                                    "font-size": "25px",
                                })
                                .appendTo(e.component.$content());
                            closeButton.on("click", function () {
                                e.component.hide();
                            });
                        }
                    });
                    return false;
                }
                results = $.grep(ChargesGrid, function (e) { return e.GSTLedgerType === "Central Tax"; });
                if (results.length === 0) {
                    //DevExpress.ui.notify("Please add central GST tax ledger..!", "error", 1000);
                    DevExpress.ui.notify({
                        message: "Please add central GST tax ledger..!", type: "error", displayTime: 5000, width: "900px",
                        onContentReady: function (e) {
                            e.component.$content().find(".dx-toast-message").css({
                                "font-size": "13px",
                                "font-weight": "bold",
                            });
                            const closeButton = $("<div>")
                                .addClass("dx-notification-close")
                                .text("×")
                                .css({
                                    "position": "absolute",
                                    "top": "5px",
                                    "right": "5px",
                                    "cursor": "pointer",
                                    "font-size": "25px",
                                })
                                .appendTo(e.component.$content());
                            closeButton.on("click", function () {
                                e.component.hide();
                            });
                        }
                    });
                    return false;
                } else {
                    var results1 = $.grep(ChargesGrid, function (e) { return e.GSTLedgerType === "State Tax"; });
                    if (results1.length === 0) {
                        //DevExpress.ui.notify("Please add state GST tax ledger..!", "error", 1000);
                        DevExpress.ui.notify({
                            message: "Please add state GST tax ledger..!", type: "error", displayTime: 5000, width: "900px",
                            onContentReady: function (e) {
                                e.component.$content().find(".dx-toast-message").css({
                                    "font-size": "13px",
                                    "font-weight": "bold",
                                });
                                const closeButton = $("<div>")
                                    .addClass("dx-notification-close")
                                    .text("×")
                                    .css({
                                        "position": "absolute",
                                        "top": "5px",
                                        "right": "5px",
                                        "cursor": "pointer",
                                        "font-size": "25px",
                                    })
                                    .appendTo(e.component.$content());
                                closeButton.on("click", function () {
                                    e.component.hide();
                                });
                            }
                        });
                        return false;
                    }
                }
            } else {
                results = $.grep(ChargesGrid, function (e) { return e.GSTLedgerType === "Central Tax"; });
                if (results.length > 0) {
                    //DevExpress.ui.notify("You can't add central GST tax ledger..!", "error", 1000);
                    DevExpress.ui.notify({
                        message: "You can't add central GST tax ledger..!", type: "error", displayTime: 5000, width: "900px",
                        onContentReady: function (e) {
                            e.component.$content().find(".dx-toast-message").css({
                                "font-size": "13px",
                                "font-weight": "bold",
                            });
                            const closeButton = $("<div>")
                                .addClass("dx-notification-close")
                                .text("×")
                                .css({
                                    "position": "absolute",
                                    "top": "5px",
                                    "right": "5px",
                                    "cursor": "pointer",
                                    "font-size": "25px",
                                })
                                .appendTo(e.component.$content());
                            closeButton.on("click", function () {
                                e.component.hide();
                            });
                        }
                    });
                    return false;
                }
                results = $.grep(ChargesGrid, function (e) { return e.GSTLedgerType === "State Tax"; });
                if (results.length > 0) {
                    //DevExpress.ui.notify("You can't add state GST tax ledger..!", "error", 1000);
                    DevExpress.ui.notify({
                        message: "You can't add state GST tax ledger..!", type: "error", displayTime: 5000, width: "900px",
                        onContentReady: function (e) {
                            e.component.$content().find(".dx-toast-message").css({
                                "font-size": "13px",
                                "font-weight": "bold",
                            });
                            const closeButton = $("<div>")
                                .addClass("dx-notification-close")
                                .text("×")
                                .css({
                                    "position": "absolute",
                                    "top": "5px",
                                    "right": "5px",
                                    "cursor": "pointer",
                                    "font-size": "25px",
                                })
                                .appendTo(e.component.$content());
                            closeButton.on("click", function () {
                                e.component.hide();
                            });
                        }
                    });
                    return false;
                }
                results = $.grep(ChargesGrid, function (e) { return e.GSTLedgerType === "Integrated Tax"; });
                if (results.length === 0) {
                    //DevExpress.ui.notify("Please add integrated GST tax ledger..!", "error", 1000);
                    DevExpress.ui.notify({
                        message: "Please add integrated GST tax ledger..!", type: "error", displayTime: 5000, width: "900px",
                        onContentReady: function (e) {
                            e.component.$content().find(".dx-toast-message").css({
                                "font-size": "13px",
                                "font-weight": "bold",
                            });
                            const closeButton = $("<div>")
                                .addClass("dx-notification-close")
                                .text("×")
                                .css({
                                    "position": "absolute",
                                    "top": "5px",
                                    "right": "5px",
                                    "cursor": "pointer",
                                    "font-size": "25px",
                                })
                                .appendTo(e.component.$content());
                            closeButton.on("click", function () {
                                e.component.hide();
                            });
                        }
                    });
                    return false;
                }
            }
        }
    } else if (GblCompanyConfiguration[0].IsVatApplicable === true) {
        if (GblGSTApplicable === true) {

            if (ChargesGrid.length <= 0) {
                //swal("Error!", "Please add Item in given below Grid..", "");
                //DevExpress.ui.notify("Please add tax ledger..!", "error", 1000);
                DevExpress.ui.notify({
                    message: "Please add tax ledger..!", type: "error", displayTime: 5000, width: "900px",
                    onContentReady: function (e) {
                        e.component.$content().find(".dx-toast-message").css({
                            "font-size": "13px",
                            "font-weight": "bold",
                        });
                        const closeButton = $("<div>")
                            .addClass("dx-notification-close")
                            .text("×")
                            .css({
                                "position": "absolute",
                                "top": "5px",
                                "right": "5px",
                                "cursor": "pointer",
                                "font-size": "25px",
                            })
                            .appendTo(e.component.$content());
                        closeButton.on("click", function () {
                            e.component.hide();
                        });
                    }
                });
                return false;
            }
            var results = $.grep(ChargesGrid, function (e) { return e.TaxType === GblCompanyConfiguration[0].DefaultTaxLedgerTypeName.toUpperCase(); });
            if (results.length === 0) {
                //DevExpress.ui.notify("Please add " + GblCompanyConfiguration[0].DefaultTaxLedgerTypeName + " tax ledger..!", "error", 1000);
                DevExpress.ui.notify({
                    message: "Please add " + GblCompanyConfiguration[0].DefaultTaxLedgerTypeName + " tax ledger..!", type: "error", displayTime: 5000, width: "1000px",
                    onContentReady: function (e) {
                        e.component.$content().find(".dx-toast-message").css({
                            "font-size": "13px",
                            "font-weight": "bold",
                        });
                        const closeButton = $("<div>")
                            .addClass("dx-notification-close")
                            .text("×")
                            .css({
                                "position": "absolute",
                                "top": "5px",
                                "right": "5px",
                                "cursor": "pointer",
                                "font-size": "25px",
                            })
                            .appendTo(e.component.$content());
                        closeButton.on("click", function () {
                            e.component.hide();
                        });
                    }
                });
                return false;
            }
        }
    }

    //if (DealerName === "" || DealerName === undefined || DealerName === null) {
    //    //swal("Error!", "Please Choose Dealer Name..", "");
    //    DevExpress.ui.notify("Please Choose Dealer Name..!", "error", 1000);
    //    document.getElementById("ValStrDealerName").style.fontSize = "10px";
    //    document.getElementById("ValStrDealerName").style.display = "block";
    //    document.getElementById("ValStrDealerName").innerHTML = 'This field should not be empty..Dealer Name';
    //    return false;
    //}
    //else {
    //    document.getElementById("ValStrDealerName").style.display = "none";
    //}

    //if(document.getElementById("PORefernce").value==""||document.getElementById("PORefernce").value==undefined||document.getElementById("PORefernce").value==null){
    //    swal("Error!", "Please Enter PO Refernce..", "");
    //DevExpress.ui.notify("Please Enter PO Refernce..!", "error", 1000);
    //    document.getElementById("ValStrPORefernce").style.fontSize = "10px";
    //    document.getElementById("ValStrPORefernce").style.display = "block";
    //    document.getElementById("ValStrPORefernce").innerHTML = 'This field should not be empty..PO Refernce';
    //    return false;
    //}
    //else {
    //    document.getElementById("ValStrPORefernce").style.display = "none";
    //}

    if (ModeOfTransport === "" || ModeOfTransport === undefined || ModeOfTransport === null) {
        // swal("Error!", "Please Choose Mode Of Transport..", "");
        //DevExpress.ui.notify("Please Choose Mode Of Transport..!", "error", 1000);
        DevExpress.ui.notify({
            message: "Please Choose Mode Of Transport..!", type: "error", displayTime: 5000, width: "900px",
            onContentReady: function (e) {
                e.component.$content().find(".dx-toast-message").css({
                    "font-size": "13px",
                    "font-weight": "bold",
                });
                const closeButton = $("<div>")
                    .addClass("dx-notification-close")
                    .text("×")
                    .css({
                        "position": "absolute",
                        "top": "5px",
                        "right": "5px",
                        "cursor": "pointer",
                        "font-size": "25px",
                    })
                    .appendTo(e.component.$content());
                closeButton.on("click", function () {
                    e.component.hide();
                });
            }
        });
        document.getElementById("ValStrModeOfTransport").style.fontSize = "10px";
        document.getElementById("ValStrModeOfTransport").style.display = "block";
        document.getElementById("ValStrModeOfTransport").innerHTML = 'This field should not be empty..Mode Of Transport';
        return false;
    }
    else {
        document.getElementById("ValStrModeOfTransport").style.display = "none";
    }

    if (GblStatus === "Update" && validateUserData.isUserInfoFilled === false) {
        if (Number(SelectedProductionUnitID) != 0) {
            if (GBLProductionUnitID != SelectedProductionUnitID) {
                swal("Attention!", "Selected transaction is related to unit " + SelectedProductionUnitName + ", Kindly login with unit " + SelectedProductionUnitName + " to process.", "warning");
                $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                return;
            }
        }
        validateUserData.userName = ""; validateUserData.password = ""; validateUserData.RecordID = document.getElementById("TxtPOID").value; validateUserData.actionType = "Update"; validateUserData.transactionRemark = ""; validateUserData.isUserInfoFilled = false; validateUserData.documentNo = document.getElementById("LblPONo").value;
        let result = await openSecurityPanelModal(validateUserData);
    }

    var TotalHeadAmt = 0;
    var termsofpayment = "";
    var PaymentTermsGrid = $("#PaymentTermsGrid").dxDataGrid('instance');
    var PaymentTermsGridRow = PaymentTermsGrid.totalCount();
    var CurrencyCode = $('#SelCurrencyCode').dxSelectBox('instance').option('value');
    if (CurrencyCode === null || CurrencyCode === "" || CurrencyCode === undefined) {
        CurrencyCode = (GblCompanyConfiguration !== undefined && GblCompanyConfiguration !== null && GblCompanyConfiguration.length > 0) ? GblCompanyConfiguration[0].CurrencyCode : "INR";
    }
    if (PaymentTermsGridRow > 0) {
        for (var tp = 0; tp < PaymentTermsGridRow; tp++) {
            if (termsofpayment === "") {
                termsofpayment = PaymentTermsGrid._options.dataSource[tp].Terms;
            } else {
                termsofpayment = termsofpayment + "," + PaymentTermsGrid._options.dataSource[tp].Terms;
            }
        }
    }

    var jsonObjectsRecordRequisition = [];
    var OperationRecordRequisition = {};

    if (SubGridData.length > 0) {
        for (var sb = 0; sb < SubGridData.length; sb++) {
            if (Number(SubGridData[sb].ItemID) > 0) {
                OperationRecordRequisition = {};
                OperationRecordRequisition.TransID = sb + 1;
                OperationRecordRequisition.ItemID = SubGridData[sb].ItemID;
                OperationRecordRequisition.RequisitionProcessQuantity = Number(SubGridData[sb].RequiredQuantity).toFixed(3);
                OperationRecordRequisition.StockUnit = SubGridData[sb].StockUnit;
                OperationRecordRequisition.RequisitionTransactionID = SubGridData[sb].TransactionID;

                jsonObjectsRecordRequisition.push(OperationRecordRequisition);
            }

        }
    }

    var OtherHeadsGrid = $("#OtherHeadsGrid").dxDataGrid('instance');
    var OtherHeadsGridRow = OtherHeadsGrid.totalCount();

    var jsonObjectsRecordOverHead = [];
    var OperationRecordOverHead = {};
    if (OtherHeadsGridRow > 0) {
        for (var h = 0; h < OtherHeadsGridRow; h++) {
            if (OtherHeadsGrid._options.dataSource[h].Sel === true || OtherHeadsGrid._options.dataSource[h].Sel === "on" || OtherHeadsGrid._options.dataSource[h].Sel === "true") {
                OperationRecordOverHead = {};

                OperationRecordOverHead.headID = OtherHeadsGrid._options.dataSource[h].HeadID;
                OperationRecordOverHead.TransID = h + 1;
                OperationRecordOverHead.headName = OtherHeadsGrid._options.dataSource[h].Head;
                OperationRecordOverHead.Quantity = Number(OtherHeadsGrid._options.dataSource[h].Weight).toFixed(3);
                OperationRecordOverHead.ChargesType = OtherHeadsGrid._options.dataSource[h].RateType;
                OperationRecordOverHead.Rate = Number(OtherHeadsGrid._options.dataSource[h].Rate).toFixed(4);
                OperationRecordOverHead.Amount = Number(OtherHeadsGrid._options.dataSource[h].HeadAmount).toFixed(2);
                TotalHeadAmt = Number(TotalHeadAmt) + Number(OtherHeadsGrid._options.dataSource[h].HeadAmount);
                TotalHeadAmt = Number(TotalHeadAmt).toFixed(2);

                jsonObjectsRecordOverHead.push(OperationRecordOverHead);
            }
        }
    }

    var jsonObjectsRecordMain = [];
    var OperationRecordMain = {};

    OperationRecordMain = {};
    //if (GblStatus === "Update") {
    //    OperationRecordMain.TransactionID = document.getElementById("TxtPOID").value;
    //}
    OperationRecordMain.VoucherID = -11;
    OperationRecordMain.VoucherDate = VoucherDate;
    OperationRecordMain.LedgerID = SupplierName;
    //OperationRecordMain.ProductionUnitID = 0;
    OperationRecordMain.ContactPersonID = (ContactPersonName === undefined || ContactPersonName === null || ContactPersonName === "") ? 0 : ContactPersonName;
    OperationRecordMain.TotalQuantity = TxtTotalQty;
    OperationRecordMain.TotalBasicAmount = Number(TxtBasicAmt).toFixed(2);
    OperationRecordMain.TotalCGSTTaxAmount = Number(TxtCGSTAmt).toFixed(2);
    OperationRecordMain.TotalSGSTTaxAmount = Number(TxtSGSTAmt).toFixed(2);
    OperationRecordMain.TotalIGSTTaxAmount = Number(TxtIGSTAmt).toFixed(2);
    OperationRecordMain.TotalTaxAmount = Number(TxtTaxAmt).toFixed(2);
    OperationRecordMain.NetAmount = Number(TxtNetAmt).toFixed(2);
    OperationRecordMain.TotalOverheadAmount = Number(TotalHeadAmt).toFixed(2);
    OperationRecordMain.PurchaseDivision = PurchaseDivision;
    OperationRecordMain.PurchaseReferenceRemark = PORefernce;
    OperationRecordMain.DeliveryAddress = textDeliverAt;
    //OperationRecordMain.Narration = textNaretion;
    OperationRecordMain.TermsOfPayment = termsofpayment;
    OperationRecordMain.CurrencyCode = CurrencyCode;
    OperationRecordMain.ModeOfTransport = ModeOfTransport;
    OperationRecordMain.DealerID = DealerName;
    OperationRecordMain.VoucherApprovalByEmployeeID = SelPOApprovalBy;
    jsonObjectsRecordMain.push(OperationRecordMain);

    let UserApprovalProcessArray = [];
    let UserApprovalProcessObj = {};

    for (let q = 0; q < CreatePOGridRow; q++) {
        UserApprovalProcessObj = {};
        UserApprovalProcessObj.LedgerID = $('#SupplierName').dxSelectBox('instance').option('value');
        UserApprovalProcessObj.LedgerName = $('#SupplierName').dxSelectBox('instance').option('text');
        UserApprovalProcessObj.ItemRate = CreatePOGrid._options.dataSource[q].PurchaseRate;
        UserApprovalProcessObj.ItemAmount = CreatePOGrid._options.dataSource[q].TotalAmount;
        UserApprovalProcessObj.ItemID = CreatePOGrid._options.dataSource[q].ItemID;
        UserApprovalProcessObj.ItemName = CreatePOGrid._options.dataSource[q].ItemName;
        UserApprovalProcessObj.ItemCode = CreatePOGrid._options.dataSource[q].ItemCode;
        UserApprovalProcessObj.ExpectedDeliveryDate = CreatePOGrid._options.dataSource[q].ExpectedDeliveryDate;
        UserApprovalProcessObj.PurchaseQty = Number(CreatePOGrid._options.dataSource[q].PurchaseQuantity);
        UserApprovalProcessArray.push(UserApprovalProcessObj)
    }

    var jsonObjectsRecordDetail = [];
    var OperationRecordDetail = {};
    if (CreatePOGridRow > 0) {
        for (var e = 0; e < CreatePOGridRow; e++) {
            OperationRecordDetail = {};

            OperationRecordDetail.ItemID = CreatePOGrid._options.dataSource[e].ItemID;
            OperationRecordDetail.TransID = e + 1;
            //OperationRecordDetail.ProductionUnitID = 0;
            OperationRecordDetail.ItemGroupID = CreatePOGrid._options.dataSource[e].ItemGroupID;
            OperationRecordDetail.RequiredQuantity = Number(CreatePOGrid._options.dataSource[e].RequiredQuantity).toFixed(3);
            OperationRecordDetail.RequiredNoOfPacks = Number(CreatePOGrid._options.dataSource[e].RequiredNoOfPacks).toFixed(0);
            OperationRecordDetail.QuantityPerPack = Number(CreatePOGrid._options.dataSource[e].QuantityPerPack);
            OperationRecordDetail.PurchaseOrderQuantity = Number(CreatePOGrid._options.dataSource[e].PurchaseQuantity).toFixed(2);
            OperationRecordDetail.ChallanWeight = Number(CreatePOGrid._options.dataSource[e].PurchaseQuantityInStockUnit).toFixed(2); //Added by pkp For print out 06052020
            OperationRecordDetail.PurchaseUnit = CreatePOGrid._options.dataSource[e].PurchaseUnit;
            OperationRecordDetail.StockUnit = CreatePOGrid._options.dataSource[e].StockUnit;
            OperationRecordDetail.ItemDescription = CreatePOGrid._options.dataSource[e].ItemDescription;
            OperationRecordDetail.PurchaseRate = Number(CreatePOGrid._options.dataSource[e].PurchaseRate).toFixed(4);
            OperationRecordDetail.PurchaseTolerance = CreatePOGrid._options.dataSource[e].Tolerance;
            OperationRecordDetail.GrossAmount = Number(CreatePOGrid._options.dataSource[e].BasicAmount).toFixed(2);
            OperationRecordDetail.DiscountPercentage = Number(CreatePOGrid._options.dataSource[e].Disc).toFixed(2);
            OperationRecordDetail.DiscountAmount = (Number(CreatePOGrid._options.dataSource[e].BasicAmount) - Number(CreatePOGrid._options.dataSource[e].AfterDisAmt)).toFixed(2);
            OperationRecordDetail.BasicAmount = Number(CreatePOGrid._options.dataSource[e].AfterDisAmt).toFixed(2);
            OperationRecordDetail.TaxableAmount = Number(CreatePOGrid._options.dataSource[e].TaxableAmount).toFixed(2);
            OperationRecordDetail.GSTPercentage = CreatePOGrid._options.dataSource[e].GSTTaxPercentage;
            OperationRecordDetail.CGSTPercentage = CreatePOGrid._options.dataSource[e].CGSTTaxPercentage;
            OperationRecordDetail.SGSTPercentage = CreatePOGrid._options.dataSource[e].SGSTTaxPercentage;
            OperationRecordDetail.IGSTPercentage = CreatePOGrid._options.dataSource[e].IGSTTaxPercentage;
            OperationRecordDetail.CGSTAmount = Number(CreatePOGrid._options.dataSource[e].CGSTAmt).toFixed(2);
            OperationRecordDetail.SGSTAmount = Number(CreatePOGrid._options.dataSource[e].SGSTAmt).toFixed(2);
            OperationRecordDetail.IGSTAmount = Number(CreatePOGrid._options.dataSource[e].IGSTAmt).toFixed(2);
            OperationRecordDetail.NetAmount = Number(CreatePOGrid._options.dataSource[e].TotalAmount).toFixed(2);
            OperationRecordDetail.ItemNarration = (CreatePOGrid._options.dataSource[e].ItemNarration === undefined || CreatePOGrid._options.dataSource[e].ItemNarration === null) ? "" : CreatePOGrid._options.dataSource[e].ItemNarration;
            OperationRecordDetail.ExpectedDeliveryDate = CreatePOGrid._options.dataSource[e].ExpectedDeliveryDate;
            OperationRecordDetail.RefJobBookingJobCardContentsID = CreatePOGrid._options.dataSource[e].RefJobBookingJobCardContentsID;
            OperationRecordDetail.RefJobCardContentNo = CreatePOGrid._options.dataSource[e].RefJobCardContentNo;
            OperationRecordDetail.ClientID = CreatePOGrid._options.dataSource[e].ClientID || 0;
            OperationRecordDetail.Remark = CreatePOGrid._options.dataSource[e].Remark;
            //OperationRecordDetail.HSNCode = CreatePOGrid._options.dataSource[e].HSNCode;
            OperationRecordDetail.ProductHSNID = (CreatePOGrid._options.dataSource[e].ProductHSNID === undefined || CreatePOGrid._options.dataSource[e].ProductHSNID === null) ? 0 : CreatePOGrid._options.dataSource[e].ProductHSNID;

            jsonObjectsRecordDetail.push(OperationRecordDetail);
        }
    }
    //console.log(jsonObjectsRecordDetail);
    var jsonObjectsRecordTax = [];
    var OperationRecordTax = {};

    if (ChargesGrid.length > 0) {
        for (var ch = 0; ch < ChargesGrid.length; ch++) {
            OperationRecordTax = {};
            OperationRecordTax.TransID = ch + 1;
            OperationRecordTax.LedgerID = ChargesGrid[ch].LedgerID;
            OperationRecordTax.TaxPercentage = ChargesGrid[ch].TaxRatePer;
            OperationRecordTax.Amount = Number(ChargesGrid[ch].ChargesAmount).toFixed(2);
            OperationRecordTax.TaxInAmount = ChargesGrid[ch].InAmount;
            OperationRecordTax.IsComulative = (ChargesGrid[ch].IsCumulative === undefined || ChargesGrid[ch].IsCumulative === null) ? false : ChargesGrid[ch].IsCumulative;
            OperationRecordTax.GSTApplicable = ChargesGrid[ch].GSTApplicable;
            OperationRecordTax.CalculatedON = ChargesGrid[ch].CalculateON;

            jsonObjectsRecordTax.push(OperationRecordTax);
        }
    }

    var jsonObjectsRecordSchedule = [];
    var OperationRecordSchedule = {};
    if (ScheduleListOBJ.length > 0) {
        for (var sch = 0; sch < ScheduleListOBJ.length; sch++) {
            OperationRecordSchedule = {};
            OperationRecordSchedule.TransID = sch + 1;
            OperationRecordSchedule.ItemID = ScheduleListOBJ[sch].ItemID;
            OperationRecordSchedule.Quantity = Number(ScheduleListOBJ[sch].Quantity).toFixed(3);
            OperationRecordSchedule.Unit = ScheduleListOBJ[sch].PurchaseUnit;
            OperationRecordSchedule.ScheduleDeliveryDate = ScheduleListOBJ[sch].SchDate;

            jsonObjectsRecordSchedule.push(OperationRecordSchedule);
        }
    }

    jsonObjectsRecordMain = JSON.stringify(jsonObjectsRecordMain);
    jsonObjectsRecordDetail = JSON.stringify(jsonObjectsRecordDetail);
    jsonObjectsRecordOverHead = JSON.stringify(jsonObjectsRecordOverHead);
    jsonObjectsRecordTax = JSON.stringify(jsonObjectsRecordTax);
    jsonObjectsRecordSchedule = JSON.stringify(jsonObjectsRecordSchedule);
    //jsonObjectsRecordRequisition = JSON.stringify(jsonObjectsRecordRequisition);
    let transactID = 0;
    let PONo = "";
    if (GblStatus !== "Update") {
        if (Number(SelectedProductionUnitID) != 0) {
            if (GBLProductionUnitID != SelectedProductionUnitID) {
                swal("Attention!", "Selected transaction is related to unit " + SelectedProductionUnitName + ", Kindly login with unit " + SelectedProductionUnitName + " to process.", "warning");
                $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                return;
            }
        }
        var txt = 'If you confident please click on \n' + 'Yes \n' + 'otherwise click on \n' + 'Cancel';
        //swal({
        //    title: "Do you want to continue..?",
        //    text: txt,
        //    type: "warning",
        //    showCancelButton: true,
        //    confirmButtonColor: "#DD6B55",
        //    confirmButtonText: "Yes",
        //    closeOnConfirm: true
        //},
        //    function () {
        //        (async () => {
        //            //data: '{prefix:' + JSON.stringify(prefix) + ',jsonObjectsRecordMain:' + jsonObjectsRecordMain + ',jsonObjectsRecordDetail:' + jsonObjectsRecordDetail + ',jsonObjectsRecordOverHead:' + jsonObjectsRecordOverHead + ',jsonObjectsRecordTax:' + jsonObjectsRecordTax + ',jsonObjectsRecordSchedule:' + jsonObjectsRecordSchedule + ',jsonObjectsRecordRequisition:' + JSON.stringify(jsonObjectsRecordRequisition) + ',TxtNetAmt:' + JSON.stringify(TxtNetAmt) + ',CurrencyCode:' + JSON.stringify(CurrencyCode) + ',jsonObjectsUserApprovalProcessArray:' + JSON.stringify(UserApprovalProcessArray) + '}',

        //            //data: '{TransactionID:' + JSON.stringify(document.getElementById("TxtPOID").value) + ',jsonObjectsRecordMain:' + jsonObjectsRecordMain + ',jsonObjectsRecordDetail:' + jsonObjectsRecordDetail + ',jsonObjectsRecordOverHead:' + jsonObjectsRecordOverHead + ',jsonObjectsRecordTax:' + jsonObjectsRecordTax + ',jsonObjectsRecordSchedule:' + jsonObjectsRecordSchedule + ',jsonObjectsRecordRequisition:' + JSON.stringify(jsonObjectsRecordRequisition) + ',TxtNetAmt:' + JSON.stringify(TxtNetAmt) + ',CurrencyCode:' + JSON.stringify(CurrencyCode) + ',jsonObjectsUserApprovalProcessArray:' + JSON.stringify(UserApprovalProcessArray) + ',ObjvalidateLoginUser:' + JSON.stringify(validateUserData) + '}',
        //            transactID = 0;
        //            PONo = "";
        //            await savePurchaseOrderData(prefix, transactID, jsonObjectsRecordMain, jsonObjectsRecordDetail, jsonObjectsRecordOverHead, jsonObjectsRecordTax, jsonObjectsRecordSchedule, jsonObjectsRecordRequisition, TxtNetAmt, CurrencyCode, UserApprovalProcessArray, validateUserData, PONo, FilejsonObjectsTransactionMain)
        //        })();
        //    }
        //);
        // STEP 1: Check missing client names from grid
        let missingClients = rows
            .filter(r => !r.ClientID || r.ClientID === 0)
            .map(r => `${r.ItemCode} (${r.ItemName})`);


        // STEP 2: If missing found → show different swal
        if (missingClients.length > 0) {

            const result = await Swal.fire({
                title: "Client Name is Empty!",
                text: "Client not selected for: " + missingClients.join(", "),
                icon: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "Yes, Continue",
                cancelButtonText: "Cancel",
                didOpen: () => {
                    document.querySelector(".swal2-popup").style.borderRadius = "10px";
                }
            });

            if (result.isConfirmed) {

                // 2nd Confirmation Popup (DOUBLE CONFIRMATION)
                const second = await Swal.fire({
                    title: "Are you sure?",
                    text: "Do you really want to continue without Client Name?",
                    icon: "question",
                    showCancelButton: true,
                    confirmButtonText: "Yes, Proceed",
                    cancelButtonText: "No, Stop"
                });

                if (!second.isConfirmed) {
                    return; // User cancelled second confirmation
                }

                // ---- IF Both Confirmations YES ----
                let transactID = 0;
                let PONo = "";

                await savePurchaseOrderData(
                    prefix, transactID, jsonObjectsRecordMain, jsonObjectsRecordDetail,
                    jsonObjectsRecordOverHead, jsonObjectsRecordTax, jsonObjectsRecordSchedule,
                    jsonObjectsRecordRequisition, TxtNetAmt, CurrencyCode,
                    UserApprovalProcessArray, validateUserData, PONo, FilejsonObjectsTransactionMain
                );
            }

            return; // Stop normal save flow
        }

        swal({
            title: "Do you want to continue..?",
            text: txt,
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "Yes",
            closeOnConfirm: true
        },
            function () {
                (async () => {
                    let transactID = 0;
                    let PONo = "";

                    await savePurchaseOrderData(prefix, transactID, jsonObjectsRecordMain, jsonObjectsRecordDetail, jsonObjectsRecordOverHead, jsonObjectsRecordTax, jsonObjectsRecordSchedule, jsonObjectsRecordRequisition, TxtNetAmt, CurrencyCode, UserApprovalProcessArray, validateUserData, PONo, FilejsonObjectsTransactionMain)

                })(); 
            }
        ); 
    } else {
        (async () => {
            transactID = document.getElementById("TxtPOID").value;
            PONo = document.getElementById("LblPONo").value;
            await savePurchaseOrderData(prefix, transactID, jsonObjectsRecordMain, jsonObjectsRecordDetail, jsonObjectsRecordOverHead, jsonObjectsRecordTax, jsonObjectsRecordSchedule, jsonObjectsRecordRequisition, TxtNetAmt, CurrencyCode, UserApprovalProcessArray, validateUserData, PONo, FilejsonObjectsTransactionMain)
        })();
    }

    //var txt = 'If you confident please click on \n' + 'Yes \n' + 'otherwise click on \n' + 'Cancel';
    //swal({
    //    title: "Do you want to continue..?",
    //    text: txt,
    //    type: "warning",
    //    showCancelButton: true,
    //    confirmButtonColor: "#DD6B55",
    //    confirmButtonText: "Yes",
    //    closeOnConfirm: true
    //},
    //    function () {
    //        $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
    //        if (GblStatus === "Update") {
    //            //alert(JSON.stringify(jsonObjectsRecordMain));
    //            $.ajax({
    //                type: "POST",
    //                url: "WebService_PurchaseOrder.asmx/UpdatePurchaseOrder",
    //                data: '{TransactionID:' + JSON.stringify(document.getElementById("TxtPOID").value) + ',jsonObjectsRecordMain:' + jsonObjectsRecordMain + ',jsonObjectsRecordDetail:' + jsonObjectsRecordDetail + ',jsonObjectsRecordOverHead:' + jsonObjectsRecordOverHead + ',jsonObjectsRecordTax:' + jsonObjectsRecordTax + ',jsonObjectsRecordSchedule:' + jsonObjectsRecordSchedule + ',jsonObjectsRecordRequisition:' + JSON.stringify(jsonObjectsRecordRequisition) + ',TxtNetAmt:' + JSON.stringify(TxtNetAmt) + ',CurrencyCode:' + JSON.stringify(CurrencyCode) + '}',
    //                contentType: "application/json; charset=utf-8",
    //                dataType: "json",
    //                success: function (results) {
    //                    var res = JSON.stringify(results);
    //                    res = res.replace(/"d":/g, '');
    //                    res = res.replace(/{/g, '');
    //                    res = res.replace(/}/g, '');
    //                    res = res.substr(1);
    //                    res = res.slice(0, -1);

    //                    $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);

    //                    var Title, Text, Type;
    //                    if (results.d === "Success") {
    //                        document.getElementById("BtnSave").setAttribute("data-dismiss", "modal");
    //                        Text = "Your data updated successfully..";
    //                        Title = "Updated...";
    //                        Type = "success";
    //                    } else if (results.d.includes("not authorized")) {
    //                        Title = "Can't Update..!";
    //                        Text = results.d;
    //                        Type = "warning";
    //                    } else if (results.d.includes("Error:")) {
    //                        Title = "Error..!";
    //                        Text = results.d;
    //                        Type = "error";
    //                    }
    //                    swal(Title, Text, Type);
    //                    if (Type === "success") location.reload();
    //                },
    //                error: function errorFunc(jqXHR) {
    //                    $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
    //                    swal("Error!", "Please try after some time..", "");
    //                }
    //            });
    //        }
    //        else {
    //            $.ajax({
    //                type: "POST",
    //                url: "WebService_PurchaseOrder.asmx/SavePaperPurchaseOrder",
    //                data: '{prefix:' + JSON.stringify(prefix) + ',jsonObjectsRecordMain:' + jsonObjectsRecordMain + ',jsonObjectsRecordDetail:' + jsonObjectsRecordDetail + ',jsonObjectsRecordOverHead:' + jsonObjectsRecordOverHead + ',jsonObjectsRecordTax:' + jsonObjectsRecordTax + ',jsonObjectsRecordSchedule:' + jsonObjectsRecordSchedule + ',jsonObjectsRecordRequisition:' + JSON.stringify(jsonObjectsRecordRequisition) + ',TxtNetAmt:' + JSON.stringify(TxtNetAmt) + ',CurrencyCode:' + JSON.stringify(CurrencyCode) + '}',
    //                // data: '{prefix:' + JSON.stringify(prefix) + '}',
    //                contentType: "application/json; charset=utf-8",
    //                dataType: "json",
    //                success: function (results) {
    //                    var res = JSON.stringify(results);
    //                    res = res.replace(/"d":/g, '');
    //                    res = res.replace(/{/g, '');
    //                    res = res.replace(/}/g, '');
    //                    res = res.substr(1);
    //                    res = res.slice(0, -1);

    //                    $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
    //                    var Title, Text, Type;
    //                    if (results.d === "Success") {
    //                        Text = "Your data saved successfully..";
    //                        Title = "Success...";
    //                        Type = "success";
    //                    } else if (results.d.includes("not authorized")) {
    //                        Title = "Not Save..!";
    //                        Text = results.d;
    //                        Type = "warning";
    //                    } else {
    //                        Title = "Error..!";
    //                        Text = results.d;
    //                        Type = "error";
    //                    }
    //                    swal(Title, Text, Type);
    //                    if (Type === "success") location.reload();
    //                },
    //                error: function errorFunc(jqXHR) {
    //                    $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
    //                    swal("Error!", "Please try after some time..", "");
    //                    console.log(jqXHR);
    //                }
    //            });
    //        }
    //    });

});

async function savePurchaseOrderData(prefix, transactID, jsonObjectsRecordMain, jsonObjectsRecordDetail, jsonObjectsRecordOverHead, jsonObjectsRecordTax, jsonObjectsRecordSchedule, jsonObjectsRecordRequisition, TxtNetAmt, CurrencyCode, UserApprovalProcessArray, validateUserData, PONo, FilejsonObjectsTransactionMain) {
    if (GblStatus === "Update") {
        if (Number(SelectedProductionUnitID) != 0) {
            if (GBLProductionUnitID != SelectedProductionUnitID) {
                swal("Attention!", "Selected transaction is related to unit " + SelectedProductionUnitName + ", Kindly login with unit " + SelectedProductionUnitName + " to process.", "warning");
                $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                return;
            }
        }
        try {
            $.ajax({
                type: "POST",
                url: "WebService_PurchaseOrder.asmx/UpdatePurchaseOrder",
                data: '{TransactionID:' + JSON.stringify(transactID) + ',jsonObjectsRecordMain:' + jsonObjectsRecordMain + ',jsonObjectsRecordDetail:' + jsonObjectsRecordDetail + ',jsonObjectsRecordOverHead:' + jsonObjectsRecordOverHead + ',jsonObjectsRecordTax:' + jsonObjectsRecordTax + ',jsonObjectsRecordSchedule:' + jsonObjectsRecordSchedule + ',jsonObjectsRecordRequisition:' + JSON.stringify(jsonObjectsRecordRequisition) + ',TxtNetAmt:' + JSON.stringify(TxtNetAmt) + ',CurrencyCode:' + JSON.stringify(CurrencyCode) + ',jsonObjectsUserApprovalProcessArray:' + JSON.stringify(UserApprovalProcessArray) + ',ObjvalidateLoginUser:' + JSON.stringify(validateUserData) + ',voucherNo:' + JSON.stringify(PONo) + ',FilejsonObjectsTransactionMain:' + FilejsonObjectsTransactionMain + '}',
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (results) {
                    var res = JSON.stringify(results);
                    res = res.replace(/"d":/g, '');
                    res = res.replace(/{/g, '');
                    res = res.replace(/}/g, '');
                    res = res.substr(1);
                    res = res.slice(0, -1);

                    $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);

                    var Title, Text, Type;
                    if (results.d === "Success") {
                        document.getElementById("BtnSave").setAttribute("data-dismiss", "modal");
                        Text = "Your data updated successfully..";
                        Title = "Updated...";
                        Type = "success";
                        validateUserData.isUserInfoFilled = false;
                    } else if (results.d.includes("not authorized")) {
                        Title = "Can't Update..!";
                        Text = results.d;
                        Type = "warning";
                    } else if (res === "InvalidUser") {
                        swal("Invalid User!", "Invalid user credentials, please enter valid username or password to update the information.", "error");
                        validateUserData.isUserInfoFilled = false;
                        return false;
                    } else if (results.d.includes("Error:")) {
                        Title = "Error..!";
                        Text = results.d;
                        Type = "error";
                    } else if (res === "TransactionUsed") {
                        swal("error", "This item is used in another process..! Record can not be Updated.", "error");
                        return false;
                    } else if (res === "PurchaseOrderApproved") {
                        swal("error", "Sorry Your Purchase Order is Approved. Please go and Unapprove this Order first and then Updated.", "error");
                        return false;
                    } else {
                        swal.close();
                        setTimeout(() => {
                            swal("Warning..!", res, "warning");
                        }, 100);
                    }
                    //swal(Title, Text, Type);
                    //setTimeout(function () {

                    //    if (Type === "success") location.reload();
                    //}, 3000);
                    swal({
                        title: Title,
                        text: Text,
                        type: Type
                    }, function (isConfirm) {
                        if (isConfirm && Type === "success") {
                            location.reload();
                            $("#POGridProcess").dxDataGrid("instance").refresh();
                            $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                            document.getElementById("BtnSave").disabled = false;
                        }
                    });

                },
                error: function errorFunc(jqXHR) {
                    $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                    swal("Error!", "Please try after some time..", "");
                }
            });
        } catch (e) {
            console.log(e.message);
        }
    } else {
        try {
            $.ajax({
                type: "POST",
                url: "WebService_PurchaseOrder.asmx/SavePaperPurchaseOrder",
                data: '{prefix:' + JSON.stringify(prefix) + ',jsonObjectsRecordMain:' + jsonObjectsRecordMain + ',jsonObjectsRecordDetail:' + jsonObjectsRecordDetail + ',jsonObjectsRecordOverHead:' + jsonObjectsRecordOverHead + ',jsonObjectsRecordTax:' + jsonObjectsRecordTax + ',jsonObjectsRecordSchedule:' + jsonObjectsRecordSchedule + ',jsonObjectsRecordRequisition:' + JSON.stringify(jsonObjectsRecordRequisition) + ',TxtNetAmt:' + JSON.stringify(TxtNetAmt) + ',CurrencyCode:' + JSON.stringify(CurrencyCode) + ',jsonObjectsUserApprovalProcessArray:' + JSON.stringify(UserApprovalProcessArray) + ',FilejsonObjectsTransactionMain:' + FilejsonObjectsTransactionMain + '}',
                // data: '{prefix:' + JSON.stringify(prefix) + '}',
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (results) {
                    var res = JSON.stringify(results);
                    res = res.replace(/"d":/g, '');
                    res = res.replace(/{/g, '');
                    res = res.replace(/}/g, '');
                    res = res.substr(1);
                    res = res.slice(0, -1);

                    $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                    var Title, Text, Type;
                    var transactionID = "";
                    if (res.startsWith("Success")) {
                        Text = "Your data saved successfully..";
                        Title = "Success...";
                        Type = "success";

                        var match = res.match(/TransactionID:\s*(\d+)/);
                        if (match) {
                            transactionID = match[1]; // Extracted TransactionID
                        }

                    } else if (results.d.includes("not authorized")) {
                        Title = "Not Save..!";
                        Text = results.d;
                        Type = "warning";
                    } else if (res === "TransactionDateError") {
                        swal("error!", "Please select requisition data greater than the last date the requisition was created", "error");
                    } else if (res === "EmptyDate") {
                        swal("error!", "Requistion Date Is Empty", "error");
                    } else {
                        swal.close();
                        setTimeout(() => {
                            swal("Warning..!", res, "warning");
                        }, 100);
                    }
                    //swal(Title, Text, Type);
                    //if (Type === "success") location.reload();
                    swal({
                        title: Title,
                        text: Text,
                        type: Type
                    }, function (isConfirm) {
                        if (isConfirm && Type === "success") {
                            location.reload();
                            document.getElementById("BtnSave").disabled = true;
                            document.getElementById("POPrintButton").disabled = false;
                            document.getElementById("TxtPOID").value = transactionID
                        }
                    });
                },
                error: function errorFunc(jqXHR) {
                    $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                    swal("Error!", "Please try after some time..", "");
                    console.log(jqXHR);
                }
            });
        } catch (e) {
            console.log(e.message)
        }
    }
}


function ModalPopupScreencontrols() {
    //PaymentTermsGrid

    $("#AdditionalChargesGrid").dxDataGrid({
        dataSource: []
    });
    $("#CreatePOGrid").dxDataGrid({
        dataSource: []
    });
    for (var j = 0; j < OtherHead.length; j++) {
        OtherHead[j].Weight = 0;
        OtherHead[j].Rate = 0;
        OtherHead[j].HeadAmount = 0;
        OtherHead[j].Sel = false;
    }
    $("#OtherHeadsGrid").dxDataGrid({
        dataSource: OtherHead
    });
    $("#PORefernce").val('');
    $("#textDeliverAt").val('');
    //$("#textNaretion").val('');
    $("#TxtNetAmt").val(0);
    $("#TxtTaxAmt").val(0);
    $("#TxtTotalQty").val(0);
    $("#Txt_TaxAbleSum").val(0);
    $("#TxtAfterDisAmt").val(0);
    $("#Txt_TaxAbleSum").val(0);
    $("#TxtTotalQty").val(0);
    $("#TxtGstamt").val(0);
    $("#TxtOtheramt").val(0);

    document.getElementById("LblSupplierStateTin").innerHTML = 0;
    document.getElementById("CurrentCurrency").innerHTML = "";
    document.getElementById("ConversionRate").innerHTML = "";
    document.getElementById("VatGSTApplicable").innerHTML = "";
    document.getElementById("LblCountry").innerHTML = "";
    document.getElementById("LblCountry").innerHTML = "";
    document.getElementById("LblState").innerHTML = "";
    GblGSTApplicable = true;
    $("#DealerName").dxSelectBox({ value: "" });
    $("#ModeOfTransport").dxSelectBox({ value: "" });
    $("#PurchaseDivision").dxSelectBox({ value: PurchaseDivisionText[0] });
    $("#ContactPersonName").dxSelectBox({ value: "" });
    $("#SupplierName").dxSelectBox({ value: "" });
    $("#VoucherDate").dxDateBox({ value: new Date().toISOString().substr(0, 10) });
}

$("#BtnAddLedgerCharge").click(function () {
    var AdditionalChargesGrid = $("#AdditionalChargesGrid").dxDataGrid('instance');
    var rowCountAC = AdditionalChargesGrid.totalCount();

    var ChooseText = $("#SelLnameChargesGrid").dxSelectBox('instance').option('text');
    if (ChooseText === "" || ChooseText === "undefined" || ChooseText === null) {
        document.getElementById("SelLnameChargesGrid").style.borderColor = "#00DDD2";
        //DevExpress.ui.notify("Please Choose Ledger Name..!", "warning", 1200);
        DevExpress.ui.notify({
            message: "Please Choose Ledger Name..!", type: "warning", displayTime: 5000, width: "900px",
            onContentReady: function (e) {
                e.component.$content().find(".dx-toast-message").css({
                    "font-size": "13px",
                    "font-weight": "bold",
                });
                const closeButton = $("<div>")
                    .addClass("dx-notification-close")
                    .text("×")
                    .css({
                        "position": "absolute",
                        "top": "5px",
                        "right": "5px",
                        "cursor": "pointer",
                        "font-size": "25px",
                    })
                    .appendTo(e.component.$content());
                closeButton.on("click", function () {
                    e.component.hide();
                });
            }
        });
        return;
    }
    var ChooseID = $("#SelLnameChargesGrid").dxSelectBox('instance').option('value');

    if (rowCountAC > 0) {
        for (var cl = 0; cl < rowCountAC; cl++) {
            if (ChooseID === AdditionalChargesGrid._options.dataSource[cl].LedgerID) {
                //DevExpress.ui.notify("This Tax Ledger already exist.. please add another Tax ledger..!", "warning", 1200);
                DevExpress.ui.notify({
                    message: "This Tax Ledger already exist.. please add another Tax ledger..!", type: "warning", displayTime: 5000, width: "900px",
                    onContentReady: function (e) {
                        e.component.$content().find(".dx-toast-message").css({
                            "font-size": "13px",
                            "font-weight": "bold",
                        });
                        const closeButton = $("<div>")
                            .addClass("dx-notification-close")
                            .text("×")
                            .css({
                                "position": "absolute",
                                "top": "5px",
                                "right": "5px",
                                "cursor": "pointer",
                                "font-size": "25px",
                            })
                            .appendTo(e.component.$content());
                        closeButton.on("click", function () {
                            e.component.hide();
                        });
                    }
                });
                return false;
            }
        }
    }

    if (Number(document.getElementById("TxtAfterDisAmt").value) === 0) {
        //DevExpress.ui.notify("Please enter Purchase rate in above grid before add charges..!", "error", 1000);
        DevExpress.ui.notify({
            message: "Please enter Purchase rate in above grid before add charges..!", type: "error", displayTime: 5000, width: "900px",
            onContentReady: function (e) {
                e.component.$content().find(".dx-toast-message").css({
                    "font-size": "13px",
                    "font-weight": "bold",
                });
                const closeButton = $("<div>")
                    .addClass("dx-notification-close")
                    .text("×")
                    .css({
                        "position": "absolute",
                        "top": "5px",
                        "right": "5px",
                        "cursor": "pointer",
                        "font-size": "25px",
                    })
                    .appendTo(e.component.$content());
                closeButton.on("click", function () {
                    e.component.hide();
                });
            }
        });
        window.setTimeout(function () { e.component.cancelEditData(); }, 0);
    } else {
        var optCH = {};
        if (ChooseText !== "" && ChooseText !== undefined && ChooseText !== null) {
            ObjChargeHead = Var_ChargeHead.LedgerDetail.filter(function (el) {
                return el.LedgerID === ChooseID;
            });
            optCH.LedgerName = ChooseText;
            optCH.LedgerID = ObjChargeHead[0].LedgerID;
            var gstapl = ObjChargeHead[0].GSTApplicable;
            if (gstapl === "True" || gstapl === true) {
                gstapl = true;
            } else {
                gstapl = false;
            }
            optCH.GSTApplicable = gstapl;
            optCH.TaxType = ObjChargeHead[0].TaxType;
            optCH.GSTLedgerType = ObjChargeHead[0].GSTLedgerType;
            optCH.CalculateON = ObjChargeHead[0].GSTCalculationOn;
            optCH.TaxRatePer = ObjChargeHead[0].TaxPercentage;
            optCH.InAmount = false;
            ChargesGrid.push(optCH);

            $("#AdditionalChargesGrid").dxDataGrid({
                dataSource: ChargesGrid
            });

            AddItemCalculation();
            GridColumnCal();
            //AddItemWithChargessGrid();
            CalculateAmount();
            var CreatePOGrid = $("#CreatePOGrid").dxDataGrid('instance');
            CreatePOGrid.refresh();
            //var AdditionalChargesGrid = $('#AdditionalChargesGrid').dxDataGrid('instance');
            AdditionalChargesGrid.refresh();
        }
    }
});

$("#BtnAddPayTerms").click(function () {
    var TxtAddPayTerms = $("#TxtAddPayTerms").dxSelectBox("instance").option("value");
    //if (TxtAddPayTerms === "" || TxtAddPayTerms === "undefined" || TxtAddPayTerms === null) {
    //    DevExpress.ui.notify("Please Add Payment Terms..!", "warning", 1200);
    //    return;
    //}

    var GetPaymentTermsGrid = $("#PaymentTermsGrid").dxDataGrid('instance');
    var PaymentTermsGridCount = GetPaymentTermsGrid.totalCount();

    var optpaytr = {};

    optpaytr.TermsID = PaymentTermsGridCount + 1;
    optpaytr.Terms = TxtAddPayTerms;
    PaymentTermsGrid.push(optpaytr);

    $("#PaymentTermsGrid").dxDataGrid({
        dataSource: PaymentTermsGrid
    });
});



$("#POPrintButton").click(function () {
    var TxtPOID = document.getElementById("TxtPOID").value;
    var CreatePOGrid = $("#CreatePOGrid").dxDataGrid('instance');
    var CreatePOGridRow = CreatePOGrid._options.dataSource.length;

    var pageName = window.location.pathname.split("/").pop();
    try {
        $.ajax({
            async: false,
            type: "POST",
            url: "WebServiceOthers.asmx/GetReportViewerName",
            data: '{DocumentName:' + JSON.stringify(pageName) + '}',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                let pagename = (response.d === undefined || response.d === null || response.d === "") ? "ReportPurchaseOrder.aspx" : response.d;
                var url = pagename + "?TransactionID=" + TxtPOID + "&ItemGroupID=" + CreatePOGrid._options.dataSource[0].ItemGroupID;
                window.open(url, "blank", "location=yes,height=" + 1100 + ",width=" + window.innerWidth + ",scrollbars=yes,status=no", true);
            },
            error: function (error) {
                swal("Error", "Something went wrong. Please try again!", "error");
            }
        });
    } catch (e) {
        console.log(e);
    }
    //var url = "ReportPurchaseOrder.aspx?TransactionID=" + TxtPOID + "&ItemGroupID=" + CreatePOGrid._options.dataSource[0].ItemGroupID;
    //window.open(url, "blank", "location=yes,height=1100,width=1050,scrollbars=yes,status=no", true);
});

//$("#largeModalDisNone").click(function () {
//    document.getElementById("largeModalDisNone").setAttribute("data-dismiss", "modal");
//});

$("#BtnNotification").click(function () {
    var reqid = "";
    var purchaseid = "";
    var commentData = "";
    var newHtml = '';
    if (GblStatus === "Save" || GblStatus === "save" || GblStatus === "") {
        if (SubGridData.length > 0) {
            for (var i = 0; i < SubGridData.length; i++) {
                if (SubGridData[i].TransactionID > 0) {
                    if (reqid === "") {
                        reqid = SubGridData[i].TransactionID.toString();
                    } else {
                        reqid = reqid + "," + SubGridData[i].TransactionID.toString();
                    }
                }
            }

            document.getElementById("commentbody").innerHTML = "";
            if (reqid !== "") {
                $.ajax({
                    type: "POST",
                    url: "WebService_PurchaseOrder.asmx/GetCommentData",
                    data: '{PurchaseTransactionID:0,requisitionIDs:' + JSON.stringify(reqid) + '}',
                    contentType: "application/json; charset=utf-8",
                    dataType: "text",
                    success: function (results) {
                        var res = results.replace(/\\/g, '');
                        res = res.replace(/"d":""/g, '');
                        res = res.replace(/""/g, '');
                        res = res.replace(/u0026/g, '&');
                        res = res.replace(/u0027/g, "'");
                        res = res.replace(/:,/g, ":null,");
                        res = res.replace(/,}/g, ",null}");
                        res = res.substr(1);
                        res = res.slice(0, -1);
                        commentData = JSON.parse(res);
                        if (commentData.length > 0) {
                            for (var x = 0; x < commentData.length; x++) {
                                newHtml = newHtml + '<div style="width:100%"><b style="text-align: left; color: red; float: left; margin-top: 5px;width: 100%">' + (x + 1) + '. ' + commentData[x].ModuleName + ', Title : ' + commentData[x].CommentTitle + ', Type : ' + commentData[x].CommentType + '</b>';
                                newHtml = newHtml + '<p style="text-align: left; margin-top: 2px; float: left; margin-left: 20px">' + commentData[x].CommentDescription + '</p><span style="float: right">Comment By : ' + commentData[x].UserName + '</span></div>';
                            }
                        }
                        $("#commentbody").append(newHtml);
                        $(".commentInput").hide();
                    }
                });
            }
        }
    } else {
        purchaseid = document.getElementById("TxtPOID").value;
        if (purchaseid === "" || purchaseid === null || purchaseid === undefined) {
            alert("Please select valid purchase order to view comment details..!");
            return false;
        }
        document.getElementById("commentbody").innerHTML = "";
        if (purchaseid !== "") {
            $.ajax({
                type: "POST",
                url: "WebService_PurchaseOrder.asmx/GetCommentData",
                data: '{PurchaseTransactionID:' + JSON.stringify(purchaseid) + ',requisitionIDs:0}',
                contentType: "application/json; charset=utf-8",
                dataType: "text",
                success: function (results) {
                    var res = results.replace(/\\/g, '');
                    res = res.replace(/"d":""/g, '');
                    res = res.replace(/""/g, '');
                    res = res.replace(/:,/g, ":null,");
                    res = res.replace(/,}/g, ",null}");
                    res = res.replace(/u0026/g, '&');
                    res = res.replace(/u0027/g, "'");
                    res = res.substr(1);
                    res = res.slice(0, -1);
                    commentData = JSON.parse(res);
                    if (commentData.length > 0) {
                        for (var x = 0; x < commentData.length; x++) {
                            newHtml = newHtml + '<div style="width:100%"><b style="text-align: left; color: red; float: left; margin-top: 5px;width: 100%">' + (x + 1) + '. ' + commentData[x].ModuleName + ', Title : ' + commentData[x].CommentTitle + ', Type : ' + commentData[x].CommentType + '</b>';
                            newHtml = newHtml + '<p style="text-align: left; margin-top: 2px; float: left; margin-left: 20px">' + commentData[x].CommentDescription + '</p><span style="float: right">Comment By : ' + commentData[x].UserName + '</span></div>';
                        }
                    }
                    $("#commentbody").append(newHtml);
                    $(".commentInput").show();
                }
            });
        }
    }
    document.getElementById("BtnNotification").setAttribute("data-toggle", "modal");
    document.getElementById("BtnNotification").setAttribute("data-target", "#CommentModal");
});

$(function () {
    $("#BtnSaveComment").click(function () {
        var purchaseid = document.getElementById("TxtPOID").value;
        if (purchaseid === "" || purchaseid === null || purchaseid === undefined) {
            alert("Please select valid purchase order to view comment details..!");
            return false;
        }

        var commentTitle = document.getElementById("TxtCommentTitle").value.trim();
        var commentDesc = document.getElementById("TxtCommentDetail").value.trim();
        var commentType = $('#selCommentType').dxSelectBox('instance').option('value');
        if (commentTitle === undefined || commentTitle === "" || commentTitle === null || commentType === undefined || commentType === "" || commentType === null || commentDesc === undefined || commentDesc === null || commentDesc === "") {
            alert("Please enter valid comment title, type and description..!");
            return false;
        }

        var jsonObjectCommentDetail = [];
        var objectCommentDetail = {};

        objectCommentDetail.CommentDate = new Date();
        objectCommentDetail.ModuleID = 0;
        objectCommentDetail.ModuleName = "Purchase Order";
        objectCommentDetail.CommentTitle = commentTitle;
        objectCommentDetail.CommentDescription = commentDesc;
        objectCommentDetail.CommentType = commentType;
        objectCommentDetail.TransactionID = purchaseid;

        jsonObjectCommentDetail.push(objectCommentDetail);
        jsonObjectCommentDetail = JSON.stringify(jsonObjectCommentDetail);
        $.ajax({
            type: "POST",
            url: "WebService_PurchaseOrder.asmx/SaveCommentData",
            data: '{jsonObjectCommentDetail:' + jsonObjectCommentDetail + '}',
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            success: function (results) {
                var res = JSON.stringify(results);
                res = res.replace(/"d":/g, '');
                res = res.replace(/{/g, '');
                res = res.replace(/}/g, '');
                res = res.substr(1);
                res = res.slice(0, -1);
                //if (res === "Success") {
                // RadioValue = "Pending Requisitions";
                alert("Comment saved!", "Comment saved successfully.", "success");
                var commentData = "";
                var newHtml = '';
                var purchaseid = document.getElementById("TxtPOID").value;
                if (purchaseid === "" || purchaseid === null || purchaseid === undefined) {
                    alert("Please select valid purchase order to view comment details..!");
                    return false;
                }
                document.getElementById("commentbody").innerHTML = "";
                if (purchaseid !== "") {
                    $.ajax({
                        type: "POST",
                        url: "WebService_PurchaseOrder.asmx/GetCommentData",
                        data: '{PurchaseTransactionID:' + JSON.stringify(purchaseid) + ',requisitionIDs:0}',
                        contentType: "application/json; charset=utf-8",
                        dataType: "text",
                        success: function (results) {
                            var res = results.replace(/\\/g, '');
                            res = res.replace(/"d":""/g, '');
                            res = res.replace(/""/g, '');
                            res = res.replace(/:,/g, ":null,");
                            res = res.replace(/,}/g, ",null}");
                            res = res.replace(/u0026/g, '&');
                            res = res.replace(/u0027/g, "'");
                            res = res.substr(1);
                            res = res.slice(0, -1);
                            commentData = JSON.parse(res);

                            if (commentData.length > 0) {
                                for (var x = 0; x < commentData.length; x++) {
                                    newHtml = newHtml + '<div style="width:100%"><b style="text-align: left; color: red; float: left; margin-top: 5px;width: 100%">' + (x + 1) + '. ' + commentData[x].ModuleName + ', Title : ' + commentData[x].CommentTitle + ', Type : ' + commentData[x].CommentType + '</b>';
                                    newHtml = newHtml + '<p style="text-align: left; margin-top: 2px; float: left; margin-left: 20px">' + commentData[x].CommentDescription + '</p><span style="float: right">Comment By : ' + commentData[x].UserName + '</span></div>';
                                }
                            }
                            $("#commentbody").append(newHtml);
                            $(".commentInput").show();
                        }
                    });
                }
            },
            error: function errorFunc(jqXHR) {
                swal("Error!", "Please try after some time..", "");
                alert(jqXHR);
            }
        });
    });
});

//Load Unit Conversion Formula List Added by Minesh Jain 03-Sep-2022
$.ajax({
    type: "POST",
    async: false,
    url: "WebServiceOthers.asmx/GetConversionFormulaList",
    data: '{}',
    contentType: "application/json; charset=utf-8",
    dataType: "text",
    success: function (results) {
        let res = results.replace(/\\/g, '');
        res = res.replace(/"d":""/g, '');
        res = res.replace(/""/g, '');
        res = res.replace(/:,/g, ":null,");
        res = res.replace(/,}/g, ",null}");
        res = res.replace(/u0026/g, '&');
        res = res.substr(1);
        res = res.slice(0, -1);
        GblUnitConversionFormula = JSON.parse(res);
    }
});

function StockUnitConversion(formula, PhysicalStock, UnitPerPacking, WtPerPacking, ConversionFactor, SizeW, UnitDecimalPlace, BaseUnit, ConversionUnit, PaperGSM, ReleaseGSM, AdhesiveGSM, Thickness, Density) {
    let convertedQuantity = 0;
    let conversionFormula = "";
    let GSM = 0;
    GSM = (Number(PaperGSM) + Number(ReleaseGSM) + Number(AdhesiveGSM));
    GSM === 0 ? GSM = (Number(Thickness) * Number(Density)).toFixed(0) : GSM = Number(GSM);
    GSM === 0 ? GSM = 1 : GSM = Number(GSM);

    let ConvertedUnitDecimalPlace = 0;
    if (GblUnitConversionFormula.length > 0) {
        for (let k = 0; k < GblUnitConversionFormula.length; k++) {
            if (GblUnitConversionFormula[k].BaseUnitSymbol.toString().toUpperCase() === BaseUnit.toString().toUpperCase() && GblUnitConversionFormula[k].ConvertedUnitSymbol.toString().toUpperCase() === ConversionUnit.toString().toUpperCase()) {
                conversionFormula = GblUnitConversionFormula[k].ConversionFormula.toString();
                ConvertedUnitDecimalPlace = Number(GblUnitConversionFormula[k].ConvertedUnitDecimalPlace);
            }
        }
        if (conversionFormula === "") {
            conversionFormula = formula;
            ConvertedUnitDecimalPlace = Number(UnitDecimalPlace);
        }

        if (conversionFormula !== "" && conversionFormula !== null && conversionFormula !== undefined && conversionFormula !== "undefined") {
            conversionFormula = conversionFormula.split('e.').join('');
            conversionFormula = conversionFormula.replace("Quantity", "PhysicalStock");

            var n = conversionFormula.search("UnitPerPacking");
            if (n > 0) {
                if (Number(UnitPerPacking) > 0) {
                    convertedQuantity = eval(conversionFormula);
                    convertedQuantity = Number(convertedQuantity).toFixed(Number(ConvertedUnitDecimalPlace));
                }
            } else {
                n = conversionFormula.search("SizeW");
                if (n > 0) {
                    if (Number(SizeW) > 0) {
                        convertedQuantity = eval(conversionFormula);
                        convertedQuantity = Number(convertedQuantity).toFixed(Number(ConvertedUnitDecimalPlace));
                    }
                } else {
                    convertedQuantity = eval(conversionFormula);
                    convertedQuantity = Number(convertedQuantity).toFixed(Number(ConvertedUnitDecimalPlace));
                }
            }
        } else {
            convertedQuantity = Number(PhysicalStock);
        }
    } else {
        convertedQuantity = Number(PhysicalStock).toFixed(Number(UnitDecimalPlace));
    }
    return convertedQuantity;
}

$("#FieldCntainerRow").height = function () {
    return window.innerHeight / 1.2;
};


function calculatePurchaseQuantity(newData, value, currentRowData) {
    var newPurchaseData = {};

    //if (newData.RequiredNoOfPacks !== undefined || newData.RequiredNoOfPacks !== null) {
    //    newPurchaseData.RequiredNoOfPacks = Number(value);
    //    (Number(currentRowData.QuantityPerPack) <= 0 || currentRowData.QuantityPerPack === undefined || currentRowData.QuantityPerPack === null) ? newPurchaseData.QuantityPerPack = 1 : newPurchaseData.QuantityPerPack = Number(currentRowData.QuantityPerPack);
    //    if (newData.PurchaseQuantity !== undefined && newData.PurchaseQuantity !== null && Number(newData.PurchaseQuantity) > 0) {
    //        newPurchaseData.PurchaseQuantityInStockUnit = Number(StockUnitConversion("", Number(newData.PurchaseQuantity), Number(currentRowData.UnitPerPacking), Number(currentRowData.WtPerPacking), Number(currentRowData.ConversionFactor), Number(currentRowData.SizeW), Number(currentRowData.ConvertedUnitDecimalPlace), currentRowData.PurchaseUnit.toString(), currentRowData.StockUnit.toString(), Number(currentRowData.GSM), Number(currentRowData.ReleaseGSM), Number(currentRowData.AdhesiveGSM), Number(currentRowData.Thickness), Number(currentRowData.Density)));
    //        newPurchaseData.RequiredNoOfPacks = Number(newPurchaseData.PurchaseQuantityInStockUnit) / Number(newPurchaseData.QuantityPerPack)
    //    } else {
    //        newPurchaseData.PurchaseQuantityInStockUnit = Number(newPurchaseData.QuantityPerPack) * Number(newPurchaseData.RequiredNoOfPacks);
    //        newPurchaseData.PurchaseQuantity = Number(StockUnitConversion("", Number(newPurchaseData.PurchaseQuantityInStockUnit), Number(currentRowData.UnitPerPacking), Number(currentRowData.WtPerPacking), Number(currentRowData.ConversionFactor), Number(currentRowData.SizeW), Number(currentRowData.ConvertedUnitDecimalPlace), currentRowData.StockUnit.toString(), currentRowData.PurchaseUnit.toString(), Number(currentRowData.GSM), Number(currentRowData.ReleaseGSM), Number(currentRowData.AdhesiveGSM), Number(currentRowData.Thickness), Number(currentRowData.Density)));
    //    }
    if (newData.RequiredNoOfPacks !== undefined && newData.RequiredNoOfPacks !== null) {
        newPurchaseData.RequiredNoOfPacks = Number(value);
        (Number(currentRowData.QuantityPerPack) <= 0 || currentRowData.QuantityPerPack === undefined || currentRowData.QuantityPerPack === null) ? newPurchaseData.QuantityPerPack = 1 : newPurchaseData.QuantityPerPack = Number(currentRowData.QuantityPerPack);

        newPurchaseData.PurchaseQuantityInStockUnit = Number(newPurchaseData.QuantityPerPack) * Number(newPurchaseData.RequiredNoOfPacks);
        newPurchaseData.PurchaseQuantity = Number(StockUnitConversion("", Number(newPurchaseData.PurchaseQuantityInStockUnit), Number(currentRowData.UnitPerPacking), Number(currentRowData.WtPerPacking), Number(currentRowData.ConversionFactor), Number(currentRowData.SizeW), Number(currentRowData.ConvertedUnitDecimalPlace), currentRowData.StockUnit.toString(), currentRowData.PurchaseUnit.toString(), Number(currentRowData.GSM), Number(currentRowData.ReleaseGSM), Number(currentRowData.AdhesiveGSM), Number(currentRowData.Thickness), Number(currentRowData.Density)));

    } else if (newData.QuantityPerPack !== undefined && newData.QuantityPerPack !== null) {
        (Number(currentRowData.RequiredNoOfPacks) <= 0 || currentRowData.RequiredNoOfPacks === undefined || currentRowData.RequiredNoOfPacks === null) ? newPurchaseData.RequiredNoOfPacks = 1 : newPurchaseData.RequiredNoOfPacks = Number(currentRowData.RequiredNoOfPacks);
        newPurchaseData.QuantityPerPack = Number(value);
        newPurchaseData.PurchaseQuantityInStockUnit = Number(newPurchaseData.QuantityPerPack) * Number(newPurchaseData.RequiredNoOfPacks);
        newPurchaseData.PurchaseQuantity = Number(StockUnitConversion("", Number(newPurchaseData.PurchaseQuantityInStockUnit), Number(currentRowData.UnitPerPacking), Number(currentRowData.WtPerPacking), Number(currentRowData.ConversionFactor), Number(currentRowData.SizeW), Number(currentRowData.ConvertedUnitDecimalPlace), currentRowData.StockUnit.toString(), currentRowData.PurchaseUnit.toString(), Number(currentRowData.GSM), Number(currentRowData.ReleaseGSM), Number(currentRowData.AdhesiveGSM), Number(currentRowData.Thickness), Number(currentRowData.Density)));
    } else if (newData.PurchaseQuantity !== undefined && newData.PurchaseQuantity !== null) {
        newPurchaseData.PurchaseQuantity = Number(value);
        (Number(currentRowData.QuantityPerPack) <= 0 || currentRowData.QuantityPerPack === undefined || currentRowData.QuantityPerPack === null) ? newPurchaseData.QuantityPerPack = 1 : newPurchaseData.QuantityPerPack = Number(currentRowData.QuantityPerPack);
        newPurchaseData.PurchaseQuantityInStockUnit = Number(StockUnitConversion("", Number(newPurchaseData.PurchaseQuantity), Number(currentRowData.UnitPerPacking), Number(currentRowData.WtPerPacking), Number(currentRowData.ConversionFactor), Number(currentRowData.SizeW), Number(currentRowData.ConvertedUnitDecimalPlace), currentRowData.PurchaseUnit.toString(), currentRowData.StockUnit.toString(), Number(currentRowData.GSM), Number(currentRowData.ReleaseGSM), Number(currentRowData.AdhesiveGSM), Number(currentRowData.Thickness), Number(currentRowData.Density)));
        newPurchaseData.RequiredNoOfPacks = Math.round(Number(newPurchaseData.PurchaseQuantityInStockUnit) / Number(newPurchaseData.QuantityPerPack));
    }
    return newPurchaseData;
}


function AddAditionalCharges(existReq) {
    var CreatePOGrids = $('#CreatePOGrid').dxDataGrid('instance');

    var gridData = existReq;
    var AdditionalChargesGrid = $('#AdditionalChargesGrid').dxDataGrid('instance');
    ChargesGrid = AdditionalChargesGrid.option('dataSource');
    if (ChargesGrid.length > 0) {
        ChargesGrid = []; // Clear the ChargesGrid

    }

    var rowCountAC = ChargesGrid.length;
    //if (gridData.length <= 0) return false;
    if (gridData.length > 0) {
        if (Number(document.getElementById("TxtAfterDisAmt").value) === 0) {
            //DevExpress.ui.notify("Please enter rate in the above grid before adding charges..!", "error", 1000);
            DevExpress.ui.notify({
                message: "Please enter rate in the above grid before adding charges..!", type: "error", displayTime: 5000, width: "900px",
                onContentReady: function (e) {
                    e.component.$content().find(".dx-toast-message").css({
                        "font-size": "13px",
                        "font-weight": "bold",
                    });
                    const closeButton = $("<div>")
                        .addClass("dx-notification-close")
                        .text("×")
                        .css({
                            "position": "absolute",
                            "top": "5px",
                            "right": "5px",
                            "cursor": "pointer",
                            "font-size": "25px",
                        })
                        .appendTo(e.component.$content());
                    closeButton.on("click", function () {
                        e.component.hide();
                    });
                }
            });
            //window.setTimeout(function () { e.component.cancelEditData(); }, 0);
        } else {
            var optCH = {};
            const ObjChargeHead = Var_ChargeHead.LedgerDetail.filter((ledgerItem) => {
                // Check if the corresponding row in gridData exists and has the expected structure
                if (
                    gridData[0] &&
                    typeof gridData[0].CGSTAmt !== 'undefined' &&
                    typeof gridData[0].IGSTAmt !== 'undefined' &&
                    typeof gridData[0].SGSTAmt !== 'undefined' &&
                    ledgerItem.TaxType.toUpperCase().trim() !== 'VAT'
                ) {
                    return (
                        (gridData[0].CGSTAmt > 0 && ledgerItem.LedgerName === 'CGST') ||
                        (gridData[0].SGSTAmt > 0 && ledgerItem.LedgerName === 'SGST') ||
                        (gridData[0].IGSTAmt > 0 && ledgerItem.LedgerName === 'IGST')

                    );
                } else if (gridData[0] && typeof gridData[0].IGSTAmt !== 'undefined') {
                    return (
                        (gridData[0].IGSTAmt > 0 && ledgerItem.TaxType.toUpperCase().trim() === 'VAT')

                    );
                }
                else {
                    // Log an error or handle the case where the structure is not as expected
                    console.error('Invalid or missing structure in gridData at index:');
                    return false;
                }
            });

            for (var i = 0; i <= ObjChargeHead.length - 1; i++) {
                optCH = {};
                optCH.LedgerID = ObjChargeHead[i].LedgerID;
                optCH.LedgerName = ObjChargeHead[i].LedgerName;
                var gstapl = ObjChargeHead[i].GSTApplicable;
                optCH.GSTApplicable = gstapl === "True" || gstapl === true;
                optCH.TaxType = ObjChargeHead[i].TaxType;
                optCH.GSTLedgerType = ObjChargeHead[i].GSTLedgerType;
                optCH.CalculateON = ObjChargeHead[i].GSTCalculationOn;
                optCH.TaxRatePer = ObjChargeHead[i].TaxPercentage;
                optCH.InAmount = false;

                ChargesGrid.push(optCH);

                AdditionalChargesGrid.option('dataSource', ChargesGrid);

                // Call your functions
                AddItemCalculation();
                CalculateAmount();
                CreatePOGrids.refresh();
                AdditionalChargesGrid.refresh();
            }
        }
    } else {
        var optCH = {};
        let ledgerID = $("#SupplierName").dxSelectBox('instance').option('value');
        if (ledgerID === undefined || ledgerID === null || ledgerID === "") {
            return false;
        }
        const ObjChargeHead = Var_ChargeHead.LedgerDetail.filter((ledgerItem) => {
            // Check if the corresponding row in gridData exists and has the expected structure
            if (GblGSTApplicable) {
                let LblSupplierStateTin = document.getElementById("LblSupplierStateTin").innerHTML;
                if (GblCompanyConfiguration[0].IsGstApplicable === true) {
                    if (Number(LblSupplierStateTin) === Number(GblCompanyStateTin)) {
                        return (
                            (ledgerItem.LedgerName === 'CGST') || (ledgerItem.LedgerName === 'SGST')
                        );
                    }
                    else {
                        return (
                            (ledgerItem.LedgerName === 'IGST') || (ledgerItem.LedgerName === 'IGST')
                        );
                    }
                } else if (GblCompanyConfiguration[0].IsVatApplicable === true) {
                    return (
                        (ledgerItem.TaxType.toUpperCase().trim() === 'VAT')
                    );
                }
            }
        });

        for (var i = 0; i <= ObjChargeHead.length - 1; i++) {
            optCH = {};
            optCH.LedgerID = ObjChargeHead[i].LedgerID;
            optCH.LedgerName = ObjChargeHead[i].LedgerName;
            var gstapl = ObjChargeHead[i].GSTApplicable;
            optCH.GSTApplicable = gstapl === "True" || gstapl === true;
            optCH.TaxType = ObjChargeHead[i].TaxType;
            optCH.GSTLedgerType = ObjChargeHead[i].GSTLedgerType;
            optCH.CalculateON = ObjChargeHead[i].GSTCalculationOn;
            optCH.TaxRatePer = ObjChargeHead[i].TaxPercentage;
            optCH.InAmount = false;


            ChargesGrid.push(optCH);

            AdditionalChargesGrid.option('dataSource', ChargesGrid);

            // Call your functions
            AddItemCalculation();
            CalculateAmount();
            CreatePOGrids.refresh();
            AdditionalChargesGrid.refresh();
        }
    }
}

function setLastTransactiondate() {
    try {
        $.ajax({
            type: "POST",
            url: "WebService_PurchaseOrder.asmx/GetLastTransactionDate",
            data: '{}',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (results) {
                var res = results.d.replace(/\\/g, '');
                res = res.replace(/"d":""/g, '');
                res = res.replace(/""/g, '');
                res = res.replace(/u0026/g, '&');
                res = res.substr(1);
                res = res.slice(0, -1);
                let customDate = res;

                // Initialize dxDateBox
                $("#VoucherDate").dxDateBox({
                    value: new Date().toISOString().substr(0, 10),
                    min: customDate, // Set minimum date to the parsed VoucherDate
                });
            }
        });
    } catch (e) {
        console.log(e.message);
    }
}

$("#BtnRefresh1").click(function () {
    GetDataGrid();
});

$("#PrintPOButton").click(function () {
    let insPOGridProcessSelData = ProcessedGridSelectData;
    VarItemApproved = insPOGridProcessSelData[0].IsVoucherItemApproved;

    let isApproved = String(VarItemApproved).toLowerCase() === "true";
    if (!isApproved) {
        alert("Please Approved P.O ");
        return;
    }

    var TxtPOID = document.getElementById("TxtPOID").value;
    var CreatePOGrid = $("#POGridProcess").dxDataGrid('instance');
    var CreatePOGridRow = CreatePOGrid.totalCount();
    var pageName = window.location.pathname.split("/").pop();
    try {
        $.ajax({
            async: false,
            type: "POST",
            url: "WebServiceOthers.asmx/GetReportViewerName",
            data: '{DocumentName:' + JSON.stringify(pageName) + '}',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                let pagename = (response.d === undefined || response.d === null || response.d === "") ? "ReportPurchaseOrder.aspx" : response.d;
                var url = pagename + "?TransactionID=" + TxtPOID + "&ItemGroupID=" + CreatePOGrid._options.dataSource[0].ItemGroupID;
                window.open(url, "blank", "location=yes,height=" + 1100 + ",width=" + window.innerWidth + ",scrollbars=yes,status=no", true);
            },
            error: function (error) {
                swal("Error", "Something went wrong. Please try again!", "error");
            }
        });
    } catch (e) {
        console.log(e);
    }
    //var url = "ReportPurchaseOrder.aspx?TransactionID=" + TxtPOID + "&ItemGroupID=" + CreatePOGrid._options.dataSource[0].ItemGroupID;
    //window.open(url, "blank", "location=yes,height=1100,width=1050,scrollbars=yes,status=no", true);
});



function restrictQuotes(event) {
    // Get the key code of the pressed key
    const key = event.key;
    // Prevent single quote (') and double quote (")
    if (key === "'" || key === '"') {
        event.preventDefault(); // Stop the key from being entered
    }
}


//$(document).ready(function () {
//    // Initially hide the text and set the button to just show the "+"
//    $("#BtnopenPop").val("+");

//    // When mouse enters the button
//    $("#BtnopenPop").hover(function () {
//        // Animate the button's width and padding even faster
//        $(this).animate({
//            width: "150px",  // Expand the button width
//            paddingLeft: "20px", // Adjust padding for space
//            paddingRight: "20px" // Adjust padding for space
//        }, 100, function () { // 100ms for lightning-fast animation
//            // After animation completes, change text to full button label
//            $(this).val("Add Item");  // Change the text to "Add Item"
//        });
//    }, function () {
//        // On mouse leave, revert back to the original size and text
//        $(this).animate({
//            width: "40px", // Reset width
//            paddingLeft: "10px", // Reset padding
//            paddingRight: "10px" // Reset padding
//        }, 100, function () { // 100ms for lightning-fast animation
//            // After animation completes, change text back to "+"
//            $(this).val("+");  // Set the text back to "+"
//        });
//    });
//});


$("#BtnopenPopOldPO").click(function () {
    var grid = $("#OldPOGrid").dxDataGrid('instance');
    grid.option('dataSource', []);

    var SelSupplierName = $('#SupplierName').dxSelectBox('instance').option('value');
    if (SelSupplierName !== "" && SelSupplierName !== null) {
        OldPOGrid();
    } else {
        swal("Warning", "Please select the supplier name first. Showing old purchase orders...", "warning")
        return true;
    }

    document.getElementById("BtnopenPopOldPO").setAttribute("data-toggle", "modal");
    document.getElementById("BtnopenPopOldPO").setAttribute("data-target", "#largeModalOldPO");
});

$("#OldPOGrid").dxDataGrid({
    dataSource: [],
    columnAutoWidth: true,
    showBorders: true,
    showRowLines: true,
    allowColumnReordering: true,
    allowColumnResizing: true,
    columnResizingMode: "widget",
    sorting: {
        mode: "multiple"
    },
    selection: { mode: "single" },
    paging: {
        pageSize: 50
    },
    pager: {
        showPageSizeSelector: true,
        allowedPageSizes: [50, 100, 500, 1000]
    },
    filterRow: { visible: true, applyFilter: "auto" },
    headerFilter: { visible: true },
    //rowAlternationEnabled: true,
    searchPanel: { visible: true },
    loadPanel: {
        enabled: true,
        text: 'Data is loading...'
    },
    export: {
        enabled: true,
        fileName: "Old Purchase Orders",
        allowExportSelectedData: true
    },
    onExporting(e) {
        const workbook = new ExcelJS.Workbook();
        const worksheet = workbook.addWorksheet('OldPurchaseOrders');
        DevExpress.excelExporter.exportDataGrid({
            component: e.component,
            worksheet,
            autoFilterEnabled: true,
        }).then(() => {
            workbook.xlsx.writeBuffer().then((buffer) => {
                saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'OldPurchaseOrders.xlsx');
            });
        });
        e.cancel = true;
    },
    onRowPrepared: function (e) {
        if (e.rowType === "header") {
            e.rowElement.css('background', '#509EBC');
            e.rowElement.css('color', 'white');
            e.rowElement.css('font-weight', 'bold');
        }
        e.rowElement.css('fontSize', '11px');

        if (e.rowType === "data") {
            if (e.data.VoucherItemApproved === false && e.data.VoucherCancelled === false) {
                e.rowElement.addClass('approvalpending');
            } else if (e.data.VoucherItemApproved === true) {
                e.rowElement.addClass('approvedorders');
            } else if (e.data.VoucherCancelled === true) {
                e.rowElement.addClass('cancelledorders');
            }
        }
    },
    onSelectionChanged: function (clickedCell) {

    },
    onRowDblClick: function (e) {
        let data = e.data;
    },
    columns: [
        { dataField: "LedgerName", visible: true, caption: "Supplier Name", width: 200 },
        { dataField: "VoucherNo", visible: true, caption: "P.O. No", width: 100 },
        { dataField: "VoucherDate", visible: true, caption: "P.O. Date", width: 120 },
        { dataField: "ItemCode", visible: true, caption: "Item Code", width: 120 },
        { dataField: "ItemGroupName", visible: true, caption: "Item Group", width: 120 },
        { dataField: "ItemSubGroupName", visible: true, caption: "Sub Group", width: 150 },
        { dataField: "ItemName", visible: true, caption: "Item Name", width: 300 },
        { dataField: "ItemDescription", visible: false, caption: "Item Description", width: 300 },
        { dataField: "PurchaseQuantity", visible: true, caption: "P.O. Qty", width: 100 },
        { dataField: "PurchaseUnit", visible: true, caption: "Unit", width: 80 },
        { dataField: "ExpectedDeliveryDate", visible: true, caption: "ExpectedDeliveryDate", width: 80 },
        { dataField: "PurchaseRate", visible: true, caption: "Rate", width: 80 },
        { dataField: "PendingToReceiveQty", visible: true, width: 100 },
        { dataField: "GrossAmount", visible: false, caption: "Gross Amount", width: 100 },
        { dataField: "DiscountAmount", visible: false, caption: "Disc. Amount", width: 100 },
        { dataField: "BasicAmount", visible: false, caption: "Basic Amount", width: 100 },
        { dataField: "GSTPercentage", visible: false, caption: "GST %", width: 80 },
        { dataField: "GSTTaxAmount", visible: false, caption: "GST Amount", width: 100 },
        { dataField: "NetAmount", visible: true, caption: "Net Amount", width: 100 },
        { dataField: "RefJobCardContentNo", visible: true, caption: "Ref.J.C.No.", width: 120 },
        { dataField: "CreatedBy", visible: true, caption: "Created By", width: 100 },
        { dataField: "ApprovedBy", visible: true, caption: "Approved By", width: 100 },
        { dataField: "ReceiptTransactionID", visible: false, caption: "ReceiptTransactionID", width: 120 },
        { dataField: "IsVoucherItemApproved", visible: false, caption: "IsVoucherItemApproved", width: 120 },
        { dataField: "IsReworked", visible: false, caption: "Is Reworked", width: 120 },
        { dataField: "ReworkRemark", visible: false, caption: "Rework Remark", width: 120 },
        { dataField: "PurchaseDivision", visible: true, caption: "Purchase Division", width: 120 },
        { dataField: "PurchaseReference", visible: true, caption: "Purchase Reference", width: 120 },
        { dataField: "Narration", visible: true, caption: "Narration", width: 120 },
        { dataField: "Details", visible: true, caption: "Update Remark", width: 120 },
        { dataField: "TaxableAmount", visible: false, caption: "TaxableAmount", width: 80 },
        { dataField: "ContactPersonID", visible: false, caption: "Contact PersonID", width: 120 },
        { dataField: "RequiredQuantity", visible: false, caption: "RequiredQuantity", width: 80 },
        { dataField: "TotalTaxAmount", visible: false, caption: "TotalTaxAmount", width: 80 },
        { dataField: "TotalOverheadAmount", visible: false, caption: "TotalOverheadAmount", width: 80 },
        { dataField: "DeliveryAddress", visible: false, caption: "DeliveryAddress", width: 120 },
        { dataField: "TotalQuantity", visible: false, caption: "TotalQuantity", width: 120 },
        { dataField: "TermsOfPayment", visible: false, caption: "TermsOfPayment", width: 120 },
        { dataField: "ModeOfTransport ", visible: false, caption: "ModeOfTransport", width: 120 },
        { dataField: "DealerID", visible: false, caption: "DealerID", width: 120 },
        { dataField: "VoucherItemApproved", visible: false, caption: "VoucherItemApproved", dataType: "boolean" },
        { dataField: "VoucherCancelled", visible: false, caption: "VoucherCancelled", dataType: "boolean" },
        { dataField: "CurrencyCode", visible: false, caption: "CurrencyCode" },
    ]
});

$("#BtnRefreshOldPO").click(function () {
    OldPOGrid();
});

function OldPOGrid() {
    let fromDateValue = new Date($("#FromDateOldPO").dxDateBox("instance").option("value"));
    fromDateValue = fromDateValue.toISOString().substr(0, 10);
    let ToDateValue = new Date($("#ToDateOldPO").dxDateBox("instance").option("value"));
    ToDateValue = ToDateValue.toISOString().substr(0, 10);
    var SelSupplierName = $('#SupplierName').dxSelectBox('instance').option('value');

    $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
    $.ajax({
        type: "POST",
        url: "WebService_PurchaseOrder.asmx/OldPOHistoryGrid",
        data: '{fromDateValue:' + JSON.stringify(fromDateValue) + ',ToDateValue:' + JSON.stringify(ToDateValue) + ',LedgerID:' + JSON.stringify(SelSupplierName) + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            var res = results.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/""/g, '');
            res = res.replace(/u0026/g, '&');
            res = res.replace(/u0027/g, "'");
            res = res.replace(/:,/g, ":null,");
            res = res.replace(/,}/g, ",null}");
            res = res.substr(1);
            res = res.slice(0, -1);
            $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
            let ProcessRESS = JSON.parse(res);
            $("#OldPOGrid").dxDataGrid({ dataSource: ProcessRESS });
        }
    });
}

var dataGrids = $("#MultipleFileAdds").dxDataGrid({
    dataSource: [],
    columnAutoWidth: true,
    showBorders: true,
    showRowLines: true,
    allowColumnReordering: true,
    allowColumnResizing: true,
    sorting: {
        mode: "none"
    },
    paging: {
        pageSize: 10
    },
    pager: {
        showPageSizeSelector: true,
    },
    loadPanel: {
        enabled: true,
        height: 90,
        width: 200,
        text: 'Data is loading...'
    },
    height: function () {
        return window.innerHeight / 3.2;
    },
    rowAlternationEnabled: true,
    selection: {
        mode: "multiple",
        showCheckBoxesMode: "always"
    },
    onRowPrepared: function (e) {
        if (e.rowType === "header") {
            e.rowElement.css('background', '#42909A');
            e.rowElement.css('color', 'white');
        }
        e.rowElement.css('fontSize', '11px');
    },
    onRowRemoved: function (e) {
        $('#previewImages').remove();
        $('#pdfPreviews').remove();
        $('#HalfPortions').hide();
    },
    editing: {
        mode: 'cell',
        allowDeleting: false,
        allowUpdating: true,
        onDeletedRow: function (e) {
            deleteRows(e.data);
        }
    },

    columns: [
        {
            dataField: "AttachmentFileID",
            visible: false,
            caption: "AttachmentFileID",
            alignment: 'Left',
            allowEditing: false
        },
        {
            type: "selection",
            width: 50,
            alignment: "center"
        },
        {
            dataField: "AttachedFileName",
            caption: "File Name",
            alignment: 'Left',
            allowEditing: false
        },
        {
            dataField: "AttachedFileRemark",
            caption: "Attached File Remark",
            alignment: 'left',
            allowEditing: true,

        },
        {
            dataField: "AttachedFileUrl",
            caption: "Preview",
            alignment: 'center',
            allowEditing: false,
            width: 100,
            cellTemplate: function (container, options) {
                $("<div>")
                    .addClass("ImagePreview-button")
                    .html('<i class="fa fa-eye" aria-hidden="true" style="width:15%; height:15%"></i>')
                    .attr('title', 'Click Me To Preview ')
                    .on("dxclick", function (e) {
                        openModals(options.data);
                    })
                    .appendTo(container);
            }
        },
        {
            dataField: "AttachedFileUrl",
            caption: "Delete",
            alignment: 'center',
            allowEditing: false,
            width: 100,
            cellTemplate: function (container, options) {
                $("<div>")
                    .addClass("Delete-button")
                    .html('<i class="fa fa-trash" aria-hidden="true" style="width:10%; height:10%;color:red"></i>')
                    .attr('title', 'Click Me To Delete ')
                    .on("dxclick", function (e) {
                        deleteRows(options.data);
                    })
                    .appendTo(container);
            }
        }
    ],
    onToolbarPreparing: function (e) {
        var toolbarItems = e.toolbarOptions.items;
        toolbarItems.push({
            widget: "dxButton",
            options: {
                icon: "download",
                text: "Download Selected Files",
                onClick: function () {
                    downloadSelectedFiles();
                }
            },
            location: "after"
        });
    }
}).dxDataGrid("instance");
function downloadSelectedFiles() {
    var selectedData = dataGrids.getSelectedRowsData();

    if (selectedData.length === 0) {
        alert("Please select at least one file to download.");
        return;
    }

    selectedData.forEach(function (rowData) {
        var link = document.createElement("a");
        link.href = rowData.AttachedFileUrl;
        link.download = rowData.AttachedFileName; // Optional: specify the download file name
        link.click();
    });
}
$("#FileAttachments").on('change', function () {
    var fileInput = $(this)[0];
    var files = fileInput.files;

    for (var i = 0; i < files.length; i++) {
        var file = files[i];
        var Matched = false;
        var reader = new FileReader();
        var data = dataGrids.option('dataSource');
        for (var j = 0; j < data.length; j++) {
            if (file.name == data[j].AttachedFileName) {
                Matched = true;
            }
        }
        if (Matched == true) {
            continue;
        }
        reader.onload = (function (file) {
            return function (e) {
                var fileUrl = e.target.result;
                var fileName = file.name;
                var dataSource = dataGrids.option('dataSource');

                dataSource.push({ AttachedFileName: fileName, AttachedFileUrl: fileUrl });

                dataGrids.option('dataSource', dataSource);
            };
        })(file);

        reader.readAsDataURL(file);
    }
    document.getElementById("FileAttachments").value = "";
});
function openModals(rowData) {
    var file = rowData.AttachedFileName;
    var fileUrl = rowData.AttachedFileUrl;
    var fileExtension = file.split('.').pop().toLowerCase();
    var fileName = '';

    if (fileUrl.includes('base64')) {
        fileName = fileUrl;
    } else {
        fileName = 'Files/POAttchmentFile/' + file;
    }

    const imageExtensions = ['jpg', 'jpeg', 'png', 'gif'];
    const isImage = imageExtensions.includes(fileExtension);
    const isPdf = fileExtension === 'pdf';

    var modalContent = '';
    if (isImage) {
        modalContent = '<img id="previewImages" src="' + fileName + '" alt="Image Preview" style="width: 350px; height: 28vh;">';
    } else if (isPdf) {
        modalContent = '<iframe id="pdfPreviews" src="' + fileName + '" style="width: 350px; height: 28vh;"></iframe>';
    }
    $('#HalfPortions').html(modalContent);
    $('#HalfPortions').show();
}
function deleteRows(rowData) {
    var dataSource = dataGrids.option('dataSource');
    var index = dataSource.indexOf(rowData);

    if (index !== -1) {
        dataSource.splice(index, 1);
        dataGrids.option('dataSource', dataSource);
    }

    dataGrids.refresh();

    if ($('#previewImages').length) {
        $('#previewImages').remove();
    }
    if ($('#pdfPreviews').length) {
        $('#pdfPreviews').remove();
    }
    if (!$('#previewImages').length && !$('#pdfPreviews').length) {
        $('#HalfPortions').hide();
        document.getElementById("FileAttachments").value = "";
    }

}
 
function LoadFileData() {
    var Grid = $('#POGridProcess').dxDataGrid('instance').getSelectedRowsData();
    var Grid = $("#POGridProcess").dxDataGrid("instance");
    var selectedRows = Grid.getSelectedRowsData();

    if (selectedRows.length > 0 && selectedRows[0].TransactionID) {
        $.ajax({
            type: 'POST',
            url: "WebService_PurchaseOrder.asmx/GetFiledata",
            data: '{TransactionID :' + JSON.stringify(selectedRows[0].TransactionID) + '}',
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            success: function (results) {
                let res1 = results.d.replace(/\\/g, '');
                res1 = res1.replace(/"d":""/g, '');
                res1 = res1.replace(/""/g, '');
                res1 = res1.replace(/u0027/g, "'");
                res1 = res1.replace(/u0026/g, "&");
                res1 = res1.replace(/:,/g, ":null,");
                res1 = res1.replace(/,}/g, ",null}");
                res1 = res1.replace(/:}/g, ":null}");
                res1 = res1.substr(1);
                res1 = res1.slice(0, -1);

                var RES1 = JSON.parse(res1);
                $("#MultipleFileAdds").dxDataGrid({
                    dataSource: RES1,
                });
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.error("Error fetching file data: ", textStatus, errorThrown);
            }
        });
    }
}
