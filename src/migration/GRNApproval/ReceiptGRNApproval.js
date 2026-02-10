
var GblStatus = "";
var GetPendingData = "";
var GBLProductionUnitID = getProductionUnitID('ProductionUnitID');
var SelectedProductionUnitID = 0;
var SelectedProductionUnitName = null;
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
$.ajax({
    type: "POST",
    url: "WebServicePurchaseGRN.asmx/GetReceiverList",
    data: '{}',
    contentType: "application/json; charset=utf-8",
    dataType: "text",
    success: function (results) {
        var res = results.replace(/\\/g, '');
        res = res.replace(/"d":""/g, '');
        res = res.replace(/""/g, '');
        res = res.replace(/:,/g, ":null,");
        res = res.replace(/,}/g, ",null}");
        res = res.substr(1);
        res = res.slice(0, -1);
        var receivers = JSON.parse(res);

        $("#sel_Receiver").dxSelectBox({
            items: receivers,
            placeholder: "Select Received By",
            displayExpr: 'LedgerName',
            valueExpr: 'LedgerID',
            searchEnabled: true,
            showClearButton: true,
            disabled: true,
            onValueChanged: function (data) {
                //alert(data.value);
            }
        });
    }
});

$("#PendingReceiptGRNGrid").dxDataGrid({
    //dataSource: PendingList,
    columnAutoWidth: true,
    showBorders: true,
    showRowLines: true,
    allowColumnReordering: true,
    allowColumnResizing: true,
    columnResizingMode: "widget",
    sorting: {
        mode: "multiple"
    },
    height: function () {
        return window.innerHeight / 1.2;
    },
    //selection: { mode: "multiple", showCheckBoxesMode: "always", },
    selection: { mode: "single" },
    paging: {
        pageSize: 15
    },
    pager: {
        showPageSizeSelector: true,
        allowedPageSizes: [15, 25, 50, 100]
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
        fileName: RadioValue,
        allowExportSelectedData: true
    },
    onExporting(e) {
        const workbook = new ExcelJS.Workbook();
        const worksheet = workbook.addWorksheet(RadioValue);

        DevExpress.excelExporter.exportDataGrid({
            component: e.component,
            worksheet,
            autoFilterEnabled: true,
        }).then(() => {
            workbook.xlsx.writeBuffer().then((buffer) => {
                saveAs(new Blob([buffer], { type: 'application/octet-stream' }), `${RadioValue}.xlsx`);
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
    onSelectionChanged: function (clickedIndentCell) {
        GetPendingData = clickedIndentCell.selectedRowsData;

    },
    columns: [
        { dataField: "TransactionID", visible: false, caption: "Transaction ID", width: 120 },
        { dataField: "PurchaseTransactionID", visible: false, caption: "PurchaseTransactionID", width: 120 },
        { dataField: "LedgerID", visible: false, caption: "LedgerID", width: 120 },
        { dataField: "MaxVoucherNo", visible: true, caption: "Ref.No.", width: 80 },
        { dataField: "LedgerName", visible: true, caption: "Supplier Name", width: 180 },
        { dataField: "ReceiptVoucherNo", visible: true, caption: "Receipt Note No.", width: 120 },
        { dataField: "ReceiptVoucherDate", visible: true, caption: "Receipt Note Date", width: 140 },
        { dataField: "PurchaseVoucherNo", visible: true, caption: "P.O. No.", width: 100 },
        { dataField: "PurchaseVoucherDate", visible: true, caption: "P.O. Date", width: 100 },
        { dataField: "DeliveryNoteNo", visible: true, caption: "D.N. No.", width: 100 },
        { dataField: "DeliveryNoteDate", visible: true, caption: "D.N. Date", width: 100 },
        { dataField: "GateEntryNo", visible: false, caption: "Gate Entry No.", width: 80 },
        { dataField: "GateEntryDate", visible: false, caption: "Gate Entry Date", width: 60 },
        { dataField: "LRNoVehicleNo", visible: false, caption: "LR No./Vehicle No.", width: 60 },
        { dataField: "Transporter", visible: true, caption: "Transporter", width: 100 },
        { dataField: "ReceiverName", visible: true, caption: "Received By", width: 100 },
        { dataField: "CreatedBy", visible: true, caption: "Created By", width: 100 },
        { dataField: "ApprovedBy", visible: true, caption: "Approved By", width: 100 },
        { dataField: "ApprovalDate", visible: true, caption: "Approval Date", width: 100 },
        { dataField: "Narration", visible: true, caption: "Remark", width: 200 },
        { dataField: "ProductionUnitId", visible: false, caption: "ProductionUnitId", width: 200 },
        { dataField: "ProductionUnitName", visible: true, caption: "ProductionUnitName", width: 120 },
        { dataField: "CompanyName", visible: true, caption: "CompanyName", width: 120 }
    ]
});

$("#GridPurchaseGRNApproval").dxDataGrid({
    columnAutoWidth: true,
    showRowLines: true,
    allowSorting: false,
    allowColumnResizing: true,
    allowColumnReordering: true,
    columnResizingMode: "widget",
    showBorders: true,
    height: function () {
        return window.innerHeight / 3;
    },
    sorting: {
        mode: "none"
    },
    //selection: { mode: "multiple", showCheckBoxesMode: "always", },
    selection: { mode: "single" },
    paging: {
        pageSize: 15
    },
    pager: {
        showPageSizeSelector: true,
        allowedPageSizes: [15, 25, 50, 100]
    },
    // scrolling: { mode: 'virtual' },
    filterRow: { visible: false, applyFilter: "auto" },
    //columnChooser: { enabled: true },
    headerFilter: { visible: false },
    //rowAlternationEnabled: true,
    //searchPanel: { visible: true },
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
    editing: {
        mode: "cell",
        allowUpdating: true
    },
    onEditingStart: function (e) {
        //if (e.column.dataField === "ApprovedQuantity" || e.column.dataField === "QCApprovalNO" || e.column.dataField === "QCApprovedNarration") {
        //    e.cancel = false;
        //    //return false;
        //}
        //else {
        //    e.cancel = true;
        //}
        //alert(e.column.visibleIndex);
        //if (e.column.visibleIndex = 0 || e.column.visibleIndex === 1 || e.column.visibleIndex === 2 || e.column.visibleIndex === 3 || e.column.visibleIndex === 4 || e.column.visibleIndex === 5 || e.column.visibleIndex === 6 || e.column.visibleIndex === 7 || e.column.visibleIndex === 8 || e.column.visibleIndex === 9 || e.column.visibleIndex === 10 || e.column.visibleIndex === 11 || e.column.visibleIndex === 13 || e.column.visibleIndex === 16 || e.column.visibleIndex === 17 || e.column.visibleIndex === 18 || e.column.visibleIndex === 19 || e.column.visibleIndex === 20 || e.column.visibleIndex === 21 || e.column.visibleIndex === 22 || e.column.visibleIndex === 23 || e.column.visibleIndex === 24 || e.column.visibleIndex === 25 || e.column.visibleIndex === 26 || e.column.visibleIndex === 27 || e.column.visibleIndex === 28) {
        //    e.cancel = true;
        //}
        if (e.column.visibleIndex === 14 || e.column.visibleIndex === 15 || e.column.visibleIndex === 16 || e.column.visibleIndex === 17) {
            e.cancel = false;
        } else {
            e.cancel = true;
        }
    },

    onRowUpdated: function (e) {
        if (e.data.ApprovedQuantity !== undefined) {
            if (Number(e.data.ApprovedQuantity) > Number(e.key.ChallanQuantity)) {
                e.data.ApprovedQuantity = 0;
            } else if (Number(e.data.ApprovedQuantity) < 0) {
                e.data.ApprovedQuantity = 0;
            }
            e.key.RejectedQuantity = Number(e.key.ChallanQuantity) - Number(e.data.ApprovedQuantity);
        } else if (e.data.RejectedQuantity !== undefined) {
            if (Number(e.data.RejectedQuantity) > Number(e.key.ChallanQuantity)) {
                e.data.RejectedQuantity = 0;
            } else if (Number(e.data.RejectedQuantity) < 0) {
                e.data.RejectedQuantity = 0;
            }
            e.key.ApprovedQuantity = Number(e.key.ChallanQuantity) - Number(e.data.RejectedQuantity);
        }
        //var dataGrid = $('#GridPurchaseGRNApproval').dxDataGrid('instance');
        //var ChallanQuantity = 0, ApprQty = 0;
        //var decimal = /^[0-9]+\.?[0-9]*$/;
        //for (var xx = 0; xx < dataGrid.totalCount() ; xx++) {
        //if (ApprQty === "" || ApprQty === undefined || ApprQty === null) {
        //    ApprQty = 0;
        //}           
        //var finalQty = 0;
        //if (decimal.test(dataGrid._options.dataSource[xx].ApprovedQuantity) === true) {
        //ChallanQuantity = Number(dataGrid._options.dataSource[xx].ChallanQuantity);
        //ApprQty = dataGrid._options.dataSource[xx].ApprovedQuantity;
        //if (ChallanQuantity === "" || ChallanQuantity === undefined || ChallanQuantity === null) {
        //    ChallanQuantity = 0;
        //}
        //finalQty = Number(ChallanQuantity) - Number(ApprQty);
        //dataGrid.cellValue(xx, "RejectedQuantity", Number(finalQty));
        //}
        //else {
        //    //dataGrid.cellValue(xx, "ApprovedQuantity", "");
        //    DevExpress.ui.notify("Please eneter Numeric Or Decimal value..!", "error", 1000);
        //    return false;
        //}

        //if (ReqQty < PerQty) {
        //    dataGrid.cellValue(xx, "PurchaseQuantity", ReqQty)
        //    DevExpress.ui.notify("Purchase Quantity should not be greater then Pending Quantity..!", "error", 1000);
        //    return false;
        //}           

        //}
    },

    columns: [
        { dataField: "PurchaseTransactionID", visible: false, caption: "PurchaseTransactionID" },
        { dataField: "LedgerID", visible: false, caption: "LedgerID" },
        { dataField: "ItemID", visible: false, caption: "ItemID" },
        { dataField: "ItemGroupID", visible: false, caption: "ItemGroupID" },
        { dataField: "ItemDescription", visible: false, caption: "ItemDescription", width: 80 },
        { dataField: "PurchaseVoucherNo", visible: true, caption: "P.O. No.", width: 100 },
        { dataField: "PurchaseVoucherDate", visible: true, caption: "P.O. Date", width: 100 },
        { dataField: "ItemCode", visible: true, caption: "Item Code", width: 80 },
        { dataField: "ItemGroupName", visible: true, caption: "Item Group", width: 100 },
        { dataField: "ItemSubGroupName", visible: true, caption: "Sub Group", width: 100 },
        { dataField: "ItemName", visible: true, caption: "Item Name", width: 300 },
        { dataField: "PurchaseOrderQuantity", visible: true, caption: "P.O. Qty", width: 80 },
        { dataField: "PurchaseUnit", visible: true, caption: "Purchase Unit", width: 100 },
        { dataField: "ChallanQuantity", caption: "Receipt Qty", visible: true, width: 100 },
        { dataField: "ApprovedQuantity", caption: "Appr.Qty", visible: true, width: 80, validationRules: [{ type: "required" }, { type: "numeric" }] },//12,
        { dataField: "RejectedQuantity", caption: "Reject Qty", visible: true, width: 80, validationRules: [{ type: "required" }, { type: "numeric" }] },
        {
            dataField: "QCApprovalNO", caption: "COA NO", visible: true, width: 80,
            setCellValue: function (newData, value, currentRowData) {
                if (value.trim() === undefined || value.trim() === null || value.trim() === '') {
                    newData.QCApprovalNO = ''
                } else {
                    newData.QCApprovalNO = value.trim();
                }
            }
        },//14
        { dataField: "QCApprovedNarration", caption: "Remark", visible: true, width: 100 },//15
        { dataField: "BatchNo", visible: true, width: 140 },
        { dataField: "StockUnit", visible: true, width: 80 },
        { dataField: "ReceiptWtPerPacking", caption: "Wt/Packing" },
        { dataField: "Warehouse", visible: true, caption: "Warehouse", width: 120 },
        { dataField: "Bin", visible: true, width: 120 },
        { dataField: "PurchaseTolerance", visible: true, caption: "P.O. Tol.(%)", width: 80 },
        { dataField: "WtPerPacking", visible: false, caption: "WtPerPacking", width: 80 },
        { dataField: "UnitPerPacking", visible: false, caption: "UnitPerPacking" },
        { dataField: "ConversionFactor", visible: false, caption: "ConversionFactor" },
        { dataField: "SizeW", visible: false, caption: "SizeW", width: 80 },
        { dataField: "WarehouseID", visible: false, caption: "WarehouseID" },
        { dataField: "ItemGroupNameID", visible: false, caption: "ItemGroupNameID" },
        { dataField: "TransactionID", visible: false, caption: "TransactionID" }
    ]
});

var priorities = ["Pending Receipt Note", "Approved Receipt Note"];
$("#RadioButtonPGRN").dxRadioGroup({
    items: priorities,
    value: priorities[0],
    layout: 'horizontal',
    onValueChanged: function (e) {
        GblStatus = "";
        RadioValue = e.value;
        GetDataGrid();
    }
});

var RadioValue = "Pending Receipt Note";
GetDataGrid();


$("#RefreshBtn").click(function () {
    GetDataGrid();
});
function GetDataGrid() {
    var FromDate = $('#FromDate').dxDateBox('instance').option('value');
    var ToDate = $('#ToDate').dxDateBox('instance').option('value');

    if (RadioValue === "Pending Receipt Note") {
        document.getElementById("POPrintButton").disabled = true;
        document.getElementById("BtnTransporterSlip").disabled = true;
        //document.getElementById("TxtGRNID").value = "";
        $('#BtnPermit').text("Approve");
        $.ajax({
            type: "POST",
            url: "WebService_ReceiptGRNApproval.asmx/FillGrid",
            data: '{RadioValue:' + JSON.stringify(RadioValue) + ',FromDate:' + JSON.stringify(FromDate) + ',ToDate: ' + JSON.stringify(ToDate) + '}',
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            success: function (results) {
                ////console.debug(results);
                var res = results.replace(/\\/g, '');
                res = res.replace(/"d":""/g, '');
                res = res.replace(/""/g, '');
                res = res.replace(/:,/g, ":null,");
                res = res.replace(/,}/g, ",null}");
                res = res.substr(1);
                res = res.slice(0, -1);
                var PendingList = JSON.parse(res);
                document.getElementById("LOADER").style.display = "none";
                fillGridPending(PendingList);
                var gridInstance = $("#PendingReceiptGRNGrid").dxDataGrid('instance');
                gridInstance.columnOption("ApprovedBy", "visible", false);
                gridInstance.columnOption("ApprovalDate", "visible", false);
            }
        });
    }

    else if (RadioValue === "Approved Receipt Note") {
        $('#BtnPermit').text("UnApprove");
        document.getElementById("POPrintButton").disabled = false;
        document.getElementById("BtnTransporterSlip").disabled = false;
        $.ajax({
            type: "POST",
            url: "WebService_ReceiptGRNApproval.asmx/FillGrid",
            data: '{RadioValue:' + JSON.stringify(RadioValue) + ',FromDate:' + JSON.stringify(FromDate) + ',ToDate: ' + JSON.stringify(ToDate) + '}',
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            success: function (results) {
                var res = results.replace(/\\/g, '');
                res = res.replace(/"d":""/g, '');
                res = res.replace(/""/g, '');
                res = res.replace(/u0026/g, '&');
                res = res.replace(/u0027/g, "'");
                res = res.replace(/:,/g, ':null,');
                res = res.replace(/,}/g, ",null}");
                res = res.substr(1);
                res = res.slice(0, -1);
                var PendingList = JSON.parse(res);
                document.getElementById("LOADER").style.display = "none";
                fillGridPending(PendingList);

                var gridInstance = $("#PendingReceiptGRNGrid").dxDataGrid('instance');
                gridInstance.columnOption("ApprovedBy", "visible", true);
                gridInstance.columnOption("ApprovalDate", "visible", true);
            }
        });

    }
}

function fillGridPending(PendingList) {
    $("#PendingReceiptGRNGrid").dxDataGrid({
        dataSource: PendingList
    });
}

$("#NextButton").click(function () {
    // var TxtPOID = document.getElementById("TxtPOID").value;

    if (GetPendingData === "" || GetPendingData === null || GetPendingData === undefined || GetPendingData === []) {
        alert("Please select any receipt note voucher from the list..!");
        return false;
    }
    var dataGrid;
    TransactionID = 0;
    receiptBatchDetail = [];
    SelectedProductionUnitName = null;
    SelectedProductionUnitID = 0;
    document.getElementById("TxtVoucherNo").value = '';
    document.getElementById("TxtMaxVoucherNo").value = '';
    document.getElementById("TxtSupplierName").value = '';
    document.getElementById("TxtSupplierID").value = '';
    document.getElementById("TxtDnNo").value = '';
    document.getElementById("TxtGENo").value = '';
    document.getElementById("TxtLRNo").value = '';
    document.getElementById("TxtTransporters").value = '';
    document.getElementById("TxtNarration").value = '';
    $("#sel_Receiver").dxSelectBox({
        value: ''
    });
    $("#DtPickerVoucherDate").dxDateBox({
        value: new Date().toISOString().substr(0, 10),
        disabled: true,
        displayFormat: 'dd-MMM-yyyy'
    });
    $("#DtPickerDnDate").dxDateBox({
        value: new Date().toISOString().substr(0, 10),
        disabled: true,
        displayFormat: 'dd-MMM-yyyy'
    });
    $("#DtPickerGEDate").dxDateBox({
        value: new Date().toISOString().substr(0, 10),
        disabled: true,
        displayFormat: 'dd-MMM-yyyy'
    });

    $("#GridPurchaseGRNApproval").dxDataGrid({
        dataSource: []
    });

    if (GetPendingData !== "") {
        TransactionID = GetPendingData[0].TransactionID;
        SelectedProductionUnitID = GetPendingData[0].ProductionUnitID;
        SelectedProductionUnitName = GetPendingData[0].ProductionUnitName;
        document.getElementById("TxtSupplierName").value = GetPendingData[0].LedgerName;
        document.getElementById("TxtSupplierID").value = GetPendingData[0].LedgerID;
        document.getElementById("TxtVoucherNo").value = GetPendingData[0].ReceiptVoucherNo;
        document.getElementById("TxtMaxVoucherNo").value = GetPendingData[0].MaxVoucherNo;
        document.getElementById("TxtDnNo").value = GetPendingData[0].DeliveryNoteNo;
        document.getElementById("TxtGENo").value = GetPendingData[0].GateEntryNo;
        document.getElementById("TxtLRNo").value = GetPendingData[0].LRNoVehicleNo;
        document.getElementById("TxtTransporters").value = GetPendingData[0].Transporter;
        document.getElementById("TxtNarration").value = GetPendingData[0].Narration;
        $("#sel_Receiver").dxSelectBox({
            value: GetPendingData[0].ReceivedBy,
            disabled: true
        });
        $("#DtPickerVoucherDate").dxDateBox({
            value: GetPendingData[0].ReceiptVoucherDate,
            disabled: true
        });
        $("#DtPickerDnDate").dxDateBox({
            value: GetPendingData[0].DeliveryNoteDate,
            disabled: true
        });
        $("#DtPickerGEDate").dxDateBox({
            value: GetPendingData[0].GateEntryDate,
            disabled: true
        });
    }



    $.ajax({
        type: "POST",
        url: "WebService_ReceiptGRNApproval.asmx/GetReceiptVoucherBatchDetail",
        data: '{TransactionID:' + JSON.stringify(GetPendingData[0].TransactionID) + ',RadioValue:' + JSON.stringify(RadioValue) + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            var res = results.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/""/g, '');
            res = res.replace(/:,/g, ":null,");
            res = res.replace(/,}/g, ",null}");
            res = res.substr(1);
            res = res.slice(0, -1);
            var RES1 = JSON.parse(res);

            $("#GridPurchaseGRNApproval").dxDataGrid({
                dataSource: RES1
            });
        }
    });

    //document.getElementById("NextButton").setAttribute("data-toggle", "modal");
    //document.getElementById("NextButton").setAttribute("data-target", "#largeModal");
    $('#largeModal').modal({
        show: 'true'
    });
});

