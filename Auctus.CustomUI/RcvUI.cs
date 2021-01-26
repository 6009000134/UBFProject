using Auctus.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using UFIDA.U9.CBO.PubBE.YYC;
using UFIDA.U9.PM.Enums;
using UFIDA.U9.PM.Rcv;
using UFIDA.U9.UI.PDHelper;
using UFSoft.UBF.Sys.Database;
using UFSoft.UBF.UI;
using UFSoft.UBF.UI.ActionProcess;
using UFSoft.UBF.UI.ControlModel;
using UFSoft.UBF.UI.IView;
using UFSoft.UBF.UI.MD.Runtime;
using UFSoft.UBF.UI.WebControlAdapter;
using UFSoft.UBF.Util.Cache;
using UFSoft.UBF.Util.DataAccess;
using UFSoft.UBF.Util.Log;

namespace Auctus.CustomUI
{
    public class RcvUI : UFSoft.UBF.UI.Custom.ExtendedPartBase
    {
        private IPart uiPart;

        //private UFIDA.U9.SCM.PM.ReceivementUIModel.ReceivementMainUIFormWebPart uiPart;
        // Token: 0x0600000E RID: 14 RVA: 0x00002804 File Offset: 0x00000A04
        public override void AfterEventProcess(IPart Part, string eventName, object sender, EventArgs args)
        {
            base.AfterEventProcess(Part, eventName, sender, args);
            this.uiPart = Part;
            UFWebButton4ToolbarAdapter adapter = sender as UFWebButton4ToolbarAdapter;
            IUIRecord porec = this.uiPart.Model.Views["Receivement"].FocusedRecord;
            bool flag = adapter != null && adapter.Action == "UnDoApproveClick";
            if (flag)
            {
                ILogger logger = LoggerManager.GetLogger(typeof(CacheManager));
                logger.Error(string.Format("单号为：{0}的采购收货单弃审了!", porec["DocNo"].ToString()), new object[0]);
                string UpSQL = string.Format("UPDATE PM_Receivement set DescFlexField_PrivateDescSeg4='是' WHERE ID = {0}", porec["ID"].ToString());
                DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), UpSQL, null);
                ((BaseAction)this.uiPart.Action).NavigateAction.Refresh(null);
            }
        }

        public override void AfterInit(IPart Part, EventArgs e)
        {
            base.AfterInit(Part, e);
            uiPart = Part;
            //OA流程提交按钮
            IUFButton btnCustSubmit = new UFWebButtonAdapter();
            btnCustSubmit.Text = "行提交";
            btnCustSubmit.ToolTip = "行提交";
            btnCustSubmit.ID = "btnCustSubmit";
            btnCustSubmit.AutoPostBack = true;
            btnCustSubmit.Visible = true;

            //OA流程提交按钮
            IUFButton btnOAFlows = new UFWebButtonAdapter();
            btnOAFlows.Text = "查看流程";
            btnOAFlows.ToolTip = "查看流程";
            btnOAFlows.ID = "btnOAFlows";
            btnOAFlows.AutoPostBack = true;
            btnOAFlows.Visible = true;

            //OA弃审按钮
            IUFButton btnUnApprove = new UFWebButtonAdapter();
            btnUnApprove.Text = "行弃审";
            btnUnApprove.ToolTip = "行弃审";
            btnUnApprove.ID = "btnUnApprove";
            btnUnApprove.AutoPostBack = true;
            btnUnApprove.Visible = true;

            //批号导入
            IUFButton btnLotExcelInput = new UFWebButtonAdapter();
            btnLotExcelInput.Text = "批号导入";
            btnLotExcelInput.ToolTip = "批号导入";
            btnLotExcelInput.ID = "btnLotExcelInput";
            btnLotExcelInput.AutoPostBack = true;
            btnLotExcelInput.Visible = true;


            IUFCard card = (IUFCard)Part.GetUFControlByName(Part.TopLevelContainer, "Card0");

            card.Controls.Add(btnLotExcelInput);
            CommonFunction.Layout(card, btnLotExcelInput, 16, 0);
            btnLotExcelInput.Click += this.BtntnLotExcelInput_Click;

            card.Controls.Add(btnCustSubmit);
            CommonFunction.Layout(card, btnCustSubmit, 20, 0);
            btnCustSubmit.Click += btnCustSubmit_Click;

            card.Controls.Add(btnUnApprove);
            CommonFunction.Layout(card, btnUnApprove, 14, 0);
            btnUnApprove.Click += btnUnApprove_Click;

            card.Controls.Add(btnOAFlows);
            CommonFunction.Layout(card, btnOAFlows, 18, 0);
            btnOAFlows.Click += btnOAFlows_Click;

            bool flag = this.uiPart.CurrentSessionState["DataTable_LotExcel"] != null;
            if (flag)
            {
                DataTable table = new DataTable();
                table = (DataTable)this.uiPart.CurrentSessionState["DataTable_LotExcel"];
                bool flag2 = table != null && table.Rows.Count > 0;
                if (flag2)
                {
                    this.SetGridViewLotValue(this.uiPart, table);
                }
            }
        }
        /// <summary>
        /// 设置批号值
        /// </summary>
        /// <param name="Part"></param>
        /// <param name="p_dtb"></param>
        private void SetGridViewLotValue(IPart Part, DataTable p_dtb)
        {
            bool flag = this.uiPart.Model.Views[3].RecordCount != 0;
            if (flag)
            {
                foreach (IUIRecord record in this.uiPart.Model.Views[3].Records)
                {
                    bool flag2 = !string.IsNullOrEmpty(record["ItemInfo_ItemID_InventoryInfo_LotParam"].ToString());
                    if (flag2)
                    {
                        foreach (object obj in p_dtb.Rows)
                        {
                            DataRow row = (DataRow)obj;
                            bool flag3 = string.IsNullOrEmpty(row[1].ToString());
                            if (flag3)
                            {
                                throw new Exception("导入模板中批号对应的料号不能为空，请检查！");
                            }
                            bool flag4 = string.IsNullOrEmpty(row[2].ToString());
                            if (flag4)
                            {
                                throw new Exception("导入模板中料号对应的批号不能为空，请检查！");
                            }
                            bool flag5 = row[1].ToString() == record["ItemInfo_ItemCode"].ToString() && record["DocLineNo"].ToString().Trim().Contains(row[0].ToString().Trim() + "0");
                            if (flag5)
                            {
                                record["RcvLotCode"] = row[2].ToString();
                                record["InvLotCode"] = row[2].ToString();
                            }
                        }
                    }
                }
            }
        }


        public override void AfterRender(IPart Part, EventArgs args)
        {
            base.AfterRender(Part, args);
            uiPart = Part;
            IUFToolbar bar = (IUFToolbar)Part.GetUFControlByName(Part.TopLevelContainer, "Toolbar1");
            IUIRecord headRec = uiPart.Model.Views["Receivement"].FocusedRecord;
            IUFControl ctrl1 = CommonFunction.FindControl(Part, "Card0", "btnCustSubmit");
            IUFControl ctrl2 = CommonFunction.FindControl(Part, "Card0", "btnUnApprove");
            IUFControl ctrl3 = CommonFunction.FindControl(Part, "Card0", "btnOAFlows");

            if (headRec != null && Int64.Parse(headRec["ID"].ToString()) > 0)
            {
                DataSet ds = new DataSet();
                string sql = string.Format(@" SELECT a.IsLineApprove,b.DescFlexField_PrivateDescSeg1 IsToOA FROM dbo.PM_Receivement a INNER JOIN dbo.PM_RcvDocType b ON a.RcvDocType=b.ID
WHERE a.ID = {0} ", headRec["ID"].ToString());
                DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), sql, null, out ds);
                string IsToOA = "";
                bool IsLineApprove = false;
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    IsToOA = row["IsToOA"].ToString();
                    if (row["IsLineApprove"].ToString().ToLower()=="true")
                    {
                        IsLineApprove = true;
                    }
                }
                if (IsToOA == "1")//触发到OA，且是行审
                {
                    ctrl3.Enabled = true;//查看OA流程按钮
                    if (IsLineApprove)//后端数据库是否行审
                    {
                        //前端也要勾选上是否行审才能显示“行提交”按钮
                        if (headRec["IsLineApprove"].ToString().ToLower() == "true")
                        {
                            ctrl1.Enabled = true;
                            ctrl2.Enabled = true;
                        }
                        else
                        {
                            ctrl1.Enabled = false;
                            ctrl2.Enabled = false;
                        }
                        foreach (IUFControl ctrl in bar.Controls)
                        {
                            switch (ctrl.ID)
                            {
                                case "BtnSubmit":
                                    ctrl.Enabled = false;
                                    break;
                                case "BtnRevocate":
                                    ctrl.Enabled = false;
                                    break;
                                case "BtnApprove":
                                    ctrl.Enabled = false;
                                    break;
                                case "BtnUndoApprove":
                                    ctrl.Enabled = false;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    else
                    {
                        ctrl1.Enabled = false;
                        ctrl2.Enabled = false;
                        foreach (IUFControl ctrl in bar.Controls)
                        {
                            switch (ctrl.ID)
                            {
                                case "BtnRevocate":
                                    ctrl.Enabled = false;
                                    break;
                                case "BtnApprove":
                                    ctrl.Enabled = false;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    ctrl1.Enabled = false;
                    ctrl2.Enabled = false;

                }
            }
        }
        /// <summary>
        /// 保存当前页面数据，才能获取到选中行SelectRecords
        /// </summary>
        public void SaveData()
        {
            this.uiPart.Model.ClearErrorMessage();
            this.uiPart.DataCollect();
            this.uiPart.IsDataBinding = true;
            this.uiPart.IsConsuming = false;
            //this.uiPart            
        }
        /// <summary>
        /// 批号导入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void BtntnLotExcelInput_Click(object sender, EventArgs e)
        {
            this.uiPart.Model.ClearErrorMessage();
            this.uiPart.DataCollect();
            NameValueCollection values = new NameValueCollection();
            this.uiPart.CurrentSessionState["ExcelSheetName"] = "批号导入";
            this.uiPart.ShowModalDialog("A7D3E602-8C35-43B4-8528-B9F585464FD7", "批号导入", "656", "112", string.Empty, values, true, true, false);
        }
        /// <summary>
        /// 查看流程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnOAFlows_Click(object sender, EventArgs e)
        {
            SaveData();
            IUIRecord headRec = uiPart.Model.Views["Receivement"].FocusedRecord;
            if (headRec == null)
                return;

            bool IsLineApprove = headRec["IsLineApprove"].ToString().ToLower() == "true";

            string OAFlowID = "";
            if (IsLineApprove)//行审查看流程
            {
                IUIRecordCollection ius = uiPart.Model.Views["Receivement_RcvLines"].SelectRecords;
                IEnumerator enumerator;
                string lineIDs = "";
                if (ius.Count > 0)//获取选中行ID集合
                {
                    enumerator = ius.GetEnumerator();
                }
                else//未选行，视为整单提交
                {
                    enumerator = uiPart.Model.Views["Receivement_RcvLines"].Records.GetEnumerator();
                }

                while (enumerator.MoveNext())
                {
                    IUIRecord i = (IUIRecord)enumerator.Current;
                    lineIDs += i["ID"].ToString() + ",";
                }

                //查询OA的流程ID
                string sql = string.Format(@"SELECT DescFlexSegments_PrivateDescSeg5 AS OAFlowID
                        FROM dbo.PM_RcvLine WHERE ID in ({0})", lineIDs.Substring(0, lineIDs.Length - 1));
                DataSet ds = new DataSet();
                DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), sql, null, out ds);
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    OAFlowID = row["OAFlowID"].ToString();
                    if (!string.IsNullOrEmpty(OAFlowID))
                    {
                        break;
                    }
                }
                if (string.IsNullOrEmpty(OAFlowID))
                {
                    SetErrorMsg("选中的行没有OA流程！");
                    return;
                }
            }
            else
            {
                //查询OA的流程ID
                string sql = string.Format(@"SELECT DescFlexField_PrivateDescSeg5 AS OAFlowID
                        FROM dbo.PM_Receivement WHERE ID in ({0})", headRec["ID"].ToString());
                DataSet ds = new DataSet();
                DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), sql, null, out ds);
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    OAFlowID = row["OAFlowID"].ToString();
                }
            }
            string script = PubFunction.GetOAFlowScript(PDContext.Current.UserCode, OAFlowID);//打开流程页面脚本
            AtlasHelper.RegisterAtlasStartupScript
             ((Control)this.uiPart.TopLevelContainer, this.uiPart.GetType(), "ReferenceReturn", script, false);

        }
        /// <summary>
        /// 提交按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnCustSubmit_Click(object sender, EventArgs e)
        {
            SaveData();
            try
            {
                //UFIDA.U9.SCM.PM.ReceivementUIModel.ReceivementMainUIFormWebPart uiPart2 = (UFIDA.U9.SCM.PM.ReceivementUIModel.ReceivementMainUIFormWebPart)uiPart;                
                UFIDA.U9.SCM.PM.ReceivementUIModel.ReceivementRecord rcvRec = (UFIDA.U9.SCM.PM.ReceivementUIModel.ReceivementRecord)uiPart.Model.Views["Receivement"].FocusedRecord;

                if (rcvRec == null)
                {
                    SetErrorMsg("请选择收货单");
                    return;
                }

                if (PDContext.Current.UserName != rcvRec.CreatedBy.ToString())
                {
                    SetErrorMsg("OA审批的单据只允许建单人进行提交");
                    return;
                }

                DataSet ds2 = new DataSet();
                string sql = string.Format(@" SELECT a.IsLineApprove,b.DescFlexField_PrivateDescSeg1 IsToOA FROM dbo.PM_Receivement a INNER JOIN dbo.PM_RcvDocType b ON a.RcvDocType=b.ID
WHERE a.ID = {0} ", rcvRec["ID"].ToString());
                DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), sql, null, out ds2);
                foreach (DataRow row in ds2.Tables[0].Rows)
                {
                    if (row["IsLineApprove"].ToString().ToLower() == "false")
                    {
                        SetErrorMsg("请勾选上表头的'是否行审'后再选择行提交！");
                        return;
                    }
                }

                string rcvID = rcvRec.ID.ToString();
                string lineIDs = "";

                IEnumerator enumerator;
                IUIRecordCollection ius = uiPart.Model.Views["Receivement_RcvLines"].SelectRecords;
                bool IsWholeOrderSubmit = rcvRec["IsLineApprove"].ToString().ToLower() == "false";
                if (ius.Count > 0 && !IsWholeOrderSubmit)//获取选中行ID集合
                {
                    enumerator = ius.GetEnumerator();
                }
                else//未选行，视为整单提交
                {
                    SetErrorMsg("请选择要提交的行！");
                    return;
                    //enumerator = uiPart.Model.Views["Receivement_RcvLines"].Records.GetEnumerator();
                }

                while (enumerator.MoveNext())
                {
                    IUIRecord i = (IUIRecord)enumerator.Current;
                    lineIDs += i["ID"].ToString() + ",";
                }

                string sqlData = string.Format(@"SELECT 
a.ID,a.DocNo,a.CreatedBy
,a.TotalMnyTC--实收税价合计
,b.ConfirmDate
,b.RcvDept,dept1.Name RcvDeptName,b.WhMan,op1.Name WhMan_Name, b.ArrivedTime,b.IsPriceIncludeTax,a.TC,bz.Code TCCode
,dbo.F_GetEnumName('UFIDA.U9.PM.Enums.RcvSrcDocTypeEnum', a.SrcDocType, 'zh-cn')SrcDocType
,a.TaxSchedule
,a.IsLineApprove
,t1.Name TaxScheduleName, a.Supplier_Supplier,s1.Name Supplier
 , b.Wh,w.Code WhCode,w1.Name Wh_Name
  , a.Org
,a.RcvDocType,doctype.Code RcvDocTypeCode,doctype.DescFlexField_PrivateDescSeg1 IsToOA
,b.SumDispenseQtyAU--分发数量
,mrp.Code MRPCode, mrp.Name MRPName
 , m.Code,m.Name,m.SPECS
,b.ArriveQtyTU--世道
,b.EyeballingQtyTU--点收
,b.RcvQtyTU--实收
,b.RtnFillQtyTU--退补
,b.RtnDeductQtyTU--退扣
,b.RcvLot,b.RcvLotCode--Lot_LotMaster
,b.SrcDoc_SrcDocNo,b.SrcDoc_SrcDocLineNo,b.SrcDoc_SrcDocSubLineNo
,b.FinallyPriceTC
,b.ArriveTotalNetMnyTC
,b.ArriveTotalTaxTC
,b.ArriveTotalMnyTC
,b.PlanArrivedDate
,b.DescFlexSegments_PrivateDescSeg1--检验员
,b.DescFlexSegments_PrivateDescSeg2--处理意见
,b.DescFlexSegments_PrivateDescSeg3--判退原因
,b.DescFlexSegments_PrivateDescSeg4 IsRefuse--是否驳回
,b.DescFlexSegments_PrivateDescSeg5 OAFlowID--OA流程ID
,b.DocLineNo
,b.ID LineID
,b.Status
FROM dbo.PM_Receivement a INNER JOIN dbo.PM_RcvLine b ON a.ID = b.Receivement
LEFT JOIN dbo.CBO_TaxSchedule t ON a.TaxSchedule = t.ID LEFT JOIN dbo.CBO_TaxSchedule_Trl t1 ON t.ID = t1.ID AND ISNULL(t1.SysMLFlag, 'zh-cn')= 'zh-cn'
LEFT JOIN dbo.CBO_Supplier s ON a.Supplier_Supplier = s.ID LEFT JOIN dbo.CBO_Supplier_Trl s1 ON s.ID = s1.ID AND ISNULL(s1.SysMLFlag, 'zh-cn')= 'zh-cn'
LEFT JOIN dbo.CBO_Wh w ON b.Wh = w.ID LEFT JOIN dbo.CBO_Wh_Trl w1 ON w.ID = w1.ID AND ISNULL(w1.SysMLFlag, 'zh-cn')= 'zh-cn'
LEFT JOIN dbo.CBO_Operators op ON b.WhMan = op.ID  LEFT JOIN dbo.CBO_Operators_Trl op1 ON op.ID = op1.ID  AND ISNULL(op1.SysMLFlag, 'zh-cn')= 'zh-cn'
LEFT JOIN dbo.CBO_ItemMaster m ON b.ItemInfo_ItemID = m.ID LEFT JOIN dbo.vw_MRPCategory mrp ON m.DescFlexField_PrivateDescSeg22 = mrp.Code
LEFT JOIN dbo.CBO_Department dept ON b.RcvDept=dept.ID LEFT JOIN dbo.CBO_Department_Trl dept1 ON dept.ID=dept1.ID AND ISNULL(dept1.SysMLFlag,'zh-cn')='zh-cn'
LEFT JOIN dbo.PM_RcvDocType doctype ON a.RcvDocType=doctype.ID
LEFT JOIN dbo.Base_Currency bz ON a.TC=bz.ID
WHERE 1 = 1
AND a.ID = {0} AND b.ID IN ({1})", rcvID, lineIDs.Substring(0, lineIDs.Length - 1));
                DataSet ds = new DataSet();
                DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), sqlData, null, out ds);

                string errorInfo = "";//错误信息

                ////判断行是否都是“开立”状态
                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow item in ds.Tables[0].Rows)
                        {
                            if (item["Status"].ToString() != "0")//非开立状态
                            {
                                errorInfo += item["DocLineNo"].ToString() + ",";
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(errorInfo))//错误信息抛出
                {
                    SetErrorMsg(errorInfo.Substring(0, errorInfo.Length) + "行不是开立状态，无法提交");
                    return;
                }
                else
                {
                    Dictionary<string, object> dicResult = new Dictionary<string, object>();
                    Dictionary<string, object> dicMain = new Dictionary<string, object>();
                    List<Dictionary<string, object>> liDicDt1 = new List<Dictionary<string, object>>();
                    if (ds.Tables.Count > 0)
                    {
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            //流程提交到OA
                            if (ds.Tables[0].Rows[0]["IsToOA"].ToString() == "1")
                            {
                                //TODO:两行是同一个单退回，再次提交只提交了其中一行，需要将另一行的退回标识干掉
                                //当有两行是被不同OA流程退回的，需要分开提交。因为退回的单据提交是做更新操作

                                bool clearIsRefuse = false;//当有多行是同一个流程退回的，且此次这几行没有一起提交，那么需要将未提交行的是否退回标识清楚，否则下次提交行会更新OA流程，将此次提交流程数据替换掉
                                string refuseOAFlowids = "";
                                DataRow[] drs = ds.Tables[0].Select("IsRefuse='是'");
                                if (drs.Count() > 1)
                                {
                                    string oaid = drs[0]["OAFlowID"].ToString();
                                    foreach (DataRow item in drs)
                                    {
                                        if (oaid != item["OAFlowID"].ToString())
                                        {
                                            clearIsRefuse = true;
                                            break;
                                            //SetErrorMsg("超过两行是被不同的OA流程退回的，请分开提交！");
                                            //return;
                                        }
                                    }
                                }
                                //有行是被退回的，给OA上下文requestid赋值OAFlowID，其它非退回的行会这行一起提交到被退回的OA流程，做更新操作
                                if (drs.Count() > 0)
                                {
                                    foreach (DataRow item in drs)
                                    {
                                        refuseOAFlowids += item["OAFlowID"].ToString() + ",";
                                    }
                                    refuseOAFlowids = refuseOAFlowids.Substring(0, refuseOAFlowids.Length - 1);
                                    if (IsWholeOrderSubmit)
                                    {
                                        //OA上下文信息
                                        dicResult.Add("base", Auctus.Common.Utils.GenerateOABaseInfo(PDContext.Current.UserCode, PubFunction.GetOAInfoByCode("07"), rcvID, 1, drs[0]["OAFlowID"].ToString(), 1, "采购收货单审批-订单号" + ds.Tables[0].Rows[0]["DocNo"].ToString() + "(整单" + ds.Tables[0].Rows.Count.ToString() + "行)"));
                                        //dicResult.Add("base", Auctus.Common.Utils.GenerateOABaseInfo(PDContext.Current.UserCode, PubFunction.GetOAInfoByCode("07"), rcvID, 1, drs[0]["OAFlowID"].ToString(), 1, "采购收货单" + ds.Tables[0].Rows[0]["DocNo"].ToString() + "(整单)"));
                                    }
                                    else
                                    {
                                        //OA上下文信息
                                        dicResult.Add("base", Auctus.Common.Utils.GenerateOABaseInfo(PDContext.Current.UserCode, PubFunction.GetOAInfoByCode("07"), rcvID, 1, drs[0]["OAFlowID"].ToString(), 1, "采购收货单审批-订单号" + ds.Tables[0].Rows[0]["DocNo"].ToString() + "(" + ds.Tables[0].Rows.Count.ToString() + "行)"));
                                    }

                                }
                                else
                                {
                                    if (IsWholeOrderSubmit)
                                    {
                                        //OA上下文信息
                                        dicResult.Add("base", Auctus.Common.Utils.GenerateOABaseInfo(PDContext.Current.UserCode, PubFunction.GetOAInfoByCode("07"), rcvID, 1, "", 1, "采购收货单审批-订单号" + ds.Tables[0].Rows[0]["DocNo"].ToString() + "(整单" + ds.Tables[0].Rows.Count.ToString() + "行)"));
                                        //dicResult.Add("base", Auctus.Common.Utils.GenerateOABaseInfo(PDContext.Current.UserCode, PubFunction.GetOAInfoByCode("07"), rcvID, 1, "", 1, "采购收货单" + ds.Tables[0].Rows[0]["DocNo"].ToString() + "(整单"));
                                    }
                                    else
                                    {
                                        //OA上下文信息
                                        dicResult.Add("base", Auctus.Common.Utils.GenerateOABaseInfo(PDContext.Current.UserCode, PubFunction.GetOAInfoByCode("07"), rcvID, 1, "", 1, "采购收货单审批-订单号" + ds.Tables[0].Rows[0]["DocNo"].ToString() + "(" + ds.Tables[0].Rows.Count.ToString() + "行)"));
                                    }

                                }

                                #region main
                                //设置申请人信息
                                dicMain = Auctus.Common.Utils.GenerateOAUserInfo(dicMain, PDContext.Current.UserCode, DateTime.Now.ToString("yyyy-MM-dd"));
                                //设置主表其他信息                
                                Dictionary<string, object> dicTemp = new Dictionary<string, object>();

                                dicTemp = new Dictionary<string, object>();
                                dicTemp.Add("value", ds.Tables[0].Rows[0]["RcvDeptName"]);
                                dicMain.Add("shbm", dicTemp);
                                dicTemp = new Dictionary<string, object>();
                                dicTemp.Add("value", ds.Tables[0].Rows[0]["ID"]);
                                dicMain.Add("fpkid", dicTemp);
                                if (ds.Tables[0].Rows[0]["ConfirmDate"] != DBNull.Value)
                                {
                                    dicTemp = new Dictionary<string, object>();
                                    dicTemp.Add("value", ((DateTime)ds.Tables[0].Rows[0]["ConfirmDate"]).ToString("yyyy-MM-dd"));
                                    dicMain.Add("rkqrsj", dicTemp);
                                }
                                else
                                {
                                    dicMain.Add("rkqrsj", null);
                                }



                                dicTemp = new Dictionary<string, object>();
                                dicTemp.Add("value", ds.Tables[0].Rows[0]["WhMan_Name"]);
                                dicMain.Add("kgy", dicTemp);
                                dicTemp = new Dictionary<string, object>();
                                dicTemp.Add("value", ds.Tables[0].Rows[0]["TotalMnyTC"]);
                                dicMain.Add("zje", dicTemp);
                                dicTemp = new Dictionary<string, object>();
                                dicTemp.Add("value", ((DateTime)ds.Tables[0].Rows[0]["ArrivedTime"]).ToString("yyyy-MM-dd"));
                                dicMain.Add("dhsj", dicTemp);
                                dicTemp = new Dictionary<string, object>();
                                dicTemp.Add("value", ds.Tables[0].Rows[0]["IsPriceIncludeTax"].ToString() == "True" ? "1" : "0");
                                dicMain.Add("sfhs", dicTemp);
                                dicTemp = new Dictionary<string, object>();
                                dicTemp.Add("value", ds.Tables[0].Rows[0]["DocNo"]);
                                dicMain.Add("dh", dicTemp);
                                dicTemp = new Dictionary<string, object>();
                                dicTemp.Add("value", ds.Tables[0].Rows[0]["TCCode"].ToString());
                                dicMain.Add("bz", dicTemp);
                                dicTemp = new Dictionary<string, object>();
                                dicTemp.Add("value", ds.Tables[0].Rows[0]["SrcDocType"]);
                                dicMain.Add("lylb", dicTemp);
                                dicTemp = new Dictionary<string, object>();
                                dicTemp.Add("value", ds.Tables[0].Rows[0]["TaxScheduleName"]);
                                dicMain.Add("szh", dicTemp);
                                dicTemp = new Dictionary<string, object>();
                                dicTemp.Add("value", ds.Tables[0].Rows[0]["Supplier"]);
                                dicMain.Add("gys", dicTemp);
                                dicTemp = new Dictionary<string, object>();
                                dicTemp.Add("value", ds.Tables[0].Rows[0]["Wh_Name"]);
                                dicMain.Add("ccdd", dicTemp);

                                dicTemp = new Dictionary<string, object>();
                                dicTemp.Add("value", ds.Tables[0].Rows[0]["Org"].ToString());
                                dicMain.Add("u9zz", dicTemp);
                                dicTemp = new Dictionary<string, object>();
                                dicTemp.Add("value", ds.Tables[0].Rows[0]["RcvDocTypeCode"].ToString());
                                dicMain.Add("djlx", dicTemp);
                                dicMain.Add("lcbh", null);//流程编号
                                dicMain.Add("fjsc", null);//附件上传
                                dicMain.Add("lydj", null);//来源单据
                                dicMain.Add("mrplx", null);//MRP分类
                                dicMain.Add("sftqjl", null);//是否提前进料
                                dicMain.Add("tqshbs", null);//提前收货标识
                                dicMain.Add("tqshfhxx", null);//提前收货返回信息
                                                              //sfffwl 是否分发物料
                                dicTemp = new Dictionary<string, object>();
                                dicTemp.Add("value", "1");
                                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                                {
                                    if (Convert.ToInt32(ds.Tables[0].Rows[i]["SumDispenseQtyAU"]) > 0)
                                    {
                                        dicTemp["value"] = "0";
                                        break;
                                    }
                                }
                                dicMain.Add("sfffwl", dicTemp);
                                dicTemp = new Dictionary<string, object>();
                                if (ds.Tables[0].Rows[0]["IsLineApprove"].ToString().ToLower() == "true")
                                {
                                    dicTemp.Add("value", "1");
                                }
                                else
                                {
                                    dicTemp.Add("value", "0");
                                }
                                dicMain.Add("sfxs", dicTemp);


                                #endregion
                                //行提交接口
                                UFIDA.U9.PM.Rcv.Proxy.RcvLineListSubmitBPProxy lineSubmitProx = new UFIDA.U9.PM.Rcv.Proxy.RcvLineListSubmitBPProxy();
                                UFIDA.U9.PM.Rcv.RcvAutoCreateSnAndLot s = new RcvAutoCreateSnAndLot();

                                lineSubmitProx.RcvLineKeys = new List<long>();

                                #region Dt1 行信息
                                bool IsOAFlowSame = true;//整单提交的所有行流程ID是否一致，若不一致，只提交收货行中的关联流程
                                string firstOAFlowID = "";
                                //行信息
                                foreach (DataRow item in ds.Tables[0].Rows)
                                {
                                    if (item["WhMan"] == DBNull.Value || string.IsNullOrEmpty(item["WhMan"].ToString()))
                                    {
                                        SetErrorMsg("库管员不能为空！");
                                        return;
                                    }
                                    if (firstOAFlowID == "" && item["OAFlowID"].ToString() != "")
                                    {
                                        firstOAFlowID = item["OAFlowID"].ToString();
                                    }
                                    if (IsOAFlowSame && IsWholeOrderSubmit && item["OAFlowID"].ToString() != ds.Tables[0].Rows[0]["OAFlowID"].ToString() && item["OAFlowID"].ToString() != "")
                                    {
                                        IsOAFlowSame = false;
                                    }
                                    lineSubmitProx.RcvLineKeys.Add(Convert.ToInt64(item["LineID"]));
                                    Dictionary<string, object> dicDt1 = new Dictionary<string, object>();
                                    //有OA流程ID，且不是退回的，说明是弃审的，需要把OAFlowID传到行的关联流程
                                    if (item["IsRefuse"].ToString() == "" && item["OAFlowID"].ToString() != "")
                                    {
                                        dicTemp = new Dictionary<string, object>();
                                        dicTemp.Add("value", item["OAFlowID"]);
                                        dicDt1.Add("gllc", dicTemp);
                                    }
                                    dicTemp = new Dictionary<string, object>();
                                    dicTemp.Add("value", item["LineID"]);
                                    dicDt1.Add("xid", dicTemp);

                                    dicTemp = new Dictionary<string, object>();
                                    dicTemp.Add("value", item["MRPCode"]);
                                    dicDt1.Add("mrpfl", dicTemp);
                                    dicTemp = new Dictionary<string, object>();
                                    dicTemp.Add("value", item["Code"]);
                                    dicDt1.Add("lh", dicTemp);
                                    dicTemp = new Dictionary<string, object>();
                                    dicTemp.Add("value", item["Name"]);
                                    dicDt1.Add("pm", dicTemp);
                                    dicTemp = new Dictionary<string, object>();
                                    dicTemp.Add("value", item["SPECS"]);
                                    dicDt1.Add("gg", dicTemp);

                                    dicTemp = new Dictionary<string, object>();
                                    dicTemp.Add("value", Convert.ToInt32(item["ArriveQtyTU"]).ToString());
                                    dicDt1.Add("sdsl", dicTemp);
                                    dicTemp = new Dictionary<string, object>();
                                    dicTemp.Add("value", Convert.ToInt32(item["EyeballingQtyTU"]).ToString());
                                    dicDt1.Add("dssl", dicTemp);

                                    dicTemp = new Dictionary<string, object>();
                                    dicTemp.Add("value", Convert.ToInt32(item["RcvQtyTU"]).ToString());
                                    dicDt1.Add("sssl", dicTemp);
                                    dicTemp = new Dictionary<string, object>();
                                    dicTemp.Add("value", Convert.ToInt32(item["RtnFillQtyTU"]).ToString());
                                    dicDt1.Add("tbsl", dicTemp);
                                    dicTemp = new Dictionary<string, object>();
                                    dicTemp.Add("value", Convert.ToInt32(item["RtnDeductQtyTU"]).ToString());
                                    dicDt1.Add("tksl", dicTemp);

                                    dicTemp = new Dictionary<string, object>();
                                    dicTemp.Add("value", item["WhCode"]);
                                    dicDt1.Add("ccdd", dicTemp);

                                    dicTemp = new Dictionary<string, object>();
                                    dicTemp.Add("value", item["RcvLotCode"]);
                                    dicDt1.Add("shph", dicTemp);
                                    dicTemp = new Dictionary<string, object>();
                                    dicTemp.Add("value", item["SrcDoc_SrcDocNo"]);
                                    dicDt1.Add("lydjh", dicTemp);

                                    dicTemp = new Dictionary<string, object>();
                                    dicTemp.Add("value", item["FinallyPriceTC"].ToString());
                                    dicDt1.Add("zzj", dicTemp);
                                    dicTemp = new Dictionary<string, object>();
                                    dicTemp.Add("value", item["ArriveTotalNetMnyTC"].ToString());
                                    dicDt1.Add("sdwse", dicTemp);

                                    dicTemp = new Dictionary<string, object>();
                                    dicTemp.Add("value", item["ArriveTotalTaxTC"].ToString());
                                    dicDt1.Add("sdse", dicTemp);
                                    dicTemp = new Dictionary<string, object>();
                                    dicTemp.Add("value", item["ArriveTotalMnyTC"]);
                                    dicDt1.Add("sdsjhj", dicTemp);
                                    if (item["PlanArrivedDate"] != DBNull.Value)
                                    {
                                        dicTemp = new Dictionary<string, object>();
                                        dicTemp.Add("value", Convert.ToDateTime(item["PlanArrivedDate"]).ToString("yyyy-MM-dd"));
                                        dicDt1.Add("jhdhrq", dicTemp);
                                    }
                                    else
                                    {
                                        dicTemp = new Dictionary<string, object>();
                                        dicTemp.Add("value", DateTime.Now.ToString("yyyy-MM-dd"));
                                        dicDt1.Add("jhdhrq", dicTemp);
                                    }
                                    dicTemp = new Dictionary<string, object>();
                                    dicTemp.Add("value", item["DescFlexSegments_PrivateDescSeg1"]);
                                    dicDt1.Add("jyy", dicTemp);
                                    dicTemp = new Dictionary<string, object>();
                                    dicTemp.Add("value", item["DescFlexSegments_PrivateDescSeg3"]);
                                    dicDt1.Add("ptyy", dicTemp);
                                    dicTemp = new Dictionary<string, object>();
                                    dicTemp.Add("value", item["DescFlexSegments_PrivateDescSeg2"]);
                                    dicDt1.Add("clyj", dicTemp);

                                    dicTemp = new Dictionary<string, object>();
                                    dicTemp.Add("value", item["DocLineNo"]);
                                    dicDt1.Add("hh", dicTemp);
                                    dicTemp = new Dictionary<string, object>();
                                    if (Convert.ToInt32(item["SumDispenseQtyAU"]) > 0)
                                    {
                                        dicTemp.Add("value", "0");
                                    }
                                    else
                                    {
                                        dicTemp.Add("value", "1");
                                    }
                                    dicDt1.Add("sfffwl", dicTemp);
                                    liDicDt1.Add(dicDt1);


                                }
                                #endregion
                                if (IsWholeOrderSubmit && IsOAFlowSame)
                                {
                                    dicTemp = new Dictionary<string, object>();
                                    dicTemp.Add("value", firstOAFlowID);
                                    dicMain.Add("gllc", dicTemp);//关联流程
                                }
                                else
                                {
                                    dicMain.Add("gllc", null);//关联流程
                                }
                                dicResult.Add("main", dicMain);
                                dicResult.Add("dt1", liDicDt1);
                                List<Dictionary<string, object>> liResult = new List<Dictionary<string, object>>();
                                liResult.Add(dicResult);
                                string json = UFIDA.U9.CBO.PubBE.YYC.JsonHelper.GetJsonJS(liResult);
                                string OAFlowID = PubFunction.OAService(json);
                                if (!string.IsNullOrEmpty(OAFlowID))//整单提交
                                {
                                    string updateSql1 = "";
                                    if (IsWholeOrderSubmit)
                                    {
                                        //整单提交接口
                                        SubmitWholeOrder(rcvID);
                                        updateSql1 = string.Format("update PM_Receivement set DescFlexField_PrivateDescSeg7='是' where id in ({0})", rcvID);
                                    }
                                    else
                                    {
                                        lineSubmitProx.Do();
                                        updateSql1 = string.Format("update PM_Receivement set DescFlexField_PrivateDescSeg7='' where id in ({0})", rcvID);
                                    }
                                    DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), updateSql1, null);
                                }

                                string updateSql = string.Format("update PM_RcvLine set DescFlexSegments_PrivateDescSeg4='',DescFlexSegments_PrivateDescSeg5={0} where id in ({1})", OAFlowID, lineIDs.Substring(0, lineIDs.Length - 1));
                                DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), updateSql, null);
                                if (!string.IsNullOrEmpty(refuseOAFlowids))//如果有一同退回的行未再提交，需要将其是否退回标识清除，否则会影响此行下次提交                            {
                                {
                                    string updateSql2 = string.Format("update PM_RcvLine set DescFlexSegments_PrivateDescSeg4='' where DescFlexSegments_PrivateDescSeg5 in ({0}) ", refuseOAFlowids);
                                    DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), updateSql2, null);
                                }
                            }
                            else if (IsWholeOrderSubmit)//不触发OA流程，U9整单提交
                            {
                                SubmitWholeOrder(rcvID);
                            }
                            else
                            {
                                SetErrorMsg("只有在OA审批的流程才支持行审，请联系管理员！");
                                return;
                            }
                        }
                        else
                        {
                            SetErrorMsg("未找到ID：" + rcvRec.DocNo + "收货单数据");
                            return;
                        }
                    }
                    else
                    {
                        SetErrorMsg("未找到ID：" + rcvRec.DocNo + "收货单数据");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                SetErrorMsg("提交异常：" + ex.Message);
            }

        }

        //{"requestid":"123445","u9no":"123445","hh":[{"hanhao":10,"content":"修改内容","fieldname":"11"},{"hanhao":20,"content":"修改内容","fieldname":"111"}]}
        /// <summary>
        /// 弃审按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnUnApprove_Click(object sender, EventArgs e)
        {
            try
            {
                SaveData();
                //TODO:整单提交的不允许选择行弃审
                UFIDA.U9.SCM.PM.ReceivementUIModel.ReceivementRecord rcvRec = (UFIDA.U9.SCM.PM.ReceivementUIModel.ReceivementRecord)uiPart.Model.Views["Receivement"].FocusedRecord;
                string rcvID = rcvRec.ID.ToString();
                string lineIDs = "";
                IEnumerator enumerator;
                IUIRecordCollection ius = uiPart.Model.Views["Receivement_RcvLines"].SelectRecords;
                if (ius.Count > 0)//获取选中行ID集合
                {
                    enumerator = ius.GetEnumerator();

                }
                else//未选行，视为整单提交
                {
                    SetErrorMsg("请选择要弃审的行！");
                    return;
                    //enumerator = uiPart.Model.Views["Receivement_RcvLines"].Records.GetEnumerator();
                }

                while (enumerator.MoveNext())
                {
                    IUIRecord i = (IUIRecord)enumerator.Current;
                    lineIDs += i["ID"].ToString() + ",";
                }

                //校验单据是否全部为已审核状态，如果可以弃审，则调用OA接口将已归档流程中的收货行置为已弃审
                string sql = string.Format(@"SELECT 
a.ID,a.DocNo,a.DescFlexField_PrivateDescSeg7 IsWholeOrderSubmit,b.ID LineID,b.DocLineNo,a.Status,b.Status LineStatus,b.DescFlexSegments_PrivateDescSeg4 IsRefuse
,b.DescFlexSegments_PrivateDescSeg5 OAFlowID,0 IsNeed
FROM dbo.PM_Receivement a INNER JOIN dbo.PM_RcvLine b ON a.ID = b.Receivement
WHERE 1 = 1
AND a.ID = {0} AND b.ID IN ({1})", rcvID, lineIDs.Substring(0, lineIDs.Length - 1));
                DataSet ds = new DataSet();
                DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), sql, null, out ds);
                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        bool canUnApprove = true;
                        string errorLines = "";
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            if (dr["LineStatus"].ToString() == "3")
                            {
                                errorLines += dr["DocLineNo"].ToString() + ",";
                            }
                        }
                        if (!string.IsNullOrEmpty(errorLines))
                        {
                            canUnApprove = false;
                            SetErrorMsg(errorLines.Substring(0, errorLines.Length - 1) + "行当前状态为'审核中'，不能弃审！");
                            return;

                        }
                        if (canUnApprove)
                        {
                            //整单
                            if (rcvRec["IsLineApprove"].ToString().ToLower() == "false")
                            {
                                UnApproveWholeOrder(rcvID);
                            }
                            else//行弃审
                            {
                                UFIDA.U9.PM.Rcv.Proxy.RcvBatchApprovedForLineApproveBPProxy rcvBPS = new UFIDA.U9.PM.Rcv.Proxy.RcvBatchApprovedForLineApproveBPProxy();
                                rcvBPS.RcvLines = new List<long>();
                                string IDs = "";
                                foreach (DataRow dr in ds.Tables[0].Rows)
                                {
                                    if (dr["LineStatus"].ToString() == "4" || dr["LineStatus"].ToString() == "5")
                                    {
                                        IDs += dr["LineID"].ToString() + ",";
                                        rcvBPS.RcvLines.Add(Convert.ToInt64(dr["LineID"]));
                                    }
                                }
                                if (rcvBPS.RcvLines.Count > 0)
                                {
                                    rcvBPS.ActType = 9;
                                    rcvBPS.Do();
                                    //string sqlUpdate = "update pm_rcvline set DescFlexSegments_PrivateDescSeg4='' where id in (" + IDs.Substring(0, IDs.Length - 1) + ")";
                                    //DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), sqlUpdate, null);
                                    PubFunction.OAModifyService(GetOAModifyInfo(ds));
                                }
                                else
                                {
                                    SetErrorMsg("没有'业务关闭'的行可以弃审！");
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        SetErrorMsg("未找到ID：" + rcvRec.DocNo + "收货单数据");
                        return;
                    }
                }
                else
                {
                    SetErrorMsg("未找到ID：" + rcvRec.DocNo + "收货单数据");
                    return;
                }
            }
            catch (Exception ex)
            {
                SetErrorMsg("弃审异常：" + ex.Message);
                return;
            }
        }

        /// <summary>
        /// 抛出错误信息
        /// </summary>
        /// <param name="msg"></param>
        public void SetErrorMsg(string msg)
        {
            this.uiPart.Model.ClearErrorMessage();
            this.uiPart.Model.ErrorMessage.Message = msg;
        }

        /// <summary>
        /// 整单提交
        /// </summary>
        /// <param name="rcvID"></param>
        public void SubmitWholeOrder(string rcvID)
        {
            UFIDA.U9.PM.Rcv.Proxy.RcvApprovedBPProxy wholeOrderSubmitProx = new UFIDA.U9.PM.Rcv.Proxy.RcvApprovedBPProxy();
            wholeOrderSubmitProx.ActType = 7;
            wholeOrderSubmitProx.DocHead = Convert.ToInt64(rcvID);
            wholeOrderSubmitProx.Do();
        }
        /// <summary>
        /// 整单弃审
        /// </summary>
        /// <param name="rcvID"></param>
        public void UnApproveWholeOrder(string rcvID)
        {
            UFIDA.U9.PM.Rcv.Proxy.RcvApprovedBPProxy wholeOrderSubmitProx = new UFIDA.U9.PM.Rcv.Proxy.RcvApprovedBPProxy();
            wholeOrderSubmitProx.ActType = 9;
            wholeOrderSubmitProx.DocHead = Convert.ToInt64(rcvID);
            wholeOrderSubmitProx.Do();
        }
        /// <summary>
        /// 弃审后，修改OA归档行状态信息
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public string GetOAModifyInfo(DataSet ds)
        {
            List<Dictionary<string, object>> li = new List<Dictionary<string, object>>();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                if (ds.Tables[0].Rows[i]["IsNeed"].ToString() == "0" && (ds.Tables[0].Rows[i]["LineStatus"].ToString() == "4" || ds.Tables[0].Rows[i]["LineStatus"].ToString() == "5"))
                {
                    string flowid = ds.Tables[0].Rows[i]["OAFlowID"].ToString();
                    for (int j = i; j < ds.Tables[0].Rows.Count; j++)
                    {
                        if (ds.Tables[0].Rows[j]["IsNeed"].ToString() == "0" && ds.Tables[0].Rows[j]["OAFlowID"].ToString() == flowid && (ds.Tables[0].Rows[j]["LineStatus"].ToString() == "4" || ds.Tables[0].Rows[j]["LineStatus"].ToString() == "5"))
                        {
                            Dictionary<string, object> dic = new Dictionary<string, object>();
                            dic.Add("requestid", ds.Tables[0].Rows[j]["OAFlowID"].ToString());
                            dic.Add("u9no", ds.Tables[0].Rows[j]["DocNo"].ToString());
                            Dictionary<string, string> dicL = new Dictionary<string, string>();
                            dicL.Add("hanhao", ds.Tables[0].Rows[j]["DocLineNo"].ToString());
                            dicL.Add("content", "已弃审");
                            dicL.Add("fieldname", "xzt");
                            List<Dictionary<string, string>> ss = new List<Dictionary<string, string>>();
                            ss.Add(dicL);
                            dic.Add("hh", ss);
                            ds.Tables[0].Rows[j]["IsNeed"] = "1";
                            li.Add(dic);
                        }
                    }
                }

            }
            return UFIDA.U9.CBO.PubBE.YYC.JsonHelper.GetJsonJS(li);
        }



    }
}
