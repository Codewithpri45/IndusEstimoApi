
var GblStatus = "";
var ShowListData = [];
var GetRowWarehouseID = "";
var GBLProductionUnitID = getProductionUnitID('ProductionUnitID');
var GBLCompanyID = getProductionUnitID('CompanyID');
var SelectedProductionUnitID = 0;
var SelectedProductionUnitName = '';
//City Name
$.ajax({
    type: "POST",
    url: "WebService_WarehouseMaster.asmx/GetCity",
    data: '{}',
    contentType: "application/json; charset=utf-8",
    dataType: "text",
    success: function (results) {
        ////console.debug(results);
        var res = results.replace(/\\/g, '');
        res = res.replace(/"d":""/g, '');
        res = res.replace(/""/g, '');
        res = res.substr(1);
        res = res.slice(0, -1);
        RES1 = JSON.parse(res);

        $("#SelCity").dxSelectBox({
            items: RES1,
            placeholder: "Select--",
            displayExpr: 'City',
            valueExpr: 'City',
            searchEnabled: true,
            showClearButton: true,
            acceptCustomValue: true,

        });
    }
});

//Get Show List
Showlist();
function Showlist() {
    document.getElementById("LOADER").style.display = "block";
    $.ajax({
        type: "POST",
        url: "WebService_WarehouseMaster.asmx/ShowListWarehouseMaster",
        data: '{}',
        contentType: "application/json; charset=utf-8",

        dataType: "text",
        success: function (results) {
            let res = results.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/""/g, '');
            res = res.replace(/u0026/g, '&');
            res = res.replace(/u0027/g, "'");
            res = res.replace(/:,/g, ":null,");
            res = res.replace(/,}/g, ",null}");
            res = res.replace(/:}/g, ":null}");
            res = res.replaceAll("\'", "#-");
            res = res.substr(1);
            res = res.slice(0, -1);

            let RES1 = JSON.parse(res);


            $("#WarehouseShowListGrid").dxDataGrid({
                dataSource: RES1,
                showBorders: true,
                paging: {
                    enabled: false
                },
                showRowLines: true,
                allowSorting: false,
                allowColumnResizing: true,
                selection: { mode: "single" },
                paging: {
                    pageSize: 15
                },
                pager: {
                    showPageSizeSelector: true,
                    allowedPageSizes: [15, 25, 50, 100]
                },
                filterRow: { visible: true, applyFilter: "auto" },
                sorting: {
                    mode: "none" // or "multiple" | "single"
                },
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
                onRowDblClick: function (e) {
                    var data = e.data;
                    if (data === undefined || data === null) return false;
                    if (data) {
                        $("#EditButton").click();
                    }
                },
                //onSelectionChanged: function (Showlist) {
                //    sholistData = Showlist.selectedRowsData;
                //    document.getElementById("TxtWarehouseID").value = sholistData[0].WarehouseName;
                //},
                columns: [{ dataField: "WarehouseName", visible: true, width: 250 },
                { dataField: "WarehouseCode", visible: true, width: 100 },
                { dataField: "RefWarehouseCode", visible: true, width: 100 },
                { dataField: "City", visible: true, width: 100 },
                { dataField: "Address", visible: true, width: 350 },
                { dataField: "ProductionUnitName", visible: true, width: 350 },
                { dataField: "BranchName", visible: true, width: 350 },
                { dataField: "IsFloorWarehouse", visible: true, width: 350, dataType: "boolean" },
                ]
            })
        }
    });
}

$("#BtnNew").click(function () {

    let warehouseList = $('#WarehouseShowListGrid').dxDataGrid('instance');
    warehouseList.clearSelection();
    GblStatus = "";
    // document.getElementById('TxtWarehouseID').value = "";
    document.getElementById('TxtWarehouseAddress').value = "";
    document.getElementById('TxtWarehouseName').value = "";
    document.getElementById('TxtWarehouseRefCode').value = "";
    GblStatus = "";
    SelectedProductionUnitID = 0;
    SelectedProductionUnitName = '';
    $("#SelCity").dxSelectBox({
        value: null
    });
    $("#BranchName").dxSelectBox({
        value: null
    });
    $("#ProductionUnit").dxSelectBox({
        value: null
    });
    document.getElementById("ChkIsFloorWareHouse").checked = false;
    ObjBinNameGrid = [];
    CreateBin();

});

