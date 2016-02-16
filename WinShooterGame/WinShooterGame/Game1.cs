using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using WinShooterGame.Core;
using WinShooterGame.Subsystems;


namespace WinShooterGame
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        static Dictionary<string, IDisposable> resourceMap = new Dictionary<string, IDisposable>();
        public static Dictionary<string, IDisposable> ResourceMap
        {
            get { return resourceMap; }
        }

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
       
        RenderSubsystem renderSubsystem;
        AnimationSubsystem animationSubsystem;
        PlayerControlSubsystem playerControlSubsystem;
        FireLaserSubsystem fireLaserSubsystem;
        ClampScreenSubsystem clampScreenSubsystem;
        WrapScreenSubsystem wrapScreenSubsystem;
        MovableSubsystem movableSubsystem;
        ConstantVelocitySubsystem constantVelocitySubsystem;
        SpawnEnemySubsystem spawnEnemySubsystem;
        DeactivateOffScreenLeftSubsystem deactivateOffScreenLeftSubsystem;
        DeactivateOffScreenRightSubsystem deactivateOffScreenRightSubsystem;
        DeactivateHealthZeroSubsystem deactivateHealthZeroSubsystem;
        RemoveInactiveSubsystem removeInactiveSubsystem;
        CollisionSubsystem collisionSubsystem;
        LifetimeSubsystem lifetimeSubsystem;

        bool saveExists = false;

        Int64 playerId;
        Int64 mainBackgroundId;
        Int64[] bgLayer1Ids;
        Int64[] bgLayer2Ids;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            saveExists = EntityManager.InitializeDatabase();

            renderSubsystem = new RenderSubsystem(GraphicsDevice);
            animationSubsystem = new AnimationSubsystem();
            playerControlSubsystem = new PlayerControlSubsystem();
            fireLaserSubsystem = new FireLaserSubsystem();
            clampScreenSubsystem = new ClampScreenSubsystem(GraphicsDevice);
            wrapScreenSubsystem = new WrapScreenSubsystem(GraphicsDevice);
            movableSubsystem = new MovableSubsystem();
            constantVelocitySubsystem = new ConstantVelocitySubsystem();
            spawnEnemySubsystem = new SpawnEnemySubsystem(GraphicsDevice);
            deactivateOffScreenLeftSubsystem = new DeactivateOffScreenLeftSubsystem();
            deactivateOffScreenRightSubsystem = new DeactivateOffScreenRightSubsystem(GraphicsDevice);
            removeInactiveSubsystem = new RemoveInactiveSubsystem();
            deactivateHealthZeroSubsystem = new DeactivateHealthZeroSubsystem();
            collisionSubsystem = new CollisionSubsystem();
            lifetimeSubsystem = new LifetimeSubsystem();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            resourceMap.Add("Graphics\\shipAnimation", Content.Load<Texture2D>("Graphics\\shipAnimation"));
            resourceMap.Add("Graphics\\mainbackground", Content.Load<Texture2D>("Graphics\\mainbackground"));
            resourceMap.Add("Graphics\\bgLayer1", Content.Load<Texture2D>("Graphics\\bgLayer1"));
            resourceMap.Add("Graphics\\bgLayer2", Content.Load<Texture2D>("Graphics\\bgLayer2"));
            resourceMap.Add("Graphics\\mineAnimation", Content.Load<Texture2D>("Graphics\\mineAnimation"));
            resourceMap.Add("Graphics\\laser", Content.Load<Texture2D>("Graphics\\laser"));
            resourceMap.Add("Graphics\\explosion", Content.Load<Texture2D>("Graphics\\explosion"));

            resourceMap.Add("Sound\\laserFire", Content.Load<SoundEffect>("Sound\\laserFire"));
            resourceMap.Add("Sound\\explosion", Content.Load<SoundEffect>("Sound\\explosion"));

            if (saveExists == false) {
                InitializeEntities();
            }
        }

        private void InitializeEntities()
        {
            const int playerFrameWidth = 115;
            const int playerFrameHeight = 69;
            const float playerScaleX = 0.7f;
            const float playerScaleY = 0.7f;

            playerId = EntityManager.CreateEntity("player");
            EntityManager.AddComponent(playerId, "position_component",
                                       "X,Y",
                                       GraphicsDevice.Viewport.TitleSafeArea.X,
                                       GraphicsDevice.Viewport.TitleSafeArea.Y + GraphicsDevice.Viewport.TitleSafeArea.Height / 2);
            EntityManager.AddComponent(playerId, "render_component",
                                       "texture, layer_depth, position_X, position_Y, origin_X, origin_Y, source_rect_X, source_rect_Y, source_rect_Width, source_rect_Height, scale_X, scale_Y",
                                       "Graphics\\shipAnimation", 0f,
                                       GraphicsDevice.Viewport.TitleSafeArea.X,
                                       GraphicsDevice.Viewport.TitleSafeArea.Y + GraphicsDevice.Viewport.TitleSafeArea.Height / 2,
                                       playerFrameWidth / 2f, playerFrameHeight / 2f,
                                       0, 0, playerFrameWidth, playerFrameHeight,
                                       playerScaleX, playerScaleY);
            EntityManager.AddComponent(playerId, "animation_component",
                                       "frame_width, frame_height, frame_count, frame_time, looping",
                                       playerFrameWidth, playerFrameHeight, 8, 30, true);
            EntityManager.AddComponent(playerId, "player_input_component");
            EntityManager.AddComponent(playerId, "clamp_screen_component");
            EntityManager.AddComponent(playerId, "movable_component");
            EntityManager.AddComponent(playerId, "active_component",
                                       "active", true);
            EntityManager.AddComponent(playerId, "health_component");
            EntityManager.AddComponent(playerId, "player_component",
                                       "damage_to_enemy", 100);
            EntityManager.AddComponent(playerId, "collidable_component",
                                       "X, Y, Width, Height",
                                       GraphicsDevice.Viewport.TitleSafeArea.X - (playerFrameWidth / 2f) * playerScaleX,
                                       GraphicsDevice.Viewport.TitleSafeArea.Y + GraphicsDevice.Viewport.TitleSafeArea.Height / 2 - (playerFrameHeight / 2f) * playerScaleY,
                                       playerFrameWidth * playerScaleX,
                                       playerFrameHeight * playerScaleY);


            mainBackgroundId = EntityManager.CreateEntity("main background");
            EntityManager.AddComponent(mainBackgroundId, "render_component",
                                       "texture, layer_depth",
                                       "Graphics\\mainbackground", 1f);

            int n1 = GraphicsDevice.Viewport.Width / ((Texture2D)resourceMap["Graphics\\bgLayer1"]).Width + 1;
            int n2 = GraphicsDevice.Viewport.Width / ((Texture2D)resourceMap["Graphics\\bgLayer2"]).Width + 1;
            bgLayer1Ids = new Int64[n1];
            bgLayer2Ids = new Int64[n2];
            for (int i = 0; i < bgLayer1Ids.Length; i++)
            {
                bgLayer1Ids[i] = EntityManager.CreateEntity("bgLayer1_" + i);
                EntityManager.AddComponent(bgLayer1Ids[i], "position_component",
                                           "X", i * ((Texture2D)resourceMap["Graphics\\bgLayer1"]).Width);
                EntityManager.AddComponent(bgLayer1Ids[i], "render_component",
                                           "texture, layer_depth, position_X",
                                           "Graphics\\bgLayer1", 0.9f, i * ((Texture2D)resourceMap["Graphics\\bgLayer1"]).Width);
                EntityManager.AddComponent(bgLayer1Ids[i], "constant_velocity_component",
                                           "constant_velocity_X",
                                           -1f);
                EntityManager.AddComponent(bgLayer1Ids[i], "wrap_screen_component");
                EntityManager.AddComponent(bgLayer1Ids[i], "movable_component");
            }
            for (int i = 0; i < bgLayer2Ids.Length; i++)
            {
                bgLayer2Ids[i] = EntityManager.CreateEntity("bgLayer2_" + i);
                EntityManager.AddComponent(bgLayer2Ids[i], "position_component",
                                           "X", i * ((Texture2D)resourceMap["Graphics\\bgLayer2"]).Width);
                EntityManager.AddComponent(bgLayer2Ids[i], "render_component",
                                           "texture, layer_depth, position_X",
                                           "Graphics\\bgLayer2", 0.8f, i * ((Texture2D)resourceMap["Graphics\\bgLayer1"]).Width);
                EntityManager.AddComponent(bgLayer2Ids[i], "constant_velocity_component",
                                           "constant_velocity_X",
                                           -2f);
                EntityManager.AddComponent(bgLayer2Ids[i], "wrap_screen_component");
                EntityManager.AddComponent(bgLayer2Ids[i], "movable_component");
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here

            EntityManager.FinalizeDatabase();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            SQLiteTransaction transaction = EntityManager.DBConnection.BeginTransaction();

            animationSubsystem.Update(gameTime);

            playerControlSubsystem.Update(gameTime);
            fireLaserSubsystem.Update(gameTime);
            spawnEnemySubsystem.Update(gameTime);
            constantVelocitySubsystem.Update(gameTime);
            clampScreenSubsystem.Update(gameTime);
            wrapScreenSubsystem.Update(gameTime);
            movableSubsystem.Update(gameTime);
            collisionSubsystem.Update(gameTime);
            lifetimeSubsystem.Update(gameTime);
            deactivateOffScreenLeftSubsystem.Update(gameTime);
            deactivateOffScreenRightSubsystem.Update(gameTime);
            deactivateHealthZeroSubsystem.Update(gameTime);
            removeInactiveSubsystem.Update(gameTime);

            transaction.Commit();

            float update = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (update < 50f)
            {
                Console.WriteLine("Update: " + update);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            renderSubsystem.Update(gameTime);

            float frameRate = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (frameRate < 50f) {
                Console.WriteLine("FPS: " + frameRate);
            }

            base.Draw(gameTime);
        }
    }
}
