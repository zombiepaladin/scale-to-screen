using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ScaleToScreen
{
    public enum DisplayStrategy
    {
        ScaleToFit,
        ScaleToCover
    }

    public enum GameResolution
    {
        FourToThree,
        SixteenToNine
    }

    public class ScaleToScreenGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private KeyboardState _priorKeyboardState;
        private SpriteFont _font;
        private DisplayStrategy _displayStrategy;
        private GameResolution _gameResolution;
        private Texture2D _standard;
        private Texture2D _widescreen;
        private float _gameScale;
        private Vector2 _gameOffset;

        public ScaleToScreenGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            // Use full-screen at screen resolution
            DisplayMode screen = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            _graphics.IsFullScreen = true;
            _graphics.PreferredBackBufferWidth = screen.Width;
            _graphics.PreferredBackBufferHeight = screen.Height;
            //_graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _gameResolution = GameResolution.FourToThree;
            _displayStrategy = DisplayStrategy.ScaleToCover;
            DetermineScreenSize();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            _font = Content.Load<SpriteFont>("DisplayFont");
            _standard = Content.Load<Texture2D>("1024x768");
            _widescreen = Content.Load<Texture2D>("1920x1080");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var keyboardState = Keyboard.GetState();

            // Toggle game resolution 
            if(keyboardState.IsKeyDown(Keys.R) && _priorKeyboardState.IsKeyUp(Keys.R))
            {
                _gameResolution++;
                if ((int)_gameResolution >= 2) _gameResolution = 0;
                DetermineScreenSize();
            }

            // Toggle display strategy 
            if(keyboardState.IsKeyDown(Keys.S) && _priorKeyboardState.IsKeyUp(Keys.S))
            {
                _displayStrategy++;
                if ((int)_displayStrategy >= 2) _displayStrategy = 0;
                DetermineScreenSize();
            }

            _priorKeyboardState = keyboardState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Determine the necessary transform to scale and position game on-screen
            Matrix transform =                 
                Matrix.CreateScale(_gameScale) * // Scale the game to screen size 
                Matrix.CreateTranslation(_gameOffset.X, _gameOffset.Y, 0); // Translate game to letterbox position

            // Draw the game using SpriteBatch
            _spriteBatch.Begin(transformMatrix: transform);
            switch(_gameResolution)
            {
                case GameResolution.FourToThree:
                    _spriteBatch.Draw(_standard, Vector2.Zero, Color.White);
                    break;
                case GameResolution.SixteenToNine:
                    _spriteBatch.Draw(_widescreen, Vector2.Zero, Color.White);
                    break;
            }
            _spriteBatch.End();

            // Draw the UI without any transformations
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_font, $"Monitor Resolution: {_graphics.GraphicsDevice.Viewport.Width}x{_graphics.GraphicsDevice.Viewport.Height}", new Vector2(50, 50), Color.White);
            _spriteBatch.DrawString(_font, $"Current Display Strategy: {_displayStrategy} (Press S to change)", new Vector2(50, 80), Color.White);
            _spriteBatch.DrawString(_font, $"Current Game Aspect Ratio: {_gameResolution} (Press R to change)", new Vector2(50, 110), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        public void DetermineScreenSize()
        {
            Viewport screen = _graphics.GraphicsDevice.Viewport;
            Viewport game;

            // Determine game size based on selected resolution
            switch(_gameResolution)
            {
                case GameResolution.FourToThree:
                    game = new Viewport(0, 0, 1024, 768);
                    break;
                case GameResolution.SixteenToNine:
                default:
                    game = new Viewport(0, 0, 1920, 1080);
                    break;
            }

            // Determine game viewport scaling and positioning based on selected display strategy

            switch (_displayStrategy)
            {
                case DisplayStrategy.ScaleToFit:
                    // 1. Determine which dimension must have letterboxing
                    if (screen.AspectRatio < game.AspectRatio)
                    {
                        // letterbox vertically
                        // Scale game to screen width
                        _gameScale = (float)screen.Width / game.Width;
                        // translate vertically
                        _gameOffset.Y = (screen.Height - game.Height * _gameScale) / 2f;
                        _gameOffset.X = 0;
                    }
                    else
                    {
                        // letterbox horizontally
                        // Scale game to screen height 
                        _gameScale = (float)screen.Height / game.Height;
                        // translate horizontally
                        _gameOffset.X = (screen.Width - game.Width * _gameScale) / 2f;
                        _gameOffset.Y = 0;
                    }
                    break;

                case DisplayStrategy.ScaleToCover:
                    // 1. Determine which dimension must overflow screen 
                    if(screen.AspectRatio < game.AspectRatio)
                    {
                        // overflow horizontally
                        // Scale game to screen height 
                        _gameScale = (float)screen.Height / game.Height;
                        // translate horizontally 
                        _gameOffset.X = (screen.Width - game.Width * _gameScale) / 2f;
                        _gameOffset.Y = 0;
                    }
                    else
                    {
                        // overflow vertically
                        // Scale game to screen width 
                        _gameScale = (float)screen.Width / game.Width;
                        // translate vertically
                        _gameOffset.Y = (screen.Height - game.Height * _gameScale) / 2f;
                        _gameOffset.X = 0;
                    }
                    break;
            }
            
        }

    }
}
