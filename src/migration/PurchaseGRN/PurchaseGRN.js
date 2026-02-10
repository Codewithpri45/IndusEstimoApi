let optionreceiptoptions = ["Purchase Orders", "Created Receipt Notes"];
var GBLProductionUnitID = getProductionUnitID('ProductionUnitID');
var SelectedProductionUnitID = 0;
var SelectedProductionUnitName = null;
let voucherPrefix = "REC";
let GetPendingData = "";
let GetSelectedListData = [];
let receiptBatchDetail = [];
let TransactionID = 0;

var PurchaseTransactionID = 0;
var GBLItemID = 0;
let flag = false;
let receiptgridrow = "";
let purchaseorderrow = "";
let FlagEdit = false;
let ResWarehouse = [], ResBin = [];
let GridReceiptBatchDetails = [];
let supplieridvalidate = "";
let ProductionUnitIDvalidate = 0;
let GblReceiptNo = "";
let ApprovedVoucher = false;
let GblUnitConversionFormula = [];
let newData = [];
let GateEntryTransactionID = 0;
let itemArr = [];
let batchRowIndex = 0;
let validateUserData = { moduleName: "", userName: "", password: "", actionType: "Update", RecordID: 0, transactionRemark: "", isUserInfoFilled: false, documentNo: "" };
var currentDate = new Date();
var GBLCompanyID = getProductionUnitID('CompanyID');
// Subtract 7 days from the current date
var sevenDaysAgo = new Date(currentDate);
sevenDaysAgo.setDate(currentDate.getDate() - 7);

// Set the dxDateBox value to the date 7 days ago
$("#FromDate").dxDateBox({
    dataType: "date",
    displayFormat: 'dd-MMM-yyyy',
    value: sevenDaysAgo
});


$("#ToDate").dxDateBox({
    dataType: "date",
    displayFormat: 'dd-MMM-yyyy',
    value: new Date().toISOString().substr(0, 10)
});

$("#optreceiptradio").dxRadioGroup({
    items: optionreceiptoptions,
    value: optionreceiptoptions[0],
    layout: "horizontal"
});

$("#DtPickerDnDate").dxDateBox({
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
            showDevExpressNotification("The selected date cannot be greater than today’s date...!", "warning");
            // Optionally reset the DateBox value to clear the invalid input
            $("#DtPickerDnDate").dxDateBox({
                value: new Date().toISOString().substr(0, 10),
            });
            return;
        } else {
            let voucherDate = $("#DtPickerVoucherDate").dxDateBox('instance').option('value');
            let voucherDateVal = new Date(voucherDate);
            voucherDateVal.setHours(0, 0, 0, 0);

            if (dateBoxDate > voucherDateVal) {
                //DevExpress.ui.notify("The receipt note date cannot be less than delivery note/ inovice date.", "warning", 1500);
                showDevExpressNotification("The receipt note date cannot be less than delivery note/ inovice date...!", "warning");
                // Optionally reset the DateBox value to clear the invalid input
                $("#DtPickerDnDate").dxDateBox({
                    value: new Date(voucherDate).toISOString().substr(0, 10),
                });
                return;
            }
            let geDate = $("#DtPickerGEDate").dxDateBox('instance').option('value');
            let geDateVal = new Date(geDate);
            geDateVal.setHours(0, 0, 0, 0);

            if (dateBoxDate > geDateVal) {
                //DevExpress.ui.notify("The delivery note date cannot be less than gate entry date.", "warning", 1500);
                showDevExpressNotification("The delivery note date cannot be less than gate entry date..!", "warning");
                // Optionally reset the DateBox value to clear the invalid input
                $("#DtPickerDnDate").dxDateBox({
                    value: new Date(geDate).toISOString().substr(0, 10),
                });
                return;
            }
        }
    }
});

$("#DtPickerEwayDate").dxDateBox({
    pickerType: 'calendar',
    type: 'date',
    displayFormat: 'dd-MMM-yyyy',
    valueExpr: 'value',
    value: new Date().toISOString().substr(0, 10),
    acceptCustomValue: false
});

$("#BiltyDate").dxDateBox({
    pickerType: 'calendar',
    type: 'date',
    displayFormat: 'dd-MMM-yyyy',
    valueExpr: 'value',
    value: new Date().toISOString().substr(0, 10),
    acceptCustomValue: false
});

$("#DtPickerGEDate").dxDateBox({
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
            showDevExpressNotification("The selected date cannot be greater than today’s date.!", "warning");
            // Optionally reset the DateBox value to clear the invalid input
            $("#DtPickerGEDate").dxDateBox({
                value: new Date().toISOString().substr(0, 10),
            });
            return;
        } else {
            let dnDate = $("#DtPickerDnDate").dxDateBox('instance').option('value');
            let dnDateVal = new Date(dnDate);
            dnDateVal.setHours(0, 0, 0, 0);

            if (dateBoxDate < dnDateVal) {
                //DevExpress.ui.notify("The gate entry date cannot be less than delivery note / invoice date.", "warning", 1500);
                showDevExpressNotification("The gate entry date cannot be less than delivery note / invoice date..!", "warning");
                // Optionally reset the DateBox value to clear the invalid input
                $("#DtPickerGEDate").dxDateBox({
                    value: new Date().toISOString().substr(0, 10),
                });
                return;
            }
            let voucherDate = $("#DtPickerVoucherDate").dxDateBox('instance').option('value');
            let voucherDateVal = new Date(voucherDate);
            voucherDateVal.setHours(0, 0, 0, 0);

            if (dateBoxDate > voucherDateVal) {
                //DevExpress.ui.notify("The gate entry date cannot be greater than receipt note date.", "warning", 1500);
                showDevExpressNotification("The gate entry date cannot be greater than receipt note date...!", "warning");
                // Optionally reset the DateBox value to clear the invalid input
                $("#DtPickerGEDate").dxDateBox({
                    value: new Date(voucherDate).toISOString().substr(0, 10),
                });
                return;
            }
        }
    }
});

$("#DtPickerVoucherDate").dxDateBox({
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
            showDevExpressNotification("The selected date cannot be greater than today’s date..!", "warning");
            // Optionally reset the DateBox value to clear the invalid input
            $("#DtPickerVoucherDate").dxDateBox({
                value: new Date().toISOString().substr(0, 10),
            });
            return;
        } else {
            let gateEntryDate = $("#DtPickerGEDate").dxDateBox('instance').option('value');
            let gateEntryDateVal = new Date(gateEntryDate);
            gateEntryDateVal.setHours(0, 0, 0, 0);

            if (dateBoxDate < gateEntryDateVal) {
                //DevExpress.ui.notify("The receipt note date cannot be less than gate entry date.", "warning", 1500);
                showDevExpressNotification("The receipt note date cannot be less than gate entry date..!", "warning");
                // Optionally reset the DateBox value to clear the invalid input
                $("#DtPickerVoucherDate").dxDateBox({
                    value: new Date().toISOString().substr(0, 10),
                });
                return;
            }

            let dnDate = $("#DtPickerDnDate").dxDateBox('instance').option('value');
            let dnDateVal = new Date(dnDate);
            dnDateVal.setHours(0, 0, 0, 0);

            if (dateBoxDate < dnDateVal) {
                //DevExpress.ui.notify("The receipt note date cannot be less than delivery note / invoice date.", "warning", 1500);
                showDevExpressNotification("The receipt note date cannot be less than delivery note / invoice date..!", "warning");
                // Optionally reset the DateBox value to clear the invalid input
                $("#DtPickerVoucherDate").dxDateBox({
                    value: new Date().toISOString().substr(0, 10),
                });
                return;
            }

        }
    }
});

