//"use strict";

//=============Active Link By Element ID By Pradeep============================
var GblStatus = "", ConcernPerson = "", GblBVStatus = false;
var LedgerGrNID = "";
var ObjGrid = [];
var isSaveASClicked = false;
var embargoStatus = 'Not Placed';
var RadioValue = ["Bad Payment Overdue history", "Others"];
var SelStatus = ["Active", "Inactive"];
var AvailableCredit = 0;
var GBLProductionUnitID = getProductionUnitID('ProductionUnitID');
var GBLCompanyID = getProductionUnitID('CompanyID');

$("#LoadIndicator").dxLoadPanel({
    shadingColor: "rgba(0,0,0,0.4)",
    indicatorSrc: "images/Indus logo.png",
    message: 'Please Wait...',
    width: 310,
    showPane: true,
    shading: true,
    closeOnOutsideClick: false,
    visible: false
});

if ((localStorage.getItem('activeID')) !== "" || (localStorage.getItem('activeID')) !== undefined || (localStorage.getItem('activeID')) !== "null") {

    var tagID = localStorage.getItem('activeID');
    var tagNAME = localStorage.getItem('activeName');

    document.getElementById("MasterID").innerHTML = tagID;
    document.getElementById("MasterName").innerHTML = tagNAME;

    var MD = tagNAME;
    // MD = MD.replace(//g, ' ');
    document.getElementById("MasterDisplayName").innerHTML = MD;
}

function OpenPopup(PU) {
    document.getElementById("mySidenav").style.width = "0";
    document.getElementById('MYbackgroundOverlay').style.display = 'none';

    document.getElementById(PU.id).setAttribute("data-toggle", "modal");
    document.getElementById(PU.id).setAttribute("data-target", "#largeModal");
}

getMasterLIST();

$("#MasterGrid").dxDataGrid({
    dataSource: [],
    columnAutoWidth: true,
    showBorders: true,
    showRowLines: true,
    allowColumnReordering: true,
    allowColumnResizing: true,
    columnResizingMode: "widget",
    paging: {
        pageSize: 25
    },
    pager: {
        showPageSizeSelector: true,
        allowedPageSizes: [25, 100, 250, 500, 1000]
    },
    height: function () {
        return window.innerHeight / 1.2;
    },
    sorting: {
        mode: "multiple"
    },
    selection: { mode: "single" },
    //height: 600,
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
        fileName: document.getElementById("MasterDisplayName").innerHTML,
        allowExportSelectedData: true
    },

    onExporting(e) {
        const workbook = new ExcelJS.Workbook();
        const worksheet = workbook.addWorksheet('LedgerMaster');

        DevExpress.excelExporter.exportDataGrid({
            component: e.component,
            worksheet,
            autoFilterEnabled: true,
        }).then(() => {
            workbook.xlsx.writeBuffer().then((buffer) => {
                saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'LedgerMaster.xlsx');
            });
        });
        e.cancel = true;
    },

    onRowPrepared: function (e) {
        if (e.rowType === "header") {
            e.rowElement.css('background', '#42909A');
            e.rowElement.css('color', 'white');
        }
        e.rowElement.css('fontSize', '11px');
    },
    onSelectionChanged: function (selectedItems) {
        document.getElementById("txtGetGridRow").value = "";
        isSaveASClicked = false;
        UnderGroupID = "";
        var data = selectedItems.selectedRowsData;
        if (data.length <= 0) return false;

        document.getElementById("txtGetGridRow").value = data[0].LedgerID; /// grid.cellValue(Row, 0);        
        UnderGroupID = data[0].LedgerGroupID; ///grid.cellValue(Row, 1);
    }
});

//Dynamic Maser UL
function getMasterLIST() {
    var masterID = document.getElementById("MasterID").innerHTML;

    var currentMaster = "";
    if (masterID !== "") {
        currentMaster = "ChooseMaster" + masterID;
    }
    try {
        $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
        $.ajax({
            type: "POST",
            url: "WebService_LedgerMaster.asmx/MasterList",
            data: '{}',
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            success: function (results) {
                var res = results.replace(/\\/g, '');
                res = res.replace(/"d":""/g, '');
                res = res.replace(/""/g, '');
                res = res.replace(/:}/g, ':null}');
                res = res.replace(/:,/g, ':null,');
                res = res.replace(/u0026/g, '&');
                res = res.substr(1);
                res = res.slice(0, -1);
                //alert(res);
                var RES1 = JSON.parse(res);
                //alert(RES1);
                var MasterList = "";
                document.getElementById("MasterUL").innerHTML = "";

                for (var i = 0; i < RES1.length; i++) {
                    //MasterList += "<li role='presentation' id=ChooseMaster" + RES1[i].LedgerGroupID + " class=''><a id=" + RES1[i].LedgerGroupID + "   href='#' data-toggle='tab'  onclick='CurrentMaster(this);' style='color:#42909A;font-size:10px;font-weight:600;width:100%'>" + RES1[i].LedgerGroupName.replace(//g, ' '); + "</a></li>";
                    MasterList += "<li role='presentation' id=ChooseMaster" + RES1[i].LedgerGroupID + " class=''><a id=" + RES1[i].LedgerGroupID + "   href='#' data-toggle='tab'  onclick='CurrentMaster(this);' style='color:#42909A;font-size:10px;font-weight:600;width:100%;text-align: left;'>" + RES1[i].LedgerGroupNameDisplay; + "</a></li>";

                }

                $("#MasterUL").append('<li style="border-bottom:1px solid #42909A"><label style="color: #42909A; margin-left: .5em;font-size:12px;font-weight:600">Select Ledger</label></li>');
                $('#MasterUL').append(MasterList);

                if (currentMaster !== "") {
                    document.getElementById(currentMaster).className = "active";
                }
                document.getElementById("LI_ChooseMaster").className = "dropdown open";
            }
        });
    } catch (e) {
        $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
    }
}

//Selected Master
function CurrentMaster(e) {

    $("#MasterUL>li.active").removeClass("active");
    var tagname = e.id;

    localStorage.setItem('activeID', e.id);
    localStorage.setItem('activeName', document.getElementById(tagname).text);

    if (localStorage.getItem('activeID') !== "") {
        document.getElementById("MasterID").innerHTML = localStorage.getItem('activeID');
        document.getElementById("MasterName").innerHTML = localStorage.getItem('activeName');

        var MD = localStorage.getItem('activeName');
        // MD = MD.replace(//g, ' ');
        document.getElementById("MasterDisplayName").innerHTML = MD;
        if (MD.toUpperCase().includes("CLIENT")) $("#btnConvertToConsignee").show(); else $("#btnConvertToConsignee").hide();
        if (MD.toUpperCase().includes("CLIENT")) $("#BtnBVSetting").show(); else $("#BtnBVSetting").hide();

        $.ajax({
            type: "POST",
            url: "WebService_LedgerMaster.asmx/GetLedgerGroupNameID",
            data: '{MID:' + JSON.stringify(document.getElementById("MasterID").innerHTML) + '}',
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            success: function (results) {
                ////console.debug(results);
                var res = results.replace(/\\/g, '');
                res = res.replace(/"d":""/g, '');
                res = res.replace(/""/g, '');
                res = res.replace(/:}/g, ':null}');
                res = res.replace(/:,/g, ':null,');
                res = res.replace(/u0026/g, '&');
                res = res.substr(1);
                res = res.slice(0, -1);
                var LGNI = JSON.parse(res);
                LedgerGrNID = "";
                LedgerGrNID = LGNI[0].LedgerGroupNameID;
                if (LedgerGrNID === 23 || LedgerGrNID === "23") {
                    document.getElementById("btnItemGroupAllo").style.display = "block";
                }
                else {
                    document.getElementById("btnItemGroupAllo").style.display = "none";
                }
            }
        });

    }
    FillGrid();

    DynamicControlls();

}

var selID = ""; var selQuery = ""; var selDefault = "";

var GBLField = "";

//Creation of Dynamic Field on Popup
$("#CreateButton").click(function () {
    AuthenticateCurdActions(GBLProductionUnitID).then(async (isAuthorized) => {
        if (!isAuthorized) {
            return;
        }
        GblStatus = "";
        refreshbtn();

        document.getElementById("BtnSaveAS").disabled = true;
        document.getElementById("BtnDeletePopUp").disabled = true;
        document.getElementById("BtnBVSetting").disabled = true;
        document.getElementById("BtnSave").disabled = "";

        document.getElementById("Isactivledgerstatic_Div").style.display = "none";
        // DynamicControlls();

        document.getElementById("mySidenav").style.width = "0";
        document.getElementById('MYbackgroundOverlay').style.display = 'none';

        let masterGrid = $("#MasterGrid").dxDataGrid("instance");
        masterGrid.clearSelection();
        document.getElementById("txtGetGridRow").value = "";
        $('#largeModal').modal({
            show: 'true'
        });
    });
});

//Function For Create Controlls
function DynamicControlls() {
    GblStatus = "";

    var masterID = document.getElementById("MasterID").innerHTML;
    var fieldContainer = "";
    document.getElementById("FieldCntainerRow").innerHTML = "";
    $.ajax({
        type: "POST",
        url: "WebService_LedgerMaster.asmx/Master",
        data: '{masterID:' + JSON.stringify(masterID) + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            ////console.debug(results);
            var res = results.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/""/g, '');
            res = res.replace(/:,/g, ':null,');
            res = res.replace(/:}/g, ':null}');
            res = res.replace(/u0027/g, "'");
            res = res.substr(1);
            res = res.slice(0, -1);
            //$("#LoadIndicator").dxLoadPanel("instance").option("visible", false);

            GBLField = JSON.parse(res);
            var RES1 = GBLField;

            selQuery = ""; selID = "";
            if (RES1.length > 0) {
                for (var i = 0; i < RES1.length; i++) {
                    var DEselID = "";
                    var IsDisplayCol = "";
                    if (RES1[i].IsDisplay === true || RES1[i].IsDisplay === 1) {
                        IsDisplayCol = "block";
                    }
                    else {
                        IsDisplayCol = "none";
                    }

                    let fieldValidatorMark = "";
                    if (RES1[i].IsRequiredFieldValidator !== undefined && RES1[i].IsRequiredFieldValidator !== null) {
                        if (RES1[i].IsRequiredFieldValidator) {
                            fieldValidatorMark = '<b style="color:red">*</b>'
                        }
                    }

                    if (RES1[i].FieldType === "text" || RES1[i].FieldType === "number") {
                        fieldContainer = "";
                        var chngevt = RES1[i].ControllValidation;
                        if (RES1[i].FieldFormula === "" || RES1[i].FieldFormula === null || RES1[i].FieldFormula === undefined) {
                            if (chngevt === "" || chngevt === null || chngevt === "null" || chngevt === undefined) {
                                fieldContainer = '<div class="col-lg-3 col-md-3 col-sm-3 col-xs-12" style="float:left;margin-bottom:0px;display:' + IsDisplayCol + '">' +
                                    '<label style="float: left; width: 100%;">' + RES1[i].FieldDisplayName + fieldValidatorMark + '</label><br />' +
                                    '<input id=' + RES1[i].FieldName + ' type="' + RES1[i].FieldType + '" class="forTextBox" min="0"/><br />' +
                                    '<div style="min-height:20px;float:left;width:100%"><strong id=ValStr' + RES1[i].FieldName + ' style="color:red;font-size:12px;display:none"></strong></div></div>';

                            } else {
                                fieldContainer = '<div class="col-lg-3 col-md-3 col-sm-3 col-xs-12" style="float:left;margin-bottom:0px;display:' + IsDisplayCol + '">' +
                                    '<label style="float: left; width: 100%;">' + RES1[i].FieldDisplayName + fieldValidatorMark + '</label><br />' +
                                    '<input id=' + RES1[i].FieldName + ' type="' + RES1[i].FieldType + '" class="forTextBox" onchange="' + chngevt + '" min="0"/><br />' +
                                    '<div style="min-height:20px;float:left;width:100%"><strong id=ValStr' + RES1[i].FieldName + ' style="color:red;font-size:10px;display:block"></strong></div></div>';
                            }
                            $("#FieldCntainerRow").append(fieldContainer);
                        } else {
                            fieldContainer = '<div class="col-lg-3 col-md-3 col-sm-3 col-xs-12" style="float:left;margin-bottom:0px;display:' + IsDisplayCol + '">' +
                                '<label style="float: left; width: 100%;">' + RES1[i].FieldDisplayName + fieldValidatorMark + '</label><br />' +
                                '<input id=' + RES1[i].FieldName + ' type="' + RES1[i].FieldType + '"  class="forTextBox" onchange="FarmulaChange(this);" min="0"/><br />' +
                                '<div style="min-height:20px;float:left;width:100%"><strong id=ValStr' + RES1[i].FieldName + ' style="color:red;font-size:10px;display:block"></strong><textarea id=ValCh' + RES1[i].FieldName + ' style="display: none" >' + RES1[i].FieldFormulaString + '</textarea><strong id=Formula' + RES1[i].FieldName + ' style="display: none">' + RES1[i].FieldFormula + '</strong></div></div>';
                            $("#FieldCntainerRow").append(fieldContainer);
                        }
                    }
                    else if (RES1[i].FieldType === "checkbox") {
                        fieldContainer = "";
                        fieldContainer = '<div class="col-lg-3 col-md-3 col-sm-3 col-xs-12" style="float:left;margin-bottom:0px;display:' + IsDisplayCol + '">' +
                            '<label style="float: left; width: 100%;">' + RES1[i].FieldDisplayName + fieldValidatorMark + '</label><br />' +
                            '<input type="checkbox" id="' + RES1[i].FieldName + '" class="filled-in chk-col-red" style="height:20px"/>' +
                            '<label for="' + RES1[i].FieldName + '" style="height:20px"></label><br />' +
                            '<div style="min-height:20px;float:left;width:100%"><strong id=ValStr' + RES1[i].FieldName + ' style="color:red;font-size:10px"></strong></div></div>';
                        $("#FieldCntainerRow").append(fieldContainer);
                    }
                    else if (RES1[i].FieldType === "textarea") {
                        fieldContainer = "";
                        var chngevtn = RES1[i].ControllValidation;
                        fieldContainer = '<div class="col-lg-3 col-md-3 col-sm-3 col-xs-12" style="float:left;margin-bottom:0px;display:' + IsDisplayCol + '">' +
                            '<label style="float: left; width: 100%;">' + RES1[i].FieldDisplayName + fieldValidatorMark + '</label><br />' +
                            '<textarea id="' + RES1[i].FieldName + '" style="float: left; width: 100%; height: 27px; border-radius: 4px;padding-left:10px;padding-right:10px;margin-top:0px" onchange="' + chngevtn + '"></textarea><br />' +
                            '<div style="min-height:20px;float:left;width:100%"><strong id=ValStr' + RES1[i].FieldName + ' style="color:red;font-size:10px"></strong></div></div>';
                        $("#FieldCntainerRow").append(fieldContainer);
                    }
                    else if (RES1[i].FieldType === "datebox") {
                        fieldContainer = "";
                        fieldContainer = '<div class="col-lg-3 col-md-3 col-sm-3 col-xs-12" style="float:left;margin-bottom:0px;display:' + IsDisplayCol + '">' +
                            '<label style="float: left; width: 100%;">' + RES1[i].FieldDisplayName + fieldValidatorMark + '</label><br />' +
                            '<div id="' + RES1[i].FieldName + '"  style="float: left; width: 100%;height:30px;border: 1px solid #d3d3d3"></div><br />' +
                            '<div style="min-height:20px;float:left;width:100%"><strong id=ValStr' + RES1[i].FieldName + ' style="color:red;font-size:10px"></strong>  </div></div>';
                        $("#FieldCntainerRow").append(fieldContainer);
                        $("#" + RES1[i].FieldName).dxDateBox({
                            pickerType: "calendar",
                            formate: 'date',
                            value: new Date().toISOString().substr(0, 10),
                            formatString: 'dd-MMM-yyyy'
                        });
                    }
                    else if (RES1[i].FieldType === "selectbox") {
                        DEselID = "";
                        DEselID = RES1[i].FieldName;
                        var DEselQuery = "";
                        DEselQuery = RES1[i].SelectBoxQueryDB;

                        var DEselDefault = "";
                        DEselDefault = RES1[i].SelectBoxDefault;

                        if (DEselQuery === "" || DEselQuery === "null" || DEselQuery === null || DEselQuery === undefined) {

                            fieldContainer = "";
                            fieldContainer = '<div class="col-lg-3 col-md-3 col-sm-3 col-xs-12" style="float:left;margin-bottom:0px;display:' + IsDisplayCol + '">' +
                                '<label style="float: left; width: 100%;">' + RES1[i].FieldDisplayName + fieldValidatorMark + '</label><br />' +
                                '<div id="' + RES1[i].FieldName + '"  style="float: left; width: 100%;height:30px;border: 1px solid #d3d3d3"></div><br />' +
                                '<div style="min-height:20px;float:left;width:100%"><strong id=ValStr' + RES1[i].FieldName + ' style="color:red;font-size:10px"></strong>  <textarea id=ValCh' + RES1[i].FieldName + ' style="display: none" >' + RES1[i].FieldFormulaString + '</textarea><strong id=Formula' + RES1[i].FieldName + ' style="display: none">' + RES1[i].FieldFormula + '</strong>  </div></div>';
                            $("#FieldCntainerRow").append(fieldContainer);

                            var LedgerPush = [];
                            var LedgerLength = 0;
                            if (DEselDefault !== null && DEselDefault !== "null" && DEselDefault !== undefined) {

                                var Ledger = DEselDefault.split(',');
                                LedgerLength = Ledger.length;
                            }


                            if (LedgerLength > 0) {
                                for (var k = 0; k < LedgerLength; k++) {
                                    LedgerPush.push(Ledger[k]);
                                }
                            }
                            var SID = "#" + DEselID;
                            if (RES1[i].FieldFormula === "" || RES1[i].FieldFormula === null || RES1[i].FieldFormula === undefined) {
                                $(SID).dxSelectBox({
                                    items: LedgerPush,
                                    placeholder: "Select--",
                                    //displayExpr: 'GroupName',
                                    //valueExpr: 'GroupID',
                                    showClearButton: true,
                                    acceptCustomValue: true,
                                    searchEnabled: true
                                });
                            } else {
                                $(SID).dxSelectBox({
                                    items: LedgerPush,
                                    placeholder: "Select--",
                                    //displayExpr: 'GroupName',
                                    //valueExpr: 'GroupID',
                                    showClearButton: true,
                                    acceptCustomValue: true,
                                    searchEnabled: true,
                                    onValueChanged: function (data) {
                                        if (data) {
                                            var currentID = data.element.context.id;
                                            FarmulaChangeSELECTBX(currentID);
                                        }
                                    }
                                });
                            }

                        }
                        else {

                            fieldContainer = "";
                            fieldContainer = '<div class="col-lg-3 col-md-3 col-sm-3 col-xs-12" style="float:left;margin-bottom:0px;display:' + IsDisplayCol + '">' +
                                '<label style="float: left; width: 100%;">' + RES1[i].FieldDisplayName + fieldValidatorMark + '</label><br />' +
                                '<div id="' + RES1[i].FieldName + '" style="float: left; width: 100%;height:30px;border: 1px solid #d3d3d3"></div><br />' +
                                '<div style="min-height:20px;float:left;width:100%"><strong id=ValStr' + RES1[i].FieldName + ' style="color:red;font-size:10px"></strong>   <textarea id=ValCh' + RES1[i].FieldName + ' style="display: none" >' + RES1[i].FieldFormulaString + '</textarea><strong id=Formula' + RES1[i].FieldName + ' style="display: none">' + RES1[i].FieldFormula + '</strong>  </div></div>';
                            $("#FieldCntainerRow").append(fieldContainer);

                            if (selID === "" || selID === null || selID === undefined) {
                                selID = RES1[i].FieldName;
                            }
                            else {
                                selID = selID + ' ? ' + RES1[i].FieldName;
                            }

                            if (selQuery === "" || selQuery === null || selQuery === undefined) {
                                // selQuery = RES1[i].SelectboxQueryDB;
                                selQuery = RES1[i].LedgerGroupFieldID;
                            }
                            else {
                                //selQuery = selQuery + ' ? ' + RES1[i].SelectboxQueryDB;
                                selQuery = selQuery + ' ? ' + RES1[i].LedgerGroupFieldID;
                            }

                        }

                    }

                }

                $("#FieldCntainerRow").append('<div id="Isactivledgerstatic_Div" class="col-lg-3 col-md-3 col-sm-3 col-xs-12" style="float:left;margin-bottom:0px;display:none">' +
                    '<label style="float: left; width: 100%;">Is Active Ledger.?</label><br />' +
                    '<input type="checkbox" id="Isactivledgerstatic" class="filled-in chk-col-red" style="height:20px" checked="true"/>' +
                    '<label for="Isactivledgerstatic" style="height:20px" ></label><br />' +
                    '<div style="min-height:20px;float:left;width:100%"><strong id="ValStrIsactivledgerstaticLabel" style="color:red;font-size:10px"></strong></div></div>');


                if (selQuery !== "") {
                    selctbox();
                }
                if (RES1.length > 1) {
                    document.getElementById("BtnSave").disabled = false;
                }
            }
            else {
                document.getElementById("BtnSave").disabled = true;
            }
        }
    });
}

