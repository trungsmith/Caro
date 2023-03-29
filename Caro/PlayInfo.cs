﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caro
{
    public class PlayInfo
    {
        private Point point;

        public Point Point { get => point; set => point=value; }

        private int currentPlayer;
        public int CurrentPlayer { get => currentPlayer; set => currentPlayer=value; }

        public PlayInfo(Point point, int currentPlayer)
        {
            this.Point = point;
            this.CurrentPlayer = currentPlayer;
        }
    }
}
