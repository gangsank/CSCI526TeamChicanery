public class Config
{
    public class Tag
    {
        public static string Respawn = "Respawn";
        public static string Finish = "Finish";
        public static string EditorOnly = "EditorOnly";
        public static string MainCamera = "MainCamera";
        public static string Player = "Player";
        public static string GameController = "GameController";
        public static string CinemachineTarget = "CinemachineTarget";
        public static string World = "World";
        public static string Obstacle = "Obstacle";
        public static string Item = "Item";
        public static string Reset = "Reset";
        public static string Goal = "Goal";
        public static string GameMenu = "GameMenu";
    }

    public class Layer
    {
        public static int Default = 0;
        public static int TransparentFX = 1;
        public static int IgnoreRaycast = 2;
        public static int Water = 4;
        public static int UI = 5;
        public static int Character= 8;
        public static int Ground = 9;
        public static int Boundary = 10;
        public static int Obstacle = 11;
    }
}
