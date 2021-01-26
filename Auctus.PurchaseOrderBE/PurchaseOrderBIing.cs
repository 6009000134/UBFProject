using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auctus.PurchaseOrderBE
{
    /// <summary>
    /// BeforeInserting
    /// 当生成的采购单中物料的产品线（扩展字段28）全部为功放类时，生单成功；
    /// 若料品全部是非功放类时，生单成功；
    /// 若同时存在功放和非功放（整机、芯片、模块等等）生单失败
    /// 
    /// </summary>
    public class PurchaseOrderBIing : UFSoft.UBF.Eventing.IEventSubscriber
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
                if (po.SubType.Value.ToString() == "-1")//标准采购
                {
                    string productLine = "";
                    productLine = po.POLines[0].ItemInfo.ItemID.DescFlexField.PrivateDescSeg28;                    
                    for (int i = 0; i < po.POLines.Count; i++)
                    {
                        if (productLine == "02")//当存在功放料品
                        {
                            if (po.POLines[i].ItemInfo.ItemID.DescFlexField.PrivateDescSeg28 != "02")//存在非功放
                            {
                                throw new Exception("订单中同时存在功放和非功放料品，创建失败！");
                            }
                            productLine = "02";
                        }
                        else//存在非功放料品
                        {
                            if (po.POLines[i].ItemInfo.ItemID.DescFlexField.PrivateDescSeg28 == "02")
                            {
                                throw new Exception("订单中同时存在功放和非功放料品，创建失败！");
                            }
                            productLine = "99";
                        }
                    }
                    po.DescFlexField.PrivateDescSeg1 = productLine;
                }
            }
        }
    }
}