$("#sel_Receiver").dxSelectBox({
    items: [],
    placeholder: "Select Received By",
    displayExpr: 'LedgerName',
    valueExpr: 'LedgerID',
    searchEnabled: true,
    showClearButton: true
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

$("#SelWarehouse").dxSelectBox({
    items: [],
    placeholder: "Select Warehouse",
    displayExpr: 'Warehouse',
    valueExpr: 'Warehouse',
    searchEnabled: true,
    showClearButton: true,
    onValueChanged: function (data) {
        let warehouseName = data.value;
        if (warehouseName !== null && warehouseName !== undefined) {
            RefreshBin(warehouseName);
            if (ResBin.length > 0) {
                $("#SelBin").dxSelectBox({
                    items: ResBin,
                });
                if (ResBin.length == 1) { $("#SelBin").dxSelectBox({ value: ResBin[0].WarehouseID }); }
            }
        } else {
            $("#SelBin").dxSelectBox({
                items: [],
                value: 0
            });
        }
    }
});

$("#SelBin").dxSelectBox({
    items: [],
    placeholder: "Select Bin",
    displayExpr: 'Bin',
    valueExpr: 'WarehouseID',
    searchEnabled: true,
    showClearButton: true
});

$(function () {
    $("#optreceiptradio").dxRadioGroup({
        onValueChanged: function (e) {
            var previousValue = e.previousValue;
            var newValue = e.value;
            document.getElementById("LOADER").style.display = "block";
            if (e.value === 'Purchase Orders') {
                document.getElementById("dateFilter").style.display = "none";
                PendingReceiptData();
                GetPendingData = "";
                document.getElementById('btnUploadGRNData').disabled = false;
            } else {
                document.getElementById('btnUploadGRNData').disabled = true;
                document.getElementById("dateFilter").style.display = "block";
                TransactionID = 0;
                ReceiptNotesVouchers();
            }
            var grid1 = $("#gridreceiptlist").dxDataGrid('instance');
            grid1.clearSelection();
            GetSelectedListData = [];
            supplieridvalidate = "";
            ProductionUnitIDvalidate = 0;
        }
    });
});

function CreateReceiptNo() {
    try {
        $.ajax({
            type: "POST",
            url: "WebServicePurchaseGRN.asmx/GetReceiptNo",
            data: '{prefix:' + JSON.stringify(voucherPrefix) + '}',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (results) {
                var res = JSON.stringify(results);
                res = res.replace(/"d":/g, '');
                res = res.replace(/{/g, '');
                res = res.replace(/}/g, '');
                res = res.substr(1);
                res = res.slice(0, -1);
                GblReceiptNo = "";
                if (res !== "") {
                    GblReceiptNo = res;
                    document.getElementById("TxtVoucherNo").value = GblReceiptNo;
                }
            }
        });
    }
    catch (e) {
        console.log(e);
    }
}
function setLastTransactiondate() {
    try {
        $.ajax({
            type: "POST",
            url: "WebServicePurchaseGRN.asmx/GetLastTransactionDate",
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
                $("#DtPickerVoucherDate").dxDateBox({
                    value: new Date().toISOString().substr(0, 10),
                    min: customDate, // Set minimum date to the parsed VoucherDate
                });
            }
        });
    } catch (e) {
        console.log(e.message);
    }
}

$("#Btn_Next").click(function () {
    var TxtGRNID = document.getElementById("TxtGRNID").value;
    ApprovedVoucher = false;
    if (TxtGRNID === "" || TxtGRNID === null || TxtGRNID === undefined) {
        alert("Please select any row from below grid !");
        return false;
    }
    var dataGrid;

    TransactionID = 0;
    receiptBatchDetail = [];
    document.getElementById("TxtVoucherNo").value = '';
    document.getElementById("TxtMaxVoucherNo").value = '';
    document.getElementById("TxtSupplierName").value = '';
    document.getElementById("TxtSupplierID").value = '';
    document.getElementById("TxtDnNo").value = '';
    document.getElementById("TxtEwayBillNo").value = '';
    document.getElementById("TxtGENo").value = '';
    document.getElementById("TxtLRNo").value = '';
    document.getElementById("TxtTransporters").value = '';
    document.getElementById("TxtNarration").value = '';
    document.getElementById("BiltyNo").value = '';
    $("#sel_Receiver").dxSelectBox({ value: null });
    $("#DtPickerVoucherDate").dxDateBox({ value: new Date().toISOString().substr(0, 10) });
    $("#DtPickerDnDate").dxDateBox({ value: new Date().toISOString().substr(0, 10) });
    $("#DtPickerEwayDate").dxDateBox({ value: new Date().toISOString().substr(0, 10) });
    $("#DtPickerGEDate").dxDateBox({ value: new Date().toISOString().substr(0, 10) });
    $("#BiltyDate").dxDateBox({ value: new Date().toISOString().substr(0, 10) });
    $("#GridPurchaseOrders").dxDataGrid({ dataSource: [] });
    $("#GridReceiptBatchDetails").dxDataGrid({ dataSource: [] });
    var optradiovalue = $("#optreceiptradio").dxRadioGroup("instance");
    if (optradiovalue.option("value").toUpperCase().trim() === "PURCHASE ORDERS") {
        document.getElementById("BtnPrint").disabled = true;
        document.getElementById("BtnTransporterSlip").disabled = true;
        dataGrid = $("#gridreceiptlist").dxDataGrid("instance");
        if (dataGrid._options.dataSource.length <= 0) {
            //DevExpress.ui.notify("No items present in the list to create receipt note..!", "warning", 1200);
            showDevExpressNotification("No items present in the list to create receipt note..!", "warning");
            return;
        }
        //Set Purchase Detail and batch detail grid
        document.getElementById("LOADER").style.display = "block";
        if (GetSelectedListData.length <= 0) {
            //DevExpress.ui.notify("Please select pending purchase orders to create new receipt note..!", "warning", 1200);
            showDevExpressNotification("Please select pending purchase orders to create new receipt note..!", "warning");
            return;
        }
        setLastTransactiondate();
        //document.getElementById("BtnDeletePopUp").disabled = true;
        GetPendingData = [];
        DefaultRowData = [];

        var OptObj = {};
        var i = 0;
        for (i = 0; i < GetSelectedListData.length; i++) {
            //if (dataGrid._options.dataSource[i].Sel === "true" || dataGrid._options.dataSource[i].Sel === "1") {
            OptObj.TransactionID = GetSelectedListData[i].TransactionID;
            document.getElementById("TxtSupplierID").value = GetSelectedListData[i].LedgerID;
            document.getElementById("TxtSupplierName").value = GetSelectedListData[i].LedgerName;
            OptObj.LedgerID = GetSelectedListData[i].LedgerID;
            OptObj.ItemID = GetSelectedListData[i].ItemID;
            OptObj.ItemGroupID = GetSelectedListData[i].ItemGroupID;
            OptObj.ItemSubGroupID = GetSelectedListData[i].ItemSubGroupID;
            OptObj.ItemGroupNameID = GetSelectedListData[i].ItemGroupNameID;
            OptObj.PurchaseVoucherNo = GetSelectedListData[i].PurchaseVoucherNo;
            OptObj.PurchaseVoucherDate = GetSelectedListData[i].PurchaseVoucherDate;
            OptObj.ItemCode = GetSelectedListData[i].ItemCode;
            OptObj.ItemName = GetSelectedListData[i].ItemName;
            OptObj.ItemSubGroupName = GetSelectedListData[i].ItemSubGroupName;
            OptObj.PurchaseOrderQuantity = GetSelectedListData[i].PurchaseOrderQuantity.toString();
            OptObj.PurchaseUnit = GetSelectedListData[i].PurchaseUnit;
            OptObj.StockUnit = GetSelectedListData[i].StockUnit;
            OptObj.PurchaseTolerance = GetSelectedListData[i].PurchaseTolerance;
            OptObj.WtPerPacking = GetSelectedListData[i].WtPerPacking.toString();
            OptObj.UnitPerPacking = GetSelectedListData[i].UnitPerPacking;
            OptObj.ConversionFactor = GetSelectedListData[i].ConversionFactor;
            OptObj.SizeW = GetSelectedListData[i].SizeW;
            OptObj.Remark = GetSelectedListData[i].Remark;

            OptObj.RefJobCardContentNo = GetSelectedListData[i].RefJobCardContentNo;
            OptObj.ClientName = GetSelectedListData[i].ClientName;
            OptObj.PendingQty = GetSelectedListData[i].PendingQty.toString();
            OptObj.FormulaPurchaseToStockUnit = GetSelectedListData[i].FormulaPurchaseToStockUnit;
            OptObj.UnitDecimalPlaceStockUnit = GetSelectedListData[i].UnitDecimalPlaceStockUnit;
            OptObj.FormulaStockToPurchaseUnit = GetSelectedListData[i].FormulaStockToPurchaseUnit;
            OptObj.UnitDecimalPlacePurchaseUnit = GetSelectedListData[i].UnitDecimalPlacePurchaseUnit;
            OptObj.GSM = GetSelectedListData[i].GSM;
            OptObj.ReleaseGSM = GetSelectedListData[i].ReleaseGSM;
            OptObj.AdhesiveGSM = GetSelectedListData[i].AdhesiveGSM;
            OptObj.Thickness = GetSelectedListData[i].Thickness;
            OptObj.Density = GetSelectedListData[i].Density;

            GetPendingData.push(OptObj);
            if (i === 0) {
                DefaultRowData.push(OptObj);
            }
            OptObj = {};
            //}
        }
        document.getElementById("LOADER").style.display = "none";
        if (GetPendingData === "" || GetPendingData === []) {
            //DevExpress.ui.notify("Please select pending purchase orders to create new receipt note..!", "warning", 1200);
            showDevExpressNotification("Please select pending purchase orders to create new receipt note..!", "warning");
            return;
        } else {
            let uniqueItemID = new Set(GetPendingData.map(obj => obj["ItemID"]));
            let itemsList = Array.from(uniqueItemID);
            itemArr = [];
            itemsList.map(function (e) {
                let itemData = {};
                let rs = GetPendingData.filter(function (x) {
                    return Number(x.ItemID) === Number(e);
                });
                if (rs.length > 0) {
                    itemData.ItemID = rs[0].ItemID;
                    itemData.ItemGroupID = rs[0].ItemGroupID;
                    itemData.ItemSubGroupID = (rs[0].ItemSubGroupID !== undefined || rs[0].ItemSubGroupID !== null) ? rs[0].ItemSubGroupID : 0;
                    itemData.QCParametersCount = 0;
                    itemData.VoucherItemApprovedBy = 0;
                    itemData.VoucherItemApprovedDate = new Date().toISOString().substr(0, 10);
                    itemArr.push(itemData);
                }
            });
            if (itemArr.length > 0) {
                try {
                    $.ajax({
                        type: "POST",
                        url: "WebServicePurchaseGRN.asmx/CheckQCParameterExists",
                        data: '{jsonItemList:' + JSON.stringify(itemArr) + '}',
                        contentType: "application/json; charset=utf-8",
                        dataType: "text",
                        success: function (results) {
                            let rs1 = results.replace(/\\/g, '');
                            rs1 = rs1.replace(/"d":""/g, '');
                            rs1 = rs1.replace(/""/g, '');
                            rs1 = rs1.replace(/:,/g, ":null,");
                            rs1 = rs1.replace(/,}/g, ",null}");
                            rs1 = rs1.substr(1);
                            rs1 = rs1.slice(0, -1);

                            itemArr = JSON.parse(rs1);

                        }
                    });
                }
                catch (e) {
                    console.log(e);
                }
            }
            FlagEdit = false;
            $("#GridPurchaseOrders").dxDataGrid({
                dataSource: GetPendingData
            });
            var dGrid = $("#GridPurchaseOrders").dxDataGrid('instance');
            dGrid.refresh();
            dGrid.repaint();
            if (GetPendingData.length > 0) {
                DefaultRowData[0].BatchNo = '_' + DefaultRowData[0].PurchaseVoucherNo + '_' + DefaultRowData[0].ItemID + '_1';
                DefaultRowData[0].ReceiptWtPerPacking = DefaultRowData[0].WtPerPacking;
                DefaultRowData[0].ReceiptQuantity = 0;
                DefaultRowData[0].WarehouseID = 0;
                DefaultRowData[0].Warehouse = "";
                DefaultRowData[0].Bin = "";
            }
            refreshbatchdetailsgrid(DefaultRowData);
            //var grid=$("#GridReceiptBatchDetails").dxDataGrid({
            //    dataSource: DefaultRowData
            //}).dxDataGrid('instance');

            document.getElementById("Btn_Next").setAttribute("data-toggle", "modal");
            document.getElementById("Btn_Next").setAttribute("data-target", "#largeModal");
            CreateReceiptNo();
        }
    } else {
        document.getElementById("BtnPrint").disabled = false;
        document.getElementById("BtnTransporterSlip").disabled = false;
        dataGrid = $("#gridreceiptlist").dxDataGrid("instance");
        if (dataGrid._options.dataSource.length <= 0) {
            //DevExpress.ui.notify("No receipt note vouchers present in the list..!", "warning", 1200);
            showDevExpressNotification("No receipt note vouchers present in the list..!", "warning");
            return;
        }
        document.getElementById("LOADER").style.display = "none";
        //document.getElementById("BtnDeletePopUp").disabled = true;

        if (receiptgridrow !== "") {
            TransactionID = receiptgridrow.TransactionID;
            GateEntryTransactionID = receiptgridrow.GateEntryTransactionID;
            document.getElementById("TxtSupplierName").value = receiptgridrow.LedgerName;
            document.getElementById("TxtSupplierID").value = receiptgridrow.LedgerID;
            document.getElementById("TxtVoucherNo").value = receiptgridrow.ReceiptVoucherNo;
            document.getElementById("TxtMaxVoucherNo").value = receiptgridrow.MaxVoucherNo;
            document.getElementById("TxtDnNo").value = receiptgridrow.DeliveryNoteNo;
            document.getElementById("TxtEwayBillNo").value = receiptgridrow.EWayBillNumber;
            document.getElementById("TxtGENo").value = receiptgridrow.GateEntryNo;
            document.getElementById("TxtLRNo").value = receiptgridrow.LRNoVehicleNo;
            document.getElementById("TxtTransporters").value = receiptgridrow.Transporter;
            document.getElementById("TxtNarration").value = receiptgridrow.Narration;
            document.getElementById("BiltyNo").value = receiptgridrow.BiltyNo;
            if (receiptgridrow.IsVoucherItemApproved > 0) {
                ApprovedVoucher = true;
            } else {
                ApprovedVoucher = false;
            }
            $("#sel_Receiver").dxSelectBox({ value: receiptgridrow.ReceivedBy });
            /*$("#DtPickerVoucherDate").dxDateBox({ value: receiptgridrow.ReceiptVoucherDate });*/
            $("#DtPickerVoucherDate").dxDateBox({
                value: receiptgridrow.ReceiptVoucherDate,
                min: null
            });
            $("#DtPickerDnDate").dxDateBox({ value: receiptgridrow.DeliveryNoteDate });
            $("#DtPickerEwayDate").dxDateBox({ value: receiptgridrow.EWayBillDate });
            $("#DtPickerGEDate").dxDateBox({ value: receiptgridrow.GateEntryDate });
            $("#BiltyDate").dxDateBox({ value: receiptgridrow.BiltyDate });
            validateUserData.userName = ""; validateUserData.password = ""; validateUserData.RecordID = document.getElementById("TxtGRNID").value; validateUserData.actionType = "Update"; validateUserData.transactionRemark = ""; validateUserData.isUserInfoFilled = false; validateUserData.documentNo = document.getElementById("TxtVoucherNo").value;
            FlagEdit = true;
        } else {
            //DevExpress.ui.notify("No receipt note vouchers selected in the list to view or edit..!", "warning", 1200);
            showDevExpressNotification("No receipt note vouchers selected in the list to view or edit..!", "warning");
            return;
        }
        if (TransactionID > 0) {
            document.getElementById("Btn_Next").setAttribute("data-toggle", "modal");
            document.getElementById("Btn_Next").setAttribute("data-target", "#largeModal");
            $.ajax({
                type: "POST",
                url: "WebServicePurchaseGRN.asmx/GetReceiptVoucherBatchDetail",
                data: '{TransactionID:' + TransactionID + '}',
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
                    receiptBatchDetail = JSON.parse(res);

                    var PurchaseTransID = 0;
                    var Item_ID = 0;
                    var sumqty = 0;
                    var receiptpoDetail = [];
                    var receiptfirstbatchDetail = [];
                    var OptObj = {};

                    //if (receiptBatchDetail.length > 0) {
                    //    PurchaseTransID = receiptBatchDetail[0].PurchaseTransactionID;
                    //    Item_ID = receiptBatchDetail[0].ItemID;
                    //}
                    for (i = 0; i < receiptBatchDetail.length; i++) {
                        var results1 = $.grep(receiptpoDetail, function (e) { return (e.TransactionID === receiptBatchDetail[i].PurchaseTransactionID && e.ItemID === receiptBatchDetail[i].ItemID); });
                        if (results1.length <= 0) {
                            var formula = 0, PurchaseQty = 0, ReceiptQty = 0, UnitPerPacking = 0;
                            var pendingQty = 0, SizeW = 0, WtPerPacking = 0, UnitDecimalPlace = 0, ConversionFactor = 0;
                            formula = receiptBatchDetail[i].FormulaStockToPurchaseUnit;
                            PurchaseQty = receiptBatchDetail[i].PurchaseOrderQuantity.toString();
                            ReceiptQty = receiptBatchDetail[i].ReceiptQuantity.toString();
                            UnitPerPacking = receiptBatchDetail[i].UnitPerPacking;
                            SizeW = receiptBatchDetail[i].SizeW;
                            WtPerPacking = receiptBatchDetail[i].WtPerPacking.toString();
                            UnitDecimalPlace = receiptBatchDetail[i].UnitDecimalPlacePurchaseUnit;
                            ConversionFactor = receiptBatchDetail[i].ConversionFactor;
                            if (Number(receiptBatchDetail[i].ReceiptQuantity) > 0) {
                                ReceiptQty = Number(StockUnitConversion("", Number(receiptBatchDetail[i].ReceiptQuantity), Number(receiptBatchDetail[i].UnitPerPacking), Number(receiptBatchDetail[i].WtPerPacking), Number(receiptBatchDetail[i].ConversionFactor), Number(receiptBatchDetail[i].SizeW), Number(receiptBatchDetail[i].UnitDecimalPlacePurchaseUnit), receiptBatchDetail[i].StockUnit.toString(), receiptBatchDetail[i].PurchaseUnit.toString(), Number(receiptBatchDetail[i].GSM), Number(receiptBatchDetail[i].ReleaseGSM), Number(receiptBatchDetail[i].AdhesiveGSM), Number(receiptBatchDetail[i].Thickness), Number(receiptBatchDetail[i].Density)));
                                pendingQty = (Number(PurchaseQty) - Number(ReceiptQty)).toFixed(Number(UnitDecimalPlace));
                            } else {
                                pendingQty = Number(PurchaseQty);
                            }

                            //if (formula !== "" && formula !== null && formula !== undefined && formula !== "undefined") {
                            //    formula = formula.split('e.').join('');
                            //    formula = formula.replace("Quantity", "ReceiptQty");

                            //    var n = formula.search("UnitPerPacking");
                            //    if (n > 0) {
                            //        if (Number(UnitPerPacking) > 0) {
                            //            pendingQty = eval(formula);
                            //            pendingQty = (Number(PurchaseQty) - Number(pendingQty)).toFixed(Number(UnitDecimalPlace));
                            //        }
                            //    } else {
                            //        n = formula.search("SizeW");
                            //        if (n > 0) {
                            //            if (Number(SizeW) > 0) {
                            //                pendingQty = eval(formula);

                            //                pendingQty = (Number(PurchaseQty) - Number(pendingQty)).toFixed(Number(UnitDecimalPlace));
                            //            }
                            //        } else {
                            //            pendingQty = eval(formula);
                            //            pendingQty = (Number(PurchaseQty) - Number(pendingQty)).toFixed(Number(UnitDecimalPlace));
                            //        }
                            //    }
                            //} else {
                            //    if (ReceiptQty > 0) {
                            //        pendingQty = (Number(PurchaseQty) - Number(ReceiptQty)).toFixed(Number(UnitDecimalPlace));
                            //    } else {
                            //        pendingQty = Number(PurchaseQty);
                            //    }
                            //}
                            OptObj = {};
                            OptObj.TransactionID = receiptBatchDetail[i].PurchaseTransactionID;
                            OptObj.LedgerID = receiptBatchDetail[i].LedgerID;
                            OptObj.ItemID = receiptBatchDetail[i].ItemID;
                            OptObj.ItemGroupID = receiptBatchDetail[i].ItemGroupID;
                            OptObj.ItemSubGroupID = receiptBatchDetail[i].ItemSubGroupID;
                            OptObj.ItemGroupNameID = receiptBatchDetail[i].ItemGroupNameID;
                            OptObj.ItemSubGroupName = receiptBatchDetail[i].ItemSubGroupName;
                            OptObj.PurchaseVoucherNo = receiptBatchDetail[i].PurchaseVoucherNo;
                            OptObj.PurchaseVoucherDate = receiptBatchDetail[i].PurchaseVoucherDate;
                            OptObj.ItemCode = receiptBatchDetail[i].ItemCode;
                            OptObj.ItemName = receiptBatchDetail[i].ItemName;
                            OptObj.PurchaseOrderQuantity = receiptBatchDetail[i].PurchaseOrderQuantity.toString();
                            OptObj.PurchaseUnit = receiptBatchDetail[i].PurchaseUnit;
                            OptObj.StockUnit = receiptBatchDetail[i].StockUnit;
                            OptObj.PurchaseTolerance = receiptBatchDetail[i].PurchaseTolerance;
                            OptObj.WtPerPacking = receiptBatchDetail[i].WtPerPacking.toString();
                            OptObj.UnitPerPacking = receiptBatchDetail[i].UnitPerPacking;
                            OptObj.ConversionFactor = receiptBatchDetail[i].ConversionFactor;
                            OptObj.SizeW = receiptBatchDetail[i].SizeW;
                            OptObj.FormulaPurchaseToStockUnit = receiptBatchDetail[i].FormulaPurchaseToStockUnit;
                            OptObj.UnitDecimalPlaceStockUnit = receiptBatchDetail[i].UnitDecimalPlaceStockUnit;
                            OptObj.FormulaStockToPurchaseUnit = receiptBatchDetail[i].FormulaStockToPurchaseUnit;
                            OptObj.UnitDecimalPlacePurchaseUnit = receiptBatchDetail[i].UnitDecimalPlacePurchaseUnit;
                            OptObj.GSM = receiptBatchDetail[i].GSM;
                            OptObj.ReleaseGSM = receiptBatchDetail[i].ReleaseGSM;
                            OptObj.AdhesiveGSM = receiptBatchDetail[i].AdhesiveGSM;
                            OptObj.Thickness = receiptBatchDetail[i].Thickness;
                            OptObj.Density = receiptBatchDetail[i].Density;
                            OptObj.RefJobCardContentNo = receiptBatchDetail[i].RefJobCardContentNo;
                            OptObj.PendingQty = pendingQty.toString();

                            receiptpoDetail.push(OptObj);

                            if (i === 0) {
                                PurchaseTransID = receiptBatchDetail[i].PurchaseTransactionID;
                                Item_ID = receiptBatchDetail[i].ItemID;
                                var rs1 = $.grep(receiptBatchDetail, function (e) { return (e.PurchaseTransactionID === PurchaseTransID && e.ItemID === Item_ID); });
                                if (rs1.length > 0) {
                                    for (var x = 0; x < rs1.length; x++) {
                                        OptObj = {};
                                        OptObj = rs1[x];
                                        OptObj.TransactionID = rs1[x].PurchaseTransactionID;
                                        OptObj.ReceiptQuantity = rs1[x].ChallanQuantity;
                                        OptObj.WarehouseBin = OptObj.Warehouse + ' - ' + OptObj.Bin;
                                        receiptfirstbatchDetail.push(OptObj);
                                    }
                                    var OptObj1 = {};
                                    //OptObj1 = rs1[0];
                                    OptObj1.TransactionID = rs1[0].PurchaseTransactionID;
                                    OptObj1.LedgerID = rs1[0].LedgerID;
                                    OptObj1.ItemID = rs1[0].ItemID;
                                    OptObj1.ItemGroupID = rs1[0].ItemGroupID;
                                    OptObj1.ItemGroupNameID = rs1[0].ItemGroupNameID;
                                    OptObj1.PurchaseVoucherNo = rs1[0].PurchaseVoucherNo;
                                    OptObj1.PurchaseVoucherDate = rs1[0].PurchaseVoucherDate;
                                    OptObj1.ItemCode = rs1[0].ItemCode;
                                    OptObj1.ItemName = rs1[0].ItemName;
                                    OptObj1.PurchaseOrderQuantity = rs1[0].PurchaseOrderQuantity.toString();
                                    OptObj1.PurchaseUnit = rs1[0].PurchaseUnit;
                                    OptObj1.StockUnit = rs1[0].StockUnit;
                                    OptObj1.ReceiptQuantity = 0;
                                    //OptObj1.BatchNo = rs1[0].BatchNo.slice(0, rs1[0].BatchNo.length - 2) + '_' + (x + 1);
                                    OptObj1.BatchNo = TransactionID + '_' + rs1[0].PurchaseVoucherNo + '_' + rs1[0].ItemID + '_' + (x + 1);
                                    OptObj1.PurchaseTolerance = rs1[0].PurchaseTolerance;
                                    OptObj1.ReceiptWtPerPacking = rs1[0].WtPerPacking;
                                    OptObj1.WtPerPacking = rs1[0].WtPerPacking.toString();
                                    OptObj1.UnitPerPacking = rs1[0].UnitPerPacking;
                                    OptObj1.ConversionFactor = rs1[0].ConversionFactor;
                                    OptObj1.SizeW = rs1[0].SizeW;
                                    OptObj1.GSM = rs1[0].GSM;
                                    OptObj1.ReleaseGSM = rs1[0].ReleaseGSM;
                                    OptObj1.AdhesiveGSM = rs1[0].AdhesiveGSM;
                                    OptObj1.Thickness = rs1[0].Thickness;
                                    OptObj1.Density = rs1[0].Density;
                                    OptObj1.WarehouseID = 0;
                                    OptObj1.Warehouse = "";
                                    OptObj1.Bin = "";
                                    OptObj1.WarehouseBin = "";
                                    OptObj1.FormulaPurchaseToStockUnit = rs1[0].FormulaPurchaseToStockUnit;
                                    OptObj1.UnitDecimalPlaceStockUnit = rs1[0].UnitDecimalPlaceStockUnit;
                                    OptObj1.FormulaStockToPurchaseUnit = rs1[0].FormulaStockToPurchaseUnit;
                                    OptObj1.UnitDecimalPlacePurchaseUnit = rs1[0].UnitDecimalPlacePurchaseUnit;
                                    receiptfirstbatchDetail.push(OptObj1);
                                }
                            }
                        }
                    }

                    let uniqueItemID = new Set(receiptpoDetail.map(obj => obj["ItemID"]));
                    let itemsList = Array.from(uniqueItemID);
                    itemArr = [];
                    itemsList.map(function (e) {
                        let itemData = {};
                        let rs = receiptpoDetail.filter(function (x) {
                            return Number(x.ItemID) === Number(e);
                        });
                        if (rs.length > 0) {
                            itemData.ItemID = rs[0].ItemID;
                            itemData.ItemGroupID = rs[0].ItemGroupID;
                            itemData.ItemSubGroupID = (rs[0].ItemSubGroupID !== undefined || rs[0].ItemSubGroupID !== null) ? rs[0].ItemSubGroupID : 0;
                            itemData.QCParametersCount = 0;
                            itemData.VoucherItemApprovedBy = 0;
                            itemData.VoucherItemApprovedDate = new Date().toISOString().substr(0, 10);
                            itemArr.push(itemData);
                        }
                    });
                    if (itemArr.length > 0) {
                        try {
                            $.ajax({
                                type: "POST",
                                url: "WebServicePurchaseGRN.asmx/CheckQCParameterExists",
                                data: '{jsonItemList:' + JSON.stringify(itemArr) + '}',
                                contentType: "application/json; charset=utf-8",
                                dataType: "text",
                                success: function (results) {
                                    let rs1 = results.replace(/\\/g, '');
                                    rs1 = rs1.replace(/"d":""/g, '');
                                    rs1 = rs1.replace(/""/g, '');
                                    rs1 = rs1.replace(/:,/g, ":null,");
                                    rs1 = rs1.replace(/,}/g, ",null}");
                                    rs1 = rs1.substr(1);
                                    rs1 = rs1.slice(0, -1);

                                    itemArr = JSON.parse(rs1);

                                }
                            });
                        }
                        catch (e) {
                            console.log(e);
                        }
                    }

                    $("#GridPurchaseOrders").dxDataGrid({
                        dataSource: receiptpoDetail
                    });
                    refreshbatchdetailsgrid(receiptfirstbatchDetail);
                }

            });
        }
    }
});

function checkExistingBatchDetail(options) {
    try {
        var i = 0;
        var newData = [];
        var objpub = {};
        if (receiptBatchDetail.length > 0) {
            for (i = 0; i < receiptBatchDetail.length; i++) {
                if (Number(receiptBatchDetail[i].PurchaseTransactionID) === Number(options.data.TransactionID) && Number(receiptBatchDetail[i].ItemID) === Number(options.data.ItemID)) {
                    objpub.TransactionID = receiptBatchDetail[i].PurchaseTransactionID;
                    objpub.LedgerID = receiptBatchDetail[i].LedgerID;
                    objpub.ItemID = receiptBatchDetail[i].ItemID;
                    objpub.ItemGroupID = receiptBatchDetail[i].ItemGroupID;
                    objpub.ItemGroupNameID = receiptBatchDetail[i].ItemGroupNameID;
                    objpub.PurchaseVoucherNo = receiptBatchDetail[i].PurchaseVoucherNo;
                    objpub.PurchaseVoucherDate = receiptBatchDetail[i].PurchaseVoucherDate;
                    objpub.ItemCode = receiptBatchDetail[i].ItemCode;
                    objpub.ItemName = receiptBatchDetail[i].ItemName;
                    objpub.PurchaseOrderQuantity = receiptBatchDetail[i].PurchaseOrderQuantity.toString();
                    objpub.PurchaseUnit = receiptBatchDetail[i].PurchaseUnit;
                    objpub.ReceiptQuantity = receiptBatchDetail[i].ChallanQuantity.toString();
                    objpub.BatchNo = receiptBatchDetail[i].BatchNo;
                    objpub.SupplierBatchNo = receiptBatchDetail[i].SupplierBatchNo;
                    objpub.StockUnit = receiptBatchDetail[i].StockUnit;
                    objpub.Warehouse = receiptBatchDetail[i].Warehouse;
                    objpub.Bin = receiptBatchDetail[i].Bin;
                    objpub.WarehouseBin = receiptBatchDetail[i].Warehouse + ' - ' + receiptBatchDetail[i].Bin;
                    objpub.PurchaseTolerance = receiptBatchDetail[i].PurchaseTolerance;
                    objpub.ReceiptWtPerPacking = receiptBatchDetail[i].ReceiptWtPerPacking.toString();
                    objpub.WtPerPacking = receiptBatchDetail[i].WtPerPacking.toString();
                    objpub.UnitPerPacking = receiptBatchDetail[i].UnitPerPacking;
                    objpub.ConversionFactor = receiptBatchDetail[i].ConversionFactor;
                    objpub.SizeW = receiptBatchDetail[i].SizeW;
                    objpub.WarehouseID = receiptBatchDetail[i].WarehouseID;
                    objpub.FormulaPurchaseToStockUnit = receiptBatchDetail[i].FormulaPurchaseToStockUnit;
                    objpub.UnitDecimalPlaceStockUnit = receiptBatchDetail[i].UnitDecimalPlaceStockUnit;
                    objpub.FormulaStockToPurchaseUnit = receiptBatchDetail[i].FormulaStockToPurchaseUnit;
                    objpub.UnitDecimalPlacePurchaseUnit = receiptBatchDetail[i].UnitDecimalPlacePurchaseUnit;
                    objpub.GSM = receiptBatchDetail[i].GSM;
                    objpub.ReleaseGSM = receiptBatchDetail[i].ReleaseGSM;
                    objpub.AdhesiveGSM = receiptBatchDetail[i].AdhesiveGSM;
                    objpub.Thickness = receiptBatchDetail[i].Thickness;
                    objpub.Density = receiptBatchDetail[i].Density;
                    newData.push(objpub);
                    objpub = {};
                }
            }
        }
        if (newData.length > 0) {
            if (FlagEdit === false) {
                options.data.BatchNo = '_' + newData[newData.length - 1].PurchaseVoucherNo + '_' + newData[newData.length - 1].ItemID + '_' + (newData.length + 1)
                options.data.ReceiptWtPerPacking = options.data.WtPerPacking.toString();
                options.data.ReceiptQuantity = 0;
            } else {
                options.data.BatchNo = TransactionID + '_' + newData[newData.length - 1].PurchaseVoucherNo + '_' + newData[newData.length - 1].ItemID + '_' + (newData.length + 1)
                options.data.ReceiptWtPerPacking = options.data.WtPerPacking.toString();
                options.data.ReceiptQuantity = 0;
            }
        } else {
            options.data.ReceiptWtPerPacking = options.data.WtPerPacking.toString();
            options.data.ReceiptQuantity = 0;
            options.data.BatchNo = '_' + options.data.PurchaseVoucherNo + '_' + options.data.ItemID + '_1';
        }
        options.data.WarehouseID = 0;
        options.data.Warehouse = "";
        options.data.Bin = "";
        options.data.WarehouseBin = "";

        newData.push(options.data);
        refreshbatchdetailsgrid(newData);
    } catch (e) {
        console.log(e);
    }

}