//Fill Dynamic Selectbox
function selctbox() {
    var selbox = "";
    $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
    $.ajax({
        type: "POST",
        url: "WebService_LedgerMaster.asmx/SelectBoxLoad",
        data: '{Qery:' + JSON.stringify(selQuery) + ',selID:' + JSON.stringify(selID) + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            var res = results.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/""/g, '');
            res = res.replace(/:}/g, ':null}');
            res = res.replace(/:,/g, ':null,');
            res = res.replace(/u0026/g, '&');
            res = res.substr(1);
            res = res.slice(0, -1);
            try {
                if (res === "") {
                    selbox = [];
                } else {
                    selbox = "";
                    selbox = JSON.parse(res);
                }
                var TBLName = Object.getOwnPropertyNames(selbox);

                for (var t in selbox) {
                    var Tblobj = selbox[t];
                    var selA = Object.getOwnPropertyNames(Tblobj[0]);
                    selA = JSON.stringify(selA);
                    selA = selA.substr(1);
                    selA = selA.slice(0, -1);
                    selA = selA.replace(/"/g, '');
                    selA = selA.split(",");

                    var Displayxpr = "";
                    var Valuexpr = "";
                    if (selA.length > 1) {
                        ///////////////////////////////////////With  valueExpr/////////////////////////////////////////////
                        Displayxpr = selA[1];
                        Valuexpr = selA[0];

                        var selectID = JSON.stringify(Tblobj[Tblobj.length - 1]);

                        selectID = selectID.substr(1);
                        selectID = selectID.slice(0, -1);
                        selectID = selectID.replace(/"/g, '');
                        selectID = selectID.replace(/ /g, '');
                        selectID = selectID.split(":");
                        selectID = "#" + selectID[2];

                        Tblobj = Tblobj.slice(0, -1);

                        $(selectID).dxSelectBox({
                            items: Tblobj,
                            placeholder: "Select--",
                            displayExpr: Displayxpr,
                            valueExpr: Valuexpr,
                            searchEnabled: true,
                            showClearButton: true,
                            onValueChanged: function (data) {
                                if (data) {
                                    var currentID = data.element.context.id;
                                    FarmulaChangeSELECTBX(currentID);
                                }
                            }
                            //onCustomItemCreating: function (e) {
                            //    //Add a new item to your data store based on the e.text value
                            //    var NewLedger = e.value;
                            //    Tblobj.push(NewLedger);
                            //    editableProduct.option("Ledgers", Tblobj);
                            //    e.customItem = NewLedger;
                            //   // return NewLedger;
                            //}
                        });

                    }
                    else {
                        Displayxpr = selA[0];
                        Valuexpr = selA[0];
                        ///////////////////////////////////////WithOut  valueExpr/////////////////////////////////////////////

                        var selectelseID = JSON.stringify(Tblobj[Tblobj.length - 1]);
                        selectelseID = selectelseID.substr(1);
                        selectelseID = selectelseID.slice(0, -1);
                        selectelseID = selectelseID.replace(/"/g, '');
                        selectelseID = selectelseID.replace(/ /g, '');
                        selectelseID = selectelseID.split(":");
                        var replaceText = selectelseID[1];
                        selectelseID = "#" + selectelseID[1];

                        Tblobj = Tblobj.slice(0, -1);

                        var ReplaceTblobj = JSON.stringify(Tblobj);
                        ReplaceTblobj = ReplaceTblobj.replace(new RegExp(replaceText, 'g'), '');
                        ReplaceTblobj = ReplaceTblobj.replace(/"":/g, '');
                        ReplaceTblobj = ReplaceTblobj.replace(/{/g, '');
                        ReplaceTblobj = ReplaceTblobj.replace(/}/g, '');
                        ReplaceTblobj = ReplaceTblobj.substr(1);
                        ReplaceTblobj = ReplaceTblobj.slice(0, -1);
                        ReplaceTblobj = ReplaceTblobj = ReplaceTblobj.replace(/"/g, '');

                        ReplaceTblobj = ReplaceTblobj.split(',');
                        var simpleProducts = [];
                        for (var Ledgertxt in ReplaceTblobj) {
                            simpleProducts.push(ReplaceTblobj[Ledgertxt]);
                        }

                        $(selectelseID).dxSelectBox({
                            items: simpleProducts,
                            //placeholder: "Select--",
                            //displayExpr: Displayxpr,
                            //valueExpr: Valuexpr,
                            searchEnabled: true,
                            showClearButton: true,
                            acceptCustomValue: true,
                            onValueChanged: function (data) {
                                if (data) {
                                    var currentID = data.element.context.id;
                                    FarmulaChangeSELECTBX(currentID);
                                }
                            }
                        });
                    }
                }
                $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);

            } catch (e) {
                $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
            }
        }
    });
}

