using Content.Shared.Actions;

namespace Content.Server.Roles.SCP.PlagueDoctor;

public sealed class PlagueDoctorHealEvent : EntityTargetActionEvent { }

public sealed class PlagueDoctorZombieEvent : EntityTargetActionEvent { }

public sealed class PlagueDoctorUnZombieEvent : EntityTargetActionEvent { }

public sealed class PlagueDoctorOpenDoorEvent : EntityTargetActionEvent { }