$("#CreateButton").click(function () {
    AuthenticateCurdActions(GBLProductionUnitID).then(isAuthorized => {
        if (!isAuthorized) {
            return;
        }
        SelectedProductionUnitID = 0;
        SelectedProductionUnitName = '';
        GblStatus = "";
        $("#BtnNew").click();
        //document.getElementById("CreateButton").setAttribute("data-toggle", "modal");
        //document.getElementById("CreateButton").setAttribute("data-target", "#largeModal");
        $('#largeModal').modal({
            show: 'true'
        });

    });
});

var ObjBinNameGrid = [];
CreateBin()
function CreateBin() {
    $("#BinNameGrid").dxDataGrid({
        dataSource: ObjBinNameGrid,
        columnAutoWidth: true,
        showBorders: true,
        showRowLines: true,
        allowColumnReordering: true,
        allowColumnResizing: true,
        sorting: {
            mode: "none"
        },
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
            allowUpdating: true
        },

        onRowRemoving: function (e) {

            GetRowWarehouseID = "";
            GetRowWarehouseID = e.data.WarehouseID;

            if (isNaN(GetRowWarehouseID)) {
                console.log("this row not exist");
            } else {
                $.ajax({
                    type: "POST",
                    async: false,
                    url: "WebService_WarehouseMaster.asmx/CheckPermission",
                    data: '{GetRowWarehouseID:' + JSON.stringify(GetRowWarehouseID) + '}',
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function (results) {
                        var res = JSON.stringify(results);
                        res = res.replace(/"d":/g, '');
                        res = res.replace(/{/g, '');
                        res = res.replace(/}/g, '');
                        res = res.substr(1);
                        res = res.slice(0, -1);
                        if (res == "Exist") {
                            swal("", "This item is used in another process..! Record can not be delete.", "error");
                            e.cancel = true;
                        }
                    }
                });
            }
        },
        //onRowRemoved: function (e) {           
        //        e.component.undeleteRow(0);
        //},
        onRowPrepared: function (e) {
            if (e.rowType === "header") {
                e.rowElement.css('background', '#509EBC');
                e.rowElement.css('color', 'white');
                e.rowElement.css('font-weight', 'bold');
            }
            e.rowElement.css('fontSize', '11px');
        },
        columns: [{ dataField: "WarehouseID", visible: false, caption: "WarehouseID", },
        { dataField: "BinName", visible: true, caption: "Bin Name", },

        ]

    })
}

$("#BtnAddRowCol").click(function () {
    var CheckExistBinName = "";
    var TxtBinName = document.getElementById("TxtBinName").value;

    var BinNameGrid = $('#BinNameGrid').dxDataGrid('instance');
    var BinNameGridCount = BinNameGrid.totalCount();

    if (BinNameGridCount > 0) {
        for (var h = 0; h < BinNameGridCount; h++) {
            if (TxtBinName != "") {
                if (BinNameGrid._options.dataSource[h].BinName.toLowerCase() == TxtBinName.toLowerCase()) {
                    CheckExistBinName = "yes";
                    BinNameGrid.cellValue(h, "BinName", BinNameGrid._options.dataSource[h].BinName);
                    //DevExpress.ui.notify("This Bin name already exist..Please enter another Bin name ..!", "error", 1000);
                    showDevExpressNotification("This Bin name already exist..Please enter another Bin name ..!", "warning");
                    return false;
                }
            }
        }
        if (CheckExistBinName == "") {
            var optpaytr = {}
            var GenID = BinNameGridCount + 1;
            optpaytr.WarehouseID = "New" + GenID;
            optpaytr.BinName = TxtBinName;
            ObjBinNameGrid.push(optpaytr);

            $("#BinNameGrid").dxDataGrid({
                dataSource: ObjBinNameGrid,
            });
        }
    } else {
        var optpaytr = {}
        var GenID = BinNameGridCount + 1;
        optpaytr.WarehouseID = "New" + GenID;
        optpaytr.BinName = TxtBinName;
        ObjBinNameGrid.push(optpaytr);

        $("#BinNameGrid").dxDataGrid({
            dataSource: ObjBinNameGrid,
        });
    }

});

