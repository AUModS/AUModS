using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using static AUMod.Roles;

namespace AUMod {
public class DeadPlayer {
    public PlayerControl player;

    public DeadPlayer(PlayerControl player, DateTime timeOfDeath, DeathReason deathReason, PlayerControl killerIfExisting)
    {
        this.player = player;
    }
}

static class GameHistory {
    public static List<Tuple<Vector3, bool>> localPlayerPositions = new List<Tuple<Vector3, bool>>();
    public static List<DeadPlayer> deadPlayers = new List<DeadPlayer>();

    public static void clearGameHistory()
    {
        localPlayerPositions = new List<Tuple<Vector3, bool>>();
        deadPlayers = new List<DeadPlayer>();
    }
}
}
