using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UFIDA.U9.Base;
using UFIDA.U9.CBO.HR.Operator;
using UFIDA.U9.CBO.PubBE.YYC;
using UFSoft.UBF.PL;
using UFSoft.UBF.Sys.Database;
using UFSoft.UBF.Util.Cache;
using UFSoft.UBF.Util.DataAccess;
using UFSoft.UBF.Util.Log;

namespace Auctus.MiscShipmentBE
{
    public class PubFunction
    {
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
        /// 获取杂发单OA流程ID
        /// </summary>
        /// <returns></returns>
        public static string GetMiscShipWorkFlow()
        {
            return GetOAInfoByCode("05");
        }
        /// <summary>
        /// 获取配置的OA地址信息
        /// </summary>
        /// <returns></returns>
        public static string GetOAIP()
        {
            string sql = @"SELECT a2.Description AS OAIP FROM dbo.Base_ValueSetDef AS a
                INNER JOIN dbo.Base_DefineValue AS a1 ON a1.ValueSetDef = a.ID
						                AND a1.Code = '01'
                INNER JOIN dbo.Base_DefineValue_Trl AS a2 ON a2.ID=a1.id
						                AND a2.SysMLFlag='zh-cn'
                WHERE a.Code = 'OAAddr'
                AND a1.Effective_IsEffective = 1
                AND a1.Effective_EffectiveDate <= GETDATE()
                AND a1.Effective_DisableDate >= GETDATE()";
            string OAIP = "";
            DataSet ds = new DataSet();
            DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), sql, null, out ds);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                OAIP = row["OAIP"].ToString();
            }
            return OAIP;
        }
        /// <summary>
        /// 是否为测试环境
        /// </summary>
        /// <returns></returns>
        public static string IsOATest()
        {
            string sql = @"SELECT a2.Description AS OATest FROM dbo.Base_ValueSetDef AS a
                INNER JOIN dbo.Base_DefineValue AS a1 ON a1.ValueSetDef = a.ID
						                AND a1.Code = '03'
                INNER JOIN dbo.Base_DefineValue_Trl AS a2 ON a2.ID=a1.id
						                AND a2.SysMLFlag='zh-cn'
                WHERE a.Code = 'OAAddr'
                AND a1.Effective_IsEffective = 1
                AND a1.Effective_EffectiveDate <= GETDATE()
                AND a1.Effective_DisableDate >= GETDATE()";
            string OATest = "";
            DataSet ds = new DataSet();
            DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), sql, null, out ds);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                OATest = row["OATest"].ToString();
            }
            return OATest;
        }
        /// <summary>
        /// 获取配置的OA准入码
        /// </summary>
        /// <returns></returns>
        public static string GetOACode()
        {
            string sql = @"SELECT a2.Description AS OACode FROM dbo.Base_ValueSetDef AS a
                INNER JOIN dbo.Base_DefineValue AS a1 ON a1.ValueSetDef = a.ID
						                AND a1.Code = '02'
                INNER JOIN dbo.Base_DefineValue_Trl AS a2 ON a2.ID=a1.id
						                AND a2.SysMLFlag='zh-cn'
                WHERE a.Code = 'OAAddr'
                AND a1.Effective_IsEffective = 1
                AND a1.Effective_EffectiveDate <= GETDATE()
                AND a1.Effective_DisableDate >= GETDATE()";
            string OACode = "";
            DataSet ds = new DataSet();
            DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), sql, null, out ds);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                OACode = row["OACode"].ToString();
            }
            return OACode;
        }
        /// <summary>
        /// 根据业务员编码取得当前组织生效的业务员
        /// </summary>
        /// <param name="Code">业务员编码</param>
        /// <returns></returns>
        public Operators GetOperators(string Code)
        {
            string path = string.Format(" Operators.Effective.IsEffective=1 and Operators.Effective.EffectiveDate < getdate() and Operators.Effective.DisableDate >= getdate() and Operators.Code = '{0}' and Operators.Org.ID={1} ", Code, Context.LoginOrg.ID.ToString());
            OperatorLine opLine = OperatorLine.Finder.Find(path, new OqlParam[] { });
            if (opLine == null)
                return null;
            return opLine.Operators;
        }
        /// <summary>
        /// 调用OA接口服务
        /// </summary>
        /// <returns></returns>
        public string OAService(string jsson)
        {
            //参数1：准入码，参数2：json
            string OACode = PubFunction.GetOACode();
            string result = string.Empty;
            string IsTest = PubFunction.IsOATest();
            ILogger logger = LoggerManager.GetLogger(typeof(CacheManager));
            if (IsTest == "是")//测试环境
            {
                OAWorkflowService4Test.WorkflowServiceToOtherSystemPortTypeClient service = new OAWorkflowService4Test.WorkflowServiceToOtherSystemPortTypeClient();
                try
                {
                    result = service.createWorkflow(OACode, jsson);
                    logger.Error("调用OA审批内网测试接口成功，返回的结果为：" + result);
                }
                catch (Exception ex)
                {
                    logger.Error("调用OA审批内网测试接口出错，错误信息为：" + ex.Message);
                    throw new Exception("调用OA审批内网测试接口出错，错误信息为：" + ex.Message);
                }
            }
            else//生产环境
            {
                OAWorkflowService.WorkflowServiceToOtherSystemPortTypeClient service = new OAWorkflowService.WorkflowServiceToOtherSystemPortTypeClient();
                try
                {
                    result = service.createWorkflow(OACode, jsson);
                    logger.Error("调用OA审批接口成功，返回的结果为：" + result);
                }
                catch (Exception ex)
                {
                    logger.Error("调用OA审批接口出错，错误信息为：" + ex.Message);
                    throw new Exception("调用OA审批接口出错，错误信息为：" + ex.Message);
                }
            }


            //将OA接口返回结果转成对象
            string OAFlowID = "";
            ResultInfo OAResult = JsonHelper.JsonDeserialize<ResultInfo>(result);
            if (OAResult.type == "1")
            {
                logger.Error("OA接口返回错误，错误信息为:" + OAResult.backmsg);
                throw new Exception("OA接口返回错误，错误信息为:" + OAResult.backmsg);
            }
            else
            {
                //检查结果集
                foreach (ResultList rList in OAResult.resultlist)
                {
                    if (rList.status == "1")
                    {
                        throw new Exception("OA接口返回错误，错误信息为:" + rList.msg);
                        throw new Exception("OA接口返回错误，错误信息为:" + OAResult.backmsg);
                    }
                    OAFlowID = rList.requestid;
                }
            }
            return OAFlowID;
        }

        /// <summary>
        /// 调用OA接口服务
        /// </summary>
        /// <returns></returns>
        public ResultInfo OAServiceDelete(string requestid)
        {
            //参数1：准入码，参数2：json
            string OACode = PubFunction.GetOACode();
            string result = string.Empty;
            string IsTest = PubFunction.IsOATest();
            ILogger logger = LoggerManager.GetLogger(typeof(CacheManager));
            if (IsTest == "是")//测试环境
            {
                OAWorkflowService4Test.WorkflowServiceToOtherSystemPortTypeClient service = new OAWorkflowService4Test.WorkflowServiceToOtherSystemPortTypeClient();
                try
                {
                    result = service.deleteWorkflow(OACode, requestid);
                    logger.Error("调用OA审批内网测试接口成功，返回的结果为：" + result);
                }
                catch (Exception ex)
                {
                    logger.Error("调用OA审批内网测试接口出错，错误信息为：" + ex.Message);
                    throw new Exception("调用OA审批内网测试接口出错，错误信息为：" + ex.Message);
                }
            }
            else//生产环境
            {
                OAWorkflowService.WorkflowServiceToOtherSystemPortTypeClient service = new OAWorkflowService.WorkflowServiceToOtherSystemPortTypeClient();
                try
                {
                    result = service.deleteWorkflow(OACode, requestid);
                    logger.Error("调用OA审批接口成功，返回的结果为：" + result);
                }
                catch (Exception ex)
                {
                    logger.Error("调用OA审批接口出错，错误信息为：" + ex.Message);
                    throw new Exception("调用OA审批接口出错，错误信息为：" + ex.Message);
                }
            }


            //将OA接口返回结果转成对象
            ResultInfo OAResult = JsonHelper.JsonDeserialize<ResultInfo>(result);
            logger.Error("删除OA审批流返回结果：" + result);
            if (OAResult.type == "1")
            {
                logger.Error("OA接口返回错误，错误信息为:" + OAResult.backmsg);
                throw new Exception("OA接口返回错误，错误信息为:" + OAResult.backmsg);
            }
            return OAResult;
        }
    }
}
