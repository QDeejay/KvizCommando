// KC Bootstrap Modal helper
// használat: kcModal.show('#id'); kcModal.hide('#id');
(function (w) {
    function q(sel) { return document.querySelector(sel); }
    function ensureInstance(el) {
        if (!el) return null;
        let inst = bootstrap.Modal.getInstance(el);
        if (!inst) inst = new bootstrap.Modal(el, { backdrop: 'static', keyboard: false });
        return inst;
    }
    w.kcModal = {
        show: function (sel) {
            const el = q(sel);
            const inst = ensureInstance(el);
            if (inst) inst.show();
        },
        hide: function (sel) {
            document.activeElement?.blur();
            const el = q(sel);
            const inst = bootstrap.Modal.getInstance(el);
            if (inst) inst.hide();
        }
    };
})(window);
