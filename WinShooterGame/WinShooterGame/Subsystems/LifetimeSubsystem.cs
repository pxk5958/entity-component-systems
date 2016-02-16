using System;
using Microsoft.Xna.Framework;
using WinShooterGame.Core;
using System.Data.SQLite;

namespace WinShooterGame.Subsystems
{
    class LifetimeSubsystem : Subsystem
    {
        SQLiteCommand getComponentDataCmd;
        SQLiteCommand getEntitiesCmd;

        SQLiteCommand activeUpdateCmd;
        SQLiteCommand lifetimeUpdateCmd;

        public LifetimeSubsystem()
        {
            getComponentDataCmd = EntityManager.PrepareGetComponentDataCommand("lifetime_component");
            getEntitiesCmd = EntityManager.PrepareGetAllEntitiesWithComponents("lifetime_component", "active_component");

            lifetimeUpdateCmd = EntityManager.PrepareUpdateComponentCommand("lifetime_component", "lifetime");
            activeUpdateCmd = EntityManager.PrepareUpdateComponentCommand("active_component", "active");
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
                            int lifetime = Convert.ToInt32(getDataReader["lifetime"]);
                            lifetime -= 1;

                            if (lifetime <= 0)
                            {
                                activeUpdateCmd.Parameters.Add(new SQLiteParameter("entity_id", entityId));
                                activeUpdateCmd.Parameters.Add(new SQLiteParameter("@param1", false));
                                activeUpdateCmd.ExecuteNonQuery();
                            }
                            else
                            {
                                lifetimeUpdateCmd.Parameters.Add(new SQLiteParameter("entity_id", entityId));
                                lifetimeUpdateCmd.Parameters.Add(new SQLiteParameter("@param1", lifetime));
                                lifetimeUpdateCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }
    }
}
