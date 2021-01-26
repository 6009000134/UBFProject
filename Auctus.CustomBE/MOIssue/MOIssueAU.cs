using Auctus.Common;
using Auctus.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UFIDA.U9.Base;
using UFIDA.U9.Base.UserRole;
using UFIDA.U9.CBO.Pub.Controller;
using UFIDA.U9.CBO.PubBE.YYC;
using UFIDA.U9.ISV.MiscRcvISV;
using UFIDA.U9.ISV.MiscShipISV;
using UFIDA.U9.MO.Enums;
using UFSoft.UBF.Sys.Database;
using UFSoft.UBF.Util.DataAccess;

namespace Auctus.CustomBE.MOIssue
{
    public class MOIssueAU : UFSoft.UBF.Eventing.IEventSubscriber
    {
        /// <summary>
        /// 1、“制损退料单”、“返工制损退料单”退料确认后参照生产退料单信息同步创建并审核杂发单、杂收单
        /// 2、控制同一需求分类号下，领料单领料数量不超过供应数量（按严格匹配的料号）（领料控制逻辑看存储过程）
        /// </summary>
        /// <param name="args"></param>
        public void Notify(params object[] args)
        {

            if (args == null || args.Length == 0 || !(args[0] is UFSoft.UBF.Business.EntityEvent))
                return;

            //将入口参数列表中第一个参数，转成EntityEvent，并取EntityKey存入key
            UFSoft.UBF.Business.BusinessEntity.EntityKey key = ((UFSoft.UBF.Business.EntityEvent)args[0]).EntityKey;
            //key的有效性判断
            if (key == null)
                return;
            //转成所需实体，同时判断有效性
            UFIDA.U9.MO.Issue.IssueDoc issueDoc = (UFIDA.U9.MO.Issue.IssueDoc)key.GetEntity();

            if (issueDoc == null)
            {
                return;
            }
            else
            {
                if (issueDoc.Org.Code == "300")
                {
                    try
                    {
                        #region“制损退料单”、“返工制损退料单”退料确认后参照生产退料单信息同步创建并审核杂发单、杂收单

                        if (issueDoc.IssueDocType.Name.Trim() == "制损退料单" || issueDoc.IssueDocType.Name.Trim() == "返工制损退料单")
                        {
                            //单据从关闭状态到已核准状态，说明是取消退料操作
                            if (issueDoc.DocState == IssueTXNStateEnum.Approved && issueDoc.OriginalData.DocState == IssueTXNStateEnum.Closed)
                            {
                                //存储过程控制取消退料操作是否开启                                
                                DataParamList dpL = new DataParamList();
                                dpL.Add(DataParamFactory.Create("Type", "BtnRecedeReverse", ParameterDirection.Input, DbType.String, 50));//禁用取消退料功能
                                dpL.Add(DataParamFactory.CreateOutput("IsOpen", DbType.String));
                                DataAccessor.RunSP("sp_Auctus_BEPlugin_Control", dpL);
                                string isOpen = dpL["IsOpen"].Value.ToString();
                                if (isOpen == "1")//打开控制
                                {
                                    UFIDA.U9.InvDoc.MiscShip.MiscShipmentL miscShipLine = UFIDA.U9.InvDoc.MiscShip.MiscShipmentL.Finder.Find("DescFlexSegments.PrivateDescSeg29='" + issueDoc.DocNo + "'");
                                    bool flag = false;
                                    string delDocs = "";
                                    if (miscShipLine != null)
                                    {
                                        flag = true;
                                        delDocs += miscShipLine.MiscShip.DocNo;
                                    }
                                    UFIDA.U9.InvDoc.MiscRcv.MiscRcvTransL rcvTransLine = UFIDA.U9.InvDoc.MiscRcv.MiscRcvTransL.Finder.Find("DescFlexSegments.PrivateDescSeg29='" + issueDoc.DocNo + "'");
                                    if (rcvTransLine != null)
                                    {
                                        flag = true;
                                        delDocs += rcvTransLine.MiscRcvTrans.DocNo;
                                    }
                                    if (flag)
                                    {
                                        throw new Exception("此单已生成杂收、杂发单，不能取消。如果需要取消需删除杂发、杂收单：" + delDocs);
                                    }
                                }
                            }
                            //单据从已核准状态到关闭状态，说明是退料确认操作
                            if (issueDoc.DocState == IssueTXNStateEnum.Closed && issueDoc.OriginalData.DocState == IssueTXNStateEnum.Approved)
                            {
                                //创建杂发单
                                #region 创建杂发杂收单
                                UFIDA.U9.ISV.MiscShipISV.Proxy.CommonCreateMiscShipProxy shipSv = new UFIDA.U9.ISV.MiscShipISV.Proxy.CommonCreateMiscShipProxy();
                                shipSv.MiscShipmentDTOList = new List<UFIDA.U9.ISV.MiscShipISV.IC_MiscShipmentDTOData>();//杂发SV参数
                                #region 杂发单数据
                                IC_MiscShipmentDTOData shipheadDto = new IC_MiscShipmentDTOData();
                                shipheadDto.BenefitOrg = Context.LoginOrg.ID;
                                shipheadDto.BusinessDate = DateTime.Now;
                                shipheadDto.MiscShipDocType = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTOData();
                                UFIDA.U9.InvDoc.MiscShip.MiscShipDocType shipDocType = UFIDA.U9.InvDoc.MiscShip.MiscShipDocType.Finder.Find("Code='MS30129' and Org=" + Context.LoginOrg.ID.ToString());//杂发单类型                                
                                shipheadDto.MiscShipDocType.ID = shipDocType.ID;
                                shipheadDto.MiscShipDocType.Code = shipDocType.Code;
                                shipheadDto.MiscShipDocType.Name = shipDocType.Name;
                                shipheadDto.Org = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTOData();
                                shipheadDto.Org.ID = Context.LoginOrg.ID;
                                shipheadDto.Org.Code = Context.LoginOrg.Code;
                                shipheadDto.Org.Name = Context.LoginOrg.Name;
                                shipheadDto.SOBAccountPeriod = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTOData();
                                UFIDA.U9.Base.SOB.SOBAccountingPeriod sobPeriod = UFIDA.U9.Base.SOB.SOBAccountingPeriod.GetSOBAccountingPeriod(UFIDA.U9.Base.SOB.SetofBooks.Finder.Find("org='" + Context.LoginOrg.ID.ToString() + "'"), DateTime.Now);
                                shipheadDto.SOBAccountPeriod.ID = sobPeriod.ID;
                                shipheadDto.SOBAccountPeriod.Code = sobPeriod.Code;
                                shipheadDto.SOBAccountPeriod.Name = sobPeriod.DisplayName;
                                #region 杂发行数据                                
                                UFIDA.U9.MO.Issue.IssueDocLine.EntityList lines = issueDoc.IssueDocLines;//退料行数据
                                shipheadDto.MiscShipLs = new List<IC_MiscShipmentLDTOData>();
                                for (int i = 0; i < lines.Count; i++)
                                {
                                    UFIDA.U9.MO.Issue.IssueDocLine line = lines[i];
                                    IC_MiscShipmentLDTOData shipLineDto = new IC_MiscShipmentLDTOData();
                                    //扩展字段中记录杂发单的来源退料单号、行号
                                    shipLineDto.DescFlexSegments = new UFIDA.U9.Base.FlexField.DescFlexField.DescFlexSegmentsData();
                                    shipLineDto.DescFlexSegments.PrivateDescSeg2 = line.MO.DocNo;
                                    shipLineDto.DescFlexSegments.PrivateDescSeg29 = issueDoc.DocNo;
                                    shipLineDto.DescFlexSegments.PrivateDescSeg30 = line.LineNum.ToString();
                                    shipLineDto.ItemInfo = new UFIDA.U9.CBO.SCM.Item.ItemInfoData();
                                    shipLineDto.ItemInfo.ItemID = line.Item.ID;
                                    shipLineDto.ItemInfo.ItemCode = line.Item.Code;
                                    shipLineDto.ItemInfo.ItemName = line.Item.Name;
                                    shipLineDto.StoreUOMQty = line.IssuedQty;//库存数量
                                    shipLineDto.CostUOMQty = line.IssuedQty;//成本数量
                                    shipLineDto.StoreUOM = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTOData();//库存单位
                                    shipLineDto.StoreUOM.ID = line.StoreBaseUOM.ID;
                                    shipLineDto.StoreUOM.Code = line.StoreBaseUOM.Code;
                                    shipLineDto.StoreUOM.Name = line.StoreBaseUOM.Name;
                                    shipLineDto.Wh = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTOData();//存储地点
                                    shipLineDto.Wh.ID = line.Wh.ID;
                                    shipLineDto.Wh.Code = line.Wh.Code;
                                    shipLineDto.Wh.Name = line.Wh.Name;
                                    shipLineDto.StoreType = line.StorageType.Value;//存储类型
                                    if (issueDoc.HandleDept != null)
                                    {
                                        shipLineDto.BenefitDept = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTOData();//受益部门
                                        shipLineDto.BenefitDept.ID = issueDoc.HandleDept.ID;
                                        shipLineDto.BenefitDept.Code = issueDoc.HandleDept.Code;
                                        shipLineDto.BenefitDept.Name = issueDoc.HandleDept.Name;
                                    }
                                    shipLineDto.BenefitPsn = new CommonArchiveDataDTOData();
                                    if (issueDoc.HandlePerson != null)
                                    {
                                        shipLineDto.BenefitPsn.ID = issueDoc.HandlePerson.ID;
                                        shipLineDto.BenefitPsn.Code = issueDoc.HandlePerson.Code;
                                        shipLineDto.BenefitPsn.Name = issueDoc.HandlePerson.Name;
                                    }
                                    if (!string.IsNullOrEmpty(lines[i].LotNo))
                                    {
                                        shipLineDto.LotInfo = new UFIDA.U9.CBO.SCM.PropertyTypes.LotInfoData();
                                        shipLineDto.LotInfo.LotCode = lines[i].LotNo;
                                        //shipLineDto.LotInfo.LotMaster = new UFIDA.U9.Base.PropertyTypes.BizEntityKeyData();
                                        //shipLineDto.LotInfo.LotMaster=lines[i].LotMaster
                                    }
                                    shipLineDto.OwnerOrg = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTOData();//货主组织
                                    shipLineDto.OwnerOrg.ID = Context.LoginOrg.ID;
                                    shipLineDto.OwnerOrg.Code = Context.LoginOrg.Code;
                                    shipLineDto.OwnerOrg.Name = Context.LoginOrg.Name;
                                    shipheadDto.MiscShipLs.Add(shipLineDto);
                                }
                                #endregion

                                #endregion
                                shipSv.MiscShipmentDTOList.Add(shipheadDto);
                                shipSv.TargetOrgCode = Context.LoginOrg.Code;
                                shipSv.TargetOrgName = Context.LoginOrg.Name;
                                List<CommonArchiveDataDTOData> liShipReturn = new List<CommonArchiveDataDTOData>();
                                liShipReturn = shipSv.Do();

                                #endregion
                                //提交杂发单
                                #region CommonCommitMiscShipSV
                                UFIDA.U9.ISV.MiscShipISV.Proxy.CommonCommitMiscShipSVProxy shipCommitSV = new UFIDA.U9.ISV.MiscShipISV.Proxy.CommonCommitMiscShipSVProxy();
                                shipCommitSV.MiscShipmentKeyList = new List<CommonArchiveDataDTOData>();
                                shipCommitSV.MiscShipmentKeyList = liShipReturn;
                                shipCommitSV.Do();
                                #endregion

                                //审核杂发单
                                #region CommonApproveMiscShipSV
                                UFIDA.U9.ISV.MiscShipISV.Proxy.CommonApproveMiscShipSVProxy shipApproveSV = new UFIDA.U9.ISV.MiscShipISV.Proxy.CommonApproveMiscShipSVProxy();
                                shipApproveSV.MiscShipmentKeyList = new List<CommonArchiveDataDTOData>();
                                shipApproveSV.MiscShipmentKeyList = liShipReturn;
                                shipApproveSV.Do();
                                #endregion

                                //创建杂收单，把上面创建的杂发单杂收了
                                #region 创建杂收单
                                UFIDA.U9.ISV.MiscRcvISV.Proxy.CommonCreateMiscRcvProxy rcvSV = new UFIDA.U9.ISV.MiscRcvISV.Proxy.CommonCreateMiscRcvProxy();
                                //杂收SV参数
                                rcvSV.TargetOrgCode = Context.LoginOrg.Code;
                                rcvSV.TargetOrgName = Context.LoginOrg.Name;
                                rcvSV.MiscRcvDTOList = new List<UFIDA.U9.ISV.MiscRcvISV.IC_MiscRcvDTOData>();
                                IC_MiscRcvDTOData rcvHeadDto = new IC_MiscRcvDTOData();//杂收单头
                                //dto.DocNo = "MR30190417999";
                                rcvHeadDto.MiscRcvDocType = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTOData();
                                UFIDA.U9.InvDoc.MiscRcv.MiscRcvDocType rcvDocType = UFIDA.U9.InvDoc.MiscRcv.MiscRcvDocType.Finder.Find("Code='MR30116' and Org= " + Context.LoginOrg.ID);
                                rcvHeadDto.MiscRcvDocType.ID = rcvDocType.ID;
                                rcvHeadDto.MiscRcvDocType.Code = rcvDocType.Code;
                                rcvHeadDto.MiscRcvDocType.Name = rcvDocType.Name;
                                rcvHeadDto.BusinessDate = DateTime.Now;//业务日期
                                rcvHeadDto.SOBAccountPeriod = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTOData();//账期
                                rcvHeadDto.SOBAccountPeriod.ID = sobPeriod.ID;
                                rcvHeadDto.SOBAccountPeriod.Code = sobPeriod.Code;
                                rcvHeadDto.SOBAccountPeriod.Name = sobPeriod.DisplayName;
                                rcvHeadDto.Org = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTOData();
                                rcvHeadDto.Org.ID = Context.LoginOrg.ID; ;
                                rcvHeadDto.Org.Code = Context.LoginOrg.Code;
                                rcvHeadDto.Org.Name = Context.LoginOrg.Name;
                                rcvHeadDto.BenefitOrg = Context.LoginOrg.ID;//受益组织                            

                                #region 杂收行
                                rcvHeadDto.MiscRcvTransLs = new List<IC_MiscRcvTransLsDTOData>();
                                UFIDA.U9.CBO.SCM.Warehouse.Warehouse wh = UFIDA.U9.CBO.SCM.Warehouse.Warehouse.Finder.Find("Code='106' and Org=" + Context.LoginOrg.ID.ToString());
                                for (int i = 0; i < lines.Count; i++)
                                {
                                    UFIDA.U9.MO.Issue.IssueDocLine line = lines[i];
                                    IC_MiscRcvTransLsDTOData rcvLineDto = new IC_MiscRcvTransLsDTOData();
                                    //扩展字段中记录杂发单的来源退料单号、行号
                                    rcvLineDto.DescFlexSegments = new UFIDA.U9.Base.FlexField.DescFlexField.DescFlexSegmentsData();
                                    rcvLineDto.DescFlexSegments.PrivateDescSeg2 = line.MO.DocNo;
                                    rcvLineDto.DescFlexSegments.PrivateDescSeg29 = issueDoc.DocNo;
                                    rcvLineDto.DescFlexSegments.PrivateDescSeg30 = line.LineNum.ToString();
                                    rcvLineDto.ItemInfo = new UFIDA.U9.CBO.SCM.Item.ItemInfoData();
                                    rcvLineDto.ItemInfo.ItemID = line.Item.ID;
                                    rcvLineDto.ItemInfo.ItemCode = line.Item.Code;
                                    rcvLineDto.ItemInfo.ItemName = line.Item.Name;
                                    rcvLineDto.Wh = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTOData();
                                    rcvLineDto.Wh.ID = wh.ID;
                                    rcvLineDto.Wh.Code = wh.Code;
                                    rcvLineDto.Wh.Name = wh.Name;
                                    rcvLineDto.BenefitDept = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTOData();
                                    rcvLineDto.BenefitDept.ID = issueDoc.HandleDept.ID;
                                    rcvLineDto.BenefitDept.Code = issueDoc.HandleDept.Code;
                                    rcvLineDto.BenefitDept.Name = issueDoc.HandleDept.Name;
                                    rcvLineDto.StoreUOMQty = line.IssuedQty;
                                    rcvLineDto.CostUOMQty = line.IssuedQty;
                                    rcvLineDto.CostUOM = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTOData();
                                    rcvLineDto.CostUOM.ID = line.StoreBaseUOM.ID;
                                    rcvLineDto.CostUOM.Code = line.StoreBaseUOM.Code;
                                    rcvLineDto.CostUOM.Name = line.StoreBaseUOM.Name;
                                    rcvLineDto.StoreUOM = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTOData();
                                    rcvLineDto.StoreUOM.ID = line.StoreBaseUOM.ID;
                                    rcvLineDto.StoreUOM.Code = line.StoreBaseUOM.Code;
                                    rcvLineDto.StoreUOM.Name = line.StoreBaseUOM.Name;
                                    rcvLineDto.IsZeroCost = true;
                                    //rcvLineDto.StoreUOM.ID = 1001708030115592;
                                    //rcvLineDto.StoreUOM.Code = "SL01";
                                    //rcvLineDto.StoreUOM.Name = "PCS";
                                    rcvLineDto.OwnerOrg = new UFIDA.U9.CBO.Pub.Controller.CommonArchiveDataDTOData();
                                    rcvLineDto.OwnerOrg.ID = Context.LoginOrg.ID;
                                    rcvLineDto.OwnerOrg.Code = Context.LoginOrg.Code;
                                    rcvLineDto.OwnerOrg.Name = Context.LoginOrg.Name;
                                    if (!string.IsNullOrEmpty(lines[i].LotNo))
                                    {
                                        rcvLineDto.LotInfo = new UFIDA.U9.CBO.SCM.PropertyTypes.LotInfoData();
                                        rcvLineDto.LotInfo.LotCode = lines[i].LotNo;
                                    }
                                    rcvHeadDto.MiscRcvTransLs.Add(rcvLineDto);
                                }
                                #endregion
                                rcvSV.MiscRcvDTOList.Add(rcvHeadDto);
                                List<CommonArchiveDataDTOData> liRcvReturn = new List<CommonArchiveDataDTOData>();
                                liRcvReturn = rcvSV.Do();//执行创建杂收单服务

                                #region 提交杂收单服务
                                UFIDA.U9.ISV.MiscRcvISV.Proxy.CommonCommitMiscRcvProxy rcvCommitSV = new UFIDA.U9.ISV.MiscRcvISV.Proxy.CommonCommitMiscRcvProxy();
                                rcvCommitSV.TargetOrgCode = Context.LoginOrg.Code;
                                rcvCommitSV.TargetOrgName = Context.LoginOrg.Name;
                                rcvCommitSV.MiscRcvKeys = new List<CommonArchiveDataDTOData>();
                                rcvCommitSV.MiscRcvKeys = liRcvReturn;
                                rcvCommitSV.Do();
                                #endregion

                                #region 审核杂收单
                                UFIDA.U9.ISV.MiscRcvISV.Proxy.CommonApproveMiscRcvProxy rcvApproveSV = new UFIDA.U9.ISV.MiscRcvISV.Proxy.CommonApproveMiscRcvProxy();
                                rcvApproveSV.TargetOrgCode = Context.LoginOrg.Code;
                                rcvApproveSV.MiscRcvKeys = new List<CommonArchiveDataDTOData>();
                                rcvApproveSV.MiscRcvKeys = liRcvReturn;
                                rcvApproveSV.Do();
                                #endregion

                                #endregion

                            }
                        }
                        #endregion

                        #region  领料控制                        
                        //根据更新前后时间判断订单是否已审核，若beforeConfirmDate=confirmDate表示订单已审核
                        string beforeConfirmDate = "";
                        string confirmDate = "";
                        if (issueDoc.OriginalData.IssueItemOn != DateTime.MinValue)
                        {
                            beforeConfirmDate = issueDoc.OriginalData.IssueItemOn.ToString();
                        }
                        if (issueDoc.IssueItemOn != DateTime.MinValue)
                        {
                            confirmDate = issueDoc.IssueItemOn.ToString();
                        }
                        DataParamList dp = new DataParamList();
                        dp.Add(DataParamFactory.Create("DocNo", issueDoc.DocNo, ParameterDirection.Input, DbType.String, 50));
                        dp.Add(DataParamFactory.Create("BeforeConfirmDate", beforeConfirmDate, ParameterDirection.Input, DbType.String, 50));
                        dp.Add(DataParamFactory.Create("ConfirmDate", confirmDate, ParameterDirection.Input, DbType.String, 50));
                        dp.Add(DataParamFactory.CreateOutput("Result", DbType.String));
                        DataAccessor.RunSP("sp_auctus_BE_MoIssueDoc", dp);
                        string result = dp["Result"].Value.ToString();
                        if (result != "1")
                        {
                            throw new Exception(result);
                        }
                        #endregion

                        #region 提交流程到OA
                        if (issueDoc.DocType.DescFlexField.PrivateDescSeg1 == "1" && issueDoc.DocState == IssueTXNStateEnum.Approving && issueDoc.OriginalData.DocState == IssueTXNStateEnum.Opened)
                        {
                            PubFunction pubFun = new PubFunction();
                            User user = User.Finder.FindByID(Context.LoginUserID);//U9用户信息
                            Dictionary<string, object> dicResult = new Dictionary<string, object>();
                            Dictionary<string, object> dicMain = GenerateMainInfo(issueDoc, user.Code);
                            List<Dictionary<string, object>> liDt = GenerateDtInfo(issueDoc);
                            BaseInfo baseInfo;
                            string workflowid = "";
                            if (issueDoc.IssueType == IssueTypeEnum.Issue)//发料
                            {
                                workflowid = PubFunction.GetOAInfoByCode("08");
                            }
                            else if (issueDoc.IssueType == IssueTypeEnum.Withdrawal)//退料
                            {
                                workflowid = PubFunction.GetOAInfoByCode("09");
                            }
                            
                            string requestName = "";
                            if (issueDoc.IssueType == IssueTypeEnum.Issue)
                            {
                                requestName = "生产领料单：" + issueDoc.DocNo;
                            }
                            else if (issueDoc.IssueType == IssueTypeEnum.Withdrawal)
                            {
                                requestName = "生产退料单：" + issueDoc.DocNo;
                            }
                            if (issueDoc.DescFlexField.PrivateDescSeg6 == "是")//提交流程或弃审后重新提交流程
                            {
                                baseInfo = Utils.GenerateOABaseInfo(user.Code, workflowid, issueDoc.ID.ToString(), 1, issueDoc.DescFlexField.PrivateDescSeg5, 1, requestName);
                            }
                            else//驳回后重新提交
                            {
                                baseInfo = Utils.GenerateOABaseInfo(user.Code, workflowid, issueDoc.ID.ToString(), 1, "", 1, requestName);
                            }
                            dicResult.Add("base", baseInfo);
                            dicResult.Add("main", dicMain);
                            dicResult.Add("dt1", liDt);
                            List<Dictionary<string, object>> li = new List<Dictionary<string, object>>();
                            li.Add(dicResult);
                            string json = JsonHelper.GetJsonJS(li);
                            Utils.LogError("生产领料JSON");
                            Utils.LogError(json);
                            string OAFlowID = pubFun.OAService(json);
                            //更新扩展字段：OA流程ID和是否驳回
                            string sql = string.Format("update mo_IssueDoc set DescFlexField_PrivateDescSeg6='否',DescFlexField_PrivateDescSeg5='{0}' where ID={1}", OAFlowID, issueDoc.ID);
                            DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), sql, null);//更新sql 
                        }
                        #endregion
                    }

                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
            }
        }

