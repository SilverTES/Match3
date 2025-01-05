using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Match3
{
    public class Hero
    {
        public enum Modes
        {
            ATTACK,
            DEFENSE,
            MANA,
            CHIMERA,
        }

        public enum Avatars
        {
            BOY,
            GIRL
        }

        public Modes Mode { get; private set; }

        public int Energy { get; private set; }
        public int MaxEnergy { get; private set; }

        Dictionary<Modes, bool> _isActiveModes = new Dictionary<Modes, bool>();

        Dictionary<Modes, int> _maxModePoints = new Dictionary<Modes, int>()
        {
            { Modes.ATTACK, 20 },
            { Modes.DEFENSE, 20 },
            { Modes.MANA, 40 },
            { Modes.CHIMERA, 40 },
        };
        Dictionary<Modes, int> _modePoints = new Dictionary<Modes, int>();

        public Avatars Avatar;

        public Hero(int maxEnergy, Avatars avatar)
        {
            MaxEnergy = maxEnergy;

            Avatar = avatar;
            
            // Create : Mode & status active Mode
            foreach (Modes it in Enum.GetValues(typeof(Modes)))
            {
                _modePoints.Add(it, 0);
                _isActiveModes.Add(it, false);
            }


            Init();
        }

        public void Init()
        {
            Energy = MaxEnergy;

            // Reset Value : Mode & status active Mode
            foreach (Modes it in Enum.GetValues(typeof(Modes)))
            {
                _modePoints[it] = 0;
                _isActiveModes[it] = false;
            }

        }

        public int GetModePoint(Modes mode)
        {
            if (_modePoints.ContainsKey(mode))
                return _modePoints[mode];

            return Retro2D.Const.NoIndex;
        }

        public bool AddModePoint(Modes mode, int points)
        {
            _modePoints[mode] += points;

            if (_modePoints[mode] >= _maxModePoints[mode]) // Active Mode if reach MaxPoints
            {
                _modePoints[mode] = 0;
                _isActiveModes[mode] = true;

                Game1._sound_Punch.Play(.2f, .5f, 0);

                return true;
            }
            return false;
        }

        public Hero SetMode(Modes mode)
        {
            Mode = mode;
            return this;
        }

        public Hero AddEnergy(int energyPoint)
        {
            Energy += energyPoint;
            if (Energy < 0) Energy = 0;
            if (Energy > MaxEnergy) Energy = MaxEnergy;

            return this;
        }

        public Hero Draw(SpriteBatch batch)
        {
            

            return this;
        }

    }
}
