// SPDX-License-Identifier: AGPL-3.0-or-later
using System.Diagnostics.CodeAnalysis;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Objectives;
using Content.Server.Traitor.Components;
using Content.Shared.Paper;
using Robust.Shared.Random;
using Content.Server.Antag;
using Content.Server.Codewords;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Server.Traitor.Systems;

public sealed class AgentTaskPaperSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly PaperSystem _paper = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly ObjectivesSystem _objectives = default!;
    [Dependency] private readonly CodewordSystem _codewordSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AgentTaskPaperComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, AgentTaskPaperComponent component, MapInitEvent args)
    {
        SetupPaper(uid, component);
    }

    private void SetupPaper(EntityUid uid, AgentTaskPaperComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!TryComp(uid, out PaperComponent? paperComp))
            return;

        // Decide whether to show codeword or agent task
        var showCode = _random.Prob(component.CodewordChance);

        if (showCode)
        {
            if (TryGetCodewords(component, out var content))
            {
                _paper.SetContent((uid, paperComp), content);
                return;
            }
        }

        // Try agent tasks mode
        if (TryGetAgentTasks(out var taskContent))
        {
            _paper.SetContent((uid, paperComp), taskContent);
            return;
        }

        // Fallback to codewords if nothing else
        if (TryGetCodewords(component, out var fallback))
            _paper.SetContent((uid, paperComp), fallback);
    }

    private bool TryGetCodewords(AgentTaskPaperComponent component, [NotNullWhen(true)] out string? outStr)
    {
        outStr = null;
        var codesMessage = new FormattedMessage();
        var codeList = _codewordSystem.GetCodewords(component.CodewordFaction).ToList();

        if (codeList.Count == 0)
        {
            if (component.FakeCodewords)
                codeList = _codewordSystem.GenerateCodewords(component.CodewordGenerator).ToList();
            else
                codeList = new List<string> { Loc.GetString("traitor-codes-none") };
        }

        _random.Shuffle(codeList);

        int i = 0;
        foreach (var code in codeList)
        {
            i++;
            if (i > component.CodewordAmount && !component.CodewordShowAll)
                break;

            codesMessage.PushNewline();
            codesMessage.AddMarkupOrThrow(code);
        }

        if (!codesMessage.IsEmpty)
        {
            if (i == 1)
                outStr = Loc.GetString("traitor-codes-message-singular") + codesMessage;
            else
                outStr = Loc.GetString("traitor-codes-message-plural") + codesMessage;
        }

        return !codesMessage.IsEmpty;
    }

    private bool TryGetAgentTasks([NotNullWhen(true)] out string? outStr)
    {
        outStr = null;

        // Gather all antag minds across traitor-like rules
        var antagMinds = new List<(EntityUid mindUid, Content.Shared.Mind.MindComponent mindComp)>();
        var query = EntityQueryEnumerator<TraitorRuleComponent>();
        while (query.MoveNext(out var ruleUid, out var traitorRule))
        {
            var minds = _antag.GetAntagMindEntityUids(ruleUid);
            foreach (var mindEnt in minds)
            {
                if (TryComp(mindEnt, out Content.Shared.Mind.MindComponent? mc))
                    antagMinds.Add((mindEnt, mc));
            }
        }

        var totalAgents = antagMinds.Count;
        if (totalAgents == 0)
            return false;

        // Decide how many tasks to show
        var tasksToShow = 1;
        if (totalAgents > 7)
            tasksToShow = 3;
        else if (totalAgents > 3)
            tasksToShow = 2;

        // Collect all objectives from these minds
        var allObjectives = new List<(EntityUid objectiveEntity, EntityUid mindUid, Content.Shared.Mind.MindComponent mindComp)>();

        foreach (var (mindUid, mindComp) in antagMinds)
        {
            foreach (var obj in mindComp.Objectives)
            {
                // 1. Пытаемся получить MetaData сущности цели
                if (!EntityManager.TryGetComponent<MetaDataComponent>(obj, out var meta))
                    continue;

                // 2. Достаем ID прототипа из метаданных
                var protoId = meta.EntityPrototype?.ID;

                // Список исключений: "Сбежать", "Спасти", "Помочь"
                if (protoId == "EscapeShuttleObjective" ||
                    protoId == "RandomTraitorAliveObjective" ||
                    protoId == "RandomTraitorProgressObjective")
                {
                    continue;
                }

                allObjectives.Add((obj, mindUid, mindComp));
            }
        }

        if (allObjectives.Count == 0)
            return false;

        // Shuffle and pick unique objective titles
        _random.Shuffle(allObjectives);

        var picked = new List<string>();
        foreach (var (objEnt, mindUid, mindComp) in allObjectives)
        {
            var info = _objectives.GetInfo(objEnt, mindUid, mindComp);
            if (info == null)
                continue;

            var title = info.Value.Title;
            if (picked.Contains(title))
                continue;

            picked.Add(title);
            if (picked.Count >= tasksToShow)
                break;
        }

        if (picked.Count == 0)
            return false;

        var fm = new FormattedMessage();
        fm.AddMarkup(Loc.GetString("agent-tasks-header"));
        foreach (var t in picked)
        {
            fm.PushNewline();
            fm.AddMarkupOrThrow("- " + t);
        }

        outStr = fm.ToString();
        return true;
    }
}
