using System;
using System.Collections.Generic;
using System.Data;
using UFIDA.U9.Base;
using UFIDA.U9.Base.UserRole;
using UFIDA.U9.CBO.PubBE.YYC;
using UFIDA.U9.InvDoc.Enums;
using UFIDA.U9.InvDoc.MiscShip;
using UFSoft.UBF.Sys.Database;
using UFSoft.UBF.Util.Cache;
using UFSoft.UBF.Util.DataAccess;
using UFSoft.UBF.Util.Log;

namespace Auctus.MiscShipmentBE
{
    public class MiscShipmentAU : UFSoft.UBF.Eventing.IEventSubscriber
    {
        [UFSoft.UBF.Eventing.Configuration.Failfast]
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
            UFIDA.U9.InvDoc.MiscShip.MiscShipment miscShip = (UFIDA.U9.InvDoc.MiscShip.MiscShipment)key.GetEntity();

            if (miscShip == null) return;
            else
            {
                if (miscShip.SysState == UFSoft.UBF.PL.Engine.ObjectState.Updated)
                {
                    ILogger log = LoggerManager.GetLogger(typeof(CacheManager));
                    //单据从开立状态->审核中（提交审核操作），调用OA接口，创建审批流
                    if (miscShip.OriginalData.Status == INVDocStatus.Open && miscShip.Status == INVDocStatus.Approving && miscShip.DocType.DescFlexField.PrivateDescSeg1 == "1")
                    {
                        //查询OA的流程ID
                        string sql = string.Format(@"SELECT DescFlexField_PrivateDescSeg4 AS IsOAtoU9
                        FROM dbo.InvDoc_MiscShip WHERE ID={0}",miscShip.ID);
                        DataSet ds = new DataSet();
                        DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), sql, null, out ds);
                        string IsOAtoU9 = "";
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            IsOAtoU9 = row["IsOAtoU9"].ToString();
                        }
                        if (IsOAtoU9=="1")//OA触发到U9的杂发单，不触发U9到OA
                        {
                            return;
                        }

                        List<MishShipDoc> list = new List<MishShipDoc>();//杂发单集合
                        MishShipDoc doc = new MishShipDoc();//杂发单数据
                        BaseInfo baseInfo = new BaseInfo();//OA上下文数据

                        ValueTrans vt = new ValueTrans();
                        User user = User.Finder.FindByID(Context.LoginUserID);
                        vt.value = user.Code;
                        vt.transrule = "getUseridByWorkcode";
                        #region 上下文基础信息
                        baseInfo.creator = vt;
                        baseInfo.fpkid = miscShip.ID.ToString();//单据ID
                        //baseInfo.isnextflow = 0;
                        //baseInfo.requestid = "1025";                        
                        //baseInfo.requestlevel = 1;  
                        baseInfo.isnextflow = 1;

                        baseInfo.requestname = "杂发单审批-订单号：" + miscShip.DocNo;//审批流标题
                        baseInfo.workflowid = PubFunction.GetMiscShipWorkFlow();//固定值，取数逻辑看文档
                        //baseInfo.workflowid = "1025";//固定值，取数逻辑看文档
                        #endregion

                        doc.@base = baseInfo;
                        #region 表头信息
                        //表头信息
                        MishShipHeadDto main = new MishShipHeadDto();

                        Value v = new Value();
                        if (miscShip.DescFlexField.PrivateDescSeg3 != "")
                        {
                            //OA弃审后，U9重新提交流程，应该走更细OA流程接口
                            //if (miscShip.DescFlexField.PrivateDescSeg4 == "1")
                            //{
                            v.value = miscShip.DescFlexField.PrivateDescSeg3;
                            main.gllc = v;
                            //main.requestid = miscShip.DescFlexField.PrivateDescSeg3; //requestid不为空，则为更新流程接口                               
                            //string sql = string.Format(@"UPDATE dbo.InvDoc_MiscShip SET DescFlexField_PrivateDescSeg4='0' WHERE ID = {0}", miscShip.ID);
                            //DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), sql, null);
                            //}
                        }
                        //申请人
                        vt = new ValueTrans();
                        vt.value = user.Code;
                        vt.transrule = "getUseridByWorkcode";
                        main.sqr = vt;
                        //申请人工号
                        vt = new ValueTrans();
                        vt.value = user.Code;
                        vt.transrule = "getWorkcodeByU9code";
                        main.sqrgh = vt;
                        //申请人部门
                        vt = new ValueTrans();
                        vt.value = user.Code;
                        vt.transrule = "getDeptidByWorkcode";
                        main.ssbm = vt;
                        //申请人岗位
                        vt = new ValueTrans();
                        vt.value = user.Code;
                        vt.transrule = "getJobidByWorkcode";
                        main.sqrgw = vt;
                        //申请人分部
                        vt = new ValueTrans();
                        vt.value = user.Code;
                        vt.transrule = "getSubcomidByWorkcode";
                        main.ssgs = vt;
                        //申请日期
                        v = new Value();
                        v.value = DateTime.Now.ToString("yyyy-MM-dd");
                        main.sqrq = v;
                        //流程编号lcbh
                        //单据类型
                        v = new Value();
                        v.value = miscShip.DocType.Code;
                        main.djlx = v;
                        //单号
                        v = new Value();
                        v.value = miscShip.DocNo;
                        main.dh = v;
                        //存储地点
                        if (miscShip.MiscShipLs[0].Wh != null)
                        {
                            v = new Value();
                            v.value = miscShip.MiscShipLs[0].Wh.Name;
                            main.ccdd = v;
                        }
                        //受益存储地点
                        if (miscShip.MiscShipLs[0].BenefitWh != null)
                        {
                            v.value = miscShip.MiscShipLs[0].BenefitWh.Name;
                            main.syccdd = v;
                        }
                        //受益人
                        if (miscShip.BenefitPsn != null)
                        {
                            v = new Value();
                            v.value = miscShip.BenefitPsn.Name;
                            main.syr = v;
                        }

                        //受益部门
                        if (miscShip.MiscShipLs[0].BenefitDept != null)
                        {
                            v = new Value();
                            v.value = miscShip.MiscShipLs[0].BenefitDept.Name;
                            main.sybm = v;
                        }

                        //记账时间jzsj
                        v = new Value();
                        v.value = miscShip.MiscShipAccountPeriods[0].SOBAccountPeriod.DisplayName;
                        main.jzsj = v;
                        //供应商gys
                        if (miscShip.MiscShipLs[0].SupplierInfo != null)
                        {
                            v = new Value();
                            v.value = miscShip.MiscShipLs[0].SupplierInfo.Name;
                            main.gys = v;
                        }
                        //客户kh
                        if (miscShip.MiscShipLs[0].CustomerInfo != null)
                        {
                            v = new Value();
                            v.value = miscShip.MiscShipLs[0].CustomerInfo.Name;
                            main.kh = v;
                        }

                        //库管员
                        if (miscShip.WhMan != null)
                        {
                            v = new Value();
                            v.value = miscShip.WhMan.Name;
                            //vt.transrule = "";
                            main.kgy = v;
                        }

                        //高新项目gxxm
                        v = new Value();
                        v.value = miscShip.DescFlexField.PrivateDescSeg1;
                        main.gxxm = v;
                        //研发项目yfxm
                        v = new Value();
                        v.value = miscShip.DescFlexField.PrivateDescSeg2;
                        main.yfxm = v;
                        //U9组织
                        v = new Value();
                        v.value = miscShip.Org.ID.ToString();
                        //vt.transrule = "getUseridByWorkcode";
                        main.u9zz1 = v;
                        //关联流程gllc
                        #endregion

                        doc.main = main;

                        #region 行信息
                        //行信息
                        List<MishShipLineDto> lines = new List<MishShipLineDto>();
                        foreach (MiscShipmentL item in miscShip.MiscShipLs)
                        {
                            MishShipLineDto l = new MishShipLineDto();
                            //料号
                            v = new Value();
                            v.value = item.ItemInfo.ItemCode;
                            l.lh = v;
                            //品名
                            v = new Value();
                            v.value = item.ItemInfo.ItemName;
                            l.pm = v;
                            //存储地点
                            v = new Value();
                            v.value = item.Wh.Name;
                            l.ccdd = v;
                            //杂发量
                            v = new Value();
                            v.value = item.CostUOMQty.ToString("0.00");
                            l.zfl = v;
                            //成本cb
                            v = new Value();
                            v.value = item.CostMny.ToString("f4");
                            l.cb = v;
                            //批号ph
                            if (item.LotInfo != null)
                            {
                                v = new Value();
                                v.value = item.LotInfo.LotCode;
                                l.ph = v;
                            }
                            //生产订单号scddh
                            v = new Value();
                            v.value = item.MoDocNo;
                            l.scddh = v;
                            //生产相关scxg
                            v = new Value();
                            if (item.IsMFG)
                            {
                                v.value = "1";
                            }
                            else
                            {
                                v.value = "0";
                            }
                            l.scxg = v;
                            //零成本
                            v = new Value();
                            if (item.IsZeroCost)
                            {
                                v.value = "1";
                            }
                            else
                            {
                                v.value = "0";
                            }
                            l.lcb = v;
                            //受益人
                            if (item.BenefitPsn != null)
                            {
                                v = new Value();
                                v.value = item.BenefitPsn.Name;
                                //vt.transrule = "getUseridByWorkcode";
                                l.syr = v;
                            }
                            //受益部门
                            if (item.BenefitDept != null)
                            {
                                v = new Value();
                                v.value = item.BenefitDept.Name;
                                //vt.transrule = "getDeptidByWorkcode";
                                l.sybm = v;
                            }
                            //供应商gys
                            if (item.SupplierInfo != null)
                            {
                                v = new Value();
                                v.value = item.SupplierInfo.Name;
                                l.gys = v;
                            }
                            //备注
                            v = new Value();
                            v.value = item.Memo;
                            l.bz = v;
                            lines.Add(l);
                        }
                        #endregion
                        doc.dt1 = new List<MishShipLineDto>();
                        doc.dt1 = lines;

                        //添加
                        list.Add(doc);
                        string json = "";
                        json = JsonHelper.GetJson<List<MishShipDoc>>(list);
                        log.Error("杂发单JSON数据为：" + json);
                        PubFunction pubFun = new PubFunction();
                        //调用OA接口
                        string OAFlowID = pubFun.OAService(json);
                        //更新返回的流程ID
                        string UpSQL = string.Format(@"UPDATE dbo.InvDoc_MiscShip SET DescFlexField_PrivateDescSeg3='{0}' WHERE ID = {1}", OAFlowID, miscShip.ID.ToString());
                        DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), UpSQL, null);
                        log.Error("结束调用OA审批接口");
                    }
                    //单据从已核准状态到开立（弃审操作）
                    if (miscShip.OriginalData.Status == INVDocStatus.Approved && miscShip.Status == INVDocStatus.Open)
                    {
                        log.Error(miscShip.DocNo + "弃审了");
                        //if (miscShip.DocType.DescFlexField.PrivateDescSeg1 == "1")//继承OA流程的单据类型
                        //{
                        //    string UpSQL = string.Format(@"UPDATE dbo.InvDoc_MiscShip SET DescFlexField_PrivateDescSeg4='1' WHERE ID = {0}", miscShip.ID);
                        //    DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), UpSQL, null);
                        //}
                        bool IsNeedCreateOA = true;
                        try
                        {
                            //300组织不允许“生产制损领料”弃审
                            if (miscShip.Org.Code == "300" && miscShip.MiscShipDocType.Name.Trim() == "生产制损领料")
                            {

                                //存储过程控制取消退料操作是否开启                                
                                DataParamList dpL = new DataParamList();
                                dpL.Add(DataParamFactory.Create("Type", "MiscShipBtnUnDoApprove", ParameterDirection.Input, DbType.String, 50));//禁用杂收单弃审功能
                                dpL.Add(DataParamFactory.CreateOutput("IsOpen", DbType.String));
                                DataAccessor.RunSP("sp_Auctus_BEPlugin_Control", dpL);
                                string isOpen = dpL["IsOpen"].Value.ToString();
                                if (isOpen == "1")
                                {
                                    DataParamList dp = new DataParamList();
                                    dp.Add(DataParamFactory.Create("DocNo", miscShip.DocNo, ParameterDirection.Input, DbType.String, 50));//禁用杂收单弃审功能
                                    dp.Add(DataParamFactory.CreateOutput("Result", DbType.String));
                                    DataAccessor.RunSP("sp_Auctus_BE_MiscShip", dp);
                                    string result = dp["Result"].Value.ToString();
                                    if (result != "1")//存在下游杂收单
                                    {
                                        IsNeedCreateOA = false;
                                        throw new Exception(result);
                                    }
                                    else
                                    {
                                        IsNeedCreateOA = true;
                                    }
                                }
                            }

                            if (IsNeedCreateOA)//需要发起OA弃审,调用删除OA流程接口
                            {
                                //TODO:调用OA弃审接口
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    }//end if
                    else if (miscShip.OriginalData.Status == INVDocStatus.Approving && miscShip.Status == INVDocStatus.Open)
                    {
                        log.Error(miscShip.DocNo+"从核准中变成了开立");
                    }

                }

            }
        }
    }
}
