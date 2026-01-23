using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Entities.Constants;
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

[MinimumApiVersion(179)]
public class Parachute : BasePlugin, IPluginConfig<ConfigGen>
{
    public override string ModuleName => "CS2 Parachute";
    public override string ModuleAuthor => "ddbmaster";
    public override string ModuleVersion => "1.6.0";

    public ConfigGen Config { get; set; } = null!;
    public void OnConfigParsed(ConfigGen config) => Config = config;

    private readonly Dictionary<int, bool> bUsingPara = new();
    private readonly Dictionary<int, int> gParaTicks = new();
    private readonly Dictionary<int, CBaseEntity?> gParaModel = new();

    private void EnsurePlayerState(CCSPlayerController player)
    {
        var idx = (int)player.Index;
        if (!bUsingPara.ContainsKey(idx)) bUsingPara[idx] = false;
        if (!gParaTicks.ContainsKey(idx)) gParaTicks[idx] = 0;
        if (!gParaModel.ContainsKey(idx)) gParaModel[idx] = null;
    }

    public override void Load(bool hotReload)
    {
        if (!Config.Enabled)
        {
            Console.WriteLine("[Parachute] Plugin not enabled!");
            return;
        }

        // Initialize for currently connected players on hot reload
        if (hotReload)
        {
            Utilities.GetPlayers().ForEach(player =>
            {
                if (player == null || !player.IsValid || player.IsBot) return;
                EnsurePlayerState(player);
                bUsingPara[(int)player.Index] = false;
                gParaTicks[(int)player.Index] = 0;
                gParaModel[(int)player.Index] = null;
            });
        }

        if (Config.ParachuteModelEnabled)
        {
            RegisterListener<Listeners.OnServerPrecacheResources>((manifest) =>
            {
                manifest.AddResource(Config.ParachuteModel);
            });
        }

        RegisterEventHandler<EventPlayerConnectFull>((@event, info) =>
        {
            var player = @event.Userid;
            if (player == null || !player.IsValid || player.IsBot)
                return HookResult.Continue;

            EnsurePlayerState(player);
            bUsingPara[(int)player.Index] = false;
            gParaTicks[(int)player.Index] = 0;
            gParaModel[(int)player.Index] = null;

            return HookResult.Continue;
        });

        RegisterEventHandler<EventPlayerDisconnect>((@event, info) =>
        {
            var player = @event.Userid;
            if (player == null || !player.IsValid || player.IsBot)
                return HookResult.Continue;

            var idx = (int)player.Index;

            // IMPORTANT: do NOT Remove() keys (prevents KeyNotFound races with other events)
            bUsingPara[idx] = false;
            gParaTicks[idx] = 0;

            if (gParaModel.TryGetValue(idx, out var ent) && ent != null && ent.IsValid)
                ent.Remove();

            gParaModel[idx] = null;

            return HookResult.Continue;
        });

        RegisterListener<Listeners.OnTick>(() =>
        {
            var players = Utilities.GetPlayers();

            foreach (var player in players)
            {
                if (player == null
                    || !player.IsValid
                    || player.IsBot
                    || !player.PawnIsAlive)
                    continue;

                if (!(Config.AccessFlag == "" || AdminManager.PlayerHasPermissions(player, Config.AccessFlag)))
                    continue;

                EnsurePlayerState(player);

                var buttons = player.Buttons;

                if ((buttons & PlayerButtons.Use) != 0 && !player.PlayerPawn.Value!.OnGroundLastTick)
                {
                    StartPara(player);
                }
                else if (bUsingPara[(int)player.Index])
                {
                    bUsingPara[(int)player.Index] = false;
                    StopPara(player);
                }
            }
        });

        RegisterEventHandler<EventPlayerDeath>((@event, info) =>
        {
            var player = @event.Userid;
            if (player == null || !player.IsValid)
                return HookResult.Continue;

            var idx = (int)player.Index;

            // No more KeyNotFound: use TryGetValue
            if (bUsingPara.TryGetValue(idx, out var usingPara) && usingPara)
            {
                bUsingPara[idx] = false;
                StopPara(player);
            }

            return HookResult.Continue;
        }, HookMode.Pre);
    }

    private void StopPara(CCSPlayerController player)
    {
        var idx = (int)player.Index;

        player.GravityScale = 1.0f;

        if (gParaTicks.ContainsKey(idx))
            gParaTicks[idx] = 0;

        if (gParaModel.TryGetValue(idx, out var ent) && ent != null && ent.IsValid)
        {
            ent.Remove();
            gParaModel[idx] = null;
        }
    }

    private void StartPara(CCSPlayerController player)
    {
        EnsurePlayerState(player);
        var idx = (int)player.Index;

        if (!bUsingPara[idx])
        {
            bUsingPara[idx] = true;
            player.GravityScale = 0.1f;

            if (Config.ParachuteModelEnabled)
            {
                var entity = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic_override");
                if (entity != null && entity.IsValid)
                {
                    entity.MoveType = MoveType_t.MOVETYPE_NOCLIP;
                    entity.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_NONE;
                    entity.Collision.CollisionAttribute.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_NONE;

                    entity.DispatchSpawn();
                    entity.SetModel(Config.ParachuteModel);

                    gParaModel[idx] = entity;
                }
            }
        }

        var fallspeed = Config.FallSpeed * (-1.0f);
        var velocity = player.PlayerPawn.Value?.AbsVelocity;
        if (velocity == null) return;

        var isFallSpeed = velocity.Z >= fallspeed;

        if (velocity.Z < 0.0f)
        {
            if ((isFallSpeed && Config.Linear) || Config.DecreaseVec == 0.0f)
            {
                velocity.Z = fallspeed;
            }
            else
            {
                velocity.Z = velocity.Z + Config.DecreaseVec;
            }

            var position = player.PlayerPawn.Value?.AbsOrigin!;
            var angle = player.PlayerPawn.Value?.AbsRotation!;

            if (gParaTicks.TryGetValue(idx, out var ticks) && ticks > Config.TeleportTicks)
            {
                player.PlayerPawn.Value!.Teleport(position, angle, velocity);

                gParaTicks[idx] = 0;
            }

            if (gParaModel.TryGetValue(idx, out var ent) && ent != null && ent.IsValid)
            {
                ent.Teleport(position, angle, velocity);
            }

            gParaTicks[idx] = gParaTicks.TryGetValue(idx, out var t) ? t + 1 : 1;
        }
    }
}
