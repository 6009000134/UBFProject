using System;
using UFSoft.UBF.UI.ControlModel;
using UFSoft.UBF.UI.IView;
using System.Web.UI;
using UFIDA.U9.SCM.PM.PRToPOUIModel;
using UFSoft.UBF.UI.WebControls;
using System.Collections;
using System.Data;
using UFSoft.UBF.Util.DataAccess;
using UFSoft.UBF.UI.MD.Runtime;
using System.Collections.Generic;

namespace Auctus.PR.UIPlugin
{
    /// <summary>
    /// 请购转采购试算结果不允许修改
    /// </summary>
    public class ToPOTryCalUIPlugin : UFSoft.UBF.UI.Custom.ExtendedPartBase
    {
        public override void AfterRender(IPart Part, EventArgs args)
        {
            base.AfterRender(Part, args);
            ToPoTryCalcUIFormWebPart p = (ToPoTryCalcUIFormWebPart)Part;
            //获取页面上的DataGrid
            UFGrid g = (UFGrid)Part.GetUFControlByName(Part.TopLevelContainer, "DataGrid01");
            //设置DataGrid01不可新增、插入、删除
            g.AllowAddRow = false;
            g.AllowDeleteRow = false;
            g.AllowInsertRow = false;

            //试算页面选中需要进行试算的请购行
            IUIRecordCollection arr = p.Model.PR_PRLineList.SelectRecords;

            //循环试算结果集合，试算结果集合的PrimaryKey是料号的ID
            for (int j = 0; j < p.Model.TryCalcView.Records.Count; j++)
            {
                for (int i = 0; i < arr.Count; i++)
                {
                    PR_PRLineListRecord r = (PR_PRLineListRecord)arr[i];
                    if (r.ItemInfo_ItemID_PurchaseInfo_PurchaseQuotaMode == 4)//料号的分配方式是4：按比例分配，则设置对应的试算结果行不可编辑供应商
                    {
                        if (r.ItemInfo_ItemID.ToString() == p.Model.TryCalcView.Records[j]["ItemID"].ToString())//请购行的料号ID与试算结果的主键（料号ID）比较
                        {
                            g.SetCellDisabled(p.Model.TryCalcView.Records[j].Index, "Supplier");
                        }
                    }
                }
                //设置试算结果的数量不可编辑
                g.SetCellDisabled(p.Model.TryCalcView.Records[j].Index, "ToPOQtyTU");
                g.SetCellDisabled(p.Model.TryCalcView.Records[j].Index, "ToPOQtyTBU");
            }

            //UFSoft.UBF.UI.AtlasHelper.RegisterClientScript(p.Page, p.Page.GetType(), "disabledategrid", "$().ready(function(){ $('#u_S_S1_DataGrid01_MainBody>table>tbody>tr>td').click(function(){$('#u_S_S1_DataGrid01_MainBody>table>tbody>tr>td input').attr('disabled', 'disabled');});});", true);            
        }
    }


}

