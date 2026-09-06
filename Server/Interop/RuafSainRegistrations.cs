using MoreBotsServer.Interop;
using MoreBotsServer.Models;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;

namespace RUAFComeHomeServer.Interop;

[Injectable(TypePriority = OnLoadOrder.Preload + 2)]
public sealed class RuafSainRegistrations(SainInteropRegistration sainInterop) : IOnLoad
{
    private const int RiflemanId = 848400;
    private const int SeniorRiflemanId = 848401;
    private const int AutoriflemanId = 848402;
    private const int GrenadierId = 848403;
    private const int MarksmanId = 848404;
    private const int MachinegunnerId = 848405;
    private const int RemnantRiflemanId = 848406;

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        RegisterRuaf();
        RegisterRemnant();

        return Task.CompletedTask;
    }

    private void RegisterRuaf()
    {
        sainInterop.RegisterBotType(CreateRegistration(
            wildSpawnType: RiflemanId,
            botDbKey: "ruafRifleman",
            name: "RUAF Rifleman",
            description: "A regular rifleman.",
            section: "RUAF",
            difficultyModifier: 0.5f));

        sainInterop.RegisterBotType(CreateRegistration(
            wildSpawnType: SeniorRiflemanId,
            botDbKey: "ruafRiflemanSenior",
            name: "RUAF Senior Rifleman",
            description: "A NCO rifleman with better rifles.",
            section: "RUAF",
            difficultyModifier: 0.66f));

        sainInterop.RegisterBotType(CreateRegistration(
            wildSpawnType: AutoriflemanId,
            botDbKey: "ruafAutorifleman",
            name: "RUAF Autorifleman",
            description: "A regular equipped with a SAW/LMG.",
            section: "RUAF",
            difficultyModifier: 0.5f));

        sainInterop.RegisterBotType(CreateRegistration(
            wildSpawnType: GrenadierId,
            botDbKey: "ruafGrenadier",
            name: "RUAF Grenadier",
            description: "A grenadier equipped with a grenade launcher.",
            section: "RUAF",
            difficultyModifier: 0.5f));

        sainInterop.RegisterBotType(CreateRegistration(
            wildSpawnType: MarksmanId,
            botDbKey: "ruafMarksman",
            name: "RUAF Marksman",
            description: "A marksman equipped with a DMR.",
            section: "RUAF",
            difficultyModifier: 0.5f));

        sainInterop.RegisterBotType(CreateRegistration(
            wildSpawnType: MachinegunnerId,
            botDbKey: "ruafMachinegunner",
            name: "RUAF Machinegunner",
            description: "A machinegunner equipped with a MMG.",
            section: "RUAF",
            difficultyModifier: 0.5f));
    }

    private void RegisterRemnant()
    {
        sainInterop.RegisterBotType(CreateRegistration(
            wildSpawnType: RemnantRiflemanId,
            botDbKey: "remnantRifleman",
            name: "Remnant Rifleman",
            description: "Russian SOF remnant equipped with specialized assault rifles.",
            section: "Remnant",
            difficultyModifier: 0.7f));
    }

    private static MoreBotsSainBotTypeRegistration CreateRegistration(
        int wildSpawnType,
        string botDbKey,
        string name,
        string description,
        string section,
        float difficultyModifier)
    {
        return new MoreBotsSainBotTypeRegistration
        {
            WildSpawnType = wildSpawnType,
            BotDbKey = botDbKey,
            Name = name,
            Description = description,
            Section = section,
            DifficultyModifier = difficultyModifier,

            BrainsToApply =
            [
                "PMC",
                "ExUsec",
            ],

            LayersToRemove =
            [
                "Request",
                "KnightFight",
                "PmcBear",
                "PmcUsec",
                "ExURequest",
                "StationaryWS",
                "Utility peace",
            ],
        };
    }
}