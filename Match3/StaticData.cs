using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Retro2D;
using static Retro2D.Controller;

namespace Match3
{
    public partial class Game1
    {
        public const int PLAYER1 = 0;
        public const int PLAYER2 = 1;
        public const int PLAYER3 = 2;
        public const int PLAYER4 = 3;
        public const int MAX_PLAYER = 4;


        
        public static Player[] _players = new Player[MAX_PLAYER];
        // Alias for _players[]
        public static Player Player1 { get; private set; }
        public static Player Player2 { get; private set; }
        public static Player Player3 { get; private set; }
        public static Player Player4 { get; private set; }

       public static Controller[] _controllers = new Controller[MAX_PLAYER];


        public static Window _window = new Window();
        public static SpriteBatch _batch;
        public static FrameCounter _frameCounter = new FrameCounter();
        public static MouseState _mouseState;
        public static int _relMouseX;
        public static int _relMouseY;
        public static int _screenW = 1920;
        public static int _screenH = 1080;

        public const int CELL_SIZEW = 64;
        public const int CELL_SIZEH = 64;

        public static SpriteFont _font_Main;
        public static SpriteFont _font_Big;
        public static SpriteFont _font_Mini;


        static public Texture2D _tex_shadow;
        static public Texture2D _tex_smoke;
        public static Texture2D _tex_Atlas;
        public static Texture2D _tex_Soldier;
        static public Texture2D _tex_fireExplosion;
        static public Texture2D _tex_blood;
        static public Texture2D _tex_glow1;

        public static SoundEffect _sound_Tic;
        public static SoundEffect _sound_Bubble;
        public static SoundEffect _sound_Pop;
        public static SoundEffect _sound_Hit;
        public static SoundEffect _sound_BlockHit;
        public static SoundEffect _sound_LevelUp;
        public static SoundEffect _sound_ShareGem;
        public static SoundEffect _sound_Punch;
        public static SoundEffect _sound_Speed;
        public static SoundEffect _sound_ShootFireGun;

        public static List<SoundEffect> _sounds_hurt = new List<SoundEffect>();

        public static Song _song_Music0;
        public static Song _song_Music1;
        public static Song _song_Music2;
        public static Song _song_Music3;
        public static Song _song_Music4;
        public static Song _song_Music5;

        public static Animation _animation_BallInCursor;
        public static Animation _animation_BallExplose;
        public static Animation _animation_smoke;
        public static Animation _animation_fireExplosion;

        public static Sprite _sprite_soldier;

        public static string _pathGamePadSetup = "controllers_Setup.xml";

        const int MAX_CHANNEL_SOUND = 8;
        const int MAX_PLAYING_SOUND = 8;
        static SoundEffectInstance[,] _soundInstances = new SoundEffectInstance[MAX_CHANNEL_SOUND, MAX_PLAYING_SOUND];

        public static void PlaySound(int channel, SoundEffect sound, float volume = 1f, float pitch = 1f, float pan = 0f, bool isLooped = false)
        {
            int index = 0;

            bool find = false; // find free instance in array

            // find a null instance in array and set it
            for (int i=0; i<_soundInstances.GetLength(1); i++)
            {
                if (_soundInstances[channel,i] == null)
                {
                    index = i;
                    find = true;
                    break;
                }
            }

            if (find)
            {
                SoundEffectInstance soundInstance = sound.CreateInstance();

                soundInstance.Volume = volume;
                soundInstance.Pitch = pitch;
                soundInstance.Pan = pan;
                soundInstance.IsLooped = isLooped;

                soundInstance.Play();

                _soundInstances[channel, index] = soundInstance;
            }

        }
        public static void UpdatePlaySound()
        {
            for (int c=0; c<_soundInstances.GetLength(0); c++)
            {
                for (int i=0; i< _soundInstances.GetLength(1); i++)
                {
                    if (null != _soundInstances[c,i])
                        if (_soundInstances[c,i].State == SoundState.Stopped)
                        {
                            //Console.WriteLine("Delete Sound Instance");
                            _soundInstances[c,i] = null;
                        }
                }
            }
        }


