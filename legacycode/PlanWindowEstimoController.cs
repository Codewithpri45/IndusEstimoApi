using Azure;
using IndusWebApi.Models;
using IndusWebApi.ValidateRequest;
using Microsoft.Graph.Drives.Item.Items.Item.Workbook.Names.Item.RangeNamespace.ColumnsBeforeWithCount;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Transactions;
using System.Web;
using System.Web.Http;
using static IndusWebApi.Controllers.Jobs.EnquiryController;
//using System.Web.Mvc;

namespace IndusWebApi.Controllers.Planning
{
    [Validate]
    [RoutePrefix("api/planwindow")]
    public class PlanWindowEstimoController : ApiController
    {
        private string FYear;
        private HelloWorldData data = new HelloWorldData();
        private DataTable dataTable = new DataTable();
        private string str;

        private string GBLUserID;
        private Int32 GBLBranchID;
        private string GBLCompanyID;
        private string GBLProductionUnitID;
        private string GBLUserName;
        private string DBType = "";

        public Api_shiring_serviceController Shirin = new Api_shiring_serviceController();

        [HttpGet]
        [Route("quality/{contenttype}")]
        public IHttpActionResult GetQuality(string contenttype = "")
        {
            try
            {
                str = DBConnection.Version;
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                if (str == "New")
                {
                    if (contenttype.ToUpper() == "FLEXO")
                    {
                        str = $"Select Distinct Quality from ItemMaster Where Isnull(ISItemActive,1)<>0 And ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where ItemGroupNameID IN(-14)) And Isnull(IsDeletedTransaction,0)=0 And ISNULL(Quality,'')<>'' Order By Quality";
                    }
                    else if (contenttype.ToUpper() == "ROTOGRAVURE")
                    {
                        str = $"Select Distinct Quality from ItemMaster Where Isnull(ISItemActive,1)<>0 And ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where ItemGroupNameID IN(-15)) And Isnull(IsDeletedTransaction,0)=0 And ISNULL(Quality,'')<>'' Order By Quality";
                    }
                    else if (contenttype.ToUpper() == "LARGEFORMAT")
                    {
                        str = $"Select Distinct Quality from ItemMaster Where Isnull(ISItemActive,1)<>0 And ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where ItemGroupNameID IN(-16)) And Isnull(IsDeletedTransaction,0)=0 And ISNULL(Quality,'')<>'' Order By Quality";
                    }
                    else
                    {
                        str = $"Select Distinct Quality from ItemMaster Where Isnull(ISItemActive,1)<>0 And ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where ItemGroupNameID IN(-1,-2)) And Isnull(IsDeletedTransaction,0)=0 And ISNULL(Quality,'')<>'' Order By Quality";
                    }
                }
                else
                {
                    str = $"select Distinct Quality from PaperMaster Union Select Distinct Quality from ReelMaster Order by Quality";
                }

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return Ok(JsonConvert.SerializeObject(data.Message));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("gsm/{contenttype}/{quality}/{thickness}")]
        public IHttpActionResult GetGSM(string contenttype = "", string quality = "", string thickness = "")
        {
            try
            {
                str = DBConnection.Version;
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                string ItemGroupNameID = "";
                if (contenttype.ToUpper() == "FLEXO")
                {
                    ItemGroupNameID = "-14";
                }
                else if (contenttype.ToUpper() == "ROTOGRAVURE")
                {
                    ItemGroupNameID = "-15";
                }
                else if (contenttype.ToUpper() == "LARGEFORMAT")
                {
                    ItemGroupNameID = "-16";
                }
                else
                {
                    ItemGroupNameID = "-1,-2";
                }
                if (string.IsNullOrEmpty(quality))
                {
                    str = $"Select Distinct GSM From ItemMaster Where Isnull(ISItemActive,1)<>0 And ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where ItemGroupNameID IN({ItemGroupNameID})) And Isnull(IsDeletedTransaction,0)=0 And (ISNULL(GSM,0)>0 OR ISNULL(Thickness,0)>0) Order By GSM";
                }
                else
                {
                    if (Convert.ToDouble(thickness) > 0)
                    {
                        str = $"Select Distinct GSM From ItemMaster Where Isnull(ISItemActive,1)<>0 And ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where ItemGroupNameID IN({ItemGroupNameID})) And ItemID IN (Select ItemID From ItemMaster Where IsDeletedTransaction=0 And Quality='{quality}' AND Thickness={Convert.ToDouble(thickness)} And Isnull(IsDeletedTransaction,0)<>1) Order By GSM";
                    }
                    else
                    {
                        str = $"Select Distinct GSM From ItemMaster Where Isnull(ISItemActive,1)<>0 And ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where ItemGroupNameID IN({ItemGroupNameID})) And ItemID IN (Select ItemID From ItemMaster Where IsDeletedTransaction=0 And Quality='{quality}' And Isnull(IsDeletedTransaction,0)<>1) Order By GSM";
                    }
                }

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return Ok(JsonConvert.SerializeObject(data.Message));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("thickness")]
        public IHttpActionResult GetThickness(string contenttype = "", string quality = "", string gsm = "")
        {
            try
            {
                str = DBConnection.Version;
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                string ItemGroupNameID = "";
                if (contenttype.ToUpper() == "FLEXO")
                {
                    ItemGroupNameID = "-14";
                }
                else if (contenttype.ToUpper() == "ROTOGRAVURE")
                {
                    ItemGroupNameID = "-15";
                }
                else if (contenttype.ToUpper() == "LARGEFORMAT")
                {
                    ItemGroupNameID = "-16";
                }
                else
                {
                    ItemGroupNameID = "-1,-2";
                }
                if (string.IsNullOrEmpty(quality))
                {
                    str = $"Select Distinct Thickness From ItemMaster Where Isnull(ISItemActive,1)<>0 And ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where ItemGroupNameID IN({ItemGroupNameID})) And Isnull(IsDeletedTransaction,0)=0 And ISNULL(Thickness,0)>0 Order By Thickness";
                }
                else
                {
                    str = $"Select Distinct Thickness From ItemMaster Where Isnull(ISItemActive,1)<>0 And ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where ItemGroupNameID IN({ItemGroupNameID})) And ItemID IN (Select ItemID From ItemMaster Where IsDeletedTransaction=0 And Quality='{quality}') And Isnull(IsDeletedTransaction,0)<>1 Order By Thickness";
                }
                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return Ok(JsonConvert.SerializeObject(data.Message));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("mill/{contenttype}/{quality}/{gsm}/{thickness}")]
        public IHttpActionResult GetMill(string contenttype = "", string quality = "", string gsm = "", string thickness = "")
        {
            try
            {
                str = DBConnection.Version;
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                string ItemGroupNameID = "";
                if (contenttype.ToUpper() == "FLEXO")
                {
                    ItemGroupNameID = "-14";
                }
                else if (contenttype.ToUpper() == "ROTOGRAVURE")
                {
                    ItemGroupNameID = "-15";
                }
                else if (contenttype.ToUpper() == "LARGEFORMAT")
                {
                    ItemGroupNameID = "-16";
                }
                else
                {
                    ItemGroupNameID = "-1,-2";
                }

                if (string.IsNullOrEmpty(quality) && Convert.ToDouble(gsm) == 0 && Convert.ToDouble(thickness) == 0)
                {
                    str = $"Select Distinct Manufecturer As Mill From ItemMaster Where Isnull(ISItemActive,1)<>0 And ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where ItemGroupNameID IN({ItemGroupNameID})) And Isnull(IsDeletedTransaction,0)<>1 And ISNULL(Manufecturer,'')<>'' ";
                }
                else
                {
                    if (!string.IsNullOrEmpty(quality) && Convert.ToDouble(gsm) > 0)
                    {
                        str = $"Select Distinct Manufecturer As Mill From ItemMaster Where Isnull(ISItemActive,1)<>0 And ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where ItemGroupNameID IN({ItemGroupNameID})) And ItemID IN (Select ItemID From ItemMaster Where Quality='{quality}' And IsDeletedTransaction=0) And ItemID IN (Select ItemID From ItemMaster Where GSM='{Convert.ToDouble(gsm)}' And IsDeletedTransaction=0) And Isnull(IsDeletedTransaction,0)<>1 Order By Mill";
                    }
                    else if (!string.IsNullOrEmpty(quality) && Convert.ToDouble(thickness) > 0)
                    {
                        str = $"Select Distinct Manufecturer As Mill From ItemMaster Where Isnull(ISItemActive,1)<>0 And ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where ItemGroupNameID IN({ItemGroupNameID})) And ItemID IN (Select ItemID From ItemMaster Where Quality='{quality}' And IsDeletedTransaction=0) And ItemID IN (Select ItemID From ItemMaster Where Thickness='{Convert.ToDouble(thickness)}' And IsDeletedTransaction=0) And Isnull(IsDeletedTransaction,0)<>1 Order By Mill";
                    }
                    else
                    {
                        str = $"Select Distinct Manufecturer As Mill From ItemMaster Where Isnull(ISItemActive,1)<>0 And ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where ItemGroupNameID IN({ItemGroupNameID})) And ItemID IN (Select ItemID From ItemMaster Where Quality='{quality}' And IsDeletedTransaction=0) And ItemID IN (Select ItemID From ItemMaster Where GSM='{gsm}' And IsDeletedTransaction=0) And Isnull(IsDeletedTransaction,0)<>1 Order By Mill";
                    }
                }

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return Ok(JsonConvert.SerializeObject(data.Message));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("rotomill/{quality}/{thickness}")]
        public IHttpActionResult GetRotoMill(string quality = "", string thickness = "")
        {
            try
            {
                str = DBConnection.Version;
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);


                if (string.IsNullOrEmpty(quality) && string.IsNullOrEmpty(thickness))
                {
                    str = $"Select Distinct Manufecturer As Mill From ItemMaster Where Isnull(ISItemActive,1)<>0 And ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where CompanyID={GBLCompanyID} And ItemGroupNameID IN(-1,-2,-14,-15,-16)) And Isnull(IsDeletedTransaction,0)<>1 And ISNULL(Manufecturer,'')<>'' And CompanyID={GBLCompanyID}";
                }
                else
                {
                    str = $"Select Distinct Manufecturer As Mill From ItemMaster Where Isnull(ISItemActive,1)<>0 And ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where CompanyID={GBLCompanyID} And ItemGroupNameID IN(-1,-2,-14,-15,-16)) And ItemID IN (Select ItemID From ItemMaster Where Quality='{quality}' And CompanyID={GBLCompanyID} And IsDeletedTransaction=0) And ItemID IN (Select ItemID From ItemMaster Where Thickness='{thickness}' And CompanyID={GBLCompanyID} And IsDeletedTransaction=0) And Isnull(IsDeletedTransaction,0)<>1 Order By Mill";
                }
                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return Ok(JsonConvert.SerializeObject(data.Message));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("layeritems")]
        public IHttpActionResult GetLayerItemMaster()
        {
            try
            {
                str = DBConnection.Version;
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                str = $"Select IM.ItemID,IM.ItemCode,IGM.ItemGroupID,ISGM.ItemSubGroupID,IGM.ItemGroupName,ISGM.ItemSubGroupName,IM.ItemName,IM.Quality,IM.SizeW,IM.Thickness,IM.Density,IM.GSM,IM.Manufecturer, IM.EstimationUnit,IM.EstimationRate,IM.StockUnit " +
                      $"From ItemMaster AS IM INNER JOIN ItemGroupMaster AS IGM on IGM.ItemGroupID=IM.ItemGroupID LEFT JOIN ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID AND Isnull(ISGM.IsDeletedTransaction,0)=0 " +
                      $"Where IM.CompanyID={GBLCompanyID} AND Isnull(IM.IsDeletedTransaction,0)=0 AND IGM.ItemGroupNameID IN(-14,-15) Order by IM.ItemGroupID,ISGM.ItemSubGroupName,IM.Quality,IM.Thickness,IM.Density,IM.GSM,IM.SizeW";

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return Ok(JsonConvert.SerializeObject(data.Message));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("availablelayers/{w}/{w1}")]
        public IHttpActionResult ChkAvailableLayers(string w, string w1)
        {
            try
            {
                str = DBConnection.Version;
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);

                str = $"Select IM.ItemID,IM.ItemCode,IGM.ItemGroupID,ISGM.ItemSubGroupID,IGM.ItemGroupName,ISGM.ItemSubGroupName,IM.ItemName,IM.Quality,IM.SizeW,IM.Thickness,IM.Density,IM.GSM,IM.Manufecturer, IM.EstimationUnit,IM.EstimationRate,IM.StockUnit From ItemMaster AS IM INNER JOIN ItemGroupMaster AS IGM on IGM.ItemGroupID=IM.ItemGroupID LEFT JOIN ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID " +
                      $"Where IM.CompanyID= {GBLCompanyID} AND Isnull(IM.IsDeletedTransaction,0)=0 AND IGM.ItemGroupNameID IN(-14,-15) and (IM.SizeW >= ({w} - {w1}) and IM.SizeW <= ({w} + {w1})) Order by IM.ItemGroupID,ISGM.ItemSubGroupName,IM.Quality,IM.Thickness,IM.Density,IM.GSM,IM.SizeW";

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);

                return Ok(JsonConvert.SerializeObject(data.Message, new JsonSerializerSettings { MaxDepth = null }));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("onetimecharges")]
        public IHttpActionResult GetOnetimeCharges()
        {
            try
            {
                str = DBConnection.Version;
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                if (DBType == "MYSQL")
                {
                    str = $"Select ParameterValue as Headname, '0' as Amount From ERPParameterSetting Where ParameterType = 'One Time Charges' and IFNULL(IsDeletedTransaction,0) = 0 Order By ParameterID";
                }
                else
                {
                    str = $"Select ParameterValue as Headname, '0' as Amount From ERPParameterSetting Where ParameterType = 'One Time Charges' and Isnull(IsDeletedTransaction,0) = 0 Order By ParameterID";
                }

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return Ok(JsonConvert.SerializeObject(data.Message));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        [HttpGet]
        [Route("processmaterials/{processIDStr}")]
        public IHttpActionResult GetProcessWiseMaterials(string processIDStr)
        {
            try
            {
                str = DBConnection.Version;
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                str = $" WITH RankedMachines AS (SELECT ProcessID, MachineID, ROW_NUMBER() OVER (PARTITION BY ProcessID ORDER BY MachineID) AS rn FROM ProcessAllocatedMachineMaster ) SELECT PM.ProcessID,PM.ProcessModuleType as DomainType, IM.PurchaseRate,IM.EstimationRate,IM.ItemID, IGM.ItemGroupID, ISG.ItemSubGroupID, IGM.ItemGroupNameID ,RM.MachineID, IM.ItemName, IGM.ItemGroupName, ISG.ItemSubGroupName, PM.ProcessName, MM.MachineName, IM.SizeL, IM.SizeW, IM.SizeH, IM.Thickness, IM.Density, IM.GSM, IM.ReleaseGSM, IM.AdhesiveGSM, IM.StockUnit, IM.PurchaseUnit, IM.EstimationUnit FROM ProcessMaster AS PM INNER JOIN ProcessAllocatedMaterialMaster AS PAM ON PAM.ProcessID = PM.ProcessID INNER JOIN RankedMachines AS RM ON RM.ProcessID = PAM.ProcessID AND RM.rn = 1 INNER JOIN MachineMaster AS MM ON MM.MachineID = RM.MachineID INNER JOIN ItemMaster AS IM ON IM.ItemID = PAM.ItemID INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID = IM.ItemGroupID INNER JOIN ItemSubGroupMaster AS ISG ON ISG.ItemSubGroupID = IM.ItemSubGroupID WHERE PM.ProcessID IN({processIDStr})";

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return Ok(JsonConvert.SerializeObject(data.Message));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("windingdirection/{contentDomainType}")]
        public IHttpActionResult GetWindingDirection(string contentDomainType)
        {
            try
            {
                str = DBConnection.Version;
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);

                str = $"Select WindingDirectionID, Direction, Image From UnwindingDirectionImage Where CompanyID={GBLCompanyID} AND ContentDomainType='{contentDomainType}'";

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);

                return Ok(JsonConvert.SerializeObject(data.Message, new JsonSerializerSettings { MaxDepth = null }));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("finish/{quality}/{gsm}/{mill}")]
        public IHttpActionResult GetFinish(string quality = "", string gsm = "", string mill = "")
        {
            try
            {
                str = DBConnection.Version;
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                if (DBType == "MYSQL")
                {
                    if (string.IsNullOrEmpty(quality) && string.IsNullOrEmpty(gsm) && string.IsNullOrEmpty(mill))
                    {
                        str = $"Select Distinct Finish From ItemMaster Where ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where CompanyID={GBLCompanyID} And ItemGroupNameID IN(-1,-2,-14)) And IFNULL(IsDeletedTransaction,0)<>1 And IFNULL(Finish,'')<>'' And CompanyID={GBLCompanyID} Order By Finish";
                    }
                    else
                    {
                        str = $"Select Distinct Finish From ItemMaster Where ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where CompanyID={GBLCompanyID} And ItemGroupNameID IN(-1,-2,-14)) And ItemID IN (Select ItemID From ItemMaster Where GSM={gsm} And CompanyID={GBLCompanyID} And IsDeletedTransaction=0) And ItemID IN (Select ItemID From ItemMaster Where Quality='{quality}' And CompanyID={GBLCompanyID} And IsDeletedTransaction=0) And ItemID IN (Select ItemID From ItemMaster Where Manufecturer='{mill}' And CompanyID={GBLCompanyID} And IsDeletedTransaction=0) And IFNULL(IsDeletedTransaction,0)<>1 Order By Finish";
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(quality) && string.IsNullOrEmpty(gsm) && string.IsNullOrEmpty(mill))
                    {
                        str = $"Select Distinct Finish From ItemMaster Where Isnull(ISItemActive,1)<>0 And ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where CompanyID={GBLCompanyID} And ItemGroupNameID IN(-1,-2,-14,-15,-16)) And Isnull(IsDeletedTransaction,0)<>1 And ISNULL(Finish,'')<>'' And CompanyID={GBLCompanyID} Order By Finish";
                    }
                    else
                    {
                        str = $"Select Distinct Finish From ItemMaster Where Isnull(ISItemActive,1)<>0 And ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where CompanyID={GBLCompanyID} And ItemGroupNameID IN(-1,-2,-14,-15,-16)) And ItemID IN (Select ItemID From ItemMaster Where GSM={gsm} And CompanyID={GBLCompanyID} And IsDeletedTransaction=0) And ItemID IN (Select ItemID From ItemMaster Where Quality='{quality}' And CompanyID={GBLCompanyID} And IsDeletedTransaction=0) And ItemID IN (Select ItemID From ItemMaster Where Manufecturer='{mill}' And CompanyID={GBLCompanyID} And IsDeletedTransaction=0) And Isnull(IsDeletedTransaction,0)<>1 Order By Finish";
                    }
                }

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return Ok(JsonConvert.SerializeObject(data.Message));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("getbf")]
        public IHttpActionResult GetBF(string quality = "", string gsm = "", string mill = "")
        {
            try
            {
                str = DBConnection.Version;
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);


                    if (string.IsNullOrEmpty(quality) && string.IsNullOrEmpty(gsm) && string.IsNullOrEmpty(mill))
                    {
                        str = $"Select Distinct BF From ItemMaster Where Isnull(ISItemActive,1)<>0 And ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where CompanyID={GBLCompanyID} And ItemGroupNameID IN(-1,-2,-14,-15,-16)) And Isnull(IsDeletedTransaction,0)<>1 And ISNULL(BF,0)>0 And CompanyID={GBLCompanyID} Order By BF";
                    }
                    else
                    {
                        str = $"Select Distinct BF From ItemMaster Where Isnull(ISItemActive,1)<>0 And ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where CompanyID={GBLCompanyID} And ItemGroupNameID IN(-1,-2,-14,-15,-16)) And ItemID IN (Select ItemID From ItemMaster Where GSM={gsm} And CompanyID={GBLCompanyID} And IsDeletedTransaction=0) And ItemID IN (Select ItemID From ItemMaster Where Quality='{quality}' And CompanyID={GBLCompanyID} And IsDeletedTransaction=0) And ItemID IN (Select ItemID From ItemMaster Where Manufecturer='{mill}' And CompanyID={GBLCompanyID} And IsDeletedTransaction=0) And Isnull(IsDeletedTransaction,0)<>1 And ISNULL(BF,0)>0 Order By BF";
                    }

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return Ok(JsonConvert.SerializeObject(data.Message));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("filteredpaper/{quality}/{gsm}/{mill}")]
        public IHttpActionResult GetFilteredItems(string quality = "", string gsm = "", string mill = "")
        {
            try
            {
                str = DBConnection.Version;
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                DataTable dtMill = new DataTable();
                DataTable dtFinish = new DataTable();
                DataTable dtGSM = new DataTable();

                if (str == "New")
                {
                    // Get GSM data
                    if (DBType == "MYSQL")
                    {
                        if (string.IsNullOrEmpty(quality))
                        {
                            str = $"Select Distinct GSM From ItemMaster Where ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where CompanyID={GBLCompanyID} And ItemGroupNameID IN(-1,-2)) And IFNULL(IsDeletedTransaction,0)<>1 And IFNULL(GSM,0)>0 And CompanyID={GBLCompanyID} Order By GSM";
                        }
                        else
                        {
                            str = $"Select Distinct GSM From ItemMaster Where ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where CompanyID={GBLCompanyID} And ItemGroupNameID IN(-1,-2)) And ItemID IN (Select ItemID From ItemMaster Where Quality='{quality}' And CompanyID={GBLCompanyID} And IsDeletedTransaction=0) And IFNULL(IsDeletedTransaction,0)<>1 Order By GSM";
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(quality))
                        {
                            str = $"Select Distinct GSM From ItemMaster Where ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where CompanyID={GBLCompanyID} And ItemGroupNameID IN(-1,-2)) And Isnull(IsDeletedTransaction,0)<>1 And ISNULL(GSM,0)>0 And CompanyID={GBLCompanyID} Order By GSM";
                        }
                        else
                        {
                            str = $"Select Distinct GSM From ItemMaster Where ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where CompanyID={GBLCompanyID} And ItemGroupNameID IN(-1,-2)) And ItemID IN (Select ItemID From ItemMaster Where Quality='{quality}' And CompanyID={GBLCompanyID} And IsDeletedTransaction=0) And Isnull(IsDeletedTransaction,0)<>1 Order By GSM";
                        }
                    }
                    DBConnection.FillDataTable(ref dtGSM, str);

                    // Get Mill data
                    if (DBType == "MYSQL")
                    {
                        if (string.IsNullOrEmpty(quality) && string.IsNullOrEmpty(gsm))
                        {
                            str = $"Select Distinct Manufecturer As Mill From ItemMaster Where ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where CompanyID={GBLCompanyID} And ItemGroupNameID IN(-1,-2)) And IFNULL(IsDeletedTransaction,0)<>1 And IFNULL(Manufecturer,'')<>'' And CompanyID={GBLCompanyID} Order By Mill";
                        }
                        else
                        {
                            str = $"Select Distinct Manufecturer As Mill From ItemMaster Where ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where CompanyID={GBLCompanyID} And ItemGroupNameID IN(-1,-2)) And ItemID IN (Select ItemID From ItemMaster Where GSM='{gsm}' And CompanyID={GBLCompanyID} And IsDeletedTransaction=0) And ItemID IN (Select ItemID From ItemMaster Where Quality='{quality}' And CompanyID={GBLCompanyID} And IsDeletedTransaction=0) And IFNULL(IsDeletedTransaction,0)<>1 Order By Mill";
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(quality) && string.IsNullOrEmpty(gsm))
                        {
                            str = $"Select Distinct Manufecturer As Mill From ItemMaster Where ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where CompanyID={GBLCompanyID} And ItemGroupNameID IN(-1,-2)) And Isnull(IsDeletedTransaction,0)<>1 And ISNULL(Manufecturer,'')<>'' And CompanyID={GBLCompanyID} Order By Mill";
                        }
                        else
                        {
                            str = $"Select Distinct Manufecturer As Mill From ItemMaster Where ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where CompanyID={GBLCompanyID} And ItemGroupNameID IN(-1,-2)) And ItemID IN (Select ItemID From ItemMaster Where GSM='{gsm}' And CompanyID={GBLCompanyID} And IsDeletedTransaction=0) And ItemID IN (Select ItemID From ItemMaster Where Quality='{quality}' And CompanyID={GBLCompanyID} And IsDeletedTransaction=0) And Isnull(IsDeletedTransaction,0)<>1 Order By Mill";
                        }
                    }
                    DBConnection.FillDataTable(ref dtMill, str);

                    // Get Finish data
                    if (DBType == "MYSQL")
                    {
                        if (string.IsNullOrEmpty(quality) && string.IsNullOrEmpty(gsm) && string.IsNullOrEmpty(mill))
                        {
                            str = $"Select Distinct Finish From ItemMaster Where ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where CompanyID={GBLCompanyID} And ItemGroupNameID IN(-1,-2)) And CompanyID={GBLCompanyID} And IFNULL(IsDeletedTransaction,0)<>1 And IFNULL(Finish,'')<>'' Order By Finish";
                        }
                        else
                        {
                            str = $"Select Distinct Finish From ItemMaster Where ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where CompanyID={GBLCompanyID} And ItemGroupNameID IN(-1,-2)) And ItemID IN (Select ItemID From ItemMaster Where GSM={gsm} And CompanyID={GBLCompanyID} And IsDeletedTransaction=0) And ItemID IN (Select ItemID From ItemMaster Where Quality='{quality}' And CompanyID={GBLCompanyID} And IsDeletedTransaction=0) And ItemID IN (Select ItemID From ItemMaster Where Manufecturer='{mill}' And CompanyID={GBLCompanyID} And IsDeletedTransaction=0) And IFNULL(IsDeletedTransaction,0)<>1 Order By Finish";
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(quality) && string.IsNullOrEmpty(gsm) && string.IsNullOrEmpty(mill))
                        {
                            str = $"Select Distinct Finish From ItemMaster Where ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where CompanyID={GBLCompanyID} And ItemGroupNameID IN(-1,-2)) And CompanyID={GBLCompanyID} And Isnull(IsDeletedTransaction,0)<>1 And ISNULL(Finish,'')<>'' Order By Finish";
                        }
                        else
                        {
                            str = $"Select Distinct Finish From ItemMaster Where ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where CompanyID={GBLCompanyID} And ItemGroupNameID IN(-1,-2)) And ItemID IN (Select ItemID From ItemMaster Where GSM={gsm} And CompanyID={GBLCompanyID} And IsDeletedTransaction=0) And ItemID IN (Select ItemID From ItemMaster Where Quality='{quality}' And CompanyID={GBLCompanyID} And IsDeletedTransaction=0) And ItemID IN (Select ItemID From ItemMaster Where Manufecturer='{mill}' And CompanyID={GBLCompanyID} And IsDeletedTransaction=0) And Isnull(IsDeletedTransaction,0)<>1 Order By Finish";
                        }
                    }
                    DBConnection.FillDataTable(ref dtFinish, str);
                }
                else
                {
                    str = $"Select Distinct Mill from PaperMaster {quality} And CompanyID={GBLCompanyID} Union Select Distinct Mill from ReelMaster {quality} And CompanyID={GBLCompanyID} And Isnull(IsDeletedTransaction,0)<>1 Order by Mill Asc";
                    DBConnection.FillDataTable(ref dtMill, str);

                    str = $"select Distinct Finish from PaperMaster {quality} And CompanyID={GBLCompanyID} Union select Distinct Finish from ReelMaster {quality} And CompanyID={GBLCompanyID} And Isnull(IsDeletedTransaction,0)<>1 Order by Finish Asc";
                    DBConnection.FillDataTable(ref dtFinish, str);

                    str = $"Select Distinct GSM from PaperMaster {quality} And CompanyID={GBLCompanyID} Union Select Distinct GSM From ReelMaster {quality} And CompanyID={GBLCompanyID} And Isnull(IsDeletedTransaction,0)<>1 Order By GSM Asc";
                    DBConnection.FillDataTable(ref dtGSM, str);
                }

                dtMill.TableName = "TblMill";
                dtGSM.TableName = "TblGSM";
                dtFinish.TableName = "TblFinish";

                DataSet dataSet = new DataSet();
                dataSet.Merge(dtMill);
                dataSet.Merge(dtGSM);
                dataSet.Merge(dtFinish);

                data.Message = DBConnection.ConvertDataSetsToJsonString(dataSet);
                return Ok(JsonConvert.SerializeObject(data.Message));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("defaultoperations/{categoryID}/{contName}")]
        public IHttpActionResult LoadDefaultOperations(int categoryID, string contName)
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                str = DBConnection.Version;
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                if (str == "New")
                {
                    if (DBType == "MYSQL")
                    {
                        str = $"SELECT DISTINCT PM.ProcessID, REPLACE(NULLIF (PM.ProcessName, ''), '\"', '') AS ProcessName, ROUND(IFNULL(NULLIF (PM.Rate, ''), 0), 4) AS Rate, '' AS RateFactor,CPA.ID " +
                              $"FROM ProcessMaster AS PM Inner Join CategoryWiseProcessAllocation As CPA On CPA.ProcessID=PM.ProcessID And CPA.CompanyID=PM.CompanyID And IFNULL(CPA.IsDeletedTransaction,0)=0 Inner Join CategoryContentAllocationMaster As PCM On PCM.CategoryID=CPA.CategoryID And PCM.ContentID=CPA.ContentID And CPA.CompanyID=PCM.CompanyID And IFNULL(PCM.IsDeletedTransaction,0)=0 Inner Join ContentMaster As CM On CM.ContentID=PCM.ContentID And CM.CompanyID=CPA.CompanyID WHERE (IFNULL(PM.IsDeletedTransaction, 0) = 0) AND (PM.CompanyID = {GBLCompanyID} ) And CPA.CategoryID={categoryID} And CM.ContentName='{contName}' ORDER BY CPA.ID";
                    }
                    else
                    {
                        str = $"SELECT PM.ProcessID, REPLACE(NULLIF (PM.ProcessName, ''), '\"', '') AS ProcessName, ROUND(ISNULL(NULLIF (PM.Rate, ''), 0), 4) AS Rate, '' AS RateFactor,Max(CPA.ID) AS ID " +
                              $"FROM ProcessMaster AS PM Inner Join CategoryWiseProcessAllocation As CPA On CPA.ProcessID=PM.ProcessID And CPA.CompanyID=PM.CompanyID And Isnull(CPA.IsDeletedTransaction,0)=0 Inner Join CategoryContentAllocationMaster As PCM On PCM.CategoryID=CPA.CategoryID And PCM.ContentID=CPA.ContentID And CPA.CompanyID=PCM.CompanyID And Isnull(PCM.IsDeletedTransaction,0)=0 Inner Join ContentMaster As CM On CM.ContentID=PCM.ContentID And CM.CompanyID=CPA.CompanyID Inner Join DepartmentMaster As DM On DM.DepartmentID=PM.DepartmentID And PM.CompanyID=DM.CompanyID " +
                              $"WHERE (ISNULL(PM.IsDeletedTransaction, 0) = 0) AND (PM.CompanyID = {GBLCompanyID} ) And CPA.CategoryID={categoryID} And CM.ContentName='{contName}' Group BY PM.ProcessID,PM.ProcessName,PM.Rate,DM.SequenceNo ORDER BY DM.SequenceNo";
                    }
                    DBConnection.FillDataTable(ref dataTable, str);
                }
                else
                {
                    str = $"Select Distinct OperationId, Replace(Nullif(OperationName,''),'\"','') as OperationName, Round(Isnull(Nullif(Rate,''),0),4) As Rate From OperationMaster WHERE CompanyId = {GBLCompanyID} Order By OperationName Asc";
                    DBConnection.FillDataTable(ref dataTable, str);
                }

                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return Ok(JsonConvert.SerializeObject(data.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        /// <summary>
        /// Plan Operations Slabs Name
        /// </summary>
        /// <returns>JSON string with operations slabs data</returns>
        [HttpGet]
        [Route("LoadOperationsSlabs")]
        public string LoadOperationsSlabs()
        {
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
            str = DBConnection.Version;
            DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

            if (str == "New")
            {
                if (DBType == "MYSQL")
                {
                    str = "Select Distinct ProcessID,RateFactor From ProcessMasterSlabs Where IFNULL(RateFactor,'')<>'' And IsLocked=0 And IFNULL(IsDeletedTransaction,0)<>1 And CompanyId = " + GBLCompanyID + "";
                }
                else
                {
                    str = "Select Distinct ProcessID,RateFactor From ProcessMasterSlabs Where ISNULL(RateFactor,'')<>'' And IsLocked=0 And Isnull(IsDeletedTransaction,0)<>1 And CompanyId = " + GBLCompanyID + "";
                }
            }

            DBConnection.FillDataTable(ref dataTable, str);
            data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
            return JsonConvert.SerializeObject(data.Message);
        }

        [HttpPost]
        [Route("GetDieMaster")]
        public string GetDieMaster([FromBody] JObject jsonData)
        {
            if (jsonData == null)
            {
                return "No JSON data received";
            }
            DataTable DT_JSON_Data = new DataTable();
            DBConnection.ConvertObjectToDatatable(jsonData, ref DT_JSON_Data, ref str);
            if (DT_JSON_Data == null || DT_JSON_Data.Rows.Count == 0)
            {

                return "Invalid or empty JSON data received";
            }

            str = DBConnection.Version;
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);

            str = "Select TM.ToolID,Nullif(TM.ToolCode,'') as ToolCode,Nullif(TM.ToolName,'') as ToolName,Nullif(TM.ToolDescription,'') as ToolDescription,Isnull(TM.LedgerName,'-') As LedgerName,TM.SizeL,TM.SizeW,TM.SizeH,ISNULL(TM.UpsL,0) as UpsL,ISNULL(TM.UpsW,0) as UpsW,ISNULL(TM.TotalUps,0) as TotalUps,TM.Manufecturer From ToolMaster AS TM INNER JOIN ToolGroupMaster AS TG ON TM.ToolGroupID =TG.ToolGroupID Where TG.ToolGroupNameID IN(-3,-8) " +
                  "and (TM.SizeL >= (" + DT_JSON_Data.Rows[0]["L"] + " - " + DT_JSON_Data.Rows[0]["L1"] + ") and TM.SizeL <= (" + DT_JSON_Data.Rows[0]["L"] + " + " + DT_JSON_Data.Rows[0]["L1"] + "))  " +
                  "and (TM.SizeW >= (" + DT_JSON_Data.Rows[0]["W"] + " - " + DT_JSON_Data.Rows[0]["W1"] + ") and TM.SizeW <= (" + DT_JSON_Data.Rows[0]["W"] + " + " + DT_JSON_Data.Rows[0]["W1"] + "))  " +
                  "and (TM.SizeH >= (" + DT_JSON_Data.Rows[0]["H"] + " - " + DT_JSON_Data.Rows[0]["H1"] + ") and TM.SizeH <= (" + DT_JSON_Data.Rows[0]["H"] + " + " + DT_JSON_Data.Rows[0]["H1"] + ")) Order BY TM.ToolCode ";

            DBConnection.FillDataTable(ref dataTable, str);
            data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
            return JsonConvert.SerializeObject(data.Message);
        }

        /// <summary>
        /// Load Book Contents
        /// </summary>
        /// <returns>JSON string with book contents data</returns>
        [HttpGet]
        [Route("LoadBookContents")]
        public string LoadBookContents()
        {
            //Response.Clear();
            //Response.ContentType = "application/json";
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
            str = " Select * From ContentMaster Where ContentsCategory='Book' And CompanyId = " + GBLCompanyID + "  Order By ContentName Asc  ";

            DBConnection.FillDataTable(ref dataTable, str);
            data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
            return JsonConvert.SerializeObject(data.Message);
        }

        /// <summary>
        /// Save Booking Data
        /// </summary>
        /// <param name="TblBooking">Details</param>
        /// <param name="TblPlanning">Plan data</param>
        /// <param name="TblOperations">Operation data</param>
        /// <param name="TblContentForms">Content forms data</param>
        /// <param name="CostingData">Costing data</param>
        /// <param name="FlagSave">Save flag</param>
        /// <param name="BookingNo">Booking number</param>
        /// <param name="ObjShippers">Shipper objects</param>
        /// <param name="ArrObjAttc">Attachment objects</param>
        /// <param name="Tblonetimecharges">One time charges</param>
        /// <param name="TblCorrugationPlyDetails">Corrugation ply details</param>
        /// <param name="TblAllocatedMaterials">Allocated materials</param>
        /// <param name="TblMaterialCostParams">Material cost parameters</param>
        /// <param name="TblContentSpecData">Content specification data</param>
        /// <param name="TblAllocatedMaterialLayers">Allocated material layers</param>
        /// <returns>Booking ID or error message</returns>
        [HttpPost]
        [Route("saveQuotationData")]
        public string SaveQuotationData([FromBody] SaveQuotationRequest request)
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                GBLProductionUnitID = Convert.ToString(HttpContext.Current.Session["ProductionUnitID"]);
                GBLUserID = Convert.ToString(HttpContext.Current.Session["UserID"]);
                FYear = DBConnection.GblFYear;
                GBLBranchID = DBConnection.GblBranchId;
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                DataTable dt = new DataTable();
                long BookingID;
                long MaxQuotationNo, RevisionNo;
                string AddColName, AddColValue, TableName, str;
                AddColName = "";
                AddColValue = "";

                SqlTransaction objtrans;
                SqlConnection con = DBConnection.OpenConnection();
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }

                // Helper to check if object is not empty
                Func<object, bool> IsObjectNotEmpty = (obj) =>
                {
                    try
                    {
                        if (obj == null)
                            return false;

                        string json = JsonConvert.SerializeObject(obj).Trim();

                        if (string.IsNullOrWhiteSpace(json) || json == "{}" || json == "[]" || json == "[{}]")
                            return false;

                        if (obj is JToken token && !token.HasValues)
                            return false;

                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                };

                TableName = "JobBooking";
                BookingID = DBConnection.GenerateMaxVoucherNo("JobBooking", "BookingId");

                if (request.FlagSave == "True" || request.FlagSave == "true")
                {
                    MaxQuotationNo = DBConnection.GenerateMaxVoucherNo(TableName, "MaxBookingNo", "Where CompanyId = " + GBLCompanyID + " AND Isnull(IsDeletedTransaction,0)=0");
                    RevisionNo = 0;
                }
                else
                {
                    RevisionNo = DBConnection.GenerateMaxVoucherNo(TableName, "RevisionNo", "Where MaxBookingNo = " + request.BookingNo + " and CompanyId = " + GBLCompanyID + " AND Isnull(IsDeletedTransaction,0)=0");
                    MaxQuotationNo = Convert.ToInt64(request.BookingNo);
                }

                string BookingNo = MaxQuotationNo + "." + RevisionNo;

                if (DBConnection.CheckAuthories("/estimation", Convert.ToInt32(GBLUserID), Convert.ToInt32(GBLCompanyID), "CanSave", BookingNo) == false)
                {
                    return "You are not authorized to save";
                }

                objtrans = con.BeginTransaction();

                // ---------------- JOB BOOKING ----------------
                if (IsObjectNotEmpty(request.TblBooking))
                {
                    TableName = "JobBooking";
                    AddColName = "BookingNo,MaxBookingNo,RevisionNo,CompanyId,BranchID,CreatedBy,FYear,IsApproved,CreatedDate,ModifiedDate,QuotedByUserID,ProductionUnitID";
                    AddColValue = "'" + BookingNo + "','" + MaxQuotationNo + "','" + RevisionNo + "','" + GBLCompanyID + "','" + GBLBranchID + "'," + GBLUserID + ",'" + FYear + "',0,getdate(),getdate()," + GBLUserID + "," + GBLProductionUnitID + "";
                    BookingID = Convert.ToInt64(DBConnection.InsertDatatableToDatabase(request.TblBooking, TableName, AddColName, AddColValue, ref con, ref objtrans));

                    if (Convert.ToInt32(BookingID) <= 0)
                    {
                        objtrans.Rollback();
                        objtrans.Dispose();
                        return "Error:500 Main" + BookingID;
                    }
                }

                // ---------------- JOB BOOKING CONTENTS ----------------
                if (IsObjectNotEmpty(request.TblPlanning))
                {
                    AddColName = "BookingId,BookingNo,CreatedDate,CompanyId,BranchID";
                    AddColValue = "" + BookingID + ",'" + BookingNo + "',getdate()," + GBLCompanyID + "," + GBLBranchID + "";
                    TableName = "JobBookingContents";
                    str = DBConnection.InsertDatatableToDatabase(request.TblPlanning, TableName, AddColName, AddColValue, ref con, ref objtrans);
                    if (!IsNumeric(str))
                    {
                        objtrans.Rollback();
                        objtrans.Dispose();
                        return "Error:500 Contents" + str;
                    }
                }

                // ---------------- JOB BOOKING PROCESS ----------------
                if (IsObjectNotEmpty(request.TblOperations))
                {
                    TableName = "JobBookingProcess";
                    str = DBConnection.AddToDatabaseOperation(request.TblOperations, TableName, AddColName, AddColValue, ref con, ref objtrans);
                    if (!IsNumeric(str))
                    {
                        objtrans.Rollback();
                        objtrans.Dispose();
                        return "Error:500 Process" + str;
                    }
                }
                AddColName = "BookingId,CreatedDate,CompanyId,BranchID";
                AddColValue = "" + BookingID + ",getdate()," + GBLCompanyID + "," + GBLBranchID + "";
                // ---------------- CONTENT FORMS ----------------
                if (IsObjectNotEmpty(request.TblContentForms))
                {

                    TableName = "JobBookingContentBookForms";
                    str = DBConnection.AddToDatabaseOperation(request.TblContentForms, TableName, AddColName, AddColValue, ref con, ref objtrans);
                    if (str != "200")
                    {
                        objtrans.Rollback();
                        objtrans.Dispose();
                        return "Error:500 Forms" + str;
                    }
                }
                // ---------------- CORRUGATION ----------------
                if (IsObjectNotEmpty(request.TblCorrugationPlyDetails))
                {
                    TableName = "JobBookingCorrugation";
                    str = DBConnection.AddToDatabaseOperation(request.TblCorrugationPlyDetails, TableName, AddColName, AddColValue, ref con, ref objtrans);
                    if (str != "200")
                    {
                        objtrans.Rollback();
                        objtrans.Dispose();
                        return "Error:500 Corrugation Ply" + str;
                    }
                }

                // ---------------- COSTINGS ----------------
                if (IsObjectNotEmpty(request.CostingData))
                {
                    TableName = "JobBookingCostings";
                    str = DBConnection.InsertDatatableToDatabase(request.CostingData, TableName, AddColName, AddColValue, ref con, ref objtrans);
                    if (!IsNumeric(str))
                    {
                        objtrans.Rollback();
                        objtrans.Dispose();
                        return "Error:500 Costings" + str;
                    }
                }

                // ---------------- MATERIAL COST ----------------
                if (IsObjectNotEmpty(request.ObjShippers))
                {
                    AddColName = "BookingId,CreatedDate,CreatedBy,CompanyId,BranchID";
                    AddColValue = "" + BookingID + ",getdate()," + GBLUserID + "," + GBLCompanyID + "," + GBLBranchID + "";
                    TableName = "JobBookingMaterialCost";
                    str = DBConnection.InsertDatatableToDatabase(request.ObjShippers, TableName, AddColName, AddColValue, ref con, ref objtrans);
                    if (!IsNumeric(str))
                    {
                        objtrans.Rollback();
                        objtrans.Dispose();
                        return "Error:500 Materials" + str;
                    }
                }

                // ---------------- ATTACHMENTS ----------------
                if (IsObjectNotEmpty(request.ArrObjAttc))
                {
                    AddColName = "BookingId,CreatedDate,CreatedBy,CompanyID,BranchID";
                    AddColValue = "" + BookingID + ",getdate()," + GBLUserID + "," + GBLCompanyID + "," + GBLBranchID + "";
                    TableName = "JobBookingAttachments";
                    str = DBConnection.InsertDatatableToDatabase(request.ArrObjAttc, TableName, AddColName, AddColValue, ref con, ref objtrans);
                    if (!IsNumeric(str))
                    {
                        objtrans.Rollback();
                        objtrans.Dispose();
                        return "Error:500 Attachments" + str;
                    }
                }

                // ---------------- ONE-TIME CHARGES ----------------
                if (IsObjectNotEmpty(request.Tblonetimecharges))
                {
                    AddColName = "BookingId,CreatedDate,CreatedBy,CompanyID,BranchID";
                    AddColValue = "" + BookingID + ",getdate()," + GBLUserID + "," + GBLCompanyID + "," + GBLBranchID + "";
                    TableName = "JobBookingOnetimeCharges";
                    str = DBConnection.InsertDatatableToDatabase(request.Tblonetimecharges, TableName, AddColName, AddColValue, ref con, ref objtrans);
                    if (!IsNumeric(str))
                    {
                        objtrans.Rollback();
                        objtrans.Dispose();
                        return "Error:500 Onetime Charges" + str;
                    }
                }
                AddColName = "BookingId,CreatedDate,CompanyId,BranchID";
                AddColValue = "" + BookingID + ",getdate()," + GBLCompanyID + "," + GBLBranchID + "";
                // ---------------- MATERIAL ALLOCATED ----------------
                if (IsObjectNotEmpty(request.TblAllocatedMaterials))
                {
                    TableName = "JobBookingProcessMaterialRequirement";
                    str = DBConnection.AddToDatabaseOperation(request.TblAllocatedMaterials, TableName, AddColName, AddColValue, ref con, ref objtrans);
                    if (str != "200")
                    {
                        objtrans.Rollback();
                        objtrans.Dispose();
                        return "Error:500 Allocated Materials" + str;
                    }
                }

                // ---------------- MATERIAL PARAMETERS ----------------
                if (IsObjectNotEmpty(request.TblMaterialCostParams))
                {
                    TableName = "JobBookingProcessMaterialParameterDetail";
                    str = DBConnection.AddToDatabaseOperation(request.TblMaterialCostParams, TableName, AddColName, AddColValue, ref con, ref objtrans);
                    if (str != "200")
                    {
                        objtrans.Rollback();
                        objtrans.Dispose();
                        return "Error:500 Material Parameters" + str;
                    }
                }

                // ---------------- LAYER DETAILS ----------------
                if (IsObjectNotEmpty(request.TblAllocatedMaterialLayers))
                {
                    TableName = "JobBookingContentsLayerDetail";
                    str = DBConnection.AddToDatabaseOperation(request.TblAllocatedMaterialLayers, TableName, AddColName, AddColValue, ref con, ref objtrans);
                    if (str != "200")
                    {
                        objtrans.Rollback();
                        objtrans.Dispose();
                        return "Error:500 Layers" + str;
                    }
                }

                // ---------------- CONTENT SPECS ----------------
                if (IsObjectNotEmpty(request.TblContentSpecData))
                {
                    AddColName = "BookingId,CompanyId";
                    AddColValue = "" + BookingID + "," + GBLCompanyID + "";
                    TableName = "JobBookingContentsSpecification";
                    str = DBConnection.AddToDatabaseOperation(request.TblContentSpecData, TableName, AddColName, AddColValue, ref con, ref objtrans);
                    if (str != "200")
                    {
                        objtrans.Rollback();
                        objtrans.Dispose();
                        return "Error:500 Content Specs" + str;
                    }
                }

                objtrans.Commit();

                // Execute approval SP
                str = DBConnection.ExecuteNonSQLQuery("EXEC CostEstimationApprovalProcess  '" + BookingID + "'," + GBLCompanyID + "");
                if (str != "Success")
                {
                    return str;
                }

                return BookingID.ToString();
            }
            catch (Exception ex)
            {
                return "Error:500 Exception " + ex.Message;
            }
        }

        /// <summary>
        /// Delete booking data (private helper method)
        /// </summary>
        /// <param name="BKID">Booking ID to delete</param>
        private void DeleteBookingData(int BKID)
        {
            DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);
            SqlConnection conn = new SqlConnection();

            try
            {
                conn = DBConnection.OpenConnection();
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                str = "Delete From JobBooking Where BookingId=" + BKID;
                var cmd = new SqlCommand(str, conn);
                cmd.ExecuteNonQuery();

                str = "Delete From JobBookingContents Where BookingId=" + BKID;
                cmd = new SqlCommand(str, conn);
                cmd.ExecuteNonQuery();

                str = "Delete From JobBookingProcess Where BookingId=" + BKID;
                cmd = new SqlCommand(str, conn);
                cmd.ExecuteNonQuery();

                str = "Delete From JobBookingContentBookForms Where BookingId=" + BKID;
                cmd = new SqlCommand(str, conn);
                cmd.ExecuteNonQuery();

                str = "Delete From JobBookingCostings Where BookingId=" + BKID;
                cmd = new SqlCommand(str, conn);
                cmd.ExecuteNonQuery();

                str = "Delete From JobBookingAttachments Where BookingId=" + BKID;
                cmd = new SqlCommand(str, conn);
                cmd.ExecuteNonQuery();

                conn.Close();
            }
            catch (Exception ex)
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// Select Box Client
        /// </summary>
        /// <returns>JSON string with client data</returns>
        [HttpGet]
        [Route("GetSbClient")]
        public string GetSbClient()
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                GBLUserID = Convert.ToString(HttpContext.Current.Session["UserID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                if (DBType == "MYSQL")
                {
                    str = "Select Distinct Nullif(Replace(LedgerName ,'\"',''),'') as LedgerName,LedgerID As LedgerId From LedgerMaster As LM Inner Join LedgerGroupMaster As LGM On LGM.LedgerGroupID=LM.LedgerGroupID AND LM.CompanyID =LGM.CompanyID  Where LGM.LedgerGroupNameID=24 And IFNULL(LM.IsDeletedTransaction,0)<>1 And LM.CompanyId = " + GBLCompanyID + " Order By LedgerName";
                }
                else
                {
                    str = "SELECT ISNULL(NULLIF(REPLACE(LM.LedgerName, '\"', ''), ''),'') AS LedgerName,LM.LedgerID AS LedgerId,ISNULL(LM.CreditDays,0) as CreditDays,ISNULL(LM.IsLead,0) as IsLead FROM LedgerMaster AS LM INNER JOIN LedgerGroupMaster AS LGM ON LGM.LedgerGroupID = LM.LedgerGroupID WHERE (LGM.LedgerGroupNameID = 24) AND (ISNULL(LM.IsDeletedTransaction, 0) <> 1) And LM.CompanyId = " + GBLCompanyID + " AND   Isnull(LM.LedgerName,'') <>'' AND LM.RefSalesRepresentativeID IN(Select OperatorID from UserOperatorAllocation Where UserID = " + GBLUserID + " AND Isnull(IsDeletedTransaction,0)=0)  Order BY LM.LedgerName";
                }

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);


                return JsonConvert.SerializeObject(data.Message);
            }
            catch (Exception ex)
            {
                return "500";
            }
        }

        /// <summary>
        /// Select Box Category
        /// </summary>
        /// <returns>JSON string with category data</returns>
        [HttpGet]
        [Route("GetSbCategory")]
        public string GetSbCategory()
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);
                GBLUserID = Convert.ToString(HttpContext.Current.Session["UserID"]);
                DataTable dtSegments = new DataTable();

                if (DBType == "MYSQL")
                {
                    str = " Select Distinct CategoryId,Nullif(Replace(CategoryName,'\"',''),'') as CategoryName From CategoryMaster Where CompanyId = " + GBLCompanyID + " And IFNULL(IsDeletedTransaction,0)<>1   Order By CategoryName ";
                }
                else
                {
                    DBConnection.FillDataTable(ref dtSegments, "Select Distinct SegmentID From UserSegmentAllocation Where UserID=" + GBLUserID + " AND Isnull(IsDeletedTransaction,0)=0 AND CompanyID=" + GBLCompanyID + "");

                    if (dtSegments.Rows.Count > 0)
                    {
                        str = "Select Distinct CM.CategoryId,SM.SegmentID,Nullif(Replace(CM.CategoryName,'\"',''),'') as CategoryName,SM.SegmentName " +
                              " From CategoryMaster AS CM INNER JOIN SegmentMaster AS SM ON SM.SegmentID=CM.SegmentID Where CM.CompanyID = " + GBLCompanyID + " And Isnull(CM.IsDeletedTransaction,0)<>1   AND CM.SegmentID IN(Select Distinct SegmentID From UserSegmentAllocation Where UserID=" + GBLUserID + " AND Isnull(IsDeletedTransaction,0)=0 AND CompanyID=" + GBLCompanyID + ") Order By CategoryName";
                    }
                    else
                    {
                        str = " Select Distinct CM.CategoryId,SM.SegmentID,Nullif(Replace(CM.CategoryName,'\"',''),'') as CategoryName,SM.SegmentName From CategoryMaster AS CM INNER JOIN SegmentMaster AS SM ON SM.SegmentID=CM.SegmentID Where CM.CompanyId = " + GBLCompanyID + " And Isnull(CM.IsDeletedTransaction,0)<>1   Order By CategoryName ";
                    }
                    dtSegments.Dispose();
                }

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return JsonConvert.SerializeObject(data.Message);
            }
            catch (Exception ex)
            {
                return "500";
            }
        }

        /// <summary>
        /// Get Flute Master data
        /// </summary>
        /// <returns>JSON string with flute data</returns>
        [HttpGet]
        [Route("GetFlute")]
        public string GetFlute()
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                str = "Select Distinct FluteName,TakeupFactor From FluteMaster ";

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return JsonConvert.SerializeObject(data.Message);
            }
            catch (Exception ex)
            {
                return "500";
            }
        }

        /// <summary>
        /// Get Reel Master data with width filtering
        /// </summary>
        /// <param name="ReqDeckle">Required deckle</param>
        /// <param name="WIdthPlus">Width plus tolerance</param>
        /// <param name="WIdthMinus">Width minus tolerance</param>
        /// <returns>JSON string with reel master data</returns>
        [HttpGet]
        [Route("GetReelMaster/{ReqDeckle}/{WIdthPlus}/{WIdthMinus}")]
        public string GetReelMaster(string ReqDeckle, string WIdthPlus, string WIdthMinus)
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                if ((!string.IsNullOrWhiteSpace(WIdthPlus) && WIdthPlus != "0") || (!string.IsNullOrWhiteSpace(WIdthMinus) && WIdthMinus != "0"))
                {
                    double reqDeckleVal = Convert.ToDouble(ReqDeckle);
                    double widthPlusVal = IsNumeric(WIdthPlus) ? Convert.ToDouble(WIdthPlus) : 0;
                    double widthMinusVal = IsNumeric(WIdthMinus) ? Convert.ToDouble(WIdthMinus) : 0;
                    double lowerLimit = reqDeckleVal - widthMinusVal;
                    double upperLimit = reqDeckleVal + widthPlusVal;

                    str = " SELECT IM.ItemID, IM.ItemCode, IM.ItemName, IM.GSM, IM.BF, IM.PhysicalStock, IM.StockUnit, IM.SizeW, IM.EstimationRate " +
                          "FROM ItemMaster AS IM " +
                          "INNER JOIN ItemGroupMaster AS IG ON IG.ItemGroupID = IM.ItemGroupID " +
                          "WHERE IG.ItemGroupNameID = -2 AND IM.CompanyID = " + GBLCompanyID + " " +
                          "AND IM.SizeW >= " + lowerLimit + " AND IM.SizeW <= " + upperLimit + " " +
                          "AND ISNULL(IM.IsDeletedTransaction, 0) = 0 " +
                          "ORDER BY IM.ItemCode";
                }
                else
                {
                    str = " Select IM.ItemID, IM.ItemCode, IM.ItemName, IM.GSM, IM.BF,IM. PhysicalStock,   IM.StockUnit, IM.SizeW,  IM.EstimationRate From ItemMaster as IM INNER JOIN ItemGroupMaster AS IG ON IG.ItemGroupID=IM.ItemGroupID Where  IG.ItemGroupNameID = -2 and IM.CompanyID = " + GBLCompanyID + " ANd IM.SizeW >= '" + ReqDeckle + "' AND Isnull(IM.IsDeletedTransaction,0)=0  Order BY IM.ItemCode ";
                }

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return JsonConvert.SerializeObject(data.Message);
            }
            catch (Exception ex)
            {
                return "500";
            }
        }