function refreshbatchdetailsgrid(newData) {
    GridReceiptBatchDetails = $("#GridReceiptBatchDetails").dxDataGrid({
        dataSource: newData,
        allowColumnReordering: true,
        allowColumnResizing: true,
        //columnAutoWidth: true,
        columnResizingMode: "widget",
        selection: { mode: "single" },
        showBorders: true,
        showRowLines: true,
        editing: {
            mode: "cell",
            allowDeleting: true,
            //allowAdding: true,
            allowUpdating: true
        },
        scrolling: { mode: "standard" },
        paging: { enabled: false },
        rowAlternationEnabled: false,
        columns: [
            { dataField: "TransactionID", visible: false, caption: "PurchaseTransactionID", width: 120 },
            { dataField: "LedgerID", visible: false, caption: "LedgerID", width: 120 },
            { dataField: "ItemID", visible: false, caption: "ItemID", width: 120 },
            { dataField: "ItemGroupID", visible: false, caption: "ItemGroupID", width: 120 },
            { dataField: "PurchaseVoucherDate", visible: false, caption: "P.O. Date", width: 120 },
            { dataField: "PurchaseOrderQuantity", visible: false, caption: "P.O. Qty", width: 120 },
            { dataField: "PurchaseUnit", visible: false, caption: "Purchase Unit", width: 120 },
            { dataField: "PurchaseVoucherNo", visible: true, caption: "P.O. No.", width: 120 },
            { dataField: "ItemCode", visible: true, caption: "Item Code", width: 120 },
            { dataField: "ItemName", visible: true, caption: "Item Name", width: 300 },
            {
                //dataField: "ReceiptQuantity", validationRules: [{ type: "required" }, { type: "numeric" }], caption: "Receipt Quantity", visible: true, width: 120,
                dataField: "ReceiptQuantity", caption: "Receipt Qty (S.U.)", validationRules: [{ type: "numeric" }], visible: true, width: 120,
                setCellValue: function (newData, value, currentRowData) {
                    if (value === null || value === undefined || isNaN(value) === true) return false;
                    newData.ReceiptQuantity = Number(value);
                    //newData.TotalPrice = currentRowData.Price * value;
                    if (currentRowData.FormulaStockToPurchaseUnit === undefined || currentRowData.FormulaStockToPurchaseUnit === null) currentRowData.FormulaStockToPurchaseUnit = "";
                    if (currentRowData.ReceiptWtPerPacking > 0) {
                        newData.ReceiptQuantityInPurchaseUnit = Number(StockUnitConversion(currentRowData.FormulaStockToPurchaseUnit, Number(value), currentRowData.UnitPerPacking, currentRowData.ReceiptWtPerPacking, currentRowData.ConversionFactor, currentRowData.SizeW, currentRowData.UnitDecimalPlacePurchaseUnit, currentRowData.StockUnit, currentRowData.PurchaseUnit, Number(currentRowData.GSM), Number(currentRowData.ReleaseGSM), Number(currentRowData.AdhesiveGSM), Number(currentRowData.Thickness), Number(currentRowData.Density)));
                    } else {
                        newData.ReceiptQuantityInPurchaseUnit = Number(StockUnitConversion(currentRowData.FormulaStockToPurchaseUnit, Number(value), currentRowData.UnitPerPacking, currentRowData.WtPerPacking, currentRowData.ConversionFactor, currentRowData.SizeW, currentRowData.UnitDecimalPlacePurchaseUnit, currentRowData.StockUnit, currentRowData.PurchaseUnit, Number(currentRowData.GSM), Number(currentRowData.ReleaseGSM), Number(currentRowData.AdhesiveGSM), Number(currentRowData.Thickness), Number(currentRowData.Density)));
                    }
                }
            },
            {
                //dataField: "ReceiptQuantityInPurchaseUnit", caption: "Receipt Qty (In P.U.)", visible: true, width: 120,
                dataField: "ReceiptQuantityInPurchaseUnit", caption: "Receipt Qty (P.U.)", validationRules: [{ type: "numeric" }], visible: true, width: 120, allowEditing: true,
                setCellValue: function (newData, value, currentRowData) {
                    if (value === null || value === undefined || isNaN(value) === true) return false;
                    newData.ReceiptQuantityInPurchaseUnit = Number(value);
                    //newData.TotalPrice = currentRowData.Price * value;
                    if (currentRowData.FormulaPurchaseToStockUnit === undefined || currentRowData.FormulaPurchaseToStockUnit === null) currentRowData.FormulaPurchaseToStockUnit = "";
                    if (currentRowData.ReceiptWtPerPacking > 0) {
                        newData.ReceiptQuantity = Number(StockUnitConversion(currentRowData.FormulaPurchaseToStockUnit, Number(value), currentRowData.UnitPerPacking, currentRowData.ReceiptWtPerPacking, currentRowData.ConversionFactor, currentRowData.SizeW, currentRowData.UnitDecimalPlaceStockUnit, currentRowData.PurchaseUnit, currentRowData.StockUnit, Number(currentRowData.GSM), Number(currentRowData.ReleaseGSM), Number(currentRowData.AdhesiveGSM), Number(currentRowData.Thickness), Number(currentRowData.Density)));
                    } else {
                        newData.ReceiptQuantity = Number(StockUnitConversion(currentRowData.FormulaPurchaseToStockUnit, Number(value), currentRowData.UnitPerPacking, currentRowData.WtPerPacking, currentRowData.ConversionFactor, currentRowData.SizeW, currentRowData.UnitDecimalPlaceStockUnit, currentRowData.PurchaseUnit, currentRowData.StockUnit, Number(currentRowData.GSM), Number(currentRowData.ReleaseGSM), Number(currentRowData.AdhesiveGSM), Number(currentRowData.Thickness), Number(currentRowData.Density)));
                    }
                },
                calculateCellValue: function (rowData) {
                    if (rowData.ItemID === undefined || rowData.ItemID === null || isNaN(rowData.ItemID) === true) return false;
                    if (rowData.FormulaStockToPurchaseUnit === undefined || rowData.FormulaStockToPurchaseUnit === null) rowData.FormulaStockToPurchaseUnit = "";
                    if (Number(rowData.ReceiptWtPerPacking) > 0) {
                        return Number(StockUnitConversion(rowData.FormulaStockToPurchaseUnit, Number(rowData.ReceiptQuantity), Number(rowData.UnitPerPacking), Number(rowData.ReceiptWtPerPacking), Number(rowData.ConversionFactor), Number(rowData.SizeW), Number(rowData.UnitDecimalPlacePurchaseUnit), rowData.StockUnit, rowData.PurchaseUnit, Number(rowData.GSM), Number(rowData.ReleaseGSM), Number(rowData.AdhesiveGSM), Number(rowData.Thickness), Number(rowData.Density)));
                    } else {
                        return Number(StockUnitConversion(rowData.FormulaStockToPurchaseUnit, Number(rowData.ReceiptQuantity), Number(rowData.UnitPerPacking), Number(rowData.WtPerPacking), Number(rowData.ConversionFactor), Number(rowData.SizeW), Number(rowData.UnitDecimalPlacePurchaseUnit), rowData.StockUnit, rowData.PurchaseUnit, Number(rowData.GSM), Number(rowData.ReleaseGSM), Number(rowData.AdhesiveGSM), Number(rowData.Thickness), Number(rowData.Density)));
                    }
                }
            },
            {
                dataField: "BatchNo", validationRules: [{ type: "required" }], caption: "Ref. Batch No.", visible: true, width: 120
            },
            { dataField: "SupplierBatchNo", caption: "Supp.Batch No.", allowEditing: true, visible: true, width: 120 },

            {
                dataField: "MfgDate", visible: true, allowEditing: true, caption: "Mfg.Date", width: 120,
                dataType: "date", format: "dd-MMM-yyyy",
                showEditorAlways: true
            },

            {
                dataField: "ExpiryDate", visible: true, allowEditing: true, caption: "Expiry/Warranty Date", width: 120,
                dataType: "date", format: "dd-MMM-yyyy",
                showEditorAlways: true
            },

            { dataField: "StockUnit", visible: true },
            { dataField: "ReceiptWtPerPacking", visible: true, validationRules: [{ type: "required" }, { type: "numeric" }], caption: "Wt/Packing", width: 120 },
            {
                caption: "Select Warehouse", visible: true, allowEditing: false, width: 100,
                cellTemplate: function (container, info) {
                    $('<div>').addClass('fa fa-plus customgridbtn')
                        .on('dxclick', function (e) {
                            this.setAttribute("data-toggle", "modal");
                            this.setAttribute("data-target", "#WarehouseSelectionModal");
                            //  $("p").text("Hello world!");
                        }).appendTo(container);
                }
            },
            { dataField: "WarehouseBin", visible: true, caption: "Warehouse-Bin", width: 200 },
            {
                dataField: "Warehouse", visible: false, caption: "Warehouse", width: 120
                //lookup: {
                //    dataSource: ResWarehouse,
                //    displayExpr: "Warehouse",
                //    valueExpr: "Warehouse"
                //    //keyExpr: "WarehouseID"
                //},
                //validationRules: [{ type: "required" }],
                //setCellValue: function (rowData, value) {
                //    rowData.Warehouse = value;
                //    if (value !== "") {
                //        RefreshBin(value);
                //    }
                //}
            },
            {
                dataField: "Bin", visible: false, width: 120
                //lookup: {
                //    dataSource: ResBin,
                //    displayExpr: "Bin",
                //    valueExpr: "WarehouseID"
                //},
                //validationRules: [{ type: "required" }],
                //setCellValue: function (rowData, value) {
                //    rowData.Bin = value;
                //    rowData.WarehouseID = 0;
                //    if (value > 0) {
                //        rowData.WarehouseID = value;
                //    }
                //}
            },
            { dataField: "PurchaseTolerance", visible: false, caption: "P.O. Tol.(%)", width: 120 },
            { dataField: "WtPerPacking", visible: false, caption: "WtPerPacking", width: 120 },
            { dataField: "UnitPerPacking", visible: false, caption: "UnitPerPacking", width: 120 },
            { dataField: "ConversionFactor", visible: false, caption: "ConversionFactor", width: 120 },
            { dataField: "SizeW", visible: false, caption: "SizeW", width: 120 },
            { dataField: "WarehouseID", visible: false, caption: "WarehouseID", width: 120 },
            { dataField: "ItemGroupNameID", visible: false, caption: "ItemGroupNameID", width: 120 },
            { dataField: "FormulaPurchaseToStockUnit", visible: false, caption: "FormulaPurchaseToStockUnit" },
            { dataField: "UnitDecimalPlaceStockUnit", visible: false, caption: "UnitDecimalPlaceStockUnit" },
            { dataField: "FormulaStockToPurchaseUnit", visible: false, caption: "FormulaStockToPurchaseUnit" },
            { dataField: "UnitDecimalPlacePurchaseUnit", visible: false, caption: "UnitDecimalPlacePurchaseUnit" }
        ],
        summary: {
            totalItems: [{
                column: "ReceiptQuantity",
                summaryType: "custom",
                customizeText: function (data) {
                    return data.value;
                }
            }],
            calculateCustomSummary: function (options) {
                if (options.summaryProcess === "start") {
                    options.totalValue = { sum: 0, count: 0 };
                } else if (options.summaryProcess === "calculate") {
                    options.totalValue.sum += (options.value || 0);
                    options.totalValue.count++;
                } else if (options.summaryProcess === "finalize") {
                    options.totalValue = `Ttl: ${options.totalValue.sum} | Row Count: ${options.totalValue.count}`;
                }
            }
        },

    }).dxDataGrid("instance");
}
function increaseGridHeight() {
    var gridInstance = $("#GridReceiptBatchDetails").dxDataGrid("instance");
    var currentHeight = gridInstance.element().height();

    if (!currentHeight) {
        // If height is not set, set a default height
        currentHeight = 400; // Set a default height, adjust as needed
    }

    gridInstance.option("height", currentHeight + 80);
}
// Function to decrease grid height
function decreaseGridHeight() {
    /*    var gridHeight = $("#GridReceiptBatchDetails").dxDataGrid("instance").option("height");*/
    $("#GridReceiptBatchDetails").dxDataGrid("instance").option("height", 200);
}

$("#RefreshBtn").click(function () {
    PendingReceiptData();
    ReceiptNotesVouchers();
});

PendingReceiptData();
function PendingReceiptData() {
    try {

        //var FromDate = $('#FromDate').dxDateBox('instance').option('value');
        //var ToDate = $('#ToDate').dxDateBox('instance').option('value');

        //document.getElementById("LOADER").style.display = "block";
        //$("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
        $.ajax({
            type: "POST",
            url: "WebServicePurchaseGRN.asmx/GetPendingOrdersList",
            //data: '{FromDate:' + JSON.stringify(FromDate) + ',ToDate: ' + JSON.stringify(ToDate) + '}',
            data: '',
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            success: function (results) {
                var res = results.replace(/\\/g, '');
                res = res.replace(/"d":""/g, '');
                res = res.replace(/""/g, '');
                res = res.replace(/:,/g, ":null,");
                res = res.replace(/,}/g, ",null}");
                res = res.replace(/u0027/g, "'");
                res = res.substr(1);
                res = res.slice(0, -1);
                // document.getElementById("LOADER").style.display = "none";
                // $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                var tt = JSON.parse(res);
                var formula = "";
                var formula1 = "";
                var PurchaseQty = 0;
                var ReceiptQty = 0;
                var pendingQty = 0;
                var UnitPerPacking = 1, WtPerPacking = 0, ConversionFactor = 1, UnitDecimalPlace = 0, SizeW = 1;
                for (var i = 0; i < tt.length; i++) {
                    formula = tt[i].FormulaStockToPurchaseUnit;
                    PurchaseQty = tt[i].PurchaseOrderQuantity;
                    ReceiptQty = tt[i].ReceiptQuantity;
                    UnitPerPacking = tt[i].UnitPerPacking;
                    SizeW = tt[i].SizeW;
                    WtPerPacking = tt[i].WtPerPacking;
                    UnitDecimalPlace = tt[i].UnitDecimalPlacePurchaseUnit;
                    ConversionFactor = tt[i].ConversionFactor;
                    if (Number(tt[i].ReceiptQuantity) > 0) {
                        ReceiptQty = Number(StockUnitConversion("", Number(tt[i].ReceiptQuantity), Number(tt[i].UnitPerPacking), Number(tt[i].WtPerPacking), Number(tt[i].ConversionFactor), Number(tt[i].SizeW), Number(tt[i].UnitDecimalPlacePurchaseUnit), tt[i].StockUnit.toString(), tt[i].PurchaseUnit.toString(), Number(tt[i].GSM), Number(tt[i].ReleaseGSM), Number(tt[i].AdhesiveGSM), Number(tt[i].Thickness), Number(tt[i].Density)));
                        pendingQty = (Number(PurchaseQty) - Number(ReceiptQty)).toFixed(Number(UnitDecimalPlace));
                    } else {
                        pendingQty = Number(PurchaseQty);
                    }


                    //if (formula !== "" && formula !== null && formula !== undefined && formula !== "undefined") {
                    //    formula = formula.split('e.').join('');
                    //    formula = formula.replace("Quantity", "ReceiptQty");

                    //    var n = formula.search("UnitPerPacking");
                    //    if (n > 0) {
                    //        if (Number(UnitPerPacking) > 0) {
                    //            pendingQty = eval(formula);
                    //            pendingQty = (Number(PurchaseQty) - Number(pendingQty)).toFixed(Number(UnitDecimalPlace));
                    //        }
                    //    } else {
                    //        n = formula.search("SizeW");
                    //        if (n > 0) {
                    //            if (Number(SizeW) > 0) {

                    //                pendingQty = eval(formula);
                    //                pendingQty = (Number(PurchaseQty) - Number(pendingQty)).toFixed(Number(UnitDecimalPlace));
                    //            }
                    //        } else {
                    //            pendingQty = eval(formula);
                    //            pendingQty = (Number(PurchaseQty) - Number(pendingQty)).toFixed(Number(UnitDecimalPlace));
                    //        }
                    //    }
                    //} else {
                    //    if (ReceiptQty > 0) {
                    //        pendingQty = (Number(PurchaseQty) - Number(ReceiptQty)).toFixed(Number(UnitDecimalPlace));
                    //    } else {
                    //        pendingQty = Number(PurchaseQty);
                    //    }
                    //}
                    tt[i].PendingQty = pendingQty;
                }
                SetPendingReceiptGrid(tt);
                document.getElementById("LOADER").style.display = "none";

                //$("#gridreceiptlist").dxDataGrid({
                //    dataSource: tt,
                //});
            }
        });

    } catch (e) {
        document.getElementById("LOADER").style.display = "none";
        console.log(e);
    }
}

function ReceiptNotesVouchers() {
    try {

        let fromDateValue = new Date($("#FromDate").dxDateBox("instance").option("value"));
        fromDateValue = fromDateValue.toISOString().substr(0, 10);

        let ToDateValue = new Date($("#ToDate").dxDateBox("instance").option("value"));
        ToDateValue = ToDateValue.toISOString().substr(0, 10);
        //document.getElementById("LOADER").style.display = "block";
        //$("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
        $.ajax({
            type: "POST",
            url: "WebServicePurchaseGRN.asmx/GetReceiptNoteList",
            data: '{fromDateValue:' + JSON.stringify(fromDateValue) + ',ToDateValue:' + JSON.stringify(ToDateValue) + '}',
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            success: function (results) {
                var res = results.replace(/\\/g, '');
                res = res.replace(/"d":""/g, '');
                res = res.replace(/""/g, '');
                res = res.replace(/:,/g, ":null,");
                res = res.replace(/,}/g, ",null}");
                res = res.replace(/u0027/g, "'");
                res = res.substr(1);
                res = res.slice(0, -1);
                // document.getElementById("LOADER").style.display = "none";
                // $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                var tt = JSON.parse(res);
                SetProcessedReceiptGrid(tt);
                document.getElementById("LOADER").style.display = "none";
            }
        });
    } catch (e) {
        document.getElementById("LOADER").style.display = "none";
        console.log(e);
    }
}

function SetPendingReceiptGrid(tt) {
    document.getElementById("BtnDelete").disabled = true;
    GetSelectedListData = [];
    supplieridvalidate = "";
    $("#gridreceiptlist").dxDataGrid({
        dataSource: tt.filter(function (e) { return e.PendingQty > 0; }),
        selection: { mode: "multiple", showCheckBoxesMode: "always" },
        export: {
            fileName: "Pending Purchase Orders",
        },
        onExporting(e) {
            const workbook = new ExcelJS.Workbook();
            const worksheet = workbook.addWorksheet('PendingPurchaseOrders');
            DevExpress.excelExporter.exportDataGrid({
                component: e.component,
                worksheet,
                autoFilterEnabled: true,
            }).then(() => {
                workbook.xlsx.writeBuffer().then((buffer) => {
                    saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'PendingPurchaseOrders.xlsx');
                });
            });
            e.cancel = true;
        },
        onSelectionChanged: function (clickedIndentCell) {

            if (clickedIndentCell.currentSelectedRowKeys.length > 0) {
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

                if (supplieridvalidate === "") {
                    supplieridvalidate = clickedIndentCell.currentSelectedRowKeys[0].LedgerID;
                    document.getElementById("TxtGRNID").value = clickedIndentCell.currentSelectedRowKeys[0].LedgerID;
                }
                else if (supplieridvalidate !== clickedIndentCell.currentSelectedRowKeys[0].LedgerID) {
                    clickedIndentCell.component.deselectRows((clickedIndentCell || {}).currentSelectedRowKeys[0]);
                    //DevExpress.ui.notify("Please select records which have same supplier..!", "warning", 3000);
                    showDevExpressNotification("Please select records which have same supplier..!", "warning");
                    clickedIndentCell.currentSelectedRowKeys = [];
                    return false;
                }
            }
            GetSelectedListData = clickedIndentCell.selectedRowsData;

            if (GetSelectedListData.length === 0) {
                supplieridvalidate = "";
                document.getElementById("TxtGRNID").value = "";
            }
        },
        columns: [
            { dataField: "LedgerName", visible: true, caption: "Supplier Name", width: 150, fixed: true },
            { dataField: "MaxVoucherNo", visible: true, caption: "Ref.P.O. No.", width: 100 },
            { dataField: "PurchaseVoucherNo", visible: true, caption: "P.O. No.", width: 100 },
            { dataField: "PurchaseVoucherDate", visible: true, caption: "P.O. Date", width: 140, dataType: "date", format: "dd-MMM-yyyy", Mode: "DateRangeCalendar" },
            { dataField: "ItemCode", visible: true, caption: "Item Code", width: 80 },
            { dataField: "ItemGroupName", visible: true, caption: "Item Group", width: 100 },
            { dataField: "ItemSubGroupName", visible: true, caption: "Sub Group", width: 120 },
            { dataField: "ItemName", visible: true, caption: "Item Name", width: 200 },
            { dataField: "PurchaseOrderQuantity", visible: true, caption: "P.O. Qty", width: 80 },
            { dataField: "PendingQty", visible: true, caption: "Pending Qty", width: 100 },
            { dataField: "PurchaseUnit", visible: true, caption: "Unit", width: 80 },
            { dataField: "StockUnit", visible: false, caption: "Stock Unit", width: 40 },
            { dataField: "PurchaseDivision", visible: true, caption: "Purchase Division", width: 120 },
            { dataField: "Remark", visible: true, caption: "P.O. Ref. Remark", width: 200 },
            { dataField: "RefJobCardContentNo", visible: true, caption: "Ref.J.C. No.", width: 200 },
            { dataField: "ClientName", visible: false, caption: "Ref.Client", width: 200 },
            { dataField: "JobName", visible: true, caption: "JobName", width: 200 },
            { dataField: "CreatedBy", visible: true, caption: "Created By", width: 100 },
            { dataField: "ApprovedBy", visible: true, caption: "Approved By", width: 100 },
            { dataField: "SizeW", visible: false, caption: "SizeW", width: 100 },
            { dataField: "WtPerPacking", visible: false, caption: "WtPerPacking", width: 100 },
            { dataField: "UnitPerPacking", visible: false, caption: "UnitPerPacking", width: 100 },
            { dataField: "ConversionFactor", visible: false, caption: "ConversionFactor", width: 100 },
            { dataField: "FormulaStockToPurchaseUnit", visible: false, caption: "FormulaStockToPurchaseUnit", width: 100 },
            { dataField: "UnitDecimalPlacePurchaseUnit", visible: false, caption: "UnitDecimalPlacePurchaseUnit", width: 100 },
            { dataField: "ReceiptQuantity", visible: false, caption: "ReceiptQuantity", width: 100 },
            { dataField: "FormulaPurchaseToStockUnit", visible: false, caption: "FormulaPurchaseToStockUnit", width: 100 },
            { dataField: "UnitDecimalPlaceStockUnit", visible: false, caption: "UnitDecimalPlaceStockUnit", width: 100 },
            { dataField: "IsVoucherItemApproved", visible: false, caption: "IsVoucherItemApproved", width: 100 },
            { dataField: "ApprovedQuantity", visible: false, caption: "ApprovedQuantity", width: 100 },
            { dataField: "ProductionUnitID", visible: false, caption: "ProductionUnitID", width: 100 },
            { dataField: "ProductionUnitName", visible: true, caption: "Production Unit Name", width: 150 },
            { dataField: "CompanyName", visible: true, caption: "Company Name", width: 300 }
        ]
    });
}

$("#gridreceiptlist").dxDataGrid({
    dataSource: [],
    columnAutoWidth: true,
    showBorders: true,
    showRowLines: true,
    allowColumnReordering: true,
    allowColumnResizing: true,
    columnResizingMode: "widget",
    //keyExpr: "TransactionID",
    sorting: {
        mode: "multiple"
    },
    selection: { mode: "multiple", showCheckBoxesMode: "always" },
    paging: {
        pageSize: 20
    },
    pager: {
        showPageSizeSelector: true,
        allowedPageSizes: [20, 40, 50, 100]
    },
    filterRow: { visible: true, applyFilter: "auto" },
    columnChooser: { enabled: true },
    headerFilter: { visible: true },
    height: function () {
        return window.innerHeight / 1.2;
    },
    searchPanel: { visible: true },
    loadPanel: {
        enabled: true,
        text: 'Data is loading...'
    },
    export: {
        enabled: true,
        fileName: "Purchase GRN",
        allowExportSelectedData: true
    },
    onRowPrepared: function (e) {
        if (e.rowType === "header") {
            e.rowElement.css('background', '#509EBC');
            e.rowElement.css('color', 'white');
            e.rowElement.css('font-weight', 'bold');
        }
        e.rowElement.css('fontSize', '11px');
    }
});

