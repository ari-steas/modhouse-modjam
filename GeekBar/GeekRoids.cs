using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRageMath;

namespace GeekBar
{
    public class Geekbar : MySessionComponentBase
    {
        public List<GeekTracker> ActiveGeeks = new List<GeekTracker>();
        public long tick = 0;
        public override void LoadData()
        {
            UpdateOrder = MyUpdateOrder.AfterSimulation;
        }

        public override void UpdateAfterSimulation()
        {
            if (tick % 10000 == 0)
            {
                SpawnGeek();
            }
            GeekGuidance();
            tick++;
        }

        private void SpawnGeek()
        {
            foreach(var grid in MyEntities.GetEntities().OfType<MyCubeGrid>())
            {
                var pos = grid.PositionComp.GetPosition();
                pos = pos + new Vector3D(5000, 5000, 5000);

                var outputgrids = new List<IMyCubeGrid>();
                MyAPIGateway.PrefabManager.SpawnPrefab(outputgrids, "geek", pos, Vector3.Forward, Vector3.Up, Vector3.Zero, Vector3.Zero, null, SpawningOptions.RandomizeColor, false, null);
                ActiveGeeks.Add(new GeekTracker() { TrackedGrid = grid, GeekGrid = (MyCubeGrid)outputgrids[0] });
            }
        }

        private void GeekGuidance()
        {
            foreach (var info in ActiveGeeks)
            {
                if (info.TrackedGrid == null || info.GeekGrid == null)
                {
                    ActiveGeeks.Remove(info);
                }
                var targetpos = info.TrackedGrid.PositionComp.GetPosition();
                var geekpos = info.GeekGrid.PositionComp.GetPosition();
                var normal = geekpos - targetpos;

                info.GeekGrid.Physics.LinearVelocity = Vector3D.Normalize(normal) * 500;
            }
        }

        protected override void UnloadData()
        {
            
        }
    }

    public class GeekTracker
    {
        public MyCubeGrid TrackedGrid;
        public MyCubeGrid GeekGrid;
    }
}
