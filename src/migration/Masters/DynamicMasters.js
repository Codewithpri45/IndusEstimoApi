
//=============Active Link By Element ID By Pradeep============================
var GblStatus = "";
var GblItemNameString = "";
var GblItemDecString = "";
var FilterObj = [];
var newArrayFilterObj = [];
var isSaveASClicked = false;

var updateColumnBasedOnGroup = [];
var UpdatedItemName = [];
var updateGroupFlag = true;
var queryString = new Array();
let PWOSpecialPaperCreateFlag = false;
var GBLProductionUnitID = getProductionUnitID('ProductionUnitID');

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

//Dynamic Maser UL
function getMasterLIST() {
    var masterID = document.getElementById("MasterID").innerHTML;

    var currentMaster = "";
    if (masterID !== "") {
        currentMaster = "ChooseMaster" + masterID;
    }
    try {
        // $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
        $.ajax({
            async: false,
            type: "POST",
            url: "WebService_Master.asmx/MasterList",
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

                // $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
                for (var i = 0; i < RES1.length; i++) {
                    MasterList += "<li role='presentation' id=ChooseMaster" + RES1[i].ItemGroupID + " class=''><a id=" + RES1[i].ItemGroupID + "   href='#' data-toggle='tab'  onclick='CurrentMaster(this);' style='color:#42909A;font-size:10px;font-weight:600;width:100%;text-align: left;'>" + RES1[i].ItemGroupName.replace(/_/g, ' '); + "</a></li>";

                }
                $("#MasterUL").append('<li style="border-bottom:1px solid #42909A"><label style="color: #42909A; margin-left: .5em;font-size:12px;font-weight:600">Select Master</label></li>');
                $('#MasterUL').append(MasterList);

                if (currentMaster !== "") {
                    document.getElementById(currentMaster).className = "active";
                }
                document.getElementById("LI_ChooseMaster").className = "dropdown open";
            }
        });
    } catch (e) {
        //  $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
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
        document.getElementById("BtnSaveAS").disabled = true;
        document.getElementById("BtnDeletePopUp").disabled = true;
        document.getElementById("BtnSave").disabled = "";

        document.getElementById("IsactivItemstatic_Div").style.display = "none";

        document.getElementById("LblItemCode").style.display = "none";
        document.getElementById("LblItemCode").innerHTML = "";

        document.getElementById("mySidenav").style.width = "0";
        document.getElementById('MYbackgroundOverlay').style.display = 'none';

        $('#largeModal').modal({
            show: 'true'
        });

        if (PWOSpecialPaperCreateFlag === true) {
            PWOSpecialItemCreate();
        } else {
            refreshbtn();
        };

        let gridInstnace = $("#MasterGrid").dxDataGrid("instance");
        gridInstnace.clearSelection();
        document.getElementById("txtGetGridRow").value = "";
        //refreshbtn();
    });
});