function SetProcessedReceiptGrid(tt) {
    document.getElementById("BtnDelete").disabled = false;
    var grid1 = $("#gridreceiptlist").dxDataGrid('instance');
    grid1.clearSelection();
    $("#gridreceiptlist").dxDataGrid({
        dataSource: tt,
        selection: { mode: "single" },
        export: {
            fileName: "Purchase GRN",
        },
        onExporting(e) {
            const workbook = new ExcelJS.Workbook();
            const worksheet = workbook.addWorksheet('ReceiptNoteList');
            DevExpress.excelExporter.exportDataGrid({
                component: e.component,
                worksheet,
                autoFilterEnabled: true,
            }).then(() => {
                workbook.xlsx.writeBuffer().then((buffer) => {
                    saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'ReceiptNoteList.xlsx');
                });
            });
            e.cancel = true;
        },
        columns: [
            { dataField: "MaxVoucherNo", visible: true, caption: "Ref.Receipt No.", width: 120 },
            { dataField: "LedgerName", visible: true, caption: "Supplier Name", width: 180, fixed: true },
            { dataField: "ReceiptVoucherNo", visible: true, caption: "Receipt Note No.", width: 120 },
            { dataField: "ReceiptVoucherDate", visible: true, caption: "Receipt Note Date", width: 140, dataType: "date", format: "dd-MMM-yyyy", Mode: "DateRangeCalendar" },
            { dataField: "PurchaseVoucherNo", visible: true, caption: "P.O. No.", width: 100 },
            { dataField: "PurchaseVoucherDate", visible: true, caption: "P.O. Date", width: 100 },
            { dataField: "DeliveryNoteNo", visible: true, caption: "D.N. No.", width: 80 },
            { dataField: "DeliveryNoteDate", visible: true, caption: "D.N. Date", width: 80 },
            { dataField: "GateEntryNo", visible: false, caption: "Gate Entry No.", width: 80 },
            { dataField: "GateEntryDate", visible: false, caption: "Gate Entry Date", width: 60 },
            { dataField: "LRNoVehicleNo", visible: false, caption: "LR No./Vehicle No.", width: 60 },
            { dataField: "Transporter", visible: true, caption: "Transporter", width: 100 },
            { dataField: "ReceiverName", visible: true, caption: "Received By", width: 100 },
            { dataField: "RefJobCardContentNo", visible: true, caption: "Ref.J.C. No.", width: 200 },
            { dataField: "JobName", visible: true, caption: "JobName", width: 200 },
            { dataField: "CreatedBy", visible: true, caption: "Created By", width: 100 },
            { dataField: "Narration", visible: true, caption: "Remark", width: 60 },
            { dataField: "IsVoucherItemApproved", visible: false, caption: "IsVoucherItemApproved" },
            { dataField: "ApprovedQuantity", visible: false, caption: "Approved Quantity" },
            { dataField: "ProductionUnitID", visible: false, caption: "ProductionUnitID", width: 100 },
            { dataField: "ProductionUnitName", visible: true, caption: "Production Unit Name", width: 150 },
            { dataField: "CompanyName", visible: true, caption: "Company Name", width: 300 }
        ],
        onSelectionChanged: function (selectedItems) {
            receiptgridrow = selectedItems.selectedRowsData[0];

            PurchaseTransactionID = 0;
            GBLItemID = 0;
            SelectedProductionUnitID = 0;
            SelectedProductionUnitName = null;

            var MakeObj = [];
            if (receiptgridrow !== "" && receiptgridrow !== undefined && receiptgridrow !== null) {
                MakeObj.push(receiptgridrow);
                document.getElementById("TxtGRNID").value = MakeObj[0].TransactionID;
                SelectedProductionUnitID = MakeObj[0].ProductionUnitID;
                SelectedProductionUnitName = MakeObj[0].ProductionUnitName;
                PurchaseTransactionID = MakeObj[0].PurchaseTransactionID;
                validateUserData.userName = ""; validateUserData.password = ""; validateUserData.RecordID = document.getElementById("TxtGRNID").value; validateUserData.transactionRemark = ""; validateUserData.isUserInfoFilled = false;
            }
        }
    });
}

$("#GridPurchaseOrders").dxDataGrid({
    dataSource: [],
    allowColumnResizing: true,
    columnResizingMode: "widget",
    columnAutoWidth: true,
    showBorders: true,
    showRowLines: true,
    allowColumnReordering: true,
    wordWrapEnabled: true,
    //keyExpr: "TransactionID",
    sorting: {
        mode: "none"
    },
    selection: {
        mode: "single"
    },
    scrolling: { mode: 'infinite' },
    rowAlternationEnabled: false,
    columns: [
        { dataField: "TransactionID", visible: false, caption: "TransactionID" },
        { dataField: "LedgerID", visible: false, caption: "LedgerID" },
        { dataField: "ItemID", visible: false, caption: "ItemID" },
        { dataField: "ItemGroupID", visible: false, caption: "ItemGroupID" },
        { dataField: "ItemGroupNameID", visible: false, caption: "ItemGroupNameID" },
        { dataField: "PurchaseVoucherNo", visible: true, caption: "P.O. No.", width: 100 },
        { dataField: "PurchaseVoucherDate", visible: true, caption: "P.O. Date", width: 80 },
        { dataField: "ItemCode", visible: true, caption: "Item Code", width: 60 },
        { dataField: "ItemSubGroupName", visible: true, caption: "Sub Group", width: 60 },
        { dataField: "ItemName", visible: true, caption: "Item Name", width: 300 },
        { dataField: "PurchaseOrderQuantity", visible: true, caption: "P.O.Qty (P.U.)", width: 80 },
        {
            dataField: "PurchaseOrderQuantityInStockUnit", visible: true, caption: "P.O.Qty (S.U.)", width: 80,
            calculateCellValue: function (rowData) {
                if (rowData.FormulaPurchaseToStockUnit === undefined || rowData.FormulaPurchaseToStockUnit === null) rowData.FormulaPurchaseToStockUnit = "";
                return Number(StockUnitConversion(rowData.FormulaPurchaseToStockUnit, Number(rowData.PurchaseOrderQuantity), Number(rowData.UnitPerPacking), Number(rowData.WtPerPacking), Number(rowData.ConversionFactor), Number(rowData.SizeW), Number(rowData.UnitDecimalPlaceStockUnit), rowData.PurchaseUnit, rowData.StockUnit, Number(rowData.GSM), Number(rowData.ReleaseGSM), Number(rowData.AdhesiveGSM), Number(rowData.Thickness), Number(rowData.Density)));
            }
        },
        { dataField: "PurchaseTolerance", visible: true, caption: "Tol.(%)", width: 40 },
        { dataField: "PendingQty", visible: true, caption: "Pending Qty (P.U.)", width: 80 },
        {
            dataField: "PendingQtyInStockUnit", visible: true, caption: "Pending Qty (S.U.)", width: 80,
            calculateCellValue: function (rowData) {
                if (rowData.FormulaPurchaseToStockUnit === undefined || rowData.FormulaPurchaseToStockUnit === null) rowData.FormulaPurchaseToStockUnit = "";
                return Number(StockUnitConversion(rowData.FormulaPurchaseToStockUnit, Number(rowData.PendingQty), Number(rowData.UnitPerPacking), Number(rowData.WtPerPacking), Number(rowData.ConversionFactor), Number(rowData.SizeW), Number(rowData.UnitDecimalPlaceStockUnit), rowData.PurchaseUnit, rowData.StockUnit, Number(rowData.GSM), Number(rowData.ReleaseGSM), Number(rowData.AdhesiveGSM), Number(rowData.Thickness), Number(rowData.Density)));
            }
        },
        { dataField: "PurchaseUnit", visible: true, caption: "Purchase Unit", width: 80 },
        { dataField: "StockUnit", visible: true, caption: "Stock Unit", width: 80 },
        { dataField: "Remark", visible: true, caption: "P.O. Ref. Remark", width: 140 },
        { dataField: "RefJobCardContentNo", visible: true, caption: "Job Card No.", width: 100 },
        { dataField: "ClientName", visible: false, caption: "Ref.Client", width: 100 },
        { dataField: "WtPerPacking", visible: false, caption: "WtPerPacking" },
        { dataField: "UnitPerPacking", visible: false, caption: "UnitPerPacking" },
        { dataField: "ConversionFactor", visible: false, caption: "ConversionFactor" },
        { dataField: "SizeW", visible: false, caption: "SizeW" },
        {
            dataField: "BatchDetail", caption: "Batch Detail", visible: true, width: 80,
            cellTemplate: function (container, options) {
                $('<div>').addClass('master-detail-label dx-link')
                    .text('Batch Detail')
                    .on('dxclick', function () { checkExistingBatchDetail(options); }).appendTo(container);
            }

        },
        { dataField: "FormulaPurchaseToStockUnit", visible: false, caption: "FormulaPurchaseToStockUnit" },
        { dataField: "UnitDecimalPlaceStockUnit", visible: false, caption: "UnitDecimalPlaceStockUnit" },
        { dataField: "FormulaStockToPurchaseUnit", visible: false, caption: "FormulaStockToPurchaseUnit" },
        { dataField: "UnitDecimalPlacePurchaseUnit", visible: false, caption: "UnitDecimalPlacePurchaseUnit" }
    ],
    //customizeColumns: function (columns) {
    //    columns[0].width = 120;
    //    columns[1].width = 150;
    //},
    height: function () {
        return window.innerHeight / 4;
    },
    onRowPrepared: function (e) {
        if (e.rowType === "header") {
            e.rowElement.css('background', '#509EBC');
            e.rowElement.css('color', 'white');
            e.rowElement.css('font-weight', 'bold');
        }
        e.rowElement.css('fontSize', '11px');
    },
    onSelectionChanged: function (selectedItems) {
        purchaseorderrow = "";
        purchaseorderrow = selectedItems.selectedRowsData[0];
    },
    onContentReady: function (e) {
        e.component.selectRowsByIndexes([0]);
    }
});

