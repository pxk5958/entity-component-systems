using System;
using WinShooterGame.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Data.SQLite;
using Microsoft.Xna.Framework.Audio;

namespace WinShooterGame.Subsystems
{
    class FireLaserSubsystem : Subsystem
    {
        // Keyboard states used to determine key presses
        KeyboardState currentKeyboardState;

        // Gamepad states used to determine button presses
        GamePadState currentGamePadState;

        // the speed the laser travels
        float laserMoveSpeed;

        // govern how fast our laser can fire.
        TimeSpan laserSpawnTime;
        TimeSpan previousLaserSpawnTime;

        Int64 index = 1;

        SQLiteCommand getComponentDataCmd;
        SQLiteCommand getEntitiesCmd;

        public FireLaserSubsystem()
        {
            laserMoveSpeed = 30f;
            const float SECONDS_IN_MINUTE = 60f;
            const float RATE_OF_FIRE = 500;
            laserSpawnTime = TimeSpan.FromSeconds(SECONDS_IN_MINUTE / RATE_OF_FIRE);
            previousLaserSpawnTime = TimeSpan.Zero;

            getComponentDataCmd = EntityManager.PrepareGetComponentDataCommand("position_component", "render_component");
            getEntitiesCmd = EntityManager.PrepareGetAllEntitiesWithComponents("player_component", "position_component", "render_component");
        }

        public override void Update(GameTime gameTime)
        {
            // Read the current state of the keyboard and gamepad and store it
            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            if (currentKeyboardState.IsKeyDown(Keys.Space) || currentGamePadState.Buttons.X == ButtonState.Pressed)
            {
                // govern the rate of fire for our lasers
                if (gameTime.TotalGameTime - previousLaserSpawnTime > laserSpawnTime)
                {
                    previousLaserSpawnTime = gameTime.TotalGameTime;

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
                                    Vector2 position = new Vector2(Convert.ToSingle(getDataReader["X"]),
                                                                   Convert.ToSingle(getDataReader["Y"]));
                                    Vector2 origin = new Vector2(Convert.ToSingle(getDataReader["origin_X"]),
                                                                Convert.ToSingle(getDataReader["origin_Y"]));
                                    Vector2 scale = new Vector2(Convert.ToSingle(getDataReader["scale_X"]),
                                                                Convert.ToSingle(getDataReader["scale_Y"]));
                                    // Add the laer to our list.
                                    AddLaser(position, origin, scale);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AddLaser(Vector2 playerPosition, Vector2 playerOrigin, Vector2 playerScale)
        {
            const int laserFrameWidth = 46;
            const int laserFrameHeight = 16;
            const float laserScaleX = 0.7f;
            const float laserScaleY = 0.7f;

            Vector2 laserPosition = new Vector2(playerPosition.X + (90 - playerOrigin.X) * playerScale.X, 
                                                playerPosition.Y + (44 - playerOrigin.Y) * playerScale.Y);

            Int64 id = EntityManager.CreateEntity("laser_" + index);

            EntityManager.AddComponent(id, "position_component",
                                          "X, Y", laserPosition.X, laserPosition.Y);
            
            EntityManager.AddComponent(id, "render_component",
                                          "texture, layer_depth, position_X, position_Y, origin_X, origin_Y,  source_rect_X, source_rect_Y, source_rect_Width, source_rect_Height, scale_X, scale_Y",
                                          "Graphics\\laser", 0f,
                                          laserPosition.X, laserPosition.Y, 
                                          laserFrameWidth / 2f, laserFrameHeight/2f, 
                                          0, 0, laserFrameWidth, laserFrameHeight,
                                          laserScaleX, laserScaleY);
            
            EntityManager.AddComponent(id, "animation_component",
                                          "frame_width, frame_height, frame_count, frame_time, looping",
                                          laserFrameWidth, laserFrameHeight, 1, 30, true);
            EntityManager.AddComponent(id, "constant_velocity_component",
                                          "constant_velocity_X",
                                          laserMoveSpeed);
            EntityManager.AddComponent(id, "movable_component");
            EntityManager.AddComponent(id, "active_component",
                                       "active", true);
            EntityManager.AddComponent(id, "deactivate_off_screen_right_component");
            EntityManager.AddComponent(id, "laser_component",
                                       "damage_to_enemy", 100);
           
            EntityManager.AddComponent(id, "collidable_component",
                                       "X, Y, Width, Height",
                                       laserPosition.X - (laserFrameWidth / 2f) * laserScaleX,
                                       laserPosition.Y - (laserFrameHeight / 2f) * laserScaleY,
                                       laserFrameWidth * laserScaleX,
                                       laserFrameHeight * laserScaleY);
            
            ((SoundEffect)Game1.ResourceMap["Sound\\laserFire"]).Play();

            index++;
        }
    }
}