        public Dictionary<string, object> GenerateMainInfo(UFIDA.U9.MO.Issue.IssueDoc issueDoc, string userCode)
        {
            Dictionary<string, object> dicMain = new Dictionary<string, object>();
            try
            {
                Dictionary<string, object> dicValue = new Dictionary<string, object>();
                if (issueDoc.DescFlexField.PrivateDescSeg6 == "否")//提交流程或弃审后重新提交流程
                {
                    if (!string.IsNullOrEmpty(issueDoc.DescFlexField.PrivateDescSeg5))//存在流程ID，说明是弃审后重新提交
                    {
                        dicValue = new Dictionary<string, object>();
                        dicValue.Add("value", issueDoc.DescFlexField.PrivateDescSeg5);
                        dicMain.Add("gllc", dicValue);
                    }
                }
                dicMain = Auctus.Common.Utils.GenerateOAUserInfo(dicMain, userCode, DateTime.Now.ToString("yyyy-MM-dd"));

                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", Context.LoginOrg.ID);
                dicMain.Add("u9zz", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", issueDoc.DocNo);
                dicMain.Add("dh", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", issueDoc.ID);
                dicMain.Add("fpkid", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", issueDoc.DocType.ID);
                dicMain.Add("djlx", dicValue);
                if (issueDoc.IssueType == IssueTypeEnum.Withdrawal)
                {
                    dicValue = new Dictionary<string, object>();
                    dicValue.Add("value", issueDoc.DescFlexField.PrivateDescSeg2);
                    dicMain.Add("tlyy", dicValue);
                }
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", issueDoc.IssueDocLines[0].MO.DescFlexField.PrivateDescSeg6);
                dicMain.Add("scxb", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", issueDoc.IssueDocLines[0].MO.DocNo);
                dicMain.Add("scdd", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", issueDoc.IssueDocLines[0].MO.ItemMaster.Code);
                dicMain.Add("cpbm", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", issueDoc.IssueDocLines[0].MO.ItemMaster.Name);
                dicMain.Add("cpmc", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", issueDoc.DescFlexField.PrivateDescSeg1);//上线时间
                dicMain.Add("sxsj", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", issueDoc.IssueDocLines[0].MO.ProductQty.ToString("##"));
                dicMain.Add("gdsl", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", issueDoc.BusinessDate.ToString("yyyy-MM-dd"));
                dicMain.Add("djrq", dicValue);
                dicValue = new Dictionary<string, object>();
                if (issueDoc.DescFlexField.PrivateDescSeg3 == "20")
                {
                    dicValue.Add("value", "1");
                }
                else
                {
                    dicValue.Add("value", "0");
                }
                dicMain.Add("yjhx", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", issueDoc.IsSpecialIssue ? 1 : 0);
                dicMain.Add("tbll", dicValue);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dicMain;
        }

        public List<Dictionary<string, object>> GenerateDtInfo(UFIDA.U9.MO.Issue.IssueDoc issueDoc)
        {
            List<Dictionary<string, object>> li = new List<Dictionary<string, object>>();
            foreach (UFIDA.U9.MO.Issue.IssueDocLine item in issueDoc.IssueDocLines)
            {
                Dictionary<string, object> dicDt = new Dictionary<string, object>();
                Dictionary<string, object> dicValue = new Dictionary<string, object>();
                dicValue.Add("value", item.LineNum);
                dicDt.Add("xh", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", item.Item.Code);
                dicDt.Add("lpbh", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", item.Item.Name);
                dicDt.Add("lpmc", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", item.Item.SPECS);
                dicDt.Add("wlgg", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", item.Wh.Name);
                dicDt.Add("flck", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", item.IssueQty.ToString("##"));
                dicDt.Add("yfs", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", item.IssuedQty.ToString("##"));
                dicDt.Add("sfs", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", item.LotNo);
                dicDt.Add("ph", dicValue);
                li.Add(dicDt);
            }
            return li;
        }
    }
}