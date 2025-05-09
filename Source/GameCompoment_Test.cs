using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;

namespace CeManualPatcher
{
    internal class GameCompoment_Test : GameComponent
    {

        public GameCompoment_Test(Game game)
        {
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();

            Log.Warning("Tick");

            foreach (var pawn in Find.CurrentMap.mapPawns.AllHumanlikeSpawned)
            {
                if (pawn.Faction.HostileTo(Faction.OfPlayer))
                {
                    InteractionWorker_RecruitAttempt.DoRecruit(null, pawn);
                    Messages.Message($"{pawn.Name.ToStringShort} Faction changed to player", pawn, MessageTypeDefOf.PositiveEvent, false);
                }
            }
        }

    }
}
