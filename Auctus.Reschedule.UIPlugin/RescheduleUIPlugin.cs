using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UFSoft.UBF.UI.IView;
using UFSoft.UBF.UI.WebControls;
using UFSoft.UBF.UI.FormProcess;
using UFSoft.UBF.UI.ActionProcess;
using System.Web.UI.WebControls;
using UFSoft.UBF.UI.ControlModel;
using UFSoft.UBF.UI.WebControlAdapter;
using UFSoft.UBF.UI.MD.Runtime;
using System.Collections.Specialized;
using UFSoft.UBF.Business;
using UFIDA.U9.Base;
using System.Data;
using UFSoft.UBF.Util.DataAccess;
//using System.Web;
using UFSoft.UBF.Sys.Database;

namespace Auctus.Reschedule.UIPlugin
{
    /// <summary>
    /// 已弃用，代码移至Auctus.CustomUI.RescheduleUIPlugin
    /// </summary>
    public class RescheduleUIPlugin : UFSoft.UBF.UI.Custom.ExtendedPartBase
    {
        private IPart iPart;
        #region 添加按钮控件
        /// <summary>
        /// UI界面布局，添加按钮控件
        /// </summary>
        /// <param name="Part"></param>
        /// <param name="args"></param>
        public override void AfterInit(IPart Part, EventArgs args)
        {
            base.AfterInit(Part, args);
            iPart = Part;
            // 增加备料预留量查询按钮
            IUFButton btnPOModify = new UFWebButtonAdapter();
            btnPOModify.Text = "创建变更单";
            btnPOModify.ID = "btnPOModify";
            btnPOModify.AutoPostBack = true;
            btnPOModify.ToolTip = "创建变更单";
            btnPOModify.Visible = true;

            // 将按钮加入到按钮栏
            IUFCard iCard = (IUFCard)Part.GetUFControlByName(Part.TopLevelContainer, "Card0");
            iCard.Controls.Add(btnPOModify);
            CommonFunction.Layout(iCard, btnPOModify, 6, 0);
            btnPOModify.Click += new EventHandler(BtnPOModify_Click);

        }
        #endregion
        #region 关闭采购计划行服务
        /// <summary>
        /// 关闭采购计划行服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPOShiplineClose_Click(object sender, EventArgs e)
        {            
            DataSet ds = new DataSet();
            DataParamList dp = new DataParamList();
            DataAccessor.RunSP("sp_Auctus_UI_GetPOShiplineClose", dp, out ds);
            if (ds.Tables[0].Rows.Count > 0)
            {
                List<long> li = new List<long>();
                foreach (DataRow item in ds.Tables[0].Rows)
                {
                    li.Add(Convert.ToInt64(item["ID"].ToString()));
                }
                UFIDA.U9.PM.PO.Proxy.ClosePOShiplineSVProxy sv = new UFIDA.U9.PM.PO.Proxy.ClosePOShiplineSVProxy();
                sv.POShiplineList = new List<long>();
                sv.POShiplineList.AddRange(li);
                sv.Do();
            }
            //DataParamList dp2 = new DataParamList();
            //DataParamFactory.CreateInput("poShiplineIDs",ds.Tables[0] , DbType.Object);
            //DataParamFactory.CreateTableParam("poShiplineIDs", "auctus_POShiplineIDs", ds.Tables[0]);
            System.Data.SqlClient.SqlParameter sp = new System.Data.SqlClient.SqlParameter("@auctus_POShiplineIDs", SqlDbType.Structured);
            sp.Value = ds.Tables[0];
            DataAccessor.RunSQLWithTableParam(DatabaseManager.GetCurrentConnection(), "UPDATE dbo.MRP_Reschedule SET ConfirmType=1 WHERE ID IN (SELECT RescheduleID FROM @auctus_POShiplineIDs)", sp);
            
            

        }
        #endregion
        #region 确认重排建议BP
        /// <summary>
        /// 确认重排建议BP
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPOModify_Click4(object sender, EventArgs e)
        {
            UFIDA.U9.MRP.MRP.Proxy.CommitRescheduleProxy bp = new UFIDA.U9.MRP.MRP.Proxy.CommitRescheduleProxy();
            bp.Reschedules = new List<long>();
            bp.Reschedules.Add(1001901030820038);
            bp.Do();
        }
        #endregion
        #region 创建单个变更单
        /// <summary>
        /// 创建单个变更单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPOModifySingle_Click(object sender, EventArgs e)
        {
            DataParamList dp = new DataParamList();
            dp.Add(DataParamFactory.CreateOutput("Result", DbType.String));
            DataAccessor.RunSP("sp_Auctus_UI_CreatePOModify2", dp);
            string result = dp["Result"].Value.ToString();
            if (result != "1")
            {
                throw new Exception(result);
            }
        }
        #endregion
        #region 批量创建变更单
        /// <summary>
        /// 开发测试版
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnTest_Click(object sender, EventArgs e)
        {           
            DataParamList dp = new DataParamList();
            dp.Add(DataParamFactory.CreateOutput("Result", DbType.String));
            DataAccessor.RunSP("sp_Auctus_UI_CreatePOModifyQty", dp);
            string result = dp["Result"].Value.ToString();            
            if (result != "1")
            {
                throw new Exception(result);
            }
        }
        /// <summary>
        /// 创建只变交期的采购变更单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPOModify_Click(object sender, EventArgs e)
        {
            string userName = Context.LoginUser;            
            DataParamList dp = new DataParamList();
            dp.Add(DataParamFactory.CreateInput("UserName",userName,DbType.String));
            dp.Add(DataParamFactory.CreateOutput("Result", DbType.String));            
            DataAccessor.RunSP("sp_Auctus_UI_CreatePOModify", dp);
            string result = dp["Result"].Value.ToString();
            if (result != "1")
            {
                throw new Exception(result);
            }
        }
        #endregion

