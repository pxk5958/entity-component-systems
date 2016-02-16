using System;
using WinShooterGame.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WinShooterGame.Subsystems
{
    class SpawnEnemySubsystem : Subsystem
    {
        // The rate at which the enemies appear
        TimeSpan enemySpawnTime;
        TimeSpan previousSpawnTime;

        // A random number generator
        Random random;

        Int64 index;

        GraphicsDevice graphicsDevice;

        float enemyMoveSpeed = -6f;

        public SpawnEnemySubsystem(GraphicsDevice graphicsDevice)
        {
            // Set the time keepers to zero
            previousSpawnTime = TimeSpan.Zero;

            // Used to determine how fast enemy respawns
            enemySpawnTime = TimeSpan.FromSeconds(0.25f);

            // Initialize our random number generator
            random = new Random();

            index = 1;

            this.graphicsDevice = graphicsDevice;
        }

        public override void Update(GameTime gameTime)
        {
            if (gameTime.TotalGameTime - previousSpawnTime > enemySpawnTime)
            {
                previousSpawnTime = gameTime.TotalGameTime;

                // Add an Enemy
                AddEnemy();
            }
        }

        private void AddEnemy()
        {
            const int enemyFrameWidth = 47;
            const int enemyFrameHeight = 61;
            const float enemyScaleX = 1.0f;
            const float enemyScaleY = 1.0f;

            Vector2 enemyPosition = new Vector2(graphicsDevice.Viewport.Width + enemyFrameWidth / 2,
                                                random.Next(100, graphicsDevice.Viewport.Height - 100));

            Int64 id = EntityManager.CreateEntity("enemy_ship_" + index);

            EntityManager.AddComponent(id, "position_component",
                                          "X, Y",           
                                          enemyPosition.X,
                                          enemyPosition.Y);

            EntityManager.AddComponent(id, "render_component",
                                          "texture, layer_depth, position_X, position_Y, origin_X, origin_Y, source_rect_X, source_rect_Y, source_rect_Width, source_rect_Height, scale_X, scale_Y",
                                          "Graphics\\mineAnimation", 0.1f,
                                          enemyPosition.X, enemyPosition.Y,
                                          enemyFrameWidth / 2f, enemyFrameHeight/2f, 
                                          0, 0, enemyFrameWidth, enemyFrameHeight,
                                          enemyScaleX, enemyScaleY);

            EntityManager.AddComponent(id, "animation_component",
                                          "frame_width, frame_height, frame_count, frame_time, looping",
                                          enemyFrameWidth, enemyFrameHeight, 8, 30, true);
            EntityManager.AddComponent(id, "constant_velocity_component",
                                          "constant_velocity_X",
                                          enemyMoveSpeed);
            EntityManager.AddComponent(id, "movable_component");
            EntityManager.AddComponent(id, "active_component",
                                       "active", true);
            EntityManager.AddComponent(id, "deactivate_off_screen_left_component");
            EntityManager.AddComponent(id, "health_component");
            EntityManager.AddComponent(id, "enemy_component",
                                       "damage_to_player", 10);
            EntityManager.AddComponent(id, "collidable_component",
                                       "X, Y, Width, Height",
                                       enemyPosition.X - (enemyFrameWidth / 2f) * enemyScaleX,
                                       enemyPosition.Y - (enemyFrameHeight / 2f) * enemyScaleY,
                                       enemyFrameWidth * enemyScaleX,
                                       enemyFrameHeight * enemyScaleY);

            index++;
        }
    }
}
