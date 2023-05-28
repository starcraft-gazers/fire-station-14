using System;
using System.Collections.Generic;
using System.Linq;
using Content.Server.Fax;
using Content.Shared.GameTicking;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Roles.SCP.Science;

//Быстренько запилил, буду переписывать в нормальный вид
public sealed class SCPStationGoalSystem : EntitySystem
{
    [Dependency] private readonly FaxSystem _faxSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _robustRandom = default!;

    private float Elapsed = -1f;
    private float ElapsedStation = -1f;
    private float NextTaskDelay = -1f;

    private float NextStationTaskDelay = -1f;

    private readonly List<string> HardcodedTasks = new List<string>
    {
        "Доставить SCP-173 на основную станцию и проверить на воздействие различными частицами (М.А.К.А.К.)",
        "Доставить SCP-173 на основную станцию к источнику радиации на 1.5 минуты",
        "Дождаться наличие преступника в пермабриге и доставить его в камеру к SCP-049, чтобы он сделал его слугой",
        "Доставить SCP-049 на основную станцию и попросить его вылечить двух раненых или больных",
        "Доставить SCP-049 на основную станцию для прогулки. 10-20 минут",
        "Доставить Странное мыло на основную станцию и выпустить на самостоятельную прогулку 1 час",
        "Доставить Странное мыло на основную станцию и проверить на воздействие различными частицами (М.А.К.А.К.)",
        "Провести медицинское обследование SCP-049",
    };

    private List<string> StationTasks = default!;
    private readonly List<string> HardCodedStationTasks = new List<string>
    {
        "Провести патруль вокруг станции в космосе",
        "Установить камеры наблюдения в камере содержания SCP-173",
        "Установить камеры наблюдения в камере содержания SCP-049",
        "Провести уборку в камере содержания SCP-173",
        "Вывести Д-Класс на обед (не более двух человек за раз)",
        "Проверить ДАМ на шаттле и в отделе SCP",
        "Пополнить запасы еды, воды и медикаментов в отделе SCP"
    };
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartedEvent>(OnRoundStarted);
        SubscribeLocalEvent<RoundEndedEvent>(OnRoundEnded);

        StationTasks = HardcodedTasks.ToList();
    }

    private void OnRoundEnded(RoundEndedEvent ev)
    {
        Elapsed = -1;
        ElapsedStation = -1;

        StationTasks = HardcodedTasks.ToList();
    }

    private void OnRoundStarted(RoundStartedEvent ev)
    {
        Elapsed = 0f;
        ElapsedStation = 0f;
        NextTaskDelay = 500f;
        NextStationTaskDelay = 120f;

        _robustRandom.Shuffle(StationTasks);
    }

    public override void Update(float frameTime)
    {
        if (Elapsed == -1)
            return;

        Elapsed += frameTime;
        ElapsedStation += frameTime;

        if (Elapsed >= NextTaskDelay)
        {
            PrintNextTask();
            Elapsed = 0f;
            NextTaskDelay = _robustRandom.NextFloat(900, 1200);
        }

        if (ElapsedStation >= NextStationTaskDelay)
        {
            PrintNextStationTask();
            ElapsedStation = 0f;
            NextStationTaskDelay = _robustRandom.NextFloat(600, 900);
        }
    }

    private void PrintNextStationTask()
    {
        _robustRandom.Shuffle(HardCodedStationTasks);

        var randomTask = _robustRandom.Pick(HardCodedStationTasks);
        var faxes = EntityQuery<FaxMachineComponent>();

        foreach (var fax in faxes)
        {
            if (!fax.ReceiveSCPGoal)
                continue;

            var printout = new FaxPrintout(
                randomTask,
                "Задание отделу SCP",
                null,
                "paper_stamp-cent",
                new() { Loc.GetString("stamp-component-stamped-name-centcom") });
            _faxSystem.Receive(fax.Owner, printout, null, fax);
        }
    }
    private void PrintNextTask()
    {
        var randomTask = _robustRandom.PickAndTake(StationTasks);
        var faxes = EntityQuery<FaxMachineComponent>();

        foreach (var fax in faxes)
        {
            if (!fax.ReceiveSCPGoal)
                continue;

            var printout = new FaxPrintout(
                randomTask,
                "Задание отделу SCP",
                null,
                "paper_stamp-cent",
                new() { Loc.GetString("stamp-component-stamped-name-centcom") });
            _faxSystem.Receive(fax.Owner, printout, null, fax);
        }

        if (StationTasks.Count() == 0)
            StationTasks = HardcodedTasks.ToList();
    }
}