        /// <summary>
        /// 创建变更单按钮(弃用)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPOModify_Clickx(object sender, EventArgs e)
        {
            List<UFIDA.U9.PM.POChange.POModifyData> liPOModifyDate = new List<UFIDA.U9.PM.POChange.POModifyData>();//只变更日期的变更单
            List<UFIDA.U9.PM.POChange.POModifyData> liPOModify = new List<UFIDA.U9.PM.POChange.POModifyData>();//变更数量的变更单
            List<long> liAutoApprove = new List<long>();//新生成的变更单ID集合（需要自动审核的）
            //MRP重排建议与对应的PO集合
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            DataParamList dp = new DataParamList();
            DataAccessor.RunSP("sp_Auctus_BE_GetReschedule", dp, out ds);
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                string type = "0";//0只改时间和取消操作

                UFIDA.U9.PM.POChange.POModifyData poModify = new UFIDA.U9.PM.POChange.POModifyData();
                //填充POModify实体
                if (dr["RType"].ToString() == "1")//更新操作
                {
                    poModify = GetPOModify(dr, ref type);
                    if (type == "1")
                    {
                        liPOModifyDate.Add(poModify);
                    }
                    else if (type == "2")
                    {
                        liPOModify.Add(poModify);
                    }
                }
                else if (dr["RType"].ToString() == "2")//TODO：取消操作，调用“关闭采购订单计划行服务”
                {

                }

            }
            if (liPOModifyDate.Count > 0)
            {
                for (int i = 0; i < liPOModifyDate.Count - 1; i++)
                {
                    for (int j = i + 1; j < liPOModifyDate.Count; j++)
                    {
                        if (liPOModifyDate[i].PODocNo == liPOModifyDate[j].PODocNo)
                        {
                            List<UFIDA.U9.PM.POChange.POModifyLineData> ll = new List<UFIDA.U9.PM.POChange.POModifyLineData>();
                            List<UFIDA.U9.PM.POChange.POShiplineModifyData> ll2 = new List<UFIDA.U9.PM.POChange.POShiplineModifyData>();
                            foreach (UFIDA.U9.PM.POChange.POModifyLineData item in liPOModifyDate[j].POModifyLine)
                            {
                                item.POModify = liPOModifyDate[i];
                                ll.Add(item);
                            }
                            foreach (UFIDA.U9.PM.POChange.POShiplineModifyData item in liPOModifyDate[j].POShiplineModify)
                            {
                                item.POModify = liPOModifyDate[i];
                                ll2.Add(item);
                            }
                            //liPOModifyDate[i].POModifyLine.AddRange(liPOModifyDate[j].POModifyLine);
                            liPOModifyDate[i].POModifyLine.AddRange(ll);
                            liPOModifyDate[i].POShiplineModify.AddRange(ll2);
                            liPOModifyDate.RemoveAt(j);
                            j--;
                        }
                    }
                }
                foreach (UFIDA.U9.PM.POChange.POModifyData item in liPOModifyDate)
                {
                    PurchaseOrderChangeListBP.Proxy.SavePOModifyBPProxy bpProxy = new PurchaseOrderChangeListBP.Proxy.SavePOModifyBPProxy();
                    bpProxy.POModify = item;
                    long i = bpProxy.Do();
                    liAutoApprove.Add(i);

                }
                #region 自动审审核采购变更单
                if (liAutoApprove.Count > 0)
                {

                    List<PurchaseOrderChangeListBP.POModifyDTOData> li = new List<PurchaseOrderChangeListBP.POModifyDTOData>();
                    PurchaseOrderChangeListBP.POModifyDTOData dto = new PurchaseOrderChangeListBP.POModifyDTOData();
                    dto.IDKey = liAutoApprove[0];
                    dto.IDKey_SKey = new BusinessEntity.EntityKey(liAutoApprove[0], "UFIDA.U9.PM.POChange.POModify");
                    liPOModifyDate[0].ID = liAutoApprove[0];
                    dto.POModify = liPOModifyDate[0];
                    //UFIDA.U9.PM.POChange.POModifyData data = new UFIDA.U9.PM.POChange.POModifyData();



                    li.Add(dto);

                    //PurchaseOrderChangeListBP.Proxy.POModifySubmitBPProxy submitBpProxy = new PurchaseOrderChangeListBP.Proxy.POModifySubmitBPProxy();
                    //submitBpProxy.POModifyDTOs = li;
                    //submitBpProxy.Do();


                    //PurchaseOrderChangeListBP.Proxy.POModifyApproveBPProxy approveBpProxy = new PurchaseOrderChangeListBP.Proxy.POModifyApproveBPProxy();
                    //approveBpProxy.POModifyDTOs = li;
                    //approveBpProxy.Do();
                }

                #endregion
            }
            //TODO:数量变更单据
            //if (liPOModify.Count > 0)
            //{
            //    foreach (UFIDA.U9.PM.POChange.POModifyData item in liPOModifyDate)
            //    {
            //        PurchaseOrderChangeListBP.Proxy.SavePOModifyBPProxy bpProxy = new PurchaseOrderChangeListBP.Proxy.SavePOModifyBPProxy();
            //        bpProxy.POModify = item;
            //        bpProxy.Do();
            //    }
            //}
        }

