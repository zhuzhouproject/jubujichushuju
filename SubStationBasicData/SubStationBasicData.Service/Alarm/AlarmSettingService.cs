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
            string mySql = @"select A.KeyID,A.OrganizationID,B.LevelCode,B.VariableId,B.Name,B.AlarmType,B.EnergyAlarmValue,B.CoalDustConsumptionAlarm,B.PowerAlarmValue 
                                from tz_Formula A,formula_FormulaDetail B,system_Organization C
                                where A.KeyID=B.KeyID
                                and A.OrganizationID=C.OrganizationID
                                and A.OrganizationID=@organizationId";
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
    }
}
