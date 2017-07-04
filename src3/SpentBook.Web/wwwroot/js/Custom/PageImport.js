$(document).ready(function () {
    configureSteps();
    configureUpload();
});

function configureSteps() {
    $("#steps").steps({
        headerTag: "h3",
        bodyTag: "section",
        transitionEffect: "slideLeft",
        autoFocus: true,
        labels:
        {
            previous: "Voltar",
            next: "Prosseguir",
            finish: "Importar!",
        },
        onInit: function (event, current) {
            $('.actions > ul > li:first-child').attr('style', 'display:none');
        },
        onStepChanging: function (event, currentIndex, newIndex) {
            if (newIndex >= 1) {
                $('.actions > ul > li:first-child').attr('style', '');
            } else {
                $('.actions > ul > li:first-child').attr('style', 'display:none');
            }

            var inputFormatValue = $('input[name=importFormat]:checked').val();
            if (!inputFormatValue) {
                alert("Selecione um formato");
                return false;
            }
            //form.validate().settings.ignore = ":disabled,:hidden";
            //return true;
            return true;
        },
        onStepChanged: function (event, currentIndex, priorIndex) {
            
        },
        onFinishing: function (event, currentIndex) {
            //form.validate().settings.ignore = ":disabled";
            //return form.valid();
        },
        onFinished: function (event, currentIndex) {
            alert("Submitted!");
        }
    });
}

function configureUpload() {
    Dropzone.autoDiscover = false;

    // Get the template HTML and remove it from the doument
    //var previewNode = document.querySelector("#template");
    //previewNode.id = "";
    //var previewTemplate = previewNode.parentNode.innerHTML;
    //previewNode.parentNode.removeChild(previewNode);

    var dropzone = new Dropzone('#dropzone', {
        //previewTemplate: document.querySelector('#preview-template').innerHTML,
        parallelUploads: 2,
        thumbnailHeight: 120,
        thumbnailWidth: 120,
        maxFilesize: 3,
        filesizeBase: 1000,
        //thumbnail: function (file, dataUrl) {
        //    if (file.previewElement) {
        //        file.previewElement.classList.remove("dz-file-preview");
        //        var images = file.previewElement.querySelectorAll("[data-dz-thumbnail]");
        //        for (var i = 0; i < images.length; i++) {
        //            var thumbnailElement = images[i];
        //            thumbnailElement.alt = file.name;
        //            thumbnailElement.src = dataUrl;
        //        }
        //        setTimeout(function () { file.previewElement.classList.add("dz-image-preview"); }, 1);
        //    }
        //}

    });

    dropzone.on("totaluploadprogress", function (progress) {
       
    });

    dropzone.on("sending", function (file) {

    });

    dropzone.on("queuecomplete", function (progress) {
        
    });

    // Now fake the file upload, since GitHub does not handle file uploads
    // and returns a 404
    var minSteps = 6,
        maxSteps = 60,
        timeBetweenSteps = 100,
        bytesPerStep = 100000;

    dropzone.uploadFiles = function (files) {
        resizeJquerySteps();

        var self = this;

        for (var i = 0; i < files.length; i++) {

            var file = files[i];
            totalSteps = Math.round(Math.min(maxSteps, Math.max(minSteps, file.size / bytesPerStep)));

            for (var step = 0; step < totalSteps; step++) {
                var duration = timeBetweenSteps * (step + 1);
                setTimeout(function (file, totalSteps, step) {
                    return function () {
                        file.upload = {
                            progress: 100 * (step + 1) / totalSteps,
                            total: file.size,
                            bytesSent: (step + 1) * file.size / totalSteps
                        };

                        self.emit('uploadprogress', file, file.upload.progress, file.upload.bytesSent);
                        if (file.upload.progress == 100) {
                            file.status = Dropzone.SUCCESS;
                            self.emit("success", file, 'success', null);
                            self.emit("complete", file);
                            self.processQueue();
                        }
                    };
                }(file, totalSteps, step), duration);
            }
        }
    };
}
