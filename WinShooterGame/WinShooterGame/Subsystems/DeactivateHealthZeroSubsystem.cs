using System;
using Microsoft.Xna.Framework;
using WinShooterGame.Core;
using System.Data.SQLite;

namespace WinShooterGame.Subsystems
{
    class DeactivateHealthZeroSubsystem : Subsystem
    {
        SQLiteCommand getComponentDataCmd;
        SQLiteCommand activeUpdateCmd;
        SQLiteCommand getEntitiesCmd;

        public DeactivateHealthZeroSubsystem()
        {
            getComponentDataCmd = EntityManager.PrepareGetComponentDataCommand("health_component");
            activeUpdateCmd = EntityManager.PrepareUpdateComponentCommand("active_component", "active");
            getEntitiesCmd = EntityManager.PrepareGetAllEntitiesWithComponents("health_component");
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
                            if (Convert.ToInt32(getDataReader["health"]) <= 0)
                            {
                                activeUpdateCmd.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
                                activeUpdateCmd.Parameters.Add(new SQLiteParameter("@param1", false));
                                activeUpdateCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }
    }
}
