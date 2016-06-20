(function () {
    window.CodeControl = window.CodeControl || {};
    window.CodeControl.Debug = false;
    var executions = {};

    window.CodeControl.Add = function (config, executeOneTime, playNow) {
        if (!config.key) {
            if (window.CodeControl.Debug)
                console.log("Key not defined");
            return;
        }
        else if (!config.method) {
            if (window.CodeControl.Debug)
                console.log("Method not defined");
            return;
        }

        if (executions[config.key])
            return;

        executions[config.key] = {
            key: config.key,
            method: config.method,
            interval: config.interval,
            autoPause: config.autoPause,
            autoRemove: config.autoRemove,
            executeIfTrue: config.executeIfTrue,
            args: config.args,
            intervalId: null
        };

        if (executeOneTime) {
            config.method(config.args);
        }

        if (playNow) {
            window.CodeControl.Play(config.key);
        }

        return window.CodeControl;
    }

    window.CodeControl.Remove = function (key) {
        if (!executions[key]) {
            if (window.CodeControl.Debug)
                console.log("Key '" + key + "' not found");
            return;
        }

        if (executions[key].intervalId) {
            window.CodeControl.Pause(key);
        }

        delete executions[key];

        return window.CodeControl;
    };

    window.CodeControl.Pause = function (key) {
        if (!executions[key]) {
            if (window.CodeControl.Debug)
                console.log("Key '" + key + "' not found");
            return;
        }

        clearInterval(executions[key].intervalId);
        executions[key].intervalId = null;

        return window.CodeControl;
    };

    window.CodeControl.Play = function (key) {
        if (!executions[key]) {
            if (window.CodeControl.Debug)
                console.log("Key '" + key + "' not found");
            return;
        }

        var executionItem = executions[key];
           
        if (executionItem.interval) {
            executionItem.intervalId = setInterval(function () {
                // 1. caso o método de auto pause retorne TRUE então não pode executar o código
                // 2. o intervalo deve ser removido
                var canExecute = true;
                var pauseNow = executionItem.autoPause && executionItem.autoPause(executionItem.args);
                if (pauseNow) {
                    window.CodeControl.Pause(executionItem.key);
                    canExecute = false;
                }

                var removeNow = executionItem.autoRemove && executionItem.autoRemove(executionItem.args);
                if (removeNow) {
                    window.CodeControl.Remove(executionItem.key);
                    canExecute = false;
                }

                // se canExecute for TRUE e executeIfTrue esta declarado então canExecute vale o retorno de executeIfTrue.
                if (canExecute && executionItem.executeIfTrue) {
                    canExecute = executionItem.executeIfTrue(executionItem.args);
                }

                if (canExecute) {
                    executionItem.method(executionItem.args);
                }
            }, executionItem.interval);
        }
        else {
            if (window.CodeControl.Debug)
                console.log("Interval not defined");
        }

        return window.CodeControl;
    }

    /* Tests
        CodeControl.Remove("teste1");
        CodeControl.Pause("teste1");
        CodeControl.Play("teste1");

        var config = {
            key: "teste1",
            method: function () { alert("teste1"); },
            interval: 2,
            autoPause: function () { return confirm("pause?"); },
            autoRemove: function () { return confirm("remove?"); },
            executeIfTrue: function() { return true; },
            args: null,
        };

        CodeControl.Add(config, true, true);
    */
})();