        // Select Box Sales Person - Added By Minesh Jain on 29-July-2022
        [HttpGet]
        [Route("getsbsalesperson")]
        public string GetSbSalesPerson()
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                GBLUserID = Convert.ToString(HttpContext.Current.Session["UserID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                str = "Select Distinct LM.LedgerID AS EmployeeID,Nullif(Replace(LM.LedgerName,'\"',''),'') as EmployeeName From LedgerMaster AS LM INNER JOIN LedgerGroupMaster AS LG ON LG.LedgerGroupID=LM.LedgerGroupID AND LG.CompanyID=LM.CompanyID Where LG.LedgerGroupNameID=27 AND LM.DepartmentID=-50 And Isnull(LM.IsDeletedTransaction,0)<>1  AND LM.LedgerID IN(Select OperatorID from UserOperatorAllocation Where UserID =" + GBLUserID + " AND Isnull(IsDeletedTransaction,0)=0) Order By EmployeeName ";

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return JsonConvert.SerializeObject(data.Message);
            }
            catch (Exception ex)
            {
                return "500";
            }
        }

        [HttpGet]
        [Route("getsbsalespersondata/{ledgerID}")]
        public string GetSbSalesPersonData(string ledgerID)
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                GBLUserID = Convert.ToString(HttpContext.Current.Session["UserID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                if (DBType == "MYSQL")
                {
                    str = "Select Distinct LM.LedgerID,LM.RefSalesRepresentativeID AS EmployeeID,Nullif(LMM.LedgerName,'') as EmployeeName From LedgerMaster AS LM Inner Join LedgerMaster as LMM on LMM.LedgerID  = ISNULL(LM.RefSalesRepresentativeID,0)  Where ISNULL(LM.IsDeletedTransaction,0)<>1 AND  LM.LedgerID=' " + ledgerID + " '";
                }
                else
                {
                    str = "Select Distinct LM.LedgerID,LM.RefSalesRepresentativeID AS EmployeeID,Nullif(LMM.LedgerName,'') as EmployeeName From LedgerMaster AS LM Inner Join LedgerMaster as LMM on LMM.LedgerID  = ISNULL(LM.RefSalesRepresentativeID,0)  Where ISNULL(LM.IsDeletedTransaction,0)<>1 AND LM.LedgerID=' " + ledgerID + " '";
                }

                DBConnection.FillDataTable(ref dataTable, str);
                if (dataTable.Rows.Count <= 0)
                {
                    dataTable.Clear();
                    str = "Select Distinct 0 AS LedgerID,LM.LedgerID AS EmployeeID,Nullif(Replace(LM.LedgerName,'\"',''),'') as EmployeeName From LedgerMaster AS LM INNER JOIN LedgerGroupMaster AS LG ON LG.LedgerGroupID=LM.LedgerGroupID AND LG.CompanyID=LM.CompanyID Where LG.LedgerGroupNameID=27 AND LM.DepartmentID=-50 AND LM.CompanyID = " + GBLCompanyID + " And Isnull(LM.IsDeletedTransaction,0)<>1  AND LM.LedgerID IN(Select OperatorID from UserOperatorAllocation Where UserID =" + GBLUserID + " AND CompanyID=" + GBLCompanyID + " AND Isnull(IsDeletedTransaction,0)=0) Order By EmployeeName ";
                    DBConnection.FillDataTable(ref dataTable, str);
                }
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return JsonConvert.SerializeObject(data.Message);
            }
            catch (Exception ex)
            {
                return "500";
            }
        }

