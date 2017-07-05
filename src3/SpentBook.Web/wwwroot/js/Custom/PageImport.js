$(document).ready(function () {
    window.PageImport = {};
    configureSteps();
    configureUpload();
    configurePreview();
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
            if (currentIndex == 0) {
                return validateStep1();
            }
            else if (currentIndex == 1) {
                var valid = validateStep2();
                if (valid) {
                    if (PageImport.Dropzone.getQueuedFiles().length == 0) {
                        return true;
                    } 
                    else {
                        PageImport.Dropzone.processQueue();
                    }
                }
                return false;
            }
            else if (currentIndex == 2) {
                return validateStep3();
            }

            return true;
        },
        onStepChanged: function (event, currentIndex, priorIndex) {
            if (currentIndex == 0) {
                PageImport.Steps.btnNext.text("Selecionar");
            }
            else if (currentIndex == 1) {
                PageImport.Steps.btnPrevious.hide();
                PageImport.Steps.firstTab.addClass("disabled");
                PageImport.Steps.firstTab.attr("aria-disabled", true);
                PageImport.Steps.btnNext.text("Importar...");
                PageImport.Dropzone.options.acceptedFiles = getAcceptExtension();
            }
            else if (currentIndex == 2) {
                PageImport.Steps.secondTab.addClass("disabled");
                PageImport.Steps.secondTab.attr("aria-disabled", true);
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

function configurePreview() {
    jQueryStepsResize();
    var data = [
        ["", "Ford", "Volvo", "Toyota", "Honda"],
        ["2016", 10, 11, 12, 13],
        ["2017", 20, 11, 14, 13],
        ["2018", 30, 15, 12, 13]
    ];

    var container = document.getElementById('example');
    var hot = new Handsontable(container, {
        data: data,
        rowHeaders: true,
        colHeaders: true
    });
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
    if (PageImport.Dropzone.files.length == 0) {
        alert("Selecione ao menos um arquivo para continuar");
        return false;
    }
    else {
        for (var index in PageImport.Dropzone.files) {
            if (PageImport.Dropzone.files[index].status == 'error') {
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
    if (format == "bradesco")
        return ".ofx";
    return ".csv";
}