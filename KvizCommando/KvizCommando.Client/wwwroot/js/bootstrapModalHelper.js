window.bootstrapModalHelper = {
    show: function (selector) {
        var modalElement = document.querySelector(selector);
        if (modalElement) {
            var modal = new bootstrap.Modal(modalElement);
            modal.show();
        }
    }
};
