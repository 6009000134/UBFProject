using System;
using UFSoft.UBF.Util.DataAccess;
using System.Data;

namespace Auctus.SOBE
{
    /// <summary>
    /// 销售订单    
    /// </summary>
    public class SOAI : UFSoft.UBF.Eventing.IEventSubscriber
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
            UFIDA.U9.SM.SO.SO so = (UFIDA.U9.SM.SO.SO)key.GetEntity();

            if (so == null)
            {
                return;
            }
            else
            {
                if (so.Org.Code == "300"&&so.DescFlexField.PrivateDescSeg1.Contains("PO"))
                {
                    try
                    {
                        DataParamList dp = new DataParamList();
                        dp.Add(DataParamFactory.Create("SODocNo", so.DocNo, ParameterDirection.Input, DbType.String, 50));
                        dp.Add(DataParamFactory.Create("PoDocNo", so.DescFlexField.PrivateDescSeg1, ParameterDirection.Input, DbType.String, 50));
                        dp.Add(DataParamFactory.CreateOutput("Result", DbType.String));
                        DataAccessor.RunSP("sp_Auctus_BE_SOAI", dp);
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
            }
        }
    }
}
