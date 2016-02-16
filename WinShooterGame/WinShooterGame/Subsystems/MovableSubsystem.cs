using System;
using Microsoft.Xna.Framework;
using WinShooterGame.Core;
using System.Data.SQLite;

namespace WinShooterGame.Subsystems
{
    class MovableSubsystem : Subsystem
    {
        SQLiteCommand getMovableDataCmd;
        SQLiteCommand getRenderDataCmd;
        SQLiteCommand getCollidableDataCmd;

        SQLiteCommand positionUpdateCmd;
        SQLiteCommand renderUpdateCmd;
        SQLiteCommand collidableUpdateCmd;
        SQLiteCommand movableUpdateCmd;

        SQLiteCommand getEntitiesCmd;

        public MovableSubsystem()
        {
            getMovableDataCmd = EntityManager.PrepareGetComponentDataCommand("movable_component", "position_component");
            getRenderDataCmd = EntityManager.PrepareGetComponentDataCommand("render_component");
            getCollidableDataCmd = EntityManager.PrepareGetComponentDataCommand("collidable_component");

            positionUpdateCmd = EntityManager.PrepareUpdateComponentCommand("position_component", "X, Y");
            renderUpdateCmd = EntityManager.PrepareUpdateComponentCommand("render_component", "position_X, position_Y");
            collidableUpdateCmd = EntityManager.PrepareUpdateComponentCommand("collidable_component", "X, Y");
            movableUpdateCmd = EntityManager.PrepareUpdateComponentCommand("movable_component", "dx, dy");

            getEntitiesCmd = EntityManager.PrepareGetAllEntitiesWithComponents("movable_component", "position_component");
        }

        public override void Update(GameTime gameTime)
        {
            using (SQLiteDataReader entitiesReader = getEntitiesCmd.ExecuteReader())
            {
                while (entitiesReader.Read())
                {
                    Int64 entityId = (Int64)entitiesReader[0];

                    getMovableDataCmd.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
                    using (SQLiteDataReader movableDataReader = getMovableDataCmd.ExecuteReader())
                    {
                        if (movableDataReader.Read())
                        {
                            Vector2 change = new Vector2(Convert.ToSingle(movableDataReader["dx"]),
                                                         Convert.ToSingle(movableDataReader["dy"]));

                            if (change != Vector2.Zero)
                            {
                                Vector2 position = new Vector2(Convert.ToSingle(movableDataReader["X"]),
                                                           Convert.ToSingle(movableDataReader["Y"]));
                                position += change;

                                positionUpdateCmd.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
                                positionUpdateCmd.Parameters.Add(new SQLiteParameter("@param1", position.X));
                                positionUpdateCmd.Parameters.Add(new SQLiteParameter("@param2", position.Y));
                                positionUpdateCmd.ExecuteNonQuery();

                                getRenderDataCmd.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
                                using (SQLiteDataReader renderDataReader = getRenderDataCmd.ExecuteReader())
                                {
                                    if (renderDataReader.Read())
                                    {
                                        Vector2 renderPosition = new Vector2(Convert.ToSingle(renderDataReader["position_X"]),
                                                                             Convert.ToSingle(renderDataReader["position_Y"]));

                                        renderPosition += change;

                                        renderUpdateCmd.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
                                        renderUpdateCmd.Parameters.Add(new SQLiteParameter("@param1", renderPosition.X));
                                        renderUpdateCmd.Parameters.Add(new SQLiteParameter("@param2", renderPosition.Y));
                                        renderUpdateCmd.ExecuteNonQuery();
                                    }
                                }

                                getCollidableDataCmd.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
                                using (SQLiteDataReader collidableDataReader = getCollidableDataCmd.ExecuteReader())
                                {
                                    if (collidableDataReader.Read())
                                    {
                                        Vector2 collidablePosition = new Vector2(Convert.ToSingle(collidableDataReader["X"]),
                                                                                 Convert.ToSingle(collidableDataReader["Y"]));

                                        collidablePosition += change;

                                        collidableUpdateCmd.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
                                        collidableUpdateCmd.Parameters.Add(new SQLiteParameter("@param1", collidablePosition.X));
                                        collidableUpdateCmd.Parameters.Add(new SQLiteParameter("@param2", collidablePosition.Y));
                                        collidableUpdateCmd.ExecuteNonQuery();
                                    }
                                }

                                movableUpdateCmd.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
                                movableUpdateCmd.Parameters.Add(new SQLiteParameter("@param1", Convert.ToInt32(0)));
                                movableUpdateCmd.Parameters.Add(new SQLiteParameter("@param2", Convert.ToInt32(0)));
                                movableUpdateCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }
    }
}
