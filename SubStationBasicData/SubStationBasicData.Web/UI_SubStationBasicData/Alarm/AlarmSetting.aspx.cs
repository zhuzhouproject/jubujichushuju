using SubStationBasicData.Service.Alarm;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SubStationBasicData.Web.UI_SubStationBasicData.Alarm
{
    public partial class AlarmSetting : WebStyleBaseForEnergy.webStyleBase
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            base.InitComponts();
            if (!IsPostBack)
            {
#if DEBUG
                ////////////////////调试用,自定义的数据授权
                List<string> m_DataValidIdItems = new List<string>() { "zc_zcshn_zhuzhouc_zhuzhouf" };
                AddDataValidIdGroup("ProductionOrganization", m_DataValidIdItems);
#elif RELEASE
#endif
                //向web用户控件传递数据授权参数
                this.OrganisationTree_ProductionLine.Organizations = GetDataValidIdGroup("ProductionOrganization");
                //向web用户控件传递当前调用的页面名称
                this.OrganisationTree_ProductionLine.PageName = "SubStationBasicData.aspx";
                //this.OrganisationTree_ProductionLine.LeveDepth = 5;
            }
        }
        /// <summary>
        /// 页面操作权限
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public static char[] AuthorityControl()
        {
            return mPageOpPermission.ToArray();
        }
        [WebMethod]
        public static string GetData(string organizationId)
        {
            DataTable table= AlarmSettingService.GetAlarmData(organizationId);
            string json = EasyUIJsonParser.TreeGridJsonParser.DataTableToJsonByLevelCode(table, "LevelCode");
            return json;
        }

        [WebMethod]
        public static int SaveAlarmValues(string organizationId, string datagridData)
        {
            if (mPageOpPermission.ToArray()[2] == '1')
            {
                DataTable table = EasyUIJsonParser.TreeGridJsonParser.JsonToDataTable(datagridData);
                int m_Result = AlarmSettingService.SaveAlarmValues(organizationId, table);
                m_Result = m_Result > 0 ? 1 : m_Result;
                return m_Result;
            }
            else
            {
                return -1;
            }
        }
    }
}