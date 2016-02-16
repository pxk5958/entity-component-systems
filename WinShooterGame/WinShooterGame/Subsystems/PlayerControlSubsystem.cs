using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using WinShooterGame.Core;
using System.Data.SQLite;

namespace WinShooterGame.Subsystems
{
    class PlayerControlSubsystem : Subsystem
    {
        // TODO: Move keyboard and other input states info to a common place so that other systems can access

        // Keyboard states used to determine key presses
        KeyboardState currentKeyboardState;
        KeyboardState previousKeyboardState;

        // Gamepad states used to determine button presses
        GamePadState currentGamePadState;
        GamePadState previousGamePadState;

        //Mouse states used to track Mouse button press
        MouseState currentMouseState;
        MouseState previousMouseState;

        // A movement speed for the player
        float playerMoveSpeed;

        SQLiteCommand getComponentDataCmd;
        SQLiteCommand movableUpdateCmd;
        SQLiteCommand getEntitiesCmd;

        public PlayerControlSubsystem()
        {
            // Set a constant player move speed
            playerMoveSpeed = 8.0f;

            //Enable the FreeDrag gesture.
            TouchPanel.EnabledGestures = GestureType.FreeDrag;

            getComponentDataCmd = EntityManager.PrepareGetComponentDataCommand("position_component", "movable_component");
            movableUpdateCmd = EntityManager.PrepareUpdateComponentCommand("movable_component", "dx, dy");
            getEntitiesCmd = EntityManager.PrepareGetAllEntitiesWithComponents("player_input_component", "position_component", "movable_component");
        }

        public override void Update(GameTime gameTime)
        {
            // Save the previous state of the keyboard and game pad so we can determine single key/button presses
            previousGamePadState = currentGamePadState;
            previousKeyboardState = currentKeyboardState;
            previousMouseState = currentMouseState;

            // Read the current state of the keyboard and gamepad and store it
            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);
            currentMouseState = Mouse.GetState();

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
                            Vector2 change = new Vector2(Convert.ToSingle(getDataReader["dx"]),
                                                         Convert.ToSingle(getDataReader["dy"]));
                            // Windows 8 Touch Gestures for MonoGame
                            while (TouchPanel.IsGestureAvailable)
                            {
                                GestureSample gesture = TouchPanel.ReadGesture();

                                if (gesture.GestureType == GestureType.FreeDrag)
                                {
                                    change += gesture.Delta;
                                }
                            }

                            //Get Mouse State then Capture the Button type and Respond Button Press
                            Vector2 mousePosition = new Vector2(currentMouseState.X, currentMouseState.Y);

                            if (currentMouseState.LeftButton == ButtonState.Pressed)
                            {
                                Vector2 position = new Vector2(Convert.ToSingle(getDataReader["X"]),
                                                               Convert.ToSingle(getDataReader["Y"]));

                                Vector2 posDelta = mousePosition - position;
                                posDelta.Normalize();
                                posDelta = posDelta * playerMoveSpeed;
                                change += posDelta;
                            }

                            // Get Thumbstick Controls
                            change.X += currentGamePadState.ThumbSticks.Left.X * playerMoveSpeed;
                            change.Y -= currentGamePadState.ThumbSticks.Left.Y * playerMoveSpeed;

                            // Use the Keyboard / Dpad
                            if (currentKeyboardState.IsKeyDown(Keys.Left) || currentGamePadState.DPad.Left == ButtonState.Pressed)
                            {
                                change.X -= playerMoveSpeed;
                            }

                            if (currentKeyboardState.IsKeyDown(Keys.Right) || currentGamePadState.DPad.Right == ButtonState.Pressed)
                            {
                                change.X += playerMoveSpeed;
                            }

                            if (currentKeyboardState.IsKeyDown(Keys.Up) || currentGamePadState.DPad.Up == ButtonState.Pressed)
                            {
                                change.Y -= playerMoveSpeed;
                            }

                            if (currentKeyboardState.IsKeyDown(Keys.Down) || currentGamePadState.DPad.Down == ButtonState.Pressed)
                            {
                                change.Y += playerMoveSpeed;
                            }

                            movableUpdateCmd.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
                            movableUpdateCmd.Parameters.Add(new SQLiteParameter("@param1", change.X));
                            movableUpdateCmd.Parameters.Add(new SQLiteParameter("@param2", change.Y));
                            movableUpdateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