//Save Dynamic Data
$("#BtnSave").click(function () {
    var columnlength = "";

    var MasterName = document.getElementById("MasterName").innerHTML;
    var masterID = document.getElementById("MasterID").innerHTML;

    $.ajax({
        type: "POST",
        url: "WebService_LedgerMaster.asmx/Master",
        data: '{masterID:' + JSON.stringify(masterID) + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            var res = results.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/""/g, '');
            res = res.replace(/:}/g, ':null}');
            res = res.replace(/:,/g, ':null,');
            res = res.replace(/u0027/g, "'");
            res = res.replace(/u0026/g, '&');
            res = res.substr(1);
            res = res.slice(0, -1);
            columnlength = JSON.parse(res);

            if (columnlength.length > 0) {
                var alertTag = "";
                for (var i = 0; i < columnlength.length; i++) {

                    var DataTypeVali = columnlength[i].FieldDataType;
                    DataTypeVali = DataTypeVali.substring(0, 3);
                    DataTypeVali = DataTypeVali.toUpperCase().trim();
                    var decimal = /^[0-9]+\.?[0-9]*$/;

                    if (columnlength[i].IsDisplay === true) {
                        if (columnlength[i].FieldType === "text" || columnlength[i].FieldType === "textarea") {
                            var x = document.getElementById(columnlength[i].FieldName).value;
                            if (DataTypeVali === "MON" || DataTypeVali === "FLO" || DataTypeVali === "REA") {
                                if (decimal.test(x) === true) {
                                    alertTag = "ValStr" + columnlength[i].FieldName;
                                    document.getElementById(alertTag).style.display = "none";
                                }
                                else {
                                    alert("Please enter Numeric OR Decimal value in " + columnlength[i].FieldDisplayName);
                                    alertTag = "ValStr" + columnlength[i].FieldName;
                                    document.getElementById(columnlength[i].FieldName).value = "";
                                    document.getElementById(columnlength[i].FieldName).focus();
                                    document.getElementById(alertTag).style.fontSize = "10px";
                                    document.getElementById(alertTag).style.display = "block";
                                    document.getElementById(alertTag).innerHTML = 'Please enter Numeric OR Decimal value in ' + columnlength[i].FieldDisplayName;
                                    return false;
                                }
                            }
                            if (DataTypeVali === "INT" || DataTypeVali === "BIG") {
                                if (isNaN(x)) {
                                    alert("Please enter numeric in " + columnlength[i].FieldDisplayName);
                                    alertTag = "ValStr" + columnlength[i].FieldName;
                                    document.getElementById(columnlength[i].FieldName).value = "";
                                    document.getElementById(columnlength[i].FieldName).focus();
                                    document.getElementById(alertTag).style.fontSize = "10px";
                                    document.getElementById(alertTag).style.display = "block";
                                    document.getElementById(alertTag).innerHTML = 'Please enter numeric in ' + columnlength[i].FieldDisplayName;
                                    return false;
                                }
                                alertTag = "ValStr" + columnlength[i].FieldName;
                                document.getElementById(alertTag).style.display = "none";
                            }
                        }
                        if (columnlength[i].FieldType === "selectbox") {
                            var xx = $("#" + columnlength[i].FieldName).dxSelectBox("instance").option('value');
                            if (DataTypeVali === "MON" || DataTypeVali === "FLO" || DataTypeVali === "REA") {
                                if (decimal.test(xx) === true) {
                                    alertTag = "ValStr" + columnlength[i].FieldName;
                                    document.getElementById(alertTag).style.display = "none";
                                }
                                else {
                                    alert("Please enter Numeric OR Decimal value in ." + columnlength[i].FieldDisplayName);
                                    alertTag = "ValStr" + columnlength[i].FieldName;
                                    $("#" + columnlength[i].FieldName).dxSelectBox({
                                        value: ""
                                    });
                                    document.getElementById(alertTag).style.fontSize = "10px";
                                    document.getElementById(alertTag).style.display = "block";
                                    document.getElementById(alertTag).innerHTML = 'Please enter Numeric OR Decimal value in ' + columnlength[i].FieldDisplayName;
                                    return false;
                                }
                            }
                            if (DataTypeVali === "INT" || DataTypeVali === "BIG") {
                                if (isNaN(xx)) {
                                    alert("Please enter numeric in " + columnlength[i].FieldDisplayName);
                                    alertTag = "ValStr" + columnlength[i].FieldName;
                                    $("#" + columnlength[i].FieldName).dxSelectBox({
                                        value: ""
                                    });
                                    document.getElementById(alertTag).style.fontSize = "10px";
                                    document.getElementById(alertTag).style.display = "block";
                                    document.getElementById(alertTag).innerHTML = 'Please enter numeric in' + columnlength[i].FieldDisplayName;
                                    return false;
                                }
                                alertTag = "ValStr" + columnlength[i].FieldName;
                                document.getElementById(alertTag).style.display = "none";
                            }
                        }
                    }

                    if (columnlength[i].IsDisplay === true && columnlength[i].IsRequiredFieldValidator === true) {

                        if (columnlength[i].FieldType === "text" || columnlength[i].FieldType === "number") {

                            if (document.getElementById(columnlength[i].FieldName).value === "" || document.getElementById(columnlength[i].FieldName).value === undefined || document.getElementById(columnlength[i].FieldName).value === null) {
                                alert("Please enter.." + columnlength[i].FieldDisplayName);
                                alertTag = "ValStr" + columnlength[i].FieldName;
                                document.getElementById(columnlength[i].FieldName).focus();
                                document.getElementById(alertTag).style.fontSize = "10px";
                                document.getElementById(alertTag).style.display = "block";
                                document.getElementById(alertTag).innerHTML = 'This field should not be empty..' + columnlength[i].FieldDisplayName;
                                return false;
                            }
                            else {
                                alertTag = "ValStr" + columnlength[i].FieldName;
                                document.getElementById(alertTag).style.display = "none";
                            }
                        }
                        if (columnlength[i].FieldType === "textarea") {
                            if (document.getElementById(columnlength[i].FieldName).value === "" || document.getElementById(columnlength[i].FieldName).value === undefined || document.getElementById(columnlength[i].FieldName).value === null) {
                                alert("Please enter.." + columnlength[i].FieldDisplayName);
                                alertTag = "ValStr" + columnlength[i].FieldName;
                                document.getElementById(alertTag).style.fontSize = "10px";
                                document.getElementById(alertTag).style.display = "block";
                                document.getElementById(alertTag).innerHTML = 'This field should not be empty..' + columnlength[i].FieldDisplayName;
                                return false;
                            }
                            else {
                                alertTag = "ValStr" + columnlength[i].FieldName;
                                document.getElementById(alertTag).style.display = "none";
                            }
                        }
                        if (columnlength[i].FieldType === "selectbox") {

                            if ($("#" + columnlength[i].FieldName).dxSelectBox("instance").option('value') === "" || $("#" + columnlength[i].FieldName).dxSelectBox("instance").option('value') === undefined || $("#" + columnlength[i].FieldName).dxSelectBox("instance").option('value') === null) {
                                alert("Please select.." + columnlength[i].FieldDisplayName);
                                alertTag = "ValStr" + columnlength[i].FieldName;
                                document.getElementById(alertTag).style.fontSize = "10px";
                                document.getElementById(alertTag).style.display = "block";
                                document.getElementById(alertTag).innerHTML = 'This field should not be empty..' + columnlength[i].FieldDisplayName;

                                return false;
                            }
                            else {
                                alertTag = "ValStr" + columnlength[i].FieldName;
                                document.getElementById(alertTag).style.display = "none";
                            }
                        }
                    }
                }
                //check curd for multi unit
                if (GblStatus === "Update") {
                    if (masterID === 3 || masterID === "3") {
                        if (GBLCompanyID != columnlength[0].CompanyID) {
                            swal({
                                title: "Access Denied!",
                                text: "You cannot select transactions related to different companies.",
                                button: "OK",
                            });
                            return;
                        }
                    }
                }
                var jsonObjectsLedgerMasterRecord = [];
                var OperationLedgerMasterRecord = {};

                OperationLedgerMasterRecord.LedgerName = document.getElementById("MasterName").innerHTML.trim();
                OperationLedgerMasterRecord.LedgerType = document.getElementById("MasterName").innerHTML.trim();
                OperationLedgerMasterRecord.LedgerGroupID = document.getElementById("MasterID").innerHTML.trim();

                var jsonObjectsLedgerMasterDetailRecord = [];
                var OperationLedgerMasterDetailRecord = {};
                let ledgerMasterField = ["LedgerID", "LedgerCode", "MaxLedgerNo", "LedgerCodePrefix", "LedgerRefCode", "LedgerRefName", "LedgerName", "LedgerDescription", "LedgerUnitID", "LedgerType", "LedgerGroupID", "ISLedgerActive", "Password", "ExLedgerName", "TallyCode", "DepartmentID", "MailingName", "MailingAddress", "Address1", "Address2", "Address3", "City", "District", "State", "Country", "Pincode", "MobileNo", "GSTNo", "PANNo", "GSTApplicable", "Email", "TaxType", "GSTLedgerType", "TaxPercentage", "GSTCalculationOn", "RefClientID", "RefSalesRepresentativeID", "TelephoneNo", "Website", "Designation", "FAX", "InventoryEffect", "IsTaxType", "MaintainBillWise", "CurrencyCode", "DateOfBirth", "LegalName", "TradeName", "SupplyTypeCode", "Remarks", "TaxRatePer", "InAmount", "IsCumulative", "TallyLedgerName", "IsIntegrated", "Target", "ProductionUnitID", "MaxCreditPeriod", "FixedLimit", "MaxCreditLimit", "ProfitMargin"];
                let fieldFound = false;
                var refcode = "";
                for (var j = 0; j < columnlength.length; j++) {
                    OperationLedgerMasterDetailRecord = {};

                    OperationLedgerMasterDetailRecord.FieldName = columnlength[j].FieldName.trim();
                    OperationLedgerMasterDetailRecord.ParentFieldName = columnlength[j].FieldName.trim();

                    let DataFieldName = columnlength[j].FieldName;
                    if (DataFieldName === undefined || DataFieldName === null) DataFieldName = "";
                    if (columnlength[j].FieldType === "text" || columnlength[j].FieldType === "number") {
                        OperationLedgerMasterDetailRecord.ParentFieldValue = (document.getElementById(columnlength[j].FieldName).value.trim()).replace(/['"]/g, '');
                        OperationLedgerMasterDetailRecord.FieldValue = (document.getElementById(columnlength[j].FieldName).value.trim()).replace(/['"]/g, '');
                        OperationLedgerMasterRecord[DataFieldName] = (document.getElementById(columnlength[j].FieldName).value.trim()).replace(/['"]/g, '');
                    }
                    if (columnlength[j].FieldType === "textarea") {
                        OperationLedgerMasterDetailRecord.ParentFieldValue = document.getElementById(columnlength[j].FieldName).value.trim();
                        OperationLedgerMasterDetailRecord.FieldValue = document.getElementById(columnlength[j].FieldName).value.trim();
                        OperationLedgerMasterRecord[DataFieldName] = document.getElementById(columnlength[j].FieldName).value.trim();
                    }
                    if (columnlength[j].FieldType === "datebox") {
                        OperationLedgerMasterDetailRecord.ParentFieldValue = $("#" + columnlength[j].FieldName).dxDateBox("instance").option('value');
                        OperationLedgerMasterDetailRecord.FieldValue = $("#" + columnlength[j].FieldName).dxDateBox("instance").option('value');
                        OperationLedgerMasterRecord[DataFieldName] = $("#" + columnlength[j].FieldName).dxDateBox("instance").option('value');
                    }
                    if (columnlength[j].FieldType === "selectbox") {

                        OperationLedgerMasterDetailRecord.ParentLedgerID = 0;
                        // OperationLedgerMasterDetailRecord.ParentLedgerID =$("#" + columnlength[j].FieldName).dxSelectBox("instance").option('value').trim();
                        var pval = $("#" + columnlength[j].FieldName).dxSelectBox("instance").option('value');
                        if (pval !== "" && pval !== "null" && pval !== null && pval !== undefined) {
                            if (isNaN(pval)) {
                                pval = pval.trim();
                            }
                        }
                        OperationLedgerMasterDetailRecord.ParentFieldValue = pval;//text
                        OperationLedgerMasterDetailRecord.FieldValue = pval;//text
                        OperationLedgerMasterRecord[DataFieldName] = pval;
                    }
                    if (columnlength[j].FieldType === "checkbox") {
                        OperationLedgerMasterDetailRecord.ParentFieldValue = document.getElementById(columnlength[j].FieldName).checked;
                        OperationLedgerMasterDetailRecord.FieldValue = document.getElementById(columnlength[j].FieldName).checked;
                        OperationLedgerMasterRecord[DataFieldName] = document.getElementById(columnlength[j].FieldName).checked;
                    }
                    if (columnlength[j].FieldName.trim() === "LedgerName") {
                        OperationLedgerMasterRecord.LedgerName = OperationLedgerMasterDetailRecord.FieldValue;
                    }
                    if (columnlength[j].FieldName.trim() === "TallyCode") {
                        OperationLedgerMasterRecord.TallyCode = OperationLedgerMasterDetailRecord.FieldValue;
                    }
                    if (columnlength[j].FieldName.trim() === "RefCode") {
                        refcode = document.getElementById(columnlength[j].FieldName).value;
                    };

                    OperationLedgerMasterDetailRecord.SequenceNo = j + 1;
                    OperationLedgerMasterDetailRecord.LedgerGroupID = document.getElementById("MasterID").innerHTML.trim();

                    jsonObjectsLedgerMasterDetailRecord.push(OperationLedgerMasterDetailRecord);
                    fieldFound = false;
                    ledgerMasterField.map(function (e) {
                        if (DataFieldName.trim().toUpperCase() === e.trim().toUpperCase()) {
                            fieldFound = true;
                        }
                    });
                    if (!fieldFound) {
                        if (OperationLedgerMasterRecord[DataFieldName] !== undefined && OperationLedgerMasterRecord[DataFieldName] !== null) {
                            delete OperationLedgerMasterRecord[DataFieldName];
                        }
                    }
                }

                jsonObjectsLedgerMasterRecord.push(OperationLedgerMasterRecord);
                var CostingDataLedgerMaster = JSON.stringify(jsonObjectsLedgerMasterRecord);

                var CostingDataLedgerDetailMaster = JSON.stringify(jsonObjectsLedgerMasterDetailRecord);
                //var refcode = document.getElementById("RefCode").value;
                //                 alert(CostingDataLedgerDetailMaster);
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
                        if (GblStatus === "Update") {
                            //  var person = confirm("Do you want to update it..?");
                            //  if (person === true) {
                            $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);

                            $.ajax({
                                type: "POST",
                                url: "WebService_LedgerMaster.asmx/UpdateData",
                                data: '{CostingDataLedgerMaster:' + CostingDataLedgerMaster + ',CostingDataLedgerDetailMaster:' + CostingDataLedgerDetailMaster + ',MasterName:' + JSON.stringify(MasterName) + ',LedgerID:' + JSON.stringify(document.getElementById("txtGetGridRow").value) + ',UnderGroupID:' + JSON.stringify(UnderGroupID) + ',ActiveLedger:' + JSON.stringify(document.getElementById("Isactivledgerstatic").checked) + ',LedgerRefCode:' + JSON.stringify(refcode) + '}',
                                contentType: "application/json; charset=utf-8",
                                dataType: "json",
                                success: function (results) {
                                    var res = JSON.stringify(results);
                                    res = res.replace(/"d":/g, '');
                                    res = res.replace(/{/g, '');
                                    res = res.replace(/}/g, '');
                                    res = res.substr(1);
                                    res = res.slice(0, -1);
                                    let Title, Text, Type;
                                    let IsSuccess = false;
                                    $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                                    if (res === "Success") {
                                        //swal("Saved!", "Your data saved", "success");
                                        //FillGrid();
                                        //$("#largeModal").modal('hide');
                                        Title = "Updated...";
                                        Text = "Your data Updated...";
                                        Type = "success";
                                        IsSuccess = true;
                                    } else if (res.includes("not authorized")) {
                                        //swal("Access Denied..!", res, "error");
                                        Title = "Access Denied..!";
                                        Text = res;
                                        Type = "error";
                                        IsSuccess = false;
                                    } else if (res.includes("Error:")) {
                                        //swal("Error..!", res, "error");
                                        Title = "Error..!";
                                        Text = res;
                                        Type = "error";
                                        IsSuccess = false;
                                    } else {
                                        swal.close();
                                        setTimeout(() => {
                                            swal("Warning..!", res, "warning");
                                        }, 100);
                                    }
                                    swal({
                                        title: Title,
                                        text: Text,
                                        type: Type
                                    }, function (isConfirm) {
                                        if (IsSuccess) {
                                            FillGrid();
                                            $("#largeModal").modal('hide');
                                        }
                                    });
                                },
                                error: function errorFunc(jqXHR) {
                                    $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                                    swal("Error!", "Please try after some time..", "");
                                    // alert(jqXHR);
                                }
                            });
                            //  }
                        } else {
                            $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
                            // alert(CostingDataLedgerMaster);
                            $.ajax({
                                type: "POST",
                                url: "WebService_LedgerMaster.asmx/SaveData",
                                data: '{CostingDataLedgerMaster:' + CostingDataLedgerMaster + ',CostingDataLedgerDetailMaster:' + CostingDataLedgerDetailMaster + ',MasterName:' + JSON.stringify(MasterName) + ',ActiveLedger:' + JSON.stringify(document.getElementById("Isactivledgerstatic").checked) + ',LedgerGroupID:' + JSON.stringify(document.getElementById("MasterID").innerHTML.trim()) + ',LedgerRefCode:' + JSON.stringify(refcode) + '}',
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
                                    let Title, Text, Type;
                                    let IsSuccess = false;
                                    if (res === "Success") {
                                        //swal("Saved!", "Your data saved", "success");
                                        //location.reload();
                                        isSaveASClicked = false;
                                        Title = "Saved!";
                                        Text = "Your data saved...";
                                        Type = "success";
                                        IsSuccess = true;
                                    } else if (res.includes("not authorized")) {
                                        //swal("Access Denied..!", res, "error");
                                        if (isSaveASClicked) {
                                            GblStatus = "Update";
                                            isSaveASClicked = false;
                                        }
                                        Title = "Access Denied..!";
                                        Text = res;
                                        Type = "error";
                                        IsSuccess = false;
                                    } else if (res.includes("Error:")) {
                                        //swal("Error..!", res, "error");
                                        if (isSaveASClicked) {
                                            GblStatus = "Update";
                                            isSaveASClicked = false;
                                        }
                                        Title = "Error..!";
                                        Text = res;
                                        Type = "error";
                                        IsSuccess = false;
                                    } else if (res === "Duplicate data found") {
                                        //swal("Duplicate!", "Your data already exist..!", "");
                                        if (isSaveASClicked) {
                                            GblStatus = "Update";
                                            isSaveASClicked = false;
                                        }
                                        Title = "Duplicate!";
                                        Text = "Your data already exist..!";
                                        Type = "warning";
                                        IsSuccess = false;
                                    } else {
                                        swal("Not Saved..!", res, "warning");
                                    }
                                    setTimeout(function () {
                                        swal({
                                            title: Title,
                                            text: Text,
                                            type: Type
                                        }, function (isConfirm) {
                                            if (IsSuccess) {
                                                location.reload();
                                            }
                                        });
                                    }, 100);

                                },
                                error: function errorFunc(jqXHR) {
                                    $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                                    swal("Error!", "Please try after some time..", "");
                                    // alert(jqXHR);
                                }
                            });
                        }
                    });
            }
        }
    });
});

