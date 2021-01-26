using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UFSoft.UBF.Util.DataAccess;
using System.Data;
using UFSoft.UBF.Sys.Database;
using System.Data.SqlClient;

namespace Auctus.InvDocBE
{
    public class InvDocAfterInserted : UFSoft.UBF.Eventing.IEventSubscriber
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
            UFIDA.U9.InvDoc.TransferIn.TransferIn transferIn = (UFIDA.U9.InvDoc.TransferIn.TransferIn)key.GetEntity();
            if (transferIn == null)
            {
                return;
            }
            else
            {
                if (transferIn.Org.Code == "300")
                {
                    try
                    {
                        if (transferIn.DocType.Name == "委外加工发料" || transferIn.DocType.Name == "委外加工退料")
                        {
                            string DocNo = transferIn.DocNo;
                            string result = "";
                            DataParamList dp = new DataParamList();
                            dp.Add(DataParamFactory.Create("DocNo", transferIn.DocNo, ParameterDirection.Input, DbType.String, 50));
                            dp.Add(DataParamFactory.CreateOutput("Result", DbType.String));
                            DataAccessor.RunSP("sp_Auctus_LockTransferIn", dp);
                            result = dp["Result"].Value.ToString();
                            if (result != "1")
                            {
                                throw new Exception(result);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
            }
        }
    }
}
