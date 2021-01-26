using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UFSoft.UBF.Eventing;
using UFSoft.UBF.Business;
using System.IO;
using UFSoft.UBF.Util.DataAccess;
using System.Data;
using UFSoft.UBF.Sys.Database;
using System.Data.SqlClient;

namespace Auctus.ReceivementBE
{
    public class ReceivementAU : UFSoft.UBF.Eventing.IEventSubscriber
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
            UFIDA.U9.PM.Rcv.Receivement receivement = (UFIDA.U9.PM.Rcv.Receivement)key.GetEntity();

            if (receivement == null) return;
            else
            {
                if (receivement.Org.Code == "300"&&receivement.Status== UFIDA.U9.PM.Rcv.RcvStatusEnum.Opened && receivement.RcvDocType.DescFlexField.PrivateDescSeg1 != "1")
                {
                    try
                    {
                        DataParamList dp = new DataParamList();
                        dp.Add(DataParamFactory.Create("DocNo", receivement.DocNo, ParameterDirection.Input, DbType.String, 50));
                        DataAccessor.RunSP("sp_Auctus_BE_Receivement", dp);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }//end if
            }
        }
    }
}
