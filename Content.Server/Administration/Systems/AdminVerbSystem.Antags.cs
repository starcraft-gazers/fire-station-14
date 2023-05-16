using Content.Server.GameTicking.Rules;
using Content.Server.Mind.Components;
using Content.Server.Roles;
using Content.Server.Zombies;
using Content.Shared.Administration;
using Content.Shared.Database;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Utility;

namespace Content.Server.Administration.Systems;

public sealed partial class AdminVerbSystem
{
    [Dependency] private readonly ZombifyOnDeathSystem _zombify = default!;
    [Dependency] private readonly TraitorRuleSystem _traitorRule = default!;
    [Dependency] private readonly NukeopsRuleSystem _nukeopsRule = default!;
    [Dependency] private readonly PiratesRuleSystem _piratesRule = default!;
    [Dependency] private readonly IRevolutionaryMaker _revolSystem = default!;

    // All antag verbs have names so invokeverb works.
    private void AddAntagVerbs(GetVerbsEvent<Verb> args)
    {
        if (!EntityManager.TryGetComponent<ActorComponent?>(args.User, out var actor))
            return;

        var player = actor.PlayerSession;

        if (!_adminManager.HasAdminFlag(player, AdminFlags.Fun))
            return;

        var targetHasMind = TryComp(args.Target, out MindComponent? targetMindComp);
        if (!targetHasMind || targetMindComp == null)
            return;

        Verb traitor = new()
        {
            Text = "Make Traitor",
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/Structures/Wallmounts/posters.rsi"), "poster5_contraband"),
            Act = () =>
            {
                if (targetMindComp.Mind == null || targetMindComp.Mind.Session == null)
                    return;

                _traitorRule.MakeTraitor(targetMindComp.Mind.Session);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-traitor"),
        };
        args.Verbs.Add(traitor);

        Verb zombie = new()
        {
            Text = "Make Zombie",
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Structures/Wallmounts/signs.rsi"), "bio"),
            Act = () =>
            {
                _zombify.ZombifyEntity(args.Target);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-zombie"),
        };
        args.Verbs.Add(zombie);


        Verb nukeOp = new()
        {
            Text = "Make nuclear operative",
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Structures/Wallmounts/signs.rsi"), "radiation"),
            Act = () =>
            {
                if (targetMindComp.Mind == null || targetMindComp.Mind.Session == null)
                    return;

                _nukeopsRule.MakeLoneNukie(targetMindComp.Mind);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-nuclear-operative"),
        };
        args.Verbs.Add(nukeOp);

        Verb pirate = new()
        {
            Text = "Make Pirate",
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Clothing/Head/Hats/pirate.rsi"), "icon"),
            Act = () =>
            {
                if (targetMindComp.Mind == null || targetMindComp.Mind.Session == null)
                    return;

                _piratesRule.MakePirate(targetMindComp.Mind);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-pirate"),
        };
        args.Verbs.Add(pirate);

        Verb revol = new()
        {
            Text = "Сделать революционером",
            Category = VerbCategory.Antag,
            Icon = null,
            Act = () =>
            {
                if (targetMindComp.Mind == null || targetMindComp.Mind.Session == null)
                    return;

                _revolSystem.MakeRevolutionary(targetMindComp.Mind.Session);
            },
            Impact = LogImpact.High,
            Message = "Делает цель командиром революционеров, не меняет игровой режим. Необходимо включить режим революции перед этим"
        };
        args.Verbs.Add(revol);
    }
}
