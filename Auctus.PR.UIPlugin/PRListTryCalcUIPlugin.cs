using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UFIDA.U9.Base.Doc;
using UFIDA.U9.CBO.DTOs;
using UFIDA.U9.CBO.SCM.Item;
using UFIDA.U9.CBO.SCM.Supplier;
using UFIDA.U9.PM.PMPubSV.Proxy;
using UFIDA.U9.SCM.PM.ListPRToPOUIModel;
using UFSoft.UBF.UI.ActionProcess;
using UFSoft.UBF.UI.ControlModel;
using UFSoft.UBF.UI.IView;
using UFSoft.UBF.UI.MD.Runtime;
using UFSoft.UBF.UI.WebControlAdapter;
using UFSoft.UBF.Util.DataAccess;

namespace Auctus.PR.UIPlugin
{
    /// <summary>
    /// 请购单列表页面的转PO弹出框中的PR转PO试算按钮
    /// </summary>
    public class PRListTryCalcUIPlugin : UFSoft.UBF.UI.Custom.ExtendedPartBase
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
            IUFButton btnTryCalcList2 = new UFWebButtonAdapter();
            btnTryCalcList2.Text = "比例试算";
            btnTryCalcList2.ID = "btnTryCalc2";
            btnTryCalcList2.AutoPostBack = true;
            btnTryCalcList2.ToolTip = "比例试算";
            btnTryCalcList2.Visible = true;

