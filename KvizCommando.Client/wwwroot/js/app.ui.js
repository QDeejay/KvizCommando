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
    if (event.key !== "F1") return;

    const helpTrigger = document.getElementById("kc-help-trigger");
    if (!helpTrigger) return;

    event.preventDefault();
    helpTrigger.click();
});
