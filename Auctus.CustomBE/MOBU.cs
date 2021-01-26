using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UFSoft.UBF.Util.DataAccess;

namespace Auctus.CustomBE
{
    public class MOBU : UFSoft.UBF.Eventing.IEventSubscriber
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
            UFIDA.U9.MO.MO.MO entity = (UFIDA.U9.MO.MO.MO)key.GetEntity();
            if (entity == null)
            {
                return;
            }
            else
            {
                try
                {
                    if (entity.SysState == UFSoft.UBF.PL.Engine.ObjectState.Updated)//更新操作
                    {
                        if (!entity.OriginalData.Cancel.Canceled && entity.Cancel.Canceled)//作废操作：校验是否有发料信息
                        {
                            foreach (UFIDA.U9.MO.MO.MOPickList item in entity.MOPickLists)
                            {
                                if (item.IssuedQty > 0)
                                throw new Exception("当前工单存在已发料的备料行，请退料后再作废!");
                            }
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
