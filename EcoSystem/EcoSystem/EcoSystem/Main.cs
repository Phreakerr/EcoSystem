using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.IO;
using System.Threading;

namespace EcoSystem
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class EcoSystemGame : Microsoft.Xna.Framework.Game
    {
        #region Global variables
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        MouseState prevMouseState;
        MouseState currMouseState;
        List<Texture2D> defaultForestTextures = new List<Texture2D>();
        List<Texture2D> factoryForestTextures = new List<Texture2D>();
        List<Texture2D> citadelForestTextures = new List<Texture2D>();
        List<Texture2D> defaultUrbanTextures = new List<Texture2D>();
        List<Texture2D> factoryUrbanTextures = new List<Texture2D>();
        List<Texture2D> citadelUrbanTextures = new List<Texture2D>();
        List<Texture2D> menuIconTextures = new List<Texture2D>();
        
        const int BOARDSIZEX = 19;
        const int BOARDSIZEY = 18;
        const int SPACINGX = 40;
        const int SPACINGY = 32;
        const int TILESCALEX = 48;
        const int TILESCALEY = 50;

        const int NARROWCOST = 100;
        const int BROADCOST = 150;
        const int FIRECOST = 300;
        const int WATERCOST = 50;
        const int UPGRADECOST = 500;

        const int CHANCEOFFIRESPREAD = 250;     //Lower is higher chance
        const int CHANCEOFFIREEXTINGUISH = 1000; //Lower is higher chance

        const int AIAGGRESSION = 300; //Lower is more aggressive

        const int RESOURCEGATHERDELAY = 30;
        int delaysSinceResourcesGathered;

        Player[] players = new Player[2];
        Player thisPlayer;
        Player AIPlayer;

        Tile[,] board = new Tile[BOARDSIZEX,BOARDSIZEY];
        Tile selectedTile;

        SpriteFont resourceFont;
        string messageText;

        FrameAnimation fire;
        int framesSinceFireAnimate;

        bool playerIsInMove = false;
        bool gameIsOver = false;

        Random rnd = new Random();
        #endregion

        public EcoSystemGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferHeight = (BOARDSIZEY*SPACINGY)+SPACINGY+100;
            graphics.PreferredBackBufferWidth = BOARDSIZEX*SPACINGX;
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

            base.Initialize();

            delaysSinceResourcesGathered = 0;

            this.IsMouseVisible = true;

            players[0] = new Player(false);
            players[1] = new Player(true);
            thisPlayer = players[0];
            AIPlayer = players[1];

            generateBoard();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            string folderPath;
            int count;

            folderPath = "Forest\\Default\\";
            count = Directory.GetFiles("Content\\" + folderPath).Length;

            for (int i = 0; i < count; i++)
            {
                try
                {
                    defaultForestTextures.Add(Content.Load<Texture2D>(folderPath + i.ToString()));
                }
                catch
                {
                    break;
                }
            }

            folderPath = "Urban\\Default\\";
            count = Directory.GetFiles("Content\\" + folderPath).Length;

            for (int i = 0; i < count; i++)
            {
                try
                {
                    defaultUrbanTextures.Add(Content.Load<Texture2D>(folderPath + i.ToString()));
                }
                catch
                {
                    break;
                }
            }

            menuIconTextures.Add(Content.Load<Texture2D>("Icons\\narrowAttack"));
            menuIconTextures.Add(Content.Load<Texture2D>("Icons\\broadAttack"));
            menuIconTextures.Add(Content.Load<Texture2D>("Icons\\fireAttack"));
            menuIconTextures.Add(Content.Load<Texture2D>("Icons\\waterAttack"));
            menuIconTextures.Add(Content.Load<Texture2D>("Icons\\upgradeAttack"));

            resourceFont = Content.Load<SpriteFont>("Resources");

            fire = new FrameAnimation(Content.Load<Texture2D>("fireAnimation"), 5);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (!gameIsOver)
            {
                // TODO: Add your update logic here

                checkForMouseClicks();
                doAI();
                spreadFires();
                doFireDamage();
                checkDeadTiles();
                gatherResources();
                animateFires();
                checkForWin();
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            Random rnd = new Random();

            #region Draw tiles and fires
            for (int y = 0; y < BOARDSIZEY; y++)
            {
                for (int x = 0; x < BOARDSIZEX; x++)
                {
                    if (selectedTile != null && x == selectedTile.position.X && y == selectedTile.position.Y)
                    {
                        spriteBatch.Draw(board[x, y].getTexture(), new Rectangle(x * SPACINGX, y * SPACINGY, TILESCALEX, TILESCALEY), new Color(255,100,100));
                    }
                    else
                    {
                            spriteBatch.Draw(board[x, y].getTexture(), new Rectangle(x * SPACINGX, y * SPACINGY, TILESCALEX, TILESCALEY), Color.White);
                    }

                    if (board[x, y].checkFire())
                    {
                        fire.Position = new Vector2(x*SPACINGX, y*SPACINGY);
                        spriteBatch.Draw(fire.Texture, fire.Position, fire.Rectangles[fire.FrameIndex], fire.Color, fire.Rotation, fire.Origin, fire.Scale, fire.SpriteEffect, 0f);
                    }
                }
            }
            #endregion

            #region Menu Buttons
            if (thisPlayer.resources < NARROWCOST || selectedTile == null)
            {
                spriteBatch.Draw(menuIconTextures[0], new Rectangle(((menuIconTextures[0].Width + 10) * 0) + 10, graphics.PreferredBackBufferHeight - menuIconTextures[0].Height - 10, menuIconTextures[0].Height, menuIconTextures[0].Width), Color.Crimson);
            }
            else
            {
                spriteBatch.Draw(menuIconTextures[0], new Rectangle(((menuIconTextures[0].Width + 10) * 0) + 10, graphics.PreferredBackBufferHeight - menuIconTextures[0].Height - 10, menuIconTextures[0].Height, menuIconTextures[0].Width), Color.White);
            }

            if (thisPlayer.resources < BROADCOST || selectedTile == null)
            {
                spriteBatch.Draw(menuIconTextures[1], new Rectangle(((menuIconTextures[0].Width + 10) * 1) + 10, graphics.PreferredBackBufferHeight - menuIconTextures[0].Height - 10, menuIconTextures[0].Height, menuIconTextures[0].Width), Color.Crimson);
            }
            else
            {
                spriteBatch.Draw(menuIconTextures[1], new Rectangle(((menuIconTextures[0].Width + 10) * 1) + 10, graphics.PreferredBackBufferHeight - menuIconTextures[0].Height - 10, menuIconTextures[0].Height, menuIconTextures[0].Width), Color.White);
            }

            if (thisPlayer.resources < FIRECOST || selectedTile == null)
            {
                spriteBatch.Draw(menuIconTextures[2], new Rectangle(((menuIconTextures[0].Width + 10) * 2) + 10, graphics.PreferredBackBufferHeight - menuIconTextures[0].Height - 10, menuIconTextures[0].Height, menuIconTextures[0].Width), Color.Crimson);
            }

            else
            {
                spriteBatch.Draw(menuIconTextures[2], new Rectangle(((menuIconTextures[0].Width + 10) * 2) + 10, graphics.PreferredBackBufferHeight - menuIconTextures[0].Height - 10, menuIconTextures[0].Height, menuIconTextures[0].Width), Color.White);
            }

            if (thisPlayer.resources < WATERCOST || selectedTile == null)
            {
                spriteBatch.Draw(menuIconTextures[3], new Rectangle(((menuIconTextures[0].Width + 10) * 3) + 10, graphics.PreferredBackBufferHeight - menuIconTextures[0].Height - 10, menuIconTextures[0].Height, menuIconTextures[0].Width), Color.Crimson);
            }

            else
            {
                spriteBatch.Draw(menuIconTextures[3], new Rectangle(((menuIconTextures[0].Width + 10) * 3) + 10, graphics.PreferredBackBufferHeight - menuIconTextures[0].Height - 10, menuIconTextures[0].Height, menuIconTextures[0].Width), Color.White);
            }

            if (thisPlayer.resources < UPGRADECOST || selectedTile == null)
            {
                spriteBatch.Draw(menuIconTextures[4], new Rectangle(((menuIconTextures[0].Width + 10) * 4) + 10, graphics.PreferredBackBufferHeight - menuIconTextures[0].Height - 10, menuIconTextures[0].Height, menuIconTextures[0].Width), Color.Crimson);
            }
            else
            {
                spriteBatch.Draw(menuIconTextures[4], new Rectangle(((menuIconTextures[0].Width + 10) * 4) + 10, graphics.PreferredBackBufferHeight - menuIconTextures[0].Height - 10, menuIconTextures[0].Height, menuIconTextures[0].Width), Color.White);
            }

            spriteBatch.DrawString(resourceFont, "Resources: " + thisPlayer.resources, new Vector2(graphics.PreferredBackBufferWidth - 10, graphics.PreferredBackBufferHeight - 10), Color.White, 0, resourceFont.MeasureString("Resources: " + thisPlayer.resources), 1, SpriteEffects.None, 0);

            if (playerIsInMove)
                spriteBatch.DrawString(resourceFont, "Click a tile to attack!", new Vector2(graphics.PreferredBackBufferWidth - 10, graphics.PreferredBackBufferHeight - 20 - resourceFont.MeasureString("Click a tile to attack!").Y), Color.White, 0, resourceFont.MeasureString("Click a tile to attack!"), 1, SpriteEffects.None, 0);
            else if (gameIsOver)
                spriteBatch.DrawString(resourceFont, messageText, new Vector2(graphics.PreferredBackBufferWidth - 10, graphics.PreferredBackBufferHeight - 20 - resourceFont.MeasureString(messageText).Y), Color.White, 0, resourceFont.MeasureString(messageText), 1, SpriteEffects.None, 0);
            
            #endregion

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void doAI()
        {
            Thread thread;
            if (rnd.Next(0, AIAGGRESSION) == 7)
            {
                if (AIPlayer.resources >= NARROWCOST)
                {
                    thread = new Thread(new ThreadStart(AINarrow));
                    thread.Start();
                }
            }

            else if (rnd.Next(0, AIAGGRESSION) == 8)
            {
                if (AIPlayer.resources >= BROADCOST)
                {
                    thread = new Thread(new ThreadStart(AIBroad));
                    thread.Start();
                }
            }

            else if (rnd.Next(0, AIAGGRESSION) == 9)
            {
                if (AIPlayer.resources >= FIRECOST)
                {
                    thread = new Thread(new ThreadStart(AIFire));
                    thread.Start();
                }
            }

            else if (rnd.Next(0, AIAGGRESSION) == 10)
            {
                if (AIPlayer.resources >= WATERCOST)
                {
                    thread = new Thread(new ThreadStart(AIWater));
                    thread.Start();
                }
            }

            else if (rnd.Next(0, AIAGGRESSION) == 11)
            {
                if (AIPlayer.resources >= UPGRADECOST)
                {
                    thread = new Thread(new ThreadStart(AIUpgrade));
                    thread.Start();
                }
            }
        }

        private void AIUpgrade()
        {
            bool tileIsOwned = false;

            do
            {
                int x = rnd.Next(0, BOARDSIZEX);
                int y = rnd.Next(0, BOARDSIZEY);

                if (board[x, y].faction == AIPlayer.faction)
                {
                    tileIsOwned = true;
                    board[x, y].upgrade();
                    AIPlayer.numberOfFactories++;
                    AIPlayer.resources -= UPGRADECOST;
                }

            } while (!tileIsOwned);


            Console.WriteLine("Building factory");
        }

        private void AIWater()
        {
            bool fireExists = false;

            foreach (Tile tile in board)
            {
                if (tile.faction == AIPlayer.faction && tile.checkFire())
                {
                    fireExists = true;
                }
            }

            if (fireExists)
            {
                List<Tile> tilesOnFire = new List<Tile>();

                foreach (Tile tile in board)
                {
                    if (tile.faction == AIPlayer.faction && tile.checkFire())
                    {
                        tilesOnFire.Add(tile);
                    }
                }

                Tile tileToPutOut = tilesOnFire[rnd.Next(0, tilesOnFire.Count)];

                for (int x = -2; x <= 2; x++)
                {
                    for (int y = -2; y <= 2; y++)
                    {
                        try
                        {
                            board[(int)tileToPutOut.position.X + x, (int)tileToPutOut.position.Y + y].fireExtinguish();
                        }
                        catch
                        {

                        }
                    }
                }


                AIPlayer.resources -= WATERCOST;

                Console.WriteLine("Putting out fires");
            }
        }

        private void AIFire()
        {
            Console.WriteLine("Setting fires");

            List<Tile> validTiles = new List<Tile>();

            foreach (Tile tile in board)
            {
                bool tileIsValid = true;

                if (tile.faction != AIPlayer.faction)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            try
                            {
                                if (board[(int)tile.position.X + x, (int)tile.position.Y + y].faction == AIPlayer.faction)
                                    tileIsValid = false;
                            }
                            catch
                            {

                            }
                        }
                    }
                }
                else
                {
                    tileIsValid = false;
                }

                if (tileIsValid)
                    validTiles.Add(tile);
            }

            bool attackFinished = false;

            do
            {
                if (validTiles.Count == 0)
                {
                    attackFinished = true;
                }
                else
                {
                    Tile tile = validTiles[rnd.Next(0, validTiles.Count)];
                    bool friendlyTileNearby = false;

                    for (int x = -4; x <= 4; x++)
                    {
                        for (int y = -4; y <= 4; y++)
                        {
                            try
                            {
                                if (board[(int)tile.position.X + x, (int)tile.position.Y + y].faction == AIPlayer.faction)
                                {
                                    friendlyTileNearby = true;
                                }
                            } 
                            catch
                            {
                            
                            }
                        }
                    }

                    if (friendlyTileNearby)
                    {
                        for (int x = -1; x <= 1; x++)
                        {
                            for (int y = -1; y <= 1; y++)
                            {
                                try
                                {
                                    board[(int)tile.position.X + x, (int)tile.position.Y + y].fireStart();

                                }
                                catch
                                {

                                }
                            }
                        }

                        AIPlayer.resources -= FIRECOST;
                        attackFinished = true;
                    }
                    else
                    {
                        validTiles.Remove(tile);
                    }
                }
            } while (!attackFinished);
        }

        private void AIBroad()
        {
            Console.WriteLine("Broad attack");

            bool validMoveTaken = false;

            Tile sourceTile = null;
            Tile targetTile = null;

            do
            {
                Tile chosenSource = board[rnd.Next(0, BOARDSIZEX), rnd.Next(0, BOARDSIZEY)];

                if (chosenSource.faction == AIPlayer.faction)
                {
                    for (int x = -2; x <= 2; x++)
                    {
                        for (int y = -2; y <= 2; y++)
                        {
                            try
                            {
                                if (board[(int)chosenSource.position.X + x, (int)chosenSource.position.Y + y].faction != AIPlayer.faction)
                                {
                                    sourceTile = chosenSource;
                                    targetTile = board[(int)chosenSource.position.X + x, (int)chosenSource.position.Y + y];
                                    validMoveTaken = true;
                                }
                            }
                            catch
                            {

                            }
                        }
                    }
                }

                if (sourceTile != null)
                {
                    foreach (Point point in Bresenham.GetPointsOnLine((int)sourceTile.position.X, (int)sourceTile.position.Y, (int)targetTile.position.X, (int)targetTile.position.Y))
                    {

                        for (int x = -1; x <= 1; x++)
                        {
                            for (int y = -1; y <= 1; y++)
                            {
                                try
                                {
                                    if (board[point.X + x, point.Y + y].faction != AIPlayer.faction)
                                        board[point.X + x, point.Y + y].doDamage(rnd.Next(10, 150));
                                }
                                catch
                                {

                                }
                            }
                        }
                    }

                    AIPlayer.resources -= BROADCOST;
                }

            } while (!validMoveTaken);

        }

        private void AINarrow()
        {
            Console.WriteLine("Narrow attack");

            bool validMoveTaken = false;

            Tile sourceTile = null;
            Tile targetTile = null;

            do
            {
                Tile chosenSource = board[rnd.Next(0, BOARDSIZEX), rnd.Next(0, BOARDSIZEY)];

                if (chosenSource.faction == AIPlayer.faction)
                {
                    for (int x = 5; x >= -5; x--)
                    {
                        for (int y = 5; y >= -5; y--)
                        {
                            try
                            {
                                if (board[(int)chosenSource.position.X + x, (int)chosenSource.position.Y + y].faction != AIPlayer.faction)
                                {
                                    sourceTile = chosenSource;
                                    targetTile = board[(int)chosenSource.position.X + x, (int)chosenSource.position.Y + y];
                                    validMoveTaken = true;
                                }
                            }
                            catch
                            {

                            }
                        }
                    }
                }

                if (sourceTile != null)
                {
                    foreach (Point point in Bresenham.GetPointsOnLine((int)sourceTile.position.X, (int)sourceTile.position.Y, (int)targetTile.position.X, (int)targetTile.position.Y))
                    {

                        for (int x = -1; x <= 1; x++)
                        {
                            for (int y = -1; y <= 1; y++)
                            {
                                try
                                {
                                    if (board[point.X + x, point.Y + y].faction != AIPlayer.faction)
                                        board[point.X + x, point.Y + y].doDamage(rnd.Next(10, 50));
                                }
                                catch
                                {

                                }
                            }
                        }
                        if (board[point.X, point.Y].faction != AIPlayer.faction)
                            board[point.X, point.Y].doDamage(100);
                    }

                    AIPlayer.resources -= NARROWCOST;
                }

            } while (!validMoveTaken);
        }

        private void checkForWin()
        {
            bool forestRemaining = false;
            bool urbanRemaining = false;

            foreach (Tile tile in board)
            {
                if (!tile.faction)
                    forestRemaining = true;
                else if (tile.faction)
                    urbanRemaining = true;
            }

            if (!forestRemaining)
                endGame(true);
            else if (!urbanRemaining)
                endGame(false);
        }

        private void endGame(bool faction)
        {
            gameIsOver = true;
            if (faction)
                messageText = "The city wins!";
            else if (!faction)
                messageText = "The forest wins!";
        }

        private void generateBoard()
        {
            Random rnd = new Random();
            for (int x = 0; x < BOARDSIZEX; x++)
            {
                for (int y = 0; y < BOARDSIZEY; y++)
                {
                    if ((x + y) >= (BOARDSIZEX + BOARDSIZEY) / 2)
                    {
                        Texture2D rndText = defaultUrbanTextures[rnd.Next(0, defaultUrbanTextures.Count)];
                        board[x, y] = new Tile(x, y, true, rndText, rnd.Next(0, 1000000));
                    }
                    else if ((x + y) < (BOARDSIZEX + BOARDSIZEY) / 2)
                    {
                        Texture2D rndText = defaultForestTextures[rnd.Next(0, defaultForestTextures.Count)];
                        board[x, y] = new Tile(x, y, false, rndText, rnd.Next(0, 1000000));
                    }
                }
            }
        }

        private void checkForMouseClicks()
        {
            currMouseState = Mouse.GetState();

            if (prevMouseState.LeftButton == ButtonState.Released && currMouseState.LeftButton == ButtonState.Pressed)
            {
                performClickAction(detectTileClicked(prevMouseState.X, prevMouseState.Y));
            }
            else if (prevMouseState.RightButton == ButtonState.Released && currMouseState.RightButton == ButtonState.Pressed)
            {
                selectedTile = null;
            }
            prevMouseState = Mouse.GetState();
        }

        private void animateFires()
        {
            if (framesSinceFireAnimate == 3)
            {
                fire.nextFrame();
                framesSinceFireAnimate = 0;
            }
            else
            {
                framesSinceFireAnimate++;
            }
        }

        private void gatherResources()
        {
            if (delaysSinceResourcesGathered == RESOURCEGATHERDELAY)
            {
                players[0].resources += (10 + (players[0].numberOfFactories*5));
                players[1].resources += (10 + (players[1].numberOfFactories*5));
                delaysSinceResourcesGathered = 0;
            }
            else
            {
                delaysSinceResourcesGathered++;
            }
        }

        private void performClickAction(Vector2 coords)
        {
            Thread thread;

            if (selectedTile != null && coords.X == -1) // Narrow clicked
            {
                thread = new Thread(new ThreadStart(narrowAttack));
                thread.Start();
            }
            else if (selectedTile != null && coords.X == -2) // Broad clicked
            {
                thread = new Thread(new ThreadStart(broadAttack));
                thread.Start();
            }
            else if (selectedTile != null && coords.X == -3) // Fire clicked
            {
                thread = new Thread(new ThreadStart(fireAttack));
                thread.Start();
            }
            else if (selectedTile != null && coords.X == -4) // Water clicked
            {
                thread = new Thread(new ThreadStart(waterAttack));
                thread.Start();
            }
            else if (selectedTile != null && coords.X == -5) // Upgrade clicked
            {
                thread = new Thread(new ThreadStart(upgradeUnit));
                thread.Start();
            }
            else if (coords.X >= 0 ) // Tile clicked
            {
                if (board[(int)coords.X, (int)coords.Y].faction == thisPlayer.faction)
                selectedTile = board[(int)coords.X, (int)coords.Y];
            }
            else
            {
                selectedTile = null;
            }
        }

        private void spreadFires()
        {
            Random rndSpread = new Random();

            foreach (Tile tile in board)
            {
                if (tile.checkFire() && rndSpread.Next(0, CHANCEOFFIRESPREAD) == 7) //Lucky number 7
                {
                    Random rndPosition = new Random();
                    try
                    {
                        board[(int)tile.position.X + rndPosition.Next(-1, 2), (int)tile.position.Y + rndPosition.Next(-1, 2)].fireStart();
                    }
                    catch
                    {

                    }
                }
                else if (tile.checkFire() && rndSpread.Next(0, CHANCEOFFIREEXTINGUISH) == 8)
                {
                    tile.fireExtinguish();
                }
            }
        }

        private void checkDeadTiles()
        {
            foreach (Tile tile in board)
            {
                if(tile.checkDestroyed())
                {

                    if (!tile.faction)
                    {
                        if (board[(int)tile.position.X, (int)tile.position.Y].isFactory)
                            players[0].numberOfFactories--;
                        Texture2D rndText = defaultUrbanTextures[rnd.Next(0,defaultUrbanTextures.Count)];
                        board[(int)tile.position.X, (int)tile.position.Y] = new Tile((int)tile.position.X, (int)tile.position.Y, true, rndText, rnd.Next(0, 1000000));
                    }
                    else if (tile.faction)
                    {
                        if (board[(int)tile.position.X, (int)tile.position.Y].isFactory)
                            players[1].numberOfFactories--;
                        Texture2D rndText = defaultForestTextures[rnd.Next(0, defaultForestTextures.Count)];
                        board[(int)tile.position.X, (int)tile.position.Y] = new Tile((int)tile.position.X, (int)tile.position.Y, false, rndText, rnd.Next(0, 1000000));
                    }
                }
            }
        }

        private void doFireDamage()
        {
            foreach (Tile tile in board)
            {
                tile.fireDamage();
            }
        }

        private Vector2 detectTileClicked(int X, int Y)
        {
            int boardX, boardY;

            boardX = (X / SPACINGX);
            boardY = (Y / SPACINGY);

            if (Y < BOARDSIZEY * SPACINGY && X < BOARDSIZEX * SPACINGX && X >= 0 && Y >=0)
            {
                return new Vector2(boardX, boardY);
            }
            else
            {
                if (Y > graphics.PreferredBackBufferHeight - menuIconTextures[0].Height - 10 && Y < graphics.PreferredBackBufferHeight - 10)
                {
                    int menuIconPressed = X / (menuIconTextures[0].Width + 10);
                    Console.Write(menuIconPressed);

                    return new Vector2(0 - (menuIconPressed + 1), 0);

                    
                } else return new Vector2(-6,0); //-6 for no tile clicked
            }

            
        }

        #region Attack methods
        private void upgradeUnit()
        {
            playerIsInMove = true;

            if (selectedTile.faction == thisPlayer.faction && thisPlayer.resources >= UPGRADECOST && !board[(int)selectedTile.position.X, (int)selectedTile.position.Y].isFactory)
            {
                board[(int)selectedTile.position.X, (int)selectedTile.position.Y].upgrade();
                thisPlayer.numberOfFactories++;
                thisPlayer.resources -= UPGRADECOST;
            }

            playerIsInMove = false;
        }

        private void waterAttack()
        {
            playerIsInMove = true;

            Tile sourceTile;
            Tile targetTile;

            if (selectedTile.faction == thisPlayer.faction && thisPlayer.resources >= WATERCOST)
            {
                sourceTile = selectedTile;

                bool exitLoop = false;

                do //POLLING FOR MOUSE INPUT
                {
                    MouseState state = Mouse.GetState();

                    if (prevMouseState.LeftButton == ButtonState.Released && state.LeftButton == ButtonState.Pressed)
                    {
                        Vector2 clickPosition = detectTileClicked(state.X, state.Y);
                        if (clickPosition.X >= 0 && (clickPosition.X - sourceTile.position.X >= -5 && clickPosition.X - sourceTile.position.X <= 5) && (clickPosition.Y - sourceTile.position.Y <= 5 && clickPosition.Y - sourceTile.position.Y >= -5))
                        {
                            targetTile = board[(int)clickPosition.X, (int)clickPosition.Y];
                            thisPlayer.resources -= WATERCOST;

                            for (int x = -2; x <= 2; x++)
                            {
                                for (int y = -2; y <= 2; y++)
                                {
                                    try
                                    {
                                        board[(int)targetTile.position.X + x, (int)targetTile.position.Y + y].fireExtinguish();
                                    }
                                    catch
                                    {

                                    }
                                }
                            }

                        }
                        exitLoop = true;
                        //selectedTile = null;
                    }

                } while (!exitLoop);
            }
            playerIsInMove = false;
        }

        private void fireAttack()
        {
            playerIsInMove = true;

            Console.Write("Fire");
            
            Tile sourceTile;
            Tile targetTile;

            if (selectedTile.faction == thisPlayer.faction && thisPlayer.resources >= FIRECOST)
            {
                sourceTile = selectedTile;

                bool exitLoop = false;

                do //POLLING FOR MOUSE INPUT
                {
                    MouseState state = Mouse.GetState();

                    if (prevMouseState.LeftButton == ButtonState.Released && state.LeftButton == ButtonState.Pressed)
                    {
                        Vector2 clickPosition = detectTileClicked(state.X, state.Y);
                        if (clickPosition.X >= 0 && (clickPosition.X - sourceTile.position.X >= -5 && clickPosition.X - sourceTile.position.X <= 5) && (clickPosition.Y - sourceTile.position.Y <= 5 && clickPosition.Y - sourceTile.position.Y >= -5))
                        {
                            targetTile = board[(int)clickPosition.X, (int)clickPosition.Y];
                            thisPlayer.resources -= FIRECOST;

                            for (int x = -1; x <= 1; x++)
                            {
                                for (int y = -1; y <= 1; y++)
                                {
                                    try
                                    {
                                        board[(int)targetTile.position.X + x, (int)targetTile.position.Y + y].fireStart();
                                    }
                                    catch
                                    {

                                    }
                                }
                            }

                        }
                        exitLoop = true;
                        //selectedTile = null;
                    }

                } while (!exitLoop);
            }
            playerIsInMove = false;
        }

        private void broadAttack()
        {
            playerIsInMove = true;

            Random damage = new Random();
            Tile sourceTile;
            Tile targetTile;

            if (selectedTile.faction == thisPlayer.faction && thisPlayer.resources >= BROADCOST)
            {
                sourceTile = selectedTile;

                bool exitLoop = false;

                do //POLLING FOR MOUSE INPUT
                {
                    MouseState state = Mouse.GetState();

                    if (prevMouseState.LeftButton == ButtonState.Released && state.LeftButton == ButtonState.Pressed)
                    {
                        Vector2 clickPosition = detectTileClicked(state.X, state.Y);
                        if (clickPosition.X >= 0 && (clickPosition.X - sourceTile.position.X >= -2 && clickPosition.X - sourceTile.position.X <= 2) && (clickPosition.Y - sourceTile.position.Y <= 2 && clickPosition.Y - sourceTile.position.Y >= -2))
                        {
                            targetTile = board[(int)clickPosition.X, (int)clickPosition.Y];
                            thisPlayer.resources -= BROADCOST;

                            foreach (Point point in Bresenham.GetPointsOnLine((int)sourceTile.position.X, (int)sourceTile.position.Y, (int)targetTile.position.X, (int)targetTile.position.Y))
                            {

                                for (int x = -1; x <= 1; x++)
                                {
                                    for (int y = -1; y <= 1; y++)
                                    {
                                        try
                                        {
                                            if (board[point.X + x, point.Y + y].faction != thisPlayer.faction)
                                            board[point.X + x, point.Y + y].doDamage(damage.Next(10, 150));
                                        }
                                        catch
                                        {

                                        }
                                    }
                                }
                            }
                        }
                        exitLoop = true;
                        //selectedTile = null;
                    }

                } while (!exitLoop);
            }
            playerIsInMove = false;
        }

        private void narrowAttack()
        {
            playerIsInMove = true;

            Random damage = new Random();
            Tile sourceTile;
            Tile targetTile;

            if (selectedTile.faction == thisPlayer.faction && thisPlayer.resources >= NARROWCOST)
            {
                sourceTile = selectedTile;

                bool exitLoop = false;

                do //POLLING FOR MOUSE INPUT  --  THIS IS HACKY AND SHOULD BE REPLACED
                {
                    MouseState state = Mouse.GetState();

                    if (prevMouseState.LeftButton == ButtonState.Released && state.LeftButton == ButtonState.Pressed)
                    {
                        Vector2 clickPosition = detectTileClicked(state.X, state.Y);
                        if (clickPosition.X >= 0 && (clickPosition.X - sourceTile.position.X >= -5 && clickPosition.X - sourceTile.position.X <= 5) && (clickPosition.Y - sourceTile.position.Y <= 5 && clickPosition.Y - sourceTile.position.Y >= -5))
                        {
                            targetTile = board[(int)clickPosition.X, (int)clickPosition.Y];
                            thisPlayer.resources -= NARROWCOST;

                            foreach (Point point in Bresenham.GetPointsOnLine((int)sourceTile.position.X, (int)sourceTile.position.Y, (int)targetTile.position.X, (int)targetTile.position.Y))
                            {

                                for (int x = -1; x <= 1; x++)
                                {
                                    for (int y = -1; y <= 1; y++)
                                    {
                                        try
                                        {
                                            if (board[point.X + x, point.Y + y].faction != thisPlayer.faction)
                                            board[point.X + x, point.Y + y].doDamage(damage.Next(10,50));
                                        }
                                        catch
                                        {

                                        }
                                    }
                                }
                                if (board[point.X, point.Y].faction != thisPlayer.faction)
                                board[point.X, point.Y].doDamage(100);
                            }
                        }
                        exitLoop = true;
                        //selectedTile = null;
                    }

                } while (!exitLoop);
            }
            playerIsInMove = false;
        }
        #endregion
    }
}