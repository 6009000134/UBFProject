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

namespace Auctus.POModify
{
    public class POModifyAU : UFSoft.UBF.Eventing.IEventSubscriber
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
            UFIDA.U9.PM.POChange.POModify poModify = (UFIDA.U9.PM.POChange.POModify)key.GetEntity();

            if (poModify == null) return;
            else
            {
                //验证标准采购或委外采购采购数量>需求数量，且采购数量-需求数量<最小交货量(最小叫货量不为0)
                if (poModify.Org.Code == "300")
                {
                    try
                    {
                        if (poModify.Status == UFIDA.U9.PM.POChange.POChangeListStatusEnum.Approved)
                        {                            
                            DataParamList dp = new DataParamList();
                            dp.Add(DataParamFactory.Create("DocNo", poModify.DocNo, ParameterDirection.Input, DbType.String, 50));
                            dp.Add(DataParamFactory.CreateOutput("Result", DbType.String));
                            DataAccessor.RunSP("sp_Auctus_BE_POModifyAU", dp);
                            string result = dp["Result"].Value.ToString();
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
                }//end if
            }
        }

    }
}
