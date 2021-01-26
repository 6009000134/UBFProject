﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UFSoft.UBF.Util.DataAccess;

namespace Auctus.CustomBE.MOIssue
{
    public class MOIssueAI : UFSoft.UBF.Eventing.IEventSubscriber
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
            UFIDA.U9.MO.Issue.IssueDoc issueDoc = (UFIDA.U9.MO.Issue.IssueDoc)key.GetEntity();
            if (issueDoc == null)
            {
                return;
            }
            else
            {
                if (issueDoc.Org.Code == "300")
                {
                    try
                    {
                        DataParamList dp = new DataParamList();
                        dp.Add(DataParamFactory.Create("DocNo", issueDoc.DocNo, ParameterDirection.Input, DbType.String, 50));
                        dp.Add(DataParamFactory.Create("BeforeConfirmDate", "", ParameterDirection.Input, DbType.String, 50));
                        dp.Add(DataParamFactory.Create("ConfirmDate", "", ParameterDirection.Input, DbType.String, 50));
                        dp.Add(DataParamFactory.CreateOutput("Result", DbType.String));
                        DataAccessor.RunSP("sp_Auctus_BE_MoIssueDoc", dp);
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
                }
            }
        }
    }
}
