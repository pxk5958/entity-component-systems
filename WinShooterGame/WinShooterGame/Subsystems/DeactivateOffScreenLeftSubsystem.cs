using System;
using Microsoft.Xna.Framework;
using WinShooterGame.Core;
using System.Data.SQLite;
using Microsoft.Xna.Framework.Graphics;

namespace WinShooterGame.Subsystems
{
    class DeactivateOffScreenLeftSubsystem : Subsystem
    {
        SQLiteCommand getComponentDataCmd;
        SQLiteCommand activeUpdateCmd;
        SQLiteCommand getEntitiesCmd;

        public DeactivateOffScreenLeftSubsystem()
        {
            getComponentDataCmd = EntityManager.PrepareGetComponentDataCommand("position_component");
            activeUpdateCmd = EntityManager.PrepareUpdateComponentCommand("active_component", "active");
            getEntitiesCmd = EntityManager.PrepareGetAllEntitiesWithComponents("deactivate_off_screen_left_component");
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
                            float x = Convert.ToSingle(getDataReader["X"]);

                            if (x <= -200)
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