//Default Grid Fill
var HideColumn = "", UnderGroupID = "", VisibleTab = "";
function FillGrid() {

    var masterID = document.getElementById("MasterID").innerHTML;
    document.getElementById("txtGetGridRow").value = "";

    var SetColumn = "", DynamicCol = {}, DynamicColPush = [];
    HideColumn = "";
    VisibleTab = "";
    try {
        if (masterID !== "") {
            $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
            document.getElementById("ButtonDiv").style.display = "block";
            document.getElementById("ButtonGridDiv").style.display = "block";
            try {
                $.ajax({
                    type: "POST",
                    url: "WebService_LedgerMaster.asmx/MasterGridColumnHide",
                    data: '{masterID:' + JSON.stringify(masterID) + '}',
                    contentType: "application/json; charset=utf-8",
                    dataType: "text",
                    success: function (results) {
                        var res = results.replace(/\\/g, '');
                        res = res.replace(/"d":""/g, '');
                        res = res.replace(/""/g, '');
                        res = res.replace(/:}/g, ':null}');
                        res = res.replace(/:,/g, ':null,');
                        res = res.substr(1);
                        res = res.slice(0, -1);
                        // HideColumn = JSON.parse(res);
                        var RES1 = JSON.parse(res);
                        HideColumn = RES1[0].GridColumnHide;
                        VisibleTab = RES1[0].TabName;
                        ConcernPerson = RES1[0].ConcernPerson;
                        if (ConcernPerson === true) {
                            document.getElementById("btnConcernPerson").style.display = "block";
                        } else {
                            document.getElementById("btnConcernPerson").style.display = "none";
                        }

                        if (RES1[0].EmployeeMachineAllocation === true) {
                            document.getElementById("btnMachineAllo").style.display = "block";
                        } else {
                            document.getElementById("btnMachineAllo").style.display = "none";
                        }

                        $.ajax({
                            type: "POST",
                            url: "WebService_LedgerMaster.asmx/MasterGridColumn",
                            data: '{masterID:' + JSON.stringify(masterID) + '}',
                            contentType: "application/json; charset=utf-8",
                            dataType: "text",
                            success: function (results) {
                                var res = results.replace(/\\/g, '');
                                res = res.replace(/"d":""/g, '');
                                res = res.replace(/""/g, '');
                                res = res.replace(/:}/g, ':null}');
                                res = res.replace(/:,/g, ':null,');
                                res = res.substr(1);
                                res = res.slice(0, -1);
                                SetColumn = JSON.parse(res);

                                var SSCol = SetColumn[0].GridColumnName;
                                if (SSCol === "" || SSCol === null || SSCol === undefined) {
                                    DynamicColPush = DynamicColPush;
                                }
                                else {
                                    SSCol = SSCol.split(',');
                                    for (var m in SSCol) {
                                        var Colobj = SSCol[m];
                                        DynamicCol = {};
                                        if (Colobj.toString().toUpperCase().indexOf(" AS ") === -1) {
                                            DynamicCol.dataField = Colobj;
                                            DynamicCol.maxWidth = 120;
                                        } else {
                                            var colDataField = Colobj.split(' As ');
                                            var colCaption = colDataField[1];
                                            DynamicCol.dataField = colDataField[0];
                                            DynamicCol.maxWidth = 120;
                                            DynamicCol.caption = colCaption;
                                        }
                                        DynamicColPush.push(DynamicCol);
                                    }
                                }

                                $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
                                $.ajax({
                                    type: "POST",
                                    url: "WebService_LedgerMaster.asmx/MasterGrid",
                                    data: '{masterID:' + JSON.stringify(masterID) + '}',
                                    contentType: "application/json; charset=utf-8",
                                    dataType: "text",
                                    success: function (results) {
                                        var res = results.replace(/\\/g, '');
                                        res = res.replace(/"d":""/g, '');
                                        res = res.replace(/""/g, '');
                                        res = res.replace(/u0026/g, '&');
                                        res = res.replace(/:,/g, ':null,');
                                        res = res.replace(/:}/g, ':null}');
                                        res = res.substr(1);
                                        res = res.slice(0, -1);

                                        var RES1 = [];
                                        if (res === "") { RES1 = []; } else RES1 = JSON.parse(res);

                                        if (RES1 === [] || RES1 === "") {
                                            RES1 = [];
                                        }
                                        // alert(JSON.stringify(DynamicColPush));
                                        $("#MasterGrid").dxDataGrid({
                                            dataSource: RES1,
                                            onContentReady: function (e) {
                                                var HCol = HideColumn;
                                                if (HCol) {
                                                    HCol = HCol.split(',');
                                                    for (var hc in HCol) {
                                                        var placedHC = HCol[hc];
                                                        $('#MasterGrid').dxDataGrid("columnOption", placedHC, "visible", false);
                                                    }
                                                }
                                                $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                                            },
                                            columns: DynamicColPush
                                        });
                                        let masterGrid = $("#MasterGrid").dxDataGrid("instance");
                                        masterGrid.clearSelection();
                                    }
                                });

                            }
                        });

                        //Create Dril Down Of Masters
                        document.getElementById("TabDiv").innerHTML = "";

                        if (VisibleTab !== "" && VisibleTab !== null && VisibleTab !== undefined) {

                            var CreTab = VisibleTab.split(',');
                            var CreTabContener = "";
                            for (var cr in CreTab) {
                                var CreTabName = CreTab[cr];
                                CreTabContener += "<li role='presentation' id=TL" + CreTabName + " ><a id=" + CreTabName + "   href='#' data-toggle='tab' onclick='DrilDown(this);' style='background-color:none;'>" + CreTabName.replace(/_/g, ' ') + "</a></li>";
                            }
                            $('#TabDiv').append('<ul  class="nav nav-tabs tab-col-red" role="tablist" style="color: green;border:none">' + CreTabContener + '</ul>');
                        }

                    }
                });

            } catch (e) {
                $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
            }
            $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
        }
        else {
            document.getElementById("ButtonDiv").style.display = "none";
            document.getElementById("ButtonGridDiv").style.display = "none";
        }
    } catch (e) {
        $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
    }

}

//Edit Selected Data on PopUp
$("#EditButton").click(function () {
    ClearBVData()
    GblStatus = "Update";
    isSaveASClicked = false;
    document.getElementById("BtnDeletePopUp").disabled = false;
    document.getElementById("BtnSaveAS").disabled = "";
    document.getElementById("BtnBVSetting").disabled = false;
    document.getElementById("Isactivledgerstatic_Div").style.display = "block";

    var txtGetGridRow = document.getElementById("txtGetGridRow").value;
    if (txtGetGridRow === "" || txtGetGridRow === null || txtGetGridRow === undefined) {
        swal("", "Please choose any row from below Grid..!");
        return false;
    }
    var DeleteField = GBLField[0].FieldName;

    $.ajax({
        type: "POST",
        url: "WebService_LedgerMaster.asmx/MasterGridLoadedData",
        data: '{masterID:' + JSON.stringify(UnderGroupID) + ',Ledgerid:' + JSON.stringify(txtGetGridRow) + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            var res = results.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/""/g, '');
            res = res.replace(/:,/g, ':null,');
            res = res.replace(/:}/g, ':null}');
            res = res.replace(/u0026/g, '&');
            res = res.substr(2);
            res = res.slice(0, -2);
            try {

                var LoadedData = JSON.parse(res);

                if (LoadedData["ISLedgerActive"] === "True") {
                    document.getElementById("Isactivledgerstatic").checked = true;
                }
                else {
                    document.getElementById("Isactivledgerstatic").checked = false;
                }

                for (var e = 0; e < GBLField.length; e++) {

                    if (GBLField[e].FieldType === "text") {
                        document.getElementById(GBLField[e].FieldName).value = (LoadedData[GBLField[e].FieldName] === undefined || LoadedData[GBLField[e].FieldName] === null) ? "" : LoadedData[GBLField[e].FieldName];
                        if (GBLField[e].FieldName == 'LedgerName') {
                            document.getElementById("txtClientName").value = LoadedData[GBLField[e].FieldName];
                        }
                    }
                    else if (GBLField[e].FieldType === "number") {
                        document.getElementById(GBLField[e].FieldName).value = (LoadedData[GBLField[e].FieldName] === undefined || LoadedData[GBLField[e].FieldName] === null) ? 0 : Number(LoadedData[GBLField[e].FieldName]);
                    }
                    else if (GBLField[e].FieldType === "textarea") {
                        document.getElementById(GBLField[e].FieldName).value = (LoadedData[GBLField[e].FieldName] === undefined || LoadedData[GBLField[e].FieldName] === null) ? "" : LoadedData[GBLField[e].FieldName];
                    }
                    else if (GBLField[e].FieldType === "checkbox") {
                        var chkStatus = "";
                        if (LoadedData[GBLField[e].FieldName] === undefined || LoadedData[GBLField[e].FieldName] === null || LoadedData[GBLField[e].FieldName] === "False" || LoadedData[GBLField[e].FieldName] === false || LoadedData[GBLField[e].FieldName] === 0) {
                            chkStatus = false;
                        }
                        else if (LoadedData[GBLField[e].FieldName] === "True" || LoadedData[GBLField[e].FieldName] === true || LoadedData[GBLField[e].FieldName] === 1) {
                            chkStatus = true;
                        }

                        document.getElementById(GBLField[e].FieldName).checked = chkStatus;
                    }
                    else if (GBLField[e].FieldType === "datebox") {
                        if (LoadedData[GBLField[e].FieldName] === null || LoadedData[GBLField[e].FieldName] === "-") continue;
                        $("#" + GBLField[e].FieldName).dxDateBox({
                            value: LoadedData[GBLField[e].FieldName]
                        });
                    }
                    else if (GBLField[e].FieldType === "selectbox") {
                        var UPSID = "#" + GBLField[e].FieldName;

                        var selValue = "";
                        if (isNaN(LoadedData[GBLField[e].FieldName])) {
                            selValue = LoadedData[GBLField[e].FieldName];
                        }
                        else {
                            selValue = JSON.parse(LoadedData[GBLField[e].FieldName]);
                        }

                        $(UPSID).dxSelectBox({
                            value: selValue
                        });

                    }
                }

                $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
            } catch (e) {
                $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
            }
        }
    });

});

//SaveAs Data On Pop Up
$("#BtnSaveAS").click(function () {
    GblStatus = "";
    isSaveASClicked = true;
    $("#BtnSave").click();
});

//Delete Selected Data on PopUp
$("#DeleteButton").click(function () {
    var MasterName = document.getElementById("MasterName").innerHTML;
    var txtGetGridRow = document.getElementById("txtGetGridRow").value;
    isSaveASClicked = false;
    if (txtGetGridRow === "" || txtGetGridRow === null || txtGetGridRow === undefined) {
        alert("Please Choose any row from below Grid..!");
        return false;
    }

    $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
    $.ajax({
        type: "POST",
        url: "WebService_LedgerMaster.asmx/CheckPermission",
        data: '{LedgerID:' + JSON.stringify(txtGetGridRow) + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (results) {
            var res = JSON.stringify(results);
            res = res.replace(/"d":/g, '');
            res = res.replace(/{/g, '');
            res = res.replace(/}/g, '');
            res = res.replace(/:}/g, ':null}');
            res = res.replace(/:,/g, ':null,');
            res = res.substr(1);
            res = res.slice(0, -1);

            $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
            if (res === "Exist") {
                swal("", "This ledger is used in another process..! Record can not be delete.", "error");
                return false;
            }
            else {
                swal({
                    title: "Are you sure?",
                    text: "You will not be able to recover this ledger!",
                    type: "warning",
                    showCancelButton: true,
                    confirmButtonColor: "#DD6B55",
                    confirmButtonText: "Yes, delete it!",
                    closeOnConfirm: true
                }, function () {
                    $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
                    $.ajax({
                        type: "POST",
                        url: "WebService_LedgerMaster.asmx/DeleteData",
                        data: '{txtGetGridRow:' + JSON.stringify(txtGetGridRow) + ',MasterName:' + JSON.stringify(MasterName) + ',UnderGroupID:' + JSON.stringify(UnderGroupID) + '}',
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
                            if (res === "Success") {
                                swal("Deleted!", "Your Content has been deleted.", "success");
                                location.reload();
                            } else {
                                swal.close();
                                setTimeout(() => {
                                    swal("Warning..!", res, "warning");
                                }, 100);
                            }

                        },
                        error: function errorFunc(jqXHR) {
                            $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                            alert(jqXHR);
                        }
                    });
                });
            }
        }, error: function (e) {
            $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
            console.log(e);
        }
    });
});

$("#BtnNew").click(function () {
    refreshbtn();
    document.getElementById("BtnDeletePopUp").disabled = true;
    document.getElementById('BtnBVSetting').style.display = 'none';  // Or 'block' depending on layout
});

$("#BtnDeletePopUp").click(function () {
    $("#DeleteButton").click();
});

//For DrillDown Tab
function DrilDown(dr) {
    var masterID = document.getElementById("MasterID").innerHTML;
    $("#DrilDownGrid").dxDataGrid({ dataSource: [] });
    $.ajax({
        type: "POST",
        url: "WebService_LedgerMaster.asmx/DrillDownMasterGrid",
        data: '{masterID:' + JSON.stringify(masterID) + ',TabID:' + JSON.stringify(dr.id) + ',LedgerID:' + Number(document.getElementById("txtGetGridRow").value) + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            var res = results.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/""/g, '');
            res = res.replace(/u0026/g, '&');
            res = res.replace(/u0027/g, "'");
            res = res.replace(/,}/g, ',null}');
            res = res.replace(/:}/g, ':null}');
            res = res.replace(/:,/g, ':null,');
            res = res.substr(1);
            res = res.slice(0, -1);
            var RES1 = JSON.parse(res);

            $("#DrilDownGrid").dxDataGrid({
                dataSource: RES1,
                onCellPrepared: function (columns) {
                    if (columns.column.dataField.substr(columns.column.dataField.length - 2, 2).toUpperCase() === "ID") {
                        columns.column.visible = false;
                        columns.column.width = 0;
                    }
                    columns.column.width = 100;
                },
                export: {
                    enabled: true,
                    fileName: document.getElementById("MasterDisplayName").innerHTML + '-' + document.getElementById(dr.id).innerHTML,
                    allowExportSelectedData: true
                }
            });
        }
    });

}

$("#btnTabModel").click(function () {

    var txtGetGridRow = document.getElementById("txtGetGridRow").value;
    isSaveASClicked = false;
    if (txtGetGridRow === "" || txtGetGridRow === null || txtGetGridRow === undefined) {
        alert("Please Choose any row from below Grid..!");
        return false;
    }

    $("#TabDiv>ul>li.active").removeClass("active");

    $("#TabDiv>ul>li.active").removeClass("active");

    $("#DrilDownGrid").dxDataGrid({
        dataSource: [],
        columnAutoWidth: true,
        showBorders: true,
        showRowLines: true,
        allowColumnReordering: true,
        allowColumnResizing: true,
        columnResizingMode: 'widget',
        sorting: { mode: "multiple" },
        selection: { mode: "single" },
        height: function () {
            return window.innerHeight / 1.5;
        },
        pager: {
            showPageSizeSelector: true,
            allowedPageSizes: [50, 100, 200],
        },
        filterRow: { visible: true, applyFilter: "auto" },
        columnChooser: { enabled: true },
        headerFilter: { visible: true },
        rowAlternationEnabled: true,
        loadPanel: {
            enabled: true,
            text: 'Data is loading...'
        },
        onRowPrepared: function (e) {
            if (e.rowType === "header") {
                e.rowElement.css('background', '#42909A');
                e.rowElement.css('color', 'white');
            }
            e.rowElement.css('fontSize', '11px');
        }
    });

    document.getElementById("btnTabModel").setAttribute("data-toggle", "modal");
    document.getElementById("btnTabModel").setAttribute("data-target", "#TabModal");
});

//Refresh Function
function refreshbtn() {
    GblStatus = "";
    isSaveASClicked = false;
    // document.getElementById("txtGetGridRow").value = "";
    if (GBLField.length > 0) {
        for (var i = 0; i < GBLField.length; i++) {
            if (GBLField[i].FieldType === "text" || GBLField[i].FieldType === "number") {
                document.getElementById(GBLField[i].FieldName).value = "";
            }
            if (GBLField[i].FieldType === "textarea") {
                document.getElementById(GBLField[i].FieldName).value = "";
            }
            if (GBLField[i].FieldType === "checkbox") {
                document.getElementById(GBLField[i].FieldName).checked = false;
            }
            if (GBLField[i].FieldType === "datebox") {
                $("#" + GBLField[i].FieldName).dxDateBox({
                    pickerType: "calendar",
                    formate: 'date',
                    value: new Date().toISOString().substr(0, 10),
                    formatString: 'dd-MMM-yyyy'
                });
            }
            if (GBLField[i].FieldType === "selectbox") {
                $("#" + GBLField[i].FieldName).dxSelectBox({
                    value: ''
                });
            }
        }
    }
}

//For Formulas
var holdvalue = "", getFormula = "";
function FarmulaChange(FC) {
    var getValid = "ValStr" + FC.id;

    var x = document.getElementById(FC.id).value;
    //if (isNaN(x)) {
    //document.getElementById(getValid).style.display = "block";
    //document.getElementById(getValid).innerHTML = 'This field must have alphanumeric characters only';
    //document.getElementById(FC.id).value = "";
    //document.getElementById(FC.id).focus();
    //return false;

    //}
    //else {
    document.getElementById(getValid).style.display = "none";
    var geval = "ValCh" + FC.id;
    var getSepValu = document.getElementById(geval).value;
    getSepValu = getSepValu.replace('/', '');
    getSepValu = getSepValu.replace(' ', '');
    var CreateVar = [];
    CreateVar = getSepValu;

    var StrgLength = 1;
    var NextString = CreateVar.toLowerCase().includes("and");

    var mergeString = "";
    if (NextString === true) {
        mergeString = CreateVar.split('and');
        StrgLength = mergeString.length;
    }
    else {
        mergeString = CreateVar;
        StrgLength = 1;
    }

    for (var z = 0; z < StrgLength; z++) {
        if (NextString === true) {
            CreateVar = mergeString[z].split(',');
        } else {
            CreateVar = mergeString.split(',');
        }

        holdvalue = "";
        var MakeObj = "", MakeObjValue = "";
        var StrVar = "", fillValue = "";
        for (var t = 0; t < CreateVar.length; t++) {
            if (t === 0) {
                holdvalue = CreateVar[t];
            } else {
                if (MakeObj === "" || MakeObj === undefined || MakeObj === null) {
                    fillValue = document.getElementById(CreateVar[t]).value;
                    StrVar = "";
                    if (isNaN(fillValue)) {
                        StrVar = '"' + fillValue + '"';
                    } else {
                        StrVar = Number(fillValue);
                    }
                    MakeObj += CreateVar[t] + '=' + StrVar;
                } else {
                    fillValue = document.getElementById(CreateVar[t]).value;
                    StrVar = "";
                    if (isNaN(fillValue)) {
                        StrVar = '"' + fillValue + '"';
                    } else {
                        StrVar = Number(fillValue);
                    }
                    MakeObj += ',' + CreateVar[t] + '=' + StrVar;
                }

            }
        }
        getFormula = "";
        var addstr = "Formula" + FC.id;
        getFormula = document.getElementById(addstr).innerHTML;

        var NextFarmula = getFormula.toLowerCase().includes("and");
        if (NextFarmula === true) {
            getFormula = getFormula.split('and');
            getFormula = getFormula[z];
        }
        ApplyOperation(MakeObj);
    }

}
//}

