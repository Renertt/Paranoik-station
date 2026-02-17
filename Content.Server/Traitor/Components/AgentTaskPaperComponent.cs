// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Server.Codewords;
using Robust.Shared.Prototypes;

namespace Content.Server.Traitor.Components;

[RegisterComponent]
public sealed partial class AgentTaskPaperComponent : Component
{
    // Faction & generator to use when this paper shows a codeword
    [DataField]
    public ProtoId<CodewordFactionPrototype> CodewordFaction = "Traitor";

    [DataField]
    public ProtoId<CodewordGeneratorPrototype> CodewordGenerator = "TraitorCodewordGenerator";

    [DataField]
    public int CodewordAmount = 1;

    [DataField]
    public bool FakeCodewords = true;

    [DataField]
    public bool CodewordShowAll = false;

    // Chance to choose the codeword variant instead of agent-task variant (0..1)
    [DataField]
    public float CodewordChance = 0.5f;
}
