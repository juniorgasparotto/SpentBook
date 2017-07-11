$(document).ready(function () {
    Dropzone.autoDiscover = false;

    window.Page = {};
    var idImport = $("#body-import > input#idImport").val();
    var steps = new Steps();

    Page.BankSelector = new BankSelector({
        done: function () {
            var transUploader = new TransactionUploader({
                acceptedFiles: Page.BankSelector.GetAcceptExtension(),
                bankName: Page.BankSelector.GetBankName(),
                done: function () {
                    var transImport = new TransactionImport({
                        idImport: idImport
                    }).Load();

                    steps.GoToPreview();
                }
            }).Load();

            steps.GoToSelectFiles();
        }
    });
    
    /*
    * Class "Steps"
    */
    function Steps() {
        var self = this;
        var headers = $('#body-import #steps-header li');
        var contents = $('#body-import #steps-content .content');
        var current = null;

        headers.find('a').click(function(e) {
            e.preventDefault();
        });

        this.GoToStep = function(index) {
            var li = headers.eq(index - 1);
            var a = li.find('a');
            var target = $(a.attr('href'));

            headers.removeClass('active');
            headers.addClass('disabled');

            li.removeClass('disabled').addClass('active');

            if (current) {
                current.fadeOut(200, function() {
                    target.fadeIn(200);
                });
            } else {
                target.show();
            }
            current = target;
        }

        this.GoToSelectFormat = function() {
            this.GoToStep(1);
        }

        this.GoToSelectFiles = function() {
            
            this.GoToStep(2);
        }

        this.GoToPreview = function() {
            this.GoToStep(3);
        }

        contents.hide().removeClass("hidden");
        this.GoToStep(1);
        return self;
    }
    
    /*
    * Class "BankSelector"
    */
    function BankSelector(options) {
        var self = this;

        if (!options.done)
            throw "O método de callback 'done' não esta definido em 'BankSelector'";

        var btn = $('#body-import .cc-selector button.continue');

        this.GetBankName = function () {
            return $('#body-import input[name=importFormat]:checked').val();
        };

        this.GetAcceptExtension = function() {
            var format = self.GetBankName();
            if (format === "bradesco")
                return ".ofx";
            return ".csv";
        }

        btn.click(function () {
            if (!self.GetBankName()) {
                alert("Selecione um formato");
                return false;
            }

            options.done();
        });

        return self;
    };

    /*
    * Class "TransactionUploader"
    */
    function TransactionUploader(options) {
        var self = this;

        if (!options.done)
            throw "O método de callback 'done' não esta definido em 'TransactionUploader'";

        if (!options.bankName)
            throw "Nenhum banco foi selecionado em 'TransactionUploader'";

        var parent = $('#body-import #file-upload');
        options.btnContinue = options.btnContinue ? options.btnContinue : parent.find('button.continue');
        options.inputBankName = options.inputBankName ? options.inputBankName : parent.find('input#bank');
        options.inputFormat = options.inputFormat ? options.inputFormat : parent.find('input#format');
        options.formName = options.formName ? options.formName : '#dropzone';
        options.previewName = options.previewName ? options.previewName : '#body-import #file-upload #preview-template';
        options.acceptedFiles = options.acceptedFiles ? options.acceptedFiles : '*.*';

        this.Dropzone = null;

        this.Load = function () {
            Dropzone.autoDiscover = false;
            Dropzone.prototype.defaultOptions.dictDefaultMessage = "Arraste os arquivos de extratos nesta área";
            Dropzone.prototype.defaultOptions.dictFileTooBig = "O arquivo é muito grande ({{filesize}}MiB). O máximo permitido é: {{maxFilesize}}MiB.";
            Dropzone.prototype.defaultOptions.dictInvalidFileType = "O formato desse arquivo não é suportado";
            Dropzone.prototype.defaultOptions.dictResponseError = "Erro do servidor (código: {{statusCode}})";
            Dropzone.prototype.defaultOptions.dictRemoveFile = "Remover";
            Dropzone.prototype.defaultOptions.dictMaxFilesExceeded = "Você não pode importar mais arquivos nesse mesmo processo.";

            options.inputBankName.val(options.bankName);
            options.inputFormat.val(options.acceptedFiles);

            var dropzone = new Dropzone(options.formName, {
                previewTemplate: document.querySelector(options.previewName).innerHTML,
                parallelUploads: 12,
                maxFiles: 12,
                maxFilesize: 20,
                filesizeBase: 1000,
                acceptedFiles: options.acceptedFiles,
                addRemoveLinks: true,
                createImageThumbnails: false,
                paramName: "files",
                autoProcessQueue: false,
                uploadMultiple: false
            });

            //dropzone.on("addedfiles", function (files) {
            //    for (var index in files) {
            //        $(files[index].previewElement).addClass(..extension..);
            //    }
            //});

            // Esconde durante o envio, e só volta se ocorrer um erro
            dropzone.on("sending", function (file) {
                $(file._removeLink).hide();
            });

            dropzone.on("error", function (file, response) {
                $(file._removeLink).show();
                var span = $(file.previewElement).find(".dz-error-message span");

                if (response.message)
                    span.text(response.message);
                else
                    span.text(response);
            });

            dropzone.on("queuecomplete", function (progress) {
                options.btnContinue.button('reset');
                if (!self.Validate())
                    return;

                if (self.Dropzone.getQueuedFiles().length === 0) {
                    options.done();
                }
            });

            self.Dropzone = dropzone;
            return dropzone;
        };

        this.Validate = function () {
            if (self.Dropzone.files.length === 0) {
                alert("Selecione ao menos um arquivo para continuar");
                return false;
            } else {
                for (var index in self.Dropzone.files) {
                    if (self.Dropzone.files[index].status === 'error') {
                        alert("Existem arquivos com problemas na importação, por favor, remova-os para continuar");
                        return false;
                    }
                }
            }
            return true;
        };

        options.btnContinue.click(function () {
            if (!self.Validate())
                return;

            options.btnContinue.button('loading');
            self.Dropzone.processQueue();
            // continue in "sending", "error" and "queuecomplete"
        });

        return self;
    };

    /*
    * Class TransactionImport
    */
    function TransactionImport(options) {
        var self = this;

        if (!options.idImport)
            throw new "A propriedade 'IdImport' não esta definido em 'TransactionEditable'";

        var btnContinue = $('#body-import #table-statement button.continue');
        var btnCancel = $('#body-import #table-statement button.cancel');

        options.urlGetData = "Import/GetByImport";
        options.urlSave = 'Import/Save';
        options.urlCancel = "Import/Cancel";
        options.urlFinishOnCancel = '/Import';
        options.urlFinishOnSuccess = '/';
        options.tableElement = '#body-import #table-statement #table';

        var transEditable = new TransactionEditable({
            tableElement: options.tableElement,
            urlSave: options.urlSave,
            beforeSave: function () {
                btnContinue.button('loading');
            },
            completeSave: function (data) {
                btnContinue.button('reset');
            },
            successOnSave: function (data) {
                alert("Importação concluída com sucesso!");
                window.location.href = options.urlFinishOnSuccess;
            },
            invalidSaveData: function (data) {
                alert(data.message);
            },
            errorOnSave: null
        });

        this.Load = function () {
            $.ajax({
                type: "GET",
                url: options.urlGetData,
                data: { idImport: options.idImport },
                beforeSend: function () {
                    btnContinue.button('loading');
                },
                complete: function () {
                    btnContinue.button('reset');
                },
                success: function (data) {
                    // é necessário o setTimeout, pois existe um bugs
                    // no handsontable que não renderiza de primeira
                    setTimeout(function () {
                        transEditable.LoadData(data);
                    }, 200);
                },
                error: function (error) {
                    Helper.ErrorResponse(error);
                }
            });
        };

        btnContinue.click(function () {
            transEditable.Save();
        });

        btnCancel.click(function () {
            if (!confirm("Deseja realmente cancelar essa operação?"))
                return;

            $.ajax({
                type: "GET",
                url: options.urlCancel,
                data: { idImport: options.IdImport },
                beforeSend: function () {
                    btnCancel.button("loading");
                },
                complete: function () {
                    btnCancel.button("reset");
                    window.location.href = options.urlFinishOnCancel;
                }
            });
        });

        return self;
    }
});