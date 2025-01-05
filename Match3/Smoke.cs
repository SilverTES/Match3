using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Retro2D;
using System.Collections.Generic;

namespace Match3
{
    public class Smoke : Node
    {
        Sprite _sprite;

        float _vx;
        float _vy;
        int _density;

        bool _shadow = true;
        bool _zIndex = true;
        float _transparency = 1f;

        List<Vector2> _smokes = new List<Vector2>();
        List<float> _smokeSize = new List<float>();

        public Smoke(float x, float y, int density = 8, bool shadow = true, bool zIndex = true, int rX = 4, int rY = 4, float vx = 0, float vy = 0)
        {

            SetSize(16, 16);
            SetPivot(8, 14);
            _x = x;
            _y = y;
            _density = density;
            _shadow = shadow;
            _vx = vx;
            _vy = vy;

            if (!_zIndex)
                SetZ(-100000);

            for (int i = 0; i < density; i++)
            {
                float sx = Misc.Rng.Next(-rX, rX);
                float sy = Misc.Rng.Next(-rY, rY);

                //float size = 1f + .2f * (Misc.Rng.Next(1, 100) / 100f);
                float size = 1f;

                _smokes.Add(new Vector2(sx, sy));
                _smokeSize.Add(size);
            }

            _sprite = new Sprite();
            _sprite.Add(Game1._animation_smoke);

            _sprite.Start("Idle", 1, 0);
        }

        public override Node Update(GameTime gameTime)
        {
            if (_zIndex)
                _z = -(int)_y;

            _sprite.Update();

            if (_sprite.OffPlay)
            {
                KillMe();
            }

            _transparency -= .01f;

            _x += _vx;
            _y += _vy;

            return this;
        }

        public override Node Render(SpriteBatch batch)
        {
            if (_shadow)
                batch.Draw(Game1._tex_shadow, new Rectangle((int)(AbsX - _oX) + 4, AbsY, 16 - 8, 4), Color.Black * _transparency * .2f); // Shadow

            //_sprite.Draw(batch, AbsX(), AbsY(), Color.White * .5f, 1);

            for (int i = 0; i < _smokes.Count; i++)
            {
                _sprite.Draw(batch, AbsX + _smokes[i].X, AbsY + _smokes[i].Y, Color.White * _transparency, _smokeSize[i], _smokeSize[i]);
            }

            return this;
        }
    }
}
