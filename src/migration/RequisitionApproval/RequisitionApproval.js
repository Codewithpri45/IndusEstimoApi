"use strict";

var optionrequisitions = ["Unapproved Requisitions", "Approved Requisitions", "Cancelled Requisitions"];
var Groupdata = [];
var GblSubGridData = [];
var GBLProductionUnitID = getProductionUnitID('ProductionUnitID');
var GBLCompanyID = getProductionUnitID('CompanyID');
var SelectedProductionUnitID = 0;
var SelectedProductionUnitName = null;
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

    if (queryString["ReqTransactionID"] !== null && queryString["ReqTransactionID"] !== undefined) {
        let tReqTransactionID = Number(queryString["ReqTransactionID"]);
        if (tReqTransactionID !== 0 && tReqTransactionID !== null && tReqTransactionID !== undefined && tReqTransactionID !== 0) {
            var urlParams = new URLSearchParams(window.location.search);
            Groupdata = JSON.parse(urlParams.get('ReqSelectedData'));
            if (Groupdata.length > 0) {
                document.getElementById("ReqDashboardDiv").style.display = "none";
                $("#DetailsReqButton").click();
            } else {
                Groupdata = [];
                document.getElementById("ReqDashboardDiv").style.display = "block";
            }
        }
    };
});

$("#FromDate").dxDateBox({
    pickerType: "calendar",
    displayFormat: 'dd-MMM-yyyy',
    value: new Date(new Date().setDate(new Date().getDate() - 15)).toISOString().substr(0, 10)
});

$("#ToDate").dxDateBox({
    pickerType: "calendar",
    displayFormat: 'dd-MMM-yyyy',
    value: new Date().toISOString().substr(0, 10)
});

$("#opt-approval-radio").dxRadioGroup({
    items: optionrequisitions,
    value: optionrequisitions[0],
    layout: "horizontal"
});

$("#LoadIndicator").dxLoadPanel({
    shadingColor: "rgba(0,0,0,0.4)",
    indicatorSrc: "images/Indus logo.png",
    message: 'Please Wait...',
    width: 310,
    showPane: true,
    shading: true,
    closeOnOutsideClick: false,
    visible: false,
});

var requisitionfilename = "Unapproved Requisitions";
var serviceUrl = "WebServiceRequisitionApproval.asmx/UnApprovedRequisitions";
$(function () {
    $("#opt-approval-radio").dxRadioGroup({
        onValueChanged: function (e) {
            var previousValue = e.previousValue;
            var newValue = e.value;
            Groupdata = [];
            if (e.value === 'Unapproved Requisitions') {
                serviceUrl = "WebServiceRequisitionApproval.asmx/UnApprovedRequisitions";
                requisitionfilename = "Unapproved Requisitions";
                $('#Btn_Update').text("Approve");
                $('#Btn_Cancel').text("Cancel");
            } else if (e.value === 'Cancelled Requisitions') {
                serviceUrl = "WebServiceRequisitionApproval.asmx/CancelledRequisitions";
                requisitionfilename = "Cancelled Requisitions";
                $('#Btn_Update').text("Approve");
                $('#Btn_Cancel').text("UnCancel");
            } else {
                serviceUrl = "WebServiceRequisitionApproval.asmx/ApprovedRequisitions";
                requisitionfilename = "Approved Requisitions";
                $('#Btn_Update').text("UnApprove");
                $('#Btn_Cancel').text("Cancel");
            }
            RefreshRequisitions();
        }
    });
});


