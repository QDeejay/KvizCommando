using KvizCommando.Shared.Models.Rules;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Components.CharacterComponent;

public partial class CharacterVitalityBar
{
    [Parameter] public int Current { get; set; }
    [Parameter] public int Level { get; set; }
    [Parameter] public int CharacterPosition { get; set; }
    [Parameter] public string Label { get; set; } = string.Empty;
    [Parameter] public bool ShowValue { get; set; } = true;
    [Parameter] public bool Compact { get; set; }

    private int _previousCurrent = -1;
    private int _previousCharacterPosition = -1;
    private bool _useAlternateDamageAnimation;
    private string _damageCssClass = string.Empty;

    private int DisplayedCurrent =>
        Math.Max(Current, 0);

    private int VitalityMaximum =>
        TeamRules.GetMemberMaxVitality(Level);

    private string VitalityWidth =>
        $"width: {TeamRules.GetMemberVitalityPercent(Current, Level)}%";

    private string VitalityCssClass =>
        TeamRules.GetMemberVitalityState(Current, Level)
            .ToString()
            .ToLowerInvariant();

    protected override void OnParametersSet()
    {
        var isSameCharacter =
            _previousCharacterPosition == CharacterPosition;

        if (isSameCharacter &&
            _previousCurrent >= 0 &&
            Current < _previousCurrent)
        {
            _useAlternateDamageAnimation =
                !_useAlternateDamageAnimation;
            _damageCssClass = _useAlternateDamageAnimation
                ? "damage-a"
                : "damage-b";
        }
        else if (!isSameCharacter)
        {
            _damageCssClass = string.Empty;
        }

        _previousCurrent = Current;
        _previousCharacterPosition = CharacterPosition;
    }
}