$("#GridReceiptBatchDetails").dxDataGrid({
    dataSource: [],
    allowColumnReordering: true,
    allowColumnResizing: true,
    //columnAutoWidth: true,
    showBorders: true,
    showRowLines: true,
    columnResizingMode: "widget",
    selection: { mode: "single" },
    //keyExpr: "TransactionID",
    sorting: {
        mode: "none"
    },
    editing: {
        mode: "cell",
        allowDeleting: true,
        allowUpdating: true
    },
    scrolling: { mode: 'infinite' },
    rowAlternationEnabled: false,
    columns: [
        { dataField: "TransactionID", visible: false, caption: "PurchaseTransactionID", width: 120 },
        { dataField: "LedgerID", visible: false, caption: "LedgerID", width: 120 },
        { dataField: "ItemID", visible: false, caption: "ItemID", width: 120 },
        { dataField: "ItemGroupID", visible: false, caption: "ItemGroupID", width: 120 },
        { dataField: "PurchaseVoucherDate", visible: false, caption: "P.O. Date", width: 120 },
        { dataField: "PurchaseOrderQuantity", visible: false, caption: "P.O.Qty", width: 120 },
        { dataField: "PurchaseUnit", visible: false, caption: "Purchase Unit", width: 120 },
        { dataField: "PurchaseVoucherNo", visible: true, caption: "P.O. No.", width: 120 },
        { dataField: "ItemCode", visible: true, caption: "Item Code", width: 120 },
        { dataField: "ItemName", visible: true, caption: "Item Name", width: 400 },
        {
            dataField: "ReceiptQuantity", validationRules: [{ type: "required" }, { type: "numeric" }], caption: "Receipt Qty (S.U.)", visible: true, width: 120,
            //setCellValue: function (newData, value, currentRowData) {
            //    if (value === undefined || value === null || isNaN(value) === true) return false;
            //    newData.ReceiptQuantity = Number(value).toFixed(Number(currentRowData.UnitDecimalPlaceStockUnit));
            //    if (currentRowData.FormulaStockToPurchaseUnit === undefined || currentRowData.FormulaStockToPurchaseUnit === null) currentRowData.FormulaStockToPurchaseUnit = "";
            //    //newData.TotalPrice = currentRowData.Price * value;
            //    if (Number(currentRowData.ReceiptWtPerPacking) > 0) {
            //        newData.ReceiptQuantityInPurchaseUnit = Number(StockUnitConversion(currentRowData.FormulaStockToPurchaseUnit, value, currentRowData.UnitPerPacking, currentRowData.ReceiptWtPerPacking, currentRowData.ConversionFactor, currentRowData.SizeW, currentRowData.UnitDecimalPlacePurchaseUnit, currentRowData.StockUnit, currentRowData.PurchaseUnit, Number(currentRowData.GSM), Number(currentRowData.ReleaseGSM), Number(currentRowData.AdhesiveGSM), Number(currentRowData.Thickness), Number(currentRowData.Density)));
            //    } else {
            //        newData.ReceiptQuantityInPurchaseUnit = Number(StockUnitConversion(currentRowData.FormulaStockToPurchaseUnit, value, currentRowData.UnitPerPacking, currentRowData.WtPerPacking, currentRowData.ConversionFactor, currentRowData.SizeW, currentRowData.UnitDecimalPlacePurchaseUnit, currentRowData.StockUnit, currentRowData.PurchaseUnit, Number(currentRowData.GSM), Number(currentRowData.ReleaseGSM), Number(currentRowData.AdhesiveGSM), Number(currentRowData.Thickness), Number(currentRowData.Density)));
            //    }
            //}
        },
        {
            dataField: "ReceiptQuantityInPurchaseUnit", caption: "Receipt Qty (P.U.)", visible: true, width: 120,
            //calculateCellValue: function (rowData) {
            //    if (rowData.FormulaPurchaseToStockUnit === undefined || rowData.FormulaPurchaseToStockUnit === null) rowData.FormulaPurchaseToStockUnit = "";
            //    if (Number(rowData.ReceiptWtPerPacking) > 0) {
            //        return Number(StockUnitConversion(rowData.FormulaPurchaseToStockUnit, Number(rowData.PurchaseOrderQuantity), Number(rowData.UnitPerPacking), Number(rowData.ReceiptWtPerPacking), Number(rowData.ConversionFactor), Number(rowData.SizeW), Number(rowData.UnitDecimalPlaceStockUnit), rowData.PurchaseUnit, rowData.StockUnit, Number(rowData.GSM), Number(rowData.ReleaseGSM), Number(rowData.AdhesiveGSM), Number(rowData.Thickness), Number(rowData.Density)));
            //    } else {
            //        return Number(StockUnitConversion(rowData.FormulaPurchaseToStockUnit, Number(rowData.PurchaseOrderQuantity), Number(rowData.UnitPerPacking), Number(rowData.WtPerPacking), Number(rowData.ConversionFactor), Number(rowData.SizeW), Number(rowData.UnitDecimalPlaceStockUnit), rowData.PurchaseUnit, rowData.StockUnit, Number(rowData.GSM), Number(rowData.ReleaseGSM), Number(rowData.AdhesiveGSM), Number(rowData.Thickness), Number(rowData.Density)));
            //    }
            //
            //}
        },
        {
            dataField: "BatchNo", validationRules: [{ type: "required" }], caption: "Ref. Batch No.", visible: true, width: 120
        },

        { dataField: "SupplierBatchNo", caption: "Supp.Batch No.", allowEditing: true, visible: true, width: 120 },

        {
            dataField: "MfgDate", visible: true, allowEditing: true, caption: "Mfg.Date", width: 120,
            dataType: "date", format: "dd-MMM-yyyy",
            showEditorAlways: true
        },

        {
            dataField: "ExpiryDate", visible: true, allowEditing: true, caption: "Expiry Date", width: 120,
            dataType: "date", format: "dd-MMM-yyyy",
            showEditorAlways: true
        },
        { dataField: "StockUnit", visible: true, width: 80 },
        { dataField: "ReceiptWtPerPacking", visible: true, validationRules: [{ type: "required" }, { type: "numeric" }], caption: "Wt/Packing", width: 120 },
        {
            caption: "Select Warehouse", visible: true, allowEditing: false, width: 100,
            cellTemplate: function (container, info) {
                $('<div>').addClass('fa fa-plus customgridbtn')
                    .on('dxclick', function (e) {
                        this.setAttribute("data-toggle", "modal");
                        this.setAttribute("data-target", "#WarehouseSelectionModal");
                        //  $("p").text("Hello world!");
                    }).appendTo(container);
            }
        },
        { dataField: "WarehouseBin", visible: true, caption: "Warehouse-Bin", width: 200 },
        {
            dataField: "Warehouse", visible: false, caption: "Warehouse", width: 120
            //lookup: {
            //    dataSource: ResWarehouse,
            //    displayExpr: "Warehouse",
            //    valueExpr: "Warehouse"
            //    //keyExpr: "WarehouseID"
            //},
            //validationRules: [{ type: "required" }],
            //setCellValue: function (rowData, value) {
            //    rowData.Warehouse = value;
            //    if (value !== "") {
            //        RefreshBin(value);
            //    }
            //}
        },
        {
            dataField: "Bin", visible: false, width: 120
            //lookup: {
            //    dataSource: ResBin,
            //    displayExpr: "Bin",
            //    valueExpr: "WarehouseID"
            //},
            //validationRules: [{ type: "required" }],
            //setCellValue: function (rowData, value) {
            //    rowData.Bin = value;
            //    rowData.WarehouseID = 0;
            //    if (value > 0) {
            //        rowData.WarehouseID = value;
            //    }
            //}
        },
        { dataField: "PurchaseTolerance", visible: false, caption: "P.O. Tol.(%)", width: 120 }, //, width: 40 
        { dataField: "WtPerPacking", visible: false, caption: "WtPerPacking", width: 120 },  //, width: 40 
        { dataField: "UnitPerPacking", visible: false, caption: "UnitPerPacking", width: 120 },  //, width: 40 
        { dataField: "ConversionFactor", visible: false, caption: "ConversionFactor", width: 120 },  //, width: 40 
        { dataField: "SizeW", visible: false, caption: "SizeW", width: 120 },//, width: 40 
        { dataField: "WarehouseID", visible: false, caption: "WarehouseID", width: 120 },    //, width: 40 
        { dataField: "ItemGroupNameID", visible: false, caption: "ItemGroupNameID", width: 120 },
        { dataField: "FormulaPurchaseToStockUnit", visible: false, caption: "FormulaPurchaseToStockUnit" },
        { dataField: "UnitDecimalPlaceStockUnit", visible: false, caption: "UnitDecimalPlaceStockUnit" },
        { dataField: "FormulaStockToPurchaseUnit", visible: false, caption: "FormulaStockToPurchaseUnit" },
        { dataField: "UnitDecimalPlacePurchaseUnit", visible: false, caption: "UnitDecimalPlacePurchaseUnit" }
    ],
    height: function () {
        return window.innerHeight / 3.4;
    },
    onEditingStart: function (e) {
        if (e.column.dataField === "SupplierBatchNo" || e.column.dataField === "MfgDate" || e.column.dataField === "ExpiryDate" || e.column.dataField === "ReceiptQuantity" || e.column.dataField === "Warehouse" || e.column.dataField === "Bin" || e.column.dataField === "ReceiptWtPerPacking" || e.column.dataField === "ReceiptQuantityInPurchaseUnit") {// || e.column.dataField === "BatchNo"
            e.cancel = false;
        } else {
            e.cancel = true;
        }

        if (e.data.ItemGroupNameID === "-1" || e.data.ItemGroupNameID === -1) {
            if (e.column.dataField === "ReceiptWtPerPacking") {
                e.cancel = false;
            }
        }
        else {
            if (e.column.dataField === "ReceiptWtPerPacking") {
                e.cancel = true;
            }
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
    onRowRemoved: function (e) {
        var dataGrid = $("#GridReceiptBatchDetails").dxDataGrid('instance');
        var newData = [];
        objpub = {};

        receiptBatchDetail = receiptBatchDetail.filter(function (obj) {
            if (obj.BatchNo === e.key.BatchNo) {
                return false;
            }
            else {
                return true;
            }
        });

        if (dataGrid._options.dataSource.length === 0 || e.key.ReceiptQuantity === 0) {
            objpub.TransactionID = e.key.TransactionID;
            objpub.LedgerID = e.key.LedgerID;
            objpub.ItemID = e.key.ItemID;
            objpub.ItemGroupID = e.key.ItemGroupID;
            objpub.ItemGroupNameID = e.key.ItemGroupNameID;
            objpub.PurchaseVoucherNo = e.key.PurchaseVoucherNo;
            objpub.PurchaseVoucherDate = e.key.PurchaseVoucherDate;
            objpub.ItemCode = e.key.ItemCode;
            objpub.ItemName = e.key.ItemName;
            objpub.PurchaseOrderQuantity = e.key.PurchaseOrderQuantity;
            objpub.PurchaseUnit = e.key.PurchaseUnit;
            objpub.ChallanQuantity = e.key.ReceiptQuantity;
            objpub.ReceiptQuantityInPurchaseUnit = e.key.ReceiptQuantityInPurchaseUnit;
            objpub.BatchNo = e.key.BatchNo;

            objpub.SupplierBatchNo = e.key.SupplierBatchNo;
            objpub.MfgDate = e.key.MfgDate;
            objpub.ExpiryDate = e.key.ExpiryDate;

            objpub.StockUnit = e.key.StockUnit;
            objpub.Warehouse = "";//e.key.Warehouse
            objpub.Bin = "";//e.key.Bin
            objpub.WarehouseBin = "";
            objpub.PurchaseTolerance = e.key.PurchaseTolerance;
            objpub.ReceiptWtPerPacking = e.key.ReceiptWtPerPacking;
            objpub.WtPerPacking = e.key.WtPerPacking;
            objpub.UnitPerPacking = e.key.UnitPerPacking;
            objpub.ConversionFactor = e.key.ConversionFactor;
            objpub.SizeW = e.key.SizeW;
            objpub.WarehouseID = 0;//e.key.WarehouseID
            objpub.ReceiptQuantity = 0;
            objpub.FormulaPurchaseToStockUnit = e.key.FormulaPurchaseToStockUnit;
            objpub.UnitDecimalPlaceStockUnit = e.key.UnitDecimalPlaceStockUnit;
            objpub.FormulaStockToPurchaseUnit = e.key.FormulaStockToPurchaseUnit;
            objpub.UnitDecimalPlacePurchaseUnit = e.key.UnitDecimalPlacePurchaseUnit;

            dataGrid._options.dataSource.push(objpub);
        }

        $("#GridReceiptBatchDetails").dxDataGrid({
            dataSource: dataGrid._options.dataSource,
            columnAutoWidth: true
        });
        dataGrid.refresh();
    },
    onRowUpdated: function (e) {
        if (e.key.StockUnit.toUpperCase() === "SHEETS" || e.key.StockUnit.toUpperCase() === "SHEET" || e.key.StockUnit.toUpperCase() === "METER" || e.key.StockUnit.toUpperCase() === "MTR") {
            var textValue = Number(e.key.ReceiptQuantity);

            var decimal = /^[0-9]*$/;
            if (decimal.test(textValue) === false) {
                //DevExpress.ui.notify("Please enter only numeric value..!", "warning", 1200);
                showDevExpressNotification("Please enter only numeric value..!", "warning");
                e.key.ReceiptQuantity = 0;
            }
        }

        if (e.key !== undefined && e.key !== null) {

            updateBatchDetail(e.key);
        }
        /*
        var i = 0;
        var rowUpdated = false;
        for (i = 0; i < receiptBatchDetail.length; i++) {
            if (receiptBatchDetail[i].PurchaseTransactionID === e.key.TransactionID && receiptBatchDetail[i].ItemID === e.key.ItemID && receiptBatchDetail[i].BatchNo === e.key.BatchNo) {
                if (e.key.ReceiptQuantity === 0) {
                    e.key.ReceiptQuantity = receiptBatchDetail[i].ChallanQuantity;
                    e.key.ReceiptQuantityInPurchaseUnit = receiptBatchDetail[i].ReceiptQuantityInPurchaseUnit;
                } else if (e.key.ReceiptQuantity <= 0) {
                    e.key.ReceiptQuantity = receiptBatchDetail[i].ChallanQuantity;
                } else {
                    if (CheckQuantityTolerance(e.key.ReceiptQuantity, e.key.TransactionID, e.key.ItemID, e.key.BatchNo, e.key.PurchaseTolerance, e.key.WtPerPacking, e.key.UnitPerPacking, e.key.ConversionFactor, e.key.SizeW, e.key.PurchaseUnit.toString(), e.key.StockUnit.toString(), Number(e.key.GSM), Number(e.key.ReleaseGSM), Number(e.key.AdhesiveGSM), Number(e.key.Thickness), Number(e.key.Density))) {
                        receiptBatchDetail[i].ChallanQuantity = e.key.ReceiptQuantity;
                        receiptBatchDetail[i].ReceiptQuantityInPurchaseUnit = e.key.ReceiptQuantityInPurchaseUnit;
                    } else {
                        e.key.ReceiptQuantity = receiptBatchDetail[i].ChallanQuantity;
                        e.key.ReceiptQuantityInPurchaseUnit = Number(receiptBatchDetail[i].ReceiptQuantityInPurchaseUnit);
                        DevExpress.ui.notify("Please enter valid receipt quantity under tolerance limit..!", "warning", 1200);
                        return;
                    }

                }
                if (e.key.ReceiptWtPerPacking === 0) {
                    e.key.ReceiptWtPerPacking = receiptBatchDetail[i].ReceiptWtPerPacking;
                } else if (e.key.ReceiptWtPerPacking <= 0) {
                    e.key.ReceiptWtPerPacking = receiptBatchDetail[i].ReceiptWtPerPacking;
                } else {
                    e.key.ReceiptWtPerPacking = Number(e.key.ReceiptWtPerPacking).toFixed(6);
                    receiptBatchDetail[i].ReceiptWtPerPacking = Number(e.key.ReceiptWtPerPacking).toFixed(6);
                }
                receiptBatchDetail[i].Warehouse = e.key.Warehouse;
                receiptBatchDetail[i].Bin = e.key.Bin;
                receiptBatchDetail[i].WarehouseID = e.key.WarehouseID;

                receiptBatchDetail[i].SupplierBatchNo = e.key.SupplierBatchNo;
                receiptBatchDetail[i].MfgDate = e.key.MfgDate;
                receiptBatchDetail[i].ExpiryDate = e.key.ExpiryDate;

                rowUpdated = true;
            }
        }
        if (rowUpdated === false) {
            if (e.key.ReceiptQuantity > 0 && e.key.TransactionID > 0 && e.key.ItemID > 0 && e.key.Warehouse !== "" && e.key.Bin !== "" && e.key.WarehouseID > 0) {
                if (!CheckQuantityTolerance(e.key.ReceiptQuantity, e.key.TransactionID, e.key.ItemID, e.key.BatchNo, e.key.PurchaseTolerance, e.key.WtPerPacking, e.key.UnitPerPacking, e.key.ConversionFactor, e.key.SizeW, e.key.PurchaseUnit.toString(), e.key.StockUnit.toString(), Number(e.key.GSM), Number(e.key.ReleaseGSM), Number(e.key.AdhesiveGSM), Number(e.key.Thickness), Number(e.key.Density))) {
                    DevExpress.ui.notify("Please enter valid receipt quantity under tolerance limit..!", "warning", 1200);
                    e.key.ReceiptQuantity = 0;
                    return;
                }
                var objpub = {};
                objpub.PurchaseTransactionID = e.key.TransactionID;
                objpub.LedgerID = e.key.LedgerID;
                objpub.ItemID = e.key.ItemID;
                objpub.ItemGroupID = e.key.ItemGroupID;
                objpub.ItemGroupNameID = e.key.ItemGroupNameID;
                objpub.PurchaseVoucherNo = e.key.PurchaseVoucherNo;
                objpub.PurchaseVoucherDate = e.key.PurchaseVoucherDate;
                objpub.ItemCode = e.key.ItemCode;
                objpub.ItemName = e.key.ItemName;
                objpub.PurchaseOrderQuantity = e.key.PurchaseOrderQuantity;
                objpub.PurchaseUnit = e.key.PurchaseUnit;
                objpub.ChallanQuantity = e.key.ReceiptQuantity;
                objpub.ReceiptQuantityInPurchaseUnit = e.key.ReceiptQuantityInPurchaseUnit;
                objpub.BatchNo = e.key.BatchNo;

                objpub.SupplierBatchNo = e.key.SupplierBatchNo;
                objpub.MfgDate = e.key.MfgDate;
                objpub.ExpiryDate = e.key.ExpiryDate;

                objpub.StockUnit = e.key.StockUnit;
                objpub.Warehouse = e.key.Warehouse;
                objpub.Bin = e.key.Bin;
                objpub.PurchaseTolerance = e.key.PurchaseTolerance;
                e.key.ReceiptWtPerPacking = Number(e.key.ReceiptWtPerPacking).toFixed(6);
                objpub.ReceiptWtPerPacking = Number(e.key.ReceiptWtPerPacking).toFixed(6);
                objpub.WtPerPacking = e.key.WtPerPacking;
                objpub.UnitPerPacking = e.key.UnitPerPacking;
                objpub.ConversionFactor = e.key.ConversionFactor;
                objpub.SizeW = e.key.SizeW;
                objpub.WarehouseID = e.key.WarehouseID;
                objpub.FormulaPurchaseToStockUnit = e.key.FormulaPurchaseToStockUnit;
                objpub.UnitDecimalPlaceStockUnit = e.key.UnitDecimalPlaceStockUnit;
                objpub.FormulaStockToPurchaseUnit = e.key.FormulaStockToPurchaseUnit;
                objpub.UnitDecimalPlacePurchaseUnit = e.key.UnitDecimalPlacePurchaseUnit;
                objpub.GSM = e.key.GSM;
                objpub.ReleaseGSM = e.key.ReleaseGSM;
                objpub.AdhesiveGSM = e.key.AdhesiveGSM;
                objpub.Thickness = e.key.Thickness;
                objpub.Density = e.key.Density;
                receiptBatchDetail.push(objpub);

                //adding new row in batch details grid
                var x = $("#GridReceiptBatchDetails").dxDataGrid("instance");

                i = 0;
                var newData = [];
                objpub = {};
                if (receiptBatchDetail.length > 0) {
                    for (i = 0; i < receiptBatchDetail.length; i++) {
                        if (receiptBatchDetail[i].PurchaseTransactionID === e.key.TransactionID && receiptBatchDetail[i].ItemID === e.key.ItemID) {
                            objpub.TransactionID = receiptBatchDetail[i].PurchaseTransactionID;
                            objpub.LedgerID = receiptBatchDetail[i].LedgerID;
                            objpub.ItemID = receiptBatchDetail[i].ItemID;
                            objpub.ItemGroupID = receiptBatchDetail[i].ItemGroupID;
                            objpub.ItemGroupNameID = receiptBatchDetail[i].ItemGroupNameID;
                            objpub.PurchaseVoucherNo = receiptBatchDetail[i].PurchaseVoucherNo;
                            objpub.PurchaseVoucherDate = receiptBatchDetail[i].PurchaseVoucherDate;
                            objpub.ItemCode = receiptBatchDetail[i].ItemCode;
                            objpub.ItemName = receiptBatchDetail[i].ItemName;
                            objpub.PurchaseOrderQuantity = receiptBatchDetail[i].PurchaseOrderQuantity;
                            objpub.PurchaseUnit = receiptBatchDetail[i].PurchaseUnit;
                            objpub.ReceiptQuantity = receiptBatchDetail[i].ChallanQuantity;
                            objpub.ReceiptQuantityInPurchaseUnit = receiptBatchDetail[i].ReceiptQuantityInPurchaseUnit;
                            objpub.BatchNo = receiptBatchDetail[i].BatchNo;
                            objpub.SupplierBatchNo = receiptBatchDetail[i].SupplierBatchNo;
                            objpub.MfgDate = receiptBatchDetail[i].MfgDate;
                            objpub.ExpiryDate = receiptBatchDetail[i].ExpiryDate;
                            objpub.StockUnit = receiptBatchDetail[i].StockUnit;
                            objpub.Warehouse = receiptBatchDetail[i].Warehouse;
                            objpub.Bin = receiptBatchDetail[i].Bin;
                            objpub.PurchaseTolerance = receiptBatchDetail[i].PurchaseTolerance;
                            objpub.ReceiptWtPerPacking = receiptBatchDetail[i].ReceiptWtPerPacking;
                            objpub.WtPerPacking = receiptBatchDetail[i].WtPerPacking;
                            objpub.UnitPerPacking = receiptBatchDetail[i].UnitPerPacking;
                            objpub.ConversionFactor = receiptBatchDetail[i].ConversionFactor;
                            objpub.SizeW = receiptBatchDetail[i].SizeW;
                            objpub.WarehouseID = receiptBatchDetail[i].WarehouseID;
                            objpub.FormulaPurchaseToStockUnit = receiptBatchDetail[i].FormulaPurchaseToStockUnit;
                            objpub.UnitDecimalPlaceStockUnit = receiptBatchDetail[i].UnitDecimalPlaceStockUnit;
                            objpub.FormulaStockToPurchaseUnit = receiptBatchDetail[i].FormulaStockToPurchaseUnit;
                            objpub.UnitDecimalPlacePurchaseUnit = receiptBatchDetail[i].UnitDecimalPlacePurchaseUnit;
                            objpub.GSM = receiptBatchDetail[i].GSM;
                            objpub.ReleaseGSM = receiptBatchDetail[i].ReleaseGSM;
                            objpub.AdhesiveGSM = receiptBatchDetail[i].AdhesiveGSM;
                            objpub.Thickness = receiptBatchDetail[i].Thickness;
                            objpub.Density = receiptBatchDetail[i].Density;

                            newData.push(objpub);
                            objpub = {};
                        }
                    }
                }
                objpub.TransactionID = e.key.TransactionID;
                objpub.LedgerID = e.key.LedgerID;
                objpub.ItemID = e.key.ItemID;
                objpub.ItemGroupID = e.key.ItemGroupID;
                objpub.ItemGroupNameID = e.key.ItemGroupNameID;
                objpub.PurchaseVoucherNo = e.key.PurchaseVoucherNo;
                objpub.PurchaseVoucherDate = e.key.PurchaseVoucherDate;
                objpub.ItemCode = e.key.ItemCode;
                objpub.ItemName = e.key.ItemName;
                objpub.PurchaseOrderQuantity = e.key.PurchaseOrderQuantity;
                objpub.PurchaseUnit = e.key.PurchaseUnit;
                objpub.StockUnit = e.key.StockUnit;
                objpub.SupplierBatchNo = e.key.SupplierBatchNo;
                objpub.MfgDate = e.key.MfgDate;
                objpub.ExpiryDate = e.key.ExpiryDate;

                objpub.PurchaseTolerance = e.key.PurchaseTolerance;
                objpub.ReceiptWtPerPacking = e.key.WtPerPacking;
                objpub.WtPerPacking = e.key.WtPerPacking;
                objpub.UnitPerPacking = e.key.UnitPerPacking;
                objpub.ConversionFactor = e.key.ConversionFactor;
                objpub.SizeW = e.key.SizeW;
                objpub.FormulaPurchaseToStockUnit = e.key.FormulaPurchaseToStockUnit;
                objpub.UnitDecimalPlaceStockUnit = e.key.UnitDecimalPlaceStockUnit;
                objpub.FormulaStockToPurchaseUnit = e.key.FormulaStockToPurchaseUnit;
                objpub.UnitDecimalPlacePurchaseUnit = e.key.UnitDecimalPlacePurchaseUnit;
                objpub.GSM = e.key.GSM;
                objpub.ReleaseGSM = e.key.ReleaseGSM;
                objpub.AdhesiveGSM = e.key.AdhesiveGSM;
                objpub.Thickness = e.key.Thickness;
                objpub.Density = e.key.Density;

                if (newData.length > 0) {
                    if (FlagEdit === false) {
                        objpub.BatchNo = '_' + newData[newData.length - 1].PurchaseVoucherNo + '_' + newData[newData.length - 1].ItemID + '_' + (newData.length + 1)
                        objpub.ReceiptQuantity = 0;
                        objpub.ReceiptQuantityInPurchaseUnit = 0;
                    } else {
                        objpub.BatchNo = TransactionID + '_' + newData[newData.length - 1].PurchaseVoucherNo + '_' + newData[newData.length - 1].ItemID + '_' + (newData.length + 1)
                        objpub.ReceiptQuantity = 0;
                        objpub.ReceiptQuantityInPurchaseUnit = 0;
                    }
                    objpub.WarehouseID = newData[newData.length - 1].WarehouseID;
                    objpub.Warehouse = newData[newData.length - 1].Warehouse;
                    objpub.Bin = newData[newData.length - 1].WarehouseID;
                } else {
                    objpub.ReceiptQuantity = 0;
                    objpub.ReceiptQuantityInPurchaseUnit = 0;
                    objpub.WarehouseID = 0;
                    objpub.Warehouse = "";
                    objpub.Bin = "";
                    objpub.BatchNo = '_' + objpub.PurchaseVoucherNo + '_' + objpub.ItemID + '_1';
                }

                newData.push(objpub);

                $("#GridReceiptBatchDetails").dxDataGrid({ dataSource: newData });
            }
        }*/
    },
    summary: {
        totalItems: [{
            column: "ReceiptQuantity",
            summaryType: "sum",
            displayFormat: "Ttl: {0}"
        }]
    }
});

function updateBatchDetail(keyObj) {

    let i = 0;
    let rowUpdated = false;


    for (i = 0; i < receiptBatchDetail.length; i++) {
        if (receiptBatchDetail[i].PurchaseTransactionID === keyObj.TransactionID && receiptBatchDetail[i].ItemID === keyObj.ItemID && receiptBatchDetail[i].BatchNo === keyObj.BatchNo) {
            if (keyObj.ReceiptQuantity === 0) {
                keyObj.ReceiptQuantity = receiptBatchDetail[i].ChallanQuantity;
                keyObj.ReceiptQuantityInPurchaseUnit = receiptBatchDetail[i].ReceiptQuantityInPurchaseUnit;
            } else if (keyObj.ReceiptQuantity <= 0) {
                keyObj.ReceiptQuantity = receiptBatchDetail[i].ChallanQuantity;
            } else {
                CheckQuantityTolerance(keyObj.ReceiptQuantity, keyObj.TransactionID, keyObj.ItemID, keyObj.BatchNo, keyObj.PurchaseTolerance, keyObj.WtPerPacking, keyObj.UnitPerPacking, keyObj.ConversionFactor, keyObj.SizeW, keyObj.PurchaseUnit.toString(), keyObj.StockUnit.toString(), Number(keyObj.GSM), Number(keyObj.ReleaseGSM), Number(keyObj.AdhesiveGSM), Number(keyObj.Thickness), Number(keyObj.Density))
                if (flag == false) {
                    receiptBatchDetail[i].ChallanQuantity = keyObj.ReceiptQuantity;
                    receiptBatchDetail[i].ReceiptQuantityInPurchaseUnit = keyObj.ReceiptQuantityInPurchaseUnit;
                } else {
                    keyObj.ReceiptQuantity = receiptBatchDetail[i].ChallanQuantity;
                    keyObj.ReceiptQuantityInPurchaseUnit = Number(receiptBatchDetail[i].ReceiptQuantityInPurchaseUnit);
                    //DevExpress.ui.notify("Please enter valid receipt quantity under tolerance limit..!", "warning", 2500);
                    showDevExpressNotification("Please enter valid receipt quantity under tolerance limit..!", "warning");
                    return;
                }

            }
            if (keyObj.ReceiptWtPerPacking === 0) {
                keyObj.ReceiptWtPerPacking = receiptBatchDetail[i].ReceiptWtPerPacking;
            } else if (keyObj.ReceiptWtPerPacking <= 0) {
                keyObj.ReceiptWtPerPacking = receiptBatchDetail[i].ReceiptWtPerPacking;
            } else {
                keyObj.ReceiptWtPerPacking = Number(keyObj.ReceiptWtPerPacking).toFixed(6);
                receiptBatchDetail[i].ReceiptWtPerPacking = Number(keyObj.ReceiptWtPerPacking).toFixed(6);
            }
            receiptBatchDetail[i].Warehouse = keyObj.Warehouse;
            receiptBatchDetail[i].Bin = keyObj.Bin;
            receiptBatchDetail[i].WarehouseID = keyObj.WarehouseID;

            receiptBatchDetail[i].SupplierBatchNo = (keyObj.SupplierBatchNo === "" || keyObj.SupplierBatchNo === null || keyObj.SupplierBatchNo === undefined || keyObj.SupplierBatchNo === "undefined") ? "" : keyObj.SupplierBatchNo;
            receiptBatchDetail[i].MfgDate = keyObj.MfgDate;
            receiptBatchDetail[i].ExpiryDate = keyObj.ExpiryDate;
            receiptBatchDetail[i].SupplierBatchNo = keyObj.SupplierBatchNo;
            const index = i; // Index of the row you want to update
            console.log(keyObj.SupplierBatchNo)
            const newValue = keyObj.SupplierBatchNo; // New value you want to set
            console.log(newData)

            updateSupplierBatchNo(newData, index, newValue);
            console.log(newData)
            //newData[i].SupplierBatchNo = (keyObj.SupplierBatchNo === "" || keyObj.SupplierBatchNo === null || keyObj.SupplierBatchNo === undefined || keyObj.SupplierBatchNo === "undefined") ? "" : keyObj.SupplierBatchNo;
            rowUpdated = true;
        }
    }
    if (rowUpdated === false) {
        if (keyObj.ReceiptQuantity > 0 && keyObj.TransactionID > 0 && keyObj.ItemID > 0 && keyObj.Warehouse !== "" && keyObj.Bin !== "" && keyObj.WarehouseID > 0) {
            CheckQuantityTolerance(keyObj.ReceiptQuantity, keyObj.TransactionID, keyObj.ItemID, keyObj.BatchNo, keyObj.PurchaseTolerance, keyObj.WtPerPacking, keyObj.UnitPerPacking, keyObj.ConversionFactor, keyObj.SizeW, keyObj.PurchaseUnit.toString(), keyObj.StockUnit.toString(), Number(keyObj.GSM), Number(keyObj.ReleaseGSM), Number(keyObj.AdhesiveGSM), Number(keyObj.Thickness), Number(keyObj.Density))
            if (flag == true) {
                //DevExpress.ui.notify("Please enter valid receipt quantity under tolerance limit..!", "warning", 1200);
                showDevExpressNotification("Please enter valid receipt quantity under tolerance limit..!", "warning");
                keyObj.ReceiptQuantity = 0;
                return;
            }
            var objpub = {};
            objpub.PurchaseTransactionID = keyObj.TransactionID;
            objpub.LedgerID = keyObj.LedgerID;
            objpub.ItemID = keyObj.ItemID;
            objpub.ItemGroupID = keyObj.ItemGroupID;
            objpub.ItemGroupNameID = keyObj.ItemGroupNameID;
            objpub.PurchaseVoucherNo = keyObj.PurchaseVoucherNo;
            objpub.PurchaseVoucherDate = keyObj.PurchaseVoucherDate;
            objpub.ItemCode = keyObj.ItemCode;
            objpub.ItemName = keyObj.ItemName;
            objpub.PurchaseOrderQuantity = keyObj.PurchaseOrderQuantity;
            objpub.PurchaseUnit = keyObj.PurchaseUnit;
            objpub.ChallanQuantity = keyObj.ReceiptQuantity;
            objpub.ReceiptQuantityInPurchaseUnit = keyObj.ReceiptQuantityInPurchaseUnit;
            objpub.BatchNo = keyObj.BatchNo;

            objpub.SupplierBatchNo = keyObj.SupplierBatchNo;
            objpub.MfgDate = keyObj.MfgDate;
            objpub.ExpiryDate = keyObj.ExpiryDate;

            objpub.StockUnit = keyObj.StockUnit;
            objpub.Warehouse = keyObj.Warehouse;
            objpub.Bin = keyObj.Bin;
            objpub.PurchaseTolerance = keyObj.PurchaseTolerance;
            keyObj.ReceiptWtPerPacking = Number(keyObj.ReceiptWtPerPacking).toFixed(6);
            objpub.ReceiptWtPerPacking = Number(keyObj.ReceiptWtPerPacking).toFixed(6);
            objpub.WtPerPacking = keyObj.WtPerPacking;
            objpub.UnitPerPacking = keyObj.UnitPerPacking;
            objpub.ConversionFactor = keyObj.ConversionFactor;
            objpub.SizeW = keyObj.SizeW;
            objpub.WarehouseID = keyObj.WarehouseID;
            objpub.FormulaPurchaseToStockUnit = keyObj.FormulaPurchaseToStockUnit;
            objpub.UnitDecimalPlaceStockUnit = keyObj.UnitDecimalPlaceStockUnit;
            objpub.FormulaStockToPurchaseUnit = keyObj.FormulaStockToPurchaseUnit;
            objpub.UnitDecimalPlacePurchaseUnit = keyObj.UnitDecimalPlacePurchaseUnit;
            objpub.GSM = keyObj.GSM;
            objpub.ReleaseGSM = keyObj.ReleaseGSM;
            objpub.AdhesiveGSM = keyObj.AdhesiveGSM;
            objpub.Thickness = keyObj.Thickness;
            objpub.Density = keyObj.Density;
            receiptBatchDetail.push(objpub);

            //adding new row in batch details grid
            var x = $("#GridReceiptBatchDetails").dxDataGrid("instance");

            i = 0;
            newData = [];
            objpub = {};
            if (receiptBatchDetail.length > 0) {
                for (i = 0; i < receiptBatchDetail.length; i++) {
                    if (receiptBatchDetail[i].PurchaseTransactionID === keyObj.TransactionID && receiptBatchDetail[i].ItemID === keyObj.ItemID) {
                        objpub.TransactionID = receiptBatchDetail[i].PurchaseTransactionID;
                        objpub.LedgerID = receiptBatchDetail[i].LedgerID;
                        objpub.ItemID = receiptBatchDetail[i].ItemID;
                        objpub.ItemGroupID = receiptBatchDetail[i].ItemGroupID;
                        objpub.ItemGroupNameID = receiptBatchDetail[i].ItemGroupNameID;
                        objpub.PurchaseVoucherNo = receiptBatchDetail[i].PurchaseVoucherNo;
                        objpub.PurchaseVoucherDate = receiptBatchDetail[i].PurchaseVoucherDate;
                        objpub.ItemCode = receiptBatchDetail[i].ItemCode;
                        objpub.ItemName = receiptBatchDetail[i].ItemName;
                        objpub.PurchaseOrderQuantity = receiptBatchDetail[i].PurchaseOrderQuantity;
                        objpub.PurchaseUnit = receiptBatchDetail[i].PurchaseUnit;
                        objpub.ReceiptQuantity = receiptBatchDetail[i].ChallanQuantity;
                        objpub.ReceiptQuantityInPurchaseUnit = receiptBatchDetail[i].ReceiptQuantityInPurchaseUnit;
                        objpub.BatchNo = receiptBatchDetail[i].BatchNo;
                        objpub.SupplierBatchNo = receiptBatchDetail[i].SupplierBatchNo;
                        objpub.MfgDate = receiptBatchDetail[i].MfgDate;
                        objpub.ExpiryDate = receiptBatchDetail[i].ExpiryDate;
                        objpub.StockUnit = receiptBatchDetail[i].StockUnit;
                        objpub.Warehouse = receiptBatchDetail[i].Warehouse;
                        objpub.Bin = receiptBatchDetail[i].Bin;
                        objpub.WarehouseBin = receiptBatchDetail[i].Warehouse + ' - ' + receiptBatchDetail[i].Bin;
                        objpub.PurchaseTolerance = receiptBatchDetail[i].PurchaseTolerance;
                        objpub.ReceiptWtPerPacking = receiptBatchDetail[i].ReceiptWtPerPacking;
                        objpub.WtPerPacking = receiptBatchDetail[i].WtPerPacking;
                        objpub.UnitPerPacking = receiptBatchDetail[i].UnitPerPacking;
                        objpub.ConversionFactor = receiptBatchDetail[i].ConversionFactor;
                        objpub.SizeW = receiptBatchDetail[i].SizeW;
                        objpub.WarehouseID = receiptBatchDetail[i].WarehouseID;
                        objpub.FormulaPurchaseToStockUnit = receiptBatchDetail[i].FormulaPurchaseToStockUnit;
                        objpub.UnitDecimalPlaceStockUnit = receiptBatchDetail[i].UnitDecimalPlaceStockUnit;
                        objpub.FormulaStockToPurchaseUnit = receiptBatchDetail[i].FormulaStockToPurchaseUnit;
                        objpub.UnitDecimalPlacePurchaseUnit = receiptBatchDetail[i].UnitDecimalPlacePurchaseUnit;
                        objpub.GSM = receiptBatchDetail[i].GSM;
                        objpub.ReleaseGSM = receiptBatchDetail[i].ReleaseGSM;
                        objpub.AdhesiveGSM = receiptBatchDetail[i].AdhesiveGSM;
                        objpub.Thickness = receiptBatchDetail[i].Thickness;
                        objpub.Density = receiptBatchDetail[i].Density;

                        newData.push(objpub);
                        objpub = {};
                    }
                }
            }
            objpub.TransactionID = keyObj.TransactionID;
            objpub.LedgerID = keyObj.LedgerID;
            objpub.ItemID = keyObj.ItemID;
            objpub.ItemGroupID = keyObj.ItemGroupID;
            objpub.ItemGroupNameID = keyObj.ItemGroupNameID;
            objpub.PurchaseVoucherNo = keyObj.PurchaseVoucherNo;
            objpub.PurchaseVoucherDate = keyObj.PurchaseVoucherDate;
            objpub.ItemCode = keyObj.ItemCode;
            objpub.ItemName = keyObj.ItemName;
            objpub.PurchaseOrderQuantity = keyObj.PurchaseOrderQuantity;
            objpub.PurchaseUnit = keyObj.PurchaseUnit;
            objpub.StockUnit = keyObj.StockUnit;
            objpub.SupplierBatchNo = keyObj.SupplierBatchNo;
            objpub.MfgDate = keyObj.MfgDate;
            objpub.ExpiryDate = keyObj.ExpiryDate;

            objpub.PurchaseTolerance = keyObj.PurchaseTolerance;
            objpub.ReceiptWtPerPacking = keyObj.WtPerPacking;
            objpub.WtPerPacking = keyObj.WtPerPacking;
            objpub.UnitPerPacking = keyObj.UnitPerPacking;
            objpub.ConversionFactor = keyObj.ConversionFactor;
            objpub.SizeW = keyObj.SizeW;
            objpub.FormulaPurchaseToStockUnit = keyObj.FormulaPurchaseToStockUnit;
            objpub.UnitDecimalPlaceStockUnit = keyObj.UnitDecimalPlaceStockUnit;
            objpub.FormulaStockToPurchaseUnit = keyObj.FormulaStockToPurchaseUnit;
            objpub.UnitDecimalPlacePurchaseUnit = keyObj.UnitDecimalPlacePurchaseUnit;
            objpub.GSM = keyObj.GSM;
            objpub.ReleaseGSM = keyObj.ReleaseGSM;
            objpub.AdhesiveGSM = keyObj.AdhesiveGSM;
            objpub.Thickness = keyObj.Thickness;
            objpub.Density = keyObj.Density;

            if (newData.length > 0) {
                if (FlagEdit === false) {
                    objpub.BatchNo = '_' + newData[newData.length - 1].PurchaseVoucherNo + '_' + newData[newData.length - 1].ItemID + '_' + (newData.length + 1)
                    objpub.ReceiptQuantity = 0;
                    objpub.ReceiptQuantityInPurchaseUnit = 0;
                } else {
                    objpub.BatchNo = TransactionID + '_' + newData[newData.length - 1].PurchaseVoucherNo + '_' + newData[newData.length - 1].ItemID + '_' + (newData.length + 1)
                    objpub.ReceiptQuantity = 0;
                    objpub.ReceiptQuantityInPurchaseUnit = 0;
                }
                objpub.WarehouseID = newData[newData.length - 1].WarehouseID;
                objpub.Warehouse = newData[newData.length - 1].Warehouse;
                objpub.Bin = newData[newData.length - 1].Bin;
                objpub.WarehouseBin = newData[newData.length - 1].Warehouse + ' - ' + newData[newData.length - 1].Bin;
            } else {
                objpub.ReceiptQuantity = 0;
                objpub.ReceiptQuantityInPurchaseUnit = 0;
                objpub.WarehouseID = 0;
                objpub.Warehouse = "";
                objpub.Bin = "";
                objpub.BatchNo = '_' + objpub.PurchaseVoucherNo + '_' + objpub.ItemID + '_1';
                objpub.WarehouseBin = "";
            }

            newData.push(objpub);
            console.log(newData);

            $("#GridReceiptBatchDetails").dxDataGrid({ dataSource: newData });
        }
    }
}
function updateSupplierBatchNo(newData, index, newValue) {
    if (index >= 0 && index < newData.length) {
        newData[index].SupplierBatchNo = newValue;
        console.log('hello')
        console.log(newData);
        $("#GridReceiptBatchDetails").dxDataGrid({ dataSource: newData });

    } else {
        console.error("Index out of range");
    }
}
//function CheckQuantityTolerance(ReceiptQUantity, PurchaseTransactionID, ItemID, ReceiptBatchNo, Tolerance, WtPerPacking, UnitPerPacking, ConversionFactor, SizeW, BaseUnit, ConversionUnit, PaperGSM, ReleaseGSM, AdhesiveGSM, Thickness, Density) {
//    var flag = false;
//    try {
//        $.ajax({
//            type: "POST",
//            async: false,
//            url: "WebServicePurchaseGRN.asmx/GetPreviousReceivedQuantity",
//            data: '{PurchaseTransactionID:' + PurchaseTransactionID + ',ItemID:' + ItemID + ',GRNTransactionID:' + TransactionID + '}',
//            contentType: "application/json; charset=utf-8",
//            dataType: "text",
//            success: function (results) {
//                var res = results.replace(/\\/g, '');
//                res = res.replace(/"d":""/g, '');
//                res = res.replace(/""/g, '');
//                res = res.replace(/:,/g, ":null,");
//                res = res.replace(/,}/g, ",null}");
//                res = res.substr(1);
//                res = res.slice(0, -1);
//                var receivers = JSON.parse(res);
//                var poquantity = receivers[0].PurchaseOrderQuantity;
//                var stockunitpoquantity = 0;
//                var prereceiptquantity = receivers[0].PreReceiptQuantity;
//                var Conversionformula = receivers[0].FormulaPurchaseToStockUnit;
//                var UnitDecimalPlace = receivers[0].UnitDecimalPlaceStockUnit;
//                var currentreceiptquantity = 0;
//                var totalreceiptquantity = 0;
//                var results1 = $.grep(receiptBatchDetail, function (e) { return (e.PurchaseTransactionID === PurchaseTransactionID && e.ItemID === ItemID && e.BatchNo !== ReceiptBatchNo); });
//                if (results1.length > 0) {
//                    for (var x = 0; x < results1.length; x++) {
//                        // currentreceiptquantity = currentreceiptquantity + Number(results[x].ReceiptQuantity);
//                        currentreceiptquantity = Number(currentreceiptquantity) + Number(results1[x].ChallanQuantity);//Updated By Pradeep Yadav 14 Oct 2019
//                    }
//                }
//                totalreceiptquantity = Number(currentreceiptquantity) + Number(ReceiptQUantity) + Number(prereceiptquantity);
//                stockunitpoquantity = StockUnitConversion("", poquantity, UnitPerPacking, WtPerPacking, ConversionFactor, SizeW, UnitDecimalPlace, BaseUnit, ConversionUnit, PaperGSM, ReleaseGSM, AdhesiveGSM, Thickness, Density);
//                //flag = Number(totalreceiptquantity) <= Number(Number(stockunitpoquantity) + Number(Number(stockunitpoquantity) * (Number(Tolerance) / 100)));
//                flag = Number(totalreceiptquantity) <= Number(Number(stockunitpoquantity) + Number(Number(stockunitpoquantity) * (Number(Tolerance) / 100))) + 1;
//                //if (Conversionformula !== "" && Conversionformula !== null && Conversionformula !== undefined && Conversionformula !== "undefined") {
//                //    Conversionformula = Conversionformula.split('e.').join('');
//                //    Conversionformula = Conversionformula.replace("Quantity", "poquantity");
//                //    var n = Conversionformula.search("WtPerPacking");
//                //    if (n > 0) {
//                //        if (Number(WtPerPacking) > 0) {
//                //            stockunitpoquantity = eval(Conversionformula);
//                //            flag = Number(totalreceiptquantity) <= Number(Number(stockunitpoquantity) + Number(Number(stockunitpoquantity) * (Number(Tolerance) / 100)));
//                //            //pendingQty = (Number(PurchaseQty) - Number(pendingQty)).toFixed(Number(UnitDecimalPlace));
//                //        }
//                //    } else {
//                //        n = Conversionformula.search("SizeW");
//                //        if (n > 0) {
//                //            if (Number(SizeW) > 0) {
//                //                stockunitpoquantity = eval(Conversionformula);
//                //                flag = Number(totalreceiptquantity) <= Number(Number(stockunitpoquantity) + (Number(stockunitpoquantity) * (Number(Tolerance) / 100)));
//                //            }
//                //        } else {
//                //            stockunitpoquantity = eval(Conversionformula);
//                //            flag = Number(totalreceiptquantity) <= Number(Number(stockunitpoquantity) + (Number(stockunitpoquantity) * (Number(Tolerance) / 100)));
//                //        }
//                //    }
//                //} else {
//                //    flag = Number(totalreceiptquantity) <= Number(poquantity) + Number(Number(poquantity) * (Number(Tolerance) / 100));
//                //}
//            }
//        });
//    } catch (e) {
//        console.log(e);
//    }
//    return flag;
//}


function CheckQuantityTolerance(ReceiptQUantity, PurchaseTransactionID, ItemID, ReceiptBatchNo, Tolerance, WtPerPacking, UnitPerPacking, ConversionFactor, SizeW, BaseUnit, ConversionUnit, PaperGSM, ReleaseGSM, AdhesiveGSM, Thickness, Density) {
    try {
        $.ajax({
            type: "POST",
            async: false,
            url: "WebServicePurchaseGRN.asmx/GetPreviousReceivedQuantity",
            data: '{PurchaseTransactionID:' + PurchaseTransactionID + ',ItemID:' + ItemID + ',GRNTransactionID:' + TransactionID + '}',
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
                var poquantity = receivers[0].PurchaseOrderQuantity;
                var stockunitpoquantity = 0;
                var prereceiptquantity = receivers[0].PreReceiptQuantity;
                var Conversionformula = receivers[0].FormulaPurchaseToStockUnit;
                var UnitDecimalPlace = receivers[0].UnitDecimalPlaceStockUnit;
                var currentreceiptquantity = 0;
                var totalreceiptquantity = 0;
                var results1 = $.grep(receiptBatchDetail, function (e) { return (e.PurchaseTransactionID === PurchaseTransactionID && e.ItemID === ItemID && e.BatchNo !== ReceiptBatchNo); });
                if (results1.length > 0) {
                    for (var x = 0; x < results1.length; x++) {
                        // currentreceiptquantity = currentreceiptquantity + Number(results[x].ReceiptQuantity);
                        currentreceiptquantity = Number(currentreceiptquantity) + Number(results1[x].ChallanQuantity);//Updated By Pradeep Yadav 14 Oct 2019
                    }
                }
                totalreceiptquantity = Number(currentreceiptquantity) + Number(ReceiptQUantity) + Number(prereceiptquantity);
                stockunitpoquantity = StockUnitConversion("", poquantity, UnitPerPacking, WtPerPacking, ConversionFactor, SizeW, UnitDecimalPlace, BaseUnit, ConversionUnit, PaperGSM, ReleaseGSM, AdhesiveGSM, Thickness, Density);


                if (isUserAllowdExcessQty === true) {
                    flag = Number(totalreceiptquantity) > Number(stockunitpoquantity) + (Number(stockunitpoquantity) * (Number(Tolerance) / 100));
                    if (flag) {
                        var isConfirm = window.confirm("Do you want to add excess quantity? You are about to add more than the pending quantity!");
                        if (isConfirm) {
                            flag = false;
                            console.log("User confirmed. Flag is set to true.");
                        } else {
                            flag = true;
                            console.log("User canceled. Flag is set to false.");
                        }
                    }

                }
                else {
                    return flag = Number(totalreceiptquantity) > Number(Number(stockunitpoquantity) + Number(Number(stockunitpoquantity) * (Number(Tolerance) / 100)));
                }

            }
        });
    } catch (e) {
        console.log(e);
    }

}

try {
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
                items: receivers
            });
        }
    });
} catch (e) {
    console.log(e);
}

