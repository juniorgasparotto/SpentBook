$(document).ready(function () {
    window.PageImport = {};
    PageImport.IdImport = $("#body-import > input#idImport").val();
    PageImport.Dropzone = null;
    PageImport.TransactionEditable = new TransactionEditable(PageImport.IdImport);
    PageImport.Steps = new Steps();
    
    configureUpload();
});

function Steps() {
    var self = this;
    var headers = $('#body-import #steps-header li');
    var contents = $('#body-import #steps-content .content');
    var btnStep1 = $('#body-import .cc-selector button.continue');
    var btnStep2 = $('#body-import #file-upload button.continue');
    var btnStep3 = $('#body-import #table-statement button.continue');
    var btnStep3Cancel = $('#body-import #table-statement button.cancel');
    var current = null;

    headers.find('a').click(function (e) {
        e.preventDefault();
    });
    
    this.GoToStep = function (index) {
        var li = headers.eq(index - 1);
        var a = li.find('a');
        var target = $(a.attr('href'));
        
        headers.removeClass('active');
        headers.addClass('disabled');

        li.removeClass('disabled').addClass('active');

        if (current) {
            current.fadeOut(200, function () {
                target.fadeIn(200);
            });
        }
        else {
            target.show();
        }
        current = target;
    }

    this.GoToSelectFormat = function () {
        this.GoToStep(1);
    }

    this.GoToSelectFiles = function () {
        PageImport.Dropzone.options.acceptedFiles = getAcceptExtension();
        $('#dropzone #bank').val(getFormatType());
        $('#dropzone #format').val(getAcceptExtension());
        this.GoToStep(2);
    }

    this.GoToPreview = function () {
        setTimeout(function () {
            PageImport.TransactionEditable.Load();
        }, 200);
        this.GoToStep(3);
    }

    contents.hide().removeClass("hidden");
    this.GoToStep(1);

    //this.GoToStep(3);
    //PageImport.TransactionEditable.Load();

    /*
    * Buttons actions and validations
    */

    this.ResetUploadFileButton = function () {
        btnStep2.button('reset');
    };

    btnStep1.click(function () {
        if (validateStep1()) {
            self.GoToSelectFiles();
        }
    });

    btnStep2.click(function () {
        if (validateStep2()) {
            if (PageImport.Dropzone.getQueuedFiles().length === 0) {
                self.GoToPreview();
            }
            else {
                btnStep2.button('loading');
                PageImport.Dropzone.processQueue();
            }
        }
    });

    btnStep3.click(function () {
        PageImport.TransactionEditable.Validate(function (valid) {
            if (valid) {
                btnStep3.button('loading');
                PageImport.TransactionEditable.Save(function (noBusinessError) {
                    btnStep3.button('reset');

                    if (noBusinessError) {
                        alert("Importação concluída com sucesso!");
                        window.location.href = '/';
                    }
                }, function (error) {
                    btnStep3.button('reset');
                });
            }
            else {
                alert("Existem erros que precisam ser corrigidos.");
            }
        });
    });

    btnStep3Cancel.click(function () {
        if (!confirm("Deseja realmente cancelar essa importação?"))
            return;

        PageImport.TransactionEditable.Cancel();
        window.location.href = '/Import';
    });
}

function configureUpload() {
    Dropzone.autoDiscover = false;
    Dropzone.prototype.defaultOptions.dictDefaultMessage = "Arraste os arquivos de extratos nesta área";
    Dropzone.prototype.defaultOptions.dictFileTooBig = "O arquivo é muito grande ({{filesize}}MiB). O máximo permitido é: {{maxFilesize}}MiB.";
    Dropzone.prototype.defaultOptions.dictInvalidFileType = "O formato desse arquivo não é suportado";
    Dropzone.prototype.defaultOptions.dictResponseError = "Erro do servidor (código: {{statusCode}})";
    Dropzone.prototype.defaultOptions.dictRemoveFile = "Remover";
    Dropzone.prototype.defaultOptions.dictMaxFilesExceeded = "Você não pode importar mais arquivos nesse mesmo processo.";

    var dropzone = new Dropzone('#dropzone', {
        previewTemplate: document.querySelector('#preview-template').innerHTML,
        parallelUploads: 12,
        maxFiles: 12,
        maxFilesize: 20,
        filesizeBase: 1000,
        acceptedFiles: ".csv,.ofx",
        addRemoveLinks: true,
        createImageThumbnails: false,
        paramName: "files",
        autoProcessQueue: false,
        uploadMultiple: false
    });

    dropzone.on("error", function (file, response) {
        $(file._removeLink).show();
        var span = $(file.previewElement).find(".dz-error-message span");

        if (response.message)
            span.text(response.message);
        else
            span.text(response);
    });

    dropzone.on("addedfiles", function (files) {
        for (var index in files) {
            $(files[index].previewElement).addClass(getFormatType());
        }
    });

    dropzone.on("removedfile", function (file) {
        
    });

    // Esconde durante o envio, e só volta se ocorrer um erro
    dropzone.on("sending", function (file) {
        $(file._removeLink).hide();
    });

    // File upload Progress
    dropzone.on("totaluploadprogress", function (progress) {

    });

    dropzone.on("queuecomplete", function (progress) {
        if (validateStep2()) {
            if (PageImport.Dropzone.getQueuedFiles().length === 0) {
                PageImport.Steps.GoToPreview();
            }
        }

        PageImport.Steps.ResetUploadFileButton();
    });

    dropzone.on("success", function (file, responseText, e) {
        
    });

    PageImport.Dropzone = dropzone;
}

