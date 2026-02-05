// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Construction.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Client.Construction;

/// <summary>
/// Client-side system for applying sprite modifications from ConstructionSpriteComponent.
/// This handles the visual updates when construction sprites are modified.
/// </summary>
public sealed class ConstructionSpriteSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _spriteSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        // Only subscribe to ComponentHandleState, not ComponentAdd.
        // ComponentAdd fires with empty defaults before Dirty() sends the real state.
        SubscribeLocalEvent<ConstructionSpriteComponent, ComponentHandleState>(OnComponentHandleState);
    }

    private void OnComponentHandleState(EntityUid uid, ConstructionSpriteComponent component, ref ComponentHandleState args)
    {
        // Prefer args.Next (upcoming state), fallback to args.Current (initial state)
        var state = args.Next as ConstructionSpriteComponentState ?? args.Current as ConstructionSpriteComponentState;

        if (state is null)
        {
            Logger.Info($"ConstructionSpriteSystem: Synced state for {uid}, no state payload");
            return;
        }

        Logger.Info($"ConstructionSpriteSystem: Synced state for {uid}, sprite={state.Sprite}, state={state.State}");

        // Update the local component so other client code can read it if needed
        component.Sprite = state.Sprite;
        component.State = state.State;
        component.Layer = state.Layer;

        ApplySpriteModification(uid, component);
    }

    /// <summary>
    /// Applies the sprite modification from the component to the entity's SpriteComponent.
    /// </summary>
    private void ApplySpriteModification(EntityUid uid, ConstructionSpriteComponent component)
    {
        if (string.IsNullOrEmpty(component.Sprite))
        {
            Logger.Warning($"ConstructionSpriteSystem: Empty sprite path for {uid}");
            return;
        }

        if (!TryComp<SpriteComponent>(uid, out var sprite))
        {
            Logger.Warning($"ConstructionSpriteSystem: Entity {uid} has no SpriteComponent!");
            return;
        }

        var rsiPath = new ResPath(component.Sprite);
        RSI.StateId? stateId = string.IsNullOrEmpty(component.State) ? null : new RSI.StateId(component.State);

        Logger.Info($"ConstructionSpriteSystem: Applying sprite {rsiPath} state {component.State} to layer {component.Layer}");
        
        // Set the sprite RSI and state on the specified layer
        _spriteSystem.LayerSetRsi((uid, sprite), component.Layer, rsiPath, stateId);
        
        Logger.Info($"ConstructionSpriteSystem: Applied successfully");
    }
}
