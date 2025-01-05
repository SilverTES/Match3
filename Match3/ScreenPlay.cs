using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Retro2D;
using System;
using System.Collections.Generic;

namespace Match3
{
    class ScreenPlay : Node
    {
        bool IsPause;

        public Area Area;

        public PlayGrid PlayGrid1;
        public PlayGrid PlayGrid2;
        public PlayGrid PlayGrid3;
        public PlayGrid PlayGrid4;

        PlayGrid[] _playGrids = new PlayGrid[Game1.MAX_PLAYER];

        static int PlayGridW;

        // Song List !
        public List<Song> _listSong = new List<Song>();
        protected int _currentSongIndex = 0;

        bool _playMusic = false;

        public Song NextSong()
        {
            _currentSongIndex++;
            if (_currentSongIndex > _listSong.Count - 1) _currentSongIndex = 0;

            return _listSong[_currentSongIndex];
        }
        public void PlaySong(bool repeat = false)
        {
            MediaPlayer.Play(_listSong[_currentSongIndex]);
            MediaPlayer.IsRepeating = repeat;
        }


        public ScreenPlay()
        {
            Area = (Area)new Area(47,9)
                .SetPosition(20,48)
                .AppendTo(this);

            // Level Playlist 
            _listSong.Add(Game1._song_Music0);
            _listSong.Add(Game1._song_Music1);
            _listSong.Add(Game1._song_Music2);
            _listSong.Add(Game1._song_Music3);
            _listSong.Add(Game1._song_Music4);
            _listSong.Add(Game1._song_Music5);

            PlayGridW = Game1._screenW / 4;

            int posY = 8 * Game1.CELL_SIZEH - Game1.CELL_SIZEH / 4;
            int level = 1;

            PlayGrid1 = _playGrids[Game1.PLAYER1] = (PlayGrid)new PlayGrid(Area, Game1.Player1, PlayGridW * 0 + 16, posY).SetLevel(level)
                .AppendTo(this);

            PlayGrid2 = _playGrids[Game1.PLAYER2] = (PlayGrid)new PlayGrid(Area, Game1.Player2, PlayGridW * 1 + 16, posY).SetLevel(level)
                .AppendTo(this);

            PlayGrid3 = _playGrids[Game1.PLAYER3] = (PlayGrid)new PlayGrid(Area, Game1.Player3, PlayGridW * 2 + 16, posY).SetLevel(level)
                .AppendTo(this);

            PlayGrid4 = _playGrids[Game1.PLAYER4] = (PlayGrid)new PlayGrid(Area, Game1.Player4, PlayGridW * 3 + 16, posY).SetLevel(level)
                .AppendTo(this);

            // Set partner for each players
            PlayGrid1.SetPartner(PlayGrid2).AddOpponents(new PlayGrid[] { PlayGrid3, PlayGrid4 });
            PlayGrid2.SetPartner(PlayGrid1).AddOpponents(new PlayGrid[] { PlayGrid3, PlayGrid4 });
            PlayGrid3.SetPartner(PlayGrid4).AddOpponents(new PlayGrid[] { PlayGrid1, PlayGrid2 });
            PlayGrid4.SetPartner(PlayGrid3).AddOpponents(new PlayGrid[] { PlayGrid1, PlayGrid2 });

            PlayGrid1.SetTeam(Teams.L).SetAvatar(Hero.Avatars.BOY).Init();
            PlayGrid2.SetTeam(Teams.L).SetAvatar(Hero.Avatars.GIRL).Init();
            PlayGrid3.SetTeam(Teams.R).SetAvatar(Hero.Avatars.BOY).Init();
            PlayGrid4.SetTeam(Teams.R).SetAvatar(Hero.Avatars.GIRL).Init();

        }

