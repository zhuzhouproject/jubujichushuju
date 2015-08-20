var myDataGridObject= new Object();
$(function () {
    loadDatagrid("first");
    myDataGridObject = $('#gridMain_ReportTemplate');
});

//初始化页面的增删改查权限
function initPageAuthority() {
    $.ajax({
        type: "POST",
        url: "AlarmSetting.aspx/AuthorityControl",
        data: "",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        async: false,//同步执行
        success: function (msg) {
            var authArray = msg.d;
            //增加
            //if (authArray[1] == '0') {
            //    $("#add").linkbutton('disable');
            //}
            //修改
            if (authArray[2] == '0') {
                $("#edit").linkbutton('disable');
            }
            //删除
            //if (authArray[3] == '0') {
            //    $("#delete").linkbutton('disable');
            //}
        }
    });
}
function loadDatagrid(loadType) {
    if ("first" == loadType) {
        $('#gridMain_ReportTemplate').treegrid({
            columns: [[
                { field: 'Name', title: '名称', width: 200 },
                { field: 'AlarmTypeName', title: '报警类型', width: 150 },
                {
                    field: 'EnergyAlarmValue', title: '能耗报警值', width: 100, editor: 'text',
                    styler: function (value, row, index) { if (row.AlarmType == 1 || row.AlarmType == 3) { return 'color:red;'; } }
                },
                {
                    field: 'PowerAlarmValue', title: '功率报警值', width: 100, editor: 'text',
                    styler: function (value, row, index) { if (row.AlarmType == 2 || row.AlarmType == 3) { return 'color:red;'; } }
                },
                {
                    field: 'CoalDustConsumptionAlarm', title: '煤耗报警值', width: 100, editor: 'text',
                    styler: function (value, row, index) { if (row.VariableId == "clinker") { return 'color:red;'; } }
                }
            ]],
            toolbar: "#toolbar_ReportTemplate",
            rownumbers: true,
            singleSelect: true,
            striped: true,
            onClickRow: onClickRow,
            idField: 'id',
            treeField: 'Name',
            data: []
        })
    }
    else {
        var organizationId = $('#organizationId').val();
        $.ajax({
            type: "POST",
            url: "AlarmSetting.aspx/GetData",
            data: '{organizationId: "' + organizationId+ '"}',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                m_MsgData = jQuery.parseJSON(msg.d);
                myDataGridObject.treegrid("loadData", m_MsgData);
            },
            error: handleError
        }); 
    }
}
function cellStyler(value, row, index) {
    if (row.AlarmType==0) {
        return 'background-color:#ffee00;color:red;';
    }
}
function handleError() {
    $('#gridMain_ReportTemplate').treegrid('loadData', []);
    $.messager.alert('失败', '获取数据失败');
}
//查询
function RefreshFun() {
    loadDatagrid("last");
}
//编辑行的ID
var editingId = undefined;
function endEditing() {
    if (editingId == undefined) {
        return true;
    }
    if (myDataGridObject.treegrid('select', editingId)) {     
        $('#gridMain_ReportTemplate').treegrid('endEdit', editingId);
        editIndex = undefined;
        return true;
    } else {
        return false;
    }
}
//行单击事件
function onClickRow(row) {
    if (editingId != row.id) {
        if (endEditing()) {
            editingId = row.id;
            $('#gridMain_ReportTemplate').treegrid('select', editingId)
                    .treegrid('beginEdit', editingId);           
        } else {
            $('#gridMain_ReportTemplate').treegrid('select', editingId);
        }
    }
}

//撤销修改
function reject() {
    if (editingId != undefined) {
        $('#gridMain_ReportTemplate').treegrid('cancelEdit', editingId);
        editingId = undefined;
    }
}
//保存修改
function saveFun() {
    endEditing();           //关闭正在编辑
    var organizationId = $('#organizationId').val();
    //var m_DataGridData = $('#gridMain_ReportTemplate').treegrid('getData');
    var m_DataGridData = $('#gridMain_ReportTemplate').datagrid('getChanges', 'updated');
    for (var i = 0; i < m_DataGridData.length; i++) {
        m_DataGridData[i]['children'] = [];
    }
    if (m_DataGridData.length > 0) {
        var m_DataGridDataJson = JSON.stringify(m_DataGridData);
        $.ajax({
            type: "POST",
            url: "AlarmSetting.aspx/SaveAlarmValues",
            data: "{organizationId:'" + organizationId + "',datagridData:'" + m_DataGridDataJson + "'}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                var m_Msg = msg.d;
                if (m_Msg == '1') {
                    $.messager.alert('提示', '修改成功！');
                }
                else if (m_Msg == '0') {
                    $.messager.alert('提示', '修改失败！');
                }
                else if (m_Msg == '-1') {
                    $.messager.alert('提示', '用户没有保存权限！');
                }
                else {
                    $.messager.alert('提示', m_Msg);
                }
            }
        });
    }
    else {
        $.messager.alert('提示', '没有任何数据需要保存');
    }
}

function onOrganisationTreeClick(node) {
    $('#productLineName').textbox('setText', node.text);
    $('#organizationId').val(node.OrganizationId);
    loadDatagrid("last");
}