$("#BtnPermit").click(function () {
    var jsonObjectsRecordGRNApproval = [];
    var OperationRecordGRNApproval = {};

    var GridPurchaseGRNApproval = $('#GridPurchaseGRNApproval').dxDataGrid('instance');
    var GridPurchaseGRNApprovalRow = GridPurchaseGRNApproval._options.dataSource.length;


    if (Number(SelectedProductionUnitID) != 0) {
        if (GBLProductionUnitID != SelectedProductionUnitID) {
            swal("Attention!", "Selected transaction is related to unit " + SelectedProductionUnitName + ", Kindly login with unit " + SelectedProductionUnitName + " to process.", "warning");
            $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
            return;
        }
    }
    for (var e = 0; e < GridPurchaseGRNApprovalRow; e++) {
        var AppQty = GridPurchaseGRNApproval._options.dataSource[e].ApprovedQuantity;
        var AppNo = GridPurchaseGRNApproval._options.dataSource[e].QCApprovalNO;
        if (RadioValue === "Approved Receipt Note") {
            AppNo = "";
        } else {
            if (AppQty === "" || AppQty === null || AppQty === undefined) {
                //DevExpress.ui.notify("Please enter approved quantity in above Grid..!", "warning", 1000);
                showDevExpressNotification("Please enter approved quantity in above Grid..!", "warning");
                return false;
            } else if (AppNo === "" || AppNo === null || AppNo === undefined) {
                //DevExpress.ui.notify("Please enter QC Approval No. in above Grid..!", "warning", 1000);
                showDevExpressNotification("Please enter QC Approval No. in above Grid..!", "warning");
                return false;
            }
        }

        OperationRecordGRNApproval = {};

        OperationRecordGRNApproval.TransactionID = GridPurchaseGRNApproval._options.dataSource[e].TransactionID;
        OperationRecordGRNApproval.ItemID = GridPurchaseGRNApproval._options.dataSource[e].ItemID;
        //  OperationRecordGRNApproval.PurchaseVoucherNo = GridPurchaseGRNApproval._options.dataSource[e].PurchaseVoucherNo;
        OperationRecordGRNApproval.BatchNo = GridPurchaseGRNApproval._options.dataSource[e].BatchNo;
        OperationRecordGRNApproval.ReceiptQuantity = GridPurchaseGRNApproval._options.dataSource[e].ChallanQuantity;
        OperationRecordGRNApproval.RejectedQuantity = GridPurchaseGRNApproval._options.dataSource[e].RejectedQuantity;
        OperationRecordGRNApproval.ApprovedQuantity = GridPurchaseGRNApproval._options.dataSource[e].ApprovedQuantity;
        OperationRecordGRNApproval.QCApprovalNO = AppNo;
        OperationRecordGRNApproval.QCApprovedNarration = GridPurchaseGRNApproval._options.dataSource[e].QCApprovedNarration;
        if (OperationRecordGRNApproval.QCApprovalNO === "" && RadioValue === "Approved Receipt Note") {
            OperationRecordGRNApproval.IsVoucherItemApproved = 0;
        } else
            OperationRecordGRNApproval.IsVoucherItemApproved = 1;

        jsonObjectsRecordGRNApproval.push(OperationRecordGRNApproval);
    }

    if (GridPurchaseGRNApprovalRow > 0) {
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
                    url: "WebService_ReceiptGRNApproval.asmx/UpdateGRNApproval",
                    data: '{jsonObjectsRecordGRNApproval:' + JSON.stringify(jsonObjectsRecordGRNApproval) + ',GRNTransactionID:' + JSON.stringify(jsonObjectsRecordGRNApproval[0].TransactionID) + ',RadioButtonValue:' + JSON.stringify(RadioValue) + '}',
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
                            swal("Updated..!", "Your data updated..", "success");
                            location.reload();
                        } else if (res === "Exist") {
                            swal("Can't Update", "This item is used in another process..! Record can not be delete.", "warning");
                        } else {
                            swal.close();
                            setTimeout(() => {
                                swal("Warning..!", res, "warning");
                            }, 100);

                        }
                    },
                    error: function errorFunc(jqXHR) {
                        document.getElementById("LOADER").style.display = "none";
                        swal("Error!", "Please try after some time..", "");
                    }
                });
            });
    }

});

