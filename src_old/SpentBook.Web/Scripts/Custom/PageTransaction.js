$(document).ready(function ($) {
    //var data = function () {
    //    return Handsontable.helper.createSpreadsheetData(100, 10);
    //};

    var container = $('#transactionSheet');

    // para validar: http://www.rawrers.org/?p=873
    // para salvar: http://www.codeproject.com/Tips/1074230/Load-and-Save-Handsontable-Data-From-To-Controller
    // suporte: http://past.handsontable.com/demo/datasources.html
    var hot = new Handsontable(container[0], {
        minSpareCols: 1,
        minSpareRows: 1,
        rowHeaders: true,
        colHeaders: true,
        contextMenu: false,
        stretchH: 'all',
        colHeaders: ["Nome", "Data", "Valor", "Categoria", "Sub-Categoria"],
        columns: [
           //{ data: "Id", type: 'text', unique: true },
           { data: "Name", type: 'text' },
           { data: "Date", type: 'date', dateFormat: 'DD/MM/YYYY', correctFormat: true, },
           { data: "Value", type: 'numeric', format: '$ 0,0.00', language: 'pt-br' },
           { data: "Category", type: 'text' },
           { data: "SubCategory", type: 'text' },
        ],
        data: PageTransaction.Transactions,
    });

    //jQuery.ajax({
    //    url: '/Transaction/Get',
    //    type: "GET",
    //    dataType: "json",
    //    contentType: 'application/json;charset=utf-8',
    //    async: true,
    //    processData: false,
    //    cache: false,
    //    success: function (data) {            
    //        hot.loadData(data);
    //    },
    //    error: function (xhr) {
    //        alert('error');
    //    }
    //});
});