// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Construction;
using Content.Shared.Construction.Components;
using JetBrains.Annotations;

namespace Content.Server.Construction.Completions;

/// <summary>
/// Construction completion action that applies sprite modifications.
/// The actual sprite changes are applied client-side through ConstructionSpriteComponent synchronization.
/// </summary>
[UsedImplicitly]
[DataDefinition]
public sealed partial class ModifySprite : IGraphAction
{
    /// <summary>
    /// The sprite path to set on the entity.
    /// </summary>
    [DataField("sprite", required: true)]
    public string Sprite = string.Empty;

    /// <summary>
    /// The sprite state to set on the entity.
    /// </summary>
    [DataField("state")]
    public string? State;

    /// <summary>
    /// The sprite layer to modify. If not specified, uses the default layer (0).
    /// </summary>
    [DataField("layer")]
    public int Layer = 0;

    public void PerformAction(EntityUid uid, EntityUid? userUid, IEntityManager entityManager)
    {
        // If the entity already has the component, update and dirty it.
        if (entityManager.HasComponent<ConstructionSpriteComponent>(uid))
        {
            var spriteComponent = entityManager.GetComponent<ConstructionSpriteComponent>(uid);
            spriteComponent.Sprite = Sprite;
            spriteComponent.State = State;
            spriteComponent.Layer = Layer;
            entityManager.Dirty(uid, spriteComponent);
        }
        else
        {
            // Create a new component instance with the desired data and add it in one operation
            var comp = new ConstructionSpriteComponent
            {
                Sprite = Sprite,
                State = State,
                Layer = Layer,
            };

            entityManager.AddComponent(uid, (Component)comp, overwrite: true);
            // Ensure the freshly added component is marked dirty so its data is sent to clients immediately
            entityManager.Dirty(uid, comp);
        }

        Logger.Info($"ModifySprite: Applied sprite {Sprite} state {State} to entity {uid}");
    }
}
