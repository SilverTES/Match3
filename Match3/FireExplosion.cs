using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Retro2D;

namespace Match3
{
    class FireExplosion : Node
    {

        Sprite _sprite = new Sprite();

        public FireExplosion(float x, float y)
        {
            SetPosition(x, y);
            SetSize(32, 32);
            SetPivot(16, 24);

            _sprite.Add(Game1._animation_fireExplosion);

            _sprite.Start("Explosion", 1, 0);

            //map2D.Get(mapX, mapY)._type = _type;
            //GoTo(mapX, mapY);

            //Game1._sound_killBlock.Play(.2f, .001f, 0);
            //Game1._sound_explosion.Play(.2f, .01f, 0);

            SetCollideZone(0, _rect);
        }

        public override Node Update(GameTime gameTime)
        {
            UpdateRect();
            UpdateCollideZone(0, Gfx.AddRect(_rect, new Rectangle(5, 10, -10, -20)));

            _z = -(int)_y - 10; // Explosion is over everything on the same line !

            _sprite.Update();

            if (_sprite.OffPlay)
                KillMe();

            return this;
        }

        public override Node Render(SpriteBatch batch)
        {

            _sprite.Draw(batch, AbsX, AbsY, Color.White);

            return this;
        }

    }
}
