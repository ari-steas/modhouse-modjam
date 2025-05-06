using VRage.Game.Components;
using Sandbox.ModAPI;
using VRageMath;
using VRage.Game.Entity;
using System.Collections.Generic;
using Sandbox.Game.Entities;
using System;

namespace FentLauncher
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class PushForceMod : MySessionComponentBase
    {
        private const float Radius = 5f;
        private const float ForceMagnitude = 500000000f;
        private const int DurationFrames = 500;
        private Vector3D ForceDirection = Vector3D.Right;
        private int _forceTimer = 0;
        private Vector3D _forceOrigin = Vector3D.Zero;
        private HashSet<long> _affectedEntityIds = new HashSet<long>();
        private bool _wasMousePressed = false;
        private const ushort MessageId = 12345;

        public override void LoadData()
        {
            if (MyAPIGateway.Multiplayer.MultiplayerActive)
            {
                MyAPIGateway.Multiplayer.RegisterMessageHandler(MessageId, HandleForceMessage);
            }
        }

        protected override void UnloadData()
        {
            if (MyAPIGateway.Multiplayer.MultiplayerActive)
            {
                MyAPIGateway.Multiplayer.UnregisterMessageHandler(MessageId, HandleForceMessage);
            }
        }

        private void SendForceMessage(Vector3D origin, Vector3D direction)
        {
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(origin.X));
            data.AddRange(BitConverter.GetBytes(origin.Y));
            data.AddRange(BitConverter.GetBytes(origin.Z));
            data.AddRange(BitConverter.GetBytes(direction.X));
            data.AddRange(BitConverter.GetBytes(direction.Y));
            data.AddRange(BitConverter.GetBytes(direction.Z));
            MyAPIGateway.Multiplayer.SendMessageToServer(MessageId, data.ToArray());
        }

        private void HandleForceMessage(byte[] data)
        {
            if (!MyAPIGateway.Multiplayer.IsServer) return;

            int index = 0;
            Vector3D origin = new Vector3D(
                BitConverter.ToDouble(data, index),
                BitConverter.ToDouble(data, index += 8),
                BitConverter.ToDouble(data, index += 8)
            );
            Vector3D direction = new Vector3D(
                BitConverter.ToDouble(data, index += 8),
                BitConverter.ToDouble(data, index += 8),
                BitConverter.ToDouble(data, index += 8)
            );

            _forceOrigin = origin;
            ForceDirection = direction;

            var sphere = new BoundingSphereD(_forceOrigin, Radius);
            _affectedEntityIds.Clear();
            var allEntities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(allEntities, e => sphere.Contains(e.GetPosition()) != ContainmentType.Disjoint);

            var playerEntity = MyAPIGateway.Session?.Player?.Controller?.ControlledEntity?.Entity;
            foreach (var entity in allEntities)
            {
                if (entity != playerEntity && entity.GetTopMostParent() != playerEntity)
                    _affectedEntityIds.Add(entity.EntityId);
            }

            _forceTimer = DurationFrames;

            SendSyncMessage();
        }

        private void SendSyncMessage()
        {
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(_forceOrigin.X));
            data.AddRange(BitConverter.GetBytes(_forceOrigin.Y));
            data.AddRange(BitConverter.GetBytes(_forceOrigin.Z));
            data.AddRange(BitConverter.GetBytes(ForceDirection.X));
            data.AddRange(BitConverter.GetBytes(ForceDirection.Y));
            data.AddRange(BitConverter.GetBytes(ForceDirection.Z));
            data.AddRange(BitConverter.GetBytes(_forceTimer));
            data.AddRange(BitConverter.GetBytes(_affectedEntityIds.Count));
            foreach (var entityId in _affectedEntityIds)
            {
                data.AddRange(BitConverter.GetBytes(entityId));
            }
            MyAPIGateway.Multiplayer.SendMessageToOthers(MessageId, data.ToArray());
        }

        private void ReceiveSyncMessage(byte[] data)
        {
            int index = 0;
            _forceOrigin = new Vector3D(
                BitConverter.ToDouble(data, index),
                BitConverter.ToDouble(data, index += 8),
                BitConverter.ToDouble(data, index += 8)
            );
            ForceDirection = new Vector3D(
                BitConverter.ToDouble(data, index += 8),
                BitConverter.ToDouble(data, index += 8),
                BitConverter.ToDouble(data, index += 8)
            );
            _forceTimer = BitConverter.ToInt32(data, index += 8);
            int entityCount = BitConverter.ToInt32(data, index += 4);
            _affectedEntityIds.Clear();
            for (int i = 0; i < entityCount; i++)
            {
                long entityId = BitConverter.ToInt64(data, index += 8);
                _affectedEntityIds.Add(entityId);
            }
        }

        public override void UpdateBeforeSimulation()
        {
            var player = MyAPIGateway.Session?.Player;
            if (player == null) return;

            var character = player.Character;
            if (character == null) return;

            var tool = character.EquippedTool as IMyEntity;
            if (tool == null) return;

            var gun = tool as IMyHandheldGunObject<MyDeviceBase>;
            if (gun == null) return;

            var definition = gun.PhysicalItemDefinition;
            if (definition == null) return;

            string definitionId = definition.Id.ToString();
            bool isRocketLauncher = definitionId.Contains("BasicHandHeldLauncherItem") || definitionId.Contains("AdvancedHandHeldLauncherItem");
            if (!isRocketLauncher) return;

            bool isMousePressed = MyAPIGateway.Input.IsNewLeftMousePressed();
            if (isMousePressed && !_wasMousePressed)
            {
                var playerController = player.Controller?.ControlledEntity?.Entity;
                if (playerController == null) return;

                ForceDirection = playerController.WorldMatrix.Forward;
                _forceOrigin = player.GetPosition() + ForceDirection * 10.0;

                if (!MyAPIGateway.Multiplayer.MultiplayerActive || MyAPIGateway.Multiplayer.IsServer)
                {
                    var sphere = new BoundingSphereD(_forceOrigin, Radius);
                    _affectedEntityIds.Clear();
                    var allEntities = new HashSet<IMyEntity>();
                    MyAPIGateway.Entities.GetEntities(allEntities, e => sphere.Contains(e.GetPosition()) != ContainmentType.Disjoint);

                    var playerEntity = playerController as IMyEntity;
                    foreach (var entity in allEntities)
                    {
                        if (entity != playerEntity && entity.GetTopMostParent() != playerEntity)
                            _affectedEntityIds.Add(entity.EntityId);
                    }

                    _forceTimer = DurationFrames;
                }
                else
                {
                    SendForceMessage(_forceOrigin, ForceDirection);
                }
            }
            _wasMousePressed = isMousePressed;

            if (_forceTimer <= 0) return;

            _forceTimer--;

            if (!MyAPIGateway.Multiplayer.MultiplayerActive || MyAPIGateway.Multiplayer.IsServer)
            {
                foreach (var entityId in _affectedEntityIds)
                {
                    IMyEntity entity;
                    if (!MyAPIGateway.Entities.TryGetEntityById(entityId, out entity) || entity?.Physics == null || entity.MarkedForClose)
                        continue;

                    var force = (Vector3)(ForceDirection * ForceMagnitude);
                    entity.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE, force, _forceOrigin, null);
                }

                if (_forceTimer <= 0)
                {
                    _affectedEntityIds.Clear();
                    if (MyAPIGateway.Multiplayer.MultiplayerActive)
                    {
                        SendSyncMessage();
                    }
                }
            }
        }
    }
}