using DataTransfer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UFSoft.UBF.Util.DataAccess;

namespace Auctus.CustomBE
{
    public class MoCompleteRptAI : UFSoft.UBF.Eventing.IEventSubscriber
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
                        string DocNo = entity.DocNo;
                        string result = "";
                        string content = HttpMethod.PostMethod("http://192.168.30.8:9090/MXQHService/common.asmx/DoBefore", GenPara("ExecPlan", "CompleteRpt", "GetQty", "{WorkOrder:'" + entity.MO.DocNo + "'}"));
                        MesReturnData r = Newtonsoft.Json.JsonConvert.DeserializeObject<MesReturnData>(content);
                        int CompleteQty = 0;
                        if (r.data.Count==0)
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
                        DataAccessor.RunSP("sp_Auctus_BE_MOCompleteQtyAI", dp);
                        result = dp["Result"].Value.ToString();
                        if (result != "1")
                        {
                            throw new Exception(result);
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


    }
}
