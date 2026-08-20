window.kcMeasure = (selector) => {
    const el = document.querySelector(selector);
    if (!el) return null;
    const rect = el.getBoundingClientRect();
    return {
        scrollTop: el.scrollTop,
        scrollHeight: el.scrollHeight,
        clientHeight: el.clientHeight,
        offsetHeight: el.offsetHeight,
        boxHeight: rect.height
    };
};

document.addEventListener("keydown", (event) => {
    const triggerId = {
        F1: "kc-help-trigger",
        F2: "kc-settings-trigger",
        F3: "kc-profile-trigger"
    }[event.key];

    if (!triggerId) return;

    const trigger = document.getElementById(triggerId);
    if (!trigger) return;

    event.preventDefault();
    trigger.click();
});