function validateStep1() {
    var inputFormatValue = getFormatType();
    if (!inputFormatValue) {
        alert("Selecione um formato");
        return false;
    }

    return true;
}

function validateStep2() {
    if (PageImport.Dropzone.files.length === 0) {
        alert("Selecione ao menos um arquivo para continuar");
        return false;
    }
    else {
        for (var index in PageImport.Dropzone.files) {
            if (PageImport.Dropzone.files[index].status === 'error') {
                alert("Existem arquivos com problemas na importação, por favor, remova-os para continuar");
                return false;
            }
        }
    }

    return true;
}

function validateStep3() {
    return true;
}

function getFormatType() {
    return $('input[name=importFormat]:checked').val();
}

function getAcceptExtension() {
    var format = getFormatType();
    if (format === "bradesco")
        return ".ofx";
    return ".csv";
}

function TransactionEditable(idImport) {
    var self = this;
    this.IdImport = idImport;
    this.UrlGetData = "Import/GetByImport";
    this.UrlSave = "Import/Save";
    this.UrlCancel = "Import/CancelImport";
    this.TableElement = "#table-statement #table";
    this.Handsontable = null;
    this.Data = null;

    this.Configure = function (data) {
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

        var errorRenderer = function (instance, td, row, col, prop, value, cellProperties) {
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
                icon.mouseover(function () {
                    messageElement.show();
                }).mouseout(function () {
                    if (time)
                        clearTimeout(time);

                    time = setTimeout(function () {
                        messageElement.hide();
                    }, 20);
                });

                messageElement.mouseenter(function () {
                    clearTimeout(time);
                }).mouseleave(function () {
                    messageElement.hide();
                });
            }
        };

        var negativeValueRenderer = function (instance, td, row, col, prop, value, cellProperties) {
            if (!value)
                value = 0;

            Handsontable.renderers.NumericRenderer.apply(this, arguments);

            if (parseInt(value, 10) <= 0) {
                td.style.color = 'red';
            }
            else {
                td.style.color = 'blue';
            }
        };

        var deleteRowRenderer = function (instance, td, row, col, prop, value, cellProperties) {
            while (td.firstChild) {
                td.removeChild(td.firstChild);
            }

            var remove = $('<span class="glyphicon glyphicon-floppy-remove icon remove" title="Remover transação"></span>');
            remove.click(function () {
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
                { data: "Id", disableVisualSelection:true, renderer: deleteRowRenderer, readOnly: true, width: "30px" },
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

        self.Handsontable = new Handsontable($(self.TableElement)[0], hotSettings);
    };

    this.Load = function () {
        $.ajax({
            type: "GET",
            url: self.UrlGetData,
            data: { idImport: self.IdImport },
            beforeSend: function () {

            },
            complete: function () {

            },
            success: function (data) {
                self.Data = data;
                if (self.Handsontable)
                    self.Handsontable.loadData(data.Transactions);
                else
                    self.Configure(data);
            },
            error: function (error) {
                Helper.ErrorResponse(error);
            }
        });
    };

    this.Validate = function (actionIfInvalid) {
        self.Handsontable.validateCells(function (valid) {
            actionIfInvalid(valid);
        });
    };

    this.Save = function (success, fail) {
        $.ajax({
            type: "POST",
            url: self.UrlSave,
            data: JSON.stringify({ InitialIds: self.Data.InitialIds, Transactions: self.Handsontable.getSourceData() }),
            dataType: 'json',
            contentType: 'application/json',
            success: function (data) {
                if (data.message === "OK") {
                    if (success)
                        success(true);
                }
                else {
                    alert(data.message);
                    self.Handsontable.loadData(data.transactions);
                    if (success)
                        success(false);
                }
            },
            error: function (error) {
                if (fail)
                    fail(error);
                Helper.ErrorResponse(error);
            }
        });
    };

    this.Cancel = function () {        
        $.ajax({
            type: "GET",
            url: self.UrlCancel,
            data: { idImport: self.IdImport },
            async: false,
            success: function (data) {
            },
            error: function (error) {
                Helper.ErrorResponse(error);
            }
        });
    };
}