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


namespace Arkanoid
{


    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region Oggetti

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Rectangle scoreBar;
        Rectangle paddle;
        Rectangle ball;
        Rectangle[,] rects;
        Rectangle[] Lives;

        #endregion

        #region Textures / Sounds

        Texture2D bgTexture;
        Texture2D paddleTexture;
        Texture2D rectTexture;
        Texture2D ballTexture;

        Color[] RectsColors = { Color.Silver, Color.Red, Color.LightBlue, Color.Yellow, Color.Pink, Color.Green };

        SoundEffect ballBounce;
        SoundEffect ballDeath;
        SoundEffect gameoverSound;

        SpriteFont font; //ADD

        #endregion

        #region Parametri gioco

        private int lives = 3;
        private bool endGame = true;
        private bool pauseGame = false;
        private bool win = false;
        private bool lose = false;
        private bool[,] rectDestroyed;

        private String endMsg;
        private String pauseMsg;

        #endregion

        #region Dimensioni oggetti

        private int wScreen;
        private int hScreen;
        private int xm;

        private int wScore;
        private int hScore = 50;

        private int wPaddle = 70;
        private int hPaddle = 10;

        private int dBall = 16;

        private int wRect;
        private int hRect = 10;
        private int pRect = 3;
        private int nRect = 12;
        private int nRectRows = 6;

        #endregion

        #region Posizioni

        private int padBotDist = 64;
        private int ballPadDist = 6;

        private int rectYstart = 100;

        #endregion

        #region Movimento

        Vector2 ballSpeed;

        private int SPEED = 8;

