using Auctus.Common;
using Auctus.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UFIDA.U9.Base;
using UFIDA.U9.Base.UserRole;
using UFIDA.U9.CBO.PubBE.YYC;
using UFSoft.UBF.Sys.Database;
using UFSoft.UBF.Util.DataAccess;

namespace Auctus.CustomBP
{
    public class StartMOBD : UFSoft.UBF.Service.BPSVExtendBase
    {
        public override void BeforeDo(object bp){}
        public override void AfterDo(object bp, ref object result)
        {
            UFIDA.U9.MO.MO.StartMO moBP = bp as UFIDA.U9.MO.MO.StartMO;
            //UFIDA.U9.MO.MO.MO mo=UFIDA.U9.MO.MO.MO.Finder.FindAll()
            for (int i = 0; i < moBP.StartInfoDTOs.Count; i++)
            {
                try
                {
                    int mesOnlineQty = 0;//MES投入数量
                    if (moBP.StartInfoDTOs[i].BusinessDirection.Value == 1)
                    {
                        string content = HttpMethod.PostMethod("http://192.168.30.8:9090/MXQHService/common.asmx/DoBefore", GenPara("ExecPlan", "CompleteRpt", "GetQty", "{WorkOrder:'" + moBP.StartInfoDTOs[i].MOKey.GetEntity().DocNo + "'}"));
                        MesReturnData r = Newtonsoft.Json.JsonConvert.DeserializeObject<MesReturnData>(content);
                        if (r.data.Count == 0)
                        {
                            mesOnlineQty = 0;
                        }
                        else
                        {
                            mesOnlineQty = r.data[0].OnLineQty;
                        }
                    }
                    DataParamList dp = new DataParamList();
                    dp.Add(DataParamFactory.Create("LoginUser", UFIDA.U9.Base.Context.LoginUser, ParameterDirection.Input, DbType.String, 50));
                    dp.Add(DataParamFactory.Create("MOID", moBP.StartInfoDTOs[i].MOKey.ID, ParameterDirection.Input, DbType.String, 50));
                    dp.Add(DataParamFactory.Create("Status", moBP.StartInfoDTOs[i].BusinessDirection.Value, ParameterDirection.Input, DbType.Int32, 32));
                    dp.Add(DataParamFactory.Create("MesCompleteQty", mesOnlineQty, ParameterDirection.Input, DbType.Int32, 32));
                    dp.Add(DataParamFactory.CreateOutput("Result", DbType.String));
                    DataAccessor.RunSP("sp_Auctus_BP_MOStartInfoAD", dp);
                    string output = dp["Result"].Value.ToString();
                    if (output != "1")
                    {
                        throw new Exception(output);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }
        private Dictionary<string, string> GenPara(string method, string planName, string shortName, string json)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("method", method);
            Dictionary<string, string> dd = new Dictionary<string, string>();
            dd.Add("planName", planName);
            dd.Add("shortName", shortName);
            dd.Add("strJson", json);
            dic.Add("Json", System.Web.HttpUtility.UrlEncode(Newtonsoft.Json.JsonConvert.SerializeObject(dd)));
            return dic;
        }

        //public override void BeforeDo(object bp)
        //{
        //    UFIDA.U9.MO.MO.StartMO moBP = bp as UFIDA.U9.MO.MO.StartMO;
        //    //UFIDA.U9.MO.MO.MO mo=UFIDA.U9.MO.MO.MO.Finder.FindAll()
        //    for (int i = 0; i < moBP.StartInfoDTOs.Count; i++)
        //    {
        //        string OAFlowID = "";
        //        try
        //        {
        //            int mesOnlineQty = 0;//MES投入数量
        //            if (moBP.StartInfoDTOs[i].BusinessDirection.Value == 1)
        //            {
        //                string content = HttpMethod.PostMethod("http://192.168.30.8:9090/MXQHService/common.asmx/DoBefore", GenPara("ExecPlan", "CompleteRpt", "GetQty", "{WorkOrder:'" + moBP.StartInfoDTOs[i].MOKey.GetEntity().DocNo + "'}"));
        //                MesReturnData r = Newtonsoft.Json.JsonConvert.DeserializeObject<MesReturnData>(content);
        //                if (r.data.Count == 0)
        //                {
        //                    mesOnlineQty = 0;
        //                }
        //                else
        //                {
        //                    mesOnlineQty = r.data[0].OnLineQty;
        //                }
        //            }
        //            DataParamList dp = new DataParamList();
        //            dp.Add(DataParamFactory.Create("LoginUser", UFIDA.U9.Base.Context.LoginUser, ParameterDirection.Input, DbType.String, 50));
        //            dp.Add(DataParamFactory.Create("MOID", moBP.StartInfoDTOs[i].MOKey.ID, ParameterDirection.Input, DbType.String, 50));
        //            dp.Add(DataParamFactory.Create("Status", moBP.StartInfoDTOs[i].BusinessDirection.Value, ParameterDirection.Input, DbType.Int32, 32));
        //            dp.Add(DataParamFactory.Create("MesCompleteQty", mesOnlineQty, ParameterDirection.Input, DbType.Int32, 32));
        //            dp.Add(DataParamFactory.CreateOutput("Result", DbType.String));
        //            DataAccessor.RunSP("sp_Auctus_BP_MOStartInfoBD", dp);
        //            string output = dp["Result"].Value.ToString();
        //            if (output != "1" && moBP.StartInfoDTOs[i].BusinessDirection.Value == 0)
        //            {
        //                //获取可开工/返工信息
        //                string sql = "select * from v_Cust_MOStartInfo where ID=" + moBP.StartInfoDTOs[i].MOKey.ID.ToString() + ";SELECT * FROM dbo.v_Cust_MOStartHistory where ID=" + moBP.StartInfoDTOs[i].MOKey.ID.ToString();
        //                DataSet ds = new DataSet();
        //                DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), sql, null, out ds);
        //                PubFunction pubFun = new PubFunction();
        //                //校验OA是否存在在途流程
        //                string docNo = ds.Tables[0].Rows[0]["DocNo"].ToString();
        //                Dictionary<string, string> dicCheck = new Dictionary<string, string>();
        //                dicCheck.Add("MainTable", "formtable_main_191");//formtable_main_204
        //                dicCheck.Add("DocNo", docNo);
        //                dicCheck.Add("field", "scdd");
        //                List<Dictionary<string, string>> liCheck = new List<Dictionary<string, string>>();
        //                liCheck.Add(dicCheck);
        //                int workflowCount = pubFun.OACheckService(JsonHelper.GetJsonJS(liCheck));
        //                if (workflowCount > 0)
        //                {
        //                    throw new Exception("OA中有当前工单的审核中开工/返工流程，请先处理该流程！");
        //                }
        //                //不可开工，触发流程到OA
        //                #region 
        //                Dictionary<string, object> dicResult = new Dictionary<string, object>();
        //                Dictionary<string, object> dicMain = new Dictionary<string, object>();
        //                User user = User.Finder.FindByID(Context.LoginUserID);
        //                BaseInfo baseInfo = Utils.GenerateOABaseInfo(user.Code, PubFunction.GetOAInfoByCode("11"), moBP.StartInfoDTOs[i].MOKey.ID.ToString(), 1, "", 1, "生产订单开工（U9发起）-" + docNo);
        //                dicMain = GenerateMainInfo(moBP.StartInfoDTOs[i], user.Code, ds);
        //                dicResult.Add("base", baseInfo);
        //                dicResult.Add("main", dicMain);
        //                List<Dictionary<string, object>> li = new List<Dictionary<string, object>>();
        //                li.Add(dicResult);
        //                string json = JsonHelper.GetJsonJS(li);
        //                OAFlowID = pubFun.OAService(json);
        //                //string sql = string.Format("update MO_MO set DescFlexField_PrivateDescSeg25='{0}' where ID={1}", OAFlowID, moBP.StartInfoDTOs[i].MOKey.ID);
        //                //DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), sql, null);//更新sql 
        //                #endregion

        //                throw new Exception(output);

        //            }
        //            else
        //            {
        //                throw new Exception(output);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new Exception(ex.Message);
        //        }
        //    }
        //}
        public Dictionary<string, object> GenerateMainInfo(UFIDA.U9.MO.MO.StartInfoDTO e, string userCode,DataSet ds)
        {
            Dictionary<string, object> dicMain = new Dictionary<string, object>();
            dicMain = Utils.GenerateOAUserInfo(dicMain, userCode, DateTime.Now.ToShortDateString());
            Dictionary<string, object> dicValue = new Dictionary<string, object>();
            dicValue.Add("value", Context.LoginOrg.ID);
            dicMain.Add("u9zz", dicValue);
            foreach (DataRow item in ds.Tables[0].Rows)
            {
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", item["DocNo"].ToString());
                dicMain.Add("scdd", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", item["Code"].ToString());
                dicMain.Add("lh", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", item["Name"].ToString());
                dicMain.Add("pm", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", item["ProductQty"].ToString());
                dicMain.Add("scsl", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", item["UOM"].ToString());
                dicMain.Add("scdw", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", item["TotalStartQty"].ToString());
                dicMain.Add("ljykgsl", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", Convert.ToInt32(item["UnStartQty"]));
                dicMain.Add("wkgsl", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", item["CanStartQty"].ToString());
                dicMain.Add("kkgsl", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", item["CanAntiStartQty"].ToString());
                dicMain.Add("bcfkgsl", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", e.BusinessDirection.Value.ToString());
                dicMain.Add("kglb", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", e.MOKey.ID);
                dicMain.Add("scddid", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value",e.BusinessDateTime.ToString("yyyy-MM-dd"));
                dicMain.Add("ywsj", dicValue);
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", item["TotalIssuedQty"]);
                dicMain.Add("ljqtslts", dicValue);
                //dicValue = new Dictionary<string, object>();
                //dicValue.Add("value", e.BusinessDirection.Value.ToString());
                //dicMain.Add("xh", dicValue);
            }
            return dicMain;
        }
    }
}
