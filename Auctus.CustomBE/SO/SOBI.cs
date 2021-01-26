using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auctus.CustomBE.SO
{
   public class SOBI : UFSoft.UBF.Eventing.IEventSubscriber
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
            UFIDA.U9.SM.SO.SO entity = (UFIDA.U9.SM.SO.SO)key.GetEntity();
            if (entity == null)
            {
                return;
            }
            else
            {
                try
                {
                    if (entity.SysState == UFSoft.UBF.PL.Engine.ObjectState.Inserted)//插入操作
                    {
                        //清空复制的OA流程扩展字段字段内容
                        entity.DescFlexField.PrivateDescSeg2 = "";
                        entity.DescFlexField.PrivateDescSeg3= "";
                        entity.DescFlexField.PrivateDescSeg4 = "";
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