function FarmulaChangeSELECTBX(currentID) {

    var geval = "ValCh" + currentID;
    var getSepValu = document.getElementById(geval).value;
    var selValue = "";
    selValue = $("#" + currentID).dxSelectBox("instance").option('text');
    /*if (selValue !== "" && selValue !== null && selValue !== "null" && selValue !== undefined) {*/

    if (getSepValu !== null && getSepValu !== undefined && getSepValu !== "null" && getSepValu !== "") {

        getSepValu = getSepValu.replace('/', '');
        getSepValu = getSepValu.replace(/ /g, '');

        var CreateVar = [];
        CreateVar = getSepValu;

        var StrgLength = 1;
        var NextString = CreateVar.toLowerCase().includes("and");

        var mergeString = "";
        if (NextString === true) {
            mergeString = CreateVar.split('and');
            StrgLength = mergeString.length;
        }
        else {
            mergeString = CreateVar;
            StrgLength = 1;
        }
        for (var no = 0; no < StrgLength; no++) {
            if (NextString === true) {
                CreateVar = mergeString[no].split(',');
            } else {
                CreateVar = mergeString.split(',');
            }

            holdvalue = "";
            var MakeObj = "", MakeObjValue = "";

            for (var t = 0; t < CreateVar.length; t++) {
                if (t === 0) {
                    holdvalue = CreateVar[t].replace(/ /g, '');
                } else {

                    var fillValue = $("#" + CreateVar[t].replace(/ /g, '')).dxSelectBox("instance").option('text');
                    var StrVar = "";
                    if (MakeObj === "" || MakeObj === undefined || MakeObj === null) {
                        if (fillValue === null || fillValue === "" || fillValue === undefined || fillValue === "null") {
                            //fillValue = "null";
                            fillValue = 0;
                        }

                        if (isNaN(fillValue)) {
                            StrVar = '"' + fillValue + '"';
                        } else {
                            StrVar = Number(fillValue);
                        }

                        MakeObj += CreateVar[t] + '=' + StrVar;
                    } else {
                        if (fillValue === null || fillValue === "" || fillValue === undefined || fillValue === "null") {
                            //fillValue = "null";
                            fillValue = 0;
                        }
                        if (isNaN(fillValue)) {//*
                            StrVar = '"' + fillValue + '"';//*
                        } else {
                            StrVar = Number(fillValue);
                        }
                        MakeObj += ',' + CreateVar[t] + '=' + StrVar;
                    }

                }
            }

            getFormula = "";
            var addstr = "Formula" + currentID;
            getFormula = document.getElementById(addstr).innerHTML;

            var NextFarmula = getFormula.toLowerCase().includes("and");
            if (NextFarmula === true) {
                getFormula = getFormula.split('and');
                getFormula = getFormula[no];
            }
            ApplyOperation(MakeObj);
        }
    }
    /*}*/
}

function ApplyOperation(MakeObj) {

    var getMakeObj = MakeObj.split(',');
    var doc = "";
    for (var e = 0; e < getMakeObj.length; e++) {
        if (getMakeObj[e] !== "" && getMakeObj[e] !== null) {
            if (doc === "" || doc === undefined || doc === null) {
                doc += "var " + getMakeObj[e];
            } else {
                doc += "; var " + getMakeObj[e];
            }
        }
    }
    //var getFormula = "PaperSize=SizeW CONC SizeL";

    getFormula = getFormula.split('=');
    getFormula = getFormula[1];


    var getFormulaConc = getFormula.replace(/conc/g, ',');
    getFormulaConc = getFormulaConc.replace(/CONC/g, ',');
    getFormulaConc = getFormulaConc.split(',');

    if (getFormulaConc.length > 1) {
        var concanate = getFormulaConc;
        var Concstrng = "";

        Concat(concanate);
    } else {
        getFormula = getFormula.replace(/u0027/g, "'");
        var CalResult = "return " + getFormula;

        if (doc === "") {
            doc = "function GetResLedger(){" + CalResult + ";}";
        } else {
            doc = doc + "; function GetResLedger(){" + CalResult + ";}";
        }

        eval(doc);

        var ResultsValue = GetResLedger();
        SetLedgerFieldValue(holdvalue, ResultsValue);
    }

    function Concat(concanate) {

        var MakeString = concanate;

        for (var g in MakeString) {
            var CalResult = "return " + MakeString[g];
            doc = doc + ";function cv(){" + CalResult + ";}";

            eval(doc);

            if (Concstrng === "") {
                Concstrng = cv();
            }
            else {
                Concstrng = Concstrng + " X " + cv();
            }
        }

        document.getElementById(holdvalue).value = Concstrng;
        document.getElementById(holdvalue).readOnly = true;
    }

}

function SetLedgerFieldValue(name, value) {
    try {
        if (GBLField.length > 0) {
            for (var i = 0; i < GBLField.length; i++) {
                if (GBLField[i].FieldName === name) {
                    var IsLocked = true;
                    if (GBLField[i].IsActive === true || GBLField[i].IsActive === "true") { IsLocked = false; } else { IsLocked = true; }
                    if (GBLField[i].FieldType === "text" || GBLField[i].FieldType === "number") {
                        document.getElementById(GBLField[i].FieldName).value = value;
                        document.getElementById(GBLField[i].FieldName).readOnly = IsLocked;
                    }
                    if (GBLField[i].FieldType === "textarea") {
                        document.getElementById(GBLField[i].FieldName).value = value;
                    }
                    if (GBLField[i].FieldType === "checkbox") {
                        document.getElementById(GBLField[i].FieldName).checked = value;
                    }
                    if (GBLField[i].FieldType === "datebox") {
                        $("#" + GBLField[i].FieldName).dxDateBox({
                            pickerType: "calendar",
                            formate: 'date',
                            value: value,
                            formatString: 'dd-MMM-yyyy'
                        });
                    }
                    if (GBLField[i].FieldType === "selectbox") {
                        $("#" + GBLField[i].FieldName).dxSelectBox({
                            value: value,
                            disabled: IsLocked
                        });
                    }
                }
            }
        }
    } catch (ex) {
        console.log(ex);
    }
}


//////////////////////////////////////////////////////////////////Get Concern Person////////////////////////////////////////
var existCocernPerson = [], ObjConcernPerson = [];
var newArray = [];
function ExistCosernPerson() {
    $.ajax({
        type: "POST",
        url: "WebService_LedgerMaster.asmx/GetExistCocrnPerson",
        data: '{}',
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            ////console.debug(results);
            var res = results.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/""/g, '');
            res = res.replace(/:}/g, ':null}');
            res = res.replace(/:,/g, ':null,');
            res = res.replace(/u0026/g, '&');
            res = res.substr(1);
            res = res.slice(0, -1);
            var RESS = JSON.parse(res);
            ObjConcernPerson = { 'AllConPer': RESS };

        }
    });
}


//var ObjDesignation = "";
//function ExistDesignation() {
//    $.ajax({
//        type: "POST",
//        url: "WebService_LedgerMaster.asmx/GetConcernPersonNameDesignation",
//        data: '{}',
//        contentType: "application/json; charset=utf-8",
//        dataType: "text",
//        success: function (results) {
//            ////console.debug(results);
//            var res = results.replace(/\\/g, '');
//            res = res.replace(/"d":""/g, '');
//            res = res.replace(/""/g, '');
//            res = res.substr(1);
//            res = res.slice(0, -1);
//            ObjDesignation = JSON.parse(res);

//        }
//    });
//}

$("#btnConvertToConsignee").click(function () {
    var LID = Number(document.getElementById("txtGetGridRow").value);
    if (LID === null || LID <= 0 || LID === undefined) {
        //DevExpress.ui.notify("Please select client first..!", "warning", 2000);
        showDevExpressNotification("Please select client first..!", "warning");
        return false;
    }

    $.ajax({
        type: "POST",
        url: "WebService_LedgerMaster.asmx/ConvertLedgerToConsignee",
        data: '{LedID:' + LID + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            var res = JSON.parse(results);
            if (res.d === "Success") {
                //DevExpress.ui.notify("Consignee created successfully", "success", 2000);
                showDevExpressNotification("Consignee created successfully!", "success");
            } else
                //DevExpress.ui.notify(res.d, "error", 2500);
                showDevExpressNotification(res.d, "error");
        }
    });
});

$("#btnConcernPerson").click(function () {
    AuthenticateCurdActions(GBLProductionUnitID).then(async (isAuthorized) => {
        if (!isAuthorized) {
            return;
        }

        $("#GridPerson").dxDataGrid({
            dataSource: []
        });
        // ExistDesignation();

        ExistCosernPerson();

        $.ajax({
            type: "POST",
            url: "WebService_LedgerMaster.asmx/GetLedgerName",
            data: '{TabelID:' + JSON.stringify(document.getElementById("MasterID").innerHTML) + '}',
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            success: function (results) {
                var res = results.replace(/\\/g, '');
                res = res.replace(/"d":""/g, '');
                res = res.replace(/""/g, '');
                res = res.replace(/:}/g, ':null}');
                res = res.replace(/:,/g, ':null,');
                res = res.replace(/u0026/g, '&');
                res = res.substr(1);
                res = res.slice(0, -1);
                var DM = JSON.parse(res);
                var LID = Number(document.getElementById("txtGetGridRow").value);
                $("#selClientName").dxSelectBox({
                    items: DM,
                    value: LID,
                    placeholder: "Select..",
                    displayExpr: 'LedgerName',
                    valueExpr: 'LedgerID',
                    searchEnabled: true,
                    showClearButton: true,
                    onValueChanged: function (data) {
                        //alert(data.value)
                        if (data.value === null || data.value === "" || data.value === undefined) {
                            existCocernPerson = [];
                            document.getElementById("GridConcernPerson").setAttribute("class", "");
                            document.getElementById("ConcernPersonBtnDeletePopUp").disabled = true;
                        } else {
                            existCocernPerson = ObjConcernPerson.AllConPer.filter(function (el) {
                                return el.LedgerID === data.value;
                            });
                            document.getElementById("ConcernPersonBtnDeletePopUp").disabled = false;
                            document.getElementById("GridConcernPerson").setAttribute("class", "active");
                        }
                        GridConcernPerson();
                    }
                });
            }
        });

        document.getElementById("ConcernPersonBtnDeletePopUp").disabled = true;

        document.getElementById("mySidenav").style.width = "0";
        document.getElementById('MYbackgroundOverlay').style.display = 'none';

        $('#ConcernPersonModal').modal({
            show: 'true'
        });
    });
});

function GridConcernPerson() {

    $("#GridPerson").dxDataGrid({
        dataSource: existCocernPerson,
        showBorders: true,
        paging: {
            enabled: false
        },
        height: function () {
            return window.innerHeight / 1.5;
        },
        showRowLines: true,
        editing: {
            mode: "cell",
            allowDeleting: true,
            allowAdding: true,
            allowUpdating: true
        },
        onRowPrepared: function (e) {
            if (e.rowType === "header") {
                e.rowElement.css('background', '#42909A');
                e.rowElement.css('color', 'white');
            }
            e.rowElement.css('fontSize', '11px');
        },
        export: {
            enabled: true,
            fileName: $("#selClientName").dxSelectBox("instance").option('text'),
            allowExportSelectedData: true
        },
        onRowUpdating: function (e) {
            var grid = $('#GridPerson').dxDataGrid('instance');
            for (var k = 0; k < grid.totalCount(); k++) {
                if (grid._options.dataSource[k].IsPrimaryConcernPerson === true) {
                    grid._options.dataSource[k].IsPrimaryConcernPerson = false;
                }
            }
        },
        columns: [{ dataField: "ConcernPersonID", visible: false, caption: "ConcernPersonID" },
        { dataField: "Name", visible: true, caption: "Name", validationRules: [{ type: "required" }] },
        { dataField: "Designation", visible: true, caption: "Designation" },
        {
            dataField: "Mobile", visible: true, caption: "Mobile No",
            validationRules: [{ type: "required" }, {
                type: "numeric",
                message: 'You must enter only numeric value..!'
            }]
        },
        { dataField: "Email", visible: true, caption: "Email", validationRules: [{ type: "required" }, { type: "email" }] },
        { dataField: "IsPrimaryConcernPerson", visible: true, caption: "Is Primary ConcernPerson", dataType: "boolean" }],
        onRowRemoving: function (select) {
            //alert(select.key.ItemGroupFieldID);
            $.ajax({
                type: "POST",
                url: "WebService_LedgerMaster.asmx/DeleteConcernPersonDataGridRow",
                data: '{ConcernPersonID:' + JSON.stringify(select.key.ConcernPersonID) + ',LedgerID:' + JSON.stringify($("#selClientName").dxSelectBox("instance").option('value')) + '}',
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (results) {
                    var res = JSON.stringify(results);
                    res = res.replace(/"d":/g, '');
                    res = res.replace(/{/g, '');
                    res = res.replace(/}/g, '');
                    res = res.substr(1);
                    res = res.slice(0, -1);
                    //if (res === "Success") {
                    //    alert("Your Data has been Deleted Successfully...!");
                    //}
                }
            });
        }
    });
}

$("#ConcernPersonBtnSave").click(function () {

    var GridPerson = $('#GridPerson').dxDataGrid('instance');
    var SlabGridRow = GridPerson.totalCount();
    var alertTag = "ValStrClientName";
    if ($("#selClientName").dxSelectBox("instance").option('value') === "" || $("#selClientName").dxSelectBox("instance").option('value') === undefined || $("#selClientName").dxSelectBox("instance").option('value') === null) {
        alert("Please select Name..");
        document.getElementById(alertTag).style.fontSize = "10px";
        document.getElementById(alertTag).style.display = "block";
        document.getElementById(alertTag).innerHTML = 'This field should not be empty.. Name';

        return false;
    }
    else {
        document.getElementById(alertTag).style.display = "none";
    }

    if (SlabGridRow > 0) {
        for (var i = 0; i < SlabGridRow; i++) {
            if (GridPerson.cellValue(i, 1) === "" || GridPerson.cellValue(i, 1) === undefined || GridPerson.cellValue(i, 1) === null) {
                alert("Grid row should not be empty..Please enter data in Grid..!");
                return false;
            }
        }
    }
    if (SlabGridRow === 0) {
        alert("Grid row should not be empty..Please enter data in Grid..!");
        return false;
    }

    var jsonObjectsSlabDetailRecord = [];
    var OperationSlabDetailRecord = {};

    var jsonObjectsSlabDetailRecordUpadate = [];
    var OperationSlabDetailRecordUpadate = {};

    var CostingDataSlab = [], CostingDataSlabUpdate = [];

    if (SlabGridRow > 0) {
        for (var k = 0; k < SlabGridRow; k++) {
            OperationSlabDetailRecord = {};
            OperationSlabDetailRecordUpadate = {};
            //alert(GridPerson._options.dataSource[k].ContactPersonID); //e.row.data.ItemID
            if (GridPerson._options.dataSource[k].ConcernPersonID === "" || GridPerson._options.dataSource[k].ConcernPersonID === null || GridPerson._options.dataSource[k].ConcernPersonID === undefined) {

                OperationSlabDetailRecord.Name = GridPerson._options.dataSource[k].Name;
                OperationSlabDetailRecord.Mobile = GridPerson._options.dataSource[k].Mobile;
                OperationSlabDetailRecord.Email = GridPerson._options.dataSource[k].Email;
                OperationSlabDetailRecord.Designation = GridPerson._options.dataSource[k].Designation;
                OperationSlabDetailRecord.IsPrimaryConcernPerson = GridPerson._options.dataSource[k].IsPrimaryConcernPerson;

                jsonObjectsSlabDetailRecord.push(OperationSlabDetailRecord);
            }
            else {
                OperationSlabDetailRecordUpadate.ConcernPersonID = GridPerson._options.dataSource[k].ConcernPersonID;
                OperationSlabDetailRecordUpadate.Name = GridPerson._options.dataSource[k].Name;
                OperationSlabDetailRecordUpadate.Mobile = GridPerson._options.dataSource[k].Mobile;
                OperationSlabDetailRecordUpadate.Email = GridPerson._options.dataSource[k].Email;
                OperationSlabDetailRecordUpadate.Designation = GridPerson._options.dataSource[k].Designation;
                OperationSlabDetailRecordUpadate.IsPrimaryConcernPerson = GridPerson._options.dataSource[k].IsPrimaryConcernPerson;

                jsonObjectsSlabDetailRecordUpadate.push(OperationSlabDetailRecordUpadate);
            }

        }

    }
    CostingDataSlab = JSON.stringify(jsonObjectsSlabDetailRecord);
    CostingDataSlabUpdate = JSON.stringify(jsonObjectsSlabDetailRecordUpadate);

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
            $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);

            $.ajax({
                type: "POST",
                url: "WebService_LedgerMaster.asmx/SaveConcernPersonData",
                data: '{CostingDataSlab:' + CostingDataSlab + ',CostingDataSlabUpdate:' + CostingDataSlabUpdate + ',LedgerID:' + JSON.stringify($("#selClientName").dxSelectBox("instance").option('value')) + '}',
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
                    if (res === "Success") {
                        swal("Saved!", "Your data saved", "success");
                        // alert("Your Data has been Saved Successfully...!");
                        location.reload();
                    }

                },
                error: function errorFunc(jqXHR) {
                    $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                    swal("Error!", "Please try after some time..", "");
                    alert(jqXHR);
                }
            });
        });

});

