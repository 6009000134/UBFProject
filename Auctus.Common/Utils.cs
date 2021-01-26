using Auctus.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UFSoft.UBF.Util.Cache;
using UFSoft.UBF.Util.Log;

namespace Auctus.Common
{
    public class Utils
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
        /// 设置申请人基本信息（申请人、工号、岗位、部门、分部、申请日期：yyyy-MM-dd格式）
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, object> GenerateOAUserInfo(Dictionary<string, object> dicMain, string userCode, string dt)
        {
            Dictionary<string, object> dicTemp = new Dictionary<string, object>();
            dicTemp.Add("value", userCode);
            dicTemp.Add("transrule", "getUseridByWorkcode");
            dicMain.Add("sqr", dicTemp);
            dicTemp = new Dictionary<string, object>();
            dicTemp.Add("value", userCode);
            dicTemp.Add("transrule", "getWorkcodeByU9code");
            dicMain.Add("sqrgh", dicTemp);
            dicTemp = new Dictionary<string, object>();
            dicTemp.Add("value", userCode);
            dicTemp.Add("transrule", "getJobidByWorkcode");
            dicMain.Add("sqrgw", dicTemp);
            dicTemp = new Dictionary<string, object>();
            dicTemp.Add("value", userCode);
            dicTemp.Add("transrule", "getDeptidByWorkcode");
            dicMain.Add("ssbm", dicTemp);
            dicTemp = new Dictionary<string, object>();
            dicTemp.Add("value", userCode);
            dicTemp.Add("transrule", "getSubcomidByWorkcode");
            dicMain.Add("ssgs", dicTemp);
            dicTemp = new Dictionary<string, object>();
            dicTemp.Add("value", dt);
            dicMain.Add("sqrq", dicTemp);
            return dicMain;
        }
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="msg"></param>
        public static void LogError(string msg)
        {
            ILogger log = LoggerManager.GetLogger(typeof(CacheManager));
            log.Error(msg);
        }

    }
}