//refreshTransporter();
//function refreshTransporter() {
//    try {
//        $.ajax({
//            type: "POST",
//            url: "WebServicePurchaseGRN.asmx/GetPurchaseSuppliersList",
//            data: '{}',
//            contentType: "application/json; charset=utf-8",
//            dataType: "text",
//            success: function (results) {
//                
//                var res = results.replace(/\\/g, '');
//                res = res.replace(/"d":""/g, '');
//                res = res.replace(/""/g, '');
//                res = res.substr(1);
//                res = res.slice(0, -1);
//                var transporters = JSON.parse(res);

//                $("#sel_Transporter").dxSelectBox({
//                    items: transporters,
//                    placeholder: "Select Transporter",
//                    displayExpr: 'LedgerName',
//                    valueExpr: 'LedgerID',
//                    searchEnabled: true,
//                    showClearButton: true,
//                    onValueChanged: function (data) {
//                        //alert(data.value);
//                    }, 
//                });
//            },
//        });

//    } catch (e) {
//        alert(e);
//    }

//}

try {
    $.ajax({
        type: "POST",
        url: "WebServicePurchaseGRN.asmx/GetWarehouseList",
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
            ResWarehouse = JSON.parse(res);
            $("#SelWarehouse").dxSelectBox({
                items: ResWarehouse
            });
            if (ResWarehouse.length == 1) {
                $("#SelWarehouse").dxSelectBox({
                    value: ResWarehouse[0].Warehouse
                });
            }
        }
    });
} catch (e) {
    console.log(e);
}

function RefreshBin(value) {
    try {
        $.ajax({
            async: false,
            type: "POST",
            url: "WebServicePurchaseGRN.asmx/GetBinsList",
            data: '{warehousename:' + JSON.stringify(value) + '}',
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
                ResBin = JSON.parse(res);

                //var lookup = GridReceiptBatchDetails.columnOption("Bin", "lookup");
                //lookup.dataSource = ResBin;
                //GridReceiptBatchDetails.columnOption("Bin", "lookup", lookup);
                //GridReceiptBatchDetails.repaint();
            }
        });
    } catch (e) {
        console.log(e);
    }
}