        public static void SaveGamePadSetupToFile(string path)
        {
            List<Controller> controllers = new List<Controller>()
            {
                _controllers[PLAYER1],
                _controllers[PLAYER2],
                _controllers[PLAYER3],
                _controllers[PLAYER4]
            };

            FileIO.XmlSerialization.WriteToXmlFile(path, controllers);

            Console.WriteLine("GamePad File Setup Saved !");
        }

        public static void LoadGamePadSetupFromFile(string path)
        {
            List<Controller> controllers = FileIO.XmlSerialization.ReadFromXmlFile<List<Controller>>(path);

            for (int i=0; i<MAX_PLAYER; i++)
            {
                _controllers[i].Copy(controllers[i]);
            }
            Console.WriteLine("GamePad File Setup Loaded !");
        }

        protected override void LoadContent()
        {
            _font_Main = Content.Load<SpriteFont>("SpriteFont/mainFont");
            _font_Big = Content.Load<SpriteFont>("SpriteFont/bigFont");
            _font_Mini = Content.Load<SpriteFont>("SpriteFont/miniFont");

            _tex_shadow = Content.Load<Texture2D>("Texture2D/shadow");
            _tex_smoke = Content.Load<Texture2D>("Texture2D/smoke");

            _tex_Atlas = Content.Load<Texture2D>("Texture2D/atlas");
            _tex_Soldier = Content.Load<Texture2D>("Texture2D/soldier");
            _tex_blood = Content.Load<Texture2D>("Texture2D/blood_64");
            _tex_fireExplosion = Content.Load<Texture2D>("Texture2D/fire_bomb_explosion");

            _tex_glow1 = Content.Load<Texture2D>("Texture2D/glow1");

            _sound_Tic = Content.Load<SoundEffect>("Sound/clock");
            _sound_Bubble = Content.Load<SoundEffect>("Sound/bubble1");
            _sound_Pop = Content.Load<SoundEffect>("Sound/mouseover");
            _sound_Hit = Content.Load<SoundEffect>("Sound/wood_hit");
            _sound_BlockHit = Content.Load<SoundEffect>("Sound/blockhit");
            _sound_LevelUp = Content.Load<SoundEffect>("Sound/coins_purchase_4");
            _sound_ShareGem = Content.Load<SoundEffect>("Sound/bomb");
            _sound_Punch = Content.Load<SoundEffect>("Sound/punch");
            _sound_Speed = Content.Load<SoundEffect>("Sound/okay");
            _sound_ShootFireGun = Content.Load<SoundEffect>("Sound/shoot_fire_gun");


            for (int i=0; i<5; i++)
            {
                SoundEffect soundHurt = Content.Load<SoundEffect>("Sound/Hurt/hurt0" + (i + 1));
                _sounds_hurt.Add(soundHurt);
            }

            //_sounds_hurt.Add(Content.Load<SoundEffect>("Sound/Hurt/hurt01"));
            //_sounds_hurt.Add(Content.Load<SoundEffect>("Sound/Hurt/hurt02"));
            //_sounds_hurt.Add(Content.Load<SoundEffect>("Sound/Hurt/hurt03"));
            //_sounds_hurt.Add(Content.Load<SoundEffect>("Sound/Hurt/hurt04"));
            //_sounds_hurt.Add(Content.Load<SoundEffect>("Sound/Hurt/hurt05"));

            _song_Music0 = Content.Load<Song>("Music/Fandream");
            _song_Music1 = Content.Load<Song>("Music/SPACE34E");
            _song_Music2 = Content.Load<Song>("Music/music3");
            _song_Music3 = Content.Load<Song>("Music/RESP-Z2");
            _song_Music4 = Content.Load<Song>("Music/steel_chambers_2");
            _song_Music5 = Content.Load<Song>("Music/spacedeb");
        }

