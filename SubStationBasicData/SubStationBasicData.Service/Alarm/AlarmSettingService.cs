using Balance.Infrastructure.Configuration;
using SqlServerDataAdapter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace SubStationBasicData.Service.Alarm
{
    public class AlarmSettingService
    {

        public static DataTable GetAlarmData(string organizationId)
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            string mySql = @"select A.KeyID,A.OrganizationID,B.LevelCode,B.VariableId,B.Name,B.AlarmType,B.EnergyAlarmValue,B.CoalDustConsumptionAlarm,B.PowerAlarmValue,B.[Target_Overall],B.[Target_Moment],B.[Target_Class],B.[Target_Day]
                                from tz_Formula A,formula_FormulaDetail B,system_Organization C
                                where A.KeyID=B.KeyID
                                and A.OrganizationID=C.OrganizationID
                                and A.OrganizationID=@organizationId
								and B.VariableId<>(case when C.Type ='熟料'  then 'clinkerElectricityGeneration' 
								else '' end)
								and B.VariableId<>(case when C.Type ='熟料'  then 'electricityOwnDemand' 
								else '' end)";
//                                and C.LevelCode like (SELECT LevelCode FROM system_Organization where OrganizationID=@organizationId)+'%'
//                                order by B.LevelCode";
            SqlParameter parameter = new SqlParameter("organizationId",organizationId);
            DataTable alarmInfoTable=dataFactory.Query(mySql,parameter);
            //报警类型名
            DataColumn alarmTypeColumn = new DataColumn("AlarmTypeName",typeof(string));
            alarmTypeColumn.DefaultValue = "无";
            alarmInfoTable.Columns.Add(alarmTypeColumn);
            foreach (DataRow dr in alarmInfoTable.Rows)
            {
                string alarmType = dr["AlarmType"].ToString();
                switch (alarmType)
                {
                    case "1":
                        dr["AlarmTypeName"] = "能耗报警";
                        break;
                    case "2":
                        dr["AlarmTypeName"] = "功率报警";
                        break;
                    case "3":
                        dr["AlarmTypeName"] = "能耗报警,功率报警";
                        break;
                    default:
                        dr["AlarmTypeName"] = "无";
                        break;
                }
                //只有熟料产线的产线级别才有煤耗报警
                if (dr["VariableId"].ToString().Trim() == "clinker")
                {
                    dr["AlarmTypeName"] = dr["AlarmTypeName"].ToString().Trim() + ",煤耗报警";
                }
            }
            
            return alarmInfoTable;
        }
        //public static DataTable JsonToDataTable(string json)
        //{
        //    //string[] rowsData = EasyUIJsonParser.Utility.JsonPickArray(json, "rows");
        //    return EasyUIJsonParser.TreeGridJsonParser.JsonToDataTable(json);
        //}
        public static int SaveAlarmValues(string organizationId, DataTable saveDataTable)
        {
            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            int m_UpdateRowCount = 0;
            if (saveDataTable != null)
            {
                saveDataTable.Columns.Remove("id");
                saveDataTable.Columns.Remove("OrganizationID");
                saveDataTable.Columns.Remove("Name");
                saveDataTable.Columns.Remove("AlarmType");
                saveDataTable.Columns.Remove("AlarmTypeName");

                m_UpdateRowCount = dataFactory.Update("formula_FormulaDetail", saveDataTable, new string[] { "KeyID", "VariableId", "LevelCode" });
                RestartDataCollection(organizationId);   //自动重启数采软件
            }
            return m_UpdateRowCount;
        }
        private static void RestartDataCollection(string organizationId)
        {

            string connectionString = ConnectionStringFactory.NXJCConnectionString;
            SystemParameters.UpdateNotification.UpdateParameters m_UpdateParameters = new SystemParameters.UpdateNotification.UpdateParameters(connectionString);
            ISqlServerDataFactory dataFactory = new SqlServerDataFactory(connectionString);
            string mySql = @"select A.Type
                                from tz_Formula A
                                where A.OrganizationID=@organizationId";
            SqlParameter parameter = new SqlParameter("organizationId", organizationId);
            DataTable alarmInfoTable = dataFactory.Query(mySql, parameter);
            if (alarmInfoTable != null)
            {
                string m_OrganizationType = alarmInfoTable.Rows[0]["Type"].ToString();
                if (m_OrganizationType == "1")   //公共公式
                {
                    m_UpdateParameters.Update(organizationId, SystemParameters.UpdateNotification.Name.公共公式, SystemParameters.UpdateNotification.TypeModify.立即);
                }
                else if (m_OrganizationType == "2")   //公式
                {
                    m_UpdateParameters.Update(organizationId, SystemParameters.UpdateNotification.Name.公式, SystemParameters.UpdateNotification.TypeModify.立即);
                }
            }
            
        }
    }
}
