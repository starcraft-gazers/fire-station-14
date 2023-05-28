// using System.Linq;
// using Content.Server.Administration;
// using Content.Server.Database;
// using Content.Shared.Administration;
// using Robust.Server.Player;
// using Robust.Shared.Console;
// using Robust.Shared.IoC;
// using Robust.Shared.Localization;
// using Robust.Shared.Prototypes;

// namespace Content.Server._Craft.Patron.Roles;

// [AdminCommand(AdminFlags.Host)]
// public sealed class PatronAddRoleCommand : IConsoleCommand
// {
//     public string Command => "addpatronrole";
//     public string Description => "Добавляет роль для патрона";
//     public string Help => "addpatronrole <UserLogin> <RolePrototype>";

//     public async void Execute(IConsoleShell shell, string argStr, string[] args)
//     {
//         var dbManager = IoCManager.Resolve<IServerDbManager>();
//         var protoManager = IoCManager.Resolve<IPrototypeManager>();
//         var locator = IoCManager.Resolve<IPlayerLocator>();

//         if (args.Length < 2)
//         {
//             shell.WriteLine($"Invalid ammount of args: {Help}");
//             return;
//         }

//         string target = args[0];
//         string roleId = args[1];

//         if (!protoManager.HasIndex<EntityPrototype>(roleId))
//         {
//             shell.WriteLine("Can't find prototype.");
//             return;
//         }

//         var located = await locator.LookupIdByNameAsync(target);
//         if (located == null)
//         {
//             shell.WriteError("Can't find player");
//             return;
//         }

//         var userId = located.UserId;

//         if (!await dbManager.IsInPatronlistAsync(userId))
//         {
//             shell.WriteError("User not patron");
//             return;
//         }
//     }

//     public CompletionResult GetComplection(IConsoleShell shell, string[] args)
//     {
//         if (args.Length == 1)
//         {
//             var playerMgr = IoCManager.Resolve<IPlayerManager>();
//             var options = playerMgr.ServerSessions.Select(c => c.Name).OrderBy(c => c).ToArray();
//             return CompletionResult.FromHintOptions(options, Loc.GetString("cmd-patron-hint-player"));
//         }

//         return CompletionResult.Empty;
//     }
// }

// [AdminCommand(AdminFlags.None)]
// public sealed class ForceRolePlay : IConsoleCommand
// {
//     public string Command => "removepatronrole";
//     public string Description => "Удаляет патрону роль";
//     public string Help => "removepatronrole <UserLogin> <RolePrototype>";

//     public async void Execute(IConsoleShell shell, string argStr, string[] args)
//     {
//         var dbManager = IoCManager.Resolve<IServerDbManager>();
//         var protoManager = IoCManager.Resolve<IPrototypeManager>();
//         var locator = IoCManager.Resolve<IPlayerLocator>();

//         if (args.Length < 2)
//         {
//             shell.WriteLine($"Invalid ammount of args: {Help}");
//             return;
//         }

//         string target = args[0];
//         string roleId = args[1];

//         if (!protoManager.HasIndex<EntityPrototype>(roleId))
//         {
//             shell.WriteLine("Can't find prototype.");
//             return;
//         }

//         var located = await locator.LookupIdByNameAsync(target);
//         if (located == null)
//         {
//             shell.WriteError("Can't find player");
//             return;
//         }

//         var userId = located.UserId;
//         if (!await dbManager.IsInPatronlistAsync(userId))
//         {
//             shell.WriteError("User not patron");
//             return;
//         }

//         // await dbManager.RemovePatronRoleAsync(userId, roleId);
//     }
// }

// [AdminCommand(AdminFlags.Host)]
// public sealed class PatronRemoveRoleCommand : IConsoleCommand
// {
//     public string Command => "removepatronrole";
//     public string Description => "Удаляет патрону роль";
//     public string Help => "removepatronrole <UserLogin> <RolePrototype>";

//     public async void Execute(IConsoleShell shell, string argStr, string[] args)
//     {
//         var dbManager = IoCManager.Resolve<IServerDbManager>();
//         var protoManager = IoCManager.Resolve<IPrototypeManager>();
//         var locator = IoCManager.Resolve<IPlayerLocator>();

//         if (args.Length < 2)
//         {
//             shell.WriteLine($"Invalid ammount of args: {Help}");
//             return;
//         }

//         string target = args[0];
//         string roleId = args[1];

//         if (!protoManager.HasIndex<EntityPrototype>(roleId))
//         {
//             shell.WriteLine("Can't find prototype.");
//             return;
//         }

//         var located = await locator.LookupIdByNameAsync(target);
//         if (located == null)
//         {
//             shell.WriteError("Can't find player");
//             return;
//         }

//         var userId = located.UserId;
//         if (!await dbManager.IsInPatronlistAsync(userId))
//         {
//             shell.WriteError("User not patron");
//             return;
//         }

//         // await dbManager.RemovePatronRoleAsync(userId, roleId);
//     }

//     public CompletionResult GetComplection(IConsoleShell shell, string[] args)
//     {
//         if (args.Length == 1)
//         {
//             var playerMgr = IoCManager.Resolve<IPlayerManager>();
//             var options = playerMgr.ServerSessions.Select(c => c.Name).OrderBy(c => c).ToArray();
//             return CompletionResult.FromHintOptions(options, Loc.GetString("cmd-patron-hint-player"));
//         }

//         return CompletionResult.Empty;
//     }
// }
