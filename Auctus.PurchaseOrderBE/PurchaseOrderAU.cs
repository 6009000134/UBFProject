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
using UFIDA.U9.PM.PO;

namespace Auctus.PurchaseOrderBE
{
    /// <summary>
    /// 采购单更新后
    /// </summary>
    public class PurchaseOrderAU : UFSoft.UBF.Eventing.IEventSubscriber
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
            UFIDA.U9.PM.PO.POLine poLine = (UFIDA.U9.PM.PO.POLine)key.GetEntity();

            if (poLine == null) return;
            else
            {
                UFIDA.U9.PM.PO.PurchaseOrder po = poLine.PurchaseOrder;
                //委外订单短缺关闭时，若财务未核销（备料单行核销数量<>已发料数量），则不允许关闭
                if (po.Org.Code == "300" && po.SubType.Value.ToString() != "-1" && poLine.Status == PODOCStatusEnum.ClosedShort)
                {
                    try
                    {                        
                        DataParamList dp = new DataParamList();
                        dp.Add(DataParamFactory.Create("DocNo", po.DocNo, ParameterDirection.Input, DbType.String, 50));
                        dp.Add(DataParamFactory.CreateOutput("Result", DbType.String));
                        DataAccessor.RunSP("sp_Auctus_BE_PurchaseOrderAU", dp);
                        string result = dp["Result"].Value.ToString();
                        if (result != "1")
                        {
                            throw new Exception(result);
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
