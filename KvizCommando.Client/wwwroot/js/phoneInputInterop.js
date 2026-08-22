window.phoneInputInterop = (() => {
    const instances = new WeakMap();

    function buildNumber(countryCode, number) {
        const trimmedNumber = (number ?? "").trim();

        if (trimmedNumber.startsWith("+"))
            return trimmedNumber;

        return `${countryCode ?? ""}${trimmedNumber.replace(/^0+/, "")}`;
    }

    async function getTranslations(culture) {
        if (!(culture ?? "").toLowerCase().startsWith("hu"))
            return {};

        const moduleUrl = new URL(
            "lib/intl-tel-input/js/locale/hu.js",
            document.baseURI);
        const translations = await import(moduleUrl.href);

        return translations.default;
    }

    function readValue(input, instance) {
        const country = instance.getSelectedCountry();
        const countryCode = country?.dialCode
            ? `+${country.dialCode}`
            : "";
        let number = input.value.trim();

        if (countryCode && instance.isValidNumber()) {
            const e164Number = instance.getNumber();

            if (e164Number.startsWith(countryCode))
                number = e164Number.slice(countryCode.length);
        }

        return { countryCode, number };
    }

    async function initialize(
        input,
        dotNetReference,
        countryCode,
        number,
        culture,
        disabled) {
        const uiTranslations = await getTranslations(culture);
        const instance = window.intlTelInput(input, {
            initialCountry: "hu",
            separateDialCode: true,
            countryNameLocale: culture || "hu",
            uiTranslations,
            strictMode: true,
            dropdownParent: document.body
        });

        await instance.promise;
        instance.setNumber(buildNumber(countryCode, number));
        instance.setDisabled(disabled);

        const notifyChanged = () => {
            const value = readValue(input, instance);
            dotNetReference.invokeMethodAsync(
                "HandlePhoneInputChangedAsync",
                value.countryCode,
                value.number);
        };

        input.addEventListener("input", notifyChanged);
        input.addEventListener("countrychange", notifyChanged);
        instances.set(input, {
            instance,
            notifyChanged
        });
    }

    function setValue(input, countryCode, number) {
        const entry = instances.get(input);

        if (entry !== undefined)
            entry.instance.setNumber(buildNumber(countryCode, number));
    }

    function setDisabled(input, disabled) {
        const entry = instances.get(input);

        if (entry !== undefined)
            entry.instance.setDisabled(disabled);
    }

    function destroy(input) {
        const entry = instances.get(input);

        if (entry === undefined)
            return;

        input.removeEventListener("input", entry.notifyChanged);
        input.removeEventListener("countrychange", entry.notifyChanged);
        entry.instance.destroy();
        instances.delete(input);
    }

    return {
        initialize,
        setValue,
        setDisabled,
        destroy
    };
})();
