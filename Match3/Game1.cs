using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Retro2D;

using static Retro2D.Node;

namespace Match3
{
    public partial class Game1 : Game
    {
        Mode _mode = Mode.NORMAL;

        public Game1()
        {
            _window.Setup(this, _mode, "Minimal", _screenW, _screenH, .5f, 0, false, true, false);
            Content.RootDirectory = "Content";
        }
        protected override void Initialize()
        {
            base.Initialize();

            _window.Init(_font_Main);
            _window.SetScale(2);
            _batch = _window.Batch;

            //_window.SetFinalScreenSize(960, 540);

            InitStatic();

            _root["ScreenPlay"] = new ScreenPlay().AppendTo(_root);
            Screen.Init(_root["ScreenPlay"]);
        }

        protected override void Update(GameTime gameTime)
        {
            _window.GetMouse(ref _relMouseX, ref _relMouseY, ref _mouseState);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Input.Button.OnePress("ModeChange", Keyboard.GetState().IsKeyDown(Keys.F10)))
            {
                if (_mode == Mode.NORMAL)
                    _window.SetMode(_mode = Mode.RETRO);
                else
                    _window.SetMode(_mode = Mode.NORMAL);

            }

            _window.UpdateStdWindowControl();
            _frameCounter.Update(gameTime);
            
            Window.Title = "Puzzle Army V 0.1 : FPS :" + _frameCounter.Fps();

            Screen.Update(gameTime);

            Game1.UpdatePlaySound();

            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            //_window.BeginRender();

            _window.SetRenderTarget(_window.NativeRenderTarget);
            _batch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp);

            Screen.Render(_batch);

            //Draw.Sight(batch, Game1._relMouseX, Game1._relMouseY, Game1._screenW, Game1._screenH, Color.RoyalBlue, 1);

            _batch.Draw(Retro2D.Draw._mouseCursor, new Vector2(_relMouseX, _relMouseY), Color.Red);

            _batch.End();


            _batch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp);

            Screen.RenderAdditive(_batch);

            _batch.End();

            // Render MainTarget in FinalTarget
            _window.SetRenderTarget(_window.FinalRenderTarget);
            _batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp);

            _window.RenderMainTarget(Color.White);

            _window.Batch.End();


            _window.SetRenderTarget(null);
            //_window._graphics.GraphicsDevice.Clear(Color.Transparent);

            _batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp);

            _window.RenderFinalTarget(Color.White);

            _batch.End();

            //_window.EndRender();

            base.Draw(gameTime);
        }
    }
}