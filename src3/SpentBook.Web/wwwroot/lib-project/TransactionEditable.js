function TransactionEditable(options) {
    var self = this;

    if (!options.urlSave)
        throw new "A propriedade 'urlSave' não esta definido em 'TransactionEditable'";

    if (!options.tableElement)
        throw new "A propriedade 'tableElement' não esta definido em 'TransactionEditable'";

    if (!options.successOnSave)
        throw new "O método de callback 'successOnSave' não esta definido em 'TransactionEditable'";

    if (!options.invalidSaveData)
        throw new "O método de callback 'invalidSaveData' não esta definido em 'TransactionEditable'";

    //if (!options.error)
    //    throw new "O método de callback 'error' não esta definido em 'TransactionEditable'";

    this.handsontable = null;
    this.initData = null;

    var init = function(data) {
        var enumStatus = [];
        enumStatus[0] = "success";
        enumStatus[1] = "warning";
        enumStatus[2] = "automatic-resolved";
        enumStatus[3] = "error";

        var iconClasses = [];
        iconClasses[0] = "glyphicon-ok";
        iconClasses[1] = "glyphicon-info-sign";
        iconClasses[2] = "glyphicon-info-sign";
        iconClasses[3] = "glyphicon-exclamation-sign";

        var errorRenderer = function(instance, td, row, col, prop, value, cellProperties) {
            var rowValue = instance.getDataAtRow(row);
            var status = value ? enumStatus[value] : enumStatus[0];
            var iconClass = value ? iconClasses[value] : iconClasses[0];
            var message = instance.getSourceDataAtRow(row).StatusMessage;

            while (td.firstChild) {
                td.removeChild(td.firstChild);
            }

            var wrapper = $('<div class="transaction-status"></div>');
            var icon = $('<span class="icon glyphicon"></span>');
            icon.addClass(iconClass);
            icon.addClass(status);
            wrapper.append(icon);
            td.appendChild(wrapper[0]);

            if (message && message.length) {

                var messageElement = $(document.createElement('DIV'));
                wrapper.append(messageElement);

                messageElement.addClass('message');
                messageElement.addClass(status);
                messageElement.html(Helper.CreateULByArray(message));

                // setar comportamento de exibição
                messageElement.hide();

                var time;
                icon.mouseover(function() {
                    messageElement.show();
                }).mouseout(function() {
                    if (time)
                        clearTimeout(time);

                    time = setTimeout(function() {
                        messageElement.hide();
                    }, 20);
                });

                messageElement.mouseenter(function() {
                    clearTimeout(time);
                }).mouseleave(function() {
                    messageElement.hide();
                });
            }
        };

        var negativeValueRenderer = function(instance, td, row, col, prop, value, cellProperties) {
            if (!value)
                value = 0;

            Handsontable.renderers.NumericRenderer.apply(this, arguments);

            if (parseInt(value, 10) <= 0) {
                td.style.color = 'red';
            } else {
                td.style.color = 'blue';
            }
        };

        var deleteRowRenderer = function(instance, td, row, col, prop, value, cellProperties) {
            while (td.firstChild) {
                td.removeChild(td.firstChild);
            }

            var remove = $('<span class="glyphicon glyphicon-floppy-remove icon remove" title="Remover transação"></span>');
            remove.click(function() {
                if (confirm("Deseja realmente excluir essa transação?")) {
                    return instance.alter("remove_row", row);
                }
            });
            td.appendChild(remove[0]);
        }

        var hotSettings = {
            data: data.Transactions,
            colHeaders: ["", "", "Nome", "Valor", "Data", "Banco", "Categoria", "Sub-Categoria"],
            columns: [
                { data: "Id", disableVisualSelection: true, renderer: deleteRowRenderer, readOnly: true, width: "30px" },
                { data: "Status", disableVisualSelection: true, type: 'text', renderer: errorRenderer, readOnly: true, width: "30px" },
                { data: "Name", type: 'text' },
                { data: "Value", type: 'numeric', format: '$ 0,0.00', renderer: negativeValueRenderer, language: 'pt-BR' },
                { data: "Date", type: 'date', dateFormat: 'DD/MM/YYYY HH:mm:ss', language: 'pt-BR', correctFormat: true },
                { data: "BankName", type: 'autocomplete', source: data.Banks, strict: false },
                { data: "Category", type: 'autocomplete', source: data.Categories, strict: false },
                { data: "SubCategory", type: 'autocomplete', source: data.SubCategories, strict: false },
            ],
            stretchH: 'all',
            autoWrapRow: true,
            rowHeaders: true,
            height: 330,
        };

        self.handsontable = new Handsontable($(options.tableElement)[0], hotSettings);
    };

    this.LoadData = function (data) {
        self.initData = data;
        if (self.handsontable)
            self.handsontable.loadData(data.Transactions);
        else
            init(data);
    };
    
    this.Save = function () {
        self.handsontable.validateCells(function (valid) {
            if (!valid) {
                alert("Existem erros que precisam ser corrigidos.");
                return;
            }

            $.ajax({
                type: "POST",
                url: options.urlSave,
                data: JSON.stringify({ InitialIds: self.initData.InitialIds, Transactions: self.handsontable.getSourceData() }),
                dataType: 'json',
                contentType: 'application/json',
                beforeSend: function () {
                    if (options.beforeSave)
                        options.beforeSave();
                },
                complete: function (data) {
                    if (options.completeSave)
                        options.completeSave(data);
                },
                success: function (data) {
                    if (data.message === "OK") {
                        options.successOnSave(data);
                    } else {
                        self.handsontable.loadData(data.transactions);
                        options.invalidSaveData(data);
                    }
                },
                error: function (error) {
                    if (options.errorOnSave)
                        options.errorOnSave(error);
                    else
                        Helper.ErrorResponse(error);
                }
            });
        });
    };
}