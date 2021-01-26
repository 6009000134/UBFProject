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

namespace Auctus.PurchaseOrderBE
{
    class PurchaseOrderAI : UFSoft.UBF.Eventing.IEventSubscriber
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
            UFIDA.U9.PM.PO.PurchaseOrder po = (UFIDA.U9.PM.PO.PurchaseOrder)key.GetEntity();

            if (po == null) return;
            else
            {
                //锁WMO转WPO功能
                if (po.Org.Code == "300")//po.SubType委外订单
                {
                    try
                    {
                        DataParamList dp = new DataParamList();
                        dp.Add(DataParamFactory.Create("DocNo", po.DocNo, ParameterDirection.Input, DbType.String, 50));
                        dp.Add(DataParamFactory.CreateOutput("Result", DbType.String));
                        DataAccessor.RunSP("sp_Auctus_BE_PurchaseOrderAI", dp);
                        string result = dp["Result"].Value.ToString();
                        if (result != "0")
                        {
                            throw new Exception(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                ////锁WMO转WPO功能
                //if (po.Org.Code == "300" && po.SubType.Value.ToString() != "-1")//po.SubType委外订单
                //{
                //    try
                //    {
                //        DataParamList dp = new DataParamList();
                //        dp.Add(DataParamFactory.Create("DocNo", po.DocNo, ParameterDirection.Input, DbType.String, 50));
                //        dp.Add(DataParamFactory.CreateOutput("Result", DbType.String));
                //        DataAccessor.RunSP("sp_Auctus_BE_PurchaseOrderAI", dp);
                //        string result = dp["Result"].Value.ToString();
                //        if (result != "0")
                //        {
                //            throw new Exception(result);
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        throw new Exception(ex.Message);
                //    }
                //}
                //end if
                ////锁WMO转WPO功能
                //if (po.Org.Code == "300")//po.SubType委外订单
                //{                    
                //    try
                //    {
                //        DataParamList dp = new DataParamList();
                //        dp.Add(DataParamFactory.Create("DocNo", po.DocNo, ParameterDirection.Input, DbType.String, 50));
                //        dp.Add(DataParamFactory.CreateOutput("Result", DbType.String));
                //        DataAccessor.RunSP("sp_Auctus_BE_PurchaseOrderAI", dp);
                //        string result = dp["Result"].Value.ToString();
                //        if (result != "1")
                //        {
                //            throw new Exception(result);
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        throw new Exception(ex.Message);
                //    }
                //}//end if  
            }
        }
    }
}
