using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using UFSoft.UBF.UI.Controls;
using UFSoft.UBF.UI.WebControls;
using UFSoft.UBF.UI.ControlModel;
using UFSoft.UBF.UI.IView;
using System.Data;
using UFSoft.UBF.Sys.Database;
using UFSoft.UBF.Util.DataAccess;

namespace Auctus.MiscShip.UIPlugin
{
    public class CommonFunction
    {
        public static void Layout(IContainer container, IUFControl ctrl, uint x, uint y)
        {
            Layout(container, ctrl, x, y, 1, 1, Unit.Pixel(0), Unit.Pixel(0), true);
        }

        public static void Layout(IContainer container, IUFControl ctrl, uint x, uint y, int width, int height)
        {
            Layout(container, ctrl, x, y, 1, 1, Unit.Pixel(width), Unit.Pixel(height), false);
        }


        public static void Layout(IContainer container, IUFControl ctrl, uint x, uint y, int xspan, int yspan,
            Unit width, Unit height, bool isAutoSize)
        {
            IGridLayout gl = container.Layout as IGridLayout;
            if (gl == null) return;
            GridLayoutInfo glInfo = new GridLayoutInfo((uint)x, (uint)y, (uint)xspan, (uint)yspan, width, height);
            glInfo.AutoSize = isAutoSize;
            gl.Controls.Add((Control)ctrl, glInfo);

        }

        public static IUFControl FindControl(IPart part, string parentControl, string control)
        {
            IUFCard card = (IUFCard)part.GetUFControlByName(part.TopLevelContainer, parentControl);
            if (card == null)
                return null;

            foreach (IUFControl ctrl in card.Controls)
            {
                if (ctrl.ID.Equals(control, StringComparison.OrdinalIgnoreCase))
                {
                    return ctrl;
                }
            }
            return null;
        }
        public static IUFControl FindControl2(IPart part, string parentControl, string control)
        {
            IUFToolbar card = (IUFToolbar)part.GetUFControlByName(part.TopLevelContainer, parentControl);
            if (card == null)
                return null;

            foreach (IUFControl ctrl in card.Controls)
            {
                if (ctrl != null)
                {
                    if (!string.IsNullOrEmpty(ctrl.ID))
                    {
                        if (ctrl.ID.Equals(control, StringComparison.OrdinalIgnoreCase))
                        {
                            return ctrl;
                        }
                    }
                }

            }
            return null;
        }

        /// <summary>
        /// 获取配置的OA地址信息
        /// </summary>
        /// <returns></returns>
        public static string GetOAIP()
        {
            string sql = @"SELECT a2.Description AS OAIP FROM dbo.Base_ValueSetDef AS a
                INNER JOIN dbo.Base_DefineValue AS a1 ON a1.ValueSetDef = a.ID
						                AND a1.Code = '01'
                INNER JOIN dbo.Base_DefineValue_Trl AS a2 ON a2.ID=a1.id
						                AND a2.SysMLFlag='zh-cn'
                WHERE a.Code = 'OAAddr'
                AND a1.Effective_IsEffective = 1
                AND a1.Effective_EffectiveDate <= GETDATE()
                AND a1.Effective_DisableDate >= GETDATE()";
            string OAIP = "";
            DataSet ds = new DataSet();
            DataAccessor.RunSQL(DatabaseManager.GetCurrentConnection(), sql, null, out ds);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                OAIP = row["OAIP"].ToString();
            }
            return OAIP;
        }

        /// <summary>  
        　/// Base64加密  
        　/// </summary>  
        　/// <param name="Message"></param>  
        　/// <returns></returns>  
        public static string Base64Encode(System.Text.Encoding en, string Message)
        {
            byte[] bytes = en.GetBytes(Message);
            return Convert.ToBase64String(bytes);
        }
    }
}