$("#POPrintButton").click(function () {
    //var url = "PrintReceiptApproval.aspx?TI=" + TransactionID;
    var url = "ReportPurchaseGRN.aspx?TransactionID=" + TransactionID;
    window.open(url, "blank", "location=yes,height=1100,width=1050,scrollbars=yes,status=no", true);
});

$("#BtnTransporterSlip").click(function () {
    //var url = "PrintReceiptApproval.aspx?TI=" + TransactionID;
    var url = "ReportGRNTransportSlip.aspx?TransactionID=" + TransactionID;
    window.open(url, "blank", "location=yes,height=1100,width=1050,scrollbars=yes,status=no", true);
});


$("#BtnNotification").click(function () {
    var receiptId = "";
    var commentData = "";
    var newHtml = '';
    if (GetPendingData === "" || GetPendingData === null || GetPendingData === undefined || GetPendingData === []) {
        alert("Please select any receipt note voucher from the list..!");
        return false;
    }
    if (GetPendingData !== "" && GetPendingData.length > 0) {
        receiptId = GetPendingData[0].TransactionID;
    }
    if (GetPendingData.length > 0) {
        document.getElementById("commentbody").innerHTML = "";
        if (receiptId !== "") {
            $.ajax({
                type: "POST",
                url: "WebServicePurchaseGRN.asmx/GetCommentData",
                data: '{receiptTransactionID:' + JSON.stringify(receiptId) + ',purchaseTransactionIDs:0}',
                contentType: "application/json; charset=utf-8",
                dataType: "text",
                success: function (results) {
                    var res = results.replace(/\\/g, '');
                    res = res.replace(/"d":""/g, '');
                    res = res.replace(/""/g, '');
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
        var receiptId = "";
        if (GetPendingData === "" || GetPendingData === null || GetPendingData === undefined || GetPendingData === []) {
            alert("Please select valid receipt note voucher from the list..!");
            return false;
        }
        if (GetPendingData !== "" && GetPendingData.length > 0) {
            receiptId = GetPendingData[0].TransactionID;
        }

        if (receiptId === "" || receiptId === null || receiptId === undefined) {
            alert("Please select valid receipt note to view comment details..!");
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
        objectCommentDetail.ModuleName = "GOODS RECEIPT NOTE QC APPROVAL";
        objectCommentDetail.CommentTitle = commentTitle;
        objectCommentDetail.CommentDescription = commentDesc;
        objectCommentDetail.CommentType = commentType;
        objectCommentDetail.TransactionID = receiptId;

        jsonObjectCommentDetail.push(objectCommentDetail);
        jsonObjectCommentDetail = JSON.stringify(jsonObjectCommentDetail);
        $.ajax({
            type: "POST",
            url: "WebServicePurchaseGRN.asmx/SaveCommentData",
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

                document.getElementById("commentbody").innerHTML = "";
                if (receiptId !== "") {
                    $.ajax({
                        type: "POST",
                        url: "WebServicePurchaseGRN.asmx/GetCommentData",
                        data: '{receiptTransactionID:' + JSON.stringify(receiptId) + ',purchaseTransactionIDs:0}',
                        contentType: "application/json; charset=utf-8",
                        dataType: "text",
                        success: function (results) {
                            var res = results.replace(/\\/g, '');
                            res = res.replace(/"d":""/g, '');
                            res = res.replace(/""/g, '');
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