        #endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 500;
            graphics.PreferredBackBufferHeight = 480;
        }

        protected override void Initialize()
        {
            //Dimensioni schermo di gioco
            wScreen = GraphicsDevice.Viewport.Bounds.Width;
            hScreen = GraphicsDevice.Viewport.Bounds.Height;
            xm = wScreen/2;

            //Larghezza barra punteggio
            wScore = wScreen;

            //Larghezza rettangoli da eliminare
            wRect = (wScreen-(pRect*(nRect/*+1*/)))/nRect;

            scoreBar = new Rectangle(0, 0, wScore, hScore);
            ballSpeed = new Vector2(0, 0);

            InitGame();

            base.Initialize();
        }

        protected void InitGame()
        {
            paddle = new Rectangle(xm - (wPaddle / 2), hScreen - padBotDist, wPaddle, hPaddle);
            ball = new Rectangle(xm - (dBall / 2), hScreen - padBotDist - hPaddle - ballPadDist, dBall, dBall);
            rects = new Rectangle[nRectRows, nRect];
            rectDestroyed = new bool[nRectRows, nRect];
            lives = 3;
            Lives = new Rectangle[lives];

            int x = pRect;
            int y = rectYstart;
            for (int i = 0; i < nRectRows; i++) {
                for (int j = 0; j < nRect; j++) {
                    rects[i, j] = new Rectangle(x, y, wRect, hRect);
                    rectDestroyed[i, j] = false;
                    x += wRect + pRect;
                }
                x = pRect;
                y += hRect + pRect;
            }

            for(int i=0; i<lives; i++) {
                Lives[i] = new Rectangle(
                    wScreen-dBall-pRect-((pRect*(i+1))+(((hScore/2)-(2*pRect))*i)),
                    pRect,
                    (hScore / 2) - (2 * pRect),
                    (hScore / 2) - (2 * pRect)
                ); 
            }

            InitBallSpeed();

            win = false;
            lose = false;
            endGame = true;
            pauseGame = false;
            pauseMsg = "[P]ause";
            endMsg = "Press [Space] to start";
        }

        private void InitBallSpeed()
        {
            Random rand = new Random();
            int speed = 2 * SPEED / 3;
            switch (rand.Next(6))
            {
                //45°
                case 0://D-R
                    ballSpeed.X = speed;
                    ballSpeed.Y = speed;
                    break;
                case 1://U-R
                    ballSpeed.X = -speed;
                    ballSpeed.Y = speed;
                    break;
                //30°
                case 2://D-RR
                    ballSpeed.X = 3 * speed / 2;
                    ballSpeed.Y = speed;
                    break;
                case 3://U-RR
                    ballSpeed.X = -(3 * speed / 2);
                    ballSpeed.Y = speed;
                    break;
                //60°
                case 4:
                    ballSpeed.X = speed;
                    ballSpeed.Y = 3 * speed / 2;
                    break;
                case 5:
                    ballSpeed.X = -speed;
                    ballSpeed.Y = 3 * speed / 2;
                    break;
            }
        }

        protected void RestartGame()
        {
            InitBallSpeed();

            endMsg = "";
            pauseMsg = "[P]ause";
            endGame = false;
        }

        protected void TogglePause()
        {
            if(pauseGame)
            {
                pauseGame = false;
                pauseMsg = "[P]ause";
                if(endGame)
                    endMsg = "Press [Space] to start";
            }
            else
            {
                pauseGame = true;
                pauseMsg = "[C]ontinue [R]estart [E]xit";
                endMsg = "";
            }
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            bgTexture = this.Content.Load<Texture2D>("background");
            paddleTexture = this.Content.Load<Texture2D>("paddle");
            ballTexture = this.Content.Load<Texture2D>("ball");
            rectTexture = this.Content.Load<Texture2D>("rect");

            ballBounce = this.Content.Load <SoundEffect>("BallBounce");
            ballDeath = this.Content.Load <SoundEffect>("BallDeath");
            gameoverSound = this.Content.Load<SoundEffect>("Gameover");

            font = this.Content.Load<SpriteFont>("font");

        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Space) && (endGame) && !(win || lose) && !pauseGame)
                RestartGame();
            if (Keyboard.GetState().IsKeyDown(Keys.R) && (pauseGame || (win || lose)))
                InitGame();

            if ((Keyboard.GetState().IsKeyDown(Keys.C) || Keyboard.GetState().IsKeyDown(Keys.Space)) && pauseGame)
                TogglePause();
            if ((Keyboard.GetState().IsKeyDown(Keys.P) || Keyboard.GetState().IsKeyDown(Keys.Escape)) && !pauseGame && !(win || lose))
                TogglePause();

            if (Keyboard.GetState().IsKeyDown(Keys.E) && (pauseGame || (win || lose)))
            this.Exit();

            if (endGame || pauseGame)
                return;

            MovePaddle();
            MoveBall();

            WinControl();

            base.Update(gameTime);
        }

        protected void MoveBall()
        {
            ball.X += (int)ballSpeed.X;
            ball.Y += (int)ballSpeed.Y;

            //Scontro bord laterali
            if (ball.X <= 0 || ball.X>=wScreen-dBall) {
                ballSpeed.X = -ballSpeed.X;
                
                if (ballSpeed.Y > SPEED)
                    ballSpeed.Y -= 1;
                else if (ballSpeed.Y < -SPEED)
                    ballSpeed.Y += 1;

                ballBounce.Play();
            }
                
            
            //Scontro con paddle
            if (ball.Intersects(paddle))
            {
                ballSpeed.Y = -Math.Abs(ballSpeed.Y);
                ballBounce.Play();
            }
                

            //Scontro bordo superiore
            if (ball.Y <= hScore)
            {
                ballSpeed.Y = -ballSpeed.Y;
                ballBounce.Play();
            }
                

            //Scontro bordo inferiore
            if (ball.Y >= hScreen)
                Die();

            //Scontro con quadrattini
            for (int i = 0; i < nRectRows; i++)
                for (int j = 0; j < nRect; j++)
                    if (ball.Intersects(rects[i, j]))
                    {
                        ballSpeed.Y = -ballSpeed.Y;
                        rects[i, j].X = 2000;
                        rectDestroyed[i, j] = true;
                        ballBounce.Play();
                    }
                        
        }

        protected void MovePaddle()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Right) && paddle.X < wScreen - wPaddle)
                paddle.X += SPEED;
            if (Keyboard.GetState().IsKeyDown(Keys.Left) && paddle.X > 0)
                paddle.X -= SPEED;
        }

        protected void WinControl()
        {
            for (int i = 0; i < nRectRows; i++)
                for (int j = 0; j < nRect; j++)
                    if (!rectDestroyed[i, j])
                        return;
            win = true;
            endMsg = "You Win!";
            pauseMsg = "[R]estart [E]xit";
            endGame = true;
        }

        protected void Die()
        {
            ball.X = (paddle.X + (wPaddle / 2)) - (dBall / 2);
            ball.Y = hScreen - padBotDist - hPaddle - ballPadDist;

            endGame = true;
            endMsg = "Press [Space] to start";
            lives--;
            if (lives <= 0)
                Gameover();
            else
                ballDeath.Play();
        }

        protected void Gameover()
        {
            lose = true;
            endMsg = "You Lose!";
            pauseMsg = "[R]estart [E]xit";
            ball.X = 2000;
            gameoverSound.Play();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkGreen);

            spriteBatch.Begin();
            spriteBatch.Draw(bgTexture, GraphicsDevice.Viewport.Bounds, Color.White);
            spriteBatch.Draw(rectTexture, scoreBar, Color.Black);
            spriteBatch.Draw(paddleTexture, paddle, Color.White);
            spriteBatch.Draw(ballTexture, ball, Color.White);

            for (int i = 0; i < nRectRows; i++)
                for (int j = 0; j < nRect; j++)
                    if(!rectDestroyed[i, j])
                        spriteBatch.Draw(rectTexture, rects[i, j], RectsColors[i]);

            for (int i = 0; i < lives; i++)
                spriteBatch.Draw(ballTexture, Lives[i], Color.White);

            spriteBatch.DrawString(font, ""+endMsg, new Vector2(0, hScore / 2), Color.White); //Aggiungere 8 spazi se si rimuove vite
            spriteBatch.DrawString(font, ""+pauseMsg, new Vector2(0, 0), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}