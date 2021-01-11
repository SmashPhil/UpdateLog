using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace UpdateLog
{
    public class UpdateSaveFileHandler : GameComponent
    {
        public UpdateSaveFileHandler(Game game)
        {
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            UpdateHandler.CheckUpdates(UpdateFor.GameInit);
        }
    }
}