        /// <summary>
        /// 生成PO变更单
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="type">0\不制作变更单;1\只变更时间;2\其他类型PO变更单;</param>
        /// <returns></returns>
        public UFIDA.U9.PM.POChange.POModifyData GetPOModify(DataRow dr, ref string type)
        {
            type = "0";
            UFIDA.U9.PM.POChange.POModifyData poModify = new UFIDA.U9.PM.POChange.POModifyData();
            #region 变更数据            
            //poModify.POShiplineModify = new List<UFIDA.U9.PM.POChange.POShiplineModifyData>();
            //poModify.POLineModify = new List<UFIDA.U9.PM.POChange.POLineModifyData>();
            //poModify.POAddressModify = new List<UFIDA.U9.PM.POChange.POAddressModifyData>();
            //poModify.POContactModify = new List<UFIDA.U9.PM.POChange.POContactModifyData>();
            //poModify.PODiscountModify = new List<UFIDA.U9.PM.POChange.PODiscountModifyData>();
            //poModify.POFeeModify = new List<UFIDA.U9.PM.POChange.POFeeModifyData>();
            //poModify.POMenoModify = new List<UFIDA.U9.PM.POChange.POMenoModifyData>();
            //poModify.POTaxModify = new List<UFIDA.U9.PM.POChange.POTaxModifyData>();
            #endregion
            poModify.ActionType = 0;//更新类型，默认值
            poModify.ApprovedBy = "";
            //poModify.ApprovedOn = DateTime.Now;
            poModify.CancelApprovedBy = "";
            //poModify.CancelApprovedOn = DateTime.Now;
            poModify.Demo = "系统自动生成单据";//备注
            poModify.DocumentType = 1001708070112532;//单据类型
            //poModify.DocumentType_SKey
            poModify.IsModifyVersion = true;//更新订单版本
            poModify.ModifyIndex = Convert.ToInt32(dr["POModifiedTimes"].ToString()) + 1;
            poModify.ModifyReason = 0;
            poModify.PO = Convert.ToInt64(dr["POID"]);
            poModify.PODocNo = dr["PODocNo"].ToString();
            //poModify.PO_SKey
            poModify.Status = 0;//开立
            poModify.WFCurrentState = -1;
            poModify.WFOriginalState = -1;
            //采购订单计划行变更表
            poModify.POShiplineModify = new List<UFIDA.U9.PM.POChange.POShiplineModifyData>();
            //poModify.POShiplineModify.Add(GetPOShiplineModifyData(dr));
            //POModifyLine PO变更单单行
            poModify.POModifyLine = new List<UFIDA.U9.PM.POChange.POModifyLineData>();



            int SupplierConfirmQtyTU = Convert.ToInt32(Convert.ToDouble(dr["SupplierConfirmQtyTU"].ToString()));
            int SMQty = Convert.ToInt32(Convert.ToDouble(dr["SMQty"].ToString()));
            int RescheduleQty = Convert.ToInt32(Convert.ToDouble(dr["RescheduleQty"].ToString()));
            DateTime OriginalDate = Convert.ToDateTime(dr["OriginalDate"].ToString());
            DateTime RescheduleDate = Convert.ToDateTime(dr["RescheduleDate"].ToString());
            if (SupplierConfirmQtyTU == SMQty)//库存主单位数量=采购确认数量
            {
                if (SMQty == RescheduleQty)//只修改日期
                {
                    if (OriginalDate != RescheduleDate)//修改日期
                    {
                        UFIDA.U9.PM.POChange.POModifyLineData pomodifyLine = new UFIDA.U9.PM.POChange.POModifyLineData();
                        pomodifyLine = GetPOModifyLineData(dr, 2, "DeliveryDate", "要求交货日", "System.DateTime", "OriginalDate", "RescheduleDate");
                        pomodifyLine.POModify = poModify;
                        poModify.POModifyLine.Add(pomodifyLine);

                        UFIDA.U9.PM.POChange.POModifyLineData pomodifyLine2 = new UFIDA.U9.PM.POChange.POModifyLineData();
                        pomodifyLine2 = GetPOModifyLineData(dr, 2, "NeedPODate", "应下订单日", "System.DateTime", "OriginalDate", "RescheduleDate");
                        pomodifyLine2.POModify = poModify;
                        poModify.POModifyLine.Add(pomodifyLine2);

                        UFIDA.U9.PM.POChange.POModifyLineData pomodifyLine3 = new UFIDA.U9.PM.POChange.POModifyLineData();
                        pomodifyLine3 = GetPOModifyLineData(dr, 2, "PlanArriveDate", "计划到货日", "System.DateTime", "OriginalDate", "RescheduleDate");
                        pomodifyLine3.POModify = poModify;
                        poModify.POModifyLine.Add(pomodifyLine3);
                        type = "1";
                    }
                    else
                    {
                        type = "0";
                    }
                }
                else if (SMQty > RescheduleQty)
                {
                    if (OriginalDate == RescheduleDate)//直接改小数量
                    {

                    }
                    else//原始行修改，拆行，拆行的数量=重排数量，时间=重排时间
                    {

                    }
                }
                else//SMQty<RescheduleQty
                {
                    if (OriginalDate == RescheduleDate)//直接修改数量
                    {

                    }
                    else//直接修改数量和时间，不需要拆行
                    {

                    }
                }
            }
            else//库存主单位数量！=采购确认数量，需要拆行
            {
                if (SMQty == RescheduleQty)
                {
                    if (OriginalDate != RescheduleDate)//拆行，原始行做修改，新拆一行
                    {

                    }
                    else
                    {
                        type = "0";
                    }
                }
                else if (SMQty > RescheduleQty)
                {
                    if (OriginalDate != RescheduleDate)//修改数量和时间
                    {

                    }
                    else//只修改数量
                    {

                    }
                }
            }
            return poModify;
        }


