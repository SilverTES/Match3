using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Retro2D;

namespace Match3
{
    public class GemExplose : Node
    {
        Vector2 _target;
        PlayGrid _playGrid;

        Sprite _sprite = new Sprite();
        public int _color { get; private set; }

        public bool IsDead;

        public bool IsMove; // Status : Gem is move
        public bool IsGoal; // Status : Gem reach goal
        public bool OnGoal; // Trigger : Gem reah goal

        int _ticMove;
        int _durationMove = 32;

        float _fromX;
        float _fromY;
        float _toX;
        float _toY;
        float _ghostX;
        float _ghostY;

        public GemExplose(PlayGrid playGrid, Vector2 target, int mapX, int mapY, int color = Gem.NULL)
        {

            _type = UID.Get<GemExplose>();

            _playGrid = playGrid;
            _target = target;

            SetZ(-10000);
            _x = mapX * Game1.CELL_SIZEW + Game1.CELL_SIZEW / 2;
            _y = mapY * Game1.CELL_SIZEH + Game1.CELL_SIZEH / 2;

            _color = color;

            _sprite.Add(Game1._animation_BallExplose);

            _sprite.Start("Explose",1,0);
        }

        public override Node Init()
        {

            return base.Init();
        }

        public override Node Update(GameTime gameTime)
        {
            _sprite.Update();

            OnGoal = false;

            if (_sprite.OffPlay && !IsDead)
            {
                IsDead = true;
                //KillMe();

                IsMove = true;
                _ticMove = 0;

                _fromX = _x + _parent.AbsX;
                _fromY = _y + _parent.AbsY;

                _toX = _target.X;
                _toY = _target.Y;

            }

            if (IsMove)
            {
                _ghostX = Easing.GetValue(Easing.CircularEaseInOut, _ticMove, _fromX, _toX, _durationMove);
                _ghostY = Easing.GetValue(Easing.CircularEaseInOut, _ticMove, _fromY, _toY, _durationMove);

                _ticMove++;
                if (_ticMove >= _durationMove)
                {
                    _x = _toX;
                    _y = _toY;

                    OnGoal = true;
                    IsGoal = true;
                    IsMove = false;

                }
            }

            if (OnGoal)
            {

                KillMe();
            }

            return base.Update(gameTime);
        }

        public override Node Render(SpriteBatch batch)
        {

            _sprite.Draw(batch, AbsX, AbsY, Gem.Colors[_color]);

            //_sprite.Draw(batch, _target.X, _target.Y, Gem.Colors[_color]);

            if (IsDead)
            {
                //batch.DrawCircle(_ghostX, _ghostY, Game1.CELL_SIZEW / 4, 16, Gem.Colors[_color], 4);

                batch.Draw(
                    Game1._tex_Atlas,
                    new Vector2(_ghostX, _ghostY),
                    new Rectangle(0, 0, 64, 64), Gem.Colors[_color],
                    0,
                    new Vector2(32,32),
                    .5f,
                    SpriteEffects.None,
                    0);

            }

            return base.Render(batch);
        }
    }
}
