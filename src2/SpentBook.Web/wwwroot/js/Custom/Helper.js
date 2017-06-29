(function () {
    // ****************************
    // extensions
    // ****************************
    String.prototype.format = function () {
        var formatted = this;
        for (var arg in arguments) {
            formatted = formatted.replace("{" + arg + "}", arguments[arg]);
        }
        return formatted;
    };

    // ****************************
    // helper class
    // ****************************

    // ****************************
    // globals
    // ****************************
    window.Helper = window.Helper || {};

    // ****************************
    // public methods
    // ****************************

    Helper = Helper.prototype = {
        ErrorResponse: function (error) {
            if (error.responseJSON) {
                if (error.responseJSON.message) {
                    // Type: Action Exception
                    alert(error.responseJSON.message)
                }
                else if (error.responseJSON[0]) {
                    // Type: Invalid Model error
                    var strError = "";
                    for (var iField in error.responseJSON)
                        for (var iErro in error.responseJSON[iField].errors)
                            strError += "- " + error.responseJSON[iField].errors[iErro] + "\r\n";
                    alert(strError);
                }
            }
            else {
                // Type: Inexpected Exception
                alert(error.statusText)
            }
        },
        OpenUrl: function (url, target) {
            target = target ? target : '_self';
            var win = window.open(url, target);
            win.focus();
        },
        DisableWindow: function () {
            $("body").append('<div style="width: 100%; height: 100%; position: absolute; left: 0; top: 0;" id="disabled-window"> </div>');
        },
        EnabledWindow: function () {
            $("#disabled-window").remove();
        },
    }
}());