//Function For Create Controlls
function DynamicControlls() {
    var masterID = document.getElementById("MasterID").innerHTML;
    var fieldContainer = "";
    document.getElementById("FieldCntainerRow").innerHTML = "";

    $.ajax({
        async: false,
        type: "POST",
        url: "WebService_Master.asmx/Master",
        data: '{masterID:' + JSON.stringify(masterID) + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            var res = results.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/""/g, '');
            res = res.replace(/:}/g, ':null}');
            res = res.replace(/:,/g, ':null,');
            res = res.replace(/u0026/g, '&');
            res = res.replace(/u0027u0027/g, "''");
            res = res.substr(1);
            res = res.slice(0, -1);
            //document.getElementById("LOADER").style.display = "none";
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

                    var chngevt = RES1[i].ControllValidation;
                    fieldContainer = "";
                    let fieldValidatorMark = "";
                    if (RES1[i].IsRequiredFieldValidator !== undefined && RES1[i].IsRequiredFieldValidator !== null) {
                        if (RES1[i].IsRequiredFieldValidator) {
                            fieldValidatorMark = '<b style="color:red">*</b>'
                        }
                    }
                    let numberMaxString = "";
                    let isDisable = "";
                    if (RES1[i].FieldType === "text" || RES1[i].FieldType === "number") {
                        if (RES1[i].IsLocked) { isDisable = "disabled" }
                        if (Number(RES1[i].MaximumValue) > 0) { numberMaxString = ' max="' + Number(RES1[i].MaximumValue) + '"' }
                        if (RES1[i].FieldFormula === "" || RES1[i].FieldFormula === null || RES1[i].FieldFormula === undefined) {
                            if (chngevt === "" || chngevt === null || chngevt === "null" || chngevt === undefined) {
                                fieldContainer = '<div class="col-lg-3 col-md-3 col-sm-3 col-xs-12" style="float:left;margin-bottom:0px;display:' + IsDisplayCol + '">' +
                                    '<label style="float: left; width: 100%;">' + RES1[i].FieldDisplayName + fieldValidatorMark + '</label><br />' +
                                    '<input id=' + RES1[i].FieldName + ' type="' + RES1[i].FieldType + '" class="forTextBox" min="0"' + numberMaxString + ' ' + isDisable + ' /><br />' +
                                    '<div style="min-height:20px;float:left;width:100%"><strong id=ValStr' + RES1[i].FieldName + ' style="color:red;font-size:12px;display:none"></strong></div></div>';

                            } else {
                                fieldContainer = '<div class="col-lg-3 col-md-3 col-sm-3 col-xs-12" style="float:left;margin-bottom:0px;display:' + IsDisplayCol + '">' +
                                    '<label style="float: left; width: 100%;">' + RES1[i].FieldDisplayName + fieldValidatorMark + '</label><br />' +
                                    '<input id=' + RES1[i].FieldName + ' type="' + RES1[i].FieldType + '" class="forTextBox" onchange="' + chngevt + '" min="0" ' + numberMaxString + ' ' + isDisable + ' /><br />' +
                                    '<div style="min-height:20px;float:left;width:100%"><strong id=ValStr' + RES1[i].FieldName + ' style="color:red;font-size:10px;display:block"></strong></div></div>';
                            }
                            $("#FieldCntainerRow").append(fieldContainer);
                        } else {
                            fieldContainer = '<div class="col-lg-3 col-md-3 col-sm-3 col-xs-12" style="float:left;margin-bottom:0px;display:' + IsDisplayCol + '">' +
                                '<label style="float: left; width: 100%;">' + RES1[i].FieldDisplayName + fieldValidatorMark + '</label><br />' +
                                '<input id=' + RES1[i].FieldName + ' type="' + RES1[i].FieldType + '" value=0 class="forTextBox" onchange="FarmulaChange(this);" min="0" ' + numberMaxString + ' ' + isDisable + ' /><br />' +
                                '<div style="min-height:20px;float:left;width:100%"><strong id=ValStr' + RES1[i].FieldName + ' style="color:red;font-size:10px;display:block"></strong><textarea id=ValCh' + RES1[i].FieldName + ' style="display: none" >' + RES1[i].FieldFormulaString + '</textarea><strong id=Formula' + RES1[i].FieldName + ' style="display: none">' + RES1[i].FieldFormula + '</strong></div></div>';
                            $("#FieldCntainerRow").append(fieldContainer);
                        }
                    }
                    else if (RES1[i].FieldType === "checkbox") {
                        fieldContainer = '<div class="col-lg-3 col-md-3 col-sm-3 col-xs-12" style="float:left;margin-bottom:0px;display:' + IsDisplayCol + '">' +
                            '<label style="float: left; width: 100%;">' + RES1[i].FieldDisplayName + fieldValidatorMark + '</label><br />' +
                            '<input type="checkbox" id="' + RES1[i].FieldName + '" class="filled-in chk-col-red" style="height:20px"/>' +
                            '<label for="' + RES1[i].FieldName + '" style="height:20px"></label><br />' +
                            '<div style="min-height:20px;float:left;width:100%"><strong id=ValStr' + RES1[i].FieldName + ' style="color:red;font-size:10px"></strong></div></div>';
                        $("#FieldCntainerRow").append(fieldContainer);
                    }
                    else if (RES1[i].FieldType === "datebox") {
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
                    else if (RES1[i].FieldType === "textarea") {
                        fieldContainer = '<div class="col-lg-3 col-md-3 col-sm-3 col-xs-12" style="float:left;margin-bottom:0px;display:' + IsDisplayCol + '">' +
                            '<label style="float: left; width: 100%;">' + RES1[i].FieldDisplayName + fieldValidatorMark + '</label><br />' +
                            '<textarea id="' + RES1[i].FieldName + '" style="float: left; width: 100%; height: 27px; border-radius: 4px;padding-left:10px;padding-right:10px" onchange="' + chngevt + '"></textarea><br />' +
                            '<div style="min-height:20px;float:left;width:100%"><strong id=ValStr' + RES1[i].FieldName + ' style="color:red;font-size:10px"></strong></div></div>';
                        $("#FieldCntainerRow").append(fieldContainer);
                    }
                    else if (RES1[i].FieldType === "selectbox") {
                        DEselID = RES1[i].FieldName;
                        var DEselQuery = "";
                        DEselQuery = RES1[i].SelectBoxQueryDB;

                        var DEselDefault = "";
                        DEselDefault = RES1[i].SelectBoxDefault;
                        if (RES1[i].IsLocked) { isDisable = "disabled" }
                        if (Number(RES1[i].MaximumValue) > 0) {
                            numberMaxString = ' validationRules: [{ type: "custom",message: "Value must not exceed ' + Number(RES1[i].MaximumValue) + '",validationCallback: function (e) {return e.value <= ' + Number(RES1[i].MaximumValue) + ';}}]'
                        }

                        if (DEselQuery === "" || DEselQuery === "null" || DEselQuery === null || DEselQuery === undefined) {

                            fieldContainer = "";
                            fieldContainer = '<div class="col-lg-3 col-md-3 col-sm-3 col-xs-12" style="float:left;margin-bottom:0px;display:' + IsDisplayCol + '">' +
                                '<label style="float: left; width: 100%;">' + RES1[i].FieldDisplayName + fieldValidatorMark + '</label><br />' +
                                '<div id="' + RES1[i].FieldName + '"  style="float: left; width: 100%;height:30px;border: 1px solid #d3d3d3"></div><br />' +
                                '<div style="min-height:20px;float:left;width:100%"><strong id=ValStr' + RES1[i].FieldName + ' style="color:red;font-size:10px"></strong>  <textarea id=ValCh' + RES1[i].FieldName + ' style="display: none" >' + RES1[i].FieldFormulaString + '</textarea><strong id=Formula' + RES1[i].FieldName + ' style="display: none">' + RES1[i].FieldFormula + '</strong>  </div></div>';
                            $("#FieldCntainerRow").append(fieldContainer);

                            var ItemPush = [];
                            var itemLength = 0;
                            if (DEselDefault !== null && DEselDefault !== "null" && DEselDefault !== undefined) {
                                var item = DEselDefault.split(',');
                                itemLength = item.length;
                            }


                            if (itemLength > 0) {
                                for (var k = 0; k < itemLength; k++) {
                                    ItemPush.push(item[k]);
                                }
                            }
                            var SID = "#" + DEselID;

                            if (RES1[i].FieldFormula === "" || RES1[i].FieldFormula === null || RES1[i].FieldFormula === undefined) {
                                $(SID).dxSelectBox({
                                    items: ItemPush,
                                    placeholder: "Select--",
                                    //displayExpr: 'GroupName',
                                    //valueExpr: 'GroupID',
                                    showClearButton: true,
                                    acceptCustomValue: true,
                                    searchEnabled: true,
                                    onValueChanged: function (data) {
                                        if (data) {

                                            const newValue = data.value;
                                            if (newValue !== undefined && newValue !== null) {
                                                const $element = data.component.element();
                                                const validator = $element.data("dxValidator")
                                                    ? $element.dxValidator("instance")
                                                    : null;

                                                if (validator) {
                                                    const validationResult = validator.validate(); // Trigger validation

                                                    if (!validationResult.isValid) {
                                                        // If invalid, revert to previous value
                                                        data.component.option("value", data.previousValue);
                                                        //DevExpress.ui.notify(validationResult.brokenRule.message, "error", 2000);
                                                        showDevExpressNotification(validationResult.brokenRule.message, "error");
                                                        return; // Prevent further execution
                                                    }
                                                } else {
                                                    //console.warn("No validator attached to this dxSelectBox.");
                                                }
                                            }
                                        }
                                    }
                                });

                                if (Number(RES1[i].MaximumValue) > 0) {
                                    let maxVal = Number(RES1[i].MaximumValue);
                                    addValidationRule(SID, {
                                        type: "custom",
                                        message: "Value must not exceed " + maxVal + "",
                                        validationCallback: function (e) {
                                            return e.value <= maxVal;
                                        }
                                    })
                                }
                            } else {
                                $(SID).dxSelectBox({
                                    items: ItemPush,
                                    placeholder: "Select--",
                                    //displayExpr: 'GroupName',
                                    //valueExpr: 'GroupID',
                                    showClearButton: true,
                                    acceptCustomValue: true,
                                    searchEnabled: true,
                                    onValueChanged: function (data) {
                                        if (data) {

                                            const newValue = data.value;
                                            if (newValue !== undefined && newValue !== null) {
                                                const $element = data.component.element();
                                                const validator = $element.data("dxValidator")
                                                    ? $element.dxValidator("instance")
                                                    : null;

                                                if (validator) {

                                                    const validationResult = validator.validate(); // Trigger validation

                                                    if (!validationResult.isValid) {
                                                        // If invalid, revert to previous value
                                                        data.component.option("value", 0);
                                                        //DevExpress.ui.notify(validationResult.brokenRule.message, "error", 2000);
                                                        showDevExpressNotification(validationResult.brokenRule.message, "error");
                                                        return; // Prevent further execution
                                                    }
                                                } else {
                                                    //console.warn("No validator attached to this dxSelectBox.");
                                                }
                                            }
                                            var currentID = data.element.context.id;
                                            FarmulaChangeSELECTBX(currentID);
                                        }
                                    }
                                });
                                if (Number(RES1[i].MaximumValue) > 0) {
                                    let maxVal = Number(RES1[i].MaximumValue);
                                    addValidationRule(SID, {
                                        type: "custom",
                                        message: "Value must not exceed " + maxVal + "",
                                        validationCallback: function (e) {
                                            return e.value <= maxVal;
                                        }
                                    })
                                }
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
                                selID = RES1[i].FieldName.replace(' ', '');
                            }
                            else {
                                selID = selID + ' ? ' + RES1[i].FieldName.replace(' ', '');
                            }

                            if (selQuery === "" || selQuery === null || selQuery === undefined) {
                                // selQuery = RES1[i].SelectboxQueryDB;
                                selQuery = RES1[i].ItemGroupFieldID;
                            }
                            else {
                                //selQuery = selQuery + ' ? ' + RES1[i].SelectboxQueryDB;
                                selQuery = selQuery + ' ? ' + RES1[i].ItemGroupFieldID;

                            }

                        }

                    }

                }

                $("#FieldCntainerRow").append('<div id="IsactivItemstatic_Div" class="col-lg-2 col-md-2 col-sm-3 col-xs-12" style="float:left;margin-bottom:0px;display:none">' +
                    '<label style="float: left; width: 100%;">Is Active Item Master.?</label><br />' +
                    '<input type="checkbox" id="IsactivItemstatic" class="filled-in chk-col-red" style="height:20px" checked="true"/>' +
                    '<label for="IsactivItemstatic" style="height:20px" ></label><br />' +
                    '<div style="min-height:20px;float:left;width:100%"><strong id="ValStrIsactivItemstaticLabel" style="color:red;font-size:10px"></strong></div></div>');
                fieldContainer = '<div class="col-lg-6 col-md-6 col-sm-6 col-xs-12" style="float:left;margin-bottom:0px;">' +
                    '<label style="float: left; width: 100%;">Item Name</label><br />' +
                    '<input id="TxtItemName" type="text" class="forTextBox"/><br />' +
                    '<div style="min-height:20px;float:left;width:100%"><strong id="ValStrTxtItemName" style="color:red;font-size:12px;display:none"></strong></div></div>';
                $("#FieldCntainerRow").append(fieldContainer);

                //Tally Code Added on 280720 pKp
                fieldContainer = '<div class="col-lg-4 col-md-4 col-sm-4 col-xs-12" style="float:left;margin-bottom:0px;">' +
                    '<label style="float: left; width: 100%;">Tally Code</label><br />' +
                    '<input id="TxtTallyItemName" type="text" class="forTextBox"/><br />' +
                    '<div style="min-height:20px;float:left;width:100%"><strong id="ValStrTxtTallyItemName" style="color:red;font-size:12px;display:none"></strong></div></div>';
                $("#FieldCntainerRow").append(fieldContainer);
                ///

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

function addValidationRule(selctboxID, newRule) {

    let validator = $(selctboxID).dxValidator({
        validationRules: []
    }).dxValidator("instance");

    let currentRules = validator.option("validationRules") || [];
    currentRules.push(newRule);
    validator.option("validationRules", currentRules);
}

//Fill Dynamic Selectbox
function selctbox() {
    var selbox = "";
    $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
    $.ajax({
        async: false,
        type: "POST",
        url: "WebService_Master.asmx/SelectBoxLoad",
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
            $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);

            if (res === "") {
                selbox = [];
            } else {
                selbox = "";
                selbox = JSON.parse(res);
            }
            //   document.getElementById("LOADER").style.display = "none";

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
                var selectID = JSON.stringify(Tblobj[Tblobj.length - 1]);

                if (selA.length > 1) {
                    ///////////////////////////////////////With  valueExpr/////////////////////////////////////////////
                    Displayxpr = selA[1];
                    Valuexpr = selA[0];

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
                        //    var NewItem = e.value;
                        //    Tblobj.push(NewItem);
                        //    editableProduct.option("items", Tblobj);
                        //    e.customItem = NewItem;
                        //   // return NewItem;
                        //}
                    });
                }
                else {
                    Displayxpr = selA[0];
                    Valuexpr = selA[0];
                    ///////////////////////////////////////WithOut  valueExpr/////////////////////////////////////////////

                    selectID = selectID.substr(1);
                    selectID = selectID.slice(0, -1);
                    selectID = selectID.replace(/"/g, '');
                    selectID = selectID.replace(/ /g, '');
                    selectID = selectID.split(":");
                    var replaceText = selectID[1];
                    selectID = "#" + selectID[1];

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
                    for (var itemtxt in ReplaceTblobj) {
                        simpleProducts.push(ReplaceTblobj[itemtxt]);
                    }

                    $(selectID).dxSelectBox({
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
        }
    });
}

//Save Dynamic Data
$("#BtnSave").click(function () {
    var xtraField = [];
    UpdatedItemName = [];
    var columnlength = "";
    var GetChar = "";
    var MasterName = document.getElementById("MasterName").innerHTML;
    var masterID = document.getElementById("MasterID").innerHTML;
    $.ajax({
        async: false,
        type: "POST",
        url: "WebService_Master.asmx/Master",
        data: '{masterID:' + JSON.stringify(masterID) + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            var res = results.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/""/g, '');
            res = res.replace(/:}/g, ':null}');
            res = res.replace(/:,/g, ':null,');
            res = res.replace(/u0026/g, '&');
            res = res.replace(/u0027u0027/g, "''");
            res = res.substr(1);
            res = res.slice(0, -1);
            columnlength = "";
            columnlength = JSON.parse(res);

            var DSValue = ""; var INValue = ""; var alertTag = ""; var x = "";
            if (columnlength.length > 0) {
                for (var i = 0; i < columnlength.length; i++) {

                    var DataTypeVali = columnlength[i].FieldDataType;
                    DataTypeVali = DataTypeVali.substring(0, 3);
                    DataTypeVali = DataTypeVali.toUpperCase().trim();
                    var decimal = /^[0-9]+\.?[0-9]*$/;
                    if (columnlength[i].IsDisplay === true) {
                        if (columnlength[i].FieldType === "text" || columnlength[i].FieldType === "textarea") {
                            x = document.getElementById(columnlength[i].FieldName).value;

                            if (DataTypeVali === "MON" || DataTypeVali === "FLO" || DataTypeVali === "REA") {
                                if (x === undefined || x === null || x === "") {
                                    document.getElementById(columnlength[i].FieldName).value = 0;
                                    x = 0;
                                }
                                if (decimal.test(x) === true) {
                                    alertTag = "ValStr" + columnlength[i].FieldName;
                                    document.getElementById(alertTag).style.display = "none";
                                }
                                else {
                                    alert("Please enter Numeric OR Decimal value in " + columnlength[i].FieldDisplayName);
                                    alertTag = "ValStr" + columnlength[i].FieldName;
                                    //document.getElementById(columnlength[i].FieldName).value = "";
                                    document.getElementById(columnlength[i].FieldName).focus();
                                    document.getElementById(alertTag).style.fontSize = "10px";
                                    document.getElementById(alertTag).style.display = "block";
                                    document.getElementById(alertTag).innerHTML = 'Please enter  Numeric OR Decimal value in ' + columnlength[i].FieldDisplayName;
                                    return false;
                                }
                            }
                            if (DataTypeVali === "INT" || DataTypeVali === "BIG") {
                                if (isNaN(x)) {
                                    alert("Please enter numeric in " + columnlength[i].FieldDisplayName);
                                    alertTag = "ValStr" + columnlength[i].FieldName;
                                    // document.getElementById(columnlength[i].FieldName).value = "";
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
                            x = $("#" + columnlength[i].FieldName).dxSelectBox("instance").option('value');
                            if (DataTypeVali === "MON" || DataTypeVali === "FLO" || DataTypeVali === "REA") {
                                if (x === undefined || x === null || x === "") {
                                    $("#" + columnlength[i].FieldName).dxSelectBox({
                                        value: 0
                                    });
                                    x = 0;
                                }
                                if (decimal.test(x) === true) {
                                    alertTag = "ValStr" + columnlength[i].FieldName;
                                    document.getElementById(alertTag).style.display = "none";
                                }
                                else {
                                    alert("Please enter Numeric OR Decimal value in " + columnlength[i].FieldDisplayName);
                                    alertTag = "ValStr" + columnlength[i].FieldName;
                                    //$("#" + columnlength[i].FieldName).dxSelectBox({
                                    //    value: "",
                                    //});
                                    document.getElementById(alertTag).style.fontSize = "10px";
                                    document.getElementById(alertTag).style.display = "block";
                                    document.getElementById(alertTag).innerHTML = 'Please enter Numeric OR Decimal value in ' + columnlength[i].FieldDisplayName;
                                    return false;
                                }
                            }
                            if (DataTypeVali === "INT" || DataTypeVali === "BIG") {
                                if (x === undefined || x === null || x === "") {
                                    $("#" + columnlength[i].FieldName).dxSelectBox({
                                        value: 0
                                    });
                                    x = 0;
                                }
                                if (isNaN(x)) {
                                    alert("Please enter numeric in " + columnlength[i].FieldDisplayName);
                                    alertTag = "ValStr" + columnlength[i].FieldName;
                                    //$("#" + columnlength[i].FieldName).dxSelectBox({
                                    //    value: "",
                                    //});
                                    document.getElementById(alertTag).style.fontSize = "10px";
                                    document.getElementById(alertTag).style.display = "block";
                                    document.getElementById(alertTag).innerHTML = 'Please enter numeric in ' + columnlength[i].FieldDisplayName;
                                    return false;
                                }
                                alertTag = "ValStr" + columnlength[i].FieldName;
                                document.getElementById(alertTag).style.display = "none";
                            }
                        }
                    }
                    if (columnlength[i].IsDisplay === true && columnlength[i].IsRequiredFieldValidator === true) {
                        let fieldValue = 0;
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

                                if (DataTypeVali === "INT" || DataTypeVali === "BIG" || DataTypeVali === "MON" || DataTypeVali === "FLO" || DataTypeVali === "REA") {
                                    fieldValue = Number(document.getElementById(columnlength[i].FieldName).value);
                                    if (Number(fieldValue) === 0) {
                                        alert("Please enter.." + columnlength[i].FieldDisplayName);
                                        alertTag = "ValStr" + columnlength[i].FieldName;
                                        document.getElementById(columnlength[i].FieldName).focus();
                                        document.getElementById(alertTag).style.fontSize = "10px";
                                        document.getElementById(alertTag).style.display = "block";
                                        document.getElementById(alertTag).innerHTML = 'Invalid input..' + columnlength[i].FieldDisplayName;
                                        return false;
                                    }
                                }
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
                                if (DataTypeVali === "INT" || DataTypeVali === "BIG" || DataTypeVali === "MON" || DataTypeVali === "FLO" || DataTypeVali === "REA") {
                                    fieldValue = Number(document.getElementById(columnlength[i].FieldName).value);
                                    if (Number(fieldValue) === 0) {
                                        alert("Please enter valid value for field " + columnlength[i].FieldDisplayName);
                                        alertTag = "ValStr" + columnlength[i].FieldName;
                                        document.getElementById(alertTag).style.fontSize = "10px";
                                        document.getElementById(alertTag).style.display = "block";
                                        document.getElementById(alertTag).innerHTML = 'Invalid input..' + columnlength[i].FieldDisplayName;
                                        return false;
                                    }
                                }
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
                                if (DataTypeVali === "INT" || DataTypeVali === "BIG" || DataTypeVali === "MON" || DataTypeVali === "FLO" || DataTypeVali === "REA") {
                                    fieldValue = Number($("#" + columnlength[i].FieldName).dxSelectBox("instance").option('value'));
                                    if (Number(fieldValue) === 0) {
                                        alert("Please enter valid value for field " + columnlength[i].FieldDisplayName);
                                        alertTag = "ValStr" + columnlength[i].FieldName;
                                        document.getElementById(alertTag).style.fontSize = "10px";
                                        document.getElementById(alertTag).style.display = "block";
                                        document.getElementById(alertTag).innerHTML = 'Invalid input..' + columnlength[i].FieldDisplayName;
                                        return false;
                                    }
                                }
                                alertTag = "ValStr" + columnlength[i].FieldName;
                                document.getElementById(alertTag).style.display = "none";
                            }
                        }
                    }
                }

                var jsonObjectsItemMasterRecord = [];
                var OperationItemMasterRecord = {};

                // OperationItemMasterRecord.ItemName = document.getElementById("MasterName").innerHTML.trim();
                OperationItemMasterRecord.ItemType = document.getElementById("MasterName").innerHTML.trim();
                OperationItemMasterRecord.ItemGroupID = document.getElementById("MasterID").innerHTML.trim();

                //jsonObjectsItemMasterRecord.push(OperationItemMasterRecord);

                var jsonObjectsItemMasterDetailRecordClon = [];

                var jsonObjectsItemMasterDetailRecord = [];
                var OperationItemMasterDetailRecord = {};

                var OptItemMasterDetailClonRecord = {};
                var OptxtraField = {};

                for (var j = 0; j < columnlength.length; j++) {

                    OperationItemMasterDetailRecord = {};

                    OptItemMasterDetailClonRecord = {};

                    OperationItemMasterDetailRecord.FieldName = columnlength[j].FieldName.trim();
                    OperationItemMasterDetailRecord.ParentFieldName = columnlength[j].FieldName.trim();

                    //CloneCopy
                    OptItemMasterDetailClonRecord.FieldName = columnlength[j].FieldName.trim();
                    OptItemMasterDetailClonRecord.ParentFieldName = columnlength[j].FieldName.trim();

                    if (columnlength[j].FieldType === "text" || columnlength[j].FieldType === "number") {
                        OperationItemMasterDetailRecord.ParentFieldValue = document.getElementById(columnlength[j].FieldName).value.trim().replace(/['"]/g, '');
                        OperationItemMasterDetailRecord.FieldValue = document.getElementById(columnlength[j].FieldName).value.trim().replace(/['"]/g, '');

                        //CloneCopy
                        OptItemMasterDetailClonRecord.ParentFieldValue = document.getElementById(columnlength[j].FieldName).value.trim().replace(/['"]/g, '');
                        OptItemMasterDetailClonRecord.FieldValue = document.getElementById(columnlength[j].FieldName).value.trim().replace(/['"]/g, '');

                        //XtraFieldAdd For ItemMaster

                        OptxtraField = {};
                        if (columnlength[j].FieldName.toUpperCase() === "STOCKUNIT") {
                            OptxtraField = {};
                            OptxtraField.StockUnit = document.getElementById(columnlength[j].FieldName).value.replace(/['"]/g, '');
                            xtraField.push(OptxtraField);
                        }
                        if (columnlength[j].FieldName.toUpperCase() === "PURCHASEUNIT") {
                            OptxtraField = {};
                            OptxtraField.PurchaseUnit = document.getElementById(columnlength[j].FieldName).value.replace(/['"]/g, '');
                            xtraField.push(OptxtraField);
                        }
                        if (columnlength[j].FieldName.toUpperCase() === "ESTIMATIONUNIT") {
                            OptxtraField = {};
                            OptxtraField.EstimationUnit = document.getElementById(columnlength[j].FieldName).value.replace(/['"]/g, '');
                            xtraField.push(OptxtraField);
                        }
                        if (columnlength[j].FieldName.toUpperCase() === "UNITPERPACKING") {
                            OptxtraField = {};

                            OptxtraField.UnitPerPacking = document.getElementById(columnlength[j].FieldName).value.replace(/['"]/g, '');
                            xtraField.push(OptxtraField);

                        }
                        if (columnlength[j].FieldName.toUpperCase() === "WTPERPACKING") {
                            OptxtraField = {};

                            OptxtraField.WtPerPacking = document.getElementById(columnlength[j].FieldName).value.replace(/['"]/g, '');
                            xtraField.push(OptxtraField);

                        }
                        if (columnlength[j].FieldName.toUpperCase() === "CONVERSIONFACTOR") {
                            OptxtraField = {};

                            OptxtraField.ConversionFactor = document.getElementById(columnlength[j].FieldName).value.replace(/['"]/g, '');
                            xtraField.push(OptxtraField);

                        }
                        if (columnlength[j].FieldName.toUpperCase() === "ITEMSUBGROUPID") {
                            OptxtraField = {};
                            OptxtraField.ItemSubGroupID = document.getElementById(columnlength[j].FieldName).value.replace(/['"]/g, '');
                            xtraField.push(OptxtraField);

                        }
                        if (columnlength[j].FieldName.toUpperCase() === "PRODUCTHSNID") {
                            OptxtraField = {};
                            OptxtraField.ProductHSNID = document.getElementById(columnlength[j].FieldName).value.replace(/['"]/g, '');
                            xtraField.push(OptxtraField);
                        }

                        //Again Next Add Field 30 Apr 2019
                        if (columnlength[j].FieldName.toUpperCase() === "STOCKTYPE") {
                            OptxtraField = {};

                            OptxtraField.StockType = document.getElementById(columnlength[j].FieldName).value.replace(/['"]/g, '');
                            xtraField.push(OptxtraField);
                        }
                        if (columnlength[j].FieldName.toUpperCase() === "STOCKCATEGORY") {
                            OptxtraField = {};

                            OptxtraField.StockCategory = document.getElementById(columnlength[j].FieldName).value.replace(/['"]/g, '');
                            xtraField.push(OptxtraField);
                        }
                        if (columnlength[j].FieldName.toUpperCase() === "SIZEW") {
                            OptxtraField = {};

                            OptxtraField.SizeW = document.getElementById(columnlength[j].FieldName).value.replace(/['"]/g, '');
                            xtraField.push(OptxtraField);
                        }
                        if (columnlength[j].FieldName.toUpperCase() === "PURCHASERATE") {
                            OptxtraField = {};

                            OptxtraField.PurchaseRate = document.getElementById(columnlength[j].FieldName).value.replace(/['"]/g, '');
                            xtraField.push(OptxtraField);
                        }
                        if (columnlength[j].FieldName.toUpperCase() === "ESTIMATIONRATE") {
                            OptxtraField = {};

                            OptxtraField.EstimationRate = document.getElementById(columnlength[j].FieldName).value.replace(/['"]/g, '');
                            xtraField.push(OptxtraField);
                        }
                        if (columnlength[j].FieldName.toUpperCase() === "BF") {
                            OptxtraField = {};

                            OptxtraField.BF = $("#" + columnlength[j].FieldName).dxSelectBox("instance").option('value');
                            xtraField.push(OptxtraField);
                        }
                    }

                    if (columnlength[j].FieldType === "textarea") {
                        OperationItemMasterDetailRecord.ParentFieldValue = document.getElementById(columnlength[j].FieldName).value.trim();
                        OperationItemMasterDetailRecord.FieldValue = document.getElementById(columnlength[j].FieldName).value.trim();

                        //CloneCopy
                        OptItemMasterDetailClonRecord.ParentFieldValue = document.getElementById(columnlength[j].FieldName).value.trim();
                        OptItemMasterDetailClonRecord.FieldValue = document.getElementById(columnlength[j].FieldName).value.trim();

                        //XtraFieldAdd For ItemMaster
                        OptxtraField = {};
                        if (columnlength[j].FieldName.toUpperCase() === "STOCKUNIT") {
                            OptxtraField = {};
                            OptxtraField.StockUnit = document.getElementById(columnlength[j].FieldName).value.replace(/['"]/g, '');
                            xtraField.push(OptxtraField);
                        }
                        if (columnlength[j].FieldName.toUpperCase() === "PURCHASEUNIT") {
                            OptxtraField = {};
                            OptxtraField.PurchaseUnit = document.getElementById(columnlength[j].FieldName).value.replace(/['"]/g, '');
                            xtraField.push(OptxtraField);
                        }
                        if (columnlength[j].FieldName.toUpperCase() === "ESTIMATIONUNIT") {
                            OptxtraField = {};
                            OptxtraField.EstimationUnit = document.getElementById(columnlength[j].FieldName).value.replace(/['"]/g, '');
                            xtraField.push(OptxtraField);
                        }
                        if (columnlength[j].FieldName.toUpperCase() === "UNITPERPACKING") {
                            OptxtraField = {};

                            OptxtraField.UnitPerPacking = document.getElementById(columnlength[j].FieldName).value.replace(/['"]/g, '');
                            xtraField.push(OptxtraField);

                        }
                        if (columnlength[j].FieldName.toUpperCase() === "WTPERPACKING") {
                            OptxtraField = {};

                            OptxtraField.WtPerPacking = document.getElementById(columnlength[j].FieldName).value.replace(/['"]/g, '');
                            xtraField.push(OptxtraField);

                        }
                        if (columnlength[j].FieldName.toUpperCase() === "CONVERSIONFACTOR") {
                            OptxtraField = {};

                            OptxtraField.ConversionFactor = document.getElementById(columnlength[j].FieldName).value.replace(/['"]/g, '');
                            xtraField.push(OptxtraField);

                        }
                        if (columnlength[j].FieldName.toUpperCase() === "ITEMSUBGROUPID") {
                            OptxtraField = {};
                            OptxtraField.ItemSubGroupID = document.getElementById(columnlength[j].FieldName).value.replace(/['"]/g, '');
                            xtraField.push(OptxtraField);

                        }
                        if (columnlength[j].FieldName.toUpperCase() === "PRODUCTHSNID") {
                            OptxtraField = {};
                            OptxtraField.ProductHSNID = document.getElementById(columnlength[j].FieldName).value.replace(/['"]/g, '');
                            xtraField.push(OptxtraField);
                        }
                        //Again Next Add Field 30 Apr 2019
                        if (columnlength[j].FieldName.toUpperCase() === "STOCKTYPE") {
                            OptxtraField = {};

                            OptxtraField.StockType = document.getElementById(columnlength[j].FieldName).value.replace(/['"]/g, '');
                            xtraField.push(OptxtraField);
                        }
                        if (columnlength[j].FieldName.toUpperCase() === "STOCKCATEGORY") {
                            OptxtraField = {};

                            OptxtraField.StockCategory = document.getElementById(columnlength[j].FieldName).value.replace(/['"]/g, '');
                            xtraField.push(OptxtraField);
                        }
                        if (columnlength[j].FieldName.toUpperCase() === "SIZEW") {
                            OptxtraField = {};

                            OptxtraField.SizeW = document.getElementById(columnlength[j].FieldName).value.replace(/['"]/g, '');
                            xtraField.push(OptxtraField);
                        }
                        if (columnlength[j].FieldName.toUpperCase() === "PURCHASERATE") {
                            OptxtraField = {};

                            OptxtraField.PurchaseRate = document.getElementById(columnlength[j].FieldName).value.replace(/['"]/g, '');
                            xtraField.push(OptxtraField);
                        }
                        if (columnlength[j].FieldName.toUpperCase() === "ESTIMATIONRATE") {
                            OptxtraField = {};

                            OptxtraField.EstimationRate = document.getElementById(columnlength[j].FieldName).value.replace(/['"]/g, '');
                            xtraField.push(OptxtraField);
                        }
                        if (columnlength[j].FieldName.toUpperCase() === "BF") {
                            OptxtraField = {};

                            OptxtraField.BF = $("#" + columnlength[j].FieldName).dxSelectBox("instance").option('value');
                            xtraField.push(OptxtraField);
                        }

                    }

                    if (columnlength[j].FieldType === "datebox") {
                        OperationItemMasterDetailRecord.ParentFieldValue = $("#" + columnlength[j].FieldName).dxDateBox("instance").option('value');
                        OperationItemMasterDetailRecord.FieldValue = $("#" + columnlength[j].FieldName).dxDateBox("instance").option('value');

                        //CloneCopy
                        OptItemMasterDetailClonRecord.ParentFieldValue = $("#" + columnlength[j].FieldName).dxDateBox("instance").option('value');
                        OptItemMasterDetailClonRecord.FieldValue = $("#" + columnlength[j].FieldName).dxDateBox("instance").option('value');

                    }

                    if (columnlength[j].FieldType === "selectbox") {
                        OperationItemMasterDetailRecord.ParentItemID = 0;

                        //CloneCopy
                        OptItemMasterDetailClonRecord.ParentItemID = 0;

                        var pval = $("#" + columnlength[j].FieldName).dxSelectBox("instance").option('value');
                        if (pval !== "" && pval !== "null" && pval !== null && pval !== undefined) {
                            if (isNaN(pval)) {
                                pval = pval.trim();
                            }
                        }
                        OperationItemMasterDetailRecord.ParentFieldValue = pval;
                        OperationItemMasterDetailRecord.FieldValue = pval;//$("#" + columnlength[j].FieldName).dxSelectBox("instance").option('value');

                        //CloneCopy
                        OptItemMasterDetailClonRecord.ParentFieldValue = pval;
                        OptItemMasterDetailClonRecord.FieldValue = pval;//$("#" + columnlength[j].FieldName).dxSelectBox("instance").option('value');

                        //XtraFieldAdd For ItemMaster
                        OptxtraField = {};
                        if (columnlength[j].FieldName.toUpperCase() === "STOCKUNIT") {
                            OptxtraField = {};
                            OptxtraField.StockUnit = $("#" + columnlength[j].FieldName).dxSelectBox("instance").option('value');
                            xtraField.push(OptxtraField);
                        }
                        if (columnlength[j].FieldName.toUpperCase() === "PURCHASEUNIT") {
                            OptxtraField = {};
                            OptxtraField.PurchaseUnit = $("#" + columnlength[j].FieldName).dxSelectBox("instance").option('value');
                            xtraField.push(OptxtraField);
                        }
                        if (columnlength[j].FieldName.toUpperCase() === "ESTIMATIONUNIT") {
                            OptxtraField = {};
                            OptxtraField.EstimationUnit = $("#" + columnlength[j].FieldName).dxSelectBox("instance").option('value');
                            xtraField.push(OptxtraField);
                        }
                        if (columnlength[j].FieldName.toUpperCase() === "UNITPERPACKING") {
                            OptxtraField = {};
                            OptxtraField.UnitPerPacking = $("#" + columnlength[j].FieldName).dxSelectBox("instance").option('value');
                            xtraField.push(OptxtraField);
                        }
                        if (columnlength[j].FieldName.toUpperCase() === "WTPERPACKING") {
                            OptxtraField = {};
                            OptxtraField.WtPerPacking = $("#" + columnlength[j].FieldName).dxSelectBox("instance").option('value');
                            xtraField.push(OptxtraField);
                        }
                        if (columnlength[j].FieldName.toUpperCase() === "CONVERSIONFACTOR") {
                            OptxtraField = {};

                            OptxtraField.ConversionFactor = $("#" + columnlength[j].FieldName).dxSelectBox("instance").option('value');
                            xtraField.push(OptxtraField);
                        }
                        if (columnlength[j].FieldName.toUpperCase() === "ITEMSUBGROUPID") {
                            OptxtraField = {};

                            OptxtraField.ItemSubGroupID = $("#" + columnlength[j].FieldName).dxSelectBox("instance").option('value');
                            xtraField.push(OptxtraField);
                        }
                        if (columnlength[j].FieldName.toUpperCase() === "PRODUCTHSNID") {
                            OptxtraField = {};

                            OptxtraField.ProductHSNID = $("#" + columnlength[j].FieldName).dxSelectBox("instance").option('value');
                            xtraField.push(OptxtraField);
                        }

                        //Again Next Add Field 30 Apr 2019
                        if (columnlength[j].FieldName.toUpperCase() === "STOCKTYPE") {
                            OptxtraField = {};

                            OptxtraField.StockType = $("#" + columnlength[j].FieldName).dxSelectBox("instance").option('value');
                            xtraField.push(OptxtraField);
                        }
                        if (columnlength[j].FieldName.toUpperCase() === "STOCKCATEGORY") {
                            OptxtraField = {};

                            OptxtraField.StockCategory = $("#" + columnlength[j].FieldName).dxSelectBox("instance").option('value');
                            xtraField.push(OptxtraField);
                        }
                        if (columnlength[j].FieldName.toUpperCase() === "SIZEW") {
                            OptxtraField = {};

                            OptxtraField.SizeW = $("#" + columnlength[j].FieldName).dxSelectBox("instance").option('value');
                            xtraField.push(OptxtraField);
                        }
                        if (columnlength[j].FieldName.toUpperCase() === "PURCHASERATE") {
                            OptxtraField = {};

                            OptxtraField.PurchaseRate = $("#" + columnlength[j].FieldName).dxSelectBox("instance").option('value');
                            xtraField.push(OptxtraField);
                        }
                        if (columnlength[j].FieldName.toUpperCase() === "ESTIMATIONRATE") {
                            OptxtraField = {};

                            OptxtraField.EstimationRate = $("#" + columnlength[j].FieldName).dxSelectBox("instance").option('value');
                            xtraField.push(OptxtraField);
                        }
                        if (columnlength[j].FieldName.toUpperCase() === "BF") {
                            OptxtraField = {};

                            OptxtraField.BF = $("#" + columnlength[j].FieldName).dxSelectBox("instance").option('value');
                            xtraField.push(OptxtraField);
                        }


                    }

                    if (columnlength[j].FieldType === "checkbox") {
                        OperationItemMasterDetailRecord.ParentFieldValue = document.getElementById(columnlength[j].FieldName).checked;
                        OperationItemMasterDetailRecord.FieldValue = document.getElementById(columnlength[j].FieldName).checked;

                        //CloneCopy
                        OptItemMasterDetailClonRecord.ParentFieldValue = document.getElementById(columnlength[j].FieldName).checked;
                        OptItemMasterDetailClonRecord.FieldValue = document.getElementById(columnlength[j].FieldName).checked;

                    }

                    OperationItemMasterDetailRecord.SequenceNo = j + 1;
                    OperationItemMasterDetailRecord.ItemGroupID = document.getElementById("MasterID").innerHTML.trim();

                    //CloneCopy
                    OptItemMasterDetailClonRecord.SequenceNo = j + 1;
                    OptItemMasterDetailClonRecord.ItemGroupID = document.getElementById("MasterID").innerHTML.trim();


                    GetChar = "";
                    if (columnlength[j].UnitMeasurement === "" || columnlength[j].UnitMeasurement === null || columnlength[j].UnitMeasurement === "null" || columnlength[j].UnitMeasurement === undefined) {
                        GetChar = "";
                    }
                    else {
                        GetChar = columnlength[j].UnitMeasurement;
                    }

                    jsonObjectsItemMasterDetailRecord.push(OperationItemMasterDetailRecord);

                    //CloneCopy
                    jsonObjectsItemMasterDetailRecordClon.push(OptItemMasterDetailClonRecord);
                    OptItemMasterDetailClonRecord.UnitMeasurement = GetChar;
                    jsonObjectsItemMasterDetailRecordClon.push(OptItemMasterDetailClonRecord);
                }

                FilterObj = [];
                FilterObj = { 'GetFilterData': jsonObjectsItemMasterDetailRecordClon };

                if (GblItemNameString !== "" && GblItemNameString !== undefined && GblItemNameString !== "null" && GblItemNameString !== null) {
                    var strIN = GblItemNameString;
                    strIN = strIN.split(",");
                    INValue = "";
                    var optIN = {};
                    for (var INS in strIN) {
                        var INobj = strIN[INS];
                        newArrayFilterObj = [];
                        newArrayFilterObj = FilterObj.GetFilterData.filter(function (el) {
                            return el.FieldName === INobj;
                        });

                        if (INValue === "") {
                            if (newArrayFilterObj.length > 0) {
                                if (newArrayFilterObj[0].UnitMeasurement === "") {
                                    if (INobj == "GSM") {
                                        INValue = (Number(newArrayFilterObj[0].FieldValue) > 0) ? (newArrayFilterObj[0].FieldValue + " GSM") : "";
                                    }
                                    else if (INobj == "ItemSize") {
                                        INValue = ((newArrayFilterObj[0].FieldValue).trim() !== "") ? (newArrayFilterObj[0].FieldValue + " MM") : "";
                                    }
                                    else {
                                        INValue = (newArrayFilterObj[0].FieldValue !== undefined && newArrayFilterObj[0].FieldValue !== null && newArrayFilterObj[0].FieldValue !== 0 && newArrayFilterObj[0].FieldValue !== "" && newArrayFilterObj[0].FieldValue !== "-") ? newArrayFilterObj[0].FieldValue : "";
                                    }

                                }
                                else {
                                    INValue = (newArrayFilterObj[0].FieldValue !== undefined && newArrayFilterObj[0].FieldValue !== null && newArrayFilterObj[0].FieldValue !== 0 && newArrayFilterObj[0].FieldValue !== "" && newArrayFilterObj[0].FieldValue !== "-") ? newArrayFilterObj[0].FieldValue + " " + newArrayFilterObj[0].UnitMeasurement : "";
                                }

                            }

                        } else {
                            if (newArrayFilterObj.length > 0) {
                                if (newArrayFilterObj[0].UnitMeasurement === "") {
                                    if (INobj == "GSM") {
                                        INValue = INValue + ((Number(newArrayFilterObj[0].FieldValue) > 0) ? (", " + newArrayFilterObj[0].FieldValue + " GSM") : "");
                                    }
                                    else if (INobj == "ItemSize") {
                                        INValue = INValue + (((newArrayFilterObj[0].FieldValue).trim() !== "") ? (", " + newArrayFilterObj[0].FieldValue + " MM") : "");
                                    }
                                    else {
                                        INValue = INValue + ((newArrayFilterObj[0].FieldValue !== undefined && newArrayFilterObj[0].FieldValue !== null && newArrayFilterObj[0].FieldValue !== 0 && newArrayFilterObj[0].FieldValue !== "" && newArrayFilterObj[0].FieldValue !== "-") ? (", " + newArrayFilterObj[0].FieldValue) : "");
                                    }

                                }

                                else {
                                    INValue = INValue + ((newArrayFilterObj[0].FieldValue !== undefined && newArrayFilterObj[0].FieldValue !== null && newArrayFilterObj[0].FieldValue !== 0 && newArrayFilterObj[0].FieldValue !== "" && newArrayFilterObj[0].FieldValue !== "-") ? (", " + newArrayFilterObj[0].FieldValue + " " + newArrayFilterObj[0].UnitMeasurement) : "");
                                }
                            }
                        }
                    }
                    //optIN = {};
                    //optIN.ItemName = INValue;

                    //OperationItemMasterRecord.ItemName = INValue;
                    //jsonObjectsItemMasterRecord.push(OperationItemMasterRecord);
                }

                if (GblItemDecString !== "" && GblItemDecString !== undefined && GblItemDecString !== "null" && GblItemDecString !== null) {
                    //Quality:COLOUR BOARD,GSM:250,Manufecturer:JAI VIJAYA,Finish:BEIGE
                    var strDec = GblItemDecString;
                    strDec = strDec.split(",");
                    DSValue = "";
                    var optDS = {};
                    for (var DSS in strDec) {
                        var DSbj = strDec[DSS];
                        newArrayFilterObj = [];
                        newArrayFilterObj = FilterObj.GetFilterData.filter(function (el) {
                            return el.FieldName === DSbj;
                        });
                        if (newArrayFilterObj.length > 0) {
                            if (DSValue === "") {
                                DSValue = newArrayFilterObj[0].FieldName + ":" + newArrayFilterObj[0].FieldValue;
                            } else {
                                DSValue = DSValue + ", " + newArrayFilterObj[0].FieldName + ":" + newArrayFilterObj[0].FieldValue;
                            }
                        }
                    }
                    //optDS = {};
                    //optDS.ItemDescription = DSValue;

                }
                var ItemName = document.getElementById("TxtItemName").value.trim();
                //UpdatedItemName = ItemName;
                var TallyItemName = document.getElementById("TxtTallyItemName").value.trim();
                //if (ItemName !== undefined && ItemName !== "") INValue = ItemName; '' Anshu 
                OperationItemMasterRecord.ItemName = INValue;
                OperationItemMasterRecord.ItemDescription = DSValue;
                OperationItemMasterRecord.TallyItemName = TallyItemName;

                var ContainerXtrafield = JSON.stringify(xtraField);

                ContainerXtrafield = ContainerXtrafield.replace(/{/g, '');
                ContainerXtrafield = ContainerXtrafield.replace(/}/g, '');
                ContainerXtrafield = ContainerXtrafield.substr(1);
                ContainerXtrafield = ContainerXtrafield.slice(0, -1);
                ContainerXtrafield = ContainerXtrafield.replace(/,{2,}/g, ','); // Remove extra commas
                var tt = [];
                var xx = "{" + ContainerXtrafield + "}";
                tt.push(xx);
                xtraField = JSON.parse(tt);

                var varUnitPerPacking = 1, varWtPerPacking = 0, varConversionFactor = 1, varItemSubGroupID = 0, varProductHSNID = 0;
                var varStockType = "", varStockCategory = "", varSizeW = 0, varPurchaseRate = 0;
                if (xtraField.UnitPerPacking !== "") {
                    varUnitPerPacking = xtraField.UnitPerPacking;
                }
                if (xtraField.WtPerPacking !== "") {
                    varWtPerPacking = xtraField.WtPerPacking;
                }
                if (xtraField.ConversionFactor !== "") {
                    varConversionFactor = xtraField.ConversionFactor;
                }
                if (xtraField.ItemSubGroupID !== "") {
                    varItemSubGroupID = xtraField.ItemSubGroupID;
                }
                if (xtraField.ProductHSNID !== "") {
                    varProductHSNID = xtraField.ProductHSNID;
                }

                if (xtraField.StockType !== "") {
                    varStockType = xtraField.StockType;
                }
                if (xtraField.StockCategory !== "") {
                    varStockCategory = xtraField.StockCategory;
                }
                if (xtraField.SizeW !== "") {
                    varSizeW = xtraField.SizeW;
                }
                if (xtraField.PurchaseRate !== "") {
                    varPurchaseRate = xtraField.PurchaseRate;
                }

                if (xtraField.BF !== "") {
                    varBF = xtraField.BF;
                }

                OperationItemMasterRecord.StockUnit = xtraField.StockUnit; //Correction 01 Apr 2019
                OperationItemMasterRecord.PurchaseUnit = xtraField.PurchaseUnit;//Correction 01 Apr 2019
                OperationItemMasterRecord.EstimationUnit = xtraField.EstimationUnit; //Correction 01 Apr 2019
                OperationItemMasterRecord.WtPerPacking = varWtPerPacking;//Correction 01 Apr 2019
                OperationItemMasterRecord.UnitPerPacking = varUnitPerPacking; //Correction 01 Apr 2019
                if (varConversionFactor === undefined || varConversionFactor === "undefined") varConversionFactor = "";
                OperationItemMasterRecord.ConversionFactor = varConversionFactor;//Correction 01 Apr 2019
                OperationItemMasterRecord.ItemSubGroupID = varItemSubGroupID; //Correction 01 Apr 2019
                OperationItemMasterRecord.ProductHSNID = varProductHSNID;//Correction 01 Apr 2019

                OperationItemMasterRecord.StockType = varStockType;//Correction 30 Apr 2019
                OperationItemMasterRecord.StockCategory = varStockCategory;//Correction 30 Apr 2019
                OperationItemMasterRecord.SizeW = varSizeW;//Correction 30 Apr 2019
                OperationItemMasterRecord.PurchaseRate = varPurchaseRate;//Correction 30 Apr 2019
                OperationItemMasterRecord.BF = varBF;

                jsonObjectsItemMasterRecord.push(OperationItemMasterRecord);

                //alert(JSON.stringify(jsonObjectsItemMasterDetailRecord));

                var CostingDataItemDetailMaster = JSON.stringify(jsonObjectsItemMasterDetailRecord);

                var CostingDataItemMaster = JSON.stringify(jsonObjectsItemMasterRecord);
                updateColumnBasedOnGroup = jsonObjectsItemMasterDetailRecord;

                let UpdatedItemNameObj = {};
                //UpdatedItemNameObj.ItemName = INValue;
                UpdatedItemNameObj.PurchaseRate = xtraField.PurchaseRate;
                UpdatedItemNameObj.EstimationRate = xtraField.EstimationRate;
                UpdatedItemNameObj.ProductHSNID = xtraField.ProductHSNID;
                UpdatedItemNameObj.StockRefCode = document.getElementById("StockRefCode").value;
                UpdatedItemNameObj.PurchaseOrderQuantity = document.getElementById("PurchaseOrderQuantity").value;
                UpdatedItemNameObj.MinimumStockQty = document.getElementById("MinimumStockQty").value;

                UpdatedItemName.push(UpdatedItemNameObj)
                var stockrefcode = document.getElementById("StockRefCode").value;
                var txt = 'If you confident please click on \n' + 'Yes, Save it ! \n' + 'otherwise click on \n' + 'Cancel';
                swal({
                    title: "Do you want to continue",
                    text: txt,
                    type: "warning",
                    showCancelButton: true,
                    confirmButtonColor: "#DD6B55",
                    confirmButtonText: "Yes, Save it !",
                    showLoaderOnConfirm: true
                },
                    function () {
                        if (GblStatus === "Update") {
                            document.getElementById("LOADER").style.display = "block";
                            if (updateGroupFlag == 'true') {
                                updateColumnOnGroup(UpdatedItemName);
                            } else {


                                $.ajax({
                                    async: false,
                                    type: "POST",
                                    url: "WebService_Master.asmx/UpdateData",
                                    data: '{CostingDataItemMaster:' + CostingDataItemMaster + ',CostingDataItemDetailMaster:' + CostingDataItemDetailMaster + ',MasterName:' + JSON.stringify(MasterName) + ',ItemID:' + JSON.stringify(document.getElementById("txtGetGridRow").value) + ',UnderGroupID:' + JSON.stringify(UnderGroupID) + ',ActiveItem:' + JSON.stringify(document.getElementById("IsactivItemstatic").checked) + ',StockRefCode:' + JSON.stringify(stockrefcode) + '}',
                                    contentType: "application/json; charset=utf-8",
                                    dataType: "json",
                                    success: function (results) {
                                        document.getElementById("LOADER").style.display = "none";
                                        let Title, Text, Type;
                                        let IsSuccess = false;
                                        if (results.d.includes("Success")) {
                                            Title = "Updated...";
                                            Text = "Your data Updated...";
                                            Type = "success";
                                            //swal("Updated!", "Your data Updated", "success");
                                            //FillGrid();
                                            //$("#largeModal").modal('hide');
                                            IsSuccess = true;

                                        } else {
                                            //swal("Can't Update!", results.d, "warning");
                                            Title = "Can't Update!";
                                            Text = results.d;
                                            Type = "warning";
                                            IsSuccess = false;
                                        }
                                        setTimeout(function () {
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
                                        }, 100);

                                    },
                                    error: function errorFunc(jqXHR) {
                                        document.getElementById("LOADER").style.display = "none";
                                        swal("Error!", "Please try after some time..", "error");
                                        console.log(jqXHR);
                                    }
                                });
                            }
                        } else {
                            document.getElementById("LOADER").style.display = "block";

                            $.ajax({
                                async: false,
                                type: "POST",
                                url: "WebService_Master.asmx/SaveData",
                                data: '{CostingDataItemMaster:' + CostingDataItemMaster + ',CostingDataItemDetailMaster:' + CostingDataItemDetailMaster + ',MasterName:' + JSON.stringify(MasterName) + ',ItemGroupID:' + JSON.stringify(document.getElementById("MasterID").innerHTML.trim()) + ',ActiveItem:' + JSON.stringify(document.getElementById("IsactivItemstatic").checked) + ',StockRefCode:' + JSON.stringify(stockrefcode) + '}',
                                contentType: "application/json; charset=utf-8",
                                dataType: "json",
                                success: function (results) {
                                    document.getElementById("LOADER").style.display = "none";
                                    let Title, Text, Type;
                                    let IsSuccess = false;
                                    if (results.d === "Success") {
                                        //swal("Saved!", "Your data Saved", "success");
                                        //FillGrid();
                                        //$("#largeModal").modal('hide');
                                        isSaveASClicked = false;
                                        Title = "Saved!";
                                        Text = "Your data Saved...";
                                        Type = "success";
                                        IsSuccess = true;
                                    } else {
                                        if (isSaveASClicked) {
                                            GblStatus = "Update";
                                            isSaveASClicked = false;
                                        }
                                        //swal("Can't Save!", results.d, "warning");
                                        Title = "Can't Save!";
                                        Text = results.d;
                                        Type = "warning";
                                        IsSuccess = false;
                                    }
                                    setTimeout(function () {
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
                                    }, 100);
                                },
                                error: function errorFunc(jqXHR) {
                                    document.getElementById("LOADER").style.display = "none";
                                    swal("Error!", "Please try after some time..", "error");
                                    console.log(jqXHR);
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

            document.getElementById("ButtonDiv").style.display = "block";
            document.getElementById("ButtonGridDiv").style.display = "block";
            try {
                $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
                $.ajax({
                    async: false,
                    type: "POST",
                    url: "WebService_Master.asmx/MasterGridColumnHide",
                    data: '{masterID:' + JSON.stringify(masterID) + '}',
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
                        // HideColumn = JSON.parse(res);
                        var RES1 = JSON.parse(res);
                        HideColumn = RES1[0].GridColumnHide;
                        VisibleTab = RES1[0].TabName;

                        GblItemNameString = "";
                        GblItemDecString = "";
                        GblItemNameString = RES1[0].ItemNameFormula;
                        GblItemDecString = RES1[0].ItemDescriptionFormula;

                        $.ajax({
                            async: false,
                            type: "POST",
                            url: "WebService_Master.asmx/MasterGridColumn",
                            data: '{masterID:' + JSON.stringify(masterID) + '}',
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
                                        if (Colobj.toString().toUpperCase().indexOf("AS ") === -1) {
                                            DynamicCol.dataField = Colobj;
                                            DynamicCol.maxWidth = 120;
                                        } else {
                                            //var colDataField = Colobj.substring(0, Colobj.toString().toUpperCase().indexOf("AS ")).trim();
                                            //var colCaption = Colobj.substring(Colobj.toString().toUpperCase().indexOf("AS ") + 2, Colobj.length).trim();
                                            var colDataField = Colobj.split(' As ');
                                            var colCaption = colDataField[1];
                                            DynamicCol.dataField = colDataField[0];
                                            DynamicCol.maxWidth = 120;
                                            DynamicCol.caption = colCaption;
                                        }
                                        DynamicColPush.push(DynamicCol);
                                    }
                                }

                                $.ajax({
                                    async: false,
                                    type: "POST",
                                    url: "WebService_Master.asmx/MasterGrid",
                                    data: '{masterID:' + JSON.stringify(masterID) + '}',
                                    contentType: "application/json; charset=utf-8",
                                    dataType: "text",
                                    success: function (results) {
                                        var res = results.replace(/\\/g, '');
                                        res = res.replace(/"d":""/g, '');
                                        res = res.replace(/""/g, '');
                                        res = res.replace(/u0026/g, '&');
                                        res = res.replace(/u0027/g, "'");
                                        res = res.replace(/,}/g, ',null}');
                                        res = res.replace(/:,/g, ':null,');
                                        res = res.replace(/:}/g, ':null}');
                                        res = res.substr(1);
                                        res = res.slice(0, -1);
                                        var RES1 = [];
                                        if (res !== "") RES1 = JSON.parse(res);

                                        $("#MasterGrid").dxDataGrid({
                                            dataSource: RES1,
                                            columnAutoWidth: true,
                                            showBorders: true,
                                            showRowLines: true,
                                            allowColumnReordering: true,
                                            allowColumnResizing: true,
                                            columnResizingMode: "widget",
                                            paging: {
                                                pageSize: 15
                                            },
                                            pager: {
                                                showPageSizeSelector: true,
                                                allowedPageSizes: [15, 50, 100, 500]
                                            },
                                            sorting: {
                                                mode: "multiple"
                                            },
                                            selection: { mode: "single" },
                                            height: function () {
                                                return window.innerHeight / 1.1;
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
                                                fileName: document.getElementById("MasterDisplayName").innerHTML,
                                                allowExportSelectedData: true
                                            },
                                            onExporting(e) {
                                                const workbook = new ExcelJS.Workbook();
                                                const worksheet = workbook.addWorksheet(document.getElementById("MasterDisplayName").innerHTML);

                                                DevExpress.excelExporter.exportDataGrid({
                                                    component: e.component,
                                                    worksheet,
                                                    autoFilterEnabled: true,
                                                }).then(() => {
                                                    workbook.xlsx.writeBuffer().then((buffer) => {
                                                        saveAs(new Blob([buffer], { type: 'application/octet-stream' }), document.getElementById("MasterDisplayName").innerHTML + '.xlsx');
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
                                            onCellClick: function (e) {
                                                if (e.row === undefined || e.rowType === "filter" || e.rowType === "header") return false;
                                                isSaveASClicked = false;
                                                document.getElementById("txtGetGridRow").value = "";
                                                document.getElementById("txtGetGridRow").value = e.row.data.ItemID; /// grid.cellValue(Row, 0);
                                                UnderGroupID = "";
                                                UnderGroupID = e.row.data.ItemGroupID; ///grid.cellValue(Row, 1);
                                                document.getElementById("LblItemCode").innerHTML = "";
                                                document.getElementById("LblItemCode").innerHTML = "Code : " + e.row.data.ItemCode;
                                                document.getElementById("TxtItemName").value = e.row.data.ItemName;
                                                document.getElementById("TxtTallyItemName").value = e.row.data.TallyItemName;
                                            },
                                            onContentReady: function (e) {
                                                //  var HCol = HideColumn[0].GridColumnHide;
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
                                            onRowDblClick: function (e) {
                                                if (!e.data) return;
                                                $("#EditButton").click();
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
            //document.getElementById("LOADER").style.display = "none";
            //$("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
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

    document.getElementById("BtnDeletePopUp").disabled = false;
    document.getElementById("BtnSaveAS").disabled = "";
    document.getElementById("BtnSave").disabled = "";

    document.getElementById("IsactivItemstatic_Div").style.display = "block";

    document.getElementById("LblItemCode").style.display = "block";

    var MasterName = document.getElementById("MasterName").innerHTML;
    var txtGetGridRow = document.getElementById("txtGetGridRow").value;


    if (txtGetGridRow === "" || txtGetGridRow === null || txtGetGridRow === undefined) {
        alert("Please Choose any row from below Grid..!");
        return false;
    }
    PermissionUpdate(txtGetGridRow);
    GblStatus = "Update";
    isSaveASClicked = false;
    var DeleteField = GBLField[0].FieldName;

    $.ajax({
        async: false,
        type: "POST",
        url: "WebService_Master.asmx/MasterGridLoadedData",
        data: '{masterID:' + JSON.stringify(UnderGroupID) + ',Itemid:' + JSON.stringify(txtGetGridRow) + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: function (results) {
            var res = results.replace(/\\/g, '');
            res = res.replace(/"d":""/g, '');
            res = res.replace(/""/g, '');
            res = res.replace(/:}/g, ':null}');
            res = res.replace(/:,/g, ':null,');
            res = res.replace(/u0026/g, '&');
            res = res.substr(2);
            res = res.slice(0, -2);

            $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);

            var LoadedData = JSON.parse(res);

            if (LoadedData["ISItemActive"] === "True") {
                document.getElementById("IsactivItemstatic").checked = true;
            }
            else {
                document.getElementById("IsactivItemstatic").checked = false;
            }

            for (var e = 0; e < GBLField.length; e++) {

                if (GBLField[e].FieldType === "text") {
                    document.getElementById(GBLField[e].FieldName).value = (LoadedData[GBLField[e].FieldName] === undefined || LoadedData[GBLField[e].FieldName] === null) ? "" : LoadedData[GBLField[e].FieldName];
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
                    // document.getElementById(GBLField[e].FieldName).value = LoadedData[GBLField[e].FieldName];
                    $("#" + GBLField[e].FieldName).dxDateBox({
                        value: LoadedData[GBLField[e].FieldName]
                    });
                }
                else if (GBLField[e].FieldType === "selectbox") {
                    var UPSID = "#" + GBLField[e].FieldName;
                    //var selValue = LoadedData[GBLField[e].FieldName];
                    var selValue = "";
                    if (isNaN(LoadedData[GBLField[e].FieldName])) {
                        selValue = LoadedData[GBLField[e].FieldName];
                    }
                    else {
                        //selValue = JSON.parse(LoadedData[GBLField[e].FieldName]);
                        selValue = JSON.parse(LoadedData[GBLField[e].FieldName]);
                    }

                    $(UPSID).dxSelectBox({
                        value: selValue
                    });

                }
            }
        }
    });

});

function PermissionUpdate(txtGetGridRow) {
    $.ajax({
        type: "POST",
        url: "WebService_Master.asmx/CheckPermissionforUpdate",
        data: '{ItemID:' + JSON.stringify(txtGetGridRow) + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (results) {
            var res = JSON.stringify(results);
            res = res.replace(/"d":/g, '');
            res = res.replace(/{/g, '');
            res = res.replace(/}/g, '');
            res = res.substr(1);
            res = res.slice(0, -1);
            if (res === "Exist") {
                //document.getElementById("BtnSave").disabled = true;
                updateGroupFlag = "true";
            }
        },
        error: function errorFunc(jqXHR) {
            $("#LoadIndicator").dxLoadPanel("instance").option("visible", false);
            alert(jqXHR);
        }
    });
};

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

    $.ajax({
        type: "POST",
        url: "WebService_Master.asmx/CheckPermission",
        data: '{TransactionID:' + JSON.stringify(txtGetGridRow) + '}',
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

            if (res === "Exist") {
                swal("", "This item is used in another process..! Record can not be delete.", "error");
                return false;
            }
            else {

                swal({
                    title: "Are you sure?",
                    text: "You will not be able to recover this Content!",
                    type: "warning",
                    showCancelButton: true,
                    confirmButtonColor: "#DD6B55",
                    confirmButtonText: "Yes, delete it!",
                    closeOnConfirm: false
                }, function () {

                    $("#LoadIndicator").dxLoadPanel("instance").option("visible", true);
                    $.ajax({
                        type: "POST",
                        url: "WebService_Master.asmx/DeleteData",
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
                                //  alert("Your Data has been Deleted Successfully...!");
                                swal("Deleted!", "Your Content has been deleted.", "success");
                                //location.reload();
                                FillGrid();
                                $("#largeModal").modal('hide');
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

        }
    });


});

$("#BtnNew").click(function () {
    refreshbtn();
    document.getElementById("BtnDeletePopUp").disabled = true;
});

$("#BtnDeletePopUp").click(function () {
    $("#DeleteButton").click();
});

//For DrillDown Tab
function DrilDown(dr) {
    var masterID = document.getElementById("MasterID").innerHTML;

    $.ajax({
        type: "POST",
        url: "WebService_Master.asmx/DrillDownMasterGrid",
        data: '{masterID:' + JSON.stringify(masterID) + ',TabID:' + JSON.stringify(dr.id) + '}',
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
            var RES1 = [];
            if (res === "") { RES1 = []; } else RES1 = JSON.parse(res);
            if (RES1 === [] || RES1 === "") {
                RES1 = [];
            }

            $("#DrilDownGrid").dxDataGrid({
                dataSource: RES1,
                columnAutoWidth: true,
                showBorders: true,
                showRowLines: true,
                allowColumnReordering: true,
                allowColumnResizing: true,
                sorting: {
                    mode: "multiple"
                },
                selection: { mode: "single" },
                //height: 600,
                scrolling: { mode: 'virtual' },
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
                    fileName: document.getElementById("MasterDisplayName").innerHTML + '-' + document.getElementById(dr.id).innerHTML,
                    allowExportSelectedData: true
                },
                onRowPrepared: function (e) {
                    if (e.rowType === "header") {
                        e.rowElement.css('background', '#42909A');
                        e.rowElement.css('color', 'white');
                    }
                    e.rowElement.css('fontSize', '11px');
                }

                //onCellClick: function (e) {
                //    var grid = $('#DrilDownGrid').dxDataGrid('instance');
                //    if (e.row === undefined || e.rowType === "filter" || e.rowType === "header") return false;
                //    var Row = e.row.rowIndex;
                //    var Col = e.columnIndex;

                //    document.getElementById("txtGetGridRow").value = "";
                //    document.getElementById("txtGetGridRow").value = grid.cellValue(Row, 0);
                //    UnderGroupID = "";
                //    UnderGroupID = grid.cellValue(Row, 1);
                //},
                //onContentReady: function (e) {
                //    //  var HCol = HideColumn[0].GridColumnHide;
                //    var HCol = HideColumn;
                //    if (HCol) {
                //        HCol = HCol.split(',');
                //        for (var hc in HCol) {
                //            var placedHC = HCol[hc];
                //            $('#MasterGrid').dxDataGrid("columnOption", placedHC, "visible", false);
                //        }
                //    }
                //},
                //columns: DynamicColPush

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
        showBorders: true,
        showRowLines: true,
        headerFilter: { visible: true },
        searchPanel: { visible: true },
        export: {
            enabled: true,
            fileName: document.getElementById("MasterDisplayName").innerHTML
            //allowExportSelectedData: false,
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
    document.getElementById("LblItemCode").innerHTML = "";
    document.getElementById("LblItemCode").style.display = "none";
    document.getElementById("TxtItemName").value = "";/// added by pKp
    document.getElementById("TxtTallyItemName").value = "";/// added by pKp

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
    if (isNaN(x)) {
        document.getElementById(getValid).style.display = "block";
        document.getElementById(getValid).innerHTML = 'This field must have alphanumeric characters only';
        document.getElementById(FC.id).value = "";
        document.getElementById(FC.id).focus();
        return false;

    }
    else {
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
            for (var t = 0; t < CreateVar.length; t++) {
                if (t === 0) {
                    holdvalue = CreateVar[t];
                } else {
                    var fillValue = 0;
                    if ($('#' + CreateVar[t].replace(/ /g, '')).hasClass('dx-selectbox')) {
                        fillValue = $("#" + CreateVar[t].replace(/ /g, '')).dxSelectBox("instance").option('text');
                    } else {
                        fillValue = document.getElementById(CreateVar[t]).value;
                    }

                    //var fillValue = document.getElementById(CreateVar[t]).value;
                    var StrVar = "";
                    if (MakeObj === "" || MakeObj === undefined || MakeObj === null) {
                        if (isNaN(fillValue)) {//*
                            StrVar = '"' + fillValue + '"';//*
                        } else {//*
                            StrVar = Number(fillValue);
                            //if (fillValue === "" || fillValue === null || fillValue === undefined) {
                            //    fillValue = 0;
                            //}
                        }//*
                        // MakeObj += CreateVar[t] + '=' + fillValue;
                        MakeObj += CreateVar[t] + '=' + StrVar;
                    } else {
                        if (isNaN(fillValue)) {//*
                            StrVar = '"' + fillValue + '"';//*
                        } else {
                            StrVar = Number(fillValue);
                        }
                        MakeObj += ',' + CreateVar[t] + '=' + StrVar;
                        //if (fillValue === "" || fillValue === null || fillValue === undefined) {
                        //    fillValue = 0;
                        //}
                        //MakeObj += ',' + CreateVar[t] + '=' + fillValue;
                    }

                }
            }
            getFormula = "";
            var addstr = "Formula" + FC.id;
            getFormula = document.getElementById(addstr).innerHTML;
            //alert(getFormula);
            var NextFarmula = getFormula.toLowerCase().includes("and");
            if (NextFarmula === true) {
                getFormula = getFormula.split('and');
                getFormula = getFormula[z];
            }
            ApplyOperation(MakeObj);
        }
    }

}

function FarmulaChangeSELECTBX(currentID) {

    var geval = "ValCh" + currentID;
    var getSepValu = document.getElementById(geval).value;

    var selValue = "";
    selValue = $("#" + currentID).dxSelectBox("instance").option('text');
    if (selValue !== "" && selValue !== null && selValue !== "null" && selValue !== undefined) {

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
                        if ($('#' + CreateVar[t].replace(/ /g, '')).hasClass('dx-selectbox')) {
                            fillValue = $("#" + CreateVar[t].replace(/ /g, '')).dxSelectBox("instance").option('text');
                        } else {
                            fillValue = document.getElementById(CreateVar[t].replace(/ /g, '')).value;
                        }
                        //var fillValue = $("#" + CreateVar[t].replace(/ /g, '')).dxSelectBox("instance").option('text');
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
    }
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

    getFormula = getFormula.split('=');
    getFormula = getFormula[1];

    var IsFarmula = getFormula.toLowerCase().includes("conc");

    if (IsFarmula === true) {
        var getFormulaConc = getFormula.replace(/conc/g, ',');
        getFormulaConc = getFormulaConc.replace(/CONC/g, ',');
        getFormulaConc = getFormulaConc.split(',');

        var concanate = getFormulaConc;
        var Concstrng = "";
        Concat(concanate);
    }
    else {
        getFormula = getFormula.replace(/u0027/g, "'");
        var CalResult = "return " + getFormula;

        if (doc === "") {
            doc = "function GetRes(){" + CalResult + ";}";
        } else {
            doc = doc + ";function GetRes(){" + CalResult + ";}";
        }
        eval(doc);

        var ResultsValue = GetRes();
        SetFieldValue(holdvalue, ResultsValue);
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

function SetFieldValue(name, value) {
    name = (name !== undefined && name !== null) ? name.toString().trim() : "";
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

function updateColumnOnGroup(UpdatedItemName) {
    var data = $('#MasterGrid').dxDataGrid('instance').getSelectedRowsData();
    $.ajax({
        async: false,
        type: "POST",
        url: "WebService_Master.asmx/UpdateUserData",
        data: '{ItemName:' + JSON.stringify(UpdatedItemName) + ',ItemID:' + JSON.stringify(data[0].ItemID) + ',StockRefCode:' + JSON.stringify(UpdatedItemName[0].StockRefCode) + '}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        //success: function (results) {
        //    var res = results.d;
        //    document.getElementById("LOADER").style.display = "none";
        //    if (res.includes("Success")) {
        //          showDevExpressNotification("Your data has been successfully updated..!", "success");
        //          setTimeout(function () {
        //               location.reload();
        //          }, 1000);

        //    } else {
        //        setTimeout(function () {
        //        swal({
        //              title: "Error",
        //               text: res,
        //              icon: "warning",
        //              button: "OK"

        //        });
        //        }, 1000);
        //    }

        //},

        success: function (results) {
            var res = results.d;
            document.getElementById("LOADER").style.display = "none";
            let Title, Text, Type;
            let IsSuccess = false;
            if (res.includes("Success")) {
                Title = "Updated...";
                Text = "Your data Partial Updated...";
                Text = "This item has already been used, so only specific fields (Purchase Rate,Estimation Rate,ProductHSN,Stock Ref Code,Re-Order Quantity,Minimum Stock Qty) were updated.";
                Type = "success";
                IsSuccess = true;

            } else {
                Title = "Can't Update!";
                Text = res;
                Type = "warning";
                IsSuccess = false;
            }
            setTimeout(function () {
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
            }, 100);

        },
        error: function errorFunc(jqXHR) {
            document.getElementById("LOADER").style.display = "none";
            swal({
                title: "Error",
                text: "Please try after some time...!",
                icon: "warning",
                button: "OK"
            });
        }
    });
}

if (queryString.length === 0) {
    if (window.location.search.split('?').length > 1) {
        var params = window.location.search.split('?')[1].split('&');
        for (var i = 0; i < params.length; i++) {
            var key = params[i].split('=')[0];
            var value = decodeURIComponent(params[i].split('=')[1]);//.replace(/"/g, '');
            queryString[key] = value;
        }

        if (queryString.MasterID === 14 || queryString.MasterID === '14') {
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

            getMasterLIST();
            let e = {};
            e.id = Number(queryString.MasterID);
            CurrentMaster(e)

            document.getElementById("EditButton").style.display = "none";
            document.getElementById("DeleteButton").style.display = "none";
            document.getElementById("btnTabModel").style.display = "none";
            document.getElementById("hiddenBtn_ChooseMaster").style.display = "none";
            document.getElementById("MasterUL").style.display = "none";

            PWOSpecialPaperCreateFlag = true;
        }
    }
};


function PWOSpecialItemCreate() {

    var DataArray = JSON.parse(queryString.ItemMasterArray);
    var LoadedData = DataArray[0];
    if (LoadedData["ISItemActive"] === "True") {
        document.getElementById("IsactivItemstatic").checked = true;
    }
    else {
        document.getElementById("IsactivItemstatic").checked = false;
    }

    for (var e = 0; e < GBLField.length; e++) {

        if (GBLField[e].FieldType === "text") {
            document.getElementById(GBLField[e].FieldName).value = (LoadedData[GBLField[e].FieldName] === undefined || LoadedData[GBLField[e].FieldName] === null) ? "" : LoadedData[GBLField[e].FieldName];
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
            // document.getElementById(GBLField[e].FieldName).value = LoadedData[GBLField[e].FieldName];
            $("#" + GBLField[e].FieldName).dxDateBox({
                value: LoadedData[GBLField[e].FieldName]
            });
        }
        else if (GBLField[e].FieldType === "selectbox") {
            var UPSID = "#" + GBLField[e].FieldName;
            //var selValue = LoadedData[GBLField[e].FieldName];
            var selValue = "";
            if (isNaN(LoadedData[GBLField[e].FieldName])) {
                selValue = LoadedData[GBLField[e].FieldName];
            }
            else {
                //selValue = JSON.parse(LoadedData[GBLField[e].FieldName]);
                selValue = JSON.parse(LoadedData[GBLField[e].FieldName]);
            }

            $(UPSID).dxSelectBox({
                value: selValue
            });

        }
    }
}