$("#ConcernPersonBtnDeletePopUp").click(function () {

    var selClientName = $("#selClientName").dxSelectBox("instance").option('value');
    var alertTag = "ValStrClientName";
    if (selClientName === "" || selClientName === undefined || selClientName === null) {
        alert("Please select Client Name..");
        document.getElementById(alertTag).style.fontSize = "10px";
        document.getElementById(alertTag).style.display = "block";
        document.getElementById(alertTag).innerHTML = 'This field should not be empty..Client Name';
        return false;
    }
    else {
        document.getElementById(alertTag).style.display = "none";
    }

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
            $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
            $.ajax({
                type: "POST",
                url: "WebService_LedgerMaster.asmx/DeleteConcernPersonData",
                data: '{LedgerID:' + JSON.stringify(selClientName) + '}',
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
                    if (res === "Success") {
                        swal("Deleted!", "Your data Deleted", "success");
                        // alert("Your Data has been Saved Successfully...!");
                        location.reload();
                    }

                },
                error: function errorFunc(jqXHR) {
                    $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                    alert(jqXHR);
                }
            });

        });
});

$("#ConcernPersonBtnNew").click(function () {
    $("#selClientName").dxSelectBox({
        value: ''
    });

    existCocernPerson = [];
    $("#GridPerson").dxDataGrid({
        dataSource: existCocernPerson
    });

});

//////////////////////////////////////////////////////////////////Get Operator Machine Allocation////////////////////////////////////////

var MachiGrid = "", Objid = [];

$("#btnMachineAllo").click(function () {
    document.getElementById("MachineAllocationBtnDeletePopUp").disabled = true;

    $.ajax({
        type: "POST",
        url: "WebService_LedgerMaster.asmx/GetOperatorName",
        data: '{}',
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            var res = results.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/""/g, '');
            res = res.replace(/:}/g, ':null}');
            res = res.replace(/:,/g, ':null,');
            res = res.replace(/u0026/g, '&');
            res = res.substr(1);
            res = res.slice(0, -1);
            var DM = JSON.parse(res);

            $("#selEmployetName").dxSelectBox({
                items: DM,
                placeholder: "Select..",
                displayExpr: 'LedgerName',
                valueExpr: 'LedgerID',
                searchEnabled: true,
                showClearButton: true,
                onValueChanged: function (data) {
                    Objid = [];
                    $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
                    $.ajax({
                        type: "POST",
                        url: "WebService_LedgerMaster.asmx/ExistMachineID",
                        data: '{EmployeeID:' + JSON.stringify(data.value) + '}',
                        contentType: "application/json; charset=utf-8",
                        dataType: "text",
                        success: function (results) {
                            ////console.debug(results);
                            var res = results.replace(/\\/g, '');
                            res = res.replace(/"d":""/g, '');
                            res = res.replace(/""/g, '');
                            res = res.replace(/:}/g, ':null}');
                            res = res.replace(/:,/g, ':null,');
                            res = res.replace(/u0027/g, "'");
                            res = res.replace(/"/g, '');
                            res = res.replace(/MachineIDString:/g, '');
                            res = res.substr(3);
                            res = res.slice(0, -3);

                            $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                            var IDString = res;
                            if (IDString === "" || IDString === null || IDString === undefined) {
                                document.getElementById("MachineAllocationBtnDeletePopUp").disabled = true;
                                Objid = [];
                                GblMachine();
                            }
                            else {
                                document.getElementById("MachineAllocationBtnDeletePopUp").disabled = false;
                                var selectMIDSplit = IDString.split(',');
                                for (var s in selectMIDSplit) {
                                    Objid.push(selectMIDSplit[s]);
                                }

                                GblMachine();
                            }

                        }
                    });

                }
            });
        }
    });

    document.getElementById("btnMachineAllo").setAttribute("data-toggle", "modal");
    document.getElementById("btnMachineAllo").setAttribute("data-target", "#MachineAllocationModal");
});

function GblMachine() {
    $.ajax({
        type: "POST",
        url: "WebServiceProcessMaster.asmx/MachiGrid",
        data: JSON.stringify({ MachineUnitWise: "Unit Wise" }),//UnderGroupID:' + JSON.stringify(UnderGroupID) + '
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            ////console.debug(results);
            var res = results.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/""/g, '');
            res = res.replace(/:}/g, ':null}');
            res = res.replace(/:,/g, ':null,');
            res = res.replace(/u0027/g, "'");
            res = res.replace(/u0026/g, '&');
            res = res.substr(1);
            res = res.slice(0, -1);
            $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
            MachiGrid = JSON.parse(res);
            MachineGrid();
        }
    });
}

function MachineGrid() {
    //alert(Objid);
    $("#GridMachineAllocation").dxDataGrid({
        // dataSource: MachiGrid,
        dataSource: {
            store: {
                type: "array",
                key: "MachineID",
                data: MachiGrid
            }
        },
        sorting: {
            mode: "multiple"
        },
        paging: false,
        showBorders: true,
        showRowLines: true,
        selection: { mode: "multiple" },
        filterRow: { visible: true, applyFilter: "auto" },
        //height: 600,
        pager: {
            showPageSizeSelector: true,
            allowedPageSizes: [15, 25, 50, 100]
        },
        // scrolling: { mode: 'virtual' },
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
        columns: [
            { dataField: "MachineID", visible: false, caption: "MachineID", width: 300 },
            { dataField: "MachineName", visible: true, caption: "Machine Name" },
            { dataField: "DepartmentID", visible: false, caption: "DepartmentID" },
            { dataField: "DepartmentName", visible: true, caption: "Department Name" }],
        selectedRowKeys: Objid,
        onSelectionChanged: function (selectedItems) {

            var data = selectedItems.selectedRowsData;
            if (data.length > 0) {
                $("#MachineId").text(
                    $.map(data, function (value) {
                        return value.MachineID;    //alert(value.ProcessId);
                    }).join(','));
            }
            else {
                $("#MachineId").text("");
            }
        }
    });
}

$("#MachineAllocationBtnSave").click(function () {
    var GridRow = "";

    var alertTag = "ValStrEmployetName";
    if ($("#selEmployetName").dxSelectBox("instance").option('value') === "" || $("#selEmployetName").dxSelectBox("instance").option('value') === undefined || $("#selEmployetName").dxSelectBox("instance").option('value') === null) {
        alert("Please select Name..");
        document.getElementById(alertTag).style.fontSize = "10px";
        document.getElementById(alertTag).style.display = "block";
        document.getElementById(alertTag).innerHTML = 'This field should not be empty..Name';

        return false;
    }
    else {
        alertTag = "ValStrClientName";
        document.getElementById(alertTag).style.display = "none";
    }

    var jsonObjectsMachineAllocationDetailRecord = [];
    var OperationMachineAllocationDetailRecord = {};

    var CostingDataMachinAllocation = [];

    var txtMID = $("#MachineId").text();

    if (txtMID === "null") {
        GridRow = JSON.stringify(Objid);
        GridRow = GridRow.replace(/"/g, '');
        GridRow = GridRow.substr(1);
        GridRow = GridRow.slice(0, -1);
    }
    else if (txtMID === "" || txtMID === null || txtMID === undefined) {
        GridRow = "";
    }
    else {
        GridRow = txtMID;
    }

    var finalString = GridRow;
    if (GridRow !== "") {
        GridRow = GridRow.split(',');
        if (GridRow.length > 0) {
            for (var m = 0; m < GridRow.length; m++) {
                OperationMachineAllocationDetailRecord = {};
                OperationMachineAllocationDetailRecord.LedgerID = $("#selEmployetName").dxSelectBox("instance").option('value');
                OperationMachineAllocationDetailRecord.MachineID = GridRow[m];

                jsonObjectsMachineAllocationDetailRecord.push(OperationMachineAllocationDetailRecord);
            }

            CostingDataMachinAllocation = JSON.stringify(jsonObjectsMachineAllocationDetailRecord);
        }
    }
    else {
        CostingDataMachinAllocation = JSON.stringify(CostingDataMachinAllocation);
    }
    //alert(CostingDataMachinAllocation);

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

            $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);

            $.ajax({
                type: "POST",
                url: "WebService_LedgerMaster.asmx/SaveEmpMachineAllocation",
                data: '{CostingDataMachinAllocation:' + CostingDataMachinAllocation + ',EmployeID:' + JSON.stringify($("#selEmployetName").dxSelectBox("instance").option('value')) + ',GridRow:' + JSON.stringify(finalString) + '}',//
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
                    if (res === "Success") {
                        swal("Saved!", "Your data saved", "success");
                        // alert("Your Data has been Saved Successfully...!");
                        //location.reload();
                    }
                },
                error: function errorFunc(jqXHR) {
                    $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                    swal("Error!", "Please try after some time..", "");
                    alert(jqXHR);
                }
            });
        });
});

$("#MachineAllocationBtnNew").click(function () {
    $("#selEmployetName").dxSelectBox({
        value: ''
    });
    Objid = [];
    GblMachine();
});

$("#MachineAllocationBtnDeletePopUp").click(function () {

    var selClientName = $("#selEmployetName").dxSelectBox("instance").option('value');
    var alertTag = "ValStrEmployetName";

    if (selClientName === "" || selClientName === undefined || selClientName === null) {
        alert("Please select Name..");
        document.getElementById(alertTag).style.fontSize = "10px";
        document.getElementById(alertTag).style.display = "block";
        document.getElementById(alertTag).innerHTML = 'This field should not be empty.. Name';
        return false;
    }
    else {
        document.getElementById(alertTag).style.display = "none";
    }

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
            $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
            $.ajax({
                type: "POST",
                url: "WebService_LedgerMaster.asmx/DeleteEmpMacineAllo",
                data: '{LedgerID:' + JSON.stringify(selClientName) + '}',
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
                    if (res === "Success") {
                        swal("Deleted!", "Your data Deleted", "success");
                        // alert("Your Data has been Saved Successfully...!");
                        location.reload();
                    }

                },
                error: function errorFunc(jqXHR) {
                    $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                    alert(jqXHR);
                }
            });

        });
});


//Supplier wise ItemGroup Allocation
var SuppObjid = [];
var GroupAlloGrid = [];
$("#btnItemGroupAllo").click(function () {
    AuthenticateCurdActions(GBLProductionUnitID).then(async (isAuthorized) => {
        if (!isAuthorized) {
            return;
        }
        document.getElementById("ItemGroupAllocationBtnDeletePopUp").disabled = true;
        $.ajax({
            type: "POST",
            url: "WebService_LedgerMaster.asmx/GetSupplierName",
            data: '{LedgerGrNID:' + JSON.stringify(LedgerGrNID) + '}',
            contentType: "application/json; charset=utf-8",
            dataType: "text",
            success: function (results) {
                var res = results.replace(/\\/g, '');
                res = res.replace(/"d":""/g, '');
                res = res.replace(/""/g, '');
                res = res.replace(/:}/g, ':null}');
                res = res.replace(/:,/g, ':null,');
                res = res.replace(/u0026/g, '&');
                res = res.substr(1);
                res = res.slice(0, -1);
                var SuppName = JSON.parse(res);
                var SupplierID = Number(document.getElementById("txtGetGridRow").value);

                $("#selSuppName").dxSelectBox({
                    items: SuppName,
                    placeholder: "Select..",
                    displayExpr: 'LedgerName',
                    valueExpr: 'LedgerID',
                    searchEnabled: true,
                    showClearButton: true,
                    onValueChanged: function (data) {
                        SuppObjid = [];

                        $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
                        GetExistSpareGrpString(data.value);
                        $.ajax({
                            type: "POST",
                            url: "WebService_LedgerMaster.asmx/ExistGroupID",
                            data: '{SupplierID:' + JSON.stringify(data.value) + '}',
                            contentType: "application/json; charset=utf-8",
                            dataType: "text",
                            success: function (results) {
                                var res = results.replace(/\\/g, '');
                                res = res.replace(/"d":""/g, '');
                                res = res.replace(/""/g, '');
                                res = res.replace(/:}/g, ':null}');
                                res = res.replace(/:,/g, ':null,');
                                res = res.replace(/u0027/g, "'");
                                res = res.replace(/"/g, '');
                                res = res.replace(/GroupAllocationIDString:/g, '');
                                res = res.substr(3);
                                res = res.slice(0, -3);

                                var IDString = res;

                                if (IDString === "" || IDString === null || IDString === undefined) {
                                    document.getElementById("ItemGroupAllocationBtnDeletePopUp").disabled = true;
                                    SuppObjid = [];
                                }
                                else {
                                    document.getElementById("ItemGroupAllocationBtnDeletePopUp").disabled = false;
                                    var selectMIDSplit = IDString.split(',');
                                    for (var s in selectMIDSplit) {
                                        SuppObjid.push(selectMIDSplit[s]);
                                    }
                                }
                                GblGroupAllocation();
                                $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                            }
                        });
                    }
                });
                if (SupplierID > 0 && SupplierID !== undefined) {
                    $("#selSuppName").dxSelectBox({ value: SupplierID });
                } else {
                    $("#selSuppName").dxSelectBox({ value: null });
                }
            }
        });

        $('#ItemGoupAllocationModal').modal({
            show: 'true'
        });
    });
});

function GblGroupAllocation() {
    $.ajax({
        type: "POST",
        url: "WebService_LedgerMaster.asmx/GroupGrid",
        data: '{}',//UnderGroupID:' + JSON.stringify(UnderGroupID) + '
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            var res = results.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/""/g, '');
            res = res.replace(/:}/g, ':null}');
            res = res.replace(/:,/g, ':null,');
            res = res.replace(/u0027/g, "'");
            res = res.replace(/u0026/g, '&');
            res = res.substr(1);
            res = res.slice(0, -1);
            $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
            GroupAlloGrid = JSON.parse(res);
            GroupAllocationGrid();
        }
    });

    $.ajax({
        type: "POST",
        url: "WebService_LedgerMaster.asmx/SpareGroupGrid",
        data: '{}',
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            var res = results.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/""/g, '');
            res = res.replace(/:}/g, ':null}');
            res = res.replace(/:,/g, ':null,');
            res = res.replace(/u0027/g, "'");
            res = res.replace(/u0026/g, '&');
            res = res.substr(1);
            res = res.slice(0, -1);
            $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
            var SpareGroupAlloGrid = JSON.parse(res);
            $("#GridSpareGroupAllocation").dxDataGrid({ dataSource: SpareGroupAlloGrid });
        }
    });
}

