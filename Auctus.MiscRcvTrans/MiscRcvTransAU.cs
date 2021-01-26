using System;
using System.Data;
using UFIDA.U9.InvDoc.Enums;
using UFSoft.UBF.Util.DataAccess;

namespace Auctus.MiscRcvTrans
{
    public class MiscRcvTransAU : UFSoft.UBF.Eventing.IEventSubscriber
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
            UFIDA.U9.InvDoc.MiscRcv.MiscRcvTrans miscRcv = (UFIDA.U9.InvDoc.MiscRcv.MiscRcvTrans)key.GetEntity();

            if (miscRcv == null) return;
            else
            {
                if (miscRcv.Org.Code == "300")
                {
                    try
                    {
                        //不允许“制损报废入库”弃审
                        if (miscRcv.MiscRcvDocType.Name.Trim() == "制损报废入库")
                        {
                            //单据从已核准状态到开立（弃审操作）
                            if (miscRcv.OriginalData.Status == INVDocStatus.Approved && miscRcv.Status == INVDocStatus.Open)
                            {
                                //存储过程控制取消退料操作是否开启                                
                                DataParamList dpL = new DataParamList();
                                dpL.Add(DataParamFactory.Create("Type", "MiscRcvBtnUnDoApprove", ParameterDirection.Input, DbType.String, 50));//禁用杂收单弃审功能
                                dpL.Add(DataParamFactory.CreateOutput("IsOpen", DbType.String));
                                DataAccessor.RunSP("sp_Auctus_BEPlugin_Control", dpL);
                                string isOpen = dpL["IsOpen"].Value.ToString();
                                if (isOpen == "1")
                                {
                                    throw new Exception("此单据为"+miscRcv.MiscRcvTransLs[0].DescFlexSegments.PrivateDescSeg29+"自动生成单据，不允许弃审！");
                                }
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