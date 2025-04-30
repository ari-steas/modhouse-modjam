using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI;
using VRage.Game.Components;

namespace ModJam
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class MainSession : MySessionComponentBase
    {
        public override void LoadData()
        {
            Log.Init();
        }

        public override void UpdateAfterSimulation()
        {
            // neeeeerd
            MyAPIGateway.Utilities.ShowNotification("hi", 1000/60);
        }

        protected override void UnloadData()
        {
            Log.Close();
        }
    }
}
