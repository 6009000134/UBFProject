using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using UFIDA.U9.UI.PDHelper;
using UFSoft.UBF.Sys.Database;
using UFSoft.UBF.UI;
using UFSoft.UBF.UI.ControlModel;
using UFSoft.UBF.UI.IView;
using UFSoft.UBF.UI.MD.Runtime;
using UFSoft.UBF.UI.WebControlAdapter;
using UFSoft.UBF.Util.DataAccess;

namespace Auctus.CustomUI
{
    public class MOModifyUI : UFSoft.UBF.UI.Custom.ExtendedPartBase
    {
        private IPart uiPart;
        public override void AfterInit(IPart Part, EventArgs e)
        {
            base.AfterInit(Part, e);
            uiPart = Part;

            IUFButton btnCustSubmit = new UFWebButtonAdapter();
            btnCustSubmit.Text = "提交流程";
            btnCustSubmit.ToolTip = "提交流程";
            btnCustSubmit.ID = "btnCustSubmit";
            btnCustSubmit.AutoPostBack = true;
            btnCustSubmit.Visible = true;
            btnCustSubmit.Enabled = false;

            //OA流程按钮
            IUFButton btnOAFlow = new UFWebButtonAdapter();
            btnOAFlow.Text = "查看流程";
            btnOAFlow.ToolTip = "查看流程";
            btnOAFlow.ID = "btnOAFlow";
            btnOAFlow.AutoPostBack = true;
            btnOAFlow.Visible = true;
            btnOAFlow.Enabled = false;


            IUFCard card = (IUFCard)Part.GetUFControlByName(Part.TopLevelContainer, "Card4");

            card.Controls.Add(btnCustSubmit);
            CommonFunction.Layout(card, btnCustSubmit, 14, 0);
            btnOAFlow.Click += btnCustSubmit_Click;

            card.Controls.Add(btnOAFlow);
            CommonFunction.Layout(card, btnOAFlow, 12, 0);
            btnOAFlow.Click += btnOAFlow_Click;


        }
        /// <summary>
        /// 页面加载后事件
        /// </summary>
        /// <param name="Part"></param>
        /// <param name="args"></param>
        public override void AfterRender(IPart Part, EventArgs args)
        {
            base.AfterRender(Part, args);
            IUFControl btnCustSubmit = CommonFunction.FindControl(Part, "Card4", "btnCustSubmit");
            IUFControl btnOAFlow = CommonFunction.FindControl(Part, "Card4", "btnOAFlow");
            IUFControl btnQuickCreate = CommonFunction.FindControl(Part, "Card4", "btnQuickCreate");
            IUIRecord rec = uiPart.Model.Views["MOModify"].FocusedRecord;
            if (rec != null && Convert.ToInt64(rec["ID"].ToString()) > 0)
            {
                //btnQuickCreate
                //查询OA的流程ID
                string sql = string.Format(@"SELECT a.DescFlexField_PrivateDescSeg2 IsBackFromOA,a.DescFlexField_PrivateDescSeg3 OAFlowID 
,b.DescFlexField_PrivateDescSeg1 IsToOA
FROM dbo.MO_MOModify a INNER JOIN dbo.MO_MOModifyDocType b ON a.MOModifyDocType=b.ID
WHERE a.ID={0}", rec["ID"].ToString());
                DataSet ds = new DataSet();
                string OAFlowID = "";
                string IsBackFromOA = "";
                string IsToOA = "";
                DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), sql, null, out ds);
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    IsBackFromOA = row["IsBackFromOA"].ToString();
                    OAFlowID = row["OAFlowID"].ToString();
                    IsToOA = row["IsToOA"].ToString();
                }
                if (IsToOA == "1")//触发到OA
                {
                    if (string.IsNullOrEmpty(OAFlowID))//未提交
                    {
                        btnQuickCreate.Enabled = true;
                        btnOAFlow.Enabled = false;
                        btnCustSubmit.Enabled = false;
                    }
                    else
                    {
                        if (IsBackFromOA == "是")//从OA退回到，允许再次提交
                        {
                            btnQuickCreate.Enabled = true;
                            btnCustSubmit.Enabled = true;
                            btnOAFlow.Enabled = true;
                        }
                        else
                        {
                            btnCustSubmit.Enabled = false;
                            btnQuickCreate.Enabled = false;
                            btnOAFlow.Enabled = true;
                        }
                    }
                }
                else
                {
                    btnOAFlow.Enabled = false;
                    btnCustSubmit.Enabled = false;
                }

            }
        }
        /// <summary>
        /// 打开OA流程页面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnOAFlow_Click(object sender, EventArgs e)
        {
            IUIRecord rec = uiPart.Model.Views["MOModify"].FocusedRecord;
            string sql = string.Format(@"SELECT a.DescFlexField_PrivateDescSeg2 IsBackFromOA,a.DescFlexField_PrivateDescSeg3 OAFlowID 
FROM dbo.MO_MOModify a WHERE a.ID={0}", rec["ID"].ToString());
            DataSet ds = new DataSet();
            string OAFlowID = "";
            DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), sql, null, out ds);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                OAFlowID = row["OAFlowID"].ToString();
            }
            string script = PubFunction.GetOAFlowScript(PDContext.Current.UserCode, OAFlowID);//打开流程页面脚本
            AtlasHelper.RegisterAtlasStartupScript
             ((Control)this.uiPart.TopLevelContainer, this.uiPart.GetType(), "ReferenceReturn", script, false);
        }

        /// <summary>
        /// 被退回的流程重新提交
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnCustSubmit_Click(object sender, EventArgs e)
        {
            //TODO:OA退回字段改成空,重新提交到OA
            //
        }
    }
}