function GroupAllocationGrid() {
    $("#GridItemGoupAllocation").dxDataGrid({
        // dataSource: GroupAlloGrid,
        dataSource: {
            store: {
                type: "array",
                key: "ItemGroupID",
                data: GroupAlloGrid
            }
        },
        sorting: {
            mode: "multiple"
        },
        paging: false,
        showBorders: true,
        showRowLines: true,
        selection: { mode: "multiple" },
        filterRow: { visible: true, applyFilter: "auto" },
        height: function () {
            return window.innerHeight / 2;
        },
        pager: {
            showPageSizeSelector: true,
            allowedPageSizes: [15, 25, 50, 100]
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
        columns: [
            { dataField: "ItemGroupID", visible: false, caption: "ItemGroupID", width: 300 },
            { dataField: "ItemGroupName", visible: true, caption: "Item Group Name" }
        ],
        selectedRowKeys: SuppObjid,
        onSelectionChanged: function (selectedItems) {
            var data = selectedItems.selectedRowsData;

            if (data.length > 0) {
                $("#TxtItemGroupNameID").text(
                    $.map(data, function (value) {
                        return value.ItemGroupID;    //alert(value.ProcessId);
                    }).join(','));
            }
            else {
                $("#TxtItemGroupNameID").text("");
            }
        }
    });
}

$("#GridSpareGroupAllocation").dxDataGrid({
    dataSource: [],
    keyExpr: "SparePartGroup",
    sorting: {
        mode: "multiple"
    },
    paging: false,
    showBorders: true,
    showRowLines: true,
    selection: { mode: "multiple" },
    filterRow: { visible: true, applyFilter: "auto" },
    height: function () {
        return window.innerHeight / 2;
    },
    pager: {
        showPageSizeSelector: true,
        allowedPageSizes: [15, 25, 50, 100]
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
    columns: [
        { dataField: "SparePartGroup", visible: true, caption: "Spare Group" }
    ]
});

$("#ItemGroupAllocationBtnSave").click(function () {
    var GridRow = "";
    var alertTag = "ValStrselSuppName";
    var SupplierID = $("#selSuppName").dxSelectBox("instance").option('value');

    if (SupplierID === "" || SupplierID === undefined || SupplierID === null) {
        alert("Please select supplier..");
        document.getElementById(alertTag).style.fontSize = "10px";
        document.getElementById(alertTag).style.display = "block";
        document.getElementById(alertTag).innerHTML = 'This field should not be empty..supplier name';
        return false;
    }
    else {
        document.getElementById(alertTag).style.display = "none";
    }

    var jsonObjectsGroupAllocationDetailRecord = [];
    var OperationGroupAllocationDetailRecord = {};

    var CostingDataGroupAllocation = [];

    var txtMID = $("#TxtItemGroupNameID").text();

    if (txtMID === "null") {
        GridRow = JSON.stringify(SuppObjid);
        GridRow = GridRow.replace(/"/g, '');
        GridRow = GridRow.substr(1);
        GridRow = GridRow.slice(0, -1);
    }
    else if (txtMID === "" || txtMID === null || txtMID === undefined) {
        GridRow = "";
    }
    else {
        GridRow = txtMID;
    }

    var finalString = GridRow;

    if (GridRow !== "") {
        GridRow = GridRow.split(',');

        if (GridRow.length > 0) {
            for (var m = 0; m < GridRow.length; m++) {

                OperationGroupAllocationDetailRecord = {};
                OperationGroupAllocationDetailRecord.LedgerID = SupplierID;
                OperationGroupAllocationDetailRecord.ItemGroupID = GridRow[m];

                jsonObjectsGroupAllocationDetailRecord.push(OperationGroupAllocationDetailRecord);
            }

            CostingDataGroupAllocation = jsonObjectsGroupAllocationDetailRecord;
        }
    }

    ///Spare Group
    var GridSpares = $("#GridSpareGroupAllocation").dxDataGrid('instance');
    var GridRowSpareGroups = GridSpares.getSelectedRowsData();

    var SparePartAllocation = {};
    var ObjSparePartAllocation = [];
    for (var i = 0; i < GridRowSpareGroups.length; i++) {
        SparePartAllocation = {};
        SparePartAllocation.LedgerID = SupplierID;
        SparePartAllocation.SparePartGroup = GridRowSpareGroups[i].SparePartGroup;

        ObjSparePartAllocation.push(SparePartAllocation);
    }
    ////

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

            $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);

            $.ajax({
                type: "POST",
                url: "WebService_LedgerMaster.asmx/SaveGroupAllocation",
                data: '{CostingDataGroupAllocation:' + JSON.stringify(CostingDataGroupAllocation) + ',SuppID:' + JSON.stringify(SupplierID) + ',GridRow:' + JSON.stringify(finalString) + ',ObjSparePartAllocation:' + JSON.stringify(ObjSparePartAllocation) + '}',//
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
                    if (res === "Success") {
                        swal("Saved..!", "Your data saved", "success");
                        $("#selSuppName").dxSelectBox({ value: null });
                    } else {
                        swal.close();
                        setTimeout(() => {
                            swal("Warning..!", res, "warning");
                        }, 100);
                    }
                },
                error: function errorFunc(jqXHR) {
                    $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                    swal("Error!", "Please try after some time..", "");
                    alert(jqXHR);
                }
            });
        });

});

$("#ItemGroupAllocationBtnNew").click(function () {
    $("#selSuppName").dxSelectBox({ value: null });
    SuppObjid = [];
    GblGroupAllocation();
});

$("#ItemGroupAllocationBtnDeletePopUp").click(function () {

    var selSuppName = $("#selSuppName").dxSelectBox("instance").option('value');
    var alertTag = "ValStrselSuppName";

    if (selSuppName === "" || selSuppName === undefined || selSuppName === null) {
        alert("Please select Name..");
        document.getElementById(alertTag).style.fontSize = "10px";
        document.getElementById(alertTag).style.display = "block";
        document.getElementById(alertTag).innerHTML = 'This field should not be empty.. Name';
        return false;
    }
    else {
        document.getElementById(alertTag).style.display = "none";
    }

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
            $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
            $.ajax({
                type: "POST",
                url: "WebService_LedgerMaster.asmx/DeleteGroupAllo",
                data: '{LedgerID:' + JSON.stringify(selSuppName) + '}',
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
                    if (res === "Success") {
                        swal("Deleted!", "Your data Deleted", "success");
                        // alert("Your Data has been Saved Successfully...!");
                        location.reload();
                    }

                },
                error: function errorFunc(jqXHR) {
                    $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                    alert(jqXHR);
                }
            });

        });
});

function GetExistSpareGrpString(SupplierID) {
    $.ajax({
        type: "POST",
        url: "WebService_LedgerMaster.asmx/ExistSparesGroupID",
        data: '{SupplierID:' + JSON.stringify(SupplierID) + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            var res = results.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/""/g, '');
            res = res.replace(/:}/g, ':null}');
            res = res.replace(/:,/g, ':null,');
            res = res.replace(/u0027/g, "'");
            res = res.replace(/"/g, '');
            res = res.replace(/IDString:/g, '');
            res = res.substr(3);
            res = res.slice(0, -3);

            $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
            var dataGridInstance = $("#GridSpareGroupAllocation").dxDataGrid("instance");
            dataGridInstance.clearSelection();
            var selectMIDSplit = res.split(',');
            for (var s in selectMIDSplit) {
                if (!dataGridInstance.isRowSelected(selectMIDSplit[s])) {
                    dataGridInstance.selectRows(selectMIDSplit[s], true);
                }
            }
        }
    });
}
/*----Business Vertical Setting---*/
$("#selBV").dxSelectBox({
    items: [],
    value: '',
    placeholder: "Select..",
    displayExpr: 'BusinessVerticalName',
    valueExpr: 'BusinessVerticalID',
    searchEnabled: true,
    showClearButton: true,
    onValueChanged: function (data) {
        BusinessVerticalDetaildata();

    }
});

$("#selBVSalesPerson").dxSelectBox({
    items: [],
    value: '',
    placeholder: "Select..",
    displayExpr: 'LedgerName',
    valueExpr: 'LedgerID',
    searchEnabled: true,
    showClearButton: true,
    onValueChanged: function (data) {

    }
});


//$("#selStatus").dxSelectBox({
//    items: SelStatus,
//    value: SelStatus[0],
//    placeholder: "Select..",
//    searchEnabled: true,
//    showClearButton: true,
//    onValueChanged: function (data) {

//    }
//});



$("#BtnBVSetting").click(function () {
    AdditionalSettingdata();
    document.getElementById("BtnBVSetting").setAttribute("data-toggle", "modal");
    document.getElementById("BtnBVSetting").setAttribute("data-target", "#BVSettingModal");

});

function AdditionalSettingdata() {
    $.ajax({
        type: "POST",
        url: "WebService_LedgerMaster.asmx/GetAdditionlSettingdata",
        data: '{}',
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            var res = results.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/""/g, '');
            res = res.replace(/:}/g, ':null}');
            res = res.replace(/:,/g, ':null,');
            res = res.replace(/u0026/g, '&');
            res = res.substr(1);
            res = res.slice(0, -1);
            let responseData = JSON.parse(res);
            let BusinessVerticalData = responseData.BusinessVerticalData;
            let SalesPersondata = responseData.SalesPersondata;
            if (BusinessVerticalData.length > 0) {
                $("#selBV").dxSelectBox({
                    items: BusinessVerticalData,
                    placeholder: "Select..",
                    displayExpr: 'BusinessVerticalName',
                    valueExpr: 'BusinessVerticalID',
                    searchEnabled: true,
                    showClearButton: true,
                    onValueChanged: function (data) {
                        BusinessVerticalDetaildata();

                    }
                });
            }
            if (SalesPersondata.length > 0) {
                if (SalesPersondata.length === 1) {
                    $("#selBVSalesPerson").dxSelectBox({
                        items: SalesPersondata,
                        placeholder: "Select..",
                        displayExpr: 'LedgerName',
                        valueExpr: 'LedgerID',
                        searchEnabled: true,
                        showClearButton: true,
                        value: SalesPersondata[0].LedgerID,
                        onValueChanged: function (data) {

                        }
                    });
                }
                else {
                    $("#selBVSalesPerson").dxSelectBox({
                        items: SalesPersondata,
                        placeholder: "Select..",
                        displayExpr: 'LedgerName',
                        valueExpr: 'LedgerID',
                        searchEnabled: true,
                        showClearButton: true,
                        onValueChanged: function (data) {
                        }
                    });
                }

            }
        }
    });
}
function BusinessVerticalDetaildata() {
    var SelectLedgerID = document.getElementById("txtGetGridRow").value;
    var BusinessVerticalID = $('#selBV').dxSelectBox('instance').option('value');
    $.ajax({
        type: "POST",
        url: "WebService_LedgerMaster.asmx/GetBusinessVerticalDetaildata",
        data: '{LedgerID:' + JSON.stringify(SelectLedgerID) + ',BusinessVerticalID:' + JSON.stringify(BusinessVerticalID) + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            var res = results.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/""/g, '');
            res = res.replace(/:}/g, ':null}');
            res = res.replace(/:,/g, ':null,');
            res = res.replace(/u0026/g, '&');
            res = res.substr(1);

            res = res.slice(0, -1);
            let RES1 = JSON.parse(res);
            var Result1 = RES1.Result1;
            AvailableCredit = RES1.Result2;

            $("#GridBVSetting").dxDataGrid({ dataSource: Result1 });
        }
    });
}
$("#BtnBVSave").click(function () {
    let LedgerID = document.getElementById('txtGetGridRow').value;
    let BusinessVerticalID = $('#selBV').dxSelectBox('instance').option('value');
    let SalesPersonLedgerID = $('#selBVSalesPerson').dxSelectBox('instance').option('value');
    let Status = document.getElementById('selStatus').value;
    //let MaxFixedLimit = document.getElementById('txtMaxFL').value || 0;
    //let MaxCreditLimit = document.getElementById('txtMaxCL').value || 0;
    //let MaxCreditPeriod = parseFloat(document.getElementById('txtMaxCP').value || 0);
    //let MaxFixedLimit = parseFloat(document.getElementById('txtMaxFL').value || 0);
    //let MaxCreditLimit = parseFloat(document.getElementById('txtMaxCL').value || 0);


    if (!BusinessVerticalID || BusinessVerticalID <= 0) {
        $('#selBV').focus();
        DevExpress.ui.notify({
            message: "Please select BusinessVertical",
            type: "warning",
            displayTime: 2000,
            position: {
                my: "end",
                at: "center",
                of: window
            }
        });
        return;
    }

    //if (!SalesPersonLedgerID || SalesPersonLedgerID <= 0) {
    //    $('#selBVSalesPerson').focus();
    //    DevExpress.ui.notify({
    //        message: "Please select Sales Person",
    //        type: "warning",
    //        displayTime: 2000,
    //        position: {
    //            my: "end",
    //            at: "center",
    //            of: window
    //        }
    //    });
    //    return;
    //}
    var gridInstance = $("#GridBVSetting").dxDataGrid("instance");
    var currentData = gridInstance.option("dataSource") || []; // Ensure currentData is an array

    let isDuplicateUser = currentData.some(row => row.BusinessVerticalID == BusinessVerticalID && row.LedgerID == LedgerID);

    if (isDuplicateUser && !GblBVStatus) {
        alert("This User is already added for the selected Business Vertical .");
        return;
    }
    //if (MaxFixedLimit > MaxCreditLimit) {
    //    alert("Fixed Limit cannot exceed Maximum Credit Limit. Please adjust the values.");
    //    document.getElementById('txtMaxFL').value = '';
    //    return;
    //}
    var jsonObjectsRecord = [];
    var jsonObjectsRecordData = {};

    jsonObjectsRecordData.LedgerID = LedgerID;
    jsonObjectsRecordData.BusinessVerticalID = BusinessVerticalID;
    jsonObjectsRecordData.SalesPersonLedgerID = SalesPersonLedgerID;
    jsonObjectsRecordData.Status = Status;
    //jsonObjectsRecordData.MaxCreditLimit = MaxCreditLimit;
    //jsonObjectsRecordData.MaxFixedLimit = MaxFixedLimit;
    //jsonObjectsRecordData.MaxCreditPeriod = MaxCreditPeriod;
    jsonObjectsRecord.push(jsonObjectsRecordData);

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
            if (GblBVStatus === true) {
                $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);

                $.ajax({
                    type: "POST",
                    url: "WebService_LedgerMaster.asmx/UpdateBusinessVerticalDetailsData",
                    data: '{BusinessVerticalDetailsData:' + JSON.stringify(jsonObjectsRecord) + ',BusinessVerticalDetailID:' + document.getElementById('GBLBusinessVerticalDetailID').value + '}',
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
                        if (res === "Success") {
                            swal("update!", "Your data Update", "success");
                            ClearBVData();
                        }
                        else if (res.includes("not authorized")) {
                            swal("Access Denied...!", res, "warning");
                        } else {
                            swal("Error Occured..!", res, "error");
                        }
                    },
                    error: function errorFunc(jqXHR) {
                        $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                        swal("Error!", "Please try after some time..", "");
                    }
                });
            } else {
                document.getElementById("LOADER").style.display = "block";

                $.ajax({
                    type: "POST",
                    url: "WebService_LedgerMaster.asmx/SaveBusinessVerticalDetailsData",
                    data: '{BusinessVerticalDetailsData:' + JSON.stringify(jsonObjectsRecord) + '}',
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
                            swal("Saved!", "Your data saved", "success");
                            ClearBVData();
                        }
                        else if (res.includes("not authorized")) {
                            swal("Access Denied...!", res, "warning");
                        } else {
                            swal("Error Occured..!", res, "error");
                        }
                    },
                    error: function errorFunc(jqXHR) {
                        document.getElementById("LOADER").style.display = "none";
                        swal("Error!", "Please try after some time..", "");
                        alert(jqXHR);
                    }
                });
            }

        });

});

