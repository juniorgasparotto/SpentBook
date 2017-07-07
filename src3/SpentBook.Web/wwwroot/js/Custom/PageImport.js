$(document).ready(function () {
    window.PageImport = {};
    PageImport.Steps = null;
    PageImport.Dropzone = null;
    PageImport.TransactionEditable = new TransactionEditable();
    PageImport.TransactionEditable.Load();

    $("#save").click(function () {
        PageImport.TransactionEditable.Validate(function (valid) {
            if (valid) {
                PageImport.TransactionEditable.Save();
            }
            else {
                alert("Existem erros que precisam ser corrigidos.");
            }
        });
    });

    configureSteps();
    configureUpload();
});

function configureSteps() {
    PageImport.Steps = $("#steps").steps({
        headerTag: "h3",
        bodyTag: "section",
        transitionEffect: "slideLeft",
        autoFocus: true,
        labels:
        {
            previous: "Voltar",
            next: "Continuar",
            finish: "Finalizar!",
        },
        onInit: function (event, current) {
            
        },
        onStepChanging: function (event, currentIndex, newIndex) {
            if (currentIndex === 0) {
                return validateStep1();
            }
            else if (currentIndex === 1) {
                var valid = validateStep2();
                if (valid) {
                    if (PageImport.Dropzone.getQueuedFiles().length === 0) {
                        return true;
                    } 
                    else {
                        PageImport.Dropzone.processQueue();
                    }
                }
                return false;
            }
            else if (currentIndex === 2) {
                return validateStep3();
            }

            return true;
        },
        onStepChanged: function (event, currentIndex, priorIndex) {
            if (currentIndex === 0) {
                PageImport.Steps.btnNext.text("Selecionar");
            }
            else if (currentIndex === 1) {
                PageImport.Steps.btnPrevious.hide();
                PageImport.Steps.firstTab.addClass("disabled");
                PageImport.Steps.firstTab.attr("aria-disabled", true);
                PageImport.Steps.btnNext.text("Importar...");
                PageImport.Dropzone.options.acceptedFiles = getAcceptExtension();
                $('#dropzoneStatement #bank').val(getFormatType());
                $('#dropzoneStatement #format').val(getAcceptExtension());
            }
            else if (currentIndex === 2) {
                PageImport.Steps.secondTab.addClass("disabled");
                PageImport.Steps.secondTab.attr("aria-disabled", true);
                PageImport.TransactionEditable.Load();
            }
        },
        onFinishing: function (event, currentIndex) {
            
        },
        onFinished: function (event, currentIndex) {
            
        }
    });

    PageImport.Steps.btnPrevious = $(".actions > ul > li a[href='#previous']");
    PageImport.Steps.btnNext = $(".actions > ul > li a[href='#next']");
    PageImport.Steps.firstTab = $('#steps li.first');
    PageImport.Steps.secondTab = PageImport.Steps.firstTab.next();
    PageImport.Steps.btnPrevious.parent().hide();
}

function configureUpload() {
    Dropzone.autoDiscover = false;
    Dropzone.prototype.defaultOptions.dictDefaultMessage = "Arraste os arquivos de extratos nesta área";
    Dropzone.prototype.defaultOptions.dictFileTooBig = "O arquivo é muito grande ({{filesize}}MiB). O máximo permitido é: {{maxFilesize}}MiB.";
    Dropzone.prototype.defaultOptions.dictInvalidFileType = "O formato desse arquivo não é suportado";
    Dropzone.prototype.defaultOptions.dictResponseError = "Erro do servidor (código: {{statusCode}})";
    Dropzone.prototype.defaultOptions.dictRemoveFile = "Remover";
    Dropzone.prototype.defaultOptions.dictMaxFilesExceeded = "Você não pode importar mais arquivos nesse mesmo processo.";

    var dropzone = new Dropzone('#dropzoneStatement', {
        previewTemplate: document.querySelector('#preview-template').innerHTML,
        parallelUploads: 12,
        maxFiles: 12,
        maxFilesize: 20,
        filesizeBase: 1000,
        acceptedFiles: ".csv,.ofx,.cs",
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
        jQueryStepsResize();
    });

    dropzone.on("removedfile", function (file) {
        jQueryStepsResize();
    });

    // Esconde durante o envio, e só volta se ocorrer um erro
    dropzone.on("sending", function (file) {
        $(file._removeLink).hide();
    });

    // File upload Progress
    dropzone.on("totaluploadprogress", function (progress) {

    });

    dropzone.on("queuecomplete", function (progress) {
        PageImport.Steps.steps("next");
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

function TransactionEditable() {
    var self = this;
    this.UrlGetData = "Import/Get";
    this.UrlSave = "Import/Save";
    this.TableElement = $("#table-statement #table");
    this.SaveButtonElement = $("#table-statement #save");
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

        //[{ "Id": "f6a6f1ec-ee37-499a-8fd7-a9e5a479d708", "UserId": "04786e69-5f8d-460f-adec-0bd145d90814", "Name": "Tes", "Date": "14-07-2017 00:00", "Value": -2.8, "Category": "alimentação", "SubCategory": "café", "BankName": "teste", "FormatFile": "teste2", "Status": 3, "StatusMessage": ["O campo 'Data' não pode estar vazio", "O campo 'Nome' não pode estar vazio"] }]

        var hotSettings = {
            data: data.Transactions,
            colHeaders: ["", "", "Nome", "Valor", "Data", "Banco", "Categoria", "Sub-Categoria"],
            columns: [
                { data: "Id", disableVisualSelection:true, renderer: deleteRowRenderer, readOnly: true, width: "20px" },
                { data: "Status", disableVisualSelection: true, type: 'text', renderer: errorRenderer, readOnly: true, width: "20px" },
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
            //hiddenColumns: {
            //    columns: [0],
            //    indicators: true
            //}
            //afterValidate: function (isValid, value, row, prop, source) {
            //    if (!isValid) 
            //        self.SaveButtonElement.hide();
            //    else 
            //        self.SaveButtonElement.show();
            //}
        };

        self.Handsontable = new Handsontable(self.TableElement[0], hotSettings);
    };

    this.Load = function () {
        $.ajax({
            type: "GET",
            url: self.UrlGetData,
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

    this.Save = function () {        
        $.ajax({
            type: "POST",
            url: self.UrlSave,
            data: JSON.stringify({ InitialIds: self.Data.InitialIds, Transactions: self.Handsontable.getSourceData() }),
            dataType: 'json',
            contentType: 'application/json',
            beforeSend: function () {

            },
            complete: function () {

            },
            success: function (data) {
                if (data.message === "OK") {
                    self.Load();
                    alert("Continuar o fluxo");
                }
                else {
                    alert(data.message);
                    self.Handsontable.loadData(data.transactions);
                }
            },
            error: function (error) {
                Helper.ErrorResponse(error);
            }
        });
    };
}