$("#BtnSave").click(function () {
    var prefix = "WH";
    var SelCity = $('#SelCity').dxSelectBox('instance').option('value');
    var BranchName = $('#BranchName').dxSelectBox('instance').option('value');
    var ProductionUnit = $('#ProductionUnit').dxSelectBox('instance').option('value');
    var TxtWarehouseName = document.getElementById("TxtWarehouseName").value;
    var TxtWarehouseAddress = document.getElementById("TxtWarehouseAddress").value;
    var WarehouseRefCode = document.getElementById("TxtWarehouseRefCode").value.toString().trim();
    var ChkIsFloorWareHouse = document.getElementById("ChkIsFloorWareHouse").checked;
    var TxtWarehouseCode = document.getElementById("TxtWarehouseCode").value;
    if (TxtWarehouseName == "" || TxtWarehouseName == undefined || TxtWarehouseName == null) {
        //DevExpress.ui.notify("Please enter Warehouse name...!", "error", 1000);
        showDevExpressNotification("Please enter Warehouse name...!", "warning");
        var text = "Please enter Warehouse name...!";
        document.getElementById("TxtWarehouseName").value = "";
        document.getElementById("TxtWarehouseName").focus();
        document.getElementById("ValStrTxtWarehouseName").style.display = "block";
        document.getElementById("ValStrTxtWarehouseName").innerHTML = text;
        return false;
    } else {
        document.getElementById("ValStrTxtWarehouseName").style.display = "none";
    }

    if (TxtWarehouseAddress == "" || TxtWarehouseAddress == undefined || TxtWarehouseAddress == null) {
        //DevExpress.ui.notify("Please enter Warehouse address...!", "error", 1000);
        showDevExpressNotification("Please enter Warehouse address...!", "warning");
        var text = "Please enter Warehouse address...!";
        document.getElementById("TxtWarehouseAddress").value = "";
        document.getElementById("TxtWarehouseAddress").focus();
        document.getElementById("ValStrTxtWarehouseAddress").style.display = "block";
        document.getElementById("ValStrTxtWarehouseAddress").innerHTML = text;
        return false;
    } else {
        document.getElementById("ValStrTxtWarehouseAddress").style.display = "none";
    }

    if (SelCity == "" || SelCity == undefined || SelCity == null) {
        //DevExpress.ui.notify("Please Choose City...!", "error", 1000);
        showDevExpressNotification("Please Choose City...!", "warning");
        var text = "Please Choose City...!";
        document.getElementById("SelCity").focus();
        document.getElementById("ValStrCity").style.display = "block";
        document.getElementById("ValStrCity").innerHTML = text;
        return false;
    } else {
        document.getElementById("ValStrCity").style.display = "none";
    }

    var BinNameGrid = $('#BinNameGrid').dxDataGrid('instance');
    var BinNameGridCount = BinNameGrid.totalCount();

    var jsonObjectsUpdateRecord = [];
    var OperationUpdateRecord = {};

    var jsonObjectsSaveRecord = [];
    var OperationSaveRecord = {};

    var CheckExistBinName = "";

    if (BinNameGridCount > 0) {
        for (var h = 0; h < BinNameGridCount; h++) {
            if (BinNameGrid._options.dataSource[h].BinName == "" || BinNameGrid._options.dataSource[h].BinName == null || BinNameGrid._options.dataSource[h].BinName == undefined) {
                BinNameGrid.cellValue(h, "BinName", "");
                //DevExpress.ui.notify("Please enter Bin name ..!", "error", 1000);
                showDevExpressNotification("Please enter Bin name ..!", "warning");
                return false;
            }
            if (h > 0) {
                CheckExistBinName = BinNameGrid._options.dataSource[h - 1].BinName;
                if (CheckExistBinName.toLowerCase() == (BinNameGrid._options.dataSource[h].BinName).toLowerCase()) {
                    BinNameGrid.cellValue(h, "BinName", BinNameGrid._options.dataSource[h].BinName);
                    //DevExpress.ui.notify("Already exist this Bin name ..!", "error", 1000);
                    showDevExpressNotification("Already exist this Bin name ..!", "warning");
                    return false;
                }
            }
        }
    }

    if (GblStatus == "Update") {
        if (BinNameGridCount > 0) {
            for (var t = 0; t < BinNameGridCount; t++) {
                var getGridBinID = BinNameGrid._options.dataSource[t].WarehouseID;

                if (isNaN(getGridBinID)) {
                    OperationSaveRecord = {};
                    OperationSaveRecord.WarehouseBinName = TxtWarehouseName + "-" + BinNameGrid._options.dataSource[t].BinName;
                    OperationSaveRecord.WarehouseName = TxtWarehouseName;
                    OperationSaveRecord.BinName = BinNameGrid._options.dataSource[t].BinName;
                    OperationSaveRecord.City = SelCity;
                    OperationSaveRecord.ProductionUnitID = ProductionUnit;
                    OperationSaveRecord.BranchID = BranchName;
                    OperationSaveRecord.Address = TxtWarehouseAddress;
                    OperationSaveRecord.IsFloorWarehouse = ChkIsFloorWareHouse;
                    jsonObjectsSaveRecord.push(OperationSaveRecord);
                }
                else {
                    OperationUpdateRecord = {};
                    OperationUpdateRecord.WarehouseID = BinNameGrid._options.dataSource[t].WarehouseID;
                    OperationUpdateRecord.WarehouseBinName = TxtWarehouseName + "-" + BinNameGrid._options.dataSource[t].BinName;
                    OperationUpdateRecord.WarehouseName = TxtWarehouseName;
                    OperationUpdateRecord.RefWarehouseCode = WarehouseRefCode;
                    OperationUpdateRecord.BinName = BinNameGrid._options.dataSource[t].BinName;
                    OperationUpdateRecord.City = SelCity;
                    OperationUpdateRecord.ProductionUnitID = ProductionUnit;
                    OperationUpdateRecord.BranchID = BranchName;
                    OperationUpdateRecord.Address = TxtWarehouseAddress;
                    OperationUpdateRecord.IsFloorWarehouse = ChkIsFloorWareHouse;
                    jsonObjectsUpdateRecord.push(OperationUpdateRecord);
                }

            }
        } else {
            //DevExpress.ui.notify("Please enter Bin name ..!", "error", 1000);
            showDevExpressNotification("Please enter Bin name ..!", "warning");
            return false;
        }
    }
    else {
        if (BinNameGridCount > 0) {
            for (var t = 0; t < BinNameGridCount; t++) {
                OperationSaveRecord = {};
                OperationSaveRecord.WarehouseBinName = TxtWarehouseName + "-" + BinNameGrid._options.dataSource[t].BinName;
                OperationSaveRecord.WarehouseName = TxtWarehouseName;
                OperationSaveRecord.BinName = BinNameGrid._options.dataSource[t].BinName;
                OperationSaveRecord.City = SelCity;
                OperationSaveRecord.ProductionUnitID = ProductionUnit;
                OperationSaveRecord.BranchID = BranchName;

                OperationSaveRecord.Address = TxtWarehouseAddress;
                OperationSaveRecord.IsFloorWarehouse = ChkIsFloorWareHouse;
                jsonObjectsSaveRecord.push(OperationSaveRecord);
            }
        }
        else {
            //DevExpress.ui.notify("Please enter Bin name ..!", "error", 1000);
            showDevExpressNotification("Please enter Bin name ..!", "warning");
            return false;
        }
    }

    var txt = 'If you confident please click on \n' + 'Yes, Save it ! \n' + 'otherwise click on \n' + 'Cancel';
    swal({
        title: "Do you want to continue",
        text: txt,
        type: "warning",
        showCancelButton: true,
        confirmButtonColor: "#DD6B55",
        confirmButtonText: "Yes, Save it !",
        closeOnConfirm: false
    },
        function () {
            if (GblStatus == "Update") {
                if (Number(SelectedProductionUnitID) != 0) {
                    if (GBLProductionUnitID != SelectedProductionUnitID) {
                        swal("Attention!", "Selected transaction is related to unit " + SelectedProductionUnitName + ", Kindly login with unit " + SelectedProductionUnitName + " to process.", "warning");
                        $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                        return;
                    }
                }

                document.getElementById("LOADER").style.display = "block";
                $.ajax({
                    type: "POST",
                    url: "WebService_WarehouseMaster.asmx/UpdateWarehouse",
                    data: '{TxtWarehouseID:' + JSON.stringify(document.getElementById("TxtWarehouseID").value) + ',jsonObjectsSaveRecord:' + JSON.stringify(jsonObjectsSaveRecord) + ',jsonObjectsUpdateRecord:' + JSON.stringify(jsonObjectsUpdateRecord) + '}',
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
                        if (res == "Success") {
                            document.getElementById("BtnSave").setAttribute("data-dismiss", "modal");
                            swal("Updated!", "Your data Updated", "success");
                            location.reload();
                        } else if (res.includes("not authorized")) {
                            swal.close();
                            setTimeout(() => {
                                swal("Warning..!", res, "warning");
                            }, 100);
                        }
                        else if (res == "Exist") {
                            swal("Duplicate!", "This Group Name allready Exist..\n Please enter another Group Name..", "");
                        }

                    },
                    error: function errorFunc(jqXHR) {
                        document.getElementById("LOADER").style.display = "none";
                        swal("Error!", "Please try after some time..", "");
                    }
                });
            }
            else {

                document.getElementById("LOADER").style.display = "block";

                $.ajax({
                    type: "POST",
                    url: "WebService_WarehouseMaster.asmx/SaveWarehouse",
                    data: '{prefix:' + JSON.stringify(prefix) + ',jsonObjectsSaveRecord:' + JSON.stringify(jsonObjectsSaveRecord) + '}',
                    // data: '{prefix:' + JSON.stringify(prefix) + '}',
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function (results) {
                        document.getElementById("LOADER").style.display = "none";

                        if (results.d == "Success") {
                            swal("Saved!", "Your data saved", "success");
                            location.reload();
                        } else if (res.includes("not authorized")) {
                            swal.close();
                            setTimeout(() => {
                                swal("Warning..!", res, "warning");
                            }, 100);
                        }
                        else {
                            swal("Not Saved!", results.d, "");
                        }
                    },
                    error: function errorFunc(jqXHR) {
                        document.getElementById("LOADER").style.display = "none";
                        //  $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                        swal("Error!", "Please try after some time..", "");
                    }
                });
            }
        });

});

