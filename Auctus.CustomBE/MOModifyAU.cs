using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UFIDA.U9.Base;
using UFIDA.U9.Base.UserRole;
using UFIDA.U9.MO.Enums;
using UFIDA.U9.MO.MOModify;

namespace Auctus.CustomBE
{
    public class MOModifyAU : UFSoft.UBF.Eventing.IEventSubscriber
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
            UFIDA.U9.MO.MOModify.MOModify entity = (UFIDA.U9.MO.MOModify.MOModify)key.GetEntity();
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
                        //提交操作
                        if (entity.OriginalData.Status == MOModifyStatusEnum.Openend && entity.Status == MOModifyStatusEnum.Approving&&entity.DocType.DescFlexField.PrivateDescSeg1=="1")
                        {
                            //TODO:变更单提交到OA
                            Dictionary<string, object> dicResult = new Dictionary<string, object>();
                            //用户
                            User user = User.Finder.FindByID(Context.LoginUserID);                            
                            
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