        #region 生成PO变更行POModifyLine
        /// <summary>
        /// 获取PO变更行
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="changeType"></param>
        /// <param name="modifiedData"></param>
        /// <param name="modifiedDataName"></param>
        /// <param name="modifiedDataType"></param>
        /// <param name="dataBeforeModified"></param>
        /// <param name="dataAfterModified"></param>
        /// <returns></returns>
        public UFIDA.U9.PM.POChange.POModifyLineData GetPOModifyLineData(DataRow dr, int changeType, string modifiedData, string modifiedDataName, string modifiedDataType, string dataBeforeModified, string dataAfterModified)
        {
            UFIDA.U9.PM.POChange.POModifyLineData pomodifyLine = new UFIDA.U9.PM.POChange.POModifyLineData();
            pomodifyLine.DocLineNo = 0;//默认值
            pomodifyLine.ItemInfo = Convert.ToInt64(dr["ItemInfo_ItemID"].ToString());
            pomodifyLine.POLineDocLineNo = Convert.ToInt32(dr["Linenum"].ToString());
            pomodifyLine.POLineID = Convert.ToInt64(dr["PODocLineID"].ToString());
            pomodifyLine.POShipLineDocLineNo = Convert.ToInt32(dr["PlanLineNum"].ToString());
            pomodifyLine.POShipLineID = Convert.ToInt64(dr["POSubLineID"].ToString());
            pomodifyLine.SysVersion = 0;
            pomodifyLine.ChangeType = changeType;//变更类型
            switch (changeType)
            {
                case 0://采购订单头变更
                    pomodifyLine.LineID = Convert.ToInt64(dr["POID"].ToString());
                    pomodifyLine.ModifiedEntity = new BusinessEntity.EntityKey(Convert.ToInt64(dr["POID"].ToString()), "UFIDA.U9.PM.PO.PurchaseOrder");
                    pomodifyLine.ModifiedEntityName = "UFIDA.U9.PM.PO.PurchaseOrder";
                    break;
                case 1://采购订单行变更
                    pomodifyLine.LineID = Convert.ToInt64(dr["PODocLineID"].ToString());
                    pomodifyLine.ModifiedEntity = new BusinessEntity.EntityKey(Convert.ToInt64(dr["PODocLineID"].ToString()), "UFIDA.U9.PM.PO.POLine");
                    pomodifyLine.ModifiedEntityName = "UFIDA.U9.PM.PO.POLine";
                    break;
                case 2://采购计划行变更
                    pomodifyLine.CodeAfterModified = "";
                    pomodifyLine.CodeBeforeModified = "";
                    pomodifyLine.NameAfterModifeid = "";
                    pomodifyLine.NameBeforeModifeid = "";
                    pomodifyLine.LineID = Convert.ToInt64(dr["POSubLineID"].ToString());
                    pomodifyLine.ModifiedEntity = new BusinessEntity.EntityKey(Convert.ToInt64(dr["POSubLineID"].ToString()), "UFIDA.U9.PM.PO.POShipLine");
                    pomodifyLine.ModifiedEntityName = "UFIDA.U9.PM.PO.POShipLine";
                    break;
                default:
                    break;
            }
            pomodifyLine.ModifiedData = modifiedData;
            pomodifyLine.ModifiedDataName = modifiedDataName;
            pomodifyLine.ModifiedDataType = modifiedDataType;
            pomodifyLine.DataBeforeModified = dr[dataBeforeModified].ToString();
            pomodifyLine.DataAfterModified = dr[dataAfterModified].ToString();
            return pomodifyLine;
        }
        #endregion
        #region 生成采购订单计划行变更表POShiplineModify
        public UFIDA.U9.PM.POChange.POShiplineModifyData GetPOShiplineModifyData(DataRow dr)
        {
            UFIDA.U9.PM.POChange.POShiplineModifyData poShipline = new UFIDA.U9.PM.POChange.POShiplineModifyData();
            try
            {
                poShipline.ActionType = 1;
                poShipline.ActiveType = 1;
                poShipline.ApprovedBy = "";
                poShipline.CancelApprovedBy = "";
                //poShipline.CancelApprovedOn
                poShipline.CreatedBy = "刘飞";
                poShipline.CreatedOn = DateTime.Now;
                poShipline.ModifiedBy = "刘飞";
                poShipline.ModifiedOn = DateTime.Now;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return poShipline;
        }
        #endregion


    }
}
