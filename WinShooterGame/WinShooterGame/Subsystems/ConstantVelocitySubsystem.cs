using System;
using Microsoft.Xna.Framework;
using WinShooterGame.Core;
using System.Data.SQLite;

namespace WinShooterGame.Subsystems
{
    class ConstantVelocitySubsystem : Subsystem
    {
        SQLiteCommand getComponentDataCmd;
        SQLiteCommand movableUpdateCmd;
        SQLiteCommand getEntitiesCmd;

        public ConstantVelocitySubsystem()
        {
            getComponentDataCmd = EntityManager.PrepareGetComponentDataCommand("constant_velocity_component", "movable_component");
            movableUpdateCmd = EntityManager.PrepareUpdateComponentCommand("movable_component", "dx, dy");
            getEntitiesCmd = EntityManager.PrepareGetAllEntitiesWithComponents("constant_velocity_component", "movable_component");
        }

        public override void Update(GameTime gameTime)
        {
            using (SQLiteDataReader entitiesReader = getEntitiesCmd.ExecuteReader())
            {
                while (entitiesReader.Read())
                {
                    Int64 entityId = (Int64)entitiesReader[0];

                    getComponentDataCmd.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
                    using (SQLiteDataReader getDataReader = getComponentDataCmd.ExecuteReader())
                    {
                        if (getDataReader.Read())
                        {
                            Vector2 constantVelocity = new Vector2(Convert.ToSingle(getDataReader["constant_velocity_X"]),
                                                                   Convert.ToSingle(getDataReader["constant_velocity_Y"]));

                            Vector2 change = new Vector2(Convert.ToSingle(getDataReader["dx"]),
                                                         Convert.ToSingle(getDataReader["dy"]));

                            movableUpdateCmd.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
                            movableUpdateCmd.Parameters.Add(new SQLiteParameter("@param1", change.X + constantVelocity.X));
                            movableUpdateCmd.Parameters.Add(new SQLiteParameter("@param2", change.Y + constantVelocity.Y));
                            movableUpdateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