$("#EditButton").click(function () {

    let warehouseList = $('#WarehouseShowListGrid').dxDataGrid('instance');
    let selectedRowData = warehouseList.getSelectedRowsData();
    if (selectedRowData.length <= 0) {
        alert("Please Choose any row from below Grid..!");
        return false;
    }

    SelectedProductionUnitID = selectedRowData[0].ProductionUnitID;
    SelectedProductionUnitName = selectedRowData[0].ProductionUnitName;
    document.getElementById("TxtWarehouseID").value = selectedRowData[0].WarehouseName;

    let TxtWarehouseID = document.getElementById("TxtWarehouseID").value;
    //if (TxtWarehouseID == "" || TxtWarehouseID == null || TxtWarehouseID == undefined) {
    //    alert("Please Choose any row from below Grid..!");
    //    return false;
    //}
    GblStatus = "Update";

    document.getElementById("EditButton").setAttribute("data-toggle", "modal");
    document.getElementById("EditButton").setAttribute("data-target", "#largeModal");

    document.getElementById("LOADER").style.display = "block";

    document.getElementById("TxtWarehouseCode").value = selectedRowData[0].WarehouseCode;
    document.getElementById("TxtWarehouseName").value = selectedRowData[0].WarehouseName;
    document.getElementById("TxtWarehouseRefCode").value = selectedRowData[0].RefWarehouseCode;
    document.getElementById("TxtWarehouseAddress").value = selectedRowData[0].Address;
    document.getElementById("ChkIsFloorWareHouse").checked = selectedRowData[0].IsFloorWarehouse;
    $("#SelCity").dxSelectBox({
        value: selectedRowData[0].City
    });

    $("#BranchName").dxSelectBox({
        value: selectedRowData[0].BranchID
    });

    $("#ProductionUnit").dxSelectBox({
        value: selectedRowData[0].ProductionUnitID
    });

    $.ajax({
        type: "POST",
        url: "WebService_WarehouseMaster.asmx/SelectBinName",
        data: '{WarehouseName:' + JSON.stringify(TxtWarehouseID) + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            ////console.debug(results);
            var res = results.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/""/g, '');
            res = res.substr(1);
            res = res.slice(0, -1);

            document.getElementById("LOADER").style.display = "none";
            var IssueRetrive = JSON.parse(res);

            ObjBinNameGrid = [];
            ObjBinNameGrid = IssueRetrive;
            CreateBin();
        }
    });

});