$("#RefreshBtn").click(function () {
    RefreshRequisitions();
});
RefreshRequisitions();
function RefreshRequisitions() {
    try {
        var FromDate = $('#FromDate').dxDateBox('instance').option('value');
        var ToDate = $('#ToDate').dxDateBox('instance').option('value');

        //document.getElementById("LOADER").style.display = "block";
        $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
        //alert("hello");
        $.ajax({
            type: "POST",
            url: serviceUrl,
            data: '{FromDate:' + JSON.stringify(FromDate) + ',ToDate: ' + JSON.stringify(ToDate) + '}',
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            success: function (results) {
                var res = results.replace(/\\/g, '');
                res = res.replace(/"d":""/g, '');
                res = res.replace(/""/g, '');
                res = res.replace(/u0027/g, "'");
                res = res.replace(/u0026/g, "&");
                res = res.substr(1);
                res = res.slice(0, -1);
                // document.getElementById("LOADER").style.display = "none";
                $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                var tt = JSON.parse(res);

                FillGrid(JSON.parse(res));
                var gridInstance = $("#grid-requisitions").dxDataGrid('instance');
                var btntext = $("#opt-approval-radio").dxRadioGroup('instance');
                if (btntext.option('value') === "Unapproved Requisitions") {
                    gridInstance.columnOption("ApprovedBy", "visible", false);
                    gridInstance.columnOption("ApprovalDate", "visible", false);
                } else if (btntext.option('value') === "Cancelled Requisitions") {
                    gridInstance.columnOption("ApprovedBy", "visible", false);
                    gridInstance.columnOption("ApprovalDate", "visible", false);
                } else {
                    gridInstance.columnOption("ApprovedBy", "visible", true);
                    gridInstance.columnOption("ApprovalDate", "visible", true);
                    gridInstance.columnOption("PurchaseTransactionID", "visible", false);
                }
            }
        });

    } catch (e) {
        alert(e);
    }
}

