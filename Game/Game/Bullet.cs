using GameEngine;
using GameEngine._2D;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Game
{
    public class Bullet<T> : Description2D, IIdentifiable where T : Description2D, IIdentifiable
    {
        private double dir;

        public Guid Id { get; private set; }

        private SinWaveSound sound;

        public Bullet(int x, int y, double dir) : base(Sprite.Sprites["bullet"], x, y, 16, 16)
        {
            this.dir = dir;
        }

        private void Boom()
        {
            sound = new SinWaveSound(
                200f, 44100 / Program.TPS, 100, 44100 / Program.TPS * 2, 80, 44100 / Program.TPS, 160, 44100 / Program.TPS, 80, 44100 / Program.TPS, 160, 44100 / Program.TPS, 80, 44100 / Program.TPS, 160, 44100 / Program.TPS);

            sound.SetWaveFormat(44100, 2);

            Program.WavProvider.AddMixerInput((ISampleProvider)sound);
            Program.WavPlayer.Play();
        }

        private Bitmap Draw()
        {
            Bitmap bmp = BitmapExtensions.CreateBitmap(4, 4);
            Graphics gfx = Graphics.FromImage(bmp);
            gfx.FillEllipse(Brushes.Black, 0, 0, 4, 4);

            return bmp;
        }

        private void Tick(Location location, Entity entity)
        {
            if (Program.Engine.Location.GetEntities<DialogBox>().Any())
            {
                return;
            }

            if (sound == null)
            {
                Boom();
            }

            Description2D d2d = entity.Description as Description2D;
            d2d.ChangeCoordsDelta(Math.Cos(dir) * 8, Math.Sin(dir) * 8);
            foreach (T enemy in location.GetEntities<T>())
            {
                if (d2d.IsCollision(enemy))
                {
                    location.RemoveEntity(enemy.Id);
                }
            }

            if (Program.Collision(d2d, location.GetEntities<Wall>().Select(w => (Description2D)w).ToList()))
            {
                location.RemoveEntity(Id);
            }
        }

        public static Entity Create(int x, int y, double dir)
        {
            Bullet<T> bullet = new Bullet<T>(x, y, dir);
            bullet.DrawAction = bullet.Draw;
            Entity entity = new Entity(bullet);
            entity.TickAction = bullet.Tick;
            bullet.Id = entity.Id;
            return entity;
        }
    }
}
