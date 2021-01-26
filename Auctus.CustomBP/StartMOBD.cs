using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UFSoft.UBF.Util.DataAccess;

namespace Auctus.CustomBP
{
    public class StartMOBD : UFSoft.UBF.Service.BPSVExtendBase
    {
        public override void AfterDo(object bp, ref object result)
        {
            UFIDA.U9.MO.MO.StartMO moBP = bp as UFIDA.U9.MO.MO.StartMO;           
            //UFIDA.U9.MO.MO.MO mo=UFIDA.U9.MO.MO.MO.Finder.FindAll()
            for (int i = 0; i < moBP.StartInfoDTOs.Count; i++)
            {
                try
                {
                    int mesOnlineQty = 0;//MES投入数量
                    if (moBP.StartInfoDTOs[i].BusinessDirection.Value==1)
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
                    dp.Add(DataParamFactory.Create("Status", moBP.StartInfoDTOs[i].BusinessDirection.Value, ParameterDirection.Input, DbType.Int32,32));
                    dp.Add(DataParamFactory.Create("MesCompleteQty", mesOnlineQty, ParameterDirection.Input, DbType.Int32,32));
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

        public override void BeforeDo(object bp)
        {         
        }
    }
}
