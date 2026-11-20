using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using System.Text.Json.Serialization;

namespace Parachute;

public class ConfigGen : BasePluginConfig
{
    [JsonPropertyName("Enabled")] public bool Enabled { get; set; } = true;
    [JsonPropertyName("DecreaseVec")] public float DecreaseVec { get; set; } = 50;
    [JsonPropertyName("Linear")] public bool Linear { get; set; } = true;
    [JsonPropertyName("FallSpeed")] public float FallSpeed { get; set; } = 100;
    [JsonPropertyName("AccessFlag")] public string AccessFlag { get; set; } = "";
    [JsonPropertyName("TeleportTicks")] public int TeleportTicks { get; set; } = 300;
    [JsonPropertyName("ParachuteModelEnabled")] public bool ParachuteModelEnabled { get; set; } = true;
    [JsonPropertyName("ParachuteModel")] public string ParachuteModel { get; set; } = "models/props_survival/parachute/chute.vmdl";
}

[MinimumApiVersion(358)]
public class Parachute : BasePlugin, IPluginConfig<ConfigGen>
{
    public override string ModuleName => "CS2 Parachute";
    public override string ModuleAuthor => "ddbmaster";
    public override string ModuleVersion => "1.6.1";

    // Wie oft (in Ticks) waehrend des Gleitens das Zugriffsrecht erneut geprueft wird
    private const int PermissionRecheckTicks = 64;

    public ConfigGen Config { get; set; } = null!;
    public void OnConfigParsed(ConfigGen config) => Config = config;

    private sealed class ParaState
    {
        public bool UsingPara;
        public int TeleportTicks;
        public int PermissionTicks;
        // Handle statt Objekt-Referenz: wird nach Runden-/Mapwechsel sicher ungueltig
        public CHandle<CDynamicProp>? Model;
    }

    private readonly Dictionary<int, ParaState> _states = new();

    private ParaState GetState(CCSPlayerController player)
    {
        var idx = (int)player.Index;
        if (!_states.TryGetValue(idx, out var state))
        {
            state = new ParaState();
            _states[idx] = state;
        }
        return state;
    }

    private bool HasAccess(CCSPlayerController player) =>
        string.IsNullOrEmpty(Config.AccessFlag) || AdminManager.PlayerHasPermissions(player, Config.AccessFlag);

    public override void Load(bool hotReload)
    {
        if (!Config.Enabled)
        {
            Console.WriteLine("[Parachute] Plugin not enabled!");
            return;
        }

        if (Config.ParachuteModelEnabled)
        {
            RegisterListener<Listeners.OnServerPrecacheResources>(manifest =>
            {
                manifest.AddResource(Config.ParachuteModel);
            });
        }

        // Map-/Rundenwechsel: Engine raeumt die Props weg, Zustand zuruecksetzen
        RegisterListener<Listeners.OnMapStart>(_ => _states.Clear());
        RegisterListener<Listeners.OnMapEnd>(() => _states.Clear());

        RegisterEventHandler<EventRoundStart>((@event, info) =>
        {
            StopAll();
            return HookResult.Continue;
        });

        RegisterEventHandler<EventPlayerConnectFull>((@event, info) =>
        {
            var player = @event.Userid;
            if (player == null || !player.IsValid || player.IsBot)
                return HookResult.Continue;

            ResetPlayer(player);
            return HookResult.Continue;
        });

        RegisterEventHandler<EventPlayerDisconnect>((@event, info) =>
        {
            var player = @event.Userid;
            if (player == null || !player.IsValid || player.IsBot)
                return HookResult.Continue;

            ResetPlayer(player);
            return HookResult.Continue;
        });

        RegisterEventHandler<EventPlayerDeath>((@event, info) =>
        {
            var player = @event.Userid;
            if (player == null || !player.IsValid)
                return HookResult.Continue;

            if (_states.TryGetValue((int)player.Index, out var state) && state.UsingPara)
                StopPara(state);

            return HookResult.Continue;
        }, HookMode.Pre);

        RegisterListener<Listeners.OnTick>(OnTick);
    }

    public override void Unload(bool hotReload) => StopAll();

    // Handles sind nach Runden-/Mapwechsel sicher ungueltig, Remove greift dann nicht ins Leere
    private void StopAll()
    {
        foreach (var state in _states.Values)
            StopPara(state);

        _states.Clear();
    }

    private void OnTick()
    {
        foreach (var player in Utilities.GetPlayers())
        {
            if (player == null || !player.IsValid || player.IsBot)
                continue;

            var pawn = player.PlayerPawn.Value;
            var alive = player.PawnIsAlive && pawn != null && pawn.IsValid;

            // Nur normales Laufen/Fallen: keine Leiter, kein Noclip, kein Wasser usw.
            var wantsPara = alive
                && (player.Buttons & PlayerButtons.Use) != 0
                && !pawn!.OnGroundLastTick
                && pawn.MoveType == MoveType_t.MOVETYPE_WALK;

            if (!_states.TryGetValue((int)player.Index, out var state))
            {
                if (!wantsPara)
                    continue;

                state = GetState(player);
            }

            if (!wantsPara)
            {
                if (state.UsingPara)
                    StopPara(state);
                continue;
            }

            if (!state.UsingPara)
            {
                if (!HasAccess(player))
                    continue;

                StartPara(state);
            }
            else if (++state.PermissionTicks >= PermissionRecheckTicks)
            {
                state.PermissionTicks = 0;
                if (!HasAccess(player))
                {
                    StopPara(state);
                    continue;
                }
            }

            UpdatePara(state, pawn!);
        }
    }

    private void ResetPlayer(CCSPlayerController player)
    {
        var idx = (int)player.Index;
        if (_states.TryGetValue(idx, out var state))
            StopPara(state);

        _states[idx] = new ParaState();
    }

    private void StartPara(ParaState state)
    {
        state.UsingPara = true;
        state.TeleportTicks = 0;
        state.PermissionTicks = 0;

        if (!Config.ParachuteModelEnabled)
            return;

        var entity = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic_override");
        if (entity == null || !entity.IsValid)
            return;

        entity.MoveType = MoveType_t.MOVETYPE_NOCLIP;
        entity.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_NONE;
        entity.Collision.CollisionAttribute.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_NONE;

        entity.DispatchSpawn();
        entity.SetModel(Config.ParachuteModel);

        state.Model = new CHandle<CDynamicProp>(entity.EntityHandle.Raw);
    }

    private static void StopPara(ParaState state)
    {
        state.UsingPara = false;
        state.TeleportTicks = 0;
        state.PermissionTicks = 0;

        var model = state.Model?.Value;
        if (model != null && model.IsValid)
            model.Remove();

        state.Model = null;
    }

    private void UpdatePara(ParaState state, CCSPlayerPawn pawn)
    {
        var velocity = pawn.AbsVelocity;
        var position = pawn.AbsOrigin;
        var angle = pawn.AbsRotation;

        if (velocity.Z < 0.0f)
        {
            var fallspeed = -Config.FallSpeed;
            var isFallSpeed = velocity.Z >= fallspeed;

            if ((isFallSpeed && Config.Linear) || Config.DecreaseVec == 0.0f)
                velocity.Z = fallspeed;
            else
                velocity.Z += Config.DecreaseVec;

            if (++state.TeleportTicks > Config.TeleportTicks)
            {
                pawn.Teleport(position, angle, velocity);
                state.TeleportTicks = 0;
            }
        }

        // Modell immer mitfuehren, auch beim Steigen (z. B. Jumppad)
        var model = state.Model?.Value;
        if (model != null && model.IsValid)
            model.Teleport(position, angle, velocity);
    }
}