        // Get All Active Contents
        [HttpGet]
        [Route("getallcontents")]
        public string GetAllContents()
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);
                GBLUserID = Convert.ToString(HttpContext.Current.Session["UserID"]);
                string strQry = "";
                DataTable dtSegments = new DataTable();
                DataTable dtSegmentContents = new DataTable();

                DBConnection.FillDataTable(ref dtSegments, "Select Distinct SM.SegmentID,SM.SegmentName From SegmentMaster AS SM INNER JOIN UserSegmentAllocation AS US ON US.SegmentID=SM.SegmentID Where Isnull(SM.IsDeletedTransaction,0)=0 AND Isnull(US.IsDeletedTransaction,0)=0 AND US.UserID=" + GBLUserID + " AND SM.CompanyID=" + GBLCompanyID + " Order By SM.SegmentName");

                if (dtSegments.Rows.Count > 0)
                {
                    str = "Select  Distinct C.ContentID,Nullif(Replace(C.ContentName,'\"',''),'') As ContentName,  Nullif(Replace(C.ContentCaption,'\"',''),'') As ContentCaption,Nullif(Replace(C.ContentOpenHref,'\"',''),'') As ContentOpenHref,  Nullif(Replace(C.ContentClosedHref,'\"',''),'') As ContentClosedHref,  Nullif(Replace(C.ContentSizes,'\"',''),'') As ContentSizes,Nullif(Replace(C.ContentDomainType,'\"',''),'') As ContentDomainType,CM.CategoryName,SM.SegmentName From ContentMaster AS C INNER JOIN CategoryContentAllocationMaster AS CCA ON CCA.ContentID=C.ContentID AND Isnull(CCA.IsDeletedTransaction,0)=0 INNER JOIN CategoryMaster AS CM ON CM.CategoryID=CCA.CategoryID  AND isnull(CM.IsDeletedTransaction,0)=0 INNER JOIN SegmentMaster AS SM ON SM.SegmentID=CM.SegmentID AND isnull(SM.IsDeletedTransaction,0)=0 Where Isnull(C.IsActive,0)=1 And C.CompanyId = " + GBLCompanyID + " AND SM.SegmentID IN(Select Distinct SegmentID From UserSegmentAllocation Where CompanyID=" + GBLCompanyID + " AND UserID=" + GBLUserID + " AND Isnull(IsDeletedTransaction,0)=0) Order By SegmentName, CategoryName,ContentName ";

                    DBConnection.FillDataTable(ref dtSegmentContents, str);
                    if (dtSegmentContents.Rows.Count == 0)
                    {
                        str = "Select  ContentID,Nullif(Replace(ContentName,'\"',''),'') As ContentName,  Nullif(Replace(ContentCaption,'\"',''),'') As ContentCaption, Nullif(Replace(ContentOpenHref,'\"',''),'') As ContentOpenHref,  Nullif(Replace(ContentClosedHref,'\"',''),'') As ContentClosedHref,  Nullif(Replace(ContentSizes,'\"',''),'') As ContentSizes,Nullif(Replace(ContentDomainType,'\"',''),'') As ContentDomainType From ContentMaster Where Isnull(IsActive,0)=1 And CompanyId = " + GBLCompanyID + " Order By SequencNo ";
                    }
                    else
                    {
                        dataTable = dtSegmentContents;
                    }
                    dtSegmentContents.Dispose();
                }
                else
                {
                    if (DBType == "MYSQL")
                    {
                        str = "Select  ContentID,Nullif(Replace(ContentName,'\"',''),'') As ContentName,  Nullif(Replace(ContentCaption,'\"',''),'') As ContentCaption, Nullif(Replace(ContentOpenHref,'\"',''),'') As ContentOpenHref,  Nullif(Replace(ContentClosedHref,'\"',''),'') As ContentClosedHref,  Nullif(Replace(ContentSizes,'\"',''),'') As ContentSizes,Nullif(Replace(ContentDomainType,'\"',''),'') As ContentDomainType From ContentMaster Where IFNULL(IsActive,0)=1 And CompanyId = " + GBLCompanyID + " Order By SequencNo ";
                    }
                    else
                    {
                        str = "Select  ContentID,Nullif(Replace(ContentName,'\"',''),'') As ContentName,  Nullif(Replace(ContentCaption,'\"',''),'') As ContentCaption, Nullif(Replace(ContentOpenHref,'\"',''),'') As ContentOpenHref,  Nullif(Replace(ContentClosedHref,'\"',''),'') As ContentClosedHref,  Nullif(Replace(ContentSizes,'\"',''),'') As ContentSizes,Nullif(Replace(ContentDomainType,'\"',''),'') As ContentDomainType From ContentMaster Where Isnull(IsActive,0)=1 And CompanyId = " + GBLCompanyID + " Order By SequencNo ";
                    }
                    DBConnection.FillDataTable(ref dataTable, str);
                }

                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return JsonConvert.SerializeObject(data.Message);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // Get Contents Sizes
        [HttpGet]
        [Route("getcontentsize/{contName}")]
        public string GetContentSize(string contName)
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                if (DBType == "MYSQL")
                {
                    str = "Select ContentID,Nullif(Replace(ContentName,'\"',''),'') As ContentName, Nullif(Replace(ContentSizes,'\"',''),'') As ContentSizes From ContentMaster Where IFNULL(IsActive,0)=1 And ContentName='" + contName + "' And CompanyId = " + GBLCompanyID + " Order By SequencNo ";
                }
                else
                {
                    str = "Select ContentID,Nullif(Replace(ContentName,'\"',''),'') As ContentName, Nullif(Replace(ContentSizes,'\"',''),'') As ContentSizes From ContentMaster Where Isnull(IsActive,0)=1 And ContentName='" + contName + "' And CompanyId = " + GBLCompanyID + " Order By SequencNo ";
                }

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return JsonConvert.SerializeObject(data.Message);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // Get Booking No
        [HttpGet]
        [Route("getquoteno")]
        public string GetQuoteNo(int bkId = 0)
        {
            long bookingNo = 0;
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                if (bkId == 0)
                {
                    bookingNo = DBConnection.GenerateMaxVoucherNo("JobBooking", "MaxBookingNo", "Where CompanyId = " + GBLCompanyID + " AND Isnull(IsDeletedTransaction,0)=0");
                }
                else
                {
                    str = "Select BookingNo From JobBooking Where BookingID=" + bkId + " And CompanyId=" + GBLCompanyID;
                    dataTable.Clear();
                    DBConnection.FillDataTable(ref dataTable, str);
                    if (dataTable.Rows.Count > 0)
                    {
                        bookingNo = Convert.ToInt64(dataTable.Rows[0][0]);
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return bookingNo.ToString();
        }
        /// <summary>
        /// Get Operation formulas
        /// </summary>
        [HttpGet]
        [Route("GetTypeOfCharges")]
        public string GetTypeOfCharges()
        {
            try
            {
                DataTable DtSlabs = new DataTable();
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);

                str = $"SELECT Distinct TypeOfCharges, Isnull(CalculationFormula,'') As CalculationFormula, isnull(FormulaVariables,'') As FormulaVariables FROM TypeOfCharges Where Isnull(IsDeletedTransaction,0)=0 And CompanyID={GBLCompanyID}";
                DBConnection.FillDataTable(ref dataTable, str);

                str = "SELECT DISTINCT PM.TypeofCharges, Case When ISNULL(PMS.Rate, 0)=0 Then Round(ISNULL(PM.Rate, 0),5) Else Round(ISNULL(PMS.Rate, 0),5) End AS Rate, Case When PMS.MinimumCharges=0 Then PM.MinimumCharges Else PMS.MinimumCharges End As MinimumCharges, PM.SetupCharges, PM.SizeToBeConsidered, PM.ChargeApplyOnSheets, PM.PrePress, PM.ProcessID, PM.ProcessName,Isnull(PMS.FromQty,0) AS FromQty,IsNull(PMS.ToQty,0) As ToQty,Isnull(PMS.RateFactor,'') As RateFactor,PMS.SlabID FROM ProcessMaster AS PM Left JOIN ProcessMasterSlabs AS PMS ON PMS.ProcessID = PM.ProcessID And PMS.IsLocked=0 /*And PM.CompanyID=PMS.CompanyID*/ Where /* PM.ProcessId In (\"\") And PM.CompanyId = " + GBLCompanyID + " And  */ Isnull(PM.IsDeletedTransaction,0)<>1 Order by PMS.SlabID Asc ";
                DBConnection.FillDataTable(ref DtSlabs, str);

                dataTable.TableName = "TypeOfCharges";
                DtSlabs.TableName = "LoadOperationSlabsDetails";

                var dataSet = new DataSet();
                dataSet.Merge(dataTable);
                dataSet.Merge(DtSlabs);
                var jsonResult = DBConnection.ConvertDataSetsToJsonString(dataSet);
                return jsonResult;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // POST: api/planwindow/upload-file
        // Uploads file to base URL location
        [HttpPost]
        [Route("upload-file")]
        public IHttpActionResult UploadFile()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;

                if (httpRequest.Files.Count == 0)
                    return BadRequest("No file uploaded");

                var file = httpRequest.Files[0];

                if (file.ContentLength == 0)
                    return BadRequest("Empty file");

                // Get subfolder from form data (optional)
                string subFolder = httpRequest.Form["subFolder"] ?? "";

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".bmp", ".doc", ".docx", ".xls", ".xlsx", ".csv", ".txt", ".zip", ".rar" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (Array.IndexOf(allowedExtensions, fileExtension) == -1)
                    return BadRequest("Invalid file type. Allowed types: jpg, jpeg, png, gif, pdf, bmp, doc, docx, xls, xlsx, csv, txt, zip, rar");

                // Validate file size (max 10MB)
                if (file.ContentLength > 10 * 1024 * 1024)
                    return BadRequest("File size exceeds 10MB limit");

                // Create uploads directory at base URL location
                string uploadsDir;
                if (!string.IsNullOrEmpty(subFolder))
                {
                    // Sanitize subfolder name to prevent path traversal
                    subFolder = subFolder.Replace("..", "").Replace("/", "\\").Trim('\\');
                    uploadsDir = HttpContext.Current.Server.MapPath($"~/Uploads/{subFolder}");
                }
                else
                {
                    uploadsDir = HttpContext.Current.Server.MapPath("~/Uploads/PlanWindow");
                }

                if (!Directory.Exists(uploadsDir))
                    Directory.CreateDirectory(uploadsDir);

                // Generate unique filename
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string randomSuffix = Guid.NewGuid().ToString("N").Substring(0, 8);
                string safeFileName = Path.GetFileNameWithoutExtension(file.FileName)
                    .Replace(" ", "_")
                    .Replace("-", "_");
                string uniqueFileName = $"{safeFileName}_{timestamp}_{randomSuffix}{fileExtension}";

                string filePath = Path.Combine(uploadsDir, uniqueFileName);

                // Save file
                file.SaveAs(filePath);

                // Return relative path for storage/reference
                string relativePath = !string.IsNullOrEmpty(subFolder)
                    ? $"/Uploads/{subFolder}/{uniqueFileName}"
                    : $"/Uploads/PlanWindow/{uniqueFileName}";

                return Ok(new
                {
                    success = true,
                    message = "File uploaded successfully",
                    data = new
                    {
                        fileName = uniqueFileName,
                        originalFileName = file.FileName,
                        filePath = relativePath,
                        fileSize = file.ContentLength,
                        fileType = fileExtension,
                        uploadedAt = DateTime.Now
                    }
                });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = "Error uploading file",
                    error = ex.Message
                });
            }
        }

        // POST: api/planwindow/upload-multiple-files
        // Uploads multiple files to base URL location
        [HttpPost]
        [Route("upload-multiple-files")]
        public IHttpActionResult UploadMultipleFiles(string type)
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;

                if (httpRequest.Files.Count == 0)
                    return BadRequest("No files uploaded");

                // Get subfolder from form data (optional)
                string subFolder = httpRequest.Form["subFolder"] ?? "";

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".bmp", ".doc", ".docx", ".xls", ".xlsx", ".csv", ".txt", ".zip", ".rar", ".eml", ".msg" };
                var uploadedFiles = new List<object>();
                var errors = new List<string>();

                // Create uploads directory
                string uploadsDir;
                if (!string.IsNullOrEmpty(subFolder))
                {
                    subFolder = subFolder.Replace("..", "").Replace("/", "\\").Trim('\\');
                    uploadsDir = HttpContext.Current.Server.MapPath($"~/Uploads/{subFolder}");
                }
                else
                {
                    uploadsDir = HttpContext.Current.Server.MapPath("~/Uploads/PlanWindow");
                }

                if (!Directory.Exists(uploadsDir))
                    Directory.CreateDirectory(uploadsDir);

                for (int i = 0; i < httpRequest.Files.Count; i++)
                {
                    var file = httpRequest.Files[i];

                    if (file.ContentLength == 0)
                    {
                        errors.Add($"File '{file.FileName}' is empty");
                        continue;
                    }

                    var fileExtension = Path.GetExtension(file.FileName).ToLower();

                    if (Array.IndexOf(allowedExtensions, fileExtension) == -1)
                    {
                        errors.Add($"File '{file.FileName}' has invalid type");
                        continue;
                    }

                    if (file.ContentLength > 10 * 1024 * 1024)
                    {
                        errors.Add($"File '{file.FileName}' exceeds 10MB limit");
                        continue;
                    }

                    // Generate safe filename - keep original name, only remove invalid characters
                    string safeFileName = Path.GetFileNameWithoutExtension(file.FileName)
                        .Replace(" ", "_");
                    foreach (char c in Path.GetInvalidFileNameChars())
                        safeFileName = safeFileName.Replace(c.ToString(), "");
                    string uniqueFileName = $"{safeFileName}{fileExtension}";

                    string filePath = Path.Combine(uploadsDir, uniqueFileName);
                    file.SaveAs(filePath);

                    string relativePath = !string.IsNullOrEmpty(subFolder)
                        ? $"/Uploads/{subFolder}/{uniqueFileName}"
                        : $"/Uploads/PlanWindow/{uniqueFileName}";

                    uploadedFiles.Add(new
                    {
                        fileName = uniqueFileName,
                        originalFileName = file.FileName,
                        filePath = relativePath,
                        fileSize = file.ContentLength,
                        fileType = fileExtension
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = $"{uploadedFiles.Count} file(s) uploaded successfully",
                    data = new
                    {
                        files = uploadedFiles,
                        errors = errors,
                        uploadedAt = DateTime.Now
                    }
                });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = "Error uploading files",
                    error = ex.Message
                });
            }
        }

        public class BookingDataRequest
        {
            public string FilterSTR { get; set; }
            public dynamic[] CompanyConfig { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string RadioValue { get; set; }
        }
        public static string ConvertToCustomFormat(string inputDate)
        {
            if (string.IsNullOrWhiteSpace(inputDate))
            {
                throw new ArgumentException("Input date cannot be null or empty.");
            }

            DateTime parsedDate;
            string[] formats = {
            "yyyy-MM-dd", "MM/dd/yyyy", "dd/MM/yyyy", "yyyy/MM/dd",
            "dd-MM-yyyy", "MM-dd-yyyy", "yyyyMMdd", "ddMMyyyy", "MM dd yyyy",
            "yyyy-MM-dd HH:mm:ss", "MM/dd/yyyy HH:mm:ss", "dd/MM/yyyy HH:mm:ss",
            "yyyy/MM/dd HH:mm:ss", "dd-MM-yyyy HH:mm:ss", "MM-dd-yyyy HH:mm:ss"
        };

            if (DateTime.TryParseExact(inputDate, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
            {
                return parsedDate.ToString("yyyy-MM-dd HH:mm:ss.fff");
            }
            else if (DateTime.TryParse(inputDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
            {
                return parsedDate.ToString("yyyy-MM-dd HH:mm:ss.fff");
            }
            else
            {
                throw new FormatException("Invalid date format.");
            }
        }

        /// <summary>
        /// Get Booking Data with filters
        /// </summary>
        [HttpPost]
        [Route("getbookingdata")]
        public string GetBookingData([FromBody] BookingDataRequest request)
        {
            try
            {
                bool canViewOtherUserQuotation = false;
                string gblUnderUserIDString = "";
                string strSegments = "";
                DataTable dtSegments = new DataTable();
                string str = "";
                DataTable dataTable = new DataTable();
                var data = new { Message = "" };
                string Filter = "";
                if (request.FilterSTR == "NewQuotes")
                {
                    Filter = " And IsSendForInternalApproval=0 And IsInternalApproved=0 And IsCancelled=0 And IsRework=0 And IsApproved=0 ";
                }
                else if (request.FilterSTR == "PendingForApproval")
                {
                    Filter = " And IsSendForInternalApproval=1 And IsInternalApproved=0 And IsCancelled=0 And IsRework=0 And IsApproved=0 ";
                }
                else if (request.FilterSTR == "IsInternalApproved")
                {
                    Filter = " And IsInternalApproved=1 And IsCancelled=0 And IsSendForPriceApproval=0 ";
                }
                else if (request.FilterSTR == "PendingForPriceApproval")
                {
                    Filter = " And IsSendForPriceApproval=1 And IsInternalApproved=1 And IsCancelled=0 And IsRework=0 And IsApproved=0 ";
                }
                else
                {
                    Filter = " And " + request.FilterSTR + "= 1 ";
                }
                if (request.FilterSTR == "JobApproved")
                {
                    Filter = " And IsApproved = 1 ";
                }
                if (request.FilterSTR == "All") request.FilterSTR = "";


                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                GBLUserID = Convert.ToString(HttpContext.Current.Session["UserID"]);
                GBLUserName = DBConnection.GblUserName;
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                ShowUnderUsersID(Convert.ToInt32(GBLUserID), ref gblUnderUserIDString);

                //if (request.CompanyConfig != null && request.CompanyConfig.Length > 0)
                //{
                //    canViewOtherUserQuotation = Convert.ToBoolean(request.CompanyConfig[0]["CanUserViewOtherQuotation"]);
                //}

                DBConnection.FillDataTable(ref dtSegments, $"Select Distinct SegmentID From UserSegmentAllocation Where UserID={GBLUserID} AND Isnull(IsDeletedTransaction,0)=0 AND CompanyID={GBLCompanyID}");

                if (dtSegments.Rows.Count > 0)
                {
                    strSegments = $" INNER JOIN SegmentMaster AS S ON S.SegmentID=CM.SegmentID AND S.SegmentID IN(Select Distinct SegmentID From UserSegmentAllocation Where UserID={GBLUserID} AND Isnull(IsDeletedTransaction,0)=0 AND CompanyID={GBLCompanyID})";
                }
                else
                {
                    strSegments = " LEFT JOIN SegmentMaster AS S ON S.SegmentID=CM.SegmentID ";
                }

                // If CanViewOtherUserQuotation = True And GBLUnderUserIDString <> ""
                if (canViewOtherUserQuotation == true && gblUnderUserIDString != "")
                {

                    //if (request.RadioValue == "Quotation Summary Only")
                    //{
                    str = "SELECT ISNULL(JE.Source,'') as Source,ISNULL(LM.IsLead,0) as IsLead,ISNULL(JE.Status,'') as Status,ISNULL(JBC.ProfitPercentage,0) as ProfitPercentage,ISNULL(JE.BookingRemark,'') as QuoteRemark,ISNULL(JQ.Remark,'') as EnquiryRemark, ISNULL(JQ.EnquiryNo,'')as EnquiryNo,JE.ArtWorkCode,Replace(LM.LedgerName,'\"','') As ClientName, S.SegmentName,CM.CategoryName, Replace(JE.JobName,'\"',' inch') As JobName, JE.BookingNo,convert(varchar, JE.CreatedDate, 103) As CreatedDate,replace(CONVERT(varchar, JE.ExpectedDeliveryDate, 120) ,'','-') AS ExpectedDeliveryDate,JE.OrderQuantity,JE.AnnualQuantity, JE.BookingID, JE.MAXBookingNo, JE.LedgerID, UM.UserName, Cast(JE.QuotedCost AS Nvarchar(50))+' '+Isnull(JE.CurrencySymbol,'INR') AS QuotedCost,JE.FinalCost,JE.TypeOfCost, JE.EnquiryID,ISNULL(JE.IsApproved,0) As JobApproved,ISNULL(JE.IsSendForPriceApproval,0) As PendingForPriceApproval,ISNULL(JE.IsInternalApproved,0) AS IsInternalApproved,JE.InternalApprovedUserID,ISNULL(JE.IsRework,0) AS IsRework ,ISNULL(JE.IsMailSent,0) AS IsMailSent,(Case When Isnull(JE.ReworkRemark ,'') ='' then REPLACE(JE.Remark,'\"','') Else ISNULL(REPLACE(JE.ReworkRemark,'\"',''),'') End) As ReworkRemark,(Case When Isnull(JE.CancelledRemark ,'') ='' then '' Else ISNULL(REPLACE(JE.CancelledRemark,'\"',''),'') End) As CancelledRemark,JE.IsSendForInternalApproval,Isnull(JE.IsCancelled,0) As IsCancelled, JE.ReasonsofQuote,Nullif(JE.ProductCode,'') As ProductCode,/*(SELECT DISTINCT A.FieldValue FROM dbo.LedgerMasterDetails A Where A.CompanyID=LM.CompanyID And A.LedgerID=LM.LedgerID And FieldName In ('MailingAddress') And Isnull(A.IsDeletedTransaction,0)=0)*/ LM.MailingAddress As Address,(Select UserName From UserMaster Where UserID=ApprovalSendTo And CompanyID=LM.CompanyID) As ApprovalSendTo,Convert(varchar, JE.InternalApprovedDate, 103) As InternalApprovedDate,JE.RemarkInternalApproved,(Select Count(CommentID) From CommentChainMaster Where ModuleName='Estimation' And BookingID=JE.BookingID And CompanyID=LM.CompanyID) As CommentCount,BM.BranchName,SM.LedgerName AS SalesEmployeeName,nullif(JE.RefProductMasterCode,'') as RefProductMasterCode ,PUM.ProductionUnitName " +
                          " FROM JobBooking As JE INNER JOIN CategoryMaster As CM ON CM.CategoryID=JE.CategoryID INNER JOIN LedgerMaster AS LM ON LM.LedgerID=JE.LedgerID  INNER JOIN UserMaster As UM ON UM.UserID=JE.CreatedBy /*AND JE.CreatedBy IN(" + gblUnderUserIDString + ")*/ INNER JOIN JobBookingCostings As JBC ON JBC.BookingID = JE.BookingID INNER JOIN ProductionUnitMaster as PUM ON PUM.ProductionUnitID = JE.PlantID " + strSegments + " LEFT JOIN LedgerMaster AS SM ON SM.LedgerID=JE.SalesEmployeeID LEFT JOIN BranchMaster AS BM ON BM.BranchID=JE.BranchID AND BM.CompanyID=JE.CompanyID  LEFT JOIN JobEnquiry AS JQ on JQ.EnquiryID =JE.EnquiryID " +
                          " Where Isnull(JE.IsEstimate,0)=1 And Isnull(JE.IsDeletedTransaction,0)=0 AND ((Cast(Floor(cast(JE.CreatedDate as float))as DateTime) >='" + ConvertToCustomFormat(request.FromDate) + "')) AND ((Cast(Floor(cast(JE.CreatedDate as float))as DateTime) <='" + ConvertToCustomFormat(request.ToDate) + "')) AND Isnull(JE.SalesEmployeeID,0) IN(Select 0 UNION ALL Select OperatorID From UserOperatorAllocation Where UserID =" + GBLUserID + " AND Isnull(IsDeletedTransaction,0)=0) " + request.FilterSTR + " " +
                          "Group By ISNULL(JE.Source,''),ISNULL(LM.IsLead,0),ISNULL(JE.Status,''),ISNULL(JBC.ProfitPercentage,0),ISNULL(JE.BookingRemark,'') ,ISNULL(JQ.Remark,''),JQ.EnquiryNo,LM.LedgerName,JE.ArtWorkCode, LM.MailingAddress,S.SegmentName,CM.CategoryName, JE.JobName, JE.BookingNo, JE.CreatedDate,JE.ExpectedDeliveryDate, JE.OrderQuantity,JE.AnnualQuantity, JE.BookingID, JE.MAXBookingNo, JE.LedgerID, UM.UserName,JE.FinalCost,JE.QuotedCost , JE.CurrencySymbol,JE.TypeOfCost, JE.EnquiryID, JE.IsApproved,JE.IsSendForPriceApproval,JE.IsCancelled, JE.InternalApprovedUserID,JE.IsInternalApproved,JE.IsRework,JE.IsMailSent,JE.ReworkRemark,JE.CancelledRemark,JE.Remark,JE.IsSendForInternalApproval, JE.ReasonsofQuote ,JE.ProductCode,LM.LedgerID,LM.CompanyID,JE.ApprovalSendTo,JE.InternalApprovedDate,JE.RemarkInternalApproved,BM.BranchName,SM.LedgerName,nullif(JE.RefProductMasterCode,'') ,PUM.ProductionUnitName Order By JE.BookingID Desc";
                    //}
                    //else
                    //{
                    //    str = "SELECT JS.PlanOnlineCoating, Case When ROUND((Isnull(A.TotalMaterialCost,0)-Isnull(A.PaperAmount,0)),2)>1 THEN ROUND((Isnull(A.TotalMaterialCost,0)-Isnull(A.PaperAmount,0)),2) ELSE 0 END AS OtherMaterialCost,A.PaperAmount,A.CorrugationAmount, A.TotalProcessCost,A.TotalMaterialCost,JS.PlanOnlineCoating,A.TotalAmount, A.MakeReadyAmount,A.TotalMakeReadies,A.PlateAmount,A.PrintingAmount,A.TotalAmount,ISNULL(SpeColorFCharges, 0) + ISNULL(SpeColorBCharges, 0) AS TotalSpecialColorCharges,A.CoatingAmount,A.CylinderCircumferenceMM,  A.TotalRequiredRunningMeter, ISNULL(A.MakeReadyWastageSheet, 0) + ISNULL(A.ActualSheets, 0) + ISNULL(A.WastageSheets, 0) AS TotalCutSheets,A.FullSheets,A.UpsL,A.UpsW,A.TotalUps,A.CylinderNoOfTeeth,A.CylinderCircumferenceInch,A.CylinderCircumferenceMM, ((ISNULL(A.CutL, 0) * ISNULL(A.CutW, 0)) +(ISNULL(A.CutLH, 0) * ISNULL(A.CutHL, 0))) AS TotalCuts, (ISNULL(JS.PlanFColor, 0) + ISNULL(JS.PlanSpeFColor, 0)) AS FrontColor,(ISNULL(JS.PlanBColor, 0) + ISNULL(JS.PlanSpeBColor, 0)) AS BackColor,A.UnitPrice,A.PlanContQty,JS.JobNoOfPages, A.PlanContName, A.MachineName,JS.JobPrePlan AS ContentSize, JS.SizeLength,JS.SizeWidth,JS.SizeHeight,  A.PaperSize, A.CutSize, A.WastePerc, A.WastageKg, A.TotalPaperWeightInKg, B.ItemCode,B.ItemName,ISNULL(JE.BookingRemark,'') as QuoteRemark,ISNULL(JQ.Remark,'') as EnquiryRemark, ISNULL(JQ.EnquiryNo,'')as EnquiryNo,Replace(LM.LedgerName,'\"','') As ClientName, S.SegmentName,CM.CategoryName, Replace(JE.JobName,'\"',' inch') As JobName, JE.BookingNo,convert(varchar, JE.CreatedDate, 103) As CreatedDate,replace(convert(nvarchar(30),JE.ExpectedDeliveryDate,106),'','-') AS ExpectedDeliveryDate, JE.OrderQuantity, JE.BookingID, JE.MAXBookingNo, JE.LedgerID, UM.UserName, Cast(JBC.QuotedCost AS Nvarchar(50))+' '+Isnull(JE.CurrencySymbol,'INR') AS QuotedCost,JBC.FinalCost,JE.TypeOfCost, JE.EnquiryID,ISNULL(JE.IsApproved,0) As JobApproved,ISNULL(JE.IsSendForPriceApproval,0) As PendingForPriceApproval,ISNULL(JE.IsInternalApproved,0) AS IsInternalApproved,JE.InternalApprovedUserID,ISNULL(JE.IsRework,0) AS IsRework ,ISNULL(JE.IsMailSent,0) AS IsMailSent,(Case When Isnull(JE.ReworkRemark ,'') ='' then REPLACE(JE.Remark,'\"','') Else ISNULL(REPLACE(JE.ReworkRemark,'\"',''),'') End) As ReworkRemark,(Case When Isnull(JE.CancelledRemark ,'') ='' then '' Else ISNULL(REPLACE(JE.CancelledRemark,'\"',''),'') End) As CancelledRemark,JE.IsSendForInternalApproval,Isnull(JE.IsCancelled,0) As IsCancelled, JE.ReasonsofQuote,Nullif(JE.ProductCode,'') As ProductCode,/*(SELECT DISTINCT A.FieldValue FROM dbo.LedgerMasterDetails A Where A.CompanyID=LM.CompanyID And A.LedgerID=LM.LedgerID And FieldName In ('MailingAddress') And Isnull(A.IsDeletedTransaction,0)=0)*/ LM.MailingAddress As Address,(Select UserName From UserMaster Where UserID=ApprovalSendTo And CompanyID=LM.CompanyID) As ApprovalSendTo,Convert(varchar, JE.InternalApprovedDate, 103) As InternalApprovedDate,JE.RemarkInternalApproved,(Select Count(CommentID) From CommentChainMaster Where ModuleName='Estimation' And BookingID=JE.BookingID And CompanyID=LM.CompanyID) As CommentCount,BM.BranchName,SM.LedgerName AS SalesEmployeeName,nullif(JE.RefProductMasterCode,'') as RefProductMasterCode  " +
                    //          " FROM JobBooking As JE  INNER JOIN JobbookingContents  As A On A.BookingID = JE.BookingID INNER JOIN JobBookingCostings AS JBC ON JBC.BookingID=A.BookingID AND JBC.PlanContQty=A.PlanContQty INNER JOIN CategoryMaster As CM ON CM.CategoryID=JE.CategoryID INNER JOIN LedgerMaster AS LM ON LM.LedgerID=JE.LedgerID  INNER JOIN UserMaster As UM ON UM.UserID=JE.CreatedBy /*AND JE.CreatedBy IN(" + gblUnderUserIDString + ")*/ " + strSegments + " LEFT JOIN JobBookingContentsSpecification  As JS On JS.BookingID = A.BookingID AND JS.ContentsID = A.JobContentsID  LEFT JOIN ItemMaster  As  B On B.ItemID =  A.PaperID And ISNULL(B.IsDeletedTransaction, 0)<>1  LEFT JOIN LedgerMaster AS SM ON SM.LedgerID=JE.SalesEmployeeID LEFT JOIN BranchMaster AS BM ON BM.BranchID=JE.BranchID AND BM.CompanyID=JE.CompanyID  LEFT JOIN JobEnquiry AS JQ on JQ.EnquiryID =JE.EnquiryID " +
                    //          " Where Isnull(JE.IsEstimate,0)=1 And Isnull(JE.IsDeletedTransaction,0)=0  And JE.COMPANYID=" + GBLCompanyID + " AND ((Cast(Floor(cast(JE.CreatedDate as float))as DateTime) >='" + request.FromDate + "')) AND ((Cast(Floor(cast(JE.CreatedDate as float))as DateTime) <='" + request.ToDate + "')) AND Isnull(JE.SalesEmployeeID,0) IN(Select 0 UNION ALL Select OperatorID From UserOperatorAllocation Where UserID =" + GBLUserID + " AND CompanyID=" + GBLCompanyID + " AND Isnull(IsDeletedTransaction,0)=0) " + request.FilterSTR + " " +
                    //          " Group By  JS.PlanOnlineCoating, A.UnitPrice,ROUND((Isnull(A.TotalMaterialCost,0)-Isnull(A.PaperAmount,0)),2),A.PaperAmount,A.CorrugationAmount,A.TotalProcessCost,A.TotalMaterialCost,JS.PlanOnlineCoating,A.TotalAmount, A.MakeReadyAmount,A.TotalMakeReadies,A.PlateAmount,A.PrintingAmount,A.TotalAmount,ISNULL(A.SpeColorFCharges, 0) + ISNULL(A.SpeColorBCharges, 0),A.CoatingAmount,A.CylinderCircumferenceMM, A.TotalRequiredRunningMeter, ISNULL(A.MakeReadyWastageSheet, 0) + ISNULL(A.ActualSheets, 0) + ISNULL(A.WastageSheets, 0),A.FullSheets,A.UpsL,A.UpsW,A.TotalUps,A.CylinderNoOfTeeth,A.CylinderCircumferenceInch,A.CylinderCircumferenceMM,  (ISNULL(A.CutL, 0) * ISNULL(A.CutW, 0)) +(ISNULL(A.CutLH, 0) * ISNULL(A.CutHL, 0)),JS.JobPrePlan, JS.SizeLength,JS.SizeWidth,JS.SizeHeight,   (ISNULL(JS.PlanFColor,0)  + ISNULL(JS.PlanSpeFColor,0)),(ISNULL(JS.PlanBColor,0)  + ISNULL(JS.PlanSpeBColor,0)), A.PlanContQty,JS.JobNoOfPages,  A.UnitPrice, A.PlanContName, A.MachineName,  A.PaperSize, A.CutSize, A.WastePerc, A.WastageKg, A.TotalPaperWeightInKg, B.ItemCode,B.ItemName,ISNULL(JE.BookingRemark,'') ,ISNULL(JQ.Remark,''),JQ.EnquiryNo,LM.LedgerName, LM.MailingAddress,S.SegmentName,CM.CategoryName, JE.JobName, JE.BookingNo, JE.CreatedDate, JE.ExpectedDeliveryDate, JE.OrderQuantity, JE.BookingID, JE.MAXBookingNo, JE.LedgerID, UM.UserName,JBC.FinalCost,JBC.QuotedCost , JE.CurrencySymbol,JE.TypeOfCost, JE.EnquiryID, JE.IsApproved,JE.IsSendForPriceApproval,JE.IsCancelled, JE.InternalApprovedUserID,JE.IsInternalApproved,JE.IsRework,JE.IsMailSent,JE.ReworkRemark,JE.CancelledRemark,JE.Remark,JE.IsSendForInternalApproval, JE.ReasonsofQuote ,JE.ProductCode,LM.LedgerID,LM.CompanyID,JE.ApprovalSendTo,JE.InternalApprovedDate,JE.RemarkInternalApproved,BM.BranchName,SM.LedgerName,nullif(JE.RefProductMasterCode,'')  Order By JE.BookingID Desc,A.PlanContQty";
                    //}
                }
                else
                {
                    //if (request.RadioValue == "Quotation Summary Only")
                    //{
                    str = " SELECT ISNULL(JE.Source,'') as Source,ISNULL(LM.IsLead,0) as IsLead,ISNULL(JE.Status,'') as Status,ISNULL(JBC.ProfitPercentage,0) as ProfitPercentage,ISNULL(JE.BookingRemark,'') as QuoteRemark,ISNULL(JQ.Remark,'') as EnquiryRemark, ISNULL(JQ.EnquiryNo,'')as EnquiryNo,JE.ArtWorkCode,Replace(LM.LedgerName,'\"','') As ClientName, S.SegmentName,CM.CategoryName, Replace(JE.JobName,'\"',' inch') As JobName, JE.BookingNo,convert(varchar, JE.CreatedDate, 103) As CreatedDate,replace(CONVERT(varchar, JE.ExpectedDeliveryDate, 120) ,'','-') AS ExpectedDeliveryDate,JE.OrderQuantity,JE.AnnualQuantity, JE.BookingID, JE.MAXBookingNo, JE.LedgerID, UM.UserName, Cast(JE.QuotedCost AS Nvarchar(50))+' '+Isnull(JE.CurrencySymbol,'INR') AS QuotedCost,JE.FinalCost,JE.TypeOfCost, JE.EnquiryID,ISNULL(JE.IsApproved,0) As JobApproved,ISNULL(JE.IsSendForPriceApproval,0) As PendingForPriceApproval,ISNULL(JE.IsInternalApproved,0) AS IsInternalApproved,JE.InternalApprovedUserID,ISNULL(JE.IsRework,0) AS IsRework ,ISNULL(JE.IsMailSent,0) AS IsMailSent,(Case When Isnull(JE.ReworkRemark ,'') ='' then REPLACE(JE.Remark,'\"','') Else ISNULL(REPLACE(JE.ReworkRemark,'\"',''),'') End) As ReworkRemark,(Case When Isnull(JE.CancelledRemark ,'') ='' then '' Else ISNULL(REPLACE(JE.CancelledRemark,'\"',''),'') End) As CancelledRemark,JE.IsSendForInternalApproval,Isnull(JE.IsCancelled,0) As IsCancelled, JE.ReasonsofQuote,Nullif(JE.ProductCode,'') As ProductCode,/*(SELECT DISTINCT A.FieldValue FROM dbo.LedgerMasterDetails A Where A.CompanyID=LM.CompanyID And A.LedgerID=LM.LedgerID And FieldName In ('MailingAddress') And Isnull(A.IsDeletedTransaction,0)=0)*/ LM.MailingAddress As Address,(Select UserName From UserMaster Where UserID=ApprovalSendTo And CompanyID=LM.CompanyID) As ApprovalSendTo,Convert(varchar, JE.InternalApprovedDate, 103) As InternalApprovedDate,JE.RemarkInternalApproved,(Select Count(CommentID) From CommentChainMaster Where ModuleName='Estimation' And BookingID=JE.BookingID And CompanyID=LM.CompanyID) As CommentCount,BM.BranchName,SM.LedgerName AS SalesEmployeeName,nullif(JE.RefProductMasterCode,'') as RefProductMasterCode ,PUM.ProductionUnitName  " +
                          " FROM JobBooking As JE INNER JOIN CategoryMaster As CM ON CM.CategoryID=JE.CategoryID INNER JOIN LedgerMaster AS LM ON LM.LedgerID=JE.LedgerID  INNER JOIN UserMaster As UM ON UM.UserID=JE.CreatedBy INNER JOIN JobBookingCostings As JBC ON JBC.BookingID = JE.BookingID INNER JOIN ProductionUnitMaster as PUM ON PUM.ProductionUnitID = JE.PlantID " + strSegments + " LEFT JOIN LedgerMaster AS SM ON SM.LedgerID=JE.SalesEmployeeID LEFT JOIN BranchMaster AS BM ON BM.BranchID=JE.BranchID AND BM.CompanyID=JE.CompanyID LEFT JOIN JobEnquiry AS JQ on JQ.EnquiryID =JE.EnquiryID " +
                          " Where Isnull(JE.IsEstimate,0)=1 And Isnull(JE.IsDeletedTransaction,0)=0  AND ((Cast(Floor(cast(JE.CreatedDate as float))as DateTime) >='" + ConvertToCustomFormat(request.FromDate) + "')) AND ((Cast(Floor(cast(JE.CreatedDate as float))as DateTime) <='" + ConvertToCustomFormat(request.ToDate) + "')) AND Isnull(JE.SalesEmployeeID,0)  IN(Select 0 UNION ALL Select OperatorID From UserOperatorAllocation Where UserID =" + GBLUserID + " AND Isnull(IsDeletedTransaction,0)=0) /*AND UM.UserID=" + GBLUserID + " */ " + request.FilterSTR + " " +
                          " Group By ISNULL(JE.Source,''),ISNULL(LM.IsLead,0),ISNULL(JE.Status,''),ISNULL(JBC.ProfitPercentage,0),ISNULL(JE.BookingRemark,'') ,ISNULL(JQ.Remark,''),JQ.EnquiryNo,LM.LedgerName,JE.ArtWorkCode, LM.MailingAddress,S.SegmentName,CM.CategoryName, JE.JobName, JE.BookingNo, JE.CreatedDate,JE.ExpectedDeliveryDate, JE.OrderQuantity,JE.AnnualQuantity, JE.BookingID, JE.MAXBookingNo, JE.LedgerID, UM.UserName,JE.FinalCost,JE.QuotedCost , JE.CurrencySymbol,JE.TypeOfCost, JE.EnquiryID, JE.IsApproved,JE.IsSendForPriceApproval,JE.IsCancelled, JE.InternalApprovedUserID,JE.IsInternalApproved,JE.IsRework,JE.IsMailSent,JE.ReworkRemark,JE.CancelledRemark,JE.Remark,JE.IsSendForInternalApproval, JE.ReasonsofQuote ,JE.ProductCode,LM.LedgerID,LM.CompanyID,JE.ApprovalSendTo,JE.InternalApprovedDate,JE.RemarkInternalApproved,BM.BranchName,SM.LedgerName,nullif(JE.RefProductMasterCode,'') ,PUM.ProductionUnitName Order By JE.BookingID Desc ";
                    //}
                    //else
                    //{
                    //    str = "SELECT JS.PlanOnlineCoating, Case When ROUND((Isnull(A.TotalMaterialCost,0)-Isnull(A.PaperAmount,0)),2)>1 THEN ROUND((Isnull(A.TotalMaterialCost,0)-Isnull(A.PaperAmount,0)),2) ELSE 0 END AS OtherMaterialCost,A.PaperAmount,A.CorrugationAmount, A.TotalProcessCost,A.TotalMaterialCost,JS.PlanOnlineCoating,A.TotalAmount, A.MakeReadyAmount,A.TotalMakeReadies,A.PlateAmount,A.PrintingAmount,A.TotalAmount,ISNULL(SpeColorFCharges, 0) + ISNULL(SpeColorBCharges, 0) AS TotalSpecialColorCharges,A.CoatingAmount,A.CylinderCircumferenceMM,  A.TotalRequiredRunningMeter, ISNULL(A.MakeReadyWastageSheet, 0) + ISNULL(A.ActualSheets, 0) + ISNULL(A.WastageSheets, 0) AS TotalCutSheets,A.FullSheets,A.UpsL,A.UpsW,A.TotalUps,A.CylinderNoOfTeeth,A.CylinderCircumferenceInch,A.CylinderCircumferenceMM, ((ISNULL(A.CutL, 0) * ISNULL(A.CutW, 0)) +(ISNULL(A.CutLH, 0) * ISNULL(A.CutHL, 0))) AS TotalCuts, (ISNULL(JS.PlanFColor, 0) + ISNULL(JS.PlanSpeFColor, 0)) AS FrontColor,(ISNULL(JS.PlanBColor, 0) + ISNULL(JS.PlanSpeBColor, 0)) AS BackColor,A.UnitPrice,A.PlanContQty,JS.JobNoOfPages, A.PlanContName, A.MachineName,JS.JobPrePlan AS ContentSize, JS.SizeLength,JS.SizeWidth,JS.SizeHeight,  A.PaperSize, A.CutSize, A.WastePerc, A.WastageKg, A.TotalPaperWeightInKg, B.ItemCode,B.ItemName,ISNULL(JE.BookingRemark,'') as QuoteRemark,ISNULL(JQ.Remark,'') as EnquiryRemark, ISNULL(JQ.EnquiryNo,'')as EnquiryNo,Replace(LM.LedgerName,'\"','') As ClientName, S.SegmentName,CM.CategoryName, Replace(JE.JobName,'\"',' inch') As JobName, JE.BookingNo,convert(varchar, JE.CreatedDate, 103) As CreatedDate,replace(convert(nvarchar(30),JE.ExpectedDeliveryDate,106),'','-') AS ExpectedDeliveryDate, JE.OrderQuantity, JE.BookingID, JE.MAXBookingNo, JE.LedgerID, UM.UserName, Cast(JBC.QuotedCost AS Nvarchar(50))+' '+Isnull(JE.CurrencySymbol,'INR') AS QuotedCost,JBC.FinalCost,JE.TypeOfCost, JE.EnquiryID,ISNULL(JE.IsApproved,0) As JobApproved,ISNULL(JE.IsSendForPriceApproval,0) As PendingForPriceApproval,ISNULL(JE.IsInternalApproved,0) AS IsInternalApproved,JE.InternalApprovedUserID,ISNULL(JE.IsRework,0) AS IsRework ,ISNULL(JE.IsMailSent,0) AS IsMailSent,(Case When Isnull(JE.ReworkRemark ,'') ='' then REPLACE(JE.Remark,'\"','') Else ISNULL(REPLACE(JE.ReworkRemark,'\"',''),'') End) As ReworkRemark,(Case When Isnull(JE.CancelledRemark ,'') ='' then '' Else ISNULL(REPLACE(JE.CancelledRemark,'\"',''),'') End) As CancelledRemark,JE.IsSendForInternalApproval,Isnull(JE.IsCancelled,0) As IsCancelled, JE.ReasonsofQuote,Nullif(JE.ProductCode,'') As ProductCode,/*(SELECT DISTINCT A.FieldValue FROM dbo.LedgerMasterDetails A Where A.CompanyID=LM.CompanyID And A.LedgerID=LM.LedgerID And FieldName In ('MailingAddress') And Isnull(A.IsDeletedTransaction,0)=0)*/ LM.MailingAddress As Address,(Select UserName From UserMaster Where UserID=ApprovalSendTo And CompanyID=LM.CompanyID) As ApprovalSendTo,Convert(varchar, JE.InternalApprovedDate, 103) As InternalApprovedDate,JE.RemarkInternalApproved,(Select Count(CommentID) From CommentChainMaster Where ModuleName='Estimation' And BookingID=JE.BookingID And CompanyID=LM.CompanyID) As CommentCount,BM.BranchName,SM.LedgerName AS SalesEmployeeName,nullif(JE.RefProductMasterCode,'') as RefProductMasterCode " +
                    //          " FROM JobBooking As JE  INNER JOIN JobbookingContents  As A On A.BookingID = JE.BookingID INNER JOIN JobBookingContentsSpecification  As JS On JS.BookingID = A.BookingID AND JS.ContentsID = A.JobContentsID INNER JOIN JobBookingCostings AS JBC ON JBC.BookingID=A.BookingID AND JBC.PlanContQty=A.PlanContQty INNER JOIN CategoryMaster As CM ON CM.CategoryID=JE.CategoryID INNER JOIN LedgerMaster AS LM ON LM.LedgerID=JE.LedgerID  INNER JOIN UserMaster As UM ON UM.UserID=JE.CreatedBy " + strSegments + " LEFT JOIN ItemMaster  As  B On B.ItemID =  A.PaperID And ISNULL(B.IsDeletedTransaction, 0)<>1  LEFT JOIN LedgerMaster AS SM ON SM.LedgerID=JE.SalesEmployeeID LEFT JOIN BranchMaster AS BM ON BM.BranchID=JE.BranchID AND BM.CompanyID=JE.CompanyID LEFT JOIN JobEnquiry AS JQ on JQ.EnquiryID =JE.EnquiryID " +
                    //          " Where Isnull(JE.IsEstimate,0)=1 And Isnull(JE.IsDeletedTransaction,0)=0  And JE.COMPANYID=" + GBLCompanyID + " AND ((Cast(Floor(cast(JE.CreatedDate as float))as DateTime) >='" + request.FromDate + "')) AND ((Cast(Floor(cast(JE.CreatedDate as float))as DateTime) <='" + request.ToDate + "')) AND Isnull(JE.SalesEmployeeID,0)  IN(Select 0 UNION ALL Select OperatorID From UserOperatorAllocation Where UserID =" + GBLUserID + " AND CompanyID=" + GBLCompanyID + " AND Isnull(IsDeletedTransaction,0)=0) /*AND UM.UserID=" + GBLUserID + " */ " + request.FilterSTR + " " +
                    //          " Group By JS.PlanOnlineCoating, A.UnitPrice,ROUND((Isnull(A.TotalMaterialCost,0)-Isnull(A.PaperAmount,0)),2),A.PaperAmount,A.CorrugationAmount,A.TotalProcessCost,A.TotalMaterialCost,JS.PlanOnlineCoating,A.TotalAmount, A.MakeReadyAmount,A.TotalMakeReadies,A.PlateAmount,A.PrintingAmount,A.TotalAmount,ISNULL(A.SpeColorFCharges, 0) + ISNULL(A.SpeColorBCharges, 0),A.CoatingAmount,A.CylinderCircumferenceMM, A.TotalRequiredRunningMeter, ISNULL(A.MakeReadyWastageSheet, 0) + ISNULL(A.ActualSheets, 0) + ISNULL(A.WastageSheets, 0),A.FullSheets,A.UpsL,A.UpsW,A.TotalUps,A.CylinderNoOfTeeth,A.CylinderCircumferenceInch,A.CylinderCircumferenceMM,  (ISNULL(A.CutL, 0) * ISNULL(A.CutW, 0)) +(ISNULL(A.CutLH, 0) * ISNULL(A.CutHL, 0)),JS.JobPrePlan, JS.SizeLength,JS.SizeWidth,JS.SizeHeight,   (ISNULL(JS.PlanFColor,0)  + ISNULL(JS.PlanSpeFColor,0)),(ISNULL(JS.PlanBColor,0)  + ISNULL(JS.PlanSpeBColor,0)), A.PlanContQty,JS.JobNoOfPages,  A.UnitPrice, A.PlanContName, A.MachineName,  A.PaperSize, A.CutSize, A.WastePerc, A.WastageKg, A.TotalPaperWeightInKg, B.ItemCode,B.ItemName,ISNULL(JE.BookingRemark,'') ,ISNULL(JQ.Remark,''),JQ.EnquiryNo,LM.LedgerName, LM.MailingAddress,S.SegmentName,CM.CategoryName, JE.JobName, JE.BookingNo, JE.CreatedDate, JE.ExpectedDeliveryDate, JE.OrderQuantity, JE.BookingID, JE.MAXBookingNo, JE.LedgerID, UM.UserName,JBC.FinalCost,JBC.QuotedCost , JE.CurrencySymbol,JE.TypeOfCost, JE.EnquiryID, JE.IsApproved,JE.IsSendForPriceApproval,JE.IsCancelled, JE.InternalApprovedUserID,JE.IsInternalApproved,JE.IsRework,JE.IsMailSent,JE.ReworkRemark,JE.CancelledRemark,JE.Remark,JE.IsSendForInternalApproval, JE.ReasonsofQuote ,JE.ProductCode,LM.LedgerID,LM.CompanyID,JE.ApprovalSendTo,JE.InternalApprovedDate,JE.RemarkInternalApproved,BM.BranchName,SM.LedgerName,nullif(JE.RefProductMasterCode,'')  Order By JE.BookingID Desc,A.PlanContQty";
                    //}

                }

                DBConnection.FillDataTable(ref dataTable, str);
                data = new { Message = DBConnection.ConvertDataTableToJsonString(dataTable) };

                var jsonSettings = new JsonSerializerSettings
                {
                    MaxDepth = int.MaxValue
                };
                return JsonConvert.SerializeObject(data.Message, jsonSettings);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public static void SeparatePaperSize(string sizeWL, out double sizeL, out double sizeH)
        {
            try
            {
                // Initialize output parameters
                sizeL = 0;
                sizeH = 0;

                if (string.IsNullOrEmpty(sizeWL))
                    return;

                // Find the position of 'x' (case insensitive)
                int xPosition = sizeWL.IndexOf("x", StringComparison.OrdinalIgnoreCase);

                if (xPosition > 0)
                {
                    // Extract height (before 'x')
                    string heightPart = sizeWL.Substring(0, xPosition);
                    double.TryParse(heightPart, out sizeH);

                    // Extract length (after 'x')
                    string lengthPart = sizeWL.Substring(xPosition + 1);
                    double.TryParse(lengthPart, out sizeL);
                }
            }
            catch
            {
                // Handle any parsing errors gracefully
                sizeL = 0;
                sizeH = 0;
            }
        }

        /// <summary>
        /// Load Printing Slabs Details
        /// </summary>
        [HttpGet]
        [Route("LoadPrintingSlabsDetails/{MachineID}/{PaperGrp}/{SizeWL}")]
        public string LoadPrintingSlabsDetails(int MachineID, string PaperGrp, string SizeWL)
        {
            try
            {
                double SheetL, SheetH;
                SeparatePaperSize(SizeWL, out SheetL, out SheetH);

                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);

                if (DBType == "MYSQL")
                {
                    str = $"Select Distinct MachineID, SheetRangeFrom, SheetRangeTo, Wastage, Rate, PlateCharges, PSPlateCharges, CTCPPlateCharges,CoatingCharges, SpecialColorFrontCharges, SpecialColorBackCharges, FlatRate, FlatWastageSheets, ApplyAsFixedCharge,PaperGroup,MaxPlanL,MaxPlanW,MinCharges From MachineSlabMaster  Where CompanyId = {GBLCompanyID} And IFNULL(IsDeletedTransaction,0)<>1 And MachineID ={MachineID} And PaperGroup='{PaperGrp}' And ((MaxPlanL >= {SheetL} AND MaxPlanW >= {SheetH}) OR (MaxPlanL >= {SheetH} AND MaxPlanW >= {SheetL}))  Order By MachineID, SheetRangeFrom, SheetRangeTo ";
                }
                else
                {
                    str = $"Select Distinct MachineID, SheetRangeFrom, SheetRangeTo, Wastage, Rate, PlateCharges, PSPlateCharges, CTCPPlateCharges,CoatingCharges, SpecialColorFrontCharges, SpecialColorBackCharges, FlatRate, FlatWastageSheets, ApplyAsFixedCharge,PaperGroup,MaxPlanL,MaxPlanW,MinCharges From MachineSlabMaster  Where CompanyId = {GBLCompanyID} And Isnull(IsDeletedTransaction,0)<>1 And MachineID ={MachineID} And PaperGroup='{PaperGrp}' And ((MaxPlanL >= {SheetL} AND MaxPlanW >= {SheetH}) OR (MaxPlanL >= {SheetH} AND MaxPlanW >= {SheetL}))  Order By MachineID, SheetRangeFrom, SheetRangeTo ";
                }

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return JsonConvert.SerializeObject(data.Message);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Get online Coating types
        /// </summary>
        [HttpGet]
        [Route("GetCoating")]
        public string GetCoating()
        {
            try
            {
                string version = DBConnection.Version;
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);

                if (version == "New")
                {
                    str = $"SELECT Distinct CoatingName From MachineOnlineCoatingRates Where Isnull(IsDeletedTransaction,0)=0 Group By CoatingName";
                }
                else
                {
                    str = $"SELECT Distinct CoatingName As CoatingName From MachineOnlineCoatingRates Where Isnull(IsDeletedTransaction,0)=0 Group By CoatingName";
                }

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return data.Message;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// ReLoad Plan Details
        /// </summary>
        [HttpPost]
        [Route("LoadPlanDetails/{BookingID}")]
        public string LoadPlanDetails(string BookingID)
        {
            try
            {
                var bookingId = BookingID;

                // Initialize DataTables
                var dtBooking = new DataTable();
                var dtCost = new DataTable();
                var dtContent = new DataTable();
                var dtProcess = new DataTable();
                var dtBookForms = new DataTable();
                var dtOneTimeCharges = new DataTable();
                var dtCorrugationPlyDetails = new DataTable();
                var dtAllocatedMaterials = new DataTable();
                var dtAllocatedMaterialParameters = new DataTable();
                var dtAllocatedMaterialLayers = new DataTable();
                var dtFileAttachment = new DataTable();
                var dtContentSpec = new DataTable();

                // Get session values
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                string str;

                // SQL Server queries (default path)
                str = $"SELECT Distinct JB.SalesType,JB.AnnualQuantity,JB.EnquiryID,JE.EnquiryNo,ISNULL(JE.Remark,'')as EnquiryRemark,JB.ArtWorkCode,JB.RefProductMasterCode,JB.BookingNo,JB.BookingID,JB.JobName, JB.SalesEmployeeID,JB.LedgerID, JB.CategoryID, OrderQuantity, TypeOfCost, FinalCost,Isnull(QuotedCost,0) As QuotedCost,Nullif(JB.BookingRemark,'') As BookingRemark, Nullif(JB.Remark,'') As Remark, ClientName, Nullif(JB.ProductCode,'') As ProductCode, ExpectedCompletionDays,JBC.PlanContentType,JBC.PlanContName, Isnull(JB.CurrencySymbol,'INR') As CurrencySymbol,Isnull(JB.ConversionValue,1) As ConversionValue,Isnull(JB.ConsigneeID,0) As ConsigneeID,Isnull(JB.ProductHSNID,0) As ProductHSNID, convert(varchar, JB.CreatedDate, 103) As CreatedDate,ShipperID,JB.EstimationUnit,Isnull(JB.IsQuotedRateTaxInclusive,0) AS IsQuotedRateTaxInclusive,JB.PlantID FROM JobBooking As JB Inner Join JobBookingContents As JBC On JB.BookingID=JBC.BookingID Left Join JobEnquiry as JE on JE.EnquiryID = JB.EnquiryID AND  ISNULL(JE.IsDeletedTransaction,0)<>1 WHERE (JB.BookingID = {bookingId}) /*And Isnull(IsCancelled,0)=0*/ And Isnull(IsEstimate,0)=1 And QuoteType ='Job Costing'  And Isnull(JB.IsDeletedTransaction,0)=0 And JB.CompanyID={GBLCompanyID}";
                DBConnection.FillDataTable(ref dtBooking, str);

                if (dtBooking.Rows.Count <= 0)
                    return "error code:404";

                str = $"SELECT JBC.KittingMultiplier,CorrugationQuantity,CorrugationAmount,MachineID, MachineName, Gripper, GripperSide, MachineColors, PaperID, PaperSize, CutSize, CutL, CutW, UpsL, UpsW, TotalUps, BalPiece, BalSide, WasteArea, WastePerc, WastageKg, GrainDirection, PlateQty, PlateRate, PlateAmount, MakeReadyWastageSheet, ActualSheets, WastageSheets,ProcessWastageSheets, TotalPaperWeightInKg, FullSheets, PaperRate, PaperAmount, PrintingImpressions, ImpressionsToBeCharged, PrintingRate, PrintingAmount, TotalMakeReadies, MakeReadyRate, MakeReadyAmount, FinalQuantity, TotalColors, TotalAmount, CutLH, CutHL, PrintingStyle, PrintingChargesType, ExpectedExecutionTime, TotalExecutionTime, MainPaperName, PlanType, PaperRateType, DieCutSize, InterlockStyle, NoOfSets, GrantAmount,UnitPrice, Packing, UnitPerPacking, RoundofImpressionsWith, SpeColorFCharges, SpeColorBCharges, Convert(real,SpeColorFAmt) as SpeColorFAmt, Convert(real,SpeColorBAmt) as SpeColorBAmt, OpAmt, PlanID, PlanContQty, PlanContentType, PlanContName, SequenceNo, Nullif(ContentSizeValues,'') As ContentSizeValues, CoatingCharges, CoatingAmount, PaperGroup,MachineType,CylinderToolID,CylinderToolCode,CylinderCircumferenceInch,CylinderCircumferenceMM,CylinderWidth,CylinderNoOfTeeth,FeedValue,AcrossGap,AroundGap,WastageStrip,RequiredRunningMeter,MakeReadyWastageRunningMeter,AvgBreakDownRunningMeter,WastageRunningMeter,ProcessWastageRunningMeter,TotalRequiredRunningMeter,RequiredSquareMeter,TotalRequiredSquareMeter,WastageSquareMeter,ScrapSquareMeter,MachineSpeed,MachinePerHourRate,PaperTotalGSM,RequiredPaperWeightKg,RollChangeWastageMeter,AverageRollLength,RollType,TotalProcessCost,TotalMaterialCost,TotalMachineCost,PaperFaceGSM,PaperReleaseGSM,PaperAdhesiveGSM,PaperMill, WindingDirectionID, LabelType, OutputType, PcsPerRoll, DieType, CoreInnerDia, CoreOuterDia, Isnull(FinalQuantityInPcs,FinalQuantity) AS FinalQuantityInPcs, Isnull(EstimationQuantityUnit,'PCS') AS EstimationQuantityUnit,CostingHeadGridSettingStr,Isnull(PlanOtherMaterialGSM,0) AS PlanOtherMaterialGSM,PlanOtherMaterialGSMSettingJSON,Isnull(TotalContentActualWeight,0) AS TotalContentActualWeight,SetupCost,ExecutionCost,MaterialCost,WastageCost,ToolCost,ProcessCost,ProcessWiseWastageType,LabourCost,ProcessWastagePercentage FROM JobBookingContents As JBC Inner Join JobBooking AS JB On JB.BookingID=JBC.BookingID  And Isnull(JB.IsDeletedTransaction,0)=0 And Isnull(JBC.IsDeletedTransaction,0)=0 WHERE (JB.BookingID = {bookingId}) /*And Isnull(JB.IsCancelled,0)=0*/ And Isnull(JB.IsEstimate,0)=1 And JB.QuoteType ='Job Costing' And JB.CompanyID={GBLCompanyID}";
                DBConnection.FillDataTable(ref dtContent, str);

                str = $"SELECT ISNULL(JBC.AnnualQuantity,0) as AnnualQuantity,ISNULL(JBC.CreditDays,0) as CreditDays,ISNULL(JBC.TotalWeightOfJob,0) as TotalWeightOfJob,ISNULL(JBC.FreightRate,0) as FreightRate,PlanContQty, TaxPercentage, PackingCostPercentage, MiscPercentage, ProfitPercentage,OverheadPercentage, ProfitFactor, DiscountPercentage,ExcisePercentage, TotalCost, PackingCost, MiscCost, ProfitCost, ProfitFactorCost,OverheadCost,FreightCost,InsuranceCost,ClearingCost, DiscountAmount,ExciseAmount, TaxAmount, GrandTotalCost, UnitCost, UnitCost1000, JBC.FinalCost,Isnull(JBC.ShipperCost,0) As ShipperCost,Isnull(JBC.QuotedCost,0) As QuotedCost FROM JobBookingCostings  As JBC Inner Join JobBooking AS JB On JB.BookingID=JBC.BookingID WHERE (JB.BookingID = {bookingId}) /*And Isnull(JB.IsCancelled,0)=0*/ And Isnull(JB.IsEstimate,0)=1 And JB.QuoteType ='Job Costing' And Isnull(JB.IsDeletedTransaction,0)=0 And Isnull(JBC.IsDeletedTransaction,0)=0 And JB.CompanyID={GBLCompanyID}";
                DBConnection.FillDataTable(ref dtCost, str);

                str = $"SELECT Distinct PM.ProcessID,PM.ProcessName,Nullif(PMS.RateFactor,'') AS RateFactor, Quantity, PlanID, PlanContQty, PlanContentType, PlanContName,ROUND(JBC.Rate,4) As Rate, Ups, NoOfPass, Pieces, NoOfStitch, NoOfLoops, NoOfColors, NoOfFolds, SizeL, SizeW, Amount, Nullif(Remarks,'') AS Remarks, JBC.SequenceNo,PM.MinimumCharges,PM.TypeofCharges,PM.SetupCharges,Isnull(JBC.IsDisplay,0) As IsDisplay,Isnull(PM.IsOnlineProcess,0) As IsOnlineProcess,Isnull(PM.Rate,0) As MasterRate,JBC.MachineID,JBC.MachineSpeed,JBC.MakeReadyTime,JBC.JobChangeOverTime,JBC.ExecutionTime,JBC.TotalExecutionTime,JBC.MakeReadyPerHourCost,JBC.MachinePerHourCost,JBC.MakeReadyMachineCost,JBC.ExecutionCost,JBC.MachineCost,MM.MachineName,Isnull(DM.DepartmentID,0) AS DepartmentID,Isnull(DM.SequenceNo,0) AS DepartmentSequenceNo,Isnull(PM.ProcessFlatWastageValue,0) AS ProcessFlatWastageValue,Isnull(PM.ProcessWastagePercentage,0) AS ProcessWastagePercentage,ISNULL(JBC.PagesPerSection,0) As PagesPerSection,ISNULL(JBC.NoOfForms,0) As NoOfForms,Isnull(PM.ProcessProductionType,'None') AS ProcessProductionType,Isnull(MM.PerHourCostingParameter,'') AS PerHourCostingParameter,Isnull(PM.MinimumQuantityToBeCharged,0) AS MinimumQuantityToBeCharged,Isnull(JBC.PerHourCalculationQuantity,0) As PerHourCalculationQuantity,JBC.SetupCost,JBC.NoOfPasses FROM JobBookingProcess  As JBC Inner Join JobBooking AS JB On JB.BookingID=JBC.BookingID  And Isnull(JB.IsDeletedTransaction,0)=0 And JBC.CompanyID=JB.CompanyID Inner Join ProcessMaster AS PM On PM.ProcessID=JBC.ProcessID And JBC.CompanyID=PM.CompanyID Inner Join DepartmentMaster AS DM On DM.DepartmentID=PM.DepartmentID Left Join ProcessMasterSlabs As PMS On PMS.ProcessID= PM.ProcessID And JBC.RateFactor=PMS.RateFactor LEFT JOIN MachineMaster AS MM ON MM.MachineID=Isnull(JBC.MachineID,0) WHERE (JB.BookingID = {bookingId}) /*And Isnull(JB.IsCancelled,0)=0*/ And Isnull(JB.IsEstimate,0)=1 And JB.QuoteType ='Job Costing' And Isnull(JBC.IsDeletedTransaction,0)=0 And JB.CompanyID={GBLCompanyID} Order By PlanID,SequenceNo";
                DBConnection.FillDataTable(ref dtProcess, str);

                str = $"SELECT PlanContQty, PlanContentType, PlanContName, Forms, Sets, Pages, Sheets, ImpressionsPerSet, FormsInPoint,FormPlanType, ImprsToChargedPerSet, BasicRate, SlabRate, RateType, Amount, WastagePercentSheet, PlateRate, PlanID,Isnull(WastageSheets,0) AS WastageSheets  FROM JobBookingContentBookForms  As JBC Inner Join JobBooking AS JB On JB.BookingID=JBC.BookingID And Isnull(JB.IsDeletedTransaction,0)=0 WHERE (JB.BookingID = {bookingId}) /*And Isnull(JB.IsCancelled,0)=0*/ And Isnull(JB.IsEstimate,0)=1 And JB.QuoteType ='Job Costing' And Isnull(JBC.IsDeletedTransaction,0)=0 And JB.CompanyID={GBLCompanyID}";
                DBConnection.FillDataTable(ref dtBookForms, str);

                str = $"Select JC.PlanContName,JC.PlanContQty,JC.PlanContentType, JC.Headname, JC.Amount	From JobBookingOnetimeCharges  as JC Inner Join JobBooking AS JB On JB.BookingID=JC.BookingID  WHERE (JB.BookingID = {bookingId})  And Isnull(JB.IsEstimate,0)=1 And JB.QuoteType ='Job Costing' And Isnull(JB.IsDeletedTransaction,0)=0 And JB.CompanyID={GBLCompanyID}";
                DBConnection.FillDataTable(ref dtOneTimeCharges, str);

                str = $"Select JC.PlanContName,JC.PlanContQty,JC.PlanContentType,JC.ProcessID,JC.MachineID,JC.TransID,JC.ItemID,ISGM.ItemSubGroupID,ISGM.ItemSubGroupName,JC.RequiredQuantityInStockUnit,JC.EstimatedQuantity,JC.EstimationRate,JC.EstimationUnit,JC.EstimatedAmount,MM.MachineName,PM.ProcessName,JC.BookedQtyInPurchaseUnit From JobBookingProcessMaterialRequirement  as JC INNER JOIN JobBooking AS JB On JB.BookingID=JC.BookingID LEFT JOIN ItemSubGroupMaster AS ISGM On ISGM.ItemSubGroupID=JC.ItemSubGroupID  And Isnull(ISGM.IsDeletedTransaction,0)=0 LEFT JOIN ProcessMaster AS PM On PM.ProcessID=JC.ProcessID LEFT JOIN MachineMaster AS MM On MM.MachineID=JC.MachineID Where (JB.BookingID = {bookingId})  And Isnull(JB.IsEstimate,0)=1 And JB.QuoteType ='Job Costing' And Isnull(JB.IsDeletedTransaction,0)=0 And JB.CompanyID={GBLCompanyID}";
                DBConnection.FillDataTable(ref dtAllocatedMaterials, str);

                str = $"Select JC.PlanContQty,JC.PlanContentType,JC.PlanContName,JC.BookingID,JC.JobContentsID,JC.ProcessID,JC.MachineID,JC.ItemID,JC.ItemSubGroupID,JC.TransID,JC.FieldName,JC.FieldDescription,JC.FieldDisplayName,JC.ItemMasterFieldName,JC.AppVariableName,JC.CalculationFormula,JC.DefaultValue,JC.DisplaySequenceNo,JC.DomainType,JC.IsDisplayField,JC.IsEditableField,JC.FieldValue,Isnull(JC.MinimumValue,0) AS MinimumValue,Isnull(JC.MaximumValue,0) AS MaximumValue From JobBookingProcessMaterialParameterDetail AS JC INNER JOIN JobBooking AS JB On JB.BookingID=JC.BookingID  Where (JB.BookingID = {bookingId})  And Isnull(JB.IsEstimate,0)=1 And JB.QuoteType ='Job Costing' And Isnull(JB.IsDeletedTransaction,0)=0 And JB.CompanyID={GBLCompanyID}";
                DBConnection.FillDataTable(ref dtAllocatedMaterialParameters, str);

                str = $"SELECT JC.BookingID, JC.ContentsID, JC.PlanContName, JC.PlanContentType, JC.PlanContQty, JC.LayerNo AS LayerNumber, JC.FluteType, JC.ItemID,Isnull(IM.ItemGroupID,0) AS ItemGroupID,Isnull(ISGM.ItemSubGroupID,0) AS ItemSubGroupID,IM.ItemCode,IM.Quality,IM.GSM,IM.Manufecturer,IM.Thickness,IM.Density,IM.SizeW,IM.ItemName,IGM.ItemGroupName,ISGM.ItemSubGroupName,JC.PlanContQty AS EstimationQuantity,JB.EstimationUnit AS EstimationQuantityUnit,0 AS FullSheets,JC.SheetSizeW, JC.SheetSizeL, JC.ImpressionLength, JC.PerImpressionWeight, JC.LayerContributionRatio, JC.ActualRequiredSheets, JC.ActualRequiredMeter, JC.ActualRequiredSqmeter, JC.ActualRequiredWeight, JC.WastageSheets, JC.WastageRunningMeter, JC.WastageSquareMeter, JC.WastageKg, JC.TotalRequiredSheets,JC.TotalRequiredRunningMeter, JC.TotalRequiredSquareMeter, JC.TotalPaperWeightInKg, JC.ItemEstimationRate AS EstimationRate, IM.EstimationUnit, JC.ItemEstimationAmount AS Amount, JC.WastePercentage, JC.TotalJobPaperWeightInKg, JC.TotalCorrugationWeight, JC.TotalImpressions, JC.ConversionRate, JC.ConversionAmount, JC.PerBoxWeight, JC.GSMTakeUp, JC.BurstingFactor, JC.BurstingStrength, JC.TotalLayerEstimationAmount, JC.TotalAmount FROM JobBookingContentsLayerDetail AS JC INNER JOIN JobBooking AS JB ON JB.BookingID=JC.BookingID INNER JOIN ItemMaster AS IM ON IM.ItemID=JC.ItemID INNER JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID=IM.ItemGroupID LEFT JOIN ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID=IM.ItemSubGroupID AND Isnull(ISGM.IsDeletedTransaction,0) =0 Where (JB.BookingID = {bookingId})  And Isnull(JB.IsEstimate,0)=1 And JB.QuoteType ='Job Costing' And Isnull(JB.IsDeletedTransaction,0)=0 And JB.CompanyID={GBLCompanyID} AND Isnull(JC.ItemID,0)>0 UNION ALL SELECT JC.BookingID, JC.ContentsID, JC.PlanContName, JC.PlanContentType, JC.PlanContQty, JC.LayerNo AS LayerNumber, JC.FluteType, JC.ItemID,Isnull(JC.ItemGroupID,0) AS ItemGroupID,Isnull(JC.ItemSubGroupID,0) AS ItemSubGroupID,'' AS ItemCode,JC.Quality,JC.GSM,JC.Manufecturer,JC.Thickness,JC.Density,JC.SizeW,JC.ItemName,IGM.ItemGroupName,ISGM.ItemSubGroupName,JC.PlanContQty AS EstimationQuantity,JB.EstimationUnit AS EstimationQuantityUnit,0 AS FullSheets,JC.SheetSizeW, JC.SheetSizeL, JC.ImpressionLength, JC.PerImpressionWeight, JC.LayerContributionRatio, JC.ActualRequiredSheets, JC.ActualRequiredMeter, JC.ActualRequiredSqmeter, JC.ActualRequiredWeight, JC.WastageSheets, JC.WastageRunningMeter, JC.WastageSquareMeter, JC.WastageKg, JC.TotalRequiredSheets,JC.TotalRequiredRunningMeter, JC.TotalRequiredSquareMeter, JC.TotalPaperWeightInKg, JC.ItemEstimationRate AS EstimationRate, JC.ItemEstimationUnit AS EstimationUnit,JC.ItemEstimationAmount AS Amount, JC.WastePercentage, JC.TotalJobPaperWeightInKg, JC.TotalCorrugationWeight,JC.TotalImpressions, JC.ConversionRate, JC.ConversionAmount, JC.PerBoxWeight, JC.GSMTakeUp, JC.BurstingFactor, JC.BurstingStrength, JC.TotalLayerEstimationAmount, JC.TotalAmount FROM JobBookingContentsLayerDetail AS JC INNER JOIN JobBooking AS JB ON JB.BookingID=JC.BookingID LEFT JOIN ItemGroupMaster AS IGM ON IGM.ItemGroupID=JC.ItemGroupID LEFT JOIN ItemSubGroupMaster AS ISGM ON ISGM.ItemSubGroupID=JC.ItemSubGroupID AND Isnull(ISGM.IsDeletedTransaction,0) =0 Where (JB.BookingID = {bookingId})  And Isnull(JB.IsEstimate,0)=1 And JB.QuoteType ='Job Costing' And Isnull(JB.IsDeletedTransaction,0)=0 And JB.CompanyID={GBLCompanyID} AND Isnull(JC.ItemID,0)=0 Order By LayerNumber";
                DBConnection.FillDataTable(ref dtAllocatedMaterialLayers, str);

                str = $"Select JC.PlanContName,JC.PlanContQty,JC.PlanContentType,JC.TotalWeightOfJob,JC.CorrugationWeight,JC.BoxWeight,JC.ConversionPerKG,JC.CalculationOn,JC.TotalAmount,JC.ConversionAmount,JC.GrandTotal , Isnull(IM.ItemID,0) As ItemID,  Isnull(IM.ItemCode,'') as ItemCode, JC.PlyNo, JC.FluteName, JC.ItemDetails, JC.Weight, JC.Rate, JC.Amount, JC.Waste, JC.Width, JC.TakeUpFactor AS TakeupFactor, JC.GSM, JC.BF, JC.BS, JC.Sheets, JC.ReqDeckle, JC.DeckleCuts,JC.Deckle,JC.Cutting  From ItemMaster as IM RIGHT JOIN JobBookingCorrugation  as JC ON JC.ItemID = IM.ItemID  Inner Join JobBooking AS JB On JB.BookingID=JC.BookingID WHERE (JB.BookingID = {bookingId}) And Isnull(JB.IsEstimate,0)=1 And JB.QuoteType ='Job Costing' And Isnull(JB.IsDeletedTransaction,0)=0 And JB.CompanyID = {GBLCompanyID}";
                DBConnection.FillDataTable(ref dtCorrugationPlyDetails, str);

                str = $"select JA.AttachementID,JA.BookingID,JA.FilePath as AttachedFileName ,JA.FilePath as  AttachedFileUrl from JobBookingAttachments as JA Inner join JobBooking as JB on JB.BookingID =JA.BookingID where isnull(JA.IsDeletedTransaction,0) <>1 and JB.BookingID = {bookingId}";
                DBConnection.FillDataTable(ref dtFileAttachment, str);

                str = $"SELECT JobBookingContentsSpecificationID, BookingID, ContentsID, JobContentsID, PlanContQty, PlanContName, PlanContentType, SizeHeight, SizeLength, SizeWidth, SizeOpenflap, SizePastingflap, SizeBottomflap, JobNoOfPages, JobUps, JobFlapHeight, JobTongHeight, JobFoldedH, JobFoldedL, PlanFColor, PlanBColor, PlanSpeFColor, PlanSpeBColor, PlanColorStrip, PlanGripper, PlanPrintingStyle, PlanWastageValue, Trimmingleft, Trimmingright, Trimmingtop, Trimmingbottom, Stripingleft, Stripingright, Stripingtop, Stripingbottom, PlanPrintingGrain, ItemPlanQuality, ItemPlanGsm, ItemPlanThickness, ItemPlanMill, PlanPlateType, PlanWastageType, ItemPlanFinish, ItemPlanBF, OperId, JobBottomPerc, JobPrePlan, ChkPlanInSpecialSizePaper, ChkPlanInStandardSizePaper, MachineId, PlanOnlineCoating, PaperTrimleft, PaperTrimright, PaperTrimtop, PaperTrimbottom, ChkPaperByClient, ChkPlanInAvailableStock, JobFoldInL, JobFoldInH, PlanPlateBearer, PlanStandardARGap, PlanStandardACGap, PlanContDomainType, Planlabeltype, Planwindingdirection, Planfinishedformat, Plandietype, PlanPcsPerRoll, PlanCoreInnerDia, PlanCoreOuterDia, EstimationQuantityUnit,SizeTopSeal, SizeSideSeal, SizeBottomGusset, SizeCenterSeal, PlanMakeReadyWastage, ProductionUnitID, CategoryID, BookSpine, BookHinge, BookCoverTurnIn, BookExtension, BookLoops, PlanOtherMaterialGSM, PlanOtherMaterialGSMSettingJSON, PlanPunchingType,ChkBackToBackPastingRequired,JobAcrossUps,JobAroundUps,SizeOpenflapPer,SizeBottomflapPer,SizeZipperLength,ZipperWeightPerMeter,JobSizeInputUnit,MaterialWetGSMConfigJSON,LedgerID,ShowPlanUptoWastePercent,PlanSpoutType FROM JobBookingContentsSpecification WHERE isnull(IsDeletedTransaction,0) <>1 and BookingID = {bookingId}";
                DBConnection.FillDataTable(ref dtContentSpec, str);

                // Set DataTable names
                dtBooking.TableName = "TblBooking";
                dtContent.TableName = "TblBookingContents";
                dtCost.TableName = "TblBookingCosting";
                dtProcess.TableName = "TblBookingProcess";
                dtBookForms.TableName = "TblBookingForms";
                dtOneTimeCharges.TableName = "Tblonetimecharges";
                dtCorrugationPlyDetails.TableName = "TblCorrugationPlyDetails";
                dtAllocatedMaterials.TableName = "TblAllocatedMaterials";
                dtAllocatedMaterialParameters.TableName = "TblMaterialCostParams";
                dtAllocatedMaterialLayers.TableName = "TblAllocatedMaterialLayers";
                dtFileAttachment.TableName = "FileAttachment";
                dtContentSpec.TableName = "TblContentsSpecification";

                // Create and merge DataSet
                var dataSet = new DataSet();
                dataSet.Merge(dtBooking);
                dataSet.Merge(dtContent);
                dataSet.Merge(dtCost);
                dataSet.Merge(dtProcess);
                dataSet.Merge(dtBookForms);
                dataSet.Merge(dtOneTimeCharges);
                dataSet.Merge(dtCorrugationPlyDetails);
                dataSet.Merge(dtAllocatedMaterials);
                dataSet.Merge(dtAllocatedMaterialParameters);
                dataSet.Merge(dtAllocatedMaterialLayers);
                dataSet.Merge(dtFileAttachment);
                dataSet.Merge(dtContentSpec);

                // Convert DataSet to JSON and return
                var jsonResult = DBConnection.ConvertDataSetsToJsonString(dataSet);
                return jsonResult;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Set Is send for Approval
        /// </summary>
        [HttpPost]
        [Route("UpdateSendForApproval/{BKID}")]
        public string UpdateSendForApproval(string BKID)
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                GBLUserID = Convert.ToString(HttpContext.Current.Session["UserID"]);

                if (DBType == "MYSQL")
                {
                    str = $"Update JobBooking Set IsSendForPriceApproval=1,ApprovalSendDate=NOW(),ApprovalSendBy={GBLUserID} Where CompanyId = {GBLCompanyID} And BookingID In ({BKID})";
                }
                else
                {
                    str = $"Update JobBooking Set IsSendForPriceApproval=1,ApprovalSendDate=Getdate(),ApprovalSendBy={GBLUserID} Where CompanyId = {GBLCompanyID} And BookingID In ({BKID})";
                }

                DBConnection.ExecuteNonSQLQuery(str);
                return "Save";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Update Expected Delivery Date
        /// </summary>
        [HttpPost]
        [Route("UpdateExpectedDeliveryDate/{ExpectedDeliveryDate}/{BKID}")]
        public string UpdateExpectedDeliveryDate(string ExpectedDeliveryDate, string BKID)
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                GBLUserID = Convert.ToString(HttpContext.Current.Session["UserID"]);

                if (DBType == "MYSQL")
                {
                    str = $"Update JobBooking Set ExpectedDeliveryDate='{ExpectedDeliveryDate}' Where CompanyId = {GBLCompanyID} And BookingID In ({BKID})";
                }
                else
                {
                    str = $"Update JobBooking Set ExpectedDeliveryDate='{ExpectedDeliveryDate}' Where CompanyId = {GBLCompanyID} And BookingID In ({BKID})";
                }

                DBConnection.ExecuteNonSQLQuery(str);
                return "Save";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Set Un send for Approval
        /// </summary>
        [HttpPost]
        [Route("UpdateUnSendForApproval/{BKID}")]
        public string UpdateUnSendForApproval(string BKID)
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                GBLUserID = Convert.ToString(HttpContext.Current.Session["UserID"]);

                if (DBType == "MYSQL")
                {
                    str = $"Update JobBooking Set IsSendForPriceApproval=0,ApprovalSendDate=NOW(),ApprovalSendBy={GBLUserID} Where CompanyId = {GBLCompanyID} And BookingID In ({BKID})";
                }
                else
                {
                    str = $"Update JobBooking Set IsSendForPriceApproval=0,ApprovalSendDate=Getdate(),ApprovalSendBy={GBLUserID} Where CompanyId = {GBLCompanyID} And BookingID In ({BKID})";
                }

                DBConnection.ExecuteNonSQLQuery(str);
                return "Save";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [HttpPost]
        [Route("UpdateSendForIA/{BKID}/{flag}/{SendTo}")]
        public IHttpActionResult UpdateSendForIA(string BKID, int flag, int SendTo)
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                string str = $"Update JobBooking Set IsSendForInternalApproval={flag},ApprovalSendTo={SendTo} Where CompanyId = {GBLCompanyID} And BookingID In ({BKID})";
                DBConnection.ExecuteNonSQLQuery(str);

                return Ok("Save");
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [HttpPost]
        [Route("DeleteQuotation/{bkId}")]
        public IHttpActionResult DeleteQuotation(string bkId)
        {
            string keyField;

            try
            {

                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                GBLUserID = Convert.ToString(HttpContext.Current.Session["UserID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                if (DBConnection.IsDeletable("BookingId", "JobApprovedCost", $"Where BookingId = {bkId} And CompanyID = {GBLCompanyID} And IsDeletedTransaction = 0 ") == false)
                {
                    keyField = "false";
                    return Ok(keyField);
                }

                if (DBConnection.IsDeletable("BookingId", "JobOrderBookingDetails", $"Where BookingId = {bkId} And CompanyID = {GBLCompanyID} and IsDeletedTransaction = 0 ") == false)
                {
                    keyField = "false";
                    return Ok(keyField);
                }
                var connection = new SqlConnection();
                var command = new SqlCommand();

                connection = DBConnection.OpenConnection();
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                var transaction = connection.BeginTransaction();

                try
                {
                    command.Transaction = transaction;
                    string sql;

                    // Update JobBooking
                    sql = $"Update JobBooking Set IsDeletedTransaction=1,DeletedDate=getdate(),DeletedBy={GBLUserID} where BookingId = '{bkId}' And CompanyID = {GBLCompanyID}";
                    command = new SqlCommand(sql, connection, transaction);
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();

                    // Update JobBookingProcess
                    sql = $"Update JobBookingProcess Set IsDeletedTransaction=1,DeletedDate=getdate(),DeletedBy={GBLUserID} Where BookingId = '{bkId}' And CompanyID = {GBLCompanyID}";
                    command = new SqlCommand(sql, connection, transaction);
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();

                    // Update JobBookingContents
                    sql = $"Update JobBookingContents Set SheetLayout='',UpsLayout='',IsDeletedTransaction=1,DeletedDate=getdate(),DeletedBy={GBLUserID} where BookingId = '{bkId}' And CompanyID = {GBLCompanyID}";
                    command = new SqlCommand(sql, connection, transaction);
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();

                    // Update JobBookingCostings
                    sql = $"Update JobBookingCostings Set IsDeletedTransaction=1,DeletedDate=getdate(),DeletedBy={GBLUserID} where BookingId = '{bkId}' And CompanyID = {GBLCompanyID}";
                    command = new SqlCommand(sql, connection, transaction);
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();

                    // Update JobBookingContentBookForms
                    sql = $"Update JobBookingContentBookForms Set IsDeletedTransaction=1,DeletedDate=getdate(),DeletedBy={GBLUserID} where BookingId = '{bkId}' And CompanyID = {GBLCompanyID}";
                    command = new SqlCommand(sql, connection, transaction);
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();

                    // Update JobBookingAttachments
                    sql = $"Update JobBookingAttachments Set IsDeletedTransaction=1,DeletedDate=getdate(),DeletedBy={GBLUserID} where BookingId = '{bkId}' And CompanyID = {GBLCompanyID}";
                    command = new SqlCommand(sql, connection, transaction);
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();

                    // Update JobBookingProcessMaterialRequirement
                    sql = $"Update JobBookingProcessMaterialRequirement Set IsDeletedTransaction=1,DeletedDate=getdate(),DeletedBy={GBLUserID} where BookingId = '{bkId}' And CompanyID = {GBLCompanyID}";
                    command = new SqlCommand(sql, connection, transaction);
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();

                    // Update JobBookingProcessMaterialParameterDetail
                    sql = $"Update JobBookingProcessMaterialParameterDetail Set IsDeletedTransaction=1,DeletedDate=getdate(),DeletedBy={GBLUserID} where BookingId = '{bkId}' And CompanyID = {GBLCompanyID}";
                    command = new SqlCommand(sql, connection, transaction);
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();

                    // Update JobBookingContentsLayerDetail
                    sql = $"Update JobBookingContentsLayerDetail Set IsDeletedTransaction=1,DeletedDate=getdate(),DeletedBy={GBLUserID} where BookingId = '{bkId}' And CompanyID = {GBLCompanyID}";
                    command = new SqlCommand(sql, connection, transaction);
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();

                    // Update JobBookingContentsSpecification
                    sql = $"Update JobBookingContentsSpecification Set IsDeletedTransaction=1,DeletedDate=getdate(),DeletedBy={GBLUserID} where BookingId = '{bkId}' And CompanyID = {GBLCompanyID}";
                    command = new SqlCommand(sql, connection, transaction);
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();

                    transaction.Commit();
                    connection.Close();
                    keyField = "Success";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    connection.Close();
                    keyField = ex.Message;
                }
            }
            catch (Exception ex)
            {
                keyField = ex.Message;
            }

            return Ok(keyField);
        }

        [HttpGet]
        [Route("GetSbCurrency")]
        public IHttpActionResult GetSbCurrency()
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                string str = "SELECT CurrencyID, Nullif(Replace(CurrencyName,'\"',''),'') As CurrencyName, CurrencyCode, CurrencySymbol, ConversionValue, INRValue FROM CurrencyMaster Order By CurrencyName";
                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return Ok(JsonConvert.SerializeObject(data.Message));
            }
            catch (Exception)
            {
                return Ok("500");
            }
        }

        [HttpGet]
        [Route("LoadShippersList/{BKID}/{PlanQty}")]
        public IHttpActionResult LoadShippersList(int BKID, int PlanQty)
        {
            try
            {
                string companyId = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                string dbType = DBType;
                string str;

                if (dbType == "MYSQL")
                {
                    str = $"SELECT Distinct EstimatedQuantity As TotalShipperQtyReq, ItemID As ShipperID,Convert(nvarchar(30),SizeL, 106) +'x'+Convert(nvarchar(30),SizeW, 106) +'x'+Convert(nvarchar(30),SizeH, 106)  As ShipperName,IFNULL(SizeL,0) As SizeL, IFNULL(SizeW,0) As SizeW,IFNULL(SizeH,0) As SizeH, IFNULL(EmptyCartonWt,0) As EmptyCartonWt,IFNULL(Capacity,0) As Capacity,'SHIPPER CARTON' As ItemGroupName,ItemGroupID,IFNULL(CBM,0) As CBM,IFNULL(CBF,0) As CBF,PackX ,PackY ,PackZ ,NoOfPly,QtyPerShipper ,EstimatedRate As ShipperRate,EstimatedCost As ShipperCost,ShippingRate,ShippingCost,TotalWtOfAllShippers,ShipperWeightPerPack,ProductLength ,ProductWidth ,ProductHeight ,ProductWt From JobBookingMaterialCost Where BookingID={BKID} And JobQuantity= {PlanQty} And IFNULL(IsDeletedTransaction,0)<>1 And CompanyID ={companyId}";
                }
                else
                {
                    str = $"SELECT Distinct EstimatedQuantity As TotalShipperQtyReq, ItemID As ShipperID,Convert(nvarchar(30),SizeL, 106) +'x'+Convert(nvarchar(30),SizeW, 106) +'x'+Convert(nvarchar(30),SizeH, 106)  As ShipperName,Isnull(SizeL,0) As SizeL, Isnull(SizeW,0) As SizeW,Isnull(SizeH,0) As SizeH, Isnull(EmptyCartonWt,0) As EmptyCartonWt,Isnull(Capacity,0) As Capacity,'SHIPPER CARTON' As ItemGroupName,ItemGroupID,Isnull(CBM,0) As CBM,Isnull(CBF,0) As CBF,PackX ,PackY ,PackZ ,NoOfPly,QtyPerShipper ,EstimatedRate As ShipperRate,EstimatedCost As ShipperCost,ShippingRate,ShippingCost,TotalWtOfAllShippers,ShipperWeightPerPack,ProductLength ,ProductWidth ,ProductHeight ,ProductWt From JobBookingMaterialCost Where BookingID={BKID} And JobQuantity= {PlanQty} And Isnull(IsDeletedTransaction,0)<>1 And CompanyID ={companyId}";
                }

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return Ok(JsonConvert.SerializeObject(data.Message));
            }
            catch (Exception)
            {
                return Ok("500");
            }
        }

        [HttpGet]
        [Route("LoadContainersList/{BKID}")]
        public IHttpActionResult LoadContainersList(int BKID)
        {
            try
            {
                string companyId = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                string dbType = DBType;
                string str;

                if (dbType == "MYSQL")
                {
                    str = $"SELECT JBMC.ContainerID, JBMC.BoxInLength AS CountL, JBMC.BoxInWidth AS CountW, JBMC.BoxInHeight AS CountH, JBMC.TotalCarton, JBMC.TotalContainers, JBMC.TotalCBF AS CBM, JBMC.TotalCBM AS CBF, JBMC.TotalContainerWt AS TotalWt, JBMC.BoxDirection AS Direction, CM.LengthMM, CM.WidthMM, CM.HeightMM, CM.LengthFT, CM.WidthFT, CM.HeightFT, CM.MaxWeight, CM.ContainerName FROM JobBookingMaterialCost AS JBMC INNER JOIN ContainerMaster AS CM ON JBMC.ContainerID = CM.ContainerID AND JBMC.CompanyID = CM.CompanyID WHERE (JBMC.BookingID = {BKID} ) And IFNULL(JBMC.IsDeletedTransaction,0)<>1 And JBMC.CompanyID ={companyId}";
                }
                else
                {
                    str = $"SELECT JBMC.ContainerID, JBMC.BoxInLength AS CountL, JBMC.BoxInWidth AS CountW, JBMC.BoxInHeight AS CountH, JBMC.TotalCarton, JBMC.TotalContainers, JBMC.TotalCBF AS CBM, JBMC.TotalCBM AS CBF, JBMC.TotalContainerWt AS TotalWt, JBMC.BoxDirection AS Direction, CM.LengthMM, CM.WidthMM, CM.HeightMM, CM.LengthFT, CM.WidthFT, CM.HeightFT, CM.MaxWeight, CM.ContainerName FROM JobBookingMaterialCost AS JBMC INNER JOIN ContainerMaster AS CM ON JBMC.ContainerID = CM.ContainerID AND JBMC.CompanyID = CM.CompanyID WHERE (JBMC.BookingID = {BKID} ) And Isnull(JBMC.IsDeletedTransaction,0)<>1 And JBMC.CompanyID ={companyId}";
                }

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return Ok(JsonConvert.SerializeObject(data.Message));
            }
            catch (Exception)
            {
                return Ok("500");
            }
        }

        [HttpGet]
        [Route("LoadShippersID/{ShipperName}")]
        public IHttpActionResult LoadShippersID(string ShipperName)
        {
            try
            {
                string companyId = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                string dbType = DBType;
                string str;

                if (dbType == "MYSQL")
                {
                    str = $"Select IFNULL(ItemID,0) As ItemID From ItemMaster Where ItemName Like '%{ShipperName}%' And ItemGroupID=7 And CompanyID={companyId} And IFNULL(IsDeletedTransaction,0)<>1";
                }
                else
                {
                    str = $"Select Isnull(ItemID,0) As ItemID From ItemMaster Where ItemName Like '%{ShipperName}%' And ItemGroupID=7 And CompanyID={companyId} And Isnull(IsDeletedTransaction,0)<>1";
                }

                DBConnection.FillDataTable(ref dataTable, str);
                if (dataTable.Rows.Count > 0)
                {
                    return Ok(Convert.ToDouble(dataTable.Rows[0][0]));
                }
                return Ok(0);
            }
            catch (Exception)
            {
                return Ok("Error");
            }
        }

        [HttpGet]
        [Route("GetCoatingMachines/{coating}")]
        public IHttpActionResult GetCoatingMachines(string coating)
        {
            try
            {
                string companyId = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                string dbType = DBType;
                string str;

                if (dbType == "MYSQL")
                {
                    str = $"Select Distinct nullif(MM.MachineID,'') As MachineID, nullif(MM.MachineName,'') As MachineName From MachineMaster As MM Where MM.CompanyID={companyId} And MM.MachineId In (Select Distinct MachineID From MachineOnlineCoatingRates Where CoatingName='{coating}' And CompanyID={companyId} ) And IFNULL(MM.IsDeletedTransaction,0)<>1 Order By MachineName";
                }
                else
                {
                    str = $"Select Distinct nullif(MM.MachineID,'') As MachineID, nullif(MM.MachineName,'') As MachineName From MachineMaster As MM Where MM.CompanyID={companyId} And MM.MachineId In (Select Distinct MachineID From MachineOnlineCoatingRates Where CoatingName='{coating}' And CompanyID={companyId} ) And Isnull(MM.IsDeletedTransaction,0)<>1 Order By MachineName";
                }

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return Ok(JsonConvert.SerializeObject(data.Message));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [HttpGet]
        [Route("LoadCategoryWiseContents/{CID}")]
        public IHttpActionResult LoadCategoryWiseContents(int CID)
        {
            try
            {
                string companyId = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                string dbType = Convert.ToString(HttpContext.Current.Session["DBType"]);
                string str;

                if (dbType == "MYSQL")
                {
                    str = $"Select Distinct CM.ContentCaption As PlanContName,CM.ContentName As PlanContentType,CM.ContentDomainType,REPLACE(Replace(ContentClosedHref,'images/Contents/',''),'.jpg','') AS ContentClosedHref From CategoryContentAllocationMaster As CAM Inner Join ContentMaster As CM On CM.ContentID=CAM.ContentID And CM.CompanyID=CAM.CompanyID Where CAM.CategoryID='{CID}' And CAM.CompanyID={companyId} And IFNULL(CAM.IsDeletedTransaction,0)<>1";
                }
                else
                {
                    str = $"Select Distinct CM.ContentCaption As PlanContName,CM.ContentName As PlanContentType,CM.ContentDomainType,REPLACE(Replace(ContentClosedHref,'images/Contents/',''),'.jpg','') AS ContentClosedHref,Isnull(CM.ContentSizeInputUnit,'MM') AS ContentSizeInputUnit,Isnull(CM.ContentSizeInputDecimalValue,2) AS ContentSizeInputDecimalValue From CategoryContentAllocationMaster As CAM Inner Join ContentMaster As CM On CM.ContentID=CAM.ContentID And CM.CompanyID=CAM.CompanyID Where CAM.CategoryID='{CID}' And CAM.CompanyID={companyId} And Isnull(CAM.IsDeletedTransaction,0)<>1 AND ISNULL(IsDefaultContent,0)=1";
                }

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return Ok(JsonConvert.SerializeObject(data.Message));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        //[HttpPost]
        //[Route("GetCommentData")]
        //public IActionResult GetCommentData([FromBody] string BookingID, [FromBody] string PriceApprovalID, [FromBody] string OrderBookingIDs, [FromBody] string ProductMasterID, [FromBody] string JobBookingID)
        //{
        //    Response.Clear();
        //    Response.ContentType = "application/json";

        //    string companyId = GBLCompanyID;
        //    string userId = GBLUserID;
        //    string dbType = DBType;
        //    string str = "";

        //    if (dbType == "MYSQL")
        //    {
        //        if (BookingID != "0")
        //            str = $" CALL GetCommentData( {companyId},'Estimation',0,0,0,'{BookingID}',0,0,0,0);";
        //        else if (PriceApprovalID != "0")
        //            str = $" CALL GetCommentData( {companyId},'Price Approval',0,0,0,0,'{PriceApprovalID}',0,0,0);";
        //        else if (OrderBookingIDs != "0")
        //            str = $" CALL GetCommentData( {companyId},'Sales Order Booking',0,0,0,0,0,'{OrderBookingIDs}',0,0);";
        //        else if (ProductMasterID != "0")
        //            str = $" CALL GetCommentData( {companyId},'Product Catalog',0,0,0,0,0,0,'{ProductMasterID}',0);";
        //        else if (JobBookingID != "0")
        //            str = $" CALL GetCommentData( {companyId},'Production Work Order',0,0,0,0,0,0'{JobBookingID}');";
        //    }
        //    else
        //    {
        //        if (BookingID != "0")
        //            str = $" EXEC GetCommentData {companyId},'Estimation',0,0,0,'{BookingID}',0,0,0,0";
        //        else if (PriceApprovalID != "0")
        //            str = $" EXEC GetCommentData {companyId},'Price Approval',0,0,0,0,'{PriceApprovalID}',0,0,0";
        //        else if (OrderBookingIDs != "0")
        //            str = $" EXEC GetCommentData {companyId},'Sales Order Booking',0,0,0,0,0,'{OrderBookingIDs}',0,0";
        //        else if (ProductMasterID != "0")
        //            str = $" EXEC GetCommentData {companyId},'Product Catalog',0,0,0,0,0,0,'{ProductMasterID}',0";
        //        else if (JobBookingID != "0")
        //            str = $" EXEC GetCommentData {companyId},'Production Work Order',0,0,0,0,0,0'{JobBookingID}'";
        //    }

        //    DBConnection.FillDataTable(ref dataTable, str);
        //    data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
        //    return Ok(JsonConvert.SerializeObject(data.Message, new JsonSerializerSettings { MaxDepth = 2147483647 }));
        //}

        //[HttpPost("SaveCommentData")]
        //public IActionResult SaveCommentData([FromBody] object jsonObjectCommentDetail)
        //{
        //    var dt = new DataTable();
        //    string keyField;
        //    string addColName, addColValue, tableName, gblFYear;

        //    string companyId = GBLCompanyID;
        //    string userId = GBLUserID;
        //    gblFYear = _httpContextAccessor.HttpContext.Session.GetString("FYear");
        //    string dbType = DBType;

        //    try
        //    {
        //        tableName = "CommentChainMaster";
        //        addColName = "ModifiedDate,CreatedDate,UserID,CompanyID,FYear,CreatedBy,ModifiedBy";

        //        if (dbType == "MYSQL")
        //            addColValue = $"Now(),Now(),'{userId}','{companyId}','{gblFYear}','{userId}','{userId}'";
        //        else
        //            addColValue = $"GetDate(),GetDate(),'{userId}','{companyId}','{gblFYear}','{userId}','{userId}'";

        //        DBConnection.InsertDatatableToDatabase(jsonObjectCommentDetail, tableName, addColName, addColValue);
        //        keyField = "Success";
        //    }
        //    catch (Exception)
        //    {
        //        keyField = "fail";
        //    }

        //    return Ok(keyField);
        //}
        [HttpGet]
        [Route("LoadItemStockDetails/{ItmID}")]
        public string LoadItemStockDetails(int ItmID)
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                if (DBType == "MYSQL")
                {
                    str = "SELECT ItemName, ItemCode, ItemDescription, PhysicalStock, BookedStock, AllocatedStock, IncomingStock, FloorStock, UnapprovedStock, IndentStock, RequisitionStock FROM ItemMaster Where CompanyID=" + GBLCompanyID + " And ItemID =" + ItmID + " And IFNULL(IsDeletedTransaction,0)<>1";
                }
                else
                {
                    str = "SELECT ItemName, ItemCode, ItemDescription, PhysicalStock, BookedStock, AllocatedStock, IncomingStock, FloorStock, UnapprovedStock, IndentStock, RequisitionStock FROM ItemMaster Where CompanyID=" + GBLCompanyID + " And ItemID =" + ItmID + " And Isnull(IsDeletedTransaction,0)<>1";
                }

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return JsonConvert.SerializeObject(data.Message);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [HttpGet]
        [Route("GetMailQuoteData/{JobBKID}")]
        public string GetMailQuoteData(string JobBKID)
        {
            try
            {
                DataTable DTQuotes = new DataTable();
                DataTable DTQty = new DataTable();

                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                if (DBType == "MYSQL")
                {
                    str = "Select Distinct JEJ.PlanContName, JE.BookingID, JE.JobName, JE.FinalCost, JE.BookingRemark, JE.Remark, JE.ClientName, JE.ConcernPerson, Case When IFNULL(JE.HeaderText,'')='' Then IFNULL(UMD.HeaderText,'') Else JE.HeaderText End As HeaderText, Case When IFNULL(JE.FooterText,'')='' Then IFNULL(UMD.FooterText,'') Else JE.FooterText End As FooterText, JE.ProductCode, JE.ReworkRemark, JE.ReasonsofQuote, JE.ConversionValue, Case When IFNULL(JE.MailingName,'')='' Then LM.LedgerName Else JE.MailingName End As MailingName,Case When IFNULL(JE.MailingAddress,'')='' Then LMD.FieldValue Else JE.MailingAddress End As MailingAddress, JE.EmailSubject, JE.ProcessContentRemarks, UM.UserName,UM.Designation, JE.BookingNo, JE.OrderQuantity, Case When IFNULL(JE.EmailBody,'')='' Then IFNULL(UMD.EmailMessage,'') Else JE.EmailBody End As EmailBody, JE.IsMailSent, JE.RemarkInternalApproval, JE.RemarkInternalApproved, JE.CurrencySymbol, JE.CurrencyName, JE.EmailTo, Case When IFNULL(JE.HeaderText,'')='' Then IFNULL(UMD.ExportHeaderText,'') Else JE.HeaderText End As ExportHeaderText,Case When IFNULL(JE.FooterText,'')='' Then IFNULL(UMD.ExportFooterText,'') Else JE.FooterText End As ExportFooterText,JE.IsExportQuotation, Stuff((Select ', '+P.DisplayProcessName + Case When IFNULL(S.RateFactor,'')='' Then '' Else '(' + S.RateFactor + ')' End From ProcessMaster As P Inner Join JobBookingProcess As S On S.BookingID=JEJ.BookingID And S.ContentsID=JEJ.JobContentsID And P.ProcessID=S.ProcessID And S.CompanyID=JEJ.CompanyID Order By S.SequenceNo For XML PATH('')),1,1,'') As ProcessDetail " +
                          " From JobBooking As JE  INNER Join JobBookingContents AS JEJ on JEJ.BookingID = JE.BookingID  And JEJ.CompanyID=JE.CompanyID And IFNULL(JE.IsDeletedTransaction,0)=0 Inner Join JobBookingProcess As JEO On JEO.ContentsID=JEJ.JobContentsID And JEJ.CompanyID=JEO.CompanyID Inner Join UserMaster As UM On JE.QuotedByUserID=UM.UserID And JE.CompanyID=UM.CompanyID And IFNULL(UM.IsBlocked,0)=0 INNER JOIN LedgerMaster As LM On JE.LedgerID=LM.LedgerID And JE.CompanyID=LM.CompanyID And IFNULL(LM.IsDeletedTransaction,0)=0 INNER JOIN LedgerMasterDetails As LMD On LM.LedgerID=LMD.LedgerID And LM.CompanyID=LMD.CompanyID And IFNULL(LMD.IsDeletedTransaction,0)=0 And LMD.FieldName='MailingAddress' Left Join UserMaster As UMD On UMD.UserName ='ADMIN' And JE.CompanyID=UM.CompanyID And IFNULL(UM.IsBlocked,0)=0 Where JE.CompanyID=" + GBLCompanyID + " And JE.BookingID =" + JobBKID + " And IFNULL(JE.IsDeletedTransaction,0)<>1";
                }
                else
                {
                    str = "Select Distinct JEJ.PlanContName, JE.BookingNo, JE.BookingID, JE.JobName, JE.FinalCost, JE.BookingRemark, JE.Remark, JE.ClientName, JE.ConcernPerson, Case When Isnull(JE.HeaderText,'')='' Then Isnull(UMD.HeaderText,'') Else JE.HeaderText End As HeaderText, Case When Isnull(JE.FooterText,'')='' Then Isnull(UMD.FooterText,'') Else JE.FooterText End As FooterText, JE.ProductCode, JE.ReworkRemark, JE.ReasonsofQuote, JE.ConversionValue, Case When Isnull(JE.MailingName,'')='' Then LM.LedgerName Else JE.MailingName End As MailingName,Case When Isnull(JE.MailingAddress,'')='' Then LMD.FieldValue Else JE.MailingAddress End As MailingAddress, JE.EmailSubject, JE.ProcessContentRemarks, UM.UserName,UM.Designation, JE.BookingNo, JE.OrderQuantity, Case When Isnull(JE.EmailBody,'')='' Then Isnull(UMD.EmailMessage,'') Else JE.EmailBody End As EmailBody, JE.IsMailSent, JE.RemarkInternalApproval, JE.RemarkInternalApproved, JE.CurrencySymbol, JE.CurrencyName, JE.EmailTo, Case When Isnull(JE.HeaderText,'')='' Then Isnull(UMD.ExportHeaderText,'') Else JE.HeaderText End As ExportHeaderText,Case When Isnull(JE.FooterText,'')='' Then Isnull(UMD.ExportFooterText,'') Else JE.FooterText End As ExportFooterText,JE.IsExportQuotation, Stuff((Select ', '+P.DisplayProcessName + Case When Isnull(S.RateFactor,'')='' Then '' Else '(' + S.RateFactor + ')' End From ProcessMaster As P Inner Join JobBookingProcess As S On S.BookingID=JEJ.BookingID And S.ContentsID=JEJ.JobContentsID And P.ProcessID=S.ProcessID And S.CompanyID=JEJ.CompanyID Group by P.DisplayProcessName,S.RateFactor,S.SequenceNo Order By S.SequenceNo For XML PATH('')),1,1,'') As ProcessDetail " +
                          " From JobBooking As JE  INNER Join JobBookingContents AS JEJ on JEJ.BookingID = JE.BookingID  And JEJ.CompanyID=JE.CompanyID And Isnull(JE.IsDeletedTransaction,0)=0 INNER JOIN (SELECT J.JobContentsID, J.PlanContentType, J.PlanContName, J.BookingID FROM JobBookingContents J INNER JOIN (SELECT BookingID, MIN(JobContentsID) AS MinID FROM JobBookingContents WHERE ISNULL(IsDeletedTransaction, 0) = 0  GROUP BY BookingID ) AS FirstQty ON J.BookingID = FirstQty.BookingID AND J.PlanContQty = ( SELECT PlanContQty  FROM JobBookingContents  WHERE JobContentsID = FirstQty.MinID) WHERE ISNULL(J.IsDeletedTransaction, 0) = 0 ) QT On QT.BookingID =  JEJ.BookingID And QT.JobContentsID =  JEJ.JobContentsID Inner Join JobBookingProcess As JEO On JEO.ContentsID=JEJ.JobContentsID And JEJ.CompanyID=JEO.CompanyID Inner Join UserMaster As UM On JE.QuotedByUserID=UM.UserID And JE.CompanyID=UM.CompanyID And Isnull(UM.IsBlocked,0)=0 INNER JOIN LedgerMaster As LM On JE.LedgerID=LM.LedgerID And JE.CompanyID=LM.CompanyID And Isnull(LM.IsDeletedTransaction,0)=0 INNER JOIN LedgerMasterDetails As LMD On LM.LedgerID=LMD.LedgerID And LM.CompanyID=LMD.CompanyID And Isnull(LMD.IsDeletedTransaction,0)=0 And LMD.FieldName='MailingAddress' Left Join UserMaster As UMD On UMD.UserName ='ADMIN' And JE.CompanyID=UM.CompanyID And Isnull(UM.IsBlocked,0)=0 Where JE.CompanyID=" + GBLCompanyID + " And JE.BookingID IN (" + JobBKID + ") And Isnull(JE.IsDeletedTransaction,0)<>1 Order by JE.BookingNo Asc ";
                }

                DBConnection.FillDataTable(ref DTQuotes, str);

                if (DBType == "MYSQL")
                {
                    str = "SELECT PlanContQty, QuotedCost AS FinalCost FROM JobBookingCostings Where CompanyID=" + GBLCompanyID + " And BookingID =" + JobBKID + " And IFNULL(IsDeletedTransaction,0)<>1";
                }
                else
                {
                    str = "SELECT JBC.PlanContQty, JBC.QuotedCost AS FinalCost, JB.BookingNo FROM JobBookingCostings As JBC INNER JOIN JobBooking As JB ON JB.BookingID = JBC.BookingID And JB.CompanyID = JBC.CompanyID And ISNULL(JB.IsDeletedTransaction,0)=0" +
                          "WHERE JBC.CompanyID = '" + GBLCompanyID + "' AND JBC.BookingID IN (" + JobBKID + ") AND ISNULL(JBC.IsDeletedTransaction, 0) <> 1 Order by JB.BookingNo Asc ";
                }
                DBConnection.FillDataTable(ref DTQty, str);

                DTQuotes.TableName = "TblQuotes";
                DTQty.TableName = "TblQuoteQties";

                DataSet Dataset = new DataSet();
                Dataset.Merge(DTQuotes);
                Dataset.Merge(DTQty);

                data.Message = DBConnection.ConvertDataSetsToJsonString(Dataset);
                return JsonConvert.SerializeObject(data.Message);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [HttpPost]
        [Route("UpdateMailQuoteData")]
        public string UpdateMailQuoteData([FromBody] object ObjData)
        {
            try
            {
                SqlTransaction objtrans;
                SqlConnection con = DBConnection.OpenConnection();
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                objtrans = con.BeginTransaction();

                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                str = DBConnection.UpdateDatatableToDatabasewithtrans(ObjData, "JobBooking", "", 1, ref con, ref objtrans, " CompanyID=" + GBLCompanyID);
                if (str != "Success")
                {
                    objtrans.Rollback();
                    objtrans.Dispose();
                    return "Error:500," + str;
                }
                objtrans.Commit();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Success";
        }

        [HttpPost]
        [Route("UpdateMailQuoteData")]
        public string UpdateMailQuoteData([FromBody] object ObjData, string BookingID)
        {
            try
            {
                SqlTransaction objtrans;
                SqlConnection con = DBConnection.OpenConnection();
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                objtrans = con.BeginTransaction();

                string wherecndtn;
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                wherecndtn = "BookingID IN ('" + BookingID.Replace(",", "','") + "') AND CompanyID='" + GBLCompanyID + "'";
                str = DBConnection.UpdateDatatableToDatabasewithtrans(ObjData, "JobBooking", "", 0, ref con, ref objtrans, wherecndtn);

                if (str != "Success")
                {
                    objtrans.Rollback();
                    objtrans.Dispose();
                    return "Error:500," + str;
                }
                objtrans.Commit();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Success";
        }

        [HttpPost]
        [Route("DeleteQuoteAttachedFiles")]
        public string DeleteQuoteAttachedFiles([FromBody] object fileUpload)
        {
            string Key_Field;

            try
            {
                if (fileUpload != null && fileUpload.ToString() != "")
                {
                    string completePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileUpload.ToString().TrimStart('/'));
                    if (File.Exists(completePath))
                    {
                        File.Delete(completePath);
                    }
                }

                Key_Field = "Success";
            }
            catch (Exception ex)
            {
                Key_Field = "Fail " + ex.Message;
            }
            return Key_Field;
        }

        [HttpPost]
        [Route("SubmitMail")]
        public string SubmitMail([FromBody] MailMessage msg)
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                GBLUserID = Convert.ToString(HttpContext.Current.Session["UserID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                DataTable DtUser = new DataTable();
                if (DBType == "MYSQL")
                {
                    DBConnection.FillDataTable(ref DtUser, "SELECT Distinct IFNULL(Nullif(EmailID,''),smtpUserName) As smtpUserID,  IFNULL(smtpUserPassword,'') As smtpUserPassword,  IFNULL(smtpServer,'smtp.gmail.com') As smtpServer,  IFNULL(smtpServerPort,'587') As smtpServerPort,  IFNULL(smtpAuthenticate,'True') As smtpAuthenticate,  IFNULL(smtpUseSSL,'True') As smtpUseSSL FROM UserMaster Where IFNULL(IsBlocked,0)=0 And IFNULL(IsHidden,0)=0 And IFNULL(IsDeletedUser,0)=0 And CompanyID=" + GBLCompanyID + " And UserID=" + GBLUserID);
                }
                else
                {
                    DBConnection.FillDataTable(ref DtUser, "SELECT Distinct IsnuLL(Nullif(EmailID,''),smtpUserName) As smtpUserID,  Isnull(smtpUserPassword,'') As smtpUserPassword,  Isnull(smtpServer,'smtp.gmail.com') As smtpServer,  Isnull(smtpServerPort,'587') As smtpServerPort,  Isnull(smtpAuthenticate,'True') As smtpAuthenticate,  Isnull(smtpUseSSL,'True') As smtpUseSSL FROM UserMaster Where Isnull(IsBlocked,0)=0 And IsnuLL(IsHidden,0)=0 And ISNULL(IsDeletedUser,0)=0 And CompanyID=" + GBLCompanyID + " And UserID=" + GBLUserID);
                }

                if (DtUser.Rows.Count <= 0) return "Invalid user details";
                if (DtUser.Rows[0]["smtpUserID"].ToString() == "" || !DtUser.Rows[0]["smtpUserID"].ToString().Contains("@"))
                {
                    return "Invalid sender mail id, Please update mail id in user master";
                }

                SmtpClient smtp = new SmtpClient();
                smtp.Credentials = new NetworkCredential(DtUser.Rows[0]["smtpUserID"].ToString(), DtUser.Rows[0]["smtpUserPassword"].ToString());
                smtp.Port = Convert.ToInt32(DtUser.Rows[0]["smtpServerPort"].ToString());
                smtp.Host = DtUser.Rows[0]["smtpServer"].ToString();
                smtp.EnableSsl = Convert.ToBoolean(DtUser.Rows[0]["smtpUseSSL"].ToString());
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;

                smtp.Send(msg);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Email Send Successfully";
        }

        [HttpGet]
        [Route("GetParentItemSubGroupID/{UID}")]
        public string GetParentItemSubGroupID(string UID)
        {
            string GBLUndeSubGroupIDString = "";
            ShowParentItemSubGroupID(UID, ref GBLUndeSubGroupIDString);
            return GBLUndeSubGroupIDString;
        }

        private void ShowUnderUsersID(long UID, ref string GBLUnderUserIDString)
        {
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
            DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

            DataTable dt = new DataTable();
            if (DBType == "MYSQL")
            {
                DBConnection.FillDataTable(ref dt, "Select UserID From UserMaster Where UnderUserID = " + UID + " And CompanyID=" + GBLCompanyID + " AND IFNULL(IsBlocked,0)=0 AND IFNULL(IsDeletedUser,0)=0 AND UnderUserID<>UserID");
            }
            else
            {
                DBConnection.FillDataTable(ref dt, "Select UserID From UserMaster Where UnderUserID = " + UID + " And CompanyID=" + GBLCompanyID + " AND isnull(IsBlocked,0)=0 AND isnull(IsDeletedUser,0)=0 AND UnderUserID<>UserID");
            }

            if (dt.Rows.Count <= 0)
            {
                GBLUnderUserIDString = (GBLUnderUserIDString.Trim() == "" ? "" : GBLUnderUserIDString + ",") + Convert.ToString(UID);
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                ShowUnderUsersID(Convert.IsDBNull(dt.Rows[i][0]) ? 0 : Convert.ToInt64(dt.Rows[i][0]), ref GBLUnderUserIDString);
                GBLUnderUserIDString = (GBLUnderUserIDString.Trim() == "" ? "" : GBLUnderUserIDString + ",") + Convert.ToString(UID);
            }
        }

        private void ShowParentItemSubGroupID(string UID, ref string GBLUndeSubGroupIDString)
        {
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
            DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

            DataTable dt = new DataTable();
            DBConnection.FillDataTable(ref dt, "Select UnderSubGroupID From ItemSubGroupMaster Where ItemSubGroupID=" + UID + " AND CompanyID=" + GBLCompanyID + " AND Isnull(IsDeletedTransaction,0)=0 AND UnderSubGroupID<>ItemSubGroupID AND ItemSubGroupID<>1");

            if (dt.Rows.Count <= 0)
            {
                GBLUndeSubGroupIDString = (GBLUndeSubGroupIDString.Trim() == "" ? "" : GBLUndeSubGroupIDString + ",") + Convert.ToString(UID);
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                ShowParentItemSubGroupID(Convert.IsDBNull(dt.Rows[i][0]) ? "0" : dt.Rows[i][0].ToString(), ref GBLUndeSubGroupIDString);
                GBLUndeSubGroupIDString = (GBLUndeSubGroupIDString.Trim() == "" ? "" : GBLUndeSubGroupIDString + ",") + Convert.ToString(UID);
            }
        }

        [HttpGet]
        [Route("GetOtherQuote/{BKID}")]
        public string GetOtherQuote(int BKID)
        {
            string bookingNo;
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);

                str = "Select isnull(Max(BookingID),0) From JobBooking Where MaxBookingNo In(Select MaxBookingNo From JobBooking Where BookingID = " + BKID + ") and BookingID > " + BKID + " And CompanyId=" + GBLCompanyID + "  AND Isnull(IsDeletedTransaction,0)=0";
                dataTable.Clear();
                DBConnection.FillDataTable(ref dataTable, str);
                if (dataTable.Rows.Count > 0)
                {
                    bookingNo = dataTable.Rows[0][0].ToString();
                }
                else
                {
                    bookingNo = "0";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return bookingNo;
        }

        [HttpGet]
        [Route("GetMachineAllocatedItemList/{MachineID}")]
        public string GetMachineAllocatedItemList(int MachineID)
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                GBLProductionUnitID = Convert.ToString(HttpContext.Current.Session["ProductionUnitID"]);

                str = " Select Distinct MIA.MachineID,ISG.ItemSubGroupID,ISG.ItemSubGroupName,RM.Rate,MIA.IsDefault From MachineItemSubGroupAllocationMaster AS MIA  INNER JOIN ItemSubGroupMaster AS ISG ON ISG.ItemSubGroupID=MIA.ItemSubGroupID AND ISG.CompanyID = MIA.CompanyID AND Isnull(ISG.IsDeletedTransaction,0)=0 INNER JOIN ItemSubGroupRateMaster as RM ON RM.ItemSubGroupID = ISG.ItemSubGroupID  " +
                      " Where MIA.MachineID=" + MachineID + " And RM.ProductionUnitID = " + GBLProductionUnitID + " AND MIA.CompanyID=" + GBLCompanyID + " AND Isnull(MIA.IsDeletedTransaction,0)=0 ";
                dataTable.Clear();
                DBConnection.FillDataTable(ref dataTable, str);

                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return JsonConvert.SerializeObject(data.Message);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [HttpGet]
        [Route("GetMaterialCostEstimationSetting/{itemGroupID}/{itemSubGroupID}/{domainType}")]
        public string GetMaterialCostEstimationSetting(int itemGroupID, int itemSubGroupID, string domainType)
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                string strFilter = "";
                if (domainType.Trim() != "")
                {
                    strFilter = " AND DomainType='" + domainType.Trim() + "'";
                }

                str = "SELECT CostEstimationSettingID, ItemGroupID, ItemSubGroupID, TransID, FieldName, FieldDescription, ItemMasterFieldName, AppVariableName, FieldDisplayName, CalculationFormula, DefaultValue, DisplaySequenceNo, IsDisplayField,DomainType,Isnull(IsEditableField,0) AS IsEditableField,Isnull(MinimumValue,0) AS MinimumValue,Isnull(MaximumValue,0) AS MaximumValue " +
                    " FROM MaterialCostEstimationSetting Where CompanyID=" + GBLCompanyID + " AND Isnull(IsDeletedTransaction,0)=0 AND ItemGroupID=" + itemGroupID + " AND ItemSubGroupID=" + itemSubGroupID + " " + strFilter + " Order By DisplaySequenceNo";
                dataTable.Clear();
                DBConnection.FillDataTable(ref dataTable, str);

                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return JsonConvert.SerializeObject(data.Message);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [HttpGet]
        [Route("GetCategoryAllocatedContents/{categoryID}")]
        public string GetCategoryAllocatedContents(string categoryID)
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);
                GBLUserID = Convert.ToString(HttpContext.Current.Session["UserID"]);
                DataTable dtSegments = new DataTable();
                DataTable dtSegmentContents = new DataTable();

                if (Convert.ToInt32(categoryID) == 0)
                {
                    if (DBType == "MYSQL")
                    {
                        str = "Select  ContentID,Nullif(Replace(ContentName,'\"',''),'') As ContentName,  Nullif(Replace(ContentCaption,'\"',''),'') As ContentCaption, Nullif(Replace(ContentOpenHref,'\"',''),'') As ContentOpenHref,  Nullif(Replace(ContentClosedHref,'\"',''),'') As ContentClosedHref,  Nullif(Replace(ContentSizes,'\"',''),'') As ContentSizes,Nullif(Replace(ContentDomainType,'\"',''),'') As ContentDomainType From ContentMaster Where IFNULL(IsActive,0)=1 And CompanyId = " + GBLCompanyID + " Order By Nullif(Replace(ContentDomainType,'\"',''),''),Nullif(Replace(ContentName,'\"',''),'') ";
                    }
                    else
                    {
                        DBConnection.FillDataTable(ref dtSegments, "Select Distinct SM.SegmentID,SM.SegmentName From SegmentMaster AS SM INNER JOIN UserSegmentAllocation AS US ON US.SegmentID=SM.SegmentID Where Isnull(SM.IsDeletedTransaction,0)=0 AND Isnull(US.IsDeletedTransaction,0)=0 AND US.UserID=" + GBLUserID + " AND SM.CompanyID=" + GBLCompanyID + " Order By SM.SegmentName");
                        if (dtSegments.Rows.Count > 0)
                        {
                            str = "Select  Distinct C.ContentID,Nullif(Replace(C.ContentName,'\"',''),'') As ContentName,  Nullif(Replace(C.ContentCaption,'\"',''),'') As ContentCaption,Nullif(Replace(C.ContentOpenHref,'\"',''),'') As ContentOpenHref,  Nullif(Replace(C.ContentClosedHref,'\"',''),'') As ContentClosedHref,  Nullif(Replace(C.ContentSizes,'\"',''),'') As ContentSizes,Nullif(Replace(C.ContentDomainType,'\"',''),'') As ContentDomainType,Isnull(C.ContentSizeInputUnit,'MM') AS ContentSizeInputUnit,Isnull(C.ContentSizeInputDecimalValue,2) AS ContentSizeInputDecimalValue,CM.CategoryName,SM.SegmentName From ContentMaster AS C INNER JOIN CategoryContentAllocationMaster AS CCA ON CCA.ContentID=C.ContentID AND Isnull(CCA.IsDeletedTransaction,0)=0 INNER JOIN CategoryMaster AS CM ON CM.CategoryID=CCA.CategoryID  AND isnull(CM.IsDeletedTransaction,0)=0 INNER JOIN SegmentMaster AS SM ON SM.SegmentID=CM.SegmentID AND isnull(SM.IsDeletedTransaction,0)=0 Where Isnull(C.IsActive,0)=1 And C.CompanyId = " + GBLCompanyID + " AND SM.SegmentID IN(Select Distinct SegmentID From UserSegmentAllocation Where CompanyID=" + GBLCompanyID + " AND UserID=" + GBLUserID + " AND Isnull(IsDeletedTransaction,0)=0) Order By SegmentName, CategoryName,ContentName ";

                            DBConnection.FillDataTable(ref dtSegmentContents, str);
                            if (dtSegmentContents.Rows.Count == 0)
                            {
                                str = "Select  ContentID,Nullif(Replace(ContentName,'\"',''),'') As ContentName,  Nullif(Replace(ContentCaption,'\"',''),'') As ContentCaption, Nullif(Replace(ContentOpenHref,'\"',''),'') As ContentOpenHref,  Nullif(Replace(ContentClosedHref,'\"',''),'') As ContentClosedHref,  Nullif(Replace(ContentSizes,'\"',''),'') As ContentSizes,Nullif(Replace(ContentDomainType,'\"',''),'') As ContentDomainType,Isnull(ContentSizeInputUnit,'MM') AS ContentSizeInputUnit,Isnull(ContentSizeInputDecimalValue,2) AS ContentSizeInputDecimalValue From ContentMaster Where Isnull(IsActive,0)=1 And CompanyId = " + GBLCompanyID + " Order By Nullif(Replace(ContentDomainType,'\"',''),''),Nullif(Replace(ContentCaption,'\"',''),'') ";
                            }
                            else
                            {
                                str = "Select  Distinct C.ContentID,Nullif(Replace(C.ContentName,'\"',''),'') As ContentName,  Nullif(Replace(C.ContentCaption,'\"',''),'') As ContentCaption,Nullif(Replace(C.ContentOpenHref,'\"',''),'') As ContentOpenHref,  Nullif(Replace(C.ContentClosedHref,'\"',''),'') As ContentClosedHref,  Nullif(Replace(C.ContentSizes,'\"',''),'') As ContentSizes,Nullif(Replace(C.ContentDomainType,'\"',''),'') As ContentDomainType,Isnull(C.ContentSizeInputUnit,'MM') AS ContentSizeInputUnit,Isnull(C.ContentSizeInputDecimalValue,2) AS ContentSizeInputDecimalValue From ContentMaster AS C INNER JOIN CategoryContentAllocationMaster AS CCA ON CCA.ContentID=C.ContentID AND Isnull(CCA.IsDeletedTransaction,0)=0 INNER JOIN CategoryMaster AS CM ON CM.CategoryID=CCA.CategoryID  AND isnull(CM.IsDeletedTransaction,0)=0 INNER JOIN SegmentMaster AS SM ON SM.SegmentID=CM.SegmentID AND isnull(SM.IsDeletedTransaction,0)=0 Where Isnull(C.IsActive,0)=1 And C.CompanyId = " + GBLCompanyID + " AND SM.SegmentID IN(Select Distinct SegmentID From UserSegmentAllocation Where CompanyID=" + GBLCompanyID + " AND UserID=" + GBLUserID + " AND Isnull(IsDeletedTransaction,0)=0) Order By ContentCaption ";
                            }
                            dtSegmentContents.Dispose();
                        }
                        else
                        {
                            str = "Select  ContentID,Nullif(Replace(ContentName,'\"',''),'') As ContentName,  Nullif(Replace(ContentCaption,'\"',''),'') As ContentCaption, Nullif(Replace(ContentOpenHref,'\"',''),'') As ContentOpenHref,  Nullif(Replace(ContentClosedHref,'\"',''),'') As ContentClosedHref,  Nullif(Replace(ContentSizes,'\"',''),'') As ContentSizes,Nullif(Replace(ContentDomainType,'\"',''),'') As ContentDomainType,Isnull(ContentSizeInputUnit,'MM') AS ContentSizeInputUnit,Isnull(ContentSizeInputDecimalValue,2) AS ContentSizeInputDecimalValue From ContentMaster Where Isnull(IsActive,0)=1 And CompanyId = " + GBLCompanyID + " Order By Nullif(Replace(ContentDomainType,'\"',''),''),Nullif(Replace(ContentCaption,'\"',''),'') ";
                        }
                        dtSegments.Dispose();
                    }
                }
                else
                {
                    if (DBType == "MYSQL")
                    {
                        str = "Select  C.ContentID,Nullif(Replace(C.ContentName,'\"',''),'') As ContentName,  Nullif(Replace(C.ContentCaption,'\"',''),'') As ContentCaption, Nullif(Replace(C.ContentOpenHref,'\"',''),'') As ContentOpenHref,  Nullif(Replace(C.ContentClosedHref,'\"',''),'') As ContentClosedHref,  Nullif(Replace(C.ContentSizes,'\"',''),'') As ContentSizes,Nullif(Replace(C.ContentDomainType,'\"',''),'') As ContentDomainType,Isnull(C.ContentSizeInputUnit,'MM') AS ContentSizeInputUnit,Isnull(C.ContentSizeInputDecimalValue,2) AS ContentSizeInputDecimalValue From ContentMaster AS C INNER JOIN CategoryContentAllocationMaster as CA ON CA.ContentID=C.ContentID AND IFNULL(CA.IsDeletedTransaction,0)=0 Where IFNULL(C.IsActive,0)=1 And C.CompanyId = " + GBLCompanyID + " Order By Nullif(Replace(C.ContentDomainType,'\"',''),''),Nullif(Replace(C.ContentName,'\"',''),'') ";
                    }
                    else
                    {
                        str = "Select  C.ContentID,Nullif(Replace(C.ContentName,'\"',''),'') As ContentName,  Nullif(Replace(C.ContentCaption,'\"',''),'') As ContentCaption, Nullif(Replace(C.ContentOpenHref,'\"',''),'') As ContentOpenHref,  Nullif(Replace(C.ContentClosedHref,'\"',''),'') As ContentClosedHref,  Nullif(Replace(C.ContentSizes,'\"',''),'') As ContentSizes,Nullif(Replace(C.ContentDomainType,'\"',''),'') As ContentDomainType,Isnull(C.ContentSizeInputUnit,'MM') AS ContentSizeInputUnit,Isnull(C.ContentSizeInputDecimalValue,2) AS ContentSizeInputDecimalValue From ContentMaster AS C INNER JOIN CategoryContentAllocationMaster as CA ON CA.ContentID=C.ContentID AND Isnull(CA.IsDeletedTransaction,0)=0 Where Isnull(C.IsActive,0)=1 And C.CompanyId = " + GBLCompanyID + " AND CA.CategoryID=" + Convert.ToInt32(categoryID) + " Order By Nullif(Replace(C.ContentDomainType,'\"',''),''),Nullif(Replace(C.ContentName,'\"',''),'') ";
                    }
                }

                DBConnection.FillDataTable(ref dataTable, str);
                if (dataTable.Rows.Count == 0)
                {
                    if (DBType == "MYSQL")
                    {
                        str = "Select  ContentID,Nullif(Replace(ContentName,'\"',''),'') As ContentName,  Nullif(Replace(ContentCaption,'\"',''),'') As ContentCaption, Nullif(Replace(ContentOpenHref,'\"',''),'') As ContentOpenHref,  Nullif(Replace(ContentClosedHref,'\"',''),'') As ContentClosedHref,  Nullif(Replace(ContentSizes,'\"',''),'') As ContentSizes,Nullif(Replace(ContentDomainType,'\"',''),'') As ContentDomainType,Isnull(ContentSizeInputUnit,'MM') AS ContentSizeInputUnit,Isnull(ContentSizeInputDecimalValue,2) AS ContentSizeInputDecimalValue From ContentMaster Where IFNULL(IsActive,0)=1 And CompanyId = " + GBLCompanyID + " Order By Nullif(Replace(ContentDomainType,'\"',''),''),Nullif(Replace(ContentName,'\"',''),'') ";
                    }
                    else
                    {
                        str = "Select  ContentID,Nullif(Replace(ContentName,'\"',''),'') As ContentName,  Nullif(Replace(ContentCaption,'\"',''),'') As ContentCaption, Nullif(Replace(ContentOpenHref,'\"',''),'') As ContentOpenHref,  Nullif(Replace(ContentClosedHref,'\"',''),'') As ContentClosedHref,  Nullif(Replace(ContentSizes,'\"',''),'') As ContentSizes,Nullif(Replace(ContentDomainType,'\"',''),'') As ContentDomainType,Isnull(ContentSizeInputUnit,'MM') AS ContentSizeInputUnit,Isnull(ContentSizeInputDecimalValue,2) AS ContentSizeInputDecimalValue From ContentMaster Where Isnull(IsActive,0)=1 And CompanyId = " + GBLCompanyID + " Order By Nullif(Replace(ContentDomainType,'\"',''),''),Nullif(Replace(ContentName,'\"',''),'') ";
                    }
                    DBConnection.FillDataTable(ref dataTable, str);
                }
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return JsonConvert.SerializeObject(data.Message);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [HttpGet]
        [Route("GetMaterialGroupCostFormulaSetting/{ItemSubGroupID}/{PlantID}")]
        public string GetMaterialGroupCostFormulaSetting(string ItemSubGroupID, string PlantID)
        {
            //HttpContext.Response.Clear();
            //HttpContext.Response.ContentType = "application/json";
            str = DBConnection.Version;
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
            DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

            str = "Select Distinct ItemGroupID, S.ItemSubGroupID, TransID, FieldDescription, FieldName, ItemMasterFieldName, AppVariableName, FieldDisplayName, CalculationFormula, CASE WHEN FieldName = 'EstimationRate' THEN IMSR.Rate ELSE DefaultValue END as DefaultValue , DisplaySequenceNo, IsDisplayField,DomainType,Isnull(IsEditableField,0) AS IsEditableField,Isnull(MinimumValue,0) AS MinimumValue,Isnull(MaximumValue,0) AS MaximumValue From MaterialCostEstimationSetting as S INNER JOIN ItemSubGroupMaster AS IMS ON IMS.ItemSubGroupID = S.ItemSubGroupID INNER JOIN ItemSubGroupRateMaster AS IMSR ON IMSR.ItemSubGroupID = IMS.ItemSubGroupID INNER JOIN ProductionUnitMaster As PUM On PUM.ProductionUnitID = IMSR.ProductionUnitID  " +
                  " Where S.CompanyID = " + GBLCompanyID + " And Isnull(S.IsDeletedTransaction,0)=0 And S.ItemSubGroupID = " + ItemSubGroupID + " And PUM.ProductionUnitID = " + PlantID + " Order By ItemGroupID,ItemSubGroupID,TransID ";
            DBConnection.FillDataTable(ref dataTable, str);
            data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
            return JsonConvert.SerializeObject(data.Message);
        }

        [HttpGet]
        [Route("GetCategoryContentWiseDefaultProcesses/{CategoryID}/{ContentType}")]
        public string GetCategoryContentWiseDefaultProcesses(string CategoryID, string ContentType)
        {
            //HttpContext.Response.Clear();
            //HttpContext.Response.ContentType = "application/json";
            str = DBConnection.Version;
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
            DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);


            str = "Select CP.CategoryID,CP.ContentID,C.ContentName,PM.ProcessID, REPLACE(NULLIF (PM.ProcessName, ''), '\"', '') AS ProcessName, NULLIF (PM.PrePress, '') AS PrePress, NULLIF (PM.TypeofCharges, '') AS TypeofCharges, NULLIF (PM.SizeToBeConsidered, '') AS SizeToBeConsidered, ROUND(ISNULL(NULLIF (PM.Rate, ''), 0), 4) AS Rate, NULLIF (PM.MinimumCharges, '') AS MinimumCharges, NULLIF (PM.SetupCharges, '') AS SetupCharges, NULLIF (PM.IsDisplay, '') AS IsDisplay,NULLIF (PM.IsOnlineProcess, '') AS IsOnlineProcess, REPLACE(NULLIF (PM.ChargeApplyOnSheets, ''), '\"', '') AS ChargeApplyOnSheets, REPLACE(NULLIF (PM.DisplayProcessName, ''), '\"', '') AS DisplayProcessName, 0 AS Amount, '' AS RateFactor, '' AS AddRow,0 AS MakeReadyTime,0 AS MachineID,0 AS MachineSpeed,0 AS JobChangeOverTime,0 AS MakeReadyPerHourCost,0 AS MachinePerHourCost,0 AS ExecutionTime,0 AS TotalExecutionTime,0 As MakeReadyMachineCost,0 As ExecutionCost,0 AS MachineCost,0 AS MaterialCost,'' AS MachineName,Isnull(DM.DepartmentID,0) AS DepartmentID,Isnull(DM.SequenceNo,0) AS DepartmentSequenceNo,Isnull(PM.ProcessFlatWastageValue,0) AS ProcessFlatWastageValue,Isnull(PM.ProcessWastagePercentage,0) AS ProcessWastagePercentage,Isnull(PM.ProcessProductionType,'None') AS ProcessProductionType,Isnull(PM.PerHourCostingParameter,'') AS PerHourCostingParameter " +
                  " From CategoryWiseProcessAllocation AS CP INNER JOIN ContentMaster AS C ON CP.ContentID=C.ContentID AND Isnull(CP.IsDeletedTransaction,0)=0 INNER JOIN ProcessMaster AS PM ON PM.ProcessID=CP.ProcessID AND Isnull(PM.IsDeletedTransaction,0)=0 INNER JOIN DepartmentMaster AS DM ON DM.DepartmentID=PM.DepartmentID " +
                  " Where CP.CategoryID=" + CategoryID + " AND C.ContentName='" + ContentType + "' Order By DM.SequenceNo";

            DBConnection.FillDataTable(ref dataTable, str);
            data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
            return JsonConvert.SerializeObject(data.Message);
        }

        [HttpGet]
        [Route("GetSbSegmentUserWise")]
        public string GetSbSegmentUserWise()
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);
                GBLUserID = Convert.ToString(HttpContext.Current.Session["UserID"]);
                DataTable dtSegments = new DataTable();

                DBConnection.FillDataTable(ref dtSegments, "Select Distinct SM.SegmentID,SM.SegmentName From SegmentMaster AS SM INNER JOIN UserSegmentAllocation AS US ON US.SegmentID=SM.SegmentID Where Isnull(SM.IsDeletedTransaction,0)=0 AND Isnull(US.IsDeletedTransaction,0)=0 AND US.UserID=" + GBLUserID + " AND SM.CompanyID=" + GBLCompanyID + " Order By SM.SegmentName");
                if (dtSegments.Rows.Count > 0)
                {
                    dataTable = dtSegments;
                }
                else
                {
                    str = " Select Distinct SegmentID,Nullif(Replace(SegmentName,'\"',''),'') as SegmentName From SegmentMaster Where CompanyId = " + GBLCompanyID + " And Isnull(IsDeletedTransaction,0)<>1   Order By SegmentName ";
                    DBConnection.FillDataTable(ref dataTable, str);
                }
                dtSegments.Dispose();

                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return JsonConvert.SerializeObject(data.Message);
            }
            catch (Exception ex)
            {
                return "500";
            }
        }

        [HttpGet]
        [Route("GetCategoryWiseSegment/{categoryID}")]
        public string GetCategoryWiseSegment(string categoryID)
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);
                GBLUserID = Convert.ToString(HttpContext.Current.Session["UserID"]);

                DBConnection.FillDataTable(ref dataTable, "Select Distinct SegmentID,Isnull((Select SegmentName From SegmentMaster Where SegmentID=Isnull(CategoryMaster.SegmentID,0)),'') AS SegmentName From CategoryMaster Where CompanyID=" + GBLCompanyID + " AND CategoryID=" + categoryID);

                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return JsonConvert.SerializeObject(data.Message);
            }
            catch (Exception ex)
            {
                return "500";
            }
        }

        [HttpGet]
        [Route("GetCategorySegmentWise/{segmentID}")]
        public string GetCategorySegmentWise(string segmentID)
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);
                GBLUserID = Convert.ToString(HttpContext.Current.Session["UserID"]);
                str = "select distinct CM.CategoryID,CM.CategoryName  from CategoryMaster as CM inner join SegmentMaster as SM  on CM.SegmentID = SM.SegmentID  where CM.SegmentID = " + segmentID + " and CM.CompanyId = " + GBLCompanyID + " and CM.UserID = " + GBLUserID + "  And Isnull(CM.IsDeletedTransaction,0)<>1 and Isnull(SM.IsDeletedTransaction,0)<>1";
                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return JsonConvert.SerializeObject(data.Message);
            }
            catch (Exception ex)
            {
                return "500";
            }
        }

        [HttpGet]
        [Route("GetCategoryUserSegmentWise/{segmentID}")]
        public string GetCategoryUserSegmentWise(string segmentID)
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);
                GBLUserID = Convert.ToString(HttpContext.Current.Session["UserID"]);
                str = "select distinct CM.CategoryID,CM.CategoryName  from CategoryMaster as CM inner join UserSegmentAllocation as SM  on CM.SegmentID = SM.SegmentID  where CM.SegmentID = " + segmentID + " and CM.CompanyId = " + GBLCompanyID + " and SM.UserID = " + GBLUserID + "  And Isnull(CM.IsDeletedTransaction,0)<>1 and Isnull(SM.IsDeletedTransaction,0)<>1";
                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return JsonConvert.SerializeObject(data.Message);
            }
            catch (Exception ex)
            {
                return "500";
            }
        }

        [HttpGet]
        [Route("GetPendingEnquiry/{Con}")]
        public string GetPendingEnquiry(string Con)
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);
                GBLUserID = Convert.ToString(HttpContext.Current.Session["UserID"]);

                str = "Select JE.EnquiryID,JE.EnquiryNo,replace(convert(nvarchar(30), JE.EnquiryDate,106),'','-') as EnquiryDate,RefEnquiryNo,Isnull(JobName,'') JobName,IsNull(EstimationUnit,'') EstimationUnit,IsNUll(Quantity,'')Quantity,ISNULL(ProductCode,'')ProductCode,ProductRefCode,JE.LedgerID,LM.LedgerName as ClientName,JE.ConsigneeLedgerID,JE.SalesEmployeeID,ProductHSNID,SLM.LedgerName as SalesEmployeeName, CLM.LedgerName as ConsigneeName ,CM.CategoryID,Cm.CategoryName  from JobEnquiry as JE  Inner Join LedgerMaster as LM On LM.LedgerId = JE.LedgerID  Inner Join LedgerMaster as CLM on CLM.LedgerID = JE.ConsigneeLedgerId  Inner Join LedgerMaster as SLM on SLM.LedgerID = JE.SalesEmployeeID Inner join CategoryMaster as CM On CM.CategoryID = JE.CategoryID  Where ISnull(JE.IsDeletedTransaction,0) <> 1 And JE.CompanyID = " + GBLCompanyID + " " + Con;

                DBConnection.FillDataTable(ref dataTable, str);

                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return JsonConvert.SerializeObject(data.Message);
            }
            catch (Exception ex)
            {
                return "500";
            }
        }

        [HttpGet]
        [Route("GetSizes/{Quality}/{GSM}/{Mill}/{Finish}")]
        public string GetSizes(string Quality, string GSM, string Mill, string Finish)
        {
            //HttpContext.Response.Clear();
            //HttpContext.Response.ContentType = "application/json";
            str = DBConnection.Version;
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
            DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

            if (DBType == "MYSQL")
            {
                if (Quality == "" && GSM == "" && Mill == "" && Finish == "")
                {
                    str = "Select Distinct SizeW,SizeL From ItemMaster Where ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where CompanyID=" + GBLCompanyID + " And ItemGroupNameID IN(-1,-2,-14,-15)) And IFNULL(IsDeletedTransaction,0)<>1 And IFNULL(Finish,'')<>'' And CompanyID=" + GBLCompanyID + " Order By Finish";
                }
                else
                {
                    str = "Select Distinct SizeW,SizeL From ItemMaster Where ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where CompanyID=" + GBLCompanyID + " And ItemGroupNameID IN(-1,-2,-14,-15)) And ItemID IN (Select ItemID From ItemMaster Where GSM=" + GSM + " And CompanyID=" + GBLCompanyID + " And IsDeletedTransaction=0) And ItemID IN (Select ItemID From ItemMaster Where Quality='" + Quality + "' And CompanyID=" + GBLCompanyID + " And IsDeletedTransaction=0) And ItemID IN (Select ItemID From ItemMaster Where Manufecturer='" + Mill + "' And CompanyID=" + GBLCompanyID + " And IsDeletedTransaction=0) And ItemID IN (Select ItemID From ItemMaster Where Finish='" + Finish + "' And CompanyID='" + GBLCompanyID + "' And IsDeletedTransaction=0) And IFNULL(IsDeletedTransaction,0)<>1 Order By SizeW";
                }
            }
            else
            {
                if (Quality == "" && GSM == "" && Mill == "" && Finish == "")
                {
                    str = "Select Distinct SizeW,SizeL From ItemMaster Where ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where CompanyID=" + GBLCompanyID + " And ItemGroupNameID IN(-1,-2,-14,-15,-16)) And Isnull(IsDeletedTransaction,0)<>1 And ISNULL(Finish,'')<>'' And CompanyID=" + GBLCompanyID + " Order By Finish";
                }
                else
                {
                    str = "Select Distinct SizeW,SizeL From ItemMaster Where ItemGroupID In (Select ItemGroupID From ItemGroupMaster Where CompanyID=" + GBLCompanyID + " And ItemGroupNameID IN(-1,-2,-14,-15,-16)) And ItemID IN (Select ItemID From ItemMaster Where GSM=" + GSM + " And CompanyID=" + GBLCompanyID + " And IsDeletedTransaction=0) And ItemID IN (Select ItemID From ItemMaster Where Quality='" + Quality + "' And CompanyID=" + GBLCompanyID + " And IsDeletedTransaction=0) And ItemID IN (Select ItemID From ItemMaster Where Manufecturer='" + Mill + "' And CompanyID=" + GBLCompanyID + " And IsDeletedTransaction=0)  And ItemID IN (Select ItemID From ItemMaster Where Finish='" + Finish + "' And CompanyID='" + GBLCompanyID + "' And IsDeletedTransaction=0) And Isnull(IsDeletedTransaction,0)<>1 Order By SizeW";
                }
            }

            DBConnection.FillDataTable(ref dataTable, str);
            data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
            return JsonConvert.SerializeObject(data.Message);
        }

        [HttpGet]
        [Route("ReloadBookPlan")]
        public string ReloadBookPlan()
        {
            //HttpContext.Response.Clear();
            //HttpContext.Response.ContentType = "application/json";
            str = DBConnection.Version;
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
            DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

            str = "Select BM.BookTemplateID,BM.TemplateName,BM.BookCategory,BM.BookTrimmingright,BM.BookTrimmingleft,BM.BookTrimmingbottom,BM.BookTrimmingtop,BM.BookExtension,BM.BookCoverTurnIn,BM.BookLoops,BM.BookSpine,BM.BookHinge,BM.BookLength,BM.BookHeight,BM.BookQuantity from BookTemplateMain  AS BM  where ISNULL(BM.IsDeletedTransaction,0) <> 1";

            DBConnection.FillDataTable(ref dataTable, str);
            data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
            return JsonConvert.SerializeObject(data.Message);
        }

        [HttpGet]
        [Route("ReloadPlanGrid/{BookTemplateID}")]
        public string ReloadPlanGrid(string BookTemplateID)
        {
            //HttpContext.Response.Clear();
            //HttpContext.Response.ContentType = "application/json";
            str = DBConnection.Version;
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
            DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);
            str = "select BookTemplateDetailID	,ContentName,BookTemplateID,Quality,ContentCaption,  CAST(CAST(GSM AS DECIMAL(10, 2)) AS INT) AS GSM,Mill,Finish,Pages,FrontColor,BackColor,PlateType,Spine,JobH,JobL,DustTrimL,DustTrimR,DustTrimB,DustTrimT  from BookTemplateDetails   where ISNULL(IsDeletedTransaction,0) <> 1 and  BookTemplateID =  ' " + BookTemplateID + " '";
            DBConnection.FillDataTable(ref dataTable, str);
            data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
            return JsonConvert.SerializeObject(data.Message);
        }

        [HttpGet]
        [Route("GetPendingEnquiryData")]
        public string GetPendingEnquiryData()
        {
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
            GBLUserID = Convert.ToString(HttpContext.Current.Session["UserID"]);

            str = " Select Distinct EQ.SalesType,ISNULL(LM.IsLead,0) as IsLead,CM.CategoryID,CM.CategoryName,EQ.ProductCode,CN.ContentDomainType as PlanContDomainType,isnull(EQ.ExpectCompletion,'') as ExpectCompletion,isnull(EQ.Quantity,'') as Quantity,EQ.EstimationUnit,EQ.Remark,LM.LedgerID, LM.LedgerName AS ClientName,EQ.SalesEmployeeID as EmployeeID,EM.LedgerName as SalesPersonName, EQ.JobName,EQ.EnquiryID,replace(convert(nvarchar(30),EQ.EnquiryDate,106),'','-')  AS EnquiryDate,LM.LedgerID,EQ.EnquiryID,EQ.EnquiryNo, EQ.CompanyID, replace(convert(nvarchar(13),EQ.EnquiryDate,106),'','-') as EnquiryDate, ISNULL(EQ.IsDetailed,0) as IsDetailed,ISNULL(EQ.AnnualQuantity,0) as AnnualQuantity,EQ.PlantID ,ISNULL(EQ.Source,'') as Source,UM.UserName as AssignToUserName " +
                  " From JobEnquiry as EQ INNER JOIN LedgerMaster AS LM ON LM.LedgerID = EQ.LEDGERID Left JOIN LedgerMaster AS EM ON EM.LedgerID = EQ.SalesEmployeeID LEFT JOIN JobEnquiryContents as JEC on JEC.EnquiryID = EQ.EnquiryID LEFT JOIN ContentMaster as CN on CN.ContentName = JEC.PlanContentType LEFT JOIN JobEnquiryProcess as  EP on EP.EnquiryID = EQ.EnquiryID LEFT JOIN CategoryMaster as CM on CM.CategoryID = EQ.CategoryID LEFT JOIN UserMaster as UM On UM.UserID = EQ.LockedByUserId where EQ.CompanyID= '" + GBLCompanyID + "'  And Isnull(EQ.IsDeletedTransaction,0 ) <> 1 AND EQ.EnquiryID NOT IN (Select JB.EnquiryID  From JobBooking AS JB Where Isnull(JB.IsDeletedTransaction,0)=0) order by EQ.EnquiryID desc";

            DBConnection.FillDataTable(ref dataTable, str);
            data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);

            return JsonConvert.SerializeObject(data.Message);
        }

        [HttpGet]
        [Route("GetEnquiryContentData/{EnquiryID}")]
        public string GetEnquiryContentData(string EnquiryID)
        {
            DataTable DTContent = new DataTable();
            DataTable DTProcess = new DataTable();
            DataTable DTLayers = new DataTable();
            DataTable DTAttachments = new DataTable();
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
            GBLUserID = Convert.ToString(HttpContext.Current.Session["UserID"]);

            str = "Select JE.SalesType,JE.PlantID,JE.FileName,JD.EnquiryContentsID,JD.ContentSizeValues,JD.EnquiryID,JD.PlanContentType ,JD.Size as PaperSize,nullif(JD.OtherDetails, '') as OtherDetails,JD.PlanContName,CM.ContentDomainType as PlanContDomainType,JE.SupplyLocation,JE.PaymentTerms from JobEnquiry AS JE INNER JOIN JobEnquiryContents as JD ON JD.EnquiryID = JE.EnquiryID   Inner Join ContentMaster AS CM on CM.ContentName = JD.PlanContentType   where JD.EnquiryID = '" + EnquiryID + "' and isnull(JD.IsDeletedTransaction,0 ) <> 1 ";

            DBConnection.FillDataTable(ref DTContent, str);

            str = "Select Distinct CP.EnquiryContentsID,CP.PlanContName,C.PlanContentType as ContentName,PM.ProcessID, REPLACE(NULLIF (PM.ProcessName, ''), '\"', '') AS ProcessName,NULLIF (PM.PrePress, '') AS PrePress,NULLIF (PM.TypeofCharges, '') AS TypeofCharges, NULLIF (PM.SizeToBeConsidered, '') AS SizeToBeConsidered,ROUND(ISNULL(NULLIF (PM.Rate, ''), 0), 4) AS Rate,NULLIF (PM.MinimumCharges, '') AS MinimumCharges, NULLIF (PM.SetupCharges, '') AS SetupCharges,NULLIF (PM.IsDisplay, '') AS IsDisplay,NULLIF (PM.IsOnlineProcess, '') AS IsOnlineProcess, REPLACE(NULLIF (PM.ChargeApplyOnSheets, ''), '\"', '') AS ChargeApplyOnSheets,REPLACE(NULLIF (PM.DisplayProcessName, ''), '\"', '') AS DisplayProcessName, 0 AS Amount, '' AS RateFactor, '' AS AddRow,0 AS MakeReadyTime,0 AS MachineID,0 AS MachineSpeed,0 AS JobChangeOverTime,0 AS MakeReadyPerHourCost,0 AS MachinePerHourCost,DM.SequenceNo AS DepartmentSequenceNo,Isnull(PM.ProcessFlatWastageValue,0) AS ProcessFlatWastageValue,Isnull(PM.ProcessProductionType,'None') AS ProcessProductionType,Isnull(PM.ProcessWastagePercentage,0) AS ProcessWastagePercentage,Isnull(NullIf(PM.ProcessModuleType,''),'Universal') AS ProcessModuleType,Isnull(PM.PerHourCostingParameter,'') AS PerHourCostingParameter " +
                  " From JobEnquiryProcess AS CP INNER JOIN JobEnquiryContents AS C ON CP.EnquiryContentsID=C.EnquiryContentsID and CP.PlanContName = C.PlanContName AND Isnull(CP.IsDeletedTransaction,0)=0 INNER JOIN ProcessMaster AS PM ON PM.ProcessID=CP.ProcessID AND Isnull(PM.IsDeletedTransaction,0)=0 INNER JOIN DepartmentMaster AS DM ON DM.DepartmentID=PM.DepartmentID Where CP.EnquiryID= '" + EnquiryID + "'  Order By DM.SequenceNo";
            DBConnection.FillDataTable(ref DTProcess, str);

            str = "Select Distinct JL.LayerID,JC.EnquiryContentsID,JL.PlanContName,JC.PlanContentType as ContentName,JL.PlanContName,JL.Quality,JL.Thickness,JL.GSM,JL.Mill From JobEnquiry AS JE INNER JOIN JobEnquiryContents AS JC ON JC.EnquiryID=JE.EnquiryID INNER JOIN JobEnquiryLayerDetail AS JL ON JL.EnquiryID=JC.EnquiryID AND JL.PlanContName =JC.PlanContName Where JE.EnquiryID=" + EnquiryID + " Order By JL.LayerID";
            DBConnection.FillDataTable(ref DTLayers, str);

            str = "select JA.AttachementID,JA.BookingID,JA.FilePath as AttachedFileName ,JA.FilePath as  AttachedFileUrl from JobBookingAttachments as JA Inner join JobEnquiry as JE on JE.EnquiryID =JA.EnquiryID Where JE.EnquiryID = " + EnquiryID;
            DBConnection.FillDataTable(ref DTAttachments, str);

            DTContent.TableName = "TblBookingContents";
            DTProcess.TableName = "TblBookingProcess";
            DTLayers.TableName = "TblBookingLayers";
            DTAttachments.TableName = "FileAttachment";
            DataSet Dataset = new DataSet();
            Dataset.Merge(DTContent);
            Dataset.Merge(DTProcess);
            Dataset.Merge(DTLayers);
            Dataset.Merge(DTAttachments);
            data.Message = DBConnection.ConvertDataSetsToJsonString(Dataset);

            return JsonConvert.SerializeObject(data.Message);
        }

        [HttpGet]
        [Route("GetCategoryContentWiseEnquiryProcesses/{EnquiryID}/{ContentName}")]
        public string GetCategoryContentWiseEnquiryProcesses(string EnquiryID, string ContentName)
        {
            //HttpContext.Response.Clear();
            //HttpContext.Response.ContentType = "application/json";
            str = DBConnection.Version;
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
            DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

            str = "Select Distinct CP.EnquiryContentsID,CP.PlanContName,JE.CategoryID,C.PlanContentType as ContentName,PM.ProcessID, REPLACE(NULLIF (PM.ProcessName, ''), '\"', '') AS ProcessName,NULLIF (PM.PrePress, '') AS PrePress,NULLIF (PM.TypeofCharges, '') AS TypeofCharges, NULLIF (PM.SizeToBeConsidered, '') AS SizeToBeConsidered,ROUND(ISNULL(NULLIF (PM.Rate, ''), 0), 4) AS Rate,NULLIF (PM.MinimumCharges, '') AS MinimumCharges, NULLIF (PM.SetupCharges, '') AS SetupCharges,NULLIF (PM.IsDisplay, '') AS IsDisplay,NULLIF (PM.IsOnlineProcess, '') AS IsOnlineProcess, REPLACE(NULLIF (PM.ChargeApplyOnSheets, ''), '\"', '') AS ChargeApplyOnSheets,REPLACE(NULLIF (PM.DisplayProcessName, ''), '\"', '') AS DisplayProcessName, 0 AS Amount, '' AS RateFactor, '' AS AddRow,0 AS MakeReadyTime,0 AS MachineID,0 AS MachineSpeed,0 AS JobChangeOverTime,0 AS MakeReadyPerHourCost,0 AS MachinePerHourCost,0 AS ExecutionTime,0 AS TotalExecutionTime,0 As MakeReadyMachineCost,0 As ExecutionCost,0 AS MachineCost,0 AS MaterialCost,DM.SequenceNo,Isnull(PM.ProcessProductionType,'None') AS ProcessProductionType,Isnull(PM.PerHourCostingParameter,'') AS PerHourCostingParameter " +
                  " From JobEnquiryProcess AS CP INNER JOIN JobEnquiryContents AS C ON CP.EnquiryContentsID=C.EnquiryContentsID And CP.PlanContName = C.PlanContName And Isnull(CP.IsDeletedTransaction,0)=0 Inner Join JobEnquiry as JE on JE.EnquiryID = C.EnquiryID INNER JOIN ProcessMaster AS PM ON PM.ProcessID=CP.ProcessID And Isnull(PM.IsDeletedTransaction,0)=0 INNER JOIN DepartmentMaster AS DM ON DM.DepartmentID=PM.DepartmentID Where CP.EnquiryID= '" + EnquiryID + "' AND C.PlanContName='" + ContentName + "' Order By DM.SequenceNo";

            DBConnection.FillDataTable(ref dataTable, str);
            data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
            return JsonConvert.SerializeObject(data.Message);
        }

        [HttpPost]
        [Route("SaveBookTemplate")]
        public string SaveBookTemplate([FromBody] dynamic request)
        {
            DataTable dt = new DataTable();
            string KeyField, BookTemplateID;
            string AddColName, AddColValue, TableName;

            GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
            GBLUserID = Convert.ToString(HttpContext.Current.Session["UserID"]);
            string GBLUserName = DBConnection.GblUserName;
            DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

            try
            {
                var JobMain = request.JobMain;
                var JobDetail = request.JobDetail;

                str = "Select * from BookTemplateMain where TemplateName = '" + JobMain[0].TemplateName + "'";
                DBConnection.FillDataTable(ref dt, str);
                if (dt.Rows.Count > 0)
                {
                    return "Duplicate data found..";
                }
                SqlTransaction objtrans;
                SqlConnection con = DBConnection.OpenConnection();

                objtrans = con.BeginTransaction();
                TableName = "BookTemplateMain";
                AddColName = "CreatedDate,CompanyID,CreatedBy";
                AddColValue = "Getdate(),'" + GBLCompanyID + "','" + GBLUserID + "'";
                BookTemplateID = DBConnection.InsertDatatableToDatabase(JobMain, TableName, AddColName, AddColValue, ref con, ref objtrans);
                if (!IsNumeric(BookTemplateID))
                {
                    objtrans.Rollback();
                    objtrans.Dispose();
                    return "Error:Main:- " + BookTemplateID;
                }
                TableName = "BookTemplateDetails";
                AddColName = "CreatedDate,CompanyID,CreatedBy,BookTemplateID";
                AddColValue = "Getdate(),'" + GBLCompanyID + "','" + GBLUserID + "','" + BookTemplateID + "'";
                KeyField = DBConnection.InsertDatatableToDatabase(JobDetail, TableName, AddColName, AddColValue, ref con, ref objtrans);
                if (!IsNumeric(KeyField))
                {
                    objtrans.Rollback();
                    objtrans.Dispose();
                    return "Error:Main:- " + KeyField;
                }
                KeyField = "Success";

            }
            catch (Exception ex)
            {
                KeyField = "Error: " + ex.Message;
            }
            return KeyField;
        }

        private bool IsNumeric(string value)
        {
            return double.TryParse(value, out _);
        }

        [HttpGet]
        [Route("GetPrintingMachineWiseLoads")]
        public string GetPrintingMachineWiseLoads()
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                str = "Select MachineID, MachineName,Case When MachineLoad > 0 Then CONCAT(FLOOR(MachineLoad / 60), ' hour(s) and ',CAST(MachineLoad AS INT) % 60, ' min(s)') Else '0' End As MachineLoadInHr From (Select MM.MachineID, MM.MachineName, (Isnull((Select Sum(Isnull(TotalTimeToBeTaken,0)) as TimeMinutes From JobScheduleReleaseMachineWise Where MachineID = MM.MachineID And CompanyID = MM.CompanyID  GROUP BY MachineID),0) + /*Isnull((Select Sum(Isnull(TotalTimeToBeTaken,0)) as TimeMinutes From JobScheduleRelease  Where MachineID = MM.MachineID And CompanyID = MM.CompanyID  GROUP BY MachineID),0)*/ Isnull(JSR.TimeMinutes,0)) As MachineLoad " +
                      " From MachineMaster as MM LEFT JOIN (Select JSR.MachineID,ROUND(Sum(Isnull(JSR.TotalTimeToBeTaken,0)),0) as TimeMinutes From JobScheduleRelease AS JSR INNER JOIN JobBookingJobCardContents AS JBJC ON JBJC.JobBookingJobCardContentsID=Isnull(JSR.JobBookingJobCardContentsID,0) INNER JOIN JobBookingJobCard AS JBJ ON JBJ.JobBookingID=Isnull(JBJC.JobBookingID,0) INNER JOIN JobBookingJobCardProcess AS JBP ON Isnull(JBP.JobBookingJobCardContentsID,0)=Isnull(JSR.JobBookingJobCardContentsID,0) AND Isnull(JBP.ProcessID,0)=Isnull(JSR.ProcessID,0) INNER JOIN ProcessMaster AS PM ON PM.ProcessID=Isnull(JSR.ProcessID,0) AND Isnull(PM.IsOnlineProcess,0)=0 Where JSR.CompanyID=" + GBLCompanyID + " AND Isnull(JSR.IsDeletedTransaction,0)=0 AND Isnull(JBJ.IsDeletedTransaction,0)=0 AND Isnull(JBJC.IsDeletedTransaction,0)=0 AND Isnull(JBJ.IsClose,0)=0 AND Isnull(JBJ.IsCancel,0)=0 AND (Isnull(JBP.Status,'')<>'Complete' AND Isnull(JBP.Status,'')<>'Part Complete') Group BY JSR.MachineID) AS JSR ON  JSR.MachineID=MM.MachineID " +
                      " Where MM.CompanyID =" + GBLCompanyID + " AND Isnull(MM.IsDeletedTransaction,0)=0 AND MM.DepartmentID =100 AND Isnull(MM.MachineType,'') NOT IN('Reel to Sheet Cutting','Corrugation')) As A  Order By MachineName";

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return JsonConvert.SerializeObject(data.Message);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [HttpGet]
        [Route("GetAllOperation")]
        public string GetAllOperation()
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                str = "Select PP.ProcessID,PP.ProcessName From JobBookingJobCardProcess AS JBP INNER JOIN ProcessMaster AS PP ON PP.ProcessID=JBP.ProcessID Where JBP.CompanyID=" + GBLCompanyID + " AND Isnull(JBP.IsDeletedTransaction,0)<>0 Group BY PP.ProcessID,PP.ProcessName";

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return JsonConvert.SerializeObject(data.Message);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [HttpGet]
        [Route("SalesLedger")]
        public string SalesLedger()
        {
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
            GBLUserID = Convert.ToString(HttpContext.Current.Session["UserID"]);
            str = "Select Distinct LM.LedgerID AS EmployeeID,Nullif(Replace(LM.LedgerName,'\"',''),'') as EmployeeName From LedgerMaster AS LM INNER JOIN LedgerGroupMaster AS LG ON LG.LedgerGroupID=LM.LedgerGroupID AND LG.CompanyID=LM.CompanyID Where LG.LedgerGroupNameID=27 AND LM.DepartmentID=-50 AND LM.CompanyID = " + GBLCompanyID + " And Isnull(LM.IsDeletedTransaction,0)<>1  AND LM.LedgerID IN(Select OperatorID from UserOperatorAllocation Where UserID =" + GBLUserID + " AND CompanyID=" + GBLCompanyID + " AND Isnull(IsDeletedTransaction,0)=0) Order By EmployeeName ";
            DBConnection.FillDataTable(ref dataTable, str);
            data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);

            return JsonConvert.SerializeObject(data.Message);
        }

        [HttpGet]
        [Route("GetCategoryWiseDefaultConfiguration/{CategoryID}")]
        public string GetCategoryWiseDefaultConfiguration(string CategoryID)
        {
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
            GBLUserID = Convert.ToString(HttpContext.Current.Session["UserID"]);
            str = "Select CM.CategoryID,CM.MinimumAroundGap,CM.MaximumAroundGap,CM.DefaultAroundGap,CM.MinimumAcrossGap,CM.MaximumAcrossGap,CM.DefaultAcrossGap,CM.MinimumPlateBearer, CM.MaximumPlateBearer,CM.DefaultPlateBearer,CM.MinimumSideStrip,CM.MaximumSideStrip,CM.DefaultSideStrip,CM.DefaultPrintingMarginLeft,CM.DefaultPrintingMarginLeft,CM.DefaultPrintingMarginRight,CM.DefaultPrintingMarginTop,CM.DefaultPrintingMarginBottom, CM.DefaultStrippingMarginLeft,CM.DefaultStrippingMarginRight,CM.DefaultStrippingMarginTop,CM.DefaultStrippingMarginBottom,CM.DefaultJobTrimmingTop,CM.DefaultJobTrimmingBottom,CM.DefaultJobTrimmingLeft,CM.DefaultJobTrimmingRight,SM.DefaultFactor,Isnull(SM.DefaultPackingCostPercentage,0) AS DefaultPackingCostPercentage,CM.Layer " +
                  " From CategoryMaster AS CM LEFT JOIN SegmentMaster AS SM ON SM.SegmentID=CM.SegmentID " +
                  " Where CM.CategoryID=" + CategoryID + " AND CM.CompanyID=" + GBLCompanyID;
            DBConnection.FillDataTable(ref dataTable, str);
            data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);

            return JsonConvert.SerializeObject(data.Message);
        }

        [HttpGet]
        [Route("GetItemGroupList")]
        public string GetItemGroupList()
        {
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
            GBLUserID = Convert.ToString(HttpContext.Current.Session["UserID"]);
            DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

            if (DBType == "MYSQL")
            {
                str = "Select ItemGroupID,ItemGroupNameID,ItemGroupName from ItemGroupMaster where CompanyID = " + GBLCompanyID + " And ISNULL(IsDeleted,0)=0";
            }
            else
            {
                str = "Select ItemGroupID,ItemGroupNameID,ItemGroupName from ItemGroupMaster Where CompanyID = " + GBLCompanyID + " And ISNULL(IsDeleted,0)=0 AND ItemGroupNameID NOT IN(-14,-1,-2,-15,-16) Order By ItemGroupName";
            }

            DBConnection.FillDataTable(ref dataTable, str);
            data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
            return JsonConvert.SerializeObject(data.Message);
        }

        [HttpGet]
        [Route("GetLedgerEmail/{LedgerID}")]
        public string GetLedgerEmail(string LedgerID)
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);
                str = "Select nullif(Email,'') as Email  From LedgerMaster   where  Isnull(IsDeletedTransaction,0)<>1 And CompanyId = " + GBLCompanyID + " and LedgerID = '" + LedgerID + "'";
                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return JsonConvert.SerializeObject(data.Message);
            }
            catch (Exception ex)
            {
                return "500";
            }
        }

        [HttpGet]
        [Route("CheckIsvalidQuotationPrintRequest/{BKID}")]
        public string CheckIsvalidQuotationPrintRequest(string BKID)
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);
                str = "Select BookingID From JobBooking Where Isnull(IsInternalApproved,0)=0 AND BookingID IN(" + BKID + ") AND Isnull(IsDeletedTransaction,0)=0";
                DBConnection.FillDataTable(ref dataTable, str);

                if (dataTable.Rows.Count > 0)
                {
                    return "Invalid";
                }
                else
                {
                    return "Valid";
                }
            }
            catch (Exception ex)
            {
                return "Invalid";
            }
        }

        [HttpGet]
        [Route("GetCalculationVal")]
        public string GetCalculationVal()
        {
            //HttpContext.Response.Clear();
            //HttpContext.Response.ContentType = "application/json";
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
            str = "SELECT ISNULL(WtCalculateOnEstimation,'') AS WtCalculateOnEstimation FROM CompanyMaster WHERE CompanyID=" + GBLCompanyID;
            DBConnection.FillDataTable(ref dataTable, str);
            data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);

            return JsonConvert.SerializeObject(data.Message);
        }

        //[HttpPost]
        //[Route("UploadLayoutImageFile")]
        //public string UploadLayoutImageFile()
        //{
        //    try
        //    {
        //        var uploadImageSolution = HttpContext.Request.Form.Files["UserAttchedFiles"];

        //        if (uploadImageSolution != null && uploadImageSolution.Length > 0)
        //        {
        //            // Valid extensions
        //            string[] validExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".psd" };
        //            string fileExtension = Path.GetExtension(uploadImageSolution.FileName).ToLower();

        //            if (validExtensions.Contains(fileExtension))
        //            {
        //                if (uploadImageSolution.Length < 8100000)
        //                {
        //                    string filename = Path.GetFileName(uploadImageSolution.FileName);
        //                    string uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "LayoutImage");
        //                    string fileSavePath = Path.Combine(uploadDirectory, filename);

        //                    // Create directory if it doesn't exist
        //                    Directory.CreateDirectory(uploadDirectory);

        //                    if (File.Exists(fileSavePath))
        //                    {
        //                        SafeDeleteFile(fileSavePath);
        //                    }

        //                    using (var stream = new FileStream(fileSavePath, FileMode.Create))
        //                    {
        //                        uploadImageSolution.CopyTo(stream);
        //                    }

        //                    return "Image uploaded and replaced successfully.";
        //                }
        //                else
        //                {
        //                    return "File size exceeds the limit (8MB).";
        //                }
        //            }
        //            else
        //            {
        //                return "Invalid file format. Supported formats: jpg, jpeg, png, gif, psd";
        //            }
        //        }
        //        else
        //        {
        //            return "No file selected for upload.";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return "Error: " + ex.Message;
        //    }
        //}

        private void SafeDeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    // Unlock file if in use
                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    {
                        // If it opens without exception, close and delete
                    }
                    File.Delete(filePath);
                }
                catch (IOException)
                {
                    // File is in use
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    try
                    {
                        File.Delete(filePath);
                    }
                    catch (Exception innerEx)
                    {
                        throw new IOException("File is in use and cannot be deleted.", innerEx);
                    }
                }
            }
        }
        [HttpGet]
        [Route("getproducthsngroups")]
        public IHttpActionResult GetProductHSNGroups()
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyId"]);

                str = "Select ProductHSNID,ProductHSNName,ProductCategory," +
                      "GSTTaxPercentage,CGSTTaxPercentage,SGSTTaxPercentage,IGSTTaxPercentage," +
                      "ExciseTaxPercentage,Isnull(MinimumExciseAmount,0) AS MinimumExciseAmount " +
                      "From ProductHSNMaster " +
                      $"Where ProductCategory='Finish Goods' AND Isnull(IsDeletedTransaction,0)=0 " +
                      "Order By ProductHSNName";

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return Ok(JsonConvert.SerializeObject(data.Message));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("LoadOperationSlabsDetails")]
        public string LoadOperationSlabsDetails()
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);
                str = "SELECT DISTINCT PM.TypeofCharges, Case When ISNULL(PMS.Rate, 0)=0 Then Round(ISNULL(PM.Rate, 0),5) Else Round(ISNULL(PMS.Rate, 0),5) End AS Rate, Case When PMS.MinimumCharges=0 Then PM.MinimumCharges Else PMS.MinimumCharges End As MinimumCharges, PM.SetupCharges, PM.SizeToBeConsidered, PM.ChargeApplyOnSheets, PM.PrePress, PM.ProcessID, PM.ProcessName,Isnull(PMS.FromQty,0) AS FromQty,IsNull(PMS.ToQty,0) As ToQty,Isnull(PMS.RateFactor,'') As RateFactor,PMS.SlabID FROM ProcessMaster AS PM Left JOIN ProcessMasterSlabs AS PMS ON PMS.ProcessID = PM.ProcessID And PMS.IsLocked=0 /*And PM.CompanyID=PMS.CompanyID*/ Where /* PM.ProcessId In (\"\") And PM.CompanyId = " + GBLCompanyID + " And  */ Isnull(PM.IsDeletedTransaction,0)<>1 Order by PMS.SlabID Asc ";
                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return JsonConvert.SerializeObject(data.Message);
            }
            catch (Exception ex)
            {
                return "500";
            }
        }

        [HttpGet]
        [Route("machigrid")]
        public IHttpActionResult MachiGrid()
        {
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
            DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

            str = "Select nullif(MM.MachineID,'') As MachineID, nullif(MM.MachineName,'') As MachineName, nullif(MM.DepartmentID,'') as DepartmentID, nullif(DM.DepartmentName,'') As DepartmentName,Isnull(MM.MachineSpeed,0) AS MachineSpeed,Isnull(MM.MakeReadyTime,0) AS MakeReadyTime,Isnull(MM.JobChangeOverTime,0) AS JobChangeOverTime,0 AS IsDefaultMachine " +
                " From MachineMaster as MM Inner Join DepartmentMaster as DM on MM.DepartmentID=DM.DepartmentID  " +
                "And Isnull(MM.IsDeletedTransaction,0)<>1 Where DM.DepartmentID = 100 Order By MM.MachineName";

            DBConnection.FillDataTable(ref dataTable, str);
            data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
            return Ok(JsonConvert.SerializeObject(data.Message));
        }

        [HttpGet]
        [Route("getallmachines")]
        public IHttpActionResult GetMachine()
        {
            try
            {
                string str;
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                str = "Select Distinct isnull(MM.MachineID,0) as MachineID,MM.DepartmentID, nullif(MM.MachineName,'') as MachineName, Case When Isnull(PAM.MachineSpeed,0)>0 THEN Isnull(PAM.MachineSpeed,0) ELSE Isnull(MM.MachineSpeed,0)  END as MachineSpeed,isnull(PM.ProcessID,0) as ProcessID,Case When Isnull(PAM.MakeReadyTime,0)>0 THEN  Isnull(PAM.MakeReadyTime,0) ELSE Isnull(MM.MakeReadyTime,0) END as MakeReadyTime, Case When Isnull(PAM.JobChangeOverTime,0)>0 THEN Isnull(PAM.JobChangeOverTime,0) ELSE Isnull(MM.JobChangeOverTime,0) END as JobChangeOverTime,ISNULL(MM.MakeReadyPerHourCost,0) AS MakeReadyPerHourCost, ISNULL(MM.PerHourCost,0) AS MachinePerHourCost,Isnull(PAM.IsDefaultMachine,0) AS IsDefaultMachine,  NULLIF(MM.SpeedUnit, '') AS SpeedUnit,Isnull(NULLIF(MM.MakeReadyTimeMode,''),'Flat') AS MakeReadyTimeMode,NULLIF(MM.PerHourCostingParameter,'') AS PerHourCostingParameter " +
                      "From ProcessMaster as PM INNER JOIN ProcessAllocatedMachineMaster as PAM ON PAM.ProcessID = PM.ProcessID And PAM.CompanyID = PM.CompanyID AND Isnull(PAM.IsDeletedTransaction,0)=0  INNER JOIN MachineMaster AS MM ON MM.MachineID = PAM.MachineID And  PAM.CompanyID = MM.CompanyID  AND Isnull(MM.IsDeletedTransaction,0)=0 " +
                      "Where PAM.CompanyID = " + GBLCompanyID + "  AND Isnull(PM.IsDeletedTransaction,0)=0 Order BY MachineName";

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return Ok(JsonConvert.SerializeObject(data.Message));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }
        [HttpGet]
        [Route("getprocessallocatedsubgroups")]
        public IHttpActionResult GetProcessAllocatedSubGroup()
        {
            try
            {
                string str;
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                str = "Select ISG.ItemSubGroupID,ISG.ItemSubGroupName,MIA.ProcessId From ProcessAllocatedMaterialGroupMaster AS MIA INNER JOIN ItemSubGroupMaster AS ISG ON ISG.ItemSubGroupID=MIA.ItemSubGroupID AND Isnull(ISG.IsDeletedTransaction,0)=0 Where MIA.CompanyID = " + GBLCompanyID + " AND Isnull(MIA.IsDeletedTransaction,0)=0 Order BY ItemSubGroupName";

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return Ok(JsonConvert.SerializeObject(data.Message));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }
        [HttpGet]
        [Route("getprocessallocatedtoolgroups")]
        public IHttpActionResult GetProcessAllocatedToolGroup()
        {
            try
            {
                string str;
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                str = "Select PT.ToolGroupID,ToolGroupName,ProcessID from ProcessToolGroupAllocationMaster As PT INNER JOIN ToolGroupMaster As TGM on PT.ToolGroupID = TGM.ToolGroupID Where TGM.CompanyID = " + GBLCompanyID + " AND Isnull(TGM.IsDeletedTransaction,0)=0 ";

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return Ok(JsonConvert.SerializeObject(data.Message));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [HttpPost]
        [Route("Shirin_Job")]
        public IHttpActionResult Shirin_Job([FromBody] JObject payload)
        {
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
            DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);
            HttpContext.Current.Session["ConversationID"] = 1;
            string res = Shirin.Shirin_Job(payload.ToString(), "");
            return Ok(res);
        }

        [HttpPost]
        [Route("Shirin_Job_Bot")]
        public IHttpActionResult Shirin_Job_Bot([FromBody] JObject payload)
        {
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
            DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);
            HttpContext.Current.Session["ConversationID"] = 1;
            string res = Shirin.Shirin_Job(payload.ToString(), "costingbot");
            return Ok(res);
        }

        [HttpGet]
        [Route("corrugationplan")]
        public IHttpActionResult Plan_On_Corrugation_Machine(string W, string L)
        {
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
            DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);
            string res = Shirin.Plan_On_Corrugation_Machine(W, L);
            return Ok(res);
        }

        [HttpGet]
        [Route("LoadOperations/{DomainType}")]
        public IHttpActionResult LoadOperations(string DomainType, string ProcessPurpose = "")
        {
            try
            {
                string GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                string version = Convert.ToString(HttpContext.Current.Session["Version"]);
                string DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                string processPurpose = string.Empty;
                if (!string.IsNullOrEmpty(ProcessPurpose))
                {
                    processPurpose = " And ProcessPurpose='" + ProcessPurpose + "'";
                }

                string domainName = string.Empty;
                if (!string.IsNullOrEmpty(DomainType))
                {
                    switch (DomainType.ToUpper().ToString())
                    {
                        case "FLEXO":
                            domainName = " AND Isnull(NullIf(PM.ProcessModuleType,''),'Universal') IN('Universal','Flexo')";
                            break;
                        case "OFFSET":
                            domainName = " AND Isnull(NullIf(PM.ProcessModuleType,''),'Universal') IN('Universal','Offset')";
                            break;
                        case "ROTOGRAVURE":
                            domainName = " AND Isnull(NullIf(PM.ProcessModuleType,''),'Universal') IN('Universal','RotoGravure')";
                            break;
                        case "LARGEFORMAT":
                            domainName = " AND Isnull(NullIf(PM.ProcessModuleType,''),'Universal') IN('Universal','LargeFormat')";
                            break;
                    }
                }
                str = "SELECT DISTINCT PM.ProcessID, REPLACE(NULLIF (ProcessName, ''), '\"', '') AS ProcessName,NULLIF (PrePress, '') AS PrePress, NULLIF (TypeofCharges, '') AS TypeofCharges,NULLIF (SizeToBeConsidered, '') AS SizeToBeConsidered, ISNULL(NULLIF(Rate, ''), 0) AS Rate,NULLIF (MinimumCharges, '') AS MinimumCharges, NULLIF (SetupCharges, '') AS SetupCharges,NULLIF (IsDisplay, '') AS IsDisplay,NULLIF (IsOnlineProcess, '') AS IsOnlineProcess,REPLACE(NULLIF (ChargeApplyOnSheets, ''), '\"', '') AS ChargeApplyOnSheets,REPLACE(NULLIF (DisplayProcessName, ''), '\"', '') AS DisplayProcessName, 0 AS Amount,'' AS RateFactor, '' AS AddRow,0 AS MakeReadyTime,0 AS MachineID,0 AS MachineSpeed,0 AS JobChangeOverTime,0 AS MakeReadyPerHourCost,0 AS MachinePerHourCost,Isnull(PM.ToolRequired,0) AS ToolRequired,Isnull(PM.ProcessProductionType,'None') AS ProcessProductionType,0 AS PaperConsumptionRequired,'' AS MachineName,Isnull(DM.DepartmentID,0) AS DepartmentID,Isnull(DM.SequenceNo,0) AS DepartmentSequenceNo,Isnull(PM.ProcessFlatWastageValue,0) AS ProcessFlatWastageValue,Isnull(PM.ProcessWastagePercentage,0) AS ProcessWastagePercentage,Isnull(NullIf(PM.ProcessModuleType,''),'Universal') AS ProcessModuleType,0 AS ExecutionTime,0 AS TotalExecutionTime,0 As MakeReadyMachineCost,0 As ExecutionCost,0 AS MachineCost,0 AS MaterialCost,1 As Pieces ,1 As NoOfStitch,1 As NoOfLoops,1 As NoOfColors,1 AS PagesPerSection,0 As NoOfForms,Case When Isnull(PTG.ToolGroupID,0)>0 THEN 1 ELSE 0 END AS PlateRequired,1 As NoOfFolds,PM.PerHourCostingParameter,Isnull(PM.MinimumQuantityToBeCharged,0) AS MinimumQuantityToBeCharged,0 AS PerHourCalculationQuantity FROM ProcessMaster AS PM INNER JOIN DepartmentMaster AS DM ON DM.DepartmentID=PM.DepartmentIDInner Join CategoryWiseProcessAllocation As CPA On CPA.ProcessID=PM.ProcessID And CPA.CompanyID=PM.CompanyIDLEFT JOIN (Select PTG.ProcessID,PTG.ToolGroupID From ProcessToolGroupAllocationMaster AS PTGINNER JOIN ToolGroupMaster AS TGM ON TGM.ToolGroupID=PTG.ToolGroupIDWhere TGM.ToolGroupNameID=-9 and ISNULL(PTG.IsDeletedTransaction,0)=0 and ISNULL(TGM.IsDeletedTransaction,0)=0)AS PTG ON PTG.ProcessID=PM.ProcessIDWHERE (ISNULL(PM.IsDeletedTransaction, 0) = 0) And CPA.CategoryID = -1000 " + processPurpose + " ORDER BY ProcessName";

                DBConnection.FillDataTable(ref dataTable, str);
                if (dataTable.Rows.Count <= 0)
                {
                    str = " Select Distinct PM.ProcessID, Replace(Nullif(PM.ProcessName,''),'\"','') as ProcessName,Nullif(PM.PrePress,'') as PrePress, Nullif(PM.TypeofCharges,'') as TypeofCharges,Nullif(PM.SizeToBeConsidered,'') as SizeToBeConsidered, ISNULL(NULLIF(PM.Rate, ''), 0) AS Rate,Nullif(PM.MinimumCharges,'') as MinimumCharges, Nullif(PM.SetupCharges,'') as SetupCharges,Nullif(PM.IsDisplay,'') as IsDisplay,Nullif(PM.IsOnlineProcess,'') as IsOnlineProcess,Replace(Nullif(PM.ChargeApplyOnSheets,''),'\"','') As ChargeApplyOnSheets,Replace( Nullif(PM.DisplayProcessName,''),'\"','') as DisplayProcessName,0 As Amount,'' As RateFactor,'' As AddRow,0 AS MakeReadyTime,0 AS MachineID,0 AS MachineSpeed,0 AS JobChangeOverTime,0 AS MakeReadyPerHourCost,0 AS MachinePerHourCost,Isnull(PM.ToolRequired,0) AS ToolRequired,Isnull(PM.ProcessProductionType,'None') AS ProcessProductionType,0 AS PaperConsumptionRequired,'' AS MachineName,Isnull(DM.DepartmentID,0) AS DepartmentID,Isnull(DM.SequenceNo,0) AS DepartmentSequenceNo,Isnull(PM.ProcessFlatWastageValue,0) AS ProcessFlatWastageValue,Isnull(PM.ProcessWastagePercentage,0) AS ProcessWastagePercentage,Isnull(NullIf(PM.ProcessModuleType,''),'Universal') AS ProcessModuleType,0 AS ExecutionTime,0 AS TotalExecutionTime,0 As MakeReadyMachineCost,0 As ExecutionCost,0 AS MachineCost,0 AS MaterialCost,1 As Pieces ,1 As NoOfStitch,1 As NoOfLoops,1 As NoOfColors,1 AS PagesPerSection,0 As NoOfForms,Case When Isnull(PTG.ToolGroupID,0)>0 THEN 1 ELSE 0 END AS PlateRequired,1 As NoOfFolds,PM.PerHourCostingParameter,Isnull(PM.MinimumQuantityToBeCharged,0) AS MinimumQuantityToBeCharged,0 AS PerHourCalculationQuantity  From ProcessMaster AS PM INNER JOIN DepartmentMaster AS DM ON DM.DepartmentID=PM.DepartmentID LEFT JOIN (Select PTG.ProcessID,PTG.ToolGroupID From ProcessToolGroupAllocationMaster AS PTG INNER JOIN ToolGroupMaster AS TGM ON TGM.ToolGroupID=PTG.ToolGroupID Where TGM.ToolGroupNameID=-9 and ISNULL(PTG.IsDeletedTransaction,0)=0 and ISNULL(TGM.IsDeletedTransaction,0)=0) AS PTG ON PTG.ProcessID=PM.ProcessID WHERE Isnull(PM.IsDeletedTransaction,0)=0 " + processPurpose + domainName + " Order By ProcessName Asc  ";

                    dataTable.Clear();
                    DBConnection.FillDataTable(ref dataTable, str);
                }

                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return Ok(JsonConvert.SerializeObject(data.Message));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [HttpGet]
        [Route("keylinecoordinates/{ContentType}/{Grain}")]
        public IHttpActionResult KeylineCoordinates(string ContentType, string Grain)
        {
            str = "Select UpsType,CoordinateID, Nullif(ShapeType,'') as ShapeType, nullif(ShapeName,'') as ShapeName , AddInX1, AddInY1, AddInX2, AddInY2,AddInXForUps,AddInYForUps, nullif(LineType,'') as LineType, nullif(LineStyles,'') as LineStyles , nullif(SheetSize,'') as SheetSize From ContentWiseKeylineCoordinates Where ContentType = '" + ContentType + "' and Grain = '" + Grain + "' Order by CoordinateID";
            DBConnection.FillDataTable(ref dataTable, str);
            if (Grain == "Across Grain")
            {
                if (dataTable.Rows.Count <= 0)
                {
                    str = "Select UpsType,CoordinateID, Nullif(ShapeType,'') as ShapeType, nullif(ShapeName,'') as ShapeName ,AddInY1 as AddInX1,   AddInX1 as AddInY1, AddInY2 as AddInX2, AddInX2 as AddInY2,AddInYForUps as AddInXForUps,AddInXForUps as AddInYForUps, nullif(LineType,'') as LineType, nullif(LineStyles,'') as LineStyles , nullif(SheetSize,'') as SheetSize From ContentWiseKeylineCoordinates Where ContentType = '" + ContentType + "' and Grain = 'With Grain' Order by CoordinateID";
                    DBConnection.FillDataTable(ref dataTable, str);
                }
            }
            data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
            return Ok(JsonConvert.SerializeObject(data.Message));
        }

        [HttpPost]
        [Route("CalculateOperation")]
        public IHttpActionResult CalculateOperation([FromBody] IndusWebApi.Controllers.Planning.Api_shiring_serviceController.OperationRequest req)
        {
            bool queryType = Convert.ToBoolean(HttpContext.Current.Request.QueryString["Isdefault"] ?? "false");
            string category = HttpContext.Current.Request.QueryString["category"] ?? "";
            string content = HttpContext.Current.Request.QueryString["content"] ?? "";
            var response = Shirin.calculateoprcost(req, queryType, category, content);
            return Ok(response);
        }
        [HttpPost]
        [Route("updateqoutestatus")]
        public IHttpActionResult UpdateQuoteStatus([FromBody] UpdateStatusRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.BookingID) || string.IsNullOrEmpty(request.Status))
                {
                    return BadRequest("BookingID ID and Status are required.");
                }
                str = "Update JobBooking Set Status = '" + request.Status + "' Where ISNULL(IsDeletedTransaction,0) = 0 And BookingID = " + request.BookingID;
                DBConnection.ExecuteNonSQLQuery(str);
                return Ok("Updated");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        [Route("getquotationDetail/{bookingid}")]
        public IHttpActionResult getquotationDetails(string bookingid)
        {
            string GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
            try
            {
                DataTable DtMain = new DataTable();
                DataTable DtDetails = new DataTable();
                DataTable DtProcess = new DataTable();

                str = " Select JE.AnnualQyantity,JE.Remark, JE.BookingID,JE.ConcernPerson,JE.MailingName,JE.EmailSubject,JE.MailingAddress,JE.EmailBody,JE.EmailTo,CCM.CINNo,CCM.ContactNO,CCM.GSTIN,Case When Isnull(JE.Designation,'') ='' Then UM.Designation Else JE.Designation End As Designation,Case When Isnull(JE.QuoteByUser,'')='' Then UM.UserName Else JE.QuoteByUser End As UserName,UM.ContactNo As UserContactNo,Nullif(JE.ProductCode,'') As ProductCode,JE.BookingNo,Replace(Convert(Nvarchar(30), JE.CreatedDate, 106),' ','-') as Job_Date,LM.LedgerName As LedgerName, Case When Isnull(JE.MailingAddress,'')='' Then (STUFF(( SELECT ','+A.FieldValue FROM dbo.LedgerMasterDetails A  Where A.CompanyID=LM.CompanyID And A.LedgerID=LM.LedgerID And FieldName In ('Address1','Address2','Address3') And Isnull(A.IsDeletedTransaction,0)<>1 Order By SequenceNo FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 1, '')) + CHAR(13)+CHAR(10)+ (SELECT DISTINCT A.FieldValue FROM dbo.LedgerMasterDetails A  Where A.CompanyID=LM.CompanyID And A.LedgerID=LM.LedgerID And FieldName In ('City') And Isnull(A.IsDeletedTransaction,0)<>1) +CHAR(13)+CHAR(10)+ (SELECT DISTINCT A.FieldValue FROM dbo.LedgerMasterDetails A Where A.CompanyID=LM.CompanyID And A.LedgerID=LM.LedgerID And FieldName In ('Pincode') And Isnull(A.IsDeletedTransaction,0)<>1) +CHAR(13)+CHAR(10)+ (SELECT DISTINCT A.FieldValue FROM dbo.LedgerMasterDetails A  Where A.CompanyID=LM.CompanyID And A.LedgerID=LM.LedgerID And FieldName In ('District') And Isnull(A.IsDeletedTransaction,0)<>1) +CHAR(13)+CHAR(10)+ (STUFF(( SELECT ','+A.FieldValue FROM dbo.LedgerMasterDetails A  Where A.CompanyID=LM.CompanyID And A.LedgerID=LM.LedgerID And FieldName In ('State','Country') And Isnull(A.IsDeletedTransaction,0)<>1 Order By SequenceNo Desc FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 1, '')) Else JE.MailingAddress End As Address, JE.JobName ,CCM.CompanyName,nullif(JE.Remark,'') as Remark,nullif(JE.HeaderText,'') as HeaderText,nullif(JE.FooterText,'') as FooterText,IsNull(JE.ProcessContentRemarks,'') As ProcessContentRemarks  " +
                      " From JobBooking As JE Inner Join LedgerMaster as LM On LM.LedgerID=JE.LedgerID  Inner Join CompanyMaster As CCM On JE.CompanyId=CCM.CompanyID Inner Join UserMaster As UM On JE.QuotedByUserID=UM.UserID And JE.CompanyID=UM.CompanyID And Isnull(UM.IsBlocked,0)=0  " +
                      " Where JE.BookingID = '" + bookingid + "' And JE.CompanyId = '" + GBLCompanyID + "' ";
                DBConnection.FillDataTable(ref DtMain, str);

                str = " Select Distinct CASE WHEN JEC.PlanContName = 'Pre Planned Sheet' THEN ISNULL(VCS.JobPrePlan, '')  ELSE  (CASE WHEN TRY_CONVERT(float, VCS.SizeHeight) = 0 THEN ''   ELSE 'H:' + CONVERT(nvarchar(10), ROUND(TRY_CONVERT(float, VCS.SizeHeight), 2))+' MM' END + CASE WHEN TRY_CONVERT(float, VCS.SizeLength) = 0 THEN '' ELSE ', L:' + CONVERT(nvarchar(10), ROUND(TRY_CONVERT(float, VCS.SizeLength), 2))+' MM' END + CASE WHEN TRY_CONVERT(float, VCS.SizeWidth) = 0 THEN '' ELSE ', W:' + CONVERT(nvarchar(10), ROUND(TRY_CONVERT(float, VCS.SizeWidth), 2))+' MM' END + CASE WHEN TRY_CONVERT(float, VCS.SizeOpenflap) = 0 THEN '' ELSE ', OF:' + CONVERT(nvarchar(10), ROUND(TRY_CONVERT(float, VCS.SizeOpenflap), 2))+' MM' END + CASE WHEN TRY_CONVERT(float, VCS.SizePastingflap) = 0 THEN '' ELSE ', PF:' + CONVERT(nvarchar(10), ROUND(TRY_CONVERT(float, VCS.SizePastingflap), 2))+' MM' END + CASE WHEN TRY_CONVERT(float, VCS.SizeBottomflap) = 0 THEN '' ELSE ', BF:' + CONVERT(nvarchar(10), ROUND(TRY_CONVERT(float, VCS.SizeBottomflap), 2))+' MM' END + ' | ' + CASE WHEN TRY_CONVERT(float, VCS.SizeHeight) = 0 THEN ''  ELSE 'H:' + CONVERT(nvarchar(10), ROUND(TRY_CONVERT(float, VCS.SizeHeight) / 10, 2))+' CM'  END +  CASE WHEN TRY_CONVERT(float, VCS.SizeLength) = 0 THEN ''  ELSE ', L:' + CONVERT(nvarchar(10), ROUND(TRY_CONVERT(float, VCS.SizeLength) / 10, 2))+' CM'  END + CASE WHEN TRY_CONVERT(float, VCS.SizeWidth) = 0 THEN '' ELSE ', W:' + CONVERT(nvarchar(10), ROUND(TRY_CONVERT(float, VCS.SizeWidth) / 10, 2))+' CM'  END + CASE WHEN TRY_CONVERT(float, VCS.SizeOpenflap) = 0 THEN ''  ELSE ', OF:' + CONVERT(nvarchar(10), ROUND(TRY_CONVERT(float, VCS.SizeOpenflap) / 10, 2))+' CM'  END + CASE WHEN TRY_CONVERT(float, VCS.SizePastingflap) = 0 THEN ''   ELSE ', PF:' + CONVERT(nvarchar(10), ROUND(TRY_CONVERT(float, VCS.SizePastingflap) / 10, 2))+' CM'   END +  CASE WHEN TRY_CONVERT(float, VCS.SizeBottomflap) = 0 THEN ''   ELSE ', BF:' + CONVERT(nvarchar(10), ROUND(TRY_CONVERT(float, VCS.SizeBottomflap)/ 10, 2))+' CM'   END) END AS JobSizeDetails,VCS.SizeHeight,CM.CategoryName,Case when nullif(VCS.ChkPaperByClient,'') = 'False' Then CMS.CompanyName  else 'Client' end as Paperby, JEC.BookingID,Convert(nvarchar(max), JEC.PlanContName) as Content_Name, Stuff((Select ', '+ ProcessName From ProcessMaster Where ProcessID In (Select ProcessID From JobBookingProcess  Where BookingID=JEO.BookingID  And ContentsID=JEO.ContentsID  And PlanContQty = JEO.PlanContQty  And Isnull(IsDisplay,0)=1)  For XML PATH('')),1,2,'') AS Operatios,Case When Isnull(VCS.JobPrePlan,'') <> '' Then Replace(Replace(Replace(VCS.JobPrePlan,'O:','OF:'),'P:','PF:'),'B:','BF:') Else 'H:' + TRY_CONVERT(nvarchar(10), VCS.SizeHeight) + ', L:' + TRY_CONVERT(nvarchar(10), VCS.SizeLength) + ', W:' + TRY_CONVERT(nvarchar(10), VCS.SizeWidth) + ', OF:' + TRY_CONVERT(nvarchar(10), VCS.SizeOpenflap)  + ', PF:' + TRY_CONVERT(nvarchar(10), VCS.SizePastingflap) + ', BF:' + TRY_CONVERT(nvarchar(10), VCS.SizeBottomflap) + ', Pages:' + VCS.JobNoOfPages End As Job_Size,'F:' + VCS.PlanFColor + ' / ' + 'B:' + VCS.PlanBColor + ' , ' + 'SF:' + VCS.PlanSpeFColor + ' / ' + 'SB:' + VCS.PlanSpeBColor As Printing, 'Quality:' + VCS.ItemPlanQuality + ', GSM:' + VCS.ItemPlanGsm As Paper,case  when TRY_CONVERT(float, VCS.SizeHeight) = 0 then ''  else 'H:' + Convert(nvarchar(10), ROUND(TRY_CONVERT(float, VCS.SizeHeight) / 25.4, 2)) end + case  when TRY_CONVERT(float, VCS.SizeLength) = 0 then '' else ', L:' + Convert(nvarchar(10), ROUND(TRY_CONVERT(float, VCS.SizeLength) / 25.4, 2)) end  + case when TRY_CONVERT(float, VCS.SizeWidth) = 0 then '' else ', W:' + Convert(nvarchar(10), ROUND(TRY_CONVERT(float, VCS.SizeWidth) / 25.4, 2))  end + case when TRY_CONVERT(float, VCS.SizeOpenflap) = 0 then ''  else ', OF:' + Convert(nvarchar(10), ROUND(TRY_CONVERT(float, VCS.SizeOpenflap) / 25.4, 2)) end + case when TRY_CONVERT(float, VCS.SizePastingflap) = 0 then '' else ', PF:' + Convert(nvarchar(10), ROUND(TRY_CONVERT(float, VCS.SizePastingflap) / 25.4, 2))  end  + case  when TRY_CONVERT(float, VCS.SizeBottomflap) = 0 then ''  else ', BF:' + Convert(nvarchar(10), ROUND(TRY_CONVERT(float, VCS.SizeBottomflap) / 25.4, 2)) end + Case When TRY_CONVERT(float, VCS.JobNoOfPages)>0 THEN Case When JEC.PlanContentType like '%leave%' THEN ',Leaves:' + VCS.JobNoOfPages ELSE ',Pages:' + VCS.JobNoOfPages END ELSE '' END /*',Pages:' + VCS.JobNoOfPages*/ As Job_Size_In_Inches  " +
                      " From CompanyMaster As CMS Inner Join JobBooking As JB On JB.CompanyID = CMS.CompanyID Inner Join CategoryMaster as CM On CM.CategoryID=JB.CategoryID and CM.CompanyId=JB.CompanyId Inner Join JobBookingContents As JEC On JB.BookingID = JEC.BookingID And JB.CompanyID=JEC.CompanyID  Inner Join ViewJobBookingContents As VCS On VCS.BookingID=JEC.BookingID Inner Join JobBookingCostings As JCO On JCO.BookingID = JEC.BookingID And JCO.PlanContQty = JEC.PlanContQty And JCO.CompanyID=JEC.CompanyID   Inner Join (Select Top(1) JobContentsID,PlanContName,PlanContQty from JobBookingContents Where BookingID = '" + bookingid + "' And Isnull(IsDeletedTransaction,0)=0 Order by JobContentsID Asc ) QT On QT.PlanContQty =  JEC.PlanContQty  Left Join JobBookingProcess As JEO On JEO.ContentsID = JEC.JobContentsID And JEC.PlanContQty = JEO.PlanContQty  Left Join JobBookingOnetimeCharges As JOC On JOC.BookingID = JEC.BookingID And JEC.PlanContQty = JEO.PlanContQty And JEO.CompanyID=JEC.CompanyID  And VCS.BookingID=JEC.BookingID And VCS.CompanyID=JEC.CompanyID  " +
                      " Where JEC.BookingID = '" + bookingid + "' And JEC.CompanyId = '" + GBLCompanyID + "' ";
                DBConnection.FillDataTable(ref DtDetails, str);

                str = " Select Distinct JCO.TaxPercentage,CM.CategoryName,JB.CurrencySymbol,JEC.BookingID   ,Nullif(JB.ProductCode,'') As ProductCode,'0.00' As GrantAmount,JB.BookingNo ,JEC.PlanContQty,JCO.UnitCost,JCO.UnitCost1000,JB.TypeOfCost,JCO.QuotedCost,JCO.GrandTotalCost,JB.JobName,Replace(Convert(Nvarchar(30), JB.CreatedDate, 106),' ','-') as Job_Date,Case when ISNULL(IsQuotedRateTaxInclusive,0) = 'False' Then 'Exclusive' else 'Inclusive' end As TaxInorExClusive,(JEC.PlanContQty*JCO.QuotedCost) as Amount,JCO.GrandTotalCost,JCO.UnitCost1000,JCO.QuotedCost,JCO.FreightCost,FreightCost1000,JCO.Cost1000Pcs,JCO.CreditCost,JCO.CreditCost1000,JCO.CreditDays,JCO.TotalWeightOfJob,JCO.FreightRate,JCO.ProfitCost,Round((JCO.ProfitCost/JCO.PlanContQty)*1000,3) as ProfitCost1000,JCO.BoardCost1000,JCO.OtherMaterialCost,JCO.ConversionCost,JEC.CutSize,JEC.TotalUps  " +
                      " From JobBooking As JB Inner Join JobBookingContents As JEC On JB.BookingID = JEC.BookingID And JB.CompanyID=JEC.CompanyID Inner Join CategoryMaster As CM On CM.CategoryID = JB.CategoryID And CM.CompanyID=JB.CompanyID Inner Join ViewJobBookingContents As VCS On VCS.BookingID=JEC.BookingID Inner Join JobBookingCostings As JCO On JCO.BookingID = JEC.BookingID And JCO.PlanContQty = JEC.PlanContQty And JCO.CompanyID=JEC.CompanyID Left Join JobBookingProcess As JEO On JEO.ContentsID = JEC.JobContentsID And JEC.PlanContQty = JEO.PlanContQty And JEO.CompanyID=JEC.CompanyID And VCS.BookingID=JEC.BookingID and VCS.CompanyID=JEC.CompanyID " +
                      " Where JEC.BookingID = '" + bookingid + "' And JEC.CompanyId = '" + GBLCompanyID + "' ";
                DBConnection.FillDataTable(ref DtProcess, str);

                DtMain.TableName = "Main";
                DtDetails.TableName = "Datails";
                DtProcess.TableName = "Price";

                DataSet dataSet = new DataSet();
                dataSet.Merge(DtMain);
                dataSet.Merge(DtDetails);
                dataSet.Merge(DtProcess);
                data.Message = DBConnection.ConvertDataSetsToJsonString(dataSet);
                return Ok(JsonConvert.SerializeObject(data.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("getwastagetypeslab/{ActualSheet}/{WastageType}")]
        public IHttpActionResult GetWastageTypeslab(string ActualSheet, string WastageType)
        {
            str = "Select ID,WastageCode,WastageType,WastagePer,WastagePerFluted,ISNULL(IsCorrugated,0) as IsCorrugated from ProcessWiseWastageSetting Where WastageType = '" + WastageType + "' And SheetFrom <= " + ActualSheet + " And SheetTo >= " + ActualSheet;
            DBConnection.FillDataTable(ref dataTable, str);

            data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
            return Ok(JsonConvert.SerializeObject(data.Message));
        }

        [HttpGet]
        [Route("getallwastagetypes")]
        public IHttpActionResult GetWastageType()
        {
            str = "Select Distinct WastageType from ProcessWiseWastageSetting Where ISNULL(IsDeletedTransaction,0) = 0 ";
            DBConnection.FillDataTable(ref dataTable, str);

            data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
            return Ok(JsonConvert.SerializeObject(data.Message));
        }
        [HttpPost]
        [Route("ShirinJobMaster")]
        public IHttpActionResult ShirinJobMaster([FromBody] IndusWebApi.Controllers.Planning.Api_shiring_serviceController.ShirinJobMasterRequest payload)
        {
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
            DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

            string res = Shirin.ShirinJobMaster(payload);
            return Ok(res);
        }
        [HttpGet]
        [Route("getsubgroups")]
        public IHttpActionResult GetSubGroup()
        {
            try
            {
                string str;
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                str = "Select ItemSubGroupID,ItemSubGroupName From ItemSubGroupMaster Where Isnull(IsDeletedTransaction,0)=0 Order BY ItemSubGroupName";

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return Ok(JsonConvert.SerializeObject(data.Message));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }
        [HttpGet]
        [Route("getitemslist/{ItemgroupID}")]
        public IHttpActionResult GetItemList(string ItemgroupID)
        {
            try
            {
                string str;
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                str = "Select ItemID,ItemCode,ItemName From ItemMaster Where ISNULL(ItemCode,'') <> '' And ISNULL(ItemName,'') <> '' And Isnull(IsDeletedTransaction,0)=0 And ItemGroupID = " + ItemgroupID + " Order BY ItemCode";

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return Ok(JsonConvert.SerializeObject(data.Message));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        public class ReqRevise
        {
            public int BookingID { get; set; }
            public double TargetedCost { get; set; }
        }

        public class InternalApprovalRequest
        {
            public string type { get; set; }  // Pending, InternalApproved, Approved, Rework
            public string status { get; set; }  // Approve, Rework, Reject
            public string remarks { get; set; }
            public string BKID { get; set; }  // BookingID(s) - can be comma separated
            public string BKNo { get; set; }  // BookingNo
            public string InternalApprovedUserID { get; set; }
        }

        [HttpPost]
        [Route("internalapprovalupdatestatus")]
        public string InternalApprovalUpdateStatus([FromBody] InternalApprovalRequest request)
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                GBLUserID = Convert.ToString(HttpContext.Current.Session["UserID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);
                string IsAdmin = Convert.ToString(HttpContext.Current.Session["IsAdmin"]);

                if (request == null)
                {
                    return "No data for update status";
                }

                DataTable DTCheck = new DataTable();
                string Str = "";
                string type = request.type?.ToLower() ?? "";
                string status = request.status ?? "";

                if (type == "pending")
                {
                    if (status == "Approve")
                    {
                        Str = "Update JobBooking Set IsInternalApproved=1,InternalApprovedDate=Getdate(),RemarkInternalApproved ='" + request.remarks + "',InternalApprovedUserID=" + GBLUserID + " Where BookingID IN(" + request.BKID + ") And CompanyID=" + GBLCompanyID;
                    }
                    else if (status == "Rework")
                    {
                        Str = "Update JobBooking Set IsRework =1 ,ReworkDate = Getdate(), ReworkRemark = '" + request.remarks + "' ,ReworkBy=" + GBLUserID + " Where BookingID IN(" + request.BKID + ")  AND CompanyId = " + GBLCompanyID;
                    }
                    else if (status == "Reject")
                    {
                        Str = "Update JobBooking Set IsInternalApproved=0,InternalApprovedDate=Getdate(),RemarkInternalApproved ='',IsCancelled =1 ,CancelledDate = Getdate(), CancelledRemark = '" + request.remarks + "',CancelledBy=" + GBLUserID + " Where BookingID IN(" + request.BKID + ")  AND CompanyId = " + GBLCompanyID;
                    }
                }
                else if (type == "internalapproved" || type == "approved")
                {
                    // Check if user has approved
                    if (!string.IsNullOrEmpty(request.InternalApprovedUserID) && IsAdmin != "1" && IsAdmin?.ToLower() != "true")
                    {
                        var approvedIds = request.InternalApprovedUserID.Split(',');
                        if (!approvedIds.Contains(GBLUserID))
                        {
                            return "Booking No(s): " + request.BKNo + " have not been approved by the current user.";
                        }
                    }

                    // Check if already in price approval
                    DBConnection.FillDataTable(ref DTCheck, "Select Isnull(IsSendForPriceApproval,0) As IsSendForPriceApproval From JobBooking Where BookingID IN(" + request.BKID + ") AND CompanyId = " + GBLCompanyID);
                    if (DTCheck.Rows.Count > 0)
                    {
                        var isSendForPriceApproval = DTCheck.Rows[0]["IsSendForPriceApproval"];
                        if (isSendForPriceApproval != DBNull.Value && (Convert.ToInt32(isSendForPriceApproval) == 1 || Convert.ToBoolean(isSendForPriceApproval) == true))
                        {
                            return "Already approved in price approval";
                        }
                    }

                    if (status == "Rework")
                    {
                        Str = "Update JobBooking Set IsInternalApproved=0,InternalApprovedDate=Getdate(),RemarkInternalApproved ='',IsRework =1 ,ReworkDate = Getdate(), ReworkRemark = '" + request.remarks + "',ReworkBy=" + GBLUserID + " Where BookingID IN(" + request.BKID + ") And CompanyID=" + GBLCompanyID;
                    }
                    else if (status == "Reject")
                    {
                        Str = "Update JobBooking Set IsInternalApproved=0,InternalApprovedDate=Getdate(),RemarkInternalApproved ='',IsCancelled =1 ,CancelledDate = Getdate(), CancelledRemark = '" + request.remarks + "',CancelledBy=" + GBLUserID + " Where BookingID IN(" + request.BKID + ")  AND CompanyId = " + GBLCompanyID;
                    }
                }
                else if (type == "rework")
                {
                    // Check if another revision exists
                    DBConnection.FillDataTable(ref DTCheck, "Select Max(BookingID) As MaxBookingID From JobBooking Where CompanyId = " + GBLCompanyID + " AND MaxBookingNo IN(Select Distinct MaxBookingNo From JobBooking Where BookingID IN(" + request.BKID + ") And CompanyID=" + GBLCompanyID + ")");
                    if (DTCheck.Rows.Count > 0)
                    {
                        var maxBookingID = DTCheck.Rows[0]["MaxBookingID"];
                        if (maxBookingID != DBNull.Value && Convert.ToInt32(maxBookingID) > Convert.ToInt32(request.BKID))
                        {
                            return "Another revision of this quote is exists, can't update status..!";
                        }
                    }

                    if (status == "Approve")
                    {
                        Str = "Update JobBooking Set IsInternalApproved=1,InternalApprovedDate=Getdate(),RemarkInternalApproved ='" + request.remarks + "',InternalApprovedUserID=" + GBLUserID + ",IsRework =0 ,ReworkDate = Getdate(), ReworkRemark = '' Where BookingID IN(" + request.BKID + ") And CompanyID=" + GBLCompanyID;
                    }
                }

                if (string.IsNullOrEmpty(Str))
                {
                    return "Invalid request";
                }

                Str = DBConnection.ExecuteNonSQLQuery(Str);
                return Str;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [HttpPost]
        [Route("generatequoterevision")]
        public string GenerateAndSaveQuotationFromBookingId([FromBody] ReqRevise req)
        {
            try
            {
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                GBLProductionUnitID = Convert.ToString(HttpContext.Current.Session["ProductionUnitID"]);
                GBLUserID = Convert.ToString(HttpContext.Current.Session["UserID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                // Build the SaveQuotationRequest from database
                SaveQuotationRequest request = BuildQuotationRequestFromDatabase(req.BookingID);

                if (request == null)
                {
                    return "Error: BookingID not found or invalid";
                }
                double NewProfitPer = 0;
                double NewQuotedCost = 0;
                if (req.TargetedCost > 0)
                {
                    if (request.CostingData != null && request.CostingData is string costingJson && !string.IsNullOrWhiteSpace(costingJson))
                    {
                        var costingList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(costingJson);
                        if (costingList != null && costingList.Count > 0)
                        {
                            foreach (var costing in costingList)
                            {
                                double ProfitPercentage = (double)costing["ProfitPercentage"];
                                double TargetedCost = req.TargetedCost;
                                double QuotedCost = (double)costing["QuotedCost"];
                                double PlanContQty = Convert.ToDouble(costing["PlanContQty"]);

                                double FreightCost1000 = ((double)costing["FreightCost"]) / PlanContQty * 1000;
                                double TaxAmount1000 = ((double)costing["TaxAmount"]) / PlanContQty * 1000;
                                double ProfitCost1000 = ((double)costing["ProfitCost"]) / PlanContQty * 1000;
                                double ActualQuotedCost = QuotedCost - FreightCost1000 - TaxAmount1000 - ProfitCost1000;

                                NewProfitPer = Math.Round((double)(ProfitPercentage - ((QuotedCost - TargetedCost) / ActualQuotedCost) * 100), 2, MidpointRounding.AwayFromZero);
                                double NewProfitCost = (ActualQuotedCost * NewProfitPer / 100) * (PlanContQty / 1000);
                                NewQuotedCost = Math.Round(ActualQuotedCost + FreightCost1000 + (ActualQuotedCost * NewProfitPer / 100), 2, MidpointRounding.AwayFromZero);

                                costing["ProfitPercentage"] = NewProfitPer;
                                costing["ProfitCost"] = NewProfitCost;
                                costing["QuotedCost"] = NewQuotedCost;
                            }
                            request.CostingData = JsonConvert.SerializeObject(costingList);
                        }
                    }
                }

                return SaveQuotationData(request);
            }
            catch (Exception ex)
            {
                return "Error:500 Exception " + ex.Message;
            }
        }

        private SaveQuotationRequest BuildQuotationRequestFromDatabase(int bookingId)
        {
            DataTable dt = new DataTable();

            string sql = @"SELECT JobName, LedgerID, CategoryID, OrderQuantity, TypeOfCost, FinalCost, BookingRemark, Remark, ClientName, ConcernPerson, HeaderText, FooterText, ProductCode, ExpectedCompletionDays, ApprovedBy, IsBooked, DeliveryDate, EmailBody, EnquiryID, IsCatalogCreated, IsDocketComplete, Prefix,0 as InternalApprovedUserID, IsMailSent, IsSendForInternalApproval, ApprovalSendDate, RemarkInternalApproval,0 as  IsInternalApproved,InternalApprovedDate,'' as  RemarkInternalApproved,0 as IsRework, ReworkDate, ReworkRemark,0 as IsCancelled, CancelledDate, CancelledRemark, ConsigneeID,0 as IsSendForPriceApproval, ReasonsofQuote, ShipperID, BackToBackPasting, ProductGroupID, CurrencySymbol, ConversionValue, CurrencyName, SalesEmployeeID, ProductBarcode, IsSendForCostChecking, IsCostCheckingDone, CostingUserID, QuoteType, IsEstimate, IsProductionWorkOrder, IsProductCatalog, IsDeletedTransaction, DeletedDate, DeletedBy, ParentBookingID, ProductHSNID, ShipperPackX, ShipperPackY, ShipperPackZ, ShipperPackQty, Back2BackPasting, ShipperName, ShipperPerBoxWt, ShipperTotalWt, TotalShipperQtyReq, ShipperNoOfPly, 
                            ShipperRate, ShipperAmount, ShipperCBM, ShipperCBF, ShipperWtPerPc, ShippingRate, ShippingAmount, ApprovalSendBy, ApprovalSendTo, CancelledBy, ReworkBy, QuotedCost, EmailTo, MailingName, EmailSubject, MailingAddress, ProcessContentRemarks, QuoteByUser, Designation, AttachedFilesPath, IsExportQuotation, RefProductMasterCode, ProductMasterID, EstimationUnit, ExpectedDeliveryDate, IsQuotedRateTaxInclusive, ArtworkCode, Status, Margin, PlantID, CreditDays, AnnualQuantity, SOURCE FROM JobBooking WHERE BookingId = " + bookingId;
            DBConnection.FillDataTable(ref dt, sql);
            if (dt.Rows.Count == 0) return null;
            var tblBooking = ConvertDataTableToDictionary(dt);

            dt = new DataTable();
            sql = @"SELECT MachineID, MachineName, Gripper, GripperSide, MachineColors, PaperID, PaperSize, CutSize, CutL, CutW, UpsL, UpsW, TotalUps, BalPiece, BalSide, WasteArea, WastePerc, WastageKg, GrainDirection, PlateQty, PlateRate, PlateAmount, MakeReadyWastageSheet, ActualSheets, WastageSheets, TotalPaperWeightInKg, FullSheets, PaperRate, PaperAmount, PrintingImpressions, ImpressionsToBeCharged, PrintingRate, PrintingAmount, TotalMakeReadies, MakeReadyRate, MakeReadyAmount, FinalQuantity, TotalColors, TotalAmount, CutLH, CutHL, PrintingStyle, PrintingChargesType, ExpectedExecutionTime, TotalExecutionTime, MainPaperName, PlanType, PaperRateType, DieCutSize, InterlockStyle, NoOfSets, GrantAmount, Packing, UnitPerPacking, RoundofImpressionsWith, SpeColorFCharges, SpeColorBCharges, SpeColorFAmt, SpeColorBAmt, OpAmt, PlanID, PlanContentType, PlanContName, PlanContQty, KittingMultiplier, SequenceNo, ContentSizeValues, CoatingCharges, CoatingAmount, PaperGroup, IsDeletedTransaction, DeletedDate, DeletedBy, JobCloseSize, UpsLayout, SheetLayout, MachineType, CylinderToolID, CylinderToolCode, CylinderCircumferenceInch, CylinderCircumferenceMM, CylinderWidth, CylinderNoOfTeeth, FeedValue, AcrossGap, AroundGap, WastageStrip, RequiredRunningMeter, MakeReadyWastageRunningMeter, AvgBreakDownRunningMeter, WastageRunningMeter, TotalRequiredRunningMeter, RequiredSquareMeter, TotalRequiredSquareMeter,
                    WastageSquareMeter, ScrapSquareMeter, MachineSpeed, MachinePerHourRate, PaperTotalGSM, RequiredPaperWeightKg, RollChangeWastageMeter, AverageRollLength, RollType, TotalProcessCost, TotalMaterialCost, TotalMachineCost, WindingDirectionID, LabelType, OutputType, PcsPerRoll, PaperFaceGSM, PaperReleaseGSM, PaperAdhesiveGSM, PaperMill, DieType, CoreInnerDia, CoreOuterDia, RefProductMasterCode, ProductMasterID, ProductMasterContentsID, FinalQuantityInPcs, EstimationQuantityUnit, EstimationSvgSheetImage, EstimationSvgUpsImage, UnitPrice, CorrugationAmount, CorrugationQuantity, ProductionUnitID, ProcessWastageSheets, ProcessWastageRunningMeter, CostingHeadGridSettingStr, PlanOtherMaterialGSM, PlanOtherMaterialGSMSettingJSON, IsChangeQuantityReplanFlag, MaterialWetGSMConfigJSON, TotalContentActualWeight, SetupCost, ExecutionCost, MaterialCost, WastageCost, ToolCost, ProcessCost, ProcessWiseWastageType, LabourCost, ProcessWastagePercentage FROM JobBookingContents WHERE BookingId = " + bookingId;
            DBConnection.FillDataTable(ref dt, sql);
            var tblPlanning = ConvertDataTableToList(dt);

            dt = new DataTable();
            sql = @"SELECT ProcessID, RateFactor, Quantity, PlanID, PlanContQty, PlanContentType, PlanContName, Rate, TransId, IsDisplay, Ups, NoOfPass, Pieces, NoOfStitch, NoOfLoops, NoOfColors, SizeL, SizeW, Amount, Remarks, SequenceNo, IsDeletedTransaction, DeletedDate, DeletedBy, MachineID, MachineSpeed, MakeReadyTime, JobChangeOverTime, ExecutionTime, TotalExecutionTime, MachinePerHourCost, ProductionUnitID, PagesPerSection, NoOfForms, NoOfFolds, PerHourCalculationQuantity, MakeReadyMachineCost, SetupCost, ExecutionCost, MachineCost, MakeReadyPerHourCost, RollChangeOverTime, SetupTime, NoOfPasses FROM JobBookingProcess WHERE BookingId = " + bookingId;
            DBConnection.FillDataTable(ref dt, sql);
            var tblOperations = ConvertDataTableToList(dt);

            dt = new DataTable();
            sql = "SELECT TOP 0 * FROM JobBookingContentBookForms WHERE BookingId = " + bookingId;
            DBConnection.FillDataTable(ref dt, sql);
            var tblContentForms = new List<Dictionary<string, object>>();

            dt = new DataTable();
            sql = @"SELECT ProfitPercentage,ProfitCost,QuotedCost,PlanContQty, AnnualQuantity, TaxPercentage, MiscPercentage,DiscountPercentage, TotalCost, MiscCost, DiscountAmount, TaxAmount, GrandTotalCost, UnitCost, UnitCost1000, FinalCost, CurrencySymbol, ConversionValue, Field_Name, Field_Value, IsDeletedTransaction, DeletedDate, DeletedBy, ShipperCost, UnitCost100, ProductionUnitID, ProfitFactor, ProfitFactorCost, OverheadPercentage, ExcisePercentage, OverheadCost, ExciseAmount, FreightCost, InsuranceCost, ClearingCost, PackingCostPercentage, PackingCost, Cost1000Pcs, CreditDays, TotalWeightOfJob, FreightRate, CreditCost, CreditCost1000, FreightCost1000, BoardCost1000, OtherMaterialCost, ConversionCost FROM JobBookingCostings WHERE BookingId = " + bookingId;
            DBConnection.FillDataTable(ref dt, sql);
            var costingData = ConvertDataTableToList(dt);

            var objShippers = new Dictionary<string, object>();
            var arrObjAttc = new List<Dictionary<string, object>>();

            dt = new DataTable();
            sql = @"SELECT Headname, Amount, PlanContName, PlanContQty, PlanContentType, ProductionUnitID FROM JobBookingOnetimeCharges WHERE BookingId = " + bookingId;
            DBConnection.FillDataTable(ref dt, sql);
            var tblOnetimeCharges = ConvertDataTableToList(dt);

            dt = new DataTable();
            sql = @"SELECT PlanContQty, PlanContentType, PlanContName, MachineID, Deckle, Cutting, DeckleCuts, ReqDeckle, ReqCutting, PlyNo, FluteName, ItemID, ItemDetails, Weight, Rate, Amount, Waste, Width, TakeUpFactor, GSM, BF, BS, Sheets, IsDeletedTransaction, DeletedDate, DeletedBy, TotalWeightOfJob, CorrugationWeight, BoxWeight, ConversionPerKG, CalculationOn, TotalAmount, ConversionAmount, GrandTotal, ProductionUnitID FROM JobBookingCorrugation WHERE BookingId = " + bookingId;
            DBConnection.FillDataTable(ref dt, sql);
            var tblCorrugationPlyDetails = ConvertDataTableToList(dt);

            dt = new DataTable();
            sql = @"SELECT ProcessID, ItemID, TransID, PlanContName, PlanContentType, PlanContQty, RequiredQty, WasteQty, Rate, Amount, IsDeletedTransaction, DeletedDate, DeletedBy, MachineID, SequenceNo, ItemGroupID, ItemGroupNameID, RequiredQtyUnit, IsPlannedItem, BookedQtyInPurchaseUnit, PurchaseUnit, EstimatedQuantity, EstimationRate, EstimationUnit, EstimatedAmount, RequiredQuantityInStockUnit, ProductionUnitID, RequiredDryWtQtyInStockUnit, ItemSubGroupID FROM JobBookingProcessMaterialRequirement WHERE BookingId = " + bookingId;
            DBConnection.FillDataTable(ref dt, sql);
            var tblAllocatedMaterials = ConvertDataTableToList(dt);

            dt = new DataTable();
            sql = @"SELECT ProcessID, MachineID, ItemID, PlanContQty, PlanContentType, PlanContName, TransID, FieldName, FieldDescription, FieldDisplayName, ItemMasterFieldName, AppVariableName, CalculationFormula, DefaultValue, DisplaySequenceNo, DomainType, IsDisplayField, IsEditableField, FieldValue, IsLocked,DeletedBy, DeletedDate, IsDeletedTransaction,ProductionUnitID, MinimumValue, MaximumValue, ItemSubGroupID FROM JobBookingProcessMaterialParameterDetail WHERE BookingId = " + bookingId;
            DBConnection.FillDataTable(ref dt, sql);
            var tblMaterialCostParams = ConvertDataTableToList(dt);
            var tblAllocatedMaterialLayers = new List<Dictionary<string, object>>();

            dt = new DataTable();
            sql = @"SELECT PlanContQty, PlanContName, PlanContentType, SizeHeight, SizeLength, SizeWidth, SizeOpenflap, SizePastingflap, SizeBottomflap, JobNoOfPages,  JobUps, JobFlapHeight, JobTongHeight, JobFoldedH, JobFoldedL, PlanFColor, PlanBColor, PlanSpeFColor, PlanSpeBColor, PlanColorStrip, PlanGripper, PlanPrintingStyle, PlanWastageValue, Trimmingleft, Trimmingright,  Trimmingtop, Trimmingbottom, Stripingleft, Stripingright, Stripingtop, Stripingbottom, PlanPrintingGrain, ItemPlanQuality, ItemPlanGsm, ItemPlanThickness, ItemPlanMill, PlanPlateType, PlanWastageType, ItemPlanFinish,  OperId, JobBottomPerc, JobPrePlan, ChkPlanInSpecialSizePaper, ChkPlanInStandardSizePaper, MachineId, PlanOnlineCoating, PaperTrimleft, PaperTrimright, PaperTrimtop, PaperTrimbottom, ChkPaperByClient,  ChkPlanInAvailableStock, JobFoldInL, JobFoldInH, PlanPlateBearer, PlanStandardARGap, PlanStandardACGap, PlanContDomainType, Planlabeltype, Planwindingdirection, Planfinishedformat, Plandietype, PlanPcsPerRoll,  PlanCoreInnerDia, PlanCoreOuterDia, EstimationQuantityUnit,IsDeletedTransaction, DeletedDate, DeletedBy, SizeTopSeal, SizeSideSeal,  SizeBottomGusset, SizeCenterSeal, PlanMakeReadyWastage, ProductionUnitID, CategoryID, BookSpine, BookHinge, BookCoverTurnIn, BookExtension, BookLoops, PlanOtherMaterialGSM, PlanOtherMaterialGSMSettingJSON,  PlanPunchingType, ChkBackToBackPastingRequired, JobAcrossUps, JobAroundUps, SizeOpenflapPer, SizeBottomflapPer, SizeZipperLength, ZipperWeightPerMeter, JobSizeInputUnit, MaterialWetGSMConfigJSON, LedgerID,  ShowPlanUptoWastePercent, PlanSpoutType FROM JobBookingContentsSpecification WHERE BookingId = " + bookingId;
            DBConnection.FillDataTable(ref dt, sql);
            var tblContentSpecData = ConvertDataTableToList(dt);
            dt = new DataTable();
            sql = "SELECT BookingNo, MaxBookingNo FROM JobBooking WHERE BookingId = " + bookingId;
            DBConnection.FillDataTable(ref dt, sql);
            string bookingNoStr = "0";
            if (dt.Rows.Count > 0)
            {
                bookingNoStr = dt.Rows[0]["MaxBookingNo"]?.ToString() ?? "0";
            }

            // Helper to serialize only if data exists, otherwise return null
            Func<object, object> SerializeIfNotEmpty = (obj) =>
            {
                if (obj == null) return null;

                if (obj is Dictionary<string, object> dict && dict.Count == 0) return null;
                if (obj is List<Dictionary<string, object>> list && list.Count == 0) return null;

                return JsonConvert.SerializeObject(obj);
            };

            return new SaveQuotationRequest
            {
                TblBooking = SerializeIfNotEmpty(tblBooking),
                TblPlanning = SerializeIfNotEmpty(tblPlanning),
                TblOperations = SerializeIfNotEmpty(tblOperations),
                TblContentForms = SerializeIfNotEmpty(tblContentForms),
                CostingData = SerializeIfNotEmpty(costingData),
                FlagSave = "false",  // false = create revision of existing quotation (e.g., 16.1, 16.2)
                BookingNo = bookingNoStr,
                ObjShippers = SerializeIfNotEmpty(objShippers),
                ArrObjAttc = SerializeIfNotEmpty(arrObjAttc),
                Tblonetimecharges = SerializeIfNotEmpty(tblOnetimeCharges),
                TblCorrugationPlyDetails = SerializeIfNotEmpty(tblCorrugationPlyDetails),
                TblAllocatedMaterials = SerializeIfNotEmpty(tblAllocatedMaterials),
                TblMaterialCostParams = SerializeIfNotEmpty(tblMaterialCostParams),
                TblContentSpecData = SerializeIfNotEmpty(tblContentSpecData),
                TblAllocatedMaterialLayers = SerializeIfNotEmpty(tblAllocatedMaterialLayers)
            };
        }

        private List<Dictionary<string, object>> ConvertDataTableToList(DataTable dt)
        {
            var list = new List<Dictionary<string, object>>();
            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    dict[col.ColumnName] = GetDefaultValueForColumn(row[col], col.DataType);
                }
                list.Add(dict);
            }
            return list;
        }

        private Dictionary<string, object> ConvertDataTableToDictionary(DataTable dt)
        {
            if (dt.Rows.Count == 0) return null;
            var dict = new Dictionary<string, object>();
            DataRow row = dt.Rows[0];
            foreach (DataColumn col in dt.Columns)
            {
                dict[col.ColumnName] = GetDefaultValueForColumn(row[col], col.DataType);
            }
            return dict;
        }

        private object GetDefaultValueForColumn(object value, Type dataType)
        {
            Type underlyingType = Nullable.GetUnderlyingType(dataType) ?? dataType;

            // Handle null/DBNull values
            if (value == DBNull.Value || value == null)
            {
                if (underlyingType == typeof(int) || underlyingType == typeof(Int32) || underlyingType == typeof(Int16)
                    || underlyingType == typeof(Int64) || underlyingType == typeof(byte) || underlyingType == typeof(sbyte))
                    return 0;
                if (underlyingType == typeof(decimal) || underlyingType == typeof(double) || underlyingType == typeof(float))
                    return 0.0;
                if (underlyingType == typeof(bool))
                    return 0;
                if (underlyingType == typeof(DateTime))
                    return ""; // Empty string - InsertDatatableToDatabase will convert to NULL
                return "";
            }

            // Handle DateTime with value - format for SQL Server
            if (underlyingType == typeof(DateTime))
            {
                DateTime dt = (DateTime)value;
                return dt.ToString("yyyy-MM-dd HH:mm:ss");
            }

            return value;
        }

        private List<Dictionary<string, object>> ConvertDataTableToListExcludeIds(DataTable dt, string[] excludeColumns)
        {
            var list = new List<Dictionary<string, object>>();
            var excludeSet = new HashSet<string>(excludeColumns, StringComparer.OrdinalIgnoreCase);

            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    if (!excludeSet.Contains(col.ColumnName))
                    {
                        dict[col.ColumnName] = GetDefaultValueForColumn(row[col], col.DataType);
                    }
                }
                list.Add(dict);
            }
            return list;
        }

        [HttpGet]
        [Route("getuserlistforassignenquiry")]
        public IHttpActionResult GetUsersListForAssignEnquiry()
        {
            try
            {
                string str;
                GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
                DBType = Convert.ToString(HttpContext.Current.Session["DBType"]);

                str = "Select UM.UserID,UM.UserName from UserMaster as UM Inner JOIN UserModuleAuthentication as UA on UA.UserID  = UM.UserID Inner JOIN ModuleMaster as MM On MM.ModuleID = UA.ModuleID Where MM.ModuleName = '/estimation' And UA.CanView	= 1 And UA.CanSave = 1 And ISNULL(IsBlocked,0) = 0 And UM.CompanyID = " + GBLCompanyID;

                DBConnection.FillDataTable(ref dataTable, str);
                data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
                return Ok(JsonConvert.SerializeObject(data.Message));
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        // Calculate Pasting Flap and Open Flap values from DimensionConfig
        [HttpPost]
        [Route("calculate-flap-dimensions")]
        public IHttpActionResult CalculateFlapDimensions([FromBody] JObject jsonData)
        {
            try
            {
                DBConnection.LoadSession();

                decimal length = jsonData["length"]?.Value<decimal>() ?? 0;
                decimal width = jsonData["width"]?.Value<decimal>() ?? 0;
                decimal height = jsonData["height"]?.Value<decimal>() ?? 0;
                string contentType = jsonData["contentType"]?.ToString() ?? "";
                bool isCorrugated = jsonData["isCorrugated"]?.Value<bool>() ?? false;

                if (string.IsNullOrWhiteSpace(contentType))
                    return BadRequest("ContentType is required.");

                string valueColumn = isCorrugated ? "CorrugatedValue" : "DefaultValue";

                // Get all rows for this ContentType in one query
                DataTable dtAll = new DataTable();
                string allQuery = "SELECT FieldName, RangeFrom, RangeTo, " + valueColumn + ", FormulaExpression FROM DimensionConfig " +
                                  "WHERE ContentType = @ContentType AND CompanyID = " + DBConnection.GblCompanyId + " ORDER BY FieldName, RangeFrom";
                using (SqlConnection conn = DBConnection.OpenConnection())
                using (SqlCommand cmd = new SqlCommand(allQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@ContentType", contentType);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dtAll);
                    }
                }

                if (dtAll.Rows.Count == 0)
                    return Ok(new { success = false, message = "No configuration found for ContentType: " + contentType });

                var result = new JObject();
                // Dictionary to hold calculated values for formula variable substitution
                var calculatedValues = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
                {
                    { "SizeLength", length },
                    { "SizeWidth", width },
                    { "SizeHeight", height }
                };

                // Step 1: Process SizePastingflap first (matched against Height)
                decimal pastingFlapValue = 0;
                DataRow[] pastingRows = dtAll.Select("FieldName = 'SizePastingflap'");
                foreach (DataRow row in pastingRows)
                {
                    decimal rangeFrom = Convert.ToDecimal(row["RangeFrom"]);
                    decimal rangeTo = Convert.ToDecimal(row["RangeTo"]);
                    if (height >= rangeFrom && height <= rangeTo)
                    {
                        string formula = row["FormulaExpression"]?.ToString()?.Trim() ?? "";
                        if (!string.IsNullOrWhiteSpace(formula))
                        {
                            pastingFlapValue = EvaluateFormula(formula, calculatedValues);
                        }
                        else
                        {
                            pastingFlapValue = row[valueColumn] != DBNull.Value ? Convert.ToDecimal(row[valueColumn]) : 0;
                        }
                        break;
                    }
                }
                result["SizePastingflap"] = pastingFlapValue;
                calculatedValues["SizePastingflap"] = pastingFlapValue;

                // Step 2: Process SizeOpenflap (matched against 2L + 2W + PastingFlapValue)
                decimal openFlapSize = (2 * length) + (2 * width) + pastingFlapValue;
                decimal openFlapValue = 0;
                DataRow[] openRows = dtAll.Select("FieldName = 'SizeOpenflap'");
                foreach (DataRow row in openRows)
                {
                    decimal rangeFrom = Convert.ToDecimal(row["RangeFrom"]);
                    decimal rangeTo = Convert.ToDecimal(row["RangeTo"]);
                    if (openFlapSize >= rangeFrom && openFlapSize <= rangeTo)
                    {
                        string formula = row["FormulaExpression"]?.ToString()?.Trim() ?? "";
                        if (!string.IsNullOrWhiteSpace(formula))
                        {
                            openFlapValue = EvaluateFormula(formula, calculatedValues);
                        }
                        else
                        {
                            openFlapValue = row[valueColumn] != DBNull.Value ? Convert.ToDecimal(row[valueColumn]) : 0;
                        }
                        break;
                    }
                }
                result["SizeOpenflap"] = openFlapValue;
                calculatedValues["SizeOpenflap"] = openFlapValue;

                // Step 3: Process all remaining fields (formula-based fields like CrashLockWithPasting etc.)
                var processedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "SizePastingflap", "SizeOpenflap" };
                var distinctFields = dtAll.AsEnumerable()
                    .Select(r => r["FieldName"].ToString())
                    .Distinct(StringComparer.OrdinalIgnoreCase);

                foreach (string fieldName in distinctFields)
                {
                    if (processedFields.Contains(fieldName))
                        continue;

                    DataRow[] fieldRows = dtAll.Select("FieldName = '" + fieldName.Replace("'", "''") + "'");
                    decimal fieldValue = 0;
                    bool matched = false;

                    foreach (DataRow row in fieldRows)
                    {
                        string formula = row["FormulaExpression"]?.ToString()?.Trim() ?? "";
                        decimal rangeFrom = Convert.ToDecimal(row["RangeFrom"]);
                        decimal rangeTo = Convert.ToDecimal(row["RangeTo"]);

                        // Formula-based field (RangeFrom and RangeTo are 0)
                        if (!string.IsNullOrWhiteSpace(formula))
                        {
                            fieldValue = EvaluateFormula(formula, calculatedValues);
                            matched = true;
                            break;
                        }

                        // Range-based field: match against openFlapSize
                        if (openFlapSize >= rangeFrom && openFlapSize < rangeTo)
                        {
                            fieldValue = row[valueColumn] != DBNull.Value ? Convert.ToDecimal(row[valueColumn]) : 0;
                            matched = true;
                            break;
                        }
                    }

                    result[fieldName] = matched ? fieldValue : 0;
                    calculatedValues[fieldName] = matched ? fieldValue : 0;
                }

                return Ok(new
                {
                    success = true,
                    contentType,
                    isCorrugated,
                    dimensions = new { length, width, height },
                    values = result
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpGet]
        [Route("freight/states")]
        public IHttpActionResult GetStates()
        {

            GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
            GBLProductionUnitID = Convert.ToString(HttpContext.Current.Session["ProductionUnitID"]);

            var str = @"SELECT DISTINCT State FROM FreightRateMaster WHERE CompanyID = " + GBLCompanyID + " AND ProductionUnitID = " + GBLProductionUnitID + " AND IsDeletedTransaction = 0 ORDER BY State";
            DBConnection.FillDataTable(ref dataTable, str);
            data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
            return Ok(data.Message);

        }
        [HttpGet]
        [Route("freight/locations")]
        public IHttpActionResult GetLocations(string stateName)
        {
            GBLCompanyID = Convert.ToString(HttpContext.Current.Session["CompanyID"]);
            GBLProductionUnitID = Convert.ToString(HttpContext.Current.Session["ProductionUnitID"]);
            str = @"SELECT DISTINCT Location,FreightRate FROM FreightRateMaster WHERE CompanyID = " + GBLCompanyID + " AND ProductionUnitID = " + GBLProductionUnitID + " AND State = '"+ stateName + "' AND IsDeletedTransaction = 0 ORDER BY Location";
            DBConnection.FillDataTable(ref dataTable, str);
            data.Message = DBConnection.ConvertDataTableToJsonString(dataTable);
            return Ok(data.Message);
        }


        private decimal EvaluateFormula(string formula, Dictionary<string, decimal> variables)
        {
            // If formula is just a plain number, return it directly
            if (decimal.TryParse(formula, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out decimal plainNumber))
                return plainNumber;

            // Replace variable names with their values
            string expression = formula;
            foreach (var kvp in variables.OrderByDescending(k => k.Key.Length))
            {
                expression = expression.Replace(kvp.Key, kvp.Value.ToString(CultureInfo.InvariantCulture));
            }

            // Evaluate the math expression using DataTable.Compute
            DataTable dt = new DataTable();
            object computed = dt.Compute(expression, "");
            return Convert.ToDecimal(computed);
        }

    }
    public class HelloWorldData
    {
        public string Message { get; set; }
    }
    public class SaveQuotationRequest
    {
        public object TblBooking { get; set; }

        public object TblPlanning { get; set; }

        public object TblOperations { get; set; }

        public object TblContentForms { get; set; }

        public object CostingData { get; set; }

        [Required]
        public string FlagSave { get; set; }

        public string BookingNo { get; set; }

        public object ObjShippers { get; set; }

        public object ArrObjAttc { get; set; }

        public object Tblonetimecharges { get; set; }

        public object TblCorrugationPlyDetails { get; set; }

        public object TblAllocatedMaterials { get; set; }

        public object TblMaterialCostParams { get; set; }

        public object TblContentSpecData { get; set; }

        public object TblAllocatedMaterialLayers { get; set; }
    }
}