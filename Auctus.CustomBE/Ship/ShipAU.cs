using Auctus.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UFIDA.U9.Base;
using UFIDA.U9.Base.UserRole;
using UFIDA.U9.CBO.PubBE.YYC;
using UFIDA.U9.CBO.SCM.Enums;
using UFIDA.U9.SM.Ship;
using UFSoft.UBF.Sys.Database;
using UFSoft.UBF.Util.Cache;
using UFSoft.UBF.Util.DataAccess;
using UFSoft.UBF.Util.Log;

namespace Auctus.CustomBE
{
    /// <summary>
    /// 出货单AU插件
    /// </summary>
    public class ShipAU : UFSoft.UBF.Eventing.IEventSubscriber
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
            UFIDA.U9.SM.Ship.Ship entity = (UFIDA.U9.SM.Ship.Ship)key.GetEntity();
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
                        if (entity.OriginalData.Status == ShipStateEnum.Creates && entity.Status == ShipStateEnum.Approving && entity.DocType.DescFlexField.PrivateDescSeg1 == "1")//提交操作
                        {
                            ILogger log = LoggerManager.GetLogger(typeof(CacheManager));
                            //传送给OA的Json对象
                            Dictionary<string, object> dicResult = new Dictionary<string, object>();
                            //U9用户信息
                            User user = User.Finder.FindByID(Context.LoginUserID);
                            //表单信息
                            Dictionary<string, object> dicMain = new Dictionary<string, object>();
                            List<Dictionary<string, object>> liDt = new List<Dictionary<string, object>>();
                            Dictionary<string, object> dicValue = new Dictionary<string, object>();
                            #region OA信息                           
                            BaseInfo baseInfo;
                            if (entity.DescFlexField.PrivateDescSeg7 == "是")
                            {
                                baseInfo = Auctus.Common.Utils.GenerateOABaseInfo(user.Code, PubFunction.GetOAInfoByCode("12"), entity.ID.ToString(), 1, entity.DescFlexField.PrivateDescSeg8, 1, "出货单：" + entity.DocNo);
                            }
                            else
                            {
                                baseInfo = Auctus.Common.Utils.GenerateOABaseInfo(user.Code, PubFunction.GetOAInfoByCode("12"), entity.ID.ToString(), 1, "", 1, "出货单：" + entity.DocNo);
                                //设置OA上下文
                                if (!string.IsNullOrEmpty(entity.DescFlexField.PrivateDescSeg7))
                                {
                                    dicValue = new Dictionary<string, object>();
                                    dicValue.Add("value", entity.DescFlexField.PrivateDescSeg7);
                                    dicMain.Add("gllc", dicValue);
                                }
                            }

                            dicMain = Auctus.Common.Utils.GenerateOAUserInfo(dicMain, user.Code, DateTime.Now.ToString("yyyy-MM-dd"));

                            #endregion
                            #region 表单信息                            
                            dicValue.Add("value", entity.DocNo);
                            dicMain.Add("dh", dicValue);
                            dicValue = new Dictionary<string, object>();
                            dicValue.Add("value", entity.Org.ID);
                            dicMain.Add("u9zz", dicValue);
                            dicValue = new Dictionary<string, object>();
                            dicValue.Add("value", entity.DocType.Code);
                            dicMain.Add("djlx", dicValue);
                            dicValue = new Dictionary<string, object>();
                            dicValue.Add("value", entity.OrderBy.Name);
                            dicMain.Add("kh", dicValue);
                            dicValue = new Dictionary<string, object>();
                            dicValue.Add("value", entity.TC.Name);
                            dicMain.Add("bz", dicValue);
                            dicValue = new Dictionary<string, object>();
                            dicValue.Add("value", entity.DescFlexField.PubDescSeg3);
                            dicMain.Add("khddh", dicValue);
                            dicValue = new Dictionary<string, object>();
                            dicValue.Add("value", entity.IsPriceIncludeTax ? "1" : "0");
                            dicMain.Add("jghs", dicValue);
                            dicValue = new Dictionary<string, object>();
                            dicValue.Add("value", entity.DescFlexField.PrivateDescSeg1);
                            dicMain.Add("cph", dicValue);
                            dicValue = new Dictionary<string, object>();
                            dicValue.Add("value", entity.ShipConfirmDate.ToString("yyyy-MM-dd"));
                            dicMain.Add("chqrr", dicValue);
                            dicValue = new Dictionary<string, object>();
                            dicValue.Add("value", entity.DescFlexField.PrivateDescSeg2);
                            dicMain.Add("sj", dicValue);
                            dicValue = new Dictionary<string, object>();
                            dicValue.Add("value", entity.DescFlexField.PrivateDescSeg4);
                            dicMain.Add("xh", dicValue);
                            dicValue = new Dictionary<string, object>();
                            dicValue.Add("value", entity.DescFlexField.PrivateDescSeg3);
                            dicMain.Add("sjdh", dicValue);
                            dicValue = new Dictionary<string, object>();
                            dicValue.Add("value", entity.DescFlexField.PrivateDescSeg5);
                            dicMain.Add("kddh", dicValue);
                            dicValue = new Dictionary<string, object>();
                            dicValue.Add("value", entity.ID);
                            dicMain.Add("fpkid", dicValue);

                            foreach (ShipLine item in entity.ShipLines)
                            {
                                Dictionary<string, object> dicDt = new Dictionary<string, object>();
                                
                                dicValue = new Dictionary<string, object>();
                                dicValue.Add("value", item.ID);
                                dicDt.Add("xid", dicValue);
                                dicValue = new Dictionary<string, object>();
                                dicValue.Add("value", item.DonationType.Value + 1);
                                dicDt.Add("mflx", dicValue);
                                dicValue = new Dictionary<string, object>();
                                dicValue.Add("value", item.DocLineNo);
                                dicDt.Add("xh", dicValue);
                                dicValue = new Dictionary<string, object>();
                                dicValue.Add("value", item.ItemInfo.ItemID.ID);
                                dicDt.Add("lpid", dicValue);
                                dicValue = new Dictionary<string, object>();
                                dicValue.Add("value", item.ItemInfo.ItemID.Code);
                                dicDt.Add("lh", dicValue);
                                dicValue = new Dictionary<string, object>();
                                dicValue.Add("value", item.ItemInfo.ItemID.Name);
                                dicDt.Add("pm", dicValue);
                                dicValue = new Dictionary<string, object>();
                                dicValue.Add("value", item.ItemInfo.ItemID.SPECS);
                                dicDt.Add("gg", dicValue);
                                dicValue = new Dictionary<string, object>();
                                dicValue.Add("value", Convert.ToInt32(item.QtyPriceAmount));
                                dicDt.Add("jhcsl", dicValue);
                                dicValue = new Dictionary<string, object>();
                                dicValue.Add("value", Convert.ToInt32(item.QtyPriceAmount));
                                dicDt.Add("sl", dicValue);
                                if (item.LotInfo != null)
                                {
                                    if (item.LotInfo.LotMaster != null)
                                    {
                                        dicValue = new Dictionary<string, object>();
                                        dicValue.Add("value", item.LotInfo.LotMaster.ID);
                                        dicDt.Add("phid", dicValue);
                                    }
                                    dicValue = new Dictionary<string, object>();
                                    dicValue.Add("value", item.LotInfo.LotCode);
                                    dicDt.Add("ph", dicValue);
                                }
                                dicValue = new Dictionary<string, object>();
                                dicValue.Add("value", item.DescFlexField.PubDescSeg3);
                                dicDt.Add("khddh", dicValue);
                                dicValue = new Dictionary<string, object>();
                                dicValue.Add("value", item.CustomerItemCode);
                                dicDt.Add("khlh", dicValue);
                                dicValue = new Dictionary<string, object>();
                                dicValue.Add("value", item.DescFlexField.PubDescSeg2);
                                dicDt.Add("khwlbm", dicValue);
                                dicValue = new Dictionary<string, object>();
                                dicValue.Add("value", item.FinallyPriceTC);
                                dicDt.Add("zzj", dicValue);
                                dicValue = new Dictionary<string, object>();
                                dicValue.Add("value", item.TotalMoneyTC);
                                dicDt.Add("je", dicValue);
                                dicValue = new Dictionary<string, object>();
                                dicValue.Add("value", item.TaxRate);
                                dicDt.Add("sli", dicValue);
                                dicValue = new Dictionary<string, object>();
                                dicValue.Add("value", item.SrcDocNo);
                                dicDt.Add("xsddh", dicValue);
                                if (item.WH != null)
                                {
                                    dicValue = new Dictionary<string, object>();
                                    dicValue.Add("value", item.WH.ID);
                                    dicDt.Add("whid", dicValue);
                                    dicValue = new Dictionary<string, object>();
                                    bool IsExistsLotID = false;
                                    if (item.LotInfo != null)
                                    {
                                        if (item.LotInfo.LotMaster != null)
                                        {
                                            IsExistsLotID = true;
                                        }
                                    }
                                    //构建存储地点、料品ID、批号组合的唯一索引
                                    string uid = item.WH.ID.ToString() + item.ItemInfo.ItemID.ID.ToString();
                                    if (IsExistsLotID)
                                    {
                                        uid += item.LotInfo.LotMaster.ID.ToString();
                                    }
                                    else
                                    {
                                        uid += "0";
                                    }

                                    dicValue.Add("value", uid);
                                    dicDt.Add("ccdd", dicValue);
                                }
                                liDt.Add(dicDt);
                            }
                            #endregion

                            dicResult.Add("base", baseInfo);
                            dicResult.Add("main", dicMain);
                            dicResult.Add("dt1", liDt);
                            List<Dictionary<string, object>> li = new List<Dictionary<string, object>>();
                            li.Add(dicResult);
                            string json = "";
                            json = JsonHelper.GetJsonJS(li);
                            log.Error("出货单JSON数据为：" + json);
                            PubFunction pubFun = new PubFunction();
                            //调用OA接口
                            string OAFlowID = pubFun.OAService(json);
                            //更新返回的流程ID
                            string UpSQL = "";
                            if (entity.DescFlexField.PrivateDescSeg7 == "是")
                            {
                                UpSQL = string.Format(@"UPDATE dbo.SM_Ship SET DescFlexField_PrivateDescSeg8='{0}',DescFlexField_PrivateDescSeg7='' WHERE ID = {1}", OAFlowID, entity.ID.ToString());
                            }
                            else
                            {
                                UpSQL = string.Format(@"UPDATE dbo.SM_Ship SET DescFlexField_PrivateDescSeg8='{0}' WHERE ID = {1}", OAFlowID, entity.ID.ToString());
                            }
                            DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), UpSQL, null);


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
