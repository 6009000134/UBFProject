using System;
using UFSoft.UBF.UI.IView;
using UFSoft.UBF.UI.ControlModel;
using UFSoft.UBF.UI.WebControlAdapter;
using System.Data;
using UFSoft.UBF.Util.DataAccess;
using UFIDA.U9.Base;

namespace Auctus.CustomUI
{
    public class RescheduleUIPlugin : UFSoft.UBF.UI.Custom.ExtendedPartBase
    {
        private IPart iPart;
        #region 添加按钮控件
        /// <summary>
        /// UI界面布局，添加按钮控件
        /// </summary>
        /// <param name="Part"></param>
        /// <param name="args"></param>
        public override void AfterInit(IPart Part, EventArgs args)
        {
            base.AfterInit(Part, args);
            iPart = Part;
            // 采购(PO)变更单按钮
            IUFButton btnPOModify = new UFWebButtonAdapter();
            btnPOModify.Text = "采购交期重排";
            btnPOModify.ID = "btnPOModify";
            btnPOModify.AutoPostBack = true;
            btnPOModify.ToolTip = "采购交期重排";
            btnPOModify.Visible = true;
            btnPOModify.Width = System.Web.UI.WebControls.Unit.Pixel(100);
            // 委外采购(WPO)变更单按钮
            IUFButton btnWPOModify = new UFWebButtonAdapter();
            btnWPOModify.Text = "委外交期重排";
            btnWPOModify.ID = "btnWPOModify";
            btnWPOModify.AutoPostBack = true;
            btnWPOModify.ToolTip = "委外交期重排";
            btnWPOModify.Visible = true;
            btnWPOModify.Width = System.Web.UI.WebControls.Unit.Pixel(100);            

            // 将按钮加入到按钮栏
            IUFCard iCard = (IUFCard)Part.GetUFControlByName(Part.TopLevelContainer, "Card0");
            iCard.Controls.Add(btnPOModify);
            CommonFunction.Layout(iCard, btnPOModify, 6, 0);
            btnPOModify.Click += new EventHandler(BtnPOModify_Click);

            // 将按钮加入到按钮栏            
            iCard.Controls.Add(btnWPOModify);
            CommonFunction.Layout(iCard, btnWPOModify, 8, 0);
            btnWPOModify.Click += new EventHandler(BtnWPOModify_Click);

        }
        #endregion

        #region 批量创建变更单
        /// <summary>
        /// 创建MRP变交期的采购变更单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPOModify_Click(object sender, EventArgs e)
        {
            string userName = Context.LoginUser;
            DataParamList dp = new DataParamList();
            dp.Add(DataParamFactory.CreateInput("UserName", userName, DbType.String));
            dp.Add(DataParamFactory.CreateOutput("Result", DbType.String));
            DataAccessor.RunSP("sp_Auctus_UI_CreatePOModify", dp);
            string result = dp["Result"].Value.ToString();
            if (result != "1")
            {
                throw new Exception(result);
            }
        }

        /// <summary>
        /// 创建MPS变交期的委外采购变更单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnWPOModify_Click(object sender, EventArgs e)
        {
            string userName = Context.LoginUser;
            DataParamList dp = new DataParamList();
            dp.Add(DataParamFactory.CreateInput("UserName", userName, DbType.String));
            dp.Add(DataParamFactory.CreateOutput("Result", DbType.String));
            DataAccessor.RunSP("sp_Auctus_UI_CreateWPOModify", dp);
            string result = dp["Result"].Value.ToString();
            if (result != "1")
            {
                throw new Exception(result);
            }
        }
        #endregion




    }
}
