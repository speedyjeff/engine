﻿using System;
using System.Collections.Generic;
using System.Text;

namespace engine.Common
{
    public static class Constants
    {
        // arrow keys
        public const char UpArrow = (char)254;
        public const char DownArrow = (char)253;
        public const char LeftArrow = (char)252;
        public const char RightArrow = (char)251;

        // keyboard
        public const char Space = (char)248;
        public const char Space2 = (char)32;
        public const char Esc = (char)247;

        public const char Up = 'W';
        public const char Up2 = 'w';
        public const char Down = 'S';
        public const char Down2 = 's';
        public const char Left = 'A';
        public const char Left2 = 'a';
        public const char Right = 'D';
        public const char Right2 = 'd';
        public const char Forward = 'z';
        public const char Forward2 = 'Z';
        public const char Back = 'c';
        public const char Back2 = 'C';

        public const char Pickup = 'F';
        public const char Pickup2 = 'f';

        public const char Reload = 'R';
        public const char MiddleMouse = 'r';

        public const char Switch = '1';

        public const char Drop = '0';
        public const char Drop2 = '2';
        public const char Drop3 = 'Q';
        public const char Drop4 = 'q';

        public const char Place = 'P';
        public const char Place2 = 'p';

        public const char Jump = 'j';
        public const char Jump2 = 'J';

        public const char RollLeft = '-';
        public const char RollLeft2 = '_';
        public const char RollRight = '=';
        public const char RollRight2 = '+';

        // mouse
        public const char LeftMouse = (char)250;
        public const char RightMouse = (char)249;

        // player options
        public const int Speed = 10;
        public const float MinSpeedMultiplier = 0.1f;
        public const int MaxShield = 100;
        public const int MaxHealth = 100;
        public const int GlobalClock = 100; // ms - cannot be below 30ms
        public const int MaxTrainedAICount = 0;
        public const int MaxAmmo = 500; // it is infinite, but any more than this is considered full
        public const float IsTouchingDistance = 0.1f;

        public const float Gravity = 10f;
        public const float Force = -20f;

        // world options
        public const float DefaultPace = 0f; // causes it to be collected from background
        public const float Ground = 0f;
        public const float Sky = 20f;
        public const float ProximityViewHeight = 888;
        public const float ProximityViewWidth = 1384;
        public const float ProximityViewDepth = 700;

        // diagnostics
        public const bool Debug_ShowHitBoxes = false;
        public const bool Debug_AIMoveDiag = false;
    }
}