$("#BtnSave").click(async function () {
    var i = 0;
    var prefix = "REC";
    var voucherid = -14;
    let PODate1 = [];
    if (Number(SelectedProductionUnitID) != 0) {
        if (GBLProductionUnitID != SelectedProductionUnitID) {
            swal("Attention!", "Selected transaction is related to unit " + SelectedProductionUnitName + ", Kindly login with unit " + SelectedProductionUnitName + " to process.", "warning");
            $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
            return;
        }
    }
    if (document.getElementById("TxtDnNo").value.trim() === "") {
        //DevExpress.ui.notify("Please enter valid delivery note no. to create receipt note..!", "warning", 1200);
        showDevExpressNotification("Please enter delivery note no. to create receipt note..!", "warning");
        document.getElementById("TxtDnNo").focus();
        return;
    }
    if ($("#sel_Receiver").dxSelectBox('instance').option('value') === "" || $("#sel_Receiver").dxSelectBox('instance').option('value') === "undefined" || $("#sel_Receiver").dxSelectBox('instance').option('value') === null) {
        //DevExpress.ui.notify("Please select received by to create receipt note..!", "warning", 1200);
        showDevExpressNotification("Please select received by to create receipt note..!", "warning");
        return;
    }

    try {
        if (receiptBatchDetail.length <= 0) {
            //DevExpress.ui.notify("Please enter batch details to create receipt note..!", "warning", 1200);
            showDevExpressNotification("Please enter batch details to create receipt note..!", "warning");
            return;
        }
        var dataGrid1 = $("#GridPurchaseOrders").dxDataGrid('instance');
        for (i = 0; i < dataGrid1.totalCount(); i++) {
            var result = $.grep(receiptBatchDetail, function (e) { return e.ItemID === dataGrid1.cellValue(i, "ItemID") && e.PurchaseTransactionID === dataGrid1.cellValue(i, "TransactionID"); });
            if (result.length === 0) {
                //Not found
                //DevExpress.ui.notify("Please enter valid receipt batch details against purchase order no. '" + dataGrid1.cellValue(i, "PurchaseVoucherNo") + "' and item name '" + dataGrid1.cellValue(i, "ItemName") + " to create receipt note..!", "warning", 1200);
                showDevExpressNotification("Please enter valid receipt batch details against purchase order no. '" + dataGrid1.cellValue(i, "PurchaseVoucherNo") + "' and item name '" + dataGrid1.cellValue(i, "ItemName") + " to create receipt note..!", "warning");
                return;
            }
        }

        if (FlagEdit === true) {
            var selectedRow = $("#gridreceiptlist").dxDataGrid("instance").getSelectedRowsData()[0];
            if (isCurrentFinancialYear(selectedRow.FYear) == false) {
                swal("Warning", "Selected GRN is not allowed to Update  in the logged-in financial year.", "warning");
                return false;
            }
        } else {
            if (isCurrentFinancialYear("") == false) {
                swal("Warning", "Selected GRN is not allowed to Create  in the logged-in financial year.", "warning");
                return false;
            }
        }
        var voucherDate = $('#DtPickerVoucherDate').dxDateBox('instance').option('value');
        var ReceiptDate = new Date(voucherDate);
        var PODate = new Date(dataGrid1._options.dataSource[0].PurchaseVoucherDate);
        var ledgerID = document.getElementById("TxtSupplierID").value;
        //var totalreceiptqty = $.receiptBatchDetail.Sum({ ChallanQuantity });
        var totalreceiptqty = 0;
        var dONo = document.getElementById("TxtDnNo").value.trim();
        var eWayNo = document.getElementById("TxtEwayBillNo").value.trim();
        var dODate = $('#DtPickerDnDate').dxDateBox('instance').option('value');
        var eWayDate = $('#DtPickerEwayDate').dxDateBox('instance').option('value');
        var transporter = document.getElementById("TxtTransporters").value.trim();
        var gateentryno = document.getElementById("TxtGENo").value.trim();
        var gateentrydate = $('#DtPickerGEDate').dxDateBox('instance').option('value');
        var LRNoVehicleNo = document.getElementById("TxtLRNo").value.trim();
        var textNarration = document.getElementById("TxtNarration").value.trim();
        var ReceivedBy = $("#sel_Receiver").dxSelectBox('instance').option('value');
        var BiltyNo = document.getElementById("BiltyNo").value.trim();
        var BiltyDate = $('#BiltyDate').dxDateBox('instance').option('value');

        for (let i = 0; i < dataGrid1._options.dataSource.length; i++) {
            let PODate = new Date(dataGrid1._options.dataSource[i].PurchaseVoucherDate);
            PODate1.push(PODate);
        }
        for (let i = 0; i < PODate1.length; i++) {
            if (ReceiptDate < PODate1[i]) {
                //DevExpress.ui.notify("Please select a valid date. Receipt date cannot be earlier than PO date.", "error", 1500);
                showDevExpressNotification("Please select a valid date. Receipt date cannot be earlier than PO date.", "error");
                document.getElementById("IssueDate").style.borderColor = "red";
                document.getElementById("IssueDate").focus();
                return false;
            }
        }

        var jsonObjectsTransactionMain = [];
        var TransactionMainRecord = {};

        TransactionMainRecord.VoucherID = -14;
        TransactionMainRecord.GateEntryTransactionID = GateEntryTransactionID;
        //TransactionMainRecord.ProductionUnitID = 0;
        TransactionMainRecord.VoucherDate = voucherDate;
        TransactionMainRecord.ReceivedBy = ReceivedBy;
        TransactionMainRecord.LedgerID = ledgerID;
        TransactionMainRecord.TotalQuantity = totalreceiptqty.toString();
        TransactionMainRecord.Particular = "Receipt Note";
        TransactionMainRecord.DeliveryNoteNo = dONo;
        TransactionMainRecord.DeliveryNoteDate = dODate;
        TransactionMainRecord.EWayBillNumber = eWayNo;
        TransactionMainRecord.EWayBillDate = eWayDate;
        TransactionMainRecord.Transporter = transporter;
        TransactionMainRecord.GateEntryNo = gateentryno;
        TransactionMainRecord.GateEntryDate = gateentrydate;
        TransactionMainRecord.LRNoVehicleNo = LRNoVehicleNo;
        TransactionMainRecord.Narration = textNarration;
        TransactionMainRecord.BiltyNo = BiltyNo;
        TransactionMainRecord.BiltyDate = BiltyDate;

        jsonObjectsTransactionMain.push(TransactionMainRecord);

        var jsonObjectsTransactionDetail = [];
        var TransactionDetailRecord = {};
        if (receiptBatchDetail.length > 0) {
            for (var e = 0; e < receiptBatchDetail.length; e++) {
                TransactionDetailRecord = {};
                if (Number(receiptBatchDetail[e].ChallanQuantity > 0)) {
                    TransactionDetailRecord.TransID = e + 1;
                    TransactionDetailRecord.ItemID = receiptBatchDetail[e].ItemID;
                    TransactionDetailRecord.ItemGroupID = receiptBatchDetail[e].ItemGroupID;
                    TransactionDetailRecord.ChallanQuantity = receiptBatchDetail[e].ChallanQuantity.toString();
                    TransactionDetailRecord.ChallanWeight = receiptBatchDetail[e].ReceiptQuantityInPurchaseUnit;
                    TransactionDetailRecord.BatchNo = "_" + receiptBatchDetail[e].PurchaseVoucherNo + "_" + receiptBatchDetail[e].ItemID;
                    TransactionDetailRecord.StockUnit = receiptBatchDetail[e].StockUnit;
                    //TransactionDetailRecord.ProductionUnitID = 0;
                    if (receiptBatchDetail[e].SupplierBatchNo != null || receiptBatchDetail[e].SupplierBatchNo != undefined) {
                        TransactionDetailRecord.SupplierBatchNo = receiptBatchDetail[e].SupplierBatchNo;
                    } else {
                        TransactionDetailRecord.SupplierBatchNo = '-';
                    }

                    (receiptBatchDetail[e].MfgDate !== null && receiptBatchDetail[e].MfgDate !== undefined) ? TransactionDetailRecord.MfgDate = new Date(receiptBatchDetail[e].MfgDate).toISOString().substr(0, 10) : TransactionDetailRecord.MfgDate = null;
                    (receiptBatchDetail[e].ExpiryDate !== null && receiptBatchDetail[e].ExpiryDate !== undefined) ? TransactionDetailRecord.ExpiryDate = new Date(receiptBatchDetail[e].ExpiryDate).toISOString().substr(0, 10) : TransactionDetailRecord.ExpiryDate = null;

                    var ReceiptWtPerPacking = 0.00;
                    ReceiptWtPerPacking = Number(receiptBatchDetail[e].ReceiptWtPerPacking).toFixed(6);
                    TransactionDetailRecord.ReceiptWtPerPacking = ReceiptWtPerPacking.toString();
                    TransactionDetailRecord.WarehouseID = receiptBatchDetail[e].WarehouseID;
                    TransactionDetailRecord.PurchaseTransactionID = receiptBatchDetail[e].PurchaseTransactionID;

                    ReceiptQuantity = receiptBatchDetail[e].ChallanQuantity.toString();
                    ApprovedQuantity = receiptBatchDetail[e].ChallanQuantity.toString();
                    ///Commented for QC approval dynamic
                    let rs = $.grep(itemArr, function (e1) { return (e1.ItemID === receiptBatchDetail[e].ItemID); });
                    if (rs.length > 0) {
                        if (Number(rs[0].QCParametersCount) == 0) {
                            TransactionDetailRecord.IsVoucherItemApproved = 1;
                            TransactionDetailRecord.ReceiptQuantity = receiptBatchDetail[e].ChallanQuantity.toString();
                            TransactionDetailRecord.ApprovedQuantity = receiptBatchDetail[e].ChallanQuantity.toString();
                            TransactionDetailRecord.VoucherItemApprovedBy = rs[0].VoucherItemApprovedBy;
                            TransactionDetailRecord.VoucherItemApprovedDate = voucherDate;
                            TransactionDetailRecord.RejectedQuantity = 0;
                            TransactionDetailRecord.QCApprovalNO = "Auto";
                            TransactionDetailRecord.QCApprovedNarration = "Auto Approved by System";
                        } else {
                            TransactionDetailRecord.IsVoucherItemApproved = 0;
                            TransactionDetailRecord.VoucherItemApprovedBy = 0;
                            TransactionDetailRecord.VoucherItemApprovedDate = null;
                            TransactionDetailRecord.ReceiptQuantity = 0;
                            TransactionDetailRecord.ApprovedQuantity = 0;
                            TransactionDetailRecord.RejectedQuantity = 0;
                            TransactionDetailRecord.QCApprovalNO = "";
                            TransactionDetailRecord.QCApprovedNarration = "";
                        }
                    }

                    jsonObjectsTransactionDetail.push(TransactionDetailRecord);
                }
            }
        }

        var ObjPO = {};
        var ArrPO = [];
        if (receiptBatchDetail.length > 0) {
            for (var j = 0; j < receiptBatchDetail.length; j++) {

                for (var k = 0; k < GetSelectedListData.length; k++) {
                    let TtlQty = parseFloat(GetSelectedListData[k].PendingQty.toString() - receiptBatchDetail[j].ReceiptQuantityInPurchaseUnit.toString());

                    if (TtlQty <= 0) {
                        ObjPO.POCompleted = 1;
                        ObjPO.purchaseTransactionID = receiptBatchDetail[j].PurchaseTransactionID;
                        ObjPO.itemID = receiptBatchDetail[j].ItemID;
                        ArrPO.push(ObjPO);
                    }
                    break;
                }
            }
        }

        jsonObjectsTransactionMain = JSON.stringify(jsonObjectsTransactionMain);
        jsonObjectsTransactionDetail = JSON.stringify(jsonObjectsTransactionDetail);
        jsonObjectsPO = JSON.stringify(ArrPO);

        //$.ajax({
        //    async: false,
        //    type: "POST",
        //    url: "WebServicePurchaseGRN.asmx/ValidateSupplierBatchReceiptData",
        //    data: '{voucherid:' + JSON.stringify(voucherid) + ',jsonObjectsTransactionMain:' + jsonObjectsTransactionMain + ',jsonObjectsTransactionDetail:' + jsonObjectsTransactionDetail + '}',
        //    contentType: "application/json; charset=utf-8",
        //    dataType: "json",
        //    success: function (results) {
        //        var res = JSON.stringify(results);
        //        res = res.replace(/"d":/g, '');
        //        res = res.replace(/{/g, '');
        //        res = res.replace(/}/g, '');
        //        res = res.substr(1);
        //        res = res.slice(0, -1);

        //        document.getElementById("LOADER").style.display = "none";

        //        if (res === "Success") {
        //        }
        //        else {
        //            alert(res);
        //        }
        //    },
        //    error: function errorFunc(jqXHR) {
        //        document.getElementById("LOADER").style.display = "none";
        //        //  $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
        //        swal("Error!", "Please try after some time..", "");
        //        console.log(jqXHR);
        //    }
        //});
        if (FlagEdit === true) {
            if (FlagEdit === true && validateUserData.isUserInfoFilled === false) {
                validateUserData.userName = ""; validateUserData.password = ""; validateUserData.RecordID = TransactionID; validateUserData.actionType = "Update"; validateUserData.transactionRemark = ""; validateUserData.isUserInfoFilled = false;
                let result = await openSecurityPanelModal(validateUserData);
            }

            (async () => {
                await saveUpdateReceiptNoteData(prefix, voucherid, jsonObjectsTransactionMain, jsonObjectsTransactionDetail, TransactionID, validateUserData, ReceiptQuantity, ApprovedQuantity, jsonObjectsPO)
            })();

        } else {
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
                    (async () => {
                        TransactionID = 0;
                        await saveUpdateReceiptNoteData(prefix, voucherid, jsonObjectsTransactionMain, jsonObjectsTransactionDetail, TransactionID, validateUserData, ReceiptQuantity, ApprovedQuantity, jsonObjectsPO)
                    })();
                }
            );
        }
        //var txt = 'If you confident please click on \n' + 'Yes, Save it ! \n' + 'otherwise click on \n' + 'Cancel';
        //swal({
        //    title: "Do you want to continue",
        //    text: txt,
        //    type: "warning",
        //    showCancelButton: true,
        //    confirmButtonColor: "#DD6B55",
        //    confirmButtonText: "Yes, Save it !",
        //    closeOnConfirm: true
        //},
        //    function () {
        //        if (FlagEdit === true) {
        //            document.getElementById("LOADER").style.display = "block";
        //            try {

        //                $.ajax({
        //                    type: "POST",
        //                    url: "WebServicePurchaseGRN.asmx/UpdateReceiptData",
        //                    //data: '{TransactionID:' + JSON.stringify(TransactionID) + ',jsonObjectsTransactionMain:' + jsonObjectsTransactionMain + ',jsonObjectsTransactionDetail:' + jsonObjectsTransactionDetail + '}',
        //                    data: '{TransactionID:' + JSON.stringify(TransactionID) + ',jsonObjectsTransactionMain:' + jsonObjectsTransactionMain + ',jsonObjectsTransactionDetail:' + jsonObjectsTransactionDetail + ',ReceiptQuantity:' + JSON.stringify(ReceiptQuantity) + ',ApprovedQuantity:' + JSON.stringify(ApprovedQuantity) + '}',
        //                    contentType: "application/json; charset=utf-8",
        //                    dataType: "json",
        //                    success: function (results) {
        //                        var res = JSON.stringify(results);
        //                        res = res.replace(/"d":/g, '');
        //                        res = res.replace(/{/g, '');
        //                        res = res.replace(/}/g, '');
        //                        res = res.substr(1);
        //                        res = res.slice(0, -1);

        //                        document.getElementById("LOADER").style.display = "none";
        //                        if (res === "Success") {
        //                            document.getElementById("BtnSave").setAttribute("data-dismiss", "modal");
        //                            swal("Updated!", "Your data Updated", "success");
        //                            location.reload();
        //                        }
        //                        else {
        //                            swal("Error..!", res, "warning");
        //                        }
        //                    },
        //                    error: function errorFunc(jqXHR) {
        //                        document.getElementById("LOADER").style.display = "none";
        //                        swal("Error!", "Please try after some time..", "");
        //                    }
        //                });
        //            } catch (e) {
        //                console.log(e);
        //            }

        //        } else {
        //            document.getElementById("LOADER").style.display = "block";
        //            try {
        //                $.ajax({
        //                    type: "POST",
        //                    url: "WebServicePurchaseGRN.asmx/SaveReceiptData",
        //                    //data: '{prefix:' + JSON.stringify(prefix) + ',voucherid:' + JSON.stringify(voucherid) + ',jsonObjectsTransactionMain:' + jsonObjectsTransactionMain + ',jsonObjectsTransactionDetail:' + jsonObjectsTransactionDetail + '}',
        //                    data: '{prefix:' + JSON.stringify(prefix) + ',voucherid:' + JSON.stringify(voucherid) + ',jsonObjectsTransactionMain:' + jsonObjectsTransactionMain + ',jsonObjectsTransactionDetail:' + jsonObjectsTransactionDetail + ',ReceiptQuantity:' + JSON.stringify(ReceiptQuantity) + ',ApprovedQuantity:' + JSON.stringify(ApprovedQuantity) + '}',
        //                    // data: '{prefix:' + JSON.stringify(prefix) + '}',
        //                    contentType: "application/json; charset=utf-8",
        //                    dataType: "json",
        //                    success: function (results) {
        //                        var res = JSON.stringify(results);
        //                        res = res.replace(/"d":/g, '');
        //                        res = res.replace(/{/g, '');
        //                        res = res.replace(/}/g, '');
        //                        res = res.substr(1);
        //                        res = res.slice(0, -1);

        //                        document.getElementById("LOADER").style.display = "none";

        //                        if (res === "Success") {
        //                            swal("Saved!", "Your data saved", "success");
        //                            location.reload();
        //                        }
        //                        else {
        //                            swal("Error..!", res, "warning");
        //                        }
        //                    },
        //                    error: function errorFunc(jqXHR) {
        //                        document.getElementById("LOADER").style.display = "none";
        //                        //  $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
        //                        swal("Error!", "Please try after some time..", "");
        //                        console.log(jqXHR);
        //                    }
        //                });
        //            } catch (e) {
        //                console.log(e);
        //            }

        //        }
        //    });

    } catch (e) {
        console.log(e);
    }
});

async function saveUpdateReceiptNoteData(prefix, voucherid, jsonObjectsTransactionMain, jsonObjectsTransactionDetail, TransactionID, validateUserData, ReceiptQuantity, ApprovedQuantity, jsonObjectsPO) {
    if (FlagEdit === true) {
        document.getElementById("LOADER").style.display = "block";
        try {

            $.ajax({
                type: "POST",
                url: "WebServicePurchaseGRN.asmx/UpdateReceiptData",
                //data: '{TransactionID:' + JSON.stringify(TransactionID) + ',jsonObjectsTransactionMain:' + jsonObjectsTransactionMain + ',jsonObjectsTransactionDetail:' + jsonObjectsTransactionDetail + '}',
                data: '{TransactionID:' + JSON.stringify(TransactionID) + ',jsonObjectsTransactionMain:' + jsonObjectsTransactionMain + ',jsonObjectsTransactionDetail:' + jsonObjectsTransactionDetail + ',ReceiptQuantity:' + JSON.stringify(ReceiptQuantity) + ',ApprovedQuantity:' + JSON.stringify(ApprovedQuantity) + ',ObjvalidateLoginUser:' + JSON.stringify(validateUserData) + ',jsonObjectsPO:' + jsonObjectsPO + '}',
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
                        document.getElementById("BtnSave").setAttribute("data-dismiss", "modal");
                        validateUserData.isUserInfoFilled = false;
                        //swal("Updated!", "Your data Updated", "success");
                        swal({
                            title: "Updated!",
                            text: "Receipt Note information has been updated successfully.",
                            type: "success"
                        }, function (isConfirm) {
                            location.reload();
                        });
                    } else if (res === "InvalidUser") {
                        swal("Invalid User!", "Invalid user credentials, please enter valid username or password to update the information.", "error");
                        validateUserData.isUserInfoFilled = false;
                        return false;
                    } else if (res === "Exist") {
                        swal("Can't Update", "Receipt Note information has been used in further transactions ..! Record can not be update.", "warning");
                    } else if (res.includes("not authorize")) {
                        swal.close();
                        setTimeout(() => {
                            swal("Warning..!", res, "warning");
                        }, 100);

                    } else {
                        swal("Error..!", res, "warning");
                    }
                },
                error: function errorFunc(jqXHR) {
                    document.getElementById("LOADER").style.display = "none";
                    swal("Error!", "Please try after some time..", "");
                }
            });
        } catch (e) {
            console.log(e);
        }
    } else {
        document.getElementById("LOADER").style.display = "block";
        try {
            $.ajax({
                type: "POST",
                url: "WebServicePurchaseGRN.asmx/SaveReceiptData",
                //data: '{prefix:' + JSON.stringify(prefix) + ',voucherid:' + JSON.stringify(voucherid) + ',jsonObjectsTransactionMain:' + jsonObjectsTransactionMain + ',jsonObjectsTransactionDetail:' + jsonObjectsTransactionDetail + '}',
                data: '{prefix:' + JSON.stringify(prefix) + ',voucherid:' + JSON.stringify(voucherid) + ',jsonObjectsTransactionMain:' + jsonObjectsTransactionMain + ',jsonObjectsTransactionDetail:' + jsonObjectsTransactionDetail + ',ReceiptQuantity:' + JSON.stringify(ReceiptQuantity) + ',ApprovedQuantity:' + JSON.stringify(ApprovedQuantity) + ',jsonObjectsPO:' + jsonObjectsPO + '}',
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

                    document.getElementById("LOADER").style.display = "none";

                    if (res === "Success") {
                        swal({
                            title: "Saved!",
                            text: "Receipt Note information has been saved successfully.",
                            type: "success"
                        }, function (isConfirm) {
                            location.reload();
                        });
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
                },
                error: function errorFunc(jqXHR) {
                    document.getElementById("LOADER").style.display = "none";
                    //  $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                    swal("Error!", "Please try after some time..", "");
                    console.log(jqXHR);
                }
            });
        } catch (e) {
            console.log(e);
        }
    }
}

$("#BtnPrint").click(function () {
    //var url = "PrintReceiptApproval.aspx?TI=" + TransactionID;
    var TxtGRNID = document.getElementById("TxtGRNID").value;
    if (TxtGRNID === "" || TxtGRNID === null || TxtGRNID === undefined) {
        alert("Please select valid grn details to print..!");
        return false;
    }
    if (ApprovedVoucher === "" || ApprovedVoucher === null || ApprovedVoucher === undefined || ApprovedVoucher === false) {
        alert("GRN is not approved, please approve it before taking print..!");
        return false;
    }
    var url = "ReportPurchaseGRN.aspx?TransactionID=" + TxtGRNID;
    window.open(url, "blank", "location=yes,height=1100,width=1050,scrollbars=yes,status=no", true);
});

$("#BtnTransporterSlip").click(function () {
    //var url = "PrintReceiptApproval.aspx?TI=" + TransactionID;
    var TxtGRNID = document.getElementById("TxtGRNID").value;
    if (TxtGRNID === "" || TxtGRNID === null || TxtGRNID === undefined) {
        alert("Please select valid grn details to print..!");
        return false;
    }

    var url = "ReportGRNTransportSlip.aspx?TransactionID=" + TxtGRNID;
    window.open(url, "blank", "location=yes,height=1100,width=1050,scrollbars=yes,status=no", true);
});

$("#BtnRollSlipPrint").click(function () {
    // added By Mohini - 22May
    $('#RollSlipModal').modal('show');
    var TxtGRNID = document.getElementById("TxtGRNID").value;
    if (TxtGRNID === "" || TxtGRNID === null || TxtGRNID === undefined) {
        alert("Please select valid grn details to print..!");
        return false;
    }

    $.ajax({
        type: "POST",
        url: "WebServicePurchaseGRN.asmx/GetGrnItemList",
        data: '{TransactionID:' + JSON.stringify(TxtGRNID) + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            let rs1 = results.replace(/\\/g, '');
            rs1 = rs1.replace(/"d":""/g, '');
            rs1 = rs1.replace(/""/g, '');
            rs1 = rs1.replace(/:,/g, ":null,");
            rs1 = rs1.replace(/,}/g, ",null}");
            rs1 = rs1.substr(1);
            rs1 = rs1.slice(0, -1);
            let RES1 = JSON.parse(rs1);
            $("#ItemDetailslist").dxDataGrid({
                dataSource: RES1
            });
        }
    });
});

//Code Modified by Mohini on 22-May for generating multiple GRN slips