function ClearBVData() {
    ObjGrid = [];
    GblBVStatus = false;
    BusinessVerticalDetaildata();
    $("#selBV").dxSelectBox({ value: null });
    //$("#selStatus").dxSelectBox({ value: null });
    $("#selBVSalesPerson").dxSelectBox({ value: null });
    document.getElementById('selStatus').value = 'Active';
    //document.getElementById('txtMaxCL').value = '';
    //document.getElementById('txtMaxFL').value = '';
    //document.getElementById('txtMaxCP').value = '';
    document.getElementById('GBLBusinessVerticalDetailID').value = '';
    document.getElementById("BtnBvUpdateACStatus").style.display = "none";
    document.getElementById("BtnBVDelete").style.display = "none";
    document.getElementById('BtnBVSave').innerText = 'Save';

}

$("#GridBVSetting").dxDataGrid({
    dataSource: [],
    columnAutoWidth: true,
    showBorders: true,
    showRowLines: true,
    allowColumnReordering: true,
    allowReordering: false,
    paging: {
        pageSize: 20
    },
    pager: {
        showPageSizeSelector: true,
        allowedPageSizes: [20, 40, 80, 300]
    },
    selection: { mode: "single" },
    grouping: {
        autoExpandAll: true
    },
    height: function () {
        return window.innerHeight / 2.5;
    },
    filterRow: { visible: true, applyFilter: "auto" },
    headerFilter: { visible: false },
    rowAlternationEnabled: true,
    searchPanel: { visible: false },
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
            e.rowElement.css('font-weight', 'bold');
        }
        e.rowElement.css('fontSize', '11px');
    },

    columns: [
        { dataField: "BusinessVerticalDetailID", visible: false, caption: "BusinessVerticalDetailID" },
        { dataField: "SalesPersonLedgerID", visible: false, caption: "SalesPersonLedgerID" },
        { dataField: "LedgerID", visible: false, caption: "LedgerID" },
        { dataField: "BusinessVerticalID", visible: false, caption: "BusinessVerticalID" },
        { dataField: "LedgerName", visible: true, caption: "Client Name" },
        { dataField: "BusinessVerticalName", visible: true, caption: "Business Vertical Name" },
        { dataField: "SalesPersonName", visible: true, caption: "Sales Person Name" },
        { dataField: "MaxCreditLimit", visible: false, caption: "Max Credit Limit" },
        { dataField: "MaxFixedLimit", visible: false, caption: "Fixed Limit" },
        { dataField: "MaxCreditPeriod", visible: false, caption: "Max Credit Period(days)" },
        { dataField: "Status", visible: true, caption: "Client Status" },

        // Add custom column for Edit Button
        {
            type: "buttons",
            caption: "Actions",
            width: 100,
            buttons: [{
                icon: "fa fa-edit",  // Custom Font Awesome icon
                hint: "Edit",
                onClick: function (e) {
                    var rowData = e.row.data;

                    ObjGrid = [];
                    ObjGrid = e.row.data;
                    var dataGrid = $("#GridBVSetting").dxDataGrid("instance");

                    // Select the row using DevExtreme method
                    dataGrid.selectRows([e.row.rowIndex], true);
                    //document.getElementById("BtnBvUpdateACStatus").style.display = "inline-block";
                    document.getElementById("BtnBVDelete").style.display = "inline-block";
                    document.getElementById('BtnBVSave').innerText = 'Update';

                    // Populate other fields
                    $("#selBV").dxSelectBox("instance").option("value", rowData.BusinessVerticalID);
                    //$("#selStatus").dxSelectBox("instance").option("value", rowData.Status);
                    $("#selBVSalesPerson").dxSelectBox("instance").option("value", rowData.SalesPersonLedgerID);
                    //document.getElementById('txtMaxCL').value = rowData.MaxCreditLimit;
                    //document.getElementById('txtMaxFL').value = rowData.MaxFixedLimit;
                    //document.getElementById('txtMaxCP').value = rowData.MaxCreditPeriod;
                    document.getElementById('selStatus').value = rowData.Status;
                    document.getElementById('GBLBusinessVerticalDetailID').value = rowData.BusinessVerticalDetailID;
                    GblBVStatus = true;
                }
            }]
        }
    ]
});
$("#BtnBVNew").click(function () {
    ClearBVData()
});
$("#BtnBVDelete").click(function () {

    var GBLBusinessVerticalDetailID = document.getElementById("GBLBusinessVerticalDetailID").value;
    if (GBLBusinessVerticalDetailID === "" || GBLBusinessVerticalDetailID === null || GBLBusinessVerticalDetailID === undefined) {
        Swal.fire({
            icon: 'warning',
            title: 'No Row Selected',
            text: 'Please choose a row from the grid below.',
            confirmButtonText: 'OK'
        });
        return false;

    }

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
                url: "WebService_LedgerMaster.asmx/DeleteBusinessVerticalDetailData",
                data: '{BusinessVerticalDetailID:' + JSON.stringify(document.getElementById("GBLBusinessVerticalDetailID").value.trim()) + '}',
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
                        ClearBVData();
                    } else if (res.includes("not authorized")) {
                        swal("Access Denied...!", res, "warning");
                    } else {
                        swal("Error Occured..!", res, "error");
                    }
                },
                error: function errorFunc(jqXHR) {
                    document.getElementById("LOADER").style.display = "none";
                    alert(jqXHR);
                }
            });

        });
});


//$("#BtnBvUpdateACStatus").click(function () {
//    var GBLBusinessVerticalDetailID = document.getElementById("GBLBusinessVerticalDetailID").value;
//    if (GBLBusinessVerticalDetailID === "" || GBLBusinessVerticalDetailID === null || GBLBusinessVerticalDetailID === undefined) {
//        Swal.fire({
//            icon: 'warning',
//            title: 'No Row Selected',
//            text: 'Please choose a row from the grid below.',
//            confirmButtonText: 'OK'
//        });
//        return false;

//    }
//    embargoStatus = ObjGrid.EmbargoStatus;
//    if (embargoStatus === 'Not Placed') {
//        $('#BtnPEmbargo').show();
//        $('#BtnLEmbargo').hide();
//        document.getElementById('txtStatus').value = "InActive";
//    } else if (embargoStatus === 'Placed') {
//        $('#BtnPEmbargo').hide();
//        $('#BtnLEmbargo').show();
//        document.getElementById('txtStatus').value = "Active";
//    } else if (embargoStatus === 'Lifted') {
//        $('#BtnPEmbargo').show();
//        $('#BtnLEmbargo').hide();
//        document.getElementById('txtStatus').value = "InActive";
//    }
//    else {
//        $('#BtnPEmbargo').show();
//        $('#BtnLEmbargo').hide();
//        document.getElementById('txtStatus').value = "InActive";

//    }
//    document.getElementById('txtACMaxCL').value = ObjGrid.MaxCreditLimit;
//    document.getElementById('txtACMaxFL').value = ObjGrid.MaxFixedLimit;
//    document.getElementById('txtACMaxCP').value = ObjGrid.MaxCreditPeriod;
//    //document.getElementById('txtStatus').value = ObjGrid.Status;
//    document.getElementById('txtSalesPerson').value = ObjGrid.SalesPersonName;
//    document.getElementById('txtBV').value = ObjGrid.BusinessVerticalName;
//    document.getElementById('txtACClientName').value = ObjGrid.LedgerName;
//    if (AvailableCredit.length > 0) {
//        document.getElementById('txtTACredit').value = ObjGrid.MaxCreditLimit - AvailableCredit[0].TotalSale;

//    } else {
//        document.getElementById('txtTACredit').value = ObjGrid.MaxCreditLimit;
//    }
//    document.getElementById('txtLYExpo').value = ObjGrid.LastYearExposure;
//    document.getElementById('txtMOverdays').value = ObjGrid.MaxOverdueDays;
//    document.getElementById('txtTOverAmount').value = ObjGrid.TotalOverdueAmount;
//    document.getElementById("BtnBvUpdateACStatus").setAttribute("data-toggle", "modal");
//    document.getElementById("BtnBvUpdateACStatus").setAttribute("data-target", "#ACSettingModal");
//});
//$('#BtnPEmbargo').on('click', function () {
//    $('#poptag3').text('Place Embargo');
//    RadioValue = ["Bad Payment Overdue history", "Others"];
//    $("#OptradioEmbargo").dxRadioGroup("instance").option("items", RadioValue);
//    $("#OptradioEmbargo").dxRadioGroup("instance").option("value", RadioValue[0]);
//    document.getElementById('txtEmbargoSalesPerson').value = ObjGrid.SalesPersonName;
//    document.getElementById('txtEmbargoBV').value = ObjGrid.BusinessVerticalName;
//    document.getElementById('txtEmbargoClientName').value = ObjGrid.LedgerName;
//    $('#EmbargoSettingModal').modal('show');
//    embargoStatus = 'Placed'
//});

//$('#BtnLEmbargo').on('click', function () {
//    $('#poptag3').text('Lift Embargo');
//    RadioValue = ["Improved Payment Overdue history", "Others"];
//    $("#OptradioEmbargo").dxRadioGroup("instance").option("items", RadioValue);
//    $("#OptradioEmbargo").dxRadioGroup("instance").option("value", RadioValue[0]);
//    document.getElementById('txtEmbargoSalesPerson').value = ObjGrid.SalesPersonName;
//    document.getElementById('txtEmbargoBV').value = ObjGrid.BusinessVerticalName;
//    document.getElementById('txtEmbargoClientName').value = ObjGrid.LedgerName;
//    $('#EmbargoSettingModal').modal('show');
//    embargoStatus = 'Lifted'
//});


//$("#OptradioEmbargo").dxRadioGroup({
//    items: RadioValue,
//    value: RadioValue[0], // Default to "Machine Dashboard"
//    layout: "horizontal",
//    onValueChanged: function (e) {
//        var selectedValue = e.value;

//    },
//});


//$("#BtnSaveEmbargo").click(function () {
//    let LedgerID = document.getElementById('txtGetGridRow').value;
//    let BusinessVerticalID = $('#selBV').dxSelectBox('instance').option('value');
//    let SalesPersonLedgerID = $('#selBVSalesPerson').dxSelectBox('instance').option('value');
//    let Remark = document.getElementById('txtEmbargoRemark').value;
//    let txtStatus = document.getElementById('txtStatus').value;
//    var EmbargoReason = $("#OptradioEmbargo").dxRadioGroup("instance").option("value");

//    if (EmbargoReason === 'Others') {
//        if (!Remark) {
//            $('#txtEmbargoRemark').focus();
//            DevExpress.ui.notify({
//                message: "Please Enter Remark",
//                type: "warning",
//                displayTime: 2000,
//                position: {
//                    my: "end",
//                    at: "center",
//                    of: window
//                }
//            });
//            return;
//        }
//    }
//    var jsonObjectsRecord = [];
//    var jsonObjectsRecordData = {};

//    jsonObjectsRecordData.LedgerID = LedgerID;
//    jsonObjectsRecordData.BusinessVerticalID = BusinessVerticalID;
//    jsonObjectsRecordData.SalesPersonLedgerID = SalesPersonLedgerID;
//    jsonObjectsRecordData.Remark = Remark;
//    jsonObjectsRecordData.EmbargoReason = EmbargoReason;
//    jsonObjectsRecordData.EmbargoStatus = embargoStatus;
//    jsonObjectsRecord.push(jsonObjectsRecordData);

//    var txt = 'If you confident please click on \n' + 'Yes, Save it ! \n' + 'otherwise click on \n' + 'Cancel';
//    swal({
//        title: "Do you want to continue",
//        text: txt,
//        type: "warning",
//        showCancelButton: true,
//        confirmButtonColor: "#DD6B55",
//        confirmButtonText: "Yes, Save it !",
//        closeOnConfirm: false
//    },
//        function () {

//            $.ajax({
//                type: "POST",
//                url: "WebService_LedgerMaster.asmx/SaveEmbargoDetailsData",
//                data: '{EmbargoDetailsData:' + JSON.stringify(jsonObjectsRecord) + ',BusinessVerticalID:' + JSON.stringify(BusinessVerticalID) + ',LedgerID:' + JSON.stringify(LedgerID) + ',EmbargoStatus:' + JSON.stringify(embargoStatus) + ',txtStatus:' + JSON.stringify(txtStatus) + '}',
//                contentType: "application/json; charset=utf-8",
//                dataType: "json",
//                success: function (results) {
//                    var res = JSON.stringify(results);
//                    res = res.replace(/"d":/g, '');
//                    res = res.replace(/{/g, '');
//                    res = res.replace(/}/g, '');
//                    res = res.substr(1);
//                    res = res.slice(0, -1);
//                    document.getElementById("LOADER").style.display = "none";
//                    if (res === "Success") {
//                        swal("Saved!", "Your data saved", "success");
//                        $('#EmbargoSettingModal').modal('hide');
//                        $('#ACSettingModal').modal('hide');
//                        ClearBVData()

//                    }
//                    else if (res.includes("not authorized")) {
//                        swal("Access Denied...!", res, "warning");
//                    } else {
//                        swal("Error Occured..!", res, "error");
//                    }
//                },
//                error: function errorFunc(jqXHR) {
//                    document.getElementById("LOADER").style.display = "none";
//                    swal("Error!", "Please try after some time..", "");
//                    alert(jqXHR);
//                }
//            });


//        });

//});
//$("#BtnBEmbargoShowList").click(function () {
//    EmbargoDetaildata();
//    $('#EmbargoShowlistModal').modal('show');
//});

//$("#GridEmbargoShowlist").dxDataGrid({
//    dataSource: [],
//    columnAutoWidth: true,
//    showBorders: true,
//    showRowLines: true,
//    allowColumnReordering: true,
//    allowReordering: false,
//    paging: {
//        pageSize: 20
//    },
//    pager: {
//        showPageSizeSelector: true,
//        allowedPageSizes: [20, 40, 80, 300]
//    },
//    selection: { mode: "single" },
//    grouping: {
//        autoExpandAll: true
//    },
//    height: function () {
//        return window.innerHeight / 1.6;
//    },
//    filterRow: { visible: true, applyFilter: "auto" },
//    headerFilter: { visible: false },
//    rowAlternationEnabled: true,
//    searchPanel: { visible: false },
//    loadPanel: {
//        enabled: true,
//        height: 90,
//        width: 200,
//        text: 'Data is loading...'
//    },

//    onRowPrepared: function (e) {
//        if (e.rowType === "header") {
//            e.rowElement.css('background', '#42909A');
//            e.rowElement.css('color', 'white');
//            e.rowElement.css('font-weight', 'bold');
//        }
//        e.rowElement.css('fontSize', '11px');
//    },

//    columns: [
//        { dataField: "BusinessVerticalName", visible: true, caption: "Business Vertical Name" },
//        { dataField: "LedgerName", visible: true, caption: "Client Name" },
//        { dataField: "SalesPersonName", visible: true, caption: "Sales Person Name" },
//        { dataField: "EmbargoReason", visible: true, caption: "Embargo Reason" },
//        { dataField: "EmbargoStatus", visible: true, caption: "Embargo Status" },
//        { dataField: "Remark", visible: true, caption: "Remark" },
//        { dataField: "UserName", visible: true, caption: "Embargo set by" }
//    ]
//});

//function EmbargoDetaildata() {
//    let LedgerID = document.getElementById('txtGetGridRow').value;
//    $.ajax({
//        type: "POST",
//        url: "WebService_LedgerMaster.asmx/GetEmbargoDetaildata",
//        data: '{LedgerID:' + JSON.stringify(LedgerID) + '}',
//        contentType: "application/json; charset=utf-8",
//        dataType: "text",
//        success: function (results) {
//            var res = results.replace(/\\/g, '');
//            res = res.replace(/"d":""/g, '');
//            res = res.replace(/""/g, '');
//            res = res.replace(/:}/g, ':null}');
//            res = res.replace(/:,/g, ':null,');
//            res = res.replace(/u0026/g, '&');
//            res = res.substr(1);
//            res = res.slice(0, -1);
//            let RES1 = JSON.parse(res);
//            $("#GridEmbargoShowlist").dxDataGrid({ dataSource: RES1 });
//        }
//    });
//}