using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UFIDA.U9.Base.Doc;
using UFIDA.U9.CBO.SCM.Item;
using UFIDA.U9.CBO.SCM.Supplier;
using UFIDA.U9.PM.PMPubSV.Proxy;
using UFIDA.U9.SCM.PM.PRToPOUIModel;
using UFSoft.UBF.UI.ActionProcess;
using UFSoft.UBF.UI.ControlModel;
using UFSoft.UBF.UI.IView;
using UFSoft.UBF.UI.MD.Runtime;
using UFSoft.UBF.UI.WebControlAdapter;
using UFSoft.UBF.Util.DataAccess;

namespace Auctus.PR.UIPlugin
{
    /// <summary>
    /// 请购单页面的转PO弹出框中的PR转PO试算按钮
    /// </summary>
    public class TryCalcUIPlugin : UFSoft.UBF.UI.Custom.ExtendedPartBase
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
            IUFButton btnTryCalc2 = new UFWebButtonAdapter();
            btnTryCalc2.Text = "试算";
            btnTryCalc2.ID = "btnTryCalc2";
            btnTryCalc2.AutoPostBack = true;
            btnTryCalc2.ToolTip = "试算";
            btnTryCalc2.Visible = true;

            IUFButton btnO = (IUFButton)Part.GetUFControlByName(Part.TopLevelContainer, "BtnTryCalc");
            btnO.Visible = false;
            // 将按钮加入到按钮栏
            IUFCard iCard = (IUFCard)Part.GetUFControlByName(Part.TopLevelContainer, "Card1");
            iCard.Controls.Add(btnTryCalc2);
            CommonFunction.Layout(iCard, btnTryCalc2, 4, 0);
            btnTryCalc2.Click += new EventHandler(BtnTryCalc2_Click);
        }
        #endregion
        /// <summary>
        /// 创建只变交期的采购变更单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnTryCalc2_Click(object sender, EventArgs e)
        {
            this.iPart.DataCollect();
            this.iPart.IsDataBinding = true;
            this.iPart.IsConsuming = false;
            this.TryCalc_Extend(sender, new UIActionEventArgs());
        }

        /// <summary>
        /// 试算2功能 region之外的代码都是copy的U9反编译的代码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TryCalc_Extend(object sender, UIActionEventArgs e)
        {
            #region Add by liufei
            string ids = "";//拼接请购行ID字符串
            #endregion
            PRToPOUIModelAction prToPO = ((PRToPOUIModelAction)this.iPart.Action);
            prToPO.ClearErrorMessage();
            //this.OnBtnTryCalcClick_DefaultImpl(sender, e);
            IList<IUIRecord> selectRecordFromCache = UIRuntimeHelper.Instance.GetSelectRecordFromCache(prToPO.CurrentModel.PR_PRLineList);
            if (selectRecordFromCache == null || selectRecordFromCache.Count == 0)
            {
                throw new Exception(PRToPOUIModelRes.NoSelectedRecord);
            }
            //this.CurrentModel.TryCalcView.Records.Clear();
            prToPO.CurrentModel.TryCalcView.Records.Clear();
            string text = "";
            GetSupplierQuotaSVProxy getSupplierQuotaSVProxy = new GetSupplierQuotaSVProxy();
            getSupplierQuotaSVProxy.QuotaReqList = new List<QuotaReqDTOData>();
            int num = 1;
            Dictionary<long, int> dictionary = new Dictionary<long, int>();
            using (IEnumerator<IUIRecord> enumerator = selectRecordFromCache.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    PR_PRLineListRecord pR_PRLineListRecord = (PR_PRLineListRecord)enumerator.Current;
                    #region Add by liufei
                    ids += pR_PRLineListRecord.ID.ToString() + ",";
                    #endregion
                    if (!(pR_PRLineListRecord.ToPOQtyTU * pR_PRLineListRecord.TUToTBURate + pR_PRLineListRecord.ToPOQtyTBU <= 0m))
                    {
                        if (!pR_PRLineListRecord.ItemInfo_ItemID_PurchaseInfo_IsNeedInquire || !(pR_PRLineListRecord.ItemInfo_ItemID_PurchaseInfo_InquireRule == InquireRuleEnum.EveryTime.Value))
                        {
                            if ((!(pR_PRLineListRecord.ItemInfo_ItemID_PurchaseInfo_PurchaseQuotaMode == 6) && pR_PRLineListRecord.BizType != BusinessTypeEnum.PM050.Value) || !pR_PRLineListRecord.SuggestedSupplier_Supplier.HasValue || !pR_PRLineListRecord.SuggestedSupplier_Supplier.HasValue || pR_PRLineListRecord.SuggestedSupplier_Supplier.Value <= 0L)
                            {
                                //QuotaReqDTOData item = this.CreateQuotaReqDTOData(pR_PRLineListRecord, num);
                                QuotaReqDTOData item = prToPO.CreateQuotaReqDTOData(pR_PRLineListRecord, num);
                                getSupplierQuotaSVProxy.QuotaReqList.Add(item);
                                dictionary.Add(pR_PRLineListRecord.ID, num);
                                num++;
                            }
                        }
                    }
                }
            }
            List<QuotaDTOData> list = getSupplierQuotaSVProxy.Do();
            #region Add by liufei
            DataParamList dp = new DataParamList();
            ids = ids.Substring(0, ids.Length - 1);
            dp.Add(DataParamFactory.Create("PRIDS", ids, ParameterDirection.Input, DbType.String, 8000));
            dp.Add(DataParamFactory.CreateOutput("Result", DbType.String));
            DataSet ds = new DataSet();
            DataAccessor.RunSP("sp_Auctus_UI_GetSupplySource", dp, out ds);
            //按固定比例分配的料品是否存无厂商价目表、无供应商-料品交叉数据（即无MPQ）
            string result = dp["Result"].Value.ToString();
            if (result != "0")
            {
                throw new Exception(PRToPOUIModelRes.DataError + result);
            }
            #endregion
            Dictionary<int, List<QuotaDTOData>> dictionary2 = new Dictionary<int, List<QuotaDTOData>>();
            foreach (QuotaDTOData current in list)
            {
                if (dictionary2.ContainsKey(current.Index_1))
                {
                    dictionary2[current.Index_1].Add(current);
                }
                else
                {
                    List<QuotaDTOData> list2 = new List<QuotaDTOData>();
                    list2.Add(current);
                    dictionary2.Add(current.Index_1, list2);
                }
            }
            using (IEnumerator<IUIRecord> enumerator = selectRecordFromCache.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    PR_PRLineListRecord pR_PRLineListRecord = (PR_PRLineListRecord)enumerator.Current;
                    if (!(pR_PRLineListRecord.ToPOQtyTU * pR_PRLineListRecord.TUToTBURate + pR_PRLineListRecord.ToPOQtyTBU <= 0m))
                    {
                        if (!pR_PRLineListRecord.ItemInfo_ItemID_PurchaseInfo_IsNeedInquire || !(pR_PRLineListRecord.ItemInfo_ItemID_PurchaseInfo_InquireRule == InquireRuleEnum.EveryTime.Value))
                        {
                            if (text == "")
                            {
                                text = pR_PRLineListRecord.ID.ToString();
                            }
                            else
                            {
                                text = text + "," + pR_PRLineListRecord.ID.ToString();
                            }
                            if ((pR_PRLineListRecord.ItemInfo_ItemID_PurchaseInfo_PurchaseQuotaMode == 6 || pR_PRLineListRecord.BizType == BusinessTypeEnum.PM050.Value) && pR_PRLineListRecord.SuggestedSupplier_Supplier.HasValue && pR_PRLineListRecord.SuggestedSupplier_Supplier.HasValue && pR_PRLineListRecord.SuggestedSupplier_Supplier.Value > 0L)
                            {
                                //TryCalcViewRecord tryCalcViewRecord = this.CurrentModel.TryCalcView.AddNewUIRecord();
                                TryCalcViewRecord tryCalcViewRecord = prToPO.CurrentModel.TryCalcView.AddNewUIRecord();
                                tryCalcViewRecord.RFQStatus = pR_PRLineListRecord.RFQStatus;
                                tryCalcViewRecord.RFQ_SrcDocNo = pR_PRLineListRecord.RFQInfo_SrcDocNo;
                                tryCalcViewRecord.Currency = pR_PRLineListRecord.PR_AC;
                                tryCalcViewRecord.LineEntityID = new long?(pR_PRLineListRecord.ID);
                                tryCalcViewRecord.ItemCode = pR_PRLineListRecord.ItemInfo_ItemCode;
                                tryCalcViewRecord.ItemID = pR_PRLineListRecord.ItemInfo_ItemID;
                                tryCalcViewRecord.ItemName = pR_PRLineListRecord.ItemInfo_ItemName;
                                tryCalcViewRecord.ItemIsDual = new bool?(pR_PRLineListRecord.ItemInfo_ItemID_IsDualQuantity);
                                tryCalcViewRecord.PRDocLineNo = new long?((long)pR_PRLineListRecord.DocLineNo);
                                tryCalcViewRecord.RequiredDeliveryDate = pR_PRLineListRecord.RequiredDeliveryDate;
                                tryCalcViewRecord.Supplier = pR_PRLineListRecord.SuggestedSupplier_Supplier;
                                tryCalcViewRecord.Supplier_Code = pR_PRLineListRecord.SuggestedSupplier_Code;
                                tryCalcViewRecord.Supplier_Name = pR_PRLineListRecord.SuggestedSupplier_Name;
                                tryCalcViewRecord.SPECS = pR_PRLineListRecord.ItemInfo_ItemID_SPECS;
                                tryCalcViewRecord.DemandCode = pR_PRLineListRecord.DemandCode;
                                if (pR_PRLineListRecord.BizType == BusinessTypeEnum.PM050.Value)
                                {
                                    tryCalcViewRecord.ToPOQtyTBU = 0m;
                                    tryCalcViewRecord.TradeBaseUom = new long?(0L);
                                    tryCalcViewRecord.TradeBaseUom_Code = "";
                                    tryCalcViewRecord.TradeBaseUom_Name = "";
                                    tryCalcViewRecord.TradeBaseUOM_Round_Precision = 0;
                                    tryCalcViewRecord.TradeBaseUOM_Round_RoundType = 0;
                                    tryCalcViewRecord.TradeBaseUOM_Round_RoundValue = new int?(0);
                                }
                                else
                                {
                                    tryCalcViewRecord.ToPOQtyTBU = pR_PRLineListRecord.ToPOQtyTBU;
                                    tryCalcViewRecord.TradeBaseUom = pR_PRLineListRecord.TradeBaseUOM;
                                    tryCalcViewRecord.TradeBaseUom_Code = pR_PRLineListRecord.TradeBaseUOM_Code;
                                    tryCalcViewRecord.TradeBaseUom_Name = pR_PRLineListRecord.TradeBaseUOM_Name;
                                    tryCalcViewRecord.TradeBaseUOM_Round_Precision = pR_PRLineListRecord.TradeBaseUOM_Round_Precision;
                                    tryCalcViewRecord.TradeBaseUOM_Round_RoundType = pR_PRLineListRecord.TradeBaseUOM_Round_RoundType;
                                    tryCalcViewRecord.TradeBaseUOM_Round_RoundValue = pR_PRLineListRecord.TradeBaseUOM_Round_RoundValue;
                                }
                                tryCalcViewRecord.ToPOQtyTU = pR_PRLineListRecord.ToPOQtyTU;
                                tryCalcViewRecord.TradeUom = pR_PRLineListRecord.TradeUOM;
                                tryCalcViewRecord.TradeUom_Code = pR_PRLineListRecord.TradeUOM_Code;
                                tryCalcViewRecord.TradeUom_Name = pR_PRLineListRecord.TradeUOM_Name;
                                tryCalcViewRecord.TradeUOM_Round_Precision = pR_PRLineListRecord.TradeUOM_Round_Precision;
                                tryCalcViewRecord.TradeUOM_Round_RoundType = pR_PRLineListRecord.TradeUOM_Round_RoundType;
                                tryCalcViewRecord.TradeUOM_Round_RoundValue = pR_PRLineListRecord.TradeUOM_Round_RoundValue;
                                tryCalcViewRecord.BizType = pR_PRLineListRecord.BizType;
                                if (pR_PRLineListRecord.ItemInfo_ItemID_ItemFormAttribute == 16)
                                {
                                    tryCalcViewRecord.IsMisc = new bool?(true);
                                }
                                else
                                {
                                    tryCalcViewRecord.IsMisc = new bool?(false);
                                }
                            }
                            else
                            {
                                try
                                {
                                    if (dictionary.ContainsKey(pR_PRLineListRecord.ID) && dictionary2.ContainsKey(dictionary[pR_PRLineListRecord.ID]))
                                    {
                                        List<QuotaDTOData> list2 = dictionary2[dictionary[pR_PRLineListRecord.ID]];
                                        #region ADD By liufei
                                        //固定比例分配模式的请购单
                                        if (pR_PRLineListRecord.ItemInfo_ItemID_PurchaseInfo_PurchaseQuotaMode == 4)//固定比例分配模式
                                        {
                                            DataRow[] dr = ds.Tables[0].Select("PRLineID=" + pR_PRLineListRecord.ID);
                                            if (dr.Length > 0)
                                            {
                                                foreach (DataRow item in dr)
                                                {
                                                    if (Convert.ToDecimal(item["ActualReq"]) == 0)
                                                    {
                                                        list2.Remove(list2.Where(m => m.Supplier == Convert.ToInt64(item["Supplier"]) && m.SupplySource == Convert.ToInt64(item["SupplySource"])).Select(m => m).First());
                                                    }
                                                    else
                                                    {
                                                        for (int i = 0; i < list2.Count; i++)
                                                        {
                                                            if (item["Supplier"].ToString() == list2[i].Supplier.ToString() && item["SupplySource"].ToString() == list2[i].SupplySource.ToString())
                                                            {
                                                                list2[i].PurNum.Amount1 = Convert.ToDecimal(item["ActualReq"]);
                                                            }
                                                        }
                                                    }

                                                }
                                            }
                                        }
                                        #endregion
                                        foreach (QuotaDTOData current2 in list2)
                                        {
                                            if (current2.PurNum.Amount1 == 0m && current2.PurNum.Amount2 == 0m)
                                            {
                                                throw new Exception(PRToPOUIModelRes.SupQtyZeroCannotGenPO);
                                            }
                                            //TryCalcViewRecord tryCalcViewRecord = this.CurrentModel.TryCalcView.AddNewUIRecord();
                                            TryCalcViewRecord tryCalcViewRecord = prToPO.CurrentModel.TryCalcView.AddNewUIRecord();
                                            tryCalcViewRecord.RFQStatus = pR_PRLineListRecord.RFQStatus;
                                            tryCalcViewRecord.RFQ_SrcDocNo = pR_PRLineListRecord.RFQInfo_SrcDocNo;
                                            tryCalcViewRecord.Currency = pR_PRLineListRecord.PR_AC;
                                            tryCalcViewRecord.ItemID = pR_PRLineListRecord.ItemInfo_ItemID;
                                            tryCalcViewRecord.ItemCode = pR_PRLineListRecord.ItemInfo_ItemCode;
                                            tryCalcViewRecord.ItemName = pR_PRLineListRecord.ItemInfo_ItemName;
                                            tryCalcViewRecord.ItemIsDual = new bool?(pR_PRLineListRecord.ItemInfo_ItemID_IsDualQuantity);
                                            tryCalcViewRecord.PRDocLineNo = new long?((long)pR_PRLineListRecord.DocLineNo);
                                            tryCalcViewRecord.RequiredDeliveryDate = pR_PRLineListRecord.RequiredDeliveryDate;
                                            tryCalcViewRecord.TradeUom = pR_PRLineListRecord.TradeUOM;
                                            tryCalcViewRecord.TradeUom_Code = pR_PRLineListRecord.TradeUOM_Code;
                                            tryCalcViewRecord.TradeUom_Name = pR_PRLineListRecord.TradeUOM_Name;
                                            tryCalcViewRecord.LineEntityID = new long?(pR_PRLineListRecord.ID);
                                            tryCalcViewRecord.TradeUOM_Round_Precision = pR_PRLineListRecord.TradeUOM_Round_Precision;
                                            tryCalcViewRecord.TradeUOM_Round_RoundType = pR_PRLineListRecord.TradeUOM_Round_RoundType;
                                            tryCalcViewRecord.TradeUOM_Round_RoundValue = pR_PRLineListRecord.TradeUOM_Round_RoundValue;
                                            tryCalcViewRecord.SPECS = pR_PRLineListRecord.ItemInfo_ItemID_SPECS;
                                            tryCalcViewRecord.DemandCode = pR_PRLineListRecord.DemandCode;
                                            if (pR_PRLineListRecord.BizType == BusinessTypeEnum.PM050.Value)
                                            {
                                                tryCalcViewRecord.ToPOQtyTBU = 0m;
                                                tryCalcViewRecord.TradeBaseUom = new long?(0L);
                                                tryCalcViewRecord.TradeBaseUom_Code = "";
                                                tryCalcViewRecord.TradeBaseUom_Name = "";
                                                tryCalcViewRecord.TradeBaseUOM_Round_Precision = 0;
                                                tryCalcViewRecord.TradeBaseUOM_Round_RoundType = 0;
                                                tryCalcViewRecord.TradeBaseUOM_Round_RoundValue = new int?(0);
                                                if (pR_PRLineListRecord.ItemInfo_ItemID_IsDualQuantity)
                                                {
                                                    tryCalcViewRecord.ToPOQtyTU = current2.PurNum.Amount1;
                                                }
                                                else
                                                {
                                                    tryCalcViewRecord.ToPOQtyTU = current2.PurNum.Amount1;
                                                }
                                            }
                                            else
                                            {
                                                tryCalcViewRecord.TradeBaseUom = pR_PRLineListRecord.TradeBaseUOM;
                                                tryCalcViewRecord.TradeBaseUom_Code = pR_PRLineListRecord.TradeBaseUOM_Code;
                                                tryCalcViewRecord.TradeBaseUom_Name = pR_PRLineListRecord.TradeBaseUOM_Name;
                                                tryCalcViewRecord.TradeBaseUOM_Round_Precision = pR_PRLineListRecord.TradeBaseUOM_Round_Precision;
                                                tryCalcViewRecord.TradeBaseUOM_Round_RoundType = pR_PRLineListRecord.TradeBaseUOM_Round_RoundType;
                                                tryCalcViewRecord.TradeBaseUOM_Round_RoundValue = pR_PRLineListRecord.TradeBaseUOM_Round_RoundValue;
                                                if (pR_PRLineListRecord.ItemInfo_ItemID_IsDualQuantity)
                                                {
                                                    tryCalcViewRecord.ToPOQtyTBU = current2.PurNum.Amount2;
                                                    tryCalcViewRecord.ToPOQtyTU = current2.PurNum.Amount1;
                                                }
                                                else
                                                {
                                                    tryCalcViewRecord.ToPOQtyTBU = 0m;
                                                    tryCalcViewRecord.ToPOQtyTU = current2.PurNum.Amount1;
                                                }
                                            }
                                            tryCalcViewRecord.BizType = pR_PRLineListRecord.BizType;
                                            tryCalcViewRecord.Supplier = new long?(current2.Supplier);
                                            tryCalcViewRecord.Supplier_Code = current2.SupplierCode;
                                            tryCalcViewRecord.Supplier_Name = current2.SupplierName;
                                        }
                                    }
                                    else
                                    {
                                        //TryCalcViewRecord tryCalcViewRecord = this.CurrentModel.TryCalcView.AddNewUIRecord();
                                        TryCalcViewRecord tryCalcViewRecord = prToPO.CurrentModel.TryCalcView.AddNewUIRecord();
                                        tryCalcViewRecord.RFQStatus = pR_PRLineListRecord.RFQStatus;
                                        tryCalcViewRecord.RFQ_SrcDocNo = pR_PRLineListRecord.RFQInfo_SrcDocNo;
                                        tryCalcViewRecord.Currency = pR_PRLineListRecord.PR_AC;
                                        tryCalcViewRecord.LineEntityID = new long?(pR_PRLineListRecord.ID);
                                        tryCalcViewRecord.ItemCode = pR_PRLineListRecord.ItemInfo_ItemCode;
                                        tryCalcViewRecord.ItemID = pR_PRLineListRecord.ItemInfo_ItemID;
                                        tryCalcViewRecord.ItemName = pR_PRLineListRecord.ItemInfo_ItemName;
                                        tryCalcViewRecord.ItemIsDual = new bool?(pR_PRLineListRecord.ItemInfo_ItemID_IsDualQuantity);
                                        tryCalcViewRecord.PRDocLineNo = new long?((long)pR_PRLineListRecord.DocLineNo);
                                        tryCalcViewRecord.RequiredDeliveryDate = pR_PRLineListRecord.RequiredDeliveryDate;
                                        tryCalcViewRecord.SPECS = pR_PRLineListRecord.ItemInfo_ItemID_SPECS;
                                        tryCalcViewRecord.DemandCode = pR_PRLineListRecord.DemandCode;
                                        if (pR_PRLineListRecord.BizType == BusinessTypeEnum.PM050.Value)
                                        {
                                            tryCalcViewRecord.ToPOQtyTBU = 0m;
                                            tryCalcViewRecord.TradeBaseUom = new long?(0L);
                                            tryCalcViewRecord.TradeBaseUom_Code = "";
                                            tryCalcViewRecord.TradeBaseUom_Name = "";
                                            tryCalcViewRecord.TradeBaseUOM_Round_Precision = 0;
                                            tryCalcViewRecord.TradeBaseUOM_Round_RoundType = 0;
                                            tryCalcViewRecord.TradeBaseUOM_Round_RoundValue = new int?(0);
                                        }
                                        else
                                        {
                                            tryCalcViewRecord.TradeBaseUom = pR_PRLineListRecord.TradeBaseUOM;
                                            tryCalcViewRecord.TradeBaseUom_Code = pR_PRLineListRecord.TradeBaseUOM_Code;
                                            tryCalcViewRecord.TradeBaseUom_Name = pR_PRLineListRecord.TradeBaseUOM_Name;
                                            tryCalcViewRecord.TradeBaseUOM_Round_Precision = pR_PRLineListRecord.TradeBaseUOM_Round_Precision;
                                            tryCalcViewRecord.TradeBaseUOM_Round_RoundType = pR_PRLineListRecord.TradeBaseUOM_Round_RoundType;
                                            tryCalcViewRecord.TradeBaseUOM_Round_RoundValue = pR_PRLineListRecord.TradeBaseUOM_Round_RoundValue;
                                            tryCalcViewRecord.ToPOQtyTBU = pR_PRLineListRecord.ToPOQtyTBU;
                                        }
                                        tryCalcViewRecord.ToPOQtyTU = pR_PRLineListRecord.ToPOQtyTU;
                                        tryCalcViewRecord.TradeUom = pR_PRLineListRecord.TradeUOM;
                                        tryCalcViewRecord.TradeUom_Code = pR_PRLineListRecord.TradeUOM_Code;
                                        tryCalcViewRecord.TradeUom_Name = pR_PRLineListRecord.TradeUOM_Name;
                                        tryCalcViewRecord.TradeUOM_Round_Precision = pR_PRLineListRecord.TradeUOM_Round_Precision;
                                        tryCalcViewRecord.TradeUOM_Round_RoundType = pR_PRLineListRecord.TradeUOM_Round_RoundType;
                                        tryCalcViewRecord.TradeUOM_Round_RoundValue = pR_PRLineListRecord.TradeUOM_Round_RoundValue;
                                        tryCalcViewRecord.BizType = pR_PRLineListRecord.BizType;
                                        if (pR_PRLineListRecord.ItemInfo_ItemID_ItemFormAttribute == 16)
                                        {
                                            tryCalcViewRecord.IsMisc = new bool?(true);
                                        }
                                        else
                                        {
                                            tryCalcViewRecord.IsMisc = new bool?(false);
                                        }
                                        //this.SetErrorMsg(tryCalcViewRecord, "Supplier", string.Concat(new string[]
                                        prToPO.SetErrorMsg(tryCalcViewRecord, "Supplier", string.Concat(new string[]
                                        {
                                                PRToPOUIModelRes.LineNum1,
                                                tryCalcViewRecord.PRDocLineNo.GetValueOrDefault().ToString(),
                                                PRToPOUIModelRes.LineNum2,
                                                PRToPOUIModelRes.ItemCode,
                                                tryCalcViewRecord.ItemCode,
                                                PRToPOUIModelRes.SupplierBeNull
                                        }));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }
                            }
                        }
                    }
                }
            }
            string partId = "b7345fdb-dab2-4b1a-b380-af6e4d2b285c";
            //string parentTaskId = base.CurrentPart.TaskId.ToString();
            string parentTaskId = prToPO.CurrentPart.TaskId.ToString();
            string width = "810";
            string height = "465";
            //base.CurrentState["PRToPO_SelectedPrLineIDs"] = text;
            prToPO.CurrentState["PRToPO_SelectedPrLineIDs"] = text;
            //base.CurrentPart.ShowModalDialog(partId, PRToPOUIModelRes.TryCalc, width, height, parentTaskId, null, false, true);
            prToPO.CurrentPart.ShowModalDialog(partId, PRToPOUIModelRes.TryCalc, width, height, parentTaskId, null, false, true);
        }

    }
}