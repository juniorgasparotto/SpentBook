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
                    if (error.responseJSON[0].errors) {
                        for (var iField in error.responseJSON)
                            for (var iErro in error.responseJSON[iField].errors)
                                strError += "- " + error.responseJSON[iField].errors[iErro] + "\r\n";
                    }
                    else {
                        for (var iErro2 in error.responseJSON)
                            strError += error.responseJSON[iErro2] + "\r\n";
                    }
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
        CreateULByArray: function (array) {
            // Create the list element:
            var list = $('<ul></ul>');

            for (var i = 0; i < array.length; i++) {
                // Create the list item:
                var item = $('<li></li>');

                // Set its contents:
                item.html(array[i]);

                // Add it to the list:
                list.append(item);
            }

            // Finally, return the constructed list:
            return list;
        }
    }
}());