            // 将按钮加入到按钮栏
            IUFCard iCard = (IUFCard)Part.GetUFControlByName(Part.TopLevelContainer, "Card1");
            iCard.Controls.Add(btnTryCalcList2);
            CommonFunction.Layout(iCard, btnTryCalcList2, 5, 0);
            btnTryCalcList2.Click += new EventHandler(BtnTryCalcList2_Click);
        }
        #endregion
        /// <summary>
        /// 创建只变交期的采购变更单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnTryCalcList2_Click(object sender, EventArgs e)
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
            ListPRToPOUIModelAction listPRToPO = ((ListPRToPOUIModelAction)this.iPart.Action);
            listPRToPO.ClearErrorMessage();
            listPRToPO.CalAmountAndSetRFQForCalcBtn();
			IList<IUIRecord> selectRecordFromCache = UIRuntimeHelper.Instance.GetSelectRecordFromCache(listPRToPO.CurrentModel.PRLine);
			if (selectRecordFromCache == null || selectRecordFromCache.Count == 0)
			{
				throw new Exception(UFIDA.U9.SCM.PM.PRToPOUIModel.PRToPOUIModelRes.NoSelectedRecord);
			}
            listPRToPO.CurrentModel.TryCalcView.Records.Clear();
			string text = "";
			GetSupplierQuotaSVProxy getSupplierQuotaSVProxy = new GetSupplierQuotaSVProxy();
			getSupplierQuotaSVProxy.QuotaReqList = new List<QuotaReqDTOData>();
			int num = 1;
			Dictionary<long, int> dictionary = new Dictionary<long, int>();
			using (IEnumerator<IUIRecord> enumerator = selectRecordFromCache.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					PRLineRecord pRLineRecord = (PRLineRecord)enumerator.Current;
                    #region Add by liufei
                    ids += pRLineRecord.ID.ToString() + ",";
                    #endregion
                    if (!(pRLineRecord.ToPOQtyTU * pRLineRecord.TUToTBURate + pRLineRecord.ToPOQtyTBU <= 0m))
					{
						if (!pRLineRecord.ItemInfo_ItemID_PurchaseInfo_IsNeedInquire || !(pRLineRecord.ItemInfo_ItemID_PurchaseInfo_InquireRule == InquireRuleEnum.EveryTime.Value))
						{
							if ((!(pRLineRecord.ItemInfo_ItemID_PurchaseInfo_PurchaseQuotaMode == 6) && pRLineRecord.BizType != BusinessTypeEnum.PM050.Value) || !pRLineRecord.SuggestedSupplier_Supplier.HasValue || !pRLineRecord.SuggestedSupplier_Supplier.HasValue || pRLineRecord.SuggestedSupplier_Supplier.Value <= 0L)
							{
								QuotaReqDTOData quotaReqDTOData = new QuotaReqDTOData();
								quotaReqDTOData.Item = new ItemInfoData();
								if (pRLineRecord.BizType == BusinessTypeEnum.PM050.Value)
								{
									quotaReqDTOData.Item.ItemCode = pRLineRecord.ProcessItem_ItemCode;
									quotaReqDTOData.Item.ItemID = pRLineRecord.ProcessItem_ItemID.Value;
									quotaReqDTOData.Item.ItemName = pRLineRecord.ProcessItem_ItemName;
									if (pRLineRecord.ProcessItem_ItemOpt1.HasValue && pRLineRecord.ProcessItem_ItemOpt1.HasValue)
									{
										quotaReqDTOData.Item.ItemOpt1 = pRLineRecord.ProcessItem_ItemOpt1.Value;
									}
									if (pRLineRecord.ProcessItem_ItemOpt2.HasValue && pRLineRecord.ProcessItem_ItemOpt2.HasValue)
									{
										quotaReqDTOData.Item.ItemOpt2 = pRLineRecord.ProcessItem_ItemOpt2.Value;
									}
									if (pRLineRecord.ProcessItem_ItemOpt3.HasValue && pRLineRecord.ProcessItem_ItemOpt3.HasValue)
									{
										quotaReqDTOData.Item.ItemOpt3 = pRLineRecord.ProcessItem_ItemOpt3.Value;
									}
									if (pRLineRecord.ProcessItem_ItemOpt4.HasValue && pRLineRecord.ProcessItem_ItemOpt4.HasValue)
									{
										quotaReqDTOData.Item.ItemOpt4 = pRLineRecord.ProcessItem_ItemOpt4.Value;
									}
									if (pRLineRecord.ProcessItem_ItemOpt5.HasValue && pRLineRecord.ProcessItem_ItemOpt5.HasValue)
									{
										quotaReqDTOData.Item.ItemOpt5 = pRLineRecord.ProcessItem_ItemOpt5.Value;
									}
									if (pRLineRecord.ProcessItem_ItemOpt6.HasValue && pRLineRecord.ProcessItem_ItemOpt6.HasValue)
									{
										quotaReqDTOData.Item.ItemOpt6 = pRLineRecord.ProcessItem_ItemOpt6.Value;
									}
									if (pRLineRecord.ProcessItem_ItemOpt7.HasValue && pRLineRecord.ProcessItem_ItemOpt7.HasValue)
									{
										quotaReqDTOData.Item.ItemOpt7 = pRLineRecord.ProcessItem_ItemOpt7.Value;
									}
									if (pRLineRecord.ProcessItem_ItemOpt8.HasValue && pRLineRecord.ProcessItem_ItemOpt8.HasValue)
									{
										quotaReqDTOData.Item.ItemOpt8 = pRLineRecord.ProcessItem_ItemOpt8.Value;
									}
									if (pRLineRecord.ProcessItem_ItemOpt9.HasValue && pRLineRecord.ProcessItem_ItemOpt9.HasValue)
									{
										quotaReqDTOData.Item.ItemOpt9 = pRLineRecord.ProcessItem_ItemOpt9.Value;
									}
									if (pRLineRecord.ProcessItem_ItemOpt10.HasValue && pRLineRecord.ProcessItem_ItemOpt10.HasValue)
									{
										quotaReqDTOData.Item.ItemOpt10 = pRLineRecord.ProcessItem_ItemOpt10.Value;
									}
									if (pRLineRecord.ProcessItem_ItemPotency.HasValue && pRLineRecord.ProcessItem_ItemPotency.HasValue)
									{
										quotaReqDTOData.Item.ItemPotency = pRLineRecord.ProcessItem_ItemPotency.Value;
									}
									if (pRLineRecord.ProcessItem_ItemGrade.HasValue && pRLineRecord.ProcessItem_ItemGrade.HasValue)
									{
										quotaReqDTOData.Item.ItemGrade = pRLineRecord.ProcessItem_ItemGrade.Value;
									}
									if (pRLineRecord.ProcessItem_ItemVersion != null)
									{
										quotaReqDTOData.Item.ItemVersion = pRLineRecord.ProcessItem_ItemVersion;
									}
								}
								else
								{
									quotaReqDTOData.Item.ItemCode = pRLineRecord.ItemInfo_ItemCode;
									quotaReqDTOData.Item.ItemID = pRLineRecord.ItemInfo_ItemID.Value;
									quotaReqDTOData.Item.ItemName = pRLineRecord.ItemInfo_ItemName;
									if (pRLineRecord.ItemInfo_ItemOpt1.HasValue && pRLineRecord.ItemInfo_ItemOpt1.HasValue)
									{
										quotaReqDTOData.Item.ItemOpt1 = pRLineRecord.ItemInfo_ItemOpt1.Value;
									}
									if (pRLineRecord.ItemInfo_ItemOpt2.HasValue && pRLineRecord.ItemInfo_ItemOpt2.HasValue)
									{
										quotaReqDTOData.Item.ItemOpt2 = pRLineRecord.ItemInfo_ItemOpt2.Value;
									}
									if (pRLineRecord.ItemInfo_ItemOpt3.HasValue && pRLineRecord.ItemInfo_ItemOpt3.HasValue)
									{
										quotaReqDTOData.Item.ItemOpt3 = pRLineRecord.ItemInfo_ItemOpt3.Value;
									}
									if (pRLineRecord.ItemInfo_ItemOpt4.HasValue && pRLineRecord.ItemInfo_ItemOpt4.HasValue)
									{
										quotaReqDTOData.Item.ItemOpt4 = pRLineRecord.ItemInfo_ItemOpt4.Value;
									}
									if (pRLineRecord.ItemInfo_ItemOpt5.HasValue && pRLineRecord.ItemInfo_ItemOpt5.HasValue)
									{
										quotaReqDTOData.Item.ItemOpt5 = pRLineRecord.ItemInfo_ItemOpt5.Value;
									}
									if (pRLineRecord.ItemInfo_ItemOpt6.HasValue && pRLineRecord.ItemInfo_ItemOpt6.HasValue)
									{
										quotaReqDTOData.Item.ItemOpt6 = pRLineRecord.ItemInfo_ItemOpt6.Value;
									}
									if (pRLineRecord.ItemInfo_ItemOpt7.HasValue && pRLineRecord.ItemInfo_ItemOpt7.HasValue)
									{
										quotaReqDTOData.Item.ItemOpt7 = pRLineRecord.ItemInfo_ItemOpt7.Value;
									}
									if (pRLineRecord.ItemInfo_ItemOpt8.HasValue && pRLineRecord.ItemInfo_ItemOpt8.HasValue)
									{
										quotaReqDTOData.Item.ItemOpt8 = pRLineRecord.ItemInfo_ItemOpt8.Value;
									}
									if (pRLineRecord.ItemInfo_ItemOpt9.HasValue && pRLineRecord.ItemInfo_ItemOpt9.HasValue)
									{
										quotaReqDTOData.Item.ItemOpt9 = pRLineRecord.ItemInfo_ItemOpt9.Value;
									}
									if (pRLineRecord.ItemInfo_ItemOpt10.HasValue && pRLineRecord.ItemInfo_ItemOpt10.HasValue)
									{
										quotaReqDTOData.Item.ItemOpt10 = pRLineRecord.ItemInfo_ItemOpt10.Value;
									}
									if (pRLineRecord.ItemInfo_ItemPotency.HasValue && pRLineRecord.ItemInfo_ItemPotency.HasValue)
									{
										quotaReqDTOData.Item.ItemPotency = pRLineRecord.ItemInfo_ItemPotency.Value;
									}
									if (pRLineRecord.ItemInfo_ItemGrade.HasValue && pRLineRecord.ItemInfo_ItemGrade.HasValue)
									{
										quotaReqDTOData.Item.ItemGrade = pRLineRecord.ItemInfo_ItemGrade.Value;
									}
									if (pRLineRecord.ItemInfo_ItemVersion != null)
									{
										quotaReqDTOData.Item.ItemVersion = pRLineRecord.ItemInfo_ItemVersion;
									}
								}
								quotaReqDTOData.PurType = 3;
								quotaReqDTOData.ReqAmount = new DoubleQuantityData();
								quotaReqDTOData.ReqAmount.Amount1 = pRLineRecord.ToPOQtyTU;
								quotaReqDTOData.ReqAmount.UOM1 = new UOMInfoDTOData();
								if (pRLineRecord.BizType == BusinessTypeEnum.PM050.Value)
								{
									quotaReqDTOData.ReqAmount.UOM1.UOMMain = pRLineRecord.PM050Uom.Value;
									quotaReqDTOData.ReqAmount.UOM1.UOMSub = pRLineRecord.PM050BaseUom.Value;
									quotaReqDTOData.UsageQuantityType = Convert.ToInt32(pRLineRecord.ChargeBasis);
									quotaReqDTOData.ItemEntityKey = pRLineRecord.ProcessItem_ItemID.GetValueOrDefault();
									quotaReqDTOData.PerProcessQty = pRLineRecord.RatioPerProduct.GetValueOrDefault();
									quotaReqDTOData.OperationQty = new DoubleQuantityData();
									quotaReqDTOData.OperationQty.UOM1 = new UOMInfoDTOData();
									quotaReqDTOData.OperationQty.UOM2 = new UOMInfoDTOData();
									quotaReqDTOData.OperationQty.Amount1 = pRLineRecord.PM050ForTOPOQtyTU.GetValueOrDefault();
									quotaReqDTOData.OperationQty.Amount2 = pRLineRecord.PM050ForTOPOQtyTBU.GetValueOrDefault();
									quotaReqDTOData.OperationQty.UOM1.UOMMain = pRLineRecord.PM050Uom.GetValueOrDefault();
									quotaReqDTOData.OperationQty.UOM1.UOMSub = pRLineRecord.PM050BaseUom.GetValueOrDefault();
									quotaReqDTOData.OperationQty.UOM1.Rate = pRLineRecord.TUToTBURate.GetValueOrDefault();
									quotaReqDTOData.OperationQty.UOM2.UOMMain = pRLineRecord.PM050BaseUom.GetValueOrDefault();
									quotaReqDTOData.OperationQty.UOM2.UOMSub = pRLineRecord.PM050BaseUom.GetValueOrDefault();
									quotaReqDTOData.OperationQty.UOM2.Rate = 1m;
								}
								else
								{
									quotaReqDTOData.ReqAmount.UOM1.UOMMain = pRLineRecord.TradeUOM.Value;
									quotaReqDTOData.ReqAmount.UOM1.UOMSub = pRLineRecord.TradeBaseUOM.Value;
								}
								quotaReqDTOData.ReqAmount.Amount2 = pRLineRecord.ToPOQtyTBU;
								quotaReqDTOData.ReqAmount.UOM1.Rate = pRLineRecord.TUToTBURate.Value;
								quotaReqDTOData.ReqTime = pRLineRecord.RequiredDeliveryDate.Value;
								if (pRLineRecord.SrcDocType.HasValue && pRLineRecord.SrcDocType.GetValueOrDefault(-1) == 1)
								{
									quotaReqDTOData.VMIType = 0;
								}
								else
								{
									quotaReqDTOData.VMIType = 2;
								}
								if (pRLineRecord.ItemInfo_ItemID_ItemFormAttribute == 0)
								{
									if (pRLineRecord.SelectedResult_EntityID.HasValue)
									{
										quotaReqDTOData.BOMID = pRLineRecord.SelectedResult_EntityID.GetValueOrDefault(-1L);
									}
								}
								else if (pRLineRecord.BomID.HasValue)
								{
									quotaReqDTOData.BOMID = pRLineRecord.BomID.GetValueOrDefault(-1L);
								}
								if (pRLineRecord.Mfc.HasValue && pRLineRecord.Mfc.GetValueOrDefault(-1L) > 0L)
								{
									quotaReqDTOData.Mfc = pRLineRecord.Mfc.Value;
								}
								quotaReqDTOData.Index_1 = num;
								if (pRLineRecord.SuggestedSupplier_Supplier.HasValue && pRLineRecord.SuggestedSupplier_Supplier.HasValue && pRLineRecord.SuggestedSupplier_Supplier.Value > 0L)
								{
									quotaReqDTOData.Supplier = pRLineRecord.SuggestedSupplier_Supplier.Value;
								}
								getSupplierQuotaSVProxy.QuotaReqList.Add(quotaReqDTOData);
								dictionary.Add(pRLineRecord.ID, num);
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
            dp.Add(DataParamFactory.Create("PRIDS", ids, ParameterDirection.Input, DbType.String, 50000));
            dp.Add(DataParamFactory.CreateOutput("Result", DbType.String));
            DataSet ds = new DataSet();
            DataAccessor.RunSP("sp_Auctus_UI_GetSupplySource", dp, out ds);
            //按固定比例分配的料品是否存无厂商价目表、无供应商-料品交叉数据（即无MPQ）
            string result = dp["Result"].Value.ToString();
            if (result != "0")
            {
                throw new Exception(UFIDA.U9.SCM.PM.PRToPOUIModel.PRToPOUIModelRes.DataError + result);
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
					PRLineRecord pRLineRecord = (PRLineRecord)enumerator.Current;
					if (!(pRLineRecord.ToPOQtyTU * pRLineRecord.TUToTBURate + pRLineRecord.ToPOQtyTBU <= 0m))
					{
						if (!pRLineRecord.ItemInfo_ItemID_PurchaseInfo_IsNeedInquire || !(pRLineRecord.ItemInfo_ItemID_PurchaseInfo_InquireRule == InquireRuleEnum.EveryTime.Value))
						{
							if (text == "")
							{
								text = pRLineRecord.ID.ToString();
							}
							else
							{
								text = text + "," + pRLineRecord.ID.ToString();
							}
							if ((pRLineRecord.ItemInfo_ItemID_PurchaseInfo_PurchaseQuotaMode == 6 || pRLineRecord.BizType == BusinessTypeEnum.PM050.Value) && pRLineRecord.SuggestedSupplier_Supplier.HasValue && pRLineRecord.SuggestedSupplier_Supplier.HasValue && pRLineRecord.SuggestedSupplier_Supplier.Value > 0L)
							{
								TryCalcViewRecord tryCalcViewRecord = listPRToPO.CurrentModel.TryCalcView.AddNewUIRecord();
								tryCalcViewRecord.RFQStatus = pRLineRecord.RFQStatus;
								tryCalcViewRecord.RFQ_SrcDocNo = pRLineRecord.RFQInfo_SrcDocNo;
								tryCalcViewRecord.Currency = pRLineRecord.PR_AC;
								tryCalcViewRecord.PRDocNo = pRLineRecord.PR_DocNo;
								tryCalcViewRecord.LineEntityID = new long?(pRLineRecord.ID);
								tryCalcViewRecord.ItemCode = pRLineRecord.ItemInfo_ItemCode;
								tryCalcViewRecord.ItemID = pRLineRecord.ItemInfo_ItemID;
								tryCalcViewRecord.ItemName = pRLineRecord.ItemInfo_ItemName;
								tryCalcViewRecord.ItemIsDual = new bool?(pRLineRecord.ItemInfo_ItemID_IsDualQuantity);
								tryCalcViewRecord.PRDocLineNo = new long?((long)pRLineRecord.DocLineNo);
								tryCalcViewRecord.RequiredDeliveryDate = pRLineRecord.RequiredDeliveryDate;
								tryCalcViewRecord.Supplier = pRLineRecord.SuggestedSupplier_Supplier;
								tryCalcViewRecord.Supplier_Code = pRLineRecord.SuggestedSupplier_Code;
								tryCalcViewRecord.Supplier_Name = pRLineRecord.SuggestedSupplier_Name;
								tryCalcViewRecord.DemandCode = pRLineRecord.DemandCode;
								tryCalcViewRecord.SPECS = pRLineRecord.ItemInfo_ItemID_SPECS;
								if (pRLineRecord.BizType == BusinessTypeEnum.PM050.Value)
								{
									tryCalcViewRecord.ToPOQtyTBU = 0m;
									tryCalcViewRecord.TradeBaseUom = new long?(0L);
									tryCalcViewRecord.TradeBaseUom_Code = "";
									tryCalcViewRecord.TradeBaseUom_Name = "";
									tryCalcViewRecord.TradeBaseUOM_Round_Precision = new int?(0);
									tryCalcViewRecord.TradeBaseUOM_Round_RoundType = new int?(0);
									tryCalcViewRecord.TradeBaseUOM_Round_RoundValue = new int?(0);
								}
								else
								{
									tryCalcViewRecord.ToPOQtyTBU = pRLineRecord.ToPOQtyTBU;
									tryCalcViewRecord.TradeBaseUom = pRLineRecord.TradeBaseUOM;
									tryCalcViewRecord.TradeBaseUom_Code = pRLineRecord.TradeBaseUOM_Code;
									tryCalcViewRecord.TradeBaseUom_Name = pRLineRecord.TradeBaseUOM_Name;
									tryCalcViewRecord.TradeBaseUOM_Round_Precision = new int?(pRLineRecord.TradeBaseUOM_Round_Precision);
									tryCalcViewRecord.TradeBaseUOM_Round_RoundType = new int?(pRLineRecord.TradeBaseUOM_Round_RoundType);
									tryCalcViewRecord.TradeBaseUOM_Round_RoundValue = pRLineRecord.TradeBaseUOM_Round_RoundValue;
								}
								tryCalcViewRecord.ToPOQtyTU = pRLineRecord.ToPOQtyTU;
								tryCalcViewRecord.TradeUom = pRLineRecord.TradeUOM;
								tryCalcViewRecord.TradeUom_Code = pRLineRecord.TradeUOM_Code;
								tryCalcViewRecord.TradeUom_Name = pRLineRecord.TradeUOM_Name;
								tryCalcViewRecord.TradeUOM_Round_Precision = new int?(pRLineRecord.TradeUOM_Round_Precision);
								tryCalcViewRecord.TradeUOM_Round_RoundType = new int?(pRLineRecord.TradeUOM_Round_RoundType);
								tryCalcViewRecord.TradeUOM_Round_RoundValue = pRLineRecord.TradeUOM_Round_RoundValue;
								tryCalcViewRecord.BizType = pRLineRecord.BizType;
								if (pRLineRecord.ItemInfo_ItemID_ItemFormAttribute == 16)
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
									if (dictionary.ContainsKey(pRLineRecord.ID) && dictionary2.ContainsKey(dictionary[pRLineRecord.ID]))
									{
										List<QuotaDTOData> list2 = dictionary2[dictionary[pRLineRecord.ID]];
                                        #region ADD By liufei
                                        //固定比例分配模式的请购单
                                        if (pRLineRecord.ItemInfo_ItemID_PurchaseInfo_PurchaseQuotaMode == 4)//固定比例分配模式
                                        {
                                            DataRow[] dr = ds.Tables[0].Select("PRLineID=" + pRLineRecord.ID);
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
												throw new Exception(UFIDA.U9.SCM.PM.PRToPOUIModel.PRToPOUIModelRes.SupQtyZeroCannotGenPO);
											}
											TryCalcViewRecord tryCalcViewRecord = listPRToPO.CurrentModel.TryCalcView.AddNewUIRecord();
											tryCalcViewRecord.RFQStatus = pRLineRecord.RFQStatus;
											tryCalcViewRecord.RFQ_SrcDocNo = pRLineRecord.RFQInfo_SrcDocNo;
											tryCalcViewRecord.Currency = pRLineRecord.PR_AC;
											tryCalcViewRecord.PRDocNo = pRLineRecord.PR_DocNo;
											tryCalcViewRecord.ItemID = pRLineRecord.ItemInfo_ItemID;
											tryCalcViewRecord.ItemCode = pRLineRecord.ItemInfo_ItemCode;
											tryCalcViewRecord.ItemName = pRLineRecord.ItemInfo_ItemName;
											tryCalcViewRecord.ItemIsDual = new bool?(pRLineRecord.ItemInfo_ItemID_IsDualQuantity);
											tryCalcViewRecord.PRDocLineNo = new long?((long)pRLineRecord.DocLineNo);
											tryCalcViewRecord.RequiredDeliveryDate = pRLineRecord.RequiredDeliveryDate;
											tryCalcViewRecord.TradeUom = pRLineRecord.TradeUOM;
											tryCalcViewRecord.TradeUom_Code = pRLineRecord.TradeUOM_Code;
											tryCalcViewRecord.TradeUom_Name = pRLineRecord.TradeUOM_Name;
											tryCalcViewRecord.LineEntityID = new long?(pRLineRecord.ID);
											tryCalcViewRecord.TradeUOM_Round_Precision = new int?(pRLineRecord.TradeUOM_Round_Precision);
											tryCalcViewRecord.TradeUOM_Round_RoundType = new int?(pRLineRecord.TradeUOM_Round_RoundType);
											tryCalcViewRecord.TradeUOM_Round_RoundValue = pRLineRecord.TradeUOM_Round_RoundValue;
											tryCalcViewRecord.DemandCode = pRLineRecord.DemandCode;
											tryCalcViewRecord.SPECS = pRLineRecord.ItemInfo_ItemID_SPECS;
											if (pRLineRecord.BizType == BusinessTypeEnum.PM050.Value)
											{
												tryCalcViewRecord.ToPOQtyTBU = 0m;
												tryCalcViewRecord.TradeBaseUom = new long?(0L);
												tryCalcViewRecord.TradeBaseUom_Code = "";
												tryCalcViewRecord.TradeBaseUom_Name = "";
												tryCalcViewRecord.TradeBaseUOM_Round_Precision = new int?(0);
												tryCalcViewRecord.TradeBaseUOM_Round_RoundType = new int?(0);
												tryCalcViewRecord.TradeBaseUOM_Round_RoundValue = new int?(0);
												if (pRLineRecord.ItemInfo_ItemID_IsDualQuantity)
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
												tryCalcViewRecord.TradeBaseUom = pRLineRecord.TradeBaseUOM;
												tryCalcViewRecord.TradeBaseUom_Code = pRLineRecord.TradeBaseUOM_Code;
												tryCalcViewRecord.TradeBaseUom_Name = pRLineRecord.TradeBaseUOM_Name;
												tryCalcViewRecord.TradeBaseUOM_Round_Precision = new int?(pRLineRecord.TradeBaseUOM_Round_Precision);
												tryCalcViewRecord.TradeBaseUOM_Round_RoundType = new int?(pRLineRecord.TradeBaseUOM_Round_RoundType);
												tryCalcViewRecord.TradeBaseUOM_Round_RoundValue = pRLineRecord.TradeBaseUOM_Round_RoundValue;
												if (pRLineRecord.ItemInfo_ItemID_IsDualQuantity)
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
											tryCalcViewRecord.BizType = pRLineRecord.BizType;
											tryCalcViewRecord.Supplier = new long?(current2.Supplier);
											tryCalcViewRecord.Supplier_Code = current2.SupplierCode;
											tryCalcViewRecord.Supplier_Name = current2.SupplierName;
										}
									}
									else
									{
										TryCalcViewRecord tryCalcViewRecord = listPRToPO.CurrentModel.TryCalcView.AddNewUIRecord();
										tryCalcViewRecord.RFQStatus = pRLineRecord.RFQStatus;
										tryCalcViewRecord.RFQ_SrcDocNo = pRLineRecord.RFQInfo_SrcDocNo;
										tryCalcViewRecord.Currency = pRLineRecord.PR_AC;
										tryCalcViewRecord.PRDocNo = pRLineRecord.PR_DocNo;
										tryCalcViewRecord.LineEntityID = new long?(pRLineRecord.ID);
										tryCalcViewRecord.ItemCode = pRLineRecord.ItemInfo_ItemCode;
										tryCalcViewRecord.ItemID = pRLineRecord.ItemInfo_ItemID;
										tryCalcViewRecord.ItemName = pRLineRecord.ItemInfo_ItemName;
										tryCalcViewRecord.ItemIsDual = new bool?(pRLineRecord.ItemInfo_ItemID_IsDualQuantity);
										tryCalcViewRecord.PRDocLineNo = new long?((long)pRLineRecord.DocLineNo);
										tryCalcViewRecord.RequiredDeliveryDate = pRLineRecord.RequiredDeliveryDate;
										tryCalcViewRecord.DemandCode = pRLineRecord.DemandCode;
										tryCalcViewRecord.SPECS = pRLineRecord.ItemInfo_ItemID_SPECS;
										if (pRLineRecord.BizType == BusinessTypeEnum.PM050.Value)
										{
											tryCalcViewRecord.ToPOQtyTBU = 0m;
											tryCalcViewRecord.TradeBaseUom = new long?(0L);
											tryCalcViewRecord.TradeBaseUom_Code = "";
											tryCalcViewRecord.TradeBaseUom_Name = "";
											tryCalcViewRecord.TradeBaseUOM_Round_Precision = new int?(0);
											tryCalcViewRecord.TradeBaseUOM_Round_RoundType = new int?(0);
											tryCalcViewRecord.TradeBaseUOM_Round_RoundValue = new int?(0);
										}
										else
										{
											tryCalcViewRecord.TradeBaseUom = pRLineRecord.TradeBaseUOM;
											tryCalcViewRecord.TradeBaseUom_Code = pRLineRecord.TradeBaseUOM_Code;
											tryCalcViewRecord.TradeBaseUom_Name = pRLineRecord.TradeBaseUOM_Name;
											tryCalcViewRecord.TradeBaseUOM_Round_Precision = new int?(pRLineRecord.TradeBaseUOM_Round_Precision);
											tryCalcViewRecord.TradeBaseUOM_Round_RoundType = new int?(pRLineRecord.TradeBaseUOM_Round_RoundType);
											tryCalcViewRecord.TradeBaseUOM_Round_RoundValue = pRLineRecord.TradeBaseUOM_Round_RoundValue;
											tryCalcViewRecord.ToPOQtyTBU = pRLineRecord.ToPOQtyTBU;
										}
										tryCalcViewRecord.ToPOQtyTU = pRLineRecord.ToPOQtyTU;
										tryCalcViewRecord.TradeUom = pRLineRecord.TradeUOM;
										tryCalcViewRecord.TradeUom_Code = pRLineRecord.TradeUOM_Code;
										tryCalcViewRecord.TradeUom_Name = pRLineRecord.TradeUOM_Name;
										tryCalcViewRecord.TradeUOM_Round_Precision = new int?(pRLineRecord.TradeUOM_Round_Precision);
										tryCalcViewRecord.TradeUOM_Round_RoundType = new int?(pRLineRecord.TradeUOM_Round_RoundType);
										tryCalcViewRecord.TradeUOM_Round_RoundValue = pRLineRecord.TradeUOM_Round_RoundValue;
										tryCalcViewRecord.BizType = pRLineRecord.BizType;
										if (pRLineRecord.ItemInfo_ItemID_ItemFormAttribute == 16)
										{
											tryCalcViewRecord.IsMisc = new bool?(true);
										}
										else
										{
											tryCalcViewRecord.IsMisc = new bool?(false);
										}
										listPRToPO.SetErrorMsg(tryCalcViewRecord, "Supplier", string.Concat(new string[]
										{
                                            UFIDA.U9.SCM.PM.PRToPOUIModel.PRToPOUIModelRes.LineNum1,
											tryCalcViewRecord.PRDocLineNo.GetValueOrDefault().ToString(),
                                            UFIDA.U9.SCM.PM.PRToPOUIModel.PRToPOUIModelRes.LineNum2,
                                            UFIDA.U9.SCM.PM.PRToPOUIModel.PRToPOUIModelRes.ItemCode,
											tryCalcViewRecord.ItemCode,
                                            UFIDA.U9.SCM.PM.PRToPOUIModel.PRToPOUIModelRes.SupplierBeNull
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
			string partId = "6b00deb4-fdb4-4a1d-9848-398735643139";
			string parentTaskId = listPRToPO.CurrentPart.TaskId.ToString();
			string width = "810";
			string height = "465";
            listPRToPO.CurrentState["PRToPO_SelectedPrLineIDs"] = text;
            listPRToPO.CurrentPart.ShowModalDialog(partId, UFIDA.U9.SCM.PM.PRToPOUIModel.PRToPOUIModelRes.TryCalc, width, height, parentTaskId, null, false, true);
        }
    }
}
