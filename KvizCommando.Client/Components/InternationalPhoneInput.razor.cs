using KvizCommando.Client.Services.Audio;
using KvizCommando.Shared.Contracts.Profile;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace KvizCommando.Client.Components;

public partial class InternationalPhoneInput : IAsyncDisposable
{
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private AudioService Audio { get; set; } = default!;

    [Parameter, EditorRequired]
    public ProfilePhoneDto Value { get; set; } = new();

    [Parameter]
    public EventCallback<ProfilePhoneDto> ValueChanged { get; set; }

    [Parameter]
    public string Culture { get; set; } = "hu";

    [Parameter]
    public bool Disabled { get; set; }

    private ElementReference _input;
    private DotNetObjectReference<InternationalPhoneInput>? _objectReference;
    private string _renderedCountryCode = string.Empty;
    private string _renderedNumber = string.Empty;
    private bool _renderedDisabled;
    private bool _isInitialized;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_isInitialized)
        {
            _objectReference = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync(
                "phoneInputInterop.initialize",
                _input,
                _objectReference,
                Value.CountryCode,
                Value.Number,
                Culture,
                Disabled);

            _renderedCountryCode = Value.CountryCode;
            _renderedNumber = Value.Number;
            _renderedDisabled = Disabled;
            _isInitialized = true;
            return;
        }

        if (_renderedCountryCode != Value.CountryCode ||
            _renderedNumber != Value.Number)
        {
            await JS.InvokeVoidAsync(
                "phoneInputInterop.setValue",
                _input,
                Value.CountryCode,
                Value.Number);
            _renderedCountryCode = Value.CountryCode;
            _renderedNumber = Value.Number;
        }

        if (_renderedDisabled != Disabled)
        {
            await JS.InvokeVoidAsync(
                "phoneInputInterop.setDisabled",
                _input,
                Disabled);
            _renderedDisabled = Disabled;
        }
    }

    /// <summary>
    /// Fogadja a telefonszámmező ország- vagy számváltozását a böngészőoldali vezérlőtől.
    /// </summary>
    /// <param name="countryCode">A kiválasztott ország nemzetközi előhívója.</param>
    /// <param name="number">Az előhívó nélküli telefonszám.</param>
    [JSInvokable]
    public async Task HandlePhoneInputChangedAsync(
        string countryCode,
        string number)
    {
        if (_renderedCountryCode != countryCode)
            await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);

        _renderedCountryCode = countryCode;
        _renderedNumber = number;

        await ValueChanged.InvokeAsync(new ProfilePhoneDto
        {
            CountryCode = countryCode,
            Number = number
        });
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_isInitialized)
        {
            try
            {
                await JS.InvokeVoidAsync(
                    "phoneInputInterop.destroy",
                    _input);
            }
            catch (JSDisconnectedException)
            {
            }
        }

        _objectReference?.Dispose();
        GC.SuppressFinalize(this);
    }
}
