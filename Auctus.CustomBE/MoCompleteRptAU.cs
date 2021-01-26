using Auctus.Common;
using Auctus.Model;
using DataTransfer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UFIDA.U9.Base;
using UFIDA.U9.Base.UserRole;
using UFIDA.U9.CBO.PubBE.YYC;
using UFSoft.UBF.Sys.Database;
using UFSoft.UBF.Util.DataAccess;

namespace Auctus.CustomBE
{
    public class MoCompleteRptAU : UFSoft.UBF.Eventing.IEventSubscriber
    {
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
            //UFIDA.U9.InvDoc.TransferIn.TransferIn transferIn = (UFIDA.U9.InvDoc.TransferIn.TransferIn)key.GetEntity();
            UFIDA.U9.MO.Complete.CompleteRpt entity = (UFIDA.U9.MO.Complete.CompleteRpt)key.GetEntity();
            if (entity == null)
            {
                return;
            }
            else
            {
                if (entity.Org.Code == "300")
                {
                    try
                    {
                        //校验mes是否完工足够的数量
                        string DocNo = entity.DocNo;
                        string result = "";
                        string content = HttpMethod.PostMethod("http://192.168.30.8:9090/MXQHService/common.asmx/DoBefore", GenPara("ExecPlan", "CompleteRpt", "GetQty", "{WorkOrder:'" + entity.MO.DocNo + "'}"));
                        MesReturnData r = Newtonsoft.Json.JsonConvert.DeserializeObject<MesReturnData>(content);
                        int CompleteQty = 0;
                        if (r.data.Count == 0)
                        {
                            CompleteQty = 0;
                        }
                        else
                        {
                            CompleteQty = r.data[0].CompleteQty;
                        }
                        DataParamList dp = new DataParamList();
                        dp.Add(DataParamFactory.Create("DocNo", entity.MO.DocNo, ParameterDirection.Input, DbType.String, 50));
                        dp.Add(DataParamFactory.Create("CompleteQty", CompleteQty, ParameterDirection.Input, DbType.Int32, 64));
                        dp.Add(DataParamFactory.CreateOutput("Result", DbType.String));
                        DataAccessor.RunSP("sp_Auctus_BE_MOCompleteQtyAU", dp);
                        result = dp["Result"].Value.ToString();
                        if (result != "1")
                        {
                            throw new Exception(result);
                        }

                        //流程触发到OA
                        if (entity.DocState == UFIDA.U9.MO.Enums.CompleteRptStateEnum.Approving && entity.OriginalData.DocState == UFIDA.U9.MO.Enums.CompleteRptStateEnum.Opened && entity.DocType.DescFlexField.PrivateDescSeg1 == "1")
                        {
                            Dictionary<string, object> dicResult = new Dictionary<string, object>();
                            Dictionary<string, object> dicMain = new Dictionary<string, object>();
                            User user = User.Finder.FindByID(Context.LoginUserID);//U9用户信息
                            #region OA信息                           
                            BaseInfo baseInfo;
                            if (entity.DescFlexField.PrivateDescSeg3 == "是")//驳回后提交
                            {
                                baseInfo = Auctus.Common.Utils.GenerateOABaseInfo(user.Code, PubFunction.GetOAInfoByCode("10"), entity.ID.ToString(), 1, entity.DescFlexField.PrivateDescSeg2, 1, "完工报告：" + entity.DocNo);
                            }
                            else//非驳回提交
                            {
                                baseInfo = Auctus.Common.Utils.GenerateOABaseInfo(user.Code, PubFunction.GetOAInfoByCode("10"), entity.ID.ToString(), 1, "", 1, "完工报告：" + entity.DocNo);
                            }
                            #endregion
                            dicMain = GenerateMainInfo(entity, user.Code);
                            dicResult.Add("base", baseInfo);
                            dicResult.Add("main", dicMain);
                            List<Dictionary<string, object>> li = new List<Dictionary<string, object>>();
                            li.Add(dicResult);
                            string json = JsonHelper.GetJsonJS(li);
                            Utils.LogError("完工报告JSON");
                            Utils.LogError(json);
                            PubFunction pubFun = new PubFunction();
                            string OAFlowID = pubFun.OAService(json);
                            //更新扩展字段：OA流程ID和是否驳回
                            string sql = string.Format("update MO_CompleteRpt set DescFlexField_PrivateDescSeg3='否',DescFlexField_PrivateDescSeg2='{0}' where ID={1}", OAFlowID, entity.ID);
                            DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), sql, null);//更新sql 
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
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

        public Dictionary<string, object> GenerateMainInfo(UFIDA.U9.MO.Complete.CompleteRpt entity, string userCode)
        {
            Dictionary<string, object> dicMain = new Dictionary<string, object>();
            Dictionary<string, object> dicValue = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(entity.DescFlexField.PrivateDescSeg2) && entity.DescFlexField.PrivateDescSeg3 != "是")//不是驳回，存在OA流程ID，代表是弃审后重新提交
            {
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", entity.DescFlexField.PrivateDescSeg2);
                dicMain.Add("gllc", dicValue);
            }
            dicMain = Auctus.Common.Utils.GenerateOAUserInfo(dicMain, userCode, DateTime.Now.ToString("yyyy-MM-dd"));
            dicValue = new Dictionary<string, object>();
            dicValue.Add("value", entity.ID);
            dicMain.Add("fpkid", dicValue);
            dicValue = new Dictionary<string, object>();
            dicValue.Add("value", entity.Org.ID);
            dicMain.Add("u9zz", dicValue);
            dicValue = new Dictionary<string, object>();
            dicValue.Add("value", entity.Remark);
            dicMain.Add("beizu", dicValue);
            dicValue = new Dictionary<string, object>();
            dicValue.Add("value", entity.DocType.Code);
            dicMain.Add("djlx", dicValue);
            dicValue = new Dictionary<string, object>();
            dicValue.Add("value", entity.Item.Code);
            dicMain.Add("lh", dicValue);
            dicValue = new Dictionary<string, object>();
            dicValue.Add("value", entity.Item.Name);
            dicMain.Add("pm", dicValue);
            dicValue = new Dictionary<string, object>();
            dicValue.Add("value", entity.Item.SPECS);
            dicMain.Add("gg", dicValue);
            dicValue = new Dictionary<string, object>();
            dicValue.Add("value", entity.Item.DescFlexField.PrivateDescSeg22);
            dicMain.Add("mrpfl", dicValue);
            dicValue = new Dictionary<string, object>();
            dicValue.Add("value", entity.DocNo);
            dicMain.Add("dh", dicValue);
            if (entity.HandlePerson != null)
            {
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", entity.HandlePerson.Name);
                dicMain.Add("rkr", dicValue);
            }
            if (entity.HandleDept != null)
            {
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", entity.HandleDept.Name);
                dicMain.Add("rkbm", dicValue);
            }
            dicValue = new Dictionary<string, object>();
            dicValue.Add("value", entity.MO.DescFlexField.PrivateDescSeg6);
            dicMain.Add("scxb", dicValue);
            dicValue = new Dictionary<string, object>();
            dicValue.Add("value", entity.MO.DocNo);
            dicMain.Add("scdd", dicValue);

            dicValue = new Dictionary<string, object>();
            dicValue.Add("value", entity.CompleteQty);
            dicMain.Add("wgsl", dicValue);
            dicValue = new Dictionary<string, object>();
            dicValue.Add("value", entity.CompleteDate.ToString("yyyy-MM-dd"));
            dicMain.Add("wgsj", dicValue);
            dicValue = new Dictionary<string, object>();
            dicValue.Add("value", Convert.ToInt32(entity.RcvQtyByProductUOM));
            dicMain.Add("sjrksl", dicValue);
            dicValue = new Dictionary<string, object>();
            dicValue.Add("value", entity.ActualRcvTime.ToString("yyyy-MM-dd"));
            dicMain.Add("sjrksj", dicValue);
            dicValue = new Dictionary<string, object>();
            dicValue.Add("value", entity.OwnerOrg.Name);
            dicMain.Add("hzzz", dicValue);
            dicValue = new Dictionary<string, object>();
            dicValue.Add("value", entity.ProductLotNo);
            dicMain.Add("wgph", dicValue);
            dicValue = new Dictionary<string, object>();
            dicValue.Add("value", entity.Item.DescFlexField.PrivateDescSeg21);
            dicMain.Add("xmdh", dicValue);
            dicValue = new Dictionary<string, object>();
            dicValue.Add("value", entity.Item.DescFlexField.PrivateDescSeg20);
            dicMain.Add("xmbm", dicValue);
            dicValue = new Dictionary<string, object>();
            dicValue.Add("value", entity.Item.DescFlexField.PrivateDescSeg19);
            dicMain.Add("khcpmc", dicValue);
            dicValue = new Dictionary<string, object>();
            dicValue.Add("value", entity.MO.DemandCode.Name);
            dicMain.Add("xqfl", dicValue);
            if (entity.RcvWh != null)
            {
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value", entity.RcvWh.ID);
                dicMain.Add("ccdd", dicValue);
            }
            if (entity.CompleteRptRcvLines.Count>0)
            {
                dicValue = new Dictionary<string, object>();
                dicValue.Add("value",entity.CompleteRptRcvLines[0].RcvLotNo);
                dicMain.Add("rkph",dicValue);
            }
            return dicMain;
        }
    }
}