// Steve's adding of old browser support
Date.now = Date.now || function () { return (+new Date()) };
// Rasmus' adding of old browser support
console = console || { log: function () { }, warn: function () { } };

!function ($, window, document, _undefined) {
    "use strict";

    XF.SqrlLogin = XF.Element.newHandler({
        options: {
            hostname: null,
            slotId: '',
        },

        init: function () {
            this.latestData = null;

            if (!this.options.hostname || !this.options.hostname) {
                console.warn('Unable to find query URL or hostname. Failed.');
                return;
            }

            // Get nut
            this.getNut();
        },

        getNut: function () {
            $.ajax({
                url: 'https://' + this.options.hostname + '/nut.sqrl?' + this.options.slotId,
                success: this.gotNut.bind(this),
                error: this.failedNut.bind(this),
                dataType: 'text',
            });
        },

        clearError: function () {
            if (this.lastErrorEl) {
                this.lastErrorEl.remove();
                this.lastErrorEl = null;
            }
        },

        failedNut: function (jqXHR, textStatus, errorThrown) {
            this.clearError();
            var errorEl = $('<div class="error"></div>');
            errorEl.text('Failed to communicate with SQRL. Please refresh if you need SQRL authentication.');
            this.$target.append(errorEl);
            this.lastErrorEl = errorEl;
        },

        gotNut: function (nutAndCan, textStatus, jqXHR) {
            this.clearError();
            var nut;
            var nameAndValuePairs = nutAndCan.split('&');
            for (var i = 0; i < nameAndValuePairs.length; i++) {
                var nameAndValue = nameAndValuePairs[i].split('=');
                if (nameAndValue[0] == 'nut') {
                    nut = nameAndValue[1];
                    break;
                }
            }
            this.latestData = { nut: nut, nutAndCan: nutAndCan };

            this.renderNut();

            this.startQrAuthCheck();
        },

        renderNut: function () {
            var link = 'sqrl://' + this.options.hostname + '/cli.sqrl?' + this.latestData.nutAndCan;
            var png = 'https://' + this.options.hostname + '/png.sqrl?nut=' + this.latestData.nut;

            this.$target.find('.frame').removeClass('disable');
            this.$target.find('a.button, a.image')
                .attr('href', link)
                .click(this.linkClicked.bind(this));
            this.$target.find('img').attr('src', png);
        },

        linkClicked: function (e) {
            var encodedSqrlUrl = window.btoa('sqrl://' + this.options.hostname + '/cli.sqrl?' + this.latestData.nutAndCan)
                .replace(/\//, "_")
                .replace(/\+/, "-")
                .replace(/=+$/, "");
            var probeImage = new Image();
            var probeError = function (err) {
                setTimeout(
                    function () {
                        probeImage.src = 'http://localhost:25519/' + Date.now() + '.gif';
                    },
                    250
                );
            };
            probeImage.onerror = probeError;
            probeImage.onload = function () {
                document.location.href = 'http://localhost:25519/' + encodedSqrlUrl;
            };
            probeError();
            return true;
        },

        startQrAuthCheck: function () {
            if (this.qrAuthTimer) {
                clearInterval(this.qrAuthTimer);
                this.qrAuthTimer = null;
            }
            this.qrAuthTimer = setInterval(this.qrAuthCheck.bind(this), 500);
        },

        qrAuthCheck: function () {
            if (this.$target.find('img').is(":hidden") || document.hidden) {
                return;
            }
            $.ajax({
                url: 'https://' + this.options.hostname + '/pag.sqrl?nut=' + this.latestData.nut,
                success: this.handleQrAuthCheckResponse.bind(this),
                // Prevents the annoying ajax animation from showing up all the time
                global: false,
            });
        },

        handleQrAuthCheckResponse: function (body, textStatus, jqXHR) {
            document.location.href = body;
        }
    });

    XF.Element.register('sqrl-login', 'XF.SqrlLogin');
}
    (jQuery, window, document);