$(document).ready(function($) {
    var transactions = new Transactions();
    transactions.PageLoad();

    transactions.BtnSearch.click(function() {
        transactions.LoadData();
    });

    transactions.BtnSave.click(function() {
        transactions.Save();
    });

    transactions.BtnCancel.click(function() {
        if (!confirm("Deseja realmente cancelar essa operação?"))
            return;

        transactions.Cancel();
    });

    /*
     * Class Transactions
     */
    function Transactions() {
        var self = this;
        var options = {};

        this.Actions = $('#body-transactions #result .actions');
        this.BtnSave = this.Actions.find('button.continue');
        this.BtnCancel = this.Actions.find('button.cancel');
        this.BtnSearch = $('#body-transactions button#search');
        this.Form = $('#body-transactions form');
        
        options.urlGetData = "Transaction/GetTable";
        options.urlSave = 'Transaction/SaveTable';
        options.urlFinishOnCancel = "/Transaction";
        options.tableElement = '#body-transactions #table';

        var transEditable = new TransactionTable({
            tableElement: options.tableElement,
            urlSave: options.urlSave,
            beforeSave: function() {
                self.BtnSave.button('loading');
            },
            completeSave: function(data) {
                self.BtnSave.button('reset');
            },
            successOnSave: function(data) {
                alert("As transações foram salvas com sucesso!");
            },
            invalidSaveData: function(data) {
                alert(data.message);
            },
            errorOnSave: null
        });
        
        this.Save = function() {
            transEditable.Save();
        };

        this.Cancel = function() {
            window.location.href = options.urlFinishOnCancel;
        };

        this.LoadData = function() {
            $.ajax({
                type: "POST",
                url: options.urlGetData,
                data: self.Form.serialize(),
                beforeSend: function() {
                    self.BtnSearch.button('loading');
                },
                complete: function () {
                    self.Actions.removeClass('hidden');

                    // apenas não piscar o loading quando o retorno é muito rápido.
                    setTimeout(function () {
                        self.BtnSearch.button('reset');
                    }, 200);
                },
                success: function(data) {
                    // � necess�rio o setTimeout, pois existe um bugs
                    // no handsontable que n�o renderiza de primeira
                    setTimeout(function() {
                        transEditable.LoadData(data);
                    }, 200);
                },
                error: function(error) {
                    Helper.ErrorResponse(error);
                }
            });
        };

        this.PageLoad = function () {
            self.Actions.addClass('hidden');
            self.LoadData();
        }

        return self;
    }
});