$("#BtnPrintSlip").click(function () {
    $('#RollSlipModal').modal('hide');

    var dataGrid = $("#ItemDetailslist").dxDataGrid('instance');
    var selectedRows = dataGrid.getSelectedRowsData();

    if (selectedRows.length === 0) {
        alert("Please Select At least One Item To Print The Slip.");
        return false;
    }

    var arrPO = [];
    for (var i = 0; i < selectedRows.length; i++) {
        arrPO.push({
            TransactionID: selectedRows[i].TransactionID,
            itemID: selectedRows[i].ItemID,
            NoOfSlip: selectedRows[i].NoOfSlip
        });
    }

    var encodedArrPO = encodeURIComponent(JSON.stringify(arrPO));
    var url = "ReportRollSlip.aspx?ArrPO=" + encodedArrPO;
    window.open(url, "_blank", "location=yes,height=1100,width=1050,scrollbars=yes,status=no");

});

$("#BtnDelete").click(async function () {

    var TxtGRNID = document.getElementById("TxtGRNID").value;
    if (TxtGRNID === "" || TxtGRNID === null || TxtGRNID === undefined) {
        alert("Please Choose any row from below Grid..!");
        return false;
    }
    var selectedRow = $("#gridreceiptlist").dxDataGrid("instance").getSelectedRowsData()[0];
    if (isCurrentFinancialYear(selectedRow.FYear) == false) {
        swal("Warning", "Selected GRN is not allowed to Delete  in the logged-in financial year.", "warning");
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
        validateUserData.userName = ""; validateUserData.password = ""; validateUserData.RecordID = TxtGRNID; validateUserData.actionType = "Delete"; validateUserData.transactionRemark = ""; validateUserData.isUserInfoFilled = false;
        let result = await openSecurityPanelModal(validateUserData);
    }
    let ObjPO = {};
    let ArrPO = [];
    var dataGrid1 = $("#GridPurchaseOrders").dxDataGrid('instance');
    for (var k = 0; k < dataGrid1._options.dataSource.length; k++) {
        ObjPO = {}
        ObjPO.purchaseTransactionID = dataGrid1._options.dataSource[k].TransactionID;
        ObjPO.itemID = dataGrid1._options.dataSource[k].ItemID;
        ArrPO.push(ObjPO);

    }
    let jsonObjectsPO = JSON.stringify(ArrPO);
    $.ajax({
        type: "POST",
        url: "WebServicePurchaseGRN.asmx/CheckPermission",
        data: '{TransactionID:' + JSON.stringify(TxtGRNID) + '}',
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

            if (res === "Exist") {
                swal("", "This item is used in another process..! Record can not be delete.", "error");
                return false;
            }
            else {
                swal({
                    title: "Are you sure?",
                    text: 'You will not be able to recover this Content!',
                    type: "warning",
                    showCancelButton: true,
                    confirmButtonColor: "#DD6B55",
                    confirmButtonText: "Yes, delete it!",
                    closeOnConfirm: false
                },
                    function () {
                        document.getElementById("LOADER").style.display = "block";
                        $.ajax({
                            type: "POST",
                            url: "WebServicePurchaseGRN.asmx/DeletePGRN",
                            data: '{TransactionID:' + JSON.stringify(TxtGRNID) + ',ObjvalidateLoginUser:' + JSON.stringify(validateUserData) + ',PurchaseTransactionID:' + JSON.stringify(PurchaseTransactionID) + ',jsonObjectsPO:' + jsonObjectsPO + '}',
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
                                    swal("Deleted!", "Your data Deleted", "success");
                                    // alert("Your Data has been Saved Successfully...!");
                                    location.reload();
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
                            },
                            error: function errorFunc(jqXHR) {
                                document.getElementById("LOADER").style.display = "none";
                                alert(jqXHR);
                            }
                        });

                    });
            }

        }
    });
});

$("#BtnNotification").click(function () {
    var purchaseId = "";
    var receiptId = "";
    var commentData = "";
    var newHtml = '';
    if (FlagEdit === false) {
        if (GetPendingData.length > 0) {
            for (var i = 0; i < GetPendingData.length; i++) {
                if (GetPendingData[i].TransactionID > 0) {
                    if (purchaseId === "") {
                        purchaseId = GetPendingData[i].TransactionID.toString();
                    } else {
                        purchaseId = purchaseId + "," + GetPendingData[i].TransactionID.toString();
                    }
                }
            }

            document.getElementById("commentbody").innerHTML = "";
            if (purchaseId !== "") {
                $.ajax({
                    type: "POST",
                    url: "WebServicePurchaseGRN.asmx/GetCommentData",
                    data: '{receiptTransactionID:0,purchaseTransactionIDs:' + JSON.stringify(purchaseId) + '}',
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
                        $(".commentInput").hide();

                    }

                });
            }

        }
    } else {
        receiptId = document.getElementById("TxtGRNID").value;
        if (receiptId === "" || receiptId === null || receiptId === undefined) {
            alert("Please select valid receipt note to view comment details..!");
            return false;
        }
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
                        console.log(commentData);
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
        var receiptId = document.getElementById("TxtGRNID").value;
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
        objectCommentDetail.ModuleName = "Goods Receipt Note";
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
                swal("Comment saved!", "Comment saved successfully.", "success");
                var commentData = "";
                var newHtml = '';
                var receiptId = document.getElementById("TxtGRNID").value;
                if (receiptId === "" || receiptId === null || receiptId === undefined) {
                    alert("Please select valid receipt note to view comment details..!");
                    return false;
                }
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
                                    newHtml = newHtml + '<div style="width:100%"><b style="text-align: left; color: red; float: left; margin-top: 5px;width: 100%">' + (x + 1) + '. ' + commentData[x].ModuleName + ', Title : ' + commentData[x].CommentTitle + ', Type : ' + commentData[x].CommentType + '</b>'
                                    newHtml = newHtml + '<p style="text-align: left; margin-top: 2px; float: left; margin-left: 20px">' + commentData[x].CommentDescription + '</p><span style="float: right">Comment By : ' + commentData[x].UserName + '</span></div>'
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

$("#BtnSelectGP").click(function () {
    document.getElementById("BtnSelectGP").setAttribute("data-toggle", "modal");
    document.getElementById("BtnSelectGP").setAttribute("data-target", "#ModalGatePass");
    GetGatePass()
});

$("#GridGatePass").dxDataGrid({
    dataSource: [],
    columns: [
        { dataField: "TransactionID", visible: false, width: 120, caption: "TransactionID" },
        { dataField: "GatePassTransactionID", visible: false, width: 120, caption: "GatePassTransactionID" },
        { dataField: "VoucherID", visible: false, width: 150, caption: "VoucherID" },
        { dataField: "VoucherNo", visible: true, width: 150, caption: "Gate Entry No." },
        { dataField: "VoucherDate", visible: true, width: 120, caption: "Gate Entry Date" },
        { dataField: "GateEntryType", visible: true, width: 200, caption: "Gate Entry Type" },
        { dataField: "MaterialSentTo", visible: true, width: 200, caption: "Received From" },
        { dataField: "SendThrough", visible: true, width: 100, caption: "Received Through" },
        { dataField: "SendThroughName", visible: true, width: 100, caption: "Received Through Name" },
        { dataField: "VehicleNo", visible: true, width: 100, caption: "Vehicle No." },
        { dataField: "Remark", visible: true, width: 100, caption: "Remark" },
    ],
    allowColumnReordering: true,
    allowColumnResizing: true,
    showRowLines: true,
    columnResizingMode: "widget",
    paging: {
        pageSize: 25
    },
    pager: {
        showPageSizeSelector: true,
        allowedPageSizes: [25, 100, 250, 500, 1000]
    },

    sorting: {
        mode: "multiple"
    },
    //export: {
    //    enabled: true,
    //    fileName: "Tools",
    //    allowExportSelectedData: true
    //},

    selection: { mode: "single" },
    filterRow: { visible: true, applyFilter: "auto" },
    columnChooser: { enabled: true },
    headerFilter: { visible: true },
    searchPanel: { visible: true },
    loadPanel: {
        enabled: true,
        height: 90,
        width: 200,
        text: 'Data is loading...'
    },
    onRowPrepared: function (e) {
        if (e.rowType === "header") {
            e.rowElement.css('background', '#42909A');
            e.rowElement.css('color', 'white');
        }
        e.rowElement.css('fontSize', '11px');
    },
    onSelectionChanged: function (clicked) {
        GridSelectedData = []
        GridSelectedData = clicked.selectedRowsData;
    },
    //onExporting: function (e) {
    //    var workbook = new ExcelJS.Workbook();
    //    var worksheet = workbook.addWorksheet('Main sheet');
    //    DevExpress.excelExporter.exportDataGrid({
    //        worksheet: worksheet,
    //        component: e.component,
    //        customizeCell: function (options) {
    //            var excelCell = options;
    //            excelCell.font = { name: 'Arial', size: 12 };
    //            excelCell.alignment = { horizontal: 'left' };
    //        }
    //    }).then(function () {
    //        workbook.xlsx.writeBuffer().then(function (buffer) {
    //            saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'GatePassEntry.xlsx');
    //        });
    //    });
    //    e.cancel = true;
    //},
});


function GetGatePass() {
    var ID = document.getElementById("TxtSupplierID").value

    $.ajax({
        type: "POST",
        url: "WebServicePurchaseGRN.asmx/GetGatePass",
        data: '{LedgerId:' + Number(ID) + '}',
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
            var RES1 = JSON.parse(res);

            $("#GridGatePass").dxDataGrid({
                dataSource: RES1,
            });
            $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
        },
        error: function errorFunc(jqXHR) {
            //DevExpress.ui.notify(jqXHR.statusText, "error", 500);
        }
    });
}

$("#BtnApply").click(function () {
    if (GridSelectedData.length <= 0) {
        //DevExpress.ui.notify(" Please Select Data From The Grid.....? ", "warning", 2500);
        showDevExpressNotification("Please Select Data From The Grid.....?", "warning");
        return
    }

    GateEntryTransactionID = GridSelectedData[0].TransactionID;
    document.getElementById("TxtDnNo").value = GridSelectedData[0].DCNo;
    document.getElementById("TxtGENo").value = GridSelectedData[0].VoucherNo;
    document.getElementById("TxtLRNo").value = GridSelectedData[0].VehicleNo;
    document.getElementById("TxtTransporters").value = GridSelectedData[0].SendThroughName;
    document.getElementById("TxtDnNo").value = GridSelectedData[0].DocumentNo;
    $("#DtPickerGEDate").dxDateBox({
        value: GridSelectedData[0].VoucherDate,
    });

    $("#CloseModalGP").click();
});

$("#BtnApplyWarehouse").click(function () {
    let warehouseName = $("#SelWarehouse").dxSelectBox('instance').option('value');
    let wID = $("#SelBin").dxSelectBox('instance').option('value');
    let binName = $("#SelBin").dxSelectBox('instance').option('text');
    if (warehouseName === undefined || warehouseName === null || warehouseName === "") {
        //DevExpress.ui.notify("Please select warehouse name.", "warning", 1200);
        showDevExpressNotification("Please select warehouse name.!", "warning");
        return;
    }
    if (wID === undefined || wID === null || Number(wID) === 0) {
        //DevExpress.ui.notify("Please select bin name.", "warning", 1200);
        showDevExpressNotification("Please select bin name.!", "warning");
        return;
    }
    let selectedRowData = GridReceiptBatchDetails.getSelectedRowsData();
    if (selectedRowData.length > 0) {
        let selectedRow = GridReceiptBatchDetails.getSelectedRowsData()[0];
        let selectedRowIndex = GridReceiptBatchDetails.getRowIndexByKey(GridReceiptBatchDetails.keyOf(selectedRow));
        let batchData = GridReceiptBatchDetails._options.dataSource;
        batchData[selectedRowIndex].WarehouseID = wID;
        batchData[selectedRowIndex].Warehouse = warehouseName;
        batchData[selectedRowIndex].Bin = binName;
        batchData[selectedRowIndex].WarehouseBin = warehouseName + ' - ' + binName;
        GridReceiptBatchDetails.refresh();
        updateBatchDetail(batchData[selectedRowIndex]);
        //if (receiptBatchDetail.length > 0) {
        //    let data = receiptBatchDetail.filter(function (e) {
        //        return Number(e.PurchaseTransactionID) == Number(batchData[selectedRowIndex].TransactionID) && e.BatchNo == batchData[selectedRowIndex].BatchNo && Number(e.ItemID) == Number(batchData[selectedRowIndex].ItemID) && Number(e.ChallanQuantity) == Number(batchData[selectedRowIndex].ReceiptQuantity);
        //    });
        //    if (data.length > 0) {
        //        receiptBatchDetail.map(function (e) {
        //            if (Number(e.PurchaseTransactionID) == Number(batchData[selectedRowIndex].TransactionID) && e.BatchNo == batchData[selectedRowIndex].BatchNo && Number(e.ItemID) == Number(batchData[selectedRowIndex].ItemID) && Number(e.ChallanQuantity) == Number(batchData[selectedRowIndex].ReceiptQuantity)) {
        //                e.Warehouse = warehouseName;
        //                e.Bin = binName;
        //                e.WarehouseID = wID;
        //            }
        //        })
        //    }
        //}
        this.setAttribute("data-toggle", "modal");
        this.setAttribute("data-target", "#WarehouseSelectionModal");
    }

});

$('#BtnRefresh1').click(function () {
    ReceiptNotesVouchers();
});

let isUserAllowdExcessQty = false

getUserAuthority();
function getUserAuthority() {
    try {
        $.ajax({
            type: "POST",
            url: "WebServicePurchaseGRN.asmx/getUserAuthority",
            data: '{}',
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            success: function (results) {
                var res = results.replace(/\\/g, '');
                res = res.replace(/"d":""/g, '');
                res = res.replace(/""/g, '');
                res = res.substr(1);
                res = res.slice(0, -1);
                let RES1 = JSON.parse(res);
                let checklength = RES1[0].CanReceiveExcessMaterial;

                if (checklength === "" || checklength === undefined || checklength === null || checklength === false || checklength === "false") {
                    isUserAllowdExcessQty = false;
                }
                else {
                    isUserAllowdExcessQty = true;
                }
            }
        });
    }
    catch (e) {
        console.log(e);
    }
}

$('#btnUploadGRNData').click(function () {
    const fileInput = document.getElementById('fileInput');

    // Reset the file input value to ensure the 'change' event fires every time
    fileInput.value = '';

    // Remove any existing 'change' event listener to ensure fresh execution
    fileInput.removeEventListener('change', handleFileUpload);

    // Add a new 'change' event listener
    fileInput.addEventListener('change', handleFileUpload);

    // Trigger the file input click to open the file dialog
    fileInput.click();
});

async function handleFileUpload(event) {
    const file = event.target.files[0];

    if (file) {
        const reader = new FileReader();

        reader.onload = async (e) => {
            const data = new Uint8Array(e.target.result);
            const workbook = XLSX.read(data, { type: 'array' });
            const firstSheetName = workbook.SheetNames[0];
            const worksheet = workbook.Sheets[firstSheetName];

            // Convert the worksheet data to JSON format with header row included
            const sheetData = XLSX.utils.sheet_to_json(worksheet, { header: 1 });
            const headerRow = sheetData[0]; // Column headers

            // Define the columns of interest for matching
            const columnsOfInterest = ["PurchaseVoucherNo", "ItemCode", "PurchaseUnit", "StockUnit"];

            // Map all column headers to their indices
            const columnIndices = headerRow.reduce((indices, column, index) => {
                indices[column] = index;
                return indices;
            }, {});

            // Array to hold matched data
            const matchedData = [];

            // Extract rows that have all required columns for matching
            for (let i = 1; i < sheetData.length; i++) {
                const row = sheetData[i];
                const rowData = {};
                let isRowValid = true;

                // Collect columnsOfInterest values to verify matching rows
                for (const column of columnsOfInterest) {
                    const columnIndex = columnIndices[column];
                    if (columnIndex !== undefined && row[columnIndex] !== undefined) {
                        rowData[column] = row[columnIndex].toString().replace(/\t/g, '').trim();
                    } else {
                        isRowValid = false; // Skip rows missing required columns for matching
                        break;
                    }
                }

                // If the row has the required columns, add all its columns for potential match storage
                if (isRowValid) {
                    for (const column in columnIndices) {
                        const columnIndex = columnIndices[column];
                        rowData[column] = row[columnIndex] !== undefined ? row[columnIndex].toString().replace(/\t/g, '').trim() : null;
                    }
                    matchedData.push(rowData);
                }
            }

            // Get data from GridPurchaseOrders
            const gridData = $('#GridPurchaseOrders').dxDataGrid('instance').option('dataSource');

            // Track occurrences of each ItemID to handle BatchNo suffixes
            const batchCount = {};

            // Compare Excel data with Grid data and replace `ReceiptQuantity` and add `WarehouseBin`
            const comparisonResults = await Promise.all(matchedData.map(async (excelRow) => {
                // Find matching row in GridPurchaseOrders by matching columnsOfInterest keys
                const matchingRow = gridData.find((gridRow) =>
                    columnsOfInterest.every((col) => gridRow[col] === excelRow[col])
                );

                if (matchingRow) {
                    // Create a clone of matchingRow to avoid modifying the original gridData directly
                    const updatedRow = { ...matchingRow };

                    // Replace `ReceiptQuantity` in gridData with the value from excelRow
                    updatedRow.ChallanQuantity = excelRow.ReceiptQuantity;
                    updatedRow.ReceiptQuantity = excelRow.ReceiptQuantity;

                    // Initialize or increment the batch count for this ItemID
                    if (!batchCount[updatedRow.ItemID]) {
                        batchCount[updatedRow.ItemID] = 1;
                    } else {
                        batchCount[updatedRow.ItemID]++;
                    }

                    // Generate BatchNo with suffix
                    updatedRow.BatchNo = `_${updatedRow.PurchaseVoucherNo}_${updatedRow.ItemID}_${batchCount[updatedRow.ItemID]}`;

                    // Combine Warehouse and Bin into WarehouseBin if both exist in excelRow
                    if (excelRow.Warehouse && excelRow.Bin) {
                        const matchingWarehouseItem = ResWarehouse.find((warehouseItem) =>
                            warehouseItem.Warehouse?.trim().toLowerCase() === excelRow.Warehouse?.trim().toLowerCase()
                        );

                        const matchingWarehouse = matchingWarehouseItem ? matchingWarehouseItem.Warehouse : '';
                        if (matchingWarehouse) {
                            await RefreshBin(matchingWarehouse);
                            const matchingBin = ResBin.find((bin) =>
                                bin.Bin?.trim().toLowerCase() === excelRow.Bin?.trim().toLowerCase()
                            );
                            if (matchingBin) {
                                updatedRow.WarehouseBin = `${excelRow.Warehouse} - ${excelRow.Bin}`;
                                updatedRow.WarehouseID = matchingBin.WarehouseID; // Adding WarehouseID from matchingBin
                            }
                        }
                    }

                    updatedRow.PurchaseTransactionID = updatedRow.TransactionID;

                    return { ...excelRow, matched: true, ...updatedRow };
                } else {
                    return { ...excelRow, matched: false };
                }
            }));

            // Filter out matched rows
            const matchedRows = comparisonResults.filter(row => row.matched);
            if (matchedRows.length > 0) {
                receiptBatchDetail = matchedRows; // Save matched rows to the array
                refreshbatchdetailsgrid(matchedRows); // Refresh grid with matched rows
            }
        };

        reader.readAsArrayBuffer(file);
    }
}

//added By Mohini - 22May
$("#ItemDetailslist").dxDataGrid({
    dataSource: [],
    columnAutoWidth: true,
    showBorders: true,
    showRowLines: true,
    allowColumnReordering: true,
    allowColumnResizing: true,
    columnResizingMode: "widget",
    //keyExpr: "TransactionID",
    sorting: {
        mode: "multiple"
    },
    selection: { mode: "multiple", showCheckBoxesMode: "always" },
    paging: {
        pageSize: 20
    },
    pager: {
        showPageSizeSelector: true,
        allowedPageSizes: [20, 40, 50, 100]
    },
    filterRow: { visible: true, applyFilter: "auto" },
    columnChooser: { enabled: true },
    headerFilter: { visible: true },
    height: function () {
        return window.innerHeight / 2.2;
    },
    searchPanel: { visible: true },
    loadPanel: {
        enabled: true,
        text: 'Data is loading...'
    },
    //export: {
    //    enabled: true,
    //    fileName: "Purchase GRN",
    //    allowExportSelectedData: true
    //},
    editing: {
        mode: "cell",
        allowDeleting: false,
        //allowAdding: true,
        allowUpdating: true
    },
    columns: [
        { dataField: "TransactionID", visible: false, allowEditing: false, width: 120, caption: "TransactionID", alignment: "center" },
        { dataField: "VoucherNo", visible: true, allowEditing: false, width: 110, caption: "Voucher No.", alignment: "center" },
        { dataField: "VoucherDate", visible: true, allowEditing: false, width: 110, caption: "Voucher Date", alignment: "center" },
        { dataField: "ItemCode", visible: true, allowEditing: false, width: 80, caption: "Item ode", alignment: "center" },
        { dataField: "ItemName", visible: true, allowEditing: false, width: 200, caption: "Item Name", alignment: "center" },
        { dataField: "BatchNo", visible: true, allowEditing: false, width: 100, caption: "Batch No.", alignment: "center" },
        { dataField: "SupplierBatchNo", visible: true, allowEditing: false, width: 100, caption: "Supplier Batch No.", alignment: "center" },
        { dataField: "NoOfSlip", visible: true, allowEditing: true, width: 100, caption: "No. Of Slip", alignment: "center" },
        // { dataField: "JobName", visible: true, width: 100, caption: "Job Name", alignment: "center" },
        { dataField: "UnitPerPacking", visible: true, allowEditing: false, width: 100, caption: "Packing Unit", alignment: "center" },
    ],

    onRowPrepared: function (e) {
        if (e.rowType === "header") {
            e.rowElement.css('background', '#509EBC');
            e.rowElement.css('color', 'white');
            e.rowElement.css('font-weight', 'bold');
        }
        e.rowElement.css('fontSize', '11px');
    }
});
