using System;
using Microsoft.Xna.Framework;
using WinShooterGame.Core;
using System.Data.SQLite;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace WinShooterGame.Subsystems
{
    class CollisionSubsystem : Subsystem
    {
        SQLiteCommand getPlayersCmd;
        SQLiteCommand getEnemiesCmd;
        SQLiteCommand getLasersCmd;
        SQLiteCommand getPlayerDataCmd;
        SQLiteCommand getEnemyDataCmd;
        SQLiteCommand getLaserDataCmd;
        SQLiteCommand getPositionDataCmd;

        SQLiteCommand healthUpdateCmd;
        SQLiteCommand activeUpdateCmd;

        Int64 explosionIndex = 1;

        public CollisionSubsystem()
        {
            getPlayersCmd = EntityManager.PrepareGetAllEntitiesWithComponents("player_component", "collidable_component", "health_component");
            getEnemiesCmd = EntityManager.PrepareGetAllEntitiesWithComponents("enemy_component", "collidable_component", "health_component");
            getLasersCmd = EntityManager.PrepareGetAllEntitiesWithComponents("laser_component", "collidable_component", "active_component");

            getPlayerDataCmd = EntityManager.PrepareGetComponentDataCommand("player_component", "collidable_component", "health_component");
            getEnemyDataCmd = EntityManager.PrepareGetComponentDataCommand("enemy_component", "collidable_component", "health_component");
            getLaserDataCmd = EntityManager.PrepareGetComponentDataCommand("laser_component", "collidable_component", "active_component");

            getPositionDataCmd = EntityManager.PrepareGetComponentDataCommand("position_component");

            healthUpdateCmd = EntityManager.PrepareUpdateComponentCommand("health_component", "health");
            activeUpdateCmd = EntityManager.PrepareUpdateComponentCommand("active_component", "active");
        }

        public override void Update(GameTime gameTime)
        {
            IList<Int64> enemies = new List<Int64>();
            using (SQLiteDataReader entitiesReader = getEnemiesCmd.ExecuteReader())
            {
                while (entitiesReader.Read())
                {
                    Int64 entityId = (Int64)entitiesReader[0];
                    enemies.Add(entityId);
                }
            }

            using (SQLiteDataReader playersReader = getPlayersCmd.ExecuteReader())
            {
                while (playersReader.Read())
                {
                    Int64 playerId = (Int64)playersReader[0];

                    getPlayerDataCmd.Parameters.Add(new SQLiteParameter("@entity_id", playerId));
                    using (SQLiteDataReader playerDataReader = getPlayerDataCmd.ExecuteReader())
                    {
                        if (playerDataReader.Read())
                        {
                            Rectangle playerRect = new Rectangle(Convert.ToInt32(playerDataReader["X"]),
                                                                 Convert.ToInt32(playerDataReader["Y"]),
                                                                 Convert.ToInt32(playerDataReader["Width"]),
                                                                 Convert.ToInt32(playerDataReader["Height"]));
                            int damageToEnemy = Convert.ToInt32(playerDataReader["damage_to_enemy"]);
                            int playerHealth = Convert.ToInt32(playerDataReader["health"]);

                            foreach (Int64 enemyId in enemies)
                            {
                                getEnemyDataCmd.Parameters.Add(new SQLiteParameter("@entity_id", enemyId));
                                using (SQLiteDataReader enemyDataReader = getEnemyDataCmd.ExecuteReader())
                                {
                                    if (enemyDataReader.Read())
                                    {
                                        Rectangle enemyRect = new Rectangle(Convert.ToInt32(enemyDataReader["X"]),
                                                                 Convert.ToInt32(enemyDataReader["Y"]),
                                                                 Convert.ToInt32(enemyDataReader["Width"]),
                                                                 Convert.ToInt32(enemyDataReader["Height"]));

                                        int damageToPlayer = Convert.ToInt32(enemyDataReader["damage_to_player"]);

                                        if (playerRect.Intersects(enemyRect))
                                        {
                                            int enemyHealth = Convert.ToInt32(enemyDataReader["health"]);

                                            getPositionDataCmd.Parameters.Add(new SQLiteParameter("@entity_id", enemyId));
                                            using (SQLiteDataReader positionDataReader = getPositionDataCmd.ExecuteReader())
                                            {
                                                if (positionDataReader.Read())
                                                {
                                                    Vector2 enemyPosition = new Vector2(Convert.ToSingle(positionDataReader["X"]),
                                                                                        Convert.ToSingle(positionDataReader["Y"]));

                                                    AddExplosion(enemyPosition);
                                                }
                                            }

                                            if (playerHealth - damageToPlayer <= 0)
                                            {
                                                getPositionDataCmd.Parameters.Add(new SQLiteParameter("@entity_id", playerId));
                                                using (SQLiteDataReader positionDataReader = getPositionDataCmd.ExecuteReader())
                                                {
                                                    if (positionDataReader.Read())
                                                    {
                                                        Vector2 playerPosition = new Vector2(Convert.ToSingle(positionDataReader["X"]),
                                                                                             Convert.ToSingle(positionDataReader["Y"]));

                                                        AddExplosion(playerPosition);
                                                    }
                                                }
                                            }

                                            healthUpdateCmd.Parameters.Add(new SQLiteParameter("entity_id", enemyId));
                                            healthUpdateCmd.Parameters.Add(new SQLiteParameter("@param1", enemyHealth - damageToEnemy));
                                            healthUpdateCmd.ExecuteNonQuery();

                                            healthUpdateCmd.Parameters.Add(new SQLiteParameter("entity_id", playerId));
                                            healthUpdateCmd.Parameters.Add(new SQLiteParameter("@param1", playerHealth - damageToPlayer));
                                            healthUpdateCmd.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            using (SQLiteDataReader lasersReader = getLasersCmd.ExecuteReader())
            {
                while (lasersReader.Read())
                {
                    Int64 laserId = (Int64)lasersReader[0];

                    getLaserDataCmd.Parameters.Add(new SQLiteParameter("@entity_id", laserId));
                    using (SQLiteDataReader laserDataReader = getLaserDataCmd.ExecuteReader())
                    {
                        if (laserDataReader.Read())
                        {
                            Rectangle laserRect = new Rectangle(Convert.ToInt32(laserDataReader["X"]),
                                                                 Convert.ToInt32(laserDataReader["Y"]),
                                                                 Convert.ToInt32(laserDataReader["Width"]),
                                                                 Convert.ToInt32(laserDataReader["Height"]));
                            int damageToEnemy = Convert.ToInt32(laserDataReader["damage_to_enemy"]);

                            foreach (Int64 enemyId in enemies)
                            {
                                getEnemyDataCmd.Parameters.Add(new SQLiteParameter("@entity_id", enemyId));
                                using (SQLiteDataReader enemyDataReader = getEnemyDataCmd.ExecuteReader())
                                {
                                    if (enemyDataReader.Read())
                                    {
                                        Rectangle enemyRect = new Rectangle(Convert.ToInt32(enemyDataReader["X"]),
                                                                 Convert.ToInt32(enemyDataReader["Y"]),
                                                                 Convert.ToInt32(enemyDataReader["Width"]),
                                                                 Convert.ToInt32(enemyDataReader["Height"]));

                                        if (laserRect.Intersects(enemyRect))
                                        {
                                            int enemyHealth = Convert.ToInt32(enemyDataReader["health"]);

                                            getPositionDataCmd.Parameters.Add(new SQLiteParameter("@entity_id", enemyId));
                                            using (SQLiteDataReader positionDataReader = getPositionDataCmd.ExecuteReader())
                                            {
                                                if (positionDataReader.Read())
                                                {
                                                    Vector2 enemyPosition = new Vector2(Convert.ToSingle(positionDataReader["X"]),
                                                                                        Convert.ToSingle(positionDataReader["Y"]));

                                                    AddExplosion(enemyPosition);
                                                }
                                            }

                                            healthUpdateCmd.Parameters.Add(new SQLiteParameter("entity_id", enemyId));
                                            healthUpdateCmd.Parameters.Add(new SQLiteParameter("@param1", enemyHealth - damageToEnemy));
                                            healthUpdateCmd.ExecuteNonQuery();

                                            activeUpdateCmd.Parameters.Add(new SQLiteParameter("entity_id", laserId));
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
        }

        private void AddExplosion(Vector2 position)
        {
            const int explosionFrameWidth = 134;
            const int explosionFrameHeight = 134;
            const float explosionScaleX = 0.7f;
            const float explosionScaleY = 0.7f;

            Int64 id = EntityManager.CreateEntity("explosion_" + explosionIndex);

            EntityManager.AddComponent(id, "position_component",
                                          "X, Y", position.X, position.Y);

            EntityManager.AddComponent(id, "render_component",
                                          "texture, layer_depth, position_X, position_Y, origin_X, origin_Y, source_rect_X, source_rect_Y, source_rect_Width, source_rect_Height, scale_X, scale_Y",
                                          "Graphics\\explosion", 0f,
                                          position.X, position.Y,
                                          explosionFrameWidth / 2f, explosionFrameHeight / 2f,
                                          0, 0, explosionFrameWidth, explosionFrameHeight,
                                          explosionScaleX, explosionScaleY);

            EntityManager.AddComponent(id, "animation_component",
                                          "frame_width, frame_height, frame_count, frame_time, looping",
                                          explosionFrameWidth, explosionFrameHeight, 12, 30, false);
            EntityManager.AddComponent(id, "active_component",
                                       "active", true);
            EntityManager.AddComponent(id, "lifetime_component",
                                       "lifetime", 30);

            ((SoundEffect)Game1.ResourceMap["Sound\\explosion"]).Play();

            explosionIndex++;
        }
    }
}
