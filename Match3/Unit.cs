using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Retro2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Match3
{
    public class Unit : Node
    {
        Unit _enemy;
        int _level;

        const int ZONE_BODY = 0; 
        const int ZONE_AGRO = 1;

        List<Collide.Zone> _zoneBody;
        List<Collide.Zone> _zoneAgro;

        public enum State
        {
            IDLE,
            WALK,
            ATTACK,
            HIT,

        }

        Teams _team;
        State _state;
        PlayGrid _playGrid;

        public Color TeamColor; // Color of the Team
        int _gemColor; // Color of Gems  

        Vector2 _moveSpeed;
        Vector2 _target;

        Sprite _sprite;

        int MaxHP;
        int HP;

        int _ticAttack = 0;
        int _tempoAttack = 64;
        bool _onAttack; // Trigger Attack
        int _powerAttack = 2;

        bool IsDead;
        bool OnReach;
        bool OnHit;
        bool IsHit;

        bool CanMoveX = true;

        int Range = 60;

        public Unit(PlayGrid playGrid, int gemColor, Vector2 start, Vector2 target, float speed = 1, int maxHP = 32)
        {
            _type = UID.Get<Unit>();

            _playGrid = playGrid;
            _team = _playGrid.Team;
            _level = _playGrid.Level;
            _gemColor = gemColor;

            _x = start.X;
            _y = start.Y;

            _target = target;

            MaxHP = maxHP;
            HP = MaxHP;

            SetSize(40, 40);
            SetPivot(Position.CENTER);

            SetCollideZone(ZONE_BODY, new RectangleF());
            SetCollideZone(ZONE_AGRO, new RectangleF());

            _moveSpeed = Geo.GetVector(start, target, speed);

            _sprite = Game1._sprite_soldier.Clone();

            _state = State.WALK;
            _sprite.Start("Walk", 1, 0);

            if (_team == Teams.L)
                TeamColor = Color.MonoGameOrange;
            else
                TeamColor = Color.RoyalBlue;
                
        }
        public Unit SetLevel(int level)
        {
            _level = level;
            return this;
        }
        public override Node Init()
        {

            return base.Init();
        }
        public Unit AddHP(int hp)
        {
            HP += hp;

            if (HP <= 0) HP = 0;
            if (HP > MaxHP) HP = MaxHP;

            return this;
        }
        public Unit Walk()
        {
            _state = State.WALK;
            _sprite.Start("Walk", 1, 0);
            return this;
        }
        public Unit Attack()
        {
            _state = State.ATTACK;
            _sprite.Start("Attack", 1, 0);
            return this;
        }

        public override Node Update(GameTime gameTime)
        {
            OnHit = false;
            OnReach = false;

            _onAttack = false;

            _z = (int)-_y;

            if (_state == State.WALK)
            {
                if (CanMoveX) _x += _moveSpeed.X;
                _y += _moveSpeed.Y;
            }

            if (_state == State.ATTACK)
            {
                _ticAttack++;
                if (_ticAttack > _tempoAttack)
                {
                    // test : level determine la vitesse de recharge de l'arme
                    //int bonusSpeedAttack = Misc.Rng.Next(0, _playGrid.Level);

                    _ticAttack = _level * 2;

                    _onAttack = true;
                }

            }

            float scanDirection = _moveSpeed.X > 0 ? 1 : -1;

            Vector2 scanPos = new Vector2(_x + (scanDirection * (Range+_rect.Width/2)), _y);

            UpdateCollideZone(ZONE_BODY, Gfx.AddRect(_rect, -4,4,8,-8));
            UpdateCollideZone(ZONE_AGRO, Gfx.TranslateRect(new Rectangle(-Range, -Area.CellH, Range*2, Area.CellH*2), scanPos));

            _zoneBody = Collision2D.ListCollideZoneByNodeType(GetCollideZone(ZONE_BODY), UID.Get<Unit>(), Unit.ZONE_BODY);
            _zoneAgro = Collision2D.ListCollideZoneByNodeType(GetCollideZone(ZONE_AGRO), UID.Get<Unit>(), Unit.ZONE_BODY);


            CanMoveX = true;

            if (_zoneBody.Count > 0)
            {
                for (int i=0; i<_zoneBody.Count; i++)
                {
                    Unit unit = _zoneBody[i]._node.This<Unit>();

                    // if unit is ally
                    if (unit._team == _team)
                    {
                        //if (_state == State.WALK)
                        {
                            if (_moveSpeed.X > 0 && _x < unit._x && !unit.IsDead) CanMoveX = false;
                            if (_moveSpeed.X < 0 && _x > unit._x && !unit.IsDead) CanMoveX = false;
                        }

                    }

                }

            }

            //if (CanMoveX)
            //    _sprite.Resume();
            //else
            //    _sprite.Pause();

            if (_zoneAgro.Count > 0)
            {
                for (int i = 0; i < _zoneAgro.Count; i++)
                {
                    Unit unit = _zoneAgro[i]._node.This<Unit>();

                    // first enemy enter agro zone
                    if (null == _enemy)
                    {
                        // if unit is enemy
                        if (unit._team != _team)
                        {
                            _enemy = unit;
                            Attack();
                        }
                    }

                    // Change target priority if enemy is on same line color
                    if (null != _enemy && _enemy._y != _y)
                    {
                        if (unit._y == _y && unit._team != _team)
                        {
                            _enemy = unit;
                            Attack();

                        }
                    }
                }

            }
            else
            {
                if (null != _enemy)
                {
                    _enemy = null;
                    Walk();
                }
            }



            if (null != _enemy)
            {
                if (_enemy.IsDead)
                {
                    _enemy = null;

                    Walk();
                }

            }

            // Attack enemy and do Damage
            if (_onAttack)
            {
                if (null != _enemy)
                {
                    //Console.WriteLine("Attack {0}", _enemy._index);
                    _enemy.AddHP(-((_powerAttack+_level)*8));

                    _enemy.IsHit = true;

                    Game1.PlaySound(0, Game1._sound_ShootFireGun, .02f, .06f);

                    new Smoke(_enemy._x, _enemy._y, 8, false)
                        .AppendTo(_parent);
                }
            }

            if (IsHit)
            {
                IsHit = false;
                OnHit = true;
            }

            if (HP == 0)
            {
                IsDead = true;
            }


            // Unit Reach enemy base !
            if (Geo.IsNear(XY, _target, 10))
            {
                OnReach = true;
                //IsDead = true;

                foreach (var opponent in _playGrid._opponents)
                {
                    opponent._hero.AddEnergy(-_powerAttack);
                    opponent.Shake(16, .2f);

                    Game1._sound_Punch.Play(.02f, .05f, 0);
                }

                KillMe();
            }

            if (IsDead)
            {
                int index = Misc.Rng.Next(0, 4);

                Game1.PlaySound(1, Game1._sounds_hurt[index], .2f, .01f);

                new FireExplosion(_x, _y)
                    .AppendTo(_parent);

                new Blood(_x, _y)
                    .AppendTo(_parent);

                KillMe();
            }


            UpdateRect();

            _sprite.Update();

            return base.Update(gameTime);
        }

        public override Node Render(SpriteBatch batch)
        {
            //Draw.RectFill(batch, (Rectangle)Gfx.TranslateRect(GetCollideZone(ZONE_AGRO)._rect, _parent.AbsXY), Color.Red * .2f);

            //if (null != _enemy)
            //    batch.DrawLine(AbsXY, _enemy.AbsXY, Color.Green);

            //Draw.RectFill(batch, AbsRect, Color.Red);
            //batch.DrawRectangle(AbsRect, Color.Yellow, 1);
            Draw.Ellipse(batch, AbsX, AbsY, 12, 6, 6, Gem.Colors[_gemColor], 2);

            if (OnHit)
                _sprite.Draw(batch, AbsX, AbsY, Color.Black, 1,1,0,0,0, _moveSpeed.X > 0 ? SpriteEffects.None: SpriteEffects.FlipHorizontally);
            else
                _sprite.Draw(batch, AbsX, AbsY, TeamColor, 1,1,0,0,0, _moveSpeed.X > 0 ? SpriteEffects.None: SpriteEffects.FlipHorizontally);

            //Draw.CenterStringXY(batch, Game1._font_Main, _state.ToString() + ":" + _owner, AbsX, AbsY - 64, Color.OrangeRed);
            //Draw.CenterStringXY(batch, Game1._font_Main, "HP =" + HP, AbsX, AbsY - 64, Color.OrangeRed);
            //Draw.CenterStringXY(batch, Game1._font_Main, _level.ToString(), AbsX, AbsY - 64, Color.White);

            //if (GetCollideZone(ZONE_BODY)._isCollide)
            //    batch.DrawRectangle(Gfx.TranslateRect(GetCollideZone(ZONE_BODY)._rect, _parent.AbsXY), Color.Yellow, 1);

            // Draw level point
            //float x = AbsX - (_level * 3) / 2;
            //float y = AbsY - 32;

            //batch.DrawLine(x, y, x + (_level-1) * 3, y, Color.Black, 4);

            //for (int i=0; i< _level - 1; i++)
            //{
            //    batch.DrawPoint(new Vector2(x + i * 3, y), Color.White, 2);
            //}

            Draw.CenterBorderedStringXY(batch, Game1._font_Mini, _level.ToString(), AbsX, AbsY - 36, Color.White, Color.Black);


            if (_onAttack)
            {
                //batch.DrawRectangle(Gfx.TranslateRect(GetCollideZone(ZONE_BODY)._rect, _parent.AbsXY), Color.Yellow, 2);
                _sprite.Draw(batch, AbsX, AbsY, Color.White, 1, 1, 0, 0, 0, _moveSpeed.X > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            }

            Rectangle rectMAXHP = (Rectangle)Gfx.TranslateRect(new Rectangle(-MaxHP / 2, - 30, MaxHP, 4), AbsXY);
            Rectangle rectHP = new Rectangle(rectMAXHP.X, rectMAXHP.Y, HP, 4);

            Draw.FillRectangle(batch, rectMAXHP, Color.Red);
            Draw.FillRectangle(batch, rectHP, Color.LightGreen);
            Draw.Rectangle(batch, rectMAXHP, Color.Black);

            return base.Render(batch);
        }
    }
}