function FillGrid(requisitionData) {
    $("#grid-requisitions").dxDataGrid({
        dataSource: requisitionData,
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
            pageSize: 25
        },
        pager: {
            showPageSizeSelector: true,
            allowedPageSizes: [25, 35, 50, 100]
        },
        // scrolling: { mode: 'virtual' },
        filterRow: { visible: true, applyFilter: "auto" },
        columnChooser: { enabled: true },
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
            fileName: requisitionfilename,
            allowExportSelectedData: true
        },
        onExporting(e) {
            const workbook = new ExcelJS.Workbook();
            const worksheet = workbook.addWorksheet(requisitionfilename);

            DevExpress.excelExporter.exportDataGrid({
                component: e.component,
                worksheet,
                autoFilterEnabled: true,
            }).then(() => {
                workbook.xlsx.writeBuffer().then((buffer) => {
                    saveAs(new Blob([buffer], { type: 'application/octet-stream' }), `${requisitionfilename}.xlsx`);
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
                if (e.data.PurchaseTransactionID > 0) {
                    e.rowElement.css('background', '#ff0c0c');
                }
            }
        },
        //onCellClick: function (e) {
        //    //var grid = $('#ProcessMasterGrid').dxDataGrid('instance');
        //    if (e.row === undefined || e.rowType === "filter" || e.rowType === "header") return false;
        //    var Row = e.row.rowIndex;
        //    var Col = e.columnIndex;
        //    //document.getElementById("ProtxtGetGridRow").value = "";
        //    //document.getElementById("ProtxtGetGridRow").value = Row;
        //    //document.getElementById("txtProcessID").value = "";
        //    //document.getElementById("txtProcessID").value = e.row.data.ProcessID;/// grid.cellValue(Row, 0);
        //},
        //editing: {
        //    mode: "cell",
        //    allowUpdating: true
        //},
        //onEditingStart: function (e) {
        //    if (e.column.visibleIndex > 1) {
        //        e.cancel = true;
        //    }
        //},
        onSelectionChanged: function (selectedItem) {
            var Radiobtnval = $("#opt-approval-radio").dxRadioGroup('instance').option('value');
            if (selectedItem.selectedRowsData.length > 0) {
                SelectedProductionUnitID = 0;
                SelectedProductionUnitName = null;
                SelectedProductionUnitID = selectedItem.selectedRowsData[0].ProductionUnitID;
                SelectedProductionUnitName = selectedItem.selectedRowsData[0].ProductionUnitName;

                if (GBLCompanyID != selectedItem.currentSelectedRowKeys[0].CompanyID) {
                    selectedItem.component.deselectRows((selectedItem || {}).currentSelectedRowKeys[0]);
                    swal({
                        title: "Access Denied!",
                        text: "You cannot select transactions related to different companies.",
                        button: "OK",
                    });
                    selectedItem.currentSelectedRowKeys = [];
                    SelectedProductionUnitID = 0;
                    SelectedProductionUnitName = null;
                    return false;
                } else if (Radiobtnval === "Approved Requisitions" || Radiobtnval === "Cancelled Requisitions") {
                    if (selectedItem.currentSelectedRowKeys[0] !== undefined) {
                        if (selectedItem.currentSelectedRowKeys[0].PurchaseTransactionID > 0) {
                            //DevExpress.ui.notify("This requisition has been used in further transactions, Can't Unapproved..!", "warning", 2000);
                            showDevExpressNotification("This requisition has been used in further transactions, Can't Unapproved..!", "warning");
                            selectedItem.component.deselectRows((selectedItem || {}).currentSelectedRowKeys[0]);
                            selectedItem.currentSelectedRowKeys = [];
                        }
                        else {
                            Groupdata = selectedItem.selectedRowsData;
                        }
                    }
                }
                else {
                    Groupdata = selectedItem.selectedRowsData;
                }
            } else {
                SelectedProductionUnitID = 0;
                SelectedProductionUnitName = null;
                Groupdata = [];
            }
        },
        onEditorPreparing: function (e) {
            if (e.parentType === 'headerRow' && e.command === 'select') {
                e.editorElement.remove();
            }
        },
        columns: [
            //{ dataField: "Sel", visible: true, caption: "Select", dataType: "boolean", width: 120 },
            { dataField: "TransactionID", visible: false, caption: "Transaction ID", width: 120 },
            { dataField: "VoucherID", visible: false, caption: "Voucher ID", width: 120 },
            { dataField: "MaxVoucherNo", visible: false, caption: "Ref.Requisition No.", width: 120 },
            { dataField: "ItemGroupID", visible: false, caption: "Item Group ID", width: 120 },
            { dataField: "ItemID", visible: false, caption: "Item ID", width: 120 },
            { dataField: "VoucherNo", visible: true, caption: "Requisition No.", width: 120, groupIndex: 0 }, { dataField: "VoucherNo", visible: true, caption: "Requisition No.", width: 120, fixed: true },
            { dataField: "VoucherDate", visible: true, caption: "Requisition Date", width: 120, fixed: true },
            { dataField: "ItemCode", visible: true, caption: "Item Code", width: 80 },
            { dataField: "ItemGroupName", visible: true, caption: "Item Group", width: 120 },
            { dataField: "ItemSubGroupName", visible: true, caption: "Sub Group", width: 120 },
            { dataField: "ItemName", visible: true, caption: "Item Name", width: 300 },
            { dataField: "RefJobCardContentNo", visible: true, caption: "Ref. J.C. No.", width: 120 },
            { dataField: "RequiredQuantity", visible: true, caption: "Requisition Quantity", width: 150 },
            { dataField: "StockUnit", visible: true, caption: "UOM", width: 120 },
            { dataField: "ExpectedDeliveryDate", visible: true, caption: "Expect.Date", width: 100 },
            { dataField: "ItemNarration", visible: true, caption: "Item Remark", width: 200 },
            { dataField: "TotalItems", visible: false, caption: "Total Items", width: 120 },
            { dataField: "TotalQuantity", visible: false, caption: "Total Quantity", width: 120 },
            { dataField: "FYear", visible: false, caption: "F Year", width: 120 },
            { dataField: "CreatedBy", visible: true, caption: "Created By", width: 120 },
            { dataField: "ApprovedBy", visible: true, caption: "Approved By", width: 120 },
            { dataField: "ApprovalDate", visible: true, caption: "Approval Date", width: 120 },
            { dataField: "PurchaseTransactionID", visible: false, caption: "PurchaseTransactionID", width: 120 },
            { dataField: "ProductionUnitID", visible: false, caption: "ProductionUnitID", width: 100 },
            { dataField: "ProductionUnitName", visible: true, caption: "Production Unit Name", width: 100 },
            { dataField: "CompanyName", visible: true, caption: "Company Name", width: 100 }
            // { dataField: "ChargeApplyOnSheets", visible: true, caption: "ChargeApplyOnSheets", width: 120 },
        ]
    });
}

$("#Btn_Update").click(function () {
    var btnstatus = document.getElementById("Btn_Update").innerText.trim();
    UpdateData(btnstatus);
});

$("#Btn_Cancel").click(function () {
    var btnstatus = document.getElementById("Btn_Cancel").innerText.trim();
    UpdateData(btnstatus);

});

function UpdateData(btnstatus) {
    var jsonObjectsRecordDetail = [];
    var OperationRecordDetail = {};
    if (Groupdata.length > 0) {
        for (var e = 0; e < Groupdata.length; e++) {
            OperationRecordDetail = {};
            OperationRecordDetail.TransactionID = Groupdata[e].TransactionID;
            //OperationRecordDetail.ItemID = Groupdata[e].ItemID;
            OperationRecordDetail.FYear = Groupdata[e].FYear;

            jsonObjectsRecordDetail.push(OperationRecordDetail);
        }


        var txt = 'If you confident please click on \n' + 'Yes, Save it ! \n' + 'otherwise click on \n' + 'Cancel';
        swal({
            title: "Do you want to continue",
            text: txt,
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "Yes, Save it !",
            closeOnConfirm: true
        },
            function () {

                document.getElementById("LOADER").style.display = "block";
                $.ajax({
                    type: "POST",
                    url: "WebServiceRequisitionApproval.asmx/UpdateData",
                    data: '{BtnText:' + JSON.stringify(btnstatus) + ',jsonObjectsRecordDetail:' + JSON.stringify(jsonObjectsRecordDetail) + '}',
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function (results) {
                        var res = JSON.stringify(results);
                        res = res.replace(/"d":/g, '');
                        res = res.replace(/{/g, '');
                        res = res.replace(/}/g, '');
                        res = res.substr(1);
                        res = res.slice(0, -1);

                        document.getElementById("LOADER").style.display = "none";
                        if (res === "Success") {
                            swal({
                                title: "Updated!",
                                text: "Your data has been saved successfully.",
                                type: "success"
                            }, function (isConfirm) {
                                location.reload();
                            });
                        } else {
                            swal.close();
                            setTimeout(() => {
                                swal("Warning..!", res, "warning");
                            }, 100);
                        }
                    },
                    error: function errorFunc(jqXHR) {
                        document.getElementById("LOADER").style.display = "none";
                        swal("Error!", "Please try after some time..", "error");
                    }
                });
            });
    } else {
        //DevExpress.ui.notify("Please choose data from given above Grid..!", "error", 1000);
        showDevExpressNotification("Please choose data from given above Grid..!", "error");
        return false;
    }
}

//////////////////////////////////////////////////////////////////DetAIL PART ////////////////////////////////////////
$("#DetailsReqButton").click(function () {
    if (Groupdata.length <= 0) {
        //DevExpress.ui.notify("Please select any requisition voucher to view..!", "warning", 1500);
        showDevExpressNotification("Please select any requisition voucher to view..!", "warning");
        return false;
    }

    var TxtPRID = Groupdata[0].TransactionID;

    document.getElementById("TxtVoucherNo").value = Groupdata[0].VoucherNo;
    document.getElementById("textNaretion").value = Groupdata[0].Narration;
    $("#VoucherDate").dxDateBox({ value: Groupdata[0].VoucherDate });

    var existReq = [];
    var ProcessRetrive = [];
    try {
        $.ajax({
            type: "POST",
            url: "WebService_PurchaseRequisition.asmx/RetriveRequisitionData",
            data: '{TransactionID:' + JSON.stringify(TxtPRID) + '}',
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            success: function (results) {
                var res = results.replace(/\\/g, '');
                res = res.replace(/"d":""/g, '');
                res = res.replace(/""/g, '');
                res = res.substr(1);
                res = res.slice(0, -1);
                ProcessRetrive = JSON.parse(res);
                var ObjRetriveArray = {};
                for (var x = 0; x < ProcessRetrive.length; x++) {
                    var result = $.grep(existReq, function (e) { return (e.ItemID === ProcessRetrive[x].RequisitionItemID); });
                    if (result.length === 0) {
                        ObjRetriveArray = {};
                        ObjRetriveArray.TransactionID = ProcessRetrive[x].RequisitionTransactionID;
                        ObjRetriveArray.ItemID = ProcessRetrive[x].RequisitionItemID;
                        ObjRetriveArray.ItemGroupID = ProcessRetrive[x].ItemGroupID;
                        ObjRetriveArray.ItemGroupNameID = ProcessRetrive[x].ItemGroupNameID;
                        ObjRetriveArray.ItemCode = ProcessRetrive[x].RequisitionItemCode;
                        ObjRetriveArray.ItemGroupName = ProcessRetrive[x].ItemGroupName;
                        ObjRetriveArray.ItemSubGroupName = ProcessRetrive[x].ItemSubGroupName;
                        ObjRetriveArray.ItemName = ProcessRetrive[x].RequisitionItemName;
                        ObjRetriveArray.RefJobCardContentNo = ProcessRetrive[x].RefJobCardContentNo;
                        ObjRetriveArray.RefJobBookingJobCardContentsID = ProcessRetrive[x].RefJobBookingJobCardContentsID;
                        ObjRetriveArray.RequisitionQty = ProcessRetrive[x].TotalRequisitionQty;
                        ObjRetriveArray.BookedStock = ProcessRetrive[x].RequisitionBookedStock;
                        ObjRetriveArray.AllocatedStock = ProcessRetrive[x].RequisitionAllocatedStock;
                        ObjRetriveArray.PhysicalStock = ProcessRetrive[x].RequisitionPhysicalStock;
                        ObjRetriveArray.StockUnit = ProcessRetrive[x].StockUnit;
                        ObjRetriveArray.OrderUnit = ProcessRetrive[x].OrderUnit;
                        ObjRetriveArray.PurchaseQty = Number(ProcessRetrive[x].PurchaseQty);
                        ObjRetriveArray.ExpectedDeliveryDate = ProcessRetrive[x].ExpectedDeliveryDate;
                        ObjRetriveArray.ItemNarration = ProcessRetrive[x].ItemNarration;
                        ObjRetriveArray.UnitDecimalPlace = ProcessRetrive[x].UnitDecimalPlace;
                        ObjRetriveArray.VoucherItemApproved = ProcessRetrive[x].VoucherItemApproved;
                        ObjRetriveArray.PhysicalStockInPurchaseUnit = ProcessRetrive[x].RequisitionPhysicalStockInPurchaseUnit;
                        ObjRetriveArray.PurchaseUnit = ProcessRetrive[x].PurchaseUnit;
                        ObjRetriveArray.LastPurchaseDate = ProcessRetrive[x].LastPurchaseDate;
                        existReq.push(ObjRetriveArray);
                    }
                }

                GblSubGridData = ProcessRetrive;

                $("#CreateReqGrid").dxDataGrid({
                    dataSource: existReq
                });
            }
        });
    } catch (e) {
        console.log(e);
    }

    $('#largeModalReqView').modal({
        show: 'true'
    });
});


var gridInstance = $("#CreateReqGrid").dxDataGrid({
    allowColumnResizing: true,
    columnResizingMode: "widget",
    keyExpr: "ItemID",
    showBorders: true,
    showRowLines: true,
    allowSorting: false,
    wordWrapEnabled: true,
    sorting: {
        mode: "none" // or "multiple" | "single"
    },
    height: function () {
        return window.innerHeight / 1.4;
    },
    loadPanel: {
        enabled: true,
        height: 90,
        width: 200,
        text: 'Data is loading...'
    },
    onRowPrepared: function (e) {
        if (e.rowType === "header") {
            e.rowElement.css('background', '#509EBC');
            e.rowElement.css('color', 'white');
            e.rowElement.css('font-weight', 'bold');
        }
        e.rowElement.css('fontSize', '11px');
    },
    columns: [
        { dataField: "TransactionID", visible: false, caption: "TransactionID", width: 100 },
        { dataField: "ItemID", visible: false, caption: "Item ID", width: 100 },
        { dataField: "ItemGroupID", visible: false, caption: "ItemGroupID", width: 100 },
        { dataField: "ItemGroupNameID", visible: false, caption: "ItemGroupNameID", width: 100 },
        { dataField: "ItemCode", visible: true, caption: "Item Code", width: 60 },
        { dataField: "ItemGroupName", visible: true, caption: "Item Group", width: 80 },
        { dataField: "ItemSubGroupName", visible: true, caption: "Sub Group", width: 100 },
        { dataField: "ItemName", visible: true, caption: "Item Name", width: 250 },
        { dataField: "RequisitionQty", visible: true, caption: "Indent Qty", width: 80 },
        { dataField: "BookedStock", visible: true, caption: "Total Booked", width: 80 },
        { dataField: "AllocatedStock", visible: true, caption: "Allocated Stock", width: 80 },
        { dataField: "PhysicalStock", visible: true, caption: "Current Stock", width: 80 },
        { dataField: "StockUnit", visible: true, caption: "Stock Unit", width: 80 },
        { dataField: "PhysicalStockInPurchaseUnit", visible: true, caption: "Current Stock (In P.U.)", width: 100 },
        { dataField: "PurchaseUnit", visible: true, caption: "Purchase Unit", width: 100 },
        {
            dataField: "OrderUnit", visible: true, caption: "Order Unit", width: 80,
            lookup: {
                dataSource: [{ OrderUnit: "Kg" }, { OrderUnit: "Sheet" }, { OrderUnit: "Meter" }],
                displayExpr: "OrderUnit",
                valueExpr: "OrderUnit"
            }
        },
        { dataField: "PurchaseQty", visible: true, dataType: "numeric", validationRules: [{ type: "required" }, { type: "numeric" }], caption: "Purchase Qty", width: 100 },
        { dataField: "ExpectedDeliveryDate", visible: true, caption: "Expec.Del.Date", width: 120, dataType: "date", format: 'dd-MMM-yyyy', validationRules: [{ type: "required" }], showEditorAlways: true },
        { dataField: "ItemNarration", visible: true, caption: "Item Remark", width: 120 },
        { dataField: "RefJobCardContentNo", visible: true, caption: "Ref. J.C. No.", width: 200 },
        { dataField: "RefJobBookingJobCardContentsID", visible: false },
        { dataField: "LastPurchaseDate", visible: true, caption: "Last P.O.Date", dataType: "date", format: 'dd-MMM-yyyy', width: 100 },
        { dataField: "UnitDecimalPlace", visible: false, caption: "UnitDecimalPlace", width: 120 },
        { dataField: "VoucherItemApproved", visible: false, caption: "VoucherItemApproved", width: 120 },
        { dataField: "AuditApprovalRequired", visible: true, caption: "Audit Required", dataType: "boolean", width: 120 }
    ],
    summary: {
        totalItems: [{
            format: "currency",
            showInColumn: "PurchaseQty",
            column: "PurchaseQty",
            summaryType: "sum",
            displayFormat: "{0}",
            alignByColumn: true
        }]
    },
    masterDetail: {
        enabled: true,
        template: function (container, options) {
            var currentEmployeeData = options.data;
            $("<div id='requisitionsubgrid'>")
                .dxDataGrid({
                    columnAutoWidth: true,
                    allowColumnResizing: true,
                    allowSorting: false,
                    sorting: {
                        mode: "none" // or "multiple" | "single"
                    },
                    showBorders: true,
                    columns: [{ dataField: "TransactionID", visible: false },
                    { dataField: "ItemID", visible: false },
                    { dataField: "RequisitionItemID", visible: false },
                    { dataField: "ItemGroupID", visible: false },
                    { dataField: "ItemGroupNameID", visible: false },
                    { dataField: "VoucherNo", caption: "Indent No." },
                    { dataField: "VoucherDate", caption: "Indent Date", dataType: "date", format: "dd-MMM-yyyy" },
                    { dataField: "ItemGroupName", caption: "Item Group" },
                    { dataField: "ItemSubGroupName", caption: "Sub Group" },
                    { dataField: "ItemCode", caption: "Item Code" },
                    { dataField: "ItemName", caption: "Item Name" },
                    { dataField: "JobCardNo", caption: "Job Card No." },
                    { dataField: "RequisitionQty", caption: "Indent Quantity" },
                    { dataField: "BookedStock", caption: "Total Booked" },
                    { dataField: "AllocatedStock", caption: "Allocated Stock" },
                    { dataField: "PhysicalStock", caption: "Current Stock" },
                    { dataField: "StockUnit", caption: "Stock Unit" },
                    { dataField: "OrderUnit", caption: "Order Unit", visible: false },
                    { dataField: "PurchaseQty", caption: "Purchase Qty", visible: false },
                    { dataField: "ExpectedDeliveryDate", dataType: "date", visible: false },
                    { dataField: "ItemNarration", caption: "Remark", visible: false }],
                    dataSource: new DevExpress.data.DataSource({
                        store: new DevExpress.data.ArrayStore({
                            key: "ItemID",
                            data: GblSubGridData
                        }),
                        filter: [
                            ["RequisitionItemID", "=", options.key],
                            "and",
                            ["RequisitionQty", ">", 0]
                        ]
                    })
                }).appendTo(container);
        }
    }

}).dxDataGrid("instance");

$("#VoucherDate").dxDateBox({
    pickerType: "calendar",
    displayFormat: 'dd-MMM-yyyy',
    value: new Date().toISOString().substr(0, 10)
});