$("#BranchName").dxSelectBox({
    items: [],
    displayExpr: 'BranchName',
    valueExpr: 'BranchID',
    searchEnabled: true,
    showClearButton: true,
});

$("#ProductionUnit").dxSelectBox({
    items: [],
    displayExpr: 'ProductionUnitName',
    valueExpr: 'ProductionUnitID',
    searchEnabled: true,
    showClearButton: true,
});



GetBranch()
function GetBranch() {
    $.ajax({
        type: "POST",
        url: "WebService_WarehouseMaster.asmx/GetBranch",
        data: {},
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            let res = results.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/""/g, '');
            res = res.replace(/u0026/g, '&');
            res = res.replace(/u0027/g, "'");
            res = res.replace(/:,/g, ":null,");
            res = res.replace(/,}/g, ",null}");
            res = res.replace(/:}/g, ":null}");
            res = res.replaceAll("\'", "#-");
            res = res.substr(1);
            res = res.slice(0, -1);
            let RES1 = JSON.parse(res);

            $("#BranchName").dxSelectBox({
                dataSource: RES1
            });
        }
    });
}


GetProduction()
function GetProduction() {
    $.ajax({
        type: "POST",
        url: "WebService_OtherMaster.asmx/GetMachineProductionUnitList",
        data: {},
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            let res = results.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/""/g, '');
            res = res.replace(/u0026/g, '&');
            res = res.replace(/u0027/g, "'");
            res = res.replace(/:,/g, ":null,");
            res = res.replace(/,}/g, ",null}");
            res = res.replace(/:}/g, ":null}");
            res = res.replaceAll("\'", "#-");
            res = res.substr(1);
            res = res.slice(0, -1);
            let RES1 = JSON.parse(res);

            $("#ProductionUnit").dxSelectBox({
                dataSource: RES1
            });
        }
    });
}


GetWarehouseNo();
function GetWarehouseNo() {
    $.ajax({
        type: "POST",
        url: "WebService_WarehouseMaster.asmx/GetWarehouseNo",
        data: {},
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (results) {
            var res = results.d;
            document.getElementById("TxtWarehouseCode").value = res;
        },

    });

}
