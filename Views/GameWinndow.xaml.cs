using igraUP.Models;
using igraUP.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace igraUP.Views
{
    public partial class GameWinndow : Window
    {
        private static Player _player;
        private User _currentUser;
        private int _playerHP = 100;
        private int _bloodAmmo = 0;
        private const int MAX_AMMO = 5;
        private int _kills = 0;
        private int _score = 0;
        private int _currentLevel = 1;
        private bool _isDead = false;
        private DateTime _startTime = DateTime.Now;

        private HashSet<Key> _keys = new HashSet<Key>();
        private double _playerDashTimer = 0;
        private double _playerDashCooldown = 0;
        private double _walkTime = 0;
        private Point _lastMousePos;

        private WriteableBitmap _bitmap = new WriteableBitmap(480, 270, 96, 96, PixelFormats.Pbgra32, null);
        private double[] _zBuffer;
        private double _currentFov = 100.0;
        private double _targetFov = 100.0;
        private double _dashVisualTimer = 0;

        private int[] _texturePixels; private int _texWidth; private int _texHeight;
        private int[] _floorPixels; private int _floorWidth; private int _floorHeight;
        private int[] _ceilPixels; private int _ceilWidth; private int _ceilHeight;
        private Map _map = new Map();

        private List<Enemy> _enemies = new List<Enemy>();
        private List<Projectile> _projectiles = new List<Projectile>();

        private int[] _enemyPixels; private int _enemyWidth; private int _enemyHeight;
        private int[] _bulletPixels; private int _bulletWidth; private int _bulletHeight;

        private int[][] _enemyDeathFrames; private int _enemyDeathWidth, _enemyDeathHeight;
        private int[][] _enemyAttackFrames; private int _enemyAnimWidth, _enemyAnimHeight;

        private BitmapImage[] _muzzleFrames;
        private int _currentMuzzleFrame = 0;
        private bool _isMuzzleFlashing = false;
        private double _muzzleTimer = 0;

        private double _shootFlashTimer = 0;
        private bool _isInjecting = false;
        private double _injectTimer = 0;
        private double _hpDecayTimer = 0;

        private System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();

        public GameWinndow()
        {
            InitializeComponent();
            _currentUser = new User { login = "Гость" };
            StartGame();
        }

        public GameWinndow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            StartGame();
        }

        private void StartGame()
        {
            GameImage.Source = _bitmap;

            PreviewKeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
            CompositionTarget.Rendering += OnRendering;
            MouseMove += OnMouseMove;
            MouseDown += OnMouseDown;
            _stopwatch.Start();

            LoadTexture("pack://application:,,,/Images/Textures/walls.png", out _texturePixels, out _texWidth, out _texHeight);
            LoadTexture("pack://application:,,,/Images/Textures/Tiles_Black_1_64.png", out _floorPixels, out _floorWidth, out _floorHeight);
            LoadTexture("pack://application:,,,/Images/Textures/ceil.png", out _ceilPixels, out _ceilWidth, out _ceilHeight);
            LoadTexture("pack://application:,,,/Images/Textures/enemy_lico.png", out _enemyPixels, out _enemyWidth, out _enemyHeight);
            LoadTexture("pack://application:,,,/Images/Textures/bullet.png", out _bulletPixels, out _bulletWidth, out _bulletHeight);

            _enemyDeathFrames = new int[14][];
            LoadAnimationSheet("pack://application:,,,/Images/Textures/enemy_lico_dying.png", 15, out _enemyDeathFrames, out _enemyDeathWidth, out _enemyDeathHeight);

            _enemyAttackFrames = new int[29][];
            LoadAnimationSheet("pack://application:,,,/Images/Textures/enemy-lico_spritelist.png", 29, out _enemyAttackFrames, out _enemyAnimWidth, out _enemyAnimHeight);

            _muzzleFrames = new BitmapImage[10];
            for (int i = 0; i < 10; i++)
            {
                _muzzleFrames[i] = new BitmapImage(new Uri($"pack://application:,,,/Images/Textures/spr_player_gun_shoot_effect_{i}.png"));
            }

            _player = new Player();
            _map = new Map();
            _zBuffer = new double[480];

            _player.posX = _map.SpawnX;
            _player.posY = _map.SpawnY;
            _player.angleDegrees = 0;

            SpawnEnemies(50);
            UpdateLeftHandVisuals();
        }

        private bool HasLineOfSight(double x1, double y1, double x2, double y2)
        {
            int mapX1 = (int)Math.Floor(x1);
            int mapY1 = (int)Math.Floor(y1);
            int mapX2 = (int)Math.Floor(x2);
            int mapY2 = (int)Math.Floor(y2);

            int dx = Math.Abs(mapX2 - mapX1);
            int dy = Math.Abs(mapY2 - mapY1);
            int sx = mapX1 < mapX2 ? 1 : -1;
            int sy = mapY1 < mapY2 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                if (mapY1 >= 0 && mapY1 < _map.map.Length && mapX1 >= 0 && mapX1 < _map.map[0].Length)
                {
                    if (_map.map[mapY1][mapX1] == 1) return false;
                }
                else
                {
                    return false;
                }

                if (mapX1 == mapX2 && mapY1 == mapY2) break;

                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; mapX1 += sx; }
                if (e2 < dx) { err += dx; mapY1 += sy; }
            }
            return true;
        }

        private void UpdateLeftHandVisuals()
        {
            Crack1.Visibility = _playerHP <= 75 ? Visibility.Visible : Visibility.Collapsed;
            Crack2.Visibility = _playerHP <= 45 ? Visibility.Visible : Visibility.Collapsed;
            Crack3.Visibility = _playerHP <= 20 ? Visibility.Visible : Visibility.Collapsed;

            double liquidHeight = ((double)_bloodAmmo / MAX_AMMO) * 650;

            if (liquidHeight > 0)
                LiquidClip.Rect = new Rect(0, 650 - liquidHeight, 650, liquidHeight);
            else
                LiquidClip.Rect = new Rect(0, 650, 650, 0);
        }

        private void UpdateEnemies(double dt)
        {
            foreach (var enemy in _enemies)
            {
                if (enemy.IsDead) continue;

                if (enemy.IsDying)
                {
                    enemy.AnimTimer += dt;
                    if (enemy.AnimTimer >= 0.04)
                    {
                        enemy.AnimTimer = 0;
                        enemy.AnimFrame++;
                        if (enemy.AnimFrame >= 15) enemy.IsDead = true;
                    }
                    continue;
                }

                double dx = _player.posX - enemy.X;
                double dy = _player.posY - enemy.Y;
                double distance = Math.Sqrt(dx * dx + dy * dy);
                if (distance < 0.1) distance = 0.1;
                double originDist = Math.Sqrt(Math.Pow(enemy.X - enemy.OriginX, 2) + Math.Pow(enemy.Y - enemy.OriginY, 2));

                if (!enemy.IsAwake && distance < 15.0) enemy.IsAwake = true;
                if (!enemy.IsAwake) continue;

                enemy.PathTimer -= dt;
                if (enemy.PathTimer <= 0)
                {
                    enemy.PathTimer = 0.5;
                    int ex = (int)Math.Floor(enemy.X);
                    int ey = (int)Math.Floor(enemy.Y);
                    int px = (int)Math.Floor(_player.posX);
                    int py = (int)Math.Floor(_player.posY);
                    enemy.NextStep = Pathfinding.GetNextStep(ex, ey, px, py, _map.map);
                }

                if (enemy.State == "idle")
                {
                    if (distance <= 12.0 && HasLineOfSight(enemy.X, enemy.Y, _player.posX, _player.posY))
                    {
                        enemy.State = "hunt";
                    }
                }
                else if (enemy.State == "hunt")
                {
                    if (enemy.NextStep.X != -1 && enemy.NextStep.Y != -1)
                    {
                        double targetX = enemy.NextStep.X + 0.5;
                        double targetY = enemy.NextStep.Y + 0.5;
                        double toX = targetX - enemy.X;
                        double toY = targetY - enemy.Y;
                        double stepDist = Math.Sqrt(toX * toX + toY * toY);

                        if (stepDist > 0.05)
                        {
                            double speed = 2.0 * dt;
                            double newX = enemy.X + (toX / stepDist) * speed;
                            double newY = enemy.Y + (toY / stepDist) * speed;

                            if (!IsCollision(newX, enemy.Y)) enemy.X = newX;
                            if (!IsCollision(enemy.X, newY)) enemy.Y = newY;
                        }
                    }

                    if (originDist >= 20.0) enemy.State = "return";

                    if (distance <= 8.0 && HasLineOfSight(enemy.X, enemy.Y, _player.posX, _player.posY) && !enemy.IsAttacking)
                    {
                        enemy.State = "attack";
                        enemy.IsAttacking = true;
                        enemy.AnimFrame = 0;
                        enemy.AnimTimer = 0;
                        enemy.HasFired = false;
                    }
                }
                else if (enemy.State == "attack")
                {
                    if (!enemy.IsAttacking)
                    {
                        enemy.Ammo--;
                        if (enemy.Ammo <= 0)
                        {
                            enemy.State = "dodge";
                            enemy.DodgeTimer = 0.4;
                            double dodgeAngle = Math.Atan2(enemy.Y - _player.posY, enemy.X - _player.posX) + (Math.PI / 2);
                            enemy.DodgeTargetX = enemy.X + Math.Cos(dodgeAngle) * 3.0;
                            enemy.DodgeTargetY = enemy.Y + Math.Sin(dodgeAngle) * 3.0;
                        }
                        else
                        {
                            enemy.State = "hunt";
                        }
                    }
                }
                else if (enemy.State == "dodge")
                {
                    enemy.DodgeTimer -= dt;
                    double toX = enemy.DodgeTargetX - enemy.X;
                    double toY = enemy.DodgeTargetY - enemy.Y;
                    double stepDist = Math.Sqrt(toX * toX + toY * toY);

                    if (stepDist > 0.1)
                    {
                        double dodgeSpeed = 8.0 * dt;
                        double newX = enemy.X + (toX / stepDist) * dodgeSpeed;
                        double newY = enemy.Y + (toY / stepDist) * dodgeSpeed;
                        if (!IsCollision(newX, enemy.Y)) enemy.X = newX;
                        if (!IsCollision(enemy.X, newY)) enemy.Y = newY;
                    }

                    if (enemy.DodgeTimer <= 0)
                    {
                        enemy.Ammo = 3;
                        enemy.State = "hunt";
                    }
                }
                else if (enemy.State == "return")
                {
                    if (distance <= 12.0 && HasLineOfSight(enemy.X, enemy.Y, _player.posX, _player.posY))
                    {
                        enemy.State = "hunt";
                    }
                    else if (originDist <= 2.0)
                    {
                        enemy.State = "idle";
                    }
                    else
                    {
                        double toX = enemy.OriginX - enemy.X;
                        double toY = enemy.OriginY - enemy.Y;
                        double len = Math.Sqrt(toX * toX + toY * toY);
                        if (len > 0.1)
                        {
                            double speed = 1.5 * dt;
                            if (!IsCollision(enemy.X + (toX / len) * speed, enemy.Y)) enemy.X += (toX / len) * speed;
                            if (!IsCollision(enemy.X, enemy.Y + (toY / len) * speed)) enemy.Y += (toY / len) * speed;
                        }
                    }
                }

                if (enemy.IsAttacking)
                {
                    enemy.AnimTimer += dt;
                    if (enemy.AnimTimer >= 0.04)
                    {
                        enemy.AnimTimer = 0;
                        enemy.AnimFrame++;

                        if (enemy.AnimFrame == 18 && !enemy.HasFired)
                        {
                            enemy.HasFired = true;
                            _projectiles.Add(new Projectile
                            {
                                X = enemy.X,
                                Y = enemy.Y,
                                Z = enemy.Z - 0.1,
                                DirX = dx / distance,
                                DirY = dy / distance
                            });
                        }

                        if (enemy.AnimFrame >= 29)
                        {
                            enemy.IsAttacking = false;
                        }
                    }
                }
            }

            _enemies.RemoveAll(e => e.IsDead);

            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                var p = _projectiles[i];
                p.X += p.DirX * (5.0 * dt);
                p.Y += p.DirY * (5.0 * dt);

                if (IsCollision(p.X, p.Y)) { _projectiles.RemoveAt(i); continue; }

                double distToPlayer = Math.Sqrt(Math.Pow(p.X - _player.posX, 2) + Math.Pow(p.Y - _player.posY, 2));
                if (distToPlayer < 0.3)
                {
                    _projectiles.RemoveAt(i);
                    _playerHP -= 20;
                    UpdateLeftHandVisuals();
                }
            }
        }

        private void LoadAnimationSheet(string resourcePath, int frameCount, out int[][] frames, out int width, out int height)
        {
            BitmapImage img = new BitmapImage(new Uri(resourcePath, UriKind.RelativeOrAbsolute));
            FormatConvertedBitmap fcb = new FormatConvertedBitmap(img, PixelFormats.Bgra32, null, 0);

            width = fcb.PixelWidth / frameCount;
            height = fcb.PixelHeight;

            int fullStride = fcb.PixelWidth * 4;
            byte[] allPixels = new byte[fcb.PixelHeight * fullStride];
            fcb.CopyPixels(allPixels, fullStride, 0);

            frames = new int[frameCount][];
            for (int f = 0; f < frameCount; f++)
            {
                frames[f] = new int[width * height];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int index = y * fullStride + (f * width + x) * 4;
                        frames[f][y * width + x] = (allPixels[index + 3] << 24) |
                                                   (allPixels[index + 2] << 16) |
                                                   (allPixels[index + 1] << 8) |
                                                   allPixels[index];
                    }
                }
            }
        }

        private void LoadTexture(string resourcePath, out int[] outPixels, out int outWidth, out int outHeight)
        {
            BitmapImage img = new BitmapImage(new Uri(resourcePath, UriKind.RelativeOrAbsolute));
            FormatConvertedBitmap fcb = new FormatConvertedBitmap(img, PixelFormats.Bgra32, null, 0);

            outWidth = fcb.PixelWidth;
            outHeight = fcb.PixelHeight;

            int stride = outWidth * 4;
            byte[] pixels = new byte[outHeight * stride];
            fcb.CopyPixels(pixels, stride, 0);

            outPixels = new int[outWidth * outHeight];

            for (int i = 0; i < pixels.Length; i += 4)
            {
                outPixels[i / 4] = (pixels[i + 3] << 24) | (pixels[i + 2] << 16) | (pixels[i + 1] << 8) | pixels[i];
            }
        }

        public double RayCast(double x, double y, double angleDegrees, out double wallX, out bool hitVertical)
        {
            double angleRad = angleDegrees * Math.PI / 180;
            double dx = Math.Cos(angleRad);
            double dy = Math.Sin(angleRad);
            int mapX = (int)Math.Floor(x);
            int mapY = (int)Math.Floor(y);

            double deltaDistX = (dx == 0) ? double.MaxValue : Math.Abs(1 / dx);
            double deltaDistY = (dy == 0) ? double.MaxValue : Math.Abs(1 / dy);

            int stepX = (dx < 0) ? -1 : 1;
            int stepY = (dy < 0) ? -1 : 1;

            double sideDistX;
            double sideDistY;

            if (dx < 0) sideDistX = (x - mapX) * deltaDistX;
            else sideDistX = (mapX + 1 - x) * deltaDistX;

            if (dy < 0) sideDistY = (y - mapY) * deltaDistY;
            else sideDistY = (mapY + 1 - y) * deltaDistY;

            hitVertical = false;

            while (true)
            {
                if (sideDistX < sideDistY)
                {
                    sideDistX += deltaDistX;
                    mapX += stepX;
                    hitVertical = true;
                }
                else
                {
                    sideDistY += deltaDistY;
                    mapY += stepY;
                    hitVertical = false;
                }

                if (mapX < 0 || mapX >= _map.map[0].Length || mapY < 0 || mapY >= _map.map.Length)
                {
                    wallX = 0;
                    return double.MaxValue;
                }

                if (_map.map[mapY][mapX] == 1) break;
            }

            double distance;
            if (hitVertical)
            {
                distance = sideDistX - deltaDistX;
                wallX = y + distance * dy;
            }
            else
            {
                distance = sideDistY - deltaDistY;
                wallX = x + distance * dx;
            }

            wallX -= Math.Floor(wallX);
            return distance;
        }

        public void Render()
        {
            int width = _bitmap.PixelWidth;
            int height = _bitmap.PixelHeight;
            double fov = _currentFov;

            _bitmap.Lock();

            try
            {
                IntPtr backBuffer = _bitmap.BackBuffer;
                int stride = _bitmap.BackBufferStride;

                int pitchOffset = (int)_player.pitch;
                int floorColor = unchecked((int)0xFF202020);
                int ceilColor = unchecked((int)0xFF101010);
                int horizon = height / 2 + pitchOffset;
                double posZ = 0.5 * height;

                for (int x = 0; x < width; x++)
                {
                    double rayAngle = _player.angleDegrees + (x - width / 2.0) * fov / width;
                    double angleRad = rayAngle * Math.PI / 180;
                    double dx = Math.Cos(angleRad);
                    double dy = Math.Sin(angleRad);

                    double distance = RayCast(_player.posX, _player.posY, rayAngle, out double wallX, out bool hitVertical);

                    _zBuffer[x] = distance;

                    if (distance <= 0 || distance == double.MaxValue) continue;

                    double lineHeight = height / distance;
                    double exactDrawStart = (height - lineHeight) / 2.0 + pitchOffset;
                    int drawStart = (int)exactDrawStart;
                    int drawEnd = (int)((height + lineHeight) / 2.0) + pitchOffset;

                    int texX = (int)(wallX * _texWidth);
                    if (hitVertical && dx > 0) texX = _texWidth - texX - 1;
                    if (!hitVertical && dy < 0) texX = _texWidth - texX - 1;
                    if (texX < 0) texX = 0;
                    if (texX >= _texWidth) texX = _texWidth - 1;

                    double step = 1.0 * _texHeight / lineHeight;
                    double texPos = (drawStart - exactDrawStart) * step;

                    for (int y = 0; y < height; y++)
                    {
                        int color;
                        if (y < drawStart)
                        {
                            int p = horizon - y;
                            if (p > 0)
                            {
                                double rowDistance = posZ / p;
                                double ceilX = _player.posX + rowDistance * dx;
                                double ceilY = _player.posY + rowDistance * dy;
                                int cTexX = (int)((ceilX - (int)ceilX) * _ceilWidth) & (_ceilWidth - 1);
                                int cTexY = (int)((ceilY - (int)ceilY) * _ceilHeight) & (_ceilHeight - 1);
                                int texColor = _ceilPixels[cTexY * _ceilWidth + cTexX];
                                int brightness = (int)(200 - rowDistance * 15);
                                if (brightness < 0) brightness = 0;
                                int r = ((texColor >> 16) & 0xFF) * brightness / 255;
                                int g = ((texColor >> 8) & 0xFF) * brightness / 255;
                                int b = (texColor & 0xFF) * brightness / 255;
                                color = (255 << 24) | (r << 16) | (g << 8) | b;
                            }
                            else color = ceilColor;
                        }
                        else if (y > drawEnd)
                        {
                            int p = y - horizon;
                            if (p > 0)
                            {
                                double rowDistance = posZ / p;
                                double floorX = _player.posX + rowDistance * dx;
                                double floorY = _player.posY + rowDistance * dy;
                                int fTexX = (int)((floorX - (int)floorX) * _floorWidth) & (_floorWidth - 1);
                                int fTexY = (int)((floorY - (int)floorY) * _floorHeight) & (_floorHeight - 1);
                                int texColor = _floorPixels[fTexY * _floorWidth + fTexX];
                                int brightness = (int)(255 - rowDistance * 12);
                                if (brightness < 0) brightness = 0;
                                int r = ((texColor >> 16) & 0xFF) * brightness / 255;
                                int g = ((texColor >> 8) & 0xFF) * brightness / 255;
                                int b = (texColor & 0xFF) * brightness / 255;
                                color = (255 << 24) | (r << 16) | (g << 8) | b;
                            }
                            else color = floorColor;
                        }
                        else
                        {
                            int texY = (int)texPos & (_texHeight - 1);
                            texPos += step;
                            int texColor = _texturePixels[texY * _texWidth + texX];
                            int brightness = (int)(255 - distance * 10);
                            if (brightness < 0) brightness = 0;
                            int r = ((texColor >> 16) & 0xFF) * brightness / 255;
                            int g = ((texColor >> 8) & 0xFF) * brightness / 255;
                            int b = (texColor & 0xFF) * brightness / 255;
                            color = (255 << 24) | (r << 16) | (g << 8) | b;
                        }

                        unsafe
                        {
                            byte* ptr = (byte*)backBuffer + y * stride + x * 4;
                            *((int*)ptr) = color;
                        }
                    }
                }

                double playerAngleRad = _player.angleDegrees * Math.PI / 180;
                double dirX = Math.Cos(playerAngleRad);
                double dirY = Math.Sin(playerAngleRad);
                double planeX = -dirY * 0.66;
                double planeY = dirX * 0.66;
                double invDet = 1.0 / (planeX * dirY - dirX * planeY);

                var sortedEnemies = _enemies.OrderByDescending(e =>
                    Math.Pow(e.X - _player.posX, 2) + Math.Pow(e.Y - _player.posY, 2)).ToList();

                foreach (var enemy in sortedEnemies)
                {
                    if (enemy.IsDead) continue;

                    int[] currentPixels;
                    int currentW, currentH;

                    if (enemy.IsDying)
                    {
                        int safeFrame = Math.Min(enemy.AnimFrame, _enemyDeathFrames.Length - 1);
                        currentPixels = _enemyDeathFrames[safeFrame];
                        currentW = _enemyDeathWidth;
                        currentH = _enemyDeathHeight;
                    }
                    else if (enemy.IsAttacking)
                    {
                        currentPixels = _enemyAttackFrames[enemy.AnimFrame];
                        currentW = _enemyAnimWidth;
                        currentH = _enemyAnimHeight;
                    }
                    else
                    {
                        currentPixels = _enemyPixels;
                        currentW = _enemyWidth;
                        currentH = _enemyHeight;
                    }

                    double spriteX = enemy.X - _player.posX;
                    double spriteY = enemy.Y - _player.posY;

                    double transformX = invDet * (dirY * spriteX - dirX * spriteY);
                    double transformY = invDet * (-planeY * spriteX + planeX * spriteY);

                    if (transformY > 0.1)
                    {
                        int spriteScreenX = (int)((width / 2) * (1 + transformX / transformY));
                        int spriteHeight = Math.Abs((int)(height / transformY * 0.7));
                        int spriteWidth = Math.Abs((int)(height / transformY * 0.7));

                        int vMoveScreen = (int)(enemy.Z * height / transformY);
                        int floorY = (int)(height / 2 + pitchOffset + (posZ / transformY));
                        int drawEndY = floorY + vMoveScreen;
                        int drawStartY = drawEndY - spriteHeight;

                        int drawStartX = -spriteWidth / 2 + spriteScreenX;
                        int drawEndX = spriteWidth / 2 + spriteScreenX;

                        for (int stripe = drawStartX; stripe < drawEndX; stripe++)
                        {
                            if (stripe < 0 || stripe >= width || transformY <= 0 || stripe >= _zBuffer.Length) continue;
                            if (transformY >= _zBuffer[stripe]) continue;

                            int texX = (int)(256 * (stripe - (-spriteWidth / 2 + spriteScreenX)) * currentW / spriteWidth / 256);
                            if (texX < 0 || texX >= currentW) continue;

                            for (int y = drawStartY; y < drawEndY; y++)
                            {
                                if (y < 0 || y >= height) continue;

                                int texY = (int)((y - drawStartY) * (double)currentH / spriteHeight);
                                if (texY < 0 || texY >= currentH) continue;

                                int texColor = currentPixels[texY * currentW + texX];

                                if (((texColor >> 24) & 0xFF) > 0)
                                {
                                    int brightness = (int)(255 - transformY * 10);
                                    if (brightness < 0) brightness = 0;

                                    int a = (texColor >> 24) & 0xFF;
                                    int r = ((texColor >> 16) & 0xFF) * brightness / 255;
                                    int g = ((texColor >> 8) & 0xFF) * brightness / 255;
                                    int b = (texColor & 0xFF) * brightness / 255;
                                    int finalColor = (a << 24) | (r << 16) | (g << 8) | b;

                                    unsafe
                                    {
                                        byte* ptr = (byte*)backBuffer + y * stride + stripe * 4;
                                        *((int*)ptr) = finalColor;
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (var p in _projectiles)
                {
                    double projSpriteX = p.X - _player.posX;
                    double projSpriteY = p.Y - _player.posY;

                    double projTransformX = invDet * (dirY * projSpriteX - dirX * projSpriteY);
                    double projTransformY = invDet * (-planeY * projSpriteX + planeX * projSpriteY);

                    if (projTransformY > 0.1)
                    {
                        int spriteScreenX = (int)((width / 2) * (1 + projTransformX / projTransformY));

                        int projHeight = Math.Abs((int)(height / projTransformY * 0.15));
                        int projWidth = Math.Abs((int)(height / projTransformY * 0.15));

                        int vMoveScreen = (int)(p.Z * height / projTransformY);
                        int floorY = (int)(height / 2 + pitchOffset + (posZ / projTransformY));
                        int drawEndY = floorY + vMoveScreen;
                        int drawStartY = drawEndY - projHeight;

                        int drawStartX = -projWidth / 2 + spriteScreenX;
                        int drawEndX = projWidth / 2 + spriteScreenX;

                        for (int stripe = drawStartX; stripe < drawEndX; stripe++)
                        {
                            if (stripe < 0 || stripe >= width || stripe >= _zBuffer.Length) continue;
                            if (projTransformY >= _zBuffer[stripe]) continue;

                            int texX = (int)(256 * (stripe - (-projWidth / 2 + spriteScreenX)) * _bulletWidth / projWidth / 256);
                            if (texX < 0 || texX >= _bulletWidth) continue;

                            for (int y = drawStartY; y < drawEndY; y++)
                            {
                                if (y < 0 || y >= height) continue;

                                int texY = (int)((y - drawStartY) * (double)_bulletHeight / projHeight);
                                if (texY < 0 || texY >= _bulletHeight) continue;

                                int texColor = _bulletPixels[texY * _bulletWidth + texX];

                                if (((texColor >> 24) & 0xFF) > 0)
                                {
                                    unsafe
                                    {
                                        byte* ptr = (byte*)backBuffer + y * stride + stripe * 4;
                                        *((int*)ptr) = texColor;
                                    }
                                }
                            }
                        }
                    }
                }

                double exSpriteX = _map.ExitX - _player.posX;
                double exSpriteY = _map.ExitY - _player.posY;

                double exTransformX = invDet * (dirY * exSpriteX - dirX * exSpriteY);
                double exTransformY = invDet * (-planeY * exSpriteX + planeX * exSpriteY);

                if (exTransformY > 0.1)
                {
                    int exScreenX = (int)((width / 2) * (1 + exTransformX / exTransformY));
                    int exHeight = Math.Abs((int)(height / exTransformY));

                    int exFloorY = (int)(height / 2 + pitchOffset + (posZ / exTransformY));
                    int exCeilY = exFloorY - exHeight;

                    int exWidth = Math.Abs((int)(height / exTransformY * 0.3));
                    int exStartX = exScreenX - exWidth / 2;
                    int exEndX = exScreenX + exWidth / 2;

                    for (int x = exStartX; x < exEndX; x++)
                    {
                        if (x < 0 || x >= width || x >= _zBuffer.Length) continue;
                        if (exTransformY >= _zBuffer[x]) continue;

                        for (int y = exCeilY; y < exFloorY; y++)
                        {
                            if (y < 0 || y >= height) continue;
                            unsafe
                            {
                                byte* ptr = (byte*)backBuffer + y * stride + x * 4;
                                int flicker = new Random().Next(0, 100);
                                int color = (flicker > 50) ? unchecked((int)0xFF00FF00) : unchecked((int)0xFF00AA00);
                                *((int*)ptr) = color;
                            }
                        }
                    }
                }

                int cx = width / 2;
                int cy = height / 2;
                unsafe
                {
                    for (int i = -4; i <= 4; i++)
                    {
                        if (cx + i >= 0 && cx + i < width && cy >= 0 && cy < height)
                        {
                            byte* ptr = (byte*)backBuffer + cy * stride + (cx + i) * 4;
                            *((int*)ptr) = unchecked((int)0xFF00FF00);
                        }
                        if (cx >= 0 && cx < width && cy + i >= 0 && cy + i < height)
                        {
                            byte* ptr = (byte*)backBuffer + (cy + i) * stride + cx * 4;
                            *((int*)ptr) = unchecked((int)0xFF00FF00);
                        }
                    }
                }

                if (_playerHP < 50)
                {
                    DamageOverlay.Opacity = ((50.0 - _playerHP) / 50.0) * 0.6;
                }
                else
                {
                    DamageOverlay.Opacity = 0;
                }

                _bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            }
            finally
            {
                _bitmap.Unlock();
            }
        }

        public void OnRendering(object sender, EventArgs e)
        {
            if (_isDead) return;

            double dt = _stopwatch.Elapsed.TotalSeconds;
            _stopwatch.Restart();

            if (dt > 0.1) dt = 0.1;

            MoveUpdate(dt);
            UpdateEnemies(dt);

            _hpDecayTimer += dt;
            if (_hpDecayTimer >= 1.0)
            {
                _hpDecayTimer = 0;
                if (_playerHP > 0)
                {
                    _playerHP -= 1;
                    UpdateLeftHandVisuals();
                }
            }

            if (_isInjecting)
            {
                _injectTimer -= dt;

                if (_injectTimer > 0.5)
                {
                    HandTranslate.X = -50;
                    HandTranslate.Y = -100;
                    HandRotate.Angle = -20;
                }
                else if (_injectTimer > 0.2)
                {
                    HandTranslate.X = 30;
                    HandTranslate.Y = 30;
                    HandRotate.Angle = 15;
                }
                else if (_injectTimer > 0)
                {
                    HandTranslate.X = 0;
                    HandTranslate.Y = 0;
                    HandRotate.Angle = 0;
                }
                else
                {
                    _isInjecting = false;
                }
            }

            if (_shootFlashTimer > 0)
            {
                _shootFlashTimer -= dt;
                if (_shootFlashTimer <= 0)
                {
                    ShootFlash.Opacity = 0;
                }
            }




            if (_isMuzzleFlashing)
            {
                _muzzleTimer += dt;
                if (_muzzleTimer > 0.025)
                {
                    _muzzleTimer = 0;
                    _currentMuzzleFrame++;
                    if (_currentMuzzleFrame >= _muzzleFrames.Length)
                    {
                        _isMuzzleFlashing = false;
                        MuzzleFlashImage.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        MuzzleFlashImage.Source = _muzzleFrames[_currentMuzzleFrame];
                    }
                }
            }

            Render();

            if (_playerHP <= 0 && !_isDead)
            {
                _isDead = true;
                CompositionTarget.Rendering -= OnRendering;

                TimeSpan playTime = DateTime.Now - _startTime;
                string timeStr = $"{playTime.Minutes:D2}:{playTime.Seconds:D2}";

                Score newScore = new Score
                {
                    Username = _currentUser.login,
                    Points = _score,
                    Kills = _kills,
                    Time = timeStr
                };
                SaveService.SaveScore(newScore);

                var deathScreen = new DeathScreenWindow(_score, _kills, timeStr, _currentUser);
                deathScreen.Show();
                this.Close();
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { Application.Current.Shutdown(); return; }


            _keys.Add(e.Key);
            e.Handled = true;
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            _keys.Remove(e.Key);
            e.Handled = true;
        }

        private void MoveUpdate(double dt)
        {
            if (_playerDashCooldown > 0) _playerDashCooldown -= dt;

            double angleRad = _player.angleDegrees * Math.PI / 180;




            double speed = 5.5 * dt;

            double moveX = 0;
            double moveY = 0;

            double forwardX = Math.Cos(angleRad);
            double forwardY = Math.Sin(angleRad);
            double rightX = -Math.Sin(angleRad);
            double rightY = Math.Cos(angleRad);

            if (_keys.Contains(Key.W)) { moveX += forwardX; moveY += forwardY; }
            if (_keys.Contains(Key.S)) { moveX -= forwardX; moveY -= forwardY; }
            if (_keys.Contains(Key.A)) { moveX -= rightX; moveY -= rightY; }
            if (_keys.Contains(Key.D)) { moveX += rightX; moveY += rightY; }

            double length = Math.Sqrt(moveX * moveX + moveY * moveY);
            if (length > 0)
            {
                moveX /= length;
                moveY /= length;

                double newX = _player.posX + moveX * speed;
                double newY = _player.posY + moveY * speed;

                if (!IsCollision(newX, _player.posY)) _player.posX = newX;
                if (!IsCollision(_player.posX, newY)) _player.posY = newY;
            }

            if (length > 0)
            {
                _walkTime += 15.0 * dt;

                WeaponTransform.X = Math.Sin(_walkTime) * 15;
                WeaponTransform.Y = Math.Abs(Math.Cos(_walkTime)) * 10;

                if (!_isInjecting)
                {
                    HandTranslate.X = Math.Sin(_walkTime * 0.8) * 25;
                    HandTranslate.Y = Math.Abs(Math.Cos(_walkTime * 0.8)) * 15;
                }

                _player.pitch = Math.Sin(_walkTime * 0.8) * 2;
            }
            else
            {
                _walkTime = 0;
                WeaponTransform.X = 0;
                WeaponTransform.Y = 0;

                if (!_isInjecting)
                {
                    HandTranslate.X = 0;
                    HandTranslate.Y = 0;
                }
            }

            double exitDx = _player.posX - _map.ExitX;
            double exitDy = _player.posY - _map.ExitY;
            double exitDistance = Math.Sqrt(exitDx * exitDx + exitDy * exitDy);

            if (exitDistance < 0.6)
            {
                _currentLevel++;
                _score += 500;

                _map = new Map();
                _player.posX = _map.SpawnX;
                _player.posY = _map.SpawnY;
                _player.angleDegrees = 0;

                _projectiles.Clear();
                SpawnEnemies(5 + (_currentLevel * 2));
                _playerHP = 100;
            }
        }

        private bool IsCollision(double x, double y)
        {
            double radius = 0.15;

            if (CheckWall(x - radius, y - radius)) return true;
            if (CheckWall(x + radius, y - radius)) return true;
            if (CheckWall(x - radius, y + radius)) return true;
            if (CheckWall(x + radius, y + radius)) return true;

            return false;
        }

        private bool CheckWall(double x, double y)
        {
            int mapX = (int)Math.Floor(x);
            int mapY = (int)Math.Floor(y);

            if (mapX < 0 || mapX >= _map.map[0].Length || mapY < 0 || mapY >= _map.map.Length)
                return true;

            return _map.map[mapY][mapX] == 1;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            Point currentPos = e.GetPosition(this);
            double centerX = ActualWidth / 2;
            double centerY = ActualHeight / 2;

            double deltaX = currentPos.X - centerX;
            double deltaY = currentPos.Y - centerY;

            double sensitivity = 0.05;

            _player.angleDegrees += deltaX * sensitivity;
            if (_player.angleDegrees < 0) _player.angleDegrees += 360;
            if (_player.angleDegrees >= 360) _player.angleDegrees -= 360;

            _player.pitch -= deltaY * sensitivity;

            double maxPitch = ActualHeight / 2.0;
            if (_player.pitch < -maxPitch) _player.pitch = -maxPitch;
            if (_player.pitch > maxPitch) _player.pitch = maxPitch;

            Point centerScreen = PointToScreen(new Point(centerX, centerY));
            SetCursorPos((int)centerScreen.X, (int)centerScreen.Y);
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _shootFlashTimer = 0.05;
                ShootFlash.Opacity = 0.4;
                _isMuzzleFlashing = true;
                _currentMuzzleFrame = 0;
                _muzzleTimer = 0;
                MuzzleFlashImage.Visibility = Visibility.Visible;
                MuzzleFlashImage.Source = _muzzleFrames[0];

                Enemy closestEnemy = null;
                double minDistance = 9999.0;

                foreach (var enemy in _enemies)
                {
                    if (enemy.IsDead || enemy.IsDying) continue;

                    double dx = enemy.X - _player.posX;
                    double dy = enemy.Y - _player.posY;
                    double distance = Math.Sqrt(dx * dx + dy * dy);
                    double enemyAngle = Math.Atan2(dy, dx) * 180 / Math.PI;

                    double angleDiff = enemyAngle - _player.angleDegrees;
                    while (angleDiff > 180) angleDiff -= 360;
                    while (angleDiff < -180) angleDiff += 360;

                    if (Math.Abs(angleDiff) < 2.0 && distance < 2.0)
                    {
                        if (HasLineOfSight(_player.posX, _player.posY, enemy.X, enemy.Y))
                        {
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                closestEnemy = enemy;
                            }
                        }
                    }
                }

                if (closestEnemy != null)
                {
                    closestEnemy.IsDying = true;
                    closestEnemy.AnimFrame = 0;
                    closestEnemy.AnimTimer = 0;

                    _kills++;
                    _score += 100;
                    _player.pitch -= 5;

                    if (_bloodAmmo < MAX_AMMO)
                    {
                        _bloodAmmo++;
                        UpdateLeftHandVisuals();
                    }
                }
            }

            if (e.RightButton == MouseButtonState.Pressed && _bloodAmmo > 0 && !_isInjecting && _playerHP < 100)
            {
                _isInjecting = true;
                _injectTimer = 0.7;
                _playerHP = Math.Min(100, _playerHP + _bloodAmmo * 15);
                _bloodAmmo = 0;
                UpdateLeftHandVisuals();
            }
        }

        private void SpawnEnemies(int count)
        {
            _enemies.Clear();
            Random rand = new Random();

            for (int i = 0; i < count; i++)
            {
                if (_map.EnemySpawnPoints.Count == 0) break;

                int index = rand.Next(0, _map.EnemySpawnPoints.Count);
                var point = _map.EnemySpawnPoints[index];

                _enemies.Add(new Enemy
                {
                    X = point.X + 0.5,
                    Y = point.Y + 0.5,
                    OriginX = point.X + 0.5,
                    OriginY = point.Y + 0.5
                });

                _map.EnemySpawnPoints.RemoveAt(index);
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int x, int y);
    }
}