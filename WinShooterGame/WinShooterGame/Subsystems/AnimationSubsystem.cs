using System;
using WinShooterGame.Core;
using System.Data.SQLite;
using Microsoft.Xna.Framework;

namespace WinShooterGame.Subsystems
{
    class AnimationSubsystem : Subsystem
    {
        SQLiteCommand getComponentDataCmd;
        SQLiteCommand animationUpdateCmd;
        SQLiteCommand renderUpdateCmd;
        SQLiteCommand getEntitiesCmd;

        public AnimationSubsystem()
        {
            getComponentDataCmd = EntityManager.PrepareGetComponentDataCommand("animation_component");
            animationUpdateCmd = EntityManager.PrepareUpdateComponentCommand("animation_component", "elapsed_time, current_frame, active");
            renderUpdateCmd = EntityManager.PrepareUpdateComponentCommand("render_component", "source_rect_X, source_rect_Y, source_rect_Width, source_rect_Height");
            getEntitiesCmd = EntityManager.PrepareGetAllEntitiesWithComponents("animation_component");
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
                            // Do not update the game if we are not active
                            bool active = Convert.ToBoolean(getDataReader["active"]);
                            if (active == false)
                            {
                                continue;
                            }

                            Int64 elapsedTime = (Int64)getDataReader["elapsed_time"];
                            Int64 frameTime = (Int64)getDataReader["frame_time"];
                            int currentFrame = Convert.ToInt32(getDataReader["current_frame"]);
                            int frameCount = Convert.ToInt32(getDataReader["frame_count"]);
                            bool looping = Convert.ToBoolean(getDataReader["looping"]);

                            int frameWidth = Convert.ToInt32(getDataReader["frame_width"]);
                            int frameHeight = Convert.ToInt32(getDataReader["frame_height"]);

                            // Update the elapsed time
                            elapsedTime += (Int64)gameTime.ElapsedGameTime.TotalMilliseconds;

                            // If the elapsed time is larger than the frame time
                            // we need to switch frames

                            if (elapsedTime > frameTime)
                            {
                                // Move to the next frame
                                currentFrame++;

                                // If the currentFrame is equal to frameCount reset currentFrame to zero
                                if (currentFrame == frameCount)
                                {
                                    currentFrame = 0;

                                    // If we are not looping deactivate the animation
                                    if (looping == false)
                                    {
                                        active = false;
                                    }
                                }

                                // Reset the elapsed time to zero
                                elapsedTime = 0;

                                //renderUpdateCmd.Parameters.Clear();
                                renderUpdateCmd.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
                                renderUpdateCmd.Parameters.Add(new SQLiteParameter("@param1", currentFrame * frameWidth));
                                renderUpdateCmd.Parameters.Add(new SQLiteParameter("@param2", Convert.ToInt32(0)));
                                renderUpdateCmd.Parameters.Add(new SQLiteParameter("@param3", frameWidth));
                                renderUpdateCmd.Parameters.Add(new SQLiteParameter("@param4", frameHeight));
                                renderUpdateCmd.ExecuteNonQuery();
                            }

                            //animationUpdateCmd.Parameters.Clear();
                            animationUpdateCmd.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
                            animationUpdateCmd.Parameters.Add(new SQLiteParameter("@param1", elapsedTime));
                            animationUpdateCmd.Parameters.Add(new SQLiteParameter("@param2", currentFrame));
                            animationUpdateCmd.Parameters.Add(new SQLiteParameter("@param3", active));
                            animationUpdateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