        public override Node Update(GameTime gameTime)
        {

            

            if (Input.Button.OnePress("PlayMusic", Keyboard.GetState().IsKeyDown(Keys.M)))
            {
                _playMusic = !_playMusic;

                if (_playMusic)
                {
                    MediaPlayer.Play(_listSong[Misc.Rng.Next(0, _listSong.Count - 1)]); // start menu music !
                    MediaPlayer.Volume = 0.1f;
                    MediaPlayer.IsRepeating = false;
                }
                else
                {
                    MediaPlayer.Stop();
                }

            }

            if (Input.Button.OnePress("ResetGame", Game1.Player1.GetButton((int)SNES.BUTTONS.SELECT)!=0))
            {
                Console.WriteLine("Reset Game");

                for(int i = 0; i < Game1.MAX_PLAYER; i++)
                {
                    _playGrids[i].SetLevel(1).Init();
                }

                Area.KillAll();

                Game1._sound_ShareGem.Play(.4f, .05f, 0);

            }

            if (Input.Button.OnePress("PauseGame", Game1.Player1.GetButton((int)SNES.BUTTONS.START)!=0))
            {
                IsPause = !IsPause;
                Game1._sound_Tic.Play(.05f, .01f, 0);
            }


            if (_playMusic)
            {
                if (MediaPlayer.State != MediaState.Playing && !MediaPlayer.IsRepeating) // if Combat Phase Play List<Song> !
                {
                    //start playing new song
                    Console.WriteLine("Start PLay New Song !");
                    MediaPlayer.Play(NextSong());
                }
            }

            // Debug Add Unit
            if (Input.Button.OnePress("AddUnitL", Keyboard.GetState().IsKeyDown(Keys.F1)))
            {
                int y = Misc.Rng.Next(0, Gem.MAX_COLOR);
                Area.AddUnit(PlayGrid1, y, y, .5f);
            }
            if (Input.Button.OnePress("AddUnitAllL", Keyboard.GetState().IsKeyDown(Keys.F2)))
            {
                for (int i = 0; i < Gem.MAX_COLOR; i++)
                {
                    Area.AddUnit(PlayGrid2, i, i, .5f, 32 + PlayGrid2.NbDestroyGem).SetLevel(4);
                }
            }


            if (Input.Button.OnePress("AddUnitR", Keyboard.GetState().IsKeyDown(Keys.F4)))
            {
                int y = Misc.Rng.Next(0, Gem.MAX_COLOR);
                Area.AddUnit(PlayGrid3, y, y, .5f);
            }
            if (Input.Button.OnePress("AddUnitAllR", Keyboard.GetState().IsKeyDown(Keys.F3)))
            {
                for (int i = 0; i < Gem.MAX_COLOR; i++)
                {
                    Area.AddUnit(PlayGrid3, i, i, .5f, 32 + PlayGrid3.NbDestroyGem).SetLevel(4);
                }
            }

            if (!IsPause)
            {
                UpdateChilds(gameTime);
            }

            return base.Update(gameTime);
        }

        public override Node Render(SpriteBatch batch)
        {
            batch.GraphicsDevice.Clear(new Color(10,40,60));

            
            RenderChilds(batch);

            //batch.DrawLine(new Vector2(Game1._screenW / 2, 0), Game1._screenH, Gfx.RAD_90, Color.Red);
            //batch.DrawLine(new Vector2(Game1._screenW / 2 / 2, 0), Game1._screenH, Gfx.RAD_90, Color.OrangeRed);
            //batch.DrawLine(new Vector2(Game1._screenW / 2 / 2 * 3, 0), Game1._screenH, Gfx.RAD_90, Color.OrangeRed);

            if (IsPause)
            {
                Draw.FillRectangle(batch, new Rectangle(0, 0, Game1._screenW, Game1._screenH), Color.Black * .8f);
                Draw.CenterStringXY(batch, Game1._font_Big, " P A U S E ", Game1._screenW / 2, Game1._screenH / 2, Color.MonoGameOrange);
            }

            return base.Render(batch);
        }

        public override Node RenderAdditive(SpriteBatch batch)
        {

            RenderAdditiveChilds(batch);

            return base.RenderAdditive(batch);
        }

    }
}
