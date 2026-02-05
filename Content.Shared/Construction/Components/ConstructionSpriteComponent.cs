// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Construction.Components;

/// <summary>
/// Component for storing sprite modification data applied during construction.
/// This allows the client to synchronize and apply sprite changes.
/// </summary>
[RegisterComponent]
[NetworkedComponent]
public sealed partial class ConstructionSpriteComponent : Component
{
    /// <summary>
    /// The sprite RSI path to apply.
    /// </summary>
    [DataField("sprite", required: true)]
    public string Sprite = string.Empty;

    /// <summary>
    /// The sprite state to apply.
    /// </summary>
    [DataField("state")]
    public string? State;

    /// <summary>
    /// The layer index to modify. Defaults to 0.
    /// </summary>
    [DataField("layer")]
    public int Layer = 0;

}

[Serializable, NetSerializable]
public sealed class ConstructionSpriteComponentState : ComponentState
{
    public string Sprite { get; set; } = string.Empty;
    public string? State { get; set; }
    public int Layer { get; set; } = 0;

    public ConstructionSpriteComponentState() { }

    public ConstructionSpriteComponentState(string sprite, string? state, int layer)
    {
        Sprite = sprite;
        State = state;
        Layer = layer;
    }
}
