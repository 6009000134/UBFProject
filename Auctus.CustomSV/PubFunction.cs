using Auctus.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UFIDA.U9.CBO.PubBE.YYC;
using UFSoft.UBF.Sys.Database;
using UFSoft.UBF.Util.Cache;
using UFSoft.UBF.Util.DataAccess;
using UFSoft.UBF.Util.Log;

namespace Auctus.CustomSV
{
    /// <summary>
    /// 公共操作方法
    /// </summary>
    public class PubFunction
    {
        /// <summary>
        /// 设置BaseInfo
        /// </summary>
        /// <param name="userCode">U9账号</param>
        /// <param name="workFlowID">OA流程编号</param>
        /// <param name="fpkid">U9单据ID</param>
        /// <param name="isNextFlow">0：触发的OA流程停留在第一个节点,1：触发的OA流程自动提交流转到下一个节点,若未传则默认为1</param>
        /// <param name="requestid">若需要更新流程则此字段必须传入，创建流程此字段没作用，传空即可</param>
        /// <param name="requestLevel"></param>
        /// <param name="requestName">若外部系统未传标题给OA，则OA自动生成标题，格式为：流程类型名+创建人+日期，例如采购订单审批流程-张三-2020-03-11</param>
        /// <returns>BaseInfo</returns>
        public static BaseInfo GenerateOABaseInfo(string userCode, string workFlowID, string fpkid, int isNextFlow, string requestid, int requestLevel, string requestName)
        {
            BaseInfo baseInfo = new BaseInfo();
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("value", userCode);
            dic.Add("transrule", "getUseridByWorkcode");
            baseInfo.creator = dic;
            baseInfo.fpkid = fpkid;
            baseInfo.isnextflow = isNextFlow;
            baseInfo.requestid = requestid;
            baseInfo.requestlevel = requestLevel;
            baseInfo.requestname = requestName;
            baseInfo.workflowid = workFlowID;
            return baseInfo;
        }


        /// <summary>
        /// 获取U9值集中的OA配置信息
        /// </summary>
        /// <returns></returns>
        public static string GetOAInfoByCode(string code)
        {
            string sql = string.Format(@"SELECT a2.Description FROM dbo.Base_ValueSetDef AS a
                INNER JOIN dbo.Base_DefineValue AS a1 ON a1.ValueSetDef = a.ID
						                AND a1.Code = '{0}'
                INNER JOIN dbo.Base_DefineValue_Trl AS a2 ON a2.ID=a1.id
						                AND a2.SysMLFlag='zh-cn'
                WHERE a.Code = 'OAAddr'
                AND a1.Effective_IsEffective = 1
                AND a1.Effective_EffectiveDate <= GETDATE()
                AND a1.Effective_DisableDate >= GETDATE()", code);
            string OACode = "";
            DataSet ds = new DataSet();
            DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), sql, null, out ds);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                OACode = row["Description"].ToString();
            }
            return OACode;
        }
        /// <summary>
        /// OA准入码
        /// </summary>
        /// <returns></returns>
        public static string GetOACode()
        {
            return GetOAInfoByCode("02");
        }
        /// <summary>
        /// 是否为测试环境
        /// </summary>
        /// <returns></returns>
        public static string IsOATest()
        {
            return GetOAInfoByCode("03");
        }
        /// <summary>
        /// 调用OA接口服务
        /// </summary>
        /// <returns></returns>
        //public string OAService(string jsson)
        //{
        //    //参数1：准入码，参数2：json
        //    string OACode = PubFunction.GetOACode();
        //    string result = string.Empty;
        //    string IsTest = PubFunction.IsOATest();
        //    ILogger logger = LoggerManager.GetLogger(typeof(CacheManager));
        //    if (IsTest == "是")//测试环境
        //    {
        //        OAWorkflowService4Test.WorkflowServiceToOtherSystemPortTypeClient service = new OAWorkflowService4Test.WorkflowServiceToOtherSystemPortTypeClient();
        //        try
        //        {
        //            result = service.createWorkflow(OACode, jsson);
        //            logger.Error("调用OA审批内网测试接口成功，返回的结果为：" + result);
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("调用OA审批内网测试接口出错，错误信息为：" + ex.Message);
        //            throw new Exception("调用OA审批内网测试接口出错，错误信息为：" + ex.Message);
        //        }
        //    }
        //    else//生产环境
        //    {
        //        OAWorkflowService.WorkflowServiceToOtherSystemPortTypeClient service = new OAWorkflowService.WorkflowServiceToOtherSystemPortTypeClient();
        //        try
        //        {
        //            result = service.createWorkflow(OACode, jsson);
        //            logger.Error("调用OA审批接口成功，返回的结果为：" + result);
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Error("调用OA审批接口出错，错误信息为：" + ex.Message);
        //            throw new Exception("调用OA审批接口出错，错误信息为：" + ex.Message);
        //        }
        //    }


        //    //将OA接口返回结果转成对象
        //    string OAFlowID = "";
        //    ResultInfo OAResult = JsonHelper.JsonDeserialize<ResultInfo>(result);
        //    if (OAResult.type == "1")
        //    {
        //        logger.Error("OA接口返回错误，错误信息为:" + OAResult.backmsg);
        //        throw new Exception("OA接口返回错误，错误信息为:" + OAResult.backmsg);
        //    }
        //    else
        //    {
        //        //检查结果集
        //        foreach (ResultList rList in OAResult.resultlist)
        //        {
        //            if (rList.status == "1")
        //            {
        //                throw new Exception("OA接口返回错误，错误信息为:" + rList.msg);
        //                throw new Exception("OA接口返回错误，错误信息为:" + OAResult.backmsg);
        //            }
        //            OAFlowID = rList.requestid;
        //        }
        //    }
        //    return OAFlowID;
        //}

    }
}
