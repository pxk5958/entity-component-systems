using System;
using Microsoft.Xna.Framework;
using WinShooterGame.Core;
using System.Data.SQLite;
using System.Collections.Generic;

namespace WinShooterGame.Subsystems
{
    class RemoveInactiveSubsystem : Subsystem
    {
        SQLiteCommand getComponentDataCmd;
        SQLiteCommand getEntitiesCmd;

        public RemoveInactiveSubsystem()
        {
            getComponentDataCmd = EntityManager.PrepareGetComponentDataCommand("active_component");
            getEntitiesCmd = EntityManager.PrepareGetAllEntitiesWithComponents("active_component");
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
                            bool active = Convert.ToBoolean(getDataReader["active"]);

                            if (active == false)
                            {
                                EntityManager.RemoveEntity(entityId);
                            }
                        }
                    }
                }
            }
        }
    }
}
