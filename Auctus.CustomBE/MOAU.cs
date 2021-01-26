using Auctus.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UFIDA.U9.Base;
using UFIDA.U9.Base.UserRole;
using UFIDA.U9.CBO.PubBE.YYC;
using UFIDA.U9.MO.Enums;
using UFSoft.UBF.Sys.Database;
using UFSoft.UBF.Util.Cache;
using UFSoft.UBF.Util.DataAccess;
using UFSoft.UBF.Util.Log;

namespace Auctus.CustomBE
{
    /// <summary>
    /// 工单更新BE
    /// </summary>
    public class MOAU : UFSoft.UBF.Eventing.IEventSubscriber
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
                        ILogger log = LoggerManager.GetLogger(typeof(CacheManager));
                        //提交操作
                        if (entity.OriginalData.DocState == MOStateEnum.Opened && entity.DocState == MOStateEnum.Approving && entity.DocType.DescFlexField.PrivateDescSeg1 == "1")
                        {
                            Dictionary<string, object> dicResult = new Dictionary<string, object>();
                            //获取工单相关信息
                            DataSet ds = new DataSet();
                            string sql = string.Format("select * from v_Cust_MOInfo where DocNo='{0}' and rn=1", entity.DocNo);
                            DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), sql, null, out ds);
                            //表单信息   
                            Dictionary<string, object> dicMain = new Dictionary<string, object>();
                            Dictionary<string, object> dicValue = new Dictionary<string, object>();
                            User user = User.Finder.FindByID(Context.LoginUserID);//U9用户信息
                            #region OA信息                           
                            BaseInfo baseInfo;
                            if (entity.DescFlexField.PrivateDescSeg8 == "是")
                            {
                                baseInfo = Auctus.Common.Utils.GenerateOABaseInfo(user.Code, PubFunction.GetOAInfoByCode("06"), entity.ID.ToString(), 1, entity.DescFlexField.PrivateDescSeg7, 1, "生产订单审批-订单号：" + entity.DocNo);
                            }
                            else
                            {
                                baseInfo = Auctus.Common.Utils.GenerateOABaseInfo(user.Code, PubFunction.GetOAInfoByCode("06"), entity.ID.ToString(), 1, "", 1, "生产订单审批-订单号：" + entity.DocNo);
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
                            #region 工单信息
                            //dicValue = new Dictionary<string, object>();
                            //dicValue.Add("value", entity.CreatedBy);
                            //dicMain.Add("CreateBy", dicValue);
                            dicValue = new Dictionary<string, object>();
                            dicValue.Add("value", entity.DocType.Code);
                            dicMain.Add("DocType", dicValue);
                            dicValue = new Dictionary<string, object>();
                            dicValue.Add("value", entity.DocNo);
                            dicMain.Add("DocNo", dicValue);
                            dicValue = new Dictionary<string, object>();
                            dicValue.Add("value", entity.Department.Name);
                            dicMain.Add("Dept", dicValue);
                            dicValue = new Dictionary<string, object>();
                            dicValue.Add("value", entity.ItemMaster.Code);
                            dicMain.Add("Code", dicValue);
                            dicValue = new Dictionary<string, object>();
                            dicValue.Add("value", entity.ItemMaster.Name);
                            dicMain.Add("Name", dicValue);
                            dicValue = new Dictionary<string, object>();
                            dicValue.Add("value", entity.ItemMaster.SPECS);
                            dicMain.Add("Specs", dicValue);
                            dicValue = new Dictionary<string, object>();
                            dicValue.Add("value", Convert.ToInt32(entity.ProductQty));
                            dicMain.Add("ProductQty", dicValue);
                            dicValue = new Dictionary<string, object>();
                            dicValue.Add("value", entity.StartDate.ToString("yyyy-MM-dd"));
                            dicMain.Add("StartDate", dicValue);
                            dicValue = new Dictionary<string, object>();
                            dicValue.Add("value", entity.CompleteDate.ToString("yyyy-MM-dd"));
                            dicMain.Add("CompleteDate", dicValue);
                            dicValue = new Dictionary<string, object>();
                            dicValue.Add("value", entity.SCVWh.Name);
                            dicMain.Add("Wh", dicValue);
                            dicValue = new Dictionary<string, object>();
                            dicValue.Add("value", entity.ID.ToString());
                            dicMain.Add("u9id", dicValue);
                            dicValue = new Dictionary<string, object>();
                            dicValue.Add("value", entity.Org.ID.ToString());
                            dicMain.Add("u9zz", dicValue);

                            if (ds.Tables.Count == 0)
                            {
                                throw new Exception("工单" + entity.DocNo + "信息在视图中找不到，请检查v_Cust_MOInfo视图！");
                            }
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                dicValue = new Dictionary<string, object>();
                                dicValue.Add("value", dr["OriginalLineName"]);
                                dicMain.Add("Line", dicValue);
                                dicValue = new Dictionary<string, object>();
                                if (dr["SODocNo"] == DBNull.Value)
                                {
                                    dicMain.Add("DemandCode", null);
                                }
                                else
                                {
                                    dicValue.Add("value", dr["SODocNo"]);
                                    dicMain.Add("DemandCode", dicValue);
                                }

                                if (dr["SODeliveryDate"] == DBNull.Value)
                                {
                                    dicMain.Add("SODeliverDate", null);
                                }
                                else
                                {
                                    dicValue = new Dictionary<string, object>();
                                    if (string.IsNullOrEmpty(dr["SODeliveryDate"].ToString()))
                                    {
                                        dicMain.Add("SODeliverDate", null);
                                    }
                                    else
                                    {
                                        DateTime d = new DateTime();
                                        if (DateTime.TryParse(dr["SODeliveryDate"].ToString(), out d))
                                        {
                                            dicValue.Add("value", d.ToString("yyyy-MM-dd"));
                                            dicMain.Add("SODeliverDate", dicValue);
                                        }
                                        else
                                        {
                                            throw new Exception("销售交期：" + dr["SODeliveryDate"].ToString() + "不是正确的日期格式！");
                                        }
                                    }

                                }
                                dicValue = new Dictionary<string, object>();
                                if (dr["SODocNo"] == DBNull.Value)
                                {
                                    dicValue.Add("value", dr["SO"]);
                                    dicMain.Add("SODocNo", dicValue);
                                }
                                else
                                {
                                    dicValue.Add("value", dr["SODocNo"]);
                                    dicMain.Add("SODocNo", dicValue);
                                }
                                dicValue = new Dictionary<string, object>();
                                dicValue.Add("value", dr["MRPName"].ToString());
                                dicMain.Add("MRP", dicValue);
                            }
                            #endregion
                            dicResult.Add("base", baseInfo);
                            dicResult.Add("main", dicMain);
                            List<Dictionary<string, object>> li = new List<Dictionary<string, object>>();
                            li.Add(dicResult);
                            string json = "";
                            json = JsonHelper.GetJsonJS(li);
                            log.Error("工单JSON数据为：" + json);
                            PubFunction pubFun = new PubFunction();
                            //调用OA接口
                            string OAFlowID = pubFun.OAService(json);
                            //更新返回的流程ID
                            string UpSQL = "";
                            if (entity.DescFlexField.PrivateDescSeg8 == "是")
                            {
                                UpSQL = string.Format(@"UPDATE dbo.MO_MO SET DescFlexField_PrivateDescSeg7='{0}',DescFlexField_PrivateDescSeg8='' WHERE ID = {1}", OAFlowID, entity.ID.ToString());
                            }
                            else
                            {
                                UpSQL = string.Format(@"UPDATE dbo.MO_MO SET DescFlexField_PrivateDescSeg7='{0}' WHERE ID = {1}", OAFlowID, entity.ID.ToString());
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