        void InitStatic()
        {

            // Animation fire explosion
            _animation_fireExplosion = new Animation(_tex_fireExplosion, "Explosion").SetLoop(Loops.NONE);
            Rectangle rect = new Rectangle(0, 0, 40, 60);
            for (int i = 0; i < 12; i++) _animation_fireExplosion.Add(new Frame(new Rectangle(i * 40, 0, 40, 60), 1, 20, 50, rect));
            for (int i = 0; i < 12; i++) _animation_fireExplosion.Add(new Frame(new Rectangle(i * 40, 60, 40, 60), 1, 20, 50, rect));

            // Animation smoke 
            rect = new Rectangle(0, 0, 16, 16);
            _animation_smoke = new Animation(_tex_smoke, "Idle").SetLoop(Loops.NONE)
                .Add(new Frame(new Rectangle(0, 0, 16, 16), 4, 8, 14, rect))
                .Add(new Frame(new Rectangle(16, 0, 16, 16), 4, 8, 14, rect))
                .Add(new Frame(new Rectangle(32, 0, 16, 16), 4, 8, 14, rect))
                .Add(new Frame(new Rectangle(48, 0, 16, 16), 4, 8, 14, rect))
                .Add(new Frame(new Rectangle(0, 16, 16, 16), 4, 8, 14, rect))
                .Add(new Frame(new Rectangle(16, 16, 16, 16), 4, 8, 14, rect))
                .Add(new Frame(new Rectangle(32, 16, 16, 16), 4, 8, 14, rect))
                .Add(new Frame(new Rectangle(48, 16, 16, 16), 4, 8, 14, rect));


            _animation_BallInCursor = new Animation(_tex_Atlas, "Indicator");
            for (int i = 0; i < 4; i++)
            {
                Frame frame = new Frame().SetSrcPosition(i * 128, 128);
                frame
                    //.SetDuration(1)
                    .SetSrcSize(128, 128)
                    .SetPivot(64, 64)
                    .SetDestSize(128, 128);

                _animation_BallInCursor.Add(frame);
            }

            _animation_BallExplose = new Animation(_tex_Atlas, "Explose");
            for (int i = 0; i < 14; i++)
            {
                Frame frame = new Frame().SetSrcPosition(i * 64, 64);
                frame
                    //.SetDuration(1)
                    .SetSrcSize(64, 64)
                    .SetPivot(32, 32)
                    .SetDestSize(CELL_SIZEW, CELL_SIZEH);

                if (i == 0) frame.SetDuration(2);
                if (i == 1) frame.SetDuration(8);

                _animation_BallExplose.Add(frame);
            }


            Animation soldierWalk = new Animation(_tex_Soldier, "Walk").SetLoop(Loops.REPEAT);
            for (int i = 0; i < 5; i++)
            {
                Frame frame = new Frame().SetSrcPosition(i * 32, 0);
                frame
                    .SetDuration(6)
                    .SetSrcSize(32, 32)
                    .SetPivot(16, 28)
                    .SetDestSize(32, 32);

                soldierWalk.Add(frame);
            }
            Animation soldierAttack = new Animation(_tex_Soldier, "Attack").SetLoop(Loops.REPEAT);
            for (int i = 0; i < 3; i++)
            {
                Frame frame = new Frame().SetSrcPosition(i * 32, 64);
                frame
                    .SetDuration(8)
                    .SetSrcSize(32, 32)
                    .SetPivot(16, 28)
                    .SetDestSize(32, 32);

                soldierAttack.Add(frame);
            }
            _sprite_soldier = new Sprite();
            soldierWalk.AppendTo(_sprite_soldier);
            soldierAttack.AppendTo(_sprite_soldier);


            Player1 = _players[PLAYER1] = new Player(0, "Mugen");
            Player2 = _players[PLAYER2] = new Player(0, "Silver");
            Player3 = _players[PLAYER3] = new Player(0, "Zero");
            Player4 = _players[PLAYER4] = new Player(0, "Alpha");

            _controllers[PLAYER1] = new Controller()
                .AsMainController(Player1);

            new Controller()
                .SetButton(new Button((int)SNES.BUTTONS.UP, (int)Keys.Up, 0, 0, 0, 0, -1))
                .SetButton(new Button((int)SNES.BUTTONS.DOWN, (int)Keys.Down, 0, 0, 0, 0, -1))
                .SetButton(new Button((int)SNES.BUTTONS.LEFT, (int)Keys.Left, 0, 0, 0, 0, -1))
                .SetButton(new Button((int)SNES.BUTTONS.RIGHT, (int)Keys.Right, 0, 0, 0, 0, -1))

                .SetButton(new Button((int)SNES.BUTTONS.A, (int)Keys.RightAlt, 0, 0, 0, 0, -1))
                .SetButton(new Button((int)SNES.BUTTONS.B, (int)Keys.Space, 0, 0, 0, 0, -1))

                .SetButton(new Button((int)SNES.BUTTONS.START, (int)Keys.Enter, 0, 0, 0, 0, -1))
                .SetButton(new Button((int)SNES.BUTTONS.SELECT, (int)Keys.Back, 0, 0, 0, 0, -1))
                .AppendTo(_players[PLAYER1]);

            _controllers[PLAYER2] = new Controller()
                .AsMainController(Player2);

            new Controller()
                .SetButton(new Button((int)SNES.BUTTONS.UP, (int)Keys.Z, 0, 0, 0, 0, -1))
                .SetButton(new Button((int)SNES.BUTTONS.DOWN, (int)Keys.S, 0, 0, 0, 0, -1))
                .SetButton(new Button((int)SNES.BUTTONS.LEFT, (int)Keys.Q, 0, 0, 0, 0, -1))
                .SetButton(new Button((int)SNES.BUTTONS.RIGHT, (int)Keys.D, 0, 0, 0, 0, -1))

                .SetButton(new Button((int)SNES.BUTTONS.A, (int)Keys.D1, 0, 0, 0, 0, -1))
                .SetButton(new Button((int)SNES.BUTTONS.B, (int)Keys.D2, 0, 0, 0, 0, -1))
                .AppendTo(_players[PLAYER2]);

            _controllers[PLAYER3] =
            new Controller()
                .SetButton(new Button((int)SNES.BUTTONS.UP, (int)Keys.O, 0, 0, 0, 0, -1))
                .SetButton(new Button((int)SNES.BUTTONS.DOWN, (int)Keys.L, 0, 0, 0, 0, -1))
                .SetButton(new Button((int)SNES.BUTTONS.LEFT, (int)Keys.K, 0, 0, 0, 0, -1))
                .SetButton(new Button((int)SNES.BUTTONS.RIGHT, (int)Keys.M, 0, 0, 0, 0, -1))

                .SetButton(new Button((int)SNES.BUTTONS.A, (int)Keys.NumPad4, 0, 0, 0, 0, -1))
                .SetButton(new Button((int)SNES.BUTTONS.B, (int)Keys.Space, 0, 0, 0, 0, -1))
                //.AppendTo(_players[PLAYER3]);
                .AsMainController(Player3);

            _controllers[PLAYER4] =
            new Controller()
                .SetButton(new Button((int)SNES.BUTTONS.UP, (int)Keys.T, 0, 0, 0, 0, -1))
                .SetButton(new Button((int)SNES.BUTTONS.DOWN, (int)Keys.G, 0, 0, 0, 0, -1))
                .SetButton(new Button((int)SNES.BUTTONS.LEFT, (int)Keys.F, 0, 0, 0, 0, -1))
                .SetButton(new Button((int)SNES.BUTTONS.RIGHT, (int)Keys.H, 0, 0, 0, 0, -1))

                .SetButton(new Button((int)SNES.BUTTONS.A, (int)Keys.NumPad6, 0, 0, 0, 0, -1))
                .SetButton(new Button((int)SNES.BUTTONS.B, (int)Keys.Space, 0, 0, 0, 0, -1))
                //.AppendTo(_players[PLAYER4]);
                .AsMainController(Player4);


            Console.WriteLine( "Number joystick connected : "+ Controller.NumJoystick());

            // Load GamePad Setup from file
            LoadGamePadSetupFromFile(_pathGamePadSetup);

        }

        public static void Trigger(ref bool ON, ref bool IS, ref bool ACTION)
        {
            ON = false; if (!IS) ACTION = false; if (IS && !ACTION) { ACTION = true; ON = true; }
        }

        protected override void UnloadContent()
        {
        